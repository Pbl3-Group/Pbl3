// File: Controllers/BaoCaoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.BaoCao; // ViewModel cho trang này
using HeThongTimViec.ViewModels.TimViec; // For PaginatedList
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HeThongTimViec.Extensions;       // For EnumExtensions.GetDisplayName() và GetSelectList()
using System;
using Microsoft.AspNetCore.Http;         // Cho StatusCodes
using Microsoft.AspNetCore.Mvc.Rendering; // Cho SelectList
using System.Collections.Generic;       // Cho List<T>

namespace HeThongTimViec.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho tất cả các action trong controller này
    [Route("[controller]")] // Route cơ sở sẽ là /BaoCao
    public class BaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BaoCaoController> _logger;

        public BaoCaoController(ApplicationDbContext context, ILogger<BaoCaoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper lấy User ID hiện tại từ ClaimsPrincipal (User)
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            _logger.LogError("BaoCaoController: Không thể parse User ID từ ClaimsPrincipal. Claim value: {ClaimValue}", userIdClaim);
            // Trong một ứng dụng thực tế, bạn có thể muốn throw một exception cụ thể hơn
            // hoặc xử lý trường hợp này một cách an toàn hơn thay vì chỉ throw UnauthorizedAccessException.
            throw new UnauthorizedAccessException("Không thể xác định người dùng hợp lệ. Vui lòng đăng nhập lại.");
        }

        // Helper lấy CSS class cho badge trạng thái xử lý báo cáo
        private string GetTrangThaiXuLyBadgeClass(TrangThaiXuLyBaoCao trangThai)
        {
            return trangThai switch
            {
                TrangThaiXuLyBaoCao.moi => "bg-secondary",
                TrangThaiXuLyBaoCao.daxemxet => "bg-info text-dark",
                TrangThaiXuLyBaoCao.daxuly => "bg-success",
                TrangThaiXuLyBaoCao.boqua => "bg-warning text-dark",
                _ => "bg-light text-dark",
            };
        }
        
        // Helper định dạng chuỗi lương (copy từ TimViecController hoặc tạo class Helper chung)
        private static string FormatSalaryHelper(LoaiLuong loaiLuong, ulong? min, ulong? max) 
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
            string prefix = loaiLuong switch { LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca", LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => "" };
            string FormatValue(ulong val) => val.ToString("N0"); // "N0" để có dấu phẩy ngăn cách
            if (min.HasValue && max.HasValue && min > 0 && max > 0) { if (min == max) return $"{FormatValue(min.Value)}{prefix}"; return $"{FormatValue(min.Value)} - {FormatValue(max.Value)}{prefix}"; }
            if (min.HasValue && min > 0) return $"Từ {FormatValue(min.Value)}{prefix}";
            if (max.HasValue && max > 0) return $"Đến {FormatValue(max.Value)}{prefix}";
            try { return loaiLuong.GetDisplayName(); } catch { return loaiLuong.ToString(); }
        }

        // Action chính hiển thị danh sách báo cáo của người dùng hiện tại
        [HttpGet("")]       // Route: /BaoCao
        [HttpGet("Index")]  // Route: /BaoCao/Index
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiXuLyBaoCao? trangThai, int page = 1)
        {
            int currentUserId = GetCurrentUserId(); // Sẽ throw exception nếu không lấy được ID
            int pageSize = 10;
            page = Math.Max(1, page);

            _logger.LogInformation("User {UserId} truy cập trang Báo cáo của tôi. Filters: TuKhoa={tuKhoa}, TrangThai={trangThai}, Page={page}", 
                currentUserId, tuKhoa, trangThai, page);

            var query = _context.BaoCaoViPhams
                .Where(bc => bc.NguoiBaoCaoId == currentUserId)
                .Include(bc => bc.TinTuyenDung)
                    .ThenInclude(ttd => ttd.NguoiDang) 
                    .ThenInclude(nd => nd.HoSoDoanhNghiep) 
                .Include(bc => bc.TinTuyenDung) 
                    .ThenInclude(ttd => ttd.ThanhPho)
                .Include(bc => bc.TinTuyenDung)
                    .ThenInclude(ttd => ttd.QuanHuyen)
                .Include(bc => bc.TinTuyenDung)
                    .ThenInclude(ttd => ttd.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .Include(bc => bc.AdminXuLy) 
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string keywordLower = tuKhoa.Trim().ToLower();
                // Sử dụng EF.Functions.Collate nếu DB của bạn hỗ trợ và cần tìm kiếm không phân biệt chữ hoa/thường/dấu
                // Ví dụ cho MySQL (Pomelo): EF.Functions.Collate(field, "utf8mb4_general_ci")
                // Nếu không, .ToLower().Contains() là một cách tiếp cận đơn giản.
                query = query.Where(bc =>
                    (bc.TinTuyenDung.TieuDe != null && bc.TinTuyenDung.TieuDe.ToLower().Contains(keywordLower)) ||
                    (bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null && bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy != null && bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy.ToLower().Contains(keywordLower)) ||
                    (bc.TinTuyenDung.NguoiDang.LoaiTk != LoaiTaiKhoan.doanhnghiep && bc.TinTuyenDung.NguoiDang.HoTen != null && bc.TinTuyenDung.NguoiDang.HoTen.ToLower().Contains(keywordLower)) ||
                    (bc.ChiTiet != null && bc.ChiTiet.ToLower().Contains(keywordLower))
                );
            }

            if (trangThai.HasValue)
            {
                query = query.Where(bc => bc.TrangThaiXuLy == trangThai.Value);
            }

            query = query.OrderByDescending(bc => bc.NgayBaoCao);

            var paginatedBaoCaos = await PaginatedList<BaoCaoViPham>.CreateAsync(query, page, pageSize);

            var itemsViewModel = paginatedBaoCaos.Select(bc => new BaoCaoItemViewModel
            {
                BaoCaoId = bc.Id,
                LyDoBaoCaoDisplay = bc.LyDo.GetDisplayName(),
                ChiTietBaoCao = bc.ChiTiet,
                NgayBaoCao = bc.NgayBaoCao,
                TrangThaiXuLy = bc.TrangThaiXuLy,
                TrangThaiXuLyDisplay = bc.TrangThaiXuLy.GetDisplayName(),
                TrangThaiXuLyBadgeClass = GetTrangThaiXuLyBadgeClass(bc.TrangThaiXuLy),
                CanDelete = bc.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi,
                GhiChuAdmin = bc.GhiChuAdmin,
                NgayXuLyCuaAdmin = bc.NgayXuLy,

                // Map thông tin tin tuyển dụng
                TinTuyenDungId = bc.TinTuyenDungId,
                TieuDeTinTuyenDung = bc.TinTuyenDung?.TieuDe ?? "N/A - Tin không còn tồn tại",
                TenNhaTuyenDungHoacNguoiDang = bc.TinTuyenDung?.NguoiDang != null ?
                    (bc.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null
                        ? bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy 
                        : bc.TinTuyenDung.NguoiDang.HoTen) ?? "Không rõ" // Fallback nếu HoTen null
                    : "Không rõ",
                LogoUrlNhaTuyenDung = bc.TinTuyenDung?.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep
                    ? bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo
                    : bc.TinTuyenDung.NguoiDang?.UrlAvatar,
                LoaiTkNguoiDang = bc.TinTuyenDung?.NguoiDang?.LoaiTk ?? LoaiTaiKhoan.canhan, // Mặc định là cá nhân nếu NguoiDang null
                DiaDiemTinTuyenDung = (bc.TinTuyenDung?.QuanHuyen?.Ten != null ? bc.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (bc.TinTuyenDung?.ThanhPho?.Ten ?? "N/A"),
                MucLuongDisplayTinTuyenDung = bc.TinTuyenDung != null ? FormatSalaryHelper(bc.TinTuyenDung.LoaiLuong, bc.TinTuyenDung.LuongToiThieu, bc.TinTuyenDung.LuongToiDa) : "N/A",
                LoaiHinhDisplayTinTuyenDung = bc.TinTuyenDung?.LoaiHinhCongViec.GetDisplayName() ?? "N/A",
                NgayHetHanTinTuyenDung = bc.TinTuyenDung?.NgayHetHan,
                TagsTinTuyenDung = bc.TinTuyenDung?.TinTuyenDungNganhNghes?.Select(tnn => tnn.NganhNghe?.Ten ?? "").Where(s => !string.IsNullOrEmpty(s)).Take(2).ToList() ?? new List<string>(),
                TinGapTinTuyenDung = bc.TinTuyenDung?.TinGap ?? false
            }).ToList();

            var viewModel = new DanhSachBaoCaoViewModel
            {
                BaoCaos = new PaginatedList<BaoCaoItemViewModel>(itemsViewModel, paginatedBaoCaos.TotalCount, paginatedBaoCaos.PageIndex, pageSize),
                tuKhoa = tuKhoa,
                trangThai = trangThai
            };
            
            ViewBag.TrangThaiXuLyOptions = new SelectList(
                EnumExtensions.GetSelectList<TrangThaiXuLyBaoCao>(includeDefaultItem: true, defaultItemText: "-- Tất cả trạng thái --", defaultItemValue: string.Empty),
                "Value", "Text", trangThai?.ToString()
            );

            ViewData["Title"] = "Báo cáo của tôi";
            return View(viewModel);
        }

        [HttpPost("XoaBaoCao")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBaoCao([FromBody] int baoCaoId)
        {
            if (baoCaoId <= 0)
            {
                _logger.LogWarning("XoaBaoCao: Yêu cầu xóa báo cáo với ID không hợp lệ: {BaoCaoId}", baoCaoId);
                return BadRequest(new { success = false, message = "ID báo cáo không hợp lệ." });
            }

            int currentUserId = GetCurrentUserId();
            var baoCaoCanXoa = await _context.BaoCaoViPhams
                .FirstOrDefaultAsync(bc => bc.Id == baoCaoId && bc.NguoiBaoCaoId == currentUserId);

            if (baoCaoCanXoa == null)
            {
                _logger.LogWarning("XoaBaoCao: Người dùng {UserId} cố gắng xóa báo cáo không tồn tại hoặc không thuộc sở hữu: ID {BaoCaoId}", currentUserId, baoCaoId);
                return NotFound(new { success = false, message = "Không tìm thấy báo cáo hoặc bạn không có quyền xóa." });
            }

            if (baoCaoCanXoa.TrangThaiXuLy != TrangThaiXuLyBaoCao.moi)
            {
                 _logger.LogWarning("XoaBaoCao: Người dùng {UserId} cố gắng xóa báo cáo ID {BaoCaoId} đã được xử lý (Trạng thái: {TrangThai})", currentUserId, baoCaoId, baoCaoCanXoa.TrangThaiXuLy);
                return BadRequest(new { success = false, message = "Chỉ có thể xóa báo cáo đang ở trạng thái 'Mới'." });
            }

            _context.BaoCaoViPhams.Remove(baoCaoCanXoa);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("XoaBaoCao: Người dùng {UserId} đã xóa thành công báo cáo ID {BaoCaoId}", currentUserId, baoCaoId);
                return Ok(new { success = true, message = "Đã xóa báo cáo thành công." });
            }
            catch (DbUpdateException ex) 
            {
                _logger.LogError(ex, "XoaBaoCao: Lỗi DbUpdateException khi xóa báo cáo ID {BaoCaoId} của người dùng {UserId}", baoCaoId, currentUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Lỗi khi cập nhật cơ sở dữ liệu. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XoaBaoCao: Lỗi không xác định khi xóa báo cáo ID {BaoCaoId} của người dùng {UserId}", baoCaoId, currentUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Lỗi máy chủ khi xóa báo cáo. Vui lòng thử lại." });
            }
        }
    }
}