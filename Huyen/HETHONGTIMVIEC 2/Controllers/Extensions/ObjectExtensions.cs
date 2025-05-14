// File: Extensions/ObjectExtensions.cs
using System.Collections.Generic;
using System.Dynamic; // Cần cho ExpandoObject
using System.Reflection;

namespace HeThongTimViec.Extensions // Đảm bảo namespace này đúng với dự án của bạn
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Chuyển đổi một anonymous object thành ExpandoObject.
        /// </summary>
        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object?>)expando; // Ép kiểu để có thể Add

            if (anonymousObject != null)
            {
                foreach (var property in anonymousObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.CanRead) // Chỉ lấy các thuộc tính có thể đọc
                    {
                        dictionary.Add(property.Name, property.GetValue(anonymousObject));
                    }
                }
            }
            return expando;
        }

        /// <summary>
        /// Thêm hoặc cập nhật các thuộc tính từ một object khác vào ExpandoObject.
        /// </summary>
        public static ExpandoObject With(this ExpandoObject expando, object additionalProperties)
        {
            var dictionary = (IDictionary<string, object?>)expando;

            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.CanRead)
                    {
                        // Nếu thuộc tính đã tồn tại trong expando, nó sẽ được cập nhật.
                        // Nếu chưa, nó sẽ được thêm mới.
                        dictionary[property.Name] = property.GetValue(additionalProperties);
                    }
                }
            }
            return expando;
        }

        /// <summary>
        /// Chuyển đổi một object (thường là anonymous object) thành Dictionary<string, object?>.
        /// Hữu ích khi làm việc với route values.
        /// </summary>
        public static IDictionary<string, object?> ToDictionary(this object source)
        {
            if (source == null) return new Dictionary<string, object?>();

            var dictionary = new Dictionary<string, object?>(System.StringComparer.OrdinalIgnoreCase); // Dùng OrdinalIgnoreCase cho tên thuộc tính
            foreach (var property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead)
                {
                    dictionary[property.Name] = property.GetValue(source);
                }
            }
            return dictionary;
        }
    }
}