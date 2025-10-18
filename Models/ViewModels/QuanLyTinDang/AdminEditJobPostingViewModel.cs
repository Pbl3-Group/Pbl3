// File: ViewModels/QuanLyTinDang/AdminEditJobPostingViewModel.cs
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.JobPosting; // For LichLamViecViewModel
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.QuanLyTinDang
{
    public class AdminEditJobPostingViewModel
    {
        public int Id { get; set; }

        // Tab: Thông tin cơ bản
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin.")]
        [StringLength(255)]
        [Display(Name = "Tiêu đề tin đăng")]
        public required string TieuDe { get; set; }

        [StringLength(255)]
        [Display(Name = "Tên công ty")]
        // This field is populated from NguoiDang.HoSoDoanhNghiep.TenCongTy or NguoiDang.HoTen.
        // If admin edits this, it implies overriding display name for this post or editing employer's profile.
        // For this implementation, it's primarily for display; saving changes to this specific field
        // would require a clear policy (e.g., update a TinTuyenDung.TenCongTyOverride field).
        public string? TenCongTy { get; set; }

        [Display(Name = "Địa chỉ làm việc chi tiết")]
        [StringLength(255)]
        public string? DiaChiLamViec { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [Display(Name = "Tỉnh / Thành phố")]
        public int ThanhPhoId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
        [Display(Name = "Quận / Huyện")]
        public int QuanHuyenId { get; set; }

        [Display(Name = "Lương tối thiểu")]
        [Range(0, ulong.MaxValue, ErrorMessage = "Mức lương không hợp lệ.")]
        public ulong? LuongToiThieu { get; set; }

        [Display(Name = "Lương tối đa")]
        [Range(0, ulong.MaxValue, ErrorMessage = "Mức lương không hợp lệ.")]
        public ulong? LuongToiDa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại lương.")]
        [Display(Name = "Hình thức trả lương")]
        public LoaiLuong LoaiLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hình công việc.")]
        [Display(Name = "Loại hình công việc")]
        public LoaiHinhCongViec LoaiHinhCongViec { get; set; }

        [Display(Name = "Lịch làm việc (mô tả)")]
        [StringLength(200)]
        public string? LichLamViecMoTa { get; set; } // Simplified text field from UI
        
        // For structured editing of schedule (if form supports it)
        public List<LichLamViecViewModel> LichLamViecItems { get; set; } = new List<LichLamViecViewModel>();

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 1.")]
        [Display(Name = "Số lượng cần tuyển")]
        public int SoLuongTuyen { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hạn nộp hồ sơ")]
        public DateTime? NgayHetHan { get; set; }

        // Tab: Chi tiết công việc
        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc.")]
        [Display(Name = "Mô tả công việc")]
        [DataType(DataType.MultilineText)]
        public required string MoTa { get; set; }

        [Display(Name = "Yêu cầu ứng viên")]
        [DataType(DataType.MultilineText)]
        public string? YeuCau { get; set; }

        [Display(Name = "Quyền lợi")]
        [DataType(DataType.MultilineText)]
        public string? QuyenLoi { get; set; }

        [Display(Name = "Ngành nghề")]
        public List<int> SelectedNganhNgheIds { get; set; } = new List<int>();

        // Tab: Cài đặt tin đăng
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        [Display(Name = "Trạng thái tin đăng")]
        public TrangThaiTinTuyenDung TrangThai { get; set; }

        [Display(Name = "Tin đăng nổi bật")]
        public bool IsFeatured { get; set; } // Assumes TinTuyenDung might have this field or it's a placeholder

        [Display(Name = "Tin tuyển gấp")]
        public bool TinGap { get; set; }

        // Display only fields
        public string? NgayTaoDisplay { get; set; }
        public string? CapNhatCuoiDisplay { get; set; }
        public string? NguoiDangDisplay { get; set; }
        public string? IdTinDangDisplay { get; set; }


        // SelectLists for dropdowns
        public SelectList? ThanhPhoOptions { get; set; }
        public SelectList? QuanHuyenOptions { get; set; }
        public SelectList? LoaiHinhCongViecOptions { get; set; }
        public SelectList? LoaiLuongOptions { get; set; }
        public MultiSelectList? NganhNgheOptions { get; set; }
        public SelectList? TrangThaiTinDangOptions { get; set; }
    }
}