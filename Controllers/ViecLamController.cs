// File: Controllers/ViecLamController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.ViecDaLuu; // ViewModel cho việc đã lưu
using HeThongTimViec.ViewModels.ViecLam; // ViewModel cho việc đã ứng tuyển
using HeThongTimViec.ViewModels.TimViec; // Dùng lại PaginatedList
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HeThongTimViec.Extensions;          // Cho EnumExtensions
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;          // Cho IFormFile và StatusCodes
using System;
using Microsoft.AspNetCore.Hosting;         // Cho IWebHostEnvironment (upload/delete file)
using System.IO;                          // Cho Path, Directory, File, FileStream, IOException
using Microsoft.AspNetCore.Mvc.Rendering;
using HeThongTimViec.Utils; // Cho SelectList
using HeThongTimViec.Services; // <<<<<<< THÊM USING CHO IThongBaoService
using System.Text.Json;        // <<<<<<< THÊM USING CHO JsonSerializer

namespace HeThongTimViec.Controllers
{
    [Route("viec-lam")] // Route cơ sở cho controller này
    [Authorize] // Yêu cầu đăng nhập cho tất cả action trong controller này
    public class ViecLamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ViecLamController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // Cần cho xử lý file CV
         private readonly IThongBaoService _thongBaoService;

        public ViecLamController(ApplicationDbContext context, ILogger<ViecLamController> logger, IWebHostEnvironment webHostEnvironment, IThongBaoService thongBaoService) // Thêm IWebHostEnvironment
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; // Gán giá trị
            _thongBaoService = thongBaoService; 
        }

        // Helper lấy User ID từ ClaimsPrincipal (User)
        private int GetCurrentUserId() // Đổi thành non-nullable, sẽ throw exception nếu không lấy được
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            _logger.LogError("Không thể parse User ID từ ClaimsPrincipal trong một action được bảo vệ [Authorize]. Claim value: {ClaimValue}", userIdClaim);
            throw new UnauthorizedAccessException("Không thể xác định người dùng hợp lệ.");
        }


        // Helper format lương
        private static string FormatSalaryHelper(LoaiLuong loaiLuong, ulong? min, ulong? max) // Đổi tên để tránh trùng lặp nếu có service khác
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
            string prefix = loaiLuong switch { LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca", LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => "" };
            string FormatValue(ulong val) => val.ToString("N0");
            if (min.HasValue && max.HasValue && min > 0 && max > 0) { if (min == max) return $"{FormatValue(min.Value)}{prefix}"; return $"{FormatValue(min.Value)} - {FormatValue(max.Value)}{prefix}"; }
            if (min.HasValue && min > 0) return $"Từ {FormatValue(min.Value)}{prefix}";
            if (max.HasValue && max > 0) return $"Đến {FormatValue(max.Value)}{prefix}";
            try { return loaiLuong.GetDisplayName(); } catch { return loaiLuong.ToString(); }
        }

        // Helper lấy CSS class cho badge trạng thái ứng tuyển
        private string GetTrangThaiBadgeClass(TrangThaiUngTuyen trangThai)
        {
             return trangThai switch
            {
                TrangThaiUngTuyen.danop => "bg-secondary",
                TrangThaiUngTuyen.ntddaxem => "bg-info text-dark",
                TrangThaiUngTuyen.bituchoi => "bg-danger",
                TrangThaiUngTuyen.daduyet => "bg-success",
                TrangThaiUngTuyen.darut => "bg-warning text-dark",
                _ => "bg-light text-dark",
            };
        }


        //=========================================
        // VIỆC ĐÃ LƯU
        //=========================================

        // GET: viec-lam/da-luu
         [HttpGet("da-luu")]
        public async Task<IActionResult> DaLuu(string? tuKhoa, string? sapXepThoiGian, int page = 1)
        {
            int currentUserId = GetCurrentUserId();
            int pageSize = 5;
            page = Math.Max(1, page);

            var savedJobsQuery = _context.TinDaLuus
                .Where(tdl => tdl.NguoiDungId == currentUserId)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                .Where(tdl => tdl.TinTuyenDung != null)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string keyword = tuKhoa.ToLower().Trim();
                savedJobsQuery = savedJobsQuery.Where(x =>
                    (x.TinTuyenDung.TieuDe != null && EF.Functions.Like(x.TinTuyenDung.TieuDe, $"%{keyword}%")) ||
                    (x.TinTuyenDung.NguoiDang != null && x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null && x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy != null && EF.Functions.Like(x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy, $"%{keyword}%")) ||
                    (x.TinTuyenDung.NguoiDang != null && x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.canhan && x.TinTuyenDung.NguoiDang.HoTen != null && EF.Functions.Like(x.TinTuyenDung.NguoiDang.HoTen, $"%{keyword}%")) ||
                    (x.TinTuyenDung.TinTuyenDungNganhNghes != null && x.TinTuyenDung.TinTuyenDungNganhNghes.Any(tnn => tnn.NganhNghe != null && tnn.NganhNghe.Ten != null && EF.Functions.Like(tnn.NganhNghe.Ten, $"%{keyword}%")))
                );
            }

            if (string.IsNullOrWhiteSpace(sapXepThoiGian))
            {
                sapXepThoiGian = "moinhat";
            }

            switch (sapXepThoiGian.ToLower())
            {
                case "cunhat": savedJobsQuery = savedJobsQuery.OrderBy(x => x.NgayLuu); break;
                case "moinhat": default: savedJobsQuery = savedJobsQuery.OrderByDescending(x => x.NgayLuu); break;
            }

            var paginatedResult = await PaginatedList<TinDaLuu>.CreateAsync(savedJobsQuery, page, pageSize);

            // Lấy danh sách ID các tin tuyển dụng mà người dùng hiện tại đã ứng tuyển
            var appliedJobIds = await _context.UngTuyens
                                     .Where(ut => ut.UngVienId == currentUserId)
                                     .Select(ut => ut.TinTuyenDungId)
                                     .Distinct() // Đảm bảo không có ID trùng lặp
                                     .ToListAsync();
            var appliedJobIdsSet = new HashSet<int>(appliedJobIds); // Chuyển sang HashSet để kiểm tra Contains() nhanh hơn

            var items = paginatedResult.Select(x => new SavedJobItemViewModel {
                 TinTuyenDungId = x.TinTuyenDungId,
                 TinDaLuuId = x.Id,
                 TieuDe = x.TinTuyenDung.TieuDe ?? "N/A",
                 TenCongTyHoacNguoiDang = x.TinTuyenDung.NguoiDang == null ? "Không rõ" : (x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? x.TinTuyenDung.NguoiDang.HoTen ?? "Doanh nghiệp không rõ") : (x.TinTuyenDung.NguoiDang.HoTen ?? "Người đăng không rõ")),
                 LogoUrl = x.TinTuyenDung.NguoiDang == null ? null : (x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo : x.TinTuyenDung.NguoiDang.UrlAvatar),
                 LoaiTkNguoiDang = x.TinTuyenDung.NguoiDang?.LoaiTk ?? LoaiTaiKhoan.canhan, // Sử dụng giá trị mặc định phù hợp
                 DiaDiem = (x.TinTuyenDung.QuanHuyen?.Ten != null ? x.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (x.TinTuyenDung.ThanhPho?.Ten ?? ""),
                 MucLuongDisplay = FormatSalaryHelper(x.TinTuyenDung.LoaiLuong, x.TinTuyenDung.LuongToiThieu, x.TinTuyenDung.LuongToiDa),
                 LoaiHinhDisplay = x.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                 NgayHetHan = x.TinTuyenDung.NgayHetHan,
                 Tags = x.TinTuyenDung.TinTuyenDungNganhNghes == null ? new List<string>() : x.TinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe?.Ten ?? "").Where(s=>!string.IsNullOrEmpty(s)).Take(2).ToList(),
                 NgayLuu = x.NgayLuu,
                 DaUngTuyen = appliedJobIdsSet.Contains(x.TinTuyenDungId) // <<--- GÁN GIÁ TRỊ Ở ĐÂY
            }).ToList();

            var sapXepOptionsLuu = new List<SelectListItem>
            {
                new SelectListItem { Value = "moinhat", Text = "Ngày lưu (Mới nhất)" },
                new SelectListItem { Value = "cunhat", Text = "Ngày lưu (Cũ nhất)" }
            };

            var viewModel = new DaLuuViewModel {
                TuKhoa = tuKhoa,
                SapXepThoiGian = sapXepThoiGian,
                SavedJobs = new PaginatedList<SavedJobItemViewModel>(items, paginatedResult.TotalCount, paginatedResult.PageIndex, pageSize),
                SapXepThoiGianOptions = new SelectList(sapXepOptionsLuu, "Value", "Text", sapXepThoiGian)
            };
            
            TempData["CurrentSavedJobSearchTerm"] = tuKhoa;
            TempData["CurrentSavedJobSortOrder"] = sapXepThoiGian;
            return View(viewModel);
        }

        // POST: viec-lam/bo-luu-tin-da-luu
        [HttpPost("bo-luu-tin-da-luu")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoLuuTinDaLuu(int tinDaLuuId)
        {
            int currentUserId = GetCurrentUserId();
            if (tinDaLuuId <= 0) { TempData["ErrorMessage"] = "Yêu cầu không hợp lệ."; return RedirectToAction(nameof(DaLuu)); }

            var tinCanXoa = await _context.TinDaLuus
                                       .FirstOrDefaultAsync(tdl => tdl.Id == tinDaLuuId && tdl.NguoiDungId == currentUserId);

            if (tinCanXoa == null) { TempData["ErrorMessage"] = "Không tìm thấy tin đã lưu hoặc bạn không có quyền xóa."; return RedirectToAction(nameof(DaLuu)); }

            _context.TinDaLuus.Remove(tinCanXoa);
            try {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã bỏ lưu tin thành công.";
            }
            catch(Exception ex) {
                TempData["ErrorMessage"] = "Lỗi khi bỏ lưu tin.";
                 _logger.LogError(ex, "Error deleting saved job ID {TinDaLuuId} for User {UserID}", tinDaLuuId, currentUserId);
            }
            // Lấy lại từ khóa tìm kiếm từ TempData để redirect
            var currentSearchTerm = TempData["CurrentSavedJobSearchTerm"] as string;
            return RedirectToAction(nameof(DaLuu), new { tuKhoa = currentSearchTerm });
        }

         // POST: viec-lam/xoa-tat-ca-tin-da-luu
        [HttpPost("xoa-tat-ca-tin-da-luu")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCaTinDaLuu()
        {
            int currentUserId = GetCurrentUserId();
             var allSavedJobs = await _context.TinDaLuus
                                         .Where(tdl => tdl.NguoiDungId == currentUserId)
                                         .ToListAsync();

             if (!allSavedJobs.Any()) { TempData["InfoMessage"] = "Không có tin nào để xóa."; return RedirectToAction(nameof(DaLuu)); }

             _context.TinDaLuus.RemoveRange(allSavedJobs);
             try {
                 int count = await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = $"Đã xóa {count} tin đã lưu.";
             }
             catch(Exception ex) {
                 TempData["ErrorMessage"] = "Lỗi khi xóa tất cả tin.";
                  _logger.LogError(ex, "Error deleting all saved jobs for User {UserID}", currentUserId);
             }
            return RedirectToAction(nameof(DaLuu));
        }


        //=========================================
        // VIỆC ĐÃ ỨNG TUYỂN
        //=========================================

        // GET: viec-lam/da-ung-tuyen
        // GET: viec-lam/da-ung-tuyen
        [HttpGet("da-ung-tuyen")]
        public async Task<IActionResult> DaUngTuyen(string? tuKhoa, TrangThaiUngTuyen? trangThaiFilter, string? sapXepThoiGian, int page = 1)
        {
            int currentUserId = GetCurrentUserId();
            int pageSize = 5; // Bạn có thể điều chỉnh pageSize nếu muốn
            page = Math.Max(1, page);

            var query = _context.UngTuyens
                .Where(ut => ut.UngVienId == currentUserId)
                .Include(ut => ut.TinTuyenDung)
                    .ThenInclude(ttd => ttd.NguoiDang)
                    .ThenInclude(nd => nd.HoSoDoanhNghiep) // Cho TenNhaTuyenDung, LogoUrl
                .Include(ut => ut.TinTuyenDung)
                    .ThenInclude(ttd => ttd.ThanhPho)    // Cho DiaDiem
                .Include(ut => ut.TinTuyenDung)
                    .ThenInclude(ttd => ttd.QuanHuyen)   // Cho DiaDiem
                .Include(ut => ut.TinTuyenDung) // <<--- BAO GỒM CHO TAGS
                    .ThenInclude(ttd => ttd.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .Where(ut => ut.TinTuyenDung != null) // Đảm bảo có tin tuyển dụng liên kết
                .AsNoTracking();

            if (trangThaiFilter.HasValue)
            {
                query = query.Where(ut => ut.TrangThai == trangThaiFilter.Value);
            }

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                string searchTerm = tuKhoa.ToLower().Trim();
                query = query.Where(ut =>
                    (ut.TinTuyenDung.TieuDe != null && EF.Functions.Like(ut.TinTuyenDung.TieuDe, $"%{searchTerm}%")) ||
                    (ut.TinTuyenDung.NguoiDang != null && ut.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null && ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy != null && EF.Functions.Like(ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy, $"%{searchTerm}%")) ||
                    (ut.TinTuyenDung.NguoiDang != null && ut.TinTuyenDung.NguoiDang.LoaiTk != LoaiTaiKhoan.doanhnghiep && ut.TinTuyenDung.NguoiDang.HoTen != null && EF.Functions.Like(ut.TinTuyenDung.NguoiDang.HoTen, $"%{searchTerm}%"))
                    // Bạn có thể thêm tìm kiếm theo Tags ở đây nếu muốn
                    // || (ut.TinTuyenDung.TinTuyenDungNganhNghes != null && ut.TinTuyenDung.TinTuyenDungNganhNghes.Any(tnn => tnn.NganhNghe != null && tnn.NganhNghe.Ten != null && EF.Functions.Like(tnn.NganhNghe.Ten, $"%{searchTerm}%")))
                );
            }
            
            // Tính toán số lượng cho từng trạng thái
            var statusCounts = await _context.UngTuyens
                                          .Where(ut => ut.UngVienId == currentUserId)
                                          .GroupBy(ut => ut.TrangThai)
                                          .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                                          .ToDictionaryAsync(x => x.TrangThai, x => x.Count);
            int totalApplicationsForAllStatuses = await _context.UngTuyens.CountAsync(ut => ut.UngVienId == currentUserId);

            if (string.IsNullOrWhiteSpace(sapXepThoiGian))
            {
                sapXepThoiGian = "ngaynop_moinhat"; 
            }

            switch (sapXepThoiGian.ToLower())
            {
                case "ngaynop_cunhat":
                    query = query.OrderBy(ut => ut.NgayNop);
                    break;
                case "ngaycapnhat_moinhat":
                    query = query.OrderByDescending(ut => ut.NgayCapNhatTrangThai ?? ut.NgayNop);
                    break;
                case "ngaycapnhat_cunhat":
                    query = query.OrderBy(ut => ut.NgayCapNhatTrangThai ?? ut.NgayNop);
                    break;
                case "ngaynop_moinhat":
                default:
                    query = query.OrderByDescending(ut => ut.NgayNop);
                    break;
            }

            var paginatedApplications = await PaginatedList<UngTuyen>.CreateAsync(query, page, pageSize);

            var viewModelItems = paginatedApplications.Select(ut =>
            {
                string tenNtd = "Không rõ"; 
                string? logoUrl = null;
                 if (ut.TinTuyenDung.NguoiDang != null) {
                     if (ut.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                     { 
                         tenNtd = ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy ?? "Doanh nghiệp"; 
                         logoUrl = ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo; 
                     }
                     else 
                     { 
                         tenNtd = ut.TinTuyenDung.NguoiDang.HoTen ?? "Cá nhân"; 
                         logoUrl = ut.TinTuyenDung.NguoiDang.UrlAvatar; 
                     }
                 }

                return new DaUngTuyenItemViewModel {
                    UngTuyenId = ut.Id, 
                    TinTuyenDungId = ut.TinTuyenDungId,
                    TieuDeCongViec = ut.TinTuyenDung.TieuDe ?? "N/A", // Null check cho TieuDe
                    TenNhaTuyenDung = tenNtd, 
                    LogoUrl = logoUrl,
                    DiaDiem = (ut.TinTuyenDung.QuanHuyen?.Ten != null ? ut.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (ut.TinTuyenDung.ThanhPho?.Ten ?? "N/A"),
                    MucLuongDisplay = FormatSalaryHelper(ut.TinTuyenDung.LoaiLuong, ut.TinTuyenDung.LuongToiThieu, ut.TinTuyenDung.LuongToiDa),
                    LoaiHinhCongViecDisplay = ut.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(), 
                    NgayNop = ut.NgayNop,
                    NgayCapNhatTrangThai = ut.NgayCapNhatTrangThai,
                    ThuGioiThieuSnippet = ut.ThuGioiThieu?.Length > 70 ? ut.ThuGioiThieu.Substring(0, 70) + "..." : ut.ThuGioiThieu, // Rút gọn snippet một chút
                    TrangThai = ut.TrangThai, 
                    TrangThaiDisplay = ut.TrangThai.GetDisplayName(), 
                    TrangThaiBadgeClass = GetTrangThaiBadgeClass(ut.TrangThai),
                    
                    // Gán giá trị cho các thuộc tính mới
                    NgayHetHan = ut.TinTuyenDung.NgayHetHan, // <<--- MAP NgayHetHan
                    Tags = ut.TinTuyenDung.TinTuyenDungNganhNghes == null ? new List<string>() // <<--- MAP Tags
                           : ut.TinTuyenDung.TinTuyenDungNganhNghes
                               .Select(tnn => tnn.NganhNghe?.Ten ?? "")
                               .Where(s => !string.IsNullOrEmpty(s))
                               .Take(2) // Lấy tối đa 2 tags
                               .ToList(),

                    // Flags điều khiển nút
                    CanEdit = ut.TrangThai == TrangThaiUngTuyen.danop || ut.TrangThai == TrangThaiUngTuyen.ntddaxem,
                    CanWithdraw = ut.TrangThai == TrangThaiUngTuyen.danop || ut.TrangThai == TrangThaiUngTuyen.ntddaxem,
                    CanUndoWithdrawal = ut.TrangThai == TrangThaiUngTuyen.darut && 
                                        ut.NgayCapNhatTrangThai.HasValue &&
                                        (DateTime.Now - ut.NgayCapNhatTrangThai.Value).TotalDays <= 7, // Có thể hoàn tác trong 7 ngày
                    CanDeletePermanently = ut.TrangThai == TrangThaiUngTuyen.darut || ut.TrangThai == TrangThaiUngTuyen.bituchoi,
                    CanContact = ut.TrangThai == TrangThaiUngTuyen.daduyet
                };
            }).ToList();

            var sapXepOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "ngaynop_moinhat", Text = "Ngày nộp (Mới nhất)" },
                new SelectListItem { Value = "ngaynop_cunhat", Text = "Ngày nộp (Cũ nhất)" },
                new SelectListItem { Value = "ngaycapnhat_moinhat", Text = "Ngày cập nhật (Mới nhất)" },
                new SelectListItem { Value = "ngaycapnhat_cunhat", Text = "Ngày cập nhật (Cũ nhất)" }
            };

            var viewModel = new DaUngTuyenViewModel {
                TuKhoa = tuKhoa, 
                TrangThaiFilter = trangThaiFilter,
                SapXepThoiGian = sapXepThoiGian,
                UngTuyenItems = new PaginatedList<DaUngTuyenItemViewModel>(viewModelItems, paginatedApplications.TotalCount, paginatedApplications.PageIndex, pageSize),
                TrangThaiOptions = new SelectList(EnumExtensions.GetSelectList<TrangThaiUngTuyen>(includeDefaultItem: true, defaultItemText:"Tất cả trạng thái", defaultItemValue:""), "Value", "Text", trangThaiFilter?.ToString()),
                SapXepThoiGianOptions = new SelectList(sapXepOptions, "Value", "Text", sapXepThoiGian),
                StatusCounts = statusCounts, 
                TotalCount = totalApplicationsForAllStatuses
            };
            return View(viewModel);
        }

        // ... (Action SuaUngTuyen, RutDonUngTuyen, XoaUngTuyenVinhVien giữ nguyên như phiên bản trước)
        [HttpGet("sua-ung-tuyen/{id:int}")]
        public async Task<IActionResult> SuaUngTuyen(int id)
        {
            int currentUserId = GetCurrentUserId();
            var ungTuyen = await _context.UngTuyens
                .Include(ut => ut.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .FirstOrDefaultAsync(ut => ut.Id == id && ut.UngVienId == currentUserId);

            if (ungTuyen == null) {
                 _logger.LogWarning("Attempt to edit non-existent or unauthorized UngTuyen ID {UngTuyenId} by User {UserId}", id, currentUserId);
                 return NotFound("Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền chỉnh sửa.");
            }
            if (!(ungTuyen.TrangThai == TrangThaiUngTuyen.danop || ungTuyen.TrangThai == TrangThaiUngTuyen.ntddaxem))
            {
                TempData["ErrorMessage"] = $"Không thể sửa đơn ứng tuyển đang ở trạng thái '{ungTuyen.TrangThai.GetDisplayName()}'.";
                return RedirectToAction(nameof(DaUngTuyen));
            }
            string tenNtd = "";
             if (ungTuyen.TinTuyenDung.NguoiDang != null) {
                 if (ungTuyen.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && ungTuyen.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                     tenNtd = ungTuyen.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy ?? "Doanh nghiệp";
                 else tenNtd = ungTuyen.TinTuyenDung.NguoiDang.HoTen;
             } else { tenNtd = "Không rõ"; }
            var viewModel = new SuaUngTuyenViewModel {
                UngTuyenId = ungTuyen.Id, TinTuyenDungId = ungTuyen.TinTuyenDungId,
                TieuDeCongViec = ungTuyen.TinTuyenDung.TieuDe, TenNhaTuyenDung = tenNtd,
                NgayNop = ungTuyen.NgayNop, ThuGioiThieu = ungTuyen.ThuGioiThieu,
                UrlCvHienTai = ungTuyen.UrlCvDaNop
            };
            return View(viewModel);
        }
                // THÊM ACTION MỚI ĐỂ HOÀN TÁC RÚT ĐƠN
              [HttpPost("hoan-tac-rut-don")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanTacRutDon([FromBody] int ungTuyenId)
        {
            if (ungTuyenId <= 0) return BadRequest(new { success = false, message = "ID đơn ứng tuyển không hợp lệ." });

            int currentUserId = GetCurrentUserId();
            var ungTuyen = await _context.UngTuyens
                // Có thể include TinTuyenDung nếu bạn cần kiểm tra điều kiện gì đó liên quan đến tin, ví dụ: còn hạn không.
                // .Include(ut => ut.TinTuyenDung) 
                .FirstOrDefaultAsync(ut => ut.Id == ungTuyenId && ut.UngVienId == currentUserId);

            if (ungTuyen == null) return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền thực hiện." });

            // Optional: Kiểm tra xem tin tuyển dụng có còn hoạt động/còn hạn không trước khi cho nộp lại
            // if (ungTuyen.TinTuyenDung != null && ungTuyen.TinTuyenDung.NgayHetHan.HasValue && ungTuyen.TinTuyenDung.NgayHetHan.Value < DateTime.UtcNow.Date)
            // {
            //     return BadRequest(new { success = false, message = "Không thể nộp lại vì tin tuyển dụng đã hết hạn." });
            // }

            if (ungTuyen.TrangThai == TrangThaiUngTuyen.darut)
            {
                TrangThaiUngTuyen oldStatus = ungTuyen.TrangThai;
                
                // Cập nhật trạng thái
                ungTuyen.TrangThai = TrangThaiUngTuyen.danop; 
                
                // Cập nhật ngày nộp thành thời điểm hiện tại (coi như nộp lại)
                ungTuyen.NgayNop = DateTime.UtcNow; 
                
                // Cập nhật ngày cập nhật trạng thái
                ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow; 

                _context.Entry(ungTuyen).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Người dùng {UserId} đã HOÀN TÁC RÚT đơn UngTuyen ID {UngTuyenId}, chuyển về trạng thái 'danop' và cập nhật ngày nộp.", currentUserId, ungTuyenId);
                    
                    // Trả về thêm ngày nộp mới để cập nhật UI nếu cần
                    return Ok(new { 
                        success = true, 
                        message = "Đã hoàn tác rút đơn thành công. Đơn đã được nộp lại.",
                        newStatus = TrangThaiUngTuyen.danop.ToString(),
                        newStatusDisplay = TrangThaiUngTuyen.danop.GetDisplayName(),
                        newStatusBadgeClass = GetTrangThaiBadgeClass(TrangThaiUngTuyen.danop),
                        newNgayNop = ungTuyen.NgayNop.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), // Gửi về giờ local cho UI
                        newNgayNopRelative = TimeHelper.TimeAgo(ungTuyen.NgayNop), // Gửi về dạng "x phút trước"
                        newNgayCapNhatTrangThai = ungTuyen.NgayCapNhatTrangThai.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                        newNgayCapNhatTrangThaiRelative = TimeHelper.TimeAgo(ungTuyen.NgayCapNhatTrangThai.Value),
                        oldStatus = oldStatus.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi hoàn tác rút đơn UngTuyen ID {UngTuyenId} bởi người dùng {UserId}", ungTuyenId, currentUserId);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Lỗi máy chủ khi hoàn tác rút đơn." });
                }
            }
            else
            {
                return BadRequest(new { success = false, message = $"Không thể hoàn tác rút đơn vì đơn không ở trạng thái 'Đã rút'." });
            }
        }

        [HttpPost("sua-ung-tuyen/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaUngTuyen(int id, SuaUngTuyenViewModel model)
        {
             if (id != model.UngTuyenId) return BadRequest("ID không khớp.");
            int currentUserId = GetCurrentUserId();
            var ungTuyen = await _context.UngTuyens
                .Include(ut => ut.TinTuyenDung)
                .FirstOrDefaultAsync(ut => ut.Id == id && ut.UngVienId == currentUserId);
            if (ungTuyen == null) return NotFound("Không tìm thấy đơn ứng tuyển.");
             if (!(ungTuyen.TrangThai == TrangThaiUngTuyen.danop || ungTuyen.TrangThai == TrangThaiUngTuyen.ntddaxem))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn ứng tuyển đã thay đổi trong lúc bạn sửa, không thể lưu.";
                var ntdRetry = await _context.NguoiDungs.Include(n=>n.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(n=>n.Id == ungTuyen.TinTuyenDung.NguoiDangId);
                model.TieuDeCongViec = ungTuyen.TinTuyenDung.TieuDe; model.TenNhaTuyenDung = ntdRetry?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? ntdRetry.HoSoDoanhNghiep?.TenCongTy ?? "DN" : ntdRetry?.HoTen ?? "N/A"; model.NgayNop = ungTuyen.NgayNop; model.UrlCvHienTai = ungTuyen.UrlCvDaNop; 
                ModelState.AddModelError("", "Trạng thái đơn ứng tuyển đã thay đổi, không thể lưu.");
                return View(model);
            }
             string? newCvUrl = ungTuyen.UrlCvDaNop; 
             string? oldCvPhysicalPath = null; 
             bool isNewCvUploaded = false; 
             if (model.CvMoi != null && model.CvMoi.Length > 0)
             {
                 if (model.CvMoi.Length > 5 * 1024 * 1024) ModelState.AddModelError(nameof(model.CvMoi), "Kích thước file CV mới không được vượt quá 5MB.");
                 string[] permittedExtensions = { ".pdf", ".doc", ".docx" };
                 var ext = Path.GetExtension(model.CvMoi.FileName)?.ToLowerInvariant();
                 if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext)) ModelState.AddModelError(nameof(model.CvMoi), "Chỉ chấp nhận file CV định dạng PDF, DOC, DOCX.");
                 if (ModelState.GetFieldValidationState(nameof(model.CvMoi)) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
                 {
                      if (!string.IsNullOrEmpty(ungTuyen.UrlCvDaNop)) { try { var relativePath = ungTuyen.UrlCvDaNop.TrimStart('/'); oldCvPhysicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath); } catch (Exception pathEx){ _logger.LogError(pathEx, "Lỗi lấy đường dẫn vật lý cho CV cũ: {Url}", ungTuyen.UrlCvDaNop);} }
                     string cvUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "file", "CV");
                     if (!Directory.Exists(cvUploadsFolder)) { try { Directory.CreateDirectory(cvUploadsFolder); } catch (Exception dirEx) { _logger.LogError(dirEx, "Không thể tạo thư mục CV: {Path}", cvUploadsFolder); ModelState.AddModelError("", "Lỗi hệ thống khi tạo thư mục lưu CV."); var ntdRetryDir = await _context.NguoiDungs.Include(n=>n.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(n=>n.Id == ungTuyen.TinTuyenDung.NguoiDangId); model.TieuDeCongViec = ungTuyen.TinTuyenDung.TieuDe; model.TenNhaTuyenDung = ntdRetryDir?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? ntdRetryDir.HoSoDoanhNghiep?.TenCongTy ?? "DN" : ntdRetryDir?.HoTen ?? "N/A"; model.NgayNop = ungTuyen.NgayNop; model.UrlCvHienTai = ungTuyen.UrlCvDaNop; return View(model); } }
                     string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CvMoi.FileName);
                     string newFilePath = Path.Combine(cvUploadsFolder, uniqueFileName);
                     try { using (var fileStream = new FileStream(newFilePath, FileMode.Create)) { await model.CvMoi.CopyToAsync(fileStream); } newCvUrl = "/file/CV/" + uniqueFileName; isNewCvUploaded = true; _logger.LogInformation("CV mới đã được tải lên thành công: {FilePath} cho UngTuyen ID {UngTuyenId}", newFilePath, id); } 
                     catch (Exception ex) { _logger.LogError(ex, "Lỗi khi upload CV mới cho UngTuyen ID {UngTuyenId}", id); ModelState.AddModelError("", "Lỗi khi tải lên CV mới."); newCvUrl = ungTuyen.UrlCvDaNop; oldCvPhysicalPath = null; isNewCvUploaded = false; }
                 }
             }
            if (!ModelState.IsValid) { var ntdRetryFinal = await _context.NguoiDungs.Include(n=>n.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(n=>n.Id == ungTuyen.TinTuyenDung.NguoiDangId); model.TieuDeCongViec = ungTuyen.TinTuyenDung.TieuDe; model.TenNhaTuyenDung = ntdRetryFinal?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? ntdRetryFinal.HoSoDoanhNghiep?.TenCongTy ?? "DN" : ntdRetryFinal?.HoTen ?? "N/A"; model.NgayNop = ungTuyen.NgayNop; model.UrlCvHienTai = ungTuyen.UrlCvDaNop; _logger.LogWarning("ModelState không hợp lệ khi chuẩn bị lưu sửa UngTuyen ID {UngTuyenId}", id); return View(model); }
            ungTuyen.ThuGioiThieu = model.ThuGioiThieu; ungTuyen.UrlCvDaNop = newCvUrl; ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow; 
            _context.Entry(ungTuyen).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); if (isNewCvUploaded && !string.IsNullOrEmpty(oldCvPhysicalPath) && System.IO.File.Exists(oldCvPhysicalPath)) { try { System.IO.File.Delete(oldCvPhysicalPath); _logger.LogInformation("CV cũ đã được xóa thành công: {FilePath}", oldCvPhysicalPath); } catch(IOException ioEx) { _logger.LogWarning(ioEx, "Không thể xóa file CV cũ sau khi cập nhật UngTuyen ID {UngTuyenId}: {FilePath}", id, oldCvPhysicalPath); } } TempData["SuccessMessage"] = "Cập nhật đơn ứng tuyển thành công."; return RedirectToAction(nameof(DaUngTuyen)); }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, "Lỗi xung đột khi sửa UngTuyen ID {UngTuyenId}", id); ModelState.AddModelError("", "Có lỗi xảy ra khi lưu, đơn ứng tuyển này có thể đã được cập nhật. Vui lòng thử lại."); }
            catch (Exception ex) { _logger.LogError(ex, "Lỗi không xác định khi sửa UngTuyen ID {UngTuyenId}", id); ModelState.AddModelError("", "Lỗi không mong muốn xảy ra khi lưu thay đổi."); }
             var ntdRetryDbError = await _context.NguoiDungs.Include(n=>n.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(n=>n.Id == ungTuyen.TinTuyenDung.NguoiDangId); model.TieuDeCongViec = ungTuyen.TinTuyenDung.TieuDe; model.TenNhaTuyenDung = ntdRetryDbError?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? ntdRetryDbError.HoSoDoanhNghiep?.TenCongTy ?? "DN" : ntdRetryDbError?.HoTen ?? "N/A"; model.NgayNop = ungTuyen.NgayNop; model.UrlCvHienTai = ungTuyen.UrlCvDaNop; 
            return View(model);
        }

             [HttpPost("rut-don-ung-tuyen")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RutDonUngTuyen([FromBody] int ungTuyenId) // Nhận ungTuyenId từ body của request AJAX
        {
            if (ungTuyenId <= 0)
            {
                return BadRequest(new { success = false, message = "ID đơn ứng tuyển không hợp lệ." });
            }

            int currentUserId = GetCurrentUserId(); // ID của ứng viên đang thực hiện hành động

            // <<<<<<< BAO GỒM THÔNG TIN CẦN THIẾT ĐỂ GỬI THÔNG BÁO >>>>>>>
            var ungTuyen = await _context.UngTuyens
                .Include(ut => ut.TinTuyenDung) // Để lấy TinTuyenDung.TieuDe và TinTuyenDung.NguoiDangId (NTD)
                    .ThenInclude(ttd => ttd.NguoiDang) // Load NguoiDang để lấy HoTen/TenCongTy của NTD (không bắt buộc cho thông báo này nhưng có thể hữu ích nếu cần)
                        // .ThenInclude(nd => nd.HoSoDoanhNghiep) // Chỉ cần nếu muốn lấy tên công ty NTD cho nội dung thông báo
                .Include(ut => ut.UngVien) // Để lấy thông tin người rút đơn (ứng viên)
                    .ThenInclude(uv => uv.HoSoUngVien) // Lấy HoSoUngVien để có thể dùng TieuDeHoSo cho tên ứng viên
                .FirstOrDefaultAsync(ut => ut.Id == ungTuyenId && ut.UngVienId == currentUserId);

            if (ungTuyen == null)
            {
                _logger.LogWarning("Người dùng {UserId} cố gắng rút đơn UngTuyen ID {UngTuyenId} không tồn tại hoặc không thuộc sở hữu.", currentUserId, ungTuyenId);
                return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền thực hiện hành động này." });
            }

            // Chỉ cho phép rút đơn nếu đang ở trạng thái "Đã nộp" hoặc "NTD đã xem"
            if (ungTuyen.TrangThai == TrangThaiUngTuyen.danop || ungTuyen.TrangThai == TrangThaiUngTuyen.ntddaxem)
            {
                var oldStatus = ungTuyen.TrangThai; // Lưu trạng thái cũ để ghi log
                ungTuyen.TrangThai = TrangThaiUngTuyen.darut;
                ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow;
                // _context.Entry(ungTuyen).State = EntityState.Modified; // Không cần thiết nếu entity đã được theo dõi

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Người dùng {UserId} đã RÚT đơn UngTuyen ID {UngTuyenId} từ trạng thái {OldStatus}.", currentUserId, ungTuyenId, oldStatus);

                    // --- BẮT ĐẦU GỬI THÔNG BÁO CHO NHÀ TUYỂN DỤNG ---
                    // Đảm bảo có đủ thông tin để gửi thông báo
                    if (ungTuyen.TinTuyenDung != null && ungTuyen.TinTuyenDung.NguoiDangId != 0 && ungTuyen.UngVien != null)
                    {
                        // Lấy tên hiển thị của ứng viên đã rút đơn
                        string? tenUngVienDisplay = ungTuyen.UngVien?.HoTen;
                        if (string.IsNullOrWhiteSpace(tenUngVienDisplay))
                        {
                            tenUngVienDisplay = $"Ứng viên #{ungTuyen.UngVienId}"; // Fallback
                        }

                        var duLieuThongBao = new
                        {
                            tenUngVien = tenUngVienDisplay,
                            avatarUngVien = ungTuyen.UngVien != null && !string.IsNullOrEmpty(ungTuyen.UngVien.UrlAvatar) ? ungTuyen.UngVien.UrlAvatar : "/images/avatars/default_user.png", // Avatar của ứng viên
                            tieuDeTin = ungTuyen.TinTuyenDung.TieuDe,
                            ungTuyenId = ungTuyen.Id, // ID của đơn ứng tuyển
                            tinId = ungTuyen.TinTuyenDungId, // ID của tin tuyển dụng
                            noiDung = $"{tenUngVienDisplay} đã rút hồ sơ ứng tuyển khỏi vị trí \"{ungTuyen.TinTuyenDung.TieuDe}\" của bạn.",
                            // URL để NTD xem chi tiết ứng tuyển (nơi họ sẽ thấy trạng thái là "Đã rút")
                            // Hoặc xem chi tiết hồ sơ ứng viên trong context của đơn ứng tuyển này
                            url = Url.Action("ChiTietHoSo", "QuanLyUngVien", new {
                                area = "NhaTuyenDung", // Area của NTD
                                ungVienId = ungTuyen.UngVienId, // ID của ứng viên
                                ungTuyenId = ungTuyen.Id // ID của đơn ứng tuyển để làm context
                            }, Request.Scheme)
                        };

                        try
                        {
                            await _thongBaoService.CreateThongBaoAsync(
                                ungTuyen.TinTuyenDung.NguoiDangId, // ID của Nhà Tuyển Dụng (người đăng tin)
                                NotificationConstants.Types.UngTuyenUngVienRut,
                                JsonSerializer.Serialize(duLieuThongBao),
                                NotificationConstants.RelatedEntities.UngTuyen,
                                ungTuyen.Id // IdLienQuan là ID của đơn ứng tuyển
                            );
                            _logger.LogInformation("Đã gửi thông báo 'ỨNG VIÊN RÚT ĐƠN' cho NTD ID {NtdId} về UngTuyen ID {UngTuyenId}.", ungTuyen.TinTuyenDung.NguoiDangId, ungTuyen.Id);
                        }
                        catch (Exception ex_notify)
                        {
                            _logger.LogError(ex_notify, "Lỗi gửi thông báo 'ỨNG VIÊN RÚT ĐƠN' cho NTD ID {NtdId}. UngTuyenID: {UngTuyenId}", ungTuyen.TinTuyenDung.NguoiDangId, ungTuyen.Id);
                            // Không làm gián đoạn response thành công cho người dùng nếu chỉ lỗi gửi thông báo
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Không thể gửi thông báo rút đơn cho NTD vì thiếu thông tin TinTuyenDung hoặc NguoiDang hoặc UngVien. UngTuyenID: {UngTuyenId}", ungTuyen.Id);
                    }
                    // --- KẾT THÚC GỬI THÔNG BÁO ---

                    return Ok(new {
                        success = true,
                        message = "Đã rút đơn ứng tuyển thành công.",
                        newStatus = TrangThaiUngTuyen.darut.ToString(),
                        newStatusDisplay = TrangThaiUngTuyen.darut.GetDisplayName(),
                        newBadgeClass = GetTrangThaiBadgeClass(TrangThaiUngTuyen.darut) // Trả về class badge mới
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu thay đổi rút đơn UngTuyen ID {UngTuyenId} bởi người dùng {UserId}", ungTuyenId, currentUserId);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Lỗi máy chủ khi thực hiện rút đơn." });
                }
            }
            else if (ungTuyen.TrangThai == TrangThaiUngTuyen.darut)
            {
                return Ok(new { success = true, message = "Đơn này đã được rút trước đó.", alreadyWithdrawn = true });
            }
            else
            {
                // Các trạng thái khác như Bị từ chối, Đã duyệt, ... không cho phép rút
                _logger.LogWarning("Người dùng {UserId} cố gắng rút đơn UngTuyen ID {UngTuyenId} đang ở trạng thái không hợp lệ: {TrangThai}", currentUserId, ungTuyenId, ungTuyen.TrangThai);
                return BadRequest(new { success = false, message = $"Không thể rút đơn ứng tuyển đang ở trạng thái '{ungTuyen.TrangThai.GetDisplayName()}'." });
            }
        }

        [HttpPost("xoa-ung-tuyen-vinh-vien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaUngTuyenVinhVien([FromBody] int ungTuyenId)
        {
             if (ungTuyenId <= 0) return BadRequest(new { success = false, message = "ID đơn ứng tuyển không hợp lệ." });
             int currentUserId = GetCurrentUserId();
             var ungTuyen = await _context.UngTuyens.FirstOrDefaultAsync(ut => ut.Id == ungTuyenId && ut.UngVienId == currentUserId);
             if (ungTuyen == null) return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền xóa." });
            if (!(ungTuyen.TrangThai == TrangThaiUngTuyen.darut || ungTuyen.TrangThai == TrangThaiUngTuyen.bituchoi))
            { return BadRequest(new { success = false, message = $"Chỉ có thể xóa đơn đã rút hoặc bị từ chối." }); }
             string? cvPhysicalPath = null;
             if (!string.IsNullOrEmpty(ungTuyen.UrlCvDaNop)) { try { var relativePath = ungTuyen.UrlCvDaNop.TrimStart('/'); cvPhysicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath); } catch (Exception pathEx){ _logger.LogError(pathEx, "Lỗi lấy đường dẫn vật lý cho CV Url khi xóa UngTuyen: {CvUrl}", ungTuyen.UrlCvDaNop); } }
             try {
                 _context.UngTuyens.Remove(ungTuyen);
                 int affectedRows = await _context.SaveChangesAsync();
                 if (affectedRows > 0) { _logger.LogInformation("Người dùng {UserId} đã XÓA VĨNH VIỄN đơn UngTuyen ID {UngTuyenId}", currentUserId, ungTuyenId); if (!string.IsNullOrEmpty(cvPhysicalPath) && System.IO.File.Exists(cvPhysicalPath)) { try { System.IO.File.Delete(cvPhysicalPath); _logger.LogInformation("Đã xóa file CV {FilePath} khi xóa vĩnh viễn UngTuyen ID {UngTuyenId}", cvPhysicalPath, ungTuyenId); } catch (IOException ioEx) { _logger.LogWarning(ioEx, "Không thể xóa file CV {FilePath} sau khi xóa vĩnh viễn UngTuyen ID {UngTuyenId}", cvPhysicalPath, ungTuyenId); } } return Ok(new { success = true, message = "Đã xóa vĩnh viễn đơn ứng tuyển." }); }
                 else { _logger.LogWarning("Xóa vĩnh viễn UngTuyen ID {UngTuyenId} bởi user {UserId} nhưng affectedRows = 0.", ungTuyenId, currentUserId); return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Không thể xóa đơn ứng tuyển khỏi cơ sở dữ liệu." }); }
             }
             catch (Exception ex) { _logger.LogError(ex, "Lỗi khi xóa vĩnh viễn đơn UngTuyen ID {UngTuyenId} bởi người dùng {UserId}", ungTuyenId, currentUserId); return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Đã có lỗi xảy ra phía máy chủ khi xóa đơn. Vui lòng thử lại." }); }
        }
    }
}