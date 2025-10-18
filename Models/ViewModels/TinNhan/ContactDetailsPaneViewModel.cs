// File: ViewModels/TinNhan/ContactDetailsPaneViewModel.cs
namespace HeThongTimViec.ViewModels.TinNhan
{
    public class ContactDetailsPaneViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string UserType { get; set; } = string.Empty; // "Doanh nghiệp", "Ứng viên", "Cá nhân"

        // For DoanhNghiep
        public string? CompanyName { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? CompanyDescription { get; set; } // Thêm mô tả công ty

        // For UngVien/CaNhan
        public string? CandidateProfileTitle { get; set; }
        public string? CandidateDesiredPosition { get; set; }
        public string? CandidateIntroduction { get; set; } // Thêm giới thiệu bản thân

        // Job Context
        public string? RelatedJobTitle { get; set; }
        public string? RelatedJobUrl { get; set; }
        public int? RelatedTinTuyenDungId { get; set; }
        public int? RelatedUngTuyenId { get; set; }
        public string? ApplicationStatus { get; set; }
        public string? JobDescriptionSummary { get; set; } // Thêm mô tả tóm tắt công việc
    }
}