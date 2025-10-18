// File: Services/ThongBaoService.cs
using HeThongTimViec.Data;
using HeThongTimViec.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongTimViec.Services
{
    public class ThongBaoService : IThongBaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ThongBaoService> _logger;

        public ThongBaoService(ApplicationDbContext context, ILogger<ThongBaoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CreateThongBaoAsync(int nguoiDungId, string loaiThongBao, string duLieuJson, string? loaiLienQuan = null, int? idLienQuan = null)
        {
            if (nguoiDungId <= 0 || string.IsNullOrWhiteSpace(loaiThongBao) || string.IsNullOrWhiteSpace(duLieuJson))
            {
                _logger.LogWarning("Tham số không hợp lệ cho CreateThongBaoAsync.");
                return false;
            }

            var thongBao = new ThongBao
            {
                NguoiDungId = nguoiDungId,
                LoaiThongBao = loaiThongBao,
                DuLieu = duLieuJson, // Phải là một chuỗi JSON
                LoaiLienQuan = loaiLienQuan,
                IdLienQuan = idLienQuan,
                DaDoc = false,
                NgayTao = DateTime.UtcNow // Nên dùng UtcNow cho thời gian trên server
            };

            try
            {
                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tạo thông báo thành công cho User ID {NguoiDungId}, Loại: {LoaiThongBao}", nguoiDungId, loaiThongBao);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thông báo cho User ID {NguoiDungId}, Loại: {LoaiThongBao}", nguoiDungId, loaiThongBao);
                return false;
            }
        }

        public async Task<IEnumerable<ThongBao>> GetThongBaosForUserAsync(int nguoiDungId, int page = 1, int pageSize = 10)
        {
            return await _context.ThongBaos
                                 .AsNoTracking()
                                 .Where(tb => tb.NguoiDungId == nguoiDungId)
                                 .OrderByDescending(tb => tb.NgayTao)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();
        }

        public async Task<int> GetUnreadThongBaoCountAsync(int nguoiDungId)
        {
            return await _context.ThongBaos
                                 .CountAsync(tb => tb.NguoiDungId == nguoiDungId && !tb.DaDoc);
        }

        public async Task<bool> MarkAsReadAsync(int thongBaoId, int nguoiDungId)
        {
            var thongBao = await _context.ThongBaos.FirstOrDefaultAsync(tb => tb.Id == thongBaoId && tb.NguoiDungId == nguoiDungId);
            if (thongBao == null)
            {
                _logger.LogWarning("Không tìm thấy thông báo hoặc quyền truy cập bị từ chối. ID: {ThongBaoId}, User ID: {NguoiDungId}", thongBaoId, nguoiDungId);
                return false;
            }

            if (!thongBao.DaDoc)
            {
                thongBao.DaDoc = true;
                thongBao.NgayDoc = DateTime.UtcNow;
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Thông báo ID {ThongBaoId} đã được đánh dấu đã đọc cho User ID {NguoiDungId}", thongBaoId, nguoiDungId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đánh dấu thông báo ID {ThongBaoId} là đã đọc cho User ID {NguoiDungId}", thongBaoId, nguoiDungId);
                    return false;
                }
            }
            return true; // Đã đọc từ trước
        }

        public async Task<bool> MarkAllAsReadAsync(int nguoiDungId)
        {
            var unreadNotifications = await _context.ThongBaos
                                                    .Where(tb => tb.NguoiDungId == nguoiDungId && !tb.DaDoc)
                                                    .ToListAsync();
            if (!unreadNotifications.Any())
            {
                return true; // Không có thông báo chưa đọc
            }

            foreach (var tb in unreadNotifications)
            {
                tb.DaDoc = true;
                tb.NgayDoc = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tất cả thông báo đã được đánh dấu đã đọc cho User ID {NguoiDungId}", nguoiDungId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo là đã đọc cho User ID {NguoiDungId}", nguoiDungId);
                return false;
            }
        }
        public async Task<ThongBao?> GetThongBaoByIdAsync(int thongBaoId)
        {
            return await _context.ThongBaos.AsNoTracking().FirstOrDefaultAsync(tb => tb.Id == thongBaoId);
        }

        public async Task<bool> DeleteAsync(int thongBaoId, int userId)
        {
            var thongBao = await _context.ThongBaos
                .FirstOrDefaultAsync(tb => tb.Id == thongBaoId && tb.NguoiDungId == userId);

            if (thongBao == null)
            {
                _logger.LogWarning("Attempted to delete non-existent or unauthorized notification. ID: {ThongBaoId}, UserID: {UserId}", thongBaoId, userId);
                return false;
            }

            try
            {
                _context.ThongBaos.Remove(thongBao);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted notification. ID: {ThongBaoId}, UserID: {UserId}", thongBaoId, userId);
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification. ID: {ThongBaoId}, UserID: {UserId}", thongBaoId, userId);
                return false;
            }
        }

        public async Task<int> DeleteMultipleAsync(int[] thongBaoIds, int userId)
        {
            var thongBaosToDelete = await _context.ThongBaos
                .Where(tb => tb.NguoiDungId == userId && thongBaoIds.Contains(tb.Id))
                .ToListAsync();

            if (!thongBaosToDelete.Any()) return 0;
            
            try
            {
                _context.ThongBaos.RemoveRange(thongBaosToDelete);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple notifications for UserID: {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> DeleteAllAsync(int userId)
        {
            try
            {
                // Cách này hiệu quả hơn là tải tất cả về rồi xóa
                // Sử dụng ExecuteDeleteAsync() nếu dùng EF Core 7 trở lên
                // return await _context.ThongBaos.Where(tb => tb.NguoiDungId == userId).ExecuteDeleteAsync();

                // Cách tương thích với các phiên bản EF Core cũ hơn
                var allThongBaos = await _context.ThongBaos
                    .Where(tb => tb.NguoiDungId == userId)
                    .ToListAsync();

                if (!allThongBaos.Any()) return 0;

                _context.ThongBaos.RemoveRange(allThongBaos);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications for UserID: {UserId}", userId);
                return 0;
            }
        }

    }
}