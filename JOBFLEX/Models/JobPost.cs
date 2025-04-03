using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class JobPost
    {
        public required int JobId { get; set; }
        public required string TieuDe { get; set; }
        public required string MoTa { get; set; }
        public LoaiCvEnum LoaiCv { get; set; }
        public TrangThaiJobEnum TrangThai { get; set; }
        public LinhVucEnum LinhVuc { get; set; }
        public ThanhPhoEnum? ThanhPho { get; set; }
        public decimal Luong { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public int? UserId { get; set; }
        public int? BusinessId { get; set; }

        // Navigation properties
        public required User User { get; set; }
        public required Business Business { get; set; }
        public ICollection<JobSchedule> Schedules { get; set; } = new List<JobSchedule>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<SavedJob> SavedByUsers { get; set; } = new List<SavedJob>();
    }
}