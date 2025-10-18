namespace HeThongTimViec.ViewModels.CongTy
{
    public class CompanyItemViewModel
    {
        public string? ProfileSlug { get; set; }
        public required string TenCongTy { get; set; }
        public string? LogoUrl { get; set; }
        public string? MoTaNgan { get; set; }
        public required string DiaDiem { get; set; }
        public int SoViecLamDangTuyen { get; set; }
    }
}