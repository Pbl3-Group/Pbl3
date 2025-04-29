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
    [Authorize(Roles = nameof(LoaiTaiKhoan.doanhnghiep))] // Chỉ Doanh nghiệp
    public class NhaTuyenDungController : Controller
    {
        private readonly ILogger<NhaTuyenDungController> _logger;
        private readonly ApplicationDbContext _context;

        public NhaTuyenDungController(ILogger<NhaTuyenDungController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /NhaTuyenDung/Index hoặc /NhaTuyenDung
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("Could not parse User ID for Employer Dashboard.");
                return Unauthorized("Không thể xác thực người dùng.");
            }

            _logger.LogInformation("NhaTuyenDung Dashboard accessed by User ID: {UserId}", userId);

            // Lấy hồ sơ doanh nghiệp của người dùng đang đăng nhập
            var employerProfile = await _context.HoSoDoanhNghieps
                                          .Include(h => h.NguoiDung) // Lấy thông tin người dùng nếu cần
                                          .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

            if (employerProfile == null)
            {
                _logger.LogWarning("Employer profile not found for User ID: {UserId}. Redirecting might be needed.", userId);
                // Cân nhắc chuyển hướng đến trang tạo/cập nhật hồ sơ hoặc báo lỗi rõ ràng hơn
                return NotFound("Không tìm thấy hồ sơ nhà tuyển dụng. Vui lòng liên hệ quản trị viên nếu bạn tin rằng đây là lỗi.");
            }

            // Truyền model HoSoDoanhNghiep vào View
            return View(employerProfile); // Sẽ tìm view tại /Views/NhaTuyenDung/Index.cshtml
        }

        // --- Thêm các Action khác cho Nhà Tuyển Dụng tại đây ---
        // Ví dụ:
        // public IActionResult DangTinTuyenDung() { /* ... */ return View(); }
        // public async Task<IActionResult> QuanLyTinDang() { /* ... */ return View(); }
        // public async Task<IActionResult> QuanLyUngVien() { /* ... */ return View(); }
        // public async Task<IActionResult> HoSoCongTy() { /* ... */ return View(); }

    }
}