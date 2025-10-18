namespace HeThongTimViec.ViewModels.TrangChu
{
    public class FounderViewModel
    {
        public string Name { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Quote { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? GithubUrl { get; set; }
        // Add other social links if needed
    }
}
