// File: Controllers/TimViecController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using HeThongTimViec.Extensions; // For EnumExtensions.GetDisplayName(), GetSelectList()
using System.Collections.Generic;
// using System.Linq; // Đã có ở trên, không cần lặp lại
using Microsoft.AspNetCore.Http;
using System;
using HeThongTimViec.Utils; // For SeoUrlHelper

namespace HeThongTimViec.Controllers
{
    [Route("[controller]")]
    public class TimViecController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimViecController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimViecController(ApplicationDbContext context, ILogger<TimViecController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper lấy User ID hiện tại
        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : (int?)null;
        }

        // Helper lấy loại tài khoản hiện tại
        private LoaiTaiKhoan? GetCurrentUserAccountType()
        {
            var accountTypeClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue("AccountType");
            if (!string.IsNullOrEmpty(accountTypeClaim) && Enum.TryParse<LoaiTaiKhoan>(accountTypeClaim, true, out var accountType))
            {
                return accountType;
            }
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role); // Fallback to Role
            if (!string.IsNullOrEmpty(roleClaim) && Enum.TryParse<LoaiTaiKhoan>(roleClaim, true, out var roleAsAccountType))
            {
                return roleAsAccountType;
            }
            return null;
        }

        // Helper định dạng chuỗi lương
       private static string FormatSalary(LoaiLuong loaiLuong, ulong? min, ulong? max)
{
    if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";
    string prefix = loaiLuong switch
    {
        LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca",
        LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => ""
    };
    string FormatValue(ulong val) => val.ToString("N0"); // Định dạng số có dấu phẩy ngăn cách hàng nghìn

    if (min.HasValue && max.HasValue && min > 0 && max > 0)
    {
        if (min == max) return $"{FormatValue(min.Value)} VNĐ {prefix}";
        return $"{FormatValue(min.Value)} - {FormatValue(max.Value)} VNĐ {prefix}";
    }
    if (min.HasValue && min > 0) return $"Từ {FormatValue(min.Value)} VNĐ {prefix}";
    if (max.HasValue && max > 0) return $"Đến {FormatValue(max.Value)} VNĐ {prefix}";

    try { return loaiLuong.GetDisplayName(); } // Fallback về tên hiển thị của Enum
    catch { return loaiLuong.ToString(); } // Fallback cuối cùng
}

        // Helper định dạng địa chỉ
        private string FormatAddress(string? chiTiet, string? quanHuyen, string? thanhPho)
        {
            var parts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(chiTiet)) parts.Add(chiTiet.Trim());
            if (!string.IsNullOrWhiteSpace(quanHuyen)) parts.Add(quanHuyen.Trim());
            if (!string.IsNullOrWhiteSpace(thanhPho)) parts.Add(thanhPho.Trim());
            return string.Join(", ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        // Helper tính điểm phù hợp
        private int CalculatePhuHopScore(TinTuyenDung tin, NguoiDung? ungVien)
        {
            if (ungVien == null || ungVien.LoaiTk != LoaiTaiKhoan.canhan) return 0;
            int score = 0;

            // 1. Địa điểm: +30 nếu cùng quận, +15 nếu cùng thành phố
            if (ungVien.DiaDiemMongMuons != null && ungVien.DiaDiemMongMuons.Any()) // Thêm kiểm tra Any()
            {
                if (ungVien.DiaDiemMongMuons.Any(dd => dd.QuanHuyenId == tin.QuanHuyenId)) score += 30;
                else if (ungVien.DiaDiemMongMuons.Any(dd => dd.ThanhPhoId == tin.ThanhPhoId)) score += 15;
            }

            // 2. Ngành nghề: +20 nếu Vị trí mong muốn của ứng viên chứa tên ngành nghề của tin
            if (tin.TinTuyenDungNganhNghes != null && tin.TinTuyenDungNganhNghes.Any() && ungVien.HoSoUngVien != null && !string.IsNullOrWhiteSpace(ungVien.HoSoUngVien.ViTriMongMuon))
            {
                var tuKhoaViTriMongMuon = ungVien.HoSoUngVien.ViTriMongMuon.ToLower().Split(new[] { ' ', ',', ';', '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var nnTinDb in tin.TinTuyenDungNganhNghes) // Sửa lại để tránh N+1 nếu NganhNghe chưa được load
                {
                    if (nnTinDb.NganhNghe != null) // Kiểm tra NganhNghe có null không
                    {
                        var nnTinTen = nnTinDb.NganhNghe.Ten.ToLower();
                        if (tuKhoaViTriMongMuon.Any(tuKhoa => nnTinTen.Contains(tuKhoa) || tuKhoa.Contains(nnTinTen)))
                        {
                            score += 20; break;
                        }
                    }
                }
            }

            // 3. Lịch rảnh: +25 nếu có ít nhất một buổi làm việc của tin khớp với lịch rảnh
            if (ungVien.LichRanhUngViens != null && ungVien.LichRanhUngViens.Any() && tin.LichLamViecCongViecs != null && tin.LichLamViecCongViecs.Any())
            {
                bool lichKhop = tin.LichLamViecCongViecs.Any(llvCongViec =>
                    ungVien.LichRanhUngViens.Any(lrUngVien =>
                        (llvCongViec.NgayTrongTuan == lrUngVien.NgayTrongTuan || llvCongViec.NgayTrongTuan == NgayTrongTuan.ngaylinhhoat || lrUngVien.NgayTrongTuan == NgayTrongTuan.ngaylinhhoat) &&
                        (llvCongViec.BuoiLamViec.HasValue && llvCongViec.BuoiLamViec == lrUngVien.BuoiLamViec || llvCongViec.BuoiLamViec == BuoiLamViec.linhhoat || lrUngVien.BuoiLamViec == BuoiLamViec.linhhoat || llvCongViec.BuoiLamViec == BuoiLamViec.cangay || lrUngVien.BuoiLamViec == BuoiLamViec.cangay)
                    )
                );
                if (lichKhop) score += 25;
            }

            // 4. Mức lương: +10 nếu lương min của tin >= lương mong muốn (cùng loại lương)
            // Hoặc +5 nếu lương max của tin >= lương mong muốn (khi lương min không khớp)
            if (ungVien.HoSoUngVien?.MucLuongMongMuon.HasValue == true &&
                ungVien.HoSoUngVien?.LoaiLuongMongMuon.HasValue == true &&
                tin.LoaiLuong == ungVien.HoSoUngVien.LoaiLuongMongMuon &&
                tin.LoaiLuong != LoaiLuong.thoathuan)
            {
                if (tin.LuongToiThieu.HasValue && tin.LuongToiThieu >= ungVien.HoSoUngVien.MucLuongMongMuon)
                    score += 10;
                else if (tin.LuongToiDa.HasValue && tin.LuongToiDa >= ungVien.HoSoUngVien.MucLuongMongMuon)
                    score += 5;
            }
            return Math.Min(100, score);
        }

        // Action chính hiển thị trang tìm việc và kết quả
        [HttpGet("")] // Mặc định cho /TimViec
        [HttpGet("trang-{page:int}")] // Route cho phân trang dạng /TimViec/trang-2
        public async Task<IActionResult> Index([FromQuery] TimViecViewModel searchModel, int page = 1)
        {
            _logger.LogInformation("Trang tìm việc được truy cập với bộ lọc: {@SearchModel}, Trang: {Page}", searchModel, page);
            int pageSize = 6; // Số tin mỗi trang
            page = Math.Max(1, page); // Đảm bảo page luôn >= 1
            int? currentUserId = GetCurrentUserId();
            LoaiTaiKhoan? currentUserAccountType = GetCurrentUserAccountType();

            NguoiDung? ungVienHienTai = null;
            if (currentUserId.HasValue && currentUserAccountType == LoaiTaiKhoan.canhan)
            {
                ungVienHienTai = await _context.NguoiDungs
                                    .Include(uv => uv.HoSoUngVien)
                                    .Include(uv => uv.DiaDiemMongMuons)
                                    .Include(uv => uv.LichRanhUngViens) // Đã include ở đây, không cần include lại khi lấy ungVienHienTai
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(uv => uv.Id == currentUserId.Value);
            }

            var query = _context.TinTuyenDungs
                .Where(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet &&
                             (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            // Áp dụng các bộ lọc SERVER-SIDE
            if (!string.IsNullOrWhiteSpace(searchModel.TuKhoa))
            {
                string keyword = searchModel.TuKhoa.Trim().ToLower(); // Chuyển keyword thành chữ thường một lần
                query = query.Where(t =>
                    (t.TieuDe != null && EF.Functions.Collate(t.TieuDe, "utf8mb4_unicode_ci").Contains(keyword)) ||
                    (t.MoTa != null && EF.Functions.Collate(t.MoTa, "utf8mb4_unicode_ci").Contains(keyword)) ||
                    (t.NguoiDang.HoSoDoanhNghiep != null && t.NguoiDang.HoSoDoanhNghiep.TenCongTy != null && EF.Functions.Collate(t.NguoiDang.HoSoDoanhNghiep.TenCongTy, "utf8mb4_unicode_ci").Contains(keyword)) ||
                    (t.NguoiDang.LoaiTk == LoaiTaiKhoan.canhan && t.NguoiDang.HoTen != null && EF.Functions.Collate(t.NguoiDang.HoTen, "utf8mb4_unicode_ci").Contains(keyword)) ||
                    t.TinTuyenDungNganhNghes.Any(tnn => tnn.NganhNghe.Ten != null && EF.Functions.Collate(tnn.NganhNghe.Ten, "utf8mb4_unicode_ci").Contains(keyword))
                );
            }
            if (searchModel.ThanhPhoId.HasValue && searchModel.ThanhPhoId > 0)
            {
                query = query.Where(t => t.ThanhPhoId == searchModel.ThanhPhoId.Value);
                if (searchModel.QuanHuyenId.HasValue && searchModel.QuanHuyenId > 0)
                {
                    query = query.Where(t => t.QuanHuyenId == searchModel.QuanHuyenId.Value);
                }
            }
            if (searchModel.NganhNgheIds != null && searchModel.NganhNgheIds.Any(id => id > 0))
            {
                var validNganhNgheIds = searchModel.NganhNgheIds.Where(id => id > 0).ToList();
                if (validNganhNgheIds.Any())
                {
                    query = query.Where(t => t.TinTuyenDungNganhNghes.Any(tnn => validNganhNgheIds.Contains(tnn.NganhNgheId)));
                }
            }
            if (searchModel.LoaiHinhCongViec.HasValue)
            {
                query = query.Where(t => t.LoaiHinhCongViec == searchModel.LoaiHinhCongViec.Value);
            }
           if (searchModel.LoaiLuong.HasValue && searchModel.LoaiLuong.Value != LoaiLuong.thoathuan)
            {
                query = query.Where(t => t.LoaiLuong == searchModel.LoaiLuong.Value);

                // BỔ SUNG LOGIC LỌC LƯƠNG MIN/MAX
                if (searchModel.LuongMinFilter.HasValue && searchModel.LuongMinFilter > 0 &&
                    searchModel.LuongMaxFilter.HasValue && searchModel.LuongMaxFilter > 0 &&
                    searchModel.LuongMinFilter.Value <= searchModel.LuongMaxFilter.Value)
                {
                    // Lọc theo khoảng lương (khi cả min và max được cung cấp và hợp lệ)
                    // Một công việc phù hợp nếu khoảng lương của nó giao với khoảng lương người dùng tìm kiếm.
                    // Điều kiện: (Công việc.Min <= NgườiDùng.Max) AND (Công việc.Max >= NgườiDùng.Min)
                    // Hoặc, nếu công việc chỉ có một mức lương cố định (Min == Max), thì lương đó phải nằm trong khoảng người dùng tìm.
                    query = query.Where(t =>
                        (t.LuongToiThieu.HasValue && t.LuongToiDa.HasValue && t.LuongToiThieu.Value <= searchModel.LuongMaxFilter.Value && t.LuongToiDa.Value >= searchModel.LuongMinFilter.Value) || // Khoảng lương của tin giao với khoảng lọc
                        (t.LuongToiThieu.HasValue && !t.LuongToiDa.HasValue && t.LuongToiThieu.Value >= searchModel.LuongMinFilter.Value && t.LuongToiThieu.Value <= searchModel.LuongMaxFilter.Value) || // Tin chỉ có lương min, và nó nằm trong khoảng lọc
                        (!t.LuongToiThieu.HasValue && t.LuongToiDa.HasValue && t.LuongToiDa.Value >= searchModel.LuongMinFilter.Value && t.LuongToiDa.Value <= searchModel.LuongMaxFilter.Value) // Tin chỉ có lương max, và nó nằm trong khoảng lọc
                    );
                }
                else if (searchModel.LuongMinFilter.HasValue && searchModel.LuongMinFilter > 0)
                {
                    // Chỉ lọc theo lương tối thiểu người dùng nhập
                    // Công việc phù hợp nếu Lương tối đa của nó >= Lương tối thiểu người dùng nhập
                    // Hoặc nếu Lương tối thiểu của nó >= Lương tối thiểu người dùng nhập (nếu không có lương tối đa)
                    query = query.Where(t =>
                        (t.LuongToiDa.HasValue && t.LuongToiDa.Value >= searchModel.LuongMinFilter.Value) ||
                        (!t.LuongToiDa.HasValue && t.LuongToiThieu.HasValue && t.LuongToiThieu.Value >= searchModel.LuongMinFilter.Value)
                    );
                }
                else if (searchModel.LuongMaxFilter.HasValue && searchModel.LuongMaxFilter > 0)
                {
                    // Chỉ lọc theo lương tối đa người dùng nhập
                    // Công việc phù hợp nếu Lương tối thiểu của nó <= Lương tối đa người dùng nhập
                    // Hoặc nếu Lương tối đa của nó <= Lương tối đa người dùng nhập (nếu không có lương tối thiểu)
                    query = query.Where(t =>
                        (t.LuongToiThieu.HasValue && t.LuongToiThieu.Value <= searchModel.LuongMaxFilter.Value) ||
                        (!t.LuongToiThieu.HasValue && t.LuongToiDa.HasValue && t.LuongToiDa.Value <= searchModel.LuongMaxFilter.Value)
                    );
                }
            }
            else if (searchModel.LoaiLuong.HasValue && searchModel.LoaiLuong.Value == LoaiLuong.thoathuan)
            {
                query = query.Where(t => t.LoaiLuong == LoaiLuong.thoathuan);
            }
            if (searchModel.CaLamViecFilter != null && searchModel.CaLamViecFilter.Any())
            {
                query = query.Where(t => t.LichLamViecCongViecs.Any(l => l.BuoiLamViec.HasValue && searchModel.CaLamViecFilter.Contains(l.BuoiLamViec.Value)));
            }
            if (searchModel.TinGap.HasValue && searchModel.TinGap.Value)
            {
                query = query.Where(t => t.TinGap == true);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.KinhNghiemFilter))
            {
                string knKeyword = searchModel.KinhNghiemFilter.Trim().ToLower();
                query = query.Where(t => t.YeuCauKinhNghiemText != null && EF.Functions.Collate(t.YeuCauKinhNghiemText, "utf8mb4_unicode_ci").Contains(knKeyword));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.HocVanFilter))
            {
                string hvKeyword = searchModel.HocVanFilter.Trim().ToLower();
                query = query.Where(t => t.YeuCauHocVanText != null && EF.Functions.Collate(t.YeuCauHocVanText, "utf8mb4_unicode_ci").Contains(hvKeyword));
            }

            if (searchModel.ChiHienThiViecLamThoiVu) { query = query.Where(t => t.LoaiHinhCongViec == LoaiHinhCongViec.thoivu); }
            if (searchModel.ChiHienThiViecLamDaLuu && currentUserId.HasValue) { query = query.Where(t => _context.TinDaLuus.Any(tdl => tdl.TinTuyenDungId == t.Id && tdl.NguoiDungId == currentUserId.Value)); }
            if (searchModel.ChiHienThiViecLamDaUngTuyen && currentUserId.HasValue) { query = query.Where(t => _context.UngTuyens.Any(ut => ut.TinTuyenDungId == t.Id && ut.UngVienId == currentUserId.Value)); }

            // Sắp xếp kết quả (TRƯỚC KHI LỌC LỊCH RẢNH Ở CLIENT)
            // Điều này quan trọng vì thứ tự có thể ảnh hưởng đến những item nào được tải về client nếu số lượng lớn
            switch (searchModel.SapXep)
            {
                case "luongcao": query = query.OrderByDescending(t => t.LoaiLuong == LoaiLuong.thoathuan ? 0 : (t.LuongToiDa ?? t.LuongToiThieu ?? 0)).ThenByDescending(t => t.TinGap).ThenByDescending(t => t.NgayDang); break;
                case "luongthap": query = query.OrderBy(t => t.LoaiLuong == LoaiLuong.thoathuan ? ulong.MaxValue : (t.LuongToiThieu ?? t.LuongToiDa ?? ulong.MaxValue)).ThenByDescending(t => t.TinGap).ThenByDescending(t => t.NgayDang); break;
                case "hannopgan": query = query.OrderBy(t => t.NgayHetHan.HasValue ? 0 : 1).ThenBy(t => t.NgayHetHan).ThenByDescending(t => t.TinGap).ThenByDescending(t => t.NgayDang); break;
                case "hannopxa": query = query.OrderBy(t => t.NgayHetHan.HasValue ? 0 : 1).ThenByDescending(t => t.NgayHetHan).ThenByDescending(t => t.TinGap).ThenByDescending(t => t.NgayDang); break;
                default: searchModel.SapXep = "ngaymoi";  query = query.OrderByDescending(t => t.NgayDang); break;
            }
            
            // CHUẨN BỊ CHO LỌC LỊCH RẢNH Ở CLIENT VÀ PHÂN TRANG
            List<TinTuyenDung> tinTuyenDungDaLocHoanChinh;
            int totalCount;

            if (searchModel.ChiHienThiViecLamPhuHopLichRanh)
            {
                if (ungVienHienTai != null && ungVienHienTai.LichRanhUngViens.Any())
                {
                    _logger.LogInformation("Thực hiện lọc lịch rảnh ở phía client.");
                    // Tải các tin đã qua bộ lọc server-side CÙNG VỚI LỊCH LÀM VIỆC của chúng
                    var tinTuyenDungTruocKhiLocLichClient = await query
                        .Include(t => t.LichLamViecCongViecs) // QUAN TRỌNG
                        .AsNoTracking()
                        .ToListAsync(); // Tải về client

                    var lichRanhCuaUngVien = ungVienHienTai.LichRanhUngViens.ToList();

                    tinTuyenDungDaLocHoanChinh = tinTuyenDungTruocKhiLocLichClient
                        .Where(t => // Thực hiện lọc .Where() này ở client (LINQ to Objects)
                            t.LichLamViecCongViecs.Any(llvCongViec =>
                                lichRanhCuaUngVien.Any(lrUngVien =>
                                    (llvCongViec.NgayTrongTuan == lrUngVien.NgayTrongTuan || llvCongViec.NgayTrongTuan == NgayTrongTuan.ngaylinhhoat || lrUngVien.NgayTrongTuan == NgayTrongTuan.ngaylinhhoat) &&
                                    (llvCongViec.BuoiLamViec.HasValue && llvCongViec.BuoiLamViec == lrUngVien.BuoiLamViec || llvCongViec.BuoiLamViec == BuoiLamViec.linhhoat || lrUngVien.BuoiLamViec == BuoiLamViec.linhhoat || llvCongViec.BuoiLamViec == BuoiLamViec.cangay || lrUngVien.BuoiLamViec == BuoiLamViec.cangay)
                                )
                            )
                        )
                        .ToList();
                    totalCount = tinTuyenDungDaLocHoanChinh.Count;
                }
                else
                {
                    _logger.LogInformation("Người dùng chọn lọc theo lịch rảnh nhưng không có thông tin lịch rảnh hợp lệ của ứng viên.");
                    tinTuyenDungDaLocHoanChinh = new List<TinTuyenDung>();
                    totalCount = 0;
                }
            }
            else // Không lọc lịch rảnh
            {
                totalCount = await query.CountAsync(); // Đếm trên server
                tinTuyenDungDaLocHoanChinh = await query
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .Include(t => t.LichLamViecCongViecs) // Include để CalculatePhuHopScore có dữ liệu
                                                .AsNoTracking()
                                                .ToListAsync(); // Phân trang trên server
            }

            // Nếu có lọc lịch rảnh ở client, thì phân trang lại trên danh sách đã lọc đó
            if (searchModel.ChiHienThiViecLamPhuHopLichRanh)
            {
                tinTuyenDungDaLocHoanChinh = tinTuyenDungDaLocHoanChinh
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToList();
            }

            // Lấy ID các tin đã lưu và đã ứng tuyển (chỉ đọc) - NÊN LÀM SAU KHI CÓ DANH SÁCH TIN HIỂN THỊ CUỐI CÙNG ĐỂ GIẢM TRUY VẤN
            // Tuy nhiên, để đơn giản, tạm giữ ở đây. Nếu có nhiều tin, có thể tối ưu hơn.
            HashSet<int> displayedJobIds = new HashSet<int>(tinTuyenDungDaLocHoanChinh.Select(t => t.Id));
            HashSet<int> savedJobIds = new HashSet<int>();
            HashSet<int> appliedJobIds = new HashSet<int>();

            if (currentUserId.HasValue && displayedJobIds.Any())
            {
                savedJobIds = new HashSet<int>(
                    await _context.TinDaLuus
                        .AsNoTracking()
                        .Where(tdl => tdl.NguoiDungId == currentUserId.Value && displayedJobIds.Contains(tdl.TinTuyenDungId))
                        .Select(tdl => tdl.TinTuyenDungId)
                        .ToListAsync()
                );
                appliedJobIds = new HashSet<int>(
                    await _context.UngTuyens
                        .AsNoTracking()
                        .Where(ut => ut.UngVienId == currentUserId.Value && displayedJobIds.Contains(ut.TinTuyenDungId))
                        .Select(ut => ut.TinTuyenDungId)
                        .ToListAsync()
                );
            }
            
            var resultViewModels = new List<KetQuaTimViecItemViewModel>();
            // Include lại các navigation properties cần thiết cho việc tạo ViewModel,
            // vì query ban đầu có thể chưa Include hết hoặc chúng ta đang làm việc với danh sách từ client.
            // Cách tốt hơn là đảm bảo tinTuyenDungDaLocHoanChinh đã có đủ thông tin.
            // Đoạn này giả định các Include cần thiết đã được thực hiện trước đó khi tạo 'query' hoặc khi tải 'tinTuyenDungTruocKhiLocLichClient'.
            
            // Để đảm bảo, chúng ta sẽ query lại các entities này với đầy đủ include nếu cần,
            // nhưng chỉ cho các IDs trong tinTuyenDungDaLocHoanChinh.
            // HOẶC: Đảm bảo các Include đã đủ từ đầu.
            // Cách hiện tại là đã Include đủ từ đầu.

            // Load NguoiDang và các thông tin liên quan cho các tin sẽ hiển thị
            // Điều này cần thiết vì AsNoTracking() và xử lý client có thể làm mất các navigation property
            var tinIdsToLoadDetails = tinTuyenDungDaLocHoanChinh.Select(t => t.Id).ToList();
            if (tinIdsToLoadDetails.Any())
            {
                var tinTuyenDungDayDuChiTiet = await _context.TinTuyenDungs
                    .Where(t => tinIdsToLoadDetails.Contains(t.Id))
                    .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                    .Include(t => t.ThanhPho)
                    .Include(t => t.QuanHuyen)
                    .Include(t => t.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                    // .Include(t => t.LichLamViecCongViecs) // Đã có nếu từ client, hoặc include lại nếu từ server
                    .AsNoTracking()
                    .ToListAsync();

                // Tạo một dictionary để dễ dàng map lại
                var tinDict = tinTuyenDungDayDuChiTiet.ToDictionary(t => t.Id);
                
                // Cập nhật lại danh sách tinTuyenDungDaLocHoanChinh với đầy đủ chi tiết
                // (chủ yếu để đảm bảo các navigation property được load cho CalculatePhuHopScore và tạo ViewModel)
                // Cách này có thể không cần thiết nếu các Includes ban đầu đã đủ và không bị mất.
                // Chúng ta sẽ dùng tinTuyenDungDayDuChiTiet để tạo ViewModel
                 tinTuyenDungDaLocHoanChinh = tinTuyenDungDayDuChiTiet
                                                .OrderBy(t => tinIdsToLoadDetails.IndexOf(t.Id)) // Giữ đúng thứ tự sau khi phân trang
                                                .ToList();
            }


            foreach (var t in tinTuyenDungDaLocHoanChinh)
            {
                resultViewModels.Add(new KetQuaTimViecItemViewModel
                {
                    Id = t.Id,
                    TieuDe = t.TieuDe,
                    NguoiDangId = t.NguoiDangId,
                    TenCongTyHoacNguoiDang = t.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? (t.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? t.NguoiDang.HoTen) : t.NguoiDang?.HoTen ?? "N/A",
                    LogoHoacAvatarUrl = t.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? t.NguoiDang.HoSoDoanhNghiep?.UrlLogo : t.NguoiDang?.UrlAvatar,
                    LoaiTaiKhoanNguoiDang = t.NguoiDang?.LoaiTk ?? default,
                    DiaDiem = FormatAddress(null, t.QuanHuyen?.Ten, t.ThanhPho?.Ten),
                    LoaiHinhCongViecDisplay = t.LoaiHinhCongViec.GetDisplayName(),
                    MucLuongDisplay = FormatSalary(t.LoaiLuong, t.LuongToiThieu, t.LuongToiDa),
                    NgayDang = t.NgayDang,
                    NgayHetHan = t.NgayHetHan,
                    TinGap = t.TinGap,
                    NganhNgheNho = t.TinTuyenDungNganhNghes?.Select(nn => nn.NganhNghe?.Ten ?? "").Where(s => !string.IsNullOrEmpty(s)).Take(2).ToList() ?? new List<string>(),
                    YeuCauKinhNghiemText = t.YeuCauKinhNghiemText,
                    YeuCauHocVanText = t.YeuCauHocVanText,
                    DaLuu = savedJobIds.Contains(t.Id),
                    DaUngTuyen = appliedJobIds.Contains(t.Id),
                    PhuHopScore = CalculatePhuHopScore(t, ungVienHienTai)
                });
            }

            searchModel.KetQua = new PaginatedList<KetQuaTimViecItemViewModel>(resultViewModels, totalCount, page, pageSize);
            await PopulateFilterOptions(searchModel);
            ViewBag.CurrentUserAccountType = currentUserAccountType;

            return View(searchModel);
        }

        // Chuẩn bị dữ liệu cho các dropdown/checkbox trên form lọc
        private async Task PopulateFilterOptions(TimViecViewModel model)
        {
            model.ThanhPhoOptions = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", model.ThanhPhoId);
            if (model.ThanhPhoId.HasValue && model.ThanhPhoId > 0)
            {
                model.QuanHuyenOptions = new SelectList(await _context.QuanHuyens.Where(q => q.ThanhPhoId == model.ThanhPhoId.Value).AsNoTracking().OrderBy(q => q.Ten).ToListAsync(), "Id", "Ten", model.QuanHuyenId);
            }
            else
            {
                model.QuanHuyenOptions = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
            }

            var loaiHinhItems = EnumExtensions.GetSelectList<LoaiHinhCongViec>(includeDefaultItem: true, defaultItemText: "-- Tất cả loại hình --", defaultItemValue: "");
            model.LoaiHinhCongViecOptions = new SelectList(loaiHinhItems, "Value", "Text", model.LoaiHinhCongViec?.ToString());

            var loaiLuongItems = EnumExtensions.GetSelectList<LoaiLuong>(includeDefaultItem: true, defaultItemText: "-- Tất cả loại lương --", defaultItemValue: "");
            model.LoaiLuongOptions = new SelectList(loaiLuongItems, "Value", "Text", model.LoaiLuong?.ToString());

            var allNganhNghe = await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten).ToListAsync();
            model.NganhNgheOptions = allNganhNghe.Select(n => new SelectListItem
            {
                Value = n.Id.ToString(), Text = n.Ten,
                Selected = model.NganhNgheIds != null && model.NganhNgheIds.Contains(n.Id)
            }).ToList();

            var caLamViecItems = EnumExtensions.GetSelectList<BuoiLamViec>(includeDefaultItem: false);
            model.CaLamViecOptions = caLamViecItems.Select(sli => new SelectListItem
            {
                Value = sli.Value, Text = sli.Text,
                Selected = model.CaLamViecFilter != null && model.CaLamViecFilter.Contains(Enum.Parse<BuoiLamViec>(sli.Value))
            }).ToList();
        }
[HttpGet("chi-tiet/{id:int}/{tieuDeSeo?}")]
public async Task<IActionResult> ChiTiet(int id, string? tieuDeSeo)
{
    _logger.LogInformation("Người dùng truy cập trang chi tiết tin ID: {TinTuyenDungId}", id);
    int? currentUserId = GetCurrentUserId();

    // Bước 1: Truy vấn cơ sở dữ liệu để lấy tin tuyển dụng cùng các thông tin liên quan
    // Sử dụng Include/ThenInclude để tránh lỗi N+1, tải tất cả trong 1 lần gọi
    var tinTuyenDung = await _context.TinTuyenDungs
        .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
        .Include(t => t.NguoiDang).ThenInclude(nd => nd.ThanhPho)
        .Include(t => t.NguoiDang).ThenInclude(nd => nd.QuanHuyen)
        .Include(t => t.ThanhPho)
        .Include(t => t.QuanHuyen)
        .Include(t => t.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
        .Include(t => t.LichLamViecCongViecs)
        .AsNoTracking() // Dùng AsNoTracking() để tăng hiệu suất vì đây là thao tác đọc
        .FirstOrDefaultAsync(t => t.Id == id && 
                                  t.TrangThai == TrangThaiTinTuyenDung.daduyet && 
                                  (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

    // Bước 2: Xử lý trường hợp không tìm thấy tin tuyển dụng
    if (tinTuyenDung == null)
    {
        _logger.LogWarning("Không tìm thấy tin ID: {TinTuyenDungId}, hoặc tin chưa được duyệt/đã hết hạn.", id);
        // Sử dụng TempData để hiển thị thông báo sau khi chuyển hướng, không cần view riêng
        TempData["ErrorMessage"] = "Tin tuyển dụng bạn tìm kiếm không tồn tại hoặc đã hết hạn.";
        return RedirectToAction("Index", "TimViec");
    }

    // Bước 3: Kiểm tra và chuyển hướng nếu URL SEO không đúng (tốt cho SEO)
    var expectedSeoTitle = SeoUrlHelper.GenerateSlug(tinTuyenDung.TieuDe);
    if (string.IsNullOrWhiteSpace(tieuDeSeo) || !tieuDeSeo.Equals(expectedSeoTitle, StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogInformation("Chuyển hướng về URL chuẩn cho Tin ID {TinTuyenDungId}. SEO mong đợi: '{ExpectedSeo}', SEO được cung cấp: '{ProvidedSeo}'", id, expectedSeoTitle, tieuDeSeo);
        return RedirectToActionPermanent("ChiTiet", new { id = id, tieuDeSeo = expectedSeoTitle });
    }

    // Bước 4: Kiểm tra các trạng thái liên quan đến người dùng hiện tại (nếu đã đăng nhập)
    bool daLuu = false; 
    bool daUngTuyen = false;
    bool daBaoCao = false;
    bool isCurrentUserThePoster = false;

    if (currentUserId.HasValue)
    {
        // Kiểm tra xem người dùng hiện tại có phải là người đăng tin không
        isCurrentUserThePoster = tinTuyenDung.NguoiDangId == currentUserId.Value;

        // Kiểm tra các trạng thái khác
        daLuu = await _context.TinDaLuus.AnyAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == id);
        daUngTuyen = await _context.UngTuyens.AnyAsync(ut => ut.UngVienId == currentUserId.Value && ut.TinTuyenDungId == id);
        daBaoCao = await _context.BaoCaoViPhams.AnyAsync(bc => bc.NguoiBaoCaoId == currentUserId.Value && bc.TinTuyenDungId == id);
    }

    // Bước 5: Chuẩn bị dữ liệu phức tạp hơn cho ViewModel (ví dụ: địa chỉ, lương)
    string diaChiLamViecDayDu = FormatAddress(tinTuyenDung.DiaChiLamViec, tinTuyenDung.QuanHuyen?.Ten, tinTuyenDung.ThanhPho?.Ten);
    string diaChiNguoiDang = tinTuyenDung.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep
        ? (tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.DiaChiDangKy ?? FormatAddress(tinTuyenDung.NguoiDang.DiaChiChiTiet, tinTuyenDung.NguoiDang.QuanHuyen?.Ten, tinTuyenDung.NguoiDang.ThanhPho?.Ten))
        : FormatAddress(tinTuyenDung.NguoiDang?.DiaChiChiTiet, tinTuyenDung.NguoiDang?.QuanHuyen?.Ten, tinTuyenDung.NguoiDang?.ThanhPho?.Ten);

    // Bước 6: Ánh xạ từ Model (tinTuyenDung) sang ViewModel (ChiTietTinTuyenDungViewModel)
    var viewModel = new ChiTietTinTuyenDungViewModel
    {
        Id = tinTuyenDung.Id, 
        TieuDe = tinTuyenDung.TieuDe, 
        MoTa = tinTuyenDung.MoTa, 
        YeuCau = tinTuyenDung.YeuCau, 
        QuyenLoi = tinTuyenDung.QuyenLoi,
        LoaiHinhCongViecDisplay = tinTuyenDung.LoaiHinhCongViec.GetDisplayName(), 
        LoaiLuongDisplay = tinTuyenDung.LoaiLuong.GetDisplayName(),
        MucLuongDisplay = FormatSalary(tinTuyenDung.LoaiLuong, tinTuyenDung.LuongToiThieu, tinTuyenDung.LuongToiDa),
        DiaChiLamViecDayDu = diaChiLamViecDayDu, 
        YeuCauKinhNghiemText = tinTuyenDung.YeuCauKinhNghiemText, 
        YeuCauHocVanText = tinTuyenDung.YeuCauHocVanText,
        SoLuongTuyen = tinTuyenDung.SoLuongTuyen, 
        TinGap = tinTuyenDung.TinGap, 
        NgayDang = tinTuyenDung.NgayDang, 
        NgayHetHan = tinTuyenDung.NgayHetHan,
        
        // Gán các giá trị đã kiểm tra ở Bước 4
        DaLuu = daLuu, 
        DaUngTuyen = daUngTuyen,
        DaBaoCao = daBaoCao,
        IsCurrentUserThePoster = isCurrentUserThePoster,

        // Thông tin người đăng/công ty
        NguoiDangId = tinTuyenDung.NguoiDangId, 
        LoaiTaiKhoanNguoiDang = tinTuyenDung.NguoiDang?.LoaiTk ?? default,
        TenNguoiDangHoacCongTy = tinTuyenDung.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep 
                                ? (tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? tinTuyenDung.NguoiDang.HoTen ?? "Không rõ") 
                                : (tinTuyenDung.NguoiDang?.HoTen ?? "Không rõ"),
        LogoHoacAvatarUrl = tinTuyenDung.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep 
                            ? tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo 
                            : tinTuyenDung.NguoiDang?.UrlAvatar,
        UrlWebsiteCongTy = tinTuyenDung.NguoiDang?.HoSoDoanhNghiep?.UrlWebsite, 
        MoTaCongTy = tinTuyenDung.NguoiDang?.HoSoDoanhNghiep?.MoTa,
        CongTyDaXacMinh = tinTuyenDung.NguoiDang?.HoSoDoanhNghiep?.DaXacMinh ?? false,
        EmailLienHe = tinTuyenDung.NguoiDang?.Email, 
        SdtLienHe = tinTuyenDung.NguoiDang?.Sdt, 
        DiaChiLienHeNguoiDang = diaChiNguoiDang,
        
        // Chuyển đổi danh sách các đối tượng sang danh sách các chuỗi/ViewModel đơn giản hơn
        NganhNghes = tinTuyenDung.TinTuyenDungNganhNghes?.Select(tnn => tnn.NganhNghe?.Ten ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
        LichLamViecs = tinTuyenDung.LichLamViecCongViecs?.Select(l => new LichLamViecViewModel
        {
           NgayTrongTuanDisplay = l.NgayTrongTuan.GetDisplayName(),
            ThoiGianDisplay = (l.GioBatDau.HasValue && l.GioKetThuc.HasValue)
                                ? $"{l.GioBatDau.Value:hh\\:mm} - {l.GioKetThuc.Value:hh\\:mm}"
                                : (l.BuoiLamViec.HasValue
                                    ? l.BuoiLamViec.Value.GetDisplayName()
                                    : "Linh hoạt")
        }).ToList() ?? new List<LichLamViecViewModel>()
    };

    // Bước 7: Trả về View cùng với ViewModel đã được điền đầy đủ thông tin
    return View(viewModel);
}

        // Action LuuTin, BoLuuTin giữ nguyên

        [HttpPost("GuiBaoCao")] // Route: /TimViec/GuiBaoCao
        [Authorize] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiBaoCao([FromForm] int tinTuyenDungId, [FromForm] LyDoBaoCao lyDo, [FromForm] string? chiTiet)
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để báo cáo." });
            }

            if (tinTuyenDungId <= 0)
            {
                return BadRequest(new { success = false, message = "ID tin tuyển dụng không hợp lệ." });
            }
            
            if (!Enum.IsDefined(typeof(LyDoBaoCao), lyDo))
            {
                 ModelState.AddModelError(nameof(lyDo), "Lý do báo cáo không hợp lệ.");
            }

            if (lyDo == LyDoBaoCao.khac && string.IsNullOrWhiteSpace(chiTiet))
            {
                ModelState.AddModelError(nameof(chiTiet), "Vui lòng cung cấp chi tiết cho lý do 'Khác'.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
                _logger.LogWarning("GuiBaoCao: Dữ liệu không hợp lệ cho TinID {TinID}. Lỗi: {Errors}", tinTuyenDungId, string.Join("; ", errors));
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", errors = errors });
            }

            var tinExists = await _context.TinTuyenDungs.AnyAsync(t => t.Id == tinTuyenDungId && t.TrangThai == TrangThaiTinTuyenDung.daduyet);
            if (!tinExists)
            {
                _logger.LogWarning("GuiBaoCao: Tin tuyển dụng ID {TinID} không tồn tại hoặc chưa được duyệt.", tinTuyenDungId);
                return NotFound(new { success = false, message = "Tin tuyển dụng này không tồn tại hoặc đã bị gỡ." });
            }

            bool daBaoCaoTruocDoVoiCungLyDo = await _context.BaoCaoViPhams.AnyAsync(bc => 
                bc.TinTuyenDungId == tinTuyenDungId && 
                bc.NguoiBaoCaoId == currentUserId.Value && 
                bc.LyDo == lyDo);

            if (daBaoCaoTruocDoVoiCungLyDo)
            {
                 _logger.LogInformation("Người dùng {UserId} đã thử báo cáo lại tin {TinId} với cùng lý do {LyDo}", currentUserId.Value, tinTuyenDungId, lyDo);
                return Ok(new { success = true, alreadyReported = true, message = "Bạn đã báo cáo tin này với lý do tương tự trước đó. Chúng tôi đang xem xét." });
            }

            var baoCao = new BaoCaoViPham
            {
                TinTuyenDungId = tinTuyenDungId,
                NguoiBaoCaoId = currentUserId.Value,
                LyDo = lyDo,
                ChiTiet = chiTiet, 
                TrangThaiXuLy = TrangThaiXuLyBaoCao.moi,
                NgayBaoCao = DateTime.UtcNow
            };

            _context.BaoCaoViPhams.Add(baoCao);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Người dùng {UserId} đã báo cáo thành công tin {TinId} với lý do {LyDo}. Chi tiết: {ChiTiet}", currentUserId.Value, tinTuyenDungId, lyDo, chiTiet);
                return Ok(new { success = true, message = "Báo cáo của bạn đã được gửi thành công. Cảm ơn bạn!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu báo cáo cho tin {TinId} bởi người dùng {UserId}", tinTuyenDungId, currentUserId.Value);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Đã xảy ra lỗi phía máy chủ khi gửi báo cáo. Vui lòng thử lại." });
            }
        }

        [HttpPost("LuuTin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuTin([FromBody] int tinTuyenDungId)
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thực hiện." });
            if (tinTuyenDungId <= 0) return BadRequest(new { success = false, message = "ID tin tuyển dụng không hợp lệ." });

            var tinExists = await _context.TinTuyenDungs.AnyAsync(t => t.Id == tinTuyenDungId && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));
            if (!tinExists) return NotFound(new { success = false, message = "Tin tuyển dụng này không còn tồn tại hoặc đã hết hạn." });

            if (await _context.TinDaLuus.AnyAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == tinTuyenDungId))
            {
                return Ok(new { success = true, message = "Tin này đã được bạn lưu trước đó.", alreadySaved = true });
            }

            _context.TinDaLuus.Add(new TinDaLuu { NguoiDungId = currentUserId.Value, TinTuyenDungId = tinTuyenDungId, NgayLuu = DateTime.UtcNow });
            try { await _context.SaveChangesAsync(); return Ok(new { success = true, message = "Đã lưu tin thành công!" }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu tin ID {TinID} cho User ID {UserID}", tinTuyenDungId, currentUserId.Value);
                return StatusCode(500, new { success = false, message = "Không thể lưu tin lúc này. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("BoLuuTin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoLuuTin([FromBody] int tinTuyenDungId)
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thực hiện." });
            if (tinTuyenDungId <= 0) return BadRequest(new { success = false, message = "ID tin tuyển dụng không hợp lệ." });

            var tinDaLuu = await _context.TinDaLuus.FirstOrDefaultAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == tinTuyenDungId);
            if (tinDaLuu == null)
            {
                return Ok(new { success = true, message = "Tin này chưa được lưu hoặc đã bỏ lưu.", notFoundOrUnsaved = true });
            }
            _context.TinDaLuus.Remove(tinDaLuu);
            try { await _context.SaveChangesAsync(); return Ok(new { success = true, message = "Đã bỏ lưu tin thành công." }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bỏ lưu tin ID {TinID} cho User ID {UserID}", tinTuyenDungId, currentUserId.Value);
                return StatusCode(500, new { success = false, message = "Không thể bỏ lưu tin lúc này. Vui lòng thử lại sau." });
            }
        }
    }
}