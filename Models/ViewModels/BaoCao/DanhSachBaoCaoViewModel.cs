// File: ViewModels/BaoCao/DanhSachBaoCaoViewModel.cs
using HeThongTimViec.ViewModels.TimViec;
using System.ComponentModel.DataAnnotations;
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic; // Thêm using này

namespace HeThongTimViec.ViewModels.BaoCao
{
    public class DanhSachBaoCaoViewModel
    {
        public PaginatedList<BaoCaoItemViewModel> BaoCaos { get; set; } = new PaginatedList<BaoCaoItemViewModel>(new List<BaoCaoItemViewModel>(), 0, 1, 10);

        // --- BỘ LỌC ---
        [Display(Name = "Từ khóa tìm kiếm")]
        [StringLength(100, ErrorMessage = "Từ khóa không được vượt quá 100 ký tự.")]
        public string? tuKhoa { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiXuLyBaoCao? trangThai { get; set; }
        
        [Display(Name = "Hiển thị")]
        public int pageSize { get; set; } = 10;
        
        // --- THỐNG KÊ ---
        public int TotalReports { get; set; }
        public int NewReports { get; set; }
        public int ReviewedReports { get; set; }
        public int ProcessedReports { get; set; }
        public int IgnoredReports { get; set; }

        public SelectList? PageSizeOptions { get; set; }
    }
}