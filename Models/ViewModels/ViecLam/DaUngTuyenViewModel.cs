// File: ViewModels/ViecLam/DaUngTuyenViewModel.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec; // Cho PaginatedList

namespace HeThongTimViec.ViewModels.ViecLam
{
    public class DaUngTuyenViewModel
    {
        public string? TuKhoa { get; set; }
        public TrangThaiUngTuyen? TrangThaiFilter { get; set; }
        public string? SapXepThoiGian { get; set; } // Ví dụ: "ngaynop_moinhat", "ngaycapnhat_moinhat"

        public PaginatedList<DaUngTuyenItemViewModel> UngTuyenItems { get; set; } = null!;
        public SelectList? TrangThaiOptions { get; set; }
        public SelectList? SapXepThoiGianOptions { get; set; } // Danh sách tùy chọn sắp xếp theo thời gian

        public Dictionary<TrangThaiUngTuyen, int>? StatusCounts { get; set; }
        public int TotalCount { get; set; } // Tổng số đơn ứng tuyển (sau khi lọc trạng thái và từ khóa)
    }
}