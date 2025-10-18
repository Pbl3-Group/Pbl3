using HeThongTimViec.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.Dashboard
{
    public class DashboardJobPostingItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Ngày đăng")]
        public DateTime PostedDate { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiTinTuyenDung Status { get; set; }

        [Display(Name = "Số lượng ứng tuyển")]
        public int ApplicantCount { get; set; }
    }
}