// File: ViewModels/ChangePasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    /// <summary>
    /// ViewModel sử dụng cho chức năng đổi mật khẩu của người dùng đã đăng nhập.
    /// </summary>
    public class ChangePasswordViewModel
    {
        // --- Các thuộc tính chỉ để hiển thị thông tin người dùng ---
        // Chúng sẽ được load từ controller và không cần người dùng nhập.
        
        [Display(Name = "Họ và tên")]
        public string? HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Loại tài khoản")]
        public string? LoaiTaiKhoan { get; set; }
        // --- Các thuộc tính người dùng phải nhập để đổi mật khẩu ---

        public string MatKhauCu { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(255, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        public string XacNhanMatKhauMoi { get; set; } = null!;
    }
}