// File: Helpers/ViewHelper.cs
using HeThongTimViec.Models; // For TrangThaiUngTuyen
using HeThongTimViec.Extensions; // For GetDisplayName

namespace HeThongTimViec.Helpers
{
    public static class ViewHelper
    {
        public static string GetUngVienTrangThaiBadgeClass(TrangThaiUngTuyen trangThai)
        {
            return trangThai switch
            {
                TrangThaiUngTuyen.danop => "badge bg-primary",
                TrangThaiUngTuyen.ntddaxem => "badge bg-info text-dark border", // Changed for better visibility
                TrangThaiUngTuyen.daduyet => "badge bg-success",
                TrangThaiUngTuyen.bituchoi => "badge bg-danger",
                TrangThaiUngTuyen.darut => "badge bg-warning text-dark",
                _ => "badge bg-secondary",
            };
        }

        // You can also move GetDisplayName extensions here if they are general enough
        // or keep them in HeThongTimViec.Extensions
    }
}