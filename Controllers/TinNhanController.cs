// File: Controllers/TinNhanController.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TinNhan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HeThongTimViec.Utils;
using Microsoft.Extensions.Logging;
using HeThongTimViec.Services;
using System.Text.Json;

namespace HeThongTimViec.Controllers
{
    [Authorize]
    public class TinNhanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TinNhanController> _logger;
        private readonly IThongBaoService _thongBaoService;

        public TinNhanController(ApplicationDbContext context, ILogger<TinNhanController> logger, IThongBaoService thongBaoService)
        {
            _context = context;
            _logger = logger;
            _thongBaoService = thongBaoService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) return userId;
            _logger.LogError("Không thể lấy hoặc phân tích User ID từ Claims. UserIdentifier Claim: {ClaimValue}", userIdClaim);
            throw new UnauthorizedAccessException("User ID không tìm thấy hoặc không hợp lệ.");
        }

        private (string DisplayName, string AvatarUrl, LoaiTaiKhoan AccountType) GetUserChatDisplayInfo(NguoiDung user)
        {
            if (user == null) return ("Người dùng không xác định", "/images/default-avatar.png", LoaiTaiKhoan.canhan);
            if (user.LoaiTk == LoaiTaiKhoan.doanhnghiep && user.HoSoDoanhNghiep != null)
            {
                return (user.HoSoDoanhNghiep.TenCongTy, user.HoSoDoanhNghiep.UrlLogo ?? "/images/default-company-logo.png", LoaiTaiKhoan.doanhnghiep);
            }
            return (user.HoTen, user.UrlAvatar ?? "/images/default-avatar.png", user.LoaiTk);
        }

        public async Task<IActionResult> Index(
            int? nguoiLienHeId, int? ungTuyenId, int? tinTuyenDungId, string? searchTerm,
            DateTime? filterDateFrom, DateTime? filterDateTo, string? filterReadStatus, int? selectedJobContextId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.NguoiDungs
                                    .Include(u => u.HoSoDoanhNghiep)
                                    .Include(u => u.HoSoUngVien)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null) return Challenge();

            var currentUserDisplayInfo = GetUserChatDisplayInfo(currentUser);
            var viewModel = new QuanLyTinNhanViewModel
            {
                SearchTerm = searchTerm, CurrentUserDisplayName = currentUserDisplayInfo.DisplayName, CurrentUserAvatarUrl = currentUserDisplayInfo.AvatarUrl,
                FilterDateFrom = filterDateFrom, FilterDateTo = filterDateTo, FilterReadStatus = filterReadStatus, SelectedJobContextId = selectedJobContextId
            };

            if (currentUser.LoaiTk == LoaiTaiKhoan.doanhnghiep)
            {
                viewModel.JobContextOptions = await _context.TinTuyenDungs
                    .Where(ttd => ttd.NguoiDangId == currentUserId).OrderBy(ttd => ttd.TieuDe)
                    .Select(ttd => new SelectListItem { Value = ttd.Id.ToString(), Text = ttd.TieuDe }).ToListAsync();
                viewModel.JobContextOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Lọc theo tin tuyển dụng --" });
            }
            else if (currentUser.LoaiTk == LoaiTaiKhoan.canhan)
            {
                viewModel.JobContextOptions = await _context.UngTuyens
                    .Where(ut => ut.UngVienId == currentUserId && ut.TinTuyenDung != null).Include(ut => ut.TinTuyenDung).OrderBy(ut => ut.TinTuyenDung!.TieuDe)
                    .Select(ut => new SelectListItem { Value = ut.Id.ToString(), Text = "Đã ứng tuyển: " + ut.TinTuyenDung!.TieuDe }).Distinct().ToListAsync();
                viewModel.JobContextOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Lọc theo đơn ứng tuyển --" });
            }

            var allUserMessages = await _context.TinNhans
                .Where(tn => tn.NguoiGuiId == currentUserId || tn.NguoiNhanId == currentUserId)
                .Include(tn => tn.NguoiGui).ThenInclude(ng => ng.HoSoDoanhNghiep)
                .Include(tn => tn.NguoiNhan).ThenInclude(nn => nn.HoSoDoanhNghiep)
                .Include(tn => tn.TinLienQuan)
                .Include(tn => tn.UngTuyenLienQuan).ThenInclude(ut => ut!.TinTuyenDung)
                .OrderByDescending(tn => tn.NgayGui).AsNoTracking().ToListAsync();

            var conversationsFromExistingMessages = allUserMessages
                .GroupBy(tn => {
                    var otherUserIdInConv = tn.NguoiGuiId == currentUserId ? tn.NguoiNhanId : tn.NguoiGuiId;
                    int? contextTinId = tn.UngTuyenLienQuan?.TinTuyenDungId ?? tn.TinLienQuanId;
                    int? contextUngTuyenId = tn.UngTuyenLienQuanId;
                    return new { OtherUserId = otherUserIdInConv, UngTuyenContextId = contextUngTuyenId, TinTuyenDungContextId = contextTinId };
                })
                .Select(g => {
                    var lastMessage = g.First();
                    var otherUserEntity = lastMessage.NguoiGuiId == currentUserId ? lastMessage.NguoiNhan : lastMessage.NguoiGui;
                    var otherUserDisplayInfo = GetUserChatDisplayInfo(otherUserEntity);
                    var contextJobTitle = lastMessage.UngTuyenLienQuan?.TinTuyenDung?.TieuDe ?? lastMessage.TinLienQuan?.TieuDe;
                    return new HoiThoaiViewModel {
                        NguoiLienHeId = g.Key.OtherUserId, TenNguoiLienHe = otherUserDisplayInfo.DisplayName, AvatarUrlNguoiLienHe = otherUserDisplayInfo.AvatarUrl,
                        TinNhanCuoiCung = lastMessage.NoiDung.Length > 35 ? lastMessage.NoiDung.Substring(0, 35) + "..." : lastMessage.NoiDung,
                        NgayGuiTinNhanCuoiCung = lastMessage.NgayGui, ThoiGianGuiTinNhanCuoiCungDisplay = TimeHelper.TimeAgo(TimeHelper.ConvertToVietnamTimeFromUtc(lastMessage.NgayGui)),
                        LaTinNhanCuoiCuaToi = lastMessage.NguoiGuiId == currentUserId, SoTinNhanChuaDoc = g.Count(m => m.NguoiNhanId == currentUserId && m.NgayDoc == null),
                        UngTuyenLienQuanId = g.Key.UngTuyenContextId, TinTuyenDungLienQuanId = g.Key.TinTuyenDungContextId, TieuDeCongViecLienQuan = contextJobTitle, IsActive = false
                    };
                }).ToList();

            List<HoiThoaiViewModel> finalConversationList = new List<HoiThoaiViewModel>(conversationsFromExistingMessages);
            bool isChatAreaExplicitlyActivatedByUrl = false;

            int? actualTinTuyenDungIdFromUrlContext = tinTuyenDungId;
            if (nguoiLienHeId.HasValue && nguoiLienHeId.Value != currentUserId)
            {
                isChatAreaExplicitlyActivatedByUrl = true;
                viewModel.IsChatAreaActive = true;

                if (ungTuyenId.HasValue) {
                    actualTinTuyenDungIdFromUrlContext = await _context.UngTuyens.Where(ut => ut.Id == ungTuyenId.Value).AsNoTracking().Select(ut => ut.TinTuyenDungId).FirstOrDefaultAsync();
                }

                var exactContextMatchInExisting = conversationsFromExistingMessages.FirstOrDefault(h =>
                    h.NguoiLienHeId == nguoiLienHeId.Value &&
                    (ungTuyenId.HasValue ? h.UngTuyenLienQuanId == ungTuyenId.Value : !h.UngTuyenLienQuanId.HasValue) &&
                    (actualTinTuyenDungIdFromUrlContext.HasValue ? h.TinTuyenDungLienQuanId == actualTinTuyenDungIdFromUrlContext.Value : !h.TinTuyenDungLienQuanId.HasValue)
                );

                if (exactContextMatchInExisting != null) {
                    exactContextMatchInExisting.IsActive = true;
                } else {
                    bool anyMessagesExistWithThisContactOverall = allUserMessages.Any(tn => (tn.NguoiGuiId == currentUserId && tn.NguoiNhanId == nguoiLienHeId.Value) || (tn.NguoiGuiId == nguoiLienHeId.Value && tn.NguoiNhanId == currentUserId));
                    if (!anyMessagesExistWithThisContactOverall) {
                        var potentialContactUser = await _context.NguoiDungs.Include(u => u.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(u => u.Id == nguoiLienHeId.Value);
                        if (potentialContactUser != null) {
                            var potentialContactDisplayInfo = GetUserChatDisplayInfo(potentialContactUser);
                            string? potentialJobTitle = null;
                            if (ungTuyenId.HasValue) {
                                var ut = await _context.UngTuyens.Include(u => u.TinTuyenDung).AsNoTracking().FirstOrDefaultAsync(u => u.Id == ungTuyenId.Value);
                                potentialJobTitle = ut?.TinTuyenDung?.TieuDe;
                            } else if (actualTinTuyenDungIdFromUrlContext.HasValue) {
                                var ttd = await _context.TinTuyenDungs.AsNoTracking().FirstOrDefaultAsync(t => t.Id == actualTinTuyenDungIdFromUrlContext.Value);
                                potentialJobTitle = ttd?.TieuDe;
                            }
                            var newPotentialConversation = new HoiThoaiViewModel {
                                NguoiLienHeId = nguoiLienHeId.Value, TenNguoiLienHe = potentialContactDisplayInfo.DisplayName, AvatarUrlNguoiLienHe = potentialContactDisplayInfo.AvatarUrl,
                                TinNhanCuoiCung = "", NgayGuiTinNhanCuoiCung = DateTime.MinValue, ThoiGianGuiTinNhanCuoiCungDisplay = "Bắt đầu chat",
                                LaTinNhanCuoiCuaToi = false, SoTinNhanChuaDoc = 0,
                                UngTuyenLienQuanId = ungTuyenId, TinTuyenDungLienQuanId = actualTinTuyenDungIdFromUrlContext, TieuDeCongViecLienQuan = potentialJobTitle, IsActive = true
                            };
                            if (!finalConversationList.Any(fc => fc.NguoiLienHeId == newPotentialConversation.NguoiLienHeId && fc.UngTuyenLienQuanId == newPotentialConversation.UngTuyenLienQuanId && fc.TinTuyenDungLienQuanId == newPotentialConversation.TinTuyenDungLienQuanId)) {
                                finalConversationList.Insert(0, newPotentialConversation);
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(searchTerm)) {
                var searchTermLower = searchTerm.ToLower();
                finalConversationList = finalConversationList.Where(h => (h.TenNguoiLienHe != null && h.TenNguoiLienHe.ToLower().Contains(searchTermLower)) || (h.TieuDeCongViecLienQuan != null && h.TieuDeCongViecLienQuan.ToLower().Contains(searchTermLower)) || (h.TinNhanCuoiCung != null && h.TinNhanCuoiCung.ToLower().Contains(searchTermLower))).ToList();
            }
            if (filterDateFrom.HasValue) { finalConversationList = finalConversationList.Where(h => h.NgayGuiTinNhanCuoiCung != DateTime.MinValue && h.NgayGuiTinNhanCuoiCung.Date >= filterDateFrom.Value.Date).ToList(); }
            if (filterDateTo.HasValue) { finalConversationList = finalConversationList.Where(h => h.NgayGuiTinNhanCuoiCung != DateTime.MinValue && h.NgayGuiTinNhanCuoiCung.Date <= filterDateTo.Value.Date).ToList(); }
            if (!string.IsNullOrEmpty(filterReadStatus) && filterReadStatus != "all") {
                if (filterReadStatus == "unread") finalConversationList = finalConversationList.Where(h => h.SoTinNhanChuaDoc > 0).ToList();
                else if (filterReadStatus == "read") finalConversationList = finalConversationList.Where(h => h.NgayGuiTinNhanCuoiCung != DateTime.MinValue && h.SoTinNhanChuaDoc == 0).ToList();
            }
            if (selectedJobContextId.HasValue) {
                if (currentUser.LoaiTk == LoaiTaiKhoan.canhan) { finalConversationList = finalConversationList.Where(h => h.UngTuyenLienQuanId == selectedJobContextId.Value).ToList(); }
                else if (currentUser.LoaiTk == LoaiTaiKhoan.doanhnghiep) { finalConversationList = finalConversationList.Where(h => h.TinTuyenDungLienQuanId == selectedJobContextId.Value).ToList(); }
            }

            viewModel.DanhSachHoiThoai = finalConversationList.OrderByDescending(h => h.IsActive).ThenByDescending(h => h.NgayGuiTinNhanCuoiCung).ToList();

            var activeConversationItemInFinalList = viewModel.DanhSachHoiThoai.FirstOrDefault(h => h.IsActive);
            if (activeConversationItemInFinalList != null) {
                await LoadMessagesForConversation(viewModel, currentUserId, currentUser, activeConversationItemInFinalList.NguoiLienHeId, activeConversationItemInFinalList.UngTuyenLienQuanId, activeConversationItemInFinalList.TinTuyenDungLienQuanId);
            } else if (isChatAreaExplicitlyActivatedByUrl && nguoiLienHeId.HasValue) {
                 int? fallbackTinTuyenDungId = tinTuyenDungId; // Giá trị ban đầu từ URL
                 if (ungTuyenId.HasValue && !actualTinTuyenDungIdFromUrlContext.HasValue) {
                    // Nếu có ungTuyenId nhưng không tìm thấy TTDId từ đó, thử lại query
                    var tinTuyenDungIdFromUngTuyen = await _context.UngTuyens
                        .Where(ut => ut.Id == ungTuyenId.Value)
                        .AsNoTracking()
                        .Select(ut => (int?)ut.TinTuyenDungId)
                        .FirstOrDefaultAsync();
                    fallbackTinTuyenDungId = tinTuyenDungIdFromUngTuyen ?? tinTuyenDungId;
                }
                else if (ungTuyenId.HasValue)
                {
                    fallbackTinTuyenDungId = actualTinTuyenDungIdFromUrlContext;
                 }
                await LoadMessagesForConversation(viewModel, currentUserId, currentUser, nguoiLienHeId.Value, ungTuyenId, fallbackTinTuyenDungId);
            }
            return View(viewModel);
        }

        private async Task LoadMessagesForConversation(QuanLyTinNhanViewModel viewModel, int currentUserId, NguoiDung currentUser, int contactId, int? ungTuyenIdContext, int? tinTuyenDungIdContext)
        {
            var otherUserEntity = await _context.NguoiDungs.Include(u => u.HoSoDoanhNghiep).Include(u => u.HoSoUngVien).AsNoTracking().FirstOrDefaultAsync(u => u.Id == contactId);
            if (otherUserEntity == null) {
                viewModel.ErrorMessage = "Không tìm thấy người liên hệ."; viewModel.IsChatAreaActive = false; return;
            }

            var otherUserDisplayInfo = GetUserChatDisplayInfo(otherUserEntity);
            viewModel.NguoiLienHeIdHienTai = otherUserEntity.Id; viewModel.TenNguoiLienHeHienTai = otherUserDisplayInfo.DisplayName; viewModel.AvatarUrlNguoiLienHeHienTai = otherUserDisplayInfo.AvatarUrl;

            int? finalTinTuyenDungIdContextForNewMessage = tinTuyenDungIdContext;
            if (ungTuyenIdContext.HasValue) {
                var ungTuyen = await _context.UngTuyens.Include(u => u.TinTuyenDung).AsNoTracking().FirstOrDefaultAsync(u => u.Id == ungTuyenIdContext.Value);
                if (ungTuyen?.TinTuyenDung != null) {
                    viewModel.TieuDeCongViecLienQuanHienTai = ungTuyen.TinTuyenDung.TieuDe; viewModel.UrlChiTietCongViecLienQuanHienTai = Url.Action("ChiTiet", "TimViec", new { id = ungTuyen.TinTuyenDungId });
                    finalTinTuyenDungIdContextForNewMessage = ungTuyen.TinTuyenDungId;
                }
                viewModel.UngTuyenIdChoTinNhanMoi = ungTuyenIdContext; viewModel.TinTuyenDungIdChoTinNhanMoi = finalTinTuyenDungIdContextForNewMessage;
            } else if (tinTuyenDungIdContext.HasValue) {
                var tinTD = await _context.TinTuyenDungs.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tinTuyenDungIdContext.Value);
                if (tinTD != null) {
                    viewModel.TieuDeCongViecLienQuanHienTai = tinTD.TieuDe; viewModel.UrlChiTietCongViecLienQuanHienTai = Url.Action("ChiTiet", "TimViec", new { id = tinTD.Id });
                }
                viewModel.UngTuyenIdChoTinNhanMoi = null; viewModel.TinTuyenDungIdChoTinNhanMoi = tinTuyenDungIdContext;
            } else {
                viewModel.UngTuyenIdChoTinNhanMoi = null; viewModel.TinTuyenDungIdChoTinNhanMoi = null; viewModel.TieuDeCongViecLienQuanHienTai = null; viewModel.UrlChiTietCongViecLienQuanHienTai = null;
            }

            var messagesQuery = _context.TinNhans.Where(tn => (tn.NguoiGuiId == currentUserId && tn.NguoiNhanId == contactId) || (tn.NguoiGuiId == contactId && tn.NguoiNhanId == currentUserId)).Include(tn => tn.NguoiGui).ThenInclude(ng => ng.HoSoDoanhNghiep).AsNoTracking();
            if (ungTuyenIdContext.HasValue) {
                messagesQuery = messagesQuery.Where(tn => tn.UngTuyenLienQuanId == ungTuyenIdContext.Value);
            } else if (finalTinTuyenDungIdContextForNewMessage.HasValue) {
                messagesQuery = messagesQuery.Where(tn => tn.TinLienQuanId == finalTinTuyenDungIdContextForNewMessage.Value && tn.UngTuyenLienQuanId == null);
            } else {
                messagesQuery = messagesQuery.Where(tn => tn.UngTuyenLienQuanId == null && tn.TinLienQuanId == null);
            }

            var messagesInContext = await messagesQuery.OrderBy(tn => tn.NgayGui).ToListAsync();
            var currentUserChatDisplayInfo = GetUserChatDisplayInfo(currentUser);
            viewModel.TinNhanTrongHoiThoaiHienTai = messagesInContext.Select(tn => {
                var nguoiGuiDisplayInfo = (tn.NguoiGuiId == currentUserId) ? currentUserChatDisplayInfo : GetUserChatDisplayInfo(tn.NguoiGui);
                return new TinNhanItemViewModel {
                    Id = tn.Id, NguoiGuiId = tn.NguoiGuiId, TenNguoiGui = nguoiGuiDisplayInfo.DisplayName, AvatarUrlNguoiGui = nguoiGuiDisplayInfo.AvatarUrl,
                    NoiDung = tn.NoiDung, NgayGui = tn.NgayGui, ThoiGianGuiDisplay = TimeHelper.FormatDateTime(TimeHelper.ConvertToVietnamTimeFromUtc(tn.NgayGui)),
                    LaCuaToi = tn.NguoiGuiId == currentUserId, DaDoc = tn.NgayDoc.HasValue,
                };
            }).ToList();

            var unreadMessageIds = messagesInContext.Where(tn => tn.NguoiNhanId == currentUserId && tn.NgayDoc == null).Select(tn => tn.Id).ToList();
            if (unreadMessageIds.Any()) {
                await _context.TinNhans.Where(tn => unreadMessageIds.Contains(tn.Id)).ExecuteUpdateAsync(setters => setters.SetProperty(m => m.NgayDoc, DateTime.UtcNow));
            }
        }

        [HttpGet("api/tinnhan/loadhoithoai")]
        public async Task<IActionResult> LoadHoiThoaiPartial(int nguoiLienHeId, int? ungTuyenId, int? tinTuyenDungId) {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.NguoiDungs.Include(u => u.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null) return Unauthorized("Current user not found.");

            var viewModel = new QuanLyTinNhanViewModel();
            await LoadMessagesForConversation(viewModel, currentUserId, currentUser, nguoiLienHeId, ungTuyenId, tinTuyenDungId);
            if (!string.IsNullOrEmpty(viewModel.ErrorMessage)) return PartialView("_ChatMessagesArea", viewModel); // Hiển thị lỗi nếu có
            if (viewModel.NguoiLienHeIdHienTai == null) return NotFound("Không tìm thấy thông tin hội thoại.");
            return PartialView("_ChatMessagesArea", viewModel);
        }

        [HttpPost("api/tinnhan/guitinnhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiTinNhanAjax([FromBody] GuiTinNhanAjaxModel model) {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.NguoiDungs.Include(u => u.HoSoDoanhNghiep).AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null) return Unauthorized(new { success = false, message = "Người dùng không hợp lệ." });
            if (string.IsNullOrWhiteSpace(model.NoiDung) || model.NguoiNhanId <= 0) return BadRequest(new { success = false, message = "Nội dung hoặc người nhận không hợp lệ." });
            if (model.NguoiNhanId == currentUserId) return BadRequest(new { success = false, message = "Bạn không thể gửi tin nhắn cho chính mình." });
            if (!await _context.NguoiDungs.AnyAsync(u => u.Id == model.NguoiNhanId)) return BadRequest(new { success = false, message = "Người nhận không tồn tại." });

            int? finalTinLienQuanId = model.TinLienQuanId;
            if (model.UngTuyenLienQuanId.HasValue) {
                var ungTuyen = await _context.UngTuyens.AsNoTracking().FirstOrDefaultAsync(ut => ut.Id == model.UngTuyenLienQuanId.Value);
                if (ungTuyen != null) finalTinLienQuanId = ungTuyen.TinTuyenDungId;
            }

            var tinNhan = new TinNhan {
                NguoiGuiId = currentUserId, NguoiNhanId = model.NguoiNhanId, NoiDung = model.NoiDung.Trim(), NgayGui = DateTime.UtcNow,
                UngTuyenLienQuanId = model.UngTuyenLienQuanId, TinLienQuanId = finalTinLienQuanId
            };
            _context.TinNhans.Add(tinNhan); await _context.SaveChangesAsync();

            try {
                var nguoiGuiDisplayInfo = GetUserChatDisplayInfo(currentUser);
                var noiDungRutGon = model.NoiDung.Length > 50 ? model.NoiDung.Substring(0, 50) + "..." : model.NoiDung;
                var duLieuThongBao = new {
                    tenNguoiGui = nguoiGuiDisplayInfo.DisplayName, avatarNguoiGui = nguoiGuiDisplayInfo.AvatarUrl, noiDungRutGon = noiDungRutGon, tinNhanId = tinNhan.Id,
                    noiDung = $"Bạn có tin nhắn mới từ {nguoiGuiDisplayInfo.DisplayName}: \"{noiDungRutGon}\"",
                    url = Url.Action("Index", "TinNhan", new { nguoiLienHeId = currentUserId, ungTuyenId = tinNhan.UngTuyenLienQuanId, tinTuyenDungId = tinNhan.TinLienQuanId }, Request.Scheme)
                };
                await _thongBaoService.CreateThongBaoAsync(model.NguoiNhanId, NotificationConstants.Types.TinNhanMoi, JsonSerializer.Serialize(duLieuThongBao), NotificationConstants.RelatedEntities.TinNhan, tinNhan.Id);
            } catch (Exception ex) { _logger.LogError(ex, "Lỗi khi gửi thông báo tin nhắn mới."); }

            var currentUserDisplayInfoForResponse = GetUserChatDisplayInfo(currentUser);
            var tinNhanVm = new TinNhanItemViewModel {
                Id = tinNhan.Id, NguoiGuiId = tinNhan.NguoiGuiId, TenNguoiGui = currentUserDisplayInfoForResponse.DisplayName, AvatarUrlNguoiGui = currentUserDisplayInfoForResponse.AvatarUrl,
                NoiDung = tinNhan.NoiDung, NgayGui = tinNhan.NgayGui, ThoiGianGuiDisplay = TimeHelper.FormatDateTime(TimeHelper.ConvertToVietnamTimeFromUtc(tinNhan.NgayGui)), LaCuaToi = true, DaDoc = false
            };
            return Ok(new { success = true, message = "Gửi thành công", data = tinNhanVm });
        }

        [HttpPost("api/tinnhan/xoahoithoai")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaHoiThoaiAjax([FromBody] XoaHoiThoaiAjaxModel model) {
            var currentUserId = GetCurrentUserId();
            var messagesQuery = _context.TinNhans.Where(tn => (tn.NguoiGuiId == currentUserId && tn.NguoiNhanId == model.NguoiLienHeId) || (tn.NguoiGuiId == model.NguoiLienHeId && tn.NguoiNhanId == currentUserId));
            if (model.UngTuyenLienQuanId.HasValue) messagesQuery = messagesQuery.Where(tn => tn.UngTuyenLienQuanId == model.UngTuyenLienQuanId.Value);
            else if (model.TinTuyenDungLienQuanId.HasValue) messagesQuery = messagesQuery.Where(tn => tn.TinLienQuanId == model.TinTuyenDungLienQuanId.Value && tn.UngTuyenLienQuanId == null);
            else messagesQuery = messagesQuery.Where(tn => tn.UngTuyenLienQuanId == null && tn.TinLienQuanId == null);
            var messagesToDelete = await messagesQuery.ToListAsync();
            if (!messagesToDelete.Any()) return Ok(new { success = true, message = "Không có tin nhắn nào trong hội thoại này để xóa." });
            _context.TinNhans.RemoveRange(messagesToDelete); await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã xóa hội thoại." });
        }

        [HttpGet("api/tinnhan/contactdetails")]
        public async Task<IActionResult> GetContactDetails(int contactId, int? ungTuyenId, int? tinTuyenDungId) {
            var contactUser = await _context.NguoiDungs.Include(u => u.HoSoDoanhNghiep).Include(u => u.HoSoUngVien).AsNoTracking().FirstOrDefaultAsync(u => u.Id == contactId);
            if (contactUser == null) return NotFound("Người dùng không tồn tại.");
            var detailsViewModel = new ContactDetailsPaneViewModel { AvatarUrl = GetUserChatDisplayInfo(contactUser).AvatarUrl, DisplayName = GetUserChatDisplayInfo(contactUser).DisplayName };
            if (contactUser.LoaiTk == LoaiTaiKhoan.doanhnghiep && contactUser.HoSoDoanhNghiep != null) {
                detailsViewModel.UserType = "Doanh nghiệp"; detailsViewModel.CompanyName = contactUser.HoSoDoanhNghiep.TenCongTy; detailsViewModel.CompanyWebsite = contactUser.HoSoDoanhNghiep.UrlWebsite;
                detailsViewModel.CompanyTaxCode = contactUser.HoSoDoanhNghiep.MaSoThue; detailsViewModel.CompanyDescription = contactUser.HoSoDoanhNghiep.MoTa;
            } else if (contactUser.LoaiTk == LoaiTaiKhoan.canhan && contactUser.HoSoUngVien != null) {
                detailsViewModel.UserType = "Ứng viên"; detailsViewModel.CandidateProfileTitle = contactUser.HoSoUngVien.TieuDeHoSo;
                detailsViewModel.CandidateDesiredPosition = contactUser.HoSoUngVien.ViTriMongMuon; detailsViewModel.CandidateIntroduction = contactUser.HoSoUngVien.GioiThieuBanThan;
            } else { detailsViewModel.UserType = "Cá nhân"; }

            int? relatedJobIdForUrl = null;
            if (ungTuyenId.HasValue) {
                var ungTuyen = await _context.UngTuyens.Include(ut => ut.TinTuyenDung).AsNoTracking().FirstOrDefaultAsync(ut => ut.Id == ungTuyenId.Value);
                if (ungTuyen?.TinTuyenDung != null) {
                    detailsViewModel.RelatedJobTitle = ungTuyen.TinTuyenDung.TieuDe; relatedJobIdForUrl = ungTuyen.TinTuyenDungId;
                    detailsViewModel.RelatedUngTuyenId = ungTuyenId.Value; detailsViewModel.RelatedTinTuyenDungId = ungTuyen.TinTuyenDungId;
                    detailsViewModel.ApplicationStatus = ungTuyen.TrangThai.ToString(); detailsViewModel.JobDescriptionSummary = TruncateString(ungTuyen.TinTuyenDung.MoTa, 150);
                }
            } else if (tinTuyenDungId.HasValue) {
                var tinTD = await _context.TinTuyenDungs.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tinTuyenDungId.Value);
                if (tinTD != null) {
                    detailsViewModel.RelatedJobTitle = tinTD.TieuDe; relatedJobIdForUrl = tinTD.Id;
                    detailsViewModel.RelatedTinTuyenDungId = tinTuyenDungId.Value; detailsViewModel.JobDescriptionSummary = TruncateString(tinTD.MoTa, 150);
                }
            }
            if (relatedJobIdForUrl.HasValue) { detailsViewModel.RelatedJobUrl = Url.Action("ChiTiet", "TimViec", new { id = relatedJobIdForUrl }); }
            return PartialView("_ContactDetailsPane", detailsViewModel);
        }

        private string TruncateString(string? value, int maxLength) {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }

    public class GuiTinNhanAjaxModel {
        public int NguoiNhanId { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public int? UngTuyenLienQuanId { get; set; }
        public int? TinLienQuanId { get; set; }
    }
    public class XoaHoiThoaiAjaxModel {
        public int NguoiLienHeId { get; set; }
        public int? UngTuyenLienQuanId { get; set; }
        public int? TinTuyenDungLienQuanId { get; set; }
    }
}