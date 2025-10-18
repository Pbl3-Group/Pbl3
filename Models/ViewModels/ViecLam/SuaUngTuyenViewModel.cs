// File: ViewModels/ViecLam/SuaUngTuyenViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HeThongTimViec.ViewModels.ViecLam
{
    public class SuaUngTuyenViewModel
    {
        [Required]
        public int UngTuyenId { get; set; }
        public int TinTuyenDungId { get; set; } // Để redirect hoặc lấy thông tin khác

        // Thông tin hiển thị (không sửa)
        public string TieuDeCongViec { get; set; } = null!;
        public string TenNhaTuyenDung { get; set; } = null!;
        public DateTime NgayNop { get; set; }

        // Thông tin có thể sửa
        [Display(Name = "Thư giới thiệu")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Thư giới thiệu không được vượt quá 2000 ký tự.")]
        public string? ThuGioiThieu { get; set; }

        [Display(Name = "CV đã nộp hiện tại")]
        public string? UrlCvHienTai { get; set; } // Chỉ hiển thị link hoặc tên file

        [Display(Name = "Tải lên CV mới (Nếu muốn thay thế)")]
        public IFormFile? CvMoi { get; set; } // Cho phép upload file mới
    }
}