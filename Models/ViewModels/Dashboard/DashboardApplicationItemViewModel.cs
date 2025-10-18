using HeThongTimViec.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // For SkillTags

namespace HeThongTimViec.ViewModels.Dashboard
{
    public class DashboardApplicationItemViewModel
    {
        public int ApplicationId { get; set; } // ID của bản ghi UngTuyen
        public int CandidateId { get; set; }   // ID của NguoiDung (ứng viên)
        public int JobId { get; set; }         // ID của TinTuyenDung liên quan
        
        [Display(Name = "Công việc ứng tuyển")]
        public string JobTitle { get; set; } = string.Empty;

        [Display(Name = "Ứng viên")]
        public string ApplicantName { get; set; } = string.Empty;
        
        public string? ApplicantAvatarUrl { get; set; }

        [Display(Name = "Vị trí mong muốn (từ hồ sơ)")]
        public string? ApplicantProfilePosition { get; set; } // From HoSoUngVien.ViTriMongMuon or TieuDeHoSo

        [Display(Name = "Ngày nộp")]
        public DateTime AppliedDate { get; set; }

        [Display(Name = "Trạng thái ứng tuyển")]
        public TrangThaiUngTuyen Status { get; set; }

        // Optional: Could add a few key skills if readily available and desired for a quick glance
        // public List<string> TopSkills { get; set; } = new List<string>();
    }
}