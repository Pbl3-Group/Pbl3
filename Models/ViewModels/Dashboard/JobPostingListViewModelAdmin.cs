// File: ViewModels/Dashboard/JobPostingListViewModelAdmin.cs
using System;
using System.Text.Json.Serialization;
using HeThongTimViec.Models;

namespace HeThongTimViec.ViewModels.Dashboard // Hoặc namespace của bạn
{
    public class JobPostingListViewModelAdmin
    {
        public int Id { get; set; }
        public string? TieuDe { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TrangThaiTinTuyenDung TrangThai { get; set; }

        public DateTime? NgayDang { get; set; }
        // public DateTime? NgayHetHan { get; set; } // Tùy chọn

        public int SoUngVien { get; set; }
        // public int LuotXem { get; set; } // Tùy chọn

        // --- CÁC TRƯỜNG MỚI ĐỂ HIỂN THỊ ĐẸP HƠN ---
        public string? TenCongTyHoacNguoiDang { get; set; }
        public string? DiaDiemDisplay { get; set; }
        public string? LoaiHinhDisplay { get; set; }
        public string? MucLuongDisplay { get; set; }
    }
}