using HeThongTimViec.Data;
using HeThongTimViec.Extensions;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.JobPosting;
using HeThongTimViec.ViewModels.TimViec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    /// <summary>
    /// Controller quản lý CRUD tin tuyển dụng cho Nhà tuyển dụng Doanh nghiệp.
    /// Yêu cầu quyền 'doanhnghiep'.
    /// Tin đăng/sửa/đăng lại bởi Doanh nghiệp sẽ phải qua quy trình chờ duyệt.
    /// </summary>
    [Authorize(Roles = nameof(LoaiTaiKhoan.doanhnghiep))]
    public class CompanyPostingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyPostingController> _logger;

        public CompanyPostingController(ApplicationDbContext context, ILogger<CompanyPostingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region --- Hàm Helper ---

        private bool TryGetUserId(out int userId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out userId))
            {
                return true;
            }
            _logger.LogError("Không thể parse UserId từ claims cho NTD Doanh nghiệp.");
            userId = 0;
            return false;
        }

        private async Task PrepareFilterDropdownsAsync(JobPostingFilterViewModel filter)
        {
            ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", filter.ThanhPhoId);
            ViewBag.NganhNgheList = new SelectList(await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten).ToListAsync(), "Id", "Ten", filter.NganhNgheId);
            if (filter.ThanhPhoId.HasValue && filter.ThanhPhoId > 0)
            {
                ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens.AsNoTracking().Where(qh => qh.ThanhPhoId == filter.ThanhPhoId).OrderBy(qh => qh.Ten).ToListAsync(), "Id", "Ten", filter.QuanHuyenId);
            }
            else
            {
                ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
            }
            ViewBag.TrangThaiSelectList = filter.TrangThai.ToSelectList(includeDefaultItem: true, defaultItemText: "Tất cả trạng thái");
        }
        
        private async Task PrepareFormDropdownsAsync(int? selectedThanhPhoId = null)
        {
            ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", selectedThanhPhoId);
            ViewBag.NganhNgheList = await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten).Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Ten }).ToListAsync();
            ViewBag.LoaiHinhCongViecList = EnumExtensions.GetSelectList<LoaiHinhCongViec>();
            ViewBag.LoaiLuongList = EnumExtensions.GetSelectList<LoaiLuong>();
            ViewBag.NgayTrongTuanList = EnumExtensions.GetSelectList<NgayTrongTuan>();
            ViewBag.BuoiLamViecList = EnumExtensions.GetSelectList<BuoiLamViec>();
            ViewBag.QuanHuyenList = selectedThanhPhoId.HasValue 
                ? new SelectList(await _context.QuanHuyens.AsNoTracking().Where(q => q.ThanhPhoId == selectedThanhPhoId).OrderBy(q => q.Ten).ToListAsync(), "Id", "Ten")
                : new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
        }

        #endregion

        #region --- Actions chính (Index, Create, Edit, Details) ---

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] JobPostingFilterViewModel filter, int? pageNumber)
        {
            if (!TryGetUserId(out int userId)) return Forbid("Không thể xác thực người dùng.");

            var query = _context.TinTuyenDungs
                .Where(t => t.NguoiDangId == userId && t.TrangThai != TrangThaiTinTuyenDung.daxoa)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword)) query = query.Where(t => t.TieuDe.ToLower().Contains(filter.Keyword.Trim().ToLower()));
            if (filter.ThanhPhoId.HasValue) query = query.Where(t => t.ThanhPhoId == filter.ThanhPhoId.Value);
            if (filter.QuanHuyenId.HasValue) query = query.Where(t => t.QuanHuyenId == filter.QuanHuyenId.Value);
            if (filter.NganhNgheId.HasValue) query = query.Where(t => t.TinTuyenDungNganhNghes.Any(tnn => tnn.NganhNgheId == filter.NganhNgheId.Value));
            if (filter.TrangThai.HasValue) query = query.Where(t => t.TrangThai == filter.TrangThai.Value);

            var orderedQuery = query.OrderByDescending(t => t.NgayCapNhat)
                                    .Select(t => new JobPostingListViewModel
                                    {
                                        Id = t.Id, TieuDe = t.TieuDe, TrangThai = t.TrangThai,
                                        NgayDang = t.NgayDang, NgayHetHan = t.NgayHetHan, SoUngVien = t.UngTuyens.Count
                                    });

            var paginatedPostings = await PaginatedList<JobPostingListViewModel>.CreateAsync(orderedQuery, pageNumber ?? 1, 10);
            await PrepareFilterDropdownsAsync(filter);
            
            var viewModel = new JobPostingIndexViewModel { Filter = filter, Postings = paginatedPostings };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!TryGetUserId(out _)) return Forbid();
            var viewModel = new IndividualPostingViewModel(); // Dùng chung VM
            viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel());
            await PrepareFormDropdownsAsync();
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IndividualPostingViewModel viewModel)
        {
            if (!TryGetUserId(out int userId)) return Forbid();
            
            viewModel.LichLamViecItems.RemoveAll(l => l.NgayTrongTuan == 0 && l.GioBatDau == null && l.GioKetThuc == null && l.BuoiLamViec == null);
            if (viewModel.NgayHetHan.HasValue && viewModel.NgayHetHan.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(viewModel.NgayHetHan), "Ngày hết hạn không được là ngày trong quá khứ.");
            }

            if (ModelState.IsValid)
            {
                var newPosting = new TinTuyenDung
                {
                    NguoiDangId = userId, TieuDe = viewModel.TieuDe, MoTa = viewModel.MoTa, YeuCau = viewModel.YeuCau,
                    QuyenLoi = viewModel.QuyenLoi, LoaiHinhCongViec = viewModel.LoaiHinhCongViec, LoaiLuong = viewModel.LoaiLuong,
                    LuongToiThieu = viewModel.LuongToiThieu, LuongToiDa = viewModel.LuongToiDa, DiaChiLamViec = viewModel.DiaChiLamViec,
                    ThanhPhoId = viewModel.ThanhPhoId, QuanHuyenId = viewModel.QuanHuyenId, YeuCauKinhNghiemText = viewModel.YeuCauKinhNghiemText ?? "Không yêu cầu",
                    YeuCauHocVanText = viewModel.YeuCauHocVanText ?? "Không yêu cầu", SoLuongTuyen = viewModel.SoLuongTuyen, TinGap = viewModel.TinGap,
                    NgayHetHan = viewModel.NgayHetHan, NgayDang = DateTime.UtcNow, NgayTao = DateTime.UtcNow, NgayCapNhat = DateTime.UtcNow,
                    TrangThai = TrangThaiTinTuyenDung.choduyet
                };
                
                if (viewModel.SelectedNganhNgheIds != null) foreach (var nnId in viewModel.SelectedNganhNgheIds) newPosting.TinTuyenDungNganhNghes.Add(new TinTuyenDung_NganhNghe { NganhNgheId = nnId });
                if (viewModel.LichLamViecItems != null) foreach (var lichVM in viewModel.LichLamViecItems) newPosting.LichLamViecCongViecs.Add(new LichLamViecCongViec { NgayTrongTuan = lichVM.NgayTrongTuan, GioBatDau = lichVM.GioBatDau, GioKetThuc = lichVM.GioKetThuc, BuoiLamViec = lichVM.BuoiLamViec });
                
                _context.TinTuyenDungs.Add(newPosting);
                await _context.SaveChangesAsync();
                _logger.LogInformation("NTD Doanh nghiệp (User ID: {UserId}) đã tạo Tin Tuyển Dụng mới (ID: {PostingId}) - Chờ duyệt.", userId, newPosting.Id);
                TempData["SuccessMessage"] = "Đăng tin mới thành công! Tin đang chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareFormDropdownsAsync(viewModel.ThanhPhoId);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            if (!TryGetUserId(out int userId)) return Forbid();

            var posting = await _context.TinTuyenDungs.Include(t => t.TinTuyenDungNganhNghes).Include(t => t.LichLamViecCongViecs).FirstOrDefaultAsync(t => t.Id == id);
            
            if (posting == null || posting.NguoiDangId != userId) return Forbid();
            if (posting.TrangThai == TrangThaiTinTuyenDung.daxoa || posting.TrangThai == TrangThaiTinTuyenDung.bituchoi)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa tin đã bị xóa hoặc bị từ chối.";
                return RedirectToAction(nameof(Index));
            }
            
            var viewModel = new IndividualPostingViewModel
            {
                Id = posting.Id, TieuDe = posting.TieuDe, MoTa = posting.MoTa, YeuCau = posting.YeuCau, QuyenLoi = posting.QuyenLoi,
                LoaiHinhCongViec = posting.LoaiHinhCongViec, LoaiLuong = posting.LoaiLuong, LuongToiThieu = posting.LuongToiThieu,
                LuongToiDa = posting.LuongToiDa, DiaChiLamViec = posting.DiaChiLamViec, ThanhPhoId = posting.ThanhPhoId,
                QuanHuyenId = posting.QuanHuyenId, YeuCauKinhNghiemText = posting.YeuCauKinhNghiemText, YeuCauHocVanText = posting.YeuCauHocVanText,
                SoLuongTuyen = posting.SoLuongTuyen, TinGap = posting.TinGap, NgayHetHan = posting.NgayHetHan,
                SelectedNganhNgheIds = posting.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNgheId).ToList(),
                LichLamViecItems = posting.LichLamViecCongViecs.Select(l => new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel { Id = l.Id, NgayTrongTuan = l.NgayTrongTuan, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc, BuoiLamViec = l.BuoiLamViec }).ToList()
            };
            if (!viewModel.LichLamViecItems.Any()) viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel ());
            
            await PrepareFormDropdownsAsync(viewModel.ThanhPhoId);
            ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens.AsNoTracking().Where(q => q.ThanhPhoId == viewModel.ThanhPhoId).OrderBy(q => q.Ten).ToListAsync(), "Id", "Ten", viewModel.QuanHuyenId);
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IndividualPostingViewModel viewModel)
        {
            if (id != viewModel.Id) return BadRequest();
            if (!TryGetUserId(out int userId)) return Forbid();
            
            if (ModelState.IsValid)
            {
                var postingInDb = await _context.TinTuyenDungs.Include(t=>t.TinTuyenDungNganhNghes).Include(t=>t.LichLamViecCongViecs).FirstOrDefaultAsync(t => t.Id == id);
                if(postingInDb == null || postingInDb.NguoiDangId != userId) return Forbid();
                
                // Cập nhật thuộc tính từ viewModel
                postingInDb.TieuDe = viewModel.TieuDe; postingInDb.MoTa = viewModel.MoTa; postingInDb.YeuCau = viewModel.YeuCau; postingInDb.QuyenLoi = viewModel.QuyenLoi;
                postingInDb.LoaiHinhCongViec = viewModel.LoaiHinhCongViec; postingInDb.LoaiLuong = viewModel.LoaiLuong; postingInDb.LuongToiThieu = viewModel.LuongToiThieu;
                postingInDb.LuongToiDa = viewModel.LuongToiDa; postingInDb.DiaChiLamViec = viewModel.DiaChiLamViec; postingInDb.ThanhPhoId = viewModel.ThanhPhoId;
                postingInDb.QuanHuyenId = viewModel.QuanHuyenId; postingInDb.YeuCauKinhNghiemText = viewModel.YeuCauKinhNghiemText ?? "Không yêu cầu";
                postingInDb.YeuCauHocVanText = viewModel.YeuCauHocVanText ?? "Không yêu cầu"; postingInDb.SoLuongTuyen = viewModel.SoLuongTuyen;
                postingInDb.TinGap = viewModel.TinGap; postingInDb.NgayHetHan = viewModel.NgayHetHan; postingInDb.NgayCapNhat = DateTime.UtcNow;

                if (postingInDb.TrangThai == TrangThaiTinTuyenDung.daduyet || postingInDb.TrangThai == TrangThaiTinTuyenDung.taman)
                {
                    postingInDb.TrangThai = TrangThaiTinTuyenDung.choduyet;
                    postingInDb.AdminDuyetId = null; postingInDb.NgayDuyet = null;
                    TempData["InfoMessage"] = "Tin đã được cập nhật và chuyển về trạng thái chờ duyệt.";
                }
                
                // (Thêm logic cập nhật Ngành nghề và Lịch làm việc ở đây nếu cần)

                await _context.SaveChangesAsync();
                if (TempData["InfoMessage"] == null) TempData["SuccessMessage"] = "Cập nhật tin thành công.";
                return RedirectToAction(nameof(Index));
            }

            await PrepareFormDropdownsAsync(viewModel.ThanhPhoId);
            return View(viewModel);
        }
        
[HttpGet]
public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();
    if (!TryGetUserId(out int userId)) return Forbid();

    var posting = await _context.TinTuyenDungs
        .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep) 
        .Include(t => t.ThanhPho).Include(t => t.QuanHuyen)
        .Include(t => t.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
        .Include(t => t.LichLamViecCongViecs)
        .AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    if (posting == null || posting.NguoiDangId != userId) return Forbid();

    // ======================================================================
    // ===          BỔ SUNG CÁC HÀM HELPER ĐỊNH DẠNG DỮ LIỆU            ===
    // ======================================================================

    // Helper function để tạo chuỗi lương
    string GetMucLuongDisplay(TinTuyenDung tin)
    {
        if (tin.LoaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";

        string luongMinStr = tin.LuongToiThieu.HasValue ? $"{tin.LuongToiThieu:N0}" : "";
        string luongMaxStr = tin.LuongToiDa.HasValue ? $"{tin.LuongToiDa:N0}" : "";

        if (!string.IsNullOrEmpty(luongMinStr) && !string.IsNullOrEmpty(luongMaxStr))
            return $"{luongMinStr} - {luongMaxStr} VNĐ";
        if (!string.IsNullOrEmpty(luongMinStr))
            return $"Từ {luongMinStr} VNĐ";
        if (!string.IsNullOrEmpty(luongMaxStr))
            return $"Đến {luongMaxStr} VNĐ";

        return "Chưa cập nhật";
    }

    // Helper function để tạo chuỗi địa điểm
    string GetDiaDiemDisplay(TinTuyenDung tin)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrWhiteSpace(tin.DiaChiLamViec)) parts.Add(tin.DiaChiLamViec);
        if (tin.QuanHuyen != null) parts.Add(tin.QuanHuyen.Ten);
        if (tin.ThanhPho != null) parts.Add(tin.ThanhPho.Ten);
        return string.Join(", ", parts);
    }

    // Tạo ViewModel để truyền sang View
    var viewModel = new ChiTietTuyenDungViewModel
    {
        Id = posting.Id, TieuDe = posting.TieuDe, MoTa = posting.MoTa, YeuCau = posting.YeuCau, 
        QuyenLoi = posting.QuyenLoi, TinGap = posting.TinGap,
        NguoiDangId = posting.NguoiDangId,
        TenNguoiDang = posting.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? posting.NguoiDang.HoTen,
        LogoUrl = posting.NguoiDang.HoSoDoanhNghiep?.UrlLogo,
        LoaiTkNguoiDang = posting.NguoiDang.LoaiTk,
        
        // Sửa các dòng này để gọi hàm helper
        MucLuongDisplay = GetMucLuongDisplay(posting),
        LoaiHinhDisplay = posting.LoaiHinhCongViec.GetDisplayName(),
        DiaDiemLamViec = GetDiaDiemDisplay(posting),

        YeuCauKinhNghiemText = posting.YeuCauKinhNghiemText, 
        YeuCauHocVanText = posting.YeuCauHocVanText, 
        SoLuongTuyen = posting.SoLuongTuyen,
        NgayDang = posting.NgayDang, NgayHetHan = posting.NgayHetHan,
        NganhNghes = posting.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
        LichLamViecs = posting.LichLamViecCongViecs.Select(l => new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel { Id = l.Id, NgayTrongTuan = l.NgayTrongTuan, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc, BuoiLamViec = l.BuoiLamViec }).ToList(),
        IsSaved = false, IsApplied = false
    };
    
    return View(viewModel);
}

#endregion

        #region --- Actions phụ (Repost, Delete, Toggle, MarkFilled) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RepostQuick(int id, DateTime? newExpiryDate)
        {
            if (!TryGetUserId(out int userId)) return Forbid();
            
            var originalPosting = await _context.TinTuyenDungs.AsNoTracking().Include(t=>t.TinTuyenDungNganhNghes).Include(t=>t.LichLamViecCongViecs).FirstOrDefaultAsync(t => t.Id == id);
            if (originalPosting == null || originalPosting.NguoiDangId != userId) return Forbid();
            if (newExpiryDate.HasValue && newExpiryDate.Value.Date < DateTime.UtcNow.Date)
            {
                TempData["ErrorMessage"] = "Ngày hết hạn mới không được là ngày trong quá khứ.";
                return RedirectToAction(nameof(Index));
            }

            var newPosting = new TinTuyenDung 
            {
                 NguoiDangId = originalPosting.NguoiDangId, TieuDe = originalPosting.TieuDe, MoTa = originalPosting.MoTa, YeuCau = originalPosting.YeuCau,
                 QuyenLoi = originalPosting.QuyenLoi, LoaiHinhCongViec = originalPosting.LoaiHinhCongViec, LoaiLuong = originalPosting.LoaiLuong,
                 LuongToiThieu = originalPosting.LuongToiThieu, LuongToiDa = originalPosting.LuongToiDa, DiaChiLamViec = originalPosting.DiaChiLamViec,
                 ThanhPhoId = originalPosting.ThanhPhoId, QuanHuyenId = originalPosting.QuanHuyenId, YeuCauKinhNghiemText = originalPosting.YeuCauKinhNghiemText,
                 YeuCauHocVanText = originalPosting.YeuCauHocVanText, SoLuongTuyen = originalPosting.SoLuongTuyen, TinGap = originalPosting.TinGap,
                 NgayHetHan = newExpiryDate, NgayDang = DateTime.UtcNow, NgayTao = DateTime.UtcNow, NgayCapNhat = DateTime.UtcNow,
                 AdminDuyetId = null, NgayDuyet = null,
                 TrangThai = TrangThaiTinTuyenDung.choduyet
            };
            newPosting.LichLamViecCongViecs = originalPosting.LichLamViecCongViecs.Select(l => new LichLamViecCongViec { NgayTrongTuan = l.NgayTrongTuan, BuoiLamViec = l.BuoiLamViec, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc }).ToList();
            newPosting.TinTuyenDungNganhNghes = originalPosting.TinTuyenDungNganhNghes.Select(n => new TinTuyenDung_NganhNghe { NganhNgheId = n.NganhNgheId }).ToList();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.TinTuyenDungs.Add(newPosting);
                    await _context.SaveChangesAsync();
                    
                    var postingToDelete = await _context.TinTuyenDungs.FindAsync(originalPosting.Id);
                    if (postingToDelete != null)
                    {
                        _context.TinTuyenDungs.Remove(postingToDelete);
                        await _context.SaveChangesAsync();
                    }
                    
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = $"Đã đăng lại tin '{newPosting.TieuDe}'. Tin đang chờ duyệt và tin cũ đã được xóa.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi Company RepostQuick cho tin {id}", id);
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng lại.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!TryGetUserId(out int userId)) return Forbid();
            var posting = await _context.TinTuyenDungs.FindAsync(id);
            if (posting == null || posting.NguoiDangId != userId) return Forbid();
            
            posting.TrangThai = TrangThaiTinTuyenDung.daxoa; // Xóa logic
            posting.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("NTD Doanh nghiệp (User ID: {UserId}) đã xóa (logic) Tin Tuyển Dụng (ID: {PostingId})", userId, id);
            TempData["SuccessMessage"] = $"Đã xóa thành công tin tuyển dụng '{posting.TieuDe}'.";
            return RedirectToAction(nameof(Index));
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleVisibility(int id)
{
    if (!TryGetUserId(out int userId)) return Forbid();
    var posting = await _context.TinTuyenDungs.FindAsync(id);
    if (posting == null || posting.NguoiDangId != userId) return Forbid();

    // Chỉ cho phép ẩn/hiện các tin đã được duyệt hoặc đang tạm ẩn
    if (posting.TrangThai != TrangThaiTinTuyenDung.daduyet && posting.TrangThai != TrangThaiTinTuyenDung.taman)
    {
        TempData["InfoMessage"] = "Chỉ có thể ẩn hoặc hiện lại các tin đang ở trạng thái 'Đã duyệt' hoặc 'Tạm ẩn'.";
        return RedirectToAction(nameof(Index));
    }
    
    if (posting.TrangThai == TrangThaiTinTuyenDung.daduyet)
    {
        // Nếu đang hiển thị -> Chuyển sang Tạm ẩn
        posting.TrangThai = TrangThaiTinTuyenDung.taman;
        TempData["SuccessMessage"] = "Đã ẩn tin thành công.";
        _logger.LogInformation("NTD Doanh nghiệp (User ID: {UserId}) đã ẩn tin {PostingId}", userId, id);
    }
    else // Logic này chỉ chạy khi posting.TrangThai == TrangThaiTinTuyenDung.taman
    {
        // === QUAY LẠI LOGIC CŨ: HIỆN LẠI PHẢI CHỜ DUYỆT ===
        // Nếu đang tạm ẩn -> Chuyển về Chờ duyệt
        posting.TrangThai = TrangThaiTinTuyenDung.daduyet;
        // Xóa thông tin duyệt cũ để admin có thể duyệt lại
        posting.AdminDuyetId = null;
        posting.NgayDuyet = null;
        TempData["InfoMessage"] = "Yêu cầu hiển thị lại đã được gửi. Tin đang chờ duyệt.";
        _logger.LogInformation("NTD Doanh nghiệp (User ID: {UserId}) đã yêu cầu hiện lại tin {PostingId}, chuyển sang chờ duyệt.", userId, id);
    }
    
    posting.NgayCapNhat = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    
    return RedirectToAction(nameof(Index));
}
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFilled(int id)
        {
            if (!TryGetUserId(out int userId)) return Forbid();
            var posting = await _context.TinTuyenDungs.FindAsync(id);
            if (posting == null || posting.NguoiDangId != userId) return Forbid();

            if (posting.TrangThai == TrangThaiTinTuyenDung.daduyet || posting.TrangThai == TrangThaiTinTuyenDung.taman)
            {
                posting.TrangThai = TrangThaiTinTuyenDung.datuyen;
                posting.NgayCapNhat = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã đánh dấu tin '{posting.TieuDe}' là đã tuyển thành công.";
            }
            else
            {
                TempData["InfoMessage"] = "Chỉ có thể đánh dấu đã tuyển cho các tin đang hoạt động.";
            }
            return RedirectToAction(nameof(Index));
        }
        
        #endregion
    }
}