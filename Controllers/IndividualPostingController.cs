using HeThongTimViec.Data;
using HeThongTimViec.Extensions;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.JobPosting; // Đảm bảo using ViewModel nếu cần
using HeThongTimViec.ViewModels.TimViec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Cần cho IHttpContextAccessor
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    // <summary>
    /// Controller quản lý CRUD tin tuyển dụng cho Nhà tuyển dụng Cá nhân.
    /// Yêu cầu quyền 'canhan' và đang ở chế độ xem NTD (Session["DangLaNTD"] == 1).
    /// Tin đăng và chỉnh sửa bởi NTD Cá nhân sẽ luôn ở trạng thái duyệt (không cần chờ).
    /// </summary>
    [Authorize(Roles = nameof(LoaiTaiKhoan.canhan))] // Chỉ role 'canhan'
    public class IndividualPostingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndividualPostingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IndividualPostingController(ApplicationDbContext context, ILogger<IndividualPostingController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // --- Hàm Helper ---

        private bool IsIndividualEmployerViewActive() => _httpContextAccessor.HttpContext?.Session?.GetInt32("DangLaNTD") == 1;

        private bool CheckAccess(out int userId)
        {
            userId = 0;
            if (!IsIndividualEmployerViewActive())
            {
                _logger.LogWarning("User không ở chế độ NTD Cá nhân cố gắng truy cập IndividualPostingController.");
                return false;
            }
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out userId))
            {
                _logger.LogError("Không thể parse UserId từ claims cho NTD Cá nhân.");
                return false;
            }
            return true;
        }

        private async Task PrepareDropdownsAsync(int? selectedThanhPhoId = null, int? selectedQuanHuyenId = null)
        {
            ViewBag.NganhNgheList = await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten)
               .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Ten }).ToListAsync();
            ViewBag.ThanhPhoList = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(),
                                                  "Id", "Ten", selectedThanhPhoId);
            if (selectedThanhPhoId.HasValue && selectedThanhPhoId > 0)
            {
                ViewBag.QuanHuyenList = new SelectList(await _context.QuanHuyens.AsNoTracking()
                   .Where(qh => qh.ThanhPhoId == selectedThanhPhoId).OrderBy(qh => qh.Ten).ToListAsync(),
                                                        "Id", "Ten", selectedQuanHuyenId);
            }
            else { ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten"); }
            ViewBag.LoaiHinhCongViecList = new SelectList(Enum.GetValues(typeof(LoaiHinhCongViec)).Cast<LoaiHinhCongViec>().Select(e => new SelectListItem { Value = e.ToString(), Text = GetEnumDisplayName(e) }), "Value", "Text");
            ViewBag.LoaiLuongList = new SelectList(Enum.GetValues(typeof(LoaiLuong)).Cast<LoaiLuong>().Select(e => new SelectListItem { Value = e.ToString(), Text = GetEnumDisplayName(e) }), "Value", "Text");
            ViewBag.NgayTrongTuanList = new SelectList(Enum.GetValues(typeof(NgayTrongTuan)).Cast<NgayTrongTuan>().Select(e => new SelectListItem { Value = e.ToString(), Text = GetEnumDisplayName(e) }), "Value", "Text");
            ViewBag.BuoiLamViecList = new SelectList(Enum.GetValues(typeof(BuoiLamViec)).Cast<BuoiLamViec>().Select(e => new SelectListItem { Value = e.ToString(), Text = GetEnumDisplayName(e) }), "Value", "Text");
        }

        private string GetEnumDisplayName(Enum enumValue)
        {
            switch (enumValue)
            {
                case LoaiHinhCongViec.banthoigian: return "Bán thời gian";
                case LoaiHinhCongViec.thoivu: return "Thời vụ";
                case LoaiHinhCongViec.linhhoatkhac: return "Linh hoạt khác";
                case LoaiLuong.theogio: return "Theo giờ";
                case LoaiLuong.theongay: return "Theo ngày";
                case LoaiLuong.theoca: return "Theo ca";
                case LoaiLuong.theothang: return "Theo tháng";
                case LoaiLuong.thoathuan: return "Thỏa thuận";
                case LoaiLuong.theoduan: return "Theo dự án";
                case NgayTrongTuan.thu2: return "Thứ 2";
                case NgayTrongTuan.thu3: return "Thứ 3";
                case NgayTrongTuan.thu4: return "Thứ 4";
                case NgayTrongTuan.thu5: return "Thứ 5";
                case NgayTrongTuan.thu6: return "Thứ 6";
                case NgayTrongTuan.thu7: return "Thứ 7";
                case NgayTrongTuan.chunhat: return "Chủ Nhật";
                case NgayTrongTuan.ngaylinhhoat: return "Ngày linh hoạt";
                case BuoiLamViec.sang: return "Buổi Sáng";
                case BuoiLamViec.chieu: return "Buổi Chiều";
                case BuoiLamViec.toi: return "Buổi Tối";
                case BuoiLamViec.cangay: return "Cả ngày";
                case BuoiLamViec.linhhoat: return "Buổi linh hoạt";
                default: return enumValue.ToString();
            }
        }

        // --- CRUD Actions ---

        // GET: /IndividualPosting
        public async Task<IActionResult> Index()
        {
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var postings = await _context.TinTuyenDungs
                                     .Where(t => t.NguoiDangId == userId && t.TrangThai != TrangThaiTinTuyenDung.daxoa)
                                     .OrderByDescending(t => t.NgayCapNhat)
                                     .Select(t => new JobPostingListViewModel
                                     {
                                         Id = t.Id,
                                         TieuDe = t.TieuDe,
                                         TrangThai = t.TrangThai,
                                         NgayDang = t.NgayDang,
                                         NgayHetHan = t.NgayHetHan,
                                         SoUngVien = t.UngTuyens.Count
                                     })
                                     .AsNoTracking().ToListAsync();
            return View(postings);
        }

        // GET: /IndividualPosting/Create
        public async Task<IActionResult> Create()
        {
            if (!CheckAccess(out _)) return RedirectToAction("Index", "Dashboard");
            await PrepareDropdownsAsync();
            var viewModel = new IndividualPostingViewModel();
            viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel());
            return View(viewModel);
        }

        // POST: /IndividualPosting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IndividualPostingViewModel viewModel)
        {
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            viewModel.LichLamViecItems.RemoveAll(l => l.NgayTrongTuan == 0 && l.GioBatDau == null && l.GioKetThuc == null && l.BuoiLamViec == null);
            if (viewModel.ThanhPhoId <= 0) { ModelState.Remove(nameof(viewModel.QuanHuyenId)); }
            if (viewModel.NgayHetHan.HasValue && viewModel.NgayHetHan.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(viewModel.NgayHetHan), "Ngày hết hạn không được là ngày trong quá khứ.");
            }

            if (ModelState.IsValid)
            {
                var newPosting = new TinTuyenDung
                {
                    NguoiDangId = userId,
                    TieuDe = viewModel.TieuDe,
                    MoTa = viewModel.MoTa,
                    YeuCau = viewModel.YeuCau,
                    QuyenLoi = viewModel.QuyenLoi,
                    LoaiHinhCongViec = viewModel.LoaiHinhCongViec,
                    LoaiLuong = viewModel.LoaiLuong,
                    LuongToiThieu = viewModel.LuongToiThieu,
                    LuongToiDa = viewModel.LuongToiDa,
                    DiaChiLamViec = viewModel.DiaChiLamViec,
                    ThanhPhoId = viewModel.ThanhPhoId,
                    QuanHuyenId = viewModel.QuanHuyenId,
                    YeuCauKinhNghiemText = viewModel.YeuCauKinhNghiemText ?? "Không yêu cầu",
                    YeuCauHocVanText = viewModel.YeuCauHocVanText ?? "Không yêu cầu",
                    SoLuongTuyen = viewModel.SoLuongTuyen,
                    TinGap = viewModel.TinGap,
                    NgayHetHan = viewModel.NgayHetHan,
                    TrangThai = TrangThaiTinTuyenDung.daduyet,
                    NgayDuyet = DateTime.UtcNow,
                    AdminDuyetId = null,
                    NgayDang = DateTime.UtcNow,
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow
                };

                if (viewModel.SelectedNganhNgheIds?.Any() ?? false) { foreach (var nnId in viewModel.SelectedNganhNgheIds) { if (await _context.NganhNghes.AnyAsync(n => n.Id == nnId)) { newPosting.TinTuyenDungNganhNghes.Add(new TinTuyenDung_NganhNghe { NganhNgheId = nnId }); } } }
                if (viewModel.LichLamViecItems?.Any() ?? false) { foreach (var lichVM in viewModel.LichLamViecItems) { if (lichVM.NgayTrongTuan != 0 || lichVM.BuoiLamViec.HasValue || lichVM.GioBatDau.HasValue || lichVM.GioKetThuc.HasValue) { newPosting.LichLamViecCongViecs.Add(new LichLamViecCongViec { NgayTrongTuan = lichVM.NgayTrongTuan, GioBatDau = lichVM.GioBatDau, GioKetThuc = lichVM.GioKetThuc, BuoiLamViec = lichVM.BuoiLamViec }); } } }

                try
                {
                    _context.TinTuyenDungs.Add(newPosting); await _context.SaveChangesAsync();
                    _logger.LogInformation("NTD Cá nhân (User ID: {UserId}) đã tạo Tin Tuyển Dụng mới (ID: {PostingId}) - Tự động duyệt.", userId, newPosting.Id);
                    TempData["SuccessMessage"] = "Đăng tin tuyển dụng mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Lỗi DB khi NTD Cá nhân (User ID: {UserId}) tạo tin.", userId);
                    ModelState.AddModelError("", "Lỗi cơ sở dữ liệu khi lưu tin.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi không xác định khi NTD Cá nhân (User ID: {UserId}) tạo tin.", userId);
                    ModelState.AddModelError("", "Đã có lỗi không mong muốn xảy ra khi lưu tin.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi NTD Cá nhân (User ID: {UserId}) tạo tin. Lỗi: {Errors}", userId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            await PrepareDropdownsAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId);
            if (!(viewModel.LichLamViecItems?.Any() ?? false))
            {
                if (viewModel.LichLamViecItems == null) viewModel.LichLamViecItems = new List<HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel>();
                viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel());
            }
            return View(viewModel);
        }

        // GET: /IndividualPosting/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0) return NotFound("ID không hợp lệ.");
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var posting = await _context.TinTuyenDungs
                                    .Include(t => t.TinTuyenDungNganhNghes)
                                    .Include(t => t.LichLamViecCongViecs)
                                    .FirstOrDefaultAsync(t => t.Id == id);

            if (posting == null) return NotFound($"Không tìm thấy tin tuyển dụng với ID {id}.");
            if (posting.NguoiDangId != userId)
            {
                _logger.LogWarning("User {UserId} cố gắng chỉnh sửa tin {PostingId} không thuộc sở hữu.", userId, id);
                return Forbid("Bạn không có quyền chỉnh sửa tin tuyển dụng này.");
            }
            if (posting.TrangThai == TrangThaiTinTuyenDung.daxoa || posting.TrangThai == TrangThaiTinTuyenDung.bituchoi)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa tin đã bị xóa hoặc bị từ chối.";
                return RedirectToAction(nameof(Index));
            }
            if (posting.TrangThai == TrangThaiTinTuyenDung.datuyen)
            {
                TempData["InfoMessage"] = "Tin đã tuyển đủ người, một số thông tin có thể bị hạn chế chỉnh sửa.";
            }

            var viewModel = new IndividualPostingViewModel
            {
                Id = posting.Id,
                TieuDe = posting.TieuDe,
                MoTa = posting.MoTa,
                YeuCau = posting.YeuCau,
                QuyenLoi = posting.QuyenLoi,
                LoaiHinhCongViec = posting.LoaiHinhCongViec,
                LoaiLuong = posting.LoaiLuong,
                LuongToiThieu = posting.LuongToiThieu,
                LuongToiDa = posting.LuongToiDa,
                DiaChiLamViec = posting.DiaChiLamViec,
                ThanhPhoId = posting.ThanhPhoId,
                QuanHuyenId = posting.QuanHuyenId,
                YeuCauKinhNghiemText = posting.YeuCauKinhNghiemText,
                YeuCauHocVanText = posting.YeuCauHocVanText,
                SoLuongTuyen = posting.SoLuongTuyen,
                TinGap = posting.TinGap,
                NgayHetHan = posting.NgayHetHan,
                SelectedNganhNgheIds = posting.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNgheId).ToList(),
                LichLamViecItems = posting.LichLamViecCongViecs.Select(l => new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel { Id = l.Id, NgayTrongTuan = l.NgayTrongTuan, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc, BuoiLamViec = l.BuoiLamViec, MarkedForDeletion = false }).ToList()
            };

            await PrepareDropdownsAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId);
            if (!viewModel.LichLamViecItems.Any()) viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel());
            return View(viewModel);
        }

        // POST: /IndividualPosting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IndividualPostingViewModel viewModel)
        {
            if (id != viewModel.Id) return BadRequest("ID không khớp.");
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            viewModel.LichLamViecItems.RemoveAll(l => l.Id == null && l.NgayTrongTuan == 0 && l.GioBatDau == null && l.GioKetThuc == null && l.BuoiLamViec == null);
            if (viewModel.NgayHetHan.HasValue && viewModel.NgayHetHan.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(viewModel.NgayHetHan), "Ngày hết hạn không được là ngày trong quá khứ.");
            }

            if (ModelState.IsValid)
            {
                var postingInDb = await _context.TinTuyenDungs
                                        .Include(t => t.TinTuyenDungNganhNghes)
                                        .Include(t => t.LichLamViecCongViecs)
                                        .FirstOrDefaultAsync(t => t.Id == id);

                if (postingInDb == null) return NotFound($"Không tìm thấy tin tuyển dụng ID {id}.");
                if (postingInDb.NguoiDangId != userId)
                {
                    _logger.LogWarning("User {UserId} cố gắng POST chỉnh sửa tin {PostingId} không thuộc sở hữu.", userId, id);
                    return Forbid("Bạn không có quyền chỉnh sửa tin này.");
                }
                if (postingInDb.TrangThai == TrangThaiTinTuyenDung.daxoa || postingInDb.TrangThai == TrangThaiTinTuyenDung.bituchoi)
                {
                    TempData["ErrorMessage"] = "Không thể chỉnh sửa tin đã bị xóa hoặc bị từ chối.";
                    return RedirectToAction(nameof(Index));
                }
                if (postingInDb.TrangThai == TrangThaiTinTuyenDung.datuyen)
                {
                    TempData["InfoMessage"] = "Tin đã tuyển đủ người.";
                }

                // --- Cập nhật thuộc tính chính ---
                postingInDb.TieuDe = viewModel.TieuDe; postingInDb.MoTa = viewModel.MoTa; postingInDb.YeuCau = viewModel.YeuCau; postingInDb.QuyenLoi = viewModel.QuyenLoi;
                postingInDb.LoaiHinhCongViec = viewModel.LoaiHinhCongViec; postingInDb.LoaiLuong = viewModel.LoaiLuong; postingInDb.LuongToiThieu = viewModel.LuongToiThieu; postingInDb.LuongToiDa = viewModel.LuongToiDa;
                postingInDb.DiaChiLamViec = viewModel.DiaChiLamViec; postingInDb.ThanhPhoId = viewModel.ThanhPhoId; postingInDb.QuanHuyenId = viewModel.QuanHuyenId;
                postingInDb.YeuCauKinhNghiemText = viewModel.YeuCauKinhNghiemText ?? "Không yêu cầu"; postingInDb.YeuCauHocVanText = viewModel.YeuCauHocVanText ?? "Không yêu cầu";
                postingInDb.SoLuongTuyen = viewModel.SoLuongTuyen; postingInDb.TinGap = viewModel.TinGap; postingInDb.NgayHetHan = viewModel.NgayHetHan;
                postingInDb.NgayCapNhat = DateTime.UtcNow;

                // *** LOGIC QUAN TRỌNG: KHÔNG ĐỔI TRẠNG THÁI KHI SỬA ***
                // Giữ nguyên trạng thái (daduyet hoặc taman) khi NTD Cá nhân sửa.
                _logger.LogInformation("NTD Cá nhân {UserId} cập nhật tin {PostingId}. Trạng thái hiện tại ({CurrentStatus}) được giữ nguyên.", userId, id, postingInDb.TrangThai);

                // --- Xử lý cập nhật Ngành nghề (Many-to-Many) ---
                var currentSelectedIdsNN = viewModel.SelectedNganhNgheIds ?? new List<int>();
                var existingEntriesNN = postingInDb.TinTuyenDungNganhNghes.ToList();
                var entriesToRemoveNN = existingEntriesNN.Where(e => !currentSelectedIdsNN.Contains(e.NganhNgheId)).ToList();
                _context.TinTuyenDung_NganhNghes.RemoveRange(entriesToRemoveNN);
                var existingIdsNN = existingEntriesNN.Select(e => e.NganhNgheId);
                var idsToAddNN = currentSelectedIdsNN.Except(existingIdsNN).ToList();
                foreach (var nnId in idsToAddNN)
                {
                    if (await _context.NganhNghes.AnyAsync(n => n.Id == nnId))
                    {
                        postingInDb.TinTuyenDungNganhNghes.Add(new TinTuyenDung_NganhNghe { NganhNgheId = nnId });
                    }
                }

                // --- Xử lý cập nhật Lịch làm việc (One-to-Many) ---
                var currentVmItemsL = viewModel.LichLamViecItems ?? new List<HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel>();
                var existingDbEntriesL = postingInDb.LichLamViecCongViecs.ToList();
                var vmIdsToKeepL = currentVmItemsL.Where(vm => vm.Id.HasValue && !vm.MarkedForDeletion).Select(vm => vm.Id!.Value).ToList();
                var dbEntriesToRemoveL = existingDbEntriesL.Where(db => !vmIdsToKeepL.Contains(db.Id)).ToList();
                _context.LichLamViecCongViecs.RemoveRange(dbEntriesToRemoveL);
                foreach (var lichVM in currentVmItemsL)
                {
                    if (!lichVM.MarkedForDeletion)
                    {
                        if (lichVM.Id.HasValue && lichVM.Id > 0)
                        { // Cập nhật dòng cũ
                            var lichInDb = existingDbEntriesL.FirstOrDefault(l => l.Id == lichVM.Id.Value);
                            if (lichInDb != null)
                            {
                                lichInDb.NgayTrongTuan = lichVM.NgayTrongTuan; lichInDb.GioBatDau = lichVM.GioBatDau;
                                lichInDb.GioKetThuc = lichVM.GioKetThuc; lichInDb.BuoiLamViec = lichVM.BuoiLamViec;
                            }
                        }
                        else
                        { // Thêm dòng mới
                            if (lichVM.NgayTrongTuan != 0 || lichVM.BuoiLamViec.HasValue || lichVM.GioBatDau.HasValue || lichVM.GioKetThuc.HasValue)
                            {
                                postingInDb.LichLamViecCongViecs.Add(new LichLamViecCongViec
                                {
                                    NgayTrongTuan = lichVM.NgayTrongTuan,
                                    GioBatDau = lichVM.GioBatDau,
                                    GioKetThuc = lichVM.GioKetThuc,
                                    BuoiLamViec = lichVM.BuoiLamViec
                                });
                            }
                        }
                    }
                }

                // --- PHẦN BỊ THIẾU TRONG LẦN TRƯỚC ĐÃ ĐƯỢC KHÔI PHỤC ---
                try
                {
                    await _context.SaveChangesAsync(); // Lưu tất cả thay đổi
                    _logger.LogInformation("NTD Cá nhân (User ID: {UserId}) đã cập nhật thành công Tin Tuyển Dụng (ID: {PostingId}).", userId, id);
                    TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex) // Xử lý lỗi concurrency
                {
                    _logger.LogWarning(ex, "Lỗi concurrency khi NTD Cá nhân (User ID: {UserId}) cập nhật tin {PostingId}", userId, id);
                    ModelState.AddModelError("", "Dữ liệu đã bị thay đổi bởi người khác trong quá trình bạn chỉnh sửa. Vui lòng tải lại trang và thử lại.");
                    // Lấy giá trị mới từ DB để hiển thị lại form nếu cần
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        ModelState.AddModelError("", "Không thể lấy dữ liệu mới nhất từ CSDL để hiển thị lại.");
                        // Có thể redirect về Index hoặc trang lỗi
                    }
                    else
                    {
                        var dbPosting = (TinTuyenDung)databaseValues.ToObject();
                        // Cập nhật lại ViewModel với giá trị mới từ DB để người dùng thấy sự thay đổi
                        viewModel.TieuDe = dbPosting.TieuDe; // Ví dụ
                        viewModel.ThanhPhoId = dbPosting.ThanhPhoId;
                        viewModel.QuanHuyenId = dbPosting.QuanHuyenId;
                        // Cập nhật các trường khác nếu cần thiết
                        await PrepareDropdownsAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId); // Chuẩn bị lại dropdowns
                    }
                }
                catch (Exception ex) // Xử lý lỗi chung
                {
                    _logger.LogError(ex, "Lỗi khi NTD Cá nhân (User ID: {UserId}) cập nhật tin {PostingId}", userId, id);
                    ModelState.AddModelError("", "Đã có lỗi không mong muốn xảy ra khi lưu thay đổi.");
                }
                // --- KẾT THÚC PHẦN KHÔI PHỤC ---
            }
            else // ModelState không hợp lệ
            {
                _logger.LogWarning("ModelState không hợp lệ khi NTD Cá nhân (User ID: {UserId}) cập nhật tin {PostingId}. Lỗi: {Errors}",
                    userId, id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Nếu có lỗi hoặc ModelState không hợp lệ, chuẩn bị lại dropdowns và trả về View Edit
            await PrepareDropdownsAsync(viewModel.ThanhPhoId, viewModel.QuanHuyenId);
            // Đảm bảo có ít nhất một dòng lịch không bị đánh dấu xóa
            if (!(viewModel.LichLamViecItems?.Any(l => !l.MarkedForDeletion) ?? false))
            {
                if (viewModel.LichLamViecItems == null) viewModel.LichLamViecItems = new List<HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel>();
                viewModel.LichLamViecItems.Add(new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel());
            }
            // Trả về View với dữ liệu đã nhập và lỗi (nếu có)
            return View(viewModel);
        }


        // GET: /IndividualPosting/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return NotFound("ID không hợp lệ.");
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var postingInfo = await _context.TinTuyenDungs
                                    .Where(t => t.Id == id)
                                    .Select(t => new { t.Id, t.TieuDe, t.NguoiDangId, t.TrangThai })
                                    .AsNoTracking().FirstOrDefaultAsync();

            if (postingInfo == null) return NotFound($"Không tìm thấy tin tuyển dụng ID {id}.");
            if (postingInfo.NguoiDangId != userId)
            {
                _logger.LogWarning("User {UserId} cố gắng xem trang xóa tin {PostingId} không thuộc sở hữu.", userId, id);
                return Forbid("Bạn không có quyền xóa tin này.");
            }
            if (postingInfo.TrangThai == TrangThaiTinTuyenDung.daxoa)
            {
                TempData["InfoMessage"] = "Tin tuyển dụng này đã được xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PostingTitle = postingInfo.TieuDe; ViewBag.PostingId = postingInfo.Id;
            return View();
        }

        // POST: /IndividualPosting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var posting = await _context.TinTuyenDungs.FindAsync(id);

            if (posting == null)
            {
                TempData["InfoMessage"] = "Tin tuyển dụng không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }
            if (posting.NguoiDangId != userId)
            {
                _logger.LogWarning("User {UserId} cố gắng xác nhận xóa tin {PostingId} không thuộc sở hữu.", userId, id);
                return Forbid("Bạn không có quyền xóa tin này.");
            }
            if (posting.TrangThai == TrangThaiTinTuyenDung.daxoa)
            {
                TempData["InfoMessage"] = "Tin tuyển dụng này đã được xóa trước đó.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                posting.TrangThai = TrangThaiTinTuyenDung.daxoa; // Xóa Logic
                posting.NgayCapNhat = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("NTD Cá nhân (User ID: {UserId}) đã xóa (logic) Tin Tuyển Dụng (ID: {PostingId})", userId, id);
                TempData["SuccessMessage"] = $"Đã xóa thành công tin tuyển dụng '{posting.TieuDe}'.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi NTD Cá nhân (User ID: {UserId}) xóa (logic) tin {PostingId}", userId, id);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi xóa tin tuyển dụng.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /IndividualPosting/ToggleVisibility/5 (Ẩn hoặc Hiện lại tin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var posting = await _context.TinTuyenDungs.FindAsync(id);

            if (posting == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng.";
                return RedirectToAction(nameof(Index));
            }
            if (posting.NguoiDangId != userId)
            {
                _logger.LogWarning("User {UserId} cố gắng ẩn/hiện tin {PostingId} không thuộc sở hữu.", userId, id);
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này với tin tuyển dụng này.";
                return RedirectToAction(nameof(Index));
            }

            TrangThaiTinTuyenDung newStatus;
            string successMessage;

            if (posting.TrangThai == TrangThaiTinTuyenDung.daduyet)
            {
                newStatus = TrangThaiTinTuyenDung.taman; // Ẩn
                successMessage = $"Đã ẩn thành công tin tuyển dụng '{posting.TieuDe}'.";
                _logger.LogInformation("NTD Cá nhân (User ID: {UserId}) đã ẩn Tin Tuyển Dụng (ID: {PostingId})", userId, id);
            }
            else if (posting.TrangThai == TrangThaiTinTuyenDung.taman)
            {
                newStatus = TrangThaiTinTuyenDung.daduyet; // Hiện lại (vẫn là daduyet)
                successMessage = $"Đã hiển thị lại thành công tin tuyển dụng '{posting.TieuDe}'.";
                _logger.LogInformation("NTD Cá nhân (User ID: {UserId}) đã hiện lại Tin Tuyển Dụng (ID: {PostingId})", userId, id);
            }
            else
            {
                _logger.LogWarning("User {UserId} cố gắng ẩn/hiện tin {PostingId} đang ở trạng thái không hợp lệ ({CurrentStatus}).", userId, id, posting.TrangThai);
                TempData["InfoMessage"] = "Chỉ có thể ẩn hoặc hiện lại các tin đang ở trạng thái 'Đang hiển thị' hoặc 'Tạm ẩn'.";
                return RedirectToAction(nameof(Index));
            }

            posting.TrangThai = newStatus;
            posting.NgayCapNhat = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi NTD Cá nhân (User ID: {UserId}) ẩn/hiện tin {PostingId}", userId, id);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi cập nhật trạng thái tin tuyển dụng.";
            }

            return RedirectToAction(nameof(Index));
        }

        // File: Controllers/IndividualPostingController.cs


        // ======================================================================
        // ===          BỔ SUNG ACTION MỚI ĐỂ XEM CHI TIẾT (PREVIEW)         ===
        // ======================================================================

        // GET: /IndividualPosting/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound("ID không hợp lệ.");
            }

            if (!CheckAccess(out int userId))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var posting = await _context.TinTuyenDungs
                // Dùng ThenInclude để lấy thông tin từ bảng NguoiDung -> HoSoUngVien (để lấy avatar và tên)
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.HoSoUngVien)
                .Include(t => t.ThanhPho)
                .Include(t => t.QuanHuyen)
                .Include(t => t.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe) // Lấy tên ngành nghề từ bảng NganhNghe
                .Include(t => t.LichLamViecCongViecs)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (posting == null)
            {
                return NotFound($"Không tìm thấy tin tuyển dụng với ID {id}.");
            }

            // Kiểm tra quyền sở hữu
            if (posting.NguoiDangId != userId)
            {
                _logger.LogWarning("User {UserId} cố gắng xem chi tiết tin {PostingId} không thuộc sở hữu.", userId, id);
                return Forbid("Bạn không có quyền xem trước tin tuyển dụng này.");
            }

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
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(tin.DiaChiLamViec)) parts.Add(tin.DiaChiLamViec);
                if (tin.QuanHuyen != null) parts.Add(tin.QuanHuyen.Ten);
                if (tin.ThanhPho != null) parts.Add(tin.ThanhPho.Ten);
                return string.Join(", ", parts);
            }

            // Tạo ViewModel để truyền sang View
            var viewModel = new ChiTietTuyenDungViewModel
            {
                Id = posting.Id,
                TieuDe = posting.TieuDe,
                MoTa = posting.MoTa,
                YeuCau = posting.YeuCau,
                QuyenLoi = posting.QuyenLoi,
                TinGap = posting.TinGap,

                // Thông tin nhà tuyển dụng
                NguoiDangId = posting.NguoiDangId,
                // Đối với NTD cá nhân, tên và avatar lấy từ bảng NguoiDung
                TenNguoiDang = posting.NguoiDang.HoTen,
                LogoUrl = posting.NguoiDang.UrlAvatar,
                LoaiTkNguoiDang = posting.NguoiDang.LoaiTk,

                // Thông tin tóm tắt
                MucLuongDisplay = GetMucLuongDisplay(posting),
                LoaiHinhDisplay = posting.LoaiHinhCongViec.GetDisplayName(), // Sử dụng Extension Method
                DiaDiemLamViec = GetDiaDiemDisplay(posting),
                YeuCauKinhNghiemText = posting.YeuCauKinhNghiemText,
                YeuCauHocVanText = posting.YeuCauHocVanText,
                SoLuongTuyen = posting.SoLuongTuyen,
                NgayDang = posting.NgayDang,
                NgayHetHan = posting.NgayHetHan,
                LuotXem = 0, // Cần có trường LuotXem trong TinTuyenDung Model nếu muốn hiển thị

                // Dữ liệu quan hệ
                NganhNghes = posting.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
                LichLamViecs = posting.LichLamViecCongViecs.Select(l => new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel
                {
                    Id = l.Id,
                    NgayTrongTuan = l.NgayTrongTuan,
                    GioBatDau = l.GioBatDau,
                    GioKetThuc = l.GioKetThuc,
                    BuoiLamViec = l.BuoiLamViec
                }).ToList(),

                // Trang này là trang xem trước của NTD, không có trạng thái Lưu/Ứng tuyển
                IsSaved = false,
                IsApplied = false
            };

            // Đặt tên View rõ ràng để không bị xung đột.
            // File view sẽ là /Views/IndividualPosting/ChiTiet.cshtml
            return View("ChiTiet", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFilled(int id)
        {
            if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

            var posting = await _context.TinTuyenDungs.FindAsync(id);

            if (posting == null || posting.NguoiDangId != userId)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng hoặc bạn không có quyền.";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cho phép đánh dấu khi tin đang hiển thị
            if (posting.TrangThai != TrangThaiTinTuyenDung.daduyet)
            {
                TempData["InfoMessage"] = "Chỉ có thể đánh dấu đã tuyển cho các tin đang hiển thị.";
                return RedirectToAction(nameof(Index));
            }

            posting.TrangThai = TrangThaiTinTuyenDung.datuyen;
            posting.NgayCapNhat = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã đánh dấu tin '{posting.TieuDe}' là đã tuyển thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu đã tuyển cho tin {PostingId}", id);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra.";
            }

            return RedirectToAction(nameof(Index));
        }
        // POST: /IndividualPosting/RepostQuick
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RepostQuick(int id, DateTime? newExpiryDate)
        {
            if (!CheckAccess(out int userId))
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy tin tuyển dụng gốc để sao chép
            // Dùng AsNoTracking vì chúng ta sẽ xóa nó bằng một lệnh riêng, không muốn EF theo dõi
            var originalPosting = await _context.TinTuyenDungs
                                    .Include(t => t.TinTuyenDungNganhNghes)
                                    .Include(t => t.LichLamViecCongViecs)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(t => t.Id == id);

            if (originalPosting == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin tuyển dụng gốc.";
                return RedirectToAction(nameof(Index));
            }

            if (originalPosting.NguoiDangId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền đăng lại tin này.";
                return RedirectToAction(nameof(Index));
            }

            if (newExpiryDate.HasValue && newExpiryDate.Value.Date < DateTime.UtcNow.Date)
            {
                TempData["ErrorMessage"] = "Ngày hết hạn mới không được là ngày trong quá khứ.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo một bản ghi TinTuyenDung mới dựa trên bản gốc
            var newPosting = new TinTuyenDung
            {
                NguoiDangId = originalPosting.NguoiDangId,
                TieuDe = originalPosting.TieuDe,
                MoTa = originalPosting.MoTa,
                YeuCau = originalPosting.YeuCau,
                QuyenLoi = originalPosting.QuyenLoi,
                LoaiHinhCongViec = originalPosting.LoaiHinhCongViec,
                LoaiLuong = originalPosting.LoaiLuong,
                LuongToiThieu = originalPosting.LuongToiThieu,
                LuongToiDa = originalPosting.LuongToiDa,
                DiaChiLamViec = originalPosting.DiaChiLamViec,
                ThanhPhoId = originalPosting.ThanhPhoId,
                QuanHuyenId = originalPosting.QuanHuyenId,
                YeuCauKinhNghiemText = originalPosting.YeuCauKinhNghiemText,
                YeuCauHocVanText = originalPosting.YeuCauHocVanText,
                SoLuongTuyen = originalPosting.SoLuongTuyen,
                TinGap = originalPosting.TinGap,
                NgayHetHan = newExpiryDate,
                TrangThai = TrangThaiTinTuyenDung.daduyet,
                NgayDang = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                NgayTao = DateTime.UtcNow,
                NgayDuyet = DateTime.UtcNow,
                AdminDuyetId = null
            };

            if (originalPosting.TinTuyenDungNganhNghes.Any())
            {
                newPosting.TinTuyenDungNganhNghes = originalPosting.TinTuyenDungNganhNghes.Select(nn => new TinTuyenDung_NganhNghe { NganhNgheId = nn.NganhNgheId }).ToList();
            }

            if (originalPosting.LichLamViecCongViecs.Any())
            {
                newPosting.LichLamViecCongViecs = originalPosting.LichLamViecCongViecs.Select(lich => new LichLamViecCongViec
                {
                    NgayTrongTuan = lich.NgayTrongTuan,
                    BuoiLamViec = lich.BuoiLamViec,
                    GioBatDau = lich.GioBatDau,
                    GioKetThuc = lich.GioKetThuc
                }).ToList();
            }

            // Sử dụng transaction để đảm bảo cả hai hành động (thêm mới và xóa cũ) cùng thành công hoặc thất bại
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Thêm tin mới
                    _context.TinTuyenDungs.Add(newPosting);
                    await _context.SaveChangesAsync();

                    // 2. Tìm lại tin cũ để xóa (lần này không dùng AsNoTracking)
                    var postingToDelete = await _context.TinTuyenDungs.FindAsync(originalPosting.Id);
                    if (postingToDelete != null)
                    {
                        // Vì các mối quan hệ với TinTuyenDung đã được cấu hình OnDelete(Cascade) hoặc tương tự,
                        // EF sẽ xóa các bản ghi liên quan (TinTuyenDung_NganhNghe, LichLamViecCongViec, UngTuyen...)
                        // Cần đảm bảo cấu hình OnDelete trong DbContext là chính xác.
                        _context.TinTuyenDungs.Remove(postingToDelete);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Nếu mọi thứ thành công, commit transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("Đăng lại thành công tin ID {OriginalId}, tạo tin mới ID {NewId} và đã xóa tin gốc.", originalPosting.Id, newPosting.Id);
                    TempData["SuccessMessage"] = $"Đã đăng lại thành công tin: '{newPosting.TieuDe}'. Tin cũ đã được xóa.";
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi, rollback transaction
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi đăng lại và xóa tin gốc {PostingId}", id);
                    TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình đăng lại. Mọi thay đổi đã được hoàn tác.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
private async Task PrepareFilterDropdownsAsync(JobPostingFilterViewModel filter)
{
    // Dùng SelectList để có thể truyền selectedValue
    ViewBag.ThanhPhoList = new SelectList(
        await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(),
        "Id", "Ten", filter.ThanhPhoId);

    ViewBag.NganhNgheList = new SelectList(
        await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten).ToListAsync(),
        "Id", "Ten", filter.NganhNgheId);
    
    // Chỉ tải danh sách Quận/Huyện nếu một Tỉnh/Thành phố đã được chọn
    if (filter.ThanhPhoId.HasValue && filter.ThanhPhoId > 0)
    {
        ViewBag.QuanHuyenList = new SelectList(
            await _context.QuanHuyens.AsNoTracking()
                .Where(qh => qh.ThanhPhoId == filter.ThanhPhoId)
                .OrderBy(qh => qh.Ten).ToListAsync(),
            "Id", "Ten", filter.QuanHuyenId);
    }
    else
    {
        // Trả về danh sách rỗng nếu chưa chọn Tỉnh/Thành phố
        ViewBag.QuanHuyenList = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
    }
    
    // Sử dụng Extension Method đã có để tạo SelectList cho Enum
    ViewBag.LoaiHinhList = EnumExtensions.GetSelectList<LoaiHinhCongViec>(includeDefaultItem: true, defaultItemText: "Tất cả loại hình");
    ViewBag.TrangThaiSelectList = filter.TrangThai.ToSelectList(includeDefaultItem: true, defaultItemText: "Tất cả trạng thái");

}
// *** THAY THẾ HOÀN TOÀN ACTION INDEX BẰNG PHIÊN BẢN NÀY ***
// GET: /IndividualPosting
// GET: /IndividualPosting?Keyword=abc&ThanhPhoId=1
[HttpGet]
public async Task<IActionResult> Index([FromQuery] JobPostingFilterViewModel filter, int? pageNumber)
{
    if (!CheckAccess(out int userId)) return RedirectToAction("Index", "Dashboard");

    var query = _context.TinTuyenDungs
                        .Where(t => t.NguoiDangId == userId && t.TrangThai != TrangThaiTinTuyenDung.daxoa)
                        .AsNoTracking()
                        .AsQueryable();

    // Áp dụng bộ lọc (giữ nguyên logic này)
    if (!string.IsNullOrWhiteSpace(filter.Keyword))
    {
        var keyword = filter.Keyword.Trim().ToLower();
        query = query.Where(t => t.TieuDe.ToLower().Contains(keyword));
    }
    if (filter.ThanhPhoId.HasValue && filter.ThanhPhoId > 0)
    {
        query = query.Where(t => t.ThanhPhoId == filter.ThanhPhoId.Value);
    }
    if (filter.QuanHuyenId.HasValue && filter.QuanHuyenId > 0)
    {
        query = query.Where(t => t.QuanHuyenId == filter.QuanHuyenId.Value);
    }
    if (filter.NganhNgheId.HasValue && filter.NganhNgheId > 0)
    {
        query = query.Where(t => t.TinTuyenDungNganhNghes.Any(tnn => tnn.NganhNgheId == filter.NganhNgheId.Value));
    }
    if (filter.TrangThai.HasValue)
    {
        query = query.Where(t => t.TrangThai == filter.TrangThai.Value);
    }
    
    // Sắp xếp trước khi phân trang
    var orderedQuery = query.OrderByDescending(t => t.NgayCapNhat)
                            .Select(t => new JobPostingListViewModel
                            {
                                Id = t.Id,
                                TieuDe = t.TieuDe,
                                TrangThai = t.TrangThai,
                                NgayDang = t.NgayDang,
                                NgayHetHan = t.NgayHetHan,
                                SoUngVien = t.UngTuyens.Count
                            });

    // --- LOGIC PHÂN TRANG MỚI ---
    int pageSize = 10; // Bạn có thể đặt số lượng item mỗi trang ở đây
    var paginatedPostings = await PaginatedList<JobPostingListViewModel>.CreateAsync(orderedQuery, pageNumber ?? 1, pageSize);

    // Chuẩn bị dữ liệu cho các dropdown của bộ lọc
    await PrepareFilterDropdownsAsync(filter);
    
    // Tạo ViewModel chính để truyền sang View
    var viewModel = new JobPostingIndexViewModel
    {
        Filter = filter,
        Postings = paginatedPostings // Truyền danh sách đã phân trang
    };

    return View(viewModel);
}
        }
    }

