# Fastship (ShipFood) — Nền tảng Giao Hàng Thức Ăn Online

> **Cập nhật**: Dựa trên mã nguồn thực tế — ASP.NET Core 8 MVC + MySQL (Pomelo) + Bootstrap 5 + SignalR + Gemini AI + Google OAuth

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
| **ORM** | Entity Framework Core (Pomelo) | 8.0.11 / 8.0.2 |
| **Database** | MySQL 8+ (MariaDB-compatible) | 8.0.20+ |
| **Template Engine** | Razor (Runtime Compilation) | 8.0.11 |
| **Real-time** | SignalR (Groups-based) | 8.0.11 |
| **AI Chatbot** | Google Gemini API | gemini-3.5-flash |
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
| **WOW.js + Animate.css** | Scroll animations |
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
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.11" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
```

---

## 📁 Cấu Trúc Dự Án

```
TMDT-master/
├── ShipFoodCore/                    # Main web application
│   ├── Controllers/                 # MVC Controllers (8 files)
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
│   ├── Models/                      # Entity Framework models (15+ files)
│   │   ├── DbContext.cs             # dbFoodyEntities context + Fluent API
│   │   ├── Cart.cs                  # Cart logic (session-based)
│   │   ├── tbUser.cs                # Users (4 roles)
│   │   ├── tbQuanAn.cs              # Restaurants
│   │   ├── tbMonAn.cs               # Menu items
│   │   ├── tbDonHang.cs             # Orders
│   │   ├── tbChiTietDonHang.cs      # Order details
│   │   ├── tbDanhGia.cs             # Reviews
│   │   ├── tbDanhMuc.cs             # Categories
│   │   ├── tbKhuyenMai.cs           # Discounts/coupons
│   │   ├── tbMonAnKhuyenMai.cs      # Product-discount mapping
│   │   ├── tbShipper.cs             # Shippers
│   │   ├── tbAdmin.cs               # Admin
│   │   ├── tbKhachHang.cs           # Customers
│   │   ├── tbThongTinDatHang.cs     # Delivery addresses
│   │   ├── tbTinNhan.cs             # Chat messages
│   │   ├── tbLoaiHinhThanhToan.cs   # Payment methods
│   │   ├── DataAnalytic.cs          # Analytics view models
│   │   ├── LichSuDonHang.cs         # Order history view models
│   │   └── DonHangDangLam.cs        # Raw SQL order queries
│   │
│   ├── Hubs/                        # SignalR hubs
│   │   └── Chats.cs                 # /nhantin hub (6 methods, 2 groups)
│   │
│   ├── Services/                    # Business logic services
│   │   ├── RecommendationService.cs  # ML-based recommendations (4 algorithms)
│   │   ├── GeminiService.cs         # Gemini AI API integration
│   │   └── AutoPreparingService.cs  # BackgroundService: 5s preparing → SignalR
│   │
│   ├── Utils/                       # Helper utilities
│   │   └── TinhToan.cs             # Shipping fee calculation
│   │
│   ├── Views/                       # Razor views (25+ views, 5 layouts)
│   │   ├── Home/                   # Customer-facing pages (11 views)
│   │   ├── Cart/                   # Cart, checkout, history (6 views)
│   │   ├── Restaurant/             # Restaurant dashboard (10 views)
│   │   ├── Shipper/                # Shipper dashboard (8 views)
│   │   ├── Admin/                  # Admin panel (11 views)
│   │   ├── AdminChat/              # Admin support chat (1 view)
│   │   └── Shared/                 # Layouts + chat widget (6 files)
│   │
│   ├── wwwroot/                     # Static assets
│   │   ├── Source/Home/            # Customer theme (CSS, JS, libs)
│   │   ├── Source/Cart/            # Ogani cart theme
│   │   ├── Source/Restaurant/      # Restaurant dashboard theme
│   │   ├── Source/Shipper/         # Shipper dashboard theme
│   │   ├── Source/Admin/           # Admin dashboard theme
│   │   └── Content/                # Bootstrap 3 (legacy)
│   │
│   ├── Program.cs                   # App startup (DI, middleware, config)
│   └── appsettings.json             # Configuration (connection strings, API keys)
│
├── UI-UX.md                         # Comprehensive UI/UX documentation (17 sections)
├── Architectural-Solution.md        # Architectural solution document (15 giải pháp cải thiện)
├── Project.md                       # This file
├── database_full.sql                # Full MySQL database dump
├── seed_mysql.sql                   # Initial seed data (categories, users, menus)
├── inserts.txt                      # Additional insert scripts
├── mysql_utf8.sql                   # UTF-8 configuration script
├── Dockerfile                       # Multi-stage Docker build (SDK + runtime)
├── railway.json                     # Railway deployment config
├── .agents/skills/                  # Codebuff skill rules
│   └── fastship-rules.md           # FastShip development rules
```

---

## 🔑 Tính Năng Chính

### 1. 🛍️ Khách Hàng (Customer)

| Tính năng | Chi tiết |
|-----------|----------|
| **Tìm kiếm quán ăn** | Server-side, không phân biệt dấu Unicode, tìm theo tên/user/món + lọc danh mục |
| **Thực đơn chi tiết** | Danh mục sidebar + tìm món + filter "Đã mua" + khuyến mãi badge |
| **Giỏ hàng** | Session-based (JSON), AJAX quantity, coupon, 3 address modes |
| **Thanh toán** | COD (immediate) + Chuyển khoản (mock: Vietcombank + test buttons) |
| **Đánh giá** | AJAX star picker + textarea + "Xem thêm" paginate 6/lần |
| **Lịch sử đơn** | DataTable với sort/search + trạng thái badge màu + emoji |
| **Gợi ý** | 4 thuật toán: collaborative filtering, market basket, time-based, trending |
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

**Google OAuth Auto-Create**: Khi người dùng đăng nhập Google lần đầu (email chưa tồn tại trong DB), hệ thống tự động:
1. Tạo password ngẫu nhiên `GG_{Guid}` → hash BCrypt workFactor 12
2. Tạo `tbUser` với username `gg_{guid12}`, email truncate 50 ký tự, role "Khách hàng", trạng thái active (trangthai=1)
3. Đồng bộ `tbKhachHang` với tên từ Google profile (cắt 50 ký tự)
4. Gán session + redirect thẳng vào trang chủ (không cần duyệt thủ công)

**Tables** (16 bảng):
| Bảng | Mô tả | Quan hệ |
|------|-------|---------|
| `tbUser` | Người dùng (4 roles) | 1:1 → tbKhachHang, tbQuanAn, tbShipper, tbAdmin |
| `tbKhachHang` | Khách hàng | 1:N → tbThongTinDatHang, tbTinNhan |
| `tbQuanAn` | Quán ăn | 1:N → tbMonAn, tbDonHang |
| `tbMonAn` | Món ăn | N:1 → tbDanhMuc; 1:N → tbChiTietDonHang, tbMonAnKhuyenMai |
| `tbDanhMuc` | Danh mục món | 1:N → tbMonAn |
| `tbDonHang` | Đơn hàng | N:1 → tbQuanAn, tbKhuyenMai, tbLoaiHinhThanhToan, tbShipper |
| `tbChiTietDonHang` | Chi tiết đơn | N:1 → tbMonAn, 1:N → tbDanhGia |
| `tbDanhGia` | Đánh giá | N:1 → tbChiTietDonHang |
| `tbKhuyenMai` | Khuyến mãi | 1:N → tbMonAnKhuyenMai, tbDonHang |
| `tbMonAnKhuyenMai` | KM của món | N:1 → tbMonAn, tbKhuyenMai |
| `tbLoaiHinhThanhToan` | Hình thức TT | 1:N → tbDonHang |
| `tbThongTinDatHang` | Địa chỉ giao | 1:N → tbDonHang |
| `tbShipper` | Shipper | 1:N → tbDonHang, tbTinNhan |
| `tbAdmin` | Quản trị viên | 1:1 → tbUser |
| `tbTinNhan` | Tin nhắn chat | N:1 → tbDonHang |
| `City/District` | Địa danh | — |

**Seed data**: `seed_mysql.sql` — categories, users, restaurants, menu items (tự động seed khi DB được tạo lần đầu)

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

### BaseController shared methods
- `CheckLogin()` — kiểm tra session "user"
- `GetCurrentUser()` → `tbUser?` — deserialize từ session JSON
- `SetSessionUser(tbUser)` — serialize user vào session
- `GetCart()` → `Cart?` — lấy giỏ hàng từ session
- `SetCart(Cart)` — lưu giỏ hàng vào session

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
| `POST` | `/Payment/ProcessPayment` | Payment | Mock payment processing |
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
| `POST` | `/Restaurant/ToggleConHang` | Restaurant | AJAX toggle 1-click hết hàng |
| `GET` | `/Restaurant/hoantatdon/{id}` | Restaurant | Chuẩn bị xong → status 'Chờ shipper lấy hàng' + SignalR broadcast to shippers |
| `GET` | `/health` | — | Healthcheck (no DB needed, always 200 OK) |
| `POST` | `/Home/GoogleResponse` | Home | **NEW** Google OAuth callback (auto-create + redirect) |

### SignalR Hub
- **Endpoint**: `/nhantin` 
- **Hub**: `Chats.cs` (12 methods, 5 group types)
  - `Message` — Broadcast to all
  - `AdminSendMessage` / `CustomerSendMessage` — Chat via Groups (không dùng ConnectionId)
  - `JoinOrderGroup(orderId)` — Join per-order group `order_{orderId}`
  - `JoinCustomerSupportGroup(userId)` — Join per-user group `customer_{userId}`
  - `JoinRestaurantGroup(restaurantId)` — Join restaurant group `restaurant_{restaurantId}` (newOrder events)
  - `JoinShipperGroup()` — Join shippers broadcast group `shippers` (newPickupOrder events)
  - `SendToOrderGroup(msg, orderId, senderName, role)` — Send within order group
  - `NotifyNewMessage(userId, count)` — Real-time unread badge
  - `NotifyShippersNewPickup(orderId, restaurantName, pickupAddress)` — Broadcast to shippers group
  - **`UpdateLocation(orderId, lat, lng)`** — Shipper coordinate streaming → `order_{orderId}` group
  - `OnConnectedAsync` / `OnDisconnectedAsync` — Connection tracking với `ConcurrentDictionary`
  - `IsUserOnline(userId)` / `GetUserConnectionId(userId)` — Static helper methods

---

## 🚀 Deployment

### Docker
- **Multi-stage build**: SDK 8.0 → runtime ASP.NET 8.0
- **Port**: 8080 (ENV `ASPNETCORE_URLS=http://+:8080`)
- **Healthcheck**: `/health` endpoint (200 OK)
- **Entrypoint**: `dotnet ShipFoodCore.dll`

### Railway
- **Builder**: Dockerfile (automatic)
- **Replicas**: 1
- **Restart**: ON_FAILURE, max 3 retries
- **MySQL**: Auto-config từ Railway env vars (MYSQLHOST, MYSQLPORT, MYSQLUSER, MYSQLPASSWORD, MYSQLDATABASE)

### Database Initialization
- `EnsureCreated()` tự động tạo bảng khi chạy lần đầu
- `seed_mysql.sql` tự động seed data nếu DB trống
- App vẫn start kể cả khi MySQL chưa sẵn sàng (try-catch)

### Environment Variables
```env
# MySQL (Railway auto)
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

# Gemini AI (optional)
Gemini__ApiKey=xxx

# Serilog Seq (optional)
SEQ_URL=http://localhost:5341
SEQ_API_KEY=xxx

# CORS Allowed Origins
ALLOWED_ORIGINS=https://shipfood.up.railway.app

# App Domain (for CORS fallback)
APP_DOMAIN=https://shipfood.up.railway.app
```

---

## 📊 Recommendation System

| Loại | Algorithm | Implementation |
|------|-----------|----------------|
| **Personalized** | Collaborative filtering | Tìm user cùng sở thích → gợi ý món chưa đặt |
| **Frequently Bought Together** | Apriori Support + Confidence (min 2%, min 50%) | Support = count(A∩B)/D, Confidence = count(A∩B)/count(A). 3-level fallback. Hỗ trợ đa phần tử (A,B,C→D). |
| **Time-based** | Keyword matching | Theo giờ: sáng (phở/bún) / trưa (cơm) / tối (lẩu/nướng) / khuya (trà sữa) |
| **Trending** | Sales volume (48h) | Top bán chạy 48h, fallback all-time |

---

## 🤖 AI Chatbot (Gemini)

### API
- **Model**: `gemini-3.5-flash` (free tier) — comment in code: gemini-2.0-flash retired as of 1/6/2026
- **System prompt**: Tiếng Việt, ngắn gọn, thân thiện
- **Context**: Phí ship 15,000đ (free ≥100,000đ), giao 30-45 phút, 7:00-21:30

### Database Queries
- `#123` hoặc `mã 123` → Tra cứu đơn hàng (trạng thái + emoji)
- "gợi ý", "nên ăn", "bán chạy" → Top 5 món bán chạy

### Features
- Contextual quick replies (dựa trên từ khóa)
- Conversation history (20 messages trong session)
- Fallback: hướng dẫn dùng lệnh khi Gemini không khả dụng

---

## 📝 Ghi Chú Phát Triển

- **Framework**: ASP.NET Core 8 (not MVC 5)
- **Database**: MySQL 8+ with Pomelo (MySqlServerVersion 8.0.20, not MariaDb)
- **ORM**: Entity Framework Core 8 (not EF6)
- **Frontend**: Bootstrap 5 (not Bootstrap 3/4)
- **Auth**: Cookie + Session (not Identity Framework)
- **Password**: BCrypt.Net-Next (hash + verify, workFactor 12)
- **Payment**: Mock (Vietcombank test mode — cần tích hợp thật)
- **Font**: Inter (unified) — removed Open Sans, Lora, Cairo, Poppins, Montserrat, Nunito
- **Charts**: Chart.js (not any commercial charting library)
- **Real-time**: SignalR 8 (not WebSocket raw)
- **AI**: Google Gemini gemini-3.5-flash (free tier, not any paid AI service)
- **Deploy**: Docker + Railway (not IIS)

### CI/CD
- Docker multi-stage build (SDK → Runtime)
- Railway auto-deploy with healthcheck
- Environment variables for all secrets

### Tech Debt / Cần cải thiện
- [x] ✅ Password hashing (BCrypt.Net-Next)
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
- [x] ✅ Auto-preparing 5s simulation (BackgroundService)
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
- [x] ✅ **fastship-design-tokens.css đồng bộ** (--fs-topbar-h: 38→34px sync với layout-sg.css)
- [x] ✅ **Fix 1 — Footer social icons alignment** (`display:inline-flex; width:36px; height:34px; margin:0 6px` trong `layout-sg.css`)
- [x] ✅ **Fix 1 — Chat toggle close không được** (`toggleChat()` bulletproofed, click-outside-to-close, `scale(0.9) translateY(20px)` animation)
- [x] ✅ **Fix 2 — Cart multi-restaurant** (`ApiThemMonAn` + `ApiForceSwitchRestaurant` endpoints, confirm dialog khi thêm món khác quán)
- [x] ✅ **Fix 3 — Geo throttling 5s** (`sendLocationThrottled()`, `_throttleInterval=5000` trong Shipper/OrderDetail)
- [x] ✅ **Fix 3 — OnDisconnectedAsync cleanup** (`Chats.cs` broadcast `shipperOffline` khi shipper mất kết nối)
- [x] ✅ **Fix 4 — Race condition Shipper** (kiểm tra `mashipper != null` + `trangthai` trước khi assign, TempData error nếu đơn đã có shipper khác)
- [x] ✅ **AdBlock Bypass Icon SVG** — `fs-icon-anchor-f/i` class trung lập, SVG data URI, inline-flex 28×28px, no-repeat center/contain
- [x] ✅ **Hero Carousel Horizontal Smooth Slide** — Thay Ken Burns zoom + crossfade bằng `transform translateX` horizontal slide, caption fade+slide phải→trái, buttons delay 150ms
- [x] ✅ **Coupon Selection Popup** — `GetAvailableCoupons` endpoint, popup modal danh sách coupon cards, click auto-apply + trigger CheckCoupon
- [x] ✅ **CSS AdBlock Refinements** — Xoá `:contains()` pseudo-selector không hợp lệ, chuẩn hoá `background-image` shorthand với `no-repeat center/contain`
- [x] ✅ **Dark Mode CSS** — `@media (prefers-color-scheme: dark)` trong `fastship-design-tokens.css` với 60+ dòng override cho header, footer, navbar, cards, forms, skeleton, chat, social links
- [x] ✅ **Auth pages CSS variables** — Login/Signup/Forgot: `#888`→`var(--fs-muted)`, `#3CB815`→`var(--fs-green)`, `#fff`→`var(--fs-white)`, thêm link design tokens
- [x] ✅ **Cart/Index ~30 hardcoded colors → CSS variables** — `#888`→`var(--fs-muted)`, `#e74c3c`→`var(--fs-danger)`, `#f0f0f0`→`var(--fs-border-soft)`, `#333`→`var(--fs-body)`, `#1a1a2e`→`var(--fs-dark)` + JS strings
- [x] ✅ **DetailRestaurant ~25 hardcoded colors → CSS variables** — CSS style block, inline styles, JS review builder colors, star picker colors
- [x] ✅ **UI/UX Pro Max editorial typography** — `--fs-letter-spacing`, `--fs-heading-letter-spacing`, premium shadows `--fs-shadow-md/xl`, section spacing tokens
- [x] ✅ **Footer social icons 30px circular** — Đồng bộ footer với topbar: `width/height: 30px`, hover nền xanh lá
- [x] ✅ **Card spring animation** — `cubic-bezier(0.34, 1.56, 0.64, 1)` overshoot, `translateY(-8px)`, shadow đậm hơn
- [x] ✅ **How-it-works hover effects** — Icon scale 1.1 + background hover, card lift, editorial letter-spacing
- [x] ✅ **Stats row hover** — Lift effect, editorial letter-spacing, refined padding
- [ ] PayPal/ZaloPay/MoMo integration (đã remove khỏi UI)
- [ ] Unit tests (chưa có — đã thêm xUnit project + 12 tests GetReviews)
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
- Import seed data: `mysql -u root -p dbFoody < seed_mysql.sql`

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
- **Website**: [https://fastship.railway.app](https://fastship.railway.app)

---

## 📜 Giấy Phép

Dự án mã nguồn mở — phát triển bởi đội ngũ ShipFood.

---

> **Phiên bản**: 3.6 / v4.12 — Dark Mode CSS, CSS variable migration (55+ colors), UI/UX Pro Max editorial typography & animations  
> **Ngôn ngữ**: C# 12, HTML5, CSS3, JavaScript ES6  
> **Kiến trúc**: ASP.NET Core MVC n-tier  
> **Database**: MySQL 8+ (MySqlServerVersion 8.0.20)  
> **Deploy**: Docker + Railway  
> **Cập nhật cuối**: Tháng 7, 2026
