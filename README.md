# HỆ THỐNG TÌM KIẾM VIỆC LÀM BÁN THỜI GIAN VÀ THỜI VỤ (PBL3)

Một ứng dụng web full-stack toàn diện giúp kết nối người tìm việc với các cơ hội việc làm bán thời gian, thời vụ một cách nhanh chóng và hiệu quả. Nền tảng cho phép nhà tuyển dụng và người tìm việc **trò chuyện trực tiếp**, nhận **thông báo tự động** về các hoạt động quan trọng, và quản lý toàn bộ quy trình tuyển dụng - ứng tuyển một cách chuyên nghiệp.

🌟 **Tổng quan dự án**

Dự án này là sản phẩm của học phần Lập trình dựa trên dự án 3 (Project-Based Learning 3), được xây dựng để giải quyết một thách thức thực tế: sự thiếu hụt một nền tảng tập trung, uy tín và dễ sử dụng cho thị trường việc làm linh hoạt.

Thị trường này vô cùng đa dạng, từ các bạn sinh viên đang tìm kiếm công việc đầu đời để tích lũy kinh nghiệm, cho đến những người lao động lành nghề muốn có thêm thu nhập, hay đơn giản là bất kỳ ai đang tìm kiếm sự linh hoạt trong công việc. Tuy nhiên, các nền tảng hiện tại thường rời rạc, thiếu thông tin xác thực, và chưa thực sự phục vụ tốt cho tất cả các đối tượng này. Trong khi đó, nhà tuyển dụng cũng cần một công cụ hiệu quả để tiếp cận và giao tiếp với đúng ứng viên.

Để giải quyết những thách thức trên, chúng tôi đã phát triển một ứng dụng web full-stack với mục tiêu tạo ra một sân chơi công bằng và minh bạch, nơi mọi người, **dù có kinh nghiệm hay không**, đều có thể tìm thấy cơ hội phù hợp. Nền tảng được trang bị các tính năng cốt lõi như đăng tin, tìm kiếm, quản lý hồ sơ, cùng với **hệ thống nhắn tin theo ngữ cảnh** và **hệ thống thông báo toàn diện** để xóa bỏ rào cản giao tiếp và nâng cao trải nghiệm người dùng.

👥 **Thành viên Nhóm**

Dự án được phát triển bởi nhóm "Pbl3-Group", bao gồm các thành viên:

*   [@Nguyễn Thanh Huyền](https://github.com/Chizk23)- Nguyễn Thanh Huyền
*   [@Bích Uyên](https://github.com/BichUyen2609)- Nguyễn Thị Bích Uyên
*   [@Phượng Trần](https://github.com/PhuongTran2212)- Trần Thị Phượng

---

🚀 **Công nghệ sử dụng**

### Backend

*   **Ngôn ngữ:** C#
*   **Framework:** ASP.NET Core MVC (.NET 8.0)
*   **Database:** **MySQL** (v8.0+)
*   **ORM:** Entity Framework Core (sử dụng `Pomelo.EntityFrameworkCore.MySql`)
*   **Authentication:** ASP.NET Core Identity, Cookie Authentication

### Frontend

*   **Ngôn ngữ:** HTML, CSS, JavaScript (jQuery, AJAX)
*   **Template Engine:** Razor View
*   **CSS Framework:** Bootstrap
*   **Library:** Chart.js (cho dashboard), Select2 (cho tìm kiếm động).

### Development Tools

*   **IDE:** Visual Studio / Visual Studio Code
*   **Version Control:** Git & GitHub
*   **Database Management:** **MySQL Workbench**
*   **Library:** ClosedXML (cho chức năng xuất Excel).

✨ **Các chức năng chính**

<details>
<summary><b>👨‍💻 dành cho Người Tìm Việc (Ứng viên)</b></summary>

#### 🔍 1. Tìm kiếm & Khám phá việc làm
*   **Tìm kiếm Nâng cao:** Tìm việc làm theo từ khóa, ngành nghề, địa điểm (tỉnh/thành, quận/huyện).
*   **Bộ lọc Thông minh:** Lọc kết quả theo loại hình công việc, **khoảng lương**, kinh nghiệm, học vấn, ca làm việc, và tin tuyển gấp.
*   **Gợi ý việc làm phù hợp:** Hệ thống tự động tính điểm phù hợp (%) dựa trên sự tương thích về **địa điểm, lịch rảnh, ngành nghề và mức lương mong muốn** của ứng viên so với tin đăng.

#### 👤 2. Quản lý Hồ sơ cá nhân Toàn diện
*   **Hồ sơ đa thành phần:** Quản lý riêng biệt thông tin tài khoản, hồ sơ chuyên môn, lịch làm việc mong muốn và khu vực làm việc yêu thích.
*   **Xây dựng hồ sơ chuyên nghiệp:** Giới thiệu bản thân, vị trí mong muốn, mức lương kỳ vọng và tải lên CV mặc định.
*   **Thiết lập linh hoạt:** Tùy chỉnh lịch rảnh theo từng buổi trong tuần và chọn nhiều khu vực làm việc để nhận gợi ý chính xác.

#### ✍️ 3. Hệ thống Ứng tuyển & Quản lý Thông minh
*   **Ứng tuyển Linh hoạt:** Nộp hồ sơ trực tiếp, có thể **tải lên một CV khác** cho từng công việc hoặc viết thư giới thiệu riêng.
*   **Quản lý Việc đã ứng tuyển:** Theo dõi trạng thái chi tiết (đã nộp, NTD đã xem, chấp nhận, từ chối), sửa thông tin, **rút đơn ứng tuyển** và **hoàn tác việc rút đơn** trong thời gian cho phép.

#### ❤️ 4. Quản lý Việc làm yêu thích & Báo cáo
*   **Lưu việc làm:** Lưu lại các tin tuyển dụng quan tâm để xem lại hoặc ứng tuyển sau.
*   **Báo cáo Vi phạm:** Báo cáo các tin tuyển dụng có dấu hiệu lừa đảo, sai sự thật và theo dõi trạng thái xử lý báo cáo của mình.

</details>

<details>
<summary><b>🏢 dành cho Nhà Tuyển Dụng (Cá nhân & Doanh nghiệp)</b></summary>

#### 📋 1. Quản lý Tin tuyển dụng Chuyên nghiệp
*   **Đăng tin Dễ dàng:** Form đăng tin chi tiết, hỗ trợ đầy đủ các trường thông tin.
*   **Quản lý Toàn diện:** Xem danh sách, chỉnh sửa, xóa (ẩn), **đăng lại nhanh (Repost)**, tạm ẩn/hiện, và đánh dấu đã tuyển.
*   **Chuyển đổi vai trò:** Người dùng cá nhân có thể chuyển đổi linh hoạt giữa giao diện "Tìm việc" và "Tuyển dụng" chỉ với một cú nhấp chuột.

#### 👨‍💼 2. Quản lý Ứng viên Hiệu quả
*   **Danh sách tập trung:** Xem tất cả ứng viên đã nộp hồ sơ, lọc theo từng tin tuyển dụng hoặc trạng thái hồ sơ.
*   **Tương tác với ứng viên:** Xem chi tiết hồ sơ, CV, thư giới thiệu và thay đổi trạng thái ứng tuyển (chấp nhận, từ chối). Trạng thái sẽ được **thông báo tự động** đến ứng viên.

</details>

<details>
<summary><b>💬 Hệ thống Nhắn tin theo Ngữ cảnh</b></summary>

*   **Trò chuyện trực tiếp:** Cho phép nhà tuyển dụng và ứng viên nhắn tin trực tiếp với nhau.
*   **Ngữ cảnh rõ ràng:** Mỗi cuộc hội thoại được gắn với một **tin tuyển dụng** hoặc một **đơn ứng tuyển** cụ thể, giúp cả hai bên dễ dàng theo dõi.
*   **Giao diện trực quan:** Giao diện chat quen thuộc, hiển thị danh sách hội thoại, tin nhắn mới, và thông tin chi tiết của người liên hệ.

</details>

<details>
<summary><b>🔔 Hệ thống Thông báo Toàn diện</b></summary>

*   **Thông báo tự động:**
    *   **Ứng viên:** Nhận thông báo khi NTD **xem hồ sơ**, **chấp nhận/từ chối** đơn; khi có tin nhắn mới.
    *   **Nhà tuyển dụng:** Nhận thông báo khi có **ứng viên mới**, hoặc khi ứng viên **rút đơn**.
    *   **Tài khoản:** Nhận thông báo khi tin đăng được duyệt/từ chối, hồ sơ được xác minh, hoặc khi có cảnh báo.
*   **Trung tâm Thông báo:** Giao diện tập trung để người dùng xem, quản lý, đánh dấu đã đọc và xóa thông báo.
*   **Cập nhật Real-time (Mô phỏng):** Hiển thị số lượng thông báo chưa đọc ngay trên thanh điều hướng.

</details>

<details>
<summary><b>🛡️ dành cho Quản trị viên (Admin)</b></summary>

#### 📊 1. Dashboard Phân tích Chuyên sâu
*   **Thống kê trực quan:** Biểu đồ động về xu hướng đăng tin, tăng trưởng người dùng, phân bổ việc làm, có thể **lọc theo tuần, tháng, năm**.
*   **Theo dõi KPIs Nâng cao:** Theo dõi các chỉ số quan trọng như **% thay đổi so với tháng trước**, **tỷ lệ tuyển dụng**, **báo cáo chờ xử lý**.
*   **Hoạt động gần đây:** Luồng cập nhật trực tiếp các hoạt động mới nhất trên hệ thống.

#### 👥 2. Quản lý Người dùng & Doanh nghiệp
*   **Quản lý Toàn diện:** Xem, tìm kiếm, lọc người dùng với giao diện **dạng lưới (grid) hoặc bảng (table)**.
*   **Thao tác Nâng cao:** Xem chi tiết, chỉnh sửa, tạo mới, và thay đổi trạng thái tài khoản (kích hoạt, đình chỉ, tạm dừng).
*   **Quy trình Xác minh Doanh nghiệp:** Duyệt và xác minh tính xác thực của hồ sơ doanh nghiệp.

#### 📝 3. Quản lý & Duyệt Tin đăng
*   **Kiểm duyệt Nội dung:** Giao diện tập trung để xem, duyệt, từ chối các tin tuyển dụng.
*   **Quản lý Nâng cao:** Chỉnh sửa trực tiếp nội dung tin đăng của người dùng khi cần thiết.

#### ⚠️ 4. Hệ thống Xử lý Báo cáo Toàn diện
*   **Quy trình Xử lý Kín:** Tiếp nhận, xem xét (tự động chuyển trạng thái), và đưa ra quyết định xử lý (Bỏ qua, Cảnh cáo & Ẩn tin, Đình chỉ tài khoản & Ẩn tin).
*   **Phản hồi Tự động:** Hệ thống **tự động gửi thông báo** kết quả xử lý đến người báo cáo và người vi phạm.

#### 🚀 5. Công cụ Admin mạnh mẽ
*   **Gửi Thông báo Hàng loạt:** Gửi thông báo tùy chỉnh đến các nhóm đối tượng khác nhau (tất cả, chỉ ứng viên, chỉ NTD, hoặc người dùng cụ thể).
*   **Xuất Dữ liệu ra Excel:** Xuất danh sách người dùng và tin tuyển dụng ra file Excel để lưu trữ hoặc phân tích ngoại tuyến.
*   **Cấu hình Hệ thống:** Quản lý các danh mục cốt lõi như Ngành nghề, Tỉnh/Thành phố, Quận/Huyện.

</details>

<details>
<summary><b>⚙️ Quy trình Nghiệp vụ & Tự động hóa</b></summary>

*   **Phân luồng Duyệt tin:** Tin đăng của **Doanh nghiệp** phải qua `chờ duyệt`, trong khi tin của **NTD Cá nhân** được `duyệt tự động` để đảm bảo cân bằng giữa kiểm soát chất lượng và tính linh hoạt.
*   **Cập nhật trạng thái tự động:**
    *   Khi NTD xem hồ sơ ứng viên mới, trạng thái tự động chuyển thành `NTD đã xem`.
    *   Khi Admin xem một báo cáo vi phạm mới, trạng thái tự động chuyển thành `Đã xem xét`.
*   **Thông báo theo Quy trình:** Mọi bước quan trọng trong quy trình (ứng tuyển, duyệt tin, xử lý báo cáo) đều được hệ thống tự động hóa bằng cách gửi thông báo đến các bên liên quan.

</details>

🏗️ **Kiến trúc Cơ sở dữ liệu**

### Các nguyên tắc thiết kế chính

*   **Chuẩn hóa dữ liệu:** Thiết kế theo các dạng chuẩn (3NF) để đảm bảo tính toàn vẹn, giảm thiểu dư thừa dữ liệu.
*   **Bảng thực thể rõ ràng:** Các thực thể chính như `NguoiDung`, `TinTuyenDung`, `UngTuyen`, `TinNhan`, `ThongBao`, `BaoCaoViPham` được tách thành các bảng riêng biệt.
*   **Quản lý Quan hệ:** Sử dụng khóa ngoại và các mối quan hệ (One-to-One, One-to-Many, Many-to-Many) được định nghĩa rõ ràng thông qua Entity Framework Core Fluent API.
*   **Khả năng mở rộng:** Cấu trúc được thiết kế để dễ dàng thêm các tính năng mới mà không cần thay đổi lớn.

📊 **Tác động & Bài học kinh nghiệm**

### Kỹ năng kỹ thuật đã phát triển

*   **Phát triển Full-Stack:** Kinh nghiệm toàn diện từ backend logic (C#, ASP.NET) đến frontend tương tác (JavaScript, AJAX).
*   **Thiết kế CSDL Quan hệ:** Hiểu sâu về cách thiết kế, tối ưu hóa và quản lý CSDL với **MySQL** và EF Core.
*   **Kiến trúc hướng dịch vụ (Service-Oriented):** Xây dựng các service riêng biệt (ví dụ: `IThongBaoService`) để xử lý các nghiệp vụ phức tạp.
*   **Phân quyền & Bảo mật:** Triển khai hệ thống xác thực, phân quyền dựa trên vai trò (Role-based).

### Các insight chính

*   **Tầm quan trọng của Database:** Việc đầu tư thời gian thiết kế CSDL đúng đắn ngay từ đầu giúp giảm đáng kể chi phí bảo trì và độ phức tạp trong tương lai.
*   **Trải nghiệm người dùng là cốt lõi:** Các tính năng như thông báo và nhắn tin, dù phức tạp, nhưng là yếu tố then chốt giúp giữ chân người dùng.
*   **Tầm quan trọng của quy trình nghiệp vụ:** Hiểu rõ các quy trình (ví dụ: duyệt tin, xử lý báo cáo, gửi thông báo) là chìa khóa để xây dựng các chức năng chính xác và hiệu quả.

🚦 **Bắt đầu**

### Yêu cầu
*   **.NET 8.0 SDK**
*   **MySQL Server** (phiên bản 8.0 hoặc cao hơn được khuyến nghị)
*   Visual Studio 2022 hoặc Visual Studio Code
*   Git

### Các bước cài đặt

1.  **Clone a Repository**
    ```sh
    git clone https://github.com/Pbl3-Group/Pbl3.git
    cd Pbl3
    ```

2.  **Thiết lập Cơ sở dữ liệu**
    *   Mở file `appsettings.json`.
    *   Cập nhật chuỗi `ConnectionStrings` để trỏ đến instance **MySQL** của bạn. Ví dụ:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=localhost;Port=3306;Database=ten_database_cua_ban;Uid=ten_user;Pwd=mat_khau_cua_ban;"
        }
        ```
    *   Mở Package Manager Console (trong Visual Studio) hoặc Terminal và chạy lệnh migration của Entity Framework để tạo cơ sở dữ liệu và các bảng:
        ```sh
        Update-Database
        ```

3.  **Build và Chạy ứng dụng**
    *   Khôi phục các package cần thiết:
        ```sh
        dotnet restore
        ```
    *   Build dự án:
        ```sh
        dotnet build
        ```
    *   Chạy ứng dụng:
        ```sh
        dotnet run
        ```

4.  **Truy cập ứng dụng**
    *   Mở trình duyệt và truy cập vào `http://localhost:5000` (hoặc cổng được chỉ định).
    *   Đăng ký tài khoản mới hoặc sử dụng tài khoản demo (nếu có).

📱 **Ảnh chụp màn hình**

*(Thêm các ảnh chụp màn hình của bạn vào đây)*

| Trang chủ | Trang tìm việc | Chi tiết công việc |
| :---: | :---: | :---: |
| *(Dán ảnh trang chủ của bạn vào đây)* | *(Dán ảnh trang tìm việc của bạn vào đây)* | *(Dán ảnh chi tiết công việc của bạn vào đây)* |

| Giao diện Chat | Trung tâm Thông báo | Dashboard Admin |
| :---: | :---: | :---: |
| *(Dán ảnh giao diện nhắn tin của bạn vào đây)* | *(Dán ảnh trang thông báo của bạn vào đây)* | *(Dán ảnh dashboard admin của bạn vào đây)* |


🔮 **Cải tiến trong tương lai**

*   **Tích hợp Real-time:** Nâng cấp hệ thống nhắn tin và thông báo sử dụng SignalR để có trải nghiệm tức thì.
*   **Ứng dụng Di động:** Phát triển ứng dụng native cho iOS và Android.
*   **Gợi ý việc làm bằng AI:** Xây dựng hệ thống gợi ý thông minh hơn dựa trên hành vi và nội dung hồ sơ.
*   **Đánh giá Nhà tuyển dụng:** Cho phép ứng viên đánh giá công ty sau khi ứng tuyển hoặc làm việc.
*   **Hỗ trợ Đa ngôn ngữ:** Mở rộng để hỗ trợ người dùng quốc tế.

🤝 **Đóng góp**

Mọi đóng góp đều được chào đón! Vui lòng tạo một Pull Request để đóng góp. Đối với các thay đổi lớn, vui lòng mở một Issue trước để chúng ta có thể thảo luận.

📄 **Giấy phép**

Dự án này được cấp phép theo Giấy phép MIT - xem file [LICENSE](https://github.com/Pbl3-Group/Pbl3/blob/main/LICENSE) để biết chi tiết.

📞 **Liên hệ**

*   [@Nguyễn Thanh Huyền](https://github.com/Chizk23)
*   [@Bích Uyên](https://github.com/BichUyen2609)
*   [@Phượng Trần](https://github.com/PhuongTran2212)

⭐ **Nếu bạn thấy dự án này hữu ích, hãy cân nhắc tặng nó một ngôi sao!**