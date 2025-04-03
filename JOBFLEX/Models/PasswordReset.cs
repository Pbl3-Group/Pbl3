using System;
using System.Collections.Generic;

namespace HeThongTimViec.Models
{
    public class PasswordReset
    {
        public int PasswordResetId { get; set; }
        public int UserId { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiryDate { get; set; }

        // Navigation properties
        public required User User { get; set; }
    }
}