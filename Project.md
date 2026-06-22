# ShipFood - Nền tảng Giao Hàng Thức Ăn Online

## 📋 Tổng Quan

**ShipFood** là ứng dụng web ASP.NET MVC 5 toàn diện để đặt hàng và giao hàng thức ăn online. Nền tảng kết nối người dùng, nhà hàng, người giao hàng (shipper) và quản trị viên trong một hệ thống thống nhất.

## 🎯 Mục Đích

Cung cấp một giải pháp hoàn chỉnh cho:
- **Khách hàng**: Duyệt nhà hàng, đặt hàng, theo dõi giao hàng
- **Nhà hàng**: Quản lý menu, xử lý đơn hàng
- **Người giao hàng (Shipper)**: Nhận và giao hàng
- **Quản trị viên**: Quản lý toàn bộ hệ thống

## 🏗️ Kiến Trúc Kỹ Thuật

### Công Nghệ Stack
- **Framework**: ASP.NET MVC 5 (.NET Framework 4.7.2)
- **Database**: SQL Server (Entity Framework 6.4.4 ORM)
- **Frontend**: HTML5, CSS3, Bootstrap 3.4.1
- **JavaScript**: jQuery 3.7.1, SignalR 2.4.3 (Real-time messaging)
- **Thanh toán**: PayPal API (v1.9.1)
- **Server**: IIS Express (Development)

### Cấu Trúc Dự Án

```
ShipFood/
├── Controllers/           # Xử lý yêu cầu HTTP
│   ├── AdminController.cs      # Quản trị hệ thống
│   ├── HomeController.cs       # Trang chủ
│   ├── CartController.cs       # Giỏ hàng
│   ├── RestaurantController.cs # Nhà hàng
│   └── ShipperController.cs    # Shipper
├── Models/               # Dữ liệu và Logic
│   ├── DAOModel.cs       # Entity Framework DbContext
│   ├── tbUser.cs         # Người dùng
│   ├── tbKhachHang.cs    # Khách hàng
│   ├── tbQuanAn.cs       # Nhà hàng
│   ├── tbMonAn.cs        # Thực đơn/Món ăn
│   ├── tbDonHang.cs      # Đơn hàng
│   ├── tbShipper.cs      # Shipper
│   ├── tbAdmin.cs        # Admin
│   ├── Cart.cs           # Giỏ hàng logic
│   ├── PaypalConfiguration.cs # Cấu hình PayPal
│   ├── Address.cs        # Địa chỉ
│   ├── City.cs, District.cs # Địa danh
│   └── [Các model khác]
├── Views/                # Giao diện người dùng
│   ├── Home/             # Trang chủ
│   ├── Cart/             # Giỏ hàng
│   ├── Restaurant/       # Danh sách nhà hàng
│   ├── Shipper/          # Giao diện shipper
│   ├── Admin/            # Trang quản trị
│   └── Shared/           # Layout chung
├── Hubs/                 # SignalR hubs (Real-time)
├── Content/              # CSS, Images
├── Scripts/              # Client-side JavaScript
├── Utils/                # Hàm tiện ích
└── App_Start/            # Cấu hình ứng dụng
```

## 🔑 Tính Năng Chính

### 1. Quản Lý Tài Khoản
- Đăng ký/Đăng nhập cho khách hàng, nhà hàng, shipper
- Xác thực và phân quyền

### 2. Tìm Kiếm & Duyệt Nhà Hàng
- Danh sách nhà hàng theo danh mục
- Xem menu, giá cả
- Đánh giá và bình luận

### 3. Đặt Hàng & Giỏ Hàng
- Thêm/xóa/chỉnh sửa món ăn trong giỏ
- Tính toán tổng tiền tự động
- Lưu giỏ hàng

### 4. Thanh Toán
- Thanh toán trực tiếp (COD)
- Thanh toán PayPal
- Quản lý lịch sử giao dịch

### 5. Quản Lý Đơn Hàng
- **Khách hàng**: Xem trạng thái, theo dõi shipper
- **Nhà hàng**: Xác nhận, từ chối, cập nhật đơn
- **Shipper**: Nhận đơn, cập nhật vị trí, hoàn thành

### 6. Real-time Notification
- SignalR để cập nhật trạng thái in-time
- Thông báo cho khách hàng, nhà hàng, shipper

### 7. Quản Trị Hệ Thống
- Quản lý người dùng, nhà hàng, shipper
- Quản lý danh mục, khuyến mại
- Thống kê doanh thu, bán hàng

## 📊 Cơ Sở Dữ Liệu

Các bảng chính:
- `tbUser` - Người dùng hệ thống
- `tbKhachHang` - Thông tin khách hàng
- `tbQuanAn` - Thông tin nhà hàng
- `tbMonAn` - Thực đơn
- `tbDonHang` - Đơn hàng
- `tbChiTietDonHang` - Chi tiết đơn hàng
- `tbShipper` - Thông tin shipper
- `tbDanhGia` - Đánh giá
- `tbKhuyenMai` - Khuyến mại
- `tbTinNhan` - Tin nhắn
- `City`, `District`, `Address` - Địa danh

## 🔒 Bảo Mật

- Xác thực người dùng
- Phân quyền theo vai trò (User, Restaurant, Shipper, Admin)
- Mã hóa thanh toán PayPal
- Xác thực yêu cầu HTTP

## 📝 Yêu Cầu Hệ Thống

- .NET Framework 4.7.2+
- SQL Server 2016+
- IIS 8.0+
- Visual Studio 2015+ (để phát triển)

## 🚀 Hướng Dẫn Chạy

1. **Clone/Tải dự án**
   ```
   ShipFood\
   ```

2. **Cài đặt NuGet packages**
   - Mở Solution trong Visual Studio
   - Package Manager > Package Manager Console
   - `Update-Package` để cập nhật dependencies

3. **Cấu hình Database**
   - Chỉnh sửa connection string trong `Web.config`
   - Tạo database từ Entity Framework

4. **Cấu hình PayPal**
   - Nhập Client ID và Secret vào `Web.config`

5. **Chạy ứng dụng**
   - Bấm F5 hoặc nhấp Start trong Visual Studio

## 📦 Dependencies Chính

- **EntityFramework** (6.4.4) - ORM
- **Microsoft.AspNet.Mvc** (5.3.0) - Web Framework
- **Microsoft.AspNet.SignalR** (2.4.3) - Real-time
- **jQuery** (3.7.1) - Frontend
- **Bootstrap** (3.4.1) - UI Framework
- **PayPal** (1.9.1) - Payment Gateway
- **Newtonsoft.Json** (13.0.3) - JSON parsing

## 👥 Vai Trò Người Dùng

| Vai Trò | Quyền |
|---------|-------|
| **Admin** | Quản lý toàn bộ hệ thống, người dùng, thống kê |
| **Restaurant** | Quản lý menu, đơn hàng, đánh giá |
| **Shipper** | Nhận đơn, giao hàng, cập nhật vị trí |
| **Customer** | Đặt hàng, thanh toán, theo dõi giao hàng |

## 📞 Ghi Chú Phát Triển

- Project sử dụng mô hình MVC truyền thống
- Database-first approach với Entity Framework
- Có thể mở rộng thêm Web API cho mobile app
- SignalR hỗ trợ real-time updates

---

**Phiên bản**: 1.0  
**Ngôn ngữ**: C#, HTML, CSS, JavaScript  
**Kiến trúc**: ASP.NET MVC n-tier
