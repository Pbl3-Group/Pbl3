using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.Dashboard; // For DashboardJobPostingItemViewModel & DashboardApplicationItemViewModel
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    public class EmployerCompanyDashboardViewModel
    {
        // Company Information (from HoSoDoanhNghiep)
        [Display(Name = "Tên công ty")]
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyWebsite { get; set; }
        [Display(Name = "Mô tả công ty")]
        public string? CompanyDescription { get; set; } // Could be a snippet
        [Display(Name = "Địa chỉ đăng ký")]
        public string? CompanyRegisteredAddress { get; set; }
        [Display(Name = "Quy mô")]
        public string? CompanySize { get; set; }
        [Display(Name = "Trạng thái xác minh")]
        public bool IsCompanyVerified { get; set; }

        // Representative User Information (from NguoiDung)
        [Display(Name = "Người đại diện")]
        public string RepresentativeUserName { get; set; } = string.Empty;
        public string? RepresentativeUserAvatarUrl { get; set; }
        public string RepresentativeUserEmail { get; set; } = string.Empty; // Added for display
        public string? RepresentativeUserPhone { get; set; } // Added for display

        // Job Posting Statistics
        public int TotalJobsPosted { get; set; }
        public int ActiveJobsCount { get; set; }
        public int PendingJobsCount { get; set; }
        public int ExpiredOrFilledJobsCount { get; set; }

        // Applicant Statistics
        public int TotalApplicationsReceived { get; set; }
        public int NewApplicationsCount { get; set; }

        // Recent Activity Lists (using the two ViewModels you mentioned)
        public List<DashboardJobPostingItemViewModel> RecentlyPostedJobs { get; set; } = new List<DashboardJobPostingItemViewModel>();
        public List<DashboardApplicationItemViewModel> RecentApplications { get; set; } = new List<DashboardApplicationItemViewModel>();

        public EmployerCompanyDashboardViewModel() { }

        // Constructor to map from HoSoDoanhNghiep and NguoiDung
        public EmployerCompanyDashboardViewModel(HoSoDoanhNghiep hoSo, NguoiDung nguoiDaiDien)
        {
            // Map Company Info
            CompanyName = hoSo.TenCongTy;
            CompanyLogoUrl = hoSo.UrlLogo;
            CompanyWebsite = hoSo.UrlWebsite;
            CompanyDescription = hoSo.MoTa; // Consider truncating if too long for dashboard
            CompanyRegisteredAddress = hoSo.DiaChiDangKy;
            CompanySize = hoSo.QuyMoCongTy;
            IsCompanyVerified = hoSo.DaXacMinh;

            // Map Representative User Info
            RepresentativeUserName = nguoiDaiDien.HoTen;
            RepresentativeUserAvatarUrl = nguoiDaiDien.UrlAvatar;
            RepresentativeUserEmail = nguoiDaiDien.Email;
            RepresentativeUserPhone = nguoiDaiDien.Sdt;
        }
    }
}