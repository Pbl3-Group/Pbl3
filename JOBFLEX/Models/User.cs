using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JOBFLEX.Models
{
    [Table("users")] // Chỉ định tên bảng
    public class User
    {
        public string? thanh_pho;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("Ho_ten")]
        [StringLength(255)]
        public required string FullName { get; set; }

        [Required]
        [Column("gioi_tinh")]
        public Gender Gender { get; set; }  // Sử dụng Enum

        [Column("email")]
        [EmailAddress]
        [StringLength(320)]
        public string? Email { get; set; }

        [Column("facebook_link")]
        [StringLength(255)]
        public string? FacebookLink { get; set; }

        [Required]
        [Column("mat_khau")]
        [StringLength(255)]
        public required string Password { get; set; }

        [Required]
        [Column("Ngay_sinh")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [Column("SDT")]
        [StringLength(10)]
        public required string PhoneNumber { get; set; }

        [Column("thanh_pho")]
        public City? City { get; set; }  // Enum Thành phố

        [Column("vai_tro")]
        public Role Role { get; set; } = Role.UngVien;  // Enum Vai trò

        [Column("trang_thai")]
        public AccountStatus Status { get; set; } = AccountStatus.Accepted;  // Enum Trạng thái

        [Column("avatar")]
        [StringLength(255)]
        public string? Avatar { get; set; }

        [Column("Mo_ta")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("Ngay_tao")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Enum Giới tính
    public enum Gender
    {
        Nam,
        Nữ
    }

    // Enum Vai trò
    public enum Role
    {
        [Display(Name = "Ứng viên")]
        UngVien,

        [Display(Name = "Nhà tuyển dụng")]
        NhaTuyenDung,

        [Display(Name = "Quản trị viên")]
        QuanTriVien
    }

    // Enum Trạng thái tài khoản
    public enum AccountStatus
    {
        [Display(Name = "Chấp thuận")]
        Accepted,

        [Display(Name = "Bị cấm")]
        Banned
    }

    // Enum Thành phố
    public enum City
    {
        An_Giang, Ba_Ria_Vung_Tau, Bac_Lieu, Bac_Giang, Bac_Kan, Bac_Ninh, Ben_Tre,
        Binh_Duong, Binh_Dinh, Binh_Phuoc, Binh_Thuan, Ca_Mau, Cao_Bang, Can_Tho,
        Da_Nang, Dak_Lak, Dak_Nong, Dien_Bien, Dong_Nai, Dong_Thap, Gia_Lai,
        Ha_Giang, Ha_Nam, Ha_Noi, Ha_Tinh, Hai_Duong, Hai_Phong, Hau_Giang,
        Hoa_Binh, Hung_Yen, Khanh_Hoa, Kien_Giang, Kon_Tum, Lai_Chau, Lam_Dong,
        Lang_Son, Lao_Cai, Long_An, Nam_Dinh, Nghe_An, Ninh_Binh, Ninh_Thuan,
        Phu_Tho, Phu_Yen, Quang_Binh, Quang_Nam, Quang_Ngai, Quang_Ninh, Quang_Tri,
        Soc_Trang, Son_La, Tay_Ninh, Thai_Binh, Thai_Nguyen, Thanh_Hoa, Thua_Thien_Hue,
        Tien_Giang, TP_Ho_Chi_Minh, Tra_Vinh, Tuyen_Quang, Vinh_Long, Vinh_Phuc, Yen_Bai
    }
}