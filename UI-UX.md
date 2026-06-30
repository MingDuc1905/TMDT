# Fastship (ShipFood) — UI/UX Documentation (Full)

> **Phiên bản**: 3.1 — Refactored Security + SignalR + Skeleton Loading + Font Standardization + Live Tracking + Responsive Dashboards  
> **Cập nhật**: Tháng 6, 2026  
> **Mô tả**: Tài liệu thiết kế giao diện & trải nghiệm người dùng toàn diện cho nền tảng đặt đồ ăn Fastship

---

## Mục Lục

1. [Tổng Quan Design System](#1-tổng-quan-design-system)
2. [Bảng Màu & Typography](#2-bảng-màu--typography)
3. [CSS Variables & Utility Classes](#3-css-variables--utility-classes)
4. [Layouts & Navigation](#4-layouts--navigation)
5. [Home Page (Khách Hàng)](#5-home-page-khách-hàng)
6. [Trang Chi Tiết Quán Ăn](#6-trang-chi-tiết-quán-ăn)
7. [Giỏ Hàng (Cart)](#7-giỏ-hàng-cart)
8. [Thanh Toán (Checkout)](#8-thanh-toán-checkout)
9. [Đăng Nhập / Đăng Ký](#9-đăng-nhập--đăng-ký)
10. [Dashboard Admin](#10-dashboard-admin)
11. [Dashboard Restaurant](#11-dashboard-restaurant)
12. [Dashboard Shipper](#12-dashboard-shipper)
13. [Chat Widget (AI + Support)](#13-chat-widget-ai--support)
14. [Live Order Tracking (Leaflet.js + SignalR)](#14-live-order-tracking-leafletjs--signalr)
15. [Reusable Components](#15-reusable-components)
16. [Micro-interactions & Animations](#16-micro-interactions--animations)
17. [Responsive Design](#17-responsive-design)
18. [Accessibility (WCAG)](#18-accessibility-wcag)
19. [Icons & Iconography](#19-icons--iconography)
20. [Error Handling & Empty States](#20-error-handling--empty-states)
21. [User Flows](#21-user-flows)
22. [Backlog & Improvements](#22-backlog--improvements)

---

## 1. Tổng Quan Design System

### 1.1 Design Tokens

Fastship sử dụng **design tokens** thông qua CSS custom properties (`:root` variables). Mỗi theme (Home, Cart, Restaurant, Shipper, Admin) có bộ variables riêng nhưng chia sẻ chung **primary green (#3CB815)** và **secondary orange (#F65005)**.

### 1.2 Theme Architecture

| Theme | CSS File(s) | Target Audience | Style |
|-------|-------------|-----------------|-------|
| **Home (Customer)** | `style.css`, `layout-sg.css`, `login.css`, `details.css`, `base.css` | Khách hàng | Sweetgreen-inspired, modern, card-based |
| **Cart/Checkout** | `style.css` (Ogani) + inline styles | Khách hàng | E-commerce, clean, data-dense |
| **Restaurant Dashboard** | `style-restaurant.css` (Bootstrap 4.3) | Chủ quán | Admin-style sidebar, data tables |
| **Shipper Dashboard** | `style-shiper.css` (Bootstrap 4.3) | Shipper | Similar to Restaurant, role-specific |
| **Admin Dashboard** | `style-admin.css` (Bootstrap 5) | Quản trị viên | Full admin, CRUD, charts |

### 1.3 Design Inspiration

- **Customer pages**: Sweetgreen + Ogani (Colorlib template)
- **Dashboard pages**: Deznox admin template (Bootstrap 4.3)
- **Login/Register**: Google Identity-inspired (clean, minimal)
- **Chat Widget**: Modern messenger-style
- **Map tracking**: Leaflet.js with OpenStreetMap tiles

---

## 2. Bảng Màu & Typography

### 2.1 Color Palette

#### Primary Colors (Home Theme)

```css
:root {
    --primary: #3CB815;    /* Xanh lá chủ đạo - buttons, links, badges */
    --secondary: #F65005;  /* Cam - accent, section headers */
    --light: #F7F8FC;      /* Nền sáng */
    --dark: #111111;       /* Text chính */
}
```

#### Dashboard Theme (Restaurant + Shipper + Admin)

```css
:root {
    --primary: #3CB815;     /* Xanh lá buttons */
    --secondary: #3e4954;   /* Xám tối */
    --success: #2bc155;     /* Xanh success */
    --info: #2781d5;        /* Xanh info */
    --warning: #ff6d4d;     /* Cam warning */
    --danger: #f72b50;      /* Đỏ danger */
    --body-bg: #fbfbfb;     /* Nền body */
    --text: #7e7e7e;        /* Text mặc định */
    --heading: #3d4465;     /* Heading color */
    --border: #EEEEEE;      /* Border màu xám nhạt */
}
```

#### Color Usage Matrix

| CSS Variable | HEX | Usage | Text Contrast |
|-------------|-----|-------|---------------|
| `--primary` | `#3CB815` | Buttons, links, active states | White text ✅ |
| `--secondary` | `#F65005` | Accent, highlight band | White text ✅ |
| `--success` | `#2bc155` | Success badges, status | White text ✅ |
| `--danger` | `#f72b50` | Delete, error | White text ✅ |
| `--warning` | `#ff6d4d` | Warning badges | White text ✅ |
| `--info` | `#2781d5` | Info boxes | White text ✅ |
| `--dark` | `#111111` | Heading text | — |
| `--light` | `#f8f9fa` | Background sections | — |
| `--border` | `#e5e7eb` | Borders, dividers | — |

### 2.2 Gradients

Dashboard sử dụng 18 gradients predefined:

| Gradient ID | Colors | Usage |
|-------------|--------|-------|
| `gradient-1` | `#f0a907 → #f53c79` | KPI cards (yellow-pink) |
| `gradient-2` | `#4dedf5 → #480ceb` | KPI cards (cyan-purple) |
| `gradient-3` | `#51f5ae → #3fbcda` | KPI cards (green-blue) |
| `gradient-4` | `#f25521 → #f9c70a` | KPI cards (orange-yellow) |
| `gradient_one` | `rgba(186,1,181) → rgba(103,25,255)` | Sidebar active |

### 2.3 Typography

#### Font Stack (Standardized v3.1)

**Toàn bộ ứng dụng chỉ sử dụng 1 font chính**:

- **Inter** — Tất cả UI elements, inputs, dashboards, body text
- **Roboto** — Chỉ dùng cho Google Identity Login component

```css
/* Customer theme */
body { font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }
/* Logo */
.fs-logo { font-family: 'Inter', sans-serif; font-weight: 800; letter-spacing: -0.5px; }

/* Dashboard themes */
body { font-family: 'Inter', sans-serif; }
```

**Removed fonts**: `Open Sans`, `Lora`, `Cairo`, `Poppins`, `Montserrat`, `Nunito` — đã xoá khỏi tất cả Google Fonts import để giảm HTTP requests và FOUT. Bao gồm:
- `style-shiper.css` (5 imports → 1)
- `style-restaurant.css` (5 imports → 1)
- `_LayoutPageHome.cshtml` (Lora → Inter)
- `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml`, `ChiTietDonHang.cshtml`

#### Font Sizes (Dashboard)

| Element | Size | Weight |
|---------|------|--------|
| h1 | 2.25rem (36px) | 500 |
| h2 | 1.875rem (30px) | 500 |
| h3 | 1.5rem (24px) | 500 |
| h4 | 1.125rem (18px) | 500 |
| h5 | 1rem (16px) | 500 |
| h6 | 0.938rem (15px) | 500 |
| Body | 0.875rem (14px) | 400 |

---

## 3. CSS Variables & Utility Classes

### 3.1 Custom Properties (layout-sg.css)

```css
:root {
    --fs-green:   #3CB815;
    --fs-orange:  #F65005;
    --fs-dark:    #1a1a2e;
    --fs-muted:   #6b7280;
    --fs-light:   #f8f9fa;
    --fs-border:  #e5e7eb;
    --fs-radius:  12px;
    --fs-shadow:  0 4px 20px rgba(0,0,0,.07);
    --fs-nav-h:   68px;
    --fs-topbar-h: 38px;
}
```

### 3.2 Skeleton Loading Overlay

```css
.fs-skeleton-overlay {
    position: fixed; inset: 0; z-index: 9999;
    background: #fff;
    pointer-events: none;
}
.fs-skeleton {
    background: linear-gradient(90deg, #f0f0f0 25%, #e8e8e8 50%, #f0f0f0 75%);
    background-size: 200% 100%;
    animation: fs-shimmer 1.5s infinite;
    border-radius: 8px;
}
.fs-skel-header { height: 68px; margin-bottom: 20px; }
.fs-skel-hero { height: 320px; margin-bottom: 24px; border-radius: 16px; }
.fs-skel-cards { display: flex; gap: 16px; }
.fs-skel-card { flex: 1; height: 220px; border-radius: 16px; }

@keyframes fs-shimmer {
    0%   { background-position: -200% 0; }
    100% { background-position: 200% 0; }
}
```

---

## 4. Layouts & Navigation

### 4.1 Layout Architecture

| Layout File | Used By | Key Features |
|-------------|---------|-------------|
| `_LayoutPageHome.cshtml` | Customer pages | Top bar + Navbar + Footer + Chat Widget |
| `_LayoutPageAmin.cshtml` | Admin pages | Sidebar + Top header + Content + Responsive CSS |
| `_LayoutPageRestaurant.cshtml` | Restaurant pages | Sidebar + Top header + Content + Responsive CSS |
| `_LayoutPageShipper.cshtml` | Shipper pages | Sidebar + Top header + Content + Leaflet CDN + Responsive CSS |
| `_Layout.cshtml` | Default fallback | Simple structure |

### 4.2 Customer Layout (`_LayoutPageHome.cshtml`)

```
┌─────────────────────────────────────────┐
│ Top Bar (38px) - Phone, Email, Social   │
├─────────────────────────────────────────┤
│ Navbar (68px) - Logo | Search | Cart UI │
├─────────────────────────────────────────┤
│                                         │
│         MAIN CONTENT AREA               │
│                                         │
├─────────────────────────────────────────┤
│ Footer - Newsletter | Links | Social    │
├─────────────────────────────────────────┤
│ Fixed: Skeleton loading overlay (on load)│
│ Fixed: Chat Widget (bottom-right)       │
│ Fixed: Back-to-Top button               │
└─────────────────────────────────────────┘
```

**Navbar**: Logo "Fastship" (Inter 800), Search form, User dropdown, Cart icon badge.
**Skeleton loading**: Replaces spinner overlay with shimmer animation.

### 4.3 Dashboard Layout (Admin/Restaurant/Shipper)

```
┌──────────────────────────────────────────────────┐
│ HAMBURGER | Logo | Search | Notif | User Avatar  │
├──────────┬───────────────────────────────────────┤
│          │                                       │
│ SIDEBAR  │          MAIN CONTENT                 │
│ (dark)   │                                       │
│          │    ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐  │
│ - Nav    │    │ KPI │ │ KPI │ │ KPI │ │ KPI │  │
│ - Menu   │    └─────┘ └─────┘ └─────┘ └─────┘  │
│ - Icons  │                                       │
│          │    ┌──────────────────────────────┐   │
│          │    │ TABLE / CHART / FORM          │   │
│          │    │ (responsive stacked cards     │   │
│          │    │  on mobile via data-label)    │   │
│          │    └──────────────────────────────┘   │
├──────────┴───────────────────────────────────────┤
└──────────────────────────────────────────────────┘
```

**Responsive features** (@media < 768px):
- Tables convert to stacked cards using `data-label` attributes
- Touch targets ≥ 44px on all interactive elements
- Sidebar collapses to hamburger overlay

---

## 5. Home Page (Khách Hàng)

### 5.1 Page Structure

```
┌─ HERO CAROUSEL ──────────────────────────────┐
│   Full-width slider with captions            │
├─ CATEGORY PILLS ─────────────────────────────┤
│   Horizontal scrollable pill buttons         │
├─ FEATURED RESTAURANTS (Product Grid) ───────┤
│   4-column grid (responsive: 2→1 col)        │
├─ STATS ROW ──────────────────────────────────┤
│   4 stats: số quán, món ăn, đơn hàng, users │
├─ HOW IT WORKS ───────────────────────────────┤
│   3-step: icon circle + title + description  │
├─ TESTIMONIAL CAROUSEL ───────────────────────┤
│   OwlCarousel with center active highlight   │
├─ PROMO BAND ─────────────────────────────────┤
│   Full-width green band with dismiss button  │
├─ FOOTER + CHAT WIDGET + SKELETON OVERLAY ───┤
└──────────────────────────────────────────────┘
```

### 5.2 Skeleton Loading Behavior

- **On page load**: `.fs-skeleton-overlay` hiển thị với shimmer animation
- **Auto-hide**: JavaScript tự động ẩn sau 600ms (`#fs-loading-skeleton` fade out)
- **Components**: Header bar, hero section, 3 product card skeletons
- **Replaces**: Spinner loading (`#spinner`) hoàn toàn

### 5.3 Product/Restaurant Cards

```
┌─────────────────────┐
│    ┌───────────┐    │
│    │   IMAGE   │    │  ← aspect-ratio: 4/3
│    │  (cover)  │    │  ← zoom 1.08 on hover
│    └───────────┘    │
│  Tên quán ăn        │  ← 2-line clamp
│  Địa chỉ            │  ← 1-line clamp
├─────────────────────┤
│ ⭐ 4.5  │ ⏱ 30ph │  ← Border separated
└─────────────────────┘
```

---

## 6-9. (Các trang còn lại giữ nguyên cấu trúc như phiên bản 3.0)

Đã được cập nhật font về Inter + Roboto, skeleton loading thay spinner, và responsive improvements.

---

## 10. Dashboard Admin

### 10.1 Responsive Behavior (v3.1)

- **Mobile < 576px**: Tables stacked cards với `data-label`, KPI cards 100% width
- **Tablet 576-768px**: Tables stacked cards, charts compact (max-height 200px)
- **Desktop > 768px**: Full layout với sidebar, DataTables, Chart.js

```css
/* Responsive stacked cards cho tất cả dashboard tables */
@media (max-width: 768px) {
    .table-responsive table,
    .table-responsive thead,
    .table-responsive tbody,
    .table-responsive th,
    .table-responsive td,
    .table-responsive tr { display: block; }
    .table-responsive thead { position: absolute; top: -9999px; left: -9999px; }
    .table-responsive td {
        position: relative;
        padding-left: 50% !important;
        border: none;
        border-bottom: 1px solid #eee;
        min-height: 44px;
        display: flex;
        align-items: center;
    }
    .table-responsive td:before {
        content: attr(data-label);
        position: absolute;
        left: 12px;
        width: 45%;
        font-weight: 700;
        color: #333;
    }
    .btn, .nav-link, .dropdown-item { min-height: 44px; min-width: 44px; }
}
```

### 10.2 Data Labels (tất cả dashboard tables)

| File | Table Columns |
|------|--------------|
| `Admin/Order.cshtml` | Đơn hàng, Ngày, Giao đến, Trạng thái, Tổng tiền, "" |
| `Restaurant/OrderList.cshtml` | Ngày đặt, Khách hàng, Tổng tiền, Trạng thái, Ghi chú, Action |
| `Shipper/Index.cshtml` | Đơn hàng, Ngày, VT quán, VT khách hàng, Thu khách, Ship, Trạng thái, "" |
| `Shipper/OrderDetail.cshtml` | Món ăn, Sl, Giá, Tổng giá |

---

## 11-12. (Restaurant & Shipper Dashboards tương tự Admin)

Đã được cập nhật:
- Font Inter cho body text
- Responsive stacked cards CSS
- Touch targets ≥ 44px

---

## 13. Chat Widget (AI + Support)

### 13.1 Widget Overview

Floating chat bubble (bottom-right, z-index 9999):
- **Closed state**: Green circle (56px) with comment icon + unread badge
- **Open state**: 360×520px popup with two tabs: AI Chat + Support

### 13.2 SignalR Real-time Features (v3.1 - Refactored)

| Feature | Before (v3.0) | After (v3.1) |
|---------|---------------|--------------|
| **Unread message check** | Polling every 30s (`setInterval`) | ✅ Real-time SignalR event (`unreadCountUpdate`) |
| **Connection tracking** | DB queries | ✅ `ConcurrentDictionary` in memory |
| **Online status** | Not tracked | ✅ `userOnline`/`userOffline` events |
| **Location streaming** | — | ✅ `UpdateLocation` → `shipperLocationUpdate` |

### 13.3 Hub Methods (Chats.cs)

| Method | Description | Groups |
|--------|-------------|--------|
| `Message(message, id)` | Broadcast to all | — |
| `AdminSendMessage(msg, orderId, connId)` | Admin → specific user | — |
| `CustomerSendMessage(msg, orderId, userName)` | Customer → admin | — |
| `JoinOrderGroup(orderId)` | Join per-order group | `order_{orderId}` |
| `JoinCustomerSupportGroup(userId)` | Join per-user group | `customer_{userId}` |
| `SendToOrderGroup(msg, orderId, senderName, role)` | Send within order group | `order_{orderId}` |
| `NotifyNewMessage(userId, count)` | Real-time unread badge | `customer_{userId}` |
| **`UpdateLocation(orderId, lat, lng)`** | **NEW** Shipper location stream | `order_{orderId}` |
| `OnConnectedAsync` | Track connection + online broadcast | — |
| `OnDisconnectedAsync` | Remove tracking + offline broadcast | — |

### 13.4 Connection State

```csharp
private static readonly ConcurrentDictionary<string, int> _connections = new(); // connId → userId
private static readonly ConcurrentDictionary<int, string> _userConnections = new(); // userId → connId

public static bool IsUserOnline(int userId) => _userConnections.ContainsKey(userId);
public static string? GetUserConnectionId(int userId) => _userConnections.TryGetValue(userId, out var connId) ? connId : null;
```

---

## 14. Live Order Tracking (Leaflet.js + SignalR)

### 14.1 Architecture

```
SHIPPER DEVICE                    SIGNALR HUB                   CUSTOMER VIEW
┌──────────────────┐    ┌────────────────────┐    ┌──────────────────────┐
│  Geolocation API  │───▶│  UpdateLocation()  │───▶│ shipperLocationUpdate│
│  getCurrentPos()  │    │  → Group broadcast │    │  → map.setView()     │
│  WatchPosition()  │    │  order_{orderId}   │    │  → marker.setLatLng()│
└──────────────────┘    └────────────────────┘    └──────────────────────┘
```

### 14.2 Shipper View (Shipper/OrderDetail.cshtml)

```javascript
var map = L.map('live-map').setView([10.8231, 106.6297], 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

// Parse toado string "x,y" → coordinates
var quanToado = '@dh.tbQuanAn?.toado ?? ""';
// → L.marker([lat, lng]) for restaurant pickup location

var khToado = '@dh.tbThongTinDatHang?.toado ?? ""';
// → L.marker([lat, lng]) for customer delivery location
```

### 14.3 Customer View (Cart/ChiTietDonHang.cshtml)

```javascript
// Only shown when order status = "Đang giao"
@if (user.loaitaikhoan.Equals("Khách hàng") && donHang.trangthai.Equals("Đang giao"))
{
    <div id="order-tracking-map" style="height:250px"></div>
}

// Leaflet.js map + SignalR connection
var conn = new signalR.HubConnectionBuilder()
    .withUrl('/nhantin')
    .withAutomaticReconnect()
    .build();

conn.on('shipperLocationUpdate', function(orderId, lat, lng) {
    shipperMarker.setLatLng([lat, lng]);
    map.setView([lat, lng], 15);
});
conn.start().then(() => conn.invoke('JoinOrderGroup', @donHang.madh));
```

### 14.4 SignalR Location Streaming

```csharp
// In Chats.cs hub
public async Task UpdateLocation(int orderId, double lat, double lng)
{
    await Clients.Group($"order_{orderId}").SendAsync("shipperLocationUpdate", orderId, lat, lng);
}
```

### 14.5 Fallback

- **Geolocation denied**: Hiển thị map trung tâm TP.HCM (10.8231, 106.6297)
- **Geolocation unavailable**: Marker tại vị trí mặc định
- **No coordinates**: Map hiển thị không marker, chỉ tile layer

---

## 15. Reusable Components

### 15.1 Component Inventory

| Component | Location(s) | CSS Classes | Status |
|-----------|-------------|-------------|--------|
| **Toast Notification** | Inline JS (checkout) | Dynamic div with fadeOut | ✅ |
| **Result Popup** | Inline JS (checkout) | `.popup-overlay` + `.popup-box` | ✅ |
| **Review Card** | `DetailRestaurant.cshtml` | `.rating`, `.stars`, `.number-rating` | ✅ |
| **Payment Option** | `Checkout.cshtml` | `.payment-option` (radio + icon) | ✅ |
| **Cart Item Row** | `Cart/Index.cshtml` | `.cart-item` (flex, image + qty + total) | ✅ JSON API |
| **Restaurant Card** | `Home/Index.cshtml` | `.product-item` (image + info + footer) | ✅ |
| **Category Pill** | `Home/Index.cshtml` | `.fs-category-pill` (pill button) | ✅ |
| **Address Card** | `Cart/Checkout.cshtml` | `.available-address-card` (radio + detail) | ✅ |
| **KPI Card** | Dashboards | Bootstrap `card` with gradient bg | ✅ Responsive |
| **Data Table** | All dashboards | Bootstrap `table` + DataTables JS | ✅ data-label |
| **Chat Message** | Chat Widget + Admin | `.msg.bot/.user/.admin/.customer` | ✅ Real-time |
| **Status Badge** | Order tables | Bootstrap `badge` with contextual color | ✅ |
| **Empty State** | Cart, reviews, orders | Centered icon + text + CTA | ✅ |
| **Skeleton Loader** | `layout-sg.css` | `.fs-skeleton` + `.fs-skeleton-overlay` | ✅ v3.1 |
| **Live Map** | Shipper + Customer | Leaflet.js + SignalR | ✅ **NEW** v3.1 |

---

## 16. Micro-interactions & Animations

### 16.1 Animation Inventory

| Element | Animation | Duration | Trigger |
|---------|-----------|----------|---------|
| Product card hover | `translateY(-6px)` | 0.3s | Hover |
| Product image hover | `scale(1.08)` | 0.5s | Card hover |
| Button hover | `translateY(-1px)` | 0.2s | Hover |
| Button active | `opacity: .88` | 0.2s | Click |
| Carousel caption | `slideInDown` / `slideInUp` | 0.7s | Slide activate |
| Navbar scroll | `box-shadow` + `bg-white` | 0.5s | Window scroll |
| Skeleton shimmer | `background-position` | 1.5s infinite | Page load |
| Chat typing dots | `typing` keyframes | 1.4s | While AI responds |
| Toast dismiss | `fadeOut` | 0.4s | After 3.5s timeout |
| Map marker update | `setLatLng` | Instant | SignalR event |

### 16.2 Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
        scroll-behavior: auto !important;
    }
}
```

---

## 17. Responsive Design

### 17.1 Breakpoints

| Device | Max Width | Breakpoint |
|--------|-----------|------------|
| Phone | 576px | `@media (max-width: 576px)` |
| Tablet portrait | 768px | `@media (max-width: 767.98px)` |
| Tablet landscape | 992px | `@media (max-width: 991.98px)` |
| Desktop | 1200px | `@media (max-width: 1199.98px)` |
| Large desktop | 1440px | `@media (max-width: 1439.98px)` |

### 17.2 Dashboard Responsive (v3.1)

| Component | Mobile <576px | Tablet 576-768px | Desktop >768px |
|-----------|---------------|------------------|----------------|
| **Data tables** | Stacked cards (`data-label`) | Stacked cards | Full DataTable |
| **Sidebar** | Hamburger overlay | Hamburger overlay | Fixed sidebar |
| **KPI cards** | 100% width (stacked) | 2 columns | 4 columns |
| **Charts** | max-height 200px | max-height 200px | Full size |
| **Touch targets** | ≥ 44px all | ≥ 44px all | Normal |
| **Map** | 250px height | 300px height | 380px height |
| **Table cells** | flex + border-bottom | flex + border-bottom | table cells |

### 17.3 Responsive Stacked Cards CSS

```css
/* Applied to: _LayoutPageAmin.cshtml, _LayoutPageShipper.cshtml, _LayoutPageRestaurant.cshtml */
@media (max-width: 768px) {
    .table-responsive thead { position: absolute; top: -9999px; left: -9999px; }
    .table-responsive td {
        display: flex;
        align-items: center;
        padding-left: 50% !important;
        min-height: 44px;
        border: none;
        border-bottom: 1px solid #eee;
    }
    .table-responsive td:before {
        content: attr(data-label);
        position: absolute;
        left: 12px;
        width: 45%;
        font-weight: 700;
        color: #333;
    }
}
```

---

## 18. Accessibility (WCAG)

### 18.1 Implemented Accessibility Features

| Feature | Implementation | Status |
|---------|---------------|--------|
| **Touch targets** | Min 44×44px on all interactive elements | ✅ v3.1 |
| **Focus states** | `outline: 3px solid var(--fs-green); outline-offset: 2px` | ✅ |
| **Reduced motion** | `@media (prefers-reduced-motion: reduce)` | ✅ |
| **Form labels** | All inputs have `<label>` elements | ✅ |
| **Error messages** | Toast notifications for validation errors | ✅ |
| **Alt text** | Images use descriptive `alt` attributes | ✅ |
| **Semantic HTML** | Proper heading hierarchy, `<main>`, `<nav>`, `<section>` | ✅ |
| **Readable font** | Body `font-size: 16px` on mobile | ✅ |
| **Line height** | `1.6` for body, `1.65` for paragraphs | ✅ |

### 18.2 Areas for Improvement

- [ ] Add `aria-label` to icon-only buttons (cart delete, chat toggle)
- [ ] Implement skip-to-content link
- [ ] Add ARIA live regions for dynamic content updates
- [ ] Add keyboard support for star rating (arrow keys)
- [ ] Ensure sufficient color contrast in dashboard gradient KPI cards

---

## 19. Icons & Iconography

(Giữ nguyên dari v3.0: Font Awesome 5, Simple Line Icons, Material Design Icons, Themify, Line Awesome, Flaticon, Icomoon, Avasta, Bootstrap Icons, Elegant Icons, SVG inline)

---

## 20. Error Handling & Empty States

### 20.1 CSS Skeleton Loading (NEW v3.1)

Thay thế spinner loading bằng shimmer skeleton:

| File | Change |
|------|--------|
| `layout-sg.css` | Added `.fs-skeleton-overlay`, `.fs-skel-header`, `.fs-skel-hero`, `.fs-skel-cards`, `.fs-skel-card`, `@keyframes fs-shimmer` |
| `main.js` | Changed spinner handler → skeleton handler (`#fs-loading-skeleton` fade out) |
| `_LayoutPageHome.cshtml` | Replaced spinner overlay HTML → skeleton overlay HTML |
| Logo font changed | Lora → Inter (800 weight, -0.5px letter-spacing) |

---

## 21. User Flows

### 21.1 Customer Flow (Updated)

```
HOME ──→ Browse categories ──→ Restaurant list
  │                                │
  │                                ▼
  │                          Restaurant detail
  │                          (info + menu + reviews)
  │                                │
  │                                ▼
  │                          Add items to cart
  │                                │
  │                                ▼
  │                          CART (adjust qty - JSON API)
  │                                │
  │                                ▼
  │                          CHECKOUT
  │                    (address + coupon + payment)
  │                                │
  │                                ▼
  │                    Payment processing (mock)
  │                          /            \
  │                        ✅            ❌
  │                  Success popup    Error popup
  │                        │
  │                        ▼
  │                   Order history
  │                   (track status - REAL-TIME with SignalR + Leaflet.js)
  │                        │
  │                        ▼
  │              LIVE MAP TRACKING (NEW v3.1)
  │              Khi đơn hàng chuyển "Đang giao"
  │              → SignalR nhận shipperLocationUpdate
  │              → Marker di chuyển real-time trên map
```

### 21.2 Shipper Flow (Updated)

```
LOGIN ──→ DASHBOARD (with LIVE MAP)
              │
              ▼
      FREE ORDER LIST + MY ORDERS
              │
      [Accept/Reject] [Update status]
              │        Đã lấy → Hoàn thành
              ▼
      RECEIVED ORDER → PICK UP → DELIVER
              │
              ├── Leaflet.js map shows current location
              ├── Geolocation API tracks position
              └── SignalR streams coordinates to customer
```

---

## 22. Backlog & Improvements

### 22.1 ✅ Completed in v3.1

| Task | Status | Details |
|------|--------|---------|
| **BCrypt password hashing** | ✅ | HomeController: Login + Signup |
| **Password strength validation** | ✅ | 8+ chars, 1 upper, 1 lower, 1 digit, 1 special |
| **ValidateAntiForgeryToken** | ✅ | All POST actions |
| **Remove 30s polling** | ✅ | _ChatWidget.cshtml → SignalR unreadCountUpdate |
| **SignalR connection tracking** | ✅ | Chats.cs ConcurrentDictionary |
| **Font standardization** | ✅ | Inter only (removed Open Sans, Lora, Cairo, Poppins, Montserrat, Nunito) |
| **Skeleton loading** | ✅ | Thay spinner bằng shimmer CSS |
| **Cart JSON API** | ✅ | ApiTangSoLuong / ApiGiamSoLuong |
| **Leaflet.js live tracking** | ✅ | Shipper + Customer views + SignalR streaming |
| **Responsive dashboards** | ✅ | data-label stacked cards, touch targets ≥44px |

### 22.2 Future Improvements

- [ ] **Dark mode**: Add CSS custom properties swap
- [ ] **Smooth page transitions**: View transitions API
- [ ] **Infinite scroll**: Replace pagination with Intersection Observer
- [ ] **Drag & drop**: For cart item reordering
- [ ] **Image lazy loading**: `loading="lazy"` on all `<img>`
- [ ] **Bottom sheet**: Replace popups with bottom sheets on mobile
- [ ] **Pull-to-refresh**: For order history on mobile
- [ ] **Search autocomplete**: Debounced API search with suggestions
- [ ] **Real payment**: Replace mock Vietcombank with Stripe/PayPal/ZaloPay
- [ ] **Unit tests**: Add frontend component tests (Jest/Cypress)
- [ ] **Image optimization**: WebP format with `<picture>` fallback
- [ ] **Critical CSS**: Inline above-fold styles
- [ ] **Service Worker**: Offline support for order tracking
- [ ] **Remove legacy Bootstrap 3**: `wwwroot/Content/bootstrap.css` unused

---

> **Document Version**: 3.1 (Full)  
> **Based on**: Actual source code analysis of 8 Controllers, 25+ Views, 15+ Models, 10+ CSS files, 5 Layout files, 1 SignalR Hub  
> **Key changes v3.1**: Security (BCrypt), SignalR real-time (no polling), Font consolidation (Inter only), Skeleton loading, Leaflet.js live tracking, Responsive dashboards
