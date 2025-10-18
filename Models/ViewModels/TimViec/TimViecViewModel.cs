// File: ViewModels/TimViec/TimViecViewModel.cs
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongTimViec.ViewModels.TimViec
{
    public class TimViecViewModel
    {
        // --- Search/Filter Parameters ---
        public string? TuKhoa { get; set; }
        public int? ThanhPhoId { get; set; }
        public int? QuanHuyenId { get; set; }
        public List<int>? NganhNgheIds { get; set; } = new List<int>();
        public LoaiHinhCongViec? LoaiHinhCongViec { get; set; }
        public LoaiLuong? LoaiLuong { get; set; }
        public ulong? LuongMinFilter { get; set; }
        public ulong? LuongMaxFilter { get; set; }
        public bool? TinGap { get; set; } // Dùng nullable bool để phân biệt không chọn và chọn "không gấp"
        public string? SapXep { get; set; }
        public string? KinhNghiemFilter { get; set; }
        public string? HocVanFilter { get; set; }
        public List<BuoiLamViec>? CaLamViecFilter { get; set; } = new List<BuoiLamViec>(); // Lọc ca làm việc
        public bool ChiHienThiViecLamThoiVu { get; set; }
        public bool ChiHienThiViecLamPhuHopLichRanh { get; set; }
        public bool ChiHienThiViecLamDaLuu { get; set; }
        public bool ChiHienThiViecLamDaUngTuyen { get; set; }

        // --- Display Data ---
        public PaginatedList<KetQuaTimViecItemViewModel> KetQua { get; set; } = null!;

        // --- Data for Dropdowns/Checkboxes in Form ---
        public SelectList? ThanhPhoOptions { get; set; }
        public SelectList? QuanHuyenOptions { get; set; }
        public SelectList? LoaiHinhCongViecOptions { get; set; }
        public SelectList? LoaiLuongOptions { get; set; }
        public List<SelectListItem>? NganhNgheOptions { get; set; }
        public List<SelectListItem>? CaLamViecOptions { get; set; } // Dữ liệu cho checkboxes ca làm việc
    }

    // Lớp PaginatedList<T> giúp phân trang dữ liệu
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
            if (TotalPages == 0 && count > 0) TotalPages = 1; // Đảm bảo có ít nhất 1 trang nếu có item
            if (TotalPages == 0 && count == 0) PageIndex = 1; // Nếu list rỗng, trang hiện tại là 1

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Phương thức tạo đối tượng PaginatedList từ IQueryable (thường dùng với Entity Framework)
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            pageIndex = Math.Max(1, pageIndex);
            int maxPageIndex = (int)Math.Ceiling(count / (double)pageSize);
            if (maxPageIndex == 0 && count > 0) maxPageIndex = 1;
            if (maxPageIndex == 0 && count == 0) { maxPageIndex = 1; pageIndex = 1; }

            pageIndex = Math.Min(pageIndex, maxPageIndex);

            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
                public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            pageIndex = Math.Max(1, pageIndex);
            int maxPageIndex = (int)Math.Ceiling(count / (double)pageSize);
            if (maxPageIndex == 0 && count > 0) maxPageIndex = 1;
            if (maxPageIndex == 0 && count == 0) { maxPageIndex = 1; pageIndex = 1; }

            pageIndex = Math.Min(pageIndex, maxPageIndex);

            // Sử dụng các phương thức LINQ to Objects (đồng bộ)
            var items = source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

    }
}