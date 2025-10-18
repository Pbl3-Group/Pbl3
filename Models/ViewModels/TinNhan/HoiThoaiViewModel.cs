using System;
using HeThongTimViec.Models; // For NguoiDung

namespace HeThongTimViec.ViewModels.TinNhan
{
    public class HoiThoaiViewModel
    {
        public int NguoiLienHeId { get; set; } // ID của người mà mình đang chat cùng
        public string TenNguoiLienHe { get; set; } = string.Empty;
        public string? AvatarUrlNguoiLienHe { get; set; }
        public string TinNhanCuoiCung { get; set; } = string.Empty;
        public DateTime NgayGuiTinNhanCuoiCung { get; set; }
        public string ThoiGianGuiTinNhanCuoiCungDisplay { get; set; } = string.Empty; // "10:30 AM", "Hôm qua", "23/03/2024"
        public bool LaTinNhanCuoiCuaToi { get; set; } // Tin nhắn cuối là do tôi gửi?
        public int SoTinNhanChuaDoc { get; set; }
        public bool IsOnline { get; set; } // (Tùy chọn, cần cơ chế theo dõi)

        // Context liên quan (nếu có)
        public int? UngTuyenLienQuanId { get; set; }
        public string? TieuDeCongViecLienQuan { get; set; } // Nếu tin nhắn liên quan đến 1 ứng tuyển/tin tuyển dụng
        public int? TinTuyenDungLienQuanId { get; set; }

        public bool IsActive { get; set; } // Đánh dấu hội thoại đang được chọn


    }
    
    
}