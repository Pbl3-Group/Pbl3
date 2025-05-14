// File: ViewModels/ViecLam/DaUngTuyenItemViewModel.cs
using System;
using System.Collections.Generic; // Cho List<string>
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
        public DateTime? NgayCapNhatTrangThai { get; set; }
        public string? ThuGioiThieuSnippet { get; set; }
        public TrangThaiUngTuyen TrangThai { get; set; }
        public string TrangThaiDisplay { get; set; } = null!;
        public string TrangThaiBadgeClass { get; set; } = null!;

        // Thông tin bổ sung cho giao diện mới
        public DateTime? NgayHetHan { get; set; } // <<--- THÊM MỚI
        public List<string> Tags { get; set; } = new List<string>(); // <<--- THÊM MỚI

        // Flags điều khiển hiển thị nút actions
        public bool CanEdit { get; set; }
        public bool CanWithdraw { get; set; }
        public bool CanUndoWithdrawal { get; set; }
        public bool CanDeletePermanently { get; set; }
        public bool CanContact { get; set; }
    }
}