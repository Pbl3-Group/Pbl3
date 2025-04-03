using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class JobSchedule
    {
        public int ScheduleId { get; set; }
        public int JobId { get; set; }
        public ThuEnum? Thu { get; set; }
        public required string GioBatDau { get; set; }
        public required string GioKetThuc { get; set; }

        // Navigation properties
        public required JobPost Job { get; set; }
    }
}