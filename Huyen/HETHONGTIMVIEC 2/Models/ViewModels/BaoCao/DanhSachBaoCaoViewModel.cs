// File: ViewModels/BaoCao/DanhSachBaoCaoViewModel.cs
using HeThongTimViec.ViewModels.TimViec;
using System.ComponentModel.DataAnnotations;
using HeThongTimViec.Models;

namespace HeThongTimViec.ViewModels.BaoCao
{
    public class DanhSachBaoCaoViewModel
    {
        public PaginatedList<BaoCaoItemViewModel> BaoCaos { get; set; } = new PaginatedList<BaoCaoItemViewModel>(new List<BaoCaoItemViewModel>(), 0, 1, 10);

        [Display(Name = "Từ khóa tìm kiếm")]
        [StringLength(100, ErrorMessage = "Từ khóa không được vượt quá 100 ký tự.")]
        public string? tuKhoa { get; set; } // THAY THẾ NGÀY BẰNG TỪ KHÓA

        [Display(Name = "Trạng thái")]
        public TrangThaiXuLyBaoCao? trangThai { get; set; }
   
    }
}