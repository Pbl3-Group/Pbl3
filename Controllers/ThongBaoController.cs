// File: Controllers/ThongBaoController.cs
using HeThongTimViec.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims; // Cần thiết cho User.FindFirstValue
using Microsoft.AspNetCore.Authorization; // Nếu bạn muốn bảo vệ các API này
using HeThongTimViec.Utils; // Để sử dụng NotificationConstants
using System.Text.Json; // Để sử dụng JsonSerializer

namespace HeThongTimViec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Đảm bảo chỉ người dùng đã đăng nhập mới truy cập được
    public class ThongBaoController : ControllerBase
    {
        private readonly IThongBaoService _thongBaoService;
        private readonly ILogger<ThongBaoController> _logger;

        public ThongBaoController(IThongBaoService thongBaoService, ILogger<ThongBaoController> logger)
        {
            _thongBaoService = thongBaoService;
            _logger = logger;
        }

        // Hàm helper để lấy ID người dùng hiện tại từ token
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Hoặc loại claim tùy chỉnh của bạn cho User ID
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            // Ghi log và ném ngoại lệ nếu không tìm thấy User ID
            _logger.LogWarning("Không thể lấy User ID từ token. Claim NameIdentifier không tồn tại hoặc không hợp lệ.");
            throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
        }

        // GET: api/ThongBao/MyNotifications
        [HttpGet("MyNotifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _thongBaoService.GetThongBaosForUserAsync(userId, page, pageSize);
                return Ok(notifications);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo cho người dùng.");
                return StatusCode(500, "Lỗi máy chủ nội bộ khi lấy thông báo.");
            }
        }

        // GET: api/ThongBao/UnreadCount
        [HttpGet("UnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _thongBaoService.GetUnreadThongBaoCountAsync(userId);
                return Ok(new { count = count });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng thông báo chưa đọc.");
                return StatusCode(500, "Lỗi máy chủ nội bộ khi lấy số lượng thông báo.");
            }
        }

        // POST: api/ThongBao/MarkAsRead/5
        [HttpPost("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _thongBaoService.MarkAsReadAsync(id, userId);
                if (success)
                {
                    return Ok(new { message = "Thông báo đã được đánh dấu là đã đọc." });
                }
                return NotFound(new { message = "Không tìm thấy thông báo hoặc bạn không có quyền." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc. ID: {NotificationId}", id);
                return StatusCode(500, "Lỗi máy chủ nội bộ.");
            }
        }

        // POST: api/ThongBao/MarkAllAsRead
        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _thongBaoService.MarkAllAsReadAsync(userId);
                if (success)
                {
                    // Kiểm tra xem có thông báo nào thực sự được đánh dấu đã đọc không
                    // Có thể service trả về true ngay cả khi không có gì để đánh dấu
                    var unreadCount = await _thongBaoService.GetUnreadThongBaoCountAsync(userId);
                    if(unreadCount == 0)
                         return Ok(new { message = "Tất cả thông báo đã được đánh dấu là đã đọc (hoặc không có thông báo nào chưa đọc)." });
                    else
                         return Ok(new { message = "Tất cả thông báo đã được đánh dấu là đã đọc." });

                }
                // Trường hợp này service trả về false nếu có lỗi
                return StatusCode(500, "Lỗi khi đánh dấu tất cả thông báo là đã đọc.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo là đã đọc.");
                return StatusCode(500, "Lỗi máy chủ nội bộ.");
            }
        }
    }
}