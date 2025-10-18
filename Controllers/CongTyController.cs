// FILE: Controllers/CongTyController.cs

using HeThongTimViec.Data;
using HeThongTimViec.ViewModels.CongTy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using System.Security.Claims;
using HeThongTimViec.ViewModels.TimViec;
using HeThongTimViec.Models;

namespace HeThongTimViec.Controllers
{
    public class CongTyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CongTyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === PHIÊN BẢN CÔNG KHAI HOÀN TOÀN + PHÂN TRANG VIỆC LÀM ===
        [HttpGet("CongTy/ChiTiet/{slug}")]
        public async Task<IActionResult> ChiTiet(string slug, int jobPage = 1) // Thêm tham số jobPage
        {
            const int JobsPerPage = 5; // Số việc làm trên mỗi trang

            if (string.IsNullOrEmpty(slug)) { return NotFound(); }

            var slugParts = slug.Split('-');
            if (!int.TryParse(slugParts.LastOrDefault(), out var companyUserId))
            {
                return NotFound("Slug không hợp lệ.");
            }

            var companyProfile = await _context.HoSoDoanhNghieps
                .AsNoTracking()
                .Include(c => c.NguoiDung)
                    .ThenInclude(u => u.ThanhPho)
                .Include(c => c.NguoiDung)
                    .ThenInclude(u => u.QuanHuyen)
                .FirstOrDefaultAsync(c => c.NguoiDungId == companyUserId);

            if (companyProfile == null || companyProfile.NguoiDung == null)
            {
                return NotFound("Không tìm thấy công ty.");
            }

            // *** BẮT ĐẦU LOGIC PHÂN TRANG CHO VIỆC LÀM ***
            // 1. Tạo truy vấn gốc
            var jobsQuery = _context.TinTuyenDungs
                .AsNoTracking()
                .Where(t => t.NguoiDangId == companyUserId && 
                            t.TrangThai == TrangThaiTinTuyenDung.daduyet &&
                            (t.NgayHetHan == null || t.NgayHetHan >= DateTime.Today))
                .OrderByDescending(t => t.NgayDang);
            
            // 2. Đếm tổng số lượng việc làm từ truy vấn gốc
            var totalJobs = await jobsQuery.CountAsync();

            // 3. Lấy dữ liệu cho trang hiện tại
            var jobs = await jobsQuery
                .Skip((jobPage - 1) * JobsPerPage)
                .Take(JobsPerPage)
                .Include(t => t.ThanhPho).Include(t => t.QuanHuyen)
                .Select(t => new KetQuaTimViecItemViewModel
                {
                    Id = t.Id,
                    TieuDe = t.TieuDe,
                    TenCongTyHoacNguoiDang = companyProfile.TenCongTy,
                    LogoHoacAvatarUrl = companyProfile.UrlLogo,
                    DiaDiem = $"{t.QuanHuyen.Ten}, {t.ThanhPho.Ten}",
                    MucLuongDisplay = FormatMucLuong(t.LoaiLuong, t.LuongToiThieu, t.LuongToiDa),
                    LoaiHinhCongViecDisplay = t.LoaiHinhCongViec.GetDisplayName()
                })
                .ToListAsync();
            
            // Logic kiểm tra IsOwner đã được loại bỏ

            // Tạo ViewModel và điền tất cả các trường dữ liệu
            var viewModel = new CompanyDetailViewModel
            {
                // Thông tin công ty
                TenCongTy = companyProfile.TenCongTy,
                UrlLogo = companyProfile.UrlLogo,
                UrlWebsite = companyProfile.UrlWebsite,
                MoTa = companyProfile.MoTa,
                DiaChiDangKy = companyProfile.DiaChiDangKy,
                QuyMoCongTy = companyProfile.QuyMoCongTy,
                DaXacMinh = companyProfile.DaXacMinh,
                DiaDiemLienHe = companyProfile.NguoiDung.ThanhPho?.Ten ?? "Nhiều chi nhánh",
                MaSoThue = companyProfile.MaSoThue,

                // Thông tin người đại diện
                HoTenNguoiDaiDien = companyProfile.NguoiDung.HoTen,
                GioiTinhNguoiDaiDien = companyProfile.NguoiDung.GioiTinh,
                NgaySinhNguoiDaiDien = companyProfile.NguoiDung.NgaySinh,
                EmailLienHe = companyProfile.NguoiDung.Email,
                SoDienThoaiLienHe = companyProfile.NguoiDung.Sdt,

                // *** THÔNG TIN PHÂN TRANG CHO VIỆC LÀM ***
                ViecLamDangTuyen = jobs,
                TotalJobsCount = totalJobs,
                JobPageNumber = jobPage,
                JobTotalPages = (int)Math.Ceiling(totalJobs / (double)JobsPerPage),
                Slug = slug // Quan trọng: Truyền lại slug để tạo link phân trang
            };

            return View(viewModel);
        }

        // Hàm helper để định dạng mức lương (giữ nguyên)
        private static string FormatMucLuong(LoaiLuong loaiLuong, ulong? luongToiThieu, ulong? luongToiDa)
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
            string suffix = loaiLuong.GetDisplayName().ToLowerInvariant() switch {
                "theo giờ" => "/giờ", "theo ngày" => "/ngày", "theo ca" => "/ca",
                "theo tháng" => "/tháng", "theo dự án" => "/dự án", _ => ""
            };
            string formattedMin = luongToiThieu.HasValue ? $"{luongToiThieu.Value:N0}" : "";
            string formattedMax = luongToiDa.HasValue ? $"{luongToiDa.Value:N0}" : "";
            if (luongToiThieu.HasValue && luongToiDa.HasValue && luongToiThieu.Value != luongToiDa.Value)
                return $"{formattedMin} - {formattedMax} VNĐ{suffix}";
            if (luongToiThieu.HasValue) return $"{formattedMin} VNĐ{suffix}";
            if (luongToiDa.HasValue) return $"Đến {formattedMax} VNĐ{suffix}";
            return "Thương lượng";
        }
    }
}