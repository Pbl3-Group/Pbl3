// File: ViewModels/ThongBaoIndexViewModel.cs (Tạo thư mục ViewModels nếu chưa có)
using HeThongTimViec.Models; // Namespace chứa model ThongBao
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels
{
    public class ThongBaoIndexViewModel
    {
        public required IEnumerable<DisplayNotificationViewModel> Notifications { get; set; }
        public required PagingInfo PagingInfo { get; set; }
        
    }

    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}