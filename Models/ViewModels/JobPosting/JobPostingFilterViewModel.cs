// File: ViewModels/JobPosting/JobPostingFilterViewModel.cs
using HeThongTimViec.Models;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.JobPosting
{
    public class JobPostingFilterViewModel
    {
        [Display(Name = "Từ khóa")]
        public string? Keyword { get; set; }

        [Display(Name = "Ngành nghề")]
        public int? NganhNgheId { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public int? ThanhPhoId { get; set; }

        [Display(Name = "Quận/Huyện")]
        public int? QuanHuyenId { get; set; }

        [Display(Name = "Loại hình công việc")]
        public LoaiHinhCongViec? LoaiHinh { get; set; }

        [Display(Name = "Trạng thái tin")]
        public TrangThaiTinTuyenDung? TrangThai { get; set; }
    }
}