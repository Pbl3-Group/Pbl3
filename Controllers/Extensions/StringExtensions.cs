// File: Extensions/StringExtensions.cs
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HeThongTimViec.Extensions // Đảm bảo namespace này phù hợp với dự án của bạn
{
    public static class StringExtensions
    {
        /// <summary>
        /// Cắt ngắn một chuỗi đến độ dài tối đa cho phép và thêm dấu "..." nếu nó bị cắt.
        /// </summary>
        /// <param name="value">Chuỗi cần cắt ngắn.</param>
        /// <param name="maxLength">Độ dài tối đa của chuỗi sau khi cắt (KHÔNG bao gồm độ dài của dấu "...").</param>
        /// <returns>Chuỗi đã được cắt ngắn, hoặc chuỗi rỗng nếu chuỗi đầu vào là null/rỗng.</returns>
        public static string Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty; // Trả về chuỗi rỗng cho trường hợp null hoặc rỗng
            }

            if (maxLength <= 0) // Xử lý trường hợp maxLength không hợp lệ
            {
                return "..."; // Hoặc string.Empty tùy theo logic bạn muốn
            }

            if (value.Length <= maxLength)
            {
                return value;
            }
            // Đảm bảo maxLength không âm khi thực hiện Substring
            return value.Substring(0, Math.Max(0, maxLength)) + "...";
        }

        /// <summary>
        /// Tạo một chuỗi "slug" thân thiện với URL từ một cụm từ.
        /// Slug sẽ là chữ thường, không dấu, với các từ được phân cách bằng dấu gạch ngang.
        /// </summary>
        /// <param name="phrase">Cụm từ cần chuyển đổi thành slug.</param>
        /// <returns>Chuỗi slug đã được tạo.</returns>
        public static string GenerateSlug(this string? phrase) // Cho phép phrase có thể null
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // Chuyển đổi chữ có dấu thành không dấu
            string str = phrase.RemoveDiacritics();

            // Loại bỏ các ký tự không hợp lệ, giữ lại chữ cái, số, khoảng trắng và dấu gạch ngang
            // Thêm cờ RegexOptions.Compiled để cải thiện hiệu suất nếu dùng nhiều lần
            str = Regex.Replace(str, @"[^a-zA-Z0-9\s-]", "", RegexOptions.IgnoreCase | RegexOptions.Compiled).Trim();

            // Chuyển đổi khoảng trắng thành dấu gạch ngang
            str = Regex.Replace(str, @"\s+", "-", RegexOptions.Compiled).ToLowerInvariant();

            // Loại bỏ các dấu gạch ngang liền kề (nếu có)
            str = Regex.Replace(str, @"-+", "-", RegexOptions.Compiled);

            // Giới hạn độ dài slug (tùy chọn, ví dụ 70 ký tự)
            // int maxSlugLength = 70;
            // if (str.Length > maxSlugLength)
            // {
            //    str = str.Substring(0, maxSlugLength).TrimEnd('-');
            // }

            return str;
        }

        /// <summary>
        /// Loại bỏ các dấu phụ (diacritics) khỏi một chuỗi.
        /// Ví dụ: "Nguyễn Văn A" -> "Nguyen Van A".
        /// </summary>
        /// <param name="text">Chuỗi cần loại bỏ dấu.</param>
        /// <returns>Chuỗi đã được loại bỏ dấu.</returns>
        private static string RemoveDiacritics(this string? text) // Cho phép text có thể null
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? string.Empty; // Trả về chuỗi rỗng nếu text là null hoặc toàn khoảng trắng

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}