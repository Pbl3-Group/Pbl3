using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public int? ReporterId { get; set; }
        public int? ReportedId { get; set; }
        public int? JobId { get; set; }
        public required string LyDo { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public TrangThaiReportEnum TrangThai { get; set; }

        // Navigation properties
        public required User Reporter { get; set; }
        public required User Reported { get; set; }
        public required JobPost Job { get; set; }
    }
}