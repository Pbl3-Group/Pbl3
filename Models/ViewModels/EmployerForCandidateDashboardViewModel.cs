using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.Dashboard; 
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels
{
    public class EmployerForCandidateDashboardViewModel
    {
        public string EmployerName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }

        // Job Posting Statistics
        public int TotalJobsPosted { get; set; }
        public int ActiveJobsCount { get; set; }
        public int PendingJobsCount { get; set; }
        public int ExpiredOrFilledJobsCount { get; set; }

        // Applicant Statistics
        public int TotalApplicationsReceived { get; set; }
        public int NewApplicationsCount { get; set; }

        // Recent Activity Lists
        public List<DashboardJobPostingItemViewModel> RecentlyPostedJobs { get; set; } = new List<DashboardJobPostingItemViewModel>();
        public List<DashboardApplicationItemViewModel> RecentApplications { get; set; } = new List<DashboardApplicationItemViewModel>();
    }
}