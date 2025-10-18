using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels
{
    public class CaiDatViewModel
    {
        [Required(ErrorMessage = "Tên website không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Tên Website")]
        public string TenWebsite { get; set; } = "JobFinder";

        [Required(ErrorMessage = "Email liên hệ không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        [Display(Name = "Email Liên Hệ Chính")]
        public string EmailLienHe { get; set; } = "contact@example.com";

        [Required]
        [Range(1, 365, ErrorMessage = "Số ngày phải từ 1 đến 365.")]
        [Display(Name = "Số ngày mặc định hết hạn tin đăng")]
        public int SoNgayHetHanMacDinh { get; set; } = 30;

        [Required]
        [Range(5, 50, ErrorMessage = "Số lượng phải từ 5 đến 50.")]
        [Display(Name = "Số tin hiển thị mỗi trang (Tìm kiếm)")]
        public int SoTinMoiTrang { get; set; } = 10;
        
        [Display(Name = "Bật chế độ bảo trì")]
        public bool CheDoBaoTri { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Nội dung thông báo bảo trì")]
        public string? ThongBaoBaoTri { get; set; }
    }
}