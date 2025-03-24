using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JOBFLEX.Models
{
    [Table("work_availability")]
    public class WorkAvailability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("ngay")]
        public WorkDay Day { get; set; }  // Sử dụng Enum để tránh lỗi nhập tay

        [Required]
        [Column("thoi_gian")]
        public WorkTime Time { get; set; }  // Sử dụng Enum để kiểm soát dữ liệu

        // Khóa ngoại liên kết với User
       public virtual User User { get; set; } = null!;
    }

    // Enum các ngày trong tuần
    public enum WorkDay
    {
        Thứ_2, Thứ_3, Thứ_4, Thứ_5, Thứ_6, Thứ_7, Chủ_Nhật
    }

    // Enum các ca làm việc
    public enum WorkTime
    {
        Sáng, Chiều, Tối
    }
}