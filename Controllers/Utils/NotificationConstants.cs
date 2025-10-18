// File: Utils/NotificationConstants.cs
namespace HeThongTimViec.Utils
{
    public static class NotificationConstants
    {
        public static class Types
        {
            // Liên quan đến Tin Tuyển Dụng
            public const string TinTuyenDungDuyet = "TINTUYENDUNG_DUYET"; // Tin được duyệt
            public const string TinTuyenDungTuChoi = "TINTUYENDUNG_TUCHOI"; // Tin bị từ chối
            public const string TinTuyenDungTamAn = "TINTUYENDUNG_TAMAN"; // Tin bị tạm ẩn
            public const string TinTuyenDungHetHan = "TINTUYENDUNG_HETHAN"; // Tin hết hạn
            public const string TinTuyenDungDaTuyen = "TINTUYENDUNG_DATUYEN"; // Tin đã tuyển đủ
            public const string TinTuyenDungMoiChoDuyet = "TINTUYENDUNG_MOI_CHO_DUYET"; // Thông báo cho Admin có tin mới chờ duyệt

            // Liên quan đến Ứng Tuyển
            public const string UngTuyenNtdXem = "UNGTUYEN_NTD_XEM"; // Nhà tuyển dụng đã xem hồ sơ
            public const string UngTuyenNtdTuChoi = "UNGTUYEN_NTD_TUCHOI"; // Nhà tuyển dụng từ chối hồ sơ
            public const string UngTuyenNtdChapNhan = "UNGTUYEN_NTD_CHAPNHAN"; // Nhà tuyển dụng chấp nhận (Đã duyệt/Phù hợp)
            public const string UngTuyenUngVienRut = "UNGTUYEN_UNGVIEN_RUT"; // Ứng viên rút hồ sơ (Thông báo cho NTD)
            public const string UngTuyenMoiChoNtd = "UNGTUYEN_MOI_CHO_NTD"; // Thông báo cho NTD có ứng tuyển mới (chờ duyệt)

            // Liên quan đến Hồ Sơ Doanh Nghiệp
            public const string HoSoDoanhNghiepXacMinh = "HOSODN_XACMINH"; // Hồ sơ được xác minh
            public const string HoSoDoanhNghiepTuChoiXacMinh = "HOSODN_TUCHOI_XACMINH"; // Hồ sơ bị từ chối xác minh
            public const string HoSoDoanhNghiepMoiChoXacMinh = "HOSODN_MOI_CHO_XACMINH"; // Thông báo cho Admin có hồ sơ DN mới chờ xác minh

            // Liên quan đến Báo Cáo Vi Phạm
            public const string BaoCaoViPhamMoi = "BAOCAO_MOI"; // Báo cáo mới (Thông báo cho Admin)
            public const string BaoCaoViPhamDaXemXet = "BAOCAO_XEMXET"; // Báo cáo đã được xem xét (Thông báo cho người báo cáo)
            public const string BaoCaoViPhamDaXuLy = "BAOCAO_XULY"; // Báo cáo đã được xử lý (Thông báo cho người báo cáo)
            public const string BaoCaoViPhamBoQua = "BAOCAO_BOQUA"; // Báo cáo bị bỏ qua (Thông báo cho người báo cáo)

            // Liên quan đến Tài Khoản
            public const string TaiKhoanTamDung = "TAIKHOAN_TAMDUNG"; // Tài khoản bị tạm dừng
            public const string TaiKhoanBiDinhChi = "TAIKHOAN_BIDINHCHI"; // Tài khoản bị đình chỉ
            public const string TaiKhoanKichHoat = "TAIKHOAN_KICHHOAT"; // Tài khoản được kích hoạt lại (sau khi bị tạm dừng/đình chỉ)

            // Chung
            public const string TinNhanMoi = "TINNHAN_MOI"; // Có tin nhắn mới
            public const string HeThongChung = "HETHONG_CHUNG"; // Thông báo chung từ hệ thống
             public const string AdminBroadcast = "ADMIN_BROADCAST"; 
        }

        public static class RelatedEntities
        {
            public const string TinTuyenDung = "TinTuyenDung";
            public const string UngTuyen = "UngTuyen";
            public const string HoSoDoanhNghiep = "HoSoDoanhNghiep";
            public const string BaoCaoViPham = "BaoCaoViPham";
            public const string NguoiDung = "NguoiDung"; // Dùng cho thay đổi trạng thái tài khoản
            public const string TinNhan = "TinNhan";
            public const string ThongBaoAdmin = "ThongBaoAdmin";
            public const string AdminBroadcast = "AdminBroadcast";
        }
    }
}