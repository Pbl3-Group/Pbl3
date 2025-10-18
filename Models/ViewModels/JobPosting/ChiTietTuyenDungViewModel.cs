// File: ViewModels/JobPosting/ChiTietTuyenDungViewModel.cs
using HeThongTimViec.Models;
using System.Collections.Generic;
using System;

namespace HeThongTimViec.ViewModels.JobPosting
{
    public class ChiTietTuyenDungViewModel
    {
        public int Id { get; set; }
        public required string TieuDe { get; set; }
        public required string MoTa { get; set; }
        public string? YeuCau { get; set; }
        public string? QuyenLoi { get; set; }
        public bool TinGap { get; set; }

        // Thông tin nhà tuyển dụng
        public int NguoiDangId { get; set; }
        public required string TenNguoiDang { get; set; }
        public string? LogoUrl { get; set; }
        public LoaiTaiKhoan LoaiTkNguoiDang { get; set; }

        // Thông tin tóm tắt
        public string ? MucLuongDisplay { get; set; }
        public required string LoaiHinhDisplay { get; set; }
        public string ? DiaDiemLamViec { get; set; } // Kết hợp từ địa chỉ, quận, thành phố
        public string ? YeuCauKinhNghiemText { get; set; }
        public string ? YeuCauHocVanText { get; set; }
        public int SoLuongTuyen { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public int LuotXem { get; set; }

        // Dữ liệu quan hệ
        public List<string> NganhNghes { get; set; } = new List<string>();
        public List<LichLamViecViewModel> LichLamViecs { get; set; } = new List<LichLamViecViewModel>();

        // Trạng thái tương tác của người dùng hiện tại
        public bool IsSaved { get; set; } // Người dùng đã lưu tin này chưa?
        public bool IsApplied { get; set; } // Người dùng đã ứng tuyển tin này chưa?

        // Trạng thái của tin
        public bool IsExpired => NgayHetHan.HasValue && NgayHetHan.Value.Date < DateTime.UtcNow.Date;
    }
}