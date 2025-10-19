# HỆ THỐNG TÌM KIẾM VIỆC LÀM BÁN THỜI GIAN VÀ THỜI VỤ (PBL3)

[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=for-the-badge&logo=asp.net&logoColor=white)](https://docs.microsoft.com/en-us/aspnet/core/)
[![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Entity Framework Core](https://img.shields.io/badge/Entity_Framework-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
[![jQuery](https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white)](https://jquery.com/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

---

Một ứng dụng web full-stack, được xây dựng trên nền tảng .NET 8 và MySQL, nhằm tạo ra một cầu nối hiệu quả giữa người tìm việc và nhà tuyển dụng trong thị trường việc làm linh hoạt (bán thời gian, thời vụ).

*[Ảnh bìa hoặc GIF demo tổng quan về ứng dụng của bạn]*

## 🌟 Tổng quan dự án

Là sản phẩm của học phần Lập trình dựa trên dự án 3 (PBL3), dự án này được xây dựng để giải quyết một thách thức thực tế: sự thiếu hụt một nền tảng tập trung, uy tín và dễ sử dụng cho thị trường việc làm linh hoạt.

Thị trường này vô cùng đa dạng, từ các bạn sinh viên đang tìm kiếm công việc đầu đời để tích lũy kinh nghiệm, cho đến những người lao động lành nghề muốn có thêm thu nhập, hay đơn giản là bất kỳ ai đang tìm kiếm sự linh hoạt trong công việc. Tuy nhiên, các nền tảng hiện tại thường rời rạc, thiếu thông tin xác thực, và chưa thực sự phục vụ tốt cho tất cả các đối tượng này. Trong khi đó, nhà tuyển dụng cũng cần một công cụ hiệu quả để tiếp cận và giao tiếp với đúng ứng viên.

Để giải quyết những thách thức trên, chúng tôi đã phát triển một ứng dụng web full-stack với mục tiêu tạo ra một sân chơi công bằng và minh bạch, nơi mọi người, **dù có kinh nghiệm hay không**, đều có thể tìm thấy cơ hội phù hợp. Nền tảng được trang bị các tính năng cốt lõi như đăng tin, tìm kiếm, quản lý hồ sơ, cùng với **hệ thống nhắn tin theo ngữ cảnh** và **hệ thống thông báo toàn diện** để xóa bỏ rào cản giao tiếp và nâng cao trải nghiệm người dùng.

---

## ✨ Các chức năng chính

Hệ thống được thiết kế với ba vai trò người dùng chính, mỗi vai trò có một bộ công cụ chuyên biệt để phục vụ đúng nhu cầu.

<details>
<summary><b>👨‍💻 1. Dành cho Người Tìm Việc (Ứng viên)</b></summary>
<br>

  #### 🔍 **Tìm kiếm & Khám phá việc làm**
  *   **Tìm kiếm & Lọc nâng cao:** Dễ dàng tìm kiếm công việc với bộ lọc đa dạng từ địa điểm, ngành nghề đến khoảng lương, loại hình công việc, giúp nhanh chóng khoanh vùng các cơ hội phù hợp nhất.
      
      *[Ảnh chụp màn hình trang tìm việc với các bộ lọc đang được sử dụng]*

  *   **Gợi ý việc làm phù hợp:** Hệ thống tự động phân tích và chấm điểm độ tương thích (%) giữa hồ sơ của bạn (kỹ năng, lịch rảnh, địa điểm, lương mong muốn) với các tin tuyển dụng, giúp bạn không bỏ lỡ những cơ hội vàng.

      *[Ảnh chụp màn hình một tin tuyển dụng có hiển thị điểm phù hợp]*

  #### 👤 **Quản lý Hồ sơ & CV toàn diện**
  *   **Hồ sơ đa thành phần:** Quản lý tập trung và riêng biệt thông tin tài khoản, hồ sơ chuyên môn, lịch làm việc mong muốn và các khu vực làm việc yêu thích.
  *   **Xây dựng hồ sơ chuyên nghiệp:** Tải lên CV, giới thiệu bản thân, thiết lập các kỳ vọng về công việc để thu hút nhà tuyển dụng.

      *[Ảnh chụp màn hình trang dashboard hoặc trang hồ sơ cá nhân của ứng viên]*

  #### ✍️ **Quản lý Ứng tuyển thông minh**
  *   **Theo dõi trạng thái chi tiết:** Nắm bắt toàn bộ hành trình ứng tuyển, từ lúc "Đã nộp" đến khi "NTD đã xem", "Chấp nhận" hoặc "Từ chối".
  *   **Tương tác linh hoạt:** Dễ dàng sửa đổi thông tin ứng tuyển, rút lại hồ sơ khi cần thiết, và đặc biệt là có thể **hoàn tác việc rút đơn** trong một khoảng thời gian nhất định.

      *[Ảnh chụp màn hình trang "Việc đã ứng tuyển" với các trạng thái khác nhau]*
      
</details>

<details>
<summary><b>🏢 2. Dành cho Nhà Tuyển Dụng (Cá nhân & Doanh nghiệp)</b></summary>
<br>

  #### 📋 **Đăng & Quản lý tin tuyển dụng chuyên nghiệp**
  *   **Giao diện đăng tin trực quan:** Form đăng tin chi tiết, khoa học, giúp nhà tuyển dụng cung cấp đầy đủ thông tin nhất có thể.
  *   **Bộ công cụ quản lý mạnh mẽ:** Toàn quyền kiểm soát các tin đã đăng với các chức năng: Sửa, Xóa (ẩn), **Đăng lại nhanh (Repost)** để làm mới tin, Tạm ẩn/Hiện, và Đánh dấu đã tuyển.
  
      *[Ảnh chụp màn hình trang quản lý tin tuyển dụng của NTD]*

  #### 👨‍💼 **Quản lý Ứng viên hiệu quả**
  *   **Sàng lọc ứng viên dễ dàng:** Xem danh sách ứng viên theo từng tin tuyển dụng, lọc và tìm kiếm hồ sơ nhanh chóng.
  *   **Tương tác và ra quyết định:** Xem chi tiết hồ sơ ứng viên, CV, thư giới thiệu và thay đổi trạng thái ứng tuyển (chấp nhận/từ chối). Mọi thay đổi sẽ được **thông báo tự động** đến ứng viên.

      *[Ảnh chụp màn hình trang quản lý ứng viên của NTD, có các nút thay đổi trạng thái]*
  
  #### 🔄 **Chuyển đổi vai trò linh hoạt**
  *   Người dùng có vai trò "Cá nhân" có thể dễ dàng chuyển đổi qua lại giữa giao diện **Tìm việc** và **Tuyển dụng** chỉ với một cú nhấp chuột, phục vụ cho cả hai nhu cầu trên cùng một tài khoản.

      *[Ảnh chụp màn hình nút chuyển đổi vai trò trên giao diện]*

</details>

<details>
<summary><b>🛡️ 3. Dành cho Quản trị viên (Admin)</b></summary>
<br>

  #### 📊 **Dashboard Phân tích & Thống kê chuyên sâu**
  *   **Bảng điều khiển trực quan:** Cung cấp cái nhìn tổng quan về sức khỏe của hệ thống qua các biểu đồ động (có thể **lọc theo tuần/tháng/năm**), KPIs quan trọng và luồng hoạt động mới nhất.
  
      *[Ảnh chụp màn hình Dashboard của Admin với các biểu đồ]*

  #### 👥 **Quản lý người dùng toàn diện**
  *   **Quản lý tài khoản tập trung:** Giao diện quản lý tất cả người dùng với các công cụ tìm kiếm, lọc, và tùy chọn hiển thị dạng lưới hoặc bảng.
  *   **Thực thi quyền hạn:** Admin có toàn quyền xem chi tiết, chỉnh sửa, tạo mới, và thay đổi trạng thái tài khoản (kích hoạt, đình chỉ, tạm dừng).

      *[Ảnh chụp màn hình trang quản lý người dùng của Admin]*

  #### ⚠️ **Hệ thống Kiểm duyệt & Xử lý Báo cáo**
  *   **Quy trình kiểm duyệt nội dung:** Giao diện chuyên biệt để duyệt hoặc từ chối các tin tuyển dụng đang chờ, đảm bảo chất lượng nội dung trên toàn hệ thống.
  *   **Xử lý báo cáo vi phạm:** Quy trình xử lý báo cáo khép kín, từ việc tiếp nhận, xem xét, đến ra quyết định xử lý và **tự động gửi thông báo** kết quả đến các bên liên quan.

      *[Ảnh chụp màn hình trang quản lý báo cáo vi phạm]*
      
  #### 🚀 **Công cụ Admin mạnh mẽ**
  *   **Gửi Thông báo Hàng loạt:** Tạo và gửi các thông báo quan trọng đến các nhóm đối tượng người dùng cụ thể.
  *   **Xuất Dữ liệu ra Excel:** Tính năng xuất danh sách người dùng và tin tuyển dụng ra file Excel để phục vụ cho việc lưu trữ và phân tích ngoại tuyến.

      *[Ảnh chụp màn hình trang gửi thông báo hàng loạt hoặc nút Xuất Excel]*

</details>

<details>
<summary><b>💬 4. Các tính năng chung (Nhắn tin, Thông báo)</b></summary>
<br>

  *   **Hệ thống Nhắn tin theo Ngữ cảnh:** Trò chuyện trực tiếp với nhà tuyển dụng/ứng viên trong một giao diện quen thuộc. Mỗi cuộc hội thoại được gắn với một công việc hoặc đơn ứng tuyển cụ thể, giúp việc trao đổi luôn rõ ràng và đúng trọng tâm.
  
      *[Ảnh chụp màn hình giao diện nhắn tin chi tiết]*

  *   **Hệ thống Thông báo Toàn diện:** Tự động thông báo cho người dùng về mọi cập nhật quan trọng (trạng thái ứng tuyển, tin nhắn mới, tin đăng được duyệt,...) và cho phép quản lý chúng tại một trung tâm thông báo duy nhất.

      *[Ảnh chụp màn hình trung tâm thông báo của người dùng]*

</details>

---

## ⚙️ Công nghệ & Kiến trúc

<details>
<summary>Xem chi tiết Công nghệ & Kiến trúc</summary>
<br>

*   **Backend:**
    *   **Ngôn ngữ & Framework:** C# trên nền tảng ASP.NET Core MVC (.NET 8.0).
    *   **Database & ORM:** MySQL 8.0+ và Entity Framework Core với provider `Pomelo.EntityFrameworkCore.MySql`.
    *   **Kiến trúc:** Thiết kế hướng dịch vụ (Service-Oriented Architecture) với các service riêng biệt cho các nghiệp vụ phức tạp (ví dụ: `IThongBaoService`).
    *   **Bảo mật:** Sử dụng ASP.NET Core Identity kết hợp Cookie Authentication để xác thực và phân quyền dựa trên vai trò (Role-Based Access Control).

*   **Frontend:**
    *   **Nền tảng:** JavaScript (ES6+), jQuery & AJAX để tạo các tương tác động.
    *   **Giao diện:** HTML5, CSS3 và Bootstrap 5 để xây dựng giao diện responsive và hiện đại.
    *   **Thư viện:** Chart.js (biểu đồ), Select2 (tìm kiếm và chọn lựa nâng cao).

*   **Quy trình & Tự động hóa:**
    *   **Phân luồng Duyệt tin:** Tin đăng của **Doanh nghiệp** phải qua `chờ duyệt`, trong khi tin của **NTD Cá nhân** được `duyệt tự động` để cân bằng giữa kiểm soát và linh hoạt.
    *   **Cập nhật trạng thái tự động:** Hệ thống tự động thay đổi trạng thái khi có tương tác mới (ví dụ: NTD xem hồ sơ, Admin xem báo cáo).
    *   **Thông báo theo Quy trình:** Mọi bước quan trọng trong quy trình nghiệp vụ (ứng tuyển, duyệt tin, xử lý báo cáo) đều được tự động hóa bằng cách gửi thông báo đến các bên liên quan.

</details>

---

## 🚦 Bắt đầu

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
    *   Mở file `appsettings.json` và cập nhật chuỗi `ConnectionStrings` cho MySQL.
        ```json
        "ConnectionStrings": {
           "DefaultConnection": "Server=localhost;Database=JOBFLEX;User=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;CharSet=utf8mb4;"
        }
        ```
    *   Chạy lệnh migration để tạo CSDL:
        ```sh
        Update-Database
        ```

3.  **Chạy ứng dụng**
    ```sh
    dotnet run
    ```
    *   Truy cập vào `http://localhost:5000` (hoặc cổng được chỉ định).

---

## 🔮 Cải tiến trong tương lai

Dựa trên nền tảng vững chắc đã xây dựng, đây là những tính năng thực tế và khả thi mà chúng tôi dự định phát triển trong các phiên bản tiếp theo:

*   **🔗 Tích hợp Đăng nhập Mạng xã hội (Social Login):** Cho phép người dùng đăng ký/đăng nhập nhanh qua tài khoản **Google** hoặc **Facebook**.
*   **📧 Hệ thống Gửi Email Thông báo Tự động:** Gửi email thông báo về các cập nhật quan trọng (tin nhắn mới, trạng thái ứng tuyển, việc làm phù hợp).
*   **🤖 Nâng cấp AI: Phân tích & Tự động điền Hồ sơ từ CV (CV Parsing):** Xây dựng tính năng AI "đọc" file CV và tự động điền thông tin vào hồ sơ trên web.
*   **💡 Cải tiến Hệ thống Gợi ý (Recommendation Engine):** Sử dụng Machine Learning để phân tích hành vi người dùng và đưa ra gợi ý việc làm chính xác hơn.
*   **⚡ Nâng cấp Chat & Thông báo Real-time với SignalR:** Chuyển sang kết nối thời gian thực để tin nhắn và thông báo xuất hiện ngay lập tức.
*   **⭐ Hệ thống Đánh giá Nhà tuyển dụng:** Cho phép ứng viên để lại đánh giá và xếp hạng về nhà tuyển dụng.

---

## 📄 Giấy phép

Dự án được cấp phép theo Giấy phép MIT - xem file [LICENSE](https://github.com/Pbl3-Group/Pbl3/blob/main/LICENSE) để biết chi tiết.

## ⭐ Ủng hộ dự án

Nếu bạn thấy dự án này hữu ích, hãy cân nhắc tặng nó một ngôi sao trên GitHub!

## 📞 Liên hệ

*   [@Chizk23](https://github.com/Chizk23) - Nguyễn Thanh Huyền
*   [@BichUyen2609](https://github.com/BichUyen2609) - Nguyễn Thị Bích Uyên
*   [@PhuongTran2212](https://github.com/PhuongTran2212) - Trần Thị Phượng