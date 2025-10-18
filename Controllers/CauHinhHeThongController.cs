// File: Areas/Admin/Controllers/CauHinhHeThongController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For DisplayAttribute
using System.Linq;
using System.Reflection; // For GetCustomAttribute
using System.Threading.Tasks;

namespace HeThongTimViec.Areas.Admin.Controllers
{
    [Route("admin/CauHinhHeThong")]
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class CauHinhHeThongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CauHinhHeThongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main Index for System Configuration - links to other sections
        [Route("Admin/CauHinhHeThong")]
        public IActionResult Index()
        {
            // This view will have links to NganhNghe, ThanhPho, QuanHuyen, etc.
            return View();
        }

        #region Ngành Nghề Management

        [Route("Admin/CauHinhHeThong/NganhNghe")]
        public async Task<IActionResult> NganhNghe_Index()
        {
            ViewData["Title"] = "Quản Lý Ngành Nghề";
            var nganhNghes = await _context.NganhNghes.OrderBy(n => n.Ten).ToListAsync();
            return View("NganhNghe/NganhNghe_Index", nganhNghes);
        }

        [Route("Admin/CauHinhHeThong/NganhNghe/Details/{id?}")]
        public async Task<IActionResult> NganhNghe_Details(int? id)
        {
            ViewData["Title"] = "Chi Tiết Ngành Nghề";
            if (id == null) return NotFound();
            var nganhNghe = await _context.NganhNghes.FirstOrDefaultAsync(m => m.Id == id);
            if (nganhNghe == null) return NotFound();
            return View("NganhNghe/NganhNghe_Details", nganhNghe);
        }

        [Route("Admin/CauHinhHeThong/NganhNghe/Create")]
        public IActionResult NganhNghe_Create()
        {
            ViewData["Title"] = "Tạo Mới Ngành Nghề";
            return View("NganhNghe/NganhNghe_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/NganhNghe/Create")]
        public async Task<IActionResult> NganhNghe_Create([Bind("Ten,MoTa")] NganhNghe nganhNghe)
        {
            ViewData["Title"] = "Tạo Mới Ngành Nghề";
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.NganhNghes.AnyAsync(n => n.Ten.Trim().ToLower() == nganhNghe.Ten.Trim().ToLower());
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên ngành nghề này đã tồn tại.");
                    return View("NganhNghe/NganhNghe_Create", nganhNghe);
                }
                _context.Add(nganhNghe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo mới ngành nghề thành công!";
                return RedirectToAction(nameof(NganhNghe_Index));
            }
            TempData["ErrorMessage"] = "Tạo mới ngành nghề thất bại. Vui lòng kiểm tra lại thông tin.";
            return View("NganhNghe/NganhNghe_Create", nganhNghe);
        }

        [Route("Admin/CauHinhHeThong/NganhNghe/Edit/{id?}")]
        public async Task<IActionResult> NganhNghe_Edit(int? id)
        {
            ViewData["Title"] = "Chỉnh Sửa Ngành Nghề";
            if (id == null) return NotFound();
            var nganhNghe = await _context.NganhNghes.FindAsync(id);
            if (nganhNghe == null) return NotFound();
            return View("NganhNghe/NganhNghe_Edit", nganhNghe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/NganhNghe/Edit/{id}")]
        public async Task<IActionResult> NganhNghe_Edit(int id, [Bind("Id,Ten,MoTa")] NganhNghe nganhNghe)
        {
            ViewData["Title"] = "Chỉnh Sửa Ngành Nghề";
            if (id != nganhNghe.Id) return NotFound();
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.NganhNghes.AnyAsync(n => n.Ten.Trim().ToLower() == nganhNghe.Ten.Trim().ToLower() && n.Id != nganhNghe.Id);
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên ngành nghề này đã tồn tại.");
                    return View("NganhNghe/NganhNghe_Edit", nganhNghe);
                }
                try
                {
                    _context.Update(nganhNghe);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật ngành nghề thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NganhNgheExists(nganhNghe.Id)) return NotFound();
                    else {
                        TempData["ErrorMessage"] = "Lỗi khi cập nhật. Dữ liệu có thể đã được thay đổi bởi người khác.";
                        return View("NganhNghe/NganhNghe_Edit", nganhNghe);
                     }
                }
                return RedirectToAction(nameof(NganhNghe_Index));
            }
            TempData["ErrorMessage"] = "Cập nhật ngành nghề thất bại. Vui lòng kiểm tra lại thông tin.";
            return View("NganhNghe/NganhNghe_Edit", nganhNghe);
        }

        [Route("Admin/CauHinhHeThong/NganhNghe/Delete/{id?}")]
        public async Task<IActionResult> NganhNghe_Delete(int? id)
        {
            ViewData["Title"] = "Xác Nhận Xóa Ngành Nghề";
            if (id == null) return NotFound();
            var nganhNghe = await _context.NganhNghes.FirstOrDefaultAsync(m => m.Id == id);
            if (nganhNghe == null) return NotFound();
            
            bool isInUse = await _context.TinTuyenDung_NganhNghes.AnyAsync(t => t.NganhNgheId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa ngành nghề này vì đang được sử dụng trong ít nhất một tin tuyển dụng. Vui lòng gỡ ngành nghề này khỏi các tin tuyển dụng trước.";
                return RedirectToAction(nameof(NganhNghe_Index));
            }
            return View("NganhNghe/NganhNghe_Delete", nganhNghe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/NganhNghe/Delete/{id}")]
        public async Task<IActionResult> NganhNghe_DeleteConfirmed(int id) // ActionName is not needed due to distinct route
        {
            var nganhNghe = await _context.NganhNghes.FindAsync(id);
            if (nganhNghe == null) {
                 TempData["ErrorMessage"] = "Không tìm thấy ngành nghề để xóa.";
                return RedirectToAction(nameof(NganhNghe_Index));
            }
             bool isInUse = await _context.TinTuyenDung_NganhNghes.AnyAsync(t => t.NganhNgheId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa ngành nghề này vì đang được sử dụng. Vui lòng gỡ khỏi các tin tuyển dụng trước.";
                return RedirectToAction(nameof(NganhNghe_Index));
            }
            try
            {
                _context.NganhNghes.Remove(nganhNghe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa ngành nghề thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ngành nghề. Có thể ngành nghề này vẫn đang được tham chiếu hoặc có lỗi cơ sở dữ liệu.";
            }
            return RedirectToAction(nameof(NganhNghe_Index));
        }

        private bool NganhNgheExists(int id) => _context.NganhNghes.Any(e => e.Id == id);

        #endregion

        #region Tỉnh/Thành Phố Management

        [Route("Admin/CauHinhHeThong/ThanhPho")]
        public async Task<IActionResult> ThanhPho_Index()
        {
            ViewData["Title"] = "Quản Lý Tỉnh/Thành Phố";
            var thanhPhos = await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync();
            return View("ThanhPho/ThanhPho_Index", thanhPhos);
        }
        
        [Route("Admin/CauHinhHeThong/ThanhPho/Details/{id?}")]
        public async Task<IActionResult> ThanhPho_Details(int? id)
        {
            ViewData["Title"] = "Chi Tiết Tỉnh/Thành Phố";
            if (id == null) return NotFound();
            var thanhPho = await _context.ThanhPhos
                                .Include(t => t.QuanHuyens) // Include related districts
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (thanhPho == null) return NotFound();
            return View("ThanhPho/ThanhPho_Details", thanhPho);
        }
        
        [Route("Admin/CauHinhHeThong/ThanhPho/Create")]
        public IActionResult ThanhPho_Create()
        {
            ViewData["Title"] = "Tạo Mới Tỉnh/Thành Phố";
            return View("ThanhPho/ThanhPho_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/ThanhPho/Create")]
        public async Task<IActionResult> ThanhPho_Create([Bind("Ten")] ThanhPho thanhPho)
        {
            ViewData["Title"] = "Tạo Mới Tỉnh/Thành Phố";
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.ThanhPhos.AnyAsync(tp => tp.Ten.Trim().ToLower() == thanhPho.Ten.Trim().ToLower());
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên Tỉnh/Thành phố này đã tồn tại.");
                    return View("ThanhPho/ThanhPho_Create", thanhPho);
                }
                _context.Add(thanhPho);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo mới Tỉnh/Thành phố thành công!";
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            TempData["ErrorMessage"] = "Tạo mới Tỉnh/Thành phố thất bại.";
            return View("ThanhPho/ThanhPho_Create", thanhPho);
        }

        [Route("Admin/CauHinhHeThong/ThanhPho/Edit/{id?}")]
        public async Task<IActionResult> ThanhPho_Edit(int? id)
        {
            ViewData["Title"] = "Chỉnh Sửa Tỉnh/Thành Phố";
            if (id == null) return NotFound();
            var thanhPho = await _context.ThanhPhos.FindAsync(id);
            if (thanhPho == null) return NotFound();
            return View("ThanhPho/ThanhPho_Edit", thanhPho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/ThanhPho/Edit/{id}")]
        public async Task<IActionResult> ThanhPho_Edit(int id, [Bind("Id,Ten")] ThanhPho thanhPho)
        {
            ViewData["Title"] = "Chỉnh Sửa Tỉnh/Thành Phố";
            if (id != thanhPho.Id) return NotFound();
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.ThanhPhos.AnyAsync(tp => tp.Ten.Trim().ToLower() == thanhPho.Ten.Trim().ToLower() && tp.Id != thanhPho.Id);
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên Tỉnh/Thành phố này đã tồn tại.");
                    return View("ThanhPho/ThanhPho_Edit", thanhPho);
                }
                try
                {
                    _context.Update(thanhPho);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật Tỉnh/Thành phố thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThanhPhoExists(thanhPho.Id)) return NotFound();
                    else { 
                        TempData["ErrorMessage"] = "Lỗi khi cập nhật. Dữ liệu có thể đã được thay đổi bởi người khác.";
                        return View("ThanhPho/ThanhPho_Edit", thanhPho);
                    }
                }
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            TempData["ErrorMessage"] = "Cập nhật Tỉnh/Thành phố thất bại.";
            return View("ThanhPho/ThanhPho_Edit", thanhPho);
        }
        
        [Route("Admin/CauHinhHeThong/ThanhPho/Delete/{id?}")]
        public async Task<IActionResult> ThanhPho_Delete(int? id)
        {
            ViewData["Title"] = "Xác Nhận Xóa Tỉnh/Thành Phố";
            if (id == null) return NotFound();
            var thanhPho = await _context.ThanhPhos.FirstOrDefaultAsync(m => m.Id == id);
            if (thanhPho == null) return NotFound();
            
            bool hasQuanHuyen = await _context.QuanHuyens.AnyAsync(qh => qh.ThanhPhoId == id);
            if (hasQuanHuyen)
            {
                TempData["ErrorMessage"] = "Không thể xóa Tỉnh/Thành phố này vì có Quận/Huyện phụ thuộc. Vui lòng xóa các Quận/Huyện liên quan trước.";
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            bool usedInTinTuyenDung = await _context.TinTuyenDungs.AnyAsync(t => t.ThanhPhoId == id);
            bool usedInNguoiDung = await _context.NguoiDungs.AnyAsync(u => u.ThanhPhoId == id);
             if (usedInTinTuyenDung || usedInNguoiDung)
            {
                TempData["ErrorMessage"] = "Không thể xóa Tỉnh/Thành phố này vì đang được sử dụng trong Tin tuyển dụng hoặc Hồ sơ người dùng.";
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            return View("ThanhPho/ThanhPho_Delete", thanhPho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/ThanhPho/Delete/{id}")]
        public async Task<IActionResult> ThanhPho_DeleteConfirmed(int id)
        {
            var thanhPho = await _context.ThanhPhos.FindAsync(id);
             if (thanhPho == null) {
                 TempData["ErrorMessage"] = "Không tìm thấy Tỉnh/Thành phố.";
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            bool hasQuanHuyen = await _context.QuanHuyens.AnyAsync(qh => qh.ThanhPhoId == id);
            if (hasQuanHuyen)
            {
                TempData["ErrorMessage"] = "Không thể xóa Tỉnh/Thành phố này vì có Quận/Huyện phụ thuộc.";
                return RedirectToAction(nameof(ThanhPho_Index));
            }
            bool usedInTinTuyenDung = await _context.TinTuyenDungs.AnyAsync(t => t.ThanhPhoId == id);
            bool usedInNguoiDung = await _context.NguoiDungs.AnyAsync(u => u.ThanhPhoId == id);
            if (usedInTinTuyenDung || usedInNguoiDung)
            {
                TempData["ErrorMessage"] = "Không thể xóa Tỉnh/Thành phố này vì đang được sử dụng.";
                return RedirectToAction(nameof(ThanhPho_Index));
            }

            try
            {
                _context.ThanhPhos.Remove(thanhPho);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Tỉnh/Thành phố thành công!";
            }
            catch (DbUpdateException)
            {
                 TempData["ErrorMessage"] = "Lỗi khi xóa Tỉnh/Thành phố. Có thể vẫn còn dữ liệu liên quan hoặc lỗi cơ sở dữ liệu.";
            }
            return RedirectToAction(nameof(ThanhPho_Index));
        }

        private bool ThanhPhoExists(int id) => _context.ThanhPhos.Any(e => e.Id == id);

        #endregion

        #region Quận/Huyện Management

        [Route("Admin/CauHinhHeThong/QuanHuyen")]
        public async Task<IActionResult> QuanHuyen_Index(int? thanhPhoIdFilter)
        {
            ViewData["Title"] = "Quản Lý Quận/Huyện";
            ViewData["ThanhPhoIdFilter"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", thanhPhoIdFilter);

            var query = _context.QuanHuyens.Include(q => q.ThanhPho).AsQueryable();

            if (thanhPhoIdFilter.HasValue && thanhPhoIdFilter > 0)
            {
                query = query.Where(q => q.ThanhPhoId == thanhPhoIdFilter.Value);
            }

            var quanHuyens = await query.OrderBy(q => q.ThanhPho.Ten).ThenBy(q => q.Ten).ToListAsync();
            return View("QuanHuyen/QuanHuyen_Index", quanHuyens);
        }

        [Route("Admin/CauHinhHeThong/QuanHuyen/Details/{id?}")]
        public async Task<IActionResult> QuanHuyen_Details(int? id)
        {
            ViewData["Title"] = "Chi Tiết Quận/Huyện";
            if (id == null) return NotFound();
            var quanHuyen = await _context.QuanHuyens
                .Include(q => q.ThanhPho)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quanHuyen == null) return NotFound();
            return View("QuanHuyen/QuanHuyen_Details", quanHuyen);
        }

        [Route("Admin/CauHinhHeThong/QuanHuyen/Create")]
        public async Task<IActionResult> QuanHuyen_Create()
        {
            ViewData["Title"] = "Tạo Mới Quận/Huyện";
            ViewData["ThanhPhoId"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten");
            return View("QuanHuyen/QuanHuyen_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/QuanHuyen/Create")]
        public async Task<IActionResult> QuanHuyen_Create([Bind("Ten,ThanhPhoId")] QuanHuyen quanHuyen)
        {
            ViewData["Title"] = "Tạo Mới Quận/Huyện";
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.QuanHuyens.AnyAsync(qh => qh.Ten.Trim().ToLower() == quanHuyen.Ten.Trim().ToLower() && qh.ThanhPhoId == quanHuyen.ThanhPhoId);
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên Quận/Huyện này đã tồn tại trong Tỉnh/Thành phố đã chọn.");
                }
                
                if (quanHuyen.ThanhPhoId <= 0) // Basic validation for ThanhPhoId
                {
                     ModelState.AddModelError("ThanhPhoId", "Vui lòng chọn Tỉnh/Thành phố.");
                }

                if(ModelState.IsValid) // Recheck after custom validation
                {
                    _context.Add(quanHuyen);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tạo mới Quận/Huyện thành công!";
                    return RedirectToAction(nameof(QuanHuyen_Index));
                }
            }
            ViewData["ThanhPhoId"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", quanHuyen.ThanhPhoId);
            TempData["ErrorMessage"] = "Tạo mới Quận/Huyện thất bại.";
            return View("QuanHuyen/QuanHuyen_Create", quanHuyen);
        }

        [Route("Admin/CauHinhHeThong/QuanHuyen/Edit/{id?}")]
        public async Task<IActionResult> QuanHuyen_Edit(int? id)
        {
            ViewData["Title"] = "Chỉnh Sửa Quận/Huyện";
            if (id == null) return NotFound();
            var quanHuyen = await _context.QuanHuyens.FindAsync(id);
            if (quanHuyen == null) return NotFound();
            ViewData["ThanhPhoId"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", quanHuyen.ThanhPhoId);
            return View("QuanHuyen/QuanHuyen_Edit", quanHuyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/QuanHuyen/Edit/{id}")]
        public async Task<IActionResult> QuanHuyen_Edit(int id, [Bind("Id,Ten,ThanhPhoId")] QuanHuyen quanHuyen)
        {
            ViewData["Title"] = "Chỉnh Sửa Quận/Huyện";
            if (id != quanHuyen.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                bool tenDaTonTai = await _context.QuanHuyens.AnyAsync(qh => qh.Ten.Trim().ToLower() == quanHuyen.Ten.Trim().ToLower() && qh.ThanhPhoId == quanHuyen.ThanhPhoId && qh.Id != quanHuyen.Id);
                if (tenDaTonTai)
                {
                    ModelState.AddModelError("Ten", "Tên Quận/Huyện này đã tồn tại trong Tỉnh/Thành phố đã chọn.");
                }
                if (quanHuyen.ThanhPhoId <= 0)
                {
                     ModelState.AddModelError("ThanhPhoId", "Vui lòng chọn Tỉnh/Thành phố.");
                }

                if(ModelState.IsValid) // Recheck
                {
                    try
                    {
                        _context.Update(quanHuyen);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật Quận/Huyện thành công!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!QuanHuyenExists(quanHuyen.Id)) return NotFound();
                        else { 
                            TempData["ErrorMessage"] = "Lỗi khi cập nhật. Dữ liệu có thể đã được thay đổi bởi người khác.";
                            ViewData["ThanhPhoId"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", quanHuyen.ThanhPhoId);
                            return View("QuanHuyen/QuanHuyen_Edit", quanHuyen);
                        }
                    }
                    return RedirectToAction(nameof(QuanHuyen_Index));
                }
            }
            ViewData["ThanhPhoId"] = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", quanHuyen.ThanhPhoId);
            TempData["ErrorMessage"] = "Cập nhật Quận/Huyện thất bại.";
            return View("QuanHuyen/QuanHuyen_Edit", quanHuyen);
        }
        
        [Route("Admin/CauHinhHeThong/QuanHuyen/Delete/{id?}")]
        public async Task<IActionResult> QuanHuyen_Delete(int? id)
        {
            ViewData["Title"] = "Xác Nhận Xóa Quận/Huyện";
            if (id == null) return NotFound();
            var quanHuyen = await _context.QuanHuyens
                .Include(q => q.ThanhPho)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quanHuyen == null) return NotFound();

            bool usedInTinTuyenDung = await _context.TinTuyenDungs.AnyAsync(t => t.QuanHuyenId == id);
            bool usedInNguoiDung = await _context.NguoiDungs.AnyAsync(u => u.QuanHuyenId == id);
             if (usedInTinTuyenDung || usedInNguoiDung)
            {
                TempData["ErrorMessage"] = "Không thể xóa Quận/Huyện này vì đang được sử dụng trong Tin tuyển dụng hoặc Hồ sơ người dùng.";
                return RedirectToAction(nameof(QuanHuyen_Index), new { thanhPhoIdFilter = quanHuyen.ThanhPhoId });
            }
            return View("QuanHuyen/QuanHuyen_Delete", quanHuyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/QuanHuyen/Delete/{id}")]
        public async Task<IActionResult> QuanHuyen_DeleteConfirmed(int id)
        {
            var quanHuyen = await _context.QuanHuyens.FindAsync(id);
            if (quanHuyen == null) {
                 TempData["ErrorMessage"] = "Không tìm thấy Quận/Huyện.";
                return RedirectToAction(nameof(QuanHuyen_Index));
            }
            bool usedInTinTuyenDung = await _context.TinTuyenDungs.AnyAsync(t => t.QuanHuyenId == id);
            bool usedInNguoiDung = await _context.NguoiDungs.AnyAsync(u => u.QuanHuyenId == id);
            if (usedInTinTuyenDung || usedInNguoiDung)
            {
                TempData["ErrorMessage"] = "Không thể xóa Quận/Huyện này vì đang được sử dụng.";
                return RedirectToAction(nameof(QuanHuyen_Index), new { thanhPhoIdFilter = quanHuyen.ThanhPhoId });
            }
            try
            {
                _context.QuanHuyens.Remove(quanHuyen);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Quận/Huyện thành công!";
            }
            catch(DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa Quận/Huyện. Có thể vẫn còn dữ liệu liên quan hoặc lỗi cơ sở dữ liệu.";
            }
            return RedirectToAction(nameof(QuanHuyen_Index), new { thanhPhoIdFilter = quanHuyen.ThanhPhoId });
        }

        private bool QuanHuyenExists(int id) => _context.QuanHuyens.Any(e => e.Id == id);

        #endregion

        #region Enum Information (LoaiHinhCongViec, BuoiLamViec)
        
        // Helper class for Enum display
        public class EnumDisplay
        {
            public string ValueString { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string? Description { get; set; } // For BuoiLamViec timeframes
        }

        // Helper to get DisplayName attribute from enum
        private string GetEnumDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()? // Use FirstOrDefault to avoid exception if enum member not found (should not happen for valid enums)
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? enumValue.ToString();
        }

        [Route("Admin/CauHinhHeThong/LoaiHinhCongViec")]
        public IActionResult LoaiHinhCongViec_View()
        {
            ViewData["Title"] = "Danh Sách Loại Hình Công Việc";
            var loaiHinhCongViecs = Enum.GetValues(typeof(LoaiHinhCongViec))
                                        .Cast<LoaiHinhCongViec>()
                                        .Select(e => new EnumDisplay { 
                                            ValueString = e.ToString(), 
                                            DisplayName = GetEnumDisplayName(e) 
                                        })
                                        .ToList();
            return View("Enum_View", loaiHinhCongViecs);
        }

        [Route("Admin/CauHinhHeThong/BuoiLamViec")]
        public IActionResult BuoiLamViec_View()
        {
            ViewData["Title"] = "Danh Sách Buổi Làm Việc (Ca)";
            
            // These are examples. For real configuration, store these in appsettings.json or a database table.
            var khungGioGoiY = new Dictionary<BuoiLamViec, string> {
               { BuoiLamViec.sang, "Thường là 07:00/08:00 - 11:00/12:00" },
               { BuoiLamViec.chieu, "Thường là 13:00/13:30 - 17:00/17:30" },
               { BuoiLamViec.toi, "Thường là 18:00 - 21:00/22:00" },
               { BuoiLamViec.cangay, "Bao gồm cả ca sáng và chiều, có thể có nghỉ trưa." },
               { BuoiLamViec.linhhoat, "Thời gian làm việc không cố định, theo thỏa thuận." }
            };

            var buoiLamViecs = Enum.GetValues(typeof(BuoiLamViec))
                                   .Cast<BuoiLamViec>()
                                   .Select(e => new EnumDisplay { 
                                       ValueString = e.ToString(), 
                                       DisplayName = GetEnumDisplayName(e),
                                       Description = khungGioGoiY.ContainsKey(e) ? khungGioGoiY[e] : "Không có gợi ý khung giờ."
                                   })
                                   .ToList();
            
            return View("Enum_View", buoiLamViecs);
        }

        #endregion

        #region System Settings (Placeholder)

        [Route("Admin/CauHinhHeThong/ThietLapHeThong")]
        public IActionResult ThietLapHeThong_View()
        {
            ViewData["Title"] = "Thiết Lập Hệ Thống";
            // This would load settings from a config file, database table, etc.
            TempData["InfoMessage"] = "Chức năng Thiết lập Hệ thống chung đang được hoạch định và phát triển.";
            return View("ThietLapHeThong_View"); 
        }
        
        // Example of how saving might look if you had a ViewModel for settings
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CauHinhHeThong/ThietLapHeThong")]
        public async Task<IActionResult> ThietLapHeThong_Save(YourSystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // await _settingsService.SaveSystemSettings(model);
                TempData["SuccessMessage"] = "Cài đặt hệ thống đã được lưu.";
                return RedirectToAction(nameof(ThietLapHeThong_View));
            }
            TempData["ErrorMessage"] = "Lưu cài đặt thất bại.";
            return View("ThietLapHeThong_View", model);
        }
        */

        #endregion
    }
}