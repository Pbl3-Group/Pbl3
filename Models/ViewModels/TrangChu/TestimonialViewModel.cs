namespace HeThongTimViec.ViewModels.TrangChu
{
    public class TestimonialViewModel
    {
        public string AuthorName { get; set; } = null!;
        public string AuthorTitle { get; set; } = null!;
        public string Quote { get; set; } = null!;
        public int Rating { get; set; } // 1-5
        public string? AvatarUrl { get; set; }
    }
}
