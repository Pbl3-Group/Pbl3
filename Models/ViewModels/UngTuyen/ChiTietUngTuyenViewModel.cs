using HeThongTimViec.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.UngTuyen
{
    public class ChiTietUngTuyenViewModel
    {
        public int Id { get; set; } // ID của đơn ứng tuyển
        
        // Thông tin về việc làm đã ứng tuyển
        public int TinTuyenDungId { get; set; }
        public string ? TieuDeTinTuyenDung { get; set; }
        public required string TenNhaTuyenDung { get; set; }
        public string? LogoNhaTuyenDung { get; set; }

        // Thông tin về ứng viên đã nộp
        public int UngVienId { get; set; }
        public required string HoTenUngVien { get; set; }
        public string? AvatarUngVien { get; set; }
        public string ? EmailUngVien { get; set; }
        public string? SdtUngVien { get; set; }

        // Thông tin trong đơn ứng tuyển
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime NgayNop { get; set; }
        public TrangThaiUngTuyen TrangThai { get; set; }
        public string? ThuGioiThieu { get; set; }
        public string? UrlCvDaNop { get; set; }
        public string ? SlugNhaTuyenDung { get; set; }

        // Cờ để xác định quyền xem của người dùng hiện tại
        public bool CanViewAsNtd { get; set; } = false; // Người xem là nhà tuyển dụng của tin này
        public bool CanViewAsUngVien { get; set; } = false; // Người xem là ứng viên đã nộp đơn
    }
}