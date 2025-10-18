// File: ViewModels/Dashboard/AdminDashboardViewModel.cs
using HeThongTimViec.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        // Card Stats
        public int TotalJobPostings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalEmployers { get; set; }
        public double RecruitmentRate { get; set; } // Percentage
        public int PendingReports { get; set; }
        public string ErrorMessage { get; set; }

        // Chart Data
        public JobTrendChartData JobTrends { get; set; } = new JobTrendChartData();
        public UserGrowthChartData UserGrowth { get; set; } = new UserGrowthChartData();
        public List<PieChartItem> JobSectorDistribution { get; set; } = new List<PieChartItem>();
        public List<PieChartItem> JobPostingStatusDistribution { get; set; } = new List<PieChartItem>();

        // Lists
        public List<LatestJobViewModel> LatestJobs { get; set; } = new List<LatestJobViewModel>();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
    }

    // --- Supporting ViewModels for Charts and Lists ---

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty; // E.g., Date or Category
        public int Value { get; set; }
    }

    public class JobTrendChartData
    {
        public List<string> Labels { get; set; } = new List<string>(); // Dates for X-axis
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class UserGrowthChartData
    {
        public List<string> Labels { get; set; } = new List<string>(); // Dates for X-axis
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }
    
    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty; // E.g., "Bán thời gian", "Cá nhân"
        public List<int> Data { get; set; } = new List<int>();
        public string? BorderColor { get; set; } // For line charts
        public string? BackgroundColor { get; set; } // For line charts (fill) or pie slices
        public bool Fill { get; set; } = false;
    }

    public class PieChartItem
    {
        public string Label { get; set; } = string.Empty; // E.g., Sector Name or Status Name
        public int Value { get; set; }
        public string? BackgroundColor { get; set; } // Optional: if you want to set colors from backend
    }

    public class LatestJobViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyOrPosterName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public DateTime DatePosted { get; set; }
        public string Status { get; set; } = string.Empty; // Display name of TrangThaiTinTuyenDung
        public string StatusCssClass { get; set; } = string.Empty; // For styling the status badge
    }

    public class RecentActivityViewModel
    {
        public int? RelatedId { get; set; } // E.g., UngTuyenId or NguoiDungId
        public string ActivityType { get; set; } = string.Empty; // "Ứng tuyển mới", "Đăng ký mới"
        public string Description { get; set; } = string.Empty; // "Nguyễn Văn A đã ứng tuyển vào...", "Công ty B đã đăng ký"
        public DateTime Timestamp { get; set; }
        public string? UserAvatarUrl { get; set; }
        public string ItemUrl { get; set; } = "#"; // URL to view details of this activity
    }
}