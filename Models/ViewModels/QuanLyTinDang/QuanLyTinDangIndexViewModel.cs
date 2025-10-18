using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec; // For PaginatedList
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.QuanLyTinDang
{
    public class QuanLyTinDangIndexViewModel
    {
        // Statistics
        public int TongSoTin { get; set; }
        public int SoTinDangHoatDong { get; set; }
        public int SoTinChoDuyet { get; set; }
        public int SoTinDaHetHan { get; set; }

        // Filtering and Tab
        public string? CurrentTab { get; set; } // "tatca", "choduyet", "danghoatdong", "daan", "dahethan", "phantic"
        public string? Keyword { get; set; }
        public string ViewMode { get; set; } = "list"; // "list" or "grid"
        public string SortBy { get; set; } = "moinhat"; // "moinhat", "cunhat", etc.

        public PaginatedList<QuanLyTinDangItemViewModel> JobPostings { get; set; } = null!;

        // Advanced Filter Fields
        public TrangThaiTinTuyenDung? FilterTrangThai { get; set; }
        public LoaiHinhCongViec? FilterLoaiCongViec { get; set; }
        public LoaiTaiKhoan? FilterLoaiNguoiDang { get; set; } // ca_nhan, doanh_nghiep
        public int? FilterThanhPhoId { get; set; }
        public int? FilterQuanHuyenId { get; set; }
        public List<int>? FilterNganhNgheIds { get; set; } = new List<int>();
        public ulong? FilterLuongMin { get; set; } // Assuming salary filter uses min/max
        public ulong? FilterLuongMax { get; set; }
        public bool? FilterTinGap { get; set; }

        // Options for Advanced Filter Dropdowns
        public SelectList? TrangThaiOptions { get; set; }
        public SelectList? LoaiCongViecOptions { get; set; }
        public SelectList? LoaiNguoiDangOptions { get; set; }
        public SelectList? ThanhPhoOptions { get; set; }
        public SelectList? QuanHuyenOptions { get; set; } // To be populated by AJAX or on city change
        public List<SelectListItem>? NganhNgheOptions { get; set; }
        public SelectList? SortByOptions { get; set; }

[Display(Name = "Chỉ hiển thị tin gấp")] // Để label hiển thị đúng trong modal
        public bool FilterTinGapCheckbox { get; set; } // Dùng cho binding với checkbox trên form
        // For Analytics Tab (Simplified for now, you might need a more complex model)
        public List<ChartDataPoint> TinMoiTheoThoiGian { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint>? TinTheoTrangThai { get; set; }
        public List<ChartDataPoint>? TinTheoLoaiCongViec { get; set; }
        // ... other analytics data

        public QuanLyTinDangIndexViewModel()
        {
            FilterNganhNgheIds = new List<int>();
        }
    }

    // Helper for analytics chart
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}