// File: ViewModels/Dashboard/CandidateDashboardViewModel.cs
using System.Collections.Generic;
using HeThongTimViec.ViewModels.BaoCao;
using HeThongTimViec.ViewModels.ViecDaLuu;
using HeThongTimViec.ViewModels.ViecLam;
using HeThongTimViec.Models;

namespace HeThongTimViec.ViewModels.Dashboard
{
    // ViewModel nhỏ cho lịch rảnh (giữ nguyên)
    public class LichRanhDisplayViewModel
    {
        public string NgayTrongTuanDisplay { get; set; } = string.Empty;
        public string BuoiLamViecDisplay { get; set; } = string.Empty;
        public BuoiLamViec BuoiEnumValue { get; set; }
        public NgayTrongTuan NgayEnumValue { get; set; }

        public string FullDisplay => $"{BuoiLamViecDisplay} {NgayTrongTuanDisplay}";
        public string BuoiBadgeClass
        {
            get
            {
                return BuoiEnumValue switch
                {
                    BuoiLamViec.sang => "buoi-sang",
                    BuoiLamViec.chieu => "buoi-chieu",
                    BuoiLamViec.toi => "buoi-toi",
                    BuoiLamViec.cangay => "buoi-cangay",
                    _ => "buoi-linhhoat",
                };
            }
        }
        public string BuoiIconClass
        {
            get
            {
                return BuoiEnumValue switch
                {
                    BuoiLamViec.sang => "fa-sun",
                    BuoiLamViec.chieu => "fa-cloud-sun",
                    BuoiLamViec.toi => "fa-moon",
                    BuoiLamViec.cangay => "fa-calendar-day",
                    _ => "fa-clock",
                };
            }
        }
    }

    // *** ViewModel mới cho Địa điểm mong muốn ***
    public class DiaDiemMongMuonDisplayViewModel
    {
        public int Id { get; set; } // Id của DiaDiemMongMuon, có thể dùng để xóa
        public string TenQuanHuyen { get; set; } = string.Empty;
        public string TenThanhPho { get; set; } = string.Empty;
        public string FullDiaDiem => $"{TenQuanHuyen}, {TenThanhPho}";
    }


    public class CandidateDashboardViewModel
    {
        public string? CandidateName { get; set; }
        public string? UserAvatarUrl { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        public bool HasProfile { get; set; }

        public string? ProfileTitle { get; set; }
        public string? DesiredPosition { get; set; }
        public TrangThaiTimViec ProfileJobSearchStatus { get; set; }
        public string ProfileJobSearchStatusDisplay { get; set; } = string.Empty;
        public bool ProfileAllowsSearch { get; set; }

        public int FreeSlotsCount { get; set; }
        public List<LichRanhDisplayViewModel> DetailedFreeSlots { get; set; } = new List<LichRanhDisplayViewModel>();

        // *** Thuộc tính mới cho Địa điểm mong muốn ***
        public int DesiredLocationsCount { get; set; }
        public List<DiaDiemMongMuonDisplayViewModel> DesiredLocations { get; set; } = new List<DiaDiemMongMuonDisplayViewModel>();


        public int ApplicationsCount { get; set; }
        public int SavedJobsCount { get; set; }
        public int ReportsCount { get; set; }

        public List<DaUngTuyenItemViewModel> RecentApplications { get; set; } = new List<DaUngTuyenItemViewModel>();
        public List<SavedJobItemViewModel> RecentSavedJobs { get; set; } = new List<SavedJobItemViewModel>();
        public List<BaoCaoItemViewModel> RecentReports { get; set; } = new List<BaoCaoItemViewModel>();
    }
}