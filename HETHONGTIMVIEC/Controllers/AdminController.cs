using HeThongTimViec.Data;
using HeThongTimViec.Models; // Đảm bảo namespace đúng cho LoaiTaiKhoan
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))] // Chỉ Admin
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Admin/Index hoặc /Admin
        public IActionResult Index()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin Dashboard accessed by Admin ID: {AdminId}", adminId);

            // TODO: Thêm logic để lấy dữ liệu tổng quan cho trang Admin (ví dụ: số tài khoản chờ duyệt)
            // var taiKhoanChoDuyetCount = await _context.NguoiDungs.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.choxacminh && u.LoaiTk == LoaiTaiKhoan.doanhnghiep);
            // ViewBag.TaiKhoanChoDuyet = taiKhoanChoDuyetCount;

            return View(); // Sẽ tìm view tại /Views/Admin/Index.cshtml
        }

        // --- Thêm các Action khác cho Admin tại đây ---
        // Ví dụ:
        // public async Task<IActionResult> QuanLyNguoiDung() { /* ... */ return View(); }
        // public async Task<IActionResult> PheDuyetDoanhNghiep() { /* ... */ return View(); }
    }
}