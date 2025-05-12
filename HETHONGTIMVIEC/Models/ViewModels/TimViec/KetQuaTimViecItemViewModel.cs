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
        public string DiaDiem { get; set; } = null!; // Ví dụ: "Quận 1, TP. Hồ Chí Minh"
        public string LoaiHinhCongViecDisplay { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!; // Ví dụ: "10-15 triệu", "Thỏa thuận"
        public DateTime NgayDang { get; set; }
        public bool TinGap { get; set; }
        public bool DaLuu { get; set; } // Người dùng hiện tại đã lưu tin này chưa?
        public List<string> NganhNgheNho { get; set; } = new List<string>(); // Hiển thị vài ngành nghề chính
    }
}