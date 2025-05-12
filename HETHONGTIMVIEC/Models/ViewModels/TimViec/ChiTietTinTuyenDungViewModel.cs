// File: ViewModels/TimViec/ChiTietTinTuyenDungViewModel.cs
using HeThongTimViec.Models;
using System;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.TimViec
{
    public class ChiTietTinTuyenDungViewModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = null!;
        public string MoTa { get; set; } = null!;
        public string? YeuCau { get; set; }
        public string? QuyenLoi { get; set; }
        public string LoaiHinhCongViecDisplay { get; set; } = null!;
        public string LoaiLuongDisplay { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!;
        public string DiaChiLamViecDayDu { get; set; } = null!; // Bao gồm cả Quận/Huyện, Thành Phố
        public string YeuCauKinhNghiemText { get; set; } = null!;
        public string YeuCauHocVanText { get; set; } = null!;
        public int SoLuongTuyen { get; set; }
        public bool TinGap { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public bool DaLuu { get; set; } // Người dùng hiện tại đã lưu chưa?

        // Thông tin Nhà tuyển dụng/Người đăng
        public int NguoiDangId { get; set; }
        public string TenNguoiDangHoacCongTy { get; set; } = null!;
        public string? LogoHoacAvatarUrl { get; set; }
        public LoaiTaiKhoan LoaiTaiKhoanNguoiDang { get; set; }
        public string? UrlWebsiteCongTy { get; set; }
        public string? MoTaCongTy { get; set; }
        public bool CongTyDaXacMinh { get; set; }

        // Thông tin bổ sung
        public List<string> NganhNghes { get; set; } = new List<string>();
        public List<LichLamViecViewModel> LichLamViecs { get; set; } = new List<LichLamViecViewModel>();

        // Thông tin liên quan (nếu cần)
        // public List<TinTuyenDungKhacViewModel> TinLienQuanCuaCongTy { get; set; }
    }

    public class LichLamViecViewModel
    {
        public string NgayTrongTuanDisplay { get; set; } = null!;
        public string? ThoiGianDisplay { get; set; } // Ví dụ: "08:00 - 12:00", "Buổi Sáng"
    }
}