using HeThongTimViec.Data;
using HeThongTimViec.Models; // Assuming ErrorViewModel is here
using HeThongTimViec.ViewModels.TrangChu;
using HeThongTimViec.ViewModels.TimViec; // For KetQuaTimViecItemViewModel
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering; // For EnumExtensions to use GetDisplayName()
using HeThongTimViec.ViewModels.CongTy;
using HeThongTimViec.Utils;


namespace HeThongTimViec.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // CẬP NHẬT: Phương thức Index giờ đây nhận tham số cho việc phân trang
        public async Task<IActionResult> Index(int fieldPage = 1, int jobPage = 1)
        {
            _logger.LogInformation("Home/Index page requested for FieldPage: {fieldPage}, JobPage: {jobPage}", fieldPage, jobPage);
            var viewModel = new HomePageViewModel();

            // CẬP NHẬT: Định nghĩa số lượng item trên mỗi trang
            const int FieldsPerPage = 6;
            const int JobsPerPage = 3;

            try
            {
                // Statistics (Không thay đổi)
                viewModel.Statistics.Add(new StatisticItemViewModel { Value = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today)) + "+", Label = "Việc làm đang tuyển" });
                viewModel.Statistics.Add(new StatisticItemViewModel { Value = await _context.NguoiDungs.CountAsync(u => u.LoaiTk == LoaiTaiKhoan.doanhnghiep && u.HoSoDoanhNghiep != null && u.HoSoDoanhNghiep.DaXacMinh) + "+", Label = "Doanh nghiệp tin cậy" });
                viewModel.Statistics.Add(new StatisticItemViewModel { Value = await _context.NguoiDungs.CountAsync(u => u.LoaiTk == LoaiTaiKhoan.canhan && u.HoSoUngVien != null) + "+", Label = "Ứng viên đã tìm việc" });
                viewModel.Statistics.Add(new StatisticItemViewModel { Value = "95%", Label = "Tỷ lệ hài lòng" });

                // CẬP NHẬT: Featured Fields (NganhNghe) với logic phân trang
                var fieldsQuery = _context.NganhNghes.AsNoTracking()
                    .Select(nn => new
                    {
                        nn.Id,
                        nn.Ten,
                        JobCount = nn.TinTuyenDungNganhNghes.Count(tnn => tnn.TinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daduyet && (tnn.TinTuyenDung.NgayHetHan == null || tnn.TinTuyenDung.NgayHetHan >= DateTime.Today))
                    })
                    .OrderByDescending(x => x.JobCount);

                var totalFields = await fieldsQuery.CountAsync();
                viewModel.FeaturedFieldsTotalPages = (int)Math.Ceiling((double)totalFields / FieldsPerPage);
                viewModel.FeaturedFieldsPageNumber = fieldPage;

                var featuredFieldsData = await fieldsQuery
                    .Skip((fieldPage - 1) * FieldsPerPage)
                    .Take(FieldsPerPage)
                    .ToListAsync();

                viewModel.FeaturedFields = featuredFieldsData.Select(nn => new FeaturedFieldViewModel
                {
                    Id = nn.Id,
                    Name = nn.Ten,
                    JobCount = nn.JobCount,
                    ImageUrl = $"/img/fields/{nn.Ten.ToLower().Replace(" ", "-").Replace("&", "and").Replace("/", "-")}.webp",
                    Slug = nn.Ten.ToLower().Replace(" ", "-").Replace("&", "and").Replace("/", "-")
                }).ToList();


                // Job Types Section (Không thay đổi)
                viewModel.JobTypes.Clear();
                viewModel.JobTypes.Add(new JobTypeViewModel { Name = LoaiHinhCongViec.banthoigian.GetDisplayName(), IconClass = "fas fa-user-clock", JobCount = await _context.TinTuyenDungs.CountAsync(t => t.LoaiHinhCongViec == LoaiHinhCongViec.banthoigian && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today)), QueryParam = $"LoaiHinhCongViec={LoaiHinhCongViec.banthoigian}" });
                viewModel.JobTypes.Add(new JobTypeViewModel { Name = LoaiHinhCongViec.thoivu.GetDisplayName(), IconClass = "fas fa-calendar-alt", JobCount = await _context.TinTuyenDungs.CountAsync(t => t.LoaiHinhCongViec == LoaiHinhCongViec.thoivu && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today)), QueryParam = $"LoaiHinhCongViec={LoaiHinhCongViec.thoivu}" });
                viewModel.JobTypes.Add(new JobTypeViewModel { Name = LoaiHinhCongViec.linhhoatkhac.GetDisplayName(), IconClass = "fas fa-people-arrows", JobCount = await _context.TinTuyenDungs.CountAsync(t => t.LoaiHinhCongViec == LoaiHinhCongViec.linhhoatkhac && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today)), QueryParam = $"LoaiHinhCongViec={LoaiHinhCongViec.linhhoatkhac}" });


                // CẬP NHẬT: Featured Jobs với logic phân trang
                var jobsQuery = _context.TinTuyenDungs
                    .AsNoTracking()
                    .Where(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today))
                    .OrderByDescending(t => t.TinGap)
                    .ThenByDescending(t => t.NgayDang);

                var totalJobs = await jobsQuery.CountAsync();
                viewModel.FeaturedJobsTotalPages = (int)Math.Ceiling((double)totalJobs / JobsPerPage);
                viewModel.FeaturedJobsPageNumber = jobPage;

                var rawFeaturedJobsData = await jobsQuery
                    .Skip((jobPage - 1) * JobsPerPage)
                    .Take(JobsPerPage)
                    .Include(t => t.NguoiDang)
                        .ThenInclude(nd => nd.HoSoDoanhNghiep)
                    .Include(t => t.QuanHuyen)
                    .Include(t => t.ThanhPho)
                    .Include(t => t.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                    .Select(t => new
                    {
                        JobAd = t,
                        NguoiDang = t.NguoiDang,
                        HoSoDoanhNghiep = t.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? t.NguoiDang.HoSoDoanhNghiep : null,
                        QuanHuyenName = t.QuanHuyen.Ten,
                        ThanhPhoName = t.ThanhPho.Ten,
                        NganhNgheNho = t.TinTuyenDungNganhNghes.Select(nn => nn.NganhNghe.Ten).Take(3).ToList()
                    })
                    .ToListAsync();

                viewModel.FeaturedJobs = rawFeaturedJobsData.Select(data => new KetQuaTimViecItemViewModel
                {
                    Id = data.JobAd.Id,
                    TieuDe = data.JobAd.TieuDe,
                    TenCongTyHoacNguoiDang = data.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && data.HoSoDoanhNghiep != null
                                            ? data.HoSoDoanhNghiep.TenCongTy
                                            : data.NguoiDang.HoTen,
                    LogoHoacAvatarUrl = data.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && data.HoSoDoanhNghiep != null
                                        ? data.HoSoDoanhNghiep.UrlLogo
                                        : data.NguoiDang.UrlAvatar,
                    LoaiTaiKhoanNguoiDang = data.NguoiDang.LoaiTk,
                    DiaDiem = $"{data.QuanHuyenName}, {data.ThanhPhoName}",
                    LoaiHinhCongViecDisplay = data.JobAd.LoaiHinhCongViec.GetDisplayName(),
                    MucLuongDisplay = FormatMucLuong(data.JobAd.LoaiLuong, data.JobAd.LuongToiThieu, data.JobAd.LuongToiDa),
                    NgayDang = data.JobAd.NgayDang,
                    NgayHetHan = data.JobAd.NgayHetHan,
                    TinGap = data.JobAd.TinGap,
                    DaLuu = false,
                    DaUngTuyen = false,
                    NganhNgheNho = data.NganhNgheNho,
                    YeuCauKinhNghiemText = data.JobAd.YeuCauKinhNghiemText,
                    YeuCauHocVanText = data.JobAd.YeuCauHocVanText
                }).ToList();

                // Các truy vấn còn lại giữ nguyên
                // Top Companies (Không thay đổi)
                var topCompaniesData = await _context.HoSoDoanhNghieps
                    .AsNoTracking()
                    .Include(hsdn => hsdn.NguoiDung)
                    .Where(hsdn => hsdn.DaXacMinh && !string.IsNullOrEmpty(hsdn.UrlLogo))
                    .Select(hsdn => new
                    {
                        hsdn.NguoiDungId,
                        hsdn.TenCongTy,
                        hsdn.UrlLogo,
                        ActiveJobCount = hsdn.NguoiDung.TinTuyenDungsDaDang.Count(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today))
                    })
                    .OrderByDescending(c => c.ActiveJobCount)
                    .Take(12)
                    .ToListAsync();
                viewModel.TopCompanies = topCompaniesData.Select(c => new TopCompanyViewModel
                {
                    Id = c.NguoiDungId,
                    Name = c.TenCongTy,
                    LogoUrl = c.UrlLogo,
                    ProfileSlug = c.TenCongTy.ToLower().Replace(" ", "-").Replace("&", "and").Replace("/", "-") + "-" + c.NguoiDungId
                }).ToList();

                // Dữ liệu cho Form tìm kiếm (Không thay đổi)
                var thanhPhoList = await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync();
                viewModel.ThanhPhoOptions = new SelectList(thanhPhoList, "Id", "Ten");
                var loaiHinhItems = EnumExtensions.GetSelectList<LoaiHinhCongViec>(includeDefaultItem: true, defaultItemText: "Tất cả loại hình", defaultItemValue: "");
                viewModel.LoaiHinhCongViecOptions = new SelectList(loaiHinhItems, "Value", "Text");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Home/Index page data.");
            }

            return View(viewModel);
        }

        // Các phương thức khác (FormatMucLuong, Contact, About, etc.) giữ nguyên
        private static string FormatMucLuong(LoaiLuong loaiLuong, ulong? luongToiThieu, ulong? luongToiDa)
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
            string suffix = loaiLuong.GetDisplayName().ToLowerInvariant() switch
            {
                "theo giờ" => "/giờ",
                "theo ngày" => "/ngày",
                "theo ca" => "/ca",
                "theo tháng" => "/tháng",
                "theo dự án" => "/dự án",
                _ => ""
            };
            if (!string.IsNullOrEmpty(suffix) && !suffix.StartsWith(" /")) { suffix = " " + suffix; }
            string formattedMin = luongToiThieu.HasValue ? $"{luongToiThieu.Value:N0}" : "";
            string formattedMax = luongToiDa.HasValue ? $"{luongToiDa.Value:N0}" : "";
            if (luongToiThieu.HasValue && luongToiDa.HasValue)
            {
                if (luongToiThieu.Value == luongToiDa.Value) return $"{formattedMin} VNĐ{suffix}";
                else return $"{formattedMin} - {formattedMax} VNĐ{suffix}";
            }
            if (luongToiThieu.HasValue) return $"Từ {formattedMin} VNĐ{suffix}";
            if (luongToiDa.HasValue) return $"Đến {formattedMax} VNĐ{suffix}";
            return "Thương lượng";
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubscribeNewsletter(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                _logger.LogInformation("Newsletter subscription attempt for email: {Email}", email);
                // TODO: Actual subscription logic
                TempData["NewsletterMessage"] = "Cảm ơn bạn đã đăng ký nhận bản tin!";
                TempData["NewsletterStatus"] = "success";
            }
            else
            {
                TempData["NewsletterMessage"] = "Vui lòng nhập một địa chỉ email hợp lệ.";
                TempData["NewsletterStatus"] = "error";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new HETHONGTIMVIEC.Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Contact()
        {
            _logger.LogInformation("Contact Us page requested at {Time}", DateTime.UtcNow);
            ViewData["Title"] = "Liên Hệ Với Chúng Tôi";
            var model = new ContactFormViewModel(); // Khởi tạo model rỗng cho form
            return View(model);
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel model)
        {
            ViewData["Title"] = "Liên Hệ Với Chúng Tôi";
            _logger.LogInformation("Contact form submission attempted by {Email} at {Time}", model.Email, DateTime.UtcNow);

            if (ModelState.IsValid)
            {
                try
                {
                    // TODO: Xử lý logic gửi email hoặc lưu thông tin liên hệ vào cơ sở dữ liệu
                    // Ví dụ: Gửi email thông báo cho admin
                    // string adminEmail = "admin@joxflex.com";
                    // string emailSubject = $"Liên hệ từ JOXFLEX: {model.Subject}";
                    // string emailBody = $"Bạn có một liên hệ mới từ: {model.Name} ({model.Email})\n" +
                    //                    $"Số điện thoại: {model.PhoneNumber ?? "Không cung cấp"}\n" +
                    //                    $"Chủ đề: {model.Subject}\n\n" +
                    //                    $"Nội dung:\n{model.Message}";
                    // await _emailSender.SendEmailAsync(adminEmail, emailSubject, emailBody);

                    // Lưu vào DB (ví dụ, nếu bạn có bảng ContactSubmissions)
                    // var submission = new ContactSubmission
                    // {
                    //     Name = model.Name,
                    //     Email = model.Email,
                    //     PhoneNumber = model.PhoneNumber,
                    //     Subject = model.Subject,
                    //     Message = model.Message,
                    //     SubmittedAt = DateTime.UtcNow
                    // };
                    // _context.ContactSubmissions.Add(submission);
                    // await _context.SaveChangesAsync();

                    _logger.LogInformation("Contact form successfully processed for {Email}", model.Email);
                    TempData["ContactMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi đã nhận được tin nhắn của bạn và sẽ phản hồi trong thời gian sớm nhất.";
                    TempData["ContactStatus"] = "success";
                    // Redirect về trang Contact GET để hiển thị thông báo và làm mới form
                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing contact form for {Email}", model.Email);
                    TempData["ContactMessage"] = "Đã có lỗi xảy ra trong quá trình gửi liên hệ. Vui lòng thử lại sau.";
                    TempData["ContactStatus"] = "danger"; // Hoặc "error" tùy theo class alert của bạn
                    // Trả về view với model hiện tại để người dùng không mất dữ liệu đã nhập
                    return View(model);
                }
            }

            // Nếu ModelState không hợp lệ
            _logger.LogWarning("Contact form submission failed validation for {Email}. Errors: {Errors}", model.Email, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["ContactMessage"] = "Thông tin bạn cung cấp không hợp lệ. Vui lòng kiểm tra lại các trường được đánh dấu.";
            TempData["ContactStatus"] = "warning";
            return View(model); // Trả về view với model và các lỗi validation
        }
        // GET: /Home/About
        public IActionResult About()
        {
            var foundersList = new List<FounderViewModel>
    {
                new FounderViewModel { Name="Nguyễn Thanh Huyền", Title="Đồng sáng lập & CEO", Quote="Chúng tôi tin rằng mọi người đều xứng đáng có cơ hội việc làm phù hợp với lịch trình và kỹ năng của họ.",  AvatarUrl="/file/img/founders/founder1.jpg", TwitterUrl="#", GithubUrl="#" },
                new FounderViewModel { Name="Nguyễn Thị Bích Uyên", Title="Đồng sáng lập & COO", Quote="JOXFLEX được tạo ra để giải quyết thách thức về việc làm linh hoạt, kết nối đúng người với đúng công việc.", AvatarUrl="/file/img/founders/founder22.jpg", TwitterUrl="#", GithubUrl="#" },
                new FounderViewModel { Name="Trần Thị Phượng", Title="Đồng sáng lập & CTO", Quote="Công nghệ của chúng tôi được xây dựng để tạo ra trải nghiệm tìm việc và tuyển dụng đơn giản và hiệu quả nhất.", AvatarUrl="/file/img/founders/founder3.jpg", TwitterUrl="#", GithubUrl="#" }
    };
            // Hoặc lấy từ một service/database nếu cần

            return View(foundersList);
        }
// FILE: Controllers/CongTyController.cs (hoặc tên controller tương ứng của bạn)

// Thêm asp-route-searchString vào các link phân trang trong View để giữ lại từ khóa tìm kiếm
[HttpGet]
public async Task<IActionResult> DanhSach(string searchString, int page = 1)
{
    const int PageSize = 12; // 12 công ty mỗi trang

    // Bắt đầu truy vấn cơ sở, chỉ lấy các công ty đã xác minh
    var companiesQuery = _context.HoSoDoanhNghieps
        .AsNoTracking()
        .Where(hsdn => hsdn.DaXacMinh);

    // *** CẬP NHẬT: ÁP DỤNG BỘ LỌC TÌM KIẾM ***
    // Nếu người dùng có nhập từ khóa tìm kiếm (searchString không rỗng)
    if (!string.IsNullOrEmpty(searchString))
    {
        // Lọc các công ty có Tên hoặc Địa chỉ chứa từ khóa tìm kiếm
        // ToLower() để tìm kiếm không phân biệt chữ hoa/thường
        string searchTerm = searchString.ToLower();
        companiesQuery = companiesQuery.Where(c => 
            c.TenCongTy.ToLower().Contains(searchTerm) || 
            (c.DiaChiDangKy != null && c.DiaChiDangKy.ToLower().Contains(searchTerm))
        );
    }
    
    // Tạo truy vấn chiếu (projection) để lấy các trường cần thiết
    // Sắp xếp các công ty có nhiều việc làm lên đầu
    var queryProjection = companiesQuery
        .Select(hsdn => new
        {
            hsdn.NguoiDungId,
            hsdn.TenCongTy,
            hsdn.UrlLogo,
            hsdn.MoTa,
            ThanhPhoName = hsdn.DiaChiDangKy,
            
            // Tính số việc làm đang hoạt động
            ActiveJobCount = hsdn.NguoiDung.TinTuyenDungsDaDang.Count(t => 
                t.TrangThai == Models.TrangThaiTinTuyenDung.daduyet && 
                (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today))
        })
        .OrderByDescending(c => c.ActiveJobCount);

    // Tính toán phân trang
    var totalCompanies = await queryProjection.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCompanies / (double)PageSize);

    // Lấy dữ liệu cho trang hiện tại
    var companiesData = await queryProjection
        .Skip((page - 1) * PageSize)
        .Take(PageSize)
        .ToListAsync();

    // Chuẩn bị ViewModel để truyền sang View
    var viewModel = new DanhSachCongTyViewModel
    {
        PageNumber = page,
        TotalPages = totalPages,
        Companies = companiesData.Select(c => new CompanyItemViewModel
        {
            TenCongTy = c.TenCongTy,
            LogoUrl = c.UrlLogo,
            DiaDiem = !string.IsNullOrEmpty(c.ThanhPhoName) ? c.ThanhPhoName : "Nhiều địa điểm",
            // Cắt ngắn mô tả để hiển thị
            MoTaNgan = c.MoTa != null && c.MoTa.Length > 120 
                        ? c.MoTa.Substring(0, 120) + "..." 
                        : c.MoTa,
            SoViecLamDangTuyen = c.ActiveJobCount,
            // Tạo slug (đảm bảo bạn có helper này)
            ProfileSlug = SeoUrlHelper.GenerateSlug(c.TenCongTy) + "-" + c.NguoiDungId
        }).ToList()
    };
    
    // Truyền lại từ khóa tìm kiếm sang View để giữ lại giá trị trong ô input
    ViewData["CurrentFilter"] = searchString;

    return View(viewModel);
}

// Giả định bạn có một helper class như thế này để tạo slug
// Nếu đã có ở nơi khác thì không cần thêm lại
public static class SeoUrlHelper
{
    public static string GenerateSlug(string phrase)
    {
        if (string.IsNullOrEmpty(phrase)) return "";
        string str = phrase.ToLower();
        // Cần một logic bỏ dấu đầy đủ hơn cho tiếng Việt
        // Đây chỉ là ví dụ đơn giản
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-"); 
        return str;
    }
}
    }
}