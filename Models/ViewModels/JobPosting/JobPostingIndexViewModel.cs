// File: ViewModels/JobPosting/JobPostingIndexViewModel.cs (ĐÚNG CẤU TRÚC)
using HeThongTimViec.ViewModels.TimViec; 
namespace HeThongTimViec.ViewModels.JobPosting
{
    /// <summary>
    /// ViewModel này đại diện cho TOÀN BỘ trang Index.
    /// Nó chứa cả bộ lọc và danh sách kết quả.
    /// </summary>
    public class JobPostingIndexViewModel
    {
        // Chứa các giá trị lọc hiện tại để hiển thị trên form
        public JobPostingFilterViewModel Filter { get; set; } = new();

        // Chứa danh sách các tin tuyển dụng đã được lọc.
        // Đây là một danh sách các đối tượng JobPostingListViewModel.
       public PaginatedList<JobPostingListViewModel>? Postings { get; set; }
    }
}