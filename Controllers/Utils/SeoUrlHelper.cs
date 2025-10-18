// File: Utils/SeoUrlHelper.cs
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HeThongTimViec.Utils
{
    public static class SeoUrlHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // Loại bỏ dấu
            string str = RemoveDiacritics(phrase).ToLowerInvariant();
            // Loại bỏ các ký tự không hợp lệ (chỉ giữ lại chữ cái, số, khoảng trắng, dấu gạch ngang)
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "", RegexOptions.Compiled);
            // Chuyển nhiều khoảng trắng thành một khoảng trắng
            str = Regex.Replace(str, @"\s+", " ", RegexOptions.Compiled).Trim();
            // Giới hạn độ dài của slug (ví dụ: 70 ký tự)
            str = str.Length <= 70 ? str : str.Substring(0, 70).Trim();
            // Thay thế khoảng trắng bằng dấu gạch ngang
            str = Regex.Replace(str, @"\s", "-", RegexOptions.Compiled);
            // Đảm bảo không có nhiều hơn một dấu gạch ngang liên tiếp
            str = Regex.Replace(str, @"-+", "-", RegexOptions.Compiled);

            return str;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}