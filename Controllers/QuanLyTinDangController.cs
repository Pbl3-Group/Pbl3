using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.QuanLyTinDang;
using HeThongTimViec.ViewModels.JobPosting;
using HeThongTimViec.ViewModels.TimViec; // For PaginatedList
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using HeThongTimViec.Services; // <<<<<<< THÊM USING CHO SERVICE THÔNG BÁO
using System.Text.Json;      // <<<<<<< THÊM USING CHO JSON SERIALIZER
using HeThongTimViec.Utils;  // <<<<<<< THÊM USING CHO NOTIFICATION CONSTANTS
using System.Globalization;

namespace HeThongTimViec.Controllers
{
    [Route("admin/QuanLyTinDang")]
    [Authorize(Roles = nameof(LoaiTaiKhoan.quantrivien))]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class QuanLyTinDangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IThongBaoService _thongBaoService; // <<<<<<< INJECT SERVICE THÔNG BÁO
        private readonly ILogger<QuanLyTinDangController> _logger; // <<<<<<< Thêm Logger

        public QuanLyTinDangController(ApplicationDbContext context, IThongBaoService thongBaoService, ILogger<QuanLyTinDangController> logger)
        {
            _context = context;
            _thongBaoService = thongBaoService; // <<<<<<< GÁN SERVICE
            _logger = logger;                  // <<<<<<< GÁN LOGGER
        }

        [HttpGet] // Mặc định cho route "admin/QuanLyTinDang"
        public async Task<IActionResult> Index(
            string? currentTab = "tatca",
            string? keyword = null,
            string viewMode = "list",
            string sortBy = "moinhat",
            bool filterTinGapCheckbox = false,
            int page = 1,
            TrangThaiTinTuyenDung? filterTrangThai = null,
            LoaiHinhCongViec? filterLoaiCongViec = null,
            LoaiTaiKhoan? filterLoaiNguoiDang = null,
            int? filterThanhPhoId = null,
            int? filterQuanHuyenId = null,
            [FromQuery] List<int>? filterNganhNgheIds = null,
            ulong? filterLuongMin = null,
            ulong? filterLuongMax = null)
        {
            var viewModel = new QuanLyTinDangIndexViewModel
            {
                CurrentTab = currentTab,
                Keyword = keyword,
                ViewMode = viewMode,
                SortBy = sortBy,
                FilterTrangThai = filterTrangThai,
                FilterLoaiCongViec = filterLoaiCongViec,
                FilterLoaiNguoiDang = filterLoaiNguoiDang,
                FilterThanhPhoId = filterThanhPhoId,
                FilterQuanHuyenId = filterQuanHuyenId,
                FilterNganhNgheIds = filterNganhNgheIds ?? new List<int>(),
                FilterLuongMin = filterLuongMin,
                FilterLuongMax = filterLuongMax,
                FilterTinGapCheckbox = filterTinGapCheckbox
            };

            bool filterTinGapQueryProvided = HttpContext.Request.Query.ContainsKey("filterTinGap");
            bool? explicitFilterTinGap = null;
            if (filterTinGapQueryProvided)
            {
                if (bool.TryParse(HttpContext.Request.Query["filterTinGap"], out bool ftgValue))
                {
                    explicitFilterTinGap = ftgValue;
                }
            }

            if (HttpContext.Request.Query.ContainsKey(nameof(filterTinGapCheckbox).ToLower()) || HttpContext.Request.Query.ContainsKey(nameof(filterTinGapCheckbox)))
            {
                if (filterTinGapCheckbox) viewModel.FilterTinGap = true;
                else viewModel.FilterTinGap = null;
            }
            else if (explicitFilterTinGap.HasValue)
            {
                viewModel.FilterTinGap = explicitFilterTinGap;
                viewModel.FilterTinGapCheckbox = explicitFilterTinGap.Value;
            }
            else viewModel.FilterTinGap = null;

            await PopulateFilterOptions(viewModel);

            IQueryable<TinTuyenDung> query = _context.TinTuyenDungs
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.ThanhPho)
                .Include(t => t.QuanHuyen)
                .Include(t => t.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .AsNoTracking();

            viewModel.TongSoTin = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa);
            viewModel.SoTinDangHoatDong = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.UtcNow));
            viewModel.SoTinChoDuyet = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.choduyet);
            viewModel.SoTinDaHetHan = await _context.TinTuyenDungs.CountAsync(t => t.TrangThai == TrangThaiTinTuyenDung.hethan || (t.TrangThai == TrangThaiTinTuyenDung.daduyet && t.NgayHetHan < DateTime.UtcNow));

            switch (currentTab?.ToLower())
            {
                case "choduyet": query = query.Where(t => t.TrangThai == TrangThaiTinTuyenDung.choduyet); break;
                case "danghoatdong": query = query.Where(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (t.NgayHetHan == null || t.NgayHetHan >= DateTime.UtcNow)); break;
                case "daan": query = query.Where(t => t.TrangThai == TrangThaiTinTuyenDung.taman); break;
                case "dahethan": query = query.Where(t => t.TrangThai == TrangThaiTinTuyenDung.hethan || (t.TrangThai == TrangThaiTinTuyenDung.daduyet && t.NgayHetHan < DateTime.UtcNow)); break;
                case "phantic": viewModel.TinMoiTheoThoiGian = await GetTinMoiTheoThoiGianData(query.Where(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa)); return View(viewModel);
                case "tatca": default: query = query.Where(t => t.TrangThai != TrangThaiTinTuyenDung.daxoa); break;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchTerm = keyword.ToLower().Trim();
                query = query.Where(t => t.TieuDe.ToLower().Contains(searchTerm) ||
                                         (t.NguoiDang.HoSoDoanhNghiep != null && t.NguoiDang.HoSoDoanhNghiep.TenCongTy.ToLower().Contains(searchTerm)) ||
                                         (t.NguoiDang.LoaiTk == LoaiTaiKhoan.canhan && t.NguoiDang.HoTen.ToLower().Contains(searchTerm)));
            }

            if (filterTrangThai.HasValue) query = query.Where(t => t.TrangThai == filterTrangThai.Value);
            if (filterLoaiCongViec.HasValue) query = query.Where(t => t.LoaiHinhCongViec == filterLoaiCongViec.Value);
            if (filterLoaiNguoiDang.HasValue) query = query.Where(t => t.NguoiDang.LoaiTk == filterLoaiNguoiDang.Value);
            if (filterThanhPhoId.HasValue) query = query.Where(t => t.ThanhPhoId == filterThanhPhoId.Value);
            if (filterQuanHuyenId.HasValue) query = query.Where(t => t.QuanHuyenId == filterQuanHuyenId.Value);
            if (viewModel.FilterNganhNgheIds != null && viewModel.FilterNganhNgheIds.Any())
            {
                query = query.Where(t => t.TinTuyenDungNganhNghes.Any(tnn => viewModel.FilterNganhNgheIds.Contains(tnn.NganhNgheId)));
            }
            if (filterLuongMin.HasValue)
            {
                query = query.Where(t => (t.LuongToiDa.HasValue && t.LuongToiDa >= filterLuongMin.Value) || (t.LuongToiThieu.HasValue && t.LuongToiThieu >= filterLuongMin.Value) || (!t.LuongToiThieu.HasValue && !t.LuongToiDa.HasValue && t.LoaiLuong != LoaiLuong.thoathuan));
            }
            if (filterLuongMax.HasValue)
            {
                 query = query.Where(t => (t.LuongToiThieu.HasValue && t.LuongToiThieu <= filterLuongMax.Value) || (!t.LuongToiThieu.HasValue && t.LoaiLuong != LoaiLuong.thoathuan));
            }
            if (viewModel.FilterTinGap.HasValue)
            {
                query = query.Where(t => t.TinGap == viewModel.FilterTinGap.Value);
            }

            switch (sortBy.ToLower())
            {
                case "cunhat": query = query.OrderBy(t => t.NgayDang); break;
                case "tieude_asc": query = query.OrderBy(t => t.TieuDe); break;
                case "tieude_desc": query = query.OrderByDescending(t => t.TieuDe); break;
                case "moinhat": default: query = query.OrderByDescending(t => t.NgayDang); break;
            }

            int pageSize = (viewMode == "grid") ? 8 : 5;
            var paginatedRaw = await PaginatedList<TinTuyenDung>.CreateAsync(query, page, pageSize);

            var jobPostingViewModels = paginatedRaw.Select(t => new QuanLyTinDangItemViewModel
            {
                Id = t.Id, TieuDe = t.TieuDe,
                TenNguoiDang = t.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? t.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? "N/A" : t.NguoiDang.HoTen,
                LogoNguoiDangUrl = t.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep ? t.NguoiDang.HoSoDoanhNghiep?.UrlLogo : t.NguoiDang.UrlAvatar,
                LoaiTaiKhoanNguoiDang = t.NguoiDang.LoaiTk,
                DiaDiemLamViec = $"{t.QuanHuyen?.Ten ?? ""}, {t.ThanhPho?.Ten ?? ""}".Trim(new char[] { ' ', ',' }),
                LoaiHinhCongViecDisplay = t.LoaiHinhCongViec.GetDisplayName(),
                ThoiGianDangTuongDoi = t.NgayDang.ToRelativeTime(),
                TrangThai = t.TrangThai, TrangThaiDisplay = t.TrangThai.GetDisplayName(), TrangThaiCssClass = GetTrangThaiCssClass(t.TrangThai),
                TinGap = t.TinGap, MucLuongDisplay = FormatMucLuong(t.LuongToiThieu, t.LuongToiDa, t.LoaiLuong),
                DanhMucNganhNghe = t.TinTuyenDungNganhNghes.Select(nn => nn.NganhNghe.Ten).ToList(),
                NgayDang = t.NgayDang, NgayHetHan = t.NgayHetHan
            }).ToList();

            viewModel.JobPostings = new PaginatedList<QuanLyTinDangItemViewModel>(jobPostingViewModels, paginatedRaw.TotalCount, paginatedRaw.PageIndex, pageSize);
            return View(viewModel);
        }

        private async Task PopulateFilterOptions(QuanLyTinDangIndexViewModel viewModel)
        {
            viewModel.TrangThaiOptions = new SelectList(Enum.GetValues(typeof(TrangThaiTinTuyenDung)).Cast<TrangThaiTinTuyenDung>().Select(e => new { Value = (int)e, Text = e.GetDisplayName() }), "Value", "Text", viewModel.FilterTrangThai);
            viewModel.LoaiCongViecOptions = new SelectList(Enum.GetValues(typeof(LoaiHinhCongViec)).Cast<LoaiHinhCongViec>().Select(e => new { Value = (int)e, Text = e.GetDisplayName() }), "Value", "Text", viewModel.FilterLoaiCongViec);
            viewModel.LoaiNguoiDangOptions = new SelectList(new List<object> { new { Value = (int)LoaiTaiKhoan.canhan, Text = LoaiTaiKhoan.canhan.GetDisplayName() }, new { Value = (int)LoaiTaiKhoan.doanhnghiep, Text = LoaiTaiKhoan.doanhnghiep.GetDisplayName() } }, "Value", "Text", viewModel.FilterLoaiNguoiDang);
            viewModel.ThanhPhoOptions = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", viewModel.FilterThanhPhoId);
            if (viewModel.FilterThanhPhoId.HasValue)
            {
                viewModel.QuanHuyenOptions = new SelectList(await _context.QuanHuyens.Where(qh => qh.ThanhPhoId == viewModel.FilterThanhPhoId.Value).OrderBy(qh => qh.Ten).ToListAsync(), "Id", "Ten", viewModel.FilterQuanHuyenId);
            }
            else { viewModel.QuanHuyenOptions = new SelectList(new List<QuanHuyen>(), "Id", "Ten"); }
            var allNganhNghe = await _context.NganhNghes.OrderBy(n => n.Ten).ToListAsync();
            viewModel.NganhNgheOptions = allNganhNghe.Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Ten, Selected = viewModel.FilterNganhNgheIds?.Contains(n.Id) ?? false }).ToList();
            viewModel.SortByOptions = new SelectList(new[] { new { Value = "moinhat", Text = "Mới nhất" }, new { Value = "cunhat", Text = "Cũ nhất" }, new { Value = "tieude_asc", Text = "Tiêu đề A-Z" }, new { Value = "tieude_desc", Text = "Tiêu đề Z-A" }, }, "Value", "Text", viewModel.SortBy);
        }

        [HttpGet("GetQuanHuyenByThanhPho")]
        public async Task<IActionResult> GetQuanHuyenByThanhPho(int thanhPhoId)
        {
            var quanHuyens = await _context.QuanHuyens
                .Where(qh => qh.ThanhPhoId == thanhPhoId)
                .OrderBy(qh => qh.Ten)
                .Select(qh => new { id = qh.Id, ten = qh.Ten })
                .ToListAsync();
            return Json(quanHuyens);
        }

        [HttpPost("DuyetTin")]
        public async Task<IActionResult> DuyetTin(int id)
        {
            // <<<<<<< BẮT ĐẦU TÍCH HỢP THÔNG BÁO >>>>>>>
            var tin = await _context.TinTuyenDungs.Include(t => t.NguoiDang).FirstOrDefaultAsync(t => t.Id == id); // Include NguoiDang
            if (tin == null || tin.TrangThai == TrangThaiTinTuyenDung.daxoa) return Json(new { success = false, message = "Tin không tồn tại." });
            if (tin.TrangThai != TrangThaiTinTuyenDung.choduyet && tin.TrangThai != TrangThaiTinTuyenDung.bituchoi) return Json(new { success = false, message = "Chỉ có thể duyệt tin đang ở trạng thái 'Chờ duyệt' hoặc 'Bị từ chối'." });

            tin.TrangThai = TrangThaiTinTuyenDung.daduyet;
            tin.AdminDuyetId = GetCurrentAdminId();
            tin.NgayDuyet = DateTime.UtcNow;
            tin.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Gửi thông báo cho người đăng tin
            try
            {
                var duLieuThongBao = new
                {
                    tieuDeTin = tin.TieuDe,
                    tinId = tin.Id,
                    noiDung = $"Tin tuyển dụng '{tin.TieuDe}' của bạn đã được quản trị viên duyệt và hiển thị.",
                    // url = $"/chi-tiet-tin/{tin.Id}" // Cân nhắc thêm URL client-side
                };
                await _thongBaoService.CreateThongBaoAsync(
                    tin.NguoiDangId,
                    NotificationConstants.Types.TinTuyenDungDuyet,
                    JsonSerializer.Serialize(duLieuThongBao),
                    NotificationConstants.RelatedEntities.TinTuyenDung,
                    tin.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo duyệt tin cho Tin ID {TinID} tới User ID {NguoiDangId}", tin.Id, tin.NguoiDangId);
                // Không làm gián đoạn quá trình duyệt tin nếu gửi thông báo lỗi
            }
            // <<<<<<< KẾT THÚC TÍCH HỢP THÔNG BÁO >>>>>>>
            return Json(new { success = true, message = "Duyệt tin thành công." });
        }

        [HttpPost("TuChoiTin")]
        public async Task<IActionResult> TuChoiTin(int id, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo)) return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });
            // <<<<<<< BẮT ĐẦU TÍCH HỢP THÔNG BÁO >>>>>>>
            var tin = await _context.TinTuyenDungs.Include(t => t.NguoiDang).FirstOrDefaultAsync(t => t.Id == id); // Include NguoiDang
            if (tin == null || tin.TrangThai == TrangThaiTinTuyenDung.daxoa) return Json(new { success = false, message = "Tin không tồn tại." });
            if (tin.TrangThai != TrangThaiTinTuyenDung.choduyet) return Json(new { success = false, message = "Chỉ có thể từ chối tin đang ở trạng thái 'Chờ duyệt'." });

            tin.TrangThai = TrangThaiTinTuyenDung.bituchoi;
            tin.AdminDuyetId = GetCurrentAdminId();
            tin.NgayDuyet = DateTime.UtcNow;
            tin.NgayCapNhat = DateTime.UtcNow;
            // Cân nhắc thêm một trường để lưu lý do từ chối vào Model TinTuyenDung nếu cần
            // tin.LyDoTuChoi = lyDo; // Bạn cần thêm trường LyDoTuChoi vào model TinTuyenDung
            await _context.SaveChangesAsync();

            // Gửi thông báo cho người đăng tin
            try
            {
                var duLieuThongBao = new
                {
                    tieuDeTin = tin.TieuDe,
                    tinId = tin.Id,
                    lyDoTuChoi = lyDo,
                    noiDung = $"Tin tuyển dụng '{tin.TieuDe}' của bạn đã bị từ chối bởi quản trị viên. Lý do: {lyDo}",
                    // url = $"/quan-ly-tin-dang-cua-toi/{tin.Id}" // URL để người dùng xem lại tin
                };
                await _thongBaoService.CreateThongBaoAsync(
                    tin.NguoiDangId,
                    NotificationConstants.Types.TinTuyenDungTuChoi,
                    JsonSerializer.Serialize(duLieuThongBao),
                    NotificationConstants.RelatedEntities.TinTuyenDung,
                    tin.Id
                );
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Lỗi khi gửi thông báo từ chối tin cho Tin ID {TinID} tới User ID {NguoiDangId}", tin.Id, tin.NguoiDangId);
            }
            // <<<<<<< KẾT THÚC TÍCH HỢP THÔNG BÁO >>>>>>>
            return Json(new { success = true, message = "Từ chối tin thành công." });
        }

        [HttpPost("XoaTin")]
        public async Task<IActionResult> XoaTin(int id)
        {
            // Thông thường không cần gửi thông báo cho người dùng khi admin xóa mềm một tin
            // Trừ khi có yêu cầu nghiệp vụ đặc biệt.
            var tin = await _context.TinTuyenDungs.FindAsync(id);
            if (tin == null || tin.TrangThai == TrangThaiTinTuyenDung.daxoa) return Json(new { success = false, message = "Tin không tồn tại hoặc đã được xóa." });
            tin.TrangThai = TrangThaiTinTuyenDung.daxoa;
            tin.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa tin thành công." });
        }

        [HttpGet("XuatBaoCao")]
        public IActionResult XuatBaoCao()
        {
            TempData["Message"] = "Chức năng xuất báo cáo đang được phát triển.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("ChiTiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id, string tab = "chitiet")
        {
            var tinTuyenDung = await _context.TinTuyenDungs
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.QuanHuyen)
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.ThanhPho)
                .Include(t => t.QuanHuyen)
                .Include(t => t.ThanhPho)
                .Include(t => t.LichLamViecCongViecs)
                .Include(t => t.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .Include(t => t.UngTuyens)
                .Include(t => t.AdminDuyet)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tinTuyenDung == null || tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daxoa)
            {
                TempData["ErrorMessage"] = "Tin tuyển dụng không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new AdminJobPostingDetailViewModel
            {
                JobPostingId = tinTuyenDung.Id,
                JobPostingTitle = tinTuyenDung.TieuDe,
                CurrentTab = tab.ToLower() switch { "nguoidang" => "nguoidang", "lichsu" => "lichsu", _ => "chitiet" }
            };

            var jobInfo = viewModel.JobInfo;
            jobInfo.TieuDe = tinTuyenDung.TieuDe;
            jobInfo.LoaiHinhCongViecDisplay = tinTuyenDung.LoaiHinhCongViec.GetDisplayName();
            jobInfo.DanhMucNganhNghe = string.Join(", ", tinTuyenDung.TinTuyenDungNganhNghes.Select(nn => nn.NganhNghe.Ten).DefaultIfEmpty("Chưa rõ"));
            jobInfo.MucLuongDisplay = FormatMucLuong(tinTuyenDung.LuongToiThieu, tinTuyenDung.LuongToiDa, tinTuyenDung.LoaiLuong);
            if (tinTuyenDung.NgayHetHan.HasValue) jobInfo.ThoiGianLamViec_Formatted = $"{tinTuyenDung.NgayDang:dd/MM/yyyy} - {tinTuyenDung.NgayHetHan.Value:dd/MM/yyyy}";
            else jobInfo.ThoiGianLamViec_Formatted = $"Từ {tinTuyenDung.NgayDang:dd/MM/yyyy} (Không giới hạn)";
            jobInfo.DiaDiemLamViec = $"{tinTuyenDung.DiaChiLamViec}, {tinTuyenDung.QuanHuyen?.Ten}, {tinTuyenDung.ThanhPho?.Ten}".Trim(',', ' ');
            jobInfo.LamViecTuXaDisplay = "Không";
            jobInfo.MoTaCongViec = tinTuyenDung.MoTa.Replace("\n", "<br />");
            jobInfo.YeuCauCongViec = (tinTuyenDung.YeuCau ?? "Không có yêu cầu cụ thể.").Replace("\n", "<br />");
            jobInfo.QuyenLoi = (tinTuyenDung.QuyenLoi ?? "Không có thông tin quyền lợi.").Replace("\n", "<br />");
            jobInfo.KyNangTags = new List<string> { "Giao tiếp", "Phục vụ khách hàng", "Làm việc nhóm" };
            jobInfo.PhuCapKhacHtml = "<p>- Hỗ trợ tiền gửi xe</p><p>- Bao ăn trưa</p>";
            jobInfo.LichLamViecs = tinTuyenDung.LichLamViecCongViecs.Select(l => new AdminLichLamViecViewModel
            { NgayTrongTuanDisplay = l.NgayTrongTuan.GetDisplayName(), ThoiGianDisplay = FormatLichLamViecThoiGianDisplay(l.GioBatDau, l.GioKetThuc, l.BuoiLamViec) }).ToList();

            var recruiterInfo = viewModel.RecruiterInfo;
            var nguoiDang = tinTuyenDung.NguoiDang;
            recruiterInfo.Email = nguoiDang.Email;
            recruiterInfo.SoDienThoai = nguoiDang.Sdt ?? "Chưa cập nhật";
            recruiterInfo.DiaChiLienHeFull = $"{nguoiDang.DiaChiChiTiet}, {nguoiDang.QuanHuyen?.Ten ?? ""}, {nguoiDang.ThanhPho?.Ten ?? ""}".Trim(',', ' ');

            if (nguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep && nguoiDang.HoSoDoanhNghiep != null)
            {
                var hsDn = nguoiDang.HoSoDoanhNghiep;
                recruiterInfo.TenNguoiHoacCongTy = hsDn.TenCongTy;
                recruiterInfo.LoaiHinhTaiKhoanDisplay = "Doanh nghiệp";
                recruiterInfo.AvatarUrl = hsDn.UrlLogo ?? nguoiDang.UrlAvatar ?? "/images/default-company-logo.png";
                recruiterInfo.WebsiteUrl = hsDn.UrlWebsite ?? string.Empty;
                recruiterInfo.MoTaCongTyHtml = hsDn.MoTa?.Replace("\n", "<br />");
                recruiterInfo.DiaChiDangKyKinhDoanh = hsDn.DiaChiDangKy;
                recruiterInfo.MaSoThue = hsDn.MaSoThue;
                recruiterInfo.QuyMoCongTy = hsDn.QuyMoCongTy;
                recruiterInfo.CongTyDaXacMinh = hsDn.DaXacMinh;
            }
            else
            {
                recruiterInfo.TenNguoiHoacCongTy = nguoiDang.HoTen;
                recruiterInfo.LoaiHinhTaiKhoanDisplay = nguoiDang.LoaiTk == LoaiTaiKhoan.canhan ? "Cá nhân" : "Doanh nghiệp (Chưa có hồ sơ)";
                recruiterInfo.AvatarUrl = nguoiDang.UrlAvatar ?? "/images/default-avatar.png";
            }

            var activityLog = viewModel.ActivityLog;
            activityLog.Add(new ActivityLogItemViewModel { NguoiThucHien = $"{nguoiDang.HoTen} ({FormatNguoiDangRoleForLog(nguoiDang)})", AvatarUrl = recruiterInfo.AvatarUrl, HanhDongChinh = "Đã tạo tin đăng", MoTaChiTietHtml = $"Tin được tạo tự động hoặc bởi người đăng.", ThoiGianDateTime = tinTuyenDung.NgayTao });
            if (tinTuyenDung.NgayCapNhat > tinTuyenDung.NgayTao && tinTuyenDung.NgayCapNhat.Date != tinTuyenDung.NgayDuyet?.Date)
            { activityLog.Add(new ActivityLogItemViewModel { NguoiThucHien = $"{nguoiDang.HoTen} ({FormatNguoiDangRoleForLog(nguoiDang)})", AvatarUrl = recruiterInfo.AvatarUrl, HanhDongChinh = "Đã chỉnh sửa tin đăng", MoTaChiTietHtml = "Nội dung tin đăng đã được cập nhật.", ThoiGianDateTime = tinTuyenDung.NgayCapNhat }); }
            if (tinTuyenDung.AdminDuyetId.HasValue && tinTuyenDung.NgayDuyet.HasValue && tinTuyenDung.AdminDuyet != null)
            {
                var adminDuyet = tinTuyenDung.AdminDuyet;
                activityLog.Add(new ActivityLogItemViewModel { NguoiThucHien = $"{adminDuyet.HoTen} (Admin)", AvatarUrl = adminDuyet.UrlAvatar ?? "/images/admin-avatar.png", HanhDongChinh = tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daduyet ? "Đã duyệt tin đăng" : tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.bituchoi ? "Đã từ chối tin đăng" : "Đã xử lý tin", ThoiGianDateTime = tinTuyenDung.NgayDuyet.Value });
            }
            viewModel.ActivityLog = activityLog.OrderBy(a => a.ThoiGianDateTime).ToList();

            var sidebar = viewModel.PostingStatusSidebar;
            sidebar.JobPostingId = tinTuyenDung.Id;
            sidebar.JobPostingTitle = tinTuyenDung.TieuDe;
            sidebar.TrangThaiTagOverall = tinTuyenDung.TinGap ? "Tuyển gấp" : tinTuyenDung.TrangThai.GetDisplayName();
            sidebar.TrangThaiTagOverallCssClass = tinTuyenDung.TinGap ? "badge-danger" : GetTrangThaiCssClass(tinTuyenDung.TrangThai);
            sidebar.ThoiGianDangDisplay = tinTuyenDung.NgayDang.ToString("HH:mm dd/MM/yyyy");
            sidebar.NguoiDangDisplay = $"{nguoiDang.HoTen} ({FormatNguoiDangRoleForLog(nguoiDang)})";
            sidebar.TrangThaiTinChiTiet = tinTuyenDung.TrangThai.GetDisplayName();
            sidebar.TrangThaiTinChiTietCssClass = GetTrangThaiCssClass(tinTuyenDung.TrangThai);
            sidebar.LuotXem = 245;
            sidebar.LuotUngTuyen = tinTuyenDung.UngTuyens.Count;
            sidebar.NgayTaoDisplay = tinTuyenDung.NgayTao.ToString("dd/MM/yyyy");
            sidebar.CapNhatLanCuoiDisplay = tinTuyenDung.NgayCapNhat.ToString("dd/MM/yyyy");
            sidebar.CanDuyet = tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.choduyet || tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.bituchoi;
            sidebar.CanTuChoi = tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.choduyet;
            sidebar.CanChinhSua = true;
            return View(viewModel);
        }

        [HttpGet("ChinhSua/{id}")]
        public async Task<IActionResult> ChinhSua(int id)
        {
            var tinTuyenDung = await _context.TinTuyenDungs
                .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.ThanhPho).Include(t => t.QuanHuyen)
                .Include(t => t.TinTuyenDungNganhNghes).ThenInclude(tnn => tnn.NganhNghe)
                .Include(t => t.LichLamViecCongViecs)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tinTuyenDung == null || tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daxoa)
            { TempData["ErrorMessage"] = "Tin tuyển dụng không tồn tại hoặc đã bị xóa."; return RedirectToAction(nameof(Index)); }

            var viewModel = new AdminEditJobPostingViewModel
            {
                Id = tinTuyenDung.Id, IdTinDangDisplay = $"JP{tinTuyenDung.Id:D4}", TieuDe = tinTuyenDung.TieuDe,
                TenCongTy = tinTuyenDung.NguoiDang?.LoaiTk == LoaiTaiKhoan.doanhnghiep ? tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? tinTuyenDung.NguoiDang.HoTen : tinTuyenDung.NguoiDang?.HoTen,
                DiaChiLamViec = tinTuyenDung.DiaChiLamViec, ThanhPhoId = tinTuyenDung.ThanhPhoId, QuanHuyenId = tinTuyenDung.QuanHuyenId,
                LuongToiThieu = tinTuyenDung.LuongToiThieu, LuongToiDa = tinTuyenDung.LuongToiDa, LoaiLuong = tinTuyenDung.LoaiLuong,
                LoaiHinhCongViec = tinTuyenDung.LoaiHinhCongViec, SoLuongTuyen = tinTuyenDung.SoLuongTuyen, NgayHetHan = tinTuyenDung.NgayHetHan,
                MoTa = tinTuyenDung.MoTa, YeuCau = tinTuyenDung.YeuCau, QuyenLoi = tinTuyenDung.QuyenLoi,
                SelectedNganhNgheIds = tinTuyenDung.TinTuyenDungNganhNghes.Select(nn => nn.NganhNgheId).ToList(),
                TrangThai = tinTuyenDung.TrangThai, TinGap = tinTuyenDung.TinGap,
                NgayTaoDisplay = tinTuyenDung.NgayTao.ToString("dd/MM/yyyy HH:mm"), CapNhatCuoiDisplay = tinTuyenDung.NgayCapNhat.ToString("dd/MM/yyyy HH:mm"),
                NguoiDangDisplay = tinTuyenDung.NguoiDang?.HoTen,
                LichLamViecItems = tinTuyenDung.LichLamViecCongViecs.Select(l => new HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel
                { Id = l.Id, NgayTrongTuan = l.NgayTrongTuan, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc, BuoiLamViec = l.BuoiLamViec }).ToList()
            };
            if (viewModel.LichLamViecItems.Any())
            {
                viewModel.LichLamViecMoTa = string.Join("; ", viewModel.LichLamViecItems.Select(l =>
                { string timePart = ""; if (l.GioBatDau.HasValue && l.GioKetThuc.HasValue) timePart = $" ({l.GioBatDau.Value:hh\\:mm} - {l.GioKetThuc.Value:hh\\:mm})"; else if (l.BuoiLamViec.HasValue) timePart = $" ({l.BuoiLamViec.Value.GetDisplayName()})"; return $"{l.NgayTrongTuan.GetDisplayName()}{timePart}"; }));
            }
            await PopulateEditViewModelDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost("ChinhSua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSua(int id, AdminEditJobPostingViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var tinTuyenDung = await _context.TinTuyenDungs
                .Include(t => t.NguoiDang).ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.TinTuyenDungNganhNghes).Include(t => t.LichLamViecCongViecs)
                .FirstOrDefaultAsync(t => t.Id == viewModel.Id);

            if (tinTuyenDung == null || tinTuyenDung.TrangThai == TrangThaiTinTuyenDung.daxoa)
            { TempData["ErrorMessage"] = "Tin tuyển dụng không tồn tại hoặc đã bị xóa."; return RedirectToAction(nameof(Index)); }
            
            if (viewModel.LuongToiThieu.HasValue && viewModel.LuongToiDa.HasValue && viewModel.LuongToiDa < viewModel.LuongToiThieu)
            { ModelState.AddModelError(nameof(viewModel.LuongToiDa), "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu."); }
            if (viewModel.LoaiLuong == LoaiLuong.thoathuan) { viewModel.LuongToiThieu = null; viewModel.LuongToiDa = null; }
            else { if (!viewModel.LuongToiThieu.HasValue && !viewModel.LuongToiDa.HasValue) { /* Allow */ } }

            if (ModelState.IsValid)
            {
                try
                {
                    tinTuyenDung.TieuDe = viewModel.TieuDe;
                    tinTuyenDung.DiaChiLamViec = viewModel.DiaChiLamViec; tinTuyenDung.ThanhPhoId = viewModel.ThanhPhoId; tinTuyenDung.QuanHuyenId = viewModel.QuanHuyenId;
                    tinTuyenDung.LuongToiThieu = viewModel.LuongToiThieu; tinTuyenDung.LuongToiDa = viewModel.LuongToiDa; tinTuyenDung.LoaiLuong = viewModel.LoaiLuong;
                    tinTuyenDung.LoaiHinhCongViec = viewModel.LoaiHinhCongViec; tinTuyenDung.SoLuongTuyen = viewModel.SoLuongTuyen; tinTuyenDung.NgayHetHan = viewModel.NgayHetHan;
                    tinTuyenDung.MoTa = viewModel.MoTa; tinTuyenDung.YeuCau = viewModel.YeuCau; tinTuyenDung.QuyenLoi = viewModel.QuyenLoi;
                    
                    var oldTrangThai = tinTuyenDung.TrangThai;
                    var newTrangThai = viewModel.TrangThai; // <<<<<<< LƯU TRẠNG THÁI MỚI
                    tinTuyenDung.TrangThai = newTrangThai;
                    
                    tinTuyenDung.TinGap = viewModel.TinGap;
                    tinTuyenDung.NgayCapNhat = DateTime.UtcNow;

                    bool guiThongBaoDuyet = false;
                    bool guiThongBaoTuChoi = false;

                    if (newTrangThai == TrangThaiTinTuyenDung.daduyet && (oldTrangThai == TrangThaiTinTuyenDung.choduyet || oldTrangThai == TrangThaiTinTuyenDung.bituchoi))
                    {
                        tinTuyenDung.AdminDuyetId = GetCurrentAdminId();
                        tinTuyenDung.NgayDuyet = DateTime.UtcNow;
                        guiThongBaoDuyet = true; // <<<<<<< ĐÁNH DẤU ĐỂ GỬI THÔNG BÁO
                    }
                    else if (newTrangThai == TrangThaiTinTuyenDung.bituchoi && oldTrangThai != TrangThaiTinTuyenDung.bituchoi)
                    {
                         tinTuyenDung.AdminDuyetId = GetCurrentAdminId(); 
                         tinTuyenDung.NgayDuyet = DateTime.UtcNow;
                         guiThongBaoTuChoi = true; // <<<<<<< ĐÁNH DẤU ĐỂ GỬI THÔNG BÁO
                    }

                    var currentNganhNgheIds = tinTuyenDung.TinTuyenDungNganhNghes.Select(nn => nn.NganhNgheId).ToList();
                    var nganhNgheToRemove = tinTuyenDung.TinTuyenDungNganhNghes.Where(tnn => !viewModel.SelectedNganhNgheIds.Contains(tnn.NganhNgheId)).ToList();
                    _context.TinTuyenDung_NganhNghes.RemoveRange(nganhNgheToRemove);
                    var nganhNgheIdsToAdd = viewModel.SelectedNganhNgheIds.Where(nnId => !currentNganhNgheIds.Contains(nnId)).ToList();
                    foreach (var nganhNgheIdToAdd in nganhNgheIdsToAdd) { tinTuyenDung.TinTuyenDungNganhNghes.Add(new TinTuyenDung_NganhNghe { NganhNgheId = nganhNgheIdToAdd }); }
                    UpdateLichLamViec(tinTuyenDung, viewModel.LichLamViecItems);

                    await _context.SaveChangesAsync();

                    // <<<<<<< GỬI THÔNG BÁO SAU KHI LƯU THÀNH CÔNG >>>>>>>
                    if (guiThongBaoDuyet)
                    {
                        try
                        {
                            var duLieuThongBao = new { tieuDeTin = tinTuyenDung.TieuDe, tinId = tinTuyenDung.Id, noiDung = $"Tin tuyển dụng '{tinTuyenDung.TieuDe}' của bạn đã được duyệt (cập nhật bởi admin)." };
                            await _thongBaoService.CreateThongBaoAsync(tinTuyenDung.NguoiDangId, NotificationConstants.Types.TinTuyenDungDuyet, JsonSerializer.Serialize(duLieuThongBao), NotificationConstants.RelatedEntities.TinTuyenDung, tinTuyenDung.Id);
                        } catch (Exception ex) { _logger.LogError(ex, "Lỗi gửi TB duyệt tin (từ ChinhSua) cho Tin ID {TinID}", tinTuyenDung.Id); }
                    }
                    else if (guiThongBaoTuChoi)
                    {
                         try
                        {
                            // Cân nhắc lấy lý do từ chối từ một trường trong viewModel nếu admin có thể nhập
                            string lyDoTuChoiMacDinh = "Admin đã cập nhật và thay đổi trạng thái.";
                            var duLieuThongBao = new { tieuDeTin = tinTuyenDung.TieuDe, tinId = tinTuyenDung.Id, lyDoTuChoi = lyDoTuChoiMacDinh, noiDung = $"Tin tuyển dụng '{tinTuyenDung.TieuDe}' của bạn bị từ chối (cập nhật bởi admin). Lý do: {lyDoTuChoiMacDinh}" };
                            await _thongBaoService.CreateThongBaoAsync(tinTuyenDung.NguoiDangId, NotificationConstants.Types.TinTuyenDungTuChoi, JsonSerializer.Serialize(duLieuThongBao), NotificationConstants.RelatedEntities.TinTuyenDung, tinTuyenDung.Id);
                        } catch (Exception ex) { _logger.LogError(ex, "Lỗi gửi TB từ chối tin (từ ChinhSua) cho Tin ID {TinID}", tinTuyenDung.Id); }
                    }
                    // <<<<<<< KẾT THÚC GỬI THÔNG BÁO >>>>>>>

                    TempData["SuccessMessage"] = "Cập nhật tin đăng thành công.";
                    return RedirectToAction(nameof(ChiTiet), new { id = tinTuyenDung.Id });
                }
                catch (DbUpdateConcurrencyException) { if (!TinTuyenDungExists(viewModel.Id)) { return NotFound(); } else { throw; } }
                catch (Exception ex) { TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}"; }
            }

            await PopulateEditViewModelDropdowns(viewModel);
            viewModel.IdTinDangDisplay = $"JP{tinTuyenDung.Id:D4}";
            viewModel.NgayTaoDisplay = tinTuyenDung.NgayTao.ToString("dd/MM/yyyy HH:mm");
            viewModel.CapNhatCuoiDisplay = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
            viewModel.NguoiDangDisplay = tinTuyenDung.NguoiDang?.HoTen;
            return View(viewModel);
        }

        private async Task PopulateEditViewModelDropdowns(AdminEditJobPostingViewModel viewModel)
        {
            viewModel.ThanhPhoOptions = new SelectList(await _context.ThanhPhos.OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", viewModel.ThanhPhoId);
            if (viewModel.ThanhPhoId > 0) viewModel.QuanHuyenOptions = new SelectList(await _context.QuanHuyens.Where(qh => qh.ThanhPhoId == viewModel.ThanhPhoId).OrderBy(qh => qh.Ten).ToListAsync(), "Id", "Ten", viewModel.QuanHuyenId);
            else viewModel.QuanHuyenOptions = new SelectList(new List<QuanHuyen>(), "Id", "Ten", viewModel.QuanHuyenId);
            viewModel.LoaiHinhCongViecOptions = new SelectList(Enum.GetValues(typeof(LoaiHinhCongViec)).Cast<LoaiHinhCongViec>().Select(e => new { Value = (int)e, Text = e.GetDisplayName() }), "Value", "Text", (int)viewModel.LoaiHinhCongViec);
            viewModel.LoaiLuongOptions = new SelectList(Enum.GetValues(typeof(LoaiLuong)).Cast<LoaiLuong>().Select(e => new { Value = (int)e, Text = e.GetDisplayName() }), "Value", "Text", (int)viewModel.LoaiLuong);
            var allNganhNghe = await _context.NganhNghes.OrderBy(n => n.Ten).ToListAsync();
            viewModel.NganhNgheOptions = new MultiSelectList(allNganhNghe, "Id", "Ten", viewModel.SelectedNganhNgheIds);
            viewModel.TrangThaiTinDangOptions = new SelectList(Enum.GetValues(typeof(TrangThaiTinTuyenDung)).Cast<TrangThaiTinTuyenDung>().Where(e => e != TrangThaiTinTuyenDung.daxoa).Select(e => new { Value = (int)e, Text = e.GetDisplayName() }), "Value", "Text", (int)viewModel.TrangThai);
        }

        private void UpdateLichLamViec(TinTuyenDung tinTuyenDung, List<HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel> lichVmItems)
        {
            if (lichVmItems == null) lichVmItems = new List<HeThongTimViec.ViewModels.JobPosting.LichLamViecViewModel>();
            var lichToRemove = tinTuyenDung.LichLamViecCongViecs.Where(l_db => !lichVmItems.Any(l_vm => l_vm.Id.HasValue && l_vm.Id == l_db.Id) || (lichVmItems.Any(l_vm => l_vm.Id.HasValue && l_vm.Id == l_db.Id && l_vm.MarkedForDeletion))).ToList();
            _context.LichLamViecCongViecs.RemoveRange(lichToRemove);
            foreach (var itemVm in lichVmItems)
            {
                if (itemVm.MarkedForDeletion) continue;
                bool isNgayTrongTuanDefault = itemVm.NgayTrongTuan == default(NgayTrongTuan) && !Enum.IsDefined(typeof(NgayTrongTuan), itemVm.NgayTrongTuan);
                if (isNgayTrongTuanDefault && !itemVm.GioBatDau.HasValue && !itemVm.GioKetThuc.HasValue && !itemVm.BuoiLamViec.HasValue) continue;
                if (itemVm.Id.HasValue && itemVm.Id > 0)
                { var existingLich = tinTuyenDung.LichLamViecCongViecs.FirstOrDefault(l => l.Id == itemVm.Id.Value); if (existingLich != null) { existingLich.NgayTrongTuan = itemVm.NgayTrongTuan; existingLich.GioBatDau = itemVm.GioBatDau; existingLich.GioKetThuc = itemVm.GioKetThuc; existingLich.BuoiLamViec = itemVm.BuoiLamViec; } }
                else { tinTuyenDung.LichLamViecCongViecs.Add(new LichLamViecCongViec { NgayTrongTuan = itemVm.NgayTrongTuan, GioBatDau = itemVm.GioBatDau, GioKetThuc = itemVm.GioKetThuc, BuoiLamViec = itemVm.BuoiLamViec }); }
            }
        }

        private bool TinTuyenDungExists(int id) { return _context.TinTuyenDungs.Any(e => e.Id == id); }
        private string FormatLichLamViecThoiGianDisplay(TimeSpan? gioBatDau, TimeSpan? gioKetThuc, BuoiLamViec? buoi)
        { if (gioBatDau.HasValue && gioKetThuc.HasValue) return $"{gioBatDau.Value:hh\\:mm} - {gioKetThuc.Value:hh\\:mm}"; if (buoi.HasValue) return buoi.Value.GetDisplayName(); return "Linh hoạt"; }
        private string FormatNguoiDangRoleForLog(NguoiDung nguoiDang)
        { if (nguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep) return nguoiDang.HoSoDoanhNghiep?.TenCongTy != null ? $"HR ({nguoiDang.HoSoDoanhNghiep.TenCongTy})" : "Doanh nghiệp"; return "Cá nhân"; }
        private int? GetCurrentAdminId()
        { var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId)) return userId; return null; }
        private string FormatMucLuong(ulong? min, ulong? max, LoaiLuong loaiLuong)
        { if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận"; string unit = loaiLuong switch { LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca", LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => "" }; if (min.HasValue && max.HasValue) { if (min == max) return $"{min:N0} VNĐ{unit}"; return $"{min:N0} - {max:N0} VNĐ{unit}"; } if (min.HasValue) return $"Từ {min:N0} VNĐ{unit}"; if (max.HasValue) return $"Đến {max:N0} VNĐ{unit}"; return "Không cụ thể"; }
        private string GetTrangThaiCssClass(TrangThaiTinTuyenDung trangThai)
        { return trangThai switch { TrangThaiTinTuyenDung.choduyet => "badge-warning text-dark", TrangThaiTinTuyenDung.daduyet => "badge-success", TrangThaiTinTuyenDung.taman => "badge-secondary", TrangThaiTinTuyenDung.hethan => "badge-danger", TrangThaiTinTuyenDung.datuyen => "badge-info", TrangThaiTinTuyenDung.bituchoi => "badge-dark", TrangThaiTinTuyenDung.daxoa => "badge-light text-dark", _ => "badge-light text-dark", }; }
       private async Task<List<ChartDataPoint>> GetTinMoiTheoThoiGianData(IQueryable<TinTuyenDung> baseQuery)
{
    // Bước 1: Lấy dữ liệu thô từ database (chưa định dạng chuỗi ngày)
    var rawData = await baseQuery
        .Where(t => t.NgayDang >= DateTime.UtcNow.AddMonths(-1))
        .GroupBy(t => t.NgayDang.Date)
        .Select(g => new
        {
            Date = g.Key,
            Count = g.Count()
        })
        .ToListAsync();

    // Bước 2: Xử lý định dạng và sắp xếp trong bộ nhớ
    var data = rawData
        .Select(g => new ChartDataPoint
        {
            Label = g.Date.ToString("dd/MM"),
            Value = g.Count
        })
        .OrderBy(dp => DateTime.ParseExact(dp.Label, "dd/MM", System.Globalization.CultureInfo.InvariantCulture))
        .Take(30)
        .ToList();

    return data;
}

    }
}