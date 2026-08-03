# 🥦 GreenBasket - E-Commerce Organic Farm-to-Table Web Application

![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)
![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![LocalStorage](https://img.shields.io/badge/State-LocalStorage%20API-green?style=for-the-badge)

**GreenBasket** là nền tảng thương mại điện tử chuyên cung cấp rau củ quả và trái cây hữu cơ tươi sạch từ nông trại đến tận tay người tiêu dùng (Farm-to-Table). Dự án được thiết kế với giao diện hiện đại, tối ưu trải nghiệm người dùng (UX/UI) và tích hợp hệ thống quản lý trạng thái client-side phản hồi theo thời gian thực.

---

## 🌟 Tính Năng Nổi Bật (Key Features)

### 🛒 Dành cho Khách hàng (Customer Interface)
- **Trang chủ & Khám phá (Landing & Homepage)**: Banner khuyến mãi, danh mục nông sản nổi bật, đánh giá từ khách hàng và cam kết nguồn gốc VietGAP/Organic.
- **Cửa hàng & Tìm kiếm nâng cao (Shop Catalog & Filters)**:
  - Lọc theo danh mục: *Leafy Greens, Root Vegetables, Seasonal Fruit, Tropical Fruit*.
  - Tìm kiếm nông sản thời gian thực và lọc theo mức giá.
  - Hiển thị đầy đủ thông tin nguồn gốc trang trại (Farm Traceability), ngày thu hoạch và trạng thái tồn kho.
- **Giỏ hàng phản hồi nhanh (Interactive Shopping Cart)**:
  - Thêm/sửa/xóa sản phẩm, tự động tính toán tổng tiền và phí giao hàng (Free Ship cho đơn hàng từ $30).
  - Tự động đồng bộ số lượng sản phẩm trên thanh Header badge.
- **Thanh toán & Chọn khung giờ giao hàng (Checkout & Delivery Slot)**:
  - Nhập/chọn địa chỉ giao hàng.
  - Lựa chọn linh hoạt khung giờ nhận hàng (Delivery Slots).
  - Đa dạng phương thức thanh toán: *Tiền mặt (COD), Thẻ tín dụng, Ví điện tử MoMo*.
- **Theo dõi đơn hàng & Báo cáo sự cố (Order Tracking & Quality Report)**:
  - Xem lịch sử mua hàng và trạng thái đơn (*Processing, Out for Delivery, Delivered, Cancelled*).
  - Gửi yêu cầu phản hồi/khiếu nại chất lượng sản phẩm (Quality Ticket / Refund Request) kèm lý do hư hỏng trong quá trình vận chuyển.

---

### 🛡️ Dành cho Quản trị viên & Nhân viên (Admin & Staff Dashboard)
- **Quản lý danh mục sản phẩm (Product Management - CRUD)**:
  - Thêm sản phẩm mới, cập nhật giá, hình ảnh, đơn vị tính, nguồn gốc nông trại và số lượng tồn kho.
  - Xóa hoặc cập nhật trạng thái kho (*In Stock / Low Stock*).
- **Xử lý đơn hàng (Order Fulfillment)**:
  - Xem danh sách toàn bộ đơn hàng của hệ thống.
  - Cập nhật trạng thái giao hàng theo thời gian thực.
- **Xử lý khiếu nại chất lượng (Quality Reports Resolution)**:
  - Tiếp nhận và duyệt/từ chối các ticket khiếu nại chất lượng nông sản từ khách hàng.
- **Báo cáo & Thống kê (Sales Analytics & Inventory Overview)**:
  - Thống kê tổng doanh thu, số lượng đơn hoàn thành, đơn đang xử lý và cảnh báo nông sản sắp hết hàng.

---

### ⚡ Động cơ Trạng thái & Đồng bộ dữ liệu (`app-state.js`)
- **Reactive Storage Engine**: Sử dụng `LocalStorage API` kết hợp với Custom Events (`gb_state_change`) để đồng bộ dữ liệu giữa các trang mà không cần tải lại trang.
- **Dữ liệu mẫu (Seed Data)**: Tự động khởi tạo dữ liệu mẫu phong phú về sản phẩm, đơn hàng và địa chỉ khi chạy lần đầu.
- **Giả lập Phân quyền (Role-Based Access Control)**: Chuyển đổi linh hoạt giữa vai trò *Khách hàng (Customer)* và *Quản trị viên / Nhân viên (Staff / Admin)*.

---

## 📁 Cấu Trúc Thư Mục (Project Structure)

```text
GreenBasketFrontEnd/
├── index.html                 # Trang chủ chính
├── landing.html               # Trang giới thiệu / Landing page phụ
├── shop.html                  # Trang cửa hàng & danh mục nông sản
├── cart.html                  # Trang giỏ hàng
├── chackout.html              # Trang thanh toán & chọn khung giờ giao hàng
├── orders.html                # Trang lịch sử & theo dõi đơn hàng
├── admin.html                 # Dashboard quản trị & xử lý đơn hàng
├── contact.html               # Trang liên hệ
├── Fruitables.jpg             # Banner minh họa
├── LICENSE.txt                # Giấy phép bản quyền
├── css/                       # Định dạng CSS (Bootstrap, Style tùy chỉnh)
│   ├── bootstrap.min.css
│   └── style.css
├── js/                        # Mã nguồn JavaScript
│   ├── app-state.js           # Core Engine quản lý state & LocalStorage
│   └── main.js                # Xử lý sự kiện UI & hiệu ứng Carousel
├── lib/                       # Thư viện bổ trợ (Lightbox, OwlCarousel, Easing)
└── img/                       # Hình ảnh nông sản, banner & icon
```

---

## 🚀 Hướng Dẫn Chạy Dự Án (How to Run)

Dự án là **Pure Front-End (HTML/CSS/JS)** thuần túy, không yêu cầu cài đặt Node.js hay Build tool phức tạp.

1. **Dùng Live Server (Khuyến nghị)**:
   - Mở thư mục `GreenBasketFrontEnd` trong VS Code.
   - Click chuột phải vào `index.html` và chọn **Open with Live Server**.
2. **Mở trực tiếp trên trình duyệt**:
   - Nhấp đôi chuột vào file `index.html` để mở trên trình duyệt web bất kỳ.

---

## 🔑 Thử Nghiệm Các Vai Trò (Demo Roles)

1. **Khách hàng (Customer Mode)**:
   - Mặc định ứng dụng chạy ở chế độ Khách hàng.
   - Thử thêm sản phẩm vào giỏ hàng tại `shop.html`, tiến hành đặt hàng tại `chackout.html` và theo dõi trạng thái tại `orders.html`.
2. **Quản trị viên / Nhân viên (Admin / Staff Mode)**:
   - Đăng nhập với email chứa từ khóa `admin` hoặc `staff` (Ví dụ: `admin@greenbasket.com`).
   - Truy cập trang `admin.html` để quản lý sản phẩm, cập nhật trạng thái đơn hàng và xử lý ticket khiếu nại.

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

- **Frontend**: HTML5, CSS3, JavaScript (ES6 Standard)
- **UI Framework**: Bootstrap 5.x
- **Icons & Fonts**: Font Awesome 5, Google Fonts (Open Sans, Raleway)
- **Plugins**: jQuery, Owl Carousel, Lightbox
- **Data Persistence**: HTML5 Web Storage API (LocalStorage)

---

## 📜 Giấy Phép & Tác Quyền (License)

Dự án phát triển dựa trên Template **Fruitables** bởi HTML Codex và được tùy biến mở rộng hệ thống State Management & Admin Dashboard bởi **GreenBasket Team**.
