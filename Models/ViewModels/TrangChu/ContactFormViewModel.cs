using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.TrangChu // Hoặc ViewModels.TrangChu nếu bạn muốn đặt ở đó
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên của bạn.")]
        [Display(Name = "Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại (tùy chọn)")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chủ đề liên hệ.")]
        [Display(Name = "Chủ đề")]
        [StringLength(200, ErrorMessage = "Chủ đề không được vượt quá 200 ký tự.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn.")]
        [Display(Name = "Nội dung tin nhắn")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Nội dung tin nhắn không được vượt quá 2000 ký tự.")]
        public string Message { get; set; } = string.Empty;
    }
}