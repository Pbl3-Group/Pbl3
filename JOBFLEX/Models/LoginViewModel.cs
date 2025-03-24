using System.ComponentModel.DataAnnotations;

namespace JOBFLEX.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email/Facebook là bắt buộc")]
        public required string EmailOrFacebook { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public required string Password { get; set; }
    }
}