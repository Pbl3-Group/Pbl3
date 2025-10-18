// File: Controllers/QuanLyUngVienController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.QuanLyUngVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HeThongTimViec.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using HeThongTimViec.ViewModels.TimViec;
using HeThongTimViec.Helpers;
using HeThongTimViec.Services; // <<<<<<< THÊM USING CHO IThongBaoService
using HeThongTimViec.Utils;    // <<<<<<< THÊM USING CHO NotificationConstants
using System.Text.Json;        // <<<<<<< THÊM USING CHO JsonSerializer


namespace HeThongTimViec.Controllers
{
    [Authorize] // <<<<<<< Đảm bảo chỉ NTD mới vào được
    [Route("QuanLyUngVien")] // Có thể là [Area("NhaTuyenDung")][Route("QuanLyUngVien")] nếu bạn dùng Area
    public class QuanLyUngVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLyUngVienController> _logger;
        private readonly IThongBaoService _thongBaoService; // <<<<<<< INJECT SERVICE

        public QuanLyUngVienController(ApplicationDbContext context, ILogger<QuanLyUngVienController> logger, IThongBaoService thongBaoService)
        {
            _context = context;
            _logger = logger;
            _thongBaoService = thongBaoService; // <<<<<<< GÁN SERVICE
        }

        private int GetCurrentEmployerId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            _logger.LogError("Không thể parse Employer ID từ ClaimsPrincipal. Claim value: {ClaimValue}", userIdClaim);
            throw new UnauthorizedAccessException("Không thể xác định nhà tuyển dụng hợp lệ.");
        }

        public string GetUngVienTrangThaiBadgeClass(TrangThaiUngTuyen trangThai)
        {
            return ViewHelper.GetUngVienTrangThaiBadgeClass(trangThai);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(QuanLyUngVienViewModel model, int page = 1)
        {
            int employerId = GetCurrentEmployerId(); int pageSize = 10;
            IQueryable<UngTuyen> query = _context.UngTuyens
                .Include(ut => ut.UngVien).ThenInclude(uv => uv.HoSoUngVien)
                .Include(ut => ut.UngVien).ThenInclude(uv => uv.ThanhPho)
                .Include(ut => ut.UngVien).ThenInclude(uv => uv.DiaDiemMongMuons).ThenInclude(ddm => ddm.ThanhPho)
                .Include(ut => ut.TinTuyenDung)
                .Where(ut => ut.TinTuyenDung.NguoiDangId == employerId).AsNoTracking();

            if (model.SelectedTinTuyenDungId.HasValue && model.SelectedTinTuyenDungId > 0) query = query.Where(ut => ut.TinTuyenDungId == model.SelectedTinTuyenDungId.Value);
            if (model.FilterByTrangThai.HasValue) query = query.Where(ut => ut.TrangThai == model.FilterByTrangThai.Value);
            if (!string.IsNullOrWhiteSpace(model.SearchTerm)) { string searchTermLower = model.SearchTerm.ToLower(); query = query.Where(ut => (ut.UngVien.HoTen != null && ut.UngVien.HoTen.ToLower().Contains(searchTermLower)) || (ut.UngVien.Email != null && ut.UngVien.Email.ToLower().Contains(searchTermLower)) || (ut.UngVien.HoSoUngVien != null && ut.UngVien.HoSoUngVien.TieuDeHoSo != null && ut.UngVien.HoSoUngVien.TieuDeHoSo.ToLower().Contains(searchTermLower)) || (ut.UngVien.HoSoUngVien != null && ut.UngVien.HoSoUngVien.ViTriMongMuon != null && ut.UngVien.HoSoUngVien.ViTriMongMuon.ToLower().Contains(searchTermLower))); }
            if (model.FilterByKhuVucMongMuonIds != null && model.FilterByKhuVucMongMuonIds.Any()) query = query.Where(ut => ut.UngVien.DiaDiemMongMuons.Any(ddm => model.FilterByKhuVucMongMuonIds.Contains(ddm.ThanhPhoId)));
            model.SortBy ??= "ngaynop_desc";
            query = model.SortBy switch { "ngaynop_asc" => query.OrderBy(ut => ut.NgayNop), "ten_asc" => query.OrderBy(ut => ut.UngVien.HoTen), "ten_desc" => query.OrderByDescending(ut => ut.UngVien.HoTen), _ => query.OrderByDescending(ut => ut.NgayNop), };
            var paginatedUngTuyens = await PaginatedList<UngTuyen>.CreateAsync(query, page, pageSize);
            var ungVienItems = paginatedUngTuyens.Select(ut => { string kinhNghiemDisplay = "Chưa cập nhật"; if (ut.UngVien.HoSoUngVien?.GioiThieuBanThan != null) { if (ut.UngVien.HoSoUngVien.GioiThieuBanThan.ToLower().Contains("năm kinh nghiệm") || ut.UngVien.HoSoUngVien.GioiThieuBanThan.ToLower().Contains("kinh nghiệm làm việc")) kinhNghiemDisplay = "Có kinh nghiệm (chi tiết trong hồ sơ)"; } List<string> skillTags = new List<string>(); if (ut.UngVien.HoSoUngVien != null) { if (!string.IsNullOrWhiteSpace(ut.UngVien.HoSoUngVien.ViTriMongMuon)) skillTags.AddRange(ExtractSkills(ut.UngVien.HoSoUngVien.ViTriMongMuon)); if (!skillTags.Any() && !string.IsNullOrWhiteSpace(ut.UngVien.HoSoUngVien.TieuDeHoSo)) skillTags.AddRange(ExtractSkills(ut.UngVien.HoSoUngVien.TieuDeHoSo)); } return new UngVienItemViewModel { UngVienId = ut.UngVienId, UngTuyenId = ut.Id, TinTuyenDungIdLienQuan = ut.TinTuyenDungId, HoTenUngVien = ut.UngVien.HoTen, AvatarUrl = ut.UngVien.UrlAvatar ?? "/images/avatars/default_user.png", ViTriHoSo = ut.UngVien.HoSoUngVien?.TieuDeHoSo ?? ut.UngVien.HoSoUngVien?.ViTriMongMuon ?? "Chưa cập nhật", KinhNghiemDisplay = kinhNghiemDisplay, ThanhPhoUngVien = ut.UngVien.ThanhPho?.Ten ?? "Chưa cập nhật", SkillTags = skillTags.Distinct().Take(3).ToList(), NgayNopHoSo = ut.NgayNop, TrangThaiHienTai = ut.TrangThai, TrangThaiHienTaiDisplay = ut.TrangThai.GetDisplayName(), TrangThaiBadgeClass = GetUngVienTrangThaiBadgeClass(ut.TrangThai), UrlCvDaNop = ut.UrlCvDaNop }; }).ToList();
            model.UngViens = new PaginatedList<UngVienItemViewModel>(ungVienItems, paginatedUngTuyens.TotalCount, page, pageSize);
            await PopulateFilterOptions(model, employerId);
            return View(model);
        }
        private List<string> ExtractSkills(string? inputText) { if (string.IsNullOrWhiteSpace(inputText)) return new List<string>(); return inputText.Split(new[] { ',', ';', '/', '(', ')', '[', ']', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(s => s.Replace("developer", "", StringComparison.OrdinalIgnoreCase).Replace("engineer", "", StringComparison.OrdinalIgnoreCase).Replace("chuyên viên", "", StringComparison.OrdinalIgnoreCase).Replace("lập trình viên", "", StringComparison.OrdinalIgnoreCase).Trim()).Where(s => !string.IsNullOrEmpty(s) && s.Length > 1 && !s.Equals("senior", StringComparison.OrdinalIgnoreCase) && !s.Equals("junior", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).Take(5).ToList(); }
        private async Task PopulateFilterOptions(QuanLyUngVienViewModel model, int employerId) { model.TinTuyenDungOptions = new SelectList(await _context.TinTuyenDungs.Where(t => t.NguoiDangId == employerId && (t.TrangThai == TrangThaiTinTuyenDung.daduyet || t.TrangThai == TrangThaiTinTuyenDung.hethan || t.TrangThai == TrangThaiTinTuyenDung.datuyen)).OrderByDescending(t => t.NgayTao).Select(t => new { t.Id, t.TieuDe }).ToListAsync(), "Id", "TieuDe", model.SelectedTinTuyenDungId); model.TrangThaiOptions = new SelectList(EnumExtensions.GetSelectList<TrangThaiUngTuyen>(includeDefaultItem: true, defaultItemText: "Tất cả trạng thái", defaultItemValue: ""), "Value", "Text", model.FilterByTrangThai?.ToString()); model.SortOptions = new SelectList(new List<SelectListItem> { new SelectListItem { Value = "ngaynop_desc", Text = "Ngày nộp (Mới nhất)" }, new SelectListItem { Value = "ngaynop_asc", Text = "Ngày nộp (Cũ nhất)" }, new SelectListItem { Value = "ten_asc", Text = "Tên ứng viên (A-Z)" }, new SelectListItem { Value = "ten_desc", Text = "Tên ứng viên (Z-A)" } }, "Value", "Text", model.SortBy); var allThanhPhos = await _context.ThanhPhos.OrderBy(tp => tp.Ten).Select(tp => new SelectListItem { Value = tp.Id.ToString(), Text = tp.Ten }).ToListAsync(); model.KhuVucMongMuonOptions = new SelectList(allThanhPhos, "Value", "Text"); }

        [HttpPost("thay-doi-trang-thai")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThayDoiTrangThai(int ungTuyenId, TrangThaiUngTuyen newStatus)
        {
            int employerId = GetCurrentEmployerId();
            var ungTuyen = await _context.UngTuyens
                                    .Include(ut => ut.TinTuyenDung) // Cần để lấy NguoiDangId, TieuDeTin
                                    .Include(ut => ut.UngVien)      // Cần UngVienId để gửi thông báo
                                    .FirstOrDefaultAsync(ut => ut.Id == ungTuyenId);

            if (ungTuyen == null) return NotFound(new { success = false, message = "Không tìm thấy đơn ứng tuyển." });
            if (ungTuyen.TinTuyenDung.NguoiDangId != employerId) { _logger.LogWarning("Forbidden: Employer {EmployerId} attempted to change status for UngTuyenId {UngTuyenId} not belonging to them.", employerId, ungTuyenId); return Forbid("Bạn không có quyền thay đổi trạng thái đơn ứng tuyển này."); }
            if (ungTuyen.TrangThai == TrangThaiUngTuyen.darut && newStatus != TrangThaiUngTuyen.darut) return BadRequest(new { success = false, message = "Ứng viên đã rút đơn, không thể thay đổi trạng thái." });
            if (ungTuyen.TrangThai == newStatus) return Ok(new { success = true, message = "Trạng thái ứng tuyển không thay đổi.", newStatusDisplay = newStatus.GetDisplayName(), newBadgeClass = GetUngVienTrangThaiBadgeClass(newStatus) });

            var oldStatus = ungTuyen.TrangThai; // Lưu lại trạng thái cũ để so sánh
            ungTuyen.TrangThai = newStatus;
            ungTuyen.NgayCapNhatTrangThai = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Nhà tuyển dụng {EmployerId} đã thay đổi trạng thái UngTuyen ID {UngTuyenId} từ {OldStatus} thành {NewStatus}", employerId, ungTuyenId, oldStatus, newStatus);

                // --- BẮT ĐẦU GỬI THÔNG BÁO CHO ỨNG VIÊN ---
                string? notificationType = null;
                string notificationMessage = "";

                if (newStatus == TrangThaiUngTuyen.daduyet) // NTD chấp nhận
                {
                    notificationType = NotificationConstants.Types.UngTuyenNtdChapNhan;
                    notificationMessage = $"Hồ sơ ứng tuyển của bạn cho vị trí \"{ungTuyen.TinTuyenDung.TieuDe}\" đã được nhà tuyển dụng chấp thuận. Chúc mừng!";
                }
                else if (newStatus == TrangThaiUngTuyen.bituchoi) // NTD từ chối
                {
                    notificationType = NotificationConstants.Types.UngTuyenNtdTuChoi;
                    notificationMessage = $"Rất tiếc, hồ sơ ứng tuyển của bạn cho vị trí \"{ungTuyen.TinTuyenDung.TieuDe}\" chưa phù hợp với yêu cầu của nhà tuyển dụng.";
                } else if (newStatus == TrangThaiUngTuyen.ntddaxem && oldStatus != TrangThaiUngTuyen.ntddaxem){
                     notificationType = NotificationConstants.Types.UngTuyenNtdXem;
                    var ntdInfoForViewed = await _context.NguoiDungs.Include(n => n.HoSoDoanhNghiep).FirstOrDefaultAsync(n => n.Id == employerId);
                    string tenNtdDisplayForViewed = ntdInfoForViewed?.HoSoDoanhNghiep?.TenCongTy ?? ntdInfoForViewed?.HoTen ?? "Nhà tuyển dụng";
                    notificationMessage = $"Nhà tuyển dụng {tenNtdDisplayForViewed} đã xem xét hồ sơ ứng tuyển của bạn cho vị trí \"{ungTuyen.TinTuyenDung.TieuDe}\".";
                }
                // Bạn có thể thêm các trường hợp khác nếu muốn thông báo (ví dụ: NTD đánh dấu "đã xem" - nhưng việc này thường xử lý ở ChiTietHoSo)

                if (notificationType != null && ungTuyen.UngVienId != 0)
                {
                    var ntdInfo = await _context.NguoiDungs.Include(n => n.HoSoDoanhNghiep).FirstOrDefaultAsync(n => n.Id == employerId);
                    string tenNtdDisplay = ntdInfo?.HoSoDoanhNghiep?.TenCongTy ?? ntdInfo?.HoTen ?? "Nhà tuyển dụng";

                    var duLieuThongBao = new
                    {
                        tenNhaTuyenDung = tenNtdDisplay,
                        tieuDeTin = ungTuyen.TinTuyenDung.TieuDe,
                        ungTuyenId = ungTuyen.Id,
                        tinId = ungTuyen.TinTuyenDungId,
                        noiDung = notificationMessage,
                        // URL để ứng viên xem chi tiết đơn ứng tuyển của họ
                        url = Url.Action("ChiTietDonUngTuyen", "HoSoCaNhan", new { area = "UngVien", id = ungTuyen.Id }, Request.Scheme)
                        // Thay "ChiTietDonUngTuyen", "HoSoCaNhan", "UngVien" bằng cấu trúc thực tế của bạn
                    };

                    try
                    {
                        await _thongBaoService.CreateThongBaoAsync(
                            ungTuyen.UngVienId,
                            notificationType,
                            JsonSerializer.Serialize(duLieuThongBao),
                            NotificationConstants.RelatedEntities.UngTuyen,
                            ungTuyen.Id
                        );
                        _logger.LogInformation("Đã gửi thông báo '{NotificationType}' cho UngVien ID {UngVienId} về UngTuyen ID {UngTuyenId}.", notificationType, ungTuyen.UngVienId, ungTuyen.Id);
                    }
                    catch (Exception ex_notify)
                    {
                        _logger.LogError(ex_notify, "Lỗi gửi thông báo '{NotificationType}' cho UngVien ID {UngVienId}. UngTuyenID: {UngTuyenId}", notificationType, ungTuyen.UngVienId, ungTuyen.Id);
                    }
                }
                // --- KẾT THÚC GỬI THÔNG BÁO ---

                return Ok(new { success = true, message = $"Cập nhật trạng thái thành '{newStatus.GetDisplayName()}' thành công.", newStatusDisplay = newStatus.GetDisplayName(), newBadgeClass = GetUngVienTrangThaiBadgeClass(newStatus) });
            }
            catch (DbUpdateException dbEx) { _logger.LogError(dbEx, "Lỗi DbUpdateException khi thay đổi trạng thái UngTuyen ID {UngTuyenId} cho nhà tuyển dụng {EmployerId}", ungTuyenId, employerId); return StatusCode(500, new { success = false, message = "Lỗi cơ sở dữ liệu khi cập nhật trạng thái." }); }
            catch (Exception ex) { _logger.LogError(ex, "Lỗi Exception khi thay đổi trạng thái UngTuyen ID {UngTuyenId} cho nhà tuyển dụng {EmployerId}", ungTuyenId, employerId); return StatusCode(500, new { success = false, message = "Lỗi máy chủ không xác định khi cập nhật trạng thái." }); }
        }

        [HttpGet("ChiTietHoSo/{ungVienId}")] // Giữ nguyên route, ungVienId là ID của NguoiDung
        public async Task<IActionResult> ChiTietHoSo(int ungVienId, int? ungTuyenId) // ungTuyenId từ query string để cung cấp context
        {
            int employerId = GetCurrentEmployerId();
            var ungVien = await _context.NguoiDungs.Include(u => u.ThanhPho).Include(u => u.QuanHuyen).Include(u => u.HoSoUngVien).Include(u => u.LichRanhUngViens).Include(u => u.DiaDiemMongMuons).ThenInclude(ddm => ddm.ThanhPho).AsNoTracking().FirstOrDefaultAsync(u => u.Id == ungVienId);
            if (ungVien == null) return NotFound("Không tìm thấy ứng viên.");
            
            bool canView = false;
            UngTuyen? ungTuyenContextEntity = null; // Entity này sẽ được theo dõi nếu cần cập nhật trạng thái

            if (ungTuyenId.HasValue)
            {
                // <<<<<<< QUAN TRỌNG: Load UngTuyen để cập nhật và gửi thông báo >>>>>>>
                ungTuyenContextEntity = await _context.UngTuyens
                                            .Include(ut => ut.TinTuyenDung) // Cần TieuDeTin
                                            .FirstOrDefaultAsync(ut => ut.Id == ungTuyenId.Value && ut.UngVienId == ungVienId && ut.TinTuyenDung.NguoiDangId == employerId);
                if (ungTuyenContextEntity != null) canView = true;
            }
            else { canView = await _context.UngTuyens.AnyAsync(ut => ut.UngVienId == ungVienId && ut.TinTuyenDung.NguoiDangId == employerId); }

            if (!canView) { _logger.LogWarning("Forbidden: Employer {EmployerId} attempted to view profile of candidate {UngVienId} (ungTuyenId: {UngTuyenIdContext}) without valid context.", employerId, ungVienId, ungTuyenId); TempData["ErrorMessage"] = "Bạn không có quyền xem hồ sơ này hoặc ứng viên này chưa ứng tuyển vào tin tuyển dụng nào của bạn."; return RedirectToAction(nameof(Index)); }

            var diaChiParts = new List<string?>(); if (!string.IsNullOrWhiteSpace(ungVien.DiaChiChiTiet)) diaChiParts.Add(ungVien.DiaChiChiTiet); if (ungVien.QuanHuyen != null && !string.IsNullOrWhiteSpace(ungVien.QuanHuyen.Ten)) diaChiParts.Add(ungVien.QuanHuyen.Ten); if (ungVien.ThanhPho != null && !string.IsNullOrWhiteSpace(ungVien.ThanhPho.Ten)) diaChiParts.Add(ungVien.ThanhPho.Ten); string diaChiDayDu = string.Join(", ", diaChiParts.Where(s => !string.IsNullOrWhiteSpace(s)));
            var viewModel = new ChiTietHoSoUngVienViewModel { UngVienId = ungVien.Id, UngTuyenIdContext = ungTuyenId, HoTen = ungVien.HoTen, Email = ungVien.Email, SoDienThoai = ungVien.Sdt, AvatarUrl = ungVien.UrlAvatar ?? "/images/avatars/default_user.png", GioiTinh = ungVien.GioiTinh?.GetDisplayName(), NgaySinh = ungVien.NgaySinh, DiaChiDayDu = diaChiDayDu, TieuDeHoSo = ungVien.HoSoUngVien?.TieuDeHoSo, GioiThieuBanThan = ungVien.HoSoUngVien?.GioiThieuBanThan, ViTriMongMuon = ungVien.HoSoUngVien?.ViTriMongMuon, LoaiLuongMongMuonDisplay = ungVien.HoSoUngVien?.LoaiLuongMongMuon?.GetDisplayName(), MucLuongMongMuonDisplay = ungVien.HoSoUngVien?.MucLuongMongMuon?.ToString("N0") + (ungVien.HoSoUngVien?.LoaiLuongMongMuon.HasValue == true ? GetLuongSuffix(ungVien.HoSoUngVien.LoaiLuongMongMuon.Value) : ""), UrlCvMacDinh = ungVien.HoSoUngVien?.UrlCvMacDinh, LichRanhs = ungVien.LichRanhUngViens.Select(lr => new LichRanhDisplayViewModel { NgayTrongTuan = lr.NgayTrongTuan.GetDisplayName(), BuoiLamViec = lr.BuoiLamViec.GetDisplayName() }).ToList(), DiaDiemMongMuonsDisplay = ungVien.DiaDiemMongMuons.Where(ddm => ddm.ThanhPho != null).Select(ddm => ddm.ThanhPho!.Ten).Distinct().ToList() };

            if (ungTuyenContextEntity != null)
            {
                viewModel.UrlCvDaNopChoTinNay = ungTuyenContextEntity.UrlCvDaNop; viewModel.ThuGioiThieuChoTinNay = ungTuyenContextEntity.ThuGioiThieu;
                viewModel.NgayNopUngTuyen = ungTuyenContextEntity.NgayNop; viewModel.TrangThaiUngTuyenHienTai = ungTuyenContextEntity.TrangThai;
                viewModel.TenTinTuyenDungUngTuyen = ungTuyenContextEntity.TinTuyenDung?.TieuDe;

                if (ungTuyenContextEntity.TrangThai == TrangThaiUngTuyen.danop)
                {
                    ungTuyenContextEntity.TrangThai = TrangThaiUngTuyen.ntddaxem;
                    ungTuyenContextEntity.NgayCapNhatTrangThai = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Lưu thay đổi trạng thái
                    _logger.LogInformation("Employer {EmployerId} viewed UngTuyen ID {UngTuyenId}. Status auto-changed to ntddaxem.", employerId, ungTuyenId);
                    viewModel.TrangThaiUngTuyenHienTai = TrangThaiUngTuyen.ntddaxem; // Cập nhật ViewModel

                    // --- BẮT ĐẦU GỬI THÔNG BÁO "NTD ĐÃ XEM" CHO ỨNG VIÊN ---
                    var ntdInfo = await _context.NguoiDungs.Include(n => n.HoSoDoanhNghiep).FirstOrDefaultAsync(n => n.Id == employerId);
                    string tenNtdDisplay = ntdInfo?.HoSoDoanhNghiep?.TenCongTy ?? ntdInfo?.HoTen ?? "Nhà tuyển dụng";

                    var duLieuThongBao = new
                    {
                        tenNhaTuyenDung = tenNtdDisplay,
                        tieuDeTin = ungTuyenContextEntity.TinTuyenDung?.TieuDe ?? string.Empty,
                        ungTuyenId = ungTuyenContextEntity.Id,
                        tinId = ungTuyenContextEntity.TinTuyenDungId,
                        noiDung = $"Nhà tuyển dụng {tenNtdDisplay} đã xem hồ sơ ứng tuyển của bạn cho vị trí \"{ungTuyenContextEntity.TinTuyenDung?.TieuDe ?? string.Empty}\".",
                         url = Url.Action(
        "ChiTiet",  // Tên Action trong TimViecController
        "TimViec",  // Tên Controller
        new 
        { 
            id = ungTuyenContextEntity.TinTuyenDungId, 
            // Tạo slug từ tiêu đề để có URL thân thiện, giống hệt cách bạn làm ở nơi khác
            tieuDeSeo = SeoUrlHelper.GenerateSlug(ungTuyenContextEntity.TinTuyenDung?.TieuDe ?? string.Empty) 
        }, 
        Request.Scheme)
                        // Thay "ChiTiet", "TimViec" bằng cấu trúc thực tế của bạn
                    };
                    try
                    {
                        await _thongBaoService.CreateThongBaoAsync(
                            ungVienId, // ID của ứng viên
                            NotificationConstants.Types.UngTuyenNtdXem,
                            JsonSerializer.Serialize(duLieuThongBao),
                            NotificationConstants.RelatedEntities.UngTuyen,
                            ungTuyenContextEntity.Id
                        );
                         _logger.LogInformation("Đã gửi thông báo 'NTD ĐÃ XEM' cho UngVien ID {UngVienId} về UngTuyen ID {UngTuyenId}.", ungVienId, ungTuyenContextEntity.Id);
                    }
                    catch (Exception ex_notify)
                    {
                         _logger.LogError(ex_notify, "Lỗi gửi thông báo 'NTD ĐÃ XEM' cho UngVien ID {UngVienId}. UngTuyenID: {UngTuyenId}", ungVienId, ungTuyenContextEntity.Id);
                    }
                    // --- KẾT THÚC GỬI THÔNG BÁO ---
                }
            }
            else if (ungTuyenId.HasValue) { _logger.LogWarning("ChiTietHoSo: ungTuyenId {UngTuyenId} was provided, but no matching UngTuyen entity found for employer {EmployerId} and candidate {UngVienId}", ungTuyenId, employerId, ungVienId); }
            
            return View(viewModel);
        }
        private string GetLuongSuffix(LoaiLuong loaiLuong) => loaiLuong switch { LoaiLuong.theogio => "/giờ", LoaiLuong.theongay => "/ngày", LoaiLuong.theoca => "/ca", LoaiLuong.theothang => "/tháng", LoaiLuong.theoduan => "/dự án", _ => "" };
    }
}