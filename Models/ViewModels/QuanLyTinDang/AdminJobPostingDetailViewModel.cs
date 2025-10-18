using HeThongTimViec.Models; // For Enums
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For GetDisplayName if you use an extension method for it

namespace HeThongTimViec.ViewModels.QuanLyTinDang
{
    public class AdminJobPostingDetailViewModel
    {
        public JobPostingInfoSection JobInfo { get; set; }
        public RecruiterInfoSection RecruiterInfo { get; set; }
        public List<ActivityLogItemViewModel> ActivityLog { get; set; }
        public PostingStatusSection PostingStatusSidebar { get; set; }

        public string CurrentTab { get; set; } // To manage active tab: "chitiet", "nguoidang", "lichsu"
        public int JobPostingId { get; set; }
        public string JobPostingTitle { get; set; }

        public AdminJobPostingDetailViewModel()
        {
            JobInfo = new JobPostingInfoSection();
            RecruiterInfo = new RecruiterInfoSection();
            ActivityLog = new List<ActivityLogItemViewModel>();
            PostingStatusSidebar = new PostingStatusSection();
            CurrentTab = "chitiet"; // Default tab
        }
    }

    public class JobPostingInfoSection
    {
        public string TieuDe { get; set; }
        public string LoaiHinhCongViecDisplay { get; set; }
        public string DanhMucNganhNghe { get; set; }
        public string MucLuongDisplay { get; set; }
        public string ThoiGianLamViec_Formatted { get; set; } // Posting validity or Job Contract Period
        public string DiaDiemLamViec { get; set; }
        public string LamViecTuXaDisplay { get; set; } // Placeholder

        public string MoTaCongViec { get; set; }
        public string YeuCauCongViec { get; set; }
        public string QuyenLoi { get; set; }
        public List<string> KyNangTags { get; set; } // Placeholder
        public string PhuCapKhacHtml { get; set; } // Placeholder
        public List<AdminLichLamViecViewModel> LichLamViecs { get; set; }

        public JobPostingInfoSection()
        {
            KyNangTags = new List<string>();
            LichLamViecs = new List<AdminLichLamViecViewModel>();
        }
    }

    public class AdminLichLamViecViewModel
    {
        public string? NgayTrongTuanDisplay { get; set; }
        public string? ThoiGianDisplay { get; set; }
    }

    public class RecruiterInfoSection
    {
        public string? TenNguoiHoacCongTy { get; set; }
        public string? LoaiHinhTaiKhoanDisplay { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChiLienHeFull { get; set; }
        public string? WebsiteUrl { get; set; }

        // Company specific
        public string? MoTaCongTyHtml { get; set; }
        public string? DiaChiDangKyKinhDoanh { get; set; }
        public string? MaSoThue { get; set; }
        public string? QuyMoCongTy { get; set; }
        public bool? CongTyDaXacMinh { get; set; }
    }

    public class ActivityLogItemViewModel
    {
        public string? NguoiThucHien { get; set; }
        public string? AvatarUrl { get; set; }
        public string? HanhDongChinh { get; set; }
        public string? MoTaChiTietHtml { get; set; }
        public DateTime ThoiGianDateTime { get; set; }
        public string ThoiGianDisplay => ThoiGianDateTime.ToString("HH:mm dd/MM/yyyy");
    }

    public class PostingStatusSection // Right sidebar
    {
        public int JobPostingId { get; set; }
        public string? JobPostingTitle { get; set; } // For breadcrumb and modal title
        public string? TrangThaiTagOverall { get; set; } // e.g. "Tuyển gấp", "Chờ duyệt" for top area
        public string? TrangThaiTagOverallCssClass { get; set; }

        public string? ThoiGianDangDisplay { get; set; }
        public string? NguoiDangDisplay { get; set; }
        public string? TrangThaiTinChiTiet { get; set; }
        public string? TrangThaiTinChiTietCssClass { get; set; }
        public int LuotXem { get; set; } // Placeholder
        public int LuotUngTuyen { get; set; }
        public string? NgayTaoDisplay { get; set; }
        public string? CapNhatLanCuoiDisplay { get; set; }

        public bool CanDuyet { get; set; }
        public bool CanTuChoi { get; set; }
        public bool CanChinhSua { get; set; } // Placeholder for edit button
    }
}