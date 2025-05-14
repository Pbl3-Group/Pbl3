// File: ViewModels/ViecDaLuu/DaLuuViewModel.cs
using System.Collections.Generic;
using HeThongTimViec.ViewModels.TimViec; // Dùng lại PaginatedList từ TimViec
using Microsoft.AspNetCore.Mvc.Rendering;   // Cho SelectList

namespace HeThongTimViec.ViewModels.ViecDaLuu
{
    public class DaLuuViewModel
    {
        public string? TuKhoa { get; set; }
        public string? SapXepThoiGian { get; set; } // "moinhat" (default), "cunhat"
        public PaginatedList<SavedJobItemViewModel> SavedJobs { get; set; } = null!;
        public SelectList? SapXepThoiGianOptions { get; set; }
    }
}