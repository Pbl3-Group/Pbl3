// File: Utils/TimeHelper.cs
using System;
using System.Globalization; // Cần cho CultureInfo nếu muốn định dạng nhất quán

namespace HeThongTimViec.Utils
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone;

        static TimeHelper()
        {
            try
            {
                VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch (Exception e)
                {
                    // Ghi log lỗi ở đây nếu cần
                    Console.WriteLine($"LỖI: Không tìm thấy múi giờ Việt Nam. {e.Message}");
                    // Nếu không tìm thấy, các hàm liên quan đến múi giờ VN có thể không hoạt động đúng.
                    // Có thể throw hoặc gán một giá trị mặc định (ít khuyến khích)
                    // Ví dụ: VietnamTimeZone = TimeZoneInfo.Utc; // Hoặc một offset cố định
                    throw new InvalidOperationException("Không thể khởi tạo múi giờ Việt Nam. Hệ thống không tìm thấy 'Asia/Ho_Chi_Minh' hoặc 'SE Asia Standard Time'.", e);
                }
            }
        }

        /// <summary>
        /// Chuyển đổi DateTime từ múi giờ Việt Nam (UTC+7) sang UTC.
        /// </summary>
        public static DateTime ConvertToUtcFromVietnamTime(DateTime vietnamTime)
        {
            if (VietnamTimeZone == null)
            {
                 throw new InvalidOperationException("Múi giờ Việt Nam chưa được khởi tạo thành công.");
            }
            // Đảm bảo DateTime được coi là 'Unspecified' để ConvertTimeToUtc diễn giải nó theo VietnamTimeZone
            DateTime unspecifiedTime = DateTime.SpecifyKind(vietnamTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedTime, VietnamTimeZone);
        }

        /// <summary>
        /// Chuyển đổi DateTime từ UTC sang múi giờ Việt Nam (UTC+7).
        /// </summary>
        public static DateTime ConvertToVietnamTimeFromUtc(DateTime utcTime)
        {
            if (VietnamTimeZone == null)
            {
                 throw new InvalidOperationException("Múi giờ Việt Nam chưa được khởi tạo thành công.");
            }
            // Đảm bảo Kind là Utc trước khi chuyển đổi
            if (utcTime.Kind != DateTimeKind.Utc)
            {
                // Nếu không phải UTC, giả định là thời gian Local của server và chuyển sang UTC trước
                // Hoặc throw lỗi nếu bạn yêu cầu đầu vào phải là UTC
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc); // Hoặc utcTime.ToUniversalTime();
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
        }

        /// <summary>
        /// Hiển thị thời gian dạng "cách đây X thời gian" (ví dụ: 5 phút trước).
        /// Đầu vào 'dateTime' nên là giờ UTC.
        /// </summary>
        public static string TimeAgo(DateTime dateTimeUtc, int showFullDateAfterDays = 7)
        {
            // Đảm bảo dateTime đầu vào thực sự là UTC hoặc được chuyển sang UTC
            if (dateTimeUtc.Kind != DateTimeKind.Utc)
            {
                dateTimeUtc = dateTimeUtc.ToUniversalTime();
            }

            TimeSpan timeSpan = DateTime.UtcNow - dateTimeUtc;

            if (timeSpan.TotalSeconds < 1) return "Vừa xong";
            if (timeSpan.TotalSeconds < 60) return $"{Math.Floor(timeSpan.TotalSeconds)} giây trước";
            if (timeSpan.TotalMinutes < 60) return timeSpan.Minutes == 1 ? "1 phút trước" : $"{timeSpan.Minutes} phút trước";
            if (timeSpan.TotalHours < 24) return timeSpan.Hours == 1 ? "1 giờ trước" : $"{timeSpan.Hours} giờ trước";

            DateTime todayUtc = DateTime.UtcNow.Date;
            DateTime yesterdayUtc = todayUtc.AddDays(-1);
            if (dateTimeUtc.Date == yesterdayUtc) return "Hôm qua";

            int daysAgo = (int)Math.Floor(timeSpan.TotalDays);
            if (daysAgo > 0 && daysAgo < showFullDateAfterDays)
            {
                return daysAgo == 1 ? "1 ngày trước" : $"{daysAgo} ngày trước";
            }

            // Hiển thị ngày cụ thể. dateTimeUtc được chuyển về Local để hiển thị cho người dùng cuối
            // hoặc bạn có thể quyết định hiển thị theo giờ Việt Nam.
            // Ở đây, chúng ta hiển thị ngày tháng của thời gian gốc (đã là UTC).
            // Nếu muốn hiển thị theo giờ Việt Nam:
            // return ConvertToVietnamTimeFromUtc(dateTimeUtc).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            // Hoặc giờ local của server (ít dùng cho hiển thị user):
            // return dateTimeUtc.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return dateTimeUtc.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Định dạng DateTime? thành chuỗi. Trả về "N/A" nếu null.
        /// </summary>
        public static string FormatDateTime(DateTime? dateTime, string format = "dd/MM/yyyy, HH:mm")
        {
            return dateTime?.ToString(format, CultureInfo.InvariantCulture) ?? "N/A";
        }

        /// <summary>
        /// Định dạng DateTime? thành chuỗi ngày. Trả về "N/A" nếu null.
        /// </summary>
        public static string FormatDate(DateTime? dateTime, string format = "dd/MM/yyyy")
        {
            return dateTime?.ToString(format, CultureInfo.InvariantCulture) ?? "N/A";
        }
    }
}