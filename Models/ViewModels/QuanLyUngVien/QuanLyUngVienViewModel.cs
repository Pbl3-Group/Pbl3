using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using HeThongTimViec.ViewModels.TimViec; // Assuming PaginatedList is here or accessible

namespace HeThongTimViec.ViewModels.QuanLyUngVien
{
    public class QuanLyUngVienViewModel
    {
        public PaginatedList<UngVienItemViewModel> UngViens { get; set; }
        public string? SearchTerm { get; set; }
        public int? SelectedTinTuyenDungId { get; set; }
        public TrangThaiUngTuyen? FilterByTrangThai { get; set; }
        public string? SortBy { get; set; } // e.g., "ngaynop_desc", "ten_asc"

        public SelectList? TinTuyenDungOptions { get; set; } // For employer's job postings
        public SelectList? TrangThaiOptions { get; set; }
        public SelectList? SortOptions { get; set; }

        // For the filter panel on the left
        public List<int>? FilterByKhuVucMongMuonIds { get; set; } // List of ThanhPho IDs
        public SelectList? KhuVucMongMuonOptions { get; set; } // Checkboxes for ThanhPho

        // public string? FilterByKinhNghiemText { get; set; } // REMOVED as per request

        public QuanLyUngVienViewModel()
        {
            UngViens = new PaginatedList<UngVienItemViewModel>(new List<UngVienItemViewModel>(), 0, 1, 10);
            FilterByKhuVucMongMuonIds = new List<int>();
        }
    }
}