// File: Controllers/UngTuyenController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.UngTuyen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Services;
using HeThongTimViec.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json; 

namespace HeThongTimViec.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class UngTuyenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UngTuyenController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IThongBaoService _thongBaoService; // <<<<<<< ĐÃ CÓ

        public UngTuyenController(
            ApplicationDbContext context,
            ILogger<UngTuyenController> logger,
            IWebHostEnvironment webHostEnvironment,
            IThongBaoService thongBaoService) // <<<<<<< Thêm vào constructor
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _thongBaoService = thongBaoService; // <<<<<<< Gán service
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            _logger.LogError("Không thể parse User ID từ ClaimsPrincipal.");
            throw new UnauthorizedAccessException("Không thể xác định người dùng.");
        }

        // GET: UngTuyen/NopHoSo/5 (id là TinTuyenDungId)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> NopHoSo(int id)
        {
            var tinTuyenDung = await _context.TinTuyenDungs
                                        .Include(t => t.NguoiDang)
                                            .ThenInclude(nd => nd.HoSoDoanhNghiep)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.Id == id && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            if (tinTuyenDung == null)
            {
                TempData["ErrorMessage"] = "Tin tuyển dụng không tồn tại, chưa được duyệt hoặc đã hết hạn.";
                return RedirectToAction("Index", "TimViec");
            }

            int currentUserId = GetCurrentUserId();
            bool daUngTuyen = await _context.UngTuyens
                                    .AnyAsync(ut => ut.TinTuyenDungId == id && ut.UngVienId == currentUserId);
            if (daUngTuyen)
            {
                TempData["WarningMessage"] = "Bạn đã ứng tuyển vào vị trí này trước đó.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = id });
            }

            var nguoiDungUngVien = await _context.NguoiDungs.FindAsync(currentUserId);
            if (nguoiDungUngVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            string? tenNhaTuyenDungDisplay = null;
            if (tinTuyenDung.NguoiDang != null)
            {
                if (tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && tinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                    tenNhaTuyenDungDisplay = tinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy;
                else tenNhaTuyenDungDisplay = tinTuyenDung.NguoiDang.HoTen;
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
            // <<<<<<< QUAN TRỌNG: Include NguoiDang và HoSoUngVien để gửi thông báo >>>>>>>
            var tinTuyenDung = await _context.TinTuyenDungs
                                        .Include(t => t.NguoiDang) // Cần NguoiDang để lấy NguoiDangId (NTD ID)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.Id == model.TinTuyenDungId && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            if (tinTuyenDung != null)
            {
                model.TieuDeTinTuyenDung = tinTuyenDung.TieuDe;
                if (tinTuyenDung.NguoiDang != null)
                {
                    model.LoaiTaiKhoanNguoiDang = tinTuyenDung.NguoiDang.LoaiTk;
                    if (tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && tinTuyenDung.NguoiDang.HoSoDoanhNghiep != null)
                        model.TenNhaTuyenDungHoacCaNhan = tinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy;
                    else model.TenNhaTuyenDungHoacCaNhan = tinTuyenDung.NguoiDang.HoTen;
                }
                else model.TenNhaTuyenDungHoacCaNhan = "Không có thông tin người đăng";
            }
            else
            {
                ModelState.AddModelError("", "Tin tuyển dụng không hợp lệ, đã hết hạn hoặc không tìm thấy.");
            }

            // <<<<<<< Lấy thông tin ứng viên (người đang nộp) cùng với HoSoUngVien >>>>>>>
            var nguoiDungUngVien = await _context.NguoiDungs
                                            .Include(u => u.HoSoUngVien) // Để lấy tên/tiêu đề hồ sơ
                                            .AsNoTracking() // Dùng AsNoTracking vì chỉ đọc
                                            .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (nguoiDungUngVien != null)
            {
                model.HoTenUngVien = nguoiDungUngVien.HoTen;
                model.EmailUngVien = nguoiDungUngVien.Email;
                model.SdtUngVien = nguoiDungUngVien.Sdt;
            }
            else
            {
                ModelState.AddModelError("", "Không thể xác thực thông tin người dùng ứng tuyển.");
            }

            bool daUngTuyen = await _context.UngTuyens
                                    .AnyAsync(ut => ut.TinTuyenDungId == model.TinTuyenDungId && ut.UngVienId == currentUserId);
            if (daUngTuyen)
            {
                TempData["WarningMessage"] = "Bạn đã ứng tuyển vào vị trí này rồi.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }

            string? uniqueFileName = null;
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                if (model.CvFile.Length > 5 * 1024 * 1024) ModelState.AddModelError(nameof(model.CvFile), "Kích thước file CV không được vượt quá 5MB.");
                string[] permittedExtensions = { ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(model.CvFile.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext)) ModelState.AddModelError(nameof(model.CvFile), "Chỉ chấp nhận file CV định dạng PDF, DOC, DOCX.");

                if (ModelState.GetFieldValidationState(nameof(model.CvFile)) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
                {
                    string cvUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "file", "CV");
                    if (!Directory.Exists(cvUploadsFolder)) { try { Directory.CreateDirectory(cvUploadsFolder); } catch (Exception ex_dir) { _logger.LogError(ex_dir, "Không thể tạo thư mục lưu CV: {Path}", cvUploadsFolder); ModelState.AddModelError("", "Lỗi hệ thống khi chuẩn bị lưu CV. Vui lòng thử lại sau."); return View(model); } }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CvFile.FileName);
                    string filePath = Path.Combine(cvUploadsFolder, uniqueFileName);
                    try { using (var fileStream = new FileStream(filePath, FileMode.Create)) { await model.CvFile.CopyToAsync(fileStream); } model.UrlCvDaNop = "/file/CV/" + uniqueFileName; }
                    catch (Exception ex_upload) { _logger.LogError(ex_upload, "Lỗi khi upload CV cho người dùng {UserId} vào tin {TinId}", currentUserId, model.TinTuyenDungId); ModelState.AddModelError("", "Đã có lỗi xảy ra khi tải lên CV của bạn. Vui lòng thử lại."); }
                }
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state không hợp lệ khi nộp hồ sơ cho tin {TinID} bởi user {UserID}.", model.TinTuyenDungId, currentUserId);
                return View(model);
            }

            var ungTuyen = new UngTuyen
            {
                TinTuyenDungId = model.TinTuyenDungId,
                UngVienId = currentUserId,
                ThuGioiThieu = model.ThuGioiThieu,
                UrlCvDaNop = model.UrlCvDaNop,
                TrangThai = TrangThaiUngTuyen.danop,
                NgayNop = DateTime.UtcNow
            };

            _context.UngTuyens.Add(ungTuyen);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Người dùng {UserId} đã ứng tuyển thành công vào tin {TinId}. CV Url: {CvUrl}. UngTuyenID: {UngTuyenID}",
                                        currentUserId, model.TinTuyenDungId, model.UrlCvDaNop, ungTuyen.Id); // Log UngTuyenID

                // ---------- BẮT ĐẦU TÍCH HỢP GỬI THÔNG BÁO CHO NHÀ TUYỂN DỤNG ----------
                // Đảm bảo tinTuyenDung và nguoiDungUngVien không null (đã được kiểm tra hoặc tải lại ở trên)
                if (tinTuyenDung != null && tinTuyenDung.NguoiDangId != 0 && nguoiDungUngVien != null)
                {
                    try
                    {
                        string? tenHienThiUngVien = nguoiDungUngVien.HoTen;
                        if (string.IsNullOrWhiteSpace(tenHienThiUngVien)) tenHienThiUngVien = nguoiDungUngVien.HoTen;
                        if (string.IsNullOrWhiteSpace(tenHienThiUngVien)) tenHienThiUngVien = $"Ứng viên #{nguoiDungUngVien.Id}";

                        var duLieuThongBao = new
                        {
                            tenUngVien = tenHienThiUngVien,
                            avatarUngVien = nguoiDungUngVien.UrlAvatar ?? "/images/default-avatar.png",
                            tieuDeTin = tinTuyenDung.TieuDe,
                            tinId = tinTuyenDung.Id,
                            ungTuyenId = ungTuyen.Id,
                            noiDung = $"{tenHienThiUngVien} vừa ứng tuyển vào vị trí \"{tinTuyenDung.TieuDe}\" của bạn.",
                            // <<< URL ĐÃ ĐƯỢC CHUẨN HÓA >>>
    url = Url.Action(
        "ChiTietHoSo",      // Action xem chi tiết hồ sơ
        "QuanLyUngVien",    // Controller của Nhà Tuyển Dụng
        new
        {
            // area = "", // Bỏ trống nếu bạn không dùng Areas
            ungVienId = ungTuyen.UngVienId, // ID của người dùng ứng viên
            ungTuyenId = ungTuyen.Id        // ID của đơn ứng tuyển để cung cấp context
        },
        Request.Scheme)
                        };

                        await _thongBaoService.CreateThongBaoAsync(
                            tinTuyenDung.NguoiDangId,
                            NotificationConstants.Types.UngTuyenMoiChoNtd,
                            JsonSerializer.Serialize(duLieuThongBao),
                            NotificationConstants.RelatedEntities.UngTuyen,
                            ungTuyen.Id
                        );
                        _logger.LogInformation("Đã gửi thông báo ứng tuyển mới cho NTD ID {NtdId} từ UngVien ID {UngVienId} cho Tin ID {TinId}. UngTuyenID: {UngTuyenId}",
                            tinTuyenDung.NguoiDangId, currentUserId, model.TinTuyenDungId, ungTuyen.Id);
                    }
                    catch (Exception ex_notify)
                    {
                        _logger.LogError(ex_notify, "Lỗi khi gửi thông báo ứng tuyển mới cho NTD ID {NtdId}. UngTuyenID: {UngTuyenId}", tinTuyenDung.NguoiDangId, ungTuyen.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Không thể gửi thông báo ứng tuyển mới: TinTuyenDung (NguoiDangId={NguoiDangId}) hoặc NguoiDungUngVien (ID={UngVienId}) không hợp lệ.",
                                        tinTuyenDung?.NguoiDangId, nguoiDungUngVien?.Id);
                }
                // ---------- KẾT THÚC TÍCH HỢP GỬI THÔNG BÁO ----------

                TempData["SuccessMessage"] = "Nộp hồ sơ ứng tuyển thành công!";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi DbUpdateException khi người dùng {UserId} ứng tuyển vào tin {TinId}", currentUserId, model.TinTuyenDungId);
                if (ex.InnerException != null && (ex.InnerException.Message.Contains("UNIQUE KEY constraint 'uq_TinTuyenDung_UngVien'", StringComparison.OrdinalIgnoreCase) || ex.InnerException.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) || ex.InnerException.Message.Contains("uq_TinTuyenDung_UngVien", StringComparison.OrdinalIgnoreCase)))
                    TempData["WarningMessage"] = "Có vẻ như bạn đã ứng tuyển vào vị trí này rồi.";
                else TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi lưu hồ sơ ứng tuyển. Vui lòng thử lại hoặc liên hệ quản trị viên.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi người dùng {UserId} ứng tuyển vào tin {TinId}", currentUserId, model.TinTuyenDungId);
                TempData["ErrorMessage"] = "Đã có lỗi không mong muốn xảy ra trong quá trình ứng tuyển. Vui lòng thử lại sau.";
                return RedirectToAction("ChiTiet", "TimViec", new { id = model.TinTuyenDungId });
            }
        }

        // === ACTION MỚI: XEM CHI TIẾT ĐƠN ỨNG TUYỂN ===
        // GET: /UngTuyen/ChiTiet/123 (id là UngTuyenId)
        // === PHIÊN BẢN CHI TIẾT ĐÃ SỬA LỖI NULL REFERENCE ===
        // === PHIÊN BẢN CHI TIẾT NGẮN GỌN VÀ AN TOÀN ===
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var currentUserId = GetCurrentUserId();

            // Lấy thông tin đơn ứng tuyển, vẫn Include đầy đủ các thông tin liên quan
            var ungTuyen = await _context.UngTuyens
                .AsNoTracking()
                .Include(ut => ut.UngVien)
                .Include(ut => ut.TinTuyenDung)
                    .ThenInclude(ttd => ttd.NguoiDang)
                        .ThenInclude(nd => nd.HoSoDoanhNghiep)
                .FirstOrDefaultAsync(ut => ut.Id == id);

            // Nếu không tìm thấy đơn ứng tuyển, trả về NotFound
            if (ungTuyen == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
                return NotFound();
            }

            // Kiểm tra quyền xem
            bool isOwnerUngVien = (ungTuyen.UngVienId == currentUserId);
            bool isOwnerNtd = (ungTuyen.TinTuyenDung.NguoiDangId == currentUserId); 

            if (!isOwnerUngVien && !isOwnerNtd)
            {
                return Forbid();
            }
             string tenNhaTuyenDung = ungTuyen.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? ungTuyen.TinTuyenDung.NguoiDang.HoTen;
        string? logoNhaTuyenDung = ungTuyen.TinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo ?? ungTuyen.TinTuyenDung.NguoiDang.UrlAvatar;

        // === TẠO SLUG CHO NHÀ TUYỂN DỤNG ===
        string companySlug = SeoUrlHelper.GenerateSlug(tenNhaTuyenDung) + "-" + ungTuyen.TinTuyenDung.NguoiDangId;

            // Tạo ViewModel bằng cách sử dụng các toán tử an toàn với null
            var viewModel = new ChiTietUngTuyenViewModel
            {
                Id = ungTuyen.Id,
                NgayNop = ungTuyen.NgayNop,
                TrangThai = ungTuyen.TrangThai,
                ThuGioiThieu = ungTuyen.ThuGioiThieu,
                UrlCvDaNop = ungTuyen.UrlCvDaNop,

                // Thông tin Tin tuyển dụng (sử dụng toán tử ?? để có giá trị mặc định)
                TinTuyenDungId = ungTuyen.TinTuyenDungId,
                TieuDeTinTuyenDung = ungTuyen.TinTuyenDung?.TieuDe ?? "Tin tuyển dụng không tồn tại",

                // Thông tin Nhà tuyển dụng (kết hợp ?. và ??)
                TenNhaTuyenDung = ungTuyen.TinTuyenDung?.NguoiDang?.HoSoDoanhNghiep?.TenCongTy
                                ?? ungTuyen.TinTuyenDung?.NguoiDang?.HoTen
                                ?? "Nhà tuyển dụng không xác định",
                LogoNhaTuyenDung = ungTuyen.TinTuyenDung?.NguoiDang?.HoSoDoanhNghiep?.UrlLogo
                                 ?? ungTuyen.TinTuyenDung?.NguoiDang?.UrlAvatar,

                // Thông tin Ứng viên (sử dụng ?. và ??)
                UngVienId = ungTuyen.UngVienId,
                HoTenUngVien = ungTuyen.UngVien?.HoTen ?? "Ứng viên không xác định",
                AvatarUngVien = ungTuyen.UngVien?.UrlAvatar,
                EmailUngVien = ungTuyen.UngVien?.Email ?? "N/A",
                SdtUngVien = ungTuyen.UngVien?.Sdt ?? "N/A",
                 SlugNhaTuyenDung = companySlug,

                // Cờ quyền xem
                CanViewAsNtd = isOwnerNtd,
                CanViewAsUngVien = isOwnerUngVien
            };

            return View(viewModel);
        }
    }
}