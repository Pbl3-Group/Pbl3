using HeThongTimViec.Data;
using HeThongTimViec.Models; // Đảm bảo namespace đúng
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.canhan))] // Chỉ Cá nhân (Ứng viên)
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class UngVienController : Controller
    {
        private readonly ILogger<UngVienController> _logger;
        private readonly ApplicationDbContext _context;

        public UngVienController(ILogger<UngVienController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /UngVien/Index hoặc /UngVien
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("Could not parse User ID for Candidate Dashboard.");
                return Unauthorized("Không thể xác thực người dùng.");
            }

            _logger.LogInformation("UngVien Dashboard accessed by User ID: {UserId}", userId);

            // Lấy hồ sơ ứng viên của người dùng đang đăng nhập
            var candidateProfile = await _context.HoSoUngViens
                                          .Include(h => h.NguoiDung) // Lấy thông tin người dùng nếu cần
                                          .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

            if (candidateProfile == null)
            {
                _logger.LogWarning("Candidate profile not found for User ID: {UserId}. Redirecting might be needed.", userId);
                // Cân nhắc chuyển hướng đến trang tạo/cập nhật hồ sơ hoặc báo lỗi rõ ràng hơn
                return NotFound("Không tìm thấy hồ sơ ứng viên. Vui lòng cập nhật hồ sơ của bạn hoặc liên hệ quản trị viên.");
            }

            // Truyền model HoSoUngVien vào View
            return View(candidateProfile); // Sẽ tìm view tại /Views/UngVien/Index.cshtml
        }

        // --- Thêm các Action khác cho Ứng viên tại đây ---
        // Ví dụ:
        // public IActionResult TimKiemViecLam() { /* ... */ return View(); }
        // public async Task<IActionResult> QuanLyHoSoCV() { /* ... */ return View(); }
        // public async Task<IActionResult> ViecLamDaUngTuyen() { /* ... */ return View(); }
        // public async Task<IActionResult> ViecLamDaLuu() { /* ... */ return View(); }
    }
}