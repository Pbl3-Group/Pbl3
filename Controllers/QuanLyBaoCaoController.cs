// File: Controllers/QuanLyBaoCaoController.cs

using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.BaoCao;
using HeThongTimViec.ViewModels.TimViec; // For PaginatedList
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions; // For GetDisplayName()
using System.Security.Claims;
using HeThongTimViec.Services;
using HeThongTimViec.Utils;
using System.Text.Json;
using System.Collections.Generic;

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
    public class QuanLyBaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLyBaoCaoController> _logger;
        private readonly IThongBaoService _thongBaoService;

        public QuanLyBaoCaoController(ApplicationDbContext context, ILogger<QuanLyBaoCaoController> logger, IThongBaoService thongBaoService)
        {
            _context = context;
            _logger = logger;
            _thongBaoService = thongBaoService;
        }

        // GET: QuanLyBaoCao
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiXuLyBaoCao? trangThai, int pageNumber = 1, int pageSize = 10)
        {
            // === PHẦN TÍNH TOÁN THỐNG KÊ ===
            var allReportsQuery = _context.BaoCaoViPhams.AsNoTracking();
            int totalReports = await allReportsQuery.CountAsync();
            int newReports = await allReportsQuery.CountAsync(r => r.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi);
            int reviewedReports = await allReportsQuery.CountAsync(r => r.TrangThaiXuLy == TrangThaiXuLyBaoCao.daxemxet);
            int processedReports = await allReportsQuery.CountAsync(r => r.TrangThaiXuLy == TrangThaiXuLyBaoCao.daxuly);
            int ignoredReports = await allReportsQuery.CountAsync(r => r.TrangThaiXuLy == TrangThaiXuLyBaoCao.boqua);

            // === PHẦN TRUY VẤN CHÍNH ===
            var query = _context.BaoCaoViPhams
                .Include(b => b.TinTuyenDung).ThenInclude(t => t.NguoiDang).ThenInclude(n => n.HoSoDoanhNghiep)
                .Include(b => b.TinTuyenDung).ThenInclude(t => t.ThanhPho)
                .Include(b => b.TinTuyenDung).ThenInclude(t => t.QuanHuyen)
                .Include(b => b.NguoiBaoCao)
                .Include(b => b.AdminXuLy)
                .AsNoTracking();

            // Lọc theo trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(b => b.TrangThaiXuLy == trangThai.Value);
            }

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(b =>
                    b.TinTuyenDung.TieuDe.Contains(tuKhoa) ||
                    (b.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null && b.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy.Contains(tuKhoa)) ||
                    b.TinTuyenDung.NguoiDang.HoTen.Contains(tuKhoa) ||
                    (b.ChiTiet != null && b.ChiTiet.Contains(tuKhoa)) ||
                    b.NguoiBaoCao.HoTen.Contains(tuKhoa)
                );
            }

            // Sắp xếp
            query = query.OrderByDescending(b => b.NgayBaoCao);

            var paginatedBaoCaos = await PaginatedList<BaoCaoItemViewModel>.CreateAsync(
                query.Select(b => new BaoCaoItemViewModel
                {
                    BaoCaoId = b.Id,
                    TinTuyenDungId = b.TinTuyenDungId,
                    TieuDeTinTuyenDung = b.TinTuyenDung.TieuDe,
                    TenNhaTuyenDungHoacNguoiDang = b.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? b.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy : b.TinTuyenDung.NguoiDang.HoTen,
                    LoaiTkNguoiDang = b.TinTuyenDung.NguoiDang.LoaiTk,
                    NguoiDangId = b.TinTuyenDung.NguoiDangId,
                    TenNguoiBaoCao = b.NguoiBaoCao.HoTen,
                    NguoiBaoCaoId = b.NguoiBaoCaoId,
                    LyDoBaoCaoDisplay = b.LyDo.GetDisplayName(),
                    NgayBaoCao = b.NgayBaoCao,
                    TrangThaiXuLy = b.TrangThaiXuLy,
                    TrangThaiXuLyDisplay = b.TrangThaiXuLy.GetDisplayName(),
                }),
                pageNumber,
                pageSize);
            
            var viewModel = new DanhSachBaoCaoViewModel
            {
                BaoCaos = paginatedBaoCaos,
                tuKhoa = tuKhoa,
                trangThai = trangThai,
                pageSize = pageSize,
                TotalReports = totalReports,
                NewReports = newReports,
                ReviewedReports = reviewedReports,
                ProcessedReports = processedReports,
                IgnoredReports = ignoredReports
            };

            return View(viewModel);
        }

        // GET: QuanLyBaoCao/Details/5
       public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    // Tải đầy đủ thông tin cần thiết
    var baoCao = await _context.BaoCaoViPhams
        .Include(b => b.TinTuyenDung).ThenInclude(t => t.NguoiDang).ThenInclude(n => n.HoSoDoanhNghiep)
        .Include(b => b.TinTuyenDung).ThenInclude(t => t.ThanhPho)
        .Include(b => b.TinTuyenDung).ThenInclude(t => t.QuanHuyen)
        .Include(b => b.NguoiBaoCao)
        .Include(b => b.AdminXuLy)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (baoCao == null) return NotFound();

    // Logic chuyển trạng thái và gửi thông báo chỉ xảy ra MỘT LẦN
    if (baoCao.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(adminId, out int currentAdminId))
        {
            // === CẬP NHẬT TRẠNG THÁI ===
            baoCao.TrangThaiXuLy = TrangThaiXuLyBaoCao.daxemxet;
            baoCao.AdminXuLyId = currentAdminId;
            baoCao.NgayXuLy = DateTime.UtcNow; // Ghi nhận cả ngày xem xét
            
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Báo cáo đã được đánh dấu là 'Đã xem xét'.";

                // === GỬI THÔNG BÁO NGAY SAU KHI CẬP NHẬT THÀNH CÔNG ===
                // Khối này đã được di chuyển vào trong if
                var notificationData = new
                {
                    tieuDeTin = baoCao.TinTuyenDung.TieuDe,
                    noiDung = "Báo cáo của bạn đang được quản trị viên xem xét. Chúng tôi sẽ sớm có phản hồi.",
                    url = Url.Action("Details", "BaoCao", new { id = baoCao.Id }, Request.Scheme)
                };

                await _thongBaoService.CreateThongBaoAsync(
                    baoCao.NguoiBaoCaoId,
                    NotificationConstants.Types.BaoCaoViPhamDaXemXet,
                    JsonSerializer.Serialize(notificationData),
                    NotificationConstants.RelatedEntities.BaoCaoViPham,
                    baoCao.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái hoặc gửi thông báo cho báo cáo ID {BaoCaoId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái báo cáo.";
                // Không cần redirect ở đây, cứ để nó hiển thị trang details
            }
        }
    }
    
    // Logic mapping sang ViewModel giữ nguyên
    var viewModel = new BaoCaoItemViewModel
    {
        BaoCaoId = baoCao.Id,
        LyDoBaoCaoDisplay = baoCao.LyDo.GetDisplayName(),
        ChiTietBaoCao = baoCao.ChiTiet,
        NgayBaoCao = baoCao.NgayBaoCao,
        TrangThaiXuLy = baoCao.TrangThaiXuLy,
        TrangThaiXuLyDisplay = baoCao.TrangThaiXuLy.GetDisplayName(),
        GhiChuAdmin = baoCao.GhiChuAdmin,
        NgayXuLyCuaAdmin = baoCao.NgayXuLy,
        TenNguoiBaoCao = baoCao.NguoiBaoCao?.HoTen,
        NguoiBaoCaoId = baoCao.NguoiBaoCaoId,
        TenAdminXuLy = baoCao.AdminXuLy?.HoTen,
        MoTaTinTuyenDung = baoCao.TinTuyenDung?.MoTa,
        YeuCauTinTuyenDung = baoCao.TinTuyenDung?.YeuCau,
        TinTuyenDungId = baoCao.TinTuyenDungId,
        TieuDeTinTuyenDung = baoCao.TinTuyenDung.TieuDe,
        LoaiTkNguoiDang = baoCao.TinTuyenDung.NguoiDang.LoaiTk,
        NguoiDangId = baoCao.TinTuyenDung.NguoiDangId,
        TenNhaTuyenDungHoacNguoiDang = baoCao.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? baoCao.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy : baoCao.TinTuyenDung.NguoiDang.HoTen,
        LogoUrlNhaTuyenDung = baoCao.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? baoCao.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo : baoCao.TinTuyenDung.NguoiDang.UrlAvatar,
        DiaDiemTinTuyenDung = $"{baoCao.TinTuyenDung.QuanHuyen.Ten}, {baoCao.TinTuyenDung.ThanhPho.Ten}",
        MucLuongDisplayTinTuyenDung = $"{baoCao.TinTuyenDung.LuongToiThieu} - {baoCao.TinTuyenDung.LuongToiDa}",
        LoaiHinhDisplayTinTuyenDung = baoCao.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
        NgayHetHanTinTuyenDung = baoCao.TinTuyenDung.NgayHetHan,
        CanDelete = baoCao.TrangThaiXuLy != TrangThaiXuLyBaoCao.daxuly
    };

    return View(viewModel);
}

        // POST: QuanLyBaoCao/ProcessReport
      [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ProcessReport(ProcessReportViewModel model)
{
    if (!ModelState.IsValid)
    {
        TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
        return RedirectToAction(nameof(Details), new { id = model.BaoCaoId });
    }

    // Tải đầy đủ thông tin cần thiết
    var baoCao = await _context.BaoCaoViPhams
                             .Include(b => b.TinTuyenDung).ThenInclude(t => t.NguoiDang)
                             .Include(b => b.NguoiBaoCao)
                             .FirstOrDefaultAsync(b => b.Id == model.BaoCaoId);

    if (baoCao == null) return NotFound();

    if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentAdminId))
    {
        TempData["ErrorMessage"] = "Không thể xác định quản trị viên.";
        return RedirectToAction(nameof(Details), new { id = model.BaoCaoId });
    }

    baoCao.AdminXuLyId = currentAdminId;
    baoCao.NgayXuLy = DateTime.UtcNow;
    baoCao.GhiChuAdmin = model.GhiChuAdmin;

    var userViolator = baoCao.TinTuyenDung.NguoiDang;

    switch (model.Action)
    {
        case ReportActionType.Ignore:
            baoCao.TrangThaiXuLy = TrangThaiXuLyBaoCao.boqua;
            TempData["SuccessMessage"] = "Đã bỏ qua báo cáo.";
            
            // GỬI THÔNG BÁO: Báo cáo bị bỏ qua
            await SendNotificationToReporter(baoCao, NotificationConstants.Types.BaoCaoViPhamBoQua, model.GhiChuAdmin ?? "Báo cáo của bạn đã được xem xét nhưng không có đủ cơ sở để xử lý. Cảm ơn bạn đã đóng góp.");
            break;

        case ReportActionType.WarnAndHide:
            baoCao.TrangThaiXuLy = TrangThaiXuLyBaoCao.daxuly;
            baoCao.TinTuyenDung.TrangThai = TrangThaiTinTuyenDung.daxoa;
             TempData["SuccessMessage"] = "Đã xử lý báo cáo: Xóa tin và gửi cảnh cáo đến người đăng.";
            
            // GỬI THÔNG BÁO: Báo cáo đã xử lý (cho người báo cáo)
            await SendNotificationToReporter(baoCao, NotificationConstants.Types.BaoCaoViPhamDaXuLy, model.GhiChuAdmin ?? "Báo cáo của bạn đã được xử lý. Tin tuyển dụng vi phạm đã bị xoá. Cảm ơn bạn.");
            
            // GỬI THÔNG BÁO: Cảnh cáo (cho người vi phạm)
            await SendWarningNotificationToViolator(userViolator, baoCao.TinTuyenDung, model.NoiDungCanhCao ?? "Tin tuyển dụng của bạn đã bị xoá do vi phạm quy định. Vui lòng không tái phạm.");
            break;

        case ReportActionType.SuspendAndHide:
            baoCao.TrangThaiXuLy = TrangThaiXuLyBaoCao.daxuly;
            baoCao.TinTuyenDung.TrangThai = TrangThaiTinTuyenDung.daxoa;
            userViolator.TrangThaiTk = TrangThaiTaiKhoan.bidinhchi;
            TempData["SuccessMessage"] = "Đã xử lý báo cáo: Xoá tin và đình chỉ tài khoản người đăng.";
            
            // GỬI THÔNG BÁO: Báo cáo đã xử lý (cho người báo cáo)
            await SendNotificationToReporter(baoCao, NotificationConstants.Types.BaoCaoViPhamDaXuLy, model.GhiChuAdmin ?? "Báo cáo của bạn đã được xử lý. Tin vi phạm đã bị xoá và tài khoản người đăng đã bị đình chỉ. Cảm ơn bạn.");
            
            // GỬI THÔNG BÁO: Đình chỉ (cho người vi phạm)
            await SendSuspendNotificationToViolator(userViolator, baoCao.TinTuyenDung, model.NoiDungCanhCao ?? "Tài khoản của bạn đã bị đình chỉ do vi phạm nghiêm trọng. Tin tuyển dụng liên quan đã bị xoá.");
            break;
    }

    try
    {
         await _context.SaveChangesAsync();
    }
    catch(DbUpdateException ex)
    {
        _logger.LogError(ex, "Lỗi khi xử lý báo cáo ID {BaoCaoId}", model.BaoCaoId);
        TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thay đổi vào cơ sở dữ liệu.";
    }
   
    return RedirectToAction(nameof(Details), new { id = model.BaoCaoId });
}
        // POST: QuanLyBaoCao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baoCao = await _context.BaoCaoViPhams.FindAsync(id);
            if (baoCao == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy báo cáo để xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (baoCao.TrangThaiXuLy == TrangThaiXuLyBaoCao.daxuly)
            {
                TempData["ErrorMessage"] = "Không thể xóa báo cáo đã được xử lý để lưu lại lịch sử.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            try
            {
                _context.BaoCaoViPhams.Remove(baoCao);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa báo cáo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa báo cáo ID {BaoCaoId}", id);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi xóa báo cáo.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        #region Private Helper Methods for Notifications
       private async Task SendNotificationToReporter(BaoCaoViPham baoCao, string notificationType, string adminNote)
{
    if (baoCao.NguoiBaoCaoId == baoCao.AdminXuLyId) return;

    var notificationData = JsonSerializer.Serialize(new
    {
        tieuDeTin = baoCao.TinTuyenDung.TieuDe,
        noiDung = adminNote,
        // URL cho người báo cáo xem lại báo cáo của họ
        url = Url.Action("Details", "BaoCao", new { id = baoCao.Id }, Request.Scheme)
    });
    await _thongBaoService.CreateThongBaoAsync(baoCao.NguoiBaoCaoId, notificationType, notificationData, NotificationConstants.RelatedEntities.BaoCaoViPham, baoCao.Id);
}

private async Task SendWarningNotificationToViolator(NguoiDung violator, TinTuyenDung tin, string warningMessage)
{
    // Xác định đúng controller quản lý tin đăng của người vi phạm
    string targetController = violator.LoaiTk == LoaiTaiKhoan.doanhnghiep ? "CompanyPosting" : "IndividualPosting";

    var notificationData = JsonSerializer.Serialize(new 
    {
        tieuDeTin = tin.TieuDe,
        noiDung = warningMessage,
        // URL cho người vi phạm xem lại tin đã bị ẩn của họ
        url = Url.Action("Details", targetController, new { id = tin.Id }, Request.Scheme)
    });
    
    await _thongBaoService.CreateThongBaoAsync(violator.Id, NotificationConstants.Types.TaiKhoanTamDung, notificationData, NotificationConstants.RelatedEntities.TinTuyenDung, tin.Id);
}

private async Task SendSuspendNotificationToViolator(NguoiDung violator, TinTuyenDung tin, string suspendMessage)
{
     var notificationData = JsonSerializer.Serialize(new 
    {
        tieuDeTin = tin.TieuDe,
        noiDung = suspendMessage,
        url = "" // Tài khoản bị đình chỉ, không thể đăng nhập để xem link
    });
    await _thongBaoService.CreateThongBaoAsync(violator.Id, NotificationConstants.Types.TaiKhoanBiDinhChi, notificationData, NotificationConstants.RelatedEntities.NguoiDung, violator.Id);
}

        #endregion
    }
}