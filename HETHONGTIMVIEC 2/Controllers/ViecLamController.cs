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
using Microsoft.AspNetCore.Mvc.Rendering; // Cho SelectList

namespace HeThongTimViec.Controllers
{
    [Route("viec-lam")] // Route cơ sở cho controller này
    [Authorize] // Yêu cầu đăng nhập cho tất cả action trong controller này
    public class ViecLamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ViecLamController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // Cần cho xử lý file CV

        public ViecLamController(ApplicationDbContext context, ILogger<ViecLamController> logger, IWebHostEnvironment webHostEnvironment) // Thêm IWebHostEnvironment
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; // Gán giá trị
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
        public async Task<IActionResult> DaLuu(string? tuKhoa, int page = 1)
        {
            int currentUserId = GetCurrentUserId(); // Đã check Authorize nên có thể dùng non-nullable
            int pageSize = 10;

            var savedJobsQuery = _context.TinDaLuus
                .Where(tdl => tdl.NguoiDungId == currentUserId)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                .Include(tdl => tdl.TinTuyenDung).ThenInclude(ttd => ttd.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                .Where(tdl => tdl.TinTuyenDung != null) // Chỉ lấy những tin đã lưu mà tin tuyển dụng còn tồn tại
                .AsNoTracking();

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string keyword = tuKhoa.ToLower().Trim();
                savedJobsQuery = savedJobsQuery.Where(x =>
                    EF.Functions.Like(x.TinTuyenDung.TieuDe, $"%{keyword}%") ||
                    (x.TinTuyenDung.NguoiDang != null && x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null && EF.Functions.Like(x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy, $"%{keyword}%")) ||
                    (x.TinTuyenDung.NguoiDang != null && x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.canhan && EF.Functions.Like(x.TinTuyenDung.NguoiDang.HoTen, $"%{keyword}%")) ||
                    x.TinTuyenDung.TinTuyenDungNganhNghes.Any(tnn => EF.Functions.Like(tnn.NganhNghe.Ten, $"%{keyword}%"))
                );
            }

            savedJobsQuery = savedJobsQuery.OrderByDescending(x => x.NgayLuu);

            var paginatedResult = await PaginatedList<TinDaLuu>.CreateAsync(savedJobsQuery, page, pageSize);

            var items = paginatedResult.Select(x => new SavedJobItemViewModel {
                 TinTuyenDungId = x.TinTuyenDungId,
                 TinDaLuuId = x.Id,
                 TieuDe = x.TinTuyenDung.TieuDe,
                 TenCongTyHoacNguoiDang = x.TinTuyenDung.NguoiDang == null ? "Không rõ" : (x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? "Doanh nghiệp") : x.TinTuyenDung.NguoiDang.HoTen),
                 LogoUrl = x.TinTuyenDung.NguoiDang == null ? null : (x.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? x.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo : x.TinTuyenDung.NguoiDang.UrlAvatar),
                 LoaiTkNguoiDang = x.TinTuyenDung.NguoiDang == null ? default : x.TinTuyenDung.NguoiDang.LoaiTk,
                 DiaDiem = (x.TinTuyenDung.QuanHuyen?.Ten != null ? x.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (x.TinTuyenDung.ThanhPho?.Ten ?? ""),
                 MucLuongDisplay = FormatSalaryHelper(x.TinTuyenDung.LoaiLuong, x.TinTuyenDung.LuongToiThieu, x.TinTuyenDung.LuongToiDa),
                 LoaiHinhDisplay = x.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                 NgayHetHan = x.TinTuyenDung.NgayHetHan,
                 Tags = x.TinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).Take(2).ToList(), // Lấy 2 tags
                 NgayLuu = x.NgayLuu
            }).ToList();

            var viewModel = new DaLuuViewModel {
                TuKhoa = tuKhoa,
                SavedJobs = new PaginatedList<SavedJobItemViewModel>(items, paginatedResult.TotalCount, paginatedResult.PageIndex, pageSize)
            };
            // Lưu lại từ khóa tìm kiếm để dùng khi redirect sau khi xóa
            TempData["CurrentSavedJobSearchTerm"] = tuKhoa;
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
        [HttpGet("da-ung-tuyen")]
        public async Task<IActionResult> DaUngTuyen(string? tuKhoa, TrangThaiUngTuyen? trangThaiFilter, string? sapXepThoiGian, int page = 1)
        {
            int currentUserId = GetCurrentUserId();
            int pageSize = 5;
            page = Math.Max(1, page);

            var query = _context.UngTuyens
                .Where(ut => ut.UngVienId == currentUserId)
                .Include(ut => ut.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(ut => ut.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                .Include(ut => ut.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                .Where(ut => ut.TinTuyenDung != null)
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
                );
            }
            
            // Tính toán số lượng cho từng trạng thái (CHỈ DÙNG CHO HIỂN THỊ STATS, KHÔNG ẢNH HƯỞNG QUERY CHÍNH)
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
                string tenNtd = ""; string? logoUrl = null;
                 if (ut.TinTuyenDung.NguoiDang != null) {
                     if (ut.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                     { tenNtd = ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy ?? "Doanh nghiệp"; logoUrl = ut.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo; }
                     else { tenNtd = ut.TinTuyenDung.NguoiDang.HoTen ?? "Cá nhân"; logoUrl = ut.TinTuyenDung.NguoiDang.UrlAvatar; }
                 } else { tenNtd = "Không rõ"; }

                return new DaUngTuyenItemViewModel {
                    UngTuyenId = ut.Id, TinTuyenDungId = ut.TinTuyenDungId,
                    TieuDeCongViec = ut.TinTuyenDung.TieuDe, TenNhaTuyenDung = tenNtd, LogoUrl = logoUrl,
                    DiaDiem = (ut.TinTuyenDung.QuanHuyen?.Ten != null ? ut.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (ut.TinTuyenDung.ThanhPho?.Ten ?? ""),
                    MucLuongDisplay = FormatSalaryHelper(ut.TinTuyenDung.LoaiLuong, ut.TinTuyenDung.LuongToiThieu, ut.TinTuyenDung.LuongToiDa),
                    LoaiHinhCongViecDisplay = ut.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(), 
                    NgayNop = ut.NgayNop,
                    NgayCapNhatTrangThai = ut.NgayCapNhatTrangThai,
                    ThuGioiThieuSnippet = ut.ThuGioiThieu?.Length > 100 ? ut.ThuGioiThieu.Substring(0, 100) + "..." : ut.ThuGioiThieu,
                    TrangThai = ut.TrangThai, TrangThaiDisplay = ut.TrangThai.GetDisplayName(), TrangThaiBadgeClass = GetTrangThaiBadgeClass(ut.TrangThai),
                    CanEdit = ut.TrangThai == TrangThaiUngTuyen.danop || ut.TrangThai == TrangThaiUngTuyen.ntddaxem,
                    CanWithdraw = ut.TrangThai == TrangThaiUngTuyen.danop || ut.TrangThai == TrangThaiUngTuyen.ntddaxem,
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
                TotalCount = totalApplicationsForAllStatuses // Sử dụng total count của tất cả đơn
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
                .FirstOrDefaultAsync(ut => ut.Id == ungTuyenId && ut.UngVienId == currentUserId);

            if (ungTuyen == null) return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền thực hiện." });

            if (ungTuyen.TrangThai == TrangThaiUngTuyen.darut)
            {
                TrangThaiUngTuyen oldStatus = ungTuyen.TrangThai;
                // Quyết định trạng thái quay lại: có thể là ntddaxem nếu NTD đã từng xem, ngược lại là danop
                // Để đơn giản, ta cứ cho về danop, NTD xem lại sẽ tự chuyển thành ntddaxem
                ungTuyen.TrangThai = TrangThaiUngTuyen.danop; 
                ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow;
                _context.Entry(ungTuyen).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Người dùng {UserId} đã HOÀN TÁC RÚT đơn UngTuyen ID {UngTuyenId}, chuyển về trạng thái 'danop'", currentUserId, ungTuyenId);
                    return Ok(new { 
                        success = true, 
                        message = "Đã hoàn tác rút đơn thành công. Đơn đã được nộp lại.",
                        newStatus = TrangThaiUngTuyen.danop.ToString(),
                        newStatusDisplay = TrangThaiUngTuyen.danop.GetDisplayName(),
                        newStatusBadgeClass = GetTrangThaiBadgeClass(TrangThaiUngTuyen.danop),
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
        public async Task<IActionResult> RutDonUngTuyen([FromBody] int ungTuyenId)
        {
            if (ungTuyenId <= 0) return BadRequest(new { success = false, message = "ID đơn ứng tuyển không hợp lệ." });
            int currentUserId = GetCurrentUserId();
            var ungTuyen = await _context.UngTuyens.FirstOrDefaultAsync(ut => ut.Id == ungTuyenId && ut.UngVienId == currentUserId);
            if (ungTuyen == null) return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền thực hiện." });
            if (ungTuyen.TrangThai == TrangThaiUngTuyen.danop || ungTuyen.TrangThai == TrangThaiUngTuyen.ntddaxem)
            {
                ungTuyen.TrangThai = TrangThaiUngTuyen.darut;
                ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow;
                _context.Entry(ungTuyen).State = EntityState.Modified;
                try { await _context.SaveChangesAsync(); _logger.LogInformation("Người dùng {UserId} đã RÚT đơn UngTuyen ID {UngTuyenId}", currentUserId, ungTuyenId); return Ok(new { success = true, message = "Đã rút đơn ứng tuyển thành công.", newStatus = TrangThaiUngTuyen.darut.ToString(), newStatusDisplay = TrangThaiUngTuyen.darut.GetDisplayName() }); }
                catch (Exception ex) { _logger.LogError(ex, "Lỗi khi rút đơn UngTuyen ID {UngTuyenId} bởi người dùng {UserId}", ungTuyenId, currentUserId); return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Lỗi máy chủ khi rút đơn." }); }
            }
            else if (ungTuyen.TrangThai == TrangThaiUngTuyen.darut) { return Ok(new { success = true, message = "Đơn này đã được rút trước đó.", alreadyWithdrawn = true }); }
            else { return BadRequest(new { success = false, message = $"Không thể rút đơn ứng tuyển đang ở trạng thái '{ungTuyen.TrangThai.GetDisplayName()}'." }); }
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