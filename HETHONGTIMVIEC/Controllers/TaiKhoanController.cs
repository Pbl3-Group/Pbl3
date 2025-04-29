// File: Controllers/TaiKhoanController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using HETHONGTIMVIEC.Controllers;
// using HETHONGTIMVIEC.Controllers; // Namespace này có thể không đúng hoặc không cần thiết

namespace HeThongTimViec.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaiKhoanController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IEmailSender _emailSender; // Dịch vụ gửi email của bạn

        public TaiKhoanController(
            ApplicationDbContext context,
            ILogger<TaiKhoanController> logger,
            IHttpContextAccessor httpContextAccessor
            /*, IEmailSender emailSender */ )
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            // _emailSender = emailSender;
        }

        // Hàm trợ giúp tạo SelectList loại tài khoản
        private SelectList GetLoaiTaiKhoanList()
        {
            return new SelectList(Enum.GetValues(typeof(LoaiTaiKhoan))
                                    .Cast<LoaiTaiKhoan>()
                                    .Where(e => e != LoaiTaiKhoan.quantrivien) // Không cho đăng ký làm Admin
                                    .Select(e => new SelectListItem
                                    {
                                        Value = e.ToString(),
                                        Text = e == LoaiTaiKhoan.canhan ? "Cá nhân / Nhà tuyển dụng cá nhân" : "Nhà tuyển dụng Doanh nghiệp" // Làm rõ hơn
                                    }), "Value", "Text");
        }

        // Hàm trợ giúp lấy danh sách Thành Phố
        private async Task PopulateThanhPhoDropdownAsync(object? selectedThanhPho = null)
        {
             ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(),
                                                 "Id", "Ten", selectedThanhPho);
        }

        // Hàm trợ giúp lấy danh sách Quận Huyện
        private async Task PopulateQuanHuyenDropdownAsync(int? thanhPhoId, object? selectedQuanHuyen = null)
        {
             if (thanhPhoId.HasValue && thanhPhoId > 0) // Thêm kiểm tra > 0
             {
                 ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens
                                                                   .Where(qh => qh.ThanhPhoId == thanhPhoId)
                                                                   .OrderBy(qh => qh.Ten).ToListAsync(),
                                                        "Id", "Ten", selectedQuanHuyen);
             } else {
                  ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten"); // Danh sách rỗng
             }
        }

        // Hàm trợ giúp lấy danh sách Giới Tính
        private SelectList GetGioiTinhList() {
            return new SelectList(Enum.GetValues(typeof(GioiTinhNguoiDung))
                                   .Cast<GioiTinhNguoiDung>()
                                   .Select(e => new SelectListItem {
                                       Value = e.ToString(),
                                       Text = e == GioiTinhNguoiDung.nam ? "Nam" : (e == GioiTinhNguoiDung.nu ? "Nữ" : "Khác")
                                   }),
                                  "Value", "Text");
        }

        // GET: /TaiKhoan/DangKy
        [AllowAnonymous]
        public async Task<IActionResult> DangKy(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.LoaiTkList = GetLoaiTaiKhoanList();
            await PopulateThanhPhoDropdownAsync();
            await PopulateQuanHuyenDropdownAsync(null); // Khởi tạo trống
            ViewBag.GioiTinhList = GetGioiTinhList();
            return View(new RegisterViewModel());
        }

        // POST: /TaiKhoan/DangKy
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
             // Load lại Dropdown Lists phòng trường hợp validation fail
            ViewBag.LoaiTkList = GetLoaiTaiKhoanList();
            await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
            await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
            ViewBag.GioiTinhList = GetGioiTinhList();

            // --- Custom Validation ---
            // (Giữ nguyên phần validation bạn đã có)
             if (model.LoaiTkDangKy == LoaiTaiKhoan.doanhnghiep)
            {
                 if (string.IsNullOrWhiteSpace(model.TenCongTy))
                 {
                     ModelState.AddModelError(nameof(model.TenCongTy), "Vui lòng nhập tên công ty.");
                 }
                 if (string.IsNullOrWhiteSpace(model.MaSoThue))
                 {
                     ModelState.AddModelError(nameof(model.MaSoThue), "Vui lòng nhập mã số thuế.");
                 }
                 if (string.IsNullOrWhiteSpace(model.DiaChiDangKyKinhDoanh))
                 {
                     ModelState.AddModelError(nameof(model.DiaChiDangKyKinhDoanh), "Vui lòng nhập địa chỉ đăng ký kinh doanh.");
                 }
                 if (!string.IsNullOrWhiteSpace(model.MaSoThue) && await _context.HoSoDoanhNghieps.AnyAsync(h => h.MaSoThue == model.MaSoThue))
                 {
                      ModelState.AddModelError(nameof(model.MaSoThue), "Mã số thuế này đã được đăng ký.");
                 }
            }

            if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Địa chỉ email này đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(model.Sdt) && await _context.NguoiDungs.AnyAsync(u => u.Sdt == model.Sdt && u.Id != 0)) // Thêm kiểm tra Id != 0 để tránh lỗi khi user chưa tồn tại
            {
                 ModelState.AddModelError(nameof(model.Sdt), "Số điện thoại này đã được sử dụng.");
            }
            // --- Hết Custom Validation ---

            if (ModelState.IsValid)
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                TrangThaiTaiKhoan initialStatus = (model.LoaiTkDangKy == LoaiTaiKhoan.doanhnghiep)
                                                ? TrangThaiTaiKhoan.choxacminh // DN cần chờ duyệt
                                                : TrangThaiTaiKhoan.kichhoat; // Cá nhân kích hoạt ngay

                var user = new NguoiDung
                {
                    Email = model.Email,
                    MatKhauHash = passwordHash,
                    HoTen = model.HoTen,
                    Sdt = model.Sdt,
                    LoaiTk = model.LoaiTkDangKy,
                    GioiTinh = model.GioiTinh,
                    NgaySinh = model.NgaySinh,
                    DiaChiChiTiet = model.DiaChiChiTiet,
                    QuanHuyenId = model.QuanHuyenId,
                    ThanhPhoId = model.ThanhPhoId,
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow,
                    TrangThaiTk = initialStatus,
                    LanDangNhapCuoi = null,
                    UrlAvatar = null // Mặc định không có avatar
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.NguoiDungs.Add(user);
                    await _context.SaveChangesAsync(); // Lưu NguoiDung trước để lấy Id
                    _logger.LogInformation("Tạo người dùng mới ID {UserId}: {Email}", user.Id, user.Email);

                    // Tạo hồ sơ tương ứng
                    if (user.LoaiTk == LoaiTaiKhoan.doanhnghiep)
                    {
                        var hoSoDN = new HoSoDoanhNghiep
                        {
                            NguoiDungId = user.Id,
                            TenCongTy = model.TenCongTy!,
                            MaSoThue = model.MaSoThue!,
                            UrlWebsite = model.UrlWebsite,
                            DiaChiDangKy = model.DiaChiDangKyKinhDoanh,
                            DaXacMinh = false, // Chờ Admin xác minh
                            AdminXacMinhId = null,
                            NgayXacMinh = null
                        };
                        _context.HoSoDoanhNghieps.Add(hoSoDN);
                        _logger.LogInformation("Tạo hồ sơ Doanh nghiệp chờ duyệt cho User ID {UserId}", user.Id);
                    }
                    else // LoaiTk = canhan
                    {
                        var hoSoUV = new HoSoUngVien
                        {
                            NguoiDungId = user.Id,
                            // Set trạng thái tìm việc mặc định hoặc các thông tin ban đầu khác
                            TrangThaiTimViec = TrangThaiTimViec.dangtimtichcuc
                        };
                        _context.HoSoUngViens.Add(hoSoUV);
                        _logger.LogInformation("Tạo hồ sơ Ứng viên cho User ID {UserId}", user.Id);
                    }
                    await _context.SaveChangesAsync(); // Lưu hồ sơ

                    await transaction.CommitAsync(); // Hoàn tất transaction
                    _logger.LogInformation("Transaction đăng ký cho User ID {UserId} thành công.", user.Id);

                    // Chuyển hướng sau khi thành công
                    if (user.LoaiTk == LoaiTaiKhoan.doanhnghiep)
                    {
                         // Chuyển đến trang thông báo chờ duyệt
                        return RedirectToAction(nameof(DangKyThanhCongChoDuyet));
                    }
                    else // canhan
                    {
                         // Chuyển đến trang đăng nhập với thông báo thành công
                         TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                         return RedirectToAction(nameof(DangNhap));
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi nghiêm trọng khi đăng ký và tạo hồ sơ cho {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn trong quá trình đăng ký, vui lòng thử lại sau hoặc liên hệ quản trị viên.");
                }
            }

            // Nếu ModelState không hợp lệ hoặc có lỗi transaction, hiển thị lại form với lỗi
            _logger.LogWarning("Đăng ký không thành công cho {Email}. ModelState Invalid: {IsInvalid}", model.Email, !ModelState.IsValid);
            return View(model);
        }

        // GET: /TaiKhoan/DangKyThanhCongChoDuyet
        [AllowAnonymous]
        public IActionResult DangKyThanhCongChoDuyet()
        {
             // View này chỉ hiển thị thông báo cho NTD Doanh nghiệp rằng tài khoản đang chờ duyệt
            return View();
        }

        // GET: /TaiKhoan/DangNhap
        [AllowAnonymous]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Xóa session cũ (nếu có) để đảm bảo trạng thái sạch trước khi đăng nhập mới
             var session = _httpContextAccessor.HttpContext?.Session;
             if(session != null) {
                 session.Remove("DangLaNTD");
                 session.Remove("TenCongTy");
             }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(LoginViewModel model, string? returnUrl = null)
        {
            string appRoot = Url.Content("~/") ?? "/";
            returnUrl ??= appRoot;
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs
                                    .Include(u => u.HoSoDoanhNghiep) // Include để lấy tên cty nếu là DN
                                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    // Kiểm tra trạng thái tài khoản trước khi kiểm tra mật khẩu
                     if (user.TrangThaiTk == TrangThaiTaiKhoan.choxacminh)
                     {
                         _logger.LogWarning("Login attempt for unverified account: {Email}", model.Email);
                         ModelState.AddModelError(string.Empty, "Tài khoản của bạn đang chờ phê duyệt.");
                         return View(model);
                     }
                     if (user.TrangThaiTk == TrangThaiTaiKhoan.tamdung || user.TrangThaiTk == TrangThaiTaiKhoan.bidinhchi)
                     {
                          _logger.LogWarning("Login attempt for suspended/banned account: {Email}", model.Email);
                         ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị tạm dừng hoặc đình chỉ.");
                         return View(model);
                     }

                    // Chỉ kiểm tra mật khẩu nếu tài khoản hợp lệ
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.MatKhauHash);

                    if (isPasswordValid)
                    {
                        _logger.LogInformation("User {Email} logged in successfully.", user.Email);

                        // Tạo claims cho user
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.HoTen), // Tên hiển thị
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.LoaiTk.ToString()) // Vai trò chính
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe, // Lưu cookie lâu dài nếu chọn RememberMe
                            IssuedUtc = DateTimeOffset.UtcNow // Thời gian phát hành cookie
                        };

                        if(model.RememberMe) {
                            // Đặt thời gian hết hạn cụ thể nếu muốn cookie tồn tại lâu hơn session
                            authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
                        }

                        // Đăng nhập người dùng, tạo cookie xác thực
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // Cập nhật lần đăng nhập cuối
                        user.LanDangNhapCuoi = DateTime.UtcNow;
                        try
                        {
                             _context.NguoiDungs.Update(user);
                             await _context.SaveChangesAsync();
                        }
                        catch(DbUpdateConcurrencyException ex) {
                             _logger.LogWarning(ex, "Concurrency error updating LastLoginTime for user {UserId}. Ignoring.", user.Id);
                             // Có thể bỏ qua lỗi này vì không quá nghiêm trọng
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update LastLoginTime for user {UserId}", user.Id);
                            // Ghi log lỗi nhưng vẫn tiếp tục đăng nhập
                        }

                        // Xử lý Session sau khi đăng nhập thành công
                        var session = _httpContextAccessor.HttpContext?.Session;
                        if (session != null)
                        {
                             session.Clear(); // Xóa hết session cũ trước khi đặt giá trị mới
                             if (user.LoaiTk == LoaiTaiKhoan.canhan)
                             {
                                 session.SetInt32("DangLaNTD", 0); // Mặc định là Ứng viên
                                 _logger.LogDebug("Session 'DangLaNTD' set to 0 for user {UserId}", user.Id);
                             }
                             else if (user.LoaiTk == LoaiTaiKhoan.doanhnghiep && user.HoSoDoanhNghiep != null)
                             {
                                  session.SetString("TenCongTy", user.HoSoDoanhNghiep.TenCongTy);
                                   _logger.LogDebug("Session 'TenCongTy' set for user {UserId}", user.Id);
                                  // Không cần đặt DangLaNTD cho Doanh nghiệp
                             }
                        } else {
                            _logger.LogWarning("Session is not available for user {UserId} on login.", user.Id);
                        }

                        // Xác định trang chuyển hướng dựa trên vai trò
                        string redirectTarget;
                        switch (user.LoaiTk)
                        {
                            case LoaiTaiKhoan.quantrivien:
                                 _logger.LogInformation("Redirecting Admin user {UserId} to Admin dashboard.", user.Id);
                                 redirectTarget = Url.Action("Index", "Admin")!;
                                break;
                            case LoaiTaiKhoan.doanhnghiep:
                                _logger.LogInformation("Redirecting Employer user {UserId} to Employer dashboard.", user.Id);
                                redirectTarget = Url.Action("Index", "NhaTuyenDung")!;
                                break;
                            case LoaiTaiKhoan.canhan:
                                // Chỉ chuyển hướng đến returnUrl nếu nó hợp lệ, local, VÀ KHÔNG PHẢI trang chủ.
                                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != appRoot)
                                {
                                    _logger.LogInformation("Redirecting logged in candidate {UserId} to specific returnUrl: {ReturnUrl}", user.Id, returnUrl);
                                    redirectTarget = returnUrl;
                                }
                                else
                                {
                                     _logger.LogInformation("Redirecting logged in candidate {UserId} to default Candidate dashboard. Original returnUrl was: '{OriginalReturnUrl}'", user.Id, ViewData["ReturnUrl"]);
                                    redirectTarget = Url.Action("Index", "UngVien")!; // Mặc định vào trang Ứng viên
                                }
                                break;
                            default:
                                _logger.LogWarning("Unknown user type {UserType} for user {UserId}. Redirecting to home.", user.LoaiTk, user.Id);
                                redirectTarget = appRoot; // Trang chủ mặc định
                                break;
                        }
                         _logger.LogInformation("Final redirect target for user {UserId}: {RedirectTarget}", user.Id, redirectTarget);
                         return LocalRedirect(redirectTarget); // An toàn hơn Redirect
                    }
                    else
                    {
                         _logger.LogWarning("Invalid password attempt for {Email}", model.Email);
                         ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
                    }
                }
                else
                {
                    _logger.LogWarning("Login attempt for non-existent email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
                }
            }
            else
            {
                 _logger.LogWarning("Login failed due to invalid ModelState.");
            }

            // Nếu ModelState không hợp lệ hoặc đăng nhập thất bại, hiển thị lại form
            return View(model);
        }

        // POST: /TaiKhoan/DangXuat
        [HttpPost]
        [Authorize] // Chỉ người dùng đã đăng nhập mới có thể đăng xuất
        [ValidateAntiForgeryToken] // Chống CSRF
        public async Task<IActionResult> DangXuat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Xóa cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa Session
            _httpContextAccessor.HttpContext?.Session.Clear();

            _logger.LogInformation("User {Email} (ID: {UserId}) logged out successfully.", userEmail, userId);

            // Chuyển hướng về trang chủ
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /TaiKhoan/QuenMatKhau
        [AllowAnonymous]
        public IActionResult QuenMatKhau() { return View(); }

        // POST: /TaiKhoan/QuenMatKhau
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Chỉ cho phép lấy lại MK cho tài khoản đang kích hoạt
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email && u.TrangThaiTk == TrangThaiTaiKhoan.kichhoat);
                if (user != null)
                {
                    // === PHẦN CẦN CẢI THIỆN BẢO MẬT ===
                    // Tạm thời dùng Guid đơn giản cho mục đích demo/phát triển
                    var resetToken = Guid.NewGuid().ToString("N").Substring(0, 20); // Token dài hơn một chút
                    // TODO: Triển khai cơ chế token an toàn hơn:
                    // 1. Tạo token sử dụng ASP.NET Core Identity UserManager (nếu dùng Identity)
                    // 2. Hoặc tạo token có thời gian hết hạn, lưu hash của token vào DB cùng UserID và ExpiresAt.
                    _logger.LogWarning("Generated INSECURE password reset token {Token} for user {Email}", resetToken, user.Email);

                    var callbackUrl = Url.Action("DatLaiMatKhau", "TaiKhoan", new { email = user.Email, code = resetToken }, protocol: Request.Scheme);
                    _logger.LogInformation("Password reset link for {Email}: {Url}", model.Email, callbackUrl);

                    // --- TODO: GỬI EMAIL CHO NGƯỜI DÙNG ---
                    // Cần triển khai dịch vụ gửi email (ví dụ dùng SendGrid, MailKit...)
                    // string emailBody = $"Vui lòng đặt lại mật khẩu bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.";
                    // await _emailSender.SendEmailAsync(model.Email, "Đặt lại mật khẩu", emailBody);
                    System.Diagnostics.Debug.WriteLine($"DEBUG ONLY - Password Reset Link for {model.Email}: {callbackUrl}");
                    // --- KẾT THÚC GỬI EMAIL ---

                     TempData["InfoMessage"] = "Nếu email của bạn tồn tại trong hệ thống và đã kích hoạt, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                     return RedirectToAction(nameof(QuenMatKhauXacNhan)); // Luôn chuyển hướng đến trang xác nhận
                }
                else {
                     _logger.LogWarning("Password reset requested for non-existent or inactive email: {Email}", model.Email);
                     // Không báo lỗi cụ thể để tránh tiết lộ email nào tồn tại/không tồn tại
                      TempData["InfoMessage"] = "Nếu email của bạn tồn tại trong hệ thống và đã kích hoạt, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                     return RedirectToAction(nameof(QuenMatKhauXacNhan));
                }
            }
            return View(model); // Hiển thị lại form nếu model không hợp lệ
        }

        // GET: /TaiKhoan/QuenMatKhauXacNhan
        [AllowAnonymous]
        public IActionResult QuenMatKhauXacNhan()
        {
             // View này chỉ hiển thị thông báo chung chung
            return View();
        }

        // GET: /TaiKhoan/DatLaiMatKhau
        [AllowAnonymous]
        public IActionResult DatLaiMatKhau(string? code = null, string? email = null)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
            {
                // Token hoặc email không hợp lệ -> Báo lỗi chung chung
                _logger.LogWarning("Invalid password reset link accessed. Code or Email missing.");
                TempData["ErrorMessage"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(DangNhap));
            }

            // === PHẦN CẦN CẢI THIỆN BẢO MẬT ===
            // TODO: Xác thực token `code` dựa trên `email`.
            // Ví dụ: Tìm trong bảng lưu token xem có token `code` cho `email` này không và còn hạn không.
            // Nếu không hợp lệ -> Redirect về DangNhap với TempData báo lỗi.
            _logger.LogWarning("Attempting reset with INSECURE token validation. Email: {Email}, Token: {Code}", email, code);

            var model = new ResetPasswordViewModel { Token = code, Email = email };
            return View(model);
        }

        // POST: /TaiKhoan/DatLaiMatKhau
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) {
                 _logger.LogWarning("ResetPassword POST failed due to invalid ModelState for Email: {Email}", model.Email);
                return View(model); // Hiển thị lại form với lỗi validation
            }

            // === PHẦN CẦN CẢI THIỆN BẢO MẬT ===
             // TODO: Xác thực lại token `model.Token` cho `model.Email` trước khi thực hiện đổi mật khẩu.
             // Nếu token không hợp lệ hoặc đã hết hạn:
             // ModelState.AddModelError(string.Empty, "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
             // return View(model);
             _logger.LogWarning("Proceeding with reset with INSECURE token validation. Email: {Email}, Token: {Token}", model.Email, model.Token);

            // Tìm user hợp lệ (đang kích hoạt)
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email && u.TrangThaiTk == TrangThaiTaiKhoan.kichhoat);
            if (user != null)
            {
                // Hash mật khẩu mới
                user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.NgayCapNhat = DateTime.UtcNow;
                // TODO: Vô hiệu hóa token đã sử dụng trong CSDL (đánh dấu là đã dùng hoặc xóa).

                try
                {
                    _context.NguoiDungs.Update(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Password reset successfully for {Email}", model.Email);
                    // TODO: Gửi email thông báo đổi MK thành công (tùy chọn)
                    TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
                    return RedirectToAction(nameof(DangNhap)); // Chuyển về trang đăng nhập
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving new password for {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu mật khẩu mới. Vui lòng thử lại.");
                }
            }
            else
            {
                 // Trường hợp user không tồn tại hoặc không kích hoạt trong lúc POST (có thể link cũ)
                 _logger.LogWarning("User not found or inactive during password reset POST for {Email}", model.Email);
                 // Không báo lỗi cụ thể để bảo mật. Chỉ chuyển hướng.
                 TempData["ErrorMessage"] = "Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                 return RedirectToAction(nameof(DangNhap));
            }

            // Nếu có lỗi xảy ra khi lưu DB
            return View(model);
        }

        // GET: /TaiKhoan/DatLaiMatKhauXacNhan - Có thể không cần action này nữa nếu chuyển thẳng về Đăng nhập
        // [AllowAnonymous]
        // public IActionResult DatLaiMatKhauXacNhan() { return View(); }

        // GET: /TaiKhoan/Lockout
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            // View hiển thị khi tài khoản bị khóa tạm thời (nếu có triển khai cơ chế lockout)
            return View();
        }

        // GET: /TaiKhoan/AccessDenied
        [AllowAnonymous] // Hoặc [Authorize] tùy theo muốn ai thấy trang này
        public IActionResult AccessDenied(string? returnUrl = null)
        {
             _logger.LogWarning("Access denied for user {UserName} attempting to access {ReturnUrl}", User.Identity?.Name ?? "Anonymous", returnUrl);
             ViewData["ReturnUrl"] = returnUrl; // Có thể hiển thị link quay lại nếu cần
            // View thông báo người dùng không có quyền truy cập
            return View();
        }

        // POST: /TaiKhoan/ChuyenDoiVaiTro
        [HttpPost]
        [Authorize(Roles = nameof(LoaiTaiKhoan.canhan))] // Chỉ user 'canhan'
        [ValidateAntiForgeryToken]
        public IActionResult ChuyenDoiVaiTro(string? returnUrl = null)
        {
             var session = _httpContextAccessor.HttpContext?.Session;
             if (session == null)
             {
                 _logger.LogError("Session is unavailable for role switch for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));
                 TempData["ErrorMessage"] = "Phiên làm việc không hợp lệ, vui lòng đăng nhập lại.";
                 return RedirectToAction(nameof(DangNhap)); // Chuyển về đăng nhập nếu mất session
             }

            // Lấy trạng thái xem HIỆN TẠI từ session
            bool dangLaNTD = session.GetInt32("DangLaNTD") == 1;

            // Tính toán trạng thái MỚI và cập nhật session
            bool seLaNTD = !dangLaNTD;
            session.SetInt32("DangLaNTD", seLaNTD ? 1 : 0);

            _logger.LogInformation("User {UserId} switched role view. IsEmployerNow: {IsEmployer}", User.FindFirstValue(ClaimTypes.NameIdentifier), seLaNTD);

            // Xác định trang đích mặc định dựa trên vai trò MỚI đã chuyển sang
            string defaultRedirectTarget;
            if (seLaNTD) // Nếu vừa chuyển SANG vai trò NTD Cá nhân
            {
                // Phải đảm bảo có TuyenDungCaNhanController và Action Index
                defaultRedirectTarget = Url.Action("Index", "TuyenDungCaNhan")!;
                _logger.LogDebug("Default redirect after switching TO Employer view: {Target}", defaultRedirectTarget);
                if (string.IsNullOrEmpty(defaultRedirectTarget)) {
                     _logger.LogError("Cannot generate URL for TuyenDungCaNhan/Index. Check controller/action names.");
                     // Fallback về trang chủ nếu không tạo được URL
                     defaultRedirectTarget = Url.Content("~/") ?? "/";
                 }
            }
            else // Nếu vừa chuyển SANG vai trò Ứng viên
            {
                defaultRedirectTarget = Url.Action("Index", "UngVien")!;
                _logger.LogDebug("Default redirect after switching TO Candidate view: {Target}", defaultRedirectTarget);
                 if (string.IsNullOrEmpty(defaultRedirectTarget)) {
                     _logger.LogError("Cannot generate URL for UngVien/Index. Check controller/action names.");
                     // Fallback về trang chủ nếu không tạo được URL
                     defaultRedirectTarget = Url.Content("~/") ?? "/";
                 }
            }

            // Ưu tiên quay lại trang trước đó nếu an toàn và hợp lý
            // Tránh quay lại trang dashboard của vai trò cũ nếu returnUrl là nó.
            string? targetUrlToRedirect = null;
             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
             {
                 // Kiểm tra xem returnUrl có phải là trang Index của vai trò cũ không
                 string oldRoleDashboardUrl = dangLaNTD ? (Url.Action("Index", "TuyenDungCaNhan") ?? "") : (Url.Action("Index", "UngVien") ?? "");
                 if (!string.IsNullOrEmpty(oldRoleDashboardUrl) && returnUrl.Equals(oldRoleDashboardUrl, StringComparison.OrdinalIgnoreCase)) {
                      _logger.LogDebug("Ignoring returnUrl because it points to the old role's dashboard: {ReturnUrl}", returnUrl);
                 } else {
                    targetUrlToRedirect = returnUrl;
                     _logger.LogDebug("Redirecting after role switch to provided returnUrl: {ReturnUrl}", returnUrl);
                 }
             }

            // Nếu không có returnUrl hợp lệ hoặc returnUrl bị bỏ qua, dùng default target
             targetUrlToRedirect ??= defaultRedirectTarget;

             return LocalRedirect(targetUrlToRedirect);
        }

        // GET: /TaiKhoan/CaiDat - Trang cài đặt thông tin chung
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> CaiDat()
        {
             var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (!int.TryParse(userIdString, out var userId))
             {
                 _logger.LogWarning("Could not parse User ID from claims in CaiDat GET.");
                 return Unauthorized();
             }

             // Lấy thông tin người dùng hiện tại để hiển thị trên form
             var user = await _context.NguoiDungs
                                    .Include(u => u.ThanhPho)
                                    .Include(u => u.QuanHuyen)
                                    .FirstOrDefaultAsync(u => u.Id == userId);

             if (user == null)
             {
                  _logger.LogError("User with ID {UserId} not found in DB for CaiDat GET.", userId);
                 return NotFound("Không tìm thấy tài khoản của bạn.");
             }

             // Chuẩn bị dropdownlists cho View
             await PopulateThanhPhoDropdownAsync(user.ThanhPhoId);
             await PopulateQuanHuyenDropdownAsync(user.ThanhPhoId, user.QuanHuyenId);
             ViewBag.GioiTinhList = GetGioiTinhList(); // Đã sửa ở trên để trả về SelectList

             // Truyền model NguoiDung vào View
             return View(user);
        }

        // POST: /TaiKhoan/CaiDat - Xử lý cập nhật thông tin chung
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        // Chỉ Bind các thuộc tính người dùng được phép sửa đổi từ form
        public async Task<IActionResult> CaiDat([Bind("Id,HoTen,Sdt,GioiTinh,NgaySinh,DiaChiChiTiet,QuanHuyenId,ThanhPhoId")] NguoiDung updatedUserFormData)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // Đã Authorize nên sẽ có ID

            // Ngăn chặn sửa thông tin của người khác qua ID giả mạo trong form
            if (updatedUserFormData.Id != currentUserId)
            {
                _logger.LogWarning("User {ActualUserId} attempted POST settings for a different ID {TargetUserId}", currentUserId, updatedUserFormData.Id);
                return Forbid("Bạn không có quyền thay đổi thông tin của tài khoản này.");
            }

            // Lấy đối tượng User gốc từ DB để cập nhật, không tạo mới
            var userInDb = await _context.NguoiDungs.FindAsync(currentUserId);
            if (userInDb == null)
            {
                _logger.LogError("User with ID {UserId} not found in DB during CaiDat POST.", currentUserId);
                return NotFound("Tài khoản không tồn tại.");
            }

             // Kiểm tra SĐT Unique (nếu thay đổi và có giá trị)
             if (!string.IsNullOrEmpty(updatedUserFormData.Sdt) && updatedUserFormData.Sdt != userInDb.Sdt) {
                 if (await _context.NguoiDungs.AnyAsync(u => u.Sdt == updatedUserFormData.Sdt && u.Id != currentUserId)) {
                     ModelState.AddModelError("Sdt", "Số điện thoại này đã được sử dụng bởi tài khoản khác.");
                 }
             }

             // Chỉ kiểm tra ModelState cho các thuộc tính đã Bind ở trên
            if (ModelState.IsValid)
            {
                 // Cập nhật các thuộc tính của userInDb từ dữ liệu form hợp lệ
                 userInDb.HoTen = updatedUserFormData.HoTen;
                 userInDb.Sdt = string.IsNullOrWhiteSpace(updatedUserFormData.Sdt) ? null : updatedUserFormData.Sdt; // Cho phép SĐT null/rỗng
                 userInDb.GioiTinh = updatedUserFormData.GioiTinh;
                 userInDb.NgaySinh = updatedUserFormData.NgaySinh;
                 userInDb.DiaChiChiTiet = updatedUserFormData.DiaChiChiTiet;
                 userInDb.QuanHuyenId = updatedUserFormData.QuanHuyenId;
                 userInDb.ThanhPhoId = updatedUserFormData.ThanhPhoId;
                 userInDb.NgayCapNhat = DateTime.UtcNow; // Luôn cập nhật ngày giờ

                 try
                 {
                     // Không cần _context.Update(userInDb) vì userInDb được theo dõi bởi DbContext
                     await _context.SaveChangesAsync();
                     TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                     _logger.LogInformation("User {UserId} updated settings successfully.", currentUserId);

                     // Cập nhật Claim Name nếu Họ Tên thay đổi để hiển thị đúng trên Layout ngay lập tức
                     if (User.FindFirstValue(ClaimTypes.Name) != userInDb.HoTen)
                     {
                        await RefreshSignIn(currentUserId); // Gọi hàm cập nhật cookie
                     }

                     return RedirectToAction(nameof(CaiDat)); // Quay lại trang cài đặt với thông báo thành công
                 }
                 catch (DbUpdateConcurrencyException ex) {
                      // Xử lý lỗi nếu có người khác sửa cùng lúc
                      _logger.LogWarning(ex, "Concurrency error updating user {UserId} settings", currentUserId);
                      ModelState.AddModelError("", "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang và thử lại.");
                      // Load lại giá trị mới nhất từ DB để hiển thị
                      await ex.Entries.Single().ReloadAsync();
                      // Cập nhật lại model trả về View với giá trị mới từ DB
                       var dbValues = (NguoiDung)ex.Entries.Single().Entity;
                        // Truyền lại dbValues vào view (hoặc cập nhật các thuộc tính của updatedUserFormData)
                        userInDb = dbValues; // Gán lại để View hiển thị đúng
                 }
                 catch (Exception ex) // Bắt lỗi chung khác
                 {
                     _logger.LogError(ex, "Error updating user {UserId} settings", currentUserId);
                     ModelState.AddModelError("", "Không thể lưu thay đổi. Đã có lỗi xảy ra, vui lòng thử lại.");
                 }
            } else {
                 // Nếu ModelState không hợp lệ ngay từ đầu
                 _logger.LogWarning("Settings update failed for user {UserId} due to invalid ModelState.", currentUserId);
            }

            // Nếu ModelState không hợp lệ hoặc có lỗi DB, hiển thị lại form với lỗi
            // Cần load lại Dropdowns và gán lại các giá trị chưa Bind (như Email)
            await PopulateThanhPhoDropdownAsync(userInDb.ThanhPhoId); // Dùng giá trị từ DB (có thể đã thay đổi nếu lỗi concurrency)
            await PopulateQuanHuyenDropdownAsync(userInDb.ThanhPhoId, userInDb.QuanHuyenId);
            ViewBag.GioiTinhList = GetGioiTinhList();
            // Truyền userInDb vào View để hiển thị đúng dữ liệu hiện tại và các lỗi ModelState
            return View(userInDb);
        }

         // API Endpoint để lấy Quận/Huyện theo Thành phố (dùng cho AJAX)
        [AllowAnonymous] // Cho phép truy cập công khai để load dropdown
        [HttpGet("api/diachi/quanhuyen/{thanhPhoId}")] // Định tuyến rõ ràng cho API
        public async Task<IActionResult> GetQuanHuyenByThanhPho(int thanhPhoId)
        {
             if (thanhPhoId <= 0) {
                 _logger.LogInformation("API GetQuanHuyenByThanhPho called with invalid thanhPhoId: {ThanhPhoId}", thanhPhoId);
                 return BadRequest("ID Thành phố không hợp lệ.");
             }

            _logger.LogDebug("API GetQuanHuyenByThanhPho called for ThanhPhoId: {ThanhPhoId}", thanhPhoId);
             var quanHuyens = await _context.QuanHuyens
                                        .AsNoTracking() // Không cần theo dõi thay đổi cho API chỉ đọc
                                        .Where(qh => qh.ThanhPhoId == thanhPhoId)
                                        .OrderBy(qh => qh.Ten)
                                        .Select(qh => new { qh.Id, qh.Ten }) // Chỉ lấy Id và Ten
                                        .ToListAsync();

            _logger.LogDebug("Found {Count} QuanHuyen for ThanhPhoId: {ThanhPhoId}", quanHuyens.Count, thanhPhoId);
             return Ok(quanHuyens); // Trả về JSON thành công
        }


        // --- Hàm nội bộ để cập nhật cookie sau khi đổi thông tin ---
        private async Task RefreshSignIn(int userId)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return; // Không tìm thấy user thì bỏ qua

            var principal = await _httpContextAccessor.HttpContext!.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (principal?.Principal == null || !principal.Succeeded) {
                 _logger.LogWarning("Could not get current principal to refresh claims for user {UserId}", userId);
                 return;
             }

             // Tạo lại claims mới với thông tin đã cập nhật
             var newClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen), // Cập nhật Họ Tên
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.LoaiTk.ToString())
                 // Thêm các claim khác nếu có
            };
            var newIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            // Đăng nhập lại với thông tin mới (ghi đè cookie cũ)
            // Sử dụng lại properties cũ để giữ cài đặt 'Remember Me'
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, principal.Properties);
             _logger.LogDebug("Refreshed sign-in cookie for user {UserId} after profile update.", userId);
        }

    }
}