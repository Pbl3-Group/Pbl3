using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;

namespace HeThongTimViec.Data
{
    public class HeThongTimViecContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<WorkAvailability> WorkAvailabilities { get; set; }
        public DbSet<BusinessMember> BusinessMembers { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<JobSchedule> JobSchedules { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }

        public HeThongTimViecContext(DbContextOptions<HeThongTimViecContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Định nghĩa các Dictionary cho các enum
            var thanhPhoMap = new Dictionary<ThanhPhoEnum, string>
            {
                { ThanhPhoEnum.An_Giang, "An Giang" },
                { ThanhPhoEnum.Ba_Ria_Vung_Tau, "Bà Rịa - Vũng Tàu" },
                { ThanhPhoEnum.Bac_Lieu, "Bạc Liêu" },
                { ThanhPhoEnum.Bac_Giang, "Bắc Giang" },
                { ThanhPhoEnum.Bac_Kan, "Bắc Kạn" },
                { ThanhPhoEnum.Bac_Ninh, "Bắc Ninh" },
                { ThanhPhoEnum.Ben_Tre, "Bến Tre" },
                { ThanhPhoEnum.Binh_Duong, "Bình Dương" },
                { ThanhPhoEnum.Binh_Dinh, "Bình Định" },
                { ThanhPhoEnum.Binh_Phuoc, "Bình Phước" },
                { ThanhPhoEnum.Binh_Thuan, "Bình Thuận" },
                { ThanhPhoEnum.Ca_Mau, "Cà Mau" },
                { ThanhPhoEnum.Cao_Bang, "Cao Bằng" },
                { ThanhPhoEnum.Can_Tho, "Cần Thơ" },
                { ThanhPhoEnum.Da_Nang, "Đà Nẵng" },
                { ThanhPhoEnum.Dak_Lak, "Đắk Lắk" },
                { ThanhPhoEnum.Dak_Nong, "Đắk Nông" },
                { ThanhPhoEnum.Dien_Bien, "Điện Biên" },
                { ThanhPhoEnum.Dong_Nai, "Đồng Nai" },
                { ThanhPhoEnum.Dong_Thap, "Đồng Tháp" },
                { ThanhPhoEnum.Gia_Lai, "Gia Lai" },
                { ThanhPhoEnum.Ha_Giang, "Hà Giang" },
                { ThanhPhoEnum.Ha_Nam, "Hà Nam" },
                { ThanhPhoEnum.Ha_Noi, "Hà Nội" },
                { ThanhPhoEnum.Ha_Tinh, "Hà Tĩnh" },
                { ThanhPhoEnum.Hai_Duong, "Hải Dương" },
                { ThanhPhoEnum.Hai_Phong, "Hải Phòng" },
                { ThanhPhoEnum.Hau_Giang, "Hậu Giang" },
                { ThanhPhoEnum.Hoa_Binh, "Hòa Bình" },
                { ThanhPhoEnum.Hung_Yen, "Hưng Yên" },
                { ThanhPhoEnum.Khanh_Hoa, "Khánh Hòa" },
                { ThanhPhoEnum.Kien_Giang, "Kiên Giang" },
                { ThanhPhoEnum.Kon_Tum, "Kon Tum" },
                { ThanhPhoEnum.Lai_Chau, "Lai Châu" },
                { ThanhPhoEnum.Lam_Dong, "Lâm Đồng" },
                { ThanhPhoEnum.Lang_Son, "Lạng Sơn" },
                { ThanhPhoEnum.Lao_Cai, "Lào Cai" },
                { ThanhPhoEnum.Long_An, "Long An" },
                { ThanhPhoEnum.Nam_Dinh, "Nam Định" },
                { ThanhPhoEnum.Nghe_An, "Nghệ An" },
                { ThanhPhoEnum.Ninh_Binh, "Ninh Bình" },
                { ThanhPhoEnum.Ninh_Thuan, "Ninh Thuận" },
                { ThanhPhoEnum.Phu_Tho, "Phú Thọ" },
                { ThanhPhoEnum.Phu_Yen, "Phú Yên" },
                { ThanhPhoEnum.Quang_Binh, "Quảng Bình" },
                { ThanhPhoEnum.Quang_Nam, "Quảng Nam" },
                { ThanhPhoEnum.Quang_Ngai, "Quảng Ngãi" },
                { ThanhPhoEnum.Quang_Ninh, "Quảng Ninh" },
                { ThanhPhoEnum.Quang_Tri, "Quảng Trị" },
                { ThanhPhoEnum.Soc_Trang, "Sóc Trăng" },
                { ThanhPhoEnum.Son_La, "Sơn La" },
                { ThanhPhoEnum.Tay_Ninh, "Tây Ninh" },
                { ThanhPhoEnum.Thai_Binh, "Thái Bình" },
                { ThanhPhoEnum.Thai_Nguyen, "Thái Nguyên" },
                { ThanhPhoEnum.Thanh_Hoa, "Thanh Hóa" },
                { ThanhPhoEnum.Thua_Thien_Hue, "Thừa Thiên Huế" },
                { ThanhPhoEnum.Tien_Giang, "Tiền Giang" },
                { ThanhPhoEnum.TP_Ho_Chi_Minh, "TP Hồ Chí Minh" },
                { ThanhPhoEnum.Tra_Vinh, "Trà Vinh" },
                { ThanhPhoEnum.Tuyen_Quang, "Tuyên Quang" },
                { ThanhPhoEnum.Vinh_Long, "Vĩnh Long" },
                { ThanhPhoEnum.Vinh_Phuc, "Vĩnh Phúc" },
                { ThanhPhoEnum.Yen_Bai, "Yên Bái" }
            };

            var gioiTinhMap = new Dictionary<GioiTinhEnum, string>
            {
                { GioiTinhEnum.Nam, "Nam" },
                { GioiTinhEnum.Nu, "Nữ" }
            };

            var vaiTroMap = new Dictionary<VaiTroEnum, string>
            {
                { VaiTroEnum.Ung_Vien, "Ứng viên" },
                { VaiTroEnum.Nha_Tuyen_Dung, "Nhà tuyển dụng" },
                { VaiTroEnum.Quan_Tri_Vien, "Quản trị viên" }
            };

            var trangThaiMap = new Dictionary<TrangThaiEnum, string>
            {
                { TrangThaiEnum.Chap_Thuan, "Chấp thuận" },
                { TrangThaiEnum.Bi_Cam, "Bị cấm" }
            };

            var quyMoMap = new Dictionary<QuyMoEnum, string>
            {
                { QuyMoEnum._1_10, "1-10" },
                { QuyMoEnum._11_50, "11-50" },
                { QuyMoEnum._51_100, "51-100" },
                { QuyMoEnum._101_500, "101-500" },
                { QuyMoEnum._500_, "500+" }
            };

            var trangThaiBusinessMap = new Dictionary<TrangThaiBusinessEnum, string>
            {
                { TrangThaiBusinessEnum.Dang_Xu_Ly, "đang xử lý" },
                { TrangThaiBusinessEnum.Chap_Thuan, "chấp thuận" },
                { TrangThaiBusinessEnum.Bi_Cam, "bị cấm" }
            };

            var ngayMap = new Dictionary<NgayEnum, string>
            {
                { NgayEnum.Thu_2, "Thứ 2" },
                { NgayEnum.Thu_3, "Thứ 3" },
                { NgayEnum.Thu_4, "Thứ 4" },
                { NgayEnum.Thu_5, "Thứ 5" },
                { NgayEnum.Thu_6, "Thứ 6" },
                { NgayEnum.Thu_7, "Thứ 7" },
                { NgayEnum.Chu_Nhat, "Chủ nhật" }
            };

            var thoiGianMap = new Dictionary<ThoiGianEnum, string>
            {
                { ThoiGianEnum.Sang, "Sáng" },
                { ThoiGianEnum.Chieu, "Chiều" },
                { ThoiGianEnum.Toi, "Tối" }
            };

            var tinhTrangMemberMap = new Dictionary<TinhTrangMemberEnum, string>
            {
                { TinhTrangMemberEnum.Cho_Duyet, "Chờ duyệt" },
                { TinhTrangMemberEnum.Dang_Hoat_Dong, "Đang hoạt động" },
                { TinhTrangMemberEnum.Bi_Cam, "Bị cấm" }
            };

            var loaiCvMap = new Dictionary<LoaiCvEnum, string>
            {
                { LoaiCvEnum.Ban_Thoi_Gian, "Bán thời gian" },
                { LoaiCvEnum.Thoi_Vu, "Thời vụ" }
            };

            var trangThaiJobMap = new Dictionary<TrangThaiJobEnum, string>
            {
                { TrangThaiJobEnum.Mo, "Mở" },
                { TrangThaiJobEnum.Dong, "Đóng" }
            };

            var thuMap = new Dictionary<ThuEnum, string>
            {
                { ThuEnum.T2, "Thứ 2" },
                { ThuEnum.T3, "Thứ 3" },
                { ThuEnum.T4, "Thứ 4" },
                { ThuEnum.T5, "Thứ 5" },
                { ThuEnum.T6, "Thứ 6" },
                { ThuEnum.T7, "Thứ 7" },
                { ThuEnum.CN, "Chủ nhật" }
            };

            var linhVucMap = new Dictionary<LinhVucEnum, string>
            {
                { LinhVucEnum.Nha_Hang_Khach_San, "Nhà hàng - Khách sạn" },
                { LinhVucEnum.Ban_Le_Sieu_Thi, "Bán lẻ - Siêu thị" },
                { LinhVucEnum.Giao_Hang_Van_Chuyen, "Giao hàng - Vận chuyển" },
                { LinhVucEnum.Dich_Vu_Khach_Hang, "Dịch vụ khách hàng" },
                { LinhVucEnum.Lao_Dong_Pho_Thong, "Lao động phổ thông" },
                { LinhVucEnum.Giao_Duc_Gia_Su, "Giáo dục - Gia sư" },
                { LinhVucEnum.IT_Cong_Nghe, "IT - Công nghệ" },
                { LinhVucEnum.Marketing_Quang_Cao, "Marketing - Quảng cáo" },
                { LinhVucEnum.Nhan_Su_Hanh_Chinh, "Nhân sự - Hành chính" },
                { LinhVucEnum.Xay_Dung_Co_Khi, "Xây dựng - Cơ khí" },
                { LinhVucEnum.Suc_Khoe_Lam_Dep, "Sức khỏe - Làm đẹp" },
                { LinhVucEnum.Giai_Tri_Su_Kien, "Giải trí - Sự kiện" },
                { LinhVucEnum.Kinh_Doanh_Ban_Hang, "Kinh doanh - Bán hàng" },
                { LinhVucEnum.Thiet_Ke_Do_Hoa, "Thiết kế - Đồ họa" },
                { LinhVucEnum.Content_Viet_Lach, "Content - Viết lách" },
                { LinhVucEnum.Tai_Chinh_Ke_Toan, "Tài chính - Kế toán" },
                { LinhVucEnum.Dien_Tu_Dien_Lanh, "Điện tử - Điện lạnh" },
                { LinhVucEnum.San_Xuat_Che_Bien, "Sản xuất - Chế biến" },
                { LinhVucEnum.Thu_Cong_My_Nghe, "Thủ công - Mỹ nghệ" },
                { LinhVucEnum.Khac, "Khác" }
            };

            var trangThaiApplicationMap = new Dictionary<TrangThaiApplicationEnum, string>
            {
                { TrangThaiApplicationEnum.Dang_Cho, "Đang chờ" },
                { TrangThaiApplicationEnum.Da_Chap_Nhan, "Đã chấp nhận" },
                { TrangThaiApplicationEnum.Da_Tu_Choi, "Đã từ chối" }
            };

            var trangThaiReportMap = new Dictionary<TrangThaiReportEnum, string>
            {
                { TrangThaiReportEnum.Dang_Cho, "Đang chờ" },
                { TrangThaiReportEnum.Da_Xem_Xet, "Đã xem xét" }
            };

            var trangThaiNotificationMap = new Dictionary<TrangThaiNotificationEnum, string>
            {
                { TrangThaiNotificationEnum.Chua_Doc, "chưa đọc" },
                { TrangThaiNotificationEnum.Da_Doc, "đã đọc" }
            };

            // Định nghĩa các ValueConverter không dùng out
            var thanhPhoConverter = new ValueConverter<ThanhPhoEnum?, string>(
                v => v.HasValue && thanhPhoMap.ContainsKey(v.Value) ? thanhPhoMap[v.Value] : "",
                v => thanhPhoMap.ContainsValue(v) ? thanhPhoMap.First(x => x.Value == v).Key : (ThanhPhoEnum?)null);

            var gioiTinhConverter = new ValueConverter<GioiTinhEnum, string>(
                v => gioiTinhMap.ContainsKey(v) ? gioiTinhMap[v] : "Nam",
                v => gioiTinhMap.ContainsValue(v) ? gioiTinhMap.First(x => x.Value == v).Key : GioiTinhEnum.Nam);

            var vaiTroConverter = new ValueConverter<VaiTroEnum, string>(
                v => vaiTroMap.ContainsKey(v) ? vaiTroMap[v] : "Ứng viên",
                v => vaiTroMap.ContainsValue(v) ? vaiTroMap.First(x => x.Value == v).Key : VaiTroEnum.Ung_Vien);

            var trangThaiConverter = new ValueConverter<TrangThaiEnum, string>(
                v => trangThaiMap.ContainsKey(v) ? trangThaiMap[v] : "Chấp thuận",
                v => trangThaiMap.ContainsValue(v) ? trangThaiMap.First(x => x.Value == v).Key : TrangThaiEnum.Chap_Thuan);

            var quyMoConverter = new ValueConverter<QuyMoEnum, string>(
                v => quyMoMap.ContainsKey(v) ? quyMoMap[v] : "1-10",
                v => quyMoMap.ContainsValue(v) ? quyMoMap.First(x => x.Value == v).Key : QuyMoEnum._1_10);

            var trangThaiBusinessConverter = new ValueConverter<TrangThaiBusinessEnum, string>(
                v => trangThaiBusinessMap.ContainsKey(v) ? trangThaiBusinessMap[v] : "đang xử lý",
                v => trangThaiBusinessMap.ContainsValue(v) ? trangThaiBusinessMap.First(x => x.Value == v).Key : TrangThaiBusinessEnum.Dang_Xu_Ly);

            var ngayConverter = new ValueConverter<NgayEnum, string>(
                v => ngayMap.ContainsKey(v) ? ngayMap[v] : "Thứ 2",
                v => ngayMap.ContainsValue(v) ? ngayMap.First(x => x.Value == v).Key : NgayEnum.Thu_2);

            var thoiGianConverter = new ValueConverter<ThoiGianEnum, string>(
                v => thoiGianMap.ContainsKey(v) ? thoiGianMap[v] : "Sáng",
                v => thoiGianMap.ContainsValue(v) ? thoiGianMap.First(x => x.Value == v).Key : ThoiGianEnum.Sang);

            var tinhTrangMemberConverter = new ValueConverter<TinhTrangMemberEnum, string>(
                v => tinhTrangMemberMap.ContainsKey(v) ? tinhTrangMemberMap[v] : "Chờ duyệt",
                v => tinhTrangMemberMap.ContainsValue(v) ? tinhTrangMemberMap.First(x => x.Value == v).Key : TinhTrangMemberEnum.Cho_Duyet);

            var loaiCvConverter = new ValueConverter<LoaiCvEnum, string>(
                v => loaiCvMap.ContainsKey(v) ? loaiCvMap[v] : "Bán thời gian",
                v => loaiCvMap.ContainsValue(v) ? loaiCvMap.First(x => x.Value == v).Key : LoaiCvEnum.Ban_Thoi_Gian);

            var trangThaiJobConverter = new ValueConverter<TrangThaiJobEnum, string>(
                v => trangThaiJobMap.ContainsKey(v) ? trangThaiJobMap[v] : "Mở",
                v => trangThaiJobMap.ContainsValue(v) ? trangThaiJobMap.First(x => x.Value == v).Key : TrangThaiJobEnum.Mo);

            var thuConverter = new ValueConverter<ThuEnum?, string>(
                v => v.HasValue && thuMap.ContainsKey(v.Value) ? thuMap[v.Value] : "",
                v => thuMap.ContainsValue(v) ? thuMap.First(x => x.Value == v).Key : (ThuEnum?)null);

            var linhVucConverter = new ValueConverter<LinhVucEnum, string>(
                v => linhVucMap.ContainsKey(v) ? linhVucMap[v] : v.ToString().Replace("_", " "),
                v => linhVucMap.ContainsValue(v) ? linhVucMap.First(x => x.Value == v).Key : (LinhVucEnum)Enum.Parse(typeof(LinhVucEnum), v.Replace(" ", "_").Replace("-", "_")));

            var trangThaiApplicationConverter = new ValueConverter<TrangThaiApplicationEnum, string>(
                v => trangThaiApplicationMap.ContainsKey(v) ? trangThaiApplicationMap[v] : "Đang chờ",
                v => trangThaiApplicationMap.ContainsValue(v) ? trangThaiApplicationMap.First(x => x.Value == v).Key : TrangThaiApplicationEnum.Dang_Cho);

            var trangThaiReportConverter = new ValueConverter<TrangThaiReportEnum, string>(
                v => trangThaiReportMap.ContainsKey(v) ? trangThaiReportMap[v] : "Đang chờ",
                v => trangThaiReportMap.ContainsValue(v) ? trangThaiReportMap.First(x => x.Value == v).Key : TrangThaiReportEnum.Dang_Cho);

            var trangThaiNotificationConverter = new ValueConverter<TrangThaiNotificationEnum, string>(
                v => trangThaiNotificationMap.ContainsKey(v) ? trangThaiNotificationMap[v] : "chưa đọc",
                v => trangThaiNotificationMap.ContainsValue(v) ? trangThaiNotificationMap.First(x => x.Value == v).Key : TrangThaiNotificationEnum.Chua_Doc);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
                entity.Property(u => u.HoTen).HasColumnName("Ho_ten").IsRequired().HasMaxLength(255);
                entity.Property(u => u.GioiTinh).HasColumnName("gioi_tinh").IsRequired().HasConversion(gioiTinhConverter);
                entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired(false);
                entity.Property(u => u.FacebookLink).HasColumnName("facebook_link").HasMaxLength(255).IsRequired(false);
                entity.Property(u => u.MatKhau).HasColumnName("mat_khau").IsRequired().HasMaxLength(255);
                entity.Property(u => u.NgaySinh).HasColumnName("Ngay_sinh").HasColumnType("date").IsRequired();
                entity.Property(u => u.SDT).HasColumnName("SDT").IsRequired().HasMaxLength(10);
                entity.Property(u => u.ThanhPho).HasColumnName("thanh_pho").HasConversion(thanhPhoConverter).IsRequired(false);
                entity.Property(u => u.VaiTro).HasColumnName("vai_tro").HasConversion(vaiTroConverter).IsRequired(true);
                entity.Property(u => u.TrangThai).HasColumnName("trang_thai").HasConversion(trangThaiConverter).IsRequired(true).HasDefaultValue(TrangThaiEnum.Chap_Thuan);
                entity.Property(u => u.Avatar).HasColumnName("avatar").HasMaxLength(255).IsRequired(false);
                entity.Property(u => u.MoTa).HasColumnName("Mo_ta").HasMaxLength(500).IsRequired(false);
                entity.Property(u => u.NgayTao)
                    .HasColumnName("Ngay_tao")
                    .HasColumnType("timestamp")
                    .IsRequired(true) // NOT NULL để khớp với DateTime trong model
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Dùng giá trị mặc định từ MySQL

                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.FacebookLink).IsUnique();
                entity.HasIndex(u => u.SDT).IsUnique();
            });

            // Cấu hình bảng Businesses
            modelBuilder.Entity<Business>(entity =>
            {
                entity.ToTable("businesses");
                entity.HasKey(b => b.BusinessId);
                entity.Property(b => b.BusinessId).HasColumnName("business_id").ValueGeneratedOnAdd();
                entity.Property(b => b.TenDoanhNghiep).HasColumnName("ten_doanh_nghiep").IsRequired().HasMaxLength(255);
                entity.Property(b => b.DiaChi).HasColumnName("dia_chi").IsRequired().HasMaxLength(500);
                entity.Property(b => b.SDT).HasColumnName("SDT").IsRequired().HasMaxLength(10);
                entity.Property(b => b.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                entity.Property(b => b.GiayPhepKinhDoanh).HasColumnName("giay_phep_kinh_doanh").IsRequired().HasMaxLength(50);
                entity.Property(b => b.MaSoThue).HasColumnName("ma_so_thue").IsRequired().HasMaxLength(50);
                entity.Property(b => b.QuyMo).HasColumnName("quy_mo").IsRequired().HasConversion(quyMoConverter);
                entity.Property(b => b.TrangThai).HasColumnName("trang_thai").IsRequired().HasConversion(trangThaiBusinessConverter);
                entity.Property(b => b.OwnerId).HasColumnName("owner_id").IsRequired();

                entity.HasIndex(b => b.SDT).IsUnique();
                entity.HasIndex(b => b.Email).IsUnique();
                entity.HasIndex(b => b.GiayPhepKinhDoanh).IsUnique();
                entity.HasIndex(b => b.MaSoThue).IsUnique();

                entity.HasOne(b => b.Owner)
                      .WithMany(u => u.OwnedBusinesses)
                      .HasForeignKey(b => b.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng CVs
            modelBuilder.Entity<CV>(entity =>
            {
                entity.ToTable("cvs");
                entity.HasKey(cv => cv.CvId);
                entity.Property(cv => cv.CvId).HasColumnName("cv_id").ValueGeneratedOnAdd();
                entity.Property(cv => cv.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(cv => cv.FilePath).HasColumnName("file_path").IsRequired().HasMaxLength(255);

                entity.HasOne(cv => cv.User)
                      .WithMany(u => u.CVs)
                      .HasForeignKey(cv => cv.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng WorkAvailabilities
            modelBuilder.Entity<WorkAvailability>(entity =>
            {
                entity.ToTable("work_availabilities");
                entity.HasKey(wa => wa.Id);
                entity.Property(wa => wa.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(wa => wa.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(wa => wa.Ngay).HasColumnName("ngay").IsRequired().HasConversion(ngayConverter);
                entity.Property(wa => wa.ThoiGian).HasColumnName("thoi_gian").IsRequired().HasConversion(thoiGianConverter);

                entity.HasOne(wa => wa.User)
                      .WithMany(u => u.WorkAvailabilities)
                      .HasForeignKey(wa => wa.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng BusinessMembers
            modelBuilder.Entity<BusinessMember>(entity =>
            {
                entity.ToTable("business_members");
                entity.HasKey(bm => bm.MemberId);
                entity.Property(bm => bm.MemberId).HasColumnName("member_id").ValueGeneratedOnAdd();
                entity.Property(bm => bm.BusinessId).HasColumnName("business_id").IsRequired();
                entity.Property(bm => bm.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(bm => bm.TinhTrang).HasColumnName("tinh_trang").IsRequired().HasConversion(tinhTrangMemberConverter);

                entity.HasIndex(bm => new { bm.BusinessId, bm.UserId }).IsUnique();

                entity.HasOne(bm => bm.Business)
                      .WithMany(b => b.Members)
                      .HasForeignKey(bm => bm.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bm => bm.User)
                      .WithMany(u => u.BusinessMemberships)
                      .HasForeignKey(bm => bm.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng PasswordResets
            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.ToTable("password_resets");
                entity.HasKey(pr => pr.PasswordResetId);
                entity.Property(pr => pr.PasswordResetId).HasColumnName("password_reset_id").ValueGeneratedOnAdd();
                entity.Property(pr => pr.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(pr => pr.Token).HasColumnName("token").IsRequired().HasMaxLength(255);
                entity.Property(pr => pr.ExpiryDate).HasColumnName("expiry_date").IsRequired();

                entity.HasOne(pr => pr.User)
                      .WithMany(u => u.PasswordResets)
                      .HasForeignKey(pr => pr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng JobPosts
            modelBuilder.Entity<JobPost>(entity =>
            {
                entity.ToTable("job_posts");
                entity.HasKey(jp => jp.JobId);
                entity.Property(jp => jp.JobId).HasColumnName("job_id").ValueGeneratedOnAdd();
                entity.Property(jp => jp.TieuDe).HasColumnName("tieu_de").IsRequired().HasMaxLength(255);
                entity.Property(jp => jp.MoTa).HasColumnName("mo_ta").IsRequired().HasMaxLength(1000);
                entity.Property(jp => jp.LoaiCv).HasColumnName("loai_cv").IsRequired().HasConversion(loaiCvConverter);
                entity.Property(jp => jp.TrangThai).HasColumnName("trang_thai").IsRequired().HasConversion(trangThaiJobConverter);
                entity.Property(jp => jp.LinhVuc).HasColumnName("linh_vuc").IsRequired().HasConversion(linhVucConverter);
                entity.Property(jp => jp.ThanhPho).HasColumnName("thanh_pho").HasConversion(thanhPhoConverter);
                entity.Property(jp => jp.Luong).HasColumnName("luong").IsRequired();
                entity.Property(jp => jp.NgayDang).HasColumnName("ngay_dang").IsRequired();
                entity.Property(jp => jp.NgayHetHan).HasColumnName("ngay_het_han");
                entity.Property(jp => jp.UserId).HasColumnName("user_id");
                entity.Property(jp => jp.BusinessId).HasColumnName("business_id");

                entity.HasOne(jp => jp.User)
                      .WithMany(u => u.JobPosts)
                      .HasForeignKey(jp => jp.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(jp => jp.Business)
                      .WithMany(b => b.JobPosts)
                      .HasForeignKey(jp => jp.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng JobSchedules
            modelBuilder.Entity<JobSchedule>(entity =>
            {
                entity.ToTable("job_schedules");
                entity.HasKey(js => js.ScheduleId);
                entity.Property(js => js.ScheduleId).HasColumnName("schedule_id").ValueGeneratedOnAdd();
                entity.Property(js => js.JobId).HasColumnName("job_id").IsRequired();
                entity.Property(js => js.Thu).HasColumnName("thu").HasConversion(thuConverter);
                entity.Property(js => js.GioBatDau).HasColumnName("gio_bat_dau").IsRequired().HasMaxLength(5);
                entity.Property(js => js.GioKetThuc).HasColumnName("gio_ket_thuc").IsRequired().HasMaxLength(5);

                entity.HasOne(js => js.Job)
                      .WithMany(jp => jp.Schedules)
                      .HasForeignKey(js => js.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng Applications
            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("applications");
                entity.HasKey(a => a.ApplicationId);
                entity.Property(a => a.ApplicationId).HasColumnName("application_id").ValueGeneratedOnAdd();
                entity.Property(a => a.JobId).HasColumnName("job_id");
                entity.Property(a => a.UserId).HasColumnName("user_id");
                entity.Property(a => a.NgayUngTuyen).HasColumnName("ngay_ung_tuyen").IsRequired();
                entity.Property(a => a.TrangThai).HasColumnName("trang_thai").IsRequired().HasConversion(trangThaiApplicationConverter);

                entity.HasOne(a => a.Job)
                      .WithMany(jp => jp.Applications)
                      .HasForeignKey(a => a.JobId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.User)
                      .WithMany(u => u.Applications)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng Messages
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(m => m.MessageId);
                entity.Property(m => m.MessageId).HasColumnName("message_id").ValueGeneratedOnAdd();
                entity.Property(m => m.SenderId).HasColumnName("sender_id");
                entity.Property(m => m.ReceiverId).HasColumnName("receiver_id");
                entity.Property(m => m.NoiDung).HasColumnName("noi_dung").IsRequired().HasMaxLength(1000);
                entity.Property(m => m.NgayGui).HasColumnName("ngay_gui").IsRequired();

                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.Receiver)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.ReceiverId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng Reports
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("reports");
                entity.HasKey(r => r.ReportId);
                entity.Property(r => r.ReportId).HasColumnName("report_id").ValueGeneratedOnAdd();
                entity.Property(r => r.ReporterId).HasColumnName("reporter_id");
                entity.Property(r => r.ReportedId).HasColumnName("reported_id");
                entity.Property(r => r.JobId).HasColumnName("job_id");
                entity.Property(r => r.LyDo).HasColumnName("ly_do").IsRequired().HasMaxLength(500);
                entity.Property(r => r.NgayBaoCao).HasColumnName("ngay_bao_cao").IsRequired();
                entity.Property(r => r.TrangThai).HasColumnName("trang_thai").IsRequired().HasConversion(trangThaiReportConverter);

                entity.HasOne(r => r.Reporter)
                      .WithMany(u => u.ReportsMade)
                      .HasForeignKey(r => r.ReporterId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Reported)
                      .WithMany(u => u.ReportsReceived)
                      .HasForeignKey(r => r.ReportedId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Job)
                      .WithMany(jp => jp.Reports)
                      .HasForeignKey(r => r.JobId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng Notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.NotificationId).HasColumnName("notification_id").ValueGeneratedOnAdd();
                entity.Property(n => n.UserId).HasColumnName("user_id");
                entity.Property(n => n.BusinessId).HasColumnName("business_id");
                entity.Property(n => n.NoiDung).HasColumnName("noi_dung").IsRequired().HasMaxLength(500);
                entity.Property(n => n.NgayTao).HasColumnName("ngay_tao").IsRequired();
                entity.Property(n => n.TrangThai).HasColumnName("trang_thai").IsRequired().HasConversion(trangThaiNotificationConverter);

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.Business)
                      .WithMany(b => b.Notifications)
                      .HasForeignKey(n => n.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng SavedJobs
            modelBuilder.Entity<SavedJob>(entity =>
            {
                entity.ToTable("saved_jobs");
                entity.HasKey(sj => sj.SavedJobId);
                entity.Property(sj => sj.SavedJobId).HasColumnName("saved_job_id").ValueGeneratedOnAdd();
                entity.Property(sj => sj.UserId).HasColumnName("user_id");
                entity.Property(sj => sj.JobId).HasColumnName("job_id");
                entity.Property(sj => sj.NgayLuu).HasColumnName("ngay_luu").IsRequired();

                entity.HasOne(sj => sj.User)
                      .WithMany(u => u.SavedJobs)
                      .HasForeignKey(sj => sj.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(sj => sj.Job)
                      .WithMany(jp => jp.SavedByUsers)
                      .HasForeignKey(sj => sj.JobId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}