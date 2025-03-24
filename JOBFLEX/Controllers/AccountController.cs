using Microsoft.AspNetCore.Mvc;
using JOBFLEX.Models;
using JOBFLEX.Data; // Namespace của JobFlexDbContext
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace JOBFLEX.Controllers
{
    public class AccountController : Controller
    {
        private readonly JobFlexDbContext _context;

        // Inject DbContext qua constructor
        public AccountController(JobFlexDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra thông tin đăng nhập bằng email hoặc số điện thoại
                var user = _context.Users.FirstOrDefault(u =>
                    (u.Email == model.EmailOrFacebook || u.PhoneNumber == model.EmailOrFacebook)
                    && u.Password == model.Password); // Nên thay bằng BCrypt sau

                if (user != null)
                {
                    // Thiết lập phiên đăng nhập
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Email/SĐT hoặc mật khẩu không đúng.";
                    return View(model);
                }
            }
            return View(model);
        }

        // Hiển thị trang đăng ký
        public IActionResult Register()
        {
            return View();
        }
    }
}