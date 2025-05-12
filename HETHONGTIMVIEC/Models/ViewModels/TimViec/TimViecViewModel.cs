// File: ViewModels/TimViec/TimViecViewModel.cs
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.TimViec
{
    public class TimViecViewModel
    {
        // --- Tham số tìm kiếm / Bộ lọc ---
        public string? TuKhoa { get; set; }
        public int? ThanhPhoId { get; set; }
        public int? QuanHuyenId { get; set; }
        public List<int>? NganhNgheIds { get; set; } = new List<int>(); // Cho phép chọn nhiều
        public LoaiHinhCongViec? LoaiHinhCongViec { get; set; }
        public LoaiLuong? LoaiLuong { get; set; } // Bộ lọc loại lương
        public ulong? LuongMin { get; set; }      // Bộ lọc lương tối thiểu
        public bool? TinGap { get; set; }
        public string? SapXep { get; set; } // Ví dụ: "ngaymoi", "luongcao"

        // --- Dữ liệu hiển thị ---
        public PaginatedList<KetQuaTimViecItemViewModel> KetQua { get; set; } = null!;

        // --- Dữ liệu cho Dropdowns/Checkboxes trong Form ---
        public SelectList? ThanhPhoOptions { get; set; }
        public SelectList? QuanHuyenOptions { get; set; } // Sẽ load bằng JS
        public SelectList? LoaiHinhCongViecOptions { get; set; }
        public SelectList? LoaiLuongOptions { get; set; }
        public List<SelectListItem>? NganhNgheOptions { get; set; } // Dùng checkboxes hoặc multi-select

        // --- Thông tin phân trang (nếu không dùng thư viện) ---
        // public int PageIndex { get; set; } = 1;
        // public int TotalPages { get; set; }
        // public bool HasPreviousPage => PageIndex > 1;
        // public bool HasNextPage => PageIndex < TotalPages;
    }

    // Helper class for pagination (you might use a library like X.PagedList)
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }


        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            // Ensure pageIndex is within valid range
            pageIndex = Math.Max(1, pageIndex); // Cannot be less than 1
            int maxPageIndex = (int)Math.Ceiling(count / (double)pageSize);
            pageIndex = Math.Min(pageIndex, Math.Max(1, maxPageIndex)); // Cannot exceed max pages (handle empty source)


            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}