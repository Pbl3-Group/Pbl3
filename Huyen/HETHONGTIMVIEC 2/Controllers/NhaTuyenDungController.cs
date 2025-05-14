// File: Controllers/NhaTuyenDungController.cs (Hoặc Areas/NhaTuyenDung/Controllers/NhaTuyenDungController.cs)
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Thêm cho SelectList
using Microsoft.AspNetCore.Hosting; // Thêm để lấy đường dẫn wwwroot
using System.IO; // Thêm để thao tác file
using System; // Thêm cho Guid
using System.Linq; // Thêm cho Linq methods như OrderBy, Where, Select

namespace HeThongTimViec.Controllers // Hoặc namespace phù hợp nếu dùng Areas
{
    // [Area("NhaTuyenDung")] // Bỏ comment nếu bạn dùng Areas
    [Authorize(Roles = nameof(LoaiTaiKhoan.doanhnghiep))]
    public class NhaTuyenDungController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NhaTuyenDungController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // Inject để lấy đường dẫn lưu file

        public NhaTuyenDungController(
            ApplicationDbContext context,
            ILogger<NhaTuyenDungController> logger,
            IWebHostEnvironment webHostEnvironment) // Inject IWebHostEnvironment
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; // Lưu lại
        }

        // --- Hàm Helper để lấy Dropdown Địa chỉ ---
        private async Task PopulateThanhPhoDropdownAsync(object? selectedThanhPho = null)
        {
            ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos
                                                             .AsNoTracking()
                                                             .OrderBy(tp => tp.Ten)
                                                             .ToListAsync(),
                                                 "Id", "Ten", selectedThanhPho);
        }

        private async Task PopulateQuanHuyenDropdownAsync(int? thanhPhoId, object? selectedQuanHuyen = null)
        {
            if (thanhPhoId.HasValue && thanhPhoId > 0)
            {
                ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens
                                                                   .AsNoTracking()
                                                                   .Where(qh => qh.ThanhPhoId == thanhPhoId)
                                                                   .OrderBy(qh => qh.Ten)
                                                                   .ToListAsync(),
                                                        "Id", "Ten", selectedQuanHuyen);
            }
            else
            {
                ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
            }
        }

        // GET: /NhaTuyenDung/HoSo
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("Không thể lấy User ID từ claims khi xem hồ sơ công ty.");
                return Challenge(); // Yêu cầu đăng nhập lại hoặc trả về 401/403
            }

            _logger.LogInformation("User ID {UserId} đang xem hồ sơ công ty của họ.", userId);

            // Lấy hồ sơ doanh nghiệp kèm thông tin người dùng liên quan và địa chỉ
            var hoSo = await _context.HoSoDoanhNghieps
                .Include(h => h.NguoiDung) // Lấy thông tin người dùng (email, sdt, địa chỉ liên hệ)
                    .ThenInclude(nd => nd.ThanhPho) // Lấy tên Thành phố từ người dùng
                .Include(h => h.NguoiDung)
                    .ThenInclude(nd => nd.QuanHuyen) // Lấy tên Quận/Huyện từ người dùng
                .Include(h => h.AdminXacMinh) // Lấy thông tin người admin đã xác minh (nếu có)
                .AsNoTracking() // Chỉ xem, không cần theo dõi thay đổi
                .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

            if (hoSo == null)
            {
                _logger.LogWarning("Không tìm thấy Hồ sơ doanh nghiệp cho User ID: {UserId}.", userId);
                // Thông báo lỗi thân thiện cho người dùng
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ công ty của bạn. Vui lòng liên hệ hỗ trợ nếu bạn cho rằng đây là lỗi.";
                // Chuyển hướng về Dashboard của Nhà tuyển dụng hoặc trang chủ
                // return RedirectToAction("Index", "Dashboard", new { area = "NhaTuyenDung" }); // Nếu dùng Area
                return RedirectToAction("Index", "Dashboard"); // Nếu không dùng Area
            }

            // Tạo ViewModel từ dữ liệu lấy được
            var viewModel = new HoSoDoanhNghiepViewModel
            {
                TenCongTy = hoSo.TenCongTy,
                MaSoThue = hoSo.MaSoThue,
                UrlLogo = hoSo.UrlLogo,
                UrlWebsite = hoSo.UrlWebsite,
                MoTa = hoSo.MoTa,
                DiaChiDangKy = hoSo.DiaChiDangKy,
                QuyMoCongTy = hoSo.QuyMoCongTy,
                DaXacMinh = hoSo.DaXacMinh,
                NgayXacMinh = hoSo.NgayXacMinh,
                // Lấy tên Admin nếu có và không null
                TenAdminXacMinh = hoSo.AdminXacMinh?.HoTen,

                // Lấy thông tin từ NguoiDung liên kết (chắc chắn không null vì hoSo tồn tại)
                EmailLienHe = hoSo.NguoiDung.Email,
                SoDienThoaiLienHe = hoSo.NguoiDung.Sdt,
                DiaChiChiTietNguoiDung = hoSo.NguoiDung.DiaChiChiTiet,
                // Lấy tên Quận/Huyện, Thành phố nếu có và không null
                TenQuanHuyen = hoSo.NguoiDung.QuanHuyen?.Ten,
                TenThanhPho = hoSo.NguoiDung.ThanhPho?.Ten
            };

            // Truyền tên công ty cho Layout (nếu layout cần)
            ViewBag.TenCongTy = hoSo.TenCongTy;

            // Truyền ViewModel vào View để hiển thị
            return View(viewModel);
        }


        // GET: /NhaTuyenDung/ChinhSuaHoSo
        [HttpGet]
        public async Task<IActionResult> ChinhSuaHoSo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                 _logger.LogWarning("Không thể lấy User ID từ claims khi vào trang chỉnh sửa hồ sơ.");
                return Challenge();
            }

            // Lấy cả HoSoDoanhNghiep và NguoiDung liên quan để điền form
             var hoSoDb = await _context.HoSoDoanhNghieps
                .Include(h => h.NguoiDung) // Quan trọng: Lấy cả NguoiDung
                .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

            if (hoSoDb == null || hoSoDb.NguoiDung == null)
            {
                _logger.LogWarning("Không tìm thấy Hồ sơ DN hoặc Người dùng liên kết cho User ID: {UserId} khi GET ChinhSuaHoSo.", userId);
                TempData["ErrorMessage"] = "Không tìm thấy thông tin để chỉnh sửa.";
                return RedirectToAction(nameof(HoSo)); // Chuyển về trang xem
            }

             // Map dữ liệu từ DB sang ViewModel
             var viewModel = new HoSoDoanhNghiepEditViewModel
            {
                 NguoiDungId = hoSoDb.NguoiDungId,
                 TenCongTy = hoSoDb.TenCongTy,
                 MaSoThue = hoSoDb.MaSoThue,
                 CurrentUrlLogo = hoSoDb.UrlLogo, // Lưu URL logo hiện tại để hiển thị
                 UrlWebsite = hoSoDb.UrlWebsite,
                 MoTa = hoSoDb.MoTa,
                 DiaChiDangKy = hoSoDb.DiaChiDangKy,
                 QuyMoCongTy = hoSoDb.QuyMoCongTy,
                 // Lấy từ NguoiDung
                 SoDienThoaiLienHe = hoSoDb.NguoiDung.Sdt,
                 DiaChiChiTietNguoiDung = hoSoDb.NguoiDung.DiaChiChiTiet,
                 ThanhPhoId = hoSoDb.NguoiDung.ThanhPhoId,
                 QuanHuyenId = hoSoDb.NguoiDung.QuanHuyenId
            };

            // Chuẩn bị Dropdown Lists
            await PopulateThanhPhoDropdownAsync(viewModel.ThanhPhoId);
            await PopulateQuanHuyenDropdownAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId);
             ViewBag.TenCongTy = hoSoDb.TenCongTy; // Cho layout

            return View(viewModel);
        }

        // POST: /NhaTuyenDung/ChinhSuaHoSo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSuaHoSo(HoSoDoanhNghiepEditViewModel model)
        {
             var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (!int.TryParse(userIdString, out var userId))
             {
                 _logger.LogWarning("POST ChinhSuaHoSo: Không thể lấy User ID từ claims.");
                 return Challenge();
             }

             // --- Bảo mật: Đảm bảo người dùng chỉ sửa hồ sơ của chính họ ---
             if (model.NguoiDungId != userId)
             {
                 _logger.LogWarning("POST ChinhSuaHoSo: User {ActualUserId} cố gắng sửa hồ sơ của {TargetUserId}.", userId, model.NguoiDungId);
                 return Forbid(); // Từ chối quyền truy cập
             }

            // Lấy tên công ty từ model để hiển thị lại trên form lỗi (nếu có)
            // Cần lấy trước khi kiểm tra ModelState vì ViewBag cần được set ngay cả khi lỗi
            ViewBag.TenCongTy = model.TenCongTy;

             // --- Luôn tải lại Dropdown phòng trường hợp ModelState invalid ---
             await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
             await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);


             // --- Xử lý Model State ---
             if (ModelState.IsValid)
            {
                // Lấy bản ghi gốc từ DB để cập nhật
                 var hoSoDb = await _context.HoSoDoanhNghieps.FindAsync(model.NguoiDungId);
                 var nguoiDungDb = await _context.NguoiDungs.FindAsync(model.NguoiDungId); // Cần cập nhật cả NguoiDung

                 if (hoSoDb == null || nguoiDungDb == null)
                {
                    _logger.LogError("POST ChinhSuaHoSo: Không tìm thấy Hồ sơ DN hoặc Người dùng ID {UserId} trong DB để cập nhật.", model.NguoiDungId);
                    TempData["ErrorMessage"] = "Không tìm thấy dữ liệu gốc để cập nhật. Vui lòng thử lại.";
                    // Tải lại dropdown một lần nữa trước khi trả về View
                    await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
                    await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
                    return View(model); // Hiển thị lại form với lỗi
                }

                 // Bắt đầu Transaction để đảm bảo tính toàn vẹn (cập nhật DB + lưu file)
                 using var transaction = await _context.Database.BeginTransactionAsync();
                 try
                {
                    // 1. Xử lý Upload Logo mới (nếu có)
                    string? uniqueFileName = null;
                    string? newLogoUrl = null; // Lưu URL tương đối mới
                     if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        // Kiểm tra kiểu file (ví dụ: chỉ cho phép ảnh)
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                             ModelState.AddModelError(nameof(model.LogoFile), "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif).");
                             await transaction.RollbackAsync(); // Hủy transaction
                             // Tải lại dropdown trước khi trả về View lỗi
                             await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
                             await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
                             return View(model);
                        }

                        // Tạo tên file duy nhất để tránh trùng lặp
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(model.LogoFile.FileName) + fileExtension;
                        string uploadsFolderRelativePath = Path.Combine("images", "logos"); // Đường dẫn tương đối
                        string uploadsFolderAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, uploadsFolderRelativePath); // Đường dẫn tuyệt đối
                        string filePath = Path.Combine(uploadsFolderAbsolutePath, uniqueFileName);
                        newLogoUrl = $"/{uploadsFolderRelativePath.Replace('\\', '/')}/{uniqueFileName}"; // URL tương đối để lưu vào DB

                         // Đảm bảo thư mục tồn tại
                         Directory.CreateDirectory(uploadsFolderAbsolutePath);

                        // Lưu file mới
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.LogoFile.CopyToAsync(fileStream);
                        }
                         _logger.LogInformation("Đã lưu logo mới '{FileName}' tại '{FilePath}' cho User ID {UserId}.", uniqueFileName, filePath, userId);

                        // Xóa file logo cũ nếu tồn tại và khác file mới
                        if (!string.IsNullOrEmpty(hoSoDb.UrlLogo) && hoSoDb.UrlLogo != newLogoUrl)
                        {
                             // Lấy đường dẫn tuyệt đối của file cũ từ URL tương đối
                             string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, hoSoDb.UrlLogo.TrimStart('/'));
                             // Thay đổi dấu / thành dấu \ nếu cần thiết cho hệ điều hành Windows
                              oldFilePath = oldFilePath.Replace('/', Path.DirectorySeparatorChar);

                             if (System.IO.File.Exists(oldFilePath))
                            {
                                try {
                                     System.IO.File.Delete(oldFilePath);
                                     _logger.LogInformation("Đã xóa logo cũ '{OldPath}' cho User ID {UserId}.", oldFilePath, userId);
                                } catch (IOException ioEx) {
                                     _logger.LogWarning(ioEx, "Không thể xóa logo cũ '{OldPath}' cho User ID {UserId}.", oldFilePath, userId);
                                     // Không cần dừng lại nếu xóa file cũ lỗi
                                 }
                             }
                         }

                        // Cập nhật đường dẫn logo mới vào DB
                        hoSoDb.UrlLogo = newLogoUrl;
                    }
                    // Nếu không upload file mới thì UrlLogo giữ nguyên giá trị cũ trong hoSoDb

                    // 2. Cập nhật thông tin HoSoDoanhNghiep
                    hoSoDb.TenCongTy = model.TenCongTy;
                    hoSoDb.MaSoThue = model.MaSoThue;
                    hoSoDb.UrlWebsite = model.UrlWebsite;
                    hoSoDb.MoTa = model.MoTa;
                    hoSoDb.DiaChiDangKy = model.DiaChiDangKy;
                    hoSoDb.QuyMoCongTy = model.QuyMoCongTy;
                    // DaXacMinh, AdminXacMinhId, NgayXacMinh không được sửa ở đây

                    // 3. Cập nhật thông tin NguoiDung liên quan
                    nguoiDungDb.Sdt = model.SoDienThoaiLienHe;
                    nguoiDungDb.DiaChiChiTiet = model.DiaChiChiTietNguoiDung;
                    nguoiDungDb.ThanhPhoId = model.ThanhPhoId;
                    nguoiDungDb.QuanHuyenId = model.QuanHuyenId;
                    nguoiDungDb.NgayCapNhat = DateTime.UtcNow; // Luôn cập nhật ngày giờ

                    // 4. Lưu tất cả thay đổi vào DB
                    // Không cần gọi _context.Update() vì các đối tượng đã được theo dõi
                    await _context.SaveChangesAsync();

                    // 5. Commit transaction thành công
                    await transaction.CommitAsync();
                    _logger.LogInformation("Cập nhật hồ sơ thành công cho User ID {UserId}.", userId);

                    // 6. Đặt thông báo thành công và chuyển hướng về trang xem hồ sơ
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ công ty thành công!";
                    return RedirectToAction(nameof(HoSo));

                }
                 catch (DbUpdateConcurrencyException ex)
                 {
                     await transaction.RollbackAsync();
                     _logger.LogWarning(ex, "Lỗi concurrency khi cập nhật hồ sơ User ID {UserId}", userId);
                     ModelState.AddModelError("", "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang và thử lại.");
                 }
                 catch (Exception ex) // Bắt các lỗi khác (DB, file IO, ...)
                 {
                     await transaction.RollbackAsync();
                     _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ User ID {UserId}", userId);
                     ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi lưu thay đổi. Vui lòng thử lại.");
                 }
             }
             else // ModelState không hợp lệ
             {
                 _logger.LogWarning("POST ChinhSuaHoSo không thành công cho User ID {UserId} do ModelState không hợp lệ.", userId);
                 // Log chi tiết lỗi ModelState
                 foreach (var modelStateKey in ModelState.Keys)
                 {
                     var value = ModelState[modelStateKey];
                     foreach (var error in value.Errors)
                     {
                         _logger.LogDebug("Validation Error for {Key}: {ErrorMessage}", modelStateKey, error.ErrorMessage);
                     }
                 }
                 // ViewModel đã có lỗi, Dropdowns đã được load lại, chỉ cần trả về View
             }

             // Nếu ModelState không hợp lệ hoặc có lỗi xảy ra trong quá trình lưu
             // Đảm bảo dropdowns đã được load lại trước khi trả về View
             await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
             await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
             return View(model);
        }
    // Action API để lấy Quận/Huyện (dùng cho Javascript)
    [AllowAnonymous] // Cho phép truy cập không cần đăng nhập (hoặc [Authorize] nếu cần)
    [HttpGet("api/diachi/quanhuyen/{thanhPhoId:int}")] // Route rõ ràng hơn, thêm :int để ràng buộc kiểu
    public async Task<IActionResult> GetQuanHuyenApi(int thanhPhoId)
    {
        _logger.LogInformation("API GetQuanHuyenApi được gọi với ThanhPhoId: {ThanhPhoId}", thanhPhoId);

        if (thanhPhoId <= 0)
        {
            _logger.LogWarning("API GetQuanHuyenApi: ThanhPhoId không hợp lệ: {ThanhPhoId}", thanhPhoId);
            // Trả về 404 hoặc 400 đều hợp lý, 404 có thể rõ hơn là không tìm thấy tài nguyên ứng với ID này
            return NotFound(new { message = "ID Tỉnh/Thành phố không hợp lệ." });
        }

        try
        {
            var quanHuyens = await _context.QuanHuyens
                                        .AsNoTracking()
                                        .Where(qh => qh.ThanhPhoId == thanhPhoId)
                                        .OrderBy(qh => qh.Ten)
                                        // Chỉ chọn các trường cần thiết để giảm lượng dữ liệu truyền đi
                                        .Select(qh => new { id = qh.Id, ten = qh.Ten })
                                        .ToListAsync();

            _logger.LogInformation("API GetQuanHuyenApi: Tìm thấy {Count} Quận/Huyện cho ThanhPhoId: {ThanhPhoId}", quanHuyens.Count, thanhPhoId);

            // Trả về kết quả (có thể là mảng rỗng nếu không tìm thấy)
            return Ok(quanHuyens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API GetQuanHuyenApi: Lỗi nghiêm trọng khi truy vấn Quận/Huyện cho ThanhPhoId: {ThanhPhoId}", thanhPhoId);
            // Trả về lỗi 500 một cách chung chung để không lộ chi tiết lỗi
            return StatusCode(500, new { message = "Đã xảy ra lỗi phía máy chủ khi lấy dữ liệu quận/huyện." });
        }
    }
    }
}
//         // Action API để lấy Quận/Huyện (dùng cho Javascript)
//         [AllowAnonymous] // Cho phép truy cập không cần đăng nhập (hoặc [Authorize] nếu cần)
//         [HttpGet("api/diachi/quanhuyen/{thanhPhoId}")] // Route rõ ràng cho API
//         public async Task<IActionResult> GetQuanHuyenApi(int thanhPhoId)
//         {
//             if (thanhPhoId <= 0)
//             {
//                 _logger.LogInformation("API GetQuanHuyenApi được gọi với thanhPhoId không hợp lệ: {ThanhPhoId}", thanhPhoId);
//                 return BadRequest("ID Thành phố không hợp lệ."); // Trả về lỗi 400
//             }

//             _logger.LogDebug("API GetQuanHuyenApi được gọi cho ThanhPhoId: {ThanhPhoId}", thanhPhoId);
//              try
//              {
//                  var quanHuyens = await _context.QuanHuyens
//                                             .AsNoTracking()
//                                             .Where(qh => qh.ThanhPhoId == thanhPhoId)
//                                             .OrderBy(qh => qh.Ten)
//                                             .Select(qh => new { id = qh.Id, ten = qh.Ten }) // Chỉ lấy id và tên
//                                             .ToListAsync();

//                  _logger.LogDebug("Tìm thấy {Count} Quận/Huyện cho ThanhPhoId: {ThanhPhoId}", quanHuyens.Count, thanhPhoId);
//                  return Ok(quanHuyens); // Trả về JSON danh sách Quận Huyện
//              }
//             catch (Exception ex)
//             {
//                  _logger.LogError(ex, "Lỗi khi truy vấn Quận/Huyện cho ThanhPhoId: {ThanhPhoId} trong API", thanhPhoId);
//                  // Trả về lỗi 500 một cách chung chung
//                  return StatusCode(500, "Lỗi máy chủ nội bộ khi lấy dữ liệu quận/huyện.");
//             }
//         }

//     }
// }