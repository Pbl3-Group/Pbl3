// File: ViewModels/UngTuyen/NopHoSoViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Required for IFormFile
using HeThongTimViec.Models;     // Required for LoaiTaiKhoan enum

namespace HeThongTimViec.ViewModels.UngTuyen
{
    public class NopHoSoViewModel
    {
        [Required]
        public int TinTuyenDungId { get; set; }

        public string? TieuDeTinTuyenDung { get; set; }
        public string? TenNhaTuyenDungHoacCaNhan { get; set; }
        public LoaiTaiKhoan? LoaiTaiKhoanNguoiDang { get; set; }


        [Display(Name = "Thư giới thiệu (tùy chọn)")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Thư giới thiệu không được vượt quá 2000 ký tự.")]
        public string? ThuGioiThieu { get; set; }

        [Display(Name = "Tải lên CV của bạn")]
        // Không [Required] vì có thể người dùng muốn ứng tuyển bằng hồ sơ trực tuyến (chưa triển khai ở đây)
        // hoặc CV đã có sẵn trong hệ thống. Trong ví dụ này, chúng ta tập trung vào việc tải CV mới.
        public IFormFile? CvFile { get; set; }

        // Không hiển thị cho người dùng, chỉ dùng để lưu đường dẫn sau khi upload
        public string? UrlCvDaNop { get; set; }

        // Thông tin ứng viên (lấy từ người dùng đăng nhập)
        public string? HoTenUngVien { get; set; }
        public string? EmailUngVien { get; set; }
        public string? SdtUngVien { get; set; }
    }
}