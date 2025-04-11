namespace HeThongTimViec.Models
{
    public enum GioiTinhEnum
    {
        Nam, Nu
    }

    public enum ThanhPhoEnum
    {
        An_Giang, Ba_Ria_Vung_Tau, Bac_Lieu, Bac_Giang, Bac_Kan, Bac_Ninh, Ben_Tre,
        Binh_Duong, Binh_Dinh, Binh_Phuoc, Binh_Thuan, Ca_Mau, Cao_Bang, Can_Tho,
        Da_Nang, Dak_Lak, Dak_Nong, Dien_Bien, Dong_Nai, Dong_Thap, Gia_Lai,
        Ha_Giang, Ha_Nam, Ha_Noi, Ha_Tinh, Hai_Duong, Hai_Phong, Hau_Giang,
        Hoa_Binh, Hung_Yen, Khanh_Hoa, Kien_Giang, Kon_Tum, Lai_Chau, Lam_Dong,
        Lang_Son, Lao_Cai, Long_An, Nam_Dinh, Nghe_An, Ninh_Binh, Ninh_Thuan,
        Phu_Tho, Phu_Yen, Quang_Binh, Quang_Nam, Quang_Ngai, Quang_Ninh, Quang_Tri,
        Soc_Trang, Son_La, Tay_Ninh, Thai_Binh, Thai_Nguyen, Thanh_Hoa, Thua_Thien_Hue,
        Tien_Giang, TP_Ho_Chi_Minh, Tra_Vinh, Tuyen_Quang, Vinh_Long, Vinh_Phuc, Yen_Bai
    }

    public enum VaiTroEnum
    {
        Ung_Vien, Nha_Tuyen_Dung, Quan_Tri_Vien
    }

    public enum TrangThaiEnum
    {
        Chap_Thuan, Bi_Cam
    }
    public enum QuyMoEnum
    {
        _1_10, _11_50, _51_100, _101_500, _500_
    }

    public enum TrangThaiBusinessEnum
    {
        Dang_Xu_Ly, Chap_Thuan, Bi_Cam
    }

    public enum NgayEnum
    {
        Thu_2, Thu_3, Thu_4, Thu_5, Thu_6, Thu_7, Chu_Nhat
    }

    public enum ThoiGianEnum
    {
        Sang, Chieu, Toi
    }

    public enum TinhTrangMemberEnum
    {
        Cho_Duyet, Dang_Hoat_Dong, Bi_Cam
    }

    public enum LoaiCvEnum
    {
        Ban_Thoi_Gian, Thoi_Vu
    }
   public enum TrangThaiJobEnum
    {
        Mo, Dong
    }

    public enum LinhVucEnum
    {
        Nha_Hang_Khach_San, Ban_Le_Sieu_Thi, Giao_Hang_Van_Chuyen, Dich_Vu_Khach_Hang,
        Lao_Dong_Pho_Thong, Giao_Duc_Gia_Su, IT_Cong_Nghe, Marketing_Quang_Cao,
        Nhan_Su_Hanh_Chinh, Xay_Dung_Co_Khi, Suc_Khoe_Lam_Dep, Giai_Tri_Su_Kien,
        Kinh_Doanh_Ban_Hang, Thiet_Ke_Do_Hoa, Content_Viet_Lach, Tai_Chinh_Ke_Toan,
        Dien_Tu_Dien_Lanh, San_Xuat_Che_Bien, Thu_Cong_My_Nghe, Khac
    }
    public enum ThuEnum
    {
        T2, T3, T4, T5, T6, T7, CN
    }
    public enum TrangThaiApplicationEnum
    {
        Dang_Cho, Da_Chap_Nhan, Da_Tu_Choi
    }
    public enum TrangThaiReportEnum
    {
        Dang_Cho, Da_Xem_Xet
    }
    public enum TrangThaiNotificationEnum
    {
        Chua_Doc, Da_Doc
    }
}