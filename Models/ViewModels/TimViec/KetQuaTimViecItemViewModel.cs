// File: ViewModels/TimViec/KetQuaTimViecItemViewModel.cs
using HeThongTimViec.Models;
using System;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.TimViec
{
    public class KetQuaTimViecItemViewModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = null!;
        public string TenCongTyHoacNguoiDang { get; set; } = null!;
        public string? LogoHoacAvatarUrl { get; set; }
        public LoaiTaiKhoan LoaiTaiKhoanNguoiDang { get; set; }
        public string DiaDiem { get; set; } = null!;
        public string LoaiHinhCongViecDisplay { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!;
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; } // Thuộc tính mới
        public bool TinGap { get; set; }
        public bool DaLuu { get; set; }
        public bool DaUngTuyen { get; set; }
        public int PhuHopScore { get; set; }
         public int NguoiDangId { get; set; }

        public List<string> NganhNgheNho { get; set; } = new List<string>();
        public string YeuCauKinhNghiemText { get; set; } = null!;
        public string YeuCauHocVanText { get; set; } = null!;
    }
}