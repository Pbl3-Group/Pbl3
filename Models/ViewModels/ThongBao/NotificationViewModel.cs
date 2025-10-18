using System;

namespace HeThongTimViec.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string LoaiThongBao { get; set; } = null!;
        public string NoiDungHienThi { get; set; } = null!; // Nội dung đã được parse và định dạng
        public string? UrlLienQuan { get; set; } // Link để redirect khi click
        public bool DaDoc { get; set; }
        public DateTime NgayTao { get; set; }
        public string ThoiGianTruoc { get; set; } = null!; // Ví dụ: "5 phút trước", "Hôm qua"
    }

    // Dùng để deserialize `DuLieu` từ ThongBao
    public class NotificationData
    {
        public string? TieuDe { get; set; } // Tiêu đề của tin, tên ứng viên, etc.
        public string? MoTaNgan { get; set; } // Mô tả ngắn gọn của thông báo
        public string? TrangThai { get; set; } // Trạng thái mới (nếu có)
        public string? LyDo { get; set; } // Lý do (nếu có, ví dụ từ chối)
        public string? TenNguoiGui { get; set; } // Tên người gửi (ví dụ NTD gửi tin nhắn)
    }
}