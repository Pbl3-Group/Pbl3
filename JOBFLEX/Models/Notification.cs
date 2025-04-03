using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; }
        public int? BusinessId { get; set; }
        public required string NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public TrangThaiNotificationEnum TrangThai { get; set; }

        // Navigation properties
        public required User User { get; set; }
        public required Business Business { get; set; }
    }
}