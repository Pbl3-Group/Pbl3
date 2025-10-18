namespace HeThongTimViec.ViewModels.TrangChu
{
    public class JobTypeViewModel
    {
        public string Name { get; set; } = null!;
        public string IconClass { get; set; } = null!; // e.g., "fas fa-clock"
        public int JobCount { get; set; }
        public string QueryParam { get; set; } = null!; // For search link, e.g. "loaiHinh=banthoigian"
    }
}