using HeThongTimViec.ViewModels;
using HeThongTimViec.Data;
using HeThongTimViec.Extensions;
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HeThongTimViec.Helpers;
using HeThongTimViec.Services;
using HeThongTimViec.Utils;
using System.Text.Json;

namespace HeThongTimViec.Controllers
{
    [Route("admin/nguoi-dung")]
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
   public class NguoiDungController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NguoiDungController> _logger;
    private readonly IThongBaoService _thongBaoService; // Dòng này

    // Constructor đã được cập nhật
    public NguoiDungController(
        ApplicationDbContext context, 
        ILogger<NguoiDungController> logger,
        IThongBaoService thongBaoService) // Tham số này
    {
        _context = context;
        _logger = logger;
        _thongBaoService = thongBaoService; // Gán giá trị
    }
        #region ==== CÁ NHÂN ====

        // GET: Admin/NguoiDung/CaNhan
        [HttpGet("canhan")]
        public async Task<IActionResult> CaNhan(string? searchTerm, TrangThaiTaiKhoan? searchStatus, DateTime? createdFrom, DateTime? createdTo, string viewMode = "grid", int pageNumber = 1)
        {
            int pageSize = (viewMode == "table") ? 10 : 9;

            var baseQuery = _context.NguoiDungs.Where(u => u.LoaiTk == LoaiTaiKhoan.canhan);

            var stats = new UserStatsViewModel
            {
                TotalUsers = await baseQuery.CountAsync(),
                ActiveUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.kichhoat),
                BannedUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.bidinhchi || u.TrangThaiTk == TrangThaiTaiKhoan.tamdung),
                PendingUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.choxacminh)
            };

            var filteredQuery = baseQuery.AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                filteredQuery = filteredQuery.Where(u => u.HoTen.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }
            if (searchStatus.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.TrangThaiTk == searchStatus.Value);
            }
            if (createdFrom.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.NgayTao >= createdFrom.Value.Date);
            }
            if (createdTo.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.NgayTao < createdTo.Value.Date.AddDays(1));
            }

            filteredQuery = filteredQuery.OrderByDescending(u => u.NgayTao);

            var totalItems = await filteredQuery.CountAsync();
            var pagedUsers = await filteredQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new NguoiDungCaNhanItem
                {
                    Id = u.Id,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    Sdt = u.Sdt,
                    UrlAvatar = u.UrlAvatar,
                    TrangThaiTk = u.TrangThaiTk,
                    ThanhPho = u.ThanhPho != null ? u.ThanhPho.Ten : "Chưa cập nhật",
                    NgayTao = u.NgayTao
                }).ToListAsync();

            var viewModel = new NguoiDungCaNhanIndexViewModel
            {
                Users = pagedUsers,
                Stats = stats,
                SearchTerm = searchTerm,
                SearchStatus = searchStatus,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                TrangThaiList = EnumExtensions.GetSelectList<TrangThaiTaiKhoan>(true, "-- Tất cả trạng thái --", ""),
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                ViewMode = viewMode
            };

            return View(viewModel);
        }

        // GET: Admin/NguoiDung/ChiTietCaNhan/5
       [HttpGet("chitietcanhan/{id}")]
public async Task<IActionResult> ChiTietCaNhan(int? id)
{
    if (id == null) return NotFound();

    // TỐI ƯU HÓA CÂU TRUY VẤN ĐỂ LẤY TẤT CẢ DỮ LIỆU CẦN THIẾT
    var user = await _context.NguoiDungs
        .AsNoTracking()
        // Dữ liệu cơ bản
        .Include(u => u.ThanhPho)
        .Include(u => u.QuanHuyen)
        .Include(u => u.HoSoUngVien)
        
        // Dữ liệu cho tab "Hoạt động"
        .Include(u => u.ThongBaos)
        
        // Dữ liệu cho tab "Việc làm đã ứng tuyển"
        .Include(u => u.UngTuyens)
            .ThenInclude(ut => ut.TinTuyenDung)       // Tải tin tuyển dụng của lượt ứng tuyển
                .ThenInclude(ttd => ttd.NguoiDang)        // Tải người đăng của tin đó
                    .ThenInclude(nd => nd.HoSoDoanhNghiep) // Tải hồ sơ công ty nếu có

        // Dữ liệu cho tab "Việc làm đã lưu"
        .Include(u => u.TinDaLuus)
            .ThenInclude(tdl => tdl.TinTuyenDung)     // Tải tin tuyển dụng của tin đã lưu
                .ThenInclude(ttd => ttd.NguoiDang)        // Tải người đăng của tin đó
                    .ThenInclude(nd => nd.HoSoDoanhNghiep) // Tải hồ sơ công ty nếu có

        // Dữ liệu cho tab "Báo cáo" -> ĐÂY LÀ PHẦN SỬA LỖI CHÍNH
        .Include(u => u.BaoCaoViPhamsDaGui)
            .ThenInclude(bc => bc.TinTuyenDung)       // Tải tin tuyển dụng của báo cáo
            
        .FirstOrDefaultAsync(u => u.Id == id && u.LoaiTk == LoaiTaiKhoan.canhan);

    if (user == null) return NotFound();
    
    // Tạo ViewModel với dữ liệu đã được tải đầy đủ
    var viewModel = new NguoiDungCaNhanDetailsViewModel
    {
        User = user,
        ActivityLogs = ActivityLogHelper.GetUserActivityLog(user) // Helper này sẽ dùng dữ liệu đã Include ở trên
    };

    return View(viewModel);
}

        // GET: Admin/NguoiDung/TaoMoiCaNhan
        [HttpGet("taomoicanhan")]
        public async Task<IActionResult> TaoMoiCaNhan()
        {
            var viewModel = new NguoiDungCaNhanCreateViewModel
            {
                GioiTinhList = EnumExtensions.GetSelectList<GioiTinhNguoiDung>(true, "-- Chọn giới tính --", ""),
                ThanhPhoList = (await _context.ThanhPhos.OrderBy(t => t.Ten).ToListAsync())
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Ten }).ToList()
            };
            viewModel.ThanhPhoList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Tỉnh/Thành phố --" });
            viewModel.QuanHuyenList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Quận/Huyện --" });
            return View(viewModel);
        }

        // POST: Admin/NguoiDung/TaoMoiCaNhan
        [HttpPost("taomoicanhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoiCaNhan(NguoiDungCaNhanCreateViewModel model)
        {
            if (await _context.NguoiDungs.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(model.Sdt) && await _context.NguoiDungs.AnyAsync(u => u.Sdt == model.Sdt))
            {
                ModelState.AddModelError("Sdt", "Số điện thoại này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                var newUser = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    Sdt = model.Sdt,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    GioiTinh = model.GioiTinh,
                    NgaySinh = model.NgaySinh,
                    ThanhPhoId = model.ThanhPhoId,
                    QuanHuyenId = model.QuanHuyenId,
                    DiaChiChiTiet = model.DiaChiChiTiet,
                    LoaiTk = LoaiTaiKhoan.canhan,
                    TrangThaiTk = TrangThaiTaiKhoan.kichhoat, 
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow
                };
                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo mới người dùng cá nhân thành công!";
                return RedirectToAction(nameof(CaNhan));
            }

            model.GioiTinhList = EnumExtensions.GetSelectList<GioiTinhNguoiDung>(true, "-- Chọn giới tính --", "");
            model.ThanhPhoList = (await _context.ThanhPhos.OrderBy(t => t.Ten).ToListAsync())
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Ten, Selected = t.Id == model.ThanhPhoId }).ToList();
            model.ThanhPhoList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Tỉnh/Thành phố --" });
            if (model.ThanhPhoId.HasValue)
            {
                model.QuanHuyenList = (await _context.QuanHuyens.Where(q => q.ThanhPhoId == model.ThanhPhoId).OrderBy(q => q.Ten).ToListAsync())
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Ten, Selected = q.Id == model.QuanHuyenId }).ToList();
            }
            model.QuanHuyenList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Quận/Huyện --" });
            return View(model);
        }

        // GET: Admin/NguoiDung/ChinhSuaCaNhan/5
        [HttpGet("chinhsuacanhan/{id}")]
        public async Task<IActionResult> ChinhSuaCaNhan(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null || user.LoaiTk != LoaiTaiKhoan.canhan) return NotFound();

            var viewModel = new NguoiDungCaNhanEditViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                Sdt = user.Sdt,
                NgaySinh = user.NgaySinh,
                GioiTinh = user.GioiTinh,
                ThanhPhoId = user.ThanhPhoId,
                QuanHuyenId = user.QuanHuyenId,
                DiaChiChiTiet = user.DiaChiChiTiet,
                TrangThaiTk = user.TrangThaiTk,
                GioiTinhList = EnumExtensions.GetSelectList<GioiTinhNguoiDung>(true, "-- Chọn giới tính --", ""),
                ThanhPhoList = (await _context.ThanhPhos.OrderBy(t => t.Ten).ToListAsync())
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Ten, Selected = user.ThanhPhoId == t.Id }).ToList()
            };
            viewModel.ThanhPhoList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Tỉnh/Thành phố --" });
            
            viewModel.QuanHuyenList = new List<SelectListItem>();
            if (user.ThanhPhoId.HasValue)
            {
                viewModel.QuanHuyenList = (await _context.QuanHuyens.Where(q => q.ThanhPhoId == user.ThanhPhoId).OrderBy(q => q.Ten).ToListAsync())
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Ten, Selected = user.QuanHuyenId == q.Id }).ToList();
            }
            viewModel.QuanHuyenList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Quận/Huyện --" });
            
            return View(viewModel);
        }

        // POST: Admin/NguoiDung/ChinhSuaCaNhan/5
        [HttpPost("chinhsuacanhan/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSuaCaNhan(int id, NguoiDungCaNhanEditViewModel model)
        {
            if (id != model.Id) return NotFound();
            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {
                var userToUpdate = await _context.NguoiDungs.FindAsync(id);
                if (userToUpdate == null) return NotFound();

                userToUpdate.HoTen = model.HoTen;
                userToUpdate.Sdt = model.Sdt;
                userToUpdate.NgaySinh = model.NgaySinh;
                userToUpdate.GioiTinh = model.GioiTinh;
                userToUpdate.ThanhPhoId = model.ThanhPhoId;
                userToUpdate.QuanHuyenId = model.QuanHuyenId;
                userToUpdate.DiaChiChiTiet = model.DiaChiChiTiet;
                userToUpdate.NgayCapNhat = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
                    return RedirectToAction(nameof(ChiTietCaNhan), new { id = model.Id });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật người dùng ID {UserId}", id);
                    ModelState.AddModelError("", "Không thể lưu thay đổi. Vui lòng thử lại.");
                }
            }
            
            await PopulateEditViewModelDropdowns(model);
            return View(model);
        }

        private async Task PopulateEditViewModelDropdowns(NguoiDungCaNhanEditViewModel viewModel)
        {
            viewModel.GioiTinhList = EnumExtensions.GetSelectList<GioiTinhNguoiDung>(true, "-- Chọn giới tính --", "");
            viewModel.ThanhPhoList = (await _context.ThanhPhos.OrderBy(t => t.Ten).ToListAsync())
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Ten, Selected = t.Id == viewModel.ThanhPhoId }).ToList();
            viewModel.ThanhPhoList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Tỉnh/Thành phố --" });

            viewModel.QuanHuyenList = new List<SelectListItem>();
            if (viewModel.ThanhPhoId.HasValue)
            {
                viewModel.QuanHuyenList = (await _context.QuanHuyens.Where(q => q.ThanhPhoId == viewModel.ThanhPhoId).OrderBy(q => q.Ten).ToListAsync())
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Ten, Selected = q.Id == viewModel.QuanHuyenId }).ToList();
            }
            viewModel.QuanHuyenList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Quận/Huyện --" });
        }
        
        #endregion

        #region ==== DOANH NGHIỆP ====

        [HttpGet("doanhnghiep")]
        public async Task<IActionResult> DoanhNghiep(string? searchTerm, TrangThaiTaiKhoan? searchStatus, DateTime? createdFrom, DateTime? createdTo, string viewMode = "table", int pageNumber = 1)
        {
            int pageSize = (viewMode == "grid") ? 9 : 10;
            var baseQuery = _context.NguoiDungs.Where(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep);

            var stats = new UserStatsViewModel
            {
                TotalUsers = await baseQuery.CountAsync(),
                ActiveUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.kichhoat),
                BannedUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.bidinhchi || u.TrangThaiTk == TrangThaiTaiKhoan.tamdung),
                PendingUsers = await baseQuery.CountAsync(u => u.TrangThaiTk == TrangThaiTaiKhoan.choxacminh || (u.HoSoDoanhNghiep != null && !u.HoSoDoanhNghiep.DaXacMinh))
            };

            // Always keep the Include() only on the initial query
            var includedQuery = baseQuery.AsNoTracking().Include(u => u.HoSoDoanhNghiep);

            // Now use a separate IQueryable for filtering and ordering
            IQueryable<NguoiDung> filteredQuery = includedQuery;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                filteredQuery = filteredQuery.Where(u => u.Email.ToLower().Contains(term) ||
                                                    (u.HoSoDoanhNghiep != null && u.HoSoDoanhNghiep.TenCongTy.ToLower().Contains(term)));
            }
            if (searchStatus.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.TrangThaiTk == searchStatus.Value);
            }
            if (createdFrom.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.NgayTao >= createdFrom.Value.Date);
            }
            if (createdTo.HasValue)
            {
                filteredQuery = filteredQuery.Where(u => u.NgayTao < createdTo.Value.Date.AddDays(1));
            }

            filteredQuery = filteredQuery
                .OrderBy(u => u.TrangThaiTk != TrangThaiTaiKhoan.choxacminh)
                .ThenByDescending(u => u.NgayTao);

            var totalItems = await filteredQuery.CountAsync();
            var pagedUsers = await filteredQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new NguoiDungDoanhNghiepItem
                {
                    Id = u.Id,
                    Email = u.Email,
                    Sdt = u.Sdt,
                    TenCongTy = u.HoSoDoanhNghiep != null ? u.HoSoDoanhNghiep.TenCongTy : "[Chưa có hồ sơ]",
                    UrlLogo = u.HoSoDoanhNghiep != null ? u.HoSoDoanhNghiep.UrlLogo : "/images/default-company.png",
                    TrangThaiTk = u.TrangThaiTk,
                    DaXacMinh = u.HoSoDoanhNghiep != null && u.HoSoDoanhNghiep.DaXacMinh,
                    NgayTao = u.NgayTao
                }).ToListAsync();

            var viewModel = new NguoiDungDoanhNghiepIndexViewModel
            {
                Users = pagedUsers,
                Stats = stats,
                SearchTerm = searchTerm,
                SearchStatus = searchStatus,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                TrangThaiList = EnumExtensions.GetSelectList<TrangThaiTaiKhoan>(true, "-- Tất cả trạng thái --", ""),
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                ViewMode = viewMode
            };

            return View(viewModel);
        }

        // ===== BẮT ĐẦU PHẦN SỬA ĐỔI QUAN TRỌNG =====
        // GET: Admin/NguoiDung/ChiTietDoanhNghiep/5
        [HttpGet("chitietdoanhnghiep/{id}")]
        public async Task<IActionResult> ChiTietDoanhNghiep(int? id)
        {
            if (id == null) return NotFound();

            // 1. Tối ưu hóa câu truy vấn để lấy TẤT CẢ dữ liệu cần thiết trong 1 lần gọi DB
            // Sử dụng AsNoTracking() để tăng hiệu suất vì đây là trang chỉ đọc.
            var user = await _context.NguoiDungs
                .AsNoTracking()
                // Dữ liệu cơ bản của doanh nghiệp
                .Include(u => u.HoSoDoanhNghiep).ThenInclude(hsdn => hsdn.AdminXacMinh)
                .Include(u => u.ThanhPho)
                .Include(u => u.QuanHuyen)
                // Dữ liệu cho tab "Hoạt động" (hành động của admin)
                .Include(u => u.ThongBaos)
                // Dữ liệu cho tab "Việc làm đã đăng" và các dữ liệu liên quan khác
                .Include(u => u.TinTuyenDungsDaDang)
                    // Lấy các báo cáo liên quan đến tin đã đăng (cho tab Báo cáo & Nhật ký hoạt động)
                    .ThenInclude(ttd => ttd.BaoCaoViPhams)
                        .ThenInclude(bc => bc.NguoiBaoCao) // Lấy thông tin người báo cáo
                .Include(u => u.TinTuyenDungsDaDang)
                    // Lấy các ứng tuyển liên quan đến tin đã đăng (cho Nhật ký hoạt động)
                    .ThenInclude(ttd => ttd.UngTuyens)
                .FirstOrDefaultAsync(u => u.Id == id && u.LoaiTk == LoaiTaiKhoan.doanhnghiep);

            if (user == null) return NotFound();

            // 2. Xử lý dữ liệu đã lấy để chuẩn bị cho ViewModel
            // Lấy danh sách các báo cáo mà doanh nghiệp này đã nhận
            var receivedReports = user.TinTuyenDungsDaDang
                                      .SelectMany(ttd => ttd.BaoCaoViPhams)
                                      .OrderByDescending(bc => bc.NgayBaoCao)
                                      .ToList();

            // Lấy nhật ký hoạt động bằng helper (helper sẽ sử dụng dữ liệu đã được Include() ở trên)
            var activityLogs = ActivityLogHelper.GetCompanyActivityLog(user);

            // 3. Tạo và gán đầy đủ dữ liệu cho ViewModel
            var viewModel = new NguoiDungDoanhNghiepDetailsViewModel
            {
                User = user,
                ReceivedReports = receivedReports, // Gán danh sách báo cáo
                ActivityLogs = activityLogs        // Gán nhật ký hoạt động
            };

            return View(viewModel);
        }
        // ===== KẾT THÚC PHẦN SỬA ĐỔI QUAN TRỌNG =====


        // POST: Admin/NguoiDung/XacMinhDoanhNghiep
        [HttpPost("xacminhdoanhnghiep")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacMinhDoanhNghiep(int id, string? redirectUrl)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.HoSoDoanhNghiep)
                .FirstOrDefaultAsync(u => u.Id == id && u.LoaiTk == LoaiTaiKhoan.doanhnghiep);

            if (user == null || user.HoSoDoanhNghiep == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ doanh nghiệp để xác minh.";
                return RedirectToAction(nameof(DoanhNghiep));
            }

            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdStr, out int parsedAdminId))
            {
                TempData["ErrorMessage"] = "Không thể xác định admin thực hiện hành động.";
                return LocalRedirect(redirectUrl ?? Url.Action(nameof(DoanhNghiep)));
            }

            var profile = user.HoSoDoanhNghiep;
            profile.DaXacMinh = true;
            profile.NgayXacMinh = DateTime.UtcNow;
            profile.AdminXacMinhId = parsedAdminId;

            string logMessage;
            if (user.TrangThaiTk == TrangThaiTaiKhoan.choxacminh)
            {
                user.TrangThaiTk = TrangThaiTaiKhoan.kichhoat;
                logMessage = "Hồ sơ doanh nghiệp đã được xác minh và tài khoản đã được kích hoạt bởi quản trị viên.";
                TempData["SuccessMessage"] = "Xác minh hồ sơ và kích hoạt tài khoản doanh nghiệp thành công!";
            }
            else
            {
                logMessage = "Hồ sơ doanh nghiệp đã được xác minh bởi quản trị viên.";
                TempData["SuccessMessage"] = "Xác minh hồ sơ doanh nghiệp thành công!";
            }
            user.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            try
            {
                var duLieuThongBao = new { noiDung = logMessage };
                await _thongBaoService.CreateThongBaoAsync(
                    nguoiDungId: id,
                    loaiThongBao: NotificationConstants.Types.HoSoDoanhNghiepXacMinh,
                    duLieuJson: JsonSerializer.Serialize(duLieuThongBao),
                    loaiLienQuan: NotificationConstants.RelatedEntities.HoSoDoanhNghiep,
                    idLienQuan: user.Id
                );
            }
            catch (Exception ex_notify)
            {
                _logger.LogError(ex_notify, "Lỗi khi gửi thông báo xác minh cho Doanh nghiệp ID {DoanhNghiepId}", id);
                // Hành động chính đã thành công nên không cần báo lỗi cho người dùng, chỉ log lại
            }

            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
            {
                return LocalRedirect(redirectUrl);
            }
            return RedirectToAction(nameof(DoanhNghiep));
        }

        // GET: Admin/NguoiDung/TaoMoiDoanhNghiep
        [HttpGet("taomoidoanhnghiep")]
        public IActionResult TaoMoiDoanhNghiep()
        {
            return View(new NguoiDungDoanhNghiepCreateViewModel());
        }

        // POST: Admin/NguoiDung/TaoMoiDoanhNghiep
        [HttpPost("taomoidoanhnghiep")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoiDoanhNghiep(NguoiDungDoanhNghiepCreateViewModel model)
        {
            if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(model.MaSoThue) && await _context.HoSoDoanhNghieps.AnyAsync(h => h.MaSoThue == model.MaSoThue))
            {
                ModelState.AddModelError("MaSoThue", "Mã số thuế này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                var newUser = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    Sdt = model.Sdt,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    LoaiTk = LoaiTaiKhoan.doanhnghiep,
                    TrangThaiTk = TrangThaiTaiKhoan.kichhoat,
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow,
                    HoSoDoanhNghiep = new HoSoDoanhNghiep
                    {
                        TenCongTy = model.TenCongTy,
                        MaSoThue = model.MaSoThue,
                        UrlWebsite = model.UrlWebsite,
                        QuyMoCongTy = model.QuyMoCongTy,
                        MoTa = model.MoTa,
                        DaXacMinh = false
                    }
                };
                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo mới tài khoản doanh nghiệp thành công!";
                return RedirectToAction(nameof(DoanhNghiep));
            }
            return View(model);
        }

        // GET: Admin/NguoiDung/ChinhSuaDoanhNghiep/5
        [HttpGet("chinhsuadoanhnghiep/{id}")]
        public async Task<IActionResult> ChinhSuaDoanhNghiep(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.NguoiDungs
                .Include(u => u.HoSoDoanhNghiep)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.LoaiTk == LoaiTaiKhoan.doanhnghiep);
            
            if (user == null) return NotFound();
            
            user.HoSoDoanhNghiep ??= new HoSoDoanhNghiep();

            var viewModel = new NguoiDungDoanhNghiepEditViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                Sdt = user.Sdt,
                NgaySinh = user.NgaySinh,
                GioiTinh = user.GioiTinh,
                ThanhPhoId = user.ThanhPhoId,
                QuanHuyenId = user.QuanHuyenId,
                DiaChiChiTiet = user.DiaChiChiTiet,
                TenCongTy = user.HoSoDoanhNghiep.TenCongTy,
                MaSoThue = user.HoSoDoanhNghiep.MaSoThue,
                UrlWebsite = user.HoSoDoanhNghiep.UrlWebsite,
                DiaChiDangKy = user.HoSoDoanhNghiep.DiaChiDangKy,
                QuyMoCongTy = user.HoSoDoanhNghiep.QuyMoCongTy,
                MoTa = user.HoSoDoanhNghiep.MoTa,
                LoaiTk = user.LoaiTk,
                TrangThaiTk = user.TrangThaiTk,
                DaXacMinh = user.HoSoDoanhNghiep.DaXacMinh 
            };
            
            await PopulateEditViewModelDropdowns(viewModel);
            return View(viewModel);
        }

        // POST: Admin/NguoiDung/ChinhSuaDoanhNghiep/5
        [HttpPost("chinhsuadoanhnghiep/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSuaDoanhNghiep(int id, NguoiDungDoanhNghiepEditViewModel model)
        {
            if (id != model.Id) return NotFound();
            
            ModelState.Remove("Email");
            ModelState.Remove("LoaiTk");
            ModelState.Remove("TrangThaiTk");
            ModelState.Remove("DaXacMinh");

            if (ModelState.IsValid)
            {
                var userToUpdate = await _context.NguoiDungs
                    .Include(u => u.HoSoDoanhNghiep)
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (userToUpdate == null) return NotFound();
                
                userToUpdate.HoSoDoanhNghiep ??= new HoSoDoanhNghiep { NguoiDungId = id };
                
                userToUpdate.HoTen = model.HoTen;
                userToUpdate.Sdt = model.Sdt;
                userToUpdate.NgaySinh = model.NgaySinh;
                userToUpdate.GioiTinh = model.GioiTinh;
                userToUpdate.ThanhPhoId = model.ThanhPhoId;
                userToUpdate.QuanHuyenId = model.QuanHuyenId;
                userToUpdate.DiaChiChiTiet = model.DiaChiChiTiet;
                userToUpdate.NgayCapNhat = DateTime.UtcNow;

                userToUpdate.HoSoDoanhNghiep.TenCongTy = model.TenCongTy;
                userToUpdate.HoSoDoanhNghiep.MaSoThue = model.MaSoThue;
                userToUpdate.HoSoDoanhNghiep.UrlWebsite = model.UrlWebsite;
                userToUpdate.HoSoDoanhNghiep.DiaChiDangKy = model.DiaChiDangKy;
                userToUpdate.HoSoDoanhNghiep.QuyMoCongTy = model.QuyMoCongTy;
                userToUpdate.HoSoDoanhNghiep.MoTa = model.MoTa;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin doanh nghiệp thành công!";
                    return RedirectToAction(nameof(ChiTietDoanhNghiep), new { id = model.Id });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật doanh nghiệp ID {UserId}", id);
                    ModelState.AddModelError("", "Không thể lưu thay đổi. Vui lòng thử lại.");
                }
            }
            
            await PopulateEditViewModelDropdowns(model);
            return View(model);
        }

        private async Task PopulateEditViewModelDropdowns(NguoiDungDoanhNghiepEditViewModel viewModel)
        {
            viewModel.GioiTinhList = EnumExtensions.GetSelectList<GioiTinhNguoiDung>(true, "-- Chọn giới tính --", "");
            viewModel.ThanhPhoList = (await _context.ThanhPhos.OrderBy(t => t.Ten).ToListAsync())
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Ten, Selected = t.Id == viewModel.ThanhPhoId }).ToList();
            viewModel.ThanhPhoList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Tỉnh/Thành phố --" });
            
            viewModel.QuanHuyenList = new List<SelectListItem>();
            if (viewModel.ThanhPhoId.HasValue)
            {
                viewModel.QuanHuyenList = (await _context.QuanHuyens.Where(q => q.ThanhPhoId == viewModel.ThanhPhoId).OrderBy(q => q.Ten).ToListAsync())
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Ten, Selected = q.Id == viewModel.QuanHuyenId }).ToList();
            }
            viewModel.QuanHuyenList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn Quận/Huyện --" });
        }

        #endregion

        #region ==== HÀNH ĐỘNG CHUNG (CẤM, KÍCH HOẠT, XÓA) ====

        [HttpPost("capnhattrangthai")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiTaiKhoan newStatus, string? redirectUrl)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            var oldStatus = user.TrangThaiTk;
            user.TrangThaiTk = newStatus;
            user.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái tài khoản thành '{newStatus.GetDisplayName()}' thành công.";
                        try
            {
                string notificationType;
                string notificationMessage;

                switch (newStatus)
                {
                    case TrangThaiTaiKhoan.bidinhchi:
                        notificationType = NotificationConstants.Types.TaiKhoanBiDinhChi;
                        notificationMessage = "Tài khoản của bạn đã bị đình chỉ do vi phạm chính sách. Vui lòng liên hệ quản trị viên để biết thêm chi tiết.";
                        break;
                    case TrangThaiTaiKhoan.tamdung:
                        notificationType = NotificationConstants.Types.TaiKhoanTamDung;
                        notificationMessage = "Tài khoản của bạn đã bị tạm dừng. Vui lòng liên hệ quản trị viên để biết thêm chi tiết.";
                        break;
                    case TrangThaiTaiKhoan.kichhoat:
                        notificationType = NotificationConstants.Types.TaiKhoanKichHoat;
                        notificationMessage = $"Tài khoản của bạn đã được quản trị viên kích hoạt lại sau khi ở trạng thái '{oldStatus.GetDisplayName()}'.";
                        break;
                    default:
                        notificationType = NotificationConstants.Types.HeThongChung;
                        notificationMessage = $"Trạng thái tài khoản của bạn đã được cập nhật thành '{newStatus.GetDisplayName()}'.";
                        break;
                }

                var duLieuJson = JsonSerializer.Serialize(new { noiDung = notificationMessage });
                await _thongBaoService.CreateThongBaoAsync(
                    nguoiDungId: id,
                    loaiThongBao: notificationType,
                    duLieuJson: duLieuJson,
                    loaiLienQuan: NotificationConstants.RelatedEntities.NguoiDung,
                    idLienQuan: id
                );
            }
            catch (Exception ex_notify)
            {
                _logger.LogError(ex_notify, "Lỗi khi gửi thông báo cập nhật trạng thái cho User ID {UserId}", id);
            }
            // ---------- KẾT THÚC TÍCH HỢP GỬI THÔNG BÁO ----------

            
            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
            {
                return LocalRedirect(redirectUrl);
            }
            return RedirectToAction(user.LoaiTk == LoaiTaiKhoan.canhan ? nameof(CaNhan) : nameof(DoanhNghiep));
        }

        [HttpPost("xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng để xóa.";
                return RedirectToAction(nameof(CaNhan));
            }
            
            var userType = user.LoaiTk;
            var userName = user.HoTen;

            try
            {
                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn người dùng '{userName}' thành công.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng ID {UserId}", id);
                TempData["ErrorMessage"] = "Xóa thất bại. Người dùng này có dữ liệu liên quan không thể xóa.";
            }
            
            return RedirectToAction(userType == LoaiTaiKhoan.canhan ? nameof(CaNhan) : nameof(DoanhNghiep));
        }

        #endregion
    }
}