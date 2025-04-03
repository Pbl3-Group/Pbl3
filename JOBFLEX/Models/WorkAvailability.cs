using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class WorkAvailability
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public NgayEnum Ngay { get; set; }
        public ThoiGianEnum ThoiGian { get; set; }

        // Navigation properties
        public required User User { get; set; }
    }
}