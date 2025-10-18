// Create a new folder: ViewModels/QuanLyTinDang
// File: ViewModels/QuanLyTinDang/QuanLyTinDangItemViewModel.cs
using HeThongTimViec.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.QuanLyTinDang
{
    public class QuanLyTinDangItemViewModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string TenNguoiDang { get; set; } = string.Empty; // Company name or Individual name
        public string? LogoNguoiDangUrl { get; set; }
        public LoaiTaiKhoan LoaiTaiKhoanNguoiDang { get; set; }
        public string DiaDiemLamViec { get; set; } = string.Empty; // e.g., "Quận 1, TP.HCM"
        public string LoaiHinhCongViecDisplay { get; set; } = string.Empty;
        public string ThoiGianDangTuongDoi { get; set; } = string.Empty; // e.g., "2 giờ trước"
        public TrangThaiTinTuyenDung TrangThai { get; set; }
        public string TrangThaiDisplay { get; set; } = string.Empty;
        public string TrangThaiCssClass { get; set; } = string.Empty; // For styling the badge
        public bool TinGap { get; set; }
        public string MucLuongDisplay { get; set; } = string.Empty;
        public List<string> DanhMucNganhNghe { get; set; } = new List<string>();
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; }
    }
}