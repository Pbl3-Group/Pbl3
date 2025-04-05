using Microsoft.AspNetCore.Mvc;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

// namespace JobFlex.Controllers
// {
//     public class UngVienController : Controller
//     {
//         // Action cho trang chủ của ứng viên
//         public ActionResult Index()
//         {
//             // Ở đây bạn có thể thêm logic để lấy dữ liệu (nếu cần)
//             // Ví dụ: lấy danh sách công việc từ database
//             return View();
//         }

//         // Action cho trang thông tin cá nhân của ứng viên
//         public ActionResult Profile()
//         {
//             // Logic để hiển thị thông tin cá nhân ứng viên
//             return View(); // Tạo file Profile.cshtml nếu cần
//         }
//     }
// }

namespace JobFlexJobFlex.Controllers
{
    public class UngVienController : Controller
    {
        // GET: Job
        public ActionResult Index()
        {
            // Trả về view Index.cshtml
            return View();
        }

        public ActionResult SavedJobs()
        {
            // Logic để lấy công việc đã lưu
            return View();
        }

        public ActionResult AppliedJobs()
        {
            // Logic để lấy công việc đang ứng tuyển
            return View();
        }

        public ActionResult Notifications()
        {
            // Logic để lấy thông báo
            return View();
        }

        public ActionResult Messages()
        {
            // Logic để lấy tin nhắn
            return View();
        }
    }
}