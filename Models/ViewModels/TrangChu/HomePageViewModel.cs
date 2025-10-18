using HeThongTimViec.Models; // For LoaiTaiKhoan, LoaiLuong etc. if needed for display logic
using HeThongTimViec.ViewModels.TimViec; // For KetQuaTimViecItemViewModel
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace HeThongTimViec.ViewModels.TrangChu
{
    public class HomePageViewModel
    {
        public string HeroSearchKeyword { get; set; } = string.Empty;
        public string? HeroImageUrl { get; set; }
        
         // THUỘC TÍNH MỚI CHO BỘ LỌC
            public int? SearchThanhPhoId { get; set; }
            public LoaiHinhCongViec? SearchLoaiHinhCongViec { get; set; }
    public SelectList? ThanhPhoOptions { get; set; }
    public SelectList? LoaiHinhCongViecOptions { get; set; }

        public List<StatisticItemViewModel> Statistics { get; set; } = new List<StatisticItemViewModel>();
        public List<BenefitItemViewModel> Benefits { get; set; } = new List<BenefitItemViewModel>();
        public List<FeaturedFieldViewModel> FeaturedFields { get; set; } = new List<FeaturedFieldViewModel>();
        public List<JobTypeViewModel> JobTypes { get; set; } = new List<JobTypeViewModel>();
        public List<KetQuaTimViecItemViewModel> FeaturedJobs { get; set; } = new List<KetQuaTimViecItemViewModel>();
        public List<TopCompanyViewModel> TopCompanies { get; set; } = new List<TopCompanyViewModel>();
        public List<TestimonialViewModel> Testimonials { get; set; } = new List<TestimonialViewModel>();
        public List<HowItWorksStepViewModel> HowItWorksSteps { get; set; } = new List<HowItWorksStepViewModel>();
        public List<FounderViewModel> Founders { get; set; } = new List<FounderViewModel>();
        public List<KetQuaTimViecItemViewModel> NewJobs { get; set; }
         
                 // THUỘC TÍNH MỚI CHO PHÂN TRANG "LĨNH VỰC"
        public int FeaturedFieldsPageNumber { get; set; }
        public int FeaturedFieldsTotalPages { get; set; }

        // THUỘC TÍNH MỚI CHO PHÂN TRANG "VIỆC LÀM NỔI BẬT"
        public int FeaturedJobsPageNumber { get; set; }
        public int FeaturedJobsTotalPages { get; set; }

        public HomePageViewModel()
        {
            NewJobs = new List<KetQuaTimViecItemViewModel>();
            // You can pre-populate static/semi-static data here or in the controller
            Benefits = new List<BenefitItemViewModel>
            {
                new BenefitItemViewModel { IconClass = "fas fa-search-dollar", Title = "Tìm kiếm thông minh", Description = "Hệ thống tìm kiếm nâng cao với nhiều bộ lọc giúp bạn tìm được công việc phù hợp nhất." },
                new BenefitItemViewModel { IconClass = "fas fa-award", Title = "Việc làm chất lượng", Description = "Tất cả các công việc đều được kiểm duyệt kỹ lưỡng để đảm bảo chất lượng và độ tin cậy." },
                new BenefitItemViewModel { IconClass = "fas fa-shield-alt", Title = "Uy tín & An toàn", Description = "Hệ thống đánh giá hai chiều giúp xây dựng môi trường làm việc minh bạch và đáng tin cậy." },
                new BenefitItemViewModel { IconClass = "fas fa-sync-alt", Title = "Vai trò linh hoạt", Description = "Dễ dàng chuyển đổi giữa vai trò ứng viên và nhà tuyển dụng khi cần thiết." },
                new BenefitItemViewModel { IconClass = "fas fa-comments", Title = "Chat trực tiếp", Description = "Trao đổi trực tiếp với nhà tuyển dụng hoặc ứng viên để làm rõ thông tin công việc." },
                new BenefitItemViewModel { IconClass = "fas fa-bell", Title = "Thông báo thông minh", Description = "Nhận thông báo về các công việc phù hợp với hồ sơ và kỹ năng của bạn." }
            };

            HowItWorksSteps = new List<HowItWorksStepViewModel>
            {
                new HowItWorksStepViewModel { Number="01", IconClass="fas fa-user-plus", Title="Tạo tài khoản", Description="Đăng ký tài khoản và hoàn thiện hồ sơ cá nhân của bạn." },
                new HowItWorksStepViewModel { Number="02", IconClass="fas fa-briefcase", Title="Tìm việc phù hợp", Description="Tìm kiếm và ứng tuyển vào các công việc phù hợp với kỹ năng và lịch trình của bạn." },
                new HowItWorksStepViewModel { Number="03", IconClass="fas fa-rocket", Title="Bắt đầu làm việc", Description="Nhận việc và bắt đầu kiếm thu nhập từ công việc bán thời gian." }
            };

            Testimonials = new List<TestimonialViewModel> // Sample data
            {
                new TestimonialViewModel { AuthorName = "Nguyễn Văn A", AuthorTitle = "Sinh viên", Quote = "Tôi đã tìm được công việc bán thời gian phù hợp với lịch học của mình chỉ sau 2 ngày đăng ký. Giao diện dễ sử dụng và có rất nhiều lựa chọn việc làm đa dạng.", Rating = 5, AvatarUrl="/file/img/Avatar/avatar1.jpg" },
                new TestimonialViewModel { AuthorName = "Trần Thị B", AuthorTitle = "Nhà tuyển dụng", Quote = "JOXFLEX giúp chúng tôi tìm kiếm nhân viên bán thời gian một cách nhanh chóng và hiệu quả. Chất lượng ứng viên rất tốt và phù hợp với yêu cầu của công ty.", Rating = 5, AvatarUrl="/file/img/Avatar/avatar2.jpg" },
                new TestimonialViewModel { AuthorName = "Lê Văn C", AuthorTitle = "Freelancer", Quote = "Tôi đã thử nhiều nền tảng tìm việc khác nhau nhưng JOXFLEX là lựa chọn tốt nhất cho công việc thời vụ. Quy trình ứng tuyển đơn giản và tôi nhanh chóng nhận được phản hồi.", Rating = 4, AvatarUrl="/file/img/Avatar/avatar3.jpg" }
            };

            Founders = new List<FounderViewModel> // Sample data
            {
                new FounderViewModel { Name="Nguyễn Thanh Huyền", Title="Đồng sáng lập & CEO", Quote="Chúng tôi tin rằng mọi người đều xứng đáng có cơ hội việc làm phù hợp với lịch trình và kỹ năng của họ.",  AvatarUrl="/file/img/founders/founder1.jpg", TwitterUrl="#", GithubUrl="#" },
                new FounderViewModel { Name="Nguyễn Thị Bích Uyên", Title="Đồng sáng lập & COO", Quote="JOXFLEX được tạo ra để giải quyết thách thức về việc làm linh hoạt, kết nối đúng người với đúng công việc.", AvatarUrl="/file/img/founders/founder22.jpg", TwitterUrl="#", GithubUrl="#" },
                new FounderViewModel { Name="Trần Thị Phượng", Title="Đồng sáng lập & CTO", Quote="Công nghệ của chúng tôi được xây dựng để tạo ra trải nghiệm tìm việc và tuyển dụng đơn giản và hiệu quả nhất.", AvatarUrl="/file/img/founders/founder3.jpg", TwitterUrl="#", GithubUrl="#" }
            };
            HeroImageUrl = "/images/hero/joxflex-hero-bg.webp"; // Example path
        }
    }
}