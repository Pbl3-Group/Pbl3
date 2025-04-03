using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public int? JobId { get; set; }
        public int? UserId { get; set; }
        public DateTime NgayUngTuyen { get; set; }
        public TrangThaiApplicationEnum TrangThai { get; set; }

        // Navigation properties
        public required JobPost Job { get; set; }
        public required User User { get; set; }
    }
}