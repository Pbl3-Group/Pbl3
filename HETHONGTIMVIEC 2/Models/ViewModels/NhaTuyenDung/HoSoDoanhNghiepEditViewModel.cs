// File: ViewModels/HoSoDoanhNghiepEditViewModel.cs
using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    public class HoSoDoanhNghiepEditViewModel
    {
        // Trường ẩn để xác định bản ghi cần sửa
        public int NguoiDungId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên công ty.")]
        [StringLength(255)]
        [Display(Name = "Tên công ty")]
        public string TenCongTy { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        [Display(Name = "Logo hiện tại")]
        public string? CurrentUrlLogo { get; set; } // Chỉ để hiển thị logo hiện tại

        [Display(Name = "Tải lên logo mới (để trống nếu không đổi)")]
        // Validate file type and size in Controller if needed
        public IFormFile? LogoFile { get; set; } // Dùng để nhận file upload

        [StringLength(255)]
        [Url(ErrorMessage = "Địa chỉ website không hợp lệ.")]
        [Display(Name = "Website")]
        public string? UrlWebsite { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả công ty")]
        public string? MoTa { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ Đăng ký Kinh doanh")]
        public string? DiaChiDangKy { get; set; }

        [StringLength(100)]
        [Display(Name = "Quy mô công ty")]
        public string? QuyMoCongTy { get; set; } // Có thể thay bằng dropdown nếu có danh sách cố định

        // --- Thông tin liên hệ từ NguoiDung ---
        [StringLength(20, ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^(\+?84|0)\d{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [Display(Name = "Số điện thoại liên hệ")]
        public string? SoDienThoaiLienHe { get; set; } // Từ NguoiDung.Sdt

        [StringLength(255)]
        [Display(Name = "Địa chỉ liên hệ chi tiết (Người đại diện)")]
        public string? DiaChiChiTietNguoiDung { get; set; } // Từ NguoiDung.DiaChiChiTiet

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public int? ThanhPhoId { get; set; } // Từ NguoiDung.ThanhPhoId

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
        [Display(Name = "Quận/Huyện")]
        public int? QuanHuyenId { get; set; } // Từ NguoiDung.QuanHuyenId
    }
}