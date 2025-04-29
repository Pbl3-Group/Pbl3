// File: Controllers/TuyenDungCaNhanController.cs
using HeThongTimViec.Data; // Cần cho DbContext (dù Index này chưa dùng)
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Cần cho IHttpContextAccessor và Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks; // Cần cho async Task

namespace HeThongTimViec.Controllers
{
    // --- BẢO VỆ CONTROLLER ---
    [Authorize(Roles = nameof(LoaiTaiKhoan.canhan))] // Chỉ user 'canhan' mới vào được
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // Ngăn caching
    public class TuyenDungCaNhanController : Controller
    {
        // --- DEPENDENCIES ---
        // Vẫn giữ lại DbContext và Logger phòng trường hợp dùng sau
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TuyenDungCaNhanController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // --- CONSTRUCTOR ---
        public TuyenDungCaNhanController(
            ApplicationDbContext context,
            ILogger<TuyenDungCaNhanController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // --- HÀM HELPER KIỂM TRA SESSION ---
        private bool IsNtdView()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetInt32("DangLaNTD") == 1;
        }

        // --- ACTION INDEX (Dashboard NTD Cá nhân - TỐI GIẢN) ---
        // GET: /TuyenDungCaNhan/Index hoặc /TuyenDungCaNhan
        public IActionResult Index() // Bỏ async Task<> vì không còn await
        {
            // 1. Kiểm tra chế độ xem từ Session
            if (!IsNtdView())
            {
                var userIdForLog = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "N/A";
                _logger.LogWarning("User {UserId} attempted to access NTD CN dashboard while in Candidate view. Redirecting to UngVien/Index.", userIdForLog);
                return RedirectToAction("Index", "UngVien");
            }

            // 2. Lấy User ID và Tên từ Claims (đơn giản hóa, không cần query DB)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var hoTen = User.FindFirstValue(ClaimTypes.Name); // Lấy tên từ Claim đã tạo khi đăng nhập

            // Kiểm tra User ID có hợp lệ không (vẫn nên làm)
             if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out _)) // Chỉ cần kiểm tra parse thành công không
             {
                 _logger.LogError("Could not parse User ID from claims for NTD CN dashboard. User Claim Name: {UserName}", hoTen ?? "N/A");
                 return Unauthorized("Không thể xác định người dùng.");
             }

            _logger.LogInformation("NTD CN Dashboard accessed by User ID: {UserId}", userIdString);

            // 3. Truyền tên vào ViewBag để chào mừng
            ViewBag.HoTenNguoiDung = hoTen;

            // 4. Trả về View (không cần model vì không có dữ liệu phức tạp)
            return View(); // Sẽ tìm Views/TuyenDungCaNhan/Index.cshtml
        }

        // --- CÁC ACTIONS KHÁC ĐỂ TRỐNG ---
        // public IActionResult DangTin() => View(); // Trả về View rỗng nếu cần
        // ...
    }
}