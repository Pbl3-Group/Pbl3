// File: Utils/TimeHelper.cs
using System;
using System.Text;

namespace HeThongTimViec.Utils
{
    public static class TimeHelper
    {
        /// <summary>
        /// Chuyển đổi một giá trị DateTime thành chuỗi hiển thị "cách đây X thời gian".
        /// Ví dụ: "vài giây trước", "5 phút trước", "Hôm qua", "3 ngày trước".
        /// Nếu thời gian quá xa (mặc định là hơn 7 ngày), sẽ hiển thị ngày cụ thể.
        /// </summary>
        /// <param name="dateTime">Thời điểm cần định dạng.</param>
        /// <param name="showFullDateAfterDays">Số ngày tối đa để hiển thị "X ngày trước",
        /// sau đó sẽ hiển thị ngày đầy đủ. Mặc định là 7 ngày.</param>
        /// <returns>Chuỗi đã định dạng.</returns>
        public static string TimeAgo(DateTime dateTime, int showFullDateAfterDays = 7)
        {
            // Quan trọng: Đảm bảo dateTime và DateTime.UtcNow cùng múi giờ (thường là UTC)
            TimeSpan timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime(); // Chuyển dateTime sang UTC để so sánh

            if (timeSpan.TotalSeconds < 1)
                return "Vừa xong";

            if (timeSpan.TotalSeconds < 60)
                return $"{Math.Floor(timeSpan.TotalSeconds)} giây trước";

            if (timeSpan.TotalMinutes < 60)
                return timeSpan.Minutes == 1 ? "1 phút trước" : $"{timeSpan.Minutes} phút trước";

            if (timeSpan.TotalHours < 24)
                return timeSpan.Hours == 1 ? "1 giờ trước" : $"{timeSpan.Hours} giờ trước";

            // Ưu tiên kiểm tra "Hôm qua" dựa trên ngày lịch
            DateTime today = DateTime.UtcNow.Date;
            DateTime yesterday = today.AddDays(-1);
            if (dateTime.ToUniversalTime().Date == yesterday)
                return "Hôm qua";

            // Nếu không phải "Hôm qua" và vẫn trong khoảng showFullDateAfterDays
            // thì hiển thị số ngày.
            // Math.Floor(timeSpan.TotalDays) sẽ cho ra số ngày trọn vẹn đã trôi qua.
            int daysAgo = (int)Math.Floor(timeSpan.TotalDays);
            if (daysAgo < showFullDateAfterDays && daysAgo > 0) // daysAgo > 0 để không hiển thị "0 ngày trước"
            {
                return daysAgo == 1 ? "1 ngày trước" : $"{daysAgo} ngày trước";
            }

            // Nếu quá xa (daysAgo >= showFullDateAfterDays hoặc các điều kiện trên không thỏa mãn)
            // hoặc nếu daysAgo = 0 (nghĩa là trong cùng một ngày và đã được xử lý bởi giờ/phút/giây)
            // thì hiển thị ngày cụ thể
            return dateTime.ToString("dd/MM/yyyy"); // Bạn có thể thay đổi định dạng này
        }


        /// <summary>
        /// Định dạng một giá trị DateTime? thành chuỗi ngày giờ theo format tùy chỉnh.
        /// Trả về "N/A" nếu giá trị là null.
        /// </summary>
        /// <param name="dateTime">Thời điểm cần định dạng (có thể null).</param>
        /// <param name="format">Chuỗi định dạng mong muốn (mặc định: "dd/MM/yyyy, HH:mm").</param>
        /// <returns>Chuỗi đã định dạng hoặc "N/A".</returns>
        public static string FormatDateTime(DateTime? dateTime, string format = "dd/MM/yyyy, HH:mm")
        {
            if (!dateTime.HasValue)
                return "N/A";

            return dateTime.Value.ToString(format);
        }

        /// <summary>
        /// Định dạng một giá trị DateTime? thành chuỗi ngày theo format tùy chỉnh.
        /// Trả về "N/A" nếu giá trị là null.
        /// </summary>
        /// <param name="dateTime">Thời điểm cần định dạng (có thể null).</param>
        /// <param name="format">Chuỗi định dạng mong muốn (mặc định: "dd/MM/yyyy").</param>
        /// <returns>Chuỗi đã định dạng hoặc "N/A".</returns>
        public static string FormatDate(DateTime? dateTime, string format = "dd/MM/yyyy")
        {
            if (!dateTime.HasValue)
                return "N/A";

            return dateTime.Value.ToString(format);
        }
    }
}