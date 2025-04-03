using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string HoTen { get; set; }
        public GioiTinhEnum GioiTinh { get; set; }
        public required string Email { get; set; }
        public required string FacebookLink { get; set; }
        public required string MatKhau { get; set; }
        public DateTime NgaySinh { get; set; }
        public required string SDT { get; set; }
        public ThanhPhoEnum? ThanhPho { get; set; }
        public VaiTroEnum VaiTro { get; set; } = VaiTroEnum.Ung_Vien;
        public TrangThaiEnum TrangThai { get; set; } = TrangThaiEnum.Chap_Thuan;
        public required string Avatar { get; set; }
        public required string MoTa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Business> OwnedBusinesses { get; set; } = new List<Business>();
        public ICollection<CV> CVs { get; set; } = new List<CV>();
        public ICollection<WorkAvailability> WorkAvailabilities { get; set; } = new List<WorkAvailability>();
        public ICollection<BusinessMember> BusinessMemberships { get; set; } = new List<BusinessMember>();
        public ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();
        public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Report> ReportsMade { get; set; } = new List<Report>();
        public ICollection<Report> ReportsReceived { get; set; } = new List<Report>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}