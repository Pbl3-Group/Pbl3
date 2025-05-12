// File: Controllers/UngTuyenController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.UngTuyen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Để lấy đường dẫn wwwroot
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO; // Để làm việc với file
using System.Security.Claims;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để ứng tuyển
    [Route("[controller]/[action]")] // Route chuẩn: /UngTuyen/NopHoSo
    public class UngTuyenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UngTuyenController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // Để upload file

        public UngTuyenController(ApplicationDbContext context, ILogger<UngTuyenController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            // Trường hợp không lấy được ID (dù đã Authorize, nhưng để an toàn)
            _logger.LogError("Không thể parse User ID từ ClaimsPrincipal.");
            throw new UnauthorizedAccessException("Không thể xác định người dùng.");
        }

        // GET: UngTuyen/NopHoSo/5 (id là TinTuyenDungId)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> NopHoSo(int id)
        {
            var tinTuyenDung = await _context.TinTuyenDungs
                                        .Include(t => t.NguoiDang)
                                            .ThenInclude(nd => nd.HoSoDoanhNghiep) // Vẫn include để lấy tên công ty nếu là doanh nghiệp
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.Id == id && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            if (tinTuyenDung == null)
            {
                TempData["ErrorMessage"] = "Tin tuyển dụng không tồn tại, chưa được duyệt hoặc đã hết hạn.";
                return RedirectToAction("Index", "TimViec"); // Hoặc hiển thị trang lỗi thân thiện hơn
            }

            int currentUserId = GetCurrentUserId();

            // Kiểm tra xem ứng viên đã ứng tuyển tin này chưa
            bool daUngTuyen = await _context.UngTuyens
                                    .AnyAsync(ut => ut.TinTuyenDungId == id && ut.UngVienId == currentUserId);
            if (daUngTuyen)
            {
                TempData["WarningMessage"] = "Bạn đã ứng tuyển vào vị trí này trước đó.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = id });
            }

            var nguoiDungUngVien = await _context.NguoiDungs.FindAsync(currentUserId);
            if (nguoiDungUngVien == null) // Hiếm khi xảy ra nếu đã Authorize
            {
                 TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng của bạn.";
                 return RedirectToAction("Index", "Home"); // Hoặc trang lỗi
            }

            string? tenNhaTuyenDungDisplay = null;
            if (tinTuyenDung.NguoiDang != null)
            {
                if (tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && tinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                {
                    tenNhaTuyenDungDisplay = tinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy;
                }
                else // Mặc định lấy Họ tên nếu là cá nhân hoặc doanh nghiệp không có hồ sơ
                {
                    tenNhaTuyenDungDisplay = tinTuyenDung.NguoiDang.HoTen;
                }
            }

            var viewModel = new NopHoSoViewModel
            {
                TinTuyenDungId = tinTuyenDung.Id,
                TieuDeTinTuyenDung = tinTuyenDung.TieuDe,
                TenNhaTuyenDungHoacCaNhan = tenNhaTuyenDungDisplay ?? "Không có thông tin người đăng",
                LoaiTaiKhoanNguoiDang = tinTuyenDung.NguoiDang?.LoaiTk,
                HoTenUngVien = nguoiDungUngVien.HoTen,
                EmailUngVien = nguoiDungUngVien.Email,
                SdtUngVien = nguoiDungUngVien.Sdt
            };

            return View(viewModel);
        }

        // POST: UngTuyen/NopHoSo/5 (id là TinTuyenDungId)
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopHoSo(int id, NopHoSoViewModel model)
        {
            if (id != model.TinTuyenDungId)
            {
                ModelState.AddModelError("", "ID tin tuyển dụng không khớp với thông tin form.");
            }

            int currentUserId = GetCurrentUserId();
            var tinTuyenDung = await _context.TinTuyenDungs
                                        .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.Id == model.TinTuyenDungId && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            // Luôn gán lại các thông tin này cho model để hiển thị lại trên view nếu có lỗi
            if (tinTuyenDung != null)
            {
                 model.TieuDeTinTuyenDung = tinTuyenDung.TieuDe;
                 if (tinTuyenDung.NguoiDang != null)
                 {
                    model.LoaiTaiKhoanNguoiDang = tinTuyenDung.NguoiDang.LoaiTk;
                    if (tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && tinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                        model.TenNhaTuyenDungHoacCaNhan = tinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy;
                    else model.TenNhaTuyenDungHoacCaNhan = tinTuyenDung.NguoiDang.HoTen;
                 } else model.TenNhaTuyenDungHoacCaNhan = "Không có thông tin người đăng";
            } else {
                ModelState.AddModelError("", "Tin tuyển dụng không hợp lệ, đã hết hạn hoặc không tìm thấy.");
            }

            var nguoiDungUngVien = await _context.NguoiDungs.FindAsync(currentUserId);
            if (nguoiDungUngVien != null)
            {
               model.HoTenUngVien = nguoiDungUngVien.HoTen;
               model.EmailUngVien = nguoiDungUngVien.Email;
               model.SdtUngVien = nguoiDungUngVien.Sdt;
            } else {
                ModelState.AddModelError("", "Không thể xác thực thông tin người dùng ứng tuyển.");
            }


            // Kiểm tra lại xem đã ứng tuyển chưa (tránh double submit sau khi qua GET check)
            bool daUngTuyen = await _context.UngTuyens
                                    .AnyAsync(ut => ut.TinTuyenDungId == model.TinTuyenDungId && ut.UngVienId == currentUserId);
            if (daUngTuyen)
            {
                TempData["WarningMessage"] = "Bạn đã ứng tuyển vào vị trí này rồi.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }

            // Xử lý upload file CV
            string? uniqueFileName = null;
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                // Validate file size (e.g., 5MB)
                if (model.CvFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.CvFile), "Kích thước file CV không được vượt quá 5MB.");
                }

                // Validate file extension
                string[] permittedExtensions = { ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(model.CvFile.FileName)?.ToLowerInvariant(); // Thêm ? để tránh null exception
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    ModelState.AddModelError(nameof(model.CvFile), "Chỉ chấp nhận file CV định dạng PDF, DOC, DOCX.");
                }

                // Chỉ thực hiện upload nếu không có lỗi validation liên quan đến file
                if (ModelState.GetFieldValidationState(nameof(model.CvFile)) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
                {
                    string cvUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "file", "CV");
                    if (!Directory.Exists(cvUploadsFolder))
                    {
                        try
                        {
                             Directory.CreateDirectory(cvUploadsFolder);
                        }
                        catch (Exception ex_dir)
                        {
                            _logger.LogError(ex_dir, "Không thể tạo thư mục lưu CV: {Path}", cvUploadsFolder);
                            ModelState.AddModelError("", "Lỗi hệ thống khi chuẩn bị lưu CV. Vui lòng thử lại sau.");
                            return View(model);
                        }
                    }
                    // Tạo tên file duy nhất để tránh ghi đè
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CvFile.FileName);
                    string filePath = Path.Combine(cvUploadsFolder, uniqueFileName);

                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.CvFile.CopyToAsync(fileStream);
                        }
                        model.UrlCvDaNop = "/file/CV/" + uniqueFileName; // Lưu đường dẫn tương đối để truy cập từ web
                    }
                    catch (Exception ex_upload)
                    {
                        _logger.LogError(ex_upload, "Lỗi khi upload CV cho người dùng {UserId} vào tin {TinId}", currentUserId, model.TinTuyenDungId);
                        ModelState.AddModelError("", "Đã có lỗi xảy ra khi tải lên CV của bạn. Vui lòng thử lại.");
                        // Có thể xóa file nếu upload dở dang (tùy chọn)
                        // if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                }
            }
            // Nếu người dùng không upload file mới, và bạn có logic lấy CV mặc định từ hồ sơ, hãy thêm logic đó ở đây.
            // Hiện tại, nếu không có model.CvFile, UrlCvDaNop sẽ là null.

            // Kiểm tra ModelState tổng thể một lần nữa sau khi xử lý file
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state không hợp lệ khi nộp hồ sơ cho tin {TinID} bởi user {UserID}.", model.TinTuyenDungId, currentUserId);
                return View(model); // Trả về view với các lỗi validation
            }


            var ungTuyen = new UngTuyen
            {
                TinTuyenDungId = model.TinTuyenDungId,
                UngVienId = currentUserId,
                ThuGioiThieu = model.ThuGioiThieu,
                UrlCvDaNop = model.UrlCvDaNop,
                TrangThai = TrangThaiUngTuyen.danop,
                NgayNop = DateTime.UtcNow // Nên sử dụng UTC cho server time
            };

            _context.UngTuyens.Add(ungTuyen);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Người dùng {UserId} đã ứng tuyển thành công vào tin {TinId}. CV Url: {CvUrl}", currentUserId, model.TinTuyenDungId, model.UrlCvDaNop);
                TempData["SuccessMessage"] = "Nộp hồ sơ ứng tuyển thành công!";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
            catch (DbUpdateException ex)
            {
                 _logger.LogError(ex, "Lỗi DbUpdateException khi người dùng {UserId} ứng tuyển vào tin {TinId}", currentUserId, model.TinTuyenDungId);
                // Cố gắng kiểm tra lỗi UNIQUE constraint cụ thể hơn
                if (ex.InnerException != null && (ex.InnerException.Message.Contains("UNIQUE KEY constraint 'uq_TinTuyenDung_UngVien'", StringComparison.OrdinalIgnoreCase) ||
                    ex.InnerException.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) || // Cho MySQL
                    ex.InnerException.Message.Contains("uq_TinTuyenDung_UngVien", StringComparison.OrdinalIgnoreCase))) // Đảm bảo tên này khớp với DB của bạn
                {
                     TempData["WarningMessage"] = "Có vẻ như bạn đã ứng tuyển vào vị trí này rồi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi lưu hồ sơ ứng tuyển. Vui lòng thử lại hoặc liên hệ quản trị viên.";
                }
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi người dùng {UserId} ứng tuyển vào tin {TinId}", currentUserId, model.TinTuyenDungId);
                TempData["ErrorMessage"] = "Đã có lỗi không mong muốn xảy ra trong quá trình ứng tuyển. Vui lòng thử lại sau.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
        }
    }
}