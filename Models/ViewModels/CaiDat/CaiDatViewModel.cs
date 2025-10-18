using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.CaiDat
{
    /// <summary>
    /// ViewModel cho trang cài đặt hệ thống của Admin.
    /// Dữ liệu được đọc/ghi từ appsettings.json hoặc một nguồn cấu hình khác.
    /// </summary>
    public class CaiDatViewModel
    {
        #region Cài đặt chung

        [Display(Name = "Tên Website")]
        [Required(ErrorMessage = "Tên website không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên website không được vượt quá 100 ký tự.")]
        public string? SiteName { get; set; }

        [Display(Name = "Email Liên Hệ")]
        [Required(ErrorMessage = "Email liên hệ không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [Display(Name = "Số Điện Thoại Liên Hệ")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        public string? ContactPhone { get; set; }

        #endregion

        #region Cài đặt hiển thị và tin đăng

        [Display(Name = "Số mục trên mỗi trang")]
        [Required(ErrorMessage = "Vui lòng nhập số mục trên mỗi trang.")]
        [Range(5, 50, ErrorMessage = "Giá trị phải từ 5 đến 50.")]
        public int ItemsPerPage { get; set; }

        [Display(Name = "Số ngày hết hạn mặc định (tin đăng)")]
        [Required(ErrorMessage = "Vui lòng nhập số ngày hết hạn.")]
        [Range(1, 365, ErrorMessage = "Giá trị phải từ 1 đến 365.")]
        public int DefaultJobExpirationDays { get; set; }

        #endregion

        #region Mạng xã hội

        [Display(Name = "Link Facebook")]
        [Url(ErrorMessage = "URL không hợp lệ.")]
        [StringLength(255)]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Link Twitter")]
        [Url(ErrorMessage = "URL không hợp lệ.")]
        [StringLength(255)]
        public string? TwitterUrl { get; set; }

        [Display(Name = "Link LinkedIn")]
        [Url(ErrorMessage = "URL không hợp lệ.")]
        [StringLength(255)]
        public string? LinkedInUrl { get; set; }

        #endregion

        #region Cài đặt Email (SMTP) - Thường chỉ để hiển thị, không cho sửa

        [Display(Name = "Máy chủ SMTP")]
        public string? SmtpServer { get; set; }

        [Display(Name = "Cổng SMTP")]
        public int? SmtpPort { get; set; }

        [Display(Name = "Tên đăng nhập SMTP")]
        public string? SmtpUsername { get; set; }
        
        // Mặc dù không đọc từ config trong controller, việc thêm trường này
        // là hợp lý để cho phép admin kiểm tra hoặc cập nhật mật khẩu SMTP.
        [Display(Name = "Mật khẩu SMTP (Nhập để thay đổi)")]
        [DataType(DataType.Password)]
        public string? SmtpPassword { get; set; }

        #endregion

        #region Hành động hệ thống

        [Display(Name = "Xóa Cache hệ thống")]
        // Thuộc tính này không được lưu, chỉ dùng như một cờ lệnh khi POST form.
        public bool ClearCache { get; set; }

        #endregion
    }
}