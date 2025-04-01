-- Tạo database với bộ mã hóa utf8mb4
CREATE DATABASE IF NOT EXISTS HETHONGTIMVIEC 
    DEFAULT CHARACTER SET utf8mb4 
    DEFAULT COLLATE utf8mb4_unicode_ci;
USE HETHONGTIMVIEC;

-- Thiết lập bộ mã hóa mặc định
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
SET collation_connection = 'utf8mb4_unicode_ci';
-- Xóa các bảng nếu đã tồn tại
DROP TABLE IF EXISTS saved_jobs;
DROP TABLE IF EXISTS notifications;
DROP TABLE IF EXISTS reports;
DROP TABLE IF EXISTS messages;
DROP TABLE IF EXISTS applications;
DROP TABLE IF EXISTS job_posts;
DROP TABLE IF EXISTS password_reset;
DROP TABLE IF EXISTS company_members;
DROP TABLE IF EXISTS companies;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS CV;
DROP TABLE IF EXISTS work_availability;
-- Bảng người dùng
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,  
    Ho_ten VARCHAR(255) NOT NULL,  
    gioi_tinh ENUM('Nam', 'Nữ') NOT NULL,  
    email VARCHAR(320) UNIQUE DEFAULT NULL,  
    facebook_id VARCHAR(255) UNIQUE DEFAULT NULL,  
    mat_khau VARCHAR(60) NOT NULL,
    Ngay_sinh DATE DEFAULT NULL,  
    SDT VARCHAR(10) NOT NULL UNIQUE,  
    thanh_pho VARCHAR(255) NOT NULL,  
    vai_tro ENUM('Ứng viên', 'Nhà tuyển dụng', 'Quản trị viên') DEFAULT 'Ứng viên',  
    trang_thai ENUM('Đang xử lý', 'Chấp thuận', 'Bị cấm') DEFAULT 'Đang xử lý',  
    avatar VARCHAR(255),  
    Mo_ta VARCHAR(500),  
    Ngay_tao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci AUTO_INCREMENT=10000;

-- Bảng công ty
CREATE TABLE companies (
    company_id INT AUTO_INCREMENT PRIMARY KEY,  
    owner_id INT NOT NULL,  
    ten VARCHAR(255) NOT NULL,  
    SDT VARCHAR(10) NOT NULL UNIQUE,  
    email VARCHAR(255) UNIQUE NOT NULL,  
    linh_vuc VARCHAR(255),  
    nam_thanh_lap DATE NOT NULL,  
    dia_chi TEXT NOT NULL,  
    giay_phep_kinh_doanh VARCHAR(255) UNIQUE NOT NULL,  
    ma_so_thue VARCHAR(20) UNIQUE,  
    quy_mo ENUM('1-10', '11-50', '51-100', '101-500', '500+') NOT NULL DEFAULT '1-10',  
    website VARCHAR(255),  
    social_links TEXT,  
    logo VARCHAR(255),  
    trang_thai ENUM('đang xử lý', 'chấp thuận', 'bị cấm') DEFAULT 'đang xử lý',  
    ngay_tao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    CONSTRAINT fk_owner FOREIGN KEY (owner_id) REFERENCES users(user_id) 
        ON DELETE CASCADE ON UPDATE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci AUTO_INCREMENT=20000;

-- Bảng CV
CREATE TABLE CV(
    cv_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,  
    file_path VARCHAR(255) NOT NULL,  -- Lưu đường dẫn file CV
    ngay_tao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng thời gian làm việc
CREATE TABLE work_availability (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,  
    ngay ENUM('Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật') NOT NULL,
    thoi_gian ENUM('Sáng', 'Chiều', 'Tối') NOT NULL,  
    trang_thai ENUM('Có thể làm việc', 'Không thể làm việc') NOT NULL,  
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng thành viên công ty
CREATE TABLE company_members (
    member_id INT AUTO_INCREMENT PRIMARY KEY,  
    company_id INT NOT NULL,
    user_id INT NOT NULL,
    vai_tro ENUM('Nhân viên', 'Quản lý') DEFAULT 'Nhân viên',
    tinh_trang ENUM('Đang hoạt động', 'Tạm ngừng', 'Bị cấm') DEFAULT 'Đang hoạt động',
    tham_gia TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, user_id),
    FOREIGN KEY (company_id) REFERENCES companies(company_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng đặt lại mật khẩu
CREATE TABLE password_reset (
    password_reset_id INT AUTO_INCREMENT PRIMARY KEY,  
    SDT VARCHAR(10) NOT NULL,  
    email VARCHAR(255) DEFAULT NULL,  
    facebook_id VARCHAR(255) DEFAULT NULL,  
    ngay_yeu_cau TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    CONSTRAINT chk_email_or_facebook_reset CHECK (email IS NOT NULL OR facebook_id IS NOT NULL),  
    FOREIGN KEY (SDT) REFERENCES users(SDT) ON DELETE CASCADE  
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng bài đăng việc làm
CREATE TABLE job_posts (
    job_id INT AUTO_INCREMENT PRIMARY KEY,  
    user_id INT DEFAULT NULL,  
    company_id INT DEFAULT NULL,  
    tieu_de VARCHAR(255) NOT NULL,  
    mo_ta TEXT NOT NULL,  
    yeu_cau TEXT DEFAULT NULL,  
    dia_diem VARCHAR(255) NOT NULL,  
    thanh_pho VARCHAR(255) NOT NULL,  
    muc_luong INT DEFAULT NULL,  
    loai_cv ENUM('Bán thời gian', 'Thời vụ') NOT NULL,  
    auto_accept BOOLEAN DEFAULT FALSE,  
    trang_thai ENUM('Mở', 'Đóng') DEFAULT 'Mở',  
    so_luong INT DEFAULT 1,  
    linh_vuc VARCHAR(255) DEFAULT NULL,  
    ngay_dang TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    ngay_het_han TIMESTAMP NOT NULL,  
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL ON UPDATE CASCADE,
    FOREIGN KEY (company_id) REFERENCES companies(company_id) ON DELETE SET NULL ON UPDATE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng ứng tuyển
CREATE TABLE applications (
    application_id INT AUTO_INCREMENT PRIMARY KEY,  
    job_id INT,  
    user_id INT,  
    Thu_ung_tuyen TEXT,  
    trang_thai ENUM('Đang chờ', 'Đã chấp nhận', 'Đã từ chối') DEFAULT 'Đang chờ',  
    ngay_ung_tuyen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (job_id) REFERENCES job_posts(job_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng tin nhắn
CREATE TABLE messages (
    message_id INT AUTO_INCREMENT PRIMARY KEY,  
    sender_id INT,  
    receiver_id INT,  
    content TEXT,  
    da_doc BOOLEAN DEFAULT FALSE,  
    ngay_gui TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (sender_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (receiver_id) REFERENCES users(user_id) ON DELETE CASCADE
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng báo cáo
CREATE TABLE reports (
    report_id INT AUTO_INCREMENT PRIMARY KEY,  
    reporter_id INT,  
    reported_id INT,  
    job_id INT DEFAULT NULL,  
    ly_do TEXT NOT NULL,  
    trang_thai ENUM('Đang chờ', 'Đã xem xét') DEFAULT 'Đang chờ',  
    ngay_bao_cao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (reporter_id) REFERENCES users(user_id) ON DELETE CASCADE,  
    FOREIGN KEY (reported_id) REFERENCES users(user_id) ON DELETE CASCADE,  
    FOREIGN KEY (job_id) REFERENCES job_posts(job_id) ON DELETE SET NULL  
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng thông báo
CREATE TABLE notifications (
    notification_id INT AUTO_INCREMENT PRIMARY KEY,  
    user_id INT,  
    thong_bao TEXT,  
    trang_thai ENUM('chưa đọc', 'đã đọc') DEFAULT 'chưa đọc',  
    thoi_gian_thong_bao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE  
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng công việc đã lưu
CREATE TABLE saved_jobs (
    saved_job_id INT AUTO_INCREMENT PRIMARY KEY,  
    user_id INT,  
    job_id INT,  
    ngay_luu TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,  
    FOREIGN KEY (job_id) REFERENCES job_posts(job_id) ON DELETE CASCADE  
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Bảng users: INDEX giúp tìm kiếm nhanh theo email, SDT, và vai trò
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_SDT ON users(SDT);
CREATE INDEX idx_users_vai_tro ON users(vai_tro);

-- Bảng companies: INDEX giúp tìm kiếm nhanh theo tên công ty và mã số thuế
CREATE INDEX idx_companies_ten ON companies(ten);
CREATE INDEX idx_companies_ma_so_thue ON companies(ma_so_thue);

-- Bảng CV: INDEX giúp tìm nhanh CV theo user_id
CREATE INDEX idx_CV_user ON CV(user_id);

-- Bảng work_availability: INDEX giúp tìm kiếm nhanh theo user_id và ngày làm việc
CREATE INDEX idx_work_user ON work_availability(user_id);
CREATE INDEX idx_work_ngay ON work_availability(ngay);

-- Bảng company_members: INDEX giúp tìm nhanh nhân sự trong công ty
CREATE INDEX idx_company_members_company ON company_members(company_id);
CREATE INDEX idx_company_members_user ON company_members(user_id);

-- Bảng job_posts: INDEX giúp tìm nhanh công việc theo công ty, loại công việc, lương, và thành phố
CREATE INDEX idx_jobs_company ON job_posts(company_id);
CREATE INDEX idx_jobs_loai_cv ON job_posts(loai_cv);
CREATE INDEX idx_jobs_muc_luong ON job_posts(muc_luong);
CREATE INDEX idx_jobs_thanh_pho ON job_posts(thanh_pho);

-- Bảng applications: INDEX giúp tìm kiếm nhanh các ứng tuyển của user và công việc
CREATE INDEX idx_applications_user ON applications(user_id);
CREATE INDEX idx_applications_job ON applications(job_id);

-- Bảng messages: INDEX giúp tìm kiếm tin nhắn nhanh giữa sender và receiver
CREATE INDEX idx_messages_sender ON messages(sender_id);
CREATE INDEX idx_messages_receiver ON messages(receiver_id);

-- Bảng reports: INDEX giúp tìm báo cáo nhanh theo người báo cáo và bài viết liên quan
CREATE INDEX idx_reports_reporter ON reports(reporter_id);
CREATE INDEX idx_reports_job ON reports(job_id);

-- Bảng notifications: INDEX giúp tìm thông báo theo user_id
CREATE INDEX idx_notifications_user ON notifications(user_id);

-- Bảng saved_jobs: INDEX giúp tìm nhanh công việc đã lưu theo user_id
CREATE INDEX idx_saved_jobs_user ON saved_jobs(user_id);
CREATE INDEX idx_saved_jobs_job ON saved_jobs(job_id);