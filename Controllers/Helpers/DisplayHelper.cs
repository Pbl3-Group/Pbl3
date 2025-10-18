// File: Helpers/DisplayHelper.cs
using HeThongTimViec.Models;

namespace HeThongTimViec.Helpers
{
    public static class DisplayHelper
    {
        // Helper lấy text và class badge cho trạng thái tài khoản (tiếng Việt)
        public static (string Text, string BadgeClass) GetTrangThaiDisplay(TrangThaiTaiKhoan status)
        {
             return status switch
            {
                TrangThaiTaiKhoan.kichhoat => ("Hoạt động", "badge bg-success"),
                TrangThaiTaiKhoan.bidinhchi => ("Bị đình chỉ", "badge bg-danger"),
                TrangThaiTaiKhoan.choxacminh => ("Chờ xác minh", "badge bg-warning text-dark"),
                TrangThaiTaiKhoan.tamdung => ("Tạm dừng", "badge bg-secondary"),
                _ => ("Không rõ", "badge bg-light text-dark")
            };
        }

        // Helper lấy text hiển thị cho vai trò người dùng (tiếng Việt)
        public static string GetVaiTroDisplay(LoaiTaiKhoan role)
        {
            return role switch
            {
                LoaiTaiKhoan.canhan => "Ứng viên",
                LoaiTaiKhoan.doanhnghiep => "Nhà tuyển dụng",
                LoaiTaiKhoan.quantrivien => "Quản trị viên",
                _ => "Không xác định"
            };
        }
    }
}