# 🏗 FastShip — Kiến trúc tổng thể

> Tài liệu này mô tả cấu trúc toàn bộ dự án ShipFood Web App (.NET 8 MVC).
> Dùng để tham chiếu khi redesign giao diện — biết file nào cần sửa, nằm ở đâu.

---

## 1. 🧩 Kiến trúc tổng quan

```
┌──────────────────────────────────────────────────────┐
│                   Views (Razor .cshtml)               │
│  Home · Restaurant · Shipper · Admin · Cart · Shared  │
└────────────────┬─────────────────────────────────────┘
                 │ RenderBody / Sections
┌────────────────▼──────────────────────────────────────┐
│              Layouts (_Layout*.cshtml)                 │
│  _LayoutPageHome · _LayoutPageRestaurant · _Layout*    │
└────────────────┬─────────────────────────────────────┘
                 │ HTTP Request
┌────────────────▼──────────────────────────────────────┐
│              Controllers (MVC)                         │
│  Home · Restaurant · Shipper · Admin · Cart · Payment  │
└────────────────┬─────────────────────────────────────┘
                 │ Services DI
┌────────────────▼──────────────────────────────────────┐
│              Services Layer                            │
│  Recommendation · Voucher · EDelivery · Gemini · MoMo  │
│  AutoPreparing (Background Service)                    │
└────────────────┬─────────────────────────────────────┘
                 │ EF Core
┌────────────────▼──────────────────────────────────────┐
│              Models / DbContext                         │
│  tbUser · tbKhachHang · tbShipper · tbQuanAn · tbMonAn │
│  tbDonHang · tbChiTietDonHang · tbDanhMuc ...          │
└────────────────┬─────────────────────────────────────┘
                 │ Npgsql
┌────────────────▼──────────────────────────────────────┐
│              PostgreSQL (Render Cloud)                  │
└───────────────────────────────────────────────────────┘
```

---

## 2. 📂 Cấu trúc thư mục

### 2.1 Controllers (`ShipFoodCore/Controllers/`)

| File | Route | Chức năng |
|------|-------|-----------|
| `HomeController.cs` | `/Home/*` | Trang chủ, đăng nhập, tìm kiếm, detail quán, chat |
| `RestaurantController.cs` | `/Restaurant/*` | Dashboard quán: đơn hàng, món ăn, doanh thu, đánh giá |
| `ShipperController.cs` | `/Shipper/*` | Dashboard shipper: nhận đơn, thu nhập, lịch sử |
| `AdminController.cs` | `/Admin/*` | Dashboard admin: quản lý user, quán, shipper |
| `CartController.cs` | `/Cart/*` | Giỏ hàng, checkout, lịch sử đặt hàng |
| `PaymentController.cs` | `/Payment/*` | Xử lý thanh toán MoMo |
| `ChatbotController.cs` | `/Chatbot/*` | Gemini AI chat API |
| `AdminChatController.cs` | `/AdminChat/*` | Admin chat với user |
| `BaseController.cs` | — | Base class: giỏ hàng, session helper |

### 2.2 Views (`ShipFoodCore/Views/`)

#### Layouts (`Shared/`)

| File | Dùng cho | Mô tả |
|------|----------|-------|
| `_LayoutPageHome.cshtml` | **Home** (public) | Layout chính cho khách hàng, font-end |
| `_LayoutPageRestaurant.cshtml` | **Restaurant dashboard** | Layout cho chủ quán |
| `_LayoutPageShipper.cshtml` | **Shipper dashboard** | Layout cho shipper |
| `_LayoutPageAmin.cshtml` | **Admin dashboard** | Layout cho admin (lưu ý typo: 'Amin') |
| `_Layout.cshtml` | Fallback | Layout mặc định |
| `_LayoutAuth.cshtml` | Login/Signup | Layout riêng cho trang đăng nhập |
| `_ChatWidget.cshtml` | — | Partial view: chatbot widget |

#### Pages theo role

**Home (Khách hàng)** — `Views/Home/`:
| File | Chức năng |
|------|-----------|
| `Index.cshtml` | 🏠 Trang chủ — danh sách quán, slider, gợi ý |
| `DetailRestaurant.cshtml` | 🍽 Trang chi tiết quán — menu, filter, cart |
| `SanPham.cshtml` | 📄 Chi tiết sản phẩm |
| `DanhMuc.cshtml` | 📂 Xem theo danh mục |
| `Login.cshtml` / `Signup.cshtml` | 🔐 Đăng nhập / Đăng ký |
| `NhanTin.cshtml` | 💬 Chat real-time với quán |
| `Forgot.cshtml` | 🔑 Quên mật khẩu |

**Restaurant (Chủ quán)** — `Views/Restaurant/`:
| File | Chức năng |
|------|-----------|
| `Index.cshtml` | Dashboard — tổng quan đơn hàng, doanh thu |
| `OrderList.cshtml` | Danh sách đơn hàng |
| `ProductList.cshtml` | Quản lý món ăn |
| `ProductDetail.cshtml` | Chi tiết / Thêm món |
| `Analytics.cshtml` | Thống kê, biểu đồ |
| `GeneralCustomer.cshtml` | Quản lý khách hàng |
| `Discount.cshtml` | Quản lý khuyến mãi |
| `Review.cshtml` | Xem đánh giá |
| `Profile.cshtml` | Thông tin quán |
| `Wallet.cshtml` | Ví tiền, lịch sử rút tiền |

**Shipper** — `Views/Shipper/`:
| File | Chức năng |
|------|-----------|
| `Index.cshtml` | Dashboard — nhận đơn, bản đồ |
| `OrderDetail.cshtml` | Chi tiết đơn giao |
| `ThuNhap.cshtml` | Thu nhập |
| `ViTien.cshtml` | Ví tiền |
| `LichSu.cshtml` | Lịch sử giao hàng |
| `NhanTin.cshtml` | Chat với quán / khách |
| `ThongBao.cshtml` | Thông báo |
| `CaiDat.cshtml` | Cài đặt |

**Admin** — `Views/Admin/`:
| File | Chức năng |
|------|-----------|
| `Dashboard.cshtml` | Tổng quan hệ thống |
| `Index.cshtml` | Danh sách quản lý |
| `QuanLyQuanAn.cshtml` | Quản lý quán ăn |
| `QuanLyKhachHang.cshtml` | Quản lý khách hàng |
| `QuanLyShipper.cshtml` | Quản lý shipper |
| `Order.cshtml` / `OrderDetail.cshtml` | Quản lý đơn hàng |
| `Category.cshtml` | Quản lý danh mục |
| `PostTaiKhoan.cshtml` | Quản lý tài khoản |

**Cart** — `Views/Cart/`:
| File | Chức năng |
|------|-----------|
| `Index.cshtml` | Giỏ hàng |
| `Checkout.cshtml` | Thanh toán (one-page) |
| `OrderTracking.cshtml` | 📍 Theo dõi đơn hàng real-time |
| `LichSuDatHang.cshtml` | Lịch sử đặt hàng |
| `ChiTietDonHang.cshtml` | Chi tiết đơn hàng |
| `EInvoice.cshtml` | Hoá đơn điện tử |
| `SuccessView.cshtml` / `FailureView.cshtml` | Kết quả thanh toán |

### 2.3 Models (`ShipFoodCore/Models/`)

| Model | Table | Vai trò |
|-------|-------|---------|
| `tbUser` | users | Tài khoản (customer + restaurant + shipper + admin) |
| `tbKhachHang` | khachhang | Thông tin khách hàng |
| `tbShipper` | shipper | Thông tin shipper |
| `tbQuanAn` | quanan | Thông tin quán ăn |
| `tbMonAn` | monan | Món ăn |
| `tbDanhMuc` | danhmuc | Danh mục món |
| `tbDonHang` | donhang | Đơn hàng |
| `tbChiTietDonHang` | chitietdonhang | Chi tiết đơn hàng |
| `tbDanhGia` | danhgia | Đánh giá |
| `tbKhuyenMai` | khuyenmai | Khuyến mãi |
| `tbLichSuSuDungKhuyenMai` | lichsusudungkhuyenmai | Lịch sử dùng KM |
| `tbBienTheMonAn` | biethemonan | Biến thể món (size, topping) |
| `tbTinNhan` | tinnhan | Tin nhắn chat |
| `tbThongTinDatHang` | thongtindathang | Thông tin đặt hàng |
| `tbEInvoice` | einfvoice | Hoá đơn điện tử |
| `tbLoaiHinhThanhToan` | loaihinhthanhtoan | Loại hình thanh toán |
| `tbAdmin` | admin | Tài khoản admin |

### 2.4 Services (`ShipFoodCore/Services/`)

| Service | Chức năng | Singleton? |
|---------|-----------|-----------|
| `RecommendationService.cs` | Gợi ý món (Apriori + time-based) | ❌ Scoped |
| `VoucherService.cs` | Quản lý voucher giảm giá | ❌ Scoped |
| `GeminiService.cs` | Chatbot AI (Gemini API) | ✅ Singleton |
| `MoMoService.cs` | Thanh toán MoMo | ❌ Scoped |
| `EDeliveryService.cs` | Hoá đơn điện tử (E-Invoice) | ❌ Scoped |
| `AutoPreparingService.cs` | Tự động chuyển trạng thái đơn | ✅ HostedService |

### 2.5 ViewComponents

| Component | File | Chức năng |
|-----------|------|-----------|
| `FilterBarViewComponent` | `Views/Shared/Components/FilterBar/` | Thanh lọc thực đơn |

### 2.6 Middleware

| Middleware | File | Chức năng |
|-----------|------|-----------|
| `RoleGuardMiddleware` | `Middleware/RoleGuardMiddleware.cs` | Kiểm tra role khi truy cập dashboard |

### 2.7 Hubs (SignalR)

| Hub | Route | Chức năng |
|-----|-------|-----------|
| `Chats` | `/nhantin` | Chat real-time + order tracking |

---

## 3. 🔄 Luồng chính

### 3.1 Customer Flow
```
Home/Index → DetailRestaurant → Cart/Index → Cart/Checkout → Payment → OrderTracking
```

### 3.2 Auth Flow
```
Login/Signup → RoleGuard Middleware → Dashboard theo role
```

### 3.3 Layout Hierarchy
```
_LayoutPageHome.cshtml (cho khách)
  ├── Index              (Home)
  ├── DetailRestaurant   (Home)
  ├── SanPham            (Home)
  ├── Cart/*             (Cart views)
  └── Chat widget (_ChatWidget.cshtml)

_LayoutPageRestaurant.cshtml (cho chủ quán)
  └── Restaurant/*

_LayoutPageShipper.cshtml (cho shipper)
  └── Shipper/*

_LayoutPageAmin.cshtml (cho admin)
  └── Admin/*
```

---

## 4. 🎨 Tài nguyên tĩnh (`wwwroot/`)

| Thư mục | Nội dung |
|---------|----------|
| `wwwroot/Source/Home/` | **Home** template: CSS, JS, images |
| `wwwroot/Source/Restaurant/` | Restaurant dashboard template |
| `wwwroot/Source/Shipper/` | Shipper dashboard template |
| `wwwroot/Source/Admin/` | Admin dashboard template |
| `wwwroot/Source/Cart/` | Cart/Checkout template |
| `wwwroot/Source/Shared/` | **Shared CSS/JS** — design tokens, core CSS |
| `wwwroot/js/` | JS modules: `cart-local.js`, `filter.js`, `map.js`, `lucide-icons.js` |
| `wwwroot/Scripts/` | jQuery, SignalR, validation libs |

### File quan trọng cần biết khi redesign:
- `wwwroot/Source/Shared/css/fastship-design-tokens.css` — Design tokens (màu sắc, font, spacing)
- `wwwroot/Source/Home/css/style.css` — Style chính của Home template
- `wwwroot/Source/Restaurant/css/style.css` — Style dashboard quán
- `wwwroot/Source/Shipper/css/style.css` — Style dashboard shipper
- `wwwroot/Source/Admin/css/style.css` — Style dashboard admin
- `wwwroot/Source/Cart/css/style.css` — Style giỏ hàng

---

## 5. 🛠 Công nghệ

| Layer | Công nghệ | Version |
|-------|-----------|---------|
| Runtime | .NET | 8.0 |
| ORM | Entity Framework Core | 8.x |
| Database | PostgreSQL (Render) | 16.x |
| Realtime | SignalR | 8.x |
| Auth | Cookie Authentication + Google OAuth | — |
| Frontend | Bootstrap 4 / 5 hybrid | — |
| Icons | Font Awesome 5 (CDN) + Lucide (local) | — |
| Payment | MoMo API | — |
| AI | Gemini API (Google) | — |
| Hosting | Render | — |
| Cache | Redis (optional) / In-Memory | — |
| Logging | Serilog + Seq (optional) | — |

---

## 6. 🌐 Environment Variables

| Variable | Mô tả | Bắt buộc |
|----------|-------|----------|
| `DATABASE_URL` | PostgreSQL connection string (Render format) | ✅ |
| `Gemini__ApiKey` | Google Gemini API key | ✅ (cho chatbot) |
| `ALLOWED_ORIGINS` | CORS allowed origins (phân cách bằng `;`) | ❌ |
| `DATA_PROTECTION_KEY_DIR` | Thư mục lưu key Data Protection (persistent) | ❌ |
| `REDIS_URL` | Redis connection string | ❌ |
| `SEQ_URL` | Seq server URL cho logging | ❌ |
| `PORT` | Cổng HTTP (Render tự set) | ✅ (production) |

---

## 7. 🔐 Routes & Authentication

| Route | Role | Layout |
|-------|------|--------|
| `/Home/*` | Public | `_LayoutPageHome` |
| `/Cart/*` | Customer (cần login) | `_LayoutPageHome` |
| `/Restaurant/*` | Restaurant | `_LayoutPageRestaurant` |
| `/Shipper/*` | Shipper | `_LayoutPageShipper` |
| `/Admin/*` | Admin | `_LayoutPageAmin` |
| `/nhantin` (SignalR) | Public (chat) | — |
