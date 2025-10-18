using System;
using HeThongTimViec.Models;
using System.Collections.Generic; // Thêm using này

namespace HeThongTimViec.ViewModels.BaoCao
{
    public class BaoCaoItemViewModel
    {
        // Thông tin về Báo cáo
        public int BaoCaoId { get; set; }
        public string LyDoBaoCaoDisplay { get; set; } = string.Empty;
        public string? ChiTietBaoCao { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public TrangThaiXuLyBaoCao TrangThaiXuLy { get; set; }
        public string TrangThaiXuLyDisplay { get; set; } = string.Empty;
        public string TrangThaiXuLyBadgeClass { get; set; } = string.Empty;
        public bool CanDelete { get; set; }
        public string? GhiChuAdmin { get; set; }
        public DateTime? NgayXuLyCuaAdmin { get; set; }

        // --- BỔ SUNG CÁC TRƯỜNG CÒN THIẾU ---
        public int NguoiDangId { get; set; }
        public int NguoiBaoCaoId { get; set; }
        public string? TenNguoiDang { get; set; }
        public string? TenNguoiBaoCao { get; set; }
        public string? TenAdminXuLy { get; set; }
        public string? MoTaTinTuyenDung { get; set; }
        public string? YeuCauTinTuyenDung { get; set; }
        // ------------------------------------

        // Thông tin chi tiết về Tin Tuyển Dụng bị báo cáo
        public int TinTuyenDungId { get; set; }
        public string TieuDeTinTuyenDung { get; set; } = string.Empty;
        public string TenNhaTuyenDungHoacNguoiDang { get; set; } = string.Empty;
        public string? LogoUrlNhaTuyenDung { get; set; }
        public LoaiTaiKhoan LoaiTkNguoiDang { get; set; }
        public string DiaDiemTinTuyenDung { get; set; } = string.Empty;
        public string MucLuongDisplayTinTuyenDung { get; set; } = string.Empty;
        public string LoaiHinhDisplayTinTuyenDung { get; set; } = string.Empty;
        public DateTime? NgayHetHanTinTuyenDung { get; set; }
        public List<string> TagsTinTuyenDung { get; set; } = new List<string>();
        public bool TinGapTinTuyenDung { get; set; }
    }
}