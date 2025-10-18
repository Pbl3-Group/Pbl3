using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using ClosedXML.Excel; // Thư viện để làm việc với Excel
using System.IO;       // Để làm việc với MemoryStream

// Giả sử có chính sách yêu cầu vai trò "quantrivien"
// [Authorize(Roles = "quantrivien")] 
public class BaoCaoThongKeController : Controller
{
    private readonly ApplicationDbContext _context;

    public BaoCaoThongKeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("admin/reports")] // URL thân thiện
    public async Task<IActionResult> Index(string tab = "tongquan")
    {
        var viewModel = new BaoCaoThongKeViewModel { ActiveTab = tab };

        switch (tab.ToLower())
        {
            case "congviec":
                await LoadCongViecDataAsync(viewModel);
                break;
            case "ungvien":
                await LoadUngVienDataAsync(viewModel);
                break;
            case "nhatuyendung":
                await LoadNhaTuyenDungDataAsync(viewModel);
                break;
            case "tongquan":
            default:
                await LoadTongQuanDataAsync(viewModel);
                viewModel.ActiveTab = "tongquan"; // Đảm bảo tab đúng
                break;
        }

        return View(viewModel);
    }

    #region Data Loading Methods

    private async Task LoadTongQuanDataAsync(BaoCaoThongKeViewModel viewModel)
    {
        // 1. KPI Cards
        viewModel.TongSoCongViec = await _context.TinTuyenDungs
        .CountAsync(t => 
            t.TrangThai == TrangThaiTinTuyenDung.daduyet &&
            (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
        );
        viewModel.TongSoNguoiDung = await _context.NguoiDungs.CountAsync();
        viewModel.BaoCaoViPhamMoi = await _context.BaoCaoViPhams.CountAsync(b => b.TrangThaiXuLy == TrangThaiXuLyBaoCao.moi);

        var jobsDaTuyen = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.datuyen);
        var jobsConsideredDone = await _context.TinTuyenDungs.CountAsync(t =>
            t.TrangThai == TrangThaiTinTuyenDung.datuyen ||
            t.TrangThai == TrangThaiTinTuyenDung.hethan);
        viewModel.TiLeTuyenDung = jobsConsideredDone > 0 ? (double)jobsDaTuyen / jobsConsideredDone * 100 : 0;
        
        // Chỉ số nâng cao cho tab tổng quan
        var closedJobDates = await _context.TinTuyenDungs
    .Where(t => t.TrangThai == TrangThaiTinTuyenDung.datuyen)
    .Select(t => new { t.NgayDang, NgayCapNhat = t.NgayCapNhat })
    .ToListAsync();

// Bước 2: Dữ liệu đã được tải vào bộ nhớ. Bây giờ thực hiện tính toán bằng C#.
// Phép tính này không còn liên quan đến database nữa.
var closedJobsDurationsInDays = closedJobDates
    .Select(d => (d.NgayCapNhat - d.NgayDang).TotalDays)
    .ToList();

// Bước 3: Gán kết quả cuối cùng.
viewModel.ThoiGianTrungBinhTuyenDung = closedJobsDurationsInDays.Any() ? closedJobsDurationsInDays.Average() : 0;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        viewModel.NguoiDungHoatDong30Ngay = await _context.NguoiDungs
            .CountAsync(u => u.LanDangNhapCuoi.HasValue && u.LanDangNhapCuoi.Value >= thirtyDaysAgo);

        viewModel.SoHoSoDoanhNghiepChoXacMinh = await _context.HoSoDoanhNghieps
            .CountAsync(h => !h.DaXacMinh);


        // 2. Charts
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var labels = Enumerable.Range(0, 6)
            .Select(i => DateTime.UtcNow.AddMonths(-i))
            .Select(d => d.ToString("MM/yyyy"))
            .Reverse()
            .ToList();

        // Chart 1: Job Trend
        var banThoiGianData = new int[6];
        var thoiVuData = new int[6];
        var allJobs = await _context.TinTuyenDungs
            .Where(t => t.NgayDang >= sixMonthsAgo)
            .Select(t => new { t.NgayDang, t.LoaiHinhCongViec })
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var month = DateTime.UtcNow.AddMonths(-5 + i);
            banThoiGianData[i] = allJobs.Count(t => t.LoaiHinhCongViec == LoaiHinhCongViec.banthoigian && t.NgayDang.Month == month.Month && t.NgayDang.Year == month.Year);
            thoiVuData[i] = allJobs.Count(t => t.LoaiHinhCongViec == LoaiHinhCongViec.thoivu && t.NgayDang.Month == month.Month && t.NgayDang.Year == month.Year);
        }

        viewModel.JobTrendChart = new ChartJsData
        {
            Labels = labels,
            Datasets = new List<ChartJsDataset>
            {
                new ChartJsDataset { Label = "Bán thời gian", Data = banThoiGianData.ToList(), BorderColor = new List<string> { "#36A2EB" }, Tension = 0.3 },
                new ChartJsDataset { Label = "Thời vụ", Data = thoiVuData.ToList(), BorderColor = new List<string> { "#FF6384" }, Tension = 0.3 }
            }
        };

        // Chart 2: Job Status
        var jobStatusData = await _context.TinTuyenDungs
            .GroupBy(t => t.TrangThai)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        viewModel.JobStatusChart = new ChartJsData
        {
            Labels = jobStatusData.Select(d => d.Status.GetDisplayName()).ToList(),
            Datasets = new List<ChartJsDataset>
            {
                new ChartJsDataset
                {
                    Data = jobStatusData.Select(d => d.Count).ToList(),
                    BackgroundColor = new List<string> { "#4BC0C0", "#FFCE56", "#FF6384", "#36A2EB", "#9966FF", "#F7464A", "#E7E9ED" }
                }
            }
        };

        // Chart 3: Geographic Distribution
        var jobLocationData = await _context.TinTuyenDungs
            .Include(t => t.ThanhPho)
            .GroupBy(t => t.ThanhPho.Ten)
            .Select(g => new { Location = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        viewModel.JobLocationChart = new ChartJsData
        {
            Labels = jobLocationData.Select(d => d.Location).ToList(),
            Datasets = new List<ChartJsDataset>
            {
                new ChartJsDataset
                {
                    Data = jobLocationData.Select(d => d.Count).ToList(),
                    BackgroundColor = new List<string> { "#4CAF50", "#2196F3", "#FFC107", "#9C27B0", "#FF5722" }
                }
            }
        };

        // Chart 4: Candidate Trend
        var caNhanData = new int[6];
        var ungVienData = new int[6]; // Distinct applicants
        var allCaNhan = await _context.NguoiDungs
            .Where(u => u.LoaiTk == LoaiTaiKhoan.canhan && u.NgayTao >= sixMonthsAgo)
            .Select(u => u.NgayTao)
            .ToListAsync();
        var allUngTuyen = await _context.UngTuyens
            .Where(ut => ut.NgayNop >= sixMonthsAgo)
            .Select(ut => new { ut.UngVienId, ut.NgayNop })
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var month = DateTime.UtcNow.AddMonths(-5 + i);
            caNhanData[i] = allCaNhan.Count(d => d.Month == month.Month && d.Year == month.Year);
            ungVienData[i] = allUngTuyen.Where(ut => ut.NgayNop.Month == month.Month && ut.NgayNop.Year == month.Year).Select(ut => ut.UngVienId).Distinct().Count();
        }

        viewModel.CandidateTrendChart = new ChartJsData
        {
            Labels = labels,
            Datasets = new List<ChartJsDataset>
            {
                new ChartJsDataset { Label = "Tài khoản cá nhân mới", Data = caNhanData.ToList(), BorderColor = new List<string> { "#4CAF50" }, Tension = 0.3 },
                new ChartJsDataset { Label = "Ứng viên có ứng tuyển", Data = ungVienData.ToList(), BorderColor = new List<string> { "#FF9800" }, Tension = 0.3 }
            }
        };
    }

    // File: Controllers/BaoCaoThongKeController.cs

private async Task LoadCongViecDataAsync(BaoCaoThongKeViewModel viewModel)
{
    // <<< TẠO TRUY VẤN CƠ SỞ ĐỂ LỌC CÁC CÔNG VIỆC ĐANG HOẠT ĐỘNG >>>
    var activeJobsQuery = _context.TinTuyenDungs
        .Where(t => 
            t.TrangThai == TrangThaiTinTuyenDung.daduyet &&
            (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date)
        );

    // Chỉ số nâng cao - đã được cập nhật để dùng activeJobsQuery
    viewModel.SoTinTuyenDungBiAnDoViPham = await _context.TinTuyenDungs
        .CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.taman && t.BaoCaoViPhams.Any()); // Giữ nguyên vì đây là trạng thái khác

    // Ngành nghề hot nhất được tính trên các công việc đang hoạt động
    var hotIndustry = await activeJobsQuery
        .SelectMany(t => t.TinTuyenDungNganhNghes) // Lấy các ngành nghề từ các tin đang hoạt động
        .GroupBy(tn => tn.NganhNghe.Ten)
        .Select(g => new { Industry = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .FirstOrDefaultAsync();
    if (hotIndustry != null)
    {
        viewModel.NganhNgheHotNhat = new KeyValuePair<string, int>(hotIndustry.Industry, hotIndustry.Count);
    }

    // Top nhà tuyển dụng được tính trên các công việc đang hoạt động
    viewModel.Top5NhaTuyenDungDangNhieuTinNhat = await activeJobsQuery
        .GroupBy(t => new { t.NguoiDangId, TenCongTy = t.NguoiDang.HoSoDoanhNghiep.TenCongTy ?? t.NguoiDang.HoTen })
        .Select(g => new TopEmployerByJobsViewModel
        {
            EmployerId = g.Key.NguoiDangId,
            TenCongTy = g.Key.TenCongTy,
            SoLuongTinDang = g.Count()
        })
        .OrderByDescending(x => x.SoLuongTinDang)
        .Take(5)
        .ToListAsync();

    // Biểu đồ
    // Phân phối loại hình công việc (Job Type) được tính trên các công việc đang hoạt động
    var jobTypeData = await activeJobsQuery
        .GroupBy(t => t.LoaiHinhCongViec)
        .Select(g => new { Type = g.Key, Count = g.Count() })
        .ToListAsync();

    viewModel.JobTypeDistributionChart = new ChartJsData
    {
        Labels = jobTypeData.Select(d => d.Type.GetDisplayName()).ToList(),
        Datasets = new List<ChartJsDataset>
        {
            new ChartJsDataset { Data = jobTypeData.Select(d => d.Count).ToList(), BackgroundColor = new List<string> { "#36A2EB", "#FF6384", "#FFCE56" } }
        }
    };

    // Các ngành nghề phổ biến được tính trên các công việc đang hoạt động
    var industryData = await activeJobsQuery
        .SelectMany(t => t.TinTuyenDungNganhNghes)
        .GroupBy(tn => tn.NganhNghe.Ten)
        .Select(g => new { Industry = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .Take(7)
        .ToListAsync();

    viewModel.PopularIndustriesChart = new ChartJsData
    {
        Labels = industryData.Select(d => d.Industry).ToList(),
        Datasets = new List<ChartJsDataset>
        {
            new ChartJsDataset { Label = "Số lượng tin đăng", Data = industryData.Select(d => d.Count).ToList(), BackgroundColor = new List<string> { "rgba(54, 162, 235, 0.6)" }, BorderColor = new List<string> { "rgba(54, 162, 235, 1)" } }
        }
    };

    // Loại người đăng được tính trên các công việc đang hoạt động
    var posterTypeData = await activeJobsQuery
        .GroupBy(t => t.NguoiDang.LoaiTk)
        .Select(g => new { Type = g.Key, Count = g.Count() })
        .ToListAsync();

    viewModel.PosterTypeChart = new ChartJsData
    {
        Labels = posterTypeData.Select(d => d.Type.GetDisplayName()).ToList(),
        Datasets = new List<ChartJsDataset>
        {
            new ChartJsDataset { Data = posterTypeData.Select(d => d.Count).ToList(), BackgroundColor = new List<string> { "#FF6384", "#36A2EB", "#4BC0C0" } }
        }
    };
}
    private async Task LoadUngVienDataAsync(BaoCaoThongKeViewModel viewModel)
    {
        // Chỉ số nâng cao
        var totalIndividuals = await _context.NguoiDungs.CountAsync(u => u.LoaiTk == LoaiTaiKhoan.canhan);
        if (totalIndividuals > 0)
        {
            var completedProfiles = await _context.HoSoUngViens.CountAsync();
            viewModel.TiLeHoanThanhHoSo = (double)completedProfiles / totalIndividuals * 100;

            var totalApplications = await _context.UngTuyens.CountAsync();
            var totalApplicants = await _context.UngTuyens.Select(u => u.UngVienId).Distinct().CountAsync();
            viewModel.SoUngTuyenTrungBinhMoiUngVien = totalApplicants > 0 ? (double)totalApplications / totalApplicants : 0;
        }

        viewModel.Top5UngVienTichCucNhat = await _context.UngTuyens
            .GroupBy(u => new { u.UngVienId, u.UngVien.HoTen })
            .Select(g => new TopCandidateByApplicationsViewModel
            {
                CandidateId = g.Key.UngVienId,
                HoTen = g.Key.HoTen,
                SoLuongUngTuyen = g.Count()
            })
            .OrderByDescending(x => x.SoLuongUngTuyen)
            .Take(5)
            .ToListAsync();
        
        // Biểu đồ
        var allIndividualsWithDob = await _context.NguoiDungs
            .Where(u => u.LoaiTk == LoaiTaiKhoan.canhan && u.NgaySinh.HasValue)
            .Select(u => u.NgaySinh)
            .ToListAsync();

        var ageGroups = new Dictionary<string, int> { { "18-22 tuổi", 0 }, { "23-27 tuổi", 0 }, { "28-35 tuổi", 0 }, { "36+ tuổi", 0 } };
        foreach (var dob in allIndividualsWithDob)
        {
            int age = DateTime.UtcNow.Year - dob!.Value.Year;
            if (dob > DateTime.UtcNow.AddYears(-age)) age--;
            if (age >= 18 && age <= 22) ageGroups["18-22 tuổi"]++;
            else if (age >= 23 && age <= 27) ageGroups["23-27 tuổi"]++;
            else if (age >= 28 && age <= 35) ageGroups["28-35 tuổi"]++;
            else if (age >= 36) ageGroups["36+ tuổi"]++;
        }

        viewModel.CandidateAgeChart = new ChartJsData
        {
            Labels = ageGroups.Keys.ToList(),
            Datasets = new List<ChartJsDataset>
            { new ChartJsDataset { Data = ageGroups.Values.ToList(), BackgroundColor = new List<string> { "#36A2EB", "#FFCE56", "#4BC0C0", "#FF6384" } } }
        };

        var experienceData = await _context.HoSoUngViens
            .Where(h => h.NguoiDung.LoaiTk == LoaiTaiKhoan.canhan)
            .GroupBy(h => !string.IsNullOrEmpty(h.TieuDeHoSo) && h.TieuDeHoSo.Contains("kinh nghiệm") ? "Có kinh nghiệm" : "Chưa có/Không rõ")
            .Select(g => new { Group = g.Key, Count = g.Count() })
            .ToListAsync();
            
        viewModel.CandidateExperienceChart = new ChartJsData
        {
            Labels = experienceData.Select(d => d.Group).ToList(),
            Datasets = new List<ChartJsDataset>
            { new ChartJsDataset { Data = experienceData.Select(d => d.Count).ToList(), BackgroundColor = new List<string> { "#9966FF", "#FF9F40" } } }
        };
    }

    private async Task LoadNhaTuyenDungDataAsync(BaoCaoThongKeViewModel viewModel)
    {
        // Chỉ số nâng cao
        viewModel.SoNhaTuyenDungDaXacMinh = await _context.HoSoDoanhNghieps.CountAsync(h => h.DaXacMinh);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        viewModel.SoNhaTuyenDungMoi30Ngay = await _context.NguoiDungs.CountAsync(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep && u.NgayTao >= thirtyDaysAgo);

        // Biểu đồ
        var allEmployers = await _context.HoSoDoanhNghieps.AsNoTracking().ToListAsync();
        var sizeGroups = allEmployers
            .Where(e => !string.IsNullOrEmpty(e.QuyMoCongTy))
            .GroupBy(e => e.QuyMoCongTy)
            .Select(g => new { Size = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        viewModel.EmployerSizeChart = new ChartJsData
        {
            Labels = sizeGroups.Select(g => g.Size!).ToList(),
            Datasets = new List<ChartJsDataset>
            { new ChartJsDataset { Data = sizeGroups.Select(g => g.Count).ToList(), BackgroundColor = new List<string> { "#4CAF50", "#2196F3", "#FFC107", "#E91E63", "#795548" } } }
        };

        var employerIndustries = await _context.TinTuyenDungs
            .Where(t => t.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep)
            .SelectMany(t => t.TinTuyenDungNganhNghes)
            .GroupBy(tn => tn.NganhNghe.Ten)
            .Select(g => new { Industry = g.Key, Count = g.Select(i => i.TinTuyenDung.NguoiDangId).Distinct().Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        viewModel.EmployerIndustryChart = new ChartJsData
        {
            Labels = employerIndustries.Select(e => e.Industry).ToList(),
            Datasets = new List<ChartJsDataset>
            { new ChartJsDataset { Label = "Số lượng NTD", Data = employerIndustries.Select(e => e.Count).ToList(), BackgroundColor = new List<string> { "rgba(255, 99, 132, 0.6)" }, } }
        };

        var employerLocations = await _context.NguoiDungs
            .Where(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep && u.ThanhPhoId.HasValue)
            .GroupBy(u => u.ThanhPho.Ten)
            .Select(g => new { Location = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        viewModel.EmployerLocationChart = new ChartJsData
        {
            Labels = employerLocations.Select(e => e.Location).ToList(),
            Datasets = new List<ChartJsDataset>
            { new ChartJsDataset { Data = employerLocations.Select(e => e.Count).ToList(), BackgroundColor = new List<string> { "#36A2EB", "#FF6384", "#FFCE56", "#4BC0C0", "#9966FF" } } }
        };
    }

    #endregion

    #region Export Methods
    
    [HttpGet]
    [Route("admin/reports/export-excel")]
    public async Task<IActionResult> ExportToExcel()
    {
        using (var workbook = new XLWorkbook())
        {
            // === Sheet 1: Tin tuyển dụng ===
            var jobs = await _context.TinTuyenDungs
                .Include(t => t.NguoiDang)
                .Include(t => t.ThanhPho)
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            var jobSheet = workbook.Worksheets.Add("TinTuyenDung");
            var jobHeaders = new string[] { "ID", "Tiêu đề", "Người đăng", "Loại hình", "Trạng thái", "Địa điểm", "Ngày đăng", "Ngày hết hạn" };
            for(int i = 0; i < jobHeaders.Length; i++)
            {
                jobSheet.Cell(1, i + 1).Value = jobHeaders[i];
            }
            
            int jobRow = 2;
            foreach (var job in jobs)
            {
                jobSheet.Cell(jobRow, 1).Value = job.Id;
                jobSheet.Cell(jobRow, 2).Value = job.TieuDe;
                jobSheet.Cell(jobRow, 3).Value = job.NguoiDang.HoTen;
                jobSheet.Cell(jobRow, 4).Value = job.LoaiHinhCongViec.GetDisplayName();
                jobSheet.Cell(jobRow, 5).Value = job.TrangThai.GetDisplayName();
                jobSheet.Cell(jobRow, 6).Value = job.ThanhPho.Ten;
                jobSheet.Cell(jobRow, 7).Value = job.NgayDang;
                jobSheet.Cell(jobRow, 8).Value = job.NgayHetHan.HasValue ? job.NgayHetHan.Value.ToShortDateString() : "N/A";
                jobRow++;
            }
            jobSheet.Row(1).Style.Font.Bold = true;
            jobSheet.Columns().AdjustToContents();

            // === Sheet 2: Người dùng ===
            var users = await _context.NguoiDungs
                .Include(u => u.ThanhPho)
                .OrderByDescending(u => u.NgayTao)
                .ToListAsync();

            var userSheet = workbook.Worksheets.Add("NguoiDung");
            var userHeaders = new string[] { "ID", "Họ Tên", "Email", "Loại Tài Khoản", "Trạng thái", "Thành phố", "Ngày tạo" };
            for(int i = 0; i < userHeaders.Length; i++)
            {
                userSheet.Cell(1, i + 1).Value = userHeaders[i];
            }

            int userRow = 2;
            foreach(var user in users)
            {
                userSheet.Cell(userRow, 1).Value = user.Id;
                userSheet.Cell(userRow, 2).Value = user.HoTen;
                userSheet.Cell(userRow, 3).Value = user.Email;
                userSheet.Cell(userRow, 4).Value = user.LoaiTk.GetDisplayName();
                userSheet.Cell(userRow, 5).Value = user.TrangThaiTk.GetDisplayName();
                userSheet.Cell(userRow, 6).Value = user.ThanhPho?.Ten ?? "N/A";
                userSheet.Cell(userRow, 7).Value = user.NgayTao;
                userRow++;
            }
            userSheet.Row(1).Style.Font.Bold = true;
            userSheet.Columns().AdjustToContents();


            // === Lưu file vào stream ===
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                string fileName = $"BaoCao_ThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }

    #endregion
}