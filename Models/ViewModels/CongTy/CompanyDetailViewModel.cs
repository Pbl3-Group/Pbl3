// File: ViewModels/CongTy/CompanyDetailViewModel.cs

using HeThongTimViec.Models; // Cần để dùng enum GioiTinhNguoiDung
using HeThongTimViec.ViewModels.TimViec;
using System; // Cần cho DateTime
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Cần cho Display và DisplayFormat

namespace HeThongTimViec.ViewModels.CongTy
{
    public class CompanyDetailViewModel
    {
        // Thông tin công khai
        public required string TenCongTy { get; set; }
        public string? UrlLogo { get; set; }
        public string? UrlWebsite { get; set; }
        public string? MoTa { get; set; }
        public string? DiaChiDangKy { get; set; }
        public string? QuyMoCongTy { get; set; }
        public bool DaXacMinh { get; set; }
        public string? DiaDiemLienHe { get; set; }

        public List<KetQuaTimViecItemViewModel> ViecLamDangTuyen { get; set; } = new List<KetQuaTimViecItemViewModel>();

        // THÔNG TIN CHỈ DÀNH CHO CHỦ SỞ HỮU
        public string? MaSoThue { get; set; }
        public string? SoDienThoaiLienHe { get; set; }
        public string? EmailLienHe { get; set; }

        // =========================================================
        // === THÊM CÁC THUỘC TÍNH CÒN THIẾU VÀO ĐÂY ===
        // =========================================================
        [Display(Name = "Họ và tên người đại diện")]
        public required string HoTenNguoiDaiDien { get; set; }

        [Display(Name = "Giới tính")]
        public GioiTinhNguoiDung? GioiTinhNguoiDaiDien { get; set; }

        [Display(Name = "Ngày sinh")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", NullDisplayText = "Chưa cập nhật")]
        public DateTime? NgaySinhNguoiDaiDien { get; set; }
        // =========================================================
         // === THÊM CÁC THUỘC TÍNH PHÂN TRANG CHO VIỆC LÀM ===
        public int JobPageNumber { get; set; }
        public int JobTotalPages { get; set; }
        public bool HasPreviousJobPage => JobPageNumber > 1;
        public bool HasNextJobPage => JobPageNumber < JobTotalPages;
        public int TotalJobsCount { get; set; } // Tổng số việc làm
        public string ? Slug { get; set; } // Lưu lại slug để tạo link phân trang
    }
}