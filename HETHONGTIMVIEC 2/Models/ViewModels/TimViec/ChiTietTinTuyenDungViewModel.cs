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
        public string MoTa { get; set; } = null!; // Mô tả chi tiết công việc
        public string? YeuCau { get; set; } // Yêu cầu ứng viên
        public string? QuyenLoi { get; set; } // Quyền lợi được hưởng
        public string LoaiHinhCongViecDisplay { get; set; } = null!;
        public string LoaiLuongDisplay { get; set; } = null!;
        public string MucLuongDisplay { get; set; } = null!;
        public string DiaChiLamViecDayDu { get; set; } = null!; // Địa chỉ làm việc đầy đủ (bao gồm quận, thành phố)
        public string YeuCauKinhNghiemText { get; set; } = null!;
        public string YeuCauHocVanText { get; set; } = null!;
        public int SoLuongTuyen { get; set; }
        public bool TinGap { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; } // Ngày hết hạn nộp hồ sơ
        public bool DaLuu { get; set; } // Người dùng hiện tại đã lưu tin này chưa?
        public bool DaUngTuyen { get; set; } // Người dùng hiện tại đã ứng tuyển tin này chưa?

        // Recruiter/Poster Information
        public int NguoiDangId { get; set; } // ID của người đăng tin
        public string TenNguoiDangHoacCongTy { get; set; } = null!;
        public string? LogoHoacAvatarUrl { get; set; }
        public LoaiTaiKhoan LoaiTaiKhoanNguoiDang { get; set; }
        public string? UrlWebsiteCongTy { get; set; } // Website công ty (nếu người đăng là doanh nghiệp)
        public string? MoTaCongTy { get; set; } // Mô tả công ty (nếu là doanh nghiệp)
        public bool CongTyDaXacMinh { get; set; } // Công ty đã được xác minh chưa?
        public string? EmailLienHe { get; set; } // Email liên hệ của người đăng/công ty
        public string? SdtLienHe { get; set; } // SĐT liên hệ
        public string? DiaChiLienHeNguoiDang { get; set; } // Địa chỉ của người đăng/công ty

        // Additional Information
        public List<string> NganhNghes { get; set; } = new List<string>(); // Danh sách các ngành nghề của tin
        public List<LichLamViecViewModel> LichLamViecs { get; set; } = new List<LichLamViecViewModel>(); // Lịch làm việc của công việc
    }

    // ViewModel cho một mục lịch làm việc
    public class LichLamViecViewModel
    {
        public string NgayTrongTuanDisplay { get; set; } = null!;
        public string? ThoiGianDisplay { get; set; } // VD: "08:00 - 12:00" hoặc "Buổi sáng"
    }
}