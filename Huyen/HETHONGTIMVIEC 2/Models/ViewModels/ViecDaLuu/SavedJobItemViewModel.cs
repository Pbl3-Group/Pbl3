// File: ViewModels/ViecDaLuu/SavedJobItemViewModel.cs
using HeThongTimViec.Models;
using System;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.ViecDaLuu // Đổi namespace
{
    // ViewModel cho mỗi mục công việc đã lưu trên trang DaLuu
    public class SavedJobItemViewModel
    {
        public int TinTuyenDungId { get; set; } // ID của TinTuyenDung gốc
        public int TinDaLuuId { get; set; } // ID của bản ghi TinDaLuu để xóa
        public string TieuDe { get; set; } = null!;
        public string TenCongTyHoacNguoiDang { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public LoaiTaiKhoan LoaiTkNguoiDang { get; set; }
        public string DiaDiem { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!;
        public string LoaiHinhDisplay { get; set; } = null!;
        public DateTime? NgayHetHan { get; set; }
        public List<string> Tags { get; set; } = new List<string>(); // VD: Ngành nghề chính
        public DateTime NgayLuu { get; set; }
        public bool DaUngTuyen { get; set; }
    }
}