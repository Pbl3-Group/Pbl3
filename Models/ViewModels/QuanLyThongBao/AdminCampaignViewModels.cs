using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.QuanLyThongBao
{
    // ===================================================================
    // ViewModels cho các trang của Admin
    // ===================================================================

    /// <summary>
    /// ViewModel chính cho trang Index, chứa cả danh sách và bộ lọc.
    /// </summary>
    public class CampaignManagementViewModel
    {
        public PaginatedList<CampaignIndexViewModel> Campaigns { get; set; } = null!;
        public CampaignFilterViewModel Filter { get; set; } = null!;
    }

    /// <summary>
    /// ViewModel chứa các trường để lọc trên trang Index.
    /// </summary>
    public class CampaignFilterViewModel
    {
        [Display(Name = "Từ khóa (Tiêu đề, Tên Admin)")]
        public string? Keyword { get; set; }

        [Display(Name = "Loại thông báo")]
        public string? LoaiThongBao { get; set; }

        [Display(Name = "Từ ngày gửi")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Đến ngày gửi")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Dùng để populate dropdowns cho bộ lọc
        public SelectList? LoaiThongBaoOptions { get; set; }
    }

    /// <summary>
    /// ViewModel cho mỗi hàng trong bảng danh sách (Index).
    /// </summary>
    public class CampaignIndexViewModel
    {
        public string BatchId { get; set; } = null!;
        public string TieuDe { get; set; } = null!;
        public string LoaiThongBao { get; set; } = null!;
        public string TenAdminGui { get; set; } = null!;
        public string NgayGuiDisplay { get; set; } = null!; // Đã format
        public int SoNguoiNhan { get; set; }
        public int SoNguoiDaDoc { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang tạo mới (Create).
    /// </summary>
    public class CampaignCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(255)]
        [Display(Name = "Tiêu đề Thông báo")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại thông báo.")]
        [Display(Name = "Phân loại Thông báo")]
        public string LoaiThongBao { get; set; } = "THONG_BAO_CHUNG"; // Mặc định

        [Required(ErrorMessage = "Vui lòng chọn đối tượng nhận.")]
        [Display(Name = "Gửi đến")]
        public NotificationTargetType TargetType { get; set; }

        [Display(Name = "Tìm và chọn người nhận")]
        public List<int>? SpecificUserIds { get; set; }

        public SelectList? LoaiThongBaoOptions { get; set; }
        public List<SelectListItem>? PreSelectedUsers { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang chi tiết (Details).
    /// </summary>
    public class CampaignDetailsViewModel
    {
        public string TieuDe { get; set; } = null!;
        public string NoiDung { get; set; } = null!;
        public string LoaiThongBaoDisplay { get; set; } = null!; // Tên hiển thị của loại
        public string TenAdminGui { get; set; } = null!;
        public string NgayGuiDisplay { get; set; } = null!; // Đã format
        public PaginatedList<Models.ThongBao> DanhSachNguoiNhan { get; set; } = null!;
    }

    // ===================================================================
    // Lớp tiện ích (giữ nguyên)
    // ===================================================================

    public enum NotificationTargetType
    {
        [Display(Name = "Người dùng cụ thể")]
        SpecificUser,
        [Display(Name = "Tất cả người dùng")]
        AllUsers,
        [Display(Name = "Tất cả Ứng viên")]
        AllCandidates,
        [Display(Name = "Tất cả Nhà tuyển dụng")]
        AllEmployers
    }

    public class AdminNotificationData
    {
        public string BatchId { get; set; } = null!;
        public string TieuDe { get; set; } = null!;
        public string NoiDung { get; set; } = null!;
        public int AdminGuiId { get; set; }
    }
}