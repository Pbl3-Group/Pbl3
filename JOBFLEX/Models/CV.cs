using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class CV
    {
        public int CvId { get; set; }
        public int UserId { get; set; }
        public required string FilePath { get; set; }

        // Navigation properties
        public required User User { get; set; }
    }
}