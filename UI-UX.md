# Fastship (ShipFood) — UI/UX Documentation (Full)

> **Phiên bản**: 3.2 — Mobile Responsive Overhaul + Header Sticky Fix + Cart/Checkout/Signup Mobile Optimization  
> **Cập nhật**: Tháng 7, 2026  
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

## 3.3 Header Sticky Fix — Skeleton Overlay Conflict (v3.2)

### 3.3.1 Vấn đề

Header (`#fs-header`) sử dụng `position: fixed` với `z-index: 1030`. Tuy nhiên, skeleton loading overlay (`#fs-loading-skeleton`) có `z-index: 9999` (cao hơn) và dùng `position: fixed` phủ kín toàn màn hình (`inset: 0`).

Vì skeleton overlay nằm TRÊN header trong DOM, header bị CHE KHUẤT hoàn toàn khi trang load. Người dùng phải chờ skeleton fade out (100ms timeout + 250ms animation) mới thấy header.

```
BEFORE (v3.1):
┌─────────────────────────────────────┐
│ SKELETON OVERLAY (z-index: 9999)    │ ← Phủ lên header
│  ┌────────────────────────────────┐ │
│  │ HEADER (z-index: 1030) ẨN sau │ │
│  │ skeleton                     │ │
│  └────────────────────────────────┘ │
│  Main content skeleton...           │
└─────────────────────────────────────┘
  → Header chỉ hiện ra sau 350ms
```

### 3.3.2 Giải pháp

1. **Header z-index**: Tăng từ `1030` lên `10000` (cao hơn skeleton overlay)
2. **Skeleton overlay top**: Thay đổi từ `top: 0` thành `top: calc(var(--fs-nav-h) + var(--fs-topbar-h))` trên desktop và `top: var(--fs-nav-h)` trên mobile
   - Desktop: skeleton bắt đầu từ vị trí **dưới header** (68px + 38px = 106px)
   - Mobile (< 992px): skeleton bắt đầu từ dưới nav (68px hoặc 60px)

```
AFTER (v3.2):
┌─ HEADER (z-index: 10000) ─────────┐ ← Luôn hiện trên cùng
├─ SKELETON OVERLAY (z-index: 9999) ─┤ ← Bắt đầu dưới header
│  Main content skeleton...           │
└─────────────────────────────────────┘
  → Header luôn visible từ đầu
```

### 3.3.3 Files thay đổi

| File | Thay đổi |
|------|----------|
| `layout-sg.css` | `.fs-header` z-index: 1030 → 10000 |
| `layout-sg.css` | `.fs-skeleton-overlay` top: 0 → `calc(var(--fs-nav-h) + var(--fs-topbar-h))` + responsive override |

### 3.3.4 CSS Code

```css
/* Header luôn trên cùng */
.fs-header {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 10000; /* Cao hơn skeleton overlay (9999) */
    background: #fff;
}

/* Skeleton chỉ che content, không che header */
.fs-skeleton-overlay {
    position: fixed;
    top: calc(var(--fs-nav-h) + var(--fs-topbar-h)); /* 106px desktop */
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 9999;
}
@media (max-width: 991.98px) {
    .fs-skeleton-overlay { top: var(--fs-nav-h); } /* 68px/60px mobile */
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

---

## 6. Trang Chi Tiết Quán Ăn (DetailRestaurant)

### 6.1 Layout Desktop

```
┌──────────────────────────────────────────────────┐
│ ┌──────────┐ ┌──────────────────────────────┐    │
│ │   IMG    │ │  Breadcrumb ▼                │    │
│ │  480×300 │ │  Tên quán, địa chỉ, rating   │    │
│ └──────────┘ │  Giờ mở cửa, giá             │    │
│              │  Utility bar                  │    │
│              └──────────────────────────────┘    │
├──────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────────────────────────┐    │
│ │ Sidebar  │ │  Search bar                  │    │
│ │ Danh mục │ │  Danh sách món ăn            │    │
│ │ 235px    │ │  (590px)                     │    │
│ └──────────┘ │  Mỗi item: img + info + giá  │    │
│              └──────────────────────────────┘    │
├──────────────────────────────────────────────────┤
│ Reviews section (grid auto-fill, minmax 280px)   │
└──────────────────────────────────────────────────┘
```

### 6.2 Vấn đề Mobile (v3.1)

Trên mobile, các element dùng width cố định gây tràn layout:

| Element | Width cố định | Vấn đề |
|---------|---------------|--------|
| `.detail-restaurant-img` | 480px | Tràn màn hình 375px |
| `.detail-restaurant-info` | 645px | Đẩy content ra ngoài |
| `.menu-restaurant-detail` | 590px (78%) | Không xuống dòng |
| `.menu-restaurant-category` | 235px | Chiếm quá nhiều space |
| `.name-restaurant` | `white-space: nowrap` | Text bị cắt |
| `.utility-item` | 140px float | Xếp chồng lộn xộn |

### 6.3 Fix Mobile (v3.2)

```css
@media (max-width: 768px) {
    /* Ảnh + info: bỏ float, full width */
    .now-detail-restaurant .detail-restaurant-img {
        width: 100%; height: auto; float: none;
    }
    .now-detail-restaurant .detail-restaurant-info {
        width: 100%; float: none; padding: 16px 0;
    }
    
    /* Sidebar chuyển thành scroll ngang */
    .now-menu-restaurant .menu-restaurant-category {
        position: relative; width: 100%; margin-bottom: 12px;
    }
    .menu-restaurant-category .scrollbar-container {
        max-height: none; overflow-x: auto;
        display: flex; flex-wrap: nowrap; gap: 6px;
    }
    
    /* Menu items xếp dọc */
    .menu-restaurant-detail { width: 100%; padding: 6px 10px; }
    .item-restaurant-row .row { flex-direction: column; }
    
    /* Utility items full width */
    .now-detail-restaurant .utility-restaurant .utility-item {
        width: 100%; float: none; margin-bottom: 10px;
    }
    
    /* Reviews: 1 cột */
    #review-list { grid-template-columns: 1fr !important; }
}

@media (max-width: 576px) {
    .now-detail-restaurant .name-restaurant { font-size: 16px; white-space: normal; }
    .item-restaurant-row .current-price { font-size: 14px; }
    .now-detail-restaurant .detail-restaurant-img img {
        max-height: 200px; object-fit: cover;
    }
}
```

### 6.4 Hình ảnh trực quan Mobile (v3.2)

```
┌──────────────────────┐
│ ┌──────────────────┐ │
│ │    IMG (100%)    │ │
│ │    maxH: 200px   │ │
│ └──────────────────┘ │
│ Breadcrumb           │
│ Tên quán (font 16px) │
│ Địa chỉ (font 12px)  │
│ ⭐ 4.5 | 30 đánh giá │
│ Utility (full width) │
├──────────────────────┤
│ [Tất cả] [Cơm] [Phở…]│ ← scroll ngang
├──────────────────────┤
│ 🔍 [Tìm món........] │
├──────────────────────┤
│ ┌──┐ ┌────────────┐ │
│ │img│ │Tên món     │ │
│ │60 │ │Mô tả       │ │
│ └──┘ │Đã đặt 100+ │ │
│      └────────────┘ │
│ Giá: 35.000đ [1][+] │ ← xếp dọc
├──────────────────────┤
│ ⭐ Đánh giá (1 cột)  │
└──────────────────────┘
```

---

## 7. Giỏ Hàng (Cart)

### 7.1 Layout Desktop

```
┌──────────────────────────────────────────────────┐
│ CỘT TRÁI (col-lg-8)      │ CỘT PHẢI (col-lg-4)  │
│ ┌────────────────────┐    │ ┌──────────────────┐ │
│ │ 🛒 Giỏ hàng   Quán │    │ │ Tóm tắt đơn hàng │ │
│ │                    │    │ │ Tổng món: XXXđ   │ │
│ │ [IMG] Tên món      │    │ │ Phí ship: 15.000đ│ │
│ │       giá: XXđ     │    │ │ ═══════════════  │ │
│ │       [-] [2] [+]  │    │ │ TỔNG: XXXđ      │ │
│ │                     │    │ │ [Thanh toán]     │ │
│ │ [IMG] Tên món 2    │    │ └──────────────────┘ │
│ └────────────────────┘    │                      │
└──────────────────────────────────────────────────┘
```

### 7.2 Cart Item Responsive (v3.2)

**Vấn đề mobile**: Trên màn hình < 576px, cart item có flex layout với ảnh 80px + info + qty control + total + delete button. Tổng cộng quá nhiều element trên 1 hàng, gây tràn.

**Fix**:

```css
@media (max-width: 768px) {
    .cart-item { flex-wrap: wrap; gap: 10px; padding: 14px 16px; }
    .cart-item img { width: 60px; height: 60px; }
    .cart-item .item-info { min-width: calc(100% - 76px); }
    .qty-control { order: 2; }
    .item-total { min-width: auto; font-size: 13px; order: 3; }
    .delete-btn { order: 4; }
}

@media (max-width: 576px) {
    .cart-item img { width: 48px; height: 48px; }
    .cart-item .item-name { font-size: 12px; }
    .qty-btn { width: 28px; height: 28px; }
    .qty-num { min-width: 28px; font-size: 14px; }
}
```

**Hình ảnh trực quan**:

```
MOBILE (< 576px):
┌────────────────────────────────────┐
│ ┌────┐ ┌───────────────────────┐   │
│ │48px│ │ Tên món (font 12px)   │   │
│ │img │ │ Giá: 35.000đ / phần   │   │
│ └────┘ └───────────────────────┘   │
│ [-] [2] [+]     35.000đ       🗑   │ ← cùng hàng
├────────────────────────────────────┤
│ Tóm tắt đơn hàng                   │
│ Tổng món: 70.000đ                  │
│ Phí ship: 15.000đ                  │
│ TỔNG: 85.000đ                      │
│ [████████ Thanh toán ██████████]   │
└────────────────────────────────────┘
```

### 7.3 Empty State (Giỏ hàng trống)

```
┌────────────────────────────────────┐
│                                    │
│         🛍️ (icon 80px)             │
│   "Giỏ hàng của bạn đang trống"    │
│   "Hãy chọn món ăn yêu thích"      │
│                                    │
│   [🏠 Khám phá quán ăn]           │
│                                    │
└────────────────────────────────────┘
```

---

## 8. Thanh Toán (Checkout)

### 8.1 Layout Desktop

```
┌────── CỘT TRÁI (col-lg-8) ──────┐ ┌── CỘT PHẢI (col-lg-4) ──┐
│ ┌─────────────────────────────┐  │ │ ┌──────────────────────┐ │
│ │ 📍 Địa chỉ giao hàng        │  │ │ │ 📋 Đơn hàng          │ │
│ │ [Nhập mới] [Vị trí] [Lưu]   │  │ │ │                      │ │
│ │ Họ tên | SĐT                │  │ │ │ Món A x2    70.000đ  │ │
│ │ Quận | Địa chỉ              │  │ │ │ Món B x1    35.000đ  │ │
│ └─────────────────────────────┘  │ │ │ Tạm tính   105.000đ  │ │
│ ┌─────────────────────────────┐  │ │ │ Giảm giá       0đ    │ │
│ 📝 Ghi chú & Khuyến mãi       │  │ │ │ Ship        15.000đ  │ │
│ └─────────────────────────────┘  │ │ │ ════════════════════ │ │
│ ┌─────────────────────────────┐  │ │ │ TỔNG:     120.000đ  │ │
│ │ 💳 Phương thức thanh toán   │  │ │ │                      │ │
│ │ ○ Tiền mặt (COD)           │  │ │ │ [Xác nhận đặt hàng]  │ │
│ │ ○ Chuyển khoản             │  │ │ └──────────────────────┘ │
│ └─────────────────────────────┘  │ └────────────────────────┘
└──────────────────────────────────┘
```

### 8.2 Mobile Checkout (v3.2)

**Vấn đề**: Payment options, address tabs, coupon box không responsive trên mobile.

**Fix** (CSS trong `layout-sg.css`):

```css
@media (max-width: 768px) {
    .checkout-card { padding: 20px 16px !important; }
    .address-tabs .tab-btn { padding: 8px 12px !important; font-size: 12px !important; }
    .payment-option { padding: 12px !important; gap: 8px !important; }
}

@media (max-width: 576px) {
    .coupon-box .input-group { flex-direction: column !important; gap: 8px !important; }
    .coupon-box .btn { width: 100% !important; }
}
```

---

## 9. Đăng Nhập / Đăng Ký / Quên Mật Khẩu

### 9.1 Vấn đề "Header bị đẩy xuống" (SCROLL ISSUE)

#### Mô tả lỗi

Trang Login, Signup, Forgot là các **standalone HTML** — hoàn toàn không sử dụng layout `_LayoutPageHome.cshtml`. Kết quả là:
1. **Không có thanh header navigation** nào ở trên cùng
2. Người dùng từ trang chủ click "Đăng nhập" → chuyển đến trang chỉ có form login, **mất hết context navigation**
3. Phải cuộn xuống hoặc click "← Quay về trang chủ" để quay lại

```
BEFORE (v3.1): Trang Login là HTML riêng, không header
┌────────────────────────────────────┐
│                                    │ ← KHÔNG CÓ HEADER
│        ← Quay về trang chủ        │
│                                    │
│          ┌──────────────┐          │
│          │   Fastship   │          │
│          │  Đăng nhập   │          │
│          │              │          │
│          │ [Username]   │          │
│          │ [Password]   │          │
│          │ [Đăng nhập]  │          │
│          └──────────────┘          │
│                                    │
└────────────────────────────────────┘
```

### 9.2 Giải pháp: Fixed Header trên trang Auth

Thêm fixed header bar cho cả 3 trang Login, Signup, Forgot:

```
AFTER (v3.2):
┌─ FIXED HEADER (position:fixed, z-index:10000) ─┐
│ [F]ast[ship]                    [← Trang chủ] [Đăng ký] │
├────────────────────────────────────────────────────┤
│                                                    │
│             ┌────────────────────┐                  │
│             │     Fastship       │                  │
│             │   Đăng nhập        │                  │
│             │                    │                  │
│             │  [Username]        │                  │
│             │  [Password]        │                  │
│             │  [Đăng nhập]       │                  │
│             └────────────────────┘                  │
│                                                    │
└────────────────────────────────────────────────────┘
```

**Chi tiết kỹ thuật**:

```html
<!-- Fixed header trên mỗi standalone page -->
<div style="position:fixed; top:0; left:0; right:0; z-index:10000;
     background:#fff; box-shadow:0 2px 12px rgba(0,0,0,.06);
     height:60px; display:flex; align-items:center; padding:0 16px;">
    <a href="/Home" style="text-decoration:none;">
        <span style="font-family:'Inter';font-size:1.35rem;font-weight:800;">
            <span style="color:#3CB815;">F</span>
            <span style="color:#F65005;">ast</span>
            <span style="color:#1a1a2e;">ship</span>
        </span>
    </a>
    <div style="margin-left:auto;">
        <a href="~/Home">← Trang chủ</a>
        <a href="~/Home/Signup" style="background:#3CB815;color:#fff;
             border-radius:20px;padding:8px 16px;">Đăng ký</a>
    </div>
</div>
```

### 9.3 Body Padding Adjustment

Khi thêm fixed header 60px, cần đẩy nội dung xuống để không bị che:

| Trang | Before padding | After padding |
|-------|---------------|---------------|
| Login | `padding: 20px` | `padding: 80px 20px 20px` |
| Signup | `padding: 20px` | `padding: 80px 20px 20px` |
| Forgot | không có padding | `padding-top: 80px` |

### 9.4 Login/Signup Form Mobile Responsive

**Vấn đề**: Container form có width 420px, trên mobile nhỏ hơn có overflow.

**Fix** trong `login.css`:

```css
@media (max-width: 576px) {
    .login-container { padding: 24px 16px; }
    .register-container { padding: 24px 16px; }
    .forgot-password-container { padding: 24px 16px; }
    
    /* iOS Safari không zoom input khi font ≥ 16px */
    form input { font-size: 16px !important; }
    
    /* Touch targets lớn hơn */
    .login-submit { padding: 14px 12px; }
    .register-submit { padding: 14px 12px; }
    .google-btn { padding: 10px 16px; font-size: 13px; }
}

@media (max-width: 400px) {
    .login-container { padding: 18px 12px; }
    .login-container h2 { font-size: 18px; }
}
```

### 9.5 Fixed Header Buttons

| Trang | Nút phải | Màu sắc |
|-------|----------|---------|
| Login | "Đăng ký" | `background: #3CB815` (xanh lá) |
| Signup | "Đăng nhập" | `background: #3CB815` (xanh lá) |
| Forgot | "Đăng nhập" | `background: #3CB815` (xanh lá) |

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

---

## 13. Chat Widget (AI + Support)

### 13.1 Widget Overview

Floating chat bubble (bottom-right, z-index 9999):
- **Closed state**: Green circle (56px) with comment icon + unread badge
- **Open state**: 360×520px popup with two tabs: AI Chat + Support

### 13.2 Chat Widget Mobile Responsive (v3.2)

**Vấn đề**: Trên mobile, chat toggle (56px) và chat box (360px) dùng kích thước cố định, không vừa màn hình nhỏ.

**Fix** (thêm vào `layout-sg.css`):

```css
@media (max-width: 576px) {
    /* Nút chat nhỏ lại */
    .chat-toggle {
        bottom: 16px !important;
        right: 16px !important;
        width: 48px !important;
        height: 48px !important;
        font-size: 20px !important;
    }
    
    /* Chat box full width gần như */
    .chat-box {
        bottom: 76px !important;
        right: 8px !important;
        left: 8px !important;
        width: auto !important;
        max-height: calc(100vh - 100px) !important;
    }
    
    /* Message area ngắn lại */
    .chat-msgs {
        max-height: 220px !important;
        min-height: 150px !important;
    }
}
```

```
MOBILE:
┌────── 8px ──────┐
│                  │
│  ┌────────────┐ │
│  │ Chat box   │ │
│  │ full width │ │
│  │ (auto)     │ │
│  └────────────┘ │
│          ┌──┐   │
│          │48│   │ ← Toggle nhỏ hơn
│          └──┘   │
└──────────────────┘
   left: 8px → ← right: 8px
```

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

## 17. Responsive Design — Mobile Optimization (v3.2)

### 17.1 Breakpoints

| Device | Max Width | Breakpoint |
|--------|-----------|------------|
| Phone nhỏ | 400px | `@media (max-width: 400px)` |
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

### 17.3 Mobile Responsive Matrix (v3.2 — Tất cả các trang)

| Trang | Vấn đề | Fix | Files |
|-------|--------|-----|-------|
| **Header/Skeleton** | Skeleton overlay z-index 9999 che header z-index 1030 | Header z-index: 10000, skeleton top: calc(header height) | `layout-sg.css` |
| **Login/Signup/Forgot** | Không có header, mất navigation context | Thêm fixed header bar (position:fixed, z-index:10000) | `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml` |
| **Login/Signup/Forgot** | Container 420px tràn trên mobile | Padding giảm 40px→24px/18px, input font-size: 16px | `login.css` |
| **DetailRestaurant** | Ảnh 480px + info 645px + sidebar 235px fixed | 100% width, sidebar scroll ngang, items xếp dọc | `DetailRestaurant.cshtml` |
| **Cart/Index** | Cart items quá nhiều element trên 1 hàng | Flex-wrap, ảnh 80px→48px, font 15px→12px | `Cart/Index.cshtml` |
| **ChiTietDonHang** | margin-top: 200px quá lớn | Giảm 200px→130px (desktop) / 80px (mobile) | `ChiTietDonHang.cshtml` |
| **LichSuDatHang** | margin-top: 150px, table không responsive | Thêm mobile styles, responsive container | `LichSuDatHang.cshtml` |
| **Thanh toán** | Payment options, coupon box không responsive | CSS responsive cho checkout cards, payment options | `layout-sg.css` |
| **Chat Widget** | Toggle 56px + box 360px cố định | Toggle 48px, box full width (8px padding) | `layout-sg.css` |
| **Nhắn tin** | Page header quá to trên mobile | Header padding giảm 12rem→5rem, chat box height 450→300 | `NhanTin.cshtml` |
| **DanhMuc/SanPham** | Ảnh height: 250px không có object-fit | Thêm `object-fit: cover` | `DanhMuc.cshtml`, `SanPham.cshtml` |
| **Page Header** | `padding-top: 12rem` quá lớn (192px) | Responsive: 7rem (tablet), 5rem (mobile) | `layout-sg.css`, `style.css` |

### 17.4 Responsive Stacked Cards CSS

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

### 17.5 Chi tiết từng breakpoint behavior

#### < 400px (Phone nhỏ: iPhone SE, Galaxy S8, ...)

| Component | Behavior |
|-----------|----------|
| Login/Signup form | Padding 18px 12px, h2 font-size 18px |
| Header | Nav height 60px, logo font-size 1.35rem |
| Cart items | Ảnh 48px, item name 12px, qty-btn 28px |

#### < 576px (Phone thường)

| Component | Behavior |
|-----------|----------|
| Header | `--fs-nav-h: 60px`, body padding-top: 60px |
| Restaurants grid | 2 columns (col-6) |
| Product card | aspect-ratio 16:9, title 13px, address 12px |
| Stats row | 2 columns, border-bottom thay border-right |
| Footer newsletter | Input-group xếp dọc, nút full width |
| Promo band | Font 13px, padding 10px 36px |
| Chat toggle | 48px, bottom: 16px, right: 16px |
| Chat box | Full width (left: 8px, right: 8px) |
| Cart item | Flex-wrap, ảnh 48px, font 12px |
| Carousel | Caption position relative, background rgba(0,0,0,.45) |
| DetailRestaurant | Ảnh max-height 200px, tên quán 16px |
| Touch targets | `min-height: 44px`, `padding: 12px 16px` |
| Body font | 16px (iOS zoom prevention) |

#### 576px - 768px (Tablet nhỏ)

| Component | Behavior |
|-----------|----------|
| Header | `--fs-nav-h: 68px`, body padding-top: 68px |
| Search bar | Ẩn trên navbar, hiện trong collapse |
| Restaurants grid | 2 columns |
| Page header | padding-top 7rem, h1 font-size 1.6rem |
| Cart | items wrap, ảnh 60px |
| Checkout cards | padding 20px 16px |

#### 768px - 992px (Tablet lớn)

| Component | Behavior |
|-----------|----------|
| Navbar collapse | Hiển thị hamburger, nav links xếp dọc |
| Top bar | Ẩn (d-none d-lg-block) |
| Search | Ẩn desktop search, hiện mobile search |
| Skeleton cards | 2 columns grid |

#### > 992px (Desktop)

Layout chuẩn: top bar, navbar full width, search inline, 4 columns grid

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

### 22.2 ✅ Completed in v3.2 (Mobile Responsive Overhaul)

| Task | Status | Files | Chi tiết |
|------|--------|-------|----------|
| **Header sticky fix** | ✅ | `layout-sg.css` | Header z-index 1030→10000; skeleton top 0→calc(header height) |
| **Login/Signup/Forgot header** | ✅ | `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml` | Thêm fixed header bar (position:fixed, z-index:10000) |
| **Login form mobile** | ✅ | `login.css` | Padding giảm 40px→24px, input font-size 16px, buttons padding 14px |
| **DetailRestaurant mobile** | ✅ | `DetailRestaurant.cshtml` | Ảnh/info 100%, sidebar scroll ngang, items xếp dọc |
| **Cart/Index responsive** | ✅ | `Cart/Index.cshtml` | Cart items flex-wrap, ảnh 80px→48px, font 15px→12px |
| **ChiTietDonHang fix** | ✅ | `ChiTietDonHang.cshtml` | margin-top 200px→130px/80px, mobile responsive styles |
| **LichSuDatHang fix** | ✅ | `LichSuDatHang.cshtml` | margin-top 150px→100px/80px, DataTables responsive |
| **Checkout mobile** | ✅ | `layout-sg.css` | Payment/coupon/address tabs responsive |
| **Chat Widget mobile** | ✅ | `layout-sg.css` | Toggle 56px→48px, box full-width (8px gutter) |
| **NhanTin responsive** | ✅ | `NhanTin.cshtml` | Page header giảm, chat box height 300-350px |
| **DanhMuc/SanPham ảnh** | ✅ | `DanhMuc.cshtml`, `SanPham.cshtml` | Thêm object-fit:cover cho ảnh |
| **Page header mobile** | ✅ | `layout-sg.css` | padding-top 12rem→7rem/5rem |

### 22.3 Future Improvements

- [ ] **Dark mode**: Add CSS custom properties swap
- [ ] **Smooth page transitions**: View transitions API
- [ ] **Infinite scroll**: Replace pagination with Intersection Observer
- [ ] **Drag & drop**: For cart item reordering
- [ ] **Bottom sheet**: Replace popups with bottom sheets on mobile
- [ ] **Pull-to-refresh**: For order history on mobile
- [ ] **Search autocomplete**: Debounced API search with suggestions
- [ ] **Real payment**: Replace mock Vietcombank with Stripe/PayPal/ZaloPay
- [ ] **Unit tests**: Add frontend component tests (Jest/Cypress)
- [ ] **Image optimization**: WebP format with `<picture>` fallback
- [ ] **Critical CSS**: Inline above-fold styles
- [ ] **Service Worker**: Offline support for order tracking
- [ ] **Remove legacy Bootstrap 3**: `wwwroot/Content/bootstrap.css` unused
- [ ] **Add `data-label` attributes to all dashboard tables**: Hiện tại stacked cards dùng CSS generic selector, nên thêm data-label cụ thể
- [ ] **Dashboard mobile optimization**: Responsive sidebar, charts, KPI cards cho Admin/Restaurant/Shipper
- [ ] **Google OAuth deployment test**: Kiểm tra đăng nhập Google trên Railway production

---

> **Document Version**: 3.2 (Full)  
> **Cập nhật**: Tháng 7, 2026  
> **Based on**: Actual source code analysis of 8 Controllers, 25+ Views, 15+ Models, 10+ CSS files, 5 Layout files, 1 SignalR Hub, 2 sessions of responsive mobile fixes  
> **Key changes v3.2**:  
> - Fixed header sticky (z-index 10000 > skeleton 9999)  
> - Added fixed header to Login/Signup/Forgot standalone pages  
> - Responsive DetailRestaurant: ảnh+info 100%, sidebar scroll ngang  
> - Responsive Cart: items flex-wrap, ảnh/thu nhỏ trên mobile  
> - Chat widget mobile positioning (48px, full-width box)  
> - Checkout mobile: payment options, coupon, address tabs responsive  
> - Page-header padding 12rem→5rem mobile  
> - Login form: padding, font-size 16px, touch targets  
> - ChiTietDonHang, LichSuDatHang, NhanTin margin/padding fix  
> - 12 files modified, ~259 insertions, ~22 deletions  

