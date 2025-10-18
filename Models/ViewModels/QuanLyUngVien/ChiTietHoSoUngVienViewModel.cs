using HeThongTimViec.Models;
using System;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.QuanLyUngVien
{
    public class ChiTietHoSoUngVienViewModel
    {
        public int UngVienId { get; set; }
        public int? UngTuyenIdContext { get; set; } // If viewing in context of a specific application
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GioiTinh {get; set;}
        public DateTime? NgaySinh {get; set;}
        public string? DiaChiDayDu { get; set; }


        // HoSoUngVien Details
        public string? TieuDeHoSo { get; set; }
        public string? GioiThieuBanThan { get; set; }
        public string? ViTriMongMuon { get; set; }
        public string? LoaiLuongMongMuonDisplay { get; set; }
        public string? MucLuongMongMuonDisplay { get; set; }
        public string? UrlCvMacDinh { get; set; } // From HoSoUngVien

        // UngTuyen specific (if UngTuyenIdContext is present)
        public string? UrlCvDaNopChoTinNay { get; set; }
        public string? ThuGioiThieuChoTinNay { get; set; }
        public DateTime? NgayNopUngTuyen { get; set; }
        public TrangThaiUngTuyen? TrangThaiUngTuyenHienTai { get; set; }
        public string? TenTinTuyenDungUngTuyen { get; set; }


        public List<LichRanhDisplayViewModel> LichRanhs { get; set; } = new List<LichRanhDisplayViewModel>();
        public List<string> DiaDiemMongMuonsDisplay { get; set; } = new List<string>();

        // Add more collections for Education, WorkExperience if you have those models
        // public List<KinhNghiemLamViecViewModel> KinhNghiemLamViecs { get; set; } = new List<KinhNghiemLamViecViewModel>();
        // public List<HocVanViewModel> HocVans { get; set; } = new List<HocVanViewModel>();
    }

    public class LichRanhDisplayViewModel
    {
        public string? NgayTrongTuan { get; set; }
        public string? BuoiLamViec { get; set; }
    }
}