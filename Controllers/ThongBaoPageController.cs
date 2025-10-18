// File: Controllers/ThongBaoPageController.cs (Phiên bản hoàn thiện)
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.Services;
using HeThongTimViec.Utils;
using HeThongTimViec.ViewModels; // Đảm bảo đã using các ViewModel mới
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HeThongTimViec.Controllers
{
    [Authorize]
    public class ThongBaoPageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IThongBaoService _thongBaoService;
        private readonly ILogger<ThongBaoPageController> _logger;
        private const int PageSize = 10;

        public ThongBaoPageController(ApplicationDbContext context, IThongBaoService thongBaoService, ILogger<ThongBaoPageController> logger)
        {
            _context = context;
            _thongBaoService = thongBaoService;
            _logger = logger;
        }

        #region Main Actions
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query = _context.ThongBaos.Where(tb => tb.NguoiDungId == userId).OrderByDescending(tb => tb.NgayTao);

                var totalItems = await query.CountAsync();
                var thongBaos = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

                var displayNotifications = thongBaos.Select(TransformToViewModel).ToList();

                var viewModel = new ThongBaoIndexViewModel
                {
                    Notifications = displayNotifications,
                    PagingInfo = new PagingInfo { CurrentPage = page, ItemsPerPage = PageSize, TotalItems = totalItems }
                };
                return View(viewModel);
            }
            catch (UnauthorizedAccessException ex) { _logger.LogWarning(ex.Message); return RedirectToAction("Login", "Account"); }
            catch (Exception ex) { _logger.LogError(ex, "Lỗi khi tải trang danh sách thông báo."); return View("Error"); }
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var thongBao = await _context.ThongBaos.FirstOrDefaultAsync(tb => tb.Id == id && tb.NguoiDungId == userId);

            if (thongBao == null) return NotFound();

            if (SystemRelatedEntities.Contains(thongBao.LoaiLienQuan ?? ""))
            {
                return BadRequest("Chỉ có thể xem chi tiết thông báo từ quản trị viên.");
            }

            var detailsViewModel = new AdminNotificationDetailsViewModel
            {
                NgayGui = thongBao.NgayTao.ToLocalTime().ToString("HH:mm 'ngày' dd/MM/yyyy"),
                LoaiThongBaoDisplay = AdminNotificationTypes.GetValueOrDefault(thongBao.LoaiThongBao, thongBao.LoaiThongBao)
            };

            try
            {
                var json = JsonDocument.Parse(thongBao.DuLieu).RootElement;
                detailsViewModel.TieuDe = json.TryGetProperty("TieuDe", out var tieuDe) ? tieuDe.GetString() ?? "" : "Không có tiêu đề";
                detailsViewModel.NoiDung = json.TryGetProperty("NoiDung", out var noiDung) ? noiDung.GetString() ?? "" : "Không có nội dung.";
                detailsViewModel.TenAdminGui = json.TryGetProperty("AdminGuiTen", out var ten) ? ten.GetString() ?? "Quản trị viên" : "Quản trị viên";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi parse JSON cho chi tiết thông báo ID {id}", id);
                detailsViewModel.NoiDung = "Lỗi hiển thị nội dung chi tiết. Vui lòng thử lại sau.";
            }

            return View(detailsViewModel);
        }
        #endregion

        #region Interaction Actions (Mark as Read, Delete)

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReadAndRedirect(int thongBaoId, string? returnUrl = null)
        {
            // ... Dán toàn bộ logic điều hướng phức tạp của bạn từ file gốc vào đây ...
            // Dưới đây là logic cơ bản
            var userId = GetCurrentUserId();
            var thongBao = await _thongBaoService.GetThongBaoByIdAsync(thongBaoId);

            if (thongBao != null && thongBao.NguoiDungId == userId)
            {
                if (!thongBao.DaDoc)
                {
                    await _thongBaoService.MarkAsReadAsync(thongBaoId, userId);
                }
                // Logic redirect sẽ ở đây. Nếu không có logic cụ thể, nó sẽ đi tiếp.
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _thongBaoService.MarkAllAsReadAsync(GetCurrentUserId());
            TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _thongBaoService.DeleteAsync(id, GetCurrentUserId());
            if (success) TempData["SuccessMessage"] = "Đã xóa thông báo thành công.";
            else TempData["ErrorMessage"] = "Không thể xóa thông báo hoặc không tìm thấy.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một thông báo để xóa.";
                return RedirectToAction(nameof(Index));
            }
            var deletedCount = await _thongBaoService.DeleteMultipleAsync(selectedIds, GetCurrentUserId());
            if (deletedCount > 0) TempData["SuccessMessage"] = $"Đã xóa thành công {deletedCount} thông báo.";
            else TempData["ErrorMessage"] = "Không có thông báo nào được xóa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            var deletedCount = await _thongBaoService.DeleteAllAsync(GetCurrentUserId());
            if (deletedCount > 0) TempData["SuccessMessage"] = $"Đã xóa tất cả {deletedCount} thông báo.";
            else TempData["InfoMessage"] = "Bạn không có thông báo nào để xóa.";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Transformation Logic & Dictionaries

        // Dịch các loại liên quan của hệ thống
        private static readonly Dictionary<string, string> RelatedEntityTypes = new()
        {
            { NotificationConstants.RelatedEntities.TinTuyenDung, "Tin Tuyển Dụng" }, { NotificationConstants.RelatedEntities.UngTuyen, "Ứng Tuyển" },
            { NotificationConstants.RelatedEntities.HoSoDoanhNghiep, "Hồ Sơ Doanh Nghiệp" }, { NotificationConstants.RelatedEntities.BaoCaoViPham, "Báo Cáo Vi Phạm" },
            { NotificationConstants.RelatedEntities.NguoiDung, "Tài Khoản" }, { NotificationConstants.RelatedEntities.TinNhan, "Tin Nhắn" }
        };

        // Dịch các loại thông báo của Admin
        private static readonly Dictionary<string, string> AdminNotificationTypes = new()
        {
            { "THONG_BAO_CHUNG", "Thông báo chung" }, { "BAO_TRI_HE_THONG", "Bảo trì hệ thống" }, { "CANH_BAO_TAI_KHOAN", "Cảnh báo tài khoản" },
            { "TIN_TUC_CAP_NHAT", "Tin tức & Cập nhật" }, { "PHAN_HOI_HO_SO", "Phản hồi hồ sơ" }, { "LOI_MO_PHONG_VIEC_LAM", "Cảnh báo lừa đảo" },
            { "THONG_BAO_SUC_KHOE", "Cảnh báo thời tiết / sức khỏe" }, { "NHAC_NHO_HO_SO", "Nhắc nhở hồ sơ" }, { "NHAC_NHO_TIN_DANG", "Nhắc nhở tin đăng" }
        };

        // Dịch các loại thông báo của Hệ thống
        private static readonly Dictionary<string, string> SystemNotificationTypes = new()
        {
            { NotificationConstants.Types.TinTuyenDungDuyet, "Tin được duyệt" }, { NotificationConstants.Types.TinTuyenDungTuChoi, "Tin bị từ chối" },
            { NotificationConstants.Types.TinTuyenDungTamAn, "Tin bị tạm ẩn" }, { NotificationConstants.Types.TinTuyenDungHetHan, "Tin hết hạn" },
            { NotificationConstants.Types.UngTuyenNtdXem, "NTD đã xem hồ sơ" }, { NotificationConstants.Types.UngTuyenNtdTuChoi, "NTD từ chối hồ sơ" },
            { NotificationConstants.Types.UngTuyenNtdChapNhan, "NTD chấp nhận hồ sơ" }, { NotificationConstants.Types.UngTuyenUngVienRut, "Ứng viên rút hồ sơ" },
            { NotificationConstants.Types.UngTuyenMoiChoNtd, "Có ứng tuyển mới" }, { NotificationConstants.Types.HoSoDoanhNghiepXacMinh, "Hồ sơ được xác minh" },
            { NotificationConstants.Types.HoSoDoanhNghiepTuChoiXacMinh, "Hồ sơ bị từ chối" }, { NotificationConstants.Types.TaiKhoanTamDung, "Tài khoản bị tạm dừng" },
            { NotificationConstants.Types.TaiKhoanBiDinhChi, "Tài khoản bị đình chỉ" }, { NotificationConstants.Types.TaiKhoanKichHoat, "Tài khoản được kích hoạt" },
            { NotificationConstants.Types.TinNhanMoi, "Có tin nhắn mới" }, { NotificationConstants.Types.HeThongChung, "Thông báo chung" }
        };

        private static readonly HashSet<string> SystemRelatedEntities = new HashSet<string>(RelatedEntityTypes.Keys);

        private DisplayNotificationViewModel TransformToViewModel(ThongBao thongBao)
        {
            var displayData = new DisplayNotificationViewModel
            {
                Id = thongBao.Id,
                Timestamp = thongBao.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                IsUnread = !thongBao.DaDoc
            };

            try
            {
                var json = JsonDocument.Parse(thongBao.DuLieu).RootElement;
                string SafeGetString(params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (json.TryGetProperty(key, out var jsonElement) && jsonElement.ValueKind == JsonValueKind.String)
                        {
                            return jsonElement.GetString() ?? "";
                        }
                    }
                    return "";
                }
                // =============================================================
                // ===        PHÂN LOẠI LOGIC: ADMIN vs. HỆ THỐNG             ===
                // =============================================================

                // KIỂM TRA XEM ĐÂY CÓ PHẢI LÀ THÔNG BÁO TỪ ADMIN KHÔNG
                if (AdminNotificationTypes.ContainsKey(thongBao.LoaiThongBao) || thongBao.LoaiThongBao == NotificationConstants.Types.AdminBroadcast)
                {
                    // --- TRƯỜNG HỢP 1: ĐÂY LÀ THÔNG BÁO TỪ ADMIN (Broadcast hoặc loại tùy chỉnh) ---
                    displayData.IsAdminNotification = true;
                    // Thông báo admin không có link trực tiếp, chỉ có thể xem chi tiết
                    displayData.TargetUrl = Url.Action("Details", "ThongBaoPage", new { id = thongBao.Id });

                    displayData.RelatedEntityDisplayName = "Quản trị viên";

                    // Lấy tên hiển thị và icon dựa trên loại thông báo
                    displayData.TypeDisplayName = AdminNotificationTypes.GetValueOrDefault(thongBao.LoaiThongBao, "Thông báo từ Admin");

                    switch (thongBao.LoaiThongBao)
                    {
                        case "CANH_BAO_TAI_KHOAN":
                        case "LOI_MO_PHONG_VIEC_LAM":
                            displayData.IconClass = "fas fa-exclamation-triangle text-danger";
                            break;
                        case "BAO_TRI_HE_THONG":
                            displayData.IconClass = "fas fa-tools text-warning";
                            break;
                        case "PHAN_HOI_HO_SO":
                        case "NHAC_NHO_HO_SO":
                        case "NHAC_NHO_TIN_DANG":
                            displayData.IconClass = "fas fa-info-circle text-info";
                            break;
                        default: // THONG_BAO_CHUNG, TIN_TUC_CAP_NHAT, etc.
                            displayData.IconClass = "fas fa-bullhorn text-primary";
                            break;
                    }

                    // Lấy tiêu đề và phụ đề từ JSON
                    displayData.Title = SafeGetString("TieuDe", "tieuDe");
                    displayData.Subtitle = SafeGetString("NoiDung", "noiDung");

                    // Xử lý dự phòng nếu tiêu đề hoặc phụ đề rỗng
                    if (string.IsNullOrEmpty(displayData.Title))
                    {
                        displayData.Title = displayData.TypeDisplayName;
                    }
                    if (string.IsNullOrEmpty(displayData.Subtitle))
                    {
                        displayData.Subtitle = "Nhấn để xem chi tiết.";
                    }
                }
                else
                {
                    // --- TRƯỜNG HỢP 2: ĐÂY LÀ THÔNG BÁO TỰ ĐỘNG CỦA HỆ THỐNG ---
                    displayData.IsAdminNotification = false;
                    displayData.TargetUrl = SafeGetString("url");
                    displayData.TypeDisplayName = SystemNotificationTypes.GetValueOrDefault(thongBao.LoaiThongBao, thongBao.LoaiThongBao);
                    displayData.RelatedEntityDisplayName = RelatedEntityTypes.GetValueOrDefault(thongBao.LoaiLienQuan, thongBao.LoaiLienQuan);

                    switch (thongBao.LoaiThongBao)
                    {
                        // --- Tin Tuyển Dụng ---
                        case NotificationConstants.Types.TinTuyenDungDuyet:
                            displayData.IconClass = "fas fa-check-circle text-success";
                            displayData.Title = $"Tin tuyển dụng của bạn đã được duyệt!";
                            displayData.Subtitle = $"Tin '{SafeGetString("tieuDeTin")}' hiện đã được hiển thị công khai.";
                            break;
                        case NotificationConstants.Types.TinTuyenDungTuChoi:
                            displayData.IconClass = "fas fa-times-circle text-danger";
                            displayData.Title = $"Tin tuyển dụng của bạn đã bị từ chối.";
                            displayData.Subtitle = $"Lý do: {SafeGetString("lyDo")}. Tin: '{SafeGetString("tieuDeTin")}'.";
                            break;
                        case NotificationConstants.Types.TinTuyenDungHetHan:
                            displayData.IconClass = "fas fa-calendar-times text-secondary";
                            displayData.Title = $"Tin tuyển dụng đã hết hạn.";
                            displayData.Subtitle = $"Tin '{SafeGetString("tieuDeTin")}' đã hết hạn hiển thị.";
                            break;

                        // --- Ứng Tuyển (cho Nhà tuyển dụng) ---
                        case NotificationConstants.Types.UngTuyenMoiChoNtd:
                            displayData.IconClass = "fas fa-user-plus text-primary";
                            displayData.Title = $"Có ứng viên mới cho vị trí của bạn!";
                            displayData.Subtitle = $"{SafeGetString("tenUngVien")} vừa ứng tuyển vào vị trí '{SafeGetString("tieuDeTin")}'.";
                            break;
                        case NotificationConstants.Types.UngTuyenUngVienRut:
                            displayData.IconClass = "fas fa-user-minus text-warning";
                            displayData.Title = $"Một ứng viên đã rút hồ sơ.";
                            displayData.Subtitle = $"{SafeGetString("tenUngVien")} đã rút hồ sơ khỏi vị trí '{SafeGetString("tieuDeTin")}'.";
                            break;

                        // --- Ứng Tuyển (cho Ứng viên) ---
                        case NotificationConstants.Types.UngTuyenNtdXem:
                            displayData.IconClass = "fas fa-eye text-info";
                            displayData.Title = $"Nhà tuyển dụng đã xem hồ sơ của bạn.";
                            displayData.Subtitle = $"Hồ sơ cho vị trí '{SafeGetString("tieuDeTin")}' đã được xem.";
                            break;
                        case NotificationConstants.Types.UngTuyenNtdChapNhan:
                            displayData.IconClass = "fas fa-award text-success";
                            displayData.Title = $"Chúc mừng! Hồ sơ của bạn đã được chấp nhận.";
                            displayData.Subtitle = $"Bạn đã vượt qua vòng hồ sơ cho vị trí '{SafeGetString("tieuDeTin")}'.";
                            break;
                        case NotificationConstants.Types.UngTuyenNtdTuChoi:
                            displayData.IconClass = "far fa-sad-tear text-muted";
                            displayData.Title = $"Hồ sơ ứng tuyển của bạn đã bị từ chối.";
                            displayData.Subtitle = $"Rất tiếc, hồ sơ cho vị trí '{SafeGetString("tieuDeTin")}' chưa phù hợp.";
                            break;

                        // --- Báo Cáo (cho người báo cáo) ---
                        case NotificationConstants.Types.BaoCaoViPhamDaXuLy:
                            displayData.IconClass = "fas fa-shield-alt text-success";
                            displayData.Title = $"Báo cáo của bạn đã được xử lý.";
                            displayData.Subtitle = $"Cảm ơn bạn đã giúp cộng đồng an toàn. Tin '{SafeGetString("tieuDeTin")}' đã được xử lý.";
                            break;
                        case NotificationConstants.Types.BaoCaoViPhamBoQua:
                            displayData.IconClass = "fas fa-info-circle text-secondary";
                            displayData.Title = $"Phản hồi về báo cáo của bạn.";
                            displayData.Subtitle = $"Tin '{SafeGetString("tieuDeTin")}' đã được xem xét. Cảm ơn bạn.";
                            break;

                        // --- Tài Khoản ---
                        case NotificationConstants.Types.TaiKhoanBiDinhChi:
                            displayData.IconClass = "fas fa-user-lock text-danger";
                            displayData.Title = "Tài khoản của bạn đã bị đình chỉ.";
                            displayData.Subtitle = SafeGetString("noiDung");
                            break;
                        case NotificationConstants.Types.TaiKhoanTamDung:
                            displayData.IconClass = "fas fa-user-clock text-warning";
                            displayData.Title = "Tài khoản của bạn nhận được cảnh báo.";
                            displayData.Subtitle = SafeGetString("noiDung");
                            break;
                        case NotificationConstants.Types.TaiKhoanKichHoat:
                            displayData.IconClass = "fas fa-user-check text-success";
                            displayData.Title = "Tài khoản của bạn đã được kích hoạt lại.";
                            displayData.Subtitle = SafeGetString("noiDung");
                            break;
                        case NotificationConstants.Types.HoSoDoanhNghiepXacMinh:
                            displayData.IconClass = "fas fa-stamp text-primary";
                            displayData.Title = "Hồ sơ doanh nghiệp đã được xác minh!";
                            displayData.Subtitle = SafeGetString("noiDung");
                            break;

                        // --- Tin nhắn ---
                        case NotificationConstants.Types.TinNhanMoi:
                            displayData.IconClass = "fas fa-comments text-primary";
                            displayData.Title = $"Bạn có tin nhắn mới từ '{SafeGetString("tenNguoiGui")}'";
                            displayData.Subtitle = $"\"{SafeGetString("noiDungRutGon")}\"";
                            break;

                        // --- Thông báo từ Admin (Broadcast) ---
                        case NotificationConstants.Types.AdminBroadcast:
                            displayData.IsAdminNotification = true;
                            displayData.IconClass = "fas fa-bullhorn text-danger";
                            displayData.TypeDisplayName = "Thông báo từ Quản trị viên";
                            displayData.Title = SafeGetString("TieuDe", "tieuDe");
                            displayData.Subtitle = SafeGetString("NoiDung", "noiDung");
                            if (string.IsNullOrEmpty(displayData.Title)) displayData.Title = "Thông báo quan trọng";
                            break;

                        // --- Trường hợp mặc định ---
                        default:
                            displayData.IconClass = "fas fa-info-circle text-secondary";
                            displayData.Title = SafeGetString("noiDung", "tieuDe", "MoTaNgan");
                            if (string.IsNullOrEmpty(displayData.Title)) displayData.Title = "Bạn có một thông báo hệ thống.";
                            displayData.Subtitle = "Vui lòng nhấn để xem chi tiết (nếu có).";
                            break;
                    }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chuyển đổi thông báo ID {id} sang ViewModel.", thongBao.Id);
            displayData.IconClass = "fas fa-exclamation-triangle text-danger";
            displayData.IsAdminNotification = true;
            displayData.TargetUrl = null;
            displayData.Title = "Thông báo này không thể hiển thị";
            displayData.Subtitle = "Đã có lỗi xảy ra. Vui lòng liên hệ quản trị viên.";
        }

        return displayData;
    }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId)) return userId;
            throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
        }
        #endregion
    }
}
