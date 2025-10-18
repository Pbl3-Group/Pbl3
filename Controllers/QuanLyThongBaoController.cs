using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.QuanLyThongBao; // Using namespace của ViewModels
using HeThongTimViec.ViewModels.TimViec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
    public class QuanLyThongBaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLyThongBaoController> _logger;

        // Định nghĩa các loại thông báo mà Admin có thể chọn, dễ dàng quản lý và mở rộng
        private static readonly Dictionary<string, string> AdminNotificationTypes = new()
        {
        { "THONG_BAO_CHUNG", "Thông báo chung" },
        { "BAO_TRI_HE_THONG", "Bảo trì hệ thống" },
        { "CANH_BAO_TAI_KHOAN", "Cảnh báo tài khoản" },
        { "TIN_TUC_CAP_NHAT", "Tin tức & Cập nhật" },
        { "PHAN_HOI_HO_SO", "Phản hồi hồ sơ" },
        { "LOI_MO_PHONG_VIEC_LAM", "Cảnh báo lừa đảo" },
        { "THONG_BAO_SUC_KHOE", "Cảnh báo thời tiết / sức khỏe" },
        { "NHAC_NHO_HO_SO", "Nhắc nhở hồ sơ" },
        { "NHAC_NHO_TIN_DANG", "Nhắc nhở tin đăng" }
        };
        public QuanLyThongBaoController(ApplicationDbContext context, ILogger<QuanLyThongBaoController> logger)
        {
            _context = context;
            _logger = logger;
        }
        

        // GET: /QuanLyThongBao
        public async Task<IActionResult> Index(CampaignFilterViewModel filter, int page = 1)
        {
            var query = _context.ThongBaos.AsNoTracking();

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var keywordLower = filter.Keyword.ToLower();
                // Tìm kiếm trong trường DuLieu JSON, một cách tiếp cận đơn giản nhưng hiệu quả
                query = query.Where(t => t.DuLieu.ToLower().Contains(keywordLower));
            }
            if (!string.IsNullOrEmpty(filter.LoaiThongBao))
            {
                query = query.Where(t => t.LoaiThongBao == filter.LoaiThongBao);
            }
            if (filter.StartDate.HasValue)
            {
                query = query.Where(t => t.NgayTao.Date >= filter.StartDate.Value.Date);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(t => t.NgayTao.Date <= filter.EndDate.Value.Date);
            }

            // Lấy danh sách thô đã được lọc
            var allAdminNotifications = await query
                .Where(t => AdminNotificationTypes.Keys.Contains(t.LoaiThongBao))
                .ToListAsync();

            // Nhóm các thông báo lại thành các "chiến dịch" dựa trên BatchId
            var campaigns = allAdminNotifications
                .Select(t =>
                {
                    try { return new { Data = JsonSerializer.Deserialize<AdminNotificationData>(t.DuLieu), Notification = t }; }
                    catch { return null; }
                })
                .Where(d => d != null && d.Data != null)
                .GroupBy(d => d!.Data!.BatchId)
                .Select(g =>
                {
                    var first = g.First();
                    var admin = (first != null && first.Data != null)
                        ? _context.NguoiDungs.AsNoTracking().FirstOrDefault(u => u.Id == first.Data.AdminGuiId)
                        : null;
                    return new CampaignIndexViewModel
                    {
                        BatchId = g.Key,
                        TieuDe = (first != null && first.Data != null) ? first.Data.TieuDe : string.Empty,
                        LoaiThongBao = (first != null && first.Notification != null)
                            ? AdminNotificationTypes.GetValueOrDefault(first.Notification.LoaiThongBao, first.Notification.LoaiThongBao)
                            : string.Empty,
                        TenAdminGui = admin?.HoTen ?? "N/A",
                        NgayGuiDisplay = (first != null && first.Notification != null)
                            ? first.Notification.NgayTao.ToLocalTime().ToString("dd/MM/yyyy 'lúc' HH:mm")
                            : string.Empty,
                        SoNguoiNhan = g.Count(),
                        SoNguoiDaDoc = g.Count(x => x != null && x.Notification != null && x.Notification.DaDoc)
                    };
                })
                .OrderByDescending(c => DateTime.ParseExact(c.NgayGuiDisplay, "dd/MM/yyyy 'lúc' HH:mm", CultureInfo.InvariantCulture));

            var paginatedCampaigns = PaginatedList<CampaignIndexViewModel>.Create(campaigns, page, 10);

            var viewModel = new CampaignManagementViewModel
            {
                Campaigns = paginatedCampaigns,
                Filter = filter
            };
            PopulateFilterOptions(viewModel.Filter);

            return View(viewModel);
        }

        // GET: /QuanLyThongBao/Details/{batchId}
        public async Task<IActionResult> Details(string batchId, int page = 1)
        {
            if (string.IsNullOrEmpty(batchId)) return NotFound();

            // Truy vấn để lấy các thông báo thuộc lô này
            var notificationsInBatchQuery = _context.ThongBaos
                .Include(t => t.NguoiDung) // Lấy thông tin người nhận
                .Where(t => t.DuLieu.Contains($"\"BatchId\":\"{batchId}\""));

            var paginatedNotifications = await PaginatedList<Models.ThongBao>.CreateAsync(notificationsInBatchQuery.AsNoTracking(), page, 20);

            if (!paginatedNotifications.Any()) return NotFound();

            var firstNotification = paginatedNotifications.First();
            var data = JsonSerializer.Deserialize<AdminNotificationData>(firstNotification.DuLieu);
            var admin = await _context.NguoiDungs.FindAsync(data!.AdminGuiId);

            var viewModel = new CampaignDetailsViewModel
            {
                TieuDe = data.TieuDe,
                NoiDung = data.NoiDung,
                LoaiThongBaoDisplay = AdminNotificationTypes.GetValueOrDefault(firstNotification.LoaiThongBao, firstNotification.LoaiThongBao),
                TenAdminGui = admin?.HoTen ?? "N/A",
                NgayGuiDisplay = firstNotification.NgayTao.ToLocalTime().ToString("dddd, 'ngày' dd 'tháng' MM 'năm' yyyy 'lúc' HH:mm", new CultureInfo("vi-VN")),
                DanhSachNguoiNhan = paginatedNotifications
            };

            return View(viewModel);
        }

        // GET: /QuanLyThongBao/Create
        public IActionResult Create()
        {
            var viewModel = new CampaignCreateViewModel();
            PopulateCreateOptions(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CampaignCreateViewModel viewModel)
        {
            // Bước 1: Kiểm tra validation tùy chỉnh
            // Nếu người dùng chọn "Người dùng cụ thể" nhưng lại không chọn ai, thêm lỗi vào ModelState.
            if (viewModel.TargetType == NotificationTargetType.SpecificUser && (viewModel.SpecificUserIds == null || !viewModel.SpecificUserIds.Any()))
            {
                ModelState.AddModelError(nameof(viewModel.SpecificUserIds), "Vui lòng chọn ít nhất một người dùng.");
            }

            // Bước 2: Kiểm tra tổng thể ModelState (bao gồm cả lỗi từ model và lỗi tùy chỉnh ở trên)
            if (!ModelState.IsValid)
            {
                // ==========================================================
                // === KHU VỰC SỬA LỖI VÀ NẠP LẠI DỮ LIỆU KHI FORM LỖI ===
                // ==========================================================

                // Nếu người dùng đã chọn một vài người nhưng các trường khác bị lỗi,
                // chúng ta cần nạp lại thông tin của những người đã chọn đó để hiển thị lại trên form.
                if (viewModel.SpecificUserIds != null && viewModel.SpecificUserIds.Any())
                {
                    var selectedUsers = await _context.NguoiDungs
                        .Where(u => viewModel.SpecificUserIds.Contains(u.Id))
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id.ToString(),
                            Text = $"{u.HoTen} ({u.Email})",
                            Selected = true
                        })
                        .ToListAsync();

                    // Gán danh sách người dùng đã chọn vào thuộc tính PreSelectedUsers của ViewModel
                    viewModel.PreSelectedUsers = selectedUsers;
                }

                // Luôn populate lại các dropdown khác (ví dụ: Loại thông báo) để form không bị mất dữ liệu
                PopulateCreateOptions(viewModel);

                // Trả về View với ViewModel đã được cập nhật, hiển thị các lỗi và giữ lại lựa chọn của người dùng
                return View(viewModel);
            }

            // ==========================================================
            // === LOGIC XỬ LÝ KHI FORM HỢP LỆ ===
            // ==========================================================

            // Bước 3: Lấy ID của Admin đang thực hiện hành động
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdString, out var adminId))
            {
                // Xử lý trường hợp không lấy được ID admin (rất hiếm khi xảy ra nếu đã đăng nhập)
                ModelState.AddModelError("", "Không thể xác thực quản trị viên. Vui lòng thử đăng nhập lại.");
                PopulateCreateOptions(viewModel);
                return View(viewModel);
            }

            // Bước 4: Lấy danh sách ID người nhận dựa trên lựa chọn
            var recipientIds = await GetRecipientIds(viewModel.TargetType, viewModel.SpecificUserIds);

            if (!recipientIds.Any())
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng nào phù hợp với đối tượng đã chọn để gửi.");
                PopulateCreateOptions(viewModel);
                return View(viewModel);
            }

            // Bước 5: Chuẩn bị dữ liệu để lưu vào database
            var batchId = Guid.NewGuid().ToString();
            var data = new AdminNotificationData
            {
                BatchId = batchId,
                TieuDe = viewModel.TieuDe,
                NoiDung = viewModel.NoiDung,
                AdminGuiId = adminId,
            };
            var jsonData = JsonSerializer.Serialize(data);

            // Tạo một danh sách các đối tượng ThongBao mới, mỗi đối tượng cho một người nhận
            var newNotifications = recipientIds.Select(userId => new Models.ThongBao
            {
                NguoiDungId = userId,
                LoaiThongBao = viewModel.LoaiThongBao,
                DuLieu = jsonData,
                DaDoc = false,
                NgayTao = DateTime.UtcNow
            }).ToList();

            // Bước 6: Lưu đồng loạt vào database
            await _context.ThongBaos.AddRangeAsync(newNotifications);
            await _context.SaveChangesAsync();

            // Bước 7: Thông báo thành công và chuyển hướng
            TempData["SuccessMessage"] = $"Đã gửi thành công thông báo '{viewModel.TieuDe}' đến {recipientIds.Count} người dùng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /QuanLyThongBao/Delete
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(string batchId)
{
    if (string.IsNullOrEmpty(batchId))
    {
        return NotFound();
    }

    // <<< SỬA LẠI ĐIỀU KIỆN TRUY VẤN TẠI ĐÂY >>>
    var notificationsToDelete = await _context.ThongBaos
       .Where(t => 
           // 1. Kiểm tra xem LoaiThongBao có phải là loại do admin gửi không
           AdminNotificationTypes.Keys.Contains(t.LoaiThongBao) && 
           // 2. Tìm chính xác BatchId trong chuỗi JSON DuLieu
           t.DuLieu.Contains($"\"BatchId\":\"{batchId}\"")
       )
       .ToListAsync();

    if (!notificationsToDelete.Any())
    {
        _logger.LogWarning("Attempted to delete a non-existent or unauthorized batch with BatchId: {BatchId}", batchId);
        TempData["ErrorMessage"] = "Không tìm thấy lô thông báo để xóa hoặc lô thông báo này không thuộc quyền quản lý của bạn.";
        return RedirectToAction(nameof(Index));
    }
    
    _context.ThongBaos.RemoveRange(notificationsToDelete);
    await _context.SaveChangesAsync();
    
    TempData["SuccessMessage"] = $"Đã xóa thành công lô thông báo và {notificationsToDelete.Count} bản ghi liên quan.";
    return RedirectToAction(nameof(Index));
}

        #region Helper Methods

        // Chuẩn bị dữ liệu cho Dropdown trong form Create
        private void PopulateCreateOptions(CampaignCreateViewModel viewModel)
        {
            viewModel.LoaiThongBaoOptions = new SelectList(AdminNotificationTypes, "Key", "Value", viewModel.LoaiThongBao);
        }

        // Chuẩn bị dữ liệu cho Dropdown trong bộ lọc
        private void PopulateFilterOptions(CampaignFilterViewModel filter)
        {
            filter.LoaiThongBaoOptions = new SelectList(AdminNotificationTypes, "Key", "Value", filter.LoaiThongBao);
        }

        // Lấy danh sách ID người nhận dựa trên lựa chọn
        private async Task<List<int>> GetRecipientIds(NotificationTargetType targetType, List<int>? specificUserIds)
        {
            return targetType switch
            {
                NotificationTargetType.AllUsers => await _context.NguoiDungs.Select(u => u.Id).ToListAsync(),
                NotificationTargetType.AllCandidates => await _context.NguoiDungs.Where(u => u.LoaiTk == LoaiTaiKhoan.canhan).Select(u => u.Id).ToListAsync(),
                NotificationTargetType.AllEmployers => await _context.NguoiDungs.Where(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep).Select(u => u.Id).ToListAsync(),
                NotificationTargetType.SpecificUser => specificUserIds ?? new List<int>(),
                _ => new List<int>(),
            };
        }

        // API phục vụ cho Select2 tìm kiếm người dùng
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new { results = new List<object>() });
            }

            var keywordLower = term.ToLower();

            var users = await _context.NguoiDungs
                .Where(u => u.HoTen.ToLower().Contains(keywordLower) || u.Email.ToLower().Contains(keywordLower))
                .Take(10)
                .Select(u => new
                {
                    // id và text là bắt buộc cho Select2
                    id = u.Id,
                    text = u.HoTen, // Text chính sẽ là họ tên

                    // Các trường dữ liệu bổ sung để hiển thị
                    email = u.Email,
                    avatar = u.UrlAvatar ?? "/images/default-avatar.png" // Cung cấp avatar mặc định nếu null
                })
                .ToListAsync();

            return Json(new { results = users });
        }
        #endregion
    }
}