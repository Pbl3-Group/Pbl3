// File: Models/PasswordResetToken.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HeThongTimViec.Models;

[Table("PasswordResetTokens")]
public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int NguoiDungId { get; set; }

    [Required]
    [StringLength(128)]
    public string TokenHash { get; set; } // Sẽ lưu hash của token, không lưu token gốc

    [Required]
    public DateTime ExpiryDate { get; set; } // Thời gian hết hạn

    public bool IsUsed { get; set; } = false; // Đã được sử dụng chưa?

    [ForeignKey("NguoiDungId")]
    public virtual NguoiDung NguoiDung { get; set; } = null!;
}