// File: ViewModels/QuanLyBaoCao/QuanLyBaoCaoIndexViewModel.cs
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec; // For PaginatedList
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;

namespace HeThongTimViec.ViewModels.QuanLyBaoCao
{
    public class QuanLyBaoCaoIndexViewModel
    {
        public PaginatedList<BaoCaoItemViewModel> Reports { get; set; }
        public string? Keyword { get; set; }
        public LyDoBaoCao? FilterLyDo { get; set; }
        public TrangThaiXuLyBaoCao? FilterTrangThaiXuLy { get; set; }
        public string SortBy { get; set; } = "moinhat"; // Default sort

        public SelectList? LyDoOptions { get; set; }
        public SelectList? TrangThaiXuLyOptions { get; set; }
        public SelectList? SortByOptions { get; set; }

        // Counts for overview
        public int TotalReports { get; set; }
        public int NewReports { get; set; }
        public int ReviewedReports { get; set; }
        public int ProcessedReports { get; set; }
        public int IgnoredReports { get; set; }
    }

    public class BaoCaoItemViewModel
    {
        public int Id { get; set; }
        public string TieuDeTinTuyenDung { get; set; }
        public int TinTuyenDungId { get; set; }
        public string TenNguoiBaoCao { get; set; }
        public int NguoiBaoCaoId { get; set; }
        public string LyDoDisplay { get; set; }
        public LyDoBaoCao LyDo { get; set; }
        public string ChiTietCatNgan { get; set; } // Shortened detail
        public string TrangThaiXuLyDisplay { get; set; }
        public TrangThaiXuLyBaoCao TrangThaiXuLy { get; set; }
        public string TenAdminXuLy { get; set; } // Nullable
        public DateTime NgayBaoCao { get; set; }
        public DateTime? NgayXuLy { get; set; }
        public string ThoiGianBaoCaoTuongDoi { get; set; }
    }
}