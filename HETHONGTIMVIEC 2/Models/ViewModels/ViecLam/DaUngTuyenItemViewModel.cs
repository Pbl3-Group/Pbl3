// File: ViewModels/ViecLam/DaUngTuyenItemViewModel.cs
using System;
using HeThongTimViec.Models; // Cho TrangThaiUngTuyen

namespace HeThongTimViec.ViewModels.ViecLam
{
    public class DaUngTuyenItemViewModel
    {
        public int UngTuyenId { get; set; }
        public int TinTuyenDungId { get; set; }
        public string TieuDeCongViec { get; set; } = null!;
        public string TenNhaTuyenDung { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string DiaDiem { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!;
        public string LoaiHinhCongViecDisplay { get; set; } = null!;
        public DateTime NgayNop { get; set; }
        public DateTime? NgayCapNhatTrangThai { get; set; } // Ngày trạng thái được cập nhật lần cuối
        public string? ThuGioiThieuSnippet { get; set; }
        public TrangThaiUngTuyen TrangThai { get; set; }
        public string TrangThaiDisplay { get; set; } = null!;
        public string TrangThaiBadgeClass { get; set; } = null!;

        // Flags điều khiển hiển thị nút actions
        public bool CanEdit { get; set; }
        public bool CanWithdraw { get; set; } // Có thể rút đơn (chuyển trạng thái sang 'darut')
        public bool CanUndoWithdrawal { get; set; } 
        public bool CanDeletePermanently { get; set; } // Có thể xóa vĩnh viễn (sau khi đã rút hoặc bị từ chối)
        public bool CanContact { get; set; } // Có thể liên hệ NTD (khi đã được duyệt)
        // public string? UrlCvHienTai { get; set; } // Uncomment nếu bạn cần hiển thị link CV trực tiếp
    }
}