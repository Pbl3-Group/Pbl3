using System.Collections.Generic;
using HeThongTimViec.Models;

namespace HeThongTimViec.ViewModels
{
    // Lớp chính chứa tất cả dữ liệu cho trang báo cáo
    public class BaoCaoThongKeViewModel
    {
        public string ActiveTab { get; set; } = "tongquan";

        // Dữ liệu cho Thẻ KPI ở Tab Tổng Quan
        public int TongSoCongViec { get; set; }
        public int TongSoNguoiDung { get; set; }
        public double TiLeTuyenDung { get; set; }
        public int BaoCaoViPhamMoi { get; set; }

        // === CÁC CHỈ SỐ NÂNG CAO MỚI ===
        // Tab Tổng quan
        public double ThoiGianTrungBinhTuyenDung { get; set; } // Số ngày trung bình để đóng một tin
        public int NguoiDungHoatDong30Ngay { get; set; } // Users có đăng nhập trong 30 ngày qua
        public int SoHoSoDoanhNghiepChoXacMinh { get; set; }
        
        // Tab Công việc
        public int SoTinTuyenDungBiAnDoViPham { get; set; }
        public KeyValuePair<string, int> NganhNgheHotNhat { get; set; }
        public List<TopEmployerByJobsViewModel> Top5NhaTuyenDungDangNhieuTinNhat { get; set; } = new();

        // Tab Ứng viên
        public double TiLeHoanThanhHoSo { get; set; } // % người dùng cá nhân có HoSoUngVien
        public double SoUngTuyenTrungBinhMoiUngVien { get; set; }
        public List<TopCandidateByApplicationsViewModel> Top5UngVienTichCucNhat { get; set; } = new();

        // Tab Nhà tuyển dụng
        public int SoNhaTuyenDungDaXacMinh { get; set; }
        public int SoNhaTuyenDungMoi30Ngay { get; set; }


        // Dữ liệu cho các biểu đồ
        public ChartJsData? JobTrendChart { get; set; } // Xu hướng công việc (Bán thời gian/Thời vụ)
        public ChartJsData? JobStatusChart { get; set; } // Phân loại công việc (Trạng thái)
        public ChartJsData? JobLocationChart { get; set; } // Phân bố địa lý
        public ChartJsData? CandidateTrendChart { get; set; } // Xu hướng ứng viên & người dùng
        public ChartJsData? JobTypeDistributionChart { get; set; } // Phân bố loại hình (Bán thời gian, Thời vụ,...)
        public ChartJsData? PopularIndustriesChart { get; set; } // Ngành nghề phổ biến
        public ChartJsData? PosterTypeChart { get; set; } // Phân loại người đăng (Cá nhân/Doanh nghiệp)
        public ChartJsData? NewJobsTrendChart { get; set; } // Xu hướng tin đăng mới
        public ChartJsData? CandidateAgeChart { get; set; } // Phân bố tuổi ứng viên
        public ChartJsData? CandidateExperienceChart { get; set; } // Kinh nghiệm ứng viên (từ text)
        public ChartJsData? EmployerSizeChart { get; set; } // Quy mô nhà tuyển dụng
        public ChartJsData? EmployerIndustryChart { get; set; } // Ngành nghề nhà tuyển dụng
        public ChartJsData? EmployerLocationChart { get; set; } // Phân bố địa lý nhà tuyển dụng

        public BaoCaoThongKeViewModel()
        {
            // Khởi tạo để tránh lỗi null
            JobTrendChart = new ChartJsData();
            JobStatusChart = new ChartJsData();
            JobLocationChart = new ChartJsData();
            CandidateTrendChart = new ChartJsData();
            JobTypeDistributionChart = new ChartJsData();
            PopularIndustriesChart = new ChartJsData();
            PosterTypeChart = new ChartJsData();
            NewJobsTrendChart = new ChartJsData();
            CandidateAgeChart = new ChartJsData();
            CandidateExperienceChart = new ChartJsData();
            EmployerSizeChart = new ChartJsData();
            EmployerIndustryChart = new ChartJsData();
            EmployerLocationChart = new ChartJsData();
        }
    }

     public class TopEmployerByJobsViewModel
    {
        public int EmployerId { get; set; }
        public string TenCongTy { get; set; } = string.Empty;
        public int SoLuongTinDang { get; set; }
    }

    public class TopCandidateByApplicationsViewModel
    {
        public int CandidateId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public int SoLuongUngTuyen { get; set; }
    }


    // Lớp cấu trúc dữ liệu cho Chart.js
    public class ChartJsData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartJsDataset> Datasets { get; set; } = new List<ChartJsDataset>();
    }

    public class ChartJsDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<int> Data { get; set; } = new List<int>();
        public List<string>? BackgroundColor { get; set; }
        public List<string>? BorderColor { get; set; }
        public double? Tension { get; set; } // For line charts
        public bool? Fill { get; set; }
    }
}