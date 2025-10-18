namespace HeThongTimViec.ViewModels
{
    public class ActivityLogItem
    {
        public DateTime Timestamp { get; set; }
        public string ActivityType { get; set; } = string.Empty; // "Tạo tài khoản", "Ứng tuyển", "Lưu tin", "Báo cáo"...
        public string Description { get; set; } = string.Empty; // Mô tả chi tiết
        public string IconClass { get; set; } = "fas fa-info-circle"; // Icon của FontAwesome
        public string IconColorClass { get; set; } = "text-primary"; // Màu của Icon
        public string? Link { get; set; } // Liên kết (nếu có)
    }
}