using HeThongTimViec.Models; // For NguoiDung, LoaiTaiKhoan
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net; // For WebUtility.HtmlEncode/Decode

namespace HeThongTimViec.ViewModels
{
    public class TinNhanTrangChinhViewModel
    {
        public List<CuocHoiThoaiViewModel> DanhSachCuocHoiThoai { get; set; } = new List<CuocHoiThoaiViewModel>();
        public int? NguoiLienHeDangChonId { get; set; }
        public NguoiDung? ThongTinNguoiLienHeDangChon { get; set; }
        public List<TinNhanChiTietViewModel> TinNhanTrongCuocHoiThoai { get; set; } = new List<TinNhanChiTietViewModel>();
        public GuiTinNhanViewModel FormGuiTinNhanMoi { get; set; } = new GuiTinNhanViewModel();
        public string CurrentUserThemeColor { get; set; } = "#007bff"; // Default theme color
    }

    public class CuocHoiThoaiViewModel
    {
        public int NguoiLienHeId { get; set; }
        public string TenNguoiLienHe { get; set; } = string.Empty;
        public string? UrlAvatarNguoiLienHe { get; set; }
        
        public string TinNhanCuoiCung { get; set; } = string.Empty;

        public DateTime ThoiGianTinNhanCuoi { get; set; }
        public int SoTinNhanMoi { get; set; }
        public bool CoTinNhanMoi => SoTinNhanMoi > 0;
        public bool DangChon { get; set; } // Quan trọng để view biết item nào active
        public bool LaTinNhanCuoiCuaToi { get; set; }
        public LoaiTaiKhoan? LoaiTaiKhoanNguoiLienHe { get; set; }
    }

    public class TinNhanChiTietViewModel
    {
        public int Id { get; set; }
        public int NguoiGuiId { get; set; }
        public string TenNguoiGui { get; set; } = string.Empty;
        public string? AvatarNguoiGui { get; set; }
        public string NoiDung { get; set; } = string.Empty; // Đã HTML encoded từ DB
        
        public DateTime NgayGui { get; set; }
        public bool LaCuaToi { get; set; }
        public bool DaXem { get; set; }

        public string NgayGuiFormatted => NgayGui.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        public string ThoiGianGuiNhan
        {
            get
            {
                if (NgayGui.Date == DateTime.Today)
                    return NgayGui.ToString("HH:mm");
                if (NgayGui.Date == DateTime.Today.AddDays(-1))
                    return "Hôm qua, " + NgayGui.ToString("HH:mm");
                return NgayGui.ToString("dd/MM HH:mm");
            }
        }
        public LoaiTaiKhoan? LoaiTaiKhoanNguoiGui { get; set; }
    }

    public class GuiTinNhanViewModel
    {
        [Required]
        public int NguoiNhanId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn.")]
        [StringLength(1000, ErrorMessage = "Nội dung tin nhắn không được vượt quá 1000 ký tự.")]
        public string NoiDung { get; set; } = string.Empty;

        public int? TinLienQuanId { get; set; }
        public int? UngTuyenLienQuanId { get; set; }
    }
}