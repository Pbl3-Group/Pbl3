using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace HeThongTimViec.ViewModels
{
    // Lớp chứa thông tin tóm tắt của một doanh nghiệp để hiển thị trong danh sách
    public class NguoiDungDoanhNghiepItem
    {
        public int Id { get; set; }
        public string? TenCongTy { get; set; }
        public string Email { get; set; } = null!;
        public string? Sdt { get; set; }
        public string? UrlLogo { get; set; }
        public TrangThaiTaiKhoan TrangThaiTk { get; set; }
        public bool DaXacMinh { get; set; }
        public DateTime NgayTao { get; set; }
    }

    // ViewModel cho trang danh sách doanh nghiệp (Index)
    public class NguoiDungDoanhNghiepIndexViewModel
    {
        public List<NguoiDungDoanhNghiepItem> Users { get; set; } = new List<NguoiDungDoanhNghiepItem>();
        public UserStatsViewModel Stats { get; set; } = new UserStatsViewModel();

        // Bộ lọc
        public string? SearchTerm { get; set; }
        public TrangThaiTaiKhoan? SearchStatus { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public List<SelectListItem> TrangThaiList { get; set; } = new List<SelectListItem>();

        // Phân trang
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1; // Added
        public bool HasNextPage => PageNumber < TotalPages; // Added

        // Chế độ xem
        public string ViewMode { get; set; } = "table"; // Mặc định là table
    }

    // Other view models remain unchanged
    public class NguoiDungDoanhNghiepDetailsViewModel
    {
        public NguoiDung User { get; set; } = null!;
        // Danh sách các báo cáo vi phạm mà doanh nghiệp này nhận được
        public List<BaoCaoViPham> ReceivedReports { get; set; } = new List<BaoCaoViPham>();
        // Nhật ký hoạt động tổng hợp cho doanh nghiệp
        public List<ActivityLogItem> ActivityLogs { get; set; } = new List<ActivityLogItem>();
        // Add other properties as needed for the view
    }

    public class NguoiDungDoanhNghiepCreateViewModel
    {
        [Required(ErrorMessage = "Tên người đại diện không được để trống")]
        [StringLength(150)]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [StringLength(20)]
        public string? Sdt { get; set; }

        [Required(ErrorMessage = "Tên công ty không được để trống")]
        [StringLength(255)]
        public string TenCongTy { get; set; } = null!;

        [StringLength(20)]
        public string? MaSoThue { get; set; }

        [Url(ErrorMessage = "URL Website không hợp lệ")]
        public string? UrlWebsite { get; set; }

        [StringLength(100)]
        public string? QuyMoCongTy { get; set; }

        public string? MoTa { get; set; }
    }



    public class NguoiDungDoanhNghiepEditViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên người đại diện.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = null!;
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Sdt { get; set; }
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }
        [Display(Name = "Giới tính")]
        public GioiTinhNguoiDung? GioiTinh { get; set; }
        [Display(Name = "Tỉnh/Thành phố")]
        public int? ThanhPhoId { get; set; }
        [Display(Name = "Quận/Huyện")]
        public int? QuanHuyenId { get; set; }
        [Display(Name = "Địa chỉ chi tiết (Số nhà, đường)")]
        public string? DiaChiChiTiet { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên công ty.")]
        [Display(Name = "Tên công ty")]
        public string TenCongTy { get; set; } = null!;
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }
        [Display(Name = "Website")]
        [Url(ErrorMessage = "URL Website không hợp lệ.")]
        public string? UrlWebsite { get; set; }
        [Display(Name = "Địa chỉ Đăng ký Kinh doanh")]
        public string? DiaChiDangKy { get; set; }
        [Display(Name = "Quy mô công ty")]
        public string? QuyMoCongTy { get; set; }
        [Display(Name = "Mô tả công ty")]
        public string? MoTa { get; set; }
        public LoaiTaiKhoan LoaiTk { get; set; }
        public TrangThaiTaiKhoan TrangThaiTk { get; set; }
        public bool DaXacMinh { get; set; }
        public List<SelectListItem> ThanhPhoList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> QuanHuyenList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> GioiTinhList { get; set; } = new List<SelectListItem>();
    }
    
    //  public class ActivityLogItem
    // {
    //     public DateTime Timestamp { get; set; }
    //     public string ActivityType { get; set; } // Ví dụ: "Tin tuyển dụng", "Báo cáo", "Tài khoản"
    //     public string Description { get; set; }
    //     public string? IconCssClass { get; set; } // Ví dụ: "fas fa-file-alt" để hiển thị icon
    //     public string? Url { get; set; } // Link tùy chọn để điều hướng
    // }
}