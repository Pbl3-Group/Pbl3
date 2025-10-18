namespace HeThongTimViec.ViewModels.TrangChu
{
    public class FeaturedFieldViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; } // Placeholder image URL
        public int JobCount { get; set; }
        public string Slug { get; set; } = null!; // For URL generation
    }
}
