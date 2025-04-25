using Microsoft.AspNetCore.Mvc;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNetCore.Http;
using BCrypt.Net; // Thêm using cho BCrypt.Net

namespace HeThongTimViec.Controllers
{
    public class AccountController : Controller
    {
        private readonly HeThongTimViecContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(HeThongTimViecContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
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

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == EmailOrPhone || u.SDT == EmailOrPhone);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại";
                return View();
            }

            if (user.TrangThai == TrangThaiEnum.Bi_Cam)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa";
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(Password, user.MatKhau))
            {
                ViewBag.Error = "Sai mật khẩu";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.VaiTro.ToString());

            switch (user.VaiTro)
            {
                case VaiTroEnum.Ung_Vien:
                return RedirectToAction("Index", "UngVien");
                case VaiTroEnum.Nha_Tuyen_Dung:
                return RedirectToAction("Index", "NhaTuyenDung");
                case VaiTroEnum.Quan_Tri_Vien:
                return RedirectToAction("Index", "Admin");
                default:
                _logger.LogWarning("Vai trò không hợp lệ cho người dùng {UserId}", user.UserId);
                ViewBag.Error = "Vai trò không hợp lệ";
                return View();
            }
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Lỗi khi đăng nhập với EmailOrPhone: {EmailOrPhone}", EmailOrPhone);
            ViewBag.Error = "Đã xảy ra lỗi, vui lòng thử lại sau";
            return View();
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/RegisterUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            // Kiểm tra email, Facebook link, và số điện thoại đã tồn tại
            if (!string.IsNullOrEmpty(model.Email) && _context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View("Register", model);
            }
            if (!string.IsNullOrEmpty(model.FacebookLink) && _context.Users.Any(u => u.FacebookLink == model.FacebookLink))
            {
                ModelState.AddModelError("FacebookLink", "Liên kết Facebook đã được sử dụng.");
                return View("Register", model);
            }
            if (_context.Users.Any(u => u.SDT == model.SDT))
            {
                ModelState.AddModelError("SDT", "Số điện thoại đã được sử dụng.");
                return View("Register", model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Ánh xạ ThanhPho từ chuỗi sang enum
                var thanhPhoMap = new Dictionary<string, ThanhPhoEnum>
                {
                    { "An Giang", ThanhPhoEnum.An_Giang },
                    { "Bà Rịa - Vũng Tàu", ThanhPhoEnum.Ba_Ria_Vung_Tau },
                    { "Bạc Liêu", ThanhPhoEnum.Bac_Lieu },
                    { "Bắc Giang", ThanhPhoEnum.Bac_Giang },
                    { "Bắc Kạn", ThanhPhoEnum.Bac_Kan },
                    { "Bắc Ninh", ThanhPhoEnum.Bac_Ninh },
                    { "Bến Tre", ThanhPhoEnum.Ben_Tre },
                    { "Bình Dương", ThanhPhoEnum.Binh_Duong },
                    { "Bình Định", ThanhPhoEnum.Binh_Dinh },
                    { "Bình Phước", ThanhPhoEnum.Binh_Phuoc },
                    { "Bình Thuận", ThanhPhoEnum.Binh_Thuan },
                    { "Cà Mau", ThanhPhoEnum.Ca_Mau },
                    { "Cao Bằng", ThanhPhoEnum.Cao_Bang },
                    { "Cần Thơ", ThanhPhoEnum.Can_Tho },
                    { "Đà Nẵng", ThanhPhoEnum.Da_Nang },
                    { "Đắk Lắk", ThanhPhoEnum.Dak_Lak },
                    { "Đắk Nông", ThanhPhoEnum.Dak_Nong },
                    { "Điện Biên", ThanhPhoEnum.Dien_Bien },
                    { "Đồng Nai", ThanhPhoEnum.Dong_Nai },
                    { "Đồng Tháp", ThanhPhoEnum.Dong_Thap },
                    { "Gia Lai", ThanhPhoEnum.Gia_Lai },
                    { "Hà Giang", ThanhPhoEnum.Ha_Giang },
                    { "Hà Nam", ThanhPhoEnum.Ha_Nam },
                    { "Hà Nội", ThanhPhoEnum.Ha_Noi },
                    { "Hà Tĩnh", ThanhPhoEnum.Ha_Tinh },
                    { "Hải Dương", ThanhPhoEnum.Hai_Duong },
                    { "Hải Phòng", ThanhPhoEnum.Hai_Phong },
                    { "Hậu Giang", ThanhPhoEnum.Hau_Giang },
                    { "Hòa Bình", ThanhPhoEnum.Hoa_Binh },
                    { "Hưng Yên", ThanhPhoEnum.Hung_Yen },
                    { "Khánh Hòa", ThanhPhoEnum.Khanh_Hoa },
                    { "Kiên Giang", ThanhPhoEnum.Kien_Giang },
                    { "Kon Tum", ThanhPhoEnum.Kon_Tum },
                    { "Lai Châu", ThanhPhoEnum.Lai_Chau },
                    { "Lâm Đồng", ThanhPhoEnum.Lam_Dong },
                    { "Lạng Sơn", ThanhPhoEnum.Lang_Son },
                    { "Lào Cai", ThanhPhoEnum.Lao_Cai },
                    { "Long An", ThanhPhoEnum.Long_An },
                    { "Nam Định", ThanhPhoEnum.Nam_Dinh },
                    { "Nghệ An", ThanhPhoEnum.Nghe_An },
                    { "Ninh Bình", ThanhPhoEnum.Ninh_Binh },
                    { "Ninh Thuận", ThanhPhoEnum.Ninh_Thuan },
                    { "Phú Thọ", ThanhPhoEnum.Phu_Tho },
                    { "Phú Yên", ThanhPhoEnum.Phu_Yen },
                    { "Quảng Bình", ThanhPhoEnum.Quang_Binh },
                    { "Quảng Nam", ThanhPhoEnum.Quang_Nam },
                    { "Quảng Ngãi", ThanhPhoEnum.Quang_Ngai },
                    { "Quảng Ninh", ThanhPhoEnum.Quang_Ninh },
                    { "Quảng Trị", ThanhPhoEnum.Quang_Tri },
                    { "Sóc Trăng", ThanhPhoEnum.Soc_Trang },
                    { "Sơn La", ThanhPhoEnum.Son_La },
                    { "Tây Ninh", ThanhPhoEnum.Tay_Ninh },
                    { "Thái Bình", ThanhPhoEnum.Thai_Binh },
                    { "Thái Nguyên", ThanhPhoEnum.Thai_Nguyen },
                    { "Thanh Hóa", ThanhPhoEnum.Thanh_Hoa },
                    { "Thừa Thiên Huế", ThanhPhoEnum.Thua_Thien_Hue },
                    { "Tiền Giang", ThanhPhoEnum.Tien_Giang },
                    { "TP. Hồ Chí Minh", ThanhPhoEnum.TP_Ho_Chi_Minh },
                    { "Trà Vinh", ThanhPhoEnum.Tra_Vinh },
                    { "Tuyên Quang", ThanhPhoEnum.Tuyen_Quang },
                    { "Vĩnh Long", ThanhPhoEnum.Vinh_Long },
                    { "Vĩnh Phúc", ThanhPhoEnum.Vinh_Phuc },
                    { "Yên Bái", ThanhPhoEnum.Yen_Bai }
                };

                var thanhPhoString = model.ThanhPho?.ToString();
                if (string.IsNullOrWhiteSpace(thanhPhoString) || !thanhPhoMap.ContainsKey(thanhPhoString))
                {
                    ModelState.AddModelError("ThanhPho", "Thành phố không hợp lệ.");
                    return View("Register", model);
                }
                ThanhPhoEnum? thanhPhoEnum = thanhPhoMap[thanhPhoString];

                // Tạo user
                var user = new User
                {
                    HoTen = model.HoTen,
                    GioiTinh = model.GioiTinh, // Đã là GioiTinhEnum
                    SDT = model.SDT,
                    NgaySinh = model.NgaySinh,
                    Email = model.Email,
                    FacebookLink = model.FacebookLink,
                    MatKhau = HashPassword(model.MatKhau),
                    ThanhPho = thanhPhoEnum,
                    MoTa = model.MoTa,
                    NgayTao = DateTime.UtcNow,
                    VaiTro = VaiTroEnum.Ung_Vien,
                    TrangThai = TrangThaiEnum.Chap_Thuan
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Lưu CV nếu có
                if (model.CVFile != null && model.CVFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var ext = Path.GetExtension(model.CVFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("CVFile", "Định dạng CV không hợp lệ.");
                        return View("Register", model);
                    }
                    if (model.CVFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("CVFile", "Dung lượng file quá lớn (tối đa 5MB).");
                        return View("Register", model);
                    }

                    var cvPath = SaveCvFile(model.CVFile);
                    _context.CVs.Add(new CV
                    {
                        UserId = user.UserId,
                        FilePath = cvPath,
                        User = user
                    });
                }
// Log WorkAvailabilities để kiểm tra
        _logger.LogInformation("WorkAvailabilities Count: {Count}", model.WorkAvailabilities?.Count ?? 0);
        if (model.WorkAvailabilities != null && model.WorkAvailabilities.Count > 0)
        {
            foreach (var availability in model.WorkAvailabilities)
            {
                // Chuyển đổi từ string sang enum
                if (!Enum.TryParse<NgayEnum>(availability.Ngay, true, out var ngayEnum))
                {
                    _logger.LogWarning("Invalid Ngay value: {Ngay}", availability.Ngay);
                    continue;
                }
                if (!Enum.TryParse<ThoiGianEnum>(availability.ThoiGian, true, out var thoiGianEnum))
                {
                    _logger.LogWarning("Invalid ThoiGian value: {ThoiGian}", availability.ThoiGian);
                    continue;
                }

                _logger.LogInformation("Adding WorkAvailability: Ngay={Ngay}, ThoiGian={ThoiGian}", ngayEnum, thoiGianEnum);
                _context.WorkAvailabilities.Add(new WorkAvailability
                {
                    UserId = user.UserId,
                    Ngay = ngayEnum,
                    ThoiGian = thoiGianEnum,
                    User = user
                });
            }
        }
        else
        {
            _logger.LogWarning("No WorkAvailabilities to save.");
        }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Lưu thông tin vào session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserRole", user.VaiTro.ToString());

                // Chuyển hướng đến trang chủ
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login","Account");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi đăng ký với SĐT: {SDT}", model.SDT);
                ModelState.AddModelError("", "Đăng ký thất bại, vui lòng thử lại sau.");
                return View("Register", model);
            }
        }

        private string SaveCvFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    
        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            try
            {
                if (string.IsNullOrEmpty(Email))
                {
                    ViewBag.Error = "Vui lòng nhập email";
                    return View();
                }

                if (!Regex.IsMatch(Email, @"^[\w\.-]+@[\w\.-]+\.\w+$"))
                {
                    ViewBag.Error = "Email không hợp lệ";
                    return View();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (user == null)
                {
                    ViewBag.Error = "Email không tồn tại";
                    return View();
                }

                ViewBag.Message = "Một email hướng dẫn đã được gửi tới địa chỉ của bạn";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi quên mật khẩu với Email: {Email}", Email);
                ViewBag.Error = "Đã xảy ra lỗi, vui lòng thử lại sau";
                return View();
            }
        }
    }
}