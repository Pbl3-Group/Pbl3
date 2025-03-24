using Microsoft.EntityFrameworkCore;
using JOBFLEX.Models;

namespace JOBFLEX.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo DbSet cho các bảng
        public DbSet<User> Users { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<WorkAvailability> WorkAvailabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chuyển đổi Enum thành chuỗi khi lưu vào MySQL
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.City)
                .HasConversion<string>();

            modelBuilder.Entity<WorkAvailability>()
                .Property(w => w.Day)
                .HasConversion<string>();

            modelBuilder.Entity<WorkAvailability>()
                .Property(w => w.Time)
                .HasConversion<string>();

            // Cấu hình quan hệ bảng CV (1-1 với User)
            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.User)
                .WithOne()
                .HasForeignKey<CV>(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ bảng WorkAvailability (1-N với User)
            modelBuilder.Entity<WorkAvailability>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}