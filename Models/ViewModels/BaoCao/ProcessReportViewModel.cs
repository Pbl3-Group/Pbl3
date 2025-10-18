// File: ViewModels/BaoCao/ProcessReportViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace HeThongTimViec.ViewModels.BaoCao
{
    public enum ReportActionType
    {
        [Display(Name = "Bỏ qua báo cáo")]
        Ignore,
        [Display(Name = "Xoá tin và Cảnh cáo người đăng")]
        WarnAndHide,
        [Display(Name = "Xoá tin và Đình chỉ tài khoản người đăng")]
        SuspendAndHide
    }

    public class ProcessReportViewModel
    {
        [Required]
        public int BaoCaoId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một hành động xử lý.")]
        [Display(Name = "Hành động")]
        public ReportActionType Action { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ghi chú của bạn.")]
        [Display(Name = "Ghi chú xử lý (sẽ được gửi tới người báo cáo)")]
        [DataType(DataType.MultilineText)]
        public string GhiChuAdmin { get; set; } = string.Empty;

        [Display(Name = "Nội dung cảnh cáo (gửi tới người vi phạm)")]
        [DataType(DataType.MultilineText)]
        public string? NoiDungCanhCao { get; set; }
    }
}