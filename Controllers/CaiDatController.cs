// File: Controllers/CaiDatController.cs
using HeThongTimViec.Models; // Để lấy enum LoaiTaiKhoan
using HeThongTimViec.ViewModels.CaiDat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))] // Chỉ Admin mới được truy cập
    [Route("admin/cai-dat")] // Định tuyến URL cho gọn gàng
    public class CaiDatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CaiDatController> _logger;
        private readonly IMemoryCache _memoryCache;

        public CaiDatController(
            IConfiguration configuration,
            ILogger<CaiDatController> logger,
            IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        // GET: /admin/cai-dat
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            // Đọc các giá trị cài đặt từ appsettings.json và đổ vào ViewModel
            var model = new CaiDatViewModel
            {
                SiteName = _configuration["AppSettings:SiteName"],
                ContactEmail = _configuration["AppSettings:ContactEmail"],
                ContactPhone = _configuration["AppSettings:ContactPhone"],
                ItemsPerPage = _configuration.GetValue<int>("AppSettings:ItemsPerPage", 10),
                DefaultJobExpirationDays = _configuration.GetValue<int>("AppSettings:DefaultJobExpirationDays", 30),
                FacebookUrl = _configuration["AppSettings:SocialLinks:Facebook"],
                TwitterUrl = _configuration["AppSettings:SocialLinks:Twitter"],
                LinkedInUrl = _configuration["AppSettings:SocialLinks:LinkedIn"],

                // Đọc cài đặt SMTP (chỉ để hiển thị)
                SmtpServer = _configuration["SmtpSettings:Server"],
                SmtpPort = _configuration.GetValue<int>("SmtpSettings:Port"),
                SmtpUsername = _configuration["SmtpSettings:Username"],
            };

            return View(model);
        }

        // POST: /admin/cai-dat
        [HttpPost("")]
        [HttpPost("index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CaiDatViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu không hợp lệ, trả về view với các lỗi
                return View(model);
            }

            // *** LƯU Ý QUAN TRỌNG ***
            // Trong một ứng dụng thực tế, bạn KHÔNG NÊN ghi trực tiếp vào file appsettings.json.
            // Thay vào đó, bạn nên tạo một bảng `Settings` trong cơ sở dữ liệu để lưu các cặp key-value.
            // Sau đó, bạn sẽ cập nhật các bản ghi trong bảng này.
            // Đoạn code dưới đây chỉ mô phỏng việc lưu trữ và ghi log.

            _logger.LogInformation("Admin đang thực hiện lưu cài đặt hệ thống.");
            _logger.LogInformation("Tên trang web mới: {SiteName}", model.SiteName);
            _logger.LogInformation("Email liên hệ mới: {ContactEmail}", model.ContactEmail);
            _logger.LogInformation("ItemsPerPage mới: {ItemsPerPage}", model.ItemsPerPage);
            // Ghi log các cài đặt khác tương tự...

            // Mô phỏng việc xóa cache nếu được chọn
            if (model.ClearCache)
            {
                _logger.LogInformation("Admin yêu cầu xóa cache hệ thống.");
                // Trong thực tế, bạn sẽ xóa các key cache cụ thể.
                // Ví dụ: _memoryCache.Remove("HomePageViewModel_Cache");
                // Ở đây chúng ta chỉ ghi log.
                TempData["SuccessMessage"] = "Cache hệ thống đã được xóa thành công!";
            }
            else
            {
                TempData["SuccessMessage"] = "Cài đặt đã được lưu thành công! (Mô phỏng)";
            }
            
            // Tạm dừng một chút để người dùng cảm nhận được hành động
            await Task.Delay(500);

            // Chuyển hướng về lại trang Index để hiển thị thông báo
            return RedirectToAction(nameof(Index));
        }
    }
}