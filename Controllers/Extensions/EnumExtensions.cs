// File: Extensions/EnumExtensions.cs
using HeThongTimViec.Models; // Đảm bảo namespace này đúng với nơi bạn định nghĩa các Enum
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectListItem

namespace HeThongTimViec.Extensions // Hoặc namespace bạn muốn
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Lấy tên hiển thị (Display Name) của một giá trị enum.
        /// Trả về tên thành viên nếu không có DisplayAttribute.
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            if (value == null) return string.Empty; // Thêm kiểm tra null cho an toàn

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return value.ToString(); // Nếu không tìm thấy field (hiếm khi xảy ra với enum hợp lệ)

            var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>(false);
            return displayAttribute?.GetName() ?? value.ToString();
        }

        /// <summary>
        /// Logic cốt lõi để tạo danh sách SelectListItem từ một kiểu Enum.
        /// </summary>
        private static List<SelectListItem> GenerateSelectList<TEnum>(TEnum? selectedValue, bool includeDefaultItem, string defaultItemText, string defaultItemValue, List<TEnum>? excludeValues = null)
            where TEnum : struct, Enum // Đảm bảo TEnum là kiểu enum
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("TEnum must be an Enum type.");
            }

            var items = new List<SelectListItem>();

            if (includeDefaultItem)
            {
                items.Add(new SelectListItem
                {
                    Text = defaultItemText,
                    Value = defaultItemValue,
                    Selected = !selectedValue.HasValue
                });
            }

            foreach (TEnum enumVal in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (excludeValues != null && excludeValues.Contains(enumVal))
                {
                    continue; // Bỏ qua giá trị này nếu nó nằm trong danh sách loại trừ
                }
                items.Add(new SelectListItem
                {
                    Text = ((Enum)(object)enumVal).GetDisplayName(), // Ép kiểu để gọi GetDisplayName
                    Value = enumVal.ToString(),
                    Selected = selectedValue.HasValue && enumVal.Equals(selectedValue.Value)
                });
            }
            return items;
        }

        /// <summary>
        /// Tạo danh sách SelectListItem từ một giá trị enum (non-nullable).
        /// Giá trị được truyền vào sẽ được đánh dấu là 'selected'.
        /// </summary>
        public static List<SelectListItem> ToSelectList<TEnum>(this TEnum enumValue,
                                                            bool includeDefaultItem = false,
                                                            string defaultItemText = "-- Vui lòng chọn --",
                                                            string defaultItemValue = "",
                                                            List<TEnum>? excludeValues = null)
            where TEnum : struct, Enum
        {
            return GenerateSelectList<TEnum>(enumValue, includeDefaultItem, defaultItemText, defaultItemValue, excludeValues);
        }

        /// <summary>
        /// Tạo danh sách SelectListItem từ một giá trị enum (nullable).
        /// Nếu giá trị là null, không có mục nào (ngoài mục default) được chọn.
        /// </summary>
        public static List<SelectListItem> ToSelectList<TEnum>(this TEnum? enumValue,
                                                            bool includeDefaultItem = false,
                                                            string defaultItemText = "-- Vui lòng chọn --",
                                                            string defaultItemValue = "",
                                                            List<TEnum>? excludeValues = null)
             where TEnum : struct, Enum
        {
            return GenerateSelectList<TEnum>(enumValue, includeDefaultItem, defaultItemText, defaultItemValue, excludeValues);
        }

        /// <summary>
        /// Tạo danh sách SelectListItem cho một kiểu Enum mà không cần một giá trị cụ thể ban đầu.
        /// </summary>
        public static List<SelectListItem> GetSelectList<TEnum>(bool includeDefaultItem = false,
                                                              string defaultItemText = "-- Vui lòng chọn --",
                                                              string defaultItemValue = "",
                                                              List<TEnum>? excludeValues = null)
             where TEnum : struct, Enum
        {
            return GenerateSelectList<TEnum>(null, includeDefaultItem, defaultItemText, defaultItemValue, excludeValues);
        }

        /// <summary>
        /// Lấy class CSS Bootstrap badge dựa trên trạng thái ứng tuyển.
        /// </summary>
        public static string GetBadgeClass(this TrangThaiUngTuyen trangThai)
        {
            return trangThai switch
            {
                TrangThaiUngTuyen.danop => "bg-info text-dark",
                TrangThaiUngTuyen.ntddaxem => "bg-secondary",
                TrangThaiUngTuyen.bituchoi => "bg-danger",
                TrangThaiUngTuyen.daduyet => "bg-success",
                TrangThaiUngTuyen.darut => "bg-warning text-dark",
                _ => "bg-light text-dark",
            };
        }

        /// <summary>
        /// Lấy class CSS Bootstrap badge dựa trên trạng thái tin tuyển dụng.
        /// </summary>
        public static string GetBadgeClass(this TrangThaiTinTuyenDung trangThai)
        {
            return trangThai switch
            {
                TrangThaiTinTuyenDung.choduyet => "bg-warning text-dark",
                TrangThaiTinTuyenDung.daduyet => "bg-success",
                TrangThaiTinTuyenDung.taman => "bg-secondary",
                TrangThaiTinTuyenDung.hethan => "bg-danger",
                TrangThaiTinTuyenDung.datuyen => "bg-primary",
                TrangThaiTinTuyenDung.bituchoi => "bg-dark",
                TrangThaiTinTuyenDung.daxoa => "bg-light text-dark border",
                _ => "bg-light text-dark",
            };
        }
    }

    public static class TrangThaiTaiKhoanExtensions
    {
        public class StatusInfo
        {
            public string CssClass { get; set; }
            public string IconClass { get; set; }
            public string DisplayName { get; set; }
        }

        public static StatusInfo GetStatusInfo(this TrangThaiTaiKhoan status)
        {
            switch (status)
            {
                case TrangThaiTaiKhoan.kichhoat:
                    return new StatusInfo
                    {
                        CssClass = "badge-success",
                        IconClass = "fas fa-check-circle",
                        DisplayName = "Đang hoạt động"
                    };
                case TrangThaiTaiKhoan.choxacminh:
                    return new StatusInfo
                    {
                        CssClass = "badge-warning",
                        IconClass = "fas fa-hourglass-half",
                        DisplayName = "Chờ xác minh"
                    };
                case TrangThaiTaiKhoan.tamdung:
                    return new StatusInfo
                    {
                        CssClass = "badge-danger",
                        IconClass = "fas fa-user-slash",
                        DisplayName = "Tạm dừng"
                    };
                case TrangThaiTaiKhoan.bidinhchi:
                    return new StatusInfo
                    {
                        CssClass = "badge-danger",
                        IconClass = "fas fa-ban",
                        DisplayName = "Bị đình chỉ"
                    };
                default:
                    return new StatusInfo
                    {
                        CssClass = "badge-secondary",
                        IconClass = "fas fa-question-circle",
                        DisplayName = "Không xác định"
                    };
            }
        }
    }
}