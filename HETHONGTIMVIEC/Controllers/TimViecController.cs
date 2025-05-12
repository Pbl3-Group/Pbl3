// File: Controllers/TimViecController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using HeThongTimViec.ViewModels.TimViec; // Đảm bảo using ViewModel PaginatedList
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using HeThongTimViec.Extensions; // Assuming EnumExtensions is here
using System.Collections.Generic; // For List and HashSet
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
using System; // For DateTime, Math, Exception

namespace HeThongTimViec.Controllers
{
    [Route("[controller]")] // Route thân thiện hơn: /TimViec, /TimViec/ChiTiet/5
    public class TimViecController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimViecController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // Để lấy User ID

        public TimViecController(ApplicationDbContext context, ILogger<TimViecController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // --- Helper Lấy User ID ---
        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        // --- Helper Tạo chuỗi hiển thị Lương (ĐÃ CHUYỂN THÀNH STATIC) ---
        private static string FormatSalary(LoaiLuong loaiLuong, ulong? min, ulong? max)
        {
            if (loaiLuong == LoaiLuong.thoathuan) return "Thỏa thuận";

            string prefix = loaiLuong switch
            {
                LoaiLuong.theogio => "/giờ",
                LoaiLuong.theongay => "/ngày",
                LoaiLuong.theoca => "/ca",
                LoaiLuong.theothang => "/tháng",
                LoaiLuong.theoduan => "/dự án",
                _ => ""
            };

            // Hàm nội tuyến để định dạng số
            string FormatValue(ulong val) => val.ToString("N0");

            if (min.HasValue && max.HasValue && min > 0 && max > 0)
            {
                if (min == max) return $"{FormatValue(min.Value)} {prefix}";
                return $"{FormatValue(min.Value)} - {FormatValue(max.Value)} {prefix}";
            }
            if (min.HasValue && min > 0) return $"Từ {FormatValue(min.Value)} {prefix}";
            if (max.HasValue && max > 0) return $"Đến {FormatValue(max.Value)} {prefix}";

            // Sử dụng EnumExtensions để lấy DisplayName một cách an toàn
            // Cần đảm bảo EnumExtensions có thể truy cập được ở đây nếu nó ở namespace khác
            // Hoặc truyền DisplayName vào nếu không muốn FormatSalary phụ thuộc vào EnumExtensions
            try
            {
                 return loaiLuong.GetDisplayName(); // Giả sử GetDisplayName là static hoặc có thể truy cập
            } catch (Exception)
            {
                 // Ghi log lỗi nếu không lấy được DisplayName
                 // Log error: Could not get display name for LoaiLuong enum value {loaiLuong}. Exception: {ex.Message}
                 return loaiLuong.ToString(); // Fallback về tên enum string
            }
        }


        // GET: /TimViec?TuKhoa=...&ThanhPhoId=...&page=1
        [HttpGet("")] // Route gốc của controller
        public async Task<IActionResult> Index([FromQuery] TimViecViewModel searchModel, int page = 1)
        {
            _logger.LogInformation("Trang tìm việc được truy cập với bộ lọc: {@SearchModel}, Trang: {Page}", searchModel, page);

            int pageSize = 10; // Số lượng tin trên mỗi trang
            int? currentUserId = GetCurrentUserId();

            // --- Query cơ bản ---
            var query = _context.TinTuyenDungs
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.ThanhPho)
                .Include(t => t.QuanHuyen)
                .Include(t => t.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .Where(t => t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date))
                .AsNoTracking();

            // --- Áp dụng bộ lọc ---
            if (!string.IsNullOrWhiteSpace(searchModel.TuKhoa))
            {
                string keyword = searchModel.TuKhoa.ToLower().Trim();
                query = query.Where(t => t.TieuDe.ToLower().Contains(keyword)
                                        || t.MoTa.ToLower().Contains(keyword)
                                        || (t.NguoiDang.HoSoDoanhNghiep != null && t.NguoiDang.HoSoDoanhNghiep.TenCongTy.ToLower().Contains(keyword))
                                        || (t.NguoiDang.LoaiTk == LoaiTaiKhoan.canhan && t.NguoiDang.HoTen.ToLower().Contains(keyword)));
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

            if (searchModel.LoaiLuong.HasValue)
            {
                query = query.Where(t => t.LoaiLuong == searchModel.LoaiLuong.Value);
                if (searchModel.LoaiLuong != LoaiLuong.thoathuan && searchModel.LuongMin.HasValue && searchModel.LuongMin > 0)
                {
                    query = query.Where(t => t.LoaiLuong != LoaiLuong.thoathuan &&
                                             ((t.LuongToiThieu.HasValue && t.LuongToiThieu >= searchModel.LuongMin.Value) ||
                                              (t.LuongToiDa.HasValue && t.LuongToiDa >= searchModel.LuongMin.Value)));
                }
            }

            // Logic lọc TinGap: chỉ lọc khi searchModel.TinGap có giá trị (true/false)
            // Nếu là null (không check checkbox), không lọc
            if (searchModel.TinGap.HasValue)
            {
                query = query.Where(t => t.TinGap == searchModel.TinGap.Value);
            }


            // --- Sắp xếp ---
            switch (searchModel.SapXep)
            {
                case "luongcao":
                    query = query.OrderByDescending(t => t.LoaiLuong == LoaiLuong.thoathuan ? 0 : (t.LuongToiDa ?? t.LuongToiThieu ?? 0))
                                 .ThenByDescending(t => t.TinGap)
                                 .ThenByDescending(t => t.NgayDang);
                    break;
                case "ngaymoi":
                default:
                    query = query.OrderByDescending(t => t.TinGap)
                                 .ThenByDescending(t => t.NgayDang);
                    break;
            }

            // --- Lấy danh sách Tin Đã Lưu (ĐÃ SỬA LỖI ToHashSetAsync) ---
            HashSet<int> savedJobIds = new HashSet<int>();
            if (currentUserId.HasValue)
            {
                var savedJobIdList = await _context.TinDaLuus
                                            .Where(tdl => tdl.NguoiDungId == currentUserId.Value)
                                            .Select(tdl => tdl.TinTuyenDungId)
                                            .ToListAsync(); // Load vào List trước
                savedJobIds = new HashSet<int>(savedJobIdList); // Tạo HashSet từ List
            }


            // --- Thực hiện truy vấn và Phân Trang ---
             // Tách riêng Select để gọi phương thức static không gây lỗi client projection
             // Các thuộc tính cần cho FormatSalary phải được lấy trước
            var intermediateQuery = query.Select(t => new
            {
                TinTuyenDung = t, // Giữ lại đối tượng gốc hoặc các thuộc tính cần thiết khác
                LoaiLuong = t.LoaiLuong,
                LuongToiThieu = t.LuongToiThieu,
                LuongToiDa = t.LuongToiDa,
                LoaiHinhCongViec = t.LoaiHinhCongViec, // Lấy Enum gốc
                 // Lấy các thông tin khác cần cho ViewModel
                NguoiDangLoaiTk = t.NguoiDang.LoaiTk,
                NguoiDangHoTen = t.NguoiDang.HoTen,
                HoSoDnTenCongTy = t.NguoiDang.HoSoDoanhNghiep != null ? t.NguoiDang.HoSoDoanhNghiep.TenCongTy : null,
                HoSoDnUrlLogo = t.NguoiDang.HoSoDoanhNghiep != null ? t.NguoiDang.HoSoDoanhNghiep.UrlLogo : null,
                NguoiDangUrlAvatar = t.NguoiDang.UrlAvatar,
                QuanHuyenTen = t.QuanHuyen != null ? t.QuanHuyen.Ten : null,
                ThanhPhoTen = t.ThanhPho != null ? t.ThanhPho.Ten : null,
                NganhNgheNho = t.TinTuyenDungNganhNghes.Select(nn => nn.NganhNghe.Ten).Take(2).ToList()
            });

            // Thực hiện phân trang trên kết quả trung gian (nếu được) hoặc thực hiện Select sau khi phân trang
            // Cách an toàn hơn là phân trang trước, rồi mới Select và gọi FormatSalary

            int totalItems = await query.CountAsync(); // Đếm tổng số mục trước khi phân trang
            page = Math.Max(1, page);
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
             // Ensure pageIndex is within valid range after calculating totalPages
            page = Math.Min(page, Math.Max(1, totalPages)); // Cannot exceed max pages (handle empty source)


            var itemsFromDb = await query
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .Select(t => new // Select lại các trường cần thiết
                                    {
                                        t.Id,
                                        t.TieuDe,
                                        t.NgayDang,
                                        t.TinGap,
                                        t.LoaiLuong,
                                        t.LuongToiThieu,
                                        t.LuongToiDa,
                                        t.LoaiHinhCongViec,
                                        NguoiDangLoaiTk = t.NguoiDang.LoaiTk,
                                        NguoiDangHoTen = t.NguoiDang.HoTen,
                                        HoSoDnTenCongTy = t.NguoiDang.HoSoDoanhNghiep != null ? t.NguoiDang.HoSoDoanhNghiep.TenCongTy : null,
                                        HoSoDnUrlLogo = t.NguoiDang.HoSoDoanhNghiep != null ? t.NguoiDang.HoSoDoanhNghiep.UrlLogo : null,
                                        NguoiDangUrlAvatar = t.NguoiDang.UrlAvatar,
                                        QuanHuyenTen = t.QuanHuyen != null ? t.QuanHuyen.Ten : null,
                                        ThanhPhoTen = t.ThanhPho != null ? t.ThanhPho.Ten : null,
                                        NganhNgheNho = t.TinTuyenDungNganhNghes.Select(nn => nn.NganhNghe.Ten).Take(2).ToList()
                                    })
                                    .ToListAsync(); // Lấy dữ liệu về client

            // Bây giờ mới tạo ViewModel và gọi FormatSalary trên client
            var resultViewModels = itemsFromDb.Select(t => new KetQuaTimViecItemViewModel
            {
                 Id = t.Id,
                TieuDe = t.TieuDe,
                TenCongTyHoacNguoiDang = t.NguoiDangLoaiTk == LoaiTaiKhoan.doanhnghiep
                                            ? (t.HoSoDnTenCongTy ?? "Doanh nghiệp") // Dùng ?? để xử lý null
                                            : t.NguoiDangHoTen,
                LogoHoacAvatarUrl = t.NguoiDangLoaiTk == LoaiTaiKhoan.doanhnghiep
                                    ? (!string.IsNullOrEmpty(t.HoSoDnUrlLogo) ? t.HoSoDnUrlLogo : "/images/default-company.png")
                                    : (!string.IsNullOrEmpty(t.NguoiDangUrlAvatar) ? t.NguoiDangUrlAvatar : "/images/default-avatar.png"),
                DiaDiem = (t.QuanHuyenTen != null ? t.QuanHuyenTen + ", " : "") + (t.ThanhPhoTen ?? ""),
                LoaiHinhCongViecDisplay = t.LoaiHinhCongViec.GetDisplayName(), // Gọi GetDisplayName ở client
                MucLuongDisplay = FormatSalary(t.LoaiLuong, t.LuongToiThieu, t.LuongToiDa), // Gọi FormatSalary ở client
                NgayDang = t.NgayDang,
                TinGap = t.TinGap,
                NganhNgheNho = t.NganhNgheNho,
                DaLuu = savedJobIds.Contains(t.Id) // Cập nhật trạng thái lưu
            }).ToList();

            // Tạo đối tượng PaginatedList thủ công vì đã lấy dữ liệu theo trang
            var paginatedResults = new PaginatedList<KetQuaTimViecItemViewModel>(resultViewModels, totalItems, page, pageSize);


            // --- Chuẩn bị dữ liệu cho Form Filter ---
            searchModel.KetQua = paginatedResults; // Gán kết quả đã phân trang vào ViewModel
            await PopulateFilterOptions(searchModel); // Hàm helper để lấy dữ liệu dropdown

            return View(searchModel);
        }

        // --- Helper Populate Filter Options (ĐÃ SỬA LỖI SelectList) ---
        private async Task PopulateFilterOptions(TimViecViewModel model)
        {
             model.ThanhPhoOptions = new SelectList(await _context.ThanhPhos.AsNoTracking().OrderBy(tp => tp.Ten).ToListAsync(), "Id", "Ten", model.ThanhPhoId);

             if(model.ThanhPhoId.HasValue && model.ThanhPhoId > 0) {
                model.QuanHuyenOptions = new SelectList(await _context.QuanHuyens.Where(q=>q.ThanhPhoId == model.ThanhPhoId.Value).AsNoTracking().OrderBy(q => q.Ten).ToListAsync(), "Id", "Ten", model.QuanHuyenId);
             } else {
                 model.QuanHuyenOptions = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Ten");
             }

             // Sửa lỗi CS0029
             var loaiHinhItems = EnumExtensions.GetSelectList<LoaiHinhCongViec>(includeDefaultItem: true, defaultItemText: "-- Tất cả loại hình --", defaultItemValue: "");
             model.LoaiHinhCongViecOptions = new SelectList(loaiHinhItems, "Value", "Text", model.LoaiHinhCongViec); // Thêm selectedValue

             var loaiLuongItems = EnumExtensions.GetSelectList<LoaiLuong>(includeDefaultItem: true, defaultItemText: "-- Tất cả mức lương --", defaultItemValue: "");
             model.LoaiLuongOptions = new SelectList(loaiLuongItems, "Value", "Text", model.LoaiLuong); // Thêm selectedValue


             var allNganhNghe = await _context.NganhNghes.AsNoTracking().OrderBy(n => n.Ten).ToListAsync();
             model.NganhNgheOptions = allNganhNghe.Select(n => new SelectListItem {
                 Value = n.Id.ToString(),
                 Text = n.Ten,
                 Selected = model.NganhNgheIds != null && model.NganhNgheIds.Contains(n.Id)
             }).ToList();
        }


        // GET: /TimViec/ChiTiet/5
        [HttpGet("ChiTiet/{id:int}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            _logger.LogInformation("Xem chi tiết tin tuyển dụng ID: {TinTuyenDungId}", id);
            int? currentUserId = GetCurrentUserId();

            var tinTuyenDung = await _context.TinTuyenDungs
                .Include(t => t.NguoiDang)
                    .ThenInclude(nd => nd.HoSoDoanhNghiep)
                .Include(t => t.ThanhPho)
                .Include(t => t.QuanHuyen)
                .Include(t => t.TinTuyenDungNganhNghes)
                    .ThenInclude(tnn => tnn.NganhNghe)
                .Include(t => t.LichLamViecCongViecs)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.TrangThai == TrangThaiTinTuyenDung.daduyet && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));

            if (tinTuyenDung == null)
            {
                _logger.LogWarning("Không tìm thấy tin tuyển dụng ID: {TinTuyenDungId} hoặc tin chưa được duyệt/đã hết hạn.", id);
                 return View("TinNotFound"); // Trả về View thông báo
            }

            bool daLuu = false;
            if (currentUserId.HasValue)
            {
                daLuu = await _context.TinDaLuus.AnyAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == id);
            }

            var addressParts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(tinTuyenDung.DiaChiLamViec)) addressParts.Add(tinTuyenDung.DiaChiLamViec);
            if (tinTuyenDung.QuanHuyen != null) addressParts.Add(tinTuyenDung.QuanHuyen.Ten);
            if (tinTuyenDung.ThanhPho != null) addressParts.Add(tinTuyenDung.ThanhPho.Ten);
            string diaChiDayDu = string.Join(", ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));


            var viewModel = new ChiTietTinTuyenDungViewModel
            {
                Id = tinTuyenDung.Id,
                TieuDe = tinTuyenDung.TieuDe,
                MoTa = tinTuyenDung.MoTa,
                YeuCau = tinTuyenDung.YeuCau,
                QuyenLoi = tinTuyenDung.QuyenLoi,
                LoaiHinhCongViecDisplay = tinTuyenDung.LoaiHinhCongViec.GetDisplayName(),
                LoaiLuongDisplay = tinTuyenDung.LoaiLuong.GetDisplayName(),
                MucLuongDisplay = FormatSalary(tinTuyenDung.LoaiLuong, tinTuyenDung.LuongToiThieu, tinTuyenDung.LuongToiDa), // Gọi FormatSalary bình thường ở đây
                DiaChiLamViecDayDu = diaChiDayDu,
                YeuCauKinhNghiemText = tinTuyenDung.YeuCauKinhNghiemText,
                YeuCauHocVanText = tinTuyenDung.YeuCauHocVanText,
                SoLuongTuyen = tinTuyenDung.SoLuongTuyen,
                TinGap = tinTuyenDung.TinGap,
                NgayDang = tinTuyenDung.NgayDang,
                NgayHetHan = tinTuyenDung.NgayHetHan,
                DaLuu = daLuu,

                NguoiDangId = tinTuyenDung.NguoiDangId,
                LoaiTaiKhoanNguoiDang = tinTuyenDung.NguoiDang.LoaiTk,
                TenNguoiDangHoacCongTy = tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep
                    ? (tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.TenCongTy ?? "Doanh nghiệp")
                    : tinTuyenDung.NguoiDang.HoTen,
                LogoHoacAvatarUrl = tinTuyenDung.NguoiDang.LoaiTk == LoaiTaiKhoan.doanhnghiep
                    ? (!string.IsNullOrEmpty(tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlLogo) ? tinTuyenDung.NguoiDang.HoSoDoanhNghiep.UrlLogo : "/images/default-company.png")
                    : (!string.IsNullOrEmpty(tinTuyenDung.NguoiDang.UrlAvatar) ? tinTuyenDung.NguoiDang.UrlAvatar : "/images/default-avatar.png"),
                UrlWebsiteCongTy = tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.UrlWebsite,
                MoTaCongTy = tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.MoTa,
                CongTyDaXacMinh = tinTuyenDung.NguoiDang.HoSoDoanhNghiep?.DaXacMinh ?? false,

                NganhNghes = tinTuyenDung.TinTuyenDungNganhNghes.Select(tnn => tnn.NganhNghe.Ten).ToList(),
                LichLamViecs = tinTuyenDung.LichLamViecCongViecs.Select(l => new LichLamViecViewModel
                {
                    NgayTrongTuanDisplay = l.NgayTrongTuan.GetDisplayName(),
                    ThoiGianDisplay = l.BuoiLamViec.HasValue
                                        ? l.BuoiLamViec.Value.GetDisplayName()
                                        : (l.GioBatDau.HasValue && l.GioKetThuc.HasValue
                                            ? $"{l.GioBatDau.Value:hh\\:mm} - {l.GioKetThuc.Value:hh\\:mm}"
                                            : "Linh hoạt")
                }).ToList()
            };

            return View(viewModel);
        }

        // --- Hành động Lưu / Bỏ Lưu Tin (Cần JavaScript gọi) ---

        [HttpPost("LuuTin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuTin([FromBody] int tinTuyenDungId)
        {
             int? currentUserId = GetCurrentUserId();
             if (!currentUserId.HasValue) { return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." }); }
             if (tinTuyenDungId <= 0) { return BadRequest(new { success = false, message = "ID tin tuyển dụng không hợp lệ." }); }

            _logger.LogInformation("Người dùng {UserId} yêu cầu lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);

            var tinExists = await _context.TinTuyenDungs.AnyAsync(t => t.Id == tinTuyenDungId
                                                                    && t.TrangThai == TrangThaiTinTuyenDung.daduyet
                                                                    && (!t.NgayHetHan.HasValue || t.NgayHetHan.Value.Date >= DateTime.UtcNow.Date));
            if (!tinExists) { return NotFound(new { success = false, message = "Tin tuyển dụng không tồn tại, chưa được duyệt hoặc đã hết hạn." }); }

            var alreadySaved = await _context.TinDaLuus.AnyAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == tinTuyenDungId);
            if (alreadySaved) { return Ok(new { success = true, message = "Tin đã được lưu trước đó.", alreadySaved = true }); }

            var tinDaLuu = new TinDaLuu { NguoiDungId = currentUserId.Value, TinTuyenDungId = tinTuyenDungId, NgayLuu = DateTime.UtcNow };
            _context.TinDaLuus.Add(tinDaLuu);

            try
            {
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Người dùng {UserId} đã lưu thành công tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return Ok(new { success = true, message = "Lưu tin thành công." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi DbUpdateException khi người dùng {UserId} lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi cơ sở dữ liệu khi lưu tin. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi Exception khi người dùng {UserId} lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống khi lưu tin. Vui lòng thử lại." });
            }
        }

        [HttpPost("BoLuuTin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoLuuTin([FromBody] int tinTuyenDungId)
        {
             int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) { return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." }); }
             if (tinTuyenDungId <= 0) { return BadRequest(new { success = false, message = "ID tin tuyển dụng không hợp lệ." }); }

            _logger.LogInformation("Người dùng {UserId} yêu cầu bỏ lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);

            var tinDaLuu = await _context.TinDaLuus.FirstOrDefaultAsync(tdl => tdl.NguoiDungId == currentUserId.Value && tdl.TinTuyenDungId == tinTuyenDungId);

            if (tinDaLuu == null) {
                 _logger.LogWarning("Người dùng {UserId} yêu cầu bỏ lưu tin ID: {TinTuyenDungId} nhưng không tìm thấy bản ghi.", currentUserId.Value, tinTuyenDungId);
                return Ok(new { success = true, message = "Tin chưa được lưu hoặc đã bỏ lưu.", notFoundOrUnsaved = true });
            }

            _context.TinDaLuus.Remove(tinDaLuu);

            try
            {
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Người dùng {UserId} đã bỏ lưu thành công tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return Ok(new { success = true, message = "Bỏ lưu tin thành công." });
            }
             catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi DbUpdateException khi người dùng {UserId} bỏ lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi cơ sở dữ liệu khi bỏ lưu tin. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi Exception khi người dùng {UserId} bỏ lưu tin ID: {TinTuyenDungId}", currentUserId.Value, tinTuyenDungId);
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống khi bỏ lưu tin. Vui lòng thử lại." });
            }
        }
    }
}