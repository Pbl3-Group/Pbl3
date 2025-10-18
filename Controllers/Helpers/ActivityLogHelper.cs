using HeThongTimViec.Models;
using HeThongTimViec.ViewModels; // <-- Đảm bảo using này trỏ đến nơi DUY NHẤT chứa ActivityLogItem
using System.Collections.Generic;
using System.Linq;
using HeThongTimViec.Extensions; // Cần để dùng GetDisplayName()

namespace HeThongTimViec.Helpers
{
    public static class ActivityLogHelper
    {
        /// <summary>
        /// Tổng hợp nhật ký hoạt động cho một người dùng CÁ NHÂN.
        /// Sử dụng đúng các thuộc tính của ViewModel ActivityLogItem.
        /// </summary>
        public static List<ActivityLogItem> GetUserActivityLog(NguoiDung user)
        {
            var activityLogs = new List<ActivityLogItem>();

            // Chỉ xử lý người dùng cá nhân hợp lệ
            if (user == null || user.LoaiTk != LoaiTaiKhoan.canhan) return activityLogs;

            // 1. Hoạt động tạo tài khoản
            activityLogs.Add(new ActivityLogItem
            {
                Timestamp = user.NgayTao,
                ActivityType = "Tạo tài khoản",
                Description = "Người dùng đã tạo tài khoản thành công.",
                IconClass = "fas fa-user-plus",
                IconColorClass = "text-success"
            });

            // 2. Lần đăng nhập cuối
            if (user.LanDangNhapCuoi.HasValue)
            {
                activityLogs.Add(new ActivityLogItem
                {
                    Timestamp = user.LanDangNhapCuoi.Value,
                    ActivityType = "Đăng nhập lần cuối",
                    Description = "Ghi nhận lần đăng nhập cuối cùng của người dùng.",
                    IconClass = "fas fa-sign-in-alt",
                    IconColorClass = "text-info"
                });
            }

            // 3. Hoạt động từ bảng ThongBao (hành động của admin)
            if (user.ThongBaos != null)
            {
                foreach (var thongBao in user.ThongBaos.Where(t => t.LoaiThongBao == "AdminAction"))
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = thongBao.NgayTao,
                        ActivityType = "Hành động từ Admin",
                        Description = thongBao.DuLieu,
                        IconClass = "fas fa-user-shield",
                        IconColorClass = "text-primary"
                    });
                }
            }

            // 4. Hoạt động ứng tuyển
            if (user.UngTuyens != null)
            {
                foreach (var ungTuyen in user.UngTuyens)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = ungTuyen.NgayNop,
                        ActivityType = "Ứng tuyển vào",
                        Description = $"'{ungTuyen.TinTuyenDung?.TieuDe ?? "Tin đã bị xóa"}'",
                        IconClass = "fas fa-file-signature",
                        IconColorClass = "text-primary",
                    });
                }
            }

            // 5. Hoạt động lưu tin
            if (user.TinDaLuus != null)
            {
                foreach (var tinDaLuu in user.TinDaLuus)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = tinDaLuu.NgayLuu,
                        ActivityType = "Lưu tin",
                        Description = $"'{tinDaLuu.TinTuyenDung?.TieuDe ?? "Tin đã bị xóa"}'",
                        IconClass = "fas fa-bookmark",
                        IconColorClass = "text-info",
                    });
                }
            }

            // 6. Hoạt động báo cáo
            if (user.BaoCaoViPhamsDaGui != null)
            {
                foreach (var baoCao in user.BaoCaoViPhamsDaGui)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = baoCao.NgayBaoCao,
                        ActivityType = "Gửi báo cáo cho tin",
                        Description = $"'{baoCao.TinTuyenDung?.TieuDe ?? "Tin đã bị xóa"}' (Lý do: {baoCao.LyDo.GetDisplayName()})",
                        IconClass = "fas fa-flag",
                        IconColorClass = "text-warning",
                    });
                }
            }

            // Sắp xếp và trả về
            return activityLogs.OrderByDescending(log => log.Timestamp).ToList();
        }

        /// <summary>
        /// Tổng hợp nhật ký hoạt động cho một người dùng DOANH NGHIỆP.
        /// Sử dụng đúng các thuộc tính của ViewModel ActivityLogItem.
        /// </summary>
        public static List<ActivityLogItem> GetCompanyActivityLog(NguoiDung user)
        {
            var activityLogs = new List<ActivityLogItem>();

            // Chỉ xử lý người dùng doanh nghiệp hợp lệ
            if (user == null || user.LoaiTk != LoaiTaiKhoan.doanhnghiep) return activityLogs;

            // 1. Hoạt động tạo tài khoản
            activityLogs.Add(new ActivityLogItem
            {
                Timestamp = user.NgayTao,
                ActivityType = "Tạo tài khoản",
                Description = "Tài khoản doanh nghiệp đã được tạo.",
                IconClass = "fas fa-building",
                IconColorClass = "text-success"
            });

            // 2. Lần đăng nhập cuối
            if (user.LanDangNhapCuoi.HasValue)
            {
                activityLogs.Add(new ActivityLogItem
                {
                    Timestamp = user.LanDangNhapCuoi.Value,
                    ActivityType = "Đăng nhập lần cuối",
                    Description = "Ghi nhận lần đăng nhập cuối cùng.",
                    IconClass = "fas fa-sign-in-alt",
                    IconColorClass = "text-info"
                });
            }

            // 3. Hoạt động từ bảng ThongBao (hành động của admin)
            if (user.ThongBaos != null)
            {
                foreach (var thongBao in user.ThongBaos.Where(t => t.LoaiThongBao == "AdminAction"))
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = thongBao.NgayTao,
                        ActivityType = "Hành động từ Admin",
                        Description = thongBao.DuLieu,
                        IconClass = "fas fa-user-shield",
                        IconColorClass = "text-primary"
                    });
                }
            }
            
            if (user.TinTuyenDungsDaDang != null)
            {
                // 4. Đăng tin tuyển dụng
                foreach (var tinDang in user.TinTuyenDungsDaDang)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = tinDang.NgayDang,
                        ActivityType = "Đăng tin",
                        Description = $"Đăng tin tuyển dụng mới: '{tinDang.TieuDe}'",
                        IconClass = "fas fa-bullhorn",
                        IconColorClass = "text-success",
                    });
                }

                // 5. Nhận hồ sơ ứng tuyển
                var applications = user.TinTuyenDungsDaDang.SelectMany(ttd => ttd.UngTuyens ?? new List<UngTuyen>());
                foreach (var ungTuyen in applications)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = ungTuyen.NgayNop,
                        ActivityType = "Nhận hồ sơ",
                        Description = $"Nhận được hồ sơ ứng tuyển cho tin '{ungTuyen.TinTuyenDung?.TieuDe ?? "Tin đã bị xóa"}'",
                        IconClass = "fas fa-file-import",
                        IconColorClass = "text-primary"
                    });
                }

                // 6. Tin tuyển dụng bị báo cáo vi phạm
                var reports = user.TinTuyenDungsDaDang.SelectMany(ttd => ttd.BaoCaoViPhams ?? new List<BaoCaoViPham>());
                foreach (var baoCao in reports)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        Timestamp = baoCao.NgayBaoCao,
                        ActivityType = "Bị báo cáo",
                        Description = $"Tin '{baoCao.TinTuyenDung?.TieuDe ?? "Tin đã bị xóa"}' bị báo cáo. (Lý do: {baoCao.LyDo.GetDisplayName()})",
                        IconClass = "fas fa-flag",
                        IconColorClass = "text-danger" // Màu đỏ để cảnh báo
                    });
                }
            }

            // Sắp xếp và trả về
            return activityLogs.OrderByDescending(log => log.Timestamp).ToList();
        }
    }
}