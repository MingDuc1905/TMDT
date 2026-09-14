# 📋 MASTER TEST PLAN — FastShip (ShipFood)

> **Phiên bản**: 2.0
> **Ngày**: 02/08/2026
> **Môi trường**: Local (deep) + Render (smoke) — Desktop 1920×1080
> **Framework**: Playwright 1.61.1 + Lightpanda (khi Docker khả dụng)
> **Phạm vi**: 56 trang / 4 roles / ~430 test cases
> **Loại trừ**: Security, Mobile/Responsive

---

## 🎯 Mục Tiêu

Bóc tách chi tiết từng trang (element-level inventory), test toàn bộ:
- ✅ Chức năng (mọi nút bấm, form, input, bảng, modal)
- ✅ Luồng flow (4 roles, cross-role lifecycle)
- ✅ Hình ảnh (0 broken image, alt, fallback)
- ✅ Nội dung (dữ liệu thật, format VND, không placeholder)
- ✅ API JSON endpoints (dashboard stats, reviews, coupons, chat)
- ✅ Real-time (SignalR, order tracking, chat)
- ❌ Bảo mật (SQLi/XSS — ngoài phạm vi)
- ❌ Mobile/Responsive (ngoài phạm vi)

---

## 🏗️ Kiến Trúc 2 Tầng

| Tầng | Môi trường | Mục đích | Runner |
|------|-----------|----------|--------|
| **Tầng 1 — Deep** | Localhost (`dotnet run` + Postgres local) | Test toàn bộ flow, CRUD, checkout, order lifecycle | Playwright Chromium |
| **Tầng 2 — Smoke** | Render (`fastship-web.onrender.com`) | Critical paths, 4 dashboards | Chromium + Lightpanda |

> ⚠️ Trên máy hiện tại: local Postgres/Docker không khả dụng → chạy toàn bộ trên Render production.
> 1 file test chạy được cả 2 tầng — chỉ khác `BASE_URL` env.

---

## 🛍️ PHẦN 1: KHÁCH HÀNG (Customer) — 22 trang — ~170 TC

| # | Trang | TC | Trọng tâm |
|---|-------|----|-----------|
| 1.1 | `/` Homepage | 14 | Hero carousel, filter chips, search, stats, restaurant grid, AI combos, footer, 0 broken img |
| 1.2 | `/Home/Login` | 8 | Form, 3 loại error, redirect, Google OAuth, Forgot/Signup link |
| 1.3 | `/Home/Signup` | 6 | Form 8 fields, validation, tạo tài khoản |
| 1.4 | `/Home/Forgot` | 3 | Form email, submit |
| 1.5 | `/Home/SelectRoleGoogle` | 3 | 3 role cards |
| 1.6 | `/Home/DetailRestaurant` | 12 | Header, sidebar, menu grid, discount badge, out-of-stock, review, "Đã mua" filter |
| 1.7 | `/Home/ChiTietSanPham` | 8 | Hero 2-col, size chips M/L/XL, add-to-cart, similar items, review paginate |
| 1.8 | `/Home/DanhMuc` | 4 | Grid category cards |
| 1.9 | `/Home/SanPham` | 6 | Grid món, filter, search |
| 1.10 | `/Home/About` | 3 | Nội dung, ảnh, heading |
| 1.11 | `/Home/Contact` | 3 | Form liên hệ, thông tin |
| 1.12 | `/Home/NhanTin` | 5 | Chat box, SignalR, gửi/nhận |
| 1.13 | `/Home/Profile` | 6 | Form profile, avatar, đổi mật khẩu |
| 1.14 | `/Home/Wallet` | 5 | Số dư, nạp tiền, lịch sử |
| 1.15 | `/Cart` | 14 | Empty state, quantity, delete, tổng VND, session persist |
| 1.16 | `/Cart/Checkout` | 16 | 3 tabs địa chỉ, COD/Chuyển khoản/MoMo, QR VietQR, coupon, confirm, idempotency |
| 1.17 | `/Cart/LichSuDatHang` | 8 | DataTable, status badges, chi tiết, tracking |
| 1.18 | `/Cart/ChiTietDonHang` | 8 | Invoice, map Leaflet, progress 7 bước, SignalR |
| 1.19 | `/Cart/OrderTracking` | 6 | 7-step progress, Leaflet, ETA, SignalR |
| 1.20 | `/Cart/SuccessView` | 3 | Thành công, mã đơn |
| 1.21 | `/Cart/FailureView` | 3 | Thất bại, retry |
| 1.22 | `/Cart/EInvoice` | 4 | Hóa đơn điện tử |

## 🏪 PHẦN 2: QUÁN ĂN (Restaurant) — 12 trang — ~95 TC

| # | Trang | TC | Trọng tâm |
|---|-------|----|-----------|
| 2.1 | `/Restaurant` Dashboard | 8 | Sidebar 8 links, 4 KPI, Chart.js, apriori insights |
| 2.2 | `/Restaurant/OrderList` | 10 | DataTable, status badges, Nhận/Hủy/Chuẩn bị xong |
| 2.3 | `/Restaurant/ProductList` | 8 | Table món, toggle conhang, edit/delete |
| 2.4 | `/Restaurant/ProductDetail` | 8 | Form, size M/L/XL, upload ảnh, validation |
| 2.5 | `/Restaurant/Analytics` | 5 | Revenue chart, top items, date filter |
| 2.6 | `/Restaurant/Discount` | 6 | Bảng KM, thêm, gắn cho món |
| 2.7 | `/Restaurant/Review` | 6 | Điểm TB, bar sao, filter, reply |
| 2.8 | `/Restaurant/Profile` | 6 | Form, avatar, toggle mở/đóng |
| 2.9 | `/Restaurant/GeneralCustomer` | 4 | Bảng khách hàng |
| 2.10 | `/Restaurant/Wallet` | 6 | Số dư, rút/nạp tiền |
| 2.11 | `/Restaurant/Scanner` | 4 | QR reader, camera controls |
| 2.12 | `/Restaurant/NhanTin` | 5 | Chat role |

## 🚚 PHẦN 3: SHIPPER — 9 trang — ~70 TC

| # | Trang | TC | Trọng tâm |
|---|-------|----|-----------|
| 3.1 | `/Shipper` Dashboard | 10 | 2 tabs FREE-PICK/ĐƠN HÀNG, Leaflet map, claim order |
| 3.2 | `/Shipper/OrderDetail` | 8 | btnPickup/Complete, QR card, map |
| 3.3 | `/Shipper/LichSu` | 5 | Bảng lịch sử, badges |
| 3.4 | `/Shipper/ThuNhap` | 6 | 4 stats, Chart.js, bảng 30 ngày |
| 3.5 | `/Shipper/ViTien` | 5 | Số dư VND, lịch sử |
| 3.6 | `/Shipper/QRDelivery` | 5 | QR list, tab filter, download |
| 3.7 | `/Shipper/CaiDat` | 4 | Profile, đổi mật khẩu |
| 3.8 | `/Shipper/ThongBao` | 4 | List thông báo, unread badge |
| 3.9 | `/Shipper/NhanTin` | 5 | Chat shipper-customer |

## 👑 PHẦN 4: ADMIN — 13 trang — ~95 TC

| # | Trang | TC | Trọng tâm |
|---|-------|----|-----------|
| 4.1 | `/Admin/Dashboard` | 10 | 4 KPI, 3 charts, date filter, CSV export |
| 4.2 | `/Admin/QuanLyQuanAn` | 8 | Bảng quán, duyệt/khóa |
| 4.3 | `/Admin/QuanLyShipper` | 7 | Bảng shipper, duyệt/khóa |
| 4.4 | `/Admin/QuanLyKhachHang` | 7 | Bảng customer, status dropdown |
| 4.5 | `/Admin/QuanLyQuanTriVien` | 6 | Bảng admin, không khóa admin cuối |
| 4.6 | `/Admin/Order` | 8 | DataTable, SignalR, dropdown action |
| 4.7 | `/Admin/OrderDetail` | 5 | Chi tiết đơn |
| 4.8 | `/Admin/EditOrder` | 5 | Form edit |
| 4.9 | `/Admin/Category` | 8 | CRUD, upload icon |
| 4.10 | `/Admin/VoucherManager` | 6 | CRUD voucher |
| 4.11 | `/Admin/WalletManager` | 5 | Ví, cộng/trừ tiền |
| 4.12 | `/AdminChat` | 6 | Conversations, gửi tin, unread badge |
| 4.13 | `/EDelivery/ScanResult` | 4 | QR scan result |

## 🧩 PHẦN 5: SHARED & AUTH — 6 trang — ~30 TC

| Trang | TC | Verify |
|-------|----|--------|
| `_LayoutPageHome` | 6 | Topbar, navbar sticky, footer, chat widget |
| `_LayoutPageAdmin/Restaurant/Shipper` | 6 | Sidebar active, header, logout |
| `_LayoutAuth` | 4 | Glassmorphism, centered form |
| `_ChatWidget` | 4 | Mở/đóng, AI tab, support tab |
| `Error.cshtml` | 3 | 404/500 |
| Accessibility basics | 7 | aria-label, alt, contrast, form labels |

---

## 🔄 3 LUỒNG E2E XUYÊN SUỐT (Smoke)

1. **Customer full**: Đăng nhập → tìm quán → thêm món → checkout COD → lịch sử → chi tiết → tracking → đánh giá
2. **Restaurant+Shipper**: Đặt đơn → quán nhận → shipper FREE-PICK → lấy hàng → giao → hoàn thành
3. **Admin**: Login → dashboard → duyệt user → quản lý đơn → category CRUD → chat hỗ trợ

---

## 📁 Cấu Trúc Files

```
docs/MASTER-TEST-PLAN.md           ← File này
docs/inventory/
├── 01-customer-pages.md           ← Bóc tách 22 trang khách hàng
├── 02-restaurant-pages.md         ← Bóc tách 12 trang quán
├── 03-shipper-pages.md            ← Bóc tách 9 trang shipper
├── 04-admin-pages.md              ← Bóc tách 13 trang admin
└── 05-public-shared.md            ← Auth, layouts, widgets

e2e-tests/tests/
├── 41-element-inventory/          ← 11 spec files element-level (BỘ TEST MỚI)
│   ├── 41-home.spec.ts
│   ├── 42-auth.spec.ts
│   ├── 43-restaurant-detail.spec.ts
│   ├── 44-cart-checkout.spec.ts
│   ├── 45-order-flow.spec.ts
│   ├── 46-restaurant-dashboard.spec.ts
│   ├── 47-shipper.spec.ts
│   ├── 48-admin.spec.ts
│   ├── 49-public-pages.spec.ts
│   ├── 50-shared-layouts.spec.ts
│   └── 51-smoke-e2e.spec.ts
```

---

## ⚙️ Lệnh Chạy

```bash
cd e2e-tests

# Toàn bộ suite mới (element inventory)
npx playwright test tests/41-element-inventory --project='Desktop Chromium'

# Theo role
npx playwright test tests/41-element-inventory/41-home.spec.ts
npx playwright test tests/41-element-inventory/48-admin.spec.ts

# Local deep (khi có local DB)
set BASE_URL=http://localhost:5000 && npx playwright test tests/41-element-inventory

# Lightpanda (khi có Docker)
npm run lightpanda:up && npx playwright test --config=lightpanda.config.ts tests/41-element-inventory
```

---

## 🚨 Lưu Ý Quan Trọng

1. **Render free tier**: 23-25s cold start → timeout 60s
2. **Rate limit**: 5 POST/5ph login → workers=1
3. **SignalR**: không dùng `networkidle` (WebSocket giữ kết nối)
4. **Unsplash**: Render IP bị rate limit → ảnh external có thể 403 (soft fail)
5. **Session**: HttpOnly cookie → server-side `isLoggedIn` flag
