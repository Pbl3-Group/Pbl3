USE HETHONGTIMVIEC;
-- Thêm dữ liệu vào bảng users (20 người dùng)
INSERT INTO users (Ho_ten, gioi_tinh, email, facebook_link, mat_khau, Ngay_sinh, SDT, thanh_pho, vai_tro, trang_thai, Mo_ta) VALUES
-- Ứng viên (12)
('Nguyễn Văn An', 'Nam', 'nguyenvanan@gmail.com', 'facebook.com/nguyenvanan', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abc', '1998-05-15', '0901234567', 'TP Hồ Chí Minh', 'Ứng viên', 'Chấp thuận', 'Sinh viên năm cuối ngành Quản trị Kinh doanh, tìm việc làm thêm.'),
('Trần Thị Bình', 'Nữ', 'tranthib@gmail.com', 'facebook.com/tranthib', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abd', '1997-08-22', '0912345678', 'Hà Nội', 'Ứng viên', 'Chấp thuận', 'Đã tốt nghiệp đại học, có kinh nghiệm làm việc nhà hàng và bán hàng.'),
('Lê Văn Cường', 'Nam', 'levancuong@gmail.com', 'facebook.com/levancuong', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abe', '2000-03-10', '0923456789', 'Đà Nẵng', 'Ứng viên', 'Chấp thuận', 'Sinh viên IT, thích làm việc bán thời gian để tích lũy kinh nghiệm.'),
('Phạm Thị Dung', 'Nữ', 'phamthidung@gmail.com', 'facebook.com/phamthidung', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abf', '1996-11-27', '0934567890', 'Hải Phòng', 'Ứng viên', 'Chấp thuận', 'Có 2 năm kinh nghiệm làm nhân viên phục vụ, thích công việc linh hoạt.'),
('Hoàng Văn Điệp', 'Nam', 'hoangvandiep@gmail.com', 'facebook.com/hoangvandiep', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abg', '1999-07-05', '0945678901', 'Cần Thơ', 'Ứng viên', 'Chấp thuận', 'Đam mê âm nhạc, làm việc bán thời gian tại quán cafe và sự kiện.'),
('Ngô Thị Hà', 'Nữ', 'ngothiha@gmail.com', 'facebook.com/ngothiha', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abh', '2001-01-15', '0956789012', 'TP Hồ Chí Minh', 'Ứng viên', 'Chấp thuận', 'Sinh viên chuyên ngành Marketing, đang tìm việc thực tập.'),
('Vũ Đình Khoa', 'Nam', 'vudinhkhoa@gmail.com', 'facebook.com/vudinhkhoa', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abi', '1995-09-20', '0967890123', 'Hà Nội', 'Ứng viên', 'Chấp thuận', 'Tốt nghiệp đại học, có kinh nghiệm làm việc nhiều lĩnh vực khác nhau.'),
('Lý Thị Loan', 'Nữ', 'lythiloan@gmail.com', 'facebook.com/lythiloan', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abj', '1998-12-03', '0978901234', 'Đà Nẵng', 'Ứng viên', 'Chấp thuận', 'Sinh viên ngành Du lịch, thích làm việc trong môi trường năng động.'),
('Trịnh Văn Minh', 'Nam', 'trinhvanminh@gmail.com', 'facebook.com/trinhvanminh', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abk', '1997-06-25', '0989012345', 'Bình Dương', 'Ứng viên', 'Chấp thuận', 'Có kinh nghiệm làm shipper và nhân viên bán hàng.'),
('Đỗ Thị Ngọc', 'Nữ', 'dothingoc@gmail.com', 'facebook.com/dothingoc', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abl', '2000-04-18', '0990123456', 'Bà Rịa - Vũng Tàu', 'Ứng viên', 'Chấp thuận', 'Làm việc bán thời gian để trang trải chi phí học tập.'),
('Bùi Văn Phong', 'Nam', 'buivanphong@gmail.com', 'facebook.com/buivanphong', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abm', '1996-02-14', '0701234568', 'TP Hồ Chí Minh', 'Ứng viên', 'Chấp thuận', 'Kỹ năng giao tiếp tốt, thích làm việc với mọi người.'),
('Mai Thị Quỳnh', 'Nữ', 'maithiquynh@gmail.com', 'facebook.com/maithiquynh', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abn', '1999-10-05', '0912345679', 'Hà Nội', 'Ứng viên', 'Chấp thuận', 'Đang tìm việc bán thời gian phù hợp với lịch học.'),

-- Nhà tuyển dụng (7)
('Dương Văn Sơn', 'Nam', 'duongvanson@gmail.com', 'facebook.com/duongvanson', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abo', '1985-07-12', '0923456780', 'TP Hồ Chí Minh', 'Nhà tuyển dụng', 'Chấp thuận', 'Chủ chuỗi quán cafe, đang mở rộng kinh doanh.'),
('Trương Thị Thanh', 'Nữ', 'truongthithanh@gmail.com', 'facebook.com/truongthithanh', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abp', '1988-03-21', '0934567891', 'Hà Nội', 'Nhà tuyển dụng', 'Chấp thuận', 'Quản lý nhà hàng, thường xuyên tuyển nhân viên phục vụ.'),
('Phan Văn Uy', 'Nam', 'phanvanuy@gmail.com', 'facebook.com/phanvanuy', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abq', '1980-11-09', '0945678902', 'Đà Nẵng', 'Nhà tuyển dụng', 'Chấp thuận', 'Giám đốc công ty thương mại điện tử, cần nhân viên bán thời gian.'),
('Hồ Thị Vân', 'Nữ', 'hothivan@gmail.com', 'facebook.com/hothivan', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abr', '1982-05-30', '0956789013', 'Khánh Hòa', 'Nhà tuyển dụng', 'Chấp thuận', 'Chủ cửa hàng thời trang, cần nhân viên bán hàng.'),
('Diệp Văn Xuân', 'Nam', 'diepvanxuan@gmail.com', 'facebook.com/diepvanxuan', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abs', '1979-02-14', '0967890124', 'Cần Thơ', 'Nhà tuyển dụng', 'Chấp thuận', 'Chủ doanh nghiệp vận chuyển, thường xuyên tuyển shipper.'),
('Lương Thị Yến', 'Nữ', 'luongthiyen@gmail.com', 'facebook.com/luongthiyen', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abt', '1984-08-07', '0978901235', 'TP Hồ Chí Minh', 'Nhà tuyển dụng', 'Chấp thuận', 'Quản lý trung tâm tiếng Anh, tuyển gia sư bán thời gian.'),
('Tạ Văn Đức', 'Nam', 'tavanduc@gmail.com', 'facebook.com/tavanduc', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abu', '1981-12-25', '0989012346', 'Hà Nội', 'Nhà tuyển dụng', 'Chấp thuận', 'Điều hành công ty tổ chức sự kiện, thường xuyên tuyển nhân viên thời vụ.'),

-- Quản trị viên (1)
('Admin System', 'Nam', 'admin@hethongtimviec.com', 'facebook.com/adminhttviec', '$2a$10$aB1c2D3e4F5g6H7i8J9k.OLmnopQrsTUvWxYz0123456789abv', '1990-01-01', '0999999999', 'Hà Nội', 'Quản trị viên', 'Chấp thuận', 'Quản trị viên hệ thống.');

-- Thêm dữ liệu vào bảng businesses (7 công ty)
INSERT INTO businesses (owner_id, ten, SDT, email, linh_vuc, nam_thanh_lap, dia_chi, giay_phep_kinh_doanh, ma_so_thue, quy_mo, website, social_links, logo, trang_thai) VALUES
(10013, 'Cafe Melody', '0901122334', 'contact@cafemelody.com', 'Nhà hàng - Khách sạn', '2018-05-10', '123 Nguyễn Trãi, Quận 1, TP Hồ Chí Minh', 'GP123456789', '0101234567', '11-50', 'cafemelody.com', 'facebook.com/cafemelody, instagram.com/cafemelody', 'logo_cafemelody.png', 'chấp thuận'),
(10014, 'Nhà hàng Phố Xưa', '0912233445', 'info@phoxuarestaurant.com', 'Nhà hàng - Khách sạn', '2015-08-22', '45 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', 'GP234567890', '0201234567', '11-50', 'phoxuarestaurant.com', 'facebook.com/phoxua, instagram.com/phoxua', 'logo_phoxua.png', 'chấp thuận'),
(10015, 'SpeedBuy Store', '0923344556', 'support@speedbuy.com', 'Bán lẻ - Siêu thị', '2020-03-15', '72 Nguyễn Thị Minh Khai, Hải Châu, Đà Nẵng', 'GP345678901', '0401234567', '51-100', 'speedbuy.com', 'facebook.com/speedbuy, instagram.com/speedbuy', 'logo_speedbuy.png', 'chấp thuận'),
(10016, 'Thời trang BlueStar', '0934455667', 'contact@bluestar.com', 'Bán lẻ - Siêu thị', '2019-11-05', '25 Trần Phú, Nha Trang, Khánh Hòa', 'GP456789012', '0601234567', '1-10', 'bluestar.com', 'facebook.com/bluestar, instagram.com/bluestar', 'logo_bluestar.png', 'chấp thuận'),
(10017, 'FastGo Express', '0945566778', 'info@fastgo.com', 'Giao hàng - Vận chuyển', '2021-02-18', '103 Nguyễn Văn Cừ, Ninh Kiều, Cần Thơ', 'GP567890123', '0801234567', '51-100', 'fastgo.com', 'facebook.com/fastgo, instagram.com/fastgo', 'logo_fastgo.png', 'chấp thuận'),
(10018, 'Trung tâm Anh ngữ SpeakWell', '0956677889', 'contact@speakwell.edu.vn', 'Giáo dục - Gia sư', '2017-07-30', '88 Cách Mạng Tháng 8, Quận 3, TP Hồ Chí Minh', 'GP678901234', '0301234567', '11-50', 'speakwell.edu.vn', 'facebook.com/speakwell, instagram.com/speakwell', 'logo_speakwell.png', 'chấp thuận'),
(10019, 'EventPro Solutions', '0967788990', 'info@eventpro.com', 'Giải trí - Sự kiện', '2016-10-12', '55 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội', 'GP789012345', '0201234568', '11-50', 'eventpro.com', 'facebook.com/eventpro, instagram.com/eventpro', 'logo_eventpro.png', 'chấp thuận');

-- Thêm dữ liệu vào bảng CV
INSERT INTO CV (user_id, file_path) VALUES
(10001, 'cv_nguyenvanan.pdf'),
(10002, 'cv_tranthib.pdf'),
(10003, 'cv_levancuong.pdf'),
(10004, 'cv_phamthidung.pdf'),
(10005, 'cv_hoangvandiep.pdf'),
(10007, 'cv_vudinhkhoa.pdf'),
(10008, 'cv_lythiloan.pdf'),
(10010, 'cv_dothingoc.pdf');

-- Thêm dữ liệu vào bảng work_availability cho 12 ứng viê
-- Người dùng 1: Nguyễn Văn An (10001) - Có thể làm việc buổi sáng và tối các ngày T2, T4, T6
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10001, 'Thứ 2', 'Sáng'),
(10001, 'Thứ 2', 'Tối'),
(10001, 'Thứ 4', 'Sáng'),
(10001, 'Thứ 4', 'Tối'),
(10001, 'Thứ 6', 'Sáng'),
(10001, 'Thứ 6', 'Tối');

-- Người dùng 2: Trần Thị Bình (10002) - Có thể làm việc buổi chiều và tối các ngày T3, T5, T7, CN
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10002, 'Thứ 3', 'Chiều'),
(10002, 'Thứ 3', 'Tối'),
(10002, 'Thứ 5', 'Chiều'),
(10002, 'Thứ 5', 'Tối'),
(10002, 'Thứ 7', 'Chiều'),
(10002, 'Thứ 7', 'Tối'),
(10002, 'Chủ nhật', 'Chiều'),
(10002, 'Chủ nhật', 'Tối');

-- Người dùng 3: Lê Văn Cường (10003) - Có thể làm việc buổi tối tất cả các ngày
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10003, 'Thứ 2', 'Tối'),
(10003, 'Thứ 3', 'Tối'),
(10003, 'Thứ 4', 'Tối'),
(10003, 'Thứ 5', 'Tối'),
(10003, 'Thứ 6', 'Tối'),
(10003, 'Thứ 7', 'Tối'),
(10003, 'Chủ nhật', 'Tối');

-- Người dùng 4: Phạm Thị Dung (10004) - Có thể làm việc cả ngày T7, CN và buổi tối các ngày trong tuần
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10004, 'Thứ 2', 'Tối'),
(10004, 'Thứ 3', 'Tối'),
(10004, 'Thứ 4', 'Tối'),
(10004, 'Thứ 5', 'Tối'),
(10004, 'Thứ 6', 'Tối'),
(10004, 'Thứ 7', 'Sáng'),
(10004, 'Thứ 7', 'Chiều'),
(10004, 'Thứ 7', 'Tối'),
(10004, 'Chủ nhật', 'Sáng'),
(10004, 'Chủ nhật', 'Chiều'),
(10004, 'Chủ nhật', 'Tối');

-- Người dùng 5: Hoàng Văn Điệp (10005) - Có thể làm việc chiều T3, T5, tối T6, T7, CN
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10005, 'Thứ 3', 'Chiều'),
(10005, 'Thứ 5', 'Chiều'),
(10005, 'Thứ 6', 'Tối'),
(10005, 'Thứ 7', 'Tối'),
(10005, 'Chủ nhật', 'Tối');

-- Người dùng 6: Ngô Thị Hà (10006) - Có thể làm việc chiều T2-T6
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10006, 'Thứ 2', 'Chiều'),
(10006, 'Thứ 3', 'Chiều'),
(10006, 'Thứ 4', 'Chiều'),
(10006, 'Thứ 5', 'Chiều'),
(10006, 'Thứ 6', 'Chiều');

-- Người dùng 7: Vũ Đình Khoa (10007) - Có thể làm việc sáng và tối T2-T7
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10007, 'Thứ 2', 'Sáng'),
(10007, 'Thứ 2', 'Tối'),
(10007, 'Thứ 3', 'Sáng'),
(10007, 'Thứ 3', 'Tối'),
(10007, 'Thứ 4', 'Sáng'),
(10007, 'Thứ 4', 'Tối'),
(10007, 'Thứ 5', 'Sáng'),
(10007, 'Thứ 5', 'Tối'),
(10007, 'Thứ 6', 'Sáng'),
(10007, 'Thứ 6', 'Tối'),
(10007, 'Thứ 7', 'Sáng'),
(10007, 'Thứ 7', 'Tối');

-- Người dùng 8: Lý Thị Loan (10008) - Có thể làm việc tối T2-T5, cả ngày T7
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10008, 'Thứ 2', 'Tối'),
(10008, 'Thứ 3', 'Tối'),
(10008, 'Thứ 4', 'Tối'),
(10008, 'Thứ 5', 'Tối'),
(10008, 'Thứ 7', 'Sáng'),
(10008, 'Thứ 7', 'Chiều'),
(10008, 'Thứ 7', 'Tối');

-- Người dùng 9: Trịnh Văn Minh (10009) - Có thể làm việc sáng và chiều T2-T6
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10009, 'Thứ 2', 'Sáng'),
(10009, 'Thứ 2', 'Chiều'),
(10009, 'Thứ 3', 'Sáng'),
(10009, 'Thứ 3', 'Chiều'),
(10009, 'Thứ 4', 'Sáng'),
(10009, 'Thứ 4', 'Chiều'),
(10009, 'Thứ 5', 'Sáng'),
(10009, 'Thứ 5', 'Chiều'),
(10009, 'Thứ 6', 'Sáng'),
(10009, 'Thứ 6', 'Chiều');

-- Người dùng 10: Đỗ Thị Ngọc (10010) - Có thể làm việc chiều và tối T6, cả ngày CN
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10010, 'Thứ 6', 'Chiều'),
(10010, 'Thứ 6', 'Tối'),
(10010, 'Chủ nhật', 'Sáng'),
(10010, 'Chủ nhật', 'Chiều'),
(10010, 'Chủ nhật', 'Tối');

-- Người dùng 11: Bùi Văn Phong (10011) - Có thể làm việc tối T3-T7, sáng CN
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10011, 'Thứ 3', 'Tối'),
(10011, 'Thứ 4', 'Tối'),
(10011, 'Thứ 5', 'Tối'),
(10011, 'Thứ 6', 'Tối'),
(10011, 'Thứ 7', 'Tối'),
(10011, 'Chủ nhật', 'Sáng');

-- Người dùng 12: Mai Thị Quỳnh (10012) - Có thể làm việc sáng T2, T4, chiều T3, T5
INSERT INTO work_availability (user_id, ngay, thoi_gian) VALUES
(10012, 'Thứ 2', 'Sáng'),
(10012, 'Thứ 3', 'Chiều'),
(10012, 'Thứ 4', 'Sáng'),
(10012, 'Thứ 5', 'Chiều');
INSERT INTO job_posts (user_id, business_id, tieu_de, mo_ta, yeu_cau, dia_diem, thanh_pho, muc_luong, loai_cv, auto_accept, trang_thai, so_luong, linh_vuc, ngay_het_han) VALUES
-- Công việc từ Cafe Melody (Dương Văn Sơn - 10013, business_id 20000)
(10013, 20000, 'Nhân viên phục vụ bàn buổi tối', 'Cần tuyển nhân viên phục vụ bàn cho ca tối tại Cafe Melody. Công việc bao gồm phục vụ khách hàng, ghi order, dọn dẹp bàn ghế.', 'Nhanh nhẹn, giao tiếp tốt, ngoại hình ưa nhìn, sẵn sàng làm việc ca tối.', '123 Nguyễn Trãi, Quận 1', 'TP Hồ Chí Minh', 25000, 'Bán thời gian', FALSE, 'Mở', 3, 'Nhà hàng - Khách sạn', '2025-06-30 00:00:00'),
(10013, 20000, 'Nhân viên pha chế cuối tuần', 'Tuyển nhân viên pha chế có kinh nghiệm làm việc vào cuối tuần. Được đào tạo thêm nếu chưa có kinh nghiệm.', 'Yêu thích pha chế, cẩn thận, chịu khó học hỏi, có thể làm việc vào T7 và CN.', '123 Nguyễn Trãi, Quận 1', 'TP Hồ Chí Minh', 28000, 'Bán thời gian', FALSE, 'Mở', 2, 'Nhà hàng - Khách sạn', '2025-06-15 00:00:00'),

-- Nhà hàng Phố Xưa (Trương Thị Thanh - 10014, business_id 20001)
(10014, 20001, 'Nhân viên phục vụ bàn ca sáng', 'Nhà hàng Phố Xưa cần tuyển nhân viên phục vụ bàn ca sáng từ 8h-14h. Môi trường làm việc thân thiện, có thưởng.', 'Chăm chỉ, nhiệt tình, có tinh thần trách nhiệm, ưu tiên ứng viên có kinh nghiệm.', '45 Trần Hưng Đạo, Hoàn Kiếm', 'Hà Nội', 22000, 'Bán thời gian', TRUE, 'Mở', 2, 'Nhà hàng - Khách sạn', '2025-05-20 00:00:00'),
(10014, 20001, 'Nhân viên phụ bếp buổi tối', 'Cần tuyển nhân viên phụ bếp làm việc từ 17h-22h các ngày trong tuần. Công việc bao gồm chuẩn bị nguyên liệu, phụ đầu bếp, dọn dẹp bếp.', 'Chịu được áp lực cao, cẩn thận, sạch sẽ, ưu tiên nam giới.', '45 Trần Hưng Đạo, Hoàn Kiếm', 'Hà Nội', 25000, 'Bán thời gian', FALSE, 'Mở', 1, 'Nhà hàng - Khách sạn', '2025-05-30 00:00:00'),

-- SpeedBuy Store (Phan Văn Uy - 10015, business_id 20002)
(10015, 20002, 'Nhân viên bán hàng cuối tuần', 'SpeedBuy Store cần tuyển nhân viên bán hàng làm việc vào T7, CN. Công việc bao gồm tư vấn khách hàng, sắp xếp hàng hóa, thanh toán.', 'Ngoại hình ưa nhìn, giao tiếp tốt, thân thiện với khách hàng.', '72 Nguyễn Thị Minh Khai, Hải Châu', 'Đà Nẵng', 23000, 'Bán thời gian', TRUE, 'Mở', 3, 'Bán lẻ - Siêu thị', '2025-04-30 00:00:00'),
(10015, 20002, 'Nhân viên kho hàng buổi sáng', 'Cần tuyển nhân viên kho làm việc buổi sáng từ 7h-12h. Công việc bao gồm kiểm hàng, sắp xếp kho, hỗ trợ giao hàng.', 'Sức khỏe tốt, cẩn thận, ưu tiên nam giới.', '72 Nguyễn Thị Minh Khai, Hải Châu', 'Đà Nẵng', 20000, 'Bán thời gian', FALSE, 'Mở', 2, 'Bán lẻ - Siêu thị', '2025-05-15 23:59:59'),

-- Công việc từ Thời trang BlueStar (Hồ Thị Vân - 10016, business_id 20003)
(10016, 20003, 'Nhân viên bán hàng thời trang', 'Tuyển nhân viên bán hàng thời trang, tư vấn khách hàng, trưng bày sản phẩm.', 'Ngoại hình ưa nhìn, yêu thích thời trang, giao tiếp tốt.', '25 Trần Phú', 'Khánh Hòa', 22000, 'Bán thời gian', TRUE, 'Mở', 2, 'Bán lẻ - Siêu thị', '2025-06-10 23:59:59'),
(10016, 20003, 'Nhân viên kiểm hàng thời vụ', 'Kiểm tra hàng hóa nhập kho, hỗ trợ đóng gói đơn hàng.', 'Cẩn thận, nhanh nhẹn, sẵn sàng làm thời vụ.', '25 Trần Phú', 'Khánh Hòa', 18000, 'Thời vụ', FALSE, 'Mở', 3, 'Bán lẻ - Siêu thị', '2025-04-20 23:59:59'),

-- Công việc từ FastGo Express (Diệp Văn Xuân - 10017, business_id 20004)
(10017, 20004, 'Nhân viên giao hàng bán thời gian', 'Giao hàng trong nội thành Cần Thơ, làm việc linh hoạt theo ca.', 'Có xe máy, thông thạo đường phố, chịu khó.', '103 Nguyễn Văn Cừ', 'Cần Thơ', 25000, 'Bán thời gian', TRUE, 'Mở', 5, 'Giao hàng - Vận chuyển', '2025-05-30 23:59:59'),
(10017, 20004, 'Nhân viên chăm sóc khách hàng', 'Hỗ trợ khách hàng qua điện thoại, xử lý đơn hàng.', 'Giọng nói dễ nghe, kỹ năng giao tiếp tốt.', '103 Nguyễn Văn Cừ', 'Cần Thơ', 20000, 'Bán thời gian', FALSE, 'Mở', 2, 'Dịch vụ khách hàng', '2025-06-15 23:59:59'),

-- Công việc từ Trung tâm Anh ngữ SpeakWell (Lương Thị Yến - 10018, business_id 20005)
(10018, 20005, 'Gia sư tiếng Anh bán thời gian', 'Dạy tiếng Anh cho học sinh tiểu học và THCS.', 'Tốt nghiệp ĐH ngành tiếng Anh, nhiệt tình, kiên nhẫn.', '88 Cách Mạng Tháng 8', 'TP Hồ Chí Minh', 30000, 'Bán thời gian', FALSE, 'Mở', 3, 'Giáo dục - Gia sư', '2025-05-25 23:59:59'),
(10018, 20005, 'Nhân viên phát triển nội dung', 'Soạn giáo trình tiếng Anh, hỗ trợ tổ chức lớp học.', 'Kỹ năng viết tốt, hiểu biết về giáo dục.', '88 Cách Mạng Tháng 8', 'TP Hồ Chí Minh', 25000, 'Thời vụ', FALSE, 'Đóng', 1, 'Giáo dục - Gia sư', '2025-03-31 23:59:59'),

-- Công việc từ EventPro Solutions (Tạ Văn Đức - 10019, business_id 20006)
(10019, 20006, 'Nhân viên tổ chức sự kiện thời vụ', 'Hỗ trợ chuẩn bị và tổ chức sự kiện cuối tuần.', 'Năng động, chịu được áp lực, làm việc nhóm tốt.', '55 Lý Thường Kiệt', 'Hà Nội', 28000, 'Thời vụ', TRUE, 'Mở', 4, 'Giải trí - Sự kiện', '2025-06-01 23:59:59'),
(10019, 20006, 'Nhân viên thiết kế backdrop', 'Thiết kế backdrop, banner cho sự kiện.', 'Sử dụng thành thạo Photoshop, có kinh nghiệm thiết kế.', '55 Lý Thường Kiệt', 'Hà Nội', 30000, 'Bán thời gian', FALSE, 'Mở', 1, 'Thiết kế - Đồ họa', '2025-05-20 23:59:59'),

-- Công việc cá nhân từ nhà tuyển dụng
(10013, NULL, 'Nhân viên pha chế tự do', 'Pha chế đồ uống tại quán cafe nhỏ, làm việc linh hoạt.', 'Yêu thích pha chế, sẵn sàng học hỏi.', '56 Lê Lợi', 'TP Hồ Chí Minh', 26000, 'Bán thời gian', FALSE, 'Mở', 1, 'Nhà hàng - Khách sạn', '2025-04-30 23:59:59'),
(10014, NULL, 'Nhân viên dọn dẹp nhà hàng', 'Dọn dẹp nhà hàng sau giờ đóng cửa.', 'Chăm chỉ, sạch sẽ, làm việc tối.', '78 Trần Phú', 'Hà Nội', 18000, 'Bán thời gian', FALSE, 'Đóng', 1, 'Nhà hàng - Khách sạn', '2025-03-25 23:59:59'),
(10015, NULL, 'Nhân viên giao hàng tự do', 'Giao hàng cho shop nhỏ, làm việc theo ca.', 'Có xe máy, thông thạo Đà Nẵng.', '12 Nguyễn Huệ', 'Đà Nẵng', 24000, 'Bán thời gian', TRUE, 'Mở', 2, 'Giao hàng - Vận chuyển', '2025-05-10 23:59:59'),
(10016, NULL, 'Nhân viên bán hàng thời trang', 'Bán hàng tại cửa hàng cá nhân, làm việc cuối tuần.', 'Thân thiện, yêu thích thời trang.', '34 Hùng Vương', 'Khánh Hòa', 20000, 'Bán thời gian', FALSE, 'Mở', 1, 'Bán lẻ - Siêu thị', '2025-04-15 23:59:59'),
(10017, NULL, 'Nhân viên giao hàng ca tối', 'Giao hàng thực phẩm tối từ 18h-22h.', 'Có xe máy, chịu khó.', '67 Phạm Ngũ Lão', 'Cần Thơ', 23000, 'Bán thời gian', TRUE, 'Mở', 2, 'Giao hàng - Vận chuyển', '2025-05-05 23:59:59'),
(10018, NULL, 'Gia sư toán cấp 2', 'Dạy toán cho học sinh cấp 2 tại nhà.', 'Tốt nghiệp ĐH sư phạm, kiên nhẫn.', '89 Lê Đại Hành', 'TP Hồ Chí Minh', 28000, 'Bán thời gian', FALSE, 'Mở', 1, 'Giáo dục - Gia sư', '2025-06-20 23:59:59');
INSERT INTO job_schedules (job_id, thu, gio_bat_dau, gio_ket_thuc, linh_hoat) VALUES
(6, 'T2', '07:00:00', '12:00:00', FALSE), -- Nhân viên kho hàng buổi sáng
(7, 'T6', '14:00:00', '18:00:00', FALSE), -- Nhân viên bán hàng thời trang
(8, NULL, NULL, NULL, TRUE), -- Nhân viên kiểm hàng thời vụ (linh hoạt)
(9, NULL, NULL, NULL, TRUE), -- Nhân viên giao hàng bán thời gian
(10, 'T3', '17:00:00', '21:00:00', FALSE), -- Nhân viên chăm sóc khách hàng
(11, 'T4', '18:00:00', '21:00:00', FALSE), -- Gia sư tiếng Anh
(12, 'T7', '09:00:00', '13:00:00', FALSE), -- Nhân viên phát triển nội dung
(13, 'CN', '08:00:00', '16:00:00', FALSE), -- Nhân viên tổ chức sự kiện
(14, 'T5', '14:00:00', '18:00:00', FALSE), -- Nhân viên thiết kế backdrop
(15, NULL, NULL, NULL, TRUE), -- Nhân viên pha chế tự do
(16, 'T2', '22:00:00', '23:30:00', FALSE), -- Nhân viên dọn dẹp nhà hàng
(17, NULL, NULL, NULL, TRUE), -- Nhân viên giao hàng tự do
(18, 'T7', '14:00:00', '18:00:00', FALSE), -- Nhân viên bán hàng thời trang
(19, 'T5', '18:00:00', '22:00:00', FALSE), -- Nhân viên giao hàng ca tối
(20, 'T3', '17:00:00', '20:00:00', FALSE); -- Gia sư toán cấp 2

INSERT INTO applications (job_id, user_id, Thu_ung_tuyen, trang_thai, ngay_ung_tuyen) VALUES
(1, 10001, 'Tôi có kinh nghiệm phục vụ quán cafe 1 năm.', 'Đang chờ', '2025-03-10 09:00:00'),
(2, 10002, 'Tôi từng pha chế tại quán trà sữa.', 'Đã chấp nhận', '2025-03-11 14:00:00'),
(3, 10003, 'Tôi sẵn sàng làm ca sáng, có kinh nghiệm phục vụ.', 'Đang chờ', '2025-03-12 10:00:00'),
(4, 10004, 'Tôi từng làm phụ bếp 6 tháng.', 'Đã từ chối', '2025-03-13 11:00:00'),
(5, 10005, 'Tôi thích bán hàng và làm việc cuối tuần.', 'Đã chấp nhận', '2025-03-14 13:00:00'),
(6, 10006, 'Tôi có sức khỏe tốt, sẵn sàng làm kho.', 'Đang chờ', '2025-03-15 09:30:00'),
(7, 10007, 'Tôi yêu thích thời trang và giao tiếp tốt.', 'Đã chấp nhận', '2025-03-16 14:00:00'),
(8, 10008, 'Tôi cẩn thận, sẵn sàng kiểm hàng.', 'Đang chờ', '2025-03-17 10:00:00'),
(9, 10009, 'Tôi có xe máy, từng giao hàng 1 năm.', 'Đã chấp nhận', '2025-03-18 15:00:00'),
(10, 10010, 'Tôi có kỹ năng giao tiếp, từng làm CSKH.', 'Đã từ chối', '2025-03-19 11:00:00'),
(11, 10011, 'Tôi tốt nghiệp ĐH tiếng Anh, sẵn sàng dạy.', 'Đang chờ', '2025-03-20 13:00:00'),
(12, 10012, 'Tôi có kinh nghiệm soạn giáo trình.', 'Đã từ chối', '2025-03-21 09:00:00'),
(13, 10001, 'Tôi năng động, từng hỗ trợ tổ chức sự kiện.', 'Đã chấp nhận', '2025-03-22 14:00:00'),
(14, 10002, 'Tôi biết dùng Photoshop, có mẫu thiết kế.', 'Đang chờ', '2025-03-23 10:30:00'),
(15, 10003, 'Tôi thích pha chế và sẵn sàng học thêm.', 'Đã chấp nhận', '2025-03-24 15:00:00'),
(16, 10004, 'Tôi chăm chỉ, sẵn sàng dọn dẹp ca tối.', 'Đã từ chối', '2025-03-25 11:00:00'),
(17, 10005, 'Tôi có xe máy, muốn giao hàng linh hoạt.', 'Đang chờ', '2025-03-26 13:00:00'),
(18, 10006, 'Tôi yêu thích thời trang, từng bán hàng.', 'Đã chấp nhận', '2025-03-27 14:00:00'),
(19, 10007, 'Tôi có thể giao hàng ca tối.', 'Đang chờ', '2025-03-28 10:00:00'),
(20, 10008, 'Tôi tốt nghiệp ĐH sư phạm, dạy toán tốt.', 'Đã chấp nhận', '2025-03-29 15:00:00');

INSERT INTO messages (sender_id, receiver_id, content, da_doc, ngay_gui) VALUES
(10001, 10013, 'Chào anh, ca tối bắt đầu mấy giờ?', FALSE, '2025-03-10 10:00:00'),
(10013, 10001, 'Chào em, từ 18h-22h nhé.', TRUE, '2025-03-10 11:00:00'),
(10002, 10013, 'Pha chế có cần kinh nghiệm không?', FALSE, '2025-03-11 15:00:00'),
(10013, 10002, 'Không cần, sẽ đào tạo.', TRUE, '2025-03-11 16:00:00'),
(10005, 10015, 'Công việc bán hàng lương thế nào?', FALSE, '2025-03-14 14:00:00'),
(10015, 10005, '23k/giờ, có thưởng thêm.', TRUE, '2025-03-14 15:00:00'),
(10009, 10017, 'Giao hàng có hỗ trợ xăng không?', FALSE, '2025-03-18 16:00:00'),
(10017, 10009, 'Có, hỗ trợ 500k/tháng.', TRUE, '2025-03-18 17:00:00'),
(10011, 10018, 'Dạy tiếng Anh lương bao nhiêu?', FALSE, '2025-03-20 14:00:00'),
(10018, 10011, '30k/giờ, dạy 3 buổi/tuần.', TRUE, '2025-03-20 15:00:00');
INSERT INTO reports (reporter_id, reported_id, job_id, ly_do, trang_thai, ngay_bao_cao) VALUES
(10001, 10013, 1, 'Công việc yêu cầu làm thêm giờ không lương.', 'Đang chờ', '2025-03-11 09:00:00'),
(10002, 10013, 2, 'Nhà tuyển dụng không trả lời sau khi chấp nhận.', 'Đã xem xét', '2025-03-12 10:00:00'),
(10003, 10014, 3, 'Mô tả công việc không đúng thực tế.', 'Đang chờ', '2025-03-13 11:00:00'),
(10004, 10014, 4, 'Yêu cầu làm việc ngoài giờ không báo trước.', 'Đã xem xét', '2025-03-14 12:00:00'),
(10005, 10015, 5, 'Lương thấp hơn quảng cáo.', 'Đang chờ', '2025-03-15 13:00:00'),
(10006, 10015, 6, 'Công việc quá nặng, không phù hợp.', 'Đã xem xét', '2025-03-16 14:00:00'),
(10007, 10016, 7, 'Nhà tuyển dụng không chuyên nghiệp.', 'Đang chờ', '2025-03-17 15:00:00'),
(10008, 10016, 8, 'Thời gian làm việc không rõ ràng.', 'Đã xem xét', '2025-03-18 16:00:00'),
(10009, 10017, 9, 'Không hỗ trợ xăng như cam kết.', 'Đang chờ', '2025-03-19 17:00:00'),
(10010, 10017, 10, 'Yêu cầu làm việc quá sức.', 'Đã xem xét', '2025-03-20 18:00:00');

INSERT INTO notifications (user_id, business_id, thong_bao, trang_thai, thoi_gian_thong_bao) VALUES
(10001, NULL, 'Đơn ứng tuyển của bạn đã được gửi.', 'chưa đọc', '2025-03-10 09:05:00'),
(NULL, 20000, 'Có ứng viên mới cho công việc phục vụ bàn.', 'đã đọc', '2025-03-10 10:05:00'),
(10002, NULL, 'Đơn ứng tuyển của bạn được chấp nhận.', 'chưa đọc', '2025-03-11 14:05:00'),
(10004, NULL, 'Đơn ứng tuyển của bạn bị từ chối.', 'đã đọc', '2025-03-13 11:05:00'),
(NULL, 20002, 'Có ứng viên mới cho công việc bán hàng.', 'chưa đọc', '2025-03-14 13:05:00'),
(10005, NULL, 'Đơn ứng tuyển của bạn được chấp nhận.', 'đã đọc', '2025-03-14 14:05:00'),
(10007, NULL, 'Đơn ứng tuyển của bạn được chấp nhận.', 'chưa đọc', '2025-03-16 14:05:00'),
(NULL, 20004, 'Có ứng viên mới cho công việc giao hàng.', 'đã đọc', '2025-03-18 15:05:00'),
(10009, NULL, 'Đơn ứng tuyển của bạn được chấp nhận.', 'chưa đọc', '2025-03-18 15:05:00'),
(10011, NULL, 'Đơn ứng tuyển của bạn đã được gửi.', 'đã đọc', '2025-03-20 13:05:00');
INSERT INTO saved_jobs (user_id, job_id, ngay_luu) VALUES
(10001, 1, '2025-03-10 08:30:00'),
(10002, 2, '2025-03-11 13:00:00'),
(10003, 3, '2025-03-12 09:00:00'),
(10004, 4, '2025-03-13 10:00:00'),
(10005, 5, '2025-03-14 12:00:00'),
(10006, 6, '2025-03-15 09:00:00'),
(10007, 7, '2025-03-16 13:00:00'),
(10008, 8, '2025-03-17 10:00:00'),
(10009, 9, '2025-03-18 14:00:00'),
(10010, 10, '2025-03-19 11:00:00');