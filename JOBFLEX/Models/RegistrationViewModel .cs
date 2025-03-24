using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JOBFLEX.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public required string Gender { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "SĐT phải từ 10-11 số")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email hoặc Facebook là bắt buộc")]
        public required string EmailOrFacebook { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản")]
        public bool TermsAgreed { get; set; }

        public string? Description { get; set; }

        public required IFormFile? CV { get; set; }

        public required string[]? Schedule { get; set; }

        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        public required string City { get; set; }
    }
}