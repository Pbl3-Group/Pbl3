using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JOBFLEX.Models
{
    [Table("CV")]
    public class CV
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("cv_id")]
        public int CVId { get; set; }

        [Required]
        [ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("file_path")]
        [StringLength(255)]
        public string? FilePath { get; set; } = null; // Cho phép NULL nếu chưa có CV

        [Column("ngay_tao")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Khóa ngoại liên kết với User
        public virtual User User { get; set; } = null!;
    }
}