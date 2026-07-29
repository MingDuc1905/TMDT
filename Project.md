# Fastship (ShipFood) — Nền tảng Giao Hàng Thức Ăn Online

> **Cập nhật**: Dựa trên mã nguồn thực tế — ASP.NET Core 8 MVC + MySQL (Pomelo) + Bootstrap 5 + SignalR + Gemini AI + Google OAuth
> **Live Demo**: [https://fastship-web.onrender.com/](https://fastship-web.onrender.com/)

---

## 📋 Tổng Quan

**Fastship** (tên mã `ShipFood`) là ứng dụng web **ASP.NET Core 8 MVC** toàn diện để đặt hàng và giao hàng thức ăn online. Nền tảng kết nối **4 vai trò**: Khách hàng, Quán ăn (Restaurant), Shipper và Quản trị viên (Admin) trong một hệ thống thống nhất với real-time messaging (SignalR) và AI chatbot (Gemini).

---

## 🎯 Mục Đích

Cung cấp một giải pháp hoàn chỉnh cho:
- **Khách hàng**: Duyệt quán ăn, đặt hàng, theo dõi giao hàng, đánh giá món ăn
- **Quán ăn (Restaurant)**: Quản lý thực đơn, xử lý đơn hàng, thống kê doanh thu, quản lý khuyến mãi
- **Shipper**: Nhận đơn giao hàng, cập nhật trạng thái, quản lý thu nhập
- **Admin**: Quản lý toàn bộ hệ thống (user, danh mục, đơn hàng), xem dashboard analytics, chat hỗ trợ

---

## 🏗️ Kiến Trúc Kỹ Thuật

### Công Nghệ Stack

| Tầng | Công nghệ | Phiên bản |
|------|-----------|-----------|
| **Backend Framework** | ASP.NET Core | 8.0 |
| **ORM** | Entity Framework Core (Npgsql) | 8.0.11 |
| **Database** | PostgreSQL 15+ (Render Managed) | 15+ |
| **Template Engine** | Razor (Runtime Compilation) | 8.0.11 |
| **Real-time** | SignalR (Groups-based, 12 methods) | 8.0.11 |
| **AI Chatbot** | Google Gemini API | gemini-3.5-flash |
| **Background Service** | AutoPreparingService (polling 10s) | — |
| **Google OAuth** | ASP.NET Core Authentication (tự động tạo tài khoản lần đầu) | 8.0.0 |
| **Charts** | Chart.js | Bundle |
| **Auth** | Cookie Authentication + Session | ASP.NET Core |
| **CORS** | Restricted (ALLOWED_ORIGINS env var) | ASP.NET Core |

### Frontend Stack

| Công nghệ | Mục đích |
|-----------|----------|
| **Bootstrap 5** | Responsive grid + components |
| **fastship-design-tokens.css** | **NEW v4.0** Global Design System (unified tokens, 16 component systems) |
| **jQuery 3.7.1** | DOM manipulation + AJAX |
| **Font Awesome 5** | **Icon system duy nhất** (đã xoá Bootstrap Icons, Flaticon, LineIcons, Line Awesome, Themify, Simple Line Icons, Material Design Iconic, Avasta, Icomoon, Font Awesome Old — chỉ còn FA5 CDN + Emojis) |
| **WOW.js + Animate.css** | Scroll animations (legacy) |
| **IntersectionObserver (vanilla)** | **NEW** Scroll-reveal: `fs-reveal`, stagger `--fs-i`, counter animation `data-count` |
| **OwlCarousel 2** | Carousel/slider |
| **DataTables** | Server-side table pagination |
| **Perfect Scrollbar** | Custom scrollbar |
| **metismenu** | Sidebar accordion menu |
| **Chart.js** | Dashboard charts (line, doughnut, bar) |
| **Leaflet.js 1.9.4** | Live order tracking maps |
| **Inter (Google Fonts)** | Unified system font (thay thế Open Sans, Lora, Cairo, Poppins, Montserrat, Nunito) |

### NuGet Packages

```xml
<PackageReference Include="Google.GenAI" Version="1.11.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Common" Version="8.0.11" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="8.0.11" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.AspNetCore.DataProtection.Extensions" Version="8.0.11" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.11" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
```

---

## 📁 Cấu Trúc Dự Án Chi Tiết

```
TMDT-master/
├── ShipFoodCore/                    # Main web application
│   ├── Controllers/                 # MVC Controllers (9 files)
│   │   ├── HomeController.cs        # Landing page, login/signup, search, reviews
│   │   ├── CartController.cs        # Cart, checkout, order history
│   │   ├── RestaurantController.cs  # Restaurant dashboard, products, orders
│   │   ├── ShipperController.cs     # Shipper dashboard, orders, earnings
│   │   ├── AdminController.cs       # Admin dashboard, user management, stats
│   │   ├── AdminChatController.cs   # Customer support chat (SignalR)
│   │   ├── ChatbotController.cs     # AI chatbot (Gemini + DB queries)
│   │   ├── PaymentController.cs     # Mock payment processing
│   │   └── BaseController.cs        # Shared session/cart methods
│   │
│   ├── Models/                      # Entity Framework models (19 files)
│   │   ├── DbContext.cs             # dbFoodyEntities context + Fluent API
│   │   ├── Cart.cs                  # Cart logic (session-based)
│   │   ├── tbUser.cs                # Users (4 roles: Khách hàng, Quán ăn, Shipper, Admin)
│   │   ├── tbQuanAn.cs              # Restaurants (FK→tbUser.userid)
│   │   ├── tbMonAn.cs               # Menu items (soft delete: isDeleted)
│   │   ├── tbDonHang.cs             # Orders (FK→tbQuanAn, tbShipper, tbKhuyenMai, tbLoaiHinhThanhToan)
│   │   ├── tbChiTietDonHang.cs      # Order details (FK→tbDonHang, tbBienTheMonAn)
│   │   ├── tbBienTheMonAn.cs        # Product variants (size M/L/XL, each with price)
│   │   ├── tbDanhGia.cs             # Reviews (FK→tbChiTietDonHang)
│   │   ├── tbDanhMuc.cs             # Categories (with icon field)
│   │   ├── tbKhuyenMai.cs           # Discounts/coupons
│   │   ├── tbMonAnKhuyenMai.cs      # Product-discount mapping (FK→tbBienTheMonAn)
│   │   ├── tbShipper.cs             # Shippers (FK→tbUser.userid)
│   │   ├── tbAdmin.cs               # Admin (FK→tbUser.userid, has hinhanh avatar)
│   │   ├── tbKhachHang.cs           # Customers (FK→tbUser.userid, has hinhanh)
│   │   ├── tbThongTinDatHang.cs     # Delivery addresses
│   │   ├── tbTinNhan.cs             # Chat messages (FK→tbDonHang, tbKhachHang, tbShipper)
│   │   ├── tbLoaiHinhThanhToan.cs   # Payment methods (COD, Chuyển khoản, MoMo, ZaloPay, PayPal)
│   │   ├── tbLichSuSuDungKhuyenMai.cs# Coupon usage history
│   │   ├── tbEInvoice.cs            # Electronic invoice
│   │   ├── DataAnalytic.cs          # Analytics view models
│   │   ├── LichSuDonHang.cs         # Order history view models
│   │   └── DonHangDangLam.cs        # Raw SQL order queries (FREE-PICK)
│   │
│   ├── Hubs/                        # SignalR hubs
│   │   └── Chats.cs                 # /nhantin hub (12 methods, 5 groups: order_{id}, customer_{id}, restaurant_{id}, shippers)
│   │
│   ├── Services/                    # Business logic services
│   │   ├── RecommendationService.cs  # ML-based recommendations (8 algorithms: collaborative, apriori, popular-pairs, time-based, trending, restaurant-insights, category-insights)
│   │   ├── GeminiService.cs         # Gemini AI API integration (gemi-ni-3.5-flash, 5 req/min rate limit)
│   │   ├── MoMoService.cs           # MoMo payment HMAC-SHA256 sandbox
│   │   ├── VoucherService.cs        # Voucher management logic
│   │   ├── EDeliveryService.cs      # Electronic delivery tracking
│   │   └── AutoPreparingService.cs  # BackgroundService: 10s polling → SignalR broadcast
│   │
│   ├── Utils/                       # Helper utilities
│   │   ├── TinhToan.cs             # Shipping fee calculation, image URL helper
│   │   └── OrderStatus.cs          # **NEW** Order status constants, transition validation, auto-message templates
│   │
│   ├── Views/                       # Razor views (50+ views, 6 layouts)
│   │   ├── Home/                   # Customer-facing pages (11 views)
│   │   │   ├── Index.cshtml        # Homepage: hero carousel, stats, restaurant grid, AI combos, how-it-works
│   │   │   ├── Login.cshtml        # Login page: form + Google OAuth
│   │   │   ├── Signup.cshtml       # Registration: form 8 fields
│   │   │   ├── Forgot.cshtml       # Forgot password
│   │   │   ├── SelectRoleGoogle.cshtml  # Google OAuth role selection (3 cards)
│   │   │   ├── DetailRestaurant.cshtml  # Restaurant detail: categories sidebar + menu grid + reviews
│   │   │   ├── ChiTietSanPham.cshtml    # Product detail: hero 2-col, size chips, add-to-cart, similar items, reviews
│   │   │   ├── DanhMuc.cshtml      # Category listing: grid of category cards
│   │   │   ├── SanPham.cshtml      # Products by category: grid of menu items
│   │   │   ├── NhanTin.cshtml      # Customer messaging page
│   │   │   ├── About.cshtml        # About page
│   │   │   └── Contact.cshtml      # Contact page
│   │   ├── Cart/                   # Cart, checkout, history (7 views)
│   │   │   ├── Index.cshtml        # Cart page: items list + summary + AI suggestions
│   │   │   ├── Checkout.cshtml     # Checkout: address 3-tabs, coupon, payment methods, order summary
│   │   │   ├── ChiTietDonHang.cshtml    # Order detail: flat invoice + live map
│   │   │   ├── LichSuDatHang.cshtml     # Order history: DataTable with status badges
│   │   │   ├── OrderTracking.cshtml     # Live tracking: 7-step progress, Leaflet map, SignalR
│   │   │   ├── SuccessView.cshtml       # Payment success
│   │   │   ├── FailureView.cshtml       # Payment failure
│   │   │   └── EInvoice.cshtml          # E-invoice page
│   │   ├── Restaurant/             # Restaurant dashboard (12 views)
│   │   │   ├── Index.cshtml        # Dashboard: KPI, apriori insights, recent orders
│   │   │   ├── OrderList.cshtml    # Order management: DataTable + actions
│   │   │   ├── ProductList.cshtml  # Menu items CRUD
│   │   │   ├── ProductDetail.cshtml# Menu item detail/edit
│   │   │   ├── AddProduct.cshtml   # Add new menu item
│   │   │   ├── Analytics.cshtml    # Revenue analytics by category, top items
│   │   │   ├── Discount.cshtml     # Discount/coupon management
│   │   │   ├── Review.cshtml       # Reviews management
│   │   │   ├── Profile.cshtml      # Restaurant profile settings
│   │   │   ├── GeneralCustomer.cshtml# Customer list
│   │   │   ├── Wallet.cshtml       # Wallet/earnings
│   │   │   ├── Scanner.cshtml      # QR scanner
│   │   │   └── NhanTin.cshtml      # Restaurant messaging
│   │   ├── Shipper/                # Shipper dashboard (9 views)
│   │   │   ├── Index.cshtml        # Dashboard: split-screen with orders + map + stats
│   │   │   ├── OrderDetail.cshtml  # Order detail + Leaflet map + SignalR geolocation
│   │   │   ├── LichSu.cshtml       # Delivery history
│   │   │   ├── ThuNhap.cshtml      # Earnings statistics (30-day)
│   │   │   ├── ViTien.cshtml       # Wallet balance
│   │   │   ├── QRDelivery.cshtml   # QR delivery scan
│   │   │   ├── CaiDat.cshtml       # Settings/profile
│   │   │   ├── ThongBao.cshtml     # Notifications
│   │   │   └── NhanTin.cshtml      # Shipper messaging
│   │   ├── Admin/                  # Admin panel (14 views)
│   │   │   ├── Dashboard.cshtml    # Dashboard: KPI cards, line/pie charts, top items, revenue tables
│   │   │   ├── Index.cshtml        # Welcome page
│   │   │   ├── QuanLyQuanAn.cshtml # Manage restaurants
│   │   │   ├── QuanLyQuanTriVien.cshtml # Manage admins
│   │   │   ├── QuanLyShipper.cshtml# Manage shippers
│   │   │   ├── QuanLyKhachHang.cshtml# Manage customers
│   │   │   ├── Order.cshtml        # Orders list with SignalR real-time
│   │   │   ├── OrderDetail.cshtml  # Order detail
│   │   │   ├── EditOrder.cshtml    # Edit order
│   │   │   ├── Category.cshtml     # Categories CRUD
│   │   │   ├── CreateCategory.cshtml# Create category
│   │   │   ├── EditCategory.cshtml # Edit category
│   │   │   ├── PostTaiKhoan.cshtml # Account management
│   │   │   ├── VoucherManager.cshtml# Voucher/coupon management
│   │   │   └── DeliveryLogs.cshtml # Delivery logs
│   │   ├── AdminChat/              # Admin support chat (1 view)
│   │   │   └── Index.cshtml        # Real-time chat with customers (SignalR)
│   │   ├── EDelivery/              # Electronic delivery (1 view)
│   │   │   └── ScanResult.cshtml   # QR scan result display
│   │   └── Shared/                 # Layouts + partials (9 files)
│   │       ├── _LayoutPageHome.cshtml  # Customer layout: topbar + navbar + footer + chat widget
│   │       ├── _LayoutPageAmin.cshtml  # Admin layout: sidebar + header + content
│   │       ├── _LayoutPageRestaurant.cshtml# Restaurant layout: sidebar + header + content
│   │       ├── _LayoutPageShipper.cshtml # Shipper layout: sidebar + header + content + Leaflet
│   │       ├── _LayoutAuth.cshtml     # Auth layout: glassmorphism header + centered form
│   │       ├── _Layout.cshtml         # Legacy layout (Bootstrap 3)
│   │       ├── LayoutPageShipper.cshtml# Legacy shipper layout
│   │       ├── _ChatWidget.cshtml     # Floating chat widget (AI + Support)
│   │       ├── _ValidationScriptsPartial.cshtml # Validation scripts
│   │       ├── Error.cshtml           # Error page
│   │       ├── Components/FilterBar/Default.cshtml# Filter bar ViewComponent
│   │       └── Web.config             # Web config
│   │
│   ├── wwwroot/                     # Static assets
│   │   ├── Source/Home/            # Customer theme (style.css, layout-sg.css, login.css, details.css, base.css)
│   │   │   ├── css/               # style.css, bootstrap.min.css, layout-sg.css, login.css, details.css, base.css, chat.css
│   │   │   ├── js/               # main.js
│   │   │   ├── lib/              # owlcarousel, wow, easing, waypoints
│   │   │   └── img/              # banners, icons, favicon
│   │   ├── Source/Cart/           # Ogani cart theme
│   │   ├── Source/Restaurant/     # Restaurant dashboard (style-restaurant.css)
│   │   ├── Source/Shipper/        # Shipper dashboard (style-shiper.css)
│   │   ├── Source/Admin/          # Admin dashboard (style-admin.css)
│   │   ├── Source/Shared/         # Global: fastship-design-tokens.css
│   │   ├── Content/               # Legacy Bootstrap 3
│   │   ├── js/                   # filter.js, cart-local.js, map.js
│   │   └── css/                  # site.css
│   │
│   ├── Program.cs                   # App startup (DI, middleware, config)
│   └── appsettings.json             # Configuration (connection strings, API keys)
│
├── UI-UX.md                         # Comprehensive UI/UX documentation (27 sections)
├── Architectural-Solution.md        # Architectural solution document
├── Project.md                       # This file
├── mysql_utf8.sql                   # Combined seed data (categories, users, menus) + UTF-8 init
├── Dockerfile                       # Multi-stage Docker build (SDK 8.0 → runtime)
├── render.yaml                     # Render deployment config
├── seed.sql                         # Seed data SQL
├── .agents/skills/                  # Codebuff skill rules
│   └── fastship-rules.md           # FastShip development rules
├── e2e-tests/                      # Playwright E2E tests
│   ├── tests/                     # 10 test spec files
│   ├── pages/                     # Page Object Model (9 pages)
│   ├── fixtures/                  # Test fixtures
│   └── playwright.config.ts       # Playwright config
└── .dockerignore                   # Docker ignore rules
```

---

## 🔑 Tính Năng Chính

### 1. 🛍️ Khách Hàng (Customer)

| Tính năng | Chi tiết |
|-----------|----------|
| **Tìm kiếm quán ăn** | Server-side, không phân biệt dấu Unicode, tìm theo tên/user/món + lọc danh mục |
| **Thực đơn chi tiết** | Danh mục sidebar + tìm món + filter "Đã mua" + khuyến mãi badge |
| **Giỏ hàng** | Session-based (JSON), AJAX quantity, coupon, 3 address modes |
| **Thanh toán** | COD (immediate) + MoMo (HMAC-SHA256 sandbox) + Chuyển khoản (mock: Vietcombank) |
| **Đánh giá** | AJAX star picker + textarea + "Xem thêm" paginate 6/lần |
| **Lịch sử đơn** | DataTable với sort/search + trạng thái badge màu + emoji |
| **Gợi ý** | 8 thuật toán: collaborative filtering, Apriori (multi-element), popular pairs, time-based, trending (48h), restaurant insights, category insights |
| **Chat** | AI chatbot (Gemini) + Admin support (SignalR) |

### 2. 🏪 Quán Ăn (Restaurant)

| Tính năng | Chi tiết |
|-----------|----------|
| **Dashboard** | KPI cards (số món, doanh thu, đơn hàng, khách hàng) |
| **Quản lý món** | CRUD + upload hình ảnh |
| **Đơn hàng** | DataTable + Nhận/Hủy/Xem chi tiết |
| **Đánh giá** | Thống kê điểm + phân bố sao + filter + danh sách |
| **Phân tích** | Doanh thu theo danh mục, top món bán chạy |
| **Khuyến mãi** | Gắn mã KM cho món ăn (phần trăm, số lượng) |
| **Profile** | Cập nhật thông tin + mật khẩu + avatar + toggle mở/đóng |
| **Toggle trạng thái** | Đóng cửa → Đang mở cửa (1 click) |

### 3. 🚚 Shipper

| Tính năng | Chi tiết |
|-----------|----------|
| **FREE-PICK** | Đơn chưa có shipper (raw SQL query) |
| **Danh sách đơn** | Bảng + dropdown Chấp nhận/Từ chối |
| **Cập nhật** | Đã lấy → Hoàn thành (AJAX JSON) |
| **Thu nhập** | Thống kê 30 ngày + hôm nay (hoàn thành, hủy) |
| **Ví tiền** | Danh sách đơn + số dư |
| **Chat** | SignalR with shipper + customer |

### 4. 👑 Admin

| Tính năng | Chi tiết |
|-----------|----------|
| **Dashboard** | 4 KPI cards + 3 charts (Chart.js) + date filter |
| **Quản lý user** | CRUD cho 4 roles + Duyệt/Hủy/Khóa/Mở khóa |
| **Bảo vệ** | Không thể khóa admin cuối cùng |
| **Quản lý danh mục** | CRUD + kiểm tra ràng buộc trước xóa |
| **Quản lý đơn** | DataTable + chi tiết + dropdown action |
| **Chat hỗ trợ** | Real-time với SignalR groups |
| **Export** | CSV doanh thu |

---

## 💾 Cơ Sở Dữ Liệu

### Database: MySQL 8+ (MySqlServerVersion 8.0.20)

**Connection**: `dbFoodyEntities` (Pomelo.EntityFrameworkCore.MySql)
> **Fix v2.6**: Đổi từ `MariaDbServerVersion(10,6)` → `MySqlServerVersion(8,0,20)` để tắt RETURNING clause (MySQL < 8.0.21 không hỗ trợ). Nếu cần auto-detect: `ServerVersion.AutoDetect(connectionString)`.

> **⚠️ Quan trọng**: `tbChiTietDonHang.mamon` là FK → `tbBienTheMonAn.id` (không phải `tbMonAn.mamon`). Mọi Apriori query cần bridge mapping qua tbBienTheMonAn.

**Google OAuth Auto-Create**: Khi người dùng đăng nhập Google lần đầu (email chưa tồn tại trong DB), hệ thống tự động:
1. Tạo password ngẫu nhiên `GG_{Guid}` → lưu plain-text
2. Tạo `tbUser` với username `gg_{guid12}`, email truncate 50 ký tự, role "Khách hàng", trạng thái active (trangthai=1)
3. Đồng bộ `tbKhachHang` với tên từ Google profile (cắt 50 ký tự)
4. Gán session + redirect thẳng vào trang chủ (không cần duyệt thủ công)

**Tables** (17 bảng):
| Bảng | Mô tả | Quan hệ |
|------|-------|---------|
| `tbUser` | Người dùng (4 roles) | 1:1 → tbKhachHang, tbQuanAn, tbShipper, tbAdmin |
| `tbKhachHang` | Khách hàng | 1:N → tbThongTinDatHang, tbTinNhan |
| `tbQuanAn` | Quán ăn | 1:N → tbMonAn, tbDonHang |
| `tbMonAn` | Món ăn (soft delete: isDeleted) | N:1 → tbDanhMuc; 1:N → tbBienTheMonAn; FK→tbQuanAn: RESTRICT |
| `tbBienTheMonAn` | **NEW** Biến thể món (size: M/L/XL) | N:1 → tbMonAn; 1:N → tbChiTietDonHang, tbMonAnKhuyenMai |
| `tbDanhMuc` | Danh mục món | 1:N → tbMonAn (RESTRICT) |
| `tbDonHang` | Đơn hàng | N:1 → tbQuanAn, tbKhuyenMai, tbLoaiHinhThanhToan, tbShipper |
| `tbChiTietDonHang` | Chi tiết đơn | N:1 → tbBienTheMonAn (mamon = FK→tbBienTheMonAn.id), 1:N → tbDanhGia |
| `tbDanhGia` | Đánh giá | N:1 → tbChiTietDonHang |
| `tbKhuyenMai` | Khuyến mãi | 1:N → tbMonAnKhuyenMai, tbDonHang, tbLichSuSuDungKhuyenMai |
| `tbMonAnKhuyenMai` | KM của món | N:1 → tbBienTheMonAn (mamon), tbKhuyenMai |
| `tbLoaiHinhThanhToan` | Hình thức TT | 1:N → tbDonHang |
| `tbThongTinDatHang` | Địa chỉ giao | 1:N → tbDonHang |
| `tbShipper` | Shipper | 1:N → tbDonHang, tbTinNhan |
| `tbAdmin` | Quản trị viên | 1:1 → tbUser |
| `tbTinNhan` | Tin nhắn chat | N:1 → tbDonHang, tbKhachHang, tbShipper |
| `City/District` | Địa danh | — |
| `tbLichSuSuDungKhuyenMai` | **NEW** Lịch sử dùng mã giảm giá | N:1 → tbUser, tbKhuyenMai; ghi nhận userid, makm, ngaydung |

**Key columns**:
| Bảng | Cột | Mô tả |
|------|------|-------|
| `tbDonHang` | `momo_trans_id` | **NEW** Lưu mã giao dịch MoMo để dùng cho Refund |
| `tbMonAn` | `conhang` | **NEW** BIT DEFAULT 1 — toggle còn hàng/hết (toggle 1-click AJAX) |
| `tbMonAn` | `giatien` | Giá gốc (đã thay thế bằng quản lý qua tbBienTheMonAn) |
| `tbMonAn` | `isDeleted` | **NEW** BIT DEFAULT 0 — xóa mềm (soft delete), bảo toàn lịch sử hóa đơn |
| `tbLichSuSuDungKhuyenMai` | `userid` | FK → tbUser, ghi nhận user dùng mã |
| `tbLichSuSuDungKhuyenMai` | `makm` | FK → tbKhuyenMai, mã giảm giá đã dùng |
| `tbLichSuSuDungKhuyenMai` | `ngaydung` | DATETIME, thời điểm áp dụng mã |

**Seed data**: `mysql_utf8.sql` — categories, users, restaurants, menu items (tự động seed khi DB được tạo lần đầu).

**ViewModels (không phải bảng)**:
- `DonHangDangLam` — Kết quả raw SQL cho Shipper FREE-PICK
- `DataAnalytic` + `DataAnalyticDanhMuc` — Analytics cho Restaurant Dashboard
- `LichSuDonHang` — Lịch sử đơn hàng (hiện không dùng)
- `Cart` — Session-based cart (JSON serialize/deserialize)

---

## 🔒 Authentication & Authorization

### Phương thức
- **Primary**: Session-based (JSON trong `HttpContext.Session`)
- **Cookie**: ASP.NET Core Cookie Authentication (Sliding 30 ngày)
- **Google OAuth**: Optional (chỉ kích hoạt nếu có ClientId/Secret trong config)

### Role-based access
| Role | Session check `loaitaikhoan` | Routes |
|------|------------------------------|--------|
| Admin | `"Admin"` | `/Admin/*` |
| Restaurant | `"Quán ăn"` | `/Restaurant/*` |
| Shipper | `"Shipper"` | `/Shipper/*` |
| Customer | `"Khách hàng"` | `/Home/*`, `/Cart/*` |



---

## 🔌 API Endpoints

### JSON API (AJAX)

| Method | Route | Controller | Mô tả |
|--------|-------|-----------|-------|
| `GET` | `/Home/GetReviews?quanId=&page=&pageSize=` | Home | Load reviews (paginated) |
| `POST` | `/Home/SubmitReview` | Home | Submit review (anti-forgery) |
| `GET` | `/Home/GetReviewableItems?quanId=` | Home | Get user's purchased items |
| `GET` | `/Cart/GetAvailableCoupons` | Cart | Danh sách mã giảm giá khả dụng (còn hạn, sắp xếp giảm dần) |
| `POST` | `/Cart/CheckCoupon` | Cart | Validate coupon code |
| `POST` | `/Payment/ProcessPayment` | Payment | **Payment processing**: force re-read giá từ DB, ghi nhận coupon usage, trả về error message chi tiết (inner exception) |
| `POST` | `/Chatbot/SendMessage` | Chatbot | AI chatbot message |
| `POST` | `/Shipper/UpdateDonHang` | Shipper | Update delivery status |
| `GET` | `/Admin/GetDashboardStats` | Admin | Dashboard stats (date filter) |
| `GET` | `/Admin/GetRevenueChart` | Admin | Revenue chart data |
| `GET` | `/Admin/GetTopRestaurants` | Admin | Top restaurants |
| `GET` | `/Admin/GetOrderStatusPie` | Admin | Order status distribution |
| `GET` | `/AdminChat/GetConversations` | AdminChat | Customer conversations list |
| `GET` | `/AdminChat/GetCustomerMessages` | AdminChat | Customer message history |
| `POST` | `/AdminChat/SendMessageToCustomer` | AdminChat | Admin send message |
| `POST` | `/AdminChat/CustomerSendMessage` | AdminChat | Customer send message (widget) |
| `GET` | `/AdminChat/GetMyMessages` | AdminChat | User's own messages |
| `GET` | `/AdminChat/GetUnreadCount` | AdminChat | Unread message count |
| `GET` | `/AdminChat/GetUserOrders` | AdminChat | User's orders for chat |
| `GET` | `/Home/SearchAutocomplete?q=` | Home | Search autocomplete (debounce 300ms) |
| `GET` | `/Home/MenuSearch` | Home | **NEW** Dynamic SQL menu search — filter by category, price, promo, rating |
| `GET` | `/Home/FixPasswords` | Home | **NEW** Ghi đè BCrypt hash → plain-text password (1 lần sau deploy) |
| `GET` | `/Home/SeedDb` | Home | **NEW** Seed database từ mysql_utf8.sql |
| `POST` | `/Restaurant/ToggleConHang` | Restaurant | AJAX toggle 1-click hết hàng |
| `GET` | `/Restaurant/hoantatdon/{id}` | Restaurant | Chuẩn bị xong → status 'Chờ shipper lấy hàng' + SignalR broadcast to shippers |
| `GET` | `/health` | — | Healthcheck (no DB needed, always 200 OK) |
| `POST` | `/Home/GoogleResponse` | Home | Google OAuth callback (auto-create + redirect) |

### SignalR Hub
- **Endpoint**: `/nhantin` 
- **Hub**: `Chats.cs` (12 methods, 5 group types)
  - `Message(message, id)` — Broadcast to all
  - `AdminSendMessage(message, orderId, connectionId)` — Admin → group `order_{orderId}`
  - `CustomerSendMessage(message, orderId, userName)` — Customer → group `order_{orderId}`
  - `JoinOrderGroup(orderId)` — Join per-order group `order_{orderId}`
  - `JoinCustomerSupportGroup(userId)` — Join per-user group `customer_{userId}`
  - `JoinRestaurantGroup(restaurantId)` — Join restaurant group `restaurant_{restaurantId}` (newOrder events)
  - `JoinShipperGroup()` — Join shippers broadcast group `shippers` (newPickupOrder events)
  - `SendToOrderGroup(message, orderId, senderName, role)` — Send within order group
  - `NotifyNewMessage(userId, count)` — Real-time unread badge to `customer_{userId}`
  - `NotifyShippersNewPickup(orderId, restaurantName, pickupAddress)` — Broadcast to `shippers` group
  - `UpdateLocation(orderId, lat, lng)` — Shipper coordinate streaming → `order_{orderId}` group
  - `OnConnectedAsync` — Track connection + broadcast `userOnline` (đọc userId từ query string)
  - `OnDisconnectedAsync` — Remove tracking + broadcast `userOffline` + `shipperOffline`

**Helpers (instance methods, DI)**:
  - `IsUserOnline(userId)` → `async Task<bool>`
  - `GetUserConnectionId(userId)` → `async Task<string?>`
  - Có try-catch + logger fallback khi Redis lỗi

**Connection tracking**: `IDistributedCache` (Redis) — Key `UserConnection:{userId}` → connectionId. Fallback sang RAM nếu Redis không khả dụng. Đảm bảo không mất kết nối khi container restart hoặc scale multi-instance.

**SignalR client events (listen)**:
  - `orderStatusChanged(orderId, status, time)` — Broadcast từ Restaurant/Shipper
  - `shipperLocationUpdate(orderId, lat, lng)` — Live coordinates từ Shipper
  - `newOrder(data)` — Restaurant nhận đơn mới khi thanh toán
  - `newPickupOrder(data)` — Shipper nhận đơn chờ lấy hàng
  - `paymentConfirmed(orderId, amount)` — Xác nhận thanh toán
  - `paymentFailed(orderId, message)` — Thanh toán thất bại
  - `adminMessage(message, orderId, sender)` — Admin gửi tin nhắn
  - `customerMessage(message, orderId, userName)` — Khách hàng gửi tin nhắn
  - `unreadCountUpdate(count)` — Badge tin nhắn chưa đọc
  - `userOnline(userId, isOnline)` — Trạng thái online/offline

---

## 🚀 Deployment

### Docker
- **Multi-stage build**: SDK 8.0 → runtime ASP.NET 8.0
- **Port**: 8080 (ENV `ASPNETCORE_URLS=http://+:8080`)
- **Healthcheck**: `/health` endpoint (200 OK)
- **Entrypoint**: `dotnet ShipFoodCore.dll`

### Render
- **Builder**: Dockerfile (automatic)
- **Replicas**: 1
- **Restart**: ON_FAILURE, max 3 retries
- **MySQL**: Auto-config từ Render env vars (MYSQLHOST, MYSQLPORT, MYSQLUSER, MYSQLPASSWORD, MYSQLDATABASE)

### Database Initialization
- `EnsureCreated()` tự động tạo bảng khi chạy lần đầu
- `mysql_utf8.sql` tự động seed data nếu DB trống
- App vẫn start kể cả khi MySQL chưa sẵn sàng (try-catch)

### Environment Variables
```env
# MySQL (Render auto)
MYSQLHOST=localhost
MYSQLPORT=3306
MYSQLUSER=root
MYSQLPASSWORD=
MYSQLDATABASE=dbFoody

# Or full URL
MYSQL_URL=Server=...;Database=...;User=...;Password=...;

# Google OAuth (optional)
Authentication__Google__ClientId=xxx
Authentication__Google__ClientSecret=xxx

# Gemini AI (optional) — Ưu tiên đọc từ Environment Variable trước, fallback xuống appsettings.json
# Trên Render: set Gemini__ApiKey=xxx (dùng __ thay : vì env var không hỗ trợ dấu hai chấm)
Gemini__ApiKey=xxx

# Serilog Seq (optional)
SEQ_URL=http://localhost:5341
SEQ_API_KEY=xxx

# CORS Allowed Origins
ALLOWED_ORIGINS=https://fastship-web.onrender.com

# App Domain (for CORS fallback)
APP_DOMAIN=https://fastship-web.onrender.com
```

---

## 📊 Recommendation System

| Loại | Algorithm | Implementation |
|------|-----------|----------------|
| **Personalized** | Collaborative filtering | Tìm user cùng sở thích → gợi ý món chưa đặt. Fallback: trending |
| **Apriori (Multi-element)** | Support + Confidence (min 2%, min 50%) | Support = count(A∩B)/D, Confidence = count(A∩B)/count(A). 3-level fallback. Hỗ trợ đa phần tử (A,B,C→D). |
| **Popular Pairs** | Co-occurrence counting | Đếm cặp món xuất hiện cùng đơn → top N cặp phổ biến nhất. Dùng cho trang chủ "Gợi ý Combo" |
| **Time-based** | Keyword matching | Theo giờ: sáng (phở/bún) / trưa (cơm) / tối (lẩu/nướng) / khuya (trà sữa) |
| **Trending (48h)** | Sales volume (48h) | Top bán chạy 48h, fallback all-time |
| **Restaurant Insights** | Apriori grouped by restaurant | **NEW** Phân tích cặp món bán chéo cho Dashboard Nhà hàng. Bridge: tbBienTheMonAn.id ↔ tbMonAn.mamon |
| **Category Insights** | Apriori grouped by category | **NEW** Phân tích liên kết Danh mục cho Admin Dashboard. Bridge: tbMonAn.mamon → tbBienTheMonAn.id → tbDanhMuc |

**Implementation Notes**:
- `tbChiTietDonHang.mamon` là FK → `tbBienTheMonAn.id` (không phải `tbMonAn.mamon`)
- Tất cả Apriori queries đều cần bridge mapping: `tbMonAn.mamon → tbBienTheMonAn.id → ct.mamon`
- 3-level fallback: (1) Support + Confidence ngưỡng → (2) Chỉ Confidence → (3) Top Confidence không lọc

---

## 🤖 AI Chatbot (Gemini)

### API
- **Model**: `gemini-3.5-flash` (free tier) — gemini-2.0-flash retired as of 1/6/2026
- **System prompt**: Tiếng Việt, ngắn gọn, thân thiện, dùng `systemInstruction` chính thức của Gemini API
- **Context**: Phí ship 15,000đ (free ≥100,000đ), giao 30-45 phút, 7:00-21:30
- **Temperature**: 0.7, **MaxOutputTokens**: 800

### Database Queries
- `#123` hoặc `mã 123` hoặc `đơn 123` hoặc `order 123` hoặc `tra 123` → Tra cứu đơn hàng (trạng thái + emoji + thông tin shipper)
- "gợi ý", "nên ăn", "bán chạy", "hot", "ngon", "món gì", "đề xuất", "gọi ý" → Top 5 món bán chạy (GroupBy + Sum)

### Features
- `EnableRateLimiting("gemini-policy")` — 5 requests/minute per user
- Contextual quick replies (dựa trên từ khóa trong message)
- Conversation history (20 messages trong session, lưu dạng JSON)
- Fallback: hướng dẫn dùng lệnh khi Gemini không khả dụng hoặc chưa cấu hình API key
- **API Key**: Đọc từ `Environment.GetEnvironmentVariable("Gemini__ApiKey")` (Render env var) trước, fallback `configuration["Gemini:ApiKey"]` (appsettings.json). KHÔNG lưu vào Session tạm.
- **HTTP 429 Handling**: Khi Gemini trả về Too Many Requests, trả về thông báo thân thiện: "⚠️ Hệ thống AI đang quá tải do lượt truy cập cao vào giờ cao điểm, vui lòng thử lại sau 1 phút." — không crash UI, không ẩn khung chat

---

## 📝 Ghi Chú Phát Triển

- **Framework**: ASP.NET Core 8 (not MVC 5)
- **Database**: MySQL 8+ with Pomelo (MySqlServerVersion 8.0.20, not MariaDb)
- **ORM**: Entity Framework Core 8 (not EF6)
- **Frontend**: Bootstrap 5 (not Bootstrap 3/4)
- **Auth**: Cookie + Session (not Identity Framework)
- **Password**: Plain-text (so sánh trực tiếp `user.pwd == pwd`, không hash)
- **Data Protection**: PersistKeysToFileSystem + SetDefaultKeyLifetime(90 ngày) — tránh mất khóa cookie khi restart container
- **Payment**: MoMo Sandbox (HMAC-SHA256, create/refund/query) + Mock COD
- **Font**: Inter (unified) — removed Open Sans, Lora, Cairo, Poppins, Montserrat, Nunito
- **Charts**: Chart.js (not any commercial charting library)
- **Real-time**: SignalR 8 (not WebSocket raw, 12 methods, 5 group types)
- **AI**: Google Gemini gemini-3.5-flash (free tier, not any paid AI service)
- **Deploy**: Docker + Render (not IIS)

### CI/CD
- Docker multi-stage build (SDK → Runtime)
- Render auto-deploy with healthcheck
- Environment variables for all secrets

### Tech Debt / Cần cải thiện
- [x] ✅ Password plain-text (xoá BCrypt, so sánh trực tiếp `user.pwd == pwd`)
- [x] ✅ Real-time SignalR (bỏ 30s polling, ConcurrentDictionary)
- [x] ✅ Font consolidation (Inter only)
- [x] ✅ Skeleton loading (shimmer CSS)
- [x] ✅ Cart JSON API
- [x] ✅ Responsive dashboards (data-label, touch targets)
- [x] ✅ Live order tracking (Leaflet.js + SignalR)
- [x] ✅ Database migrations (hybrid MigrateAsync + EnsureCreated)
- [x] ✅ API rate limiting (3 policies)
- [x] ✅ Search autocomplete (debounce 300ms)
- [x] ✅ AJAX Toggle 1-Click hết hàng
- [x] ✅ Mock Payment Webhook + SignalR broadcast
- [x] ✅ **Auto-preparing 10s polling (BackgroundService)**
- [x] ✅ Redis Distributed Session
- [x] ✅ Server-side pagination cho reviews (GetReviews IQueryable)
- [x] ✅ CORS policy (ALLOWED_ORIGINS, AllowCredentials)
- [x] ✅ Centralized logging (Serilog + Console + Seq)
- [x] ✅ Bootstrap 3 cleanup (xóa 8 files, -7,472 lines)
- [x] ✅ 429 frontend handler (Login AJAX + Checkout error callback)
- [x] ✅ Smart sticky nav (topbar scrolls away, nav fixed)
- [x] ✅ Bootstrap carousel re-trigger on window.load
- [x] ✅ **Global Design System v4.0** (fastship-design-tokens.css + dashboard override cho 4 layouts)
- [x] ✅ **Icon cleanup v4.1** (xoá 10 icon libraries thừa, chỉ còn FA5 + Emojis, -173k dòng)
- [x] ✅ **Fix: MariaDb → MySqlServerVersion 8.0.20** (giải quyết RETURNING syntax error)
- [x] ✅ **Google OAuth auto-create account** (tự động tạo tbUser + tbKhachHang khi login lần đầu)
- [x] ✅ **Carousel-fade crossfade + negative margin hero** (100vh hero, promo band compact)
- [x] ✅ **Topbar/promo band thu gọn** (34px topbar, 8px padding promo, font-size 11.5/13px)
- [x] ✅ **Apriori Support algorithm** (tính Support = cặp món cùng đơn / tổng đơn, minSup 2%, sort giảm dần)
- [x] ✅ **MoMo Payment**: HMAC-SHA256 sandbox, create payment, IPN callback, Refund API
- [x] ✅ **RoleGuard Middleware**: Chặn truy cập chéo role ở middleware layer
- [x] ✅ **Real-time Order Tracking**: 7-step progress bar + Leaflet live map + SignalR status broadcasts
- [x] ✅ **CheckRoleJson filter**: 403 Forbidden cho JSON API endpoints
- [x] ✅ **DECIMAL(2,1) rating**: Sửa kiểu diemdanhgia từ DECIMAL(18,0) → DECIMAL(2,1)
- [x] ✅ **ON DELETE RESTRICT**: Bảo vệ danh mục không bị xóa khi còn món ăn
- [x] ✅ **TryParseCoordinates helper**: Xử lý chuỗi toado, fallback tọa độ TP.HCM
- [x] ✅ **MoMo Refund + momo_trans_id**: Lưu mã giao dịch MoMo, gọi API Refund khi hủy đơn
- [x] ✅ **Data Protection keys bền vững**: PersistKeysToFileSystem + 90-day lifetime
- [x] ✅ **FixPasswords endpoint**: Ghi đè BCrypt hash trong DB → plain-text
- [x] ✅ **Fix 5 critical bugs**: NotMapped Include (Shipper/Admin), Find() trong LINQ, FK khuyenMai
- [x] ✅ **Unit tests**: ShipFoodCore.Tests (HomeControllerTest, RestaurantControllerTests, RecommendationServiceTests)
- [x] ✅ **Soft delete tbMonAn**: isDeleted + FK RESTRICT, bảo toàn lịch sử hóa đơn
- [x] ✅ **tbLichSuSuDungKhuyenMai**: Lưu vết tần suất dùng mã giảm giá, chống abuse
- [x] ✅ **Redis IDistributedCache thay ConcurrentDictionary**: Connection state persistence cho SignalR, chịu được restart container
- [x] ✅ **Force re-read giá từ DB**: PaymentController không tin frontend, chống sửa giá client
- [x] ✅ **Idempotency Lock Checkout**: Disable nút + spinner, chống double-submit
- [x] ✅ **Optimistic UI**: ToggleConHang + Add to Cart update ngay, rollback nếu API fail
- [x] ✅ **Mobile Leaflet fix**: dragging:false, scrollWheelZoom:'center' trên mobile, giải phóng scroll dọc
- [x] ✅ **Payment error detail**: Hiển thị inner exception message thay vì generic error, trace ID trong log
- [x] ✅ **AutoPreparingService tối ưu**: AsNoTracking + Attach pattern + batch query restaurant names
- [x] ✅ **QR Code bank transfer (Checkout)**: Hiển thị VietQR + bank details + escHtml protection, không auto-redirect
- [x] ✅ **Gemini prompt fix**: System prompt "không nịnh", context-limited to FastShip
- [x] ✅ **SanPham data ảo fix**: Lorem ipsum → mô tả thật, giá hardcode → giá DB, link rỗng → link đúng
- [x] ✅ **Delete category confirm**: JS confirm trước khi xóa danh mục
- [x] ✅ **Khách hàng status filter**: Dropdown lọc trạng thái + data-status numeric
- [x] ✅ **PostgreSQL migration** (Npgsql thay Pomelo/MySQL)
- [x] ✅ **ChiTietSanPham view rebuild** — trang chi tiết sản phẩm hoàn chỉnh (ảnh, giá, size, review)
- [x] ✅ **Scroll-reveal animation** — IntersectionObserver + fs-reveal + stagger + counter
- [x] ✅ **DetailRestaurant view fix** — sửa `item.giatien` → navigation đúng, null-safe `(int)(diemdanhgia ?? 0)`
- [x] ✅ **OrderStatus constants class** — xoá magic strings, centralized transition map + auto-message templates
- [x] ✅ **Atomic transaction ProcessPayment** — wrap order creation + details trong transaction, rollback nếu lỗi giữa chừng
- [x] ✅ **Fix race condition cộng tiền shipper** — transaction + status check trước khi cộng, chống double-credit
- [x] ✅ **Auto-message ClaimOrder** — tự động tạo tbTinNhan khi shipper nhận đơn, customer nhận thông báo real-time
- [x] ✅ **Fix pagination LichSu** — sửa `int page = 1` cứng → đọc từ query parameter
- [x] ✅ **Fix NullReferenceException huydon** — null check `dh.mattdh` trước khi `Find()`
- [x] ✅ **NapTien lưu deposit record** — thêm db.SaveChanges() + tạo giao dịch nạp tiền
- [x] ✅ **Thêm [ValidateAntiForgeryToken]** — 4 POST actions trong RestaurantController
- [x] ✅ **Fix catch rỗng** — thêm ILogger.Warning vào tất cả catch blocks rỗng
- [x] ✅ **Loading state ClaimOrder** — spinner + disabled button chống double-click
- [x] ✅ **Audio notification fallback** — Web Audio API → page title flash
- [x] ✅ **Geolocation cleanup** — beforeunload + clearWatch() xoá theo dõi vị trí
- [x] ✅ **Phân biệt auto-message vs real message** — CSS class `.msg.system` + icon 📢 + bg khác biệt
- [x] ✅ **Null guard RestaurantController** — check `getQuanAn()` null trong OrderList + Wallet
- [x] ✅ **Diagnostic logging ProcessPayment** — log chi tiết orderId, maquan, userId, restaurantIds
- [x] ✅ **Anti-AI-Slop P1.1: DetailRestaurant CSS refactor** — 50+ CSS utility classes, ~40 inline styles → classes (44% reduction: 99 → 55), hardcode colors → `--fs-*` tokens
- [x] ✅ **Anti-AI-Slop P1.2+P1.3: Checkout + OrderTracking** — hardcode `#3CB815`/`#e74c3c`/`#1a1a2e` → `var(--fs-green)`/`var(--fs-danger)`/`var(--fs-dark)`, add utility classes (toast, popup, coupon overlay, voucher)
- [x] ✅ **Anti-AI-Slop P2.2: onmouseenter/leave → CSS hover** — `_LayoutPageShipper.cshtml` + `_LayoutPageRestaurant.cshtml` dùng `.fs-nav-hover:hover` class, xoá inline JS handlers
- [x] ✅ **Anti-AI-Slop P2.3+P3: Accessibility audit** — `aria-label` trên 6 icon-only buttons (VoucherManager edit/delete, Checkout remove-coupon/close-popup, OrderList reset-filter, DeliveryLogs view-detail), `aria-hidden="true"` trên ~40 decorative FA icons (Dashboard KPI/charts, DeliveryLogs stats/status)
- [x] ✅ **Design tokens bổ sung** — thêm `--fs-danger: #e74c3c`, `--fs-warning: #f39c12` + 4 utility classes (`.fs-text-danger`, `.fs-text-warning`, `.fs-text-dark`, `.fs-text-white`)
- [x] ✅ **Fix: Cart lost items after placing order** — HttpOnly cookie detection bug (JavaScript không đọc được `.AspNetCore.Cookies`) → server-side `isLoggedIn` flag, xoá `justOrdered` lock block trong `RestoreFromLocal`
- [x] ✅ **Fix: Analytics null guard** — null check KPI cards trong Admin Dashboard, từ chối null model
- [x] ✅ **Fix: OrderList date filter (fromDate/toDate)** — thêm 2 input type="date" + server-side `.Where(d => d.NgayDat >= fromDate && d.NgayDat <= toDate)`
- [x] ✅ **Fix: Out-of-stock badge + disabled button** — `soluong == 0` → badge "Cháy hàng" + disabled nút "Thêm vào giỏ" + CSS `.fs-btn-disabled`
- [ ] **⚠️ Filter/Search quá phức tạp** — MenuSearch dynamic SQL + Bottom Sheet + Dual Filter Bar cần đơn giản hoá
- [ ] Real payment (Stripe/PayPal/ZaloPay)

---

## 👥 Vai Trò Người Dùng

| Vai Trò | Quyền hạn chính | Trang chính |
|---------|----------------|-------------|
| **Admin** | Quản lý toàn bộ hệ thống, user, danh mục, thống kê | `/Admin/*` |
| **Restaurant** | Quản lý thực đơn, đơn hàng, đánh giá, khuyến mãi | `/Restaurant/*` |
| **Shipper** | Nhận đơn giao, cập nhật trạng thái, xem thu nhập | `/Shipper/*` |
| **Customer** | Đặt món, thanh toán, đánh giá, lịch sử, chat | `/Home/*`, `/Cart/*` |

---

## 📦 Dependencies Chính

| Package | Version | Mục đích |
|---------|---------|----------|
| `Microsoft.AspNetCore.App` | 8.0 | Core framework (built-in) |
| `Pomelo.EntityFrameworkCore.MySql` | 8.0.2 | MySQL EF Core provider |
| `Microsoft.EntityFrameworkCore` | 8.0.11 | EF Core ORM |
| `Microsoft.AspNetCore.SignalR.Common` | 8.0.11 | Real-time messaging |
| `Microsoft.AspNetCore.Authentication.Google` | 8.0.0 | Google OAuth |
| `Google.GenAI` | 1.11.0 | Gemini AI API client |
| `Newtonsoft.Json` | 13.0.3 | JSON serialization |
| `Microsoft.AspNetCore.DataProtection.Extensions` | 8.0.11 | Persistent encryption keys |
| `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | 8.0.11 | Hot-reload Razor views |
| jQuery | 3.7.1 | Client-side scripting |
| Bootstrap | 5.x | UI framework |
| Chart.js | 3.x | Dashboard charts |
| Font Awesome | 5.10.0 | Icons |
| SignalR JS Client | 8.0.0 | Client-side SignalR |

---

## 🚀 Hướng Dẫn Chạy

### 1. Clone dự án
```bash
git clone <repo-url>
cd TMDT-master
```

### 2. Cài đặt packages
```bash
cd ShipFoodCore
dotnet restore
```

### 3. Cấu hình MySQL
- Cài đặt MySQL 8+ hoặc MariaDB 10.6+
- Tạo database: `CREATE DATABASE dbFoody CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`
- Import seed data: `mysql -u root -p dbFoody < mysql_utf8.sql`

### 4. Cấu hình Connection String
Trong `appsettings.json` hoặc environment variables:
```json
{
  "ConnectionStrings": {
    "dbFoodyEntities": "Server=localhost;Port=3306;Database=dbFoody;User=root;Password=yourpassword;"
  }
}
```

### 5. Chạy ứng dụng
```bash
dotnet run --project ShipFoodCore
```

### 6. Truy cập
- Web: `http://localhost:5000` (hoặc cổng được cấu hình)
- Healthcheck: `http://localhost:5000/health`

### Docker (optional)
```bash
docker build -t fastship .
docker run -p 8080:8080 -e MYSQLHOST=host.docker.internal -e MYSQLUSER=root -e MYSQLPASSWORD=pass -e MYSQLDATABASE=dbFoody fastship
```

---

## 📞 Liên Hệ & Hỗ Trợ

- **Email**: fastship@contact.com
- **Điện thoại**: 1900 1234
- **Địa chỉ**: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
- **Website**: [https://fastship-web.onrender.com](https://fastship-web.onrender.com)

---

## 📜 Giấy Phép

Dự án mã nguồn mở — phát triển bởi đội ngũ ShipFood.

---

> **Phiên bản**: 5.7 — Anti-AI-Slop UI refactor (3 views → design tokens, 250 inline styles ↓50%), accessibility audit (~40 icons + 6 buttons), Cart HttpOnly bug fix, OrderList date filter, out-of-stock badge  
> **Ngôn ngữ**: C# 12, HTML5, CSS3, JavaScript ES6  
> **Kiến trúc**: ASP.NET Core MVC n-tier  
> **Database**: PostgreSQL 15+ (Npgsql)  
> **Password**: Plain-text (không hash)  
> **Deploy**: Docker + Render  
> **Cập nhật**: Tháng 7, 2026
