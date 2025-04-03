using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public required string NoiDung { get; set; }
        public DateTime NgayGui { get; set; }

        // Navigation properties
        public required User Sender { get; set; }
        public required User Receiver { get; set; }
    }
}