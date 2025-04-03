using Microsoft.AspNetCore.Mvc;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Text.RegularExpressions;

public class AccountController : Controller
{
    private readonly HeThongTimViecContext _context;

    public AccountController(HeThongTimViecContext context)
    {
        _context = context;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
[HttpPost]
public async Task<IActionResult> Login(string EmailOrPhone, string Password)
{
    try
    {
        if (string.IsNullOrEmpty(EmailOrPhone) || string.IsNullOrEmpty(Password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
            return View();
        }

        if (!Regex.IsMatch(EmailOrPhone, @"^[\w\.-]+@[\w\.-]+\.\w+$") && !Regex.IsMatch(EmailOrPhone, @"^\d{10}$"))
        {
            ViewBag.Error = "Email hoặc số điện thoại không hợp lệ";
            return View();
        }

        // Truy vấn người dùng bằng Email hoặc SĐT
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == EmailOrPhone || u.SDT == EmailOrPhone);

        if (user == null)
        {
            ViewBag.Error = "Tài khoản không tồn tại";
            return View();
        }

        // Kiểm tra trạng thái tài khoản
        if (user.TrangThai == TrangThaiEnum.Bi_Cam)
        {
            ViewBag.Error = "Tài khoản của bạn đã bị khóa";
            return View();
        }

        // Kiểm tra mật khẩu (so sánh với mật khẩu băm)
        if (user.MatKhau != Password)
        {
            ViewBag.Error = "Sai mật khẩu";
            return View();
        }

        // Lưu thông tin người dùng vào session (hoặc cookie)
        HttpContext.Session.SetInt32("UserId", user.UserId);

        return RedirectToAction("Index", "Home");
    }
    catch (Exception)
    {
        // Xử lý ngoại lệ chung (có thể là lỗi DB, timeout, v.v.)
        ViewBag.Error = "Đã xảy ra lỗi, vui lòng thử lại sau";
        // Ghi log lỗi nếu cần (tùy chọn)
        // _logger.LogError("Lỗi khi đăng nhập với {EmailOrPhone}", EmailOrPhone);
        return View();
    }
}
    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    public IActionResult Register(string FullName, string Email, string Password)
    {
        // Xử lý đăng ký ở đây
        // Ghi DB giả sử
        return RedirectToAction("Login");
    }

    // GET: /Account/ForgotPassword
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    // POST: /Account/ForgotPassword
    [HttpPost]
    public IActionResult ForgotPassword(string Email)
    {
        // Giả sử gửi email reset mật khẩu (ở đây chỉ hiển thị thông báo)
        if (Email == "admin@example.com")
        {
            ViewBag.Message = "Một email hướng dẫn đã được gửi tới địa chỉ của bạn";
        }
        else
        {
            ViewBag.Error = "Email không tồn tại";
        }

        return View();
    }
}