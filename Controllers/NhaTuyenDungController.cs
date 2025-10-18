using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;
using HeThongTimViec.Extensions; // Cần cho GetDisplayName()

namespace HeThongTimViec.Controllers
{
    [Authorize(Roles = nameof(LoaiTaiKhoan.doanhnghiep))]
    public class NhaTuyenDungController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NhaTuyenDungController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NhaTuyenDungController(
            ApplicationDbContext context,
            ILogger<NhaTuyenDungController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /NhaTuyenDung/HoSo
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Challenge();
            }

            _logger.LogInformation("User ID {UserId} đang xem hồ sơ công ty.", userId);

            var hoSo = await _context.HoSoDoanhNghieps
                .Include(h => h.NguoiDung)
                    .ThenInclude(nd => nd.ThanhPho)
                .Include(h => h.NguoiDung)
                    .ThenInclude(nd => nd.QuanHuyen)
                .Include(h => h.AdminXacMinh)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.NguoiDungId == userId);

            if (hoSo == null)
            {
                _logger.LogWarning("Không tìm thấy Hồ sơ doanh nghiệp cho User ID: {UserId}.", userId);
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ công ty. Vui lòng hoàn tất hồ sơ.";
                return RedirectToAction(nameof(ChinhSuaHoSo));
            }

            // *** CẬP NHẬT PHẦN TẠO VIEWMODEL ĐỂ ĐẦY ĐỦ THÔNG TIN ***
            var viewModel = new HoSoDoanhNghiepViewModel
            {
                // Thông tin công ty
                TenCongTy = hoSo.TenCongTy,
                MaSoThue = hoSo.MaSoThue,
                UrlLogo = hoSo.UrlLogo,
                UrlWebsite = hoSo.UrlWebsite,
                MoTa = hoSo.MoTa,
                DiaChiDangKy = hoSo.DiaChiDangKy,
                QuyMoCongTy = hoSo.QuyMoCongTy,
                DaXacMinh = hoSo.DaXacMinh,
                NgayXacMinh = hoSo.NgayXacMinh,
                TenAdminXacMinh = hoSo.AdminXacMinh?.HoTen,

                // Thông tin đầy đủ của người đại diện từ NguoiDung
                HoTenNguoiDaiDien = hoSo.NguoiDung.HoTen,          // <-- LẤY DỮ LIỆU MỚI
                GioiTinhNguoiDaiDien = hoSo.NguoiDung.GioiTinh,    // <-- LẤY DỮ LIỆU MỚI
                NgaySinhNguoiDaiDien = hoSo.NguoiDung.NgaySinh,    // <-- LẤY DỮ LIỆU MỚI
                EmailLienHe = hoSo.NguoiDung.Email,
                SoDienThoaiLienHe = hoSo.NguoiDung.Sdt,
                DiaChiChiTietNguoiDung = hoSo.NguoiDung.DiaChiChiTiet,
                TenQuanHuyen = hoSo.NguoiDung.QuanHuyen?.Ten,
                TenThanhPho = hoSo.NguoiDung.ThanhPho?.Ten
            };
            

            ViewBag.TenCongTy = hoSo.TenCongTy;
            return View(viewModel);
        }

        // GET: /NhaTuyenDung/ChinhSuaHoSo (Giữ nguyên, không thay đổi)
        [HttpGet]
        public async Task<IActionResult> ChinhSuaHoSo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Challenge();
            }

            var hoSoDb = await _context.HoSoDoanhNghieps
                .Include(h => h.NguoiDung)
                .FirstOrDefaultAsync(h => h.NguoiDungId == userId);
            
            HoSoDoanhNghiepEditViewModel viewModel;

            if (hoSoDb != null)
            {
                viewModel = new HoSoDoanhNghiepEditViewModel
                {
                    NguoiDungId = hoSoDb.NguoiDungId,
                    TenCongTy = hoSoDb.TenCongTy,
                    MaSoThue = hoSoDb.MaSoThue,
                    CurrentUrlLogo = hoSoDb.UrlLogo,
                    UrlWebsite = hoSoDb.UrlWebsite,
                    MoTa = hoSoDb.MoTa,
                    DiaChiDangKy = hoSoDb.DiaChiDangKy,
                    QuyMoCongTy = hoSoDb.QuyMoCongTy,
                    SoDienThoaiLienHe = hoSoDb.NguoiDung.Sdt,
                    DiaChiChiTietNguoiDung = hoSoDb.NguoiDung.DiaChiChiTiet,
                    ThanhPhoId = hoSoDb.NguoiDung.ThanhPhoId,
                    QuanHuyenId = hoSoDb.NguoiDung.QuanHuyenId
                };
                ViewBag.TenCongTy = hoSoDb.TenCongTy;
            }
            else 
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null) return NotFound("Không tìm thấy người dùng.");

                viewModel = new HoSoDoanhNghiepEditViewModel { NguoiDungId = userId, SoDienThoaiLienHe = nguoiDung.Sdt };
                ViewBag.TenCongTy = "Hoàn tất hồ sơ";
            }
            
            await PopulateThanhPhoDropdownAsync(viewModel.ThanhPhoId);
            await PopulateQuanHuyenDropdownAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId);

            return View(viewModel);
        }

        // POST: /NhaTuyenDung/ChinhSuaHoSo (Giữ nguyên logic xử lý, chỉ sửa lỗi nhỏ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSuaHoSo(HoSoDoanhNghiepEditViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || model.NguoiDungId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật hồ sơ cho User ID {UserId}.", userId);
                await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
                await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
                ViewBag.TenCongTy = model.TenCongTy;
                return View(model);
            }

            var hoSoDb = await _context.HoSoDoanhNghieps.FindAsync(model.NguoiDungId);
            var nguoiDungDb = await _context.NguoiDungs.FindAsync(model.NguoiDungId);

            if (nguoiDungDb == null) return NotFound("Người dùng không tồn tại.");
            
            bool isCreatingNew = hoSoDb == null;
            if (isCreatingNew)
            {
                hoSoDb = new HoSoDoanhNghiep { NguoiDungId = userId };
                _context.HoSoDoanhNghieps.Add(hoSoDb);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    string uploadsFolderRelativePath = Path.Combine("file", "img", "Avatar");
                    string uploadsFolderAbsolutePath = Path.Combine(_webHostEnvironment.WebRootPath, uploadsFolderRelativePath);
                    Directory.CreateDirectory(uploadsFolderAbsolutePath);

                    string uniqueFileName = $"{userId}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(model.LogoFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolderAbsolutePath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fileStream);
                    }
                    _logger.LogInformation("Đã lưu logo mới '{FileName}' cho User ID {UserId}.", uniqueFileName, userId);

                    // Sửa lỗi: Cần kiểm tra hoSoDb không null trước khi truy cập
                    if (hoSoDb != null && !string.IsNullOrEmpty(hoSoDb.UrlLogo))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, hoSoDb.UrlLogo.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) { System.IO.File.Delete(oldFilePath); }
                    }
                    
                    if (hoSoDb != null)
                    {
                        hoSoDb.UrlLogo = $"/{uploadsFolderRelativePath.Replace('\\', '/')}/{uniqueFileName}";
                    }
                }

                if (hoSoDb != null)
                {
                    hoSoDb.TenCongTy = model.TenCongTy;
                    hoSoDb.MaSoThue = model.MaSoThue;
                    hoSoDb.UrlWebsite = model.UrlWebsite;
                    hoSoDb.MoTa = model.MoTa;
                    hoSoDb.DiaChiDangKy = model.DiaChiDangKy;
                    hoSoDb.QuyMoCongTy = model.QuyMoCongTy;
                }

                nguoiDungDb.Sdt = model.SoDienThoaiLienHe;
                nguoiDungDb.DiaChiChiTiet = model.DiaChiChiTietNguoiDung;
                nguoiDungDb.ThanhPhoId = model.ThanhPhoId;
                nguoiDungDb.QuanHuyenId = model.QuanHuyenId;
                nguoiDungDb.NgayCapNhat = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cập nhật hồ sơ thành công cho User ID {UserId}.", userId);
                TempData["SuccessMessage"] = "Cập nhật hồ sơ công ty thành công!";
                return RedirectToAction(nameof(HoSo));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ User ID {UserId}", userId);
                ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi lưu thay đổi.");
                await PopulateThanhPhoDropdownAsync(model.ThanhPhoId);
                await PopulateQuanHuyenDropdownAsync(model.ThanhPhoId, model.QuanHuyenId);
                return View(model);
            }
        }

        // --- Hàm Helper (giữ nguyên) ---
        private async Task PopulateThanhPhoDropdownAsync(object? selectedThanhPho = null)
        {
            ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", selectedThanhPho);
        }

        private async Task PopulateQuanHuyenDropdownAsync(int? thanhPhoId, object? selectedQuanHuyen = null)
        {
            if (thanhPhoId.HasValue && thanhPhoId > 0)
            {
                ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens.AsNoTracking().Where(qh => qh.ThanhPhoId == thanhPhoId).OrderBy(qh => qh.Ten).ToListAsync(), "Id", "Ten", selectedQuanHuyen);
            }
            else
            {
                ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
            }
        }
    }
}