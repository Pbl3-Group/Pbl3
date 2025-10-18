using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress]
        [Display(Name = "Email đã đăng ký")]
        public string Email { get; set; } = null!;

        // --- THÊM TRƯỜNG MỚI ---
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone]
        [Display(Name = "Số điện thoại đã đăng ký")]
        public string Sdt { get; set; } = null!;
        // --- KẾT THÚC THÊM MỚI ---
    }
}