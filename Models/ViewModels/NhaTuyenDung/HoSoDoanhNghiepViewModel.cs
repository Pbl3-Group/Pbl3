// File: ViewModels/HoSoDoanhNghiepViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using HeThongTimViec.Models;

namespace HeThongTimViec.ViewModels
{
    /// <summary>
    /// ViewModel chứa thông tin chi tiết của Hồ sơ Doanh nghiệp để hiển thị.
    /// </summary>
    public class HoSoDoanhNghiepViewModel
    {
        [Display(Name = "Tên công ty")]
        public string TenCongTy { get; set; } = string.Empty;

        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        [Display(Name = "Logo công ty")]
        public string? UrlLogo { get; set; }

        [Display(Name = "Website")]
        public string? UrlWebsite { get; set; }

        [Display(Name = "Mô tả công ty")]
        public string? MoTa { get; set; }

        [Display(Name = "Địa chỉ Đăng ký Kinh doanh")]
        public string? DiaChiDangKy { get; set; } // Từ HoSoDoanhNghiep

        [Display(Name = "Quy mô công ty")]
        public string? QuyMoCongTy { get; set; }

        [Display(Name = "Trạng thái xác minh")]
        public bool DaXacMinh { get; set; }

        [Display(Name = "Người xác minh")]
        public string? TenAdminXacMinh { get; set; } // Lấy từ HoSoDoanhNghiep.AdminXacMinh.HoTen

        [Display(Name = "Ngày xác minh")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", NullDisplayText = "Chưa xác minh")]
        public DateTime? NgayXacMinh { get; set; }
               // --- THÊM CÁC THUỘC TÍNH MỚI CHO NGƯỜI ĐẠI DIỆN ---
        [Display(Name = "Họ và tên người đại diện")]
        public required string HoTenNguoiDaiDien { get; set; } // Lấy từ NguoiDung.HoTen

        [Display(Name = "Giới tính")]
        public GioiTinhNguoiDung? GioiTinhNguoiDaiDien { get; set; } // Lấy từ NguoiDung.GioiTinh

        [Display(Name = "Ngày sinh")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", NullDisplayText = "Chưa cập nhật")]
        public DateTime? NgaySinhNguoiDaiDien { get; set; } // Lấy từ NguoiDung.NgaySinh

        // --- Thông tin liên hệ từ NguoiDung ---
        [Display(Name = "Email liên hệ")]
        public string EmailLienHe { get; set; } = string.Empty; // Từ NguoiDung.Email

        [Display(Name = "Số điện thoại liên hệ")]
        public string? SoDienThoaiLienHe { get; set; } // Từ NguoiDung.Sdt

        // --- Địa chỉ liên hệ từ NguoiDung ---
        [Display(Name = "Địa chỉ chi tiết")]
        public string? DiaChiChiTietNguoiDung { get; set; } // Từ NguoiDung.DiaChiChiTiet

        [Display(Name = "Quận/Huyện")]
        public string? TenQuanHuyen { get; set; } // Từ NguoiDung.QuanHuyen.Ten

        [Display(Name = "Tỉnh/Thành phố")]
        public string? TenThanhPho { get; set; } // Từ NguoiDung.ThanhPho.Ten

        // Có thể thêm các trường khác nếu cần
    }
}