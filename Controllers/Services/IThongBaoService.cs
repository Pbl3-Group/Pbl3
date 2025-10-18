// File: Services/IThongBaoService.cs
using HeThongTimViec.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeThongTimViec.Services
{
    public interface IThongBaoService
    {
        Task<bool> CreateThongBaoAsync(int nguoiDungId, string loaiThongBao, string duLieuJson, string? loaiLienQuan = null, int? idLienQuan = null);
        Task<IEnumerable<ThongBao>> GetThongBaosForUserAsync(int nguoiDungId, int page = 1, int pageSize = 10);
        Task<int> GetUnreadThongBaoCountAsync(int nguoiDungId);
        Task<bool> MarkAsReadAsync(int thongBaoId, int nguoiDungId);
        Task<bool> MarkAllAsReadAsync(int nguoiDungId);
        Task<ThongBao?> GetThongBaoByIdAsync(int thongBaoId);
            Task<bool> DeleteAsync(int thongBaoId, int userId);
           Task<int> DeleteMultipleAsync(int[] thongBaoIds, int userId);
            Task<int> DeleteAllAsync(int userId);
    }
}
