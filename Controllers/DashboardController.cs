// File: Controllers/DashboardController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.Extensions; // Cho GetDisplayName()
using HeThongTimViec.ViewModels.Dashboard;
using HeThongTimViec.ViewModels.BaoCao;
using HeThongTimViec.ViewModels.ViecDaLuu;
using HeThongTimViec.ViewModels.ViecLam; // Namespace cho DaUngTuyenItemViewModel và ViecLamHelper

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using HeThongTimViec.ViewModels;

namespace HeThongTimViec.Controllers
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardController(
            ILogger<DashboardController> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsNtdCaNhanViewActive()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetInt32("DangLaNTD") == 1;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            _logger.LogError("Không thể lấy User ID từ claims trong GetCurrentUserId().");
            throw new InvalidOperationException("Không thể xác định ID người dùng hiện tại. Vui lòng đăng nhập lại.");
        }


        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var roleName = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(roleName))
            {
                _logger.LogWarning("User ID {UserId} không có role claim. Không thể xác định loại dashboard.", userId);
                TempData["ErrorMessage"] = "Không thể xác định vai trò người dùng để hiển thị bảng điều khiển.";
                return RedirectToAction("AccessDenied", "TaiKhoan");
            }

            _logger.LogInformation("Dashboard access attempt by User ID: {UserId} with Role: {Role}", userId, roleName);

            if (roleName == nameof(LoaiTaiKhoan.canhan))
            {
                // Lấy thông tin người dùng một lần để sử dụng chung
                var nguoiDungCaNhan = await _context.NguoiDungs
                                           .Include(n => n.HoSoUngVien) // Bao gồm HoSoUngVien nếu là ứng viên
                                           .Include(n => n.LichRanhUngViens) // Bao gồm LichRanhUngViens nếu là ứng viên
                                           .Include(n => n.DiaDiemMongMuons) // Bao gồm DiaDiemMongMuons nếu là ứng viên
                                               .ThenInclude(dd => dd.QuanHuyen)
                                           .Include(n => n.DiaDiemMongMuons)
                                               .ThenInclude(dd => dd.ThanhPho)
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(n => n.Id == userId);

                if (nguoiDungCaNhan == null)
                {
                    _logger.LogWarning($"Người dùng (cá nhân) với ID {userId} không tìm thấy.");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "TaiKhoan");
                }

                if (IsNtdCaNhanViewActive())
                {
                    _logger.LogInformation("Hiển thị Dashboard Nhà tuyển dụng Cá nhân (NTD CN) cho User ID: {UserId}", userId);

                    var jobsQuery = _context.TinTuyenDungs.Where(t => t.NguoiDangId == userId && t.TrangThai != TrangThaiTinTuyenDung.daxoa);

                    var recentlyPostedJobs = await jobsQuery
                        .OrderByDescending(t => t.NgayDang)
                        .Take(5)
                        .Select(t => new DashboardJobPostingItemViewModel
                        {
                            Id = t.Id,
                            Title = t.TieuDe,
                            PostedDate = t.NgayDang,
                            Status = t.TrangThai,
                            ApplicantCount = t.UngTuyens.Count()
                        })
                        .ToListAsync();

                    var recentApplications = await _context.UngTuyens
                        .Include(ut => ut.TinTuyenDung)
                        .Include(ut => ut.UngVien)
                            .ThenInclude(uv => uv.HoSoUngVien) // To get HoSoUngVien.ViTriMongMuon
                        .Where(ut => ut.TinTuyenDung.NguoiDangId == userId)
                        .OrderByDescending(ut => ut.NgayNop)
                        .Take(5)
                        .Select(ut => new DashboardApplicationItemViewModel
                        {
                            ApplicationId = ut.Id,
                            CandidateId = ut.UngVienId,
                            JobId = ut.TinTuyenDungId,
                            JobTitle = ut.TinTuyenDung.TieuDe,
                            ApplicantName = ut.UngVien.HoTen,
                            ApplicantAvatarUrl = ut.UngVien.UrlAvatar,
                            ApplicantProfilePosition = ut.UngVien.HoSoUngVien != null ? (ut.UngVien.HoSoUngVien.TieuDeHoSo ?? ut.UngVien.HoSoUngVien.ViTriMongMuon) : "Chưa cập nhật",
                            AppliedDate = ut.NgayNop,
                            Status = ut.TrangThai
                        })
                        .ToListAsync();

                    var employerCandidateVm = new EmployerForCandidateDashboardViewModel
                    {
                        EmployerName = nguoiDungCaNhan.HoTen,
                        UserAvatarUrl = nguoiDungCaNhan.UrlAvatar,
                        TotalJobsPosted = await jobsQuery.CountAsync(),
                        ActiveJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet),
                        PendingJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.choduyet),
                        ExpiredOrFilledJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.hethan || t.TrangThai == TrangThaiTinTuyenDung.datuyen),

                        TotalApplicationsReceived = await _context.UngTuyens
                                                         .CountAsync(ut => ut.TinTuyenDung.NguoiDangId == userId),
                        NewApplicationsCount = await _context.UngTuyens
                                                    .CountAsync(ut => ut.TinTuyenDung.NguoiDangId == userId && ut.TrangThai == TrangThaiUngTuyen.danop),

                        RecentlyPostedJobs = recentlyPostedJobs,
                        RecentApplications = recentApplications
                    };

                    ViewBag.HoTenNguoiDung = employerCandidateVm.EmployerName;
                    ViewBag.UserAvatarUrl = employerCandidateVm.UserAvatarUrl;

                    return View("EmployerForCandidateDashboard", employerCandidateVm);
                }
                else
                {
                    _logger.LogInformation("Hiển thị Dashboard Ứng viên (chuẩn) cho User ID: {UserId}", userId);
                    // nguoiDungCaNhan đã được lấy ở trên và bao gồm các Include cần thiết

                    var viewModel = new CandidateDashboardViewModel
                    {
                        CandidateName = nguoiDungCaNhan.HoTen,
                        UserAvatarUrl = nguoiDungCaNhan.UrlAvatar
                    };

                    var hoSoUngVien = nguoiDungCaNhan.HoSoUngVien;
                    if (hoSoUngVien != null)
                    {
                        viewModel.HasProfile = true;
                        viewModel.ProfileCompletionPercentage = CalculateProfileCompletion(hoSoUngVien, nguoiDungCaNhan.LichRanhUngViens.Any(), nguoiDungCaNhan.DiaDiemMongMuons.Any());
                        viewModel.ProfileTitle = hoSoUngVien.TieuDeHoSo;
                        viewModel.DesiredPosition = hoSoUngVien.ViTriMongMuon;
                        viewModel.ProfileJobSearchStatus = hoSoUngVien.TrangThaiTimViec;
                        viewModel.ProfileJobSearchStatusDisplay = hoSoUngVien.TrangThaiTimViec.GetDisplayName();
                        viewModel.ProfileAllowsSearch = hoSoUngVien.ChoPhepTimKiem;
                    }
                    else
                    {
                        viewModel.HasProfile = false;
                        viewModel.ProfileCompletionPercentage = 0;
                    }

                    viewModel.FreeSlotsCount = nguoiDungCaNhan.LichRanhUngViens.Count;
                    if (viewModel.FreeSlotsCount > 0)
                    {
                        viewModel.DetailedFreeSlots = nguoiDungCaNhan.LichRanhUngViens
                                            .OrderBy(lr => (int)lr.NgayTrongTuan)
                                            .ThenBy(lr => (int)lr.BuoiLamViec)
                                            .Select(lr => new LichRanhDisplayViewModel
                                            {
                                                NgayTrongTuanDisplay = lr.NgayTrongTuan.GetDisplayName(),
                                                BuoiLamViecDisplay = lr.BuoiLamViec.GetDisplayName(),
                                                BuoiEnumValue = lr.BuoiLamViec,
                                                NgayEnumValue = lr.NgayTrongTuan
                                            })
                                            .ToList();
                    }

                    viewModel.DesiredLocationsCount = nguoiDungCaNhan.DiaDiemMongMuons.Count;
                    if (viewModel.DesiredLocationsCount > 0)
                    {
                        viewModel.DesiredLocations = nguoiDungCaNhan.DiaDiemMongMuons
                            .OrderBy(dd => dd.ThanhPho.Ten)
                            .ThenBy(dd => dd.QuanHuyen.Ten)
                            .Select(dd => new DiaDiemMongMuonDisplayViewModel
                            {
                                Id = dd.Id,
                                TenQuanHuyen = dd.QuanHuyen.Ten,
                                TenThanhPho = dd.ThanhPho.Ten
                            }).ToList();
                    }

                    viewModel.ApplicationsCount = await _context.UngTuyens.CountAsync(u => u.UngVienId == userId && u.TrangThai != TrangThaiUngTuyen.darut);
                    viewModel.SavedJobsCount = await _context.TinDaLuus.CountAsync(s => s.NguoiDungId == userId);
                    viewModel.ReportsCount = await _context.BaoCaoViPhams.CountAsync(r => r.NguoiBaoCaoId == userId);

                    // Recent Applications
                    viewModel.RecentApplications = await _context.UngTuyens
                        .Where(u => u.UngVienId == userId && u.TrangThai != TrangThaiUngTuyen.darut)
                        .Include(u => u.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                        .Include(u => u.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                        .Include(u => u.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                        .Include(u => u.TinTuyenDung).ThenInclude(ttd => ttd.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                        .OrderByDescending(u => u.NgayNop)
                        .Take(3)
                        .Select(u => new DaUngTuyenItemViewModel
                        {
                            UngTuyenId = u.Id,
                            TinTuyenDungId = u.TinTuyenDungId,
                            TieuDeCongViec = u.TinTuyenDung.TieuDe,
                            TenNhaTuyenDung = u.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? u.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy : u.TinTuyenDung.NguoiDang.HoTen,
                            LogoUrl = u.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (u.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? u.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo : null) : u.TinTuyenDung.NguoiDang.UrlAvatar,
                            DiaDiem = u.TinTuyenDung.DiaChiLamViec ?? $"{u.TinTuyenDung.QuanHuyen.Ten}, {u.TinTuyenDung.ThanhPho.Ten}",
                            MucLuongDisplay = ViecLamHelper.FormatMucLuong(u.TinTuyenDung.LoaiLuong, u.TinTuyenDung.LuongToiThieu, u.TinTuyenDung.LuongToiDa, "VNĐ"),
                            LoaiHinhCongViecDisplay = u.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                            NgayNop = u.NgayNop,
                            NgayCapNhatTrangThai = u.NgayCapNhatTrangThai,
                            ThuGioiThieuSnippet = u.ThuGioiThieu != null && u.ThuGioiThieu.Length > 50 ? u.ThuGioiThieu.Substring(0, 50) + "..." : u.ThuGioiThieu,
                            TrangThai = u.TrangThai,
                            TrangThaiDisplay = u.TrangThai.GetDisplayName(),
                            TrangThaiBadgeClass = GetUngTuyenBadgeClass(u.TrangThai),
                            NgayHetHan = u.TinTuyenDung.NgayHetHan,
                            Tags = u.TinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
                            CanWithdraw = (u.TrangThai == TrangThaiUngTuyen.danop || u.TrangThai == TrangThaiUngTuyen.ntddaxem) &&
                                          (u.TinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daduyet || u.TinTuyenDung.TrangThai == TrangThaiTinTuyenDung.datuyen), // Tin phải còn đang tuyển hoặc đã tuyển
                        })
                        .ToListAsync();

                    // Recent Saved Jobs
                    viewModel.RecentSavedJobs = await _context.TinDaLuus
                        .Where(s => s.NguoiDungId == userId)
                        .Include(s => s.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                        .Include(s => s.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                        .Include(s => s.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                        .Include(s => s.TinTuyenDung).ThenInclude(ttd => ttd.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                        .OrderByDescending(s => s.NgayLuu)
                        .Take(3)
                        .Select(s => new SavedJobItemViewModel
                        {
                            TinDaLuuId = s.Id,
                            TinTuyenDungId = s.TinTuyenDungId,
                            TieuDe = s.TinTuyenDung.TieuDe,
                            TenCongTyHoacNguoiDang = s.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? s.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy : s.TinTuyenDung.NguoiDang.HoTen,
                            LogoUrl = s.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (s.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? s.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo : null) : s.TinTuyenDung.NguoiDang.UrlAvatar,
                            LoaiTkNguoiDang = s.TinTuyenDung.NguoiDang.LoaiTk,
                            DiaDiem = s.TinTuyenDung.DiaChiLamViec ?? $"{s.TinTuyenDung.QuanHuyen.Ten}, {s.TinTuyenDung.ThanhPho.Ten}",
                            MucLuongDisplay = ViecLamHelper.FormatMucLuong(s.TinTuyenDung.LoaiLuong, s.TinTuyenDung.LuongToiThieu, s.TinTuyenDung.LuongToiDa, "VNĐ"),
                            LoaiHinhDisplay = s.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                            NgayHetHan = s.TinTuyenDung.NgayHetHan,
                            Tags = s.TinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
                            NgayLuu = s.NgayLuu,
                            DaUngTuyen = _context.UngTuyens.Any(ut => ut.UngVienId == userId && ut.TinTuyenDungId == s.TinTuyenDungId && ut.TrangThai != TrangThaiUngTuyen.darut)
                        })
                        .ToListAsync();

                    // Recent Reports
                    viewModel.RecentReports = await _context.BaoCaoViPhams
                        .Where(r => r.NguoiBaoCaoId == userId)
                        .Include(r => r.TinTuyenDung).ThenInclude(ttd => ttd.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                        .Include(r => r.TinTuyenDung).ThenInclude(ttd => ttd.QuanHuyen)
                        .Include(r => r.TinTuyenDung).ThenInclude(ttd => ttd.ThanhPho)
                        .Include(r => r.TinTuyenDung).ThenInclude(ttd => ttd.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                        .OrderByDescending(r => r.NgayBaoCao)
                        .Take(3)
                        .Select(r => new BaoCaoItemViewModel
                        {
                            BaoCaoId = r.Id,
                            LyDoBaoCaoDisplay = r.LyDo.GetDisplayName(),
                            ChiTietBaoCao = r.ChiTiet,
                            NgayBaoCao = r.NgayBaoCao,
                            TrangThaiXuLy = r.TrangThaiXuLy,
                            TrangThaiXuLyDisplay = r.TrangThaiXuLy.GetDisplayName(),
                            TrangThaiXuLyBadgeClass = GetBaoCaoBadgeClass(r.TrangThaiXuLy),
                            GhiChuAdmin = r.GhiChuAdmin,
                            NgayXuLyCuaAdmin = r.NgayXuLy,
                            CanDelete = r.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi,
                            TinTuyenDungId = r.TinTuyenDungId,
                            TieuDeTinTuyenDung = r.TinTuyenDung.TieuDe,
                            TenNhaTuyenDungHoacNguoiDang = r.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? r.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.TenCongTy : r.TinTuyenDung.NguoiDang.HoTen,
                            LogoUrlNhaTuyenDung = r.TinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (r.TinTuyenDung.NguoiDang.HoSoDoanhNghiep != null ? r.TinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo : null) : r.TinTuyenDung.NguoiDang.UrlAvatar,
                            LoaiTkNguoiDang = r.TinTuyenDung.NguoiDang.LoaiTk,
                            DiaDiemTinTuyenDung = r.TinTuyenDung.DiaChiLamViec ?? $"{r.TinTuyenDung.QuanHuyen.Ten}, {r.TinTuyenDung.ThanhPho.Ten}",
                            MucLuongDisplayTinTuyenDung = ViecLamHelper.FormatMucLuong(r.TinTuyenDung.LoaiLuong, r.TinTuyenDung.LuongToiThieu, r.TinTuyenDung.LuongToiDa, "VNĐ"),
                            LoaiHinhDisplayTinTuyenDung = r.TinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                            NgayHetHanTinTuyenDung = r.TinTuyenDung.NgayHetHan,
                            TagsTinTuyenDung = r.TinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
                            TinGapTinTuyenDung = r.TinTuyenDung.TinGap
                        })
                        .ToListAsync();

                    return View("CandidateDashboard", viewModel);
                }
            }
            else if (roleName == nameof(LoaiTaiKhoan.doanhnghiep))
            {
                _logger.LogInformation("Hiển thị Dashboard Nhà tuyển dụng (Doanh nghiệp) cho User ID: {UserId}", userId);

                // Lấy đầy đủ thông tin NguoiDung và HoSoDoanhNghiep
                // userId đã được lấy từ ClaimTypes.NameIdentifier ở đầu action Index
                var nguoiDungDN = await _context.NguoiDungs
                                             .Include(nd => nd.HoSoDoanhNghiep) // Quan trọng: Eager load HoSoDoanhNghiep
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(nd => nd.Id == userId);

                if (nguoiDungDN == null)
                {
                    _logger.LogError($"Lỗi không mong muốn: Người dùng doanh nghiệp với ID {userId} không tìm thấy sau khi đã xác thực ban đầu.");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin tài khoản doanh nghiệp.";
                    return RedirectToAction("DangNhap", "TaiKhoan"); // Hoặc tên action đăng nhập của bạn
                }

                if (nguoiDungDN.HoSoDoanhNghiep == null)
                {
                    _logger.LogWarning("Tài khoản doanh nghiệp ID: {UserId} chưa có Hồ sơ Doanh nghiệp.", userId);
                    ViewBag.RepresentativeUserName = nguoiDungDN.HoTen;
                    ViewBag.RepresentativeUserAvatarUrl = nguoiDungDN.UrlAvatar;
                    ViewBag.CompanyRepresentativeEmail = nguoiDungDN.Email;
                    return View("EmployerDashboard_IncompleteProfile");
                }

                // Tạo ViewModel
                var companyProfile = nguoiDungDN.HoSoDoanhNghiep;
                var viewModel = new EmployerCompanyDashboardViewModel(companyProfile, nguoiDungDN);

                // Lấy các thống kê và dữ liệu gần đây
                var jobsQuery = _context.TinTuyenDungs.Where(t => t.NguoiDangId == userId && t.TrangThai != TrangThaiTinTuyenDung.daxoa);

                viewModel.TotalJobsPosted = await jobsQuery.CountAsync();
                viewModel.ActiveJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet);
                viewModel.PendingJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.choduyet);
                viewModel.ExpiredOrFilledJobsCount = await jobsQuery.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.hethan || t.TrangThai == TrangThaiTinTuyenDung.datuyen);

                viewModel.TotalApplicationsReceived = await _context.UngTuyens
                                                         .CountAsync(ut => ut.TinTuyenDung.NguoiDangId == userId);
                viewModel.NewApplicationsCount = await _context.UngTuyens
                                                    .CountAsync(ut => ut.TinTuyenDung.NguoiDangId == userId && ut.TrangThai == TrangThaiUngTuyen.danop);

                viewModel.RecentlyPostedJobs = await jobsQuery
                    .OrderByDescending(t => t.NgayDang)
                    .Take(5) // Lấy 5 tin đăng gần nhất
                    .Select(t => new DashboardJobPostingItemViewModel
                    {
                        Id = t.Id,
                        Title = t.TieuDe,
                        PostedDate = t.NgayDang,
                        Status = t.TrangThai,
                        ApplicantCount = t.UngTuyens.Count() // Đếm số ứng viên cho mỗi tin
                    })
                    .ToListAsync();

                viewModel.RecentApplications = await _context.UngTuyens
                    .Include(ut => ut.TinTuyenDung) // Để lấy Tiêu đề công việc
                    .Include(ut => ut.UngVien)     // Để lấy Tên ứng viên, Avatar
                        .ThenInclude(uv => uv.HoSoUngVien) // Để lấy ViTriMongMuon từ HoSoUngVien
                    .Where(ut => ut.TinTuyenDung.NguoiDangId == userId) // Chỉ các ứng tuyển cho tin của NTD này
                    .OrderByDescending(ut => ut.NgayNop)
                    .Take(5) // Lấy 5 ứng tuyển gần nhất
                    .Select(ut => new DashboardApplicationItemViewModel
                    {
                        ApplicationId = ut.Id,
                        CandidateId = ut.UngVienId,
                        JobId = ut.TinTuyenDungId,
                        JobTitle = ut.TinTuyenDung.TieuDe,
                        ApplicantName = ut.UngVien.HoTen,
                        ApplicantAvatarUrl = ut.UngVien.UrlAvatar,
                        ApplicantProfilePosition = ut.UngVien.HoSoUngVien != null ? (ut.UngVien.HoSoUngVien.TieuDeHoSo ?? ut.UngVien.HoSoUngVien.ViTriMongMuon) : "Chưa cập nhật",
                        AppliedDate = ut.NgayNop,
                        Status = ut.TrangThai
                    })
                    .ToListAsync();

                // Thiết lập ViewBag cho _Layout nếu _Layout của bạn cần hiển thị thông tin này
                // (Ví dụ: tên công ty và logo ở header)
                ViewBag.DisplayNameInHeader = viewModel.CompanyName;
                ViewBag.AvatarUrlInHeader = viewModel.CompanyLogoUrl ?? viewModel.RepresentativeUserAvatarUrl;
                return View("EmployerDashboard", viewModel);
            }
            else if (roleName == nameof(LoaiTaiKhoan.quantrivien))
            {
                _logger.LogInformation("Hiển thị Dashboard Quản trị viên cho User ID: {UserId}", userId);
                // TODO: Tạo AdminDashboardViewModel và logic lấy dữ liệu tương ứng
                // var adminVm = new AdminDashboardViewModel
                // {
                //     TotalUsers = await _context.NguoiDungs.CountAsync(),
                //     PendingJobs = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.choduyet),
                //     // ...
                // };
                // return View("AdminDashboard", adminVm);
                return View("AdminDashboard");
            }
            else
            {
                _logger.LogError("User ID {UserId} có vai trò không nhận dạng được hoặc không hỗ trợ: {Role}. Truy cập bị từ chối.", userId, roleName);
                TempData["ErrorMessage"] = $"Vai trò '{roleName}' không được phép truy cập dashboard này.";
                return RedirectToAction("AccessDenied", "TaiKhoan");
            }
        }

        private int CalculateProfileCompletion(HoSoUngVien? hoSo, bool hasSchedule, bool hasDesiredLocations)
        {
            if (hoSo == null) return 0; // Nếu không có hồ sơ, độ hoàn thiện là 0

            int completedFields = 0;
            // Tổng số trường có thể có để tính phần trăm. Điều chỉnh nếu thêm/bớt trường.
            int totalCheckableFields = 8; // TieuDe, GioiThieu, ViTri, LoaiLuong, MucLuong, UrlCV, LichRanh, DiaDiemMongMuon

            if (!string.IsNullOrEmpty(hoSo.TieuDeHoSo)) completedFields++;
            if (!string.IsNullOrEmpty(hoSo.GioiThieuBanThan)) completedFields++;
            if (!string.IsNullOrEmpty(hoSo.ViTriMongMuon)) completedFields++;
            if (hoSo.LoaiLuongMongMuon.HasValue) completedFields++;
            if (hoSo.MucLuongMongMuon.HasValue && hoSo.MucLuongMongMuon > 0) completedFields++; // Lương phải > 0 mới tính
            if (!string.IsNullOrEmpty(hoSo.UrlCvMacDinh)) completedFields++;
            if (hasSchedule) completedFields++;
            if (hasDesiredLocations) completedFields++;

            return totalCheckableFields > 0 ? (int)Math.Round((double)completedFields / totalCheckableFields * 100) : 0;
        }

        private static string GetUngTuyenBadgeClass(TrangThaiUngTuyen trangThai)
        {
            return trangThai switch
            {
                TrangThaiUngTuyen.danop => "bg-primary",
                TrangThaiUngTuyen.ntddaxem => "bg-info text-dark",
                TrangThaiUngTuyen.bituchoi => "bg-danger",
                TrangThaiUngTuyen.daduyet => "bg-success",
                TrangThaiUngTuyen.darut => "bg-secondary",
                _ => "bg-light text-dark",
            };
        }

        private static string GetBaoCaoBadgeClass(TrangThaiXuLyBaoCao trangThai)
        {
            return trangThai switch
            {
                TrangThaiXuLyBaoCao.moi => "bg-warning text-dark",
                TrangThaiXuLyBaoCao.daxemxet => "bg-info text-dark",
                TrangThaiXuLyBaoCao.daxuly => "bg-success",
                TrangThaiXuLyBaoCao.boqua => "bg-secondary",
                _ => "bg-light text-dark",
            };
        }

        // --- ACTIONS FOR DASHBOARD INTERACTIONS ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RutUngTuyenDashboard(int ungTuyenId)
        {
            if (ungTuyenId <= 0)
            {
                TempData["ErrorMessage"] = "ID ứng tuyển không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var ungTuyen = await _context.UngTuyens
                                    .Include(u => u.TinTuyenDung) // Cần để kiểm tra trạng thái tin
                                    .FirstOrDefaultAsync(u => u.Id == ungTuyenId && u.UngVienId == userId);

            if (ungTuyen == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn ứng tuyển hoặc bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction(nameof(Index));
            }

            if (ungTuyen.TrangThai != TrangThaiUngTuyen.danop && ungTuyen.TrangThai != TrangThaiUngTuyen.ntddaxem)
            {
                TempData["ErrorMessage"] = "Không thể rút đơn ứng tuyển ở trạng thái này.";
                return RedirectToAction(nameof(Index));
            }
            if (ungTuyen.TinTuyenDung.TrangThai != TrangThaiTinTuyenDung.daduyet && ungTuyen.TinTuyenDung.TrangThai != TrangThaiTinTuyenDung.datuyen)
            {
                TempData["WarningMessage"] = "Tin tuyển dụng này không còn trong giai đoạn tuyển dụng tích cực. Việc rút đơn có thể không cần thiết nhưng vẫn được xử lý.";
            }

            ungTuyen.TrangThai = TrangThaiUngTuyen.darut;
            ungTuyen.NgayCapNhatTrangThai = DateTime.Now;
            _context.UngTuyens.Update(ungTuyen);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã rút đơn ứng tuyển thành công.";
                _logger.LogInformation("User {UserId} rút đơn ứng tuyển ID {UngTuyenId}", userId, ungTuyenId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi rút đơn ứng tuyển ID {UngTuyenId} cho User {UserId}", ungTuyenId, userId);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoLuuTinDashboard(int tinDaLuuId)
        {
            if (tinDaLuuId <= 0)
            {
                TempData["ErrorMessage"] = "ID tin đã lưu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }
            var userId = GetCurrentUserId();
            var tinDaLuu = await _context.TinDaLuus.FirstOrDefaultAsync(t => t.Id == tinDaLuuId && t.NguoiDungId == userId);

            if (tinDaLuu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin đã lưu hoặc bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction(nameof(Index));
            }

            _context.TinDaLuus.Remove(tinDaLuu);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã bỏ lưu tin tuyển dụng thành công.";
                _logger.LogInformation("User {UserId} bỏ lưu TinDaLuu ID {TinDaLuuId}", userId, tinDaLuuId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi bỏ lưu TinDaLuu ID {TinDaLuuId} cho User {UserId}", tinDaLuuId, userId);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.";
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBaoCaoDashboard(int baoCaoId)
        {
            if (baoCaoId <= 0)
            {
                TempData["ErrorMessage"] = "ID báo cáo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }
            var userId = GetCurrentUserId();
            var baoCao = await _context.BaoCaoViPhams.FirstOrDefaultAsync(b => b.Id == baoCaoId && b.NguoiBaoCaoId == userId);

            if (baoCao == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy báo cáo hoặc bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction(nameof(Index));
            }

            if (baoCao.TrangThaiXuLy != TrangThaiXuLyBaoCao.moi)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xóa báo cáo khi ở trạng thái 'Mới'.";
                return RedirectToAction(nameof(Index));
            }

            _context.BaoCaoViPhams.Remove(baoCao);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa báo cáo thành công.";
                _logger.LogInformation("User {UserId} xóa BaoCao ID {BaoCaoId}", userId, baoCaoId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa BaoCao ID {BaoCaoId} cho User {UserId}", baoCaoId, userId);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDiaDiemMongMuonDashboard(int diaDiemId)
        {
            if (diaDiemId <= 0)
            {
                TempData["ErrorMessage"] = "ID địa điểm mong muốn không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            var diaDiemMongMuon = await _context.DiaDiemMongMuons
                                        .FirstOrDefaultAsync(dd => dd.Id == diaDiemId && dd.NguoiDungId == userId);

            if (diaDiemMongMuon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy địa điểm mong muốn hoặc bạn không có quyền thực hiện thao tác này.";
                return RedirectToAction(nameof(Index));
            }

            _context.DiaDiemMongMuons.Remove(diaDiemMongMuon);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa địa điểm mong muốn thành công.";
                _logger.LogInformation("User {UserId} xóa DiaDiemMongMuon ID {DiaDiemId}", userId, diaDiemId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa DiaDiemMongMuon ID {DiaDiemId} cho User {UserId}", diaDiemId, userId);
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    // Lớp helper để định dạng Mức lương (có thể đặt ở file riêng trong thư mục Helpers hoặc Extensions)
    public static class ViecLamHelper
    {
        public static string FormatMucLuong(LoaiLuong loaiLuong, ulong? luongToiThieu, ulong? luongToiDa, string currencySymbol = "VNĐ")
        {
            string prefix = loaiLuong.GetDisplayName() + ": ";
            if (loaiLuong == LoaiLuong.thoathuan)
            {
                return prefix + "Thỏa thuận";
            }

            string luongMinStr = luongToiThieu.HasValue && luongToiThieu > 0 ? $"{luongToiThieu.Value:N0} {currencySymbol}" : "";
            string luongMaxStr = luongToiDa.HasValue && luongToiDa > 0 ? $"{luongToiDa.Value:N0} {currencySymbol}" : "";

            if (!string.IsNullOrEmpty(luongMinStr) && !string.IsNullOrEmpty(luongMaxStr))
            {
                if (luongToiThieu.HasValue && luongToiDa.HasValue && luongToiThieu.Value == luongToiDa.Value) // Nếu bằng nhau và > 0
                    return prefix + luongMinStr;
                return prefix + $"{luongMinStr} - {luongMaxStr}";
            }
            if (!string.IsNullOrEmpty(luongMinStr))
            {
                return prefix + luongMinStr;
            }
            if (!string.IsNullOrEmpty(luongMaxStr))
            {
                return prefix + $"Đến {luongMaxStr}";
            }
            // Mặc định nếu không có thông tin lương cụ thể hoặc lương = 0
            return prefix + "Thỏa thuận";
        }
    }
}