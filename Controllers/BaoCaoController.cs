// File: Controllers/BaoCaoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.BaoCao;
using HeThongTimViec.ViewModels.TimViec;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HeThongTimViec.Extensions;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace HeThongTimViec.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BaoCaoController> _logger;
        // --- BỔ SUNG ---
        // Định nghĩa các tùy chọn page size ở một nơi để dễ quản lý
        private readonly List<SelectListItem> _pageSizeOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "5", Text = "5 mục" },
            new SelectListItem { Value = "10", Text = "10 mục" },
            new SelectListItem { Value = "20", Text = "20 mục" },
            new SelectListItem { Value = "50", Text = "50 mục" }
        };

        public BaoCaoController(ApplicationDbContext context, ILogger<BaoCaoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ... (Các helper GetCurrentUserId, GetTrangThaiXuLyBadgeClass, FormatSalaryHelper không đổi) ...
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) { return userId; }
            _logger.LogError("BaoCaoController: Không thể parse User ID từ ClaimsPrincipal. Claim value: {ClaimValue}", userIdClaim);
            throw new UnauthorizedAccessException("Không thể xác định người dùng hợp lệ. Vui lòng đăng nhập lại.");
        }
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
        private static string FormatSalaryHelper(LoaiLuong loaiLuong, ulong? min, ulong? max)
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
            string prefix = loaiLuong switch { LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca", LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => "" };
            string FormatValue(ulong val) => val.ToString("N0");
            if (min.HasValue && max.HasValue && min > 0 && max > 0) { if (min == max) return $"{FormatValue(min.Value)}{prefix}"; return $"{FormatValue(min.Value)} - {FormatValue(max.Value)}{prefix}"; }
            if (min.HasValue && min > 0) return $"Từ {FormatValue(min.Value)}{prefix}";
            if (max.HasValue && max > 0) return $"Đến {FormatValue(max.Value)}{prefix}";
            try { return loaiLuong.GetDisplayName(); } catch { return loaiLuong.ToString(); }
        }

        [HttpGet("")]
        [HttpGet("Index")]
        // --- CHỈNH SỬA SIGNATURE ---
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiXuLyBaoCao? trangThai, int page = 1, int pageSize = 10)
        {
            int currentUserId = GetCurrentUserId();

            // --- CHỈNH SỬA XỬ LÝ PAGESIZE ---
            // Validate page size để tránh giá trị không hợp lệ
            const int maxPageSize = 50;
            if (pageSize < 5) pageSize = 10;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            page = Math.Max(1, page);

            _logger.LogInformation("User {UserId} truy cập trang Báo cáo của tôi. Filters: TuKhoa={tuKhoa}, TrangThai={trangThai}, Page={page}, PageSize={pageSize}",
                currentUserId, tuKhoa, trangThai, page, pageSize);

            // ... (Phần query không đổi) ...
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

            // Sử dụng pageSize đã được validate
            var paginatedBaoCaos = await PaginatedList<BaoCaoViPham>.CreateAsync(query, page, pageSize);

            // ... (Phần map item ViewModel không đổi) ...
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
                TinTuyenDungId = bc.TinTuyenDungId,
                TieuDeTinTuyenDung = bc.TinTuyenDung?.TieuDe ?? "N/A - Tin không còn tồn tại",
                TenNhaTuyenDungHoacNguoiDang = bc.TinTuyenDung?.NguoiDang != null ?
                    (bc.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null
                        ? bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy
                        : bc.TinTuyenDung.NguoiDang.HoTen) ?? "Không rõ"
                    : "Không rõ",
                LogoUrlNhaTuyenDung = bc.TinTuyenDung?.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep
                    ? bc.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo
                    : (bc.TinTuyenDung?.NguoiDang != null ? bc.TinTuyenDung.NguoiDang.UrlAvatar : null),
                LoaiTkNguoiDang = bc.TinTuyenDung?.NguoiDang?.LoaiTk ?? LoaiTaiKhoan.canhan,
                DiaDiemTinTuyenDung = (bc.TinTuyenDung?.QuanHuyen?.Ten != null ? bc.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (bc.TinTuyenDung?.ThanhPho?.Ten ?? "N/A"),
                MucLuongDisplayTinTuyenDung = bc.TinTuyenDung != null ? FormatSalaryHelper(bc.TinTuyenDung.LoaiLuong, bc.TinTuyenDung.LuongToiThieu, bc.TinTuyenDung.LuongToiDa) : "N/A",
                LoaiHinhDisplayTinTuyenDung = bc.TinTuyenDung?.LoaiHinhCongViec.GetDisplayName() ?? "N/A",
                NgayHetHanTinTuyenDung = bc.TinTuyenDung?.NgayHetHan,
                TagsTinTuyenDung = bc.TinTuyenDung?.TinTuyenDungNganhNghes?.Select(tnn => tnn.NganhNghe?.Ten ?? "").Where(s => !string.IsNullOrEmpty(s)).Take(2).ToList() ?? new List<string>(),
                TinGapTinTuyenDung = bc.TinTuyenDung?.TinGap ?? false
            }).ToList();


            // --- CẬP NHẬT VIEWMODEL ---
            var viewModel = new DanhSachBaoCaoViewModel
            {
                BaoCaos = new PaginatedList<BaoCaoItemViewModel>(itemsViewModel, paginatedBaoCaos.TotalCount, paginatedBaoCaos.PageIndex, pageSize),
                tuKhoa = tuKhoa,
                trangThai = trangThai,
                pageSize = pageSize, // Truyền pageSize hiện tại
                PageSizeOptions = new SelectList(_pageSizeOptions, "Value", "Text", pageSize) // Tạo SelectList
            };

            // Phần này bạn có thể giữ lại hoặc bỏ đi nếu bộ lọc đã xử lý trong partial view
            ViewBag.TrangThaiXuLyOptions = new SelectList(
                EnumExtensions.GetSelectList<TrangThaiXuLyBaoCao>(includeDefaultItem: true, defaultItemText: "-- Tất cả trạng thái --", defaultItemValue: string.Empty),
                "Value", "Text", trangThai?.ToString()
            );

            ViewData["Title"] = "Báo cáo của tôi";
            return View(viewModel);
        }

        // ... (Action XoaBaoCao không đổi) ...
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
        [HttpGet("Details/{id:int}")]
public async Task<IActionResult> Details(int id)
{
    int currentUserId;
    try
    {
        currentUserId = GetCurrentUserId();
    }
    catch (UnauthorizedAccessException)
    {
        // Chuyển hướng đến trang đăng nhập nếu không xác thực được
        return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Url.Action("Details", "BaoCao", new { id }) });
    }

    _logger.LogInformation("User {UserId} đang xem chi tiết báo cáo ID {BaoCaoId}", currentUserId, id);

    var baoCao = await _context.BaoCaoViPhams
        .Where(bc => bc.Id == id && bc.NguoiBaoCaoId == currentUserId)
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
        .AsNoTracking()
        .FirstOrDefaultAsync();

    if (baoCao == null)
    {
        _logger.LogWarning("Không tìm thấy báo cáo ID {BaoCaoId} cho người dùng {UserId}", id, currentUserId);
        return NotFound("Không tìm thấy báo cáo hoặc bạn không có quyền xem báo cáo này.");
    }
    
    // Sử dụng lại logic mapping đã có trong action Index
    var viewModel = new BaoCaoItemViewModel
    {
        BaoCaoId = baoCao.Id,
        LyDoBaoCaoDisplay = baoCao.LyDo.GetDisplayName(),
        ChiTietBaoCao = baoCao.ChiTiet,
        NgayBaoCao = baoCao.NgayBaoCao,
        TrangThaiXuLy = baoCao.TrangThaiXuLy,
        TrangThaiXuLyDisplay = baoCao.TrangThaiXuLy.GetDisplayName(),
        TrangThaiXuLyBadgeClass = GetTrangThaiXuLyBadgeClass(baoCao.TrangThaiXuLy),
        CanDelete = baoCao.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi,
        GhiChuAdmin = baoCao.GhiChuAdmin,
        NgayXuLyCuaAdmin = baoCao.NgayXuLy,
        TenAdminXuLy = baoCao.AdminXuLy?.HoTen,
        
        // Thông tin tin tuyển dụng
        TinTuyenDungId = baoCao.TinTuyenDungId,
        TieuDeTinTuyenDung = baoCao.TinTuyenDung?.TieuDe ?? "N/A - Tin không còn tồn tại",
        TenNhaTuyenDungHoacNguoiDang = baoCao.TinTuyenDung?.NguoiDang != null ?
            (baoCao.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && baoCao.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null
                ? baoCao.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy 
                : baoCao.TinTuyenDung.NguoiDang.HoTen) ?? "Không rõ"
            : "Không rõ",
        LogoUrlNhaTuyenDung = baoCao.TinTuyenDung?.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep
            ? baoCao.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo
            : (baoCao.TinTuyenDung?.NguoiDang != null ? baoCao.TinTuyenDung.NguoiDang.UrlAvatar : null),
        LoaiTkNguoiDang = baoCao.TinTuyenDung?.NguoiDang?.LoaiTk ?? LoaiTaiKhoan.canhan,
        DiaDiemTinTuyenDung = (baoCao.TinTuyenDung?.QuanHuyen?.Ten != null ? baoCao.TinTuyenDung.QuanHuyen.Ten + ", " : "") + (baoCao.TinTuyenDung?.ThanhPho?.Ten ?? "N/A"),
        MucLuongDisplayTinTuyenDung = baoCao.TinTuyenDung != null ? FormatSalaryHelper(baoCao.TinTuyenDung.LoaiLuong, baoCao.TinTuyenDung.LuongToiThieu, baoCao.TinTuyenDung.LuongToiDa) : "N/A",
        LoaiHinhDisplayTinTuyenDung = baoCao.TinTuyenDung?.LoaiHinhCongViec.GetDisplayName() ?? "N/A",
        NgayHetHanTinTuyenDung = baoCao.TinTuyenDung?.NgayHetHan,
        TagsTinTuyenDung = baoCao.TinTuyenDung?.TinTuyenDungNganhNghes?.Select(tnn => tnn.NganhNghe?.Ten ?? "").Where(s => !string.IsNullOrEmpty(s)).Take(2).ToList() ?? new List<string>(),
        TinGapTinTuyenDung = baoCao.TinTuyenDung?.TinGap ?? false
    };

    ViewData["Title"] = "Chi tiết Báo cáo vi phạm";
    return View(viewModel);
}

    }
}