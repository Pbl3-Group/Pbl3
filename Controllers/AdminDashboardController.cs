using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using Microsoft.AspNetCore.Http;
using HeThongTimViec.ViewModels.Dashboard; // Namespace của JobPostingListViewModelAdmin

// Quan trọng: Đảm bảo JobPostingListViewModelAdmin được định nghĩa đúng
// và có các trường TenCongTyHoacNguoiDang, DiaDiemDisplay, LoaiHinhDisplay, MucLuongDisplay

namespace HeThongTimViec.Controllers
{
    [Route("admin/dashboard")]
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminDashboardController> _logger;
        private const DayOfWeek FirstDayOfWeek = DayOfWeek.Monday;

        public AdminDashboardController(ApplicationDbContext context, ILogger<AdminDashboardController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // [HttpGet("")] // Redirect to overview if needed
        // public IActionResult Index()
        // {
        //     return RedirectToAction(nameof(Overview));
        // }

        [HttpGet("overview")]
        [HttpGet("")] // Make overview the default
        public async Task<IActionResult> Overview()
        {
            _logger.LogInformation("ADMIN DASHBOARD: Loading Overview data for display.");

            try
            {
                var now = DateTime.UtcNow.AddHours(7); // Điều chỉnh múi giờ +07:00
                var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
                var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);
                // NgayTao is assumed to be stored in a way that's comparable to 'now' (local +07:00)
                // Or if NgayTao is UTC, then 'now', 'startOfCurrentMonth', 'startOfPreviousMonth' should be UTC for these specific queries.
                // For simplicity and consistency with other date checks (like NgayHetHan >= now), we'll use +07:00 adjusted dates.

                // 1. Tổng số công việc (tất cả, trừ đã xóa)
                ViewBag.TotalJobPostings = await _context.TinTuyenDungs
                    .AsNoTracking()
                    .CountAsync(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa);

                // Tính % thay đổi so với tháng trước cho công việc MỚI
                int jobPostingsCreatedThisMonth = await _context.TinTuyenDungs
                    .AsNoTracking()
                    .CountAsync(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa &&
                                   t.NgayTao >= startOfCurrentMonth && t.NgayTao <= now);
                int jobPostingsCreatedLastMonth = await _context.TinTuyenDungs
                    .AsNoTracking()
                    .CountAsync(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa &&
                                   t.NgayTao >= startOfPreviousMonth && t.NgayTao < startOfCurrentMonth);
                ViewBag.TotalJobPostingsChange = CalculatePercentageChangeNullable(jobPostingsCreatedThisMonth, jobPostingsCreatedLastMonth);


                // 2. Tổng người dùng (cá nhân và doanh nghiệp)
                var userTypesToCount = new[] { LoaiTaiKhoan.canhan, LoaiTaiKhoan.doanhnghiep };
                ViewBag.TotalUsers = await _context.NguoiDungs
                    .AsNoTracking()
                    .CountAsync(u => userTypesToCount.Contains(u.LoaiTk));

                // 3. Tổng nhà tuyển dụng (chỉ doanh nghiệp)
                ViewBag.TotalEmployers = await _context.NguoiDungs
                    .AsNoTracking()
                    .CountAsync(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep);

                // Tính % thay đổi so với tháng trước cho nhà tuyển dụng MỚI
                int employersRegisteredThisMonth = await _context.NguoiDungs
                    .AsNoTracking()
                    .CountAsync(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep &&
                                   u.NgayTao >= startOfCurrentMonth && u.NgayTao <= now);
                int employersRegisteredLastMonth = await _context.NguoiDungs
                    .AsNoTracking()
                    .CountAsync(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep &&
                                   u.NgayTao >= startOfPreviousMonth && u.NgayTao < startOfCurrentMonth);
                ViewBag.TotalEmployersChange = CalculatePercentageChangeNullable(employersRegisteredThisMonth, employersRegisteredLastMonth);

                // 4. Tỷ lệ tuyển dụng (giữ nguyên)
                int totalApplications = await _context.UngTuyens
                    .AsNoTracking()
                    .CountAsync();
                int approvedApplications = await _context.UngTuyens
                    .AsNoTracking()
                    .CountAsync(u => u.TrangThai == TrangThaiUngTuyen.daduyet);
                ViewBag.RecruitmentRate = totalApplications > 0
                    ? Math.Round((double)approvedApplications / totalApplications * 100, 2)
                    : 0.0;

                // 5. Báo cáo chờ xử lý (giữ nguyên)
                ViewBag.PendingReports = await _context.BaoCaoViPhams
                    .AsNoTracking()
                    .CountAsync(b => b.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi);

                _logger.LogInformation("ADMIN DASHBOARD: Overview data loading completed successfully for view render.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADMIN DASHBOARD: CRITICAL Error loading Overview data for view render.");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi nghiêm trọng khi tải dữ liệu tổng quan. Vui lòng kiểm tra logs.";
            }
            ViewBag.DataLoadedTime = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            return View("~/Views/Dashboard/AdminDashboard.cshtml");
        }

        [HttpGet("job-trend-data")]
        public async Task<IActionResult> GetJobTrendData(string period = "year")
        {
            _logger.LogInformation("API: Fetching job trend data for period: {Period}", period);
            try
            {
                DateTime startDate;
                DateTime now = DateTime.UtcNow; // Use UTC for chart data consistency
                string labelFormat;
                Func<DateTime, DateTime> groupBySelector;
                Func<DateTime, DateTime> dateIncrement;

                switch (period.ToLowerInvariant())
                {
                    case "week":
                        startDate = now.AddDays(-7 * 8 + 1).Date; // Start of 8 weeks ago
                        labelFormat = "'W'dd/MM";
                        groupBySelector = dt => StartOfWeek(dt.Date, FirstDayOfWeek);
                        dateIncrement = dt => dt.AddDays(7);
                        break;
                    case "month": // last 30 days
                        startDate = now.AddDays(-29).Date; // Start of 30 days ago
                        labelFormat = "dd/MM";
                        groupBySelector = dt => dt.Date;
                        dateIncrement = dt => dt.AddDays(1);
                        break;
                    case "year":
                    default: // last 12 months
                        startDate = new DateTime(now.Year, now.Month, 1, 0,0,0, DateTimeKind.Utc).AddMonths(-11);
                        labelFormat = "MM/yyyy";
                        groupBySelector = dt => new DateTime(dt.Year, dt.Month, 1, 0,0,0, DateTimeKind.Utc);
                        dateIncrement = dt => dt.AddMonths(1);
                        period = "year"; // ensure period is set for logging if default
                        break;
                }
                var jobTypesToTrack = new[] { LoaiHinhCongViec.banthoigian, LoaiHinhCongViec.thoivu, LoaiHinhCongViec.linhhoatkhac };
                // Assuming NgayTao is stored in UTC or a compatible format for direct comparison with 'startDate' (which is UTC based)
                var jobPostings = await _context.TinTuyenDungs.AsNoTracking()
                    .Where(t => t.NgayTao >= startDate && jobTypesToTrack.Contains(t.LoaiHinhCongViec))
                    .Select(t => new { t.NgayTao, t.LoaiHinhCongViec }).ToListAsync();

                var groupedData = jobPostings.GroupBy(p => groupBySelector(p.NgayTao)) // NgayTao is already UTC or compatible
                    .Select(g => new { PeriodKey = g.Key, Counts = g.GroupBy(p => p.LoaiHinhCongViec).ToDictionary(sg => sg.Key, sg => sg.Count()) })
                    .OrderBy(g => g.PeriodKey).ToList();

                var labels = new List<string>();
                var datasetBanThoiGian = new List<int>();
                var datasetThoiVu = new List<int>();
                var datasetLinhHoatKhac = new List<int>();

                DateTime currentDate = groupBySelector(startDate); // This will be UTC
                DateTime endDateLoop = groupBySelector(now); // This will be UTC
                // Ensure the loop includes the current period fully
                // We iterate until currentDate is PAST the endDateLoop by one increment for date/month/year
                // For week, we need to ensure the current week is processed
                
                // Adjust loop condition to ensure the current period's end is inclusive
                DateTime loopUntilDate = dateIncrement(endDateLoop);

                while (currentDate < loopUntilDate)
                {
                    labels.Add(currentDate.ToLocalTime().AddHours(7).ToString(labelFormat, CultureInfo.InvariantCulture)); // Display in local time (+7)
                    var dataForPeriod = groupedData.FirstOrDefault(g => g.PeriodKey == currentDate);
                    datasetBanThoiGian.Add(dataForPeriod?.Counts.GetValueOrDefault(LoaiHinhCongViec.banthoigian) ?? 0);
                    datasetThoiVu.Add(dataForPeriod?.Counts.GetValueOrDefault(LoaiHinhCongViec.thoivu) ?? 0);
                    datasetLinhHoatKhac.Add(dataForPeriod?.Counts.GetValueOrDefault(LoaiHinhCongViec.linhhoatkhac) ?? 0);
                    currentDate = dateIncrement(currentDate);
                }

                var chartData = new
                {
                    labels,
                    datasets = new[] {
                        new { label = LoaiHinhCongViec.banthoigian.GetDisplayName() ?? "Bán thời gian", data = datasetBanThoiGian, borderColor = "rgb(255, 99, 132)", backgroundColor = "rgba(255, 99, 132, 0.5)", tension = 0.1 },
                        new { label = LoaiHinhCongViec.thoivu.GetDisplayName() ?? "Thời vụ", data = datasetThoiVu, borderColor = "rgb(54, 162, 235)", backgroundColor = "rgba(54, 162, 235, 0.5)", tension = 0.1 },
                        new { label = LoaiHinhCongViec.linhhoatkhac.GetDisplayName() ?? "Linh hoạt khác", data = datasetLinhHoatKhac, borderColor = "rgb(75, 192, 192)", backgroundColor = "rgba(75, 192, 192, 0.5)", tension = 0.1 }
                    }
                };
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting job trend data for period {Period}.", period);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi máy chủ khi lấy dữ liệu xu hướng công việc." });
            }
        }

        [HttpGet("user-growth-data")]
        public async Task<IActionResult> GetUserGrowthData(string period = "year")
        {
            _logger.LogInformation("API: Fetching user growth data for period: {Period}", period);
            try
            {
                DateTime startDate;
                var now = DateTime.UtcNow; // Use UTC for chart data consistency
                string labelFormat;
                Func<DateTime, DateTime> groupBySelector;
                Func<DateTime, DateTime> dateIncrement;
                switch (period.ToLowerInvariant())
                {
                    case "week": startDate = now.AddDays(-7 * 8 + 1).Date; labelFormat = "'W'dd/MM"; groupBySelector = dt => StartOfWeek(dt.Date, FirstDayOfWeek); dateIncrement = dt => dt.AddDays(7); break;
                    case "month": startDate = now.AddDays(-29).Date; labelFormat = "dd/MM"; groupBySelector = dt => dt.Date; dateIncrement = dt => dt.AddDays(1); break;
                    case "year": default: startDate = new DateTime(now.Year, now.Month, 1, 0,0,0, DateTimeKind.Utc).AddMonths(-11); labelFormat = "MM/yyyy"; groupBySelector = dt => new DateTime(dt.Year, dt.Month, 1, 0,0,0, DateTimeKind.Utc); dateIncrement = dt => dt.AddMonths(1); period = "year"; break;
                }

                var userTypesToTrack = new[] { LoaiTaiKhoan.canhan, LoaiTaiKhoan.doanhnghiep };
                // Assuming NgayTao is stored in UTC or a compatible format
                var users = await _context.NguoiDungs.AsNoTracking()
                   .Where(u => u.NgayTao >= startDate && userTypesToTrack.Contains(u.LoaiTk))
                   .Select(u => new { u.NgayTao, u.LoaiTk }).ToListAsync();

                var groupedData = users.GroupBy(u => groupBySelector(u.NgayTao)) // NgayTao is UTC or compatible
                   .Select(g => new { PeriodKey = g.Key, Counts = g.GroupBy(u => u.LoaiTk).ToDictionary(sg => sg.Key, sg => sg.Count()) })
                   .OrderBy(g => g.PeriodKey).ToList();

                var labels = new List<string>();
                var datasetCanhan = new List<int>();
                var datasetDoanhNghiep = new List<int>();

                DateTime currentDate = groupBySelector(startDate); // UTC
                DateTime endDateLoop = groupBySelector(now);     // UTC
                DateTime loopUntilDate = dateIncrement(endDateLoop);


                while (currentDate < loopUntilDate)
                {
                    labels.Add(currentDate.ToLocalTime().AddHours(7).ToString(labelFormat, CultureInfo.InvariantCulture)); // Display in local time (+7)
                    var dataForPeriod = groupedData.FirstOrDefault(g => g.PeriodKey == currentDate);
                    datasetCanhan.Add(dataForPeriod?.Counts.GetValueOrDefault(LoaiTaiKhoan.canhan) ?? 0);
                    datasetDoanhNghiep.Add(dataForPeriod?.Counts.GetValueOrDefault(LoaiTaiKhoan.doanhnghiep) ?? 0);
                    currentDate = dateIncrement(currentDate);
                }
                var chartData = new
                {
                    labels,
                    datasets = new[] {
                        new { label = LoaiTaiKhoan.canhan.GetDisplayName() ?? "Cá nhân", data = datasetCanhan, borderColor = "rgb(54, 162, 235)", backgroundColor = "rgba(54, 162, 235, 0.5)", tension = 0.1 },
                        new { label = LoaiTaiKhoan.doanhnghiep.GetDisplayName() ?? "Doanh nghiệp", data = datasetDoanhNghiep, borderColor = "rgb(255, 159, 64)", backgroundColor = "rgba(255, 159, 64, 0.5)", tension = 0.1 }
                    }
                };
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting user growth data for period {Period}.", period);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi máy chủ khi lấy dữ liệu biểu đồ tăng trưởng người dùng." });
            }
        }

        [HttpGet("jobs-by-industry")]
        public async Task<IActionResult> GetJobsByIndustry(int topN = 5)
        {
            if (topN <= 0) topN = 5; if (topN > 20) topN = 20;
            _logger.LogInformation("API: Fetching job distribution by top {TopN} industries.", topN);
            try
            {
                var relevantStates = new[] { TrangThaiTinTuyenDung.daduyet };
                var now = DateTime.UtcNow.AddHours(7); // Use +07:00 consistent with Overview for active jobs
                var industryData = await _context.TinTuyenDung_NganhNghes.AsNoTracking()
                    .Include(tnn => tnn.TinTuyenDung).Include(tnn => tnn.NganhNghe)
                    .Where(tnn => tnn.TinTuyenDung != null && relevantStates.Contains(tnn.TinTuyenDung.TrangThai) &&
                                  tnn.TinTuyenDung.TrangThai != TrangThaiTinTuyenDung.daxoa && tnn.TinTuyenDung.TrangThai != TrangThaiTinTuyenDung.datuyen &&
                                  (tnn.TinTuyenDung.NgayHetHan == null || tnn.TinTuyenDung.NgayHetHan >= now) && // Compare NgayHetHan with +07:00 now
                                  tnn.NganhNghe != null && !string.IsNullOrEmpty(tnn.NganhNghe.Ten))
                    .GroupBy(tnn => tnn.NganhNghe.Ten)
                    .Select(g => new { IndustryName = g.Key, JobCount = g.Select(i => i.TinTuyenDungId).Distinct().Count() })
                    .OrderByDescending(g => g.JobCount).ToListAsync();

                if (!industryData.Any()) return Ok(new { labels = new List<string>(), datasets = new[] { new { data = new List<int>(), backgroundColor = new List<string>() } } });

                var topIndustries = industryData.Take(topN).ToList();
                int otherCount = industryData.Skip(topN).Sum(i => i.JobCount);
                int otherIndustryCount = Math.Max(0, industryData.Count - topN);

                var chartLabels = topIndustries.Select(i => i.IndustryName).ToList();
                var chartDataPoints = topIndustries.Select(i => i.JobCount).ToList();

                if (otherCount > 0 && otherIndustryCount > 0) { chartLabels.Add($"Ngành khác ({otherIndustryCount})"); chartDataPoints.Add(otherCount); }

                var backgroundColors = GetChartColors(chartLabels.Count);
                var doughnutChartData = new
                {
                    labels = chartLabels,
                    datasets = new[] { new { label = "Số lượng bài đăng", data = chartDataPoints, backgroundColor = backgroundColors, hoverOffset = 4, borderColor = "#ffffff" } }
                };
                return Ok(doughnutChartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting job distribution by industry (TopN={TopN}).", topN);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi máy chủ khi lấy dữ liệu phân bổ việc làm theo ngành." });
            }
        }

        [HttpGet("job-status-distribution")]
        public async Task<IActionResult> GetJobPostingStatusDistribution()
        {
            _logger.LogInformation("API: Fetching job posting status distribution.");
            try
            {
                var now = DateTime.UtcNow.AddHours(7); // Use +07:00 for expired check consistency
                var allPostings = await _context.TinTuyenDungs.AsNoTracking().Select(t => new { t.TrangThai, t.NgayHetHan }).ToListAsync();

                var statusCounts = allPostings
                    // If NgayHetHan is UTC, 'now' should be UTC here. If NgayHetHan is +07:00, 'now' (+07:00) is fine.
                    // Assuming NgayHetHan is comparable with 'now' (+07:00)
                    .Select(t => (t.TrangThai == TrangThaiTinTuyenDung.daduyet && t.NgayHetHan.HasValue && t.NgayHetHan < now) ? TrangThaiTinTuyenDung.hethan : t.TrangThai)
                    .GroupBy(adjustedStatus => adjustedStatus)
                    .Select(g => new { StatusEnum = g.Key, Count = g.Count() })
                    .OrderBy(x => x.StatusEnum).ToList();

                if (!statusCounts.Any()) return Ok(new { labels = new List<string>(), datasets = new[] { new { data = new List<int>(), backgroundColor = new List<string>() } } });

                var desiredStatuses = new[] { TrangThaiTinTuyenDung.daduyet, TrangThaiTinTuyenDung.choduyet, TrangThaiTinTuyenDung.hethan, TrangThaiTinTuyenDung.datuyen, TrangThaiTinTuyenDung.bituchoi, TrangThaiTinTuyenDung.daxoa, TrangThaiTinTuyenDung.taman };
                var allChartLabels = new List<string>();
                var allChartDataPoints = new List<int>();

                foreach (var status in desiredStatuses)
                {
                    allChartLabels.Add(status.GetDisplayName() ?? status.ToString());
                    allChartDataPoints.Add(statusCounts.FirstOrDefault(sc => sc.StatusEnum == status)?.Count ?? 0);
                }

                var backgroundColors = GetChartColors(allChartLabels.Count);
                var doughnutChartData = new
                {
                    labels = allChartLabels,
                    datasets = new[] { new { label = "Số lượng", data = allChartDataPoints, backgroundColor = backgroundColors, hoverOffset = 4, borderColor = "#ffffff" } }
                };
                return Ok(doughnutChartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting job posting status distribution.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi máy chủ khi lấy dữ liệu trạng thái tin đăng." });
            }
        }

        [HttpGet("latest-jobs")]
        public async Task<IActionResult> GetLatestJobs(int count = 7)
        {
            if (count <= 0) count = 7;
            if (count > 20) count = 20;
            _logger.LogInformation("API: Fetching top {Count} latest ACTIVE jobs for dashboard (using JobPostingListViewModelAdmin).", count);

            try
            {
                var now = DateTime.UtcNow.AddHours(7); // Use +07:00 for active job check

                var query = _context.TinTuyenDungs
                    .AsNoTracking()
                    .Include(t => t.UngTuyens)
                    .Include(t => t.NguoiDang)
                        .ThenInclude(nd => nd.HoSoDoanhNghiep)
                    .Include(t => t.QuanHuyen)
                    .Include(t => t.ThanhPho);

                var latestJobsData = await query
                    .Where(t =>
                        t.TrangThai == TrangThaiTinTuyenDung.daduyet &&
                        (t.NgayHetHan == null || t.NgayHetHan >= now) && // Compare NgayHetHan with +07:00 now
                        t.TrangThai != TrangThaiTinTuyenDung.daxoa &&
                        t.TrangThai != TrangThaiTinTuyenDung.datuyen
                    )
                    .OrderByDescending(t => t.NgayDang) // NgayDang could be creation or approval date
                    .Take(count)
                    .Select(t => new JobPostingListViewModelAdmin
                    {
                        Id = t.Id,
                        TieuDe = t.TieuDe ?? "Không có tiêu đề",
                        TrangThai = t.TrangThai,
                        NgayDang = t.NgayDang, // Assuming NgayDang is already in appropriate local time or UTC for display
                        SoUngVien = t.UngTuyens.Any() ? t.UngTuyens.Count() : 0,
                        // LuotXem = t.LuotXem, 

                        TenCongTyHoacNguoiDang = t.NguoiDang != null ?
                            (t.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && t.NguoiDang.HoSoDoanhNghiep != null ?
                                t.NguoiDang.HoSoDoanhNghiep.TenCongTy : t.NguoiDang.HoTen)
                            : "Không rõ",

                        DiaDiemDisplay = t.QuanHuyen != null && t.ThanhPho != null ?
                            $"{t.QuanHuyen.Ten}, {t.ThanhPho.Ten}" :
                            (t.ThanhPho != null ? t.ThanhPho.Ten : "Không rõ"),

                        LoaiHinhDisplay = t.LoaiHinhCongViec.GetDisplayName(),
                        MucLuongDisplay = AdminDashboardController.FormatSalary(t.LuongToiThieu, t.LuongToiDa, t.LoaiLuong)
                    })
                    .ToListAsync();

                _logger.LogInformation("API: Fetched {JobCount} latest ACTIVE jobs successfully (using JobPostingListViewModelAdmin).", latestJobsData.Count);
                return Ok(latestJobsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: CRITICAL Error getting latest jobs (Count={Count}) using JobPostingListViewModelAdmin. ExceptionType: {ExceptionType}, Message: {ExceptionMessage}, StackTrace: {StackTrace}", count, ex.GetType().Name, ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = $"Lỗi máy chủ khi tải danh sách công việc mới nhất (Admin VM): {ex.Message}",
#if DEBUG
                        errorDetails = ex.ToString()
#else
                        errorDetails = "Chi tiết lỗi đã được ghi lại trong logs của máy chủ."
#endif
                    });
            }
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(int countPerType = 7)
        {
            if (countPerType <= 0) countPerType = 7; if (countPerType > 30) countPerType = 30;
            _logger.LogInformation("API: Fetching top {Count} recent activities (split by type).", countPerType);
            try
            {
                var nowForRelativeTime = DateTime.UtcNow; // Use UTC for relative time calculation
                var applications = new List<RecentActivityItem>();
                var generalActivities = new List<RecentActivityItem>();
                try
                {
                    // Assuming NgayNop, NgayTao are stored in UTC or timezone-aware
                    var recentApplications = await _context.UngTuyens.AsNoTracking()
                        .Include(ut => ut.UngVien).Include(ut => ut.TinTuyenDung)
                        .OrderByDescending(ut => ut.NgayNop).Take(countPerType)
                        .Select(ut => new RecentActivityItem { Type = "Ứng tuyển mới", Description = $"{(ut.UngVien != null ? (ut.UngVien.HoTen ?? ut.UngVien.Email) : "Ứng viên")} đã ứng tuyển vào {(ut.TinTuyenDung != null && !string.IsNullOrEmpty(ut.TinTuyenDung.TieuDe) ? $"'{ut.TinTuyenDung.TieuDe}'" : "công việc")}", Timestamp = ut.NgayNop, Icon = "fas fa-file-signature text-primary", Link = Url.Action("ChiTietCaNhan", "NguoiDung", new { id = ut.Id }) ?? "#" }).ToListAsync();
                    applications.AddRange(recentApplications);
                }
                catch (Exception ex) { _logger.LogError(ex, "Error fetching recent applications for activity feed."); }
                try
                {
                    var recentUsers = await _context.NguoiDungs.AsNoTracking()
                       .Where(u => u.LoaiTk != LoaiTaiKhoan.quantrivien)
                       .OrderByDescending(u => u.NgayTao).Take(countPerType)
                       .Select(u => new RecentActivityItem
{
    Type = u.LoaiTk == LoaiTaiKhoan.canhan ? "Ứng viên mới" : "Doanh nghiệp mới",
    Description = $"{(u.LoaiTk == LoaiTaiKhoan.canhan ? "Ứng viên" : "NTD:")} {(string.IsNullOrEmpty(u.HoTen) ? u.Email : u.HoTen)} đã đăng ký.",
    Timestamp = u.NgayTao,
    Icon = u.LoaiTk == LoaiTaiKhoan.canhan ? "fas fa-user-plus text-success" : "fas fa-building text-info",

    // Chọn action phù hợp dựa theo loại tài khoản
    Link = Url.Action(
        u.LoaiTk == LoaiTaiKhoan.canhan ? "ChiTietCaNhan" : "ChiTietDoanhNghiep",
        "NguoiDung",
        new { id = u.Id }
    ) ?? "#"
})
.ToListAsync();
                    generalActivities.AddRange(recentUsers);
                }
                catch (Exception ex) { _logger.LogError(ex, "Error fetching recent users for activity feed."); }
                try
                {
                    var recentPosts = await _context.TinTuyenDungs.AsNoTracking()
                       .Include(t => t.NguoiDang)
                       .OrderByDescending(t => t.NgayTao).Take(countPerType)
                       .Select(t => new RecentActivityItem { Type = "Tin đăng mới", Description = $"'{t.TieuDe ?? "(Chưa có tiêu đề)"}' bởi {(t.NguoiDang != null ? (string.IsNullOrEmpty(t.NguoiDang.HoTen) ? t.NguoiDang.Email : t.NguoiDang.HoTen) : "Không rõ")}", Timestamp = t.NgayTao, Icon = "fas fa-bullhorn text-warning", Link = Url.Action("ChiTiet", "QuanLyTinDang", new { id = t.Id }) ?? "#" }).ToListAsync();
                    generalActivities.AddRange(recentPosts);
                }
                catch (Exception ex) { _logger.LogError(ex, "Error fetching recent job posts for activity feed."); }

                var sortedGeneralActivities = generalActivities.OrderByDescending(a => a.Timestamp).Take(countPerType).ToList();
                Func<RecentActivityItem, object> formatActivity = a => new { a.Type, a.Description, RelativeTime = GetRelativeTime(a.Timestamp, nowForRelativeTime), a.Icon, a.Link, Timestamp = a.Timestamp.ToString("o") };

                var formattedApplications = applications.Select(formatActivity).ToList();
                var formattedGeneralActivity = sortedGeneralActivities.Select(formatActivity).ToList();
                return Ok(new { applications = formattedApplications, generalActivity = formattedGeneralActivity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: CRITICAL ERROR processing recent activity data.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi máy chủ khi xử lý hoạt động gần đây." });
            }
        }

        // --- Helper Methods ---
        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            // Ensure dt is treated as Date for DayOfWeek calculation
            DateTime dateOnly = dt.Date;
            int diff = (7 + (dateOnly.DayOfWeek - startOfWeek)) % 7;
            return dateOnly.AddDays(-1 * diff);
        }

        private static string FormatSalary(ulong? min, ulong? max, LoaiLuong type)
        {
            if (type == LoaiLuong.thoathuan)
            {
                return type.GetDisplayName() ?? "Thỏa thuận";
            }
            string luongMinStr = min.HasValue ? $"{min.Value:N0}" : string.Empty;
            string luongMaxStr = max.HasValue ? $"{max.Value:N0}" : string.Empty;
            string donVi = type switch
            {
                LoaiLuong.theogio => "/giờ",
                LoaiLuong.theongay => "/ngày",
                LoaiLuong.theoca => "/ca",
                LoaiLuong.theothang => "/tháng",
                LoaiLuong.theoduan => "/dự án",
                _ => ""
            };
            string baseSalaryString;
            if (min.HasValue && max.HasValue) baseSalaryString = (min.Value == max.Value) ? $"{luongMinStr} VND" : $"{luongMinStr} - {luongMaxStr} VND";
            else if (min.HasValue) baseSalaryString = $"Từ {luongMinStr} VND";
            else if (max.HasValue) baseSalaryString = $"Đến {luongMaxStr} VND";
            else return "Không công khai";
            return $"{baseSalaryString}{donVi}";
        }

        private static List<string> GetChartColors(int count)
        {
            var baseColors = new List<string> {
                 "rgba(78, 115, 223, 0.9)","rgba(28, 200, 138, 0.9)","rgba(54, 185, 204, 0.9)",
                 "rgba(246, 194, 62, 0.9)","rgba(231, 74, 59, 0.9)","rgba(133, 135, 150, 0.9)",
                 "rgba(110, 99, 198, 0.9)","rgba(253, 126, 20, 0.9)","rgba(220, 53, 69, 0.9)",
                 "rgba(25, 135, 84, 0.9)","rgba(13, 110, 253, 0.9)","rgba(255, 193, 7, 0.9)",
            };
            var colors = new List<string>();
            if (count <= 0) return colors;
            for (int i = 0; i < count; i++) colors.Add(baseColors[i % baseColors.Count]);
            return colors;
        }

        private static string GetRelativeTime(DateTime dateTime, DateTime now)
        {
            // Ensure both datetimes are UTC for consistent comparison
            DateTime dtUtc = dateTime.Kind switch
            {
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), // Assume UTC if unspecified
                _ => dateTime // Already UTC
            };
            DateTime nowUtc = now.Kind == DateTimeKind.Utc ? now : now.ToUniversalTime();

            TimeSpan timeSpan = nowUtc - dtUtc;

            if (timeSpan.TotalSeconds < 0) // Future date
            {
                return timeSpan.TotalSeconds > -5 ? "ngay bây giờ" : $"trong {(int)Math.Abs(timeSpan.TotalSeconds)} giây";
            }
            if (timeSpan.TotalSeconds < 2) return "vừa xong";
            if (timeSpan.TotalSeconds < 60) return $"{(int)timeSpan.TotalSeconds} giây trước";
            if (timeSpan.TotalMinutes < 2) return "1 phút trước";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 2) return "1 giờ trước";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";

            // For days, weeks, months, years, it's often better to use local time date part for "yesterday", etc.
            // However, to keep it simple and consistent with UTC comparison for smaller units:
            if (timeSpan.TotalDays < 2) return "hôm qua"; // Approximate, could be "1 day ago"
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} ngày trước";

            int weeks = (int)Math.Floor(timeSpan.TotalDays / 7);
            if (weeks < 4) return weeks <= 1 ? "1 tuần trước" : $"{weeks} tuần trước";

            int months = (int)Math.Floor(timeSpan.TotalDays / 30.4375); // Average days in a month
            if (months < 12) return months <= 1 ? "1 tháng trước" : $"{months} tháng trước";

            int years = (int)Math.Floor(timeSpan.TotalDays / 365.25); // Account for leap years
            return years <= 1 ? "1 năm trước" : $"{years} năm trước";
        }

        private static double? CalculatePercentageChangeNullable(int current, int previous)
        {
            if (previous == 0)
            {
                return (current > 0) ? (double?)100.0 : (current == 0 ? (double?)0.0 : null); // 100% if current > 0, 0% if current is 0, null if new but negative (should not happen for counts)
            }
            return Math.Round(((double)current - previous) / previous * 100, 1);
        }


        private record RecentActivityItem
        {
            public string Type { get; init; } = string.Empty;
            public string Description { get; init; } = string.Empty;
            public DateTime Timestamp { get; init; }
            public string Icon { get; init; } = string.Empty;
            public string? Link { get; init; }
        }
    }
}