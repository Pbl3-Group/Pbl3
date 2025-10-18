// File: ViewModels/QuanLyBaoCao/QuanLyBaoCaoDetailViewModel.cs
using HeThongTimViec.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.QuanLyBaoCao
{
    public class QuanLyBaoCaoDetailViewModel
    {
        public int ReportId { get; set; }
        public LyDoBaoCao LyDo { get; set; }
        public string LyDoDisplay { get; set; }
        public string? ChiTietBaoCao { get; set; }
        public DateTime NgayBaoCao { get; set; }

        public int? AdminXuLyId { get; set; }
        public string TenAdminXuLy { get; set; }
public DateTime? NgayXuLy { get; set; }

        // Job Posting Info
        public int TinTuyenDungId { get; set; }
        public string TieuDeTinTuyenDung { get; set; }
        public string NoiDungTinTuyenDungTomTat { get; set; }
        public string LinkChiTietTinTuyenDung { get; set; } // Link for admin to view job post details
        public TrangThaiTinTuyenDung TrangThaiTinTuyenDungHienTai { get; set; }

        // Reporter Info
        public int NguoiBaoCaoId { get; set; }
        public string TenNguoiBaoCao { get; set; }
        public string EmailNguoiBaoCao { get; set; }
        public string? SdtNguoiBaoCao { get; set; }
        public LoaiTaiKhoan LoaiTkNguoiBaoCao { get; set; }

        // Processing Info
        public TrangThaiXuLyBaoCao TrangThaiXuLy { get; set; }
        public string TrangThaiXuLyDisplay { get; set; }


        [Display(Name = "Ghi chú của Admin")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        [DataType(DataType.MultilineText)]
        public string? GhiChuAdmin { get; set; }

        [Display(Name = "Hành động với tin tuyển dụng")]
        public AdminActionForJobPost ActionForJobPost { get; set; }

        public SelectList? TrangThaiXuLyOptions { get; set; }
        public SelectList? ActionForJobPostOptions { get; set; }
    }

    public enum AdminActionForJobPost
    {
        [Display(Name = "Không hành động")]
        None,
        [Display(Name = "Tạm xoá tin")]
        HidePost,
        [Display(Name = "Xóa tin (Đánh dấu đã xóa)")]
        DeletePost,
        [Display(Name = "Gỡ tin (Vi phạm nghiêm trọng - Xóa vĩnh viễn)")]
        HardDeletePost, // Consider implications - usually soft delete is preferred
        [Display(Name = "Cảnh cáo người đăng")]
        WarnPoster,
        [Display(Name = "Khóa tài khoản người đăng")]
        BanPoster
    }
}