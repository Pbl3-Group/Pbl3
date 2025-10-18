using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    #region ==== ViewModels cho trang DANH SÁCH (CaNhan) ====
    
    public class NguoiDungCaNhanIndexViewModel
    {
        public List<NguoiDungCaNhanItem> Users { get; set; } = new();
        public UserStatsViewModel Stats { get; set; } = new();
        
        [Display(Name = "Tìm kiếm")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiTaiKhoan? SearchStatus { get; set; }

        [Display(Name = "Ngày tạo từ")]
        [DataType(DataType.Date)]
        public DateTime? CreatedFrom { get; set; }

        [Display(Name = "Ngày tạo đến")]
        [DataType(DataType.Date)]
        public DateTime? CreatedTo { get; set; }

        public List<SelectListItem> TrangThaiList { get; set; } = new();
        
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public string ViewMode { get; set; } = "grid"; 
    }
    
    public class NguoiDungCaNhanItem
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Sdt { get; set; }
        public string? UrlAvatar { get; set; }
        public TrangThaiTaiKhoan TrangThaiTk { get; set; }
        public string? ThanhPho { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int DeletedUsers { get; set; }
        public int PendingUsers { get; set; }
    }

    #endregion

    #region ==== ViewModels cho trang CHI TIẾT (ChiTietCaNhan) ====

    public class NguoiDungCaNhanDetailsViewModel
    {
        public NguoiDung User { get; set; } = null!; 
        public List<ActivityLogItem> ActivityLogs { get; set; } = new List<ActivityLogItem>();
    }

    #endregion

    #region ==== ViewModels cho trang TẠO MỚI (TaoMoiCaNhan) ====

  public class NguoiDungCaNhanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(150)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [Display(Name = "Giới tính")]
        public GioiTinhNguoiDung? GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public int? ThanhPhoId { get; set; }

        [Display(Name = "Quận/Huyện")]
        public int? QuanHuyenId { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ chi tiết")]
        public string? DiaChiChiTiet { get; set; }

        // --- Properties for Dropdown Lists ---
        public List<SelectListItem> GioiTinhList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ThanhPhoList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> QuanHuyenList { get; set; } = new List<SelectListItem>();
    }
    #endregion

    #region ==== ViewModels cho trang CHỈNH SỬA (ChinhSuaCaNhan) ====
    
    public class NguoiDungCaNhanEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và tên *")]
        public string HoTen { get; set; } = null!;

        [Display(Name = "Email *")]
        public string Email { get; set; } = null!; 

        [Display(Name = "Số điện thoại")]
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

        public string? UrlAvatar { get; set; }
        
        public List<SelectListItem> ThanhPhoList { get; set; } = new();
        public List<SelectListItem> QuanHuyenList { get; set; } = new();
        public List<SelectListItem> GioiTinhList { get; set; } = new();
        
        public TrangThaiTaiKhoan TrangThaiTk { get; set; }
        public LoaiTaiKhoan LoaiTk { get; set; }
    }

    #endregion
}