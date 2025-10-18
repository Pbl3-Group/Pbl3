using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.CongTy
{
    public class DanhSachCongTyViewModel
    {
        public List<CompanyItemViewModel> Companies { get; set; } = new List<CompanyItemViewModel>();
        
        // Thuộc tính cho phân trang
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}