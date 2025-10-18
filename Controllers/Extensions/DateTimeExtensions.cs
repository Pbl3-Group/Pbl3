// Extensions/DateTimeExtensions.cs
using System;

namespace HeThongTimViec.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime dateTime, DateTime? compareTo = null)
        {
            var timeSpan = (compareTo ?? DateTime.UtcNow) - dateTime.ToUniversalTime(); // Ensure both are UTC for comparison

            if (timeSpan.TotalSeconds < 60)
                return $"{(int)timeSpan.TotalSeconds} giây trước";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
            return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
        }
    }
}