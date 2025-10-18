namespace HeThongTimViec.ViewModels
{
    public class DisplayNotificationViewModel
    {
        public int Id { get; set; }
        public string IconClass { get; set; } = "fas fa-info-circle text-secondary";
        public string Title { get; set; } = "Thông báo hệ thống";
        public string Subtitle { get; set; } = "";
        public string TypeDisplayName { get; set; } = "";
        public string RelatedEntityDisplayName { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public bool IsUnread { get; set; } = false;
        public bool IsAdminNotification { get; set; } = false; // Cờ để biết khi nào hiển thị nút "Chi tiết"
         public string? TargetUrl { get; set; } 
    }
}