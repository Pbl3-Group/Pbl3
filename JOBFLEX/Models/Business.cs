using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class Business
    {
        public int BusinessId { get; set; }
        public required string TenDoanhNghiep { get; set; }
        public required string DiaChi { get; set; }
        public required string SDT { get; set; }
        public required string Email { get; set; }
        public required string GiayPhepKinhDoanh { get; set; }
        public required string MaSoThue { get; set; }
        public QuyMoEnum QuyMo { get; set; }
        public TrangThaiBusinessEnum TrangThai { get; set; }
        public int OwnerId { get; set; }

        // Navigation properties
        public required User Owner { get; set; }
        public ICollection<BusinessMember> Members { get; set; } = new List<BusinessMember>();
        public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}