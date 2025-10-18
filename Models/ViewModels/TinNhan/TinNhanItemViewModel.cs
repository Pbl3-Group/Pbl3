using System;

namespace HeThongTimViec.ViewModels.TinNhan
{
    public class TinNhanItemViewModel
    {
        public int Id { get; set; }
        public int NguoiGuiId { get; set; }
        public string TenNguoiGui { get; set; } = string.Empty;
        public string? AvatarUrlNguoiGui { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayGui { get; set; }
        public string ThoiGianGuiDisplay { get; set; } = string.Empty;
        public bool LaCuaToi { get; set; } // Tin nhắn này có phải do người dùng hiện tại gửi không?
        public bool DaDoc { get; set; }

        // Context liên quan (nếu có, hiển thị ở đầu cuộc chat)
        public int? UngTuyenLienQuanId { get; set; }
        public int? TinLienQuanId { get; set; }
        public string? TieuDeCongViecLienQuan { get; set; }
        public string? UrlChiTietCongViecLienQuan { get; set; }
    }
}