using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class BusinessMember
    {
        public int MemberId { get; set; }
        public int BusinessId { get; set; }
        public int UserId { get; set; }
        public TinhTrangMemberEnum TinhTrang { get; set; }

        // Navigation properties
        public required Business Business { get; set; }
        public required User User { get; set; }
    }
}