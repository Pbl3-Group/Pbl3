// File: ViewModels/TinNhan/QuanLyTinNhanViewModel.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System; // Required for DateTime

namespace HeThongTimViec.ViewModels.TinNhan
{
    public class QuanLyTinNhanViewModel
    {
        public List<HoiThoaiViewModel> DanhSachHoiThoai { get; set; } = new List<HoiThoaiViewModel>();
        public List<TinNhanItemViewModel> TinNhanTrongHoiThoaiHienTai { get; set; } = new List<TinNhanItemViewModel>();

        public int? NguoiLienHeIdHienTai { get; set; }
        public string? TenNguoiLienHeHienTai { get; set; }
        public string? AvatarUrlNguoiLienHeHienTai { get; set; }
        public string? TieuDeCongViecLienQuanHienTai { get; set; }
        public string? UrlChiTietCongViecLienQuanHienTai { get; set; }

        public string? CurrentUserDisplayName { get; set; }
        public string? CurrentUserAvatarUrl { get; set; }

        public string NoiDungTinNhanMoi { get; set; } = string.Empty;
        public int? UngTuyenIdChoTinNhanMoi { get; set; }
        public int? TinTuyenDungIdChoTinNhanMoi { get; set; }

        // --- NEW FILTER PROPERTIES ---
        public string? SearchTerm { get; set; }
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public string? FilterReadStatus { get; set; } // "all", "read", "unread"
        public List<SelectListItem> JobContextOptions { get; set; } = new List<SelectListItem>();
        public int? SelectedJobContextId { get; set; }
        
        public string? ErrorMessage { get; set; }
        // --- STATE INDICATOR ---
        public bool IsChatAreaActive { get; set; }

        public ContactDetailsPaneViewModel? DetailsPaneInfo { get; set; } // For pre-loading if needed, or set via AJAX

        public QuanLyTinNhanViewModel()
        {
            DanhSachHoiThoai = new List<HoiThoaiViewModel>();
            TinNhanTrongHoiThoaiHienTai = new List<TinNhanItemViewModel>();
            JobContextOptions = new List<SelectListItem>();
        }
    }
}