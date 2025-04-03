using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class SavedJob
    {
        public int SavedJobId { get; set; }
        public int? UserId { get; set; }
        public int? JobId { get; set; }
        public DateTime NgayLuu { get; set; }

        // Navigation properties
        public required User User { get; set; }
        public required JobPost Job { get; set; }
    }
}