using HeThongTimViec.Models;
using System;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.QuanLyUngVien
{
    public class UngVienItemViewModel
    {
        public int UngVienId { get; set; }
        public int UngTuyenId { get; set; }
        public string? HoTenUngVien { get; set; }
        public string? AvatarUrl { get; set; }
        public string? ViTriHoSo { get; set; } // E.g., "Lập trình viên Frontend" from HoSoUngVien.TieuDeHoSo or ViTriMongMuon
        public string? KinhNghiemDisplay { get; set; } // E.g., "3 năm kinh nghiệm" - Textual, from HoSoUngVien or future structured data
        public string? ThanhPhoUngVien { get; set; } // Candidate's primary city from NguoiDung
        public List<string> SkillTags { get; set; } = new List<string>(); // Placeholder for skills like "React", "TypeScript"
        public DateTime NgayNopHoSo { get; set; }
        public TrangThaiUngTuyen TrangThaiHienTai { get; set; }
        public string? TrangThaiHienTaiDisplay { get; set; }
        public string? TrangThaiBadgeClass { get; set; }
        public string? UrlCvDaNop { get; set; }
        public int TinTuyenDungIdLienQuan { get; set; } // To know which job this application is for
    }
}