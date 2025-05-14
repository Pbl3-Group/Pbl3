using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // For WebUtility
using System.Security.Claims;
using System.Threading.Tasks;

namespace HeThongTimViec.Controllers
{
    [Authorize]
    public class TinNhanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TinNhanController> _logger;

        public TinNhanController(ApplicationDbContext context, ILogger<TinNhanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("Không thể lấy ID người dùng hiện tại từ claims.");
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }
            return userId;
        }

        private string GetCurrentUserAvatar()
        {
            var currentUserId = GetCurrentUserId(); // Lấy ID trước
            var currentUser = _context.NguoiDungs.Find(currentUserId); // Tìm người dùng

            // Xác định vai trò của người dùng đang đăng nhập từ Claims
            var loggedInUserRoleClaim = User.FindFirstValue(ClaimTypes.Role);
            bool isEmployerLoggedIn = loggedInUserRoleClaim == nameof(LoaiTaiKhoan.doanhnghiep);

            return currentUser?.UrlAvatar ?? (isEmployerLoggedIn ? "/images/avatars/default_employer.png" : "/images/avatars/default_user.png");
        }


        public async Task<IActionResult> Index(int? nguoiLienHeId = null, int? tinId = null, int? ungTuyenId = null)
        {
            var currentUserId = GetCurrentUserId();
            var viewModel = new TinNhanTrangChinhViewModel();

            var allMessagesForUser = await _context.TinNhans
                .Include(t => t.NguoiGui)
                .Include(t => t.NguoiNhan)
                .Where(t => t.NguoiGuiId == currentUserId || t.NguoiNhanId == currentUserId)
                .OrderByDescending(t => t.NgayGui)
                .ToListAsync();

            var conversations = allMessagesForUser
                .GroupBy(t => t.NguoiGuiId == currentUserId ? t.NguoiNhanId : t.NguoiGuiId)
                .Select(g =>
                {
                    var otherUserIdInConv = g.Key;
                    var latestMessage = g.OrderByDescending(m => m.NgayGui).First();
                    NguoiDung? otherUser = latestMessage.NguoiGuiId == otherUserIdInConv ? latestMessage.NguoiGui : latestMessage.NguoiNhan;

                    if (otherUser == null) {
                        otherUser = _context.NguoiDungs.Find(otherUserIdInConv);
                        if (otherUser == null) {
                            _logger.LogWarning($"Không tìm thấy người dùng với ID {otherUserIdInConv} cho cuộc trò chuyện.");
                            return null;
                        }
                    }

                    var unreadCount = g.Count(m => m.NguoiNhanId == currentUserId && m.NguoiGuiId == otherUserIdInConv && m.NgayDoc == null);
                    var decodedContentForPreview = WebUtility.HtmlDecode(latestMessage.NoiDung);
                    var truncatedPreviewText = decodedContentForPreview.Length > 35
                                            ? decodedContentForPreview.Substring(0, 35) + "..."
                                            : decodedContentForPreview;
                    var finalPreviewForViewModel = WebUtility.HtmlEncode(truncatedPreviewText);

                    return new CuocHoiThoaiViewModel
                    {
                        NguoiLienHeId = otherUser.Id,
                        TenNguoiLienHe = otherUser.HoTen ?? "Người dùng không xác định",
                        UrlAvatarNguoiLienHe = otherUser.UrlAvatar ?? (otherUser.LoaiTk == LoaiTaiKhoan.canhan ? "/images/avatars/default_user.png" : "/images/avatars/default_employer.png"),
                        TinNhanCuoiCung = finalPreviewForViewModel,
                        ThoiGianTinNhanCuoi = latestMessage.NgayGui,
                        SoTinNhanMoi = unreadCount,
                        LaTinNhanCuoiCuaToi = latestMessage.NguoiGuiId == currentUserId,
                        LoaiTaiKhoanNguoiLienHe = otherUser.LoaiTk
                    };
                })
                .Where(c => c != null)
                .OrderByDescending(c => c.ThoiGianTinNhanCuoi)
                .ToList();

            viewModel.DanhSachCuocHoiThoai = conversations!;
            NguoiDung? selectedContactUserEntity = null;

            if (nguoiLienHeId.HasValue && nguoiLienHeId.Value > 0)
            {
                selectedContactUserEntity = await _context.NguoiDungs.FindAsync(nguoiLienHeId.Value);
                if (selectedContactUserEntity != null)
                {
                    viewModel.NguoiLienHeDangChonId = nguoiLienHeId.Value;
                    viewModel.ThongTinNguoiLienHeDangChon = selectedContactUserEntity;
                    
                    var messages = await GetTinNhanChiTietAsync(currentUserId, nguoiLienHeId.Value);
                    viewModel.TinNhanTrongCuocHoiThoai = messages;

                    await MarkMessagesAsReadAsync(currentUserId, nguoiLienHeId.Value);
                    
                    var existingConvVM = viewModel.DanhSachCuocHoiThoai
                        .FirstOrDefault(c => c.NguoiLienHeId == nguoiLienHeId.Value);
                    
                    if (existingConvVM != null)
                    {
                        existingConvVM.SoTinNhanMoi = 0;
                    }
                    else 
                    {
                        var newContactConversationVM = new CuocHoiThoaiViewModel
                        {
                            NguoiLienHeId = selectedContactUserEntity.Id,
                            TenNguoiLienHe = selectedContactUserEntity.HoTen ?? "Người dùng không xác định",
                            UrlAvatarNguoiLienHe = selectedContactUserEntity.UrlAvatar ?? 
                                (selectedContactUserEntity.LoaiTk == LoaiTaiKhoan.canhan ? "/images/avatars/default_user.png" : "/images/avatars/default_employer.png"),
                            TinNhanCuoiCung = WebUtility.HtmlEncode(""), 
                            ThoiGianTinNhanCuoi = DateTime.MinValue, 
                            SoTinNhanMoi = 0,
                            LaTinNhanCuoiCuaToi = false,
                            LoaiTaiKhoanNguoiLienHe = selectedContactUserEntity.LoaiTk
                        };
                        var tempList = viewModel.DanhSachCuocHoiThoai.ToList();
                        tempList.Insert(0, newContactConversationVM); 
                        viewModel.DanhSachCuocHoiThoai = tempList;
                    }
                }
                else {
                     _logger.LogWarning($"Người dùng {currentUserId} cố gắng mở cuộc trò chuyện với ID người liên hệ không hợp lệ {nguoiLienHeId.Value}");
                     viewModel.NguoiLienHeDangChonId = null; 
                }
            }
            else if (viewModel.DanhSachCuocHoiThoai.Any())
            {
                var firstConv = viewModel.DanhSachCuocHoiThoai.First(); 
                return RedirectToAction(nameof(Index), new { nguoiLienHeId = firstConv.NguoiLienHeId, tinId = tinId, ungTuyenId = ungTuyenId });
            }
            
            foreach (var convVM in viewModel.DanhSachCuocHoiThoai)
            {
                convVM.DangChon = (viewModel.NguoiLienHeDangChonId.HasValue && convVM.NguoiLienHeId == viewModel.NguoiLienHeDangChonId.Value);
            }
            
            viewModel.DanhSachCuocHoiThoai = viewModel.DanhSachCuocHoiThoai
                .OrderByDescending(c => c.DangChon)
                .ThenByDescending(c => c.ThoiGianTinNhanCuoi == DateTime.MinValue ? DateTime.MaxValue : c.ThoiGianTinNhanCuoi) 
                .ToList();

            viewModel.FormGuiTinNhanMoi.NguoiNhanId = viewModel.NguoiLienHeDangChonId ?? 0;
            if(viewModel.NguoiLienHeDangChonId.HasValue) 
            {
                viewModel.FormGuiTinNhanMoi.TinLienQuanId = tinId;
                viewModel.FormGuiTinNhanMoi.UngTuyenLienQuanId = ungTuyenId;
            }

            return View(viewModel);
        }

        private async Task<List<TinNhanChiTietViewModel>> GetTinNhanChiTietAsync(int currentUserId, int otherUserId)
        {
            var currentUser = await _context.NguoiDungs.FindAsync(currentUserId);
            var otherUser = await _context.NguoiDungs.FindAsync(otherUserId);

            if (currentUser == null || otherUser == null) return new List<TinNhanChiTietViewModel>();

            return await _context.TinNhans
                .Include(t => t.NguoiGui) 
                .Where(t => (t.NguoiGuiId == currentUserId && t.NguoiNhanId == otherUserId) ||
                             (t.NguoiGuiId == otherUserId && t.NguoiNhanId == currentUserId))
                .OrderBy(t => t.NgayGui)
                .Select(t => new TinNhanChiTietViewModel
                {
                    Id = t.Id,
                    NguoiGuiId = t.NguoiGuiId,
                    TenNguoiGui = t.NguoiGui.HoTen ?? "N/A",
                    AvatarNguoiGui = t.NguoiGui.UrlAvatar ?? (t.NguoiGui.LoaiTk == LoaiTaiKhoan.doanhnghiep ? "/images/avatars/default_employer.png" : "/images/avatars/default_user.png"),
                    NoiDung = t.NoiDung, 
                    NgayGui = t.NgayGui,
                    LaCuaToi = t.NguoiGuiId == currentUserId,
                    DaXem = t.NguoiGuiId == currentUserId && t.NgayDoc.HasValue, 
                    LoaiTaiKhoanNguoiGui = t.NguoiGui.LoaiTk
                })
                .ToListAsync();
        }

        private async Task MarkMessagesAsReadAsync(int currentUserId, int senderIdOfMessagesToMark)
        {
            var unreadMessages = await _context.TinNhans
                .Where(t => t.NguoiNhanId == currentUserId && t.NguoiGuiId == senderIdOfMessagesToMark && t.NgayDoc == null)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.NgayDoc = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{unreadMessages.Count} tin nhắn từ {senderIdOfMessagesToMark} cho {currentUserId} đã được đánh dấu là đã đọc.");
            }
        }

        [HttpGet("TinNhan/LayChiTietHoiThoai/{otherUserId}")]
        public async Task<IActionResult> LayChiTietHoiThoai(int otherUserId, int? tinId = null, int? ungTuyenId = null)
        {
            var currentUserId = GetCurrentUserId();
            if (otherUserId <= 0) return BadRequest("ID người liên hệ không hợp lệ.");

            var nguoiLienHe = await _context.NguoiDungs.FindAsync(otherUserId);
            if (nguoiLienHe == null) return NotFound("Không tìm thấy người liên hệ.");

            var messages = await GetTinNhanChiTietAsync(currentUserId, otherUserId);
            await MarkMessagesAsReadAsync(currentUserId, otherUserId);

            var partialViewModel = new TinNhanTrangChinhViewModel 
            {
                NguoiLienHeDangChonId = otherUserId,
                ThongTinNguoiLienHeDangChon = nguoiLienHe,
                TinNhanTrongCuocHoiThoai = messages,
                FormGuiTinNhanMoi = new GuiTinNhanViewModel { 
                    NguoiNhanId = otherUserId,
                    TinLienQuanId = tinId,
                    UngTuyenLienQuanId = ungTuyenId
                },
            };

            return PartialView("_ChiTietHoiThoaiPartial", partialViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiTinNhan([FromForm] GuiTinNhanViewModel model)
        {
            var currentUserId = GetCurrentUserId();
            
            if (model.NguoiNhanId == currentUserId)
            {
                 ModelState.AddModelError("", "Bạn không thể tự gửi tin nhắn cho chính mình.");
            }
             if (string.IsNullOrWhiteSpace(model.NoiDung))
            {
                ModelState.AddModelError("NoiDung", "Nội dung tin nhắn không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning($"Gửi tin nhắn thất bại cho người nhận {model.NguoiNhanId} bởi {currentUserId}. Lỗi: {string.Join(", ", errors)}");
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return Json(new { success = false, errors = errorMessages });
            }

            var nguoiNhan = await _context.NguoiDungs.FindAsync(model.NguoiNhanId);
            if (nguoiNhan == null)
            {
                return Json(new { success = false, message = "Người nhận không tồn tại." });
            }

            var tinNhan = new TinNhan
            {
                NguoiGuiId = currentUserId,
                NguoiNhanId = model.NguoiNhanId,
                NoiDung = WebUtility.HtmlEncode(model.NoiDung.Trim()),
                NgayGui = DateTime.Now,
                NgayDoc = null,
                TinLienQuanId = model.TinLienQuanId,
                UngTuyenLienQuanId = model.UngTuyenLienQuanId
            };

            _context.TinNhans.Add(tinNhan);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Tin nhắn ID {tinNhan.Id} từ {currentUserId} đến {model.NguoiNhanId} đã được gửi.");

            var nguoiGui = await _context.NguoiDungs.FindAsync(currentUserId);

            // Nội dung tin nhắn đã được HtmlEncode khi lưu, nên khi hiển thị thì cần Html.Raw(WebUtility.HtmlDecode(contentFromDb))
            // Hoặc nếu ViewModel.NoiDung đã là HtmlEncoded, thì view dùng Html.Raw(ViewModel.NoiDung)
            // Đoạn này gửi về client, client sẽ render. NoiDung trong tinNhanMoi đã được encode.
            string previewContent = WebUtility.HtmlDecode(tinNhan.NoiDung); // Decode for truncation logic
            string truncatedPreview = previewContent.Length > 35 ? previewContent.Substring(0, 35) + "..." : previewContent;


            return Json(new
            {
                success = true,
                message = "Tin nhắn đã gửi thành công.",
                tinNhanMoi = new TinNhanChiTietViewModel 
                {
                    Id = tinNhan.Id,
                    NguoiGuiId = tinNhan.NguoiGuiId,
                    TenNguoiGui = nguoiGui?.HoTen ?? "Bạn",
                    AvatarNguoiGui = GetCurrentUserAvatar(), // Lấy avatar của người gửi (chính là người dùng hiện tại)
                    NoiDung = tinNhan.NoiDung, // Nội dung đã được HTML encode
                    NgayGui = tinNhan.NgayGui,
                    LaCuaToi = true,
                    DaXem = false,
                    LoaiTaiKhoanNguoiGui = nguoiGui?.LoaiTk 
                },
                nguoiNhanId = model.NguoiNhanId, 
                tenNguoiNhan = nguoiNhan.HoTen,
                conversationUpdate = new CuocHoiThoaiViewModel {
                    NguoiLienHeId = nguoiNhan.Id, // ID của người nhận
                    TenNguoiLienHe = nguoiNhan.HoTen,
                    UrlAvatarNguoiLienHe = nguoiNhan.UrlAvatar ?? (nguoiNhan.LoaiTk == LoaiTaiKhoan.canhan ? "/images/avatars/default_user.png" : "/images/avatars/default_employer.png"),
                    TinNhanCuoiCung = WebUtility.HtmlEncode(truncatedPreview), // Tin nhắn cuối đã được encode và cắt ngắn
                    ThoiGianTinNhanCuoi = tinNhan.NgayGui,
                    SoTinNhanMoi = 0, // Vì người gửi vừa gửi, nên ko có tin nhắn mới từ người nhận này
                    LaTinNhanCuoiCuaToi = true, // Tin nhắn cuối cùng là của người gửi
                    LoaiTaiKhoanNguoiLienHe = nguoiNhan.LoaiTk,
                    DangChon = true // Cuộc trò chuyện này nên được đánh dấu là đang chọn
                }
            });
        }

        [HttpPost("TinNhan/SetThemeColor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetThemeColor([FromBody] string color)
        {
            if (string.IsNullOrWhiteSpace(color) || !color.StartsWith("#") || (color.Length != 7 && color.Length != 4))
            {
                return BadRequest("Mã màu không hợp lệ.");
            }
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation($"Người dùng {currentUserId} đã đổi màu theme chat thành {color}. (Chức năng mẫu)");
            return Json(new { success = true, message = "Đã lưu màu sắc."});
        }
    }
}