using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public required string HoTen { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public GioiTinhEnum GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 chữ số")]
        public required string SDT { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime NgaySinh { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Url(ErrorMessage = "Liên kết Facebook không hợp lệ")]
        public string? FacebookLink { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public required string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không khớp")]
        public required string NhapLaiMatKhau { get; set; }

        public string? MoTa { get; set; }

        public IFormFile? CVFile { get; set; }

        public List<WorkAvailabilityInput>? WorkAvailabilities { get; set; }

        public string? ThanhPho { get; set; }

        // Bổ sung validate tùy chỉnh
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(FacebookLink))
            {
                yield return new ValidationResult("Phải nhập Email hoặc liên kết Facebook.",
                    new[] { nameof(Email), nameof(FacebookLink) });
            }
        }
    }

    public class WorkAvailabilityInput
    {
        public string ? Ngay { get; set; }
        public string ? ThoiGian { get; set; }
    }
}
