# Fastship (ShipFood) — UI/UX Documentation (Full)

> **Phiên bản**: 5.7 — Chat auto-message distinction, loading states, audio notifications, geolocation cleanup  
> **Cập nhật**: Tháng 7, 2026  
> **Mô tả**: Tài liệu thiết kế giao diện & trải nghiệm người dùng toàn diện cho nền tảng đặt đồ ăn Fastship  
> **Tài liệu liên quan**: Project.md — Tổng quan kiến trúc & phát triển

## ⚠️ Lưu ý: Filter/Search đang quá phức tạp

Hệ thống filter hiện tại (`MenuSearch` API + `FilterBar` ViewComponent + `filter.js` Bottom Sheet) có **quá nhiều tầng logic**:
- 2-phase AND→OR fallback với scoring
- Dynamic SQL với 7+ tham số
- Bottom Sheet + Chip bar two-way sync
- Cart LocalStorage persistence layer

**Cần đơn giản hoá**: Giảm xuống còn 1 phase, bỏ OR fallback, gộp filter.js vào 1 file duy nhất.


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
    - [9.6 Viewport Wrapper — Fix Scroll Rác](#96-viewport-wrapper--fix-scroll-rác-v33)
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
    - [22.3 ✅ Completed in v3.3 — Layout Architecture Overhaul](#223--completed-in-v33--layout-architecture-overhaul)
    - [22.4 Future Improvements](#224-future-improvements)
23. [Dual-Filter Bar & Cart Persistence](#23-dual-filter-bar--cart-persistence-grab-ui)
    - [23.1 FilterBar ViewComponent](#231-filterbar-viewcomponent)
    - [23.2 Bottom Sheet Filter UI](#232-bottom-sheet-filter-ui)
    - [23.3 Cart LocalStorage Persistence](#233-cart-localstorage-persistence)
24. [MoMo Payment Integration](#24-momo-payment-integration)
    - [24.1 MoMoService](#241-momoservice)
    - [24.2 Payment Flow](#242-payment-flow)
25. [RoleGuard Middleware](#25-roleguard-middleware)
26. [Order Tracking & Live Map](#26-order-tracking--live-map)
    - [26.1 OrderTracking View](#261-ordertracking-view)
    - [26.2 7-Step Progress Bar](#262-7-step-progress-bar)
    - [26.3 map.js Shared Module](#263-mapjs-shared-module)
27. [Dashboard Real-time Updates](#27-dashboard-real-time-updates)
    - [27.1 Admin Dashboard SignalR](#271-admin-dashboard-signalr)
    - [27.2 Restaurant Real-time Broadcasts](#272-restaurant-real-time-broadcasts)

---

## 1. Tổng Quan Design System

### 1.1 Design Tokens

Fastship sử dụng **design tokens** thông qua CSS custom properties (`:root` variables) trong file `fastship-design-tokens.css`. File này được load trên TOÀN BỘ 4 layouts: `_LayoutPageHome`, `_LayoutPageAmin`, `_LayoutPageRestaurant`, `_LayoutPageShipper`.

### 1.2 Theme Architecture

| Theme | CSS File(s) | Target Audience | Style |
|-------|-------------|-----------------|-------|
| **Home (Customer)** | `style.css`, `layout-sg.css`, `login.css`, `details.css`, `base.css` + **`fastship-design-tokens.css`** | Khách hàng | Sweetgreen-inspired, modern, card-based |
| **Cart/Checkout** | `style.css` (Ogani) + inline styles + **`fastship-design-tokens.css`** | Khách hàng | Modern Minimalist, 12px radius, Inter font |
| **Restaurant Dashboard** | `style-restaurant.css` (Bootstrap 4.3) + **`fastship-design-tokens.css`** + inline override | Chủ quán | Modern Minimalist, flat KPI, clean sidebar |
| **Shipper Dashboard** | `style-shiper.css` (Bootstrap 4.3) + **`fastship-design-tokens.css`** + inline override | Shipper | Same as Restaurant |
| **Admin Dashboard** | `style-admin.css` (Bootstrap 5) + **`fastship-design-tokens.css`** + inline override | Quản trị viên | Full admin, flat KPI, modern tables |

> ⭐ **NEW v4.0**: `fastship-design-tokens.css` — Global Design System thống nhất cho TOÀN BỘ 4 layouts

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

#### Color Usage Matrix (v4.0)

| CSS Variable | HEX | Usage | Text Contrast |
|-------------|-----|-------|---------------|
| `--fs-green` | `#3CB815` | Buttons, links, active states | White text ✅ |
| `--fs-orange` | `#F65005` | Accent, highlight band | White text ✅ |
| `--fs-dark` | `#1a1a2e` | Heading, body text | — |
| `--fs-muted` | `#6b7280` | Secondary text, labels | — |
| `--fs-muted-soft` | `#9ca3af` | Placeholder text | — |
| `--fs-light` | `#f8f9fa` | Background sections, table headers | — |
| `--fs-light-soft` | `#f3f4f6` | Hover backgrounds | — |
| `--fs-border` | `#e5e7eb` | Borders, dividers | — |
| `--fs-border-soft` | `#f0f0f0` | Subtle borders, table rows | — |
| `--fs-white` | `#ffffff` | Card backgrounds | — |

### 2.2 Design Tokens (Global v4.0)

```css
:root {
    /* ─── Brand Colors ─── */
    --fs-green:      #3CB815;
    --fs-green-dark: #2ea310;
    --fs-green-bg:   rgba(60,184,21,.07);
    --fs-orange:     #F65005;
    --fs-orange-bg:  rgba(246,80,5,.08);

    /* ─── Neutral Palette ─── */
    --fs-dark:       #1a1a2e;
    --fs-muted:      #6b7280;
    --fs-muted-soft: #9ca3af;
    --fs-light:      #f8f9fa;
    --fs-light-soft: #f3f4f6;
    --fs-border:     #e5e7eb;
    --fs-border-soft:#f0f0f0;
    --fs-white:      #ffffff;

    /* ─── Typography ─── */
    --fs-font:       'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    --fs-font-size:  14px;
    --fs-line-height:1.6;

    /* ─── Spacing & Shape ─── */
    --fs-radius:     12px;
    --fs-radius-sm:  8px;
    --fs-radius-lg:  16px;
    --fs-radius-xl:  24px;
    --fs-shadow:     0 4px 20px rgba(0,0,0,.07);
    --fs-shadow-sm:  0 2px 8px rgba(0,0,0,.05);
    --fs-shadow-lg:  0 12px 32px rgba(0,0,0,.1);
    --fs-shadow-btn: 0 4px 12px rgba(60,184,21,.3);

    /* ─── Transitions ─── */
    --fs-transition: .2s ease;
}
```

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
    --fs-topbar-h: 34px; /* v4.2: 38→34 compact */
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

### 3.3 Scrollbar Handling

FastShip có chiến lược scrollbar nhất quán xuyên suốt các theme:

#### 3.3.1 Global Scrollbar Ẩn (Home Theme)

Trang chủ (Customer) ẩn scrollbar toàn cục nhưng vẫn giữ scroll được:

```css
/* layout-sg.css */
html {
    scrollbar-width: none;        /* Firefox */
    -ms-overflow-style: none;     /* IE/Edge legacy */
}
html::-webkit-scrollbar {
    display: none;                /* Chrome, Safari, Edge */
}
```

Điều này tạo cảm giác "app-like" — nội dung cuộn mượt mà không bị thanh cuộn ngang/dọc chiếm diện tích.

#### 3.3.2 Overflow-x: Hidden — Chỉ Container, Không Body

Để tránh scroll ngang do nội dung tràn, `overflow-x: hidden` được đặt trên **container elements**, KHÔNG phải body:

```css
/* ✅ ĐÚNG: Chỉ container */
.container-fluid, .container-xxl, .container {
    max-width: 100%;
    overflow-x: hidden;
}

/* ❌ KHÔNG đặt trên body (gây mất scroll dọc) */
/* body { overflow-x: hidden; } — KHÔNG DÙNG */
```

**Vấn đề đã gặp (Fix v3.5)**: Trước đây `body` cũng bị gán `overflow-x: hidden` thông qua selector `body, .container-fluid, ...`, dẫn đến mất thanh cuộn dọc trên Trang chủ. Đã fix bằng cách tách `body` ra khỏi selector.

#### 3.3.3 Auth Pages — Viewport Wrapper (v3.3)

Trang Login, Signup, Forgot sử dụng kỹ thuật **viewport locking** để loại bỏ scroll rác:

```css
body {
    height: 100vh;
    overflow: hidden;           /* Khóa cứng viewport, cấm scroll toàn trang */
}
.auth-page-wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;
    max-height: 100vh;
    overflow: hidden;
}
.auth-header {
    flex-shrink: 0;             /* Header chiếm chiều cao tự nhiên */
}
.auth-main {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow-y: auto;           /* Chỉ cho phép scroll nội bộ nếu form quá dài (Signup) */
}
```

#### 3.3.4 Perfect Scrollbar (Dashboard Themes)

Admin, Restaurant, Shipper dashboards dùng **Perfect Scrollbar** cho sidebar và scrollable containers:

| Library | Version | Files |
|---------|---------|-------|
| `perfect-scrollbar.css` | Bundle | `wwwroot/Source/Admin/css/`, `Restaurant/`, `Shipper/` |
| `perfect-scrollbar.js` | Bundle | Init via inline JS hoặc deznav-init.js |

```css
/* perfect-scrollbar overrides */
.scrollbar-container {
    max-height: none;
    overflow-x: auto;
    display: flex;
    flex-wrap: nowrap;
    gap: 6px;
    padding: 4px 0;
}
```

#### 3.3.5 iOS Momentum Scrolling

Để scroll mượt trên iOS Safari (DetailRestaurant categories, dashboard tables):

```css
.scrollbar-container, .table-responsive {
    -webkit-overflow-scrolling: touch;
}
```

#### 3.3.6 Navbar Scrollbar Prevention

Ngăn scrollbar không mong muốn xuất hiện cạnh icon giỏ hàng:

```css
.fs-nav .navbar-collapse,
.fs-nav .navbar-nav {
    overflow: visible !important;
}
```

---

---

## 3.4 Header Sticky Fix — Skeleton Overlay Conflict (v3.2)

### 3.4.1 Vấn đề

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

### 3.4.2 Giải pháp

1. **Header z-index**: Tăng từ `1030` lên `10000` (cao hơn skeleton overlay)
2. **Skeleton overlay top**: Thay đổi từ `top: 0` thành `top: calc(var(--fs-nav-h) + var(--fs-topbar-h))` trên desktop và `top: var(--fs-nav-h)` trên mobile
   - Desktop: skeleton bắt đầu từ vị trí **dưới header** (68px + 34px = 102px) (v4.2: topbar 38px→34px)
   - Mobile (< 992px): skeleton bắt đầu từ dưới nav (68px hoặc 60px)

```
AFTER (v3.2):
┌─ HEADER (z-index: 10000) ─────────┐ ← Luôn hiện trên cùng
├─ SKELETON OVERLAY (z-index: 9999) ─┤ ← Bắt đầu dưới header
│  Main content skeleton...           │
└─────────────────────────────────────┘
  → Header luôn visible từ đầu
```

### 3.4.3 Files thay đổi

| File | Thay đổi |
|------|----------|
| `layout-sg.css` | `.fs-header` z-index: 1030 → 10000 |
| `layout-sg.css` | `.fs-skeleton-overlay` top: 0 → `calc(var(--fs-nav-h) + var(--fs-topbar-h))` + responsive override |

### 3.4.4 CSS Code

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
    top: calc(var(--fs-nav-h) + var(--fs-topbar-h)); /* 102px desktop (v4.2: topbar 38→34) */
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

## 3.5 Global Design System v4.0 — `fastship-design-tokens.css`

### 3.5.1 Tổng quan

File `fastship-design-tokens.css` (`~/Source/Shared/css/fastship-design-tokens.css`) là **hệ thống design tokens tập trung** đầu tiên của FastShip, được load trên TOÀN BỘ 4 layouts:

- `_LayoutPageHome.cshtml`
- `_LayoutPageAmin.cshtml`
- `_LayoutPageRestaurant.cshtml`
- `_LayoutPageShipper.cshtml`

**Mục tiêu**: Giải quyết triệt để **Theme Fragmentation** — trước đây Home dùng Sweetgreen, Cart dùng Ogani, Dashboard dùng Bootstrap 4.3 — giờ tất cả đều chia sẻ chung một bộ tokens.

### 3.5.2 Components được định nghĩa

| Component | CSS Class | Key Properties |
|-----------|-----------|---------------|
| Card | `.fs-card` | `border: none; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,.07)` |
| Button Primary | `.fs-btn-primary` | Gradient xanh + box-shadow, hover translateY(-1px) |
| Button Outline | `.fs-btn-outline` | Trong suốt + border 1.5px, hover xanh |
| Button Ghost | `.fs-btn-ghost` | Trong suốt hoàn toàn, hover nền sáng |
| Input | `.fs-input` | height 44px, border-radius 12px, focus ring xanh |
| Select | `.fs-select` | Custom arrow SVG, appearance none |
| Textarea | `.fs-textarea` | border-radius 12px, resize vertical |
| Table | `.fs-table` | Uppercase header 12px, hover row xanh nhạt |
| Badge | `.fs-badge` | border-radius 20px, 5 color variants |
| KPI Card | `.fs-kpi` | padding 20px 24px, hover translateY(-2px) |
| Sidebar Nav | `.fs-sidebar .nav-link` | border-radius 8px, hover/active xanh |
| Modal | `.fs-modal` | border-radius 16px, box-shadow lg |
| Empty State | `.fs-empty-state` | Centered, icon + text + CTA |

### 3.5.3 Dashboard Override (Admin/Restaurant/Shipper)

Mỗi dashboard layout có inline `<style>` override áp dụng CSS variables từ design tokens:

| Element | Before (cũ) | After (v4.0) |
|---------|-------------|-------------|
| KPI Cards | Gradient nền rực rỡ (gradient-1..4) | `background: white; box-shadow: var(--fs-shadow)` |
| KPI Icon | 56px circle với gradient | 48px square với `var(--fs-green-bg)` flat |
| Sidebar items | Nền tối, text trắng | `border-radius: 8px; margin: 2px 8px; hover: green bg` |
| Table headers | Bootstrap default | Uppercase 12px, letter-spacing, `var(--fs-light)` bg |
| Buttons | Bootstrap default | `border-radius: 12px; font-weight: 600;` |
| Form controls | border-radius 4px | `border-radius: 12px; border: 1.5px solid var(--fs-border)` |

### 3.5.4 Files thay đổi

| File | Thay đổi |
|------|----------|
| `fastship-design-tokens.css` | **NEW** — 350+ dòng: :root tokens, base reset, 16 component systems, utility classes, FontAwesome icon protection |
| `_LayoutPageHome.cshtml` | Thêm `<link href="~/Source/Shared/css/fastship-design-tokens.css">` |
| `_LayoutPageAmin.cshtml` | Thêm CSS link + inline override (KPI, sidebar, table, button, form) |
| `_LayoutPageRestaurant.cshtml` | Thêm CSS link + inline override (same pattern) |
| `_LayoutPageShipper.cshtml` | Thêm CSS link + inline override (same pattern) |

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
│ Top Bar (34px) - Phone, Email, Social   │ (v4.2 compact: 38px→34px, font 12.5→11.5px)
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
- Sidebar collapses to hamburger overlay---

## 5. Home Page — Trang chủ Khách hàng (Index.cshtml)

**File**: `Views/Home/Index.cshtml`  
**Layout**: `_LayoutPageHome.cshtml`  
**CSS**: `layout-sg.css`, `style.css`, `fastship-design-tokens.css`  
**Route**: `/Home`  
**Controller**: `HomeController.Index()`

### 5.1 Tổng Quan Layout

Trang chủ là một **single-page scroll** dài với 9 section chính, xếp theo thứ tự từ trên xuống:

```
┌─────────────────────────────────────────────────────┐
│  0. PROMO BAND (34px, dismissible, orange/green bg)│
├─────────────────────────────────────────────────────┤
│  1. HERO CAROUSEL (full-viewport-width, 2 slides)  │
│     Bootstrap carousel-fade + crossfade transition   │
│     Caption: h1 48px, CTA buttons (primary/secondary)│
├─────────────────────────────────────────────────────┤
│  2. STATS ROW (4 stats, data thật từ DB)           │
│     Icon + counter animation (IntersectionObserver)  │
├─────────────────────────────────────────────────────┤
│  3. RE-ORDER SECTION (chỉ hiển thị khi ĐÃ ĐĂNG NHẬP)│
│     4 quick-link cards: đơn gần nhất, khám phá,     │
│     giỏ hàng, lịch sử                               │
├─────────────────────────────────────────────────────┤
│  4. RESTAURANT LIST                                 │
│     FilterBar ViewComponent + Category pills +      │
│     Product cards grid (col-xl-3 col-lg-4 col-6)    │
│     + Empty state + load more                       │
├─────────────────────────────────────────────────────┤
│  5. APRIORI AI COMBO (gợi ý từ AI, data ViewBag)   │
│     Product cards mini + AI badge + "Khám phá thêm" │
├─────────────────────────────────────────────────────┤
│  6. HIGHLIGHT BAND (marketing callout)              │
│     2-col: text trái + 4-grid icons phải            │
├─────────────────────────────────────────────────────┤
│  7. HOW IT WORKS (3-step process cards)             │
│     Icon circle + title + description + CTA btn     │
├─────────────────────────────────────────────────────┤
│  FOOTER (trong _LayoutPageHome.cshtml)              │
│  Newsletter + Links + Social + Copyright            │
└─────────────────────────────────────────────────────┘
```

### 5.2 Hero Carousel Section

**Bootstrap 5 Carousel** với 2 slides, full-width, dùng class `carousel-fade` để crossfade effect:

| Element | Mô tả | Style |
|---------|-------|-------|
| **Container** | `.container-fluid.p-0.wow.fadeIn` | Full-width, padding 0, WOW.js animation |
| **Carousel** | `#header-carousel.carousel.slide` | Bootstrap carousel với `carousel-fade` class |
| **Slide 1** | Banner 1: "Ẩm thực Sài Gòn — giao tận cửa trong 30 phút" | `w-100`, alt text tiếng Việt |
| **Slide 2** | Banner 2: "Hơn 200 quán ăn ngon — đặt trong 1 phút" | `w-100`, 2 CTA buttons |
| **Caption** | `.carousel-caption` | Left-aligned (`row justify-content-start`), `col-lg-7 col-11` |
| **Heading** | `h1.display-2` | 48px desktop, `fw-800`, `animated` class |
| **CTA Buttons** | `.btn-primary.rounded-pill` (green) + `.btn-secondary.rounded-pill` (orange) | `py-2 py-sm-3 px-4 px-sm-5`, responsive |
| **Controls** | `.carousel-control-prev/next` | Bootstrap chevron arrows |
| **Indicators** | `.carousel-indicators.d-sm-none` | Chỉ hiển thị trên mobile (< 576px) |

**Carousel Animation**:
- Caption h1: `slideInDown` 0.7s (0.4s mobile)
- Caption buttons: `slideInUp` 0.7s (0.4s mobile)
- Crossfade transition: 0.6s CSS transition
- Re-trigger sau skeleton fadeOut (callback trong `main.js`)

### 5.3 Promo Band

Thanh thông báo nằm **ngay trên carousel**, đầy đủ width, có thể đóng:

```
┌──────────────────────────────────────────────────────────────┐
│ 🏷 Mùa hè đặc biệt — Giảm 20% cho đơn đầu tiên!  [✕]      │
│   Khám phá ngay →                                              │
└──────────────────────────────────────────────────────────────┘
     height: 34px (v4.2 compact); font-size: 11.5px
     background: var(--fs-orange) hoặc var(--fs-green)
     color: #fff; display: flex; align-items: center
```

| Component | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Band** | `.fs-promo-band` | `position:relative`, flex, align-items center, padding, display none khi dismiss |
| **Dismiss button** | `.fs-promo-dismiss` | `background:none; border:none; color:#fff; cursor:pointer; margin-left:auto` |
| **Link** | Inline `<a>` | `text-decoration:underline; font-weight:600; white-space:nowrap` |
| **Fade-out** | `.fade-out` class | `opacity:0; transition: opacity .3s ease` (JS toggle) |

**JS Interaction**: Click [✕] → add `fade-out` class → 300ms later → `display:none`

### 5.4 Stats Row

4 thống kê **thực tế từ database** (không phải mock data):

```
┌──────────────────────────────────────────────────────────────┐
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────┐ │
│  │ 🏪         │  │ 🏍️          │  │ 🛍️          │  │ ⭐     │ │
│  │ 150+ quán  │  │ 30′ giao   │  │ 5,000+ đơn │  │ 4.5★  │ │
│  │  đối tác   │  │  trung bình│  │  trong tháng│  │ TB     │ │
│  └────────────┘  └────────────┘  └────────────┘  └────────┘ │
└──────────────────────────────────────────────────────────────┘
     grid: 4 columns (responsive → 2 columns mobile)
     gap: 24px; padding: 24px 0
```

| Component | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Container** | `.fs-stats-row` | `display:grid; grid-template-columns:repeat(4,1fr); gap:24px;` mobile: `repeat(2,1fr)` |
| **Stat item** | `.fs-stat-item` | `text-align:center; padding:20px 16px; border-radius:var(--fs-radius); background:var(--fs-white)` |
| **Icon** | `.fs-stat-icon` | `font-size:28px; color:var(--fs-green); margin-bottom:8px` (Font Awesome) |
| **Number** | `.stat-num` | `font-size:32px; font-weight:800; color:var(--fs-dark)` |
| **Counter** | `.fs-counter` | `data-count="N"` — IntersectionObserver animation từ 0→N |
| **Label** | `.stat-label` | `font-size:14px; color:var(--fs-muted); text-transform:uppercase; letter-spacing:0.5px` |

**Counter Animation**: `<span class="fs-counter" data-count="@ViewBag.TotalRestaurants">0</span>` — JS IntersectionObserver đếm từ 0 lên giá trị thật (WOW.js không dùng cho counter).

**Responsive**:
- Desktop (>768px): 4 cột grid
- Mobile (<768px): 2 cột grid, `border-bottom` thay `border-right`

### 5.5 Re-order Section (dành cho user đã login)

Chỉ hiển thị khi `currentUser != null` — 4 quick-link cards dạng horizontal:

```
┌──────────────────────────────────────────────────────────────┐
│ 🔄 Chào mừng trở lại, <username>!       [Lịch sử đơn hàng →]│
│ Đặt lại món yêu thích của bạn chỉ với một chạm.              │
├──────────────────────────────────────────────────────────────┤
│ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐│
│ │  🔄        │ │  🧭        │ │  🛍️        │ │  🕐         ││
│ │ Đơn gần    │ │ Khám phá   │ │ Giỏ hàng   │ │ Lịch sử   ││
│ │ nhất       │ │ món mới    │ │            │ │            ││
│ │ Đặt lại    │ │ Gợi ý AI   │ │ Tiếp tục   │ │ Theo dõi  ││
│ │ dễ dàng    │ │            │ │  đặt món   │ │  đơn hàng  ││
│ └────────────┘ └────────────┘ └────────────┘ └────────────┘│
└──────────────────────────────────────────────────────────────┘
```

| Component | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Section** | `.fs-reorder-section` | `padding:24px 0`, WOW fadeInUp |
| **Header row** | `.row.align-items-center` | Title + subtitle bên trái, button "Lịch sử" bên phải |
| **Card** | `.fs-reorder-item` | `display:flex; align-items:center; gap:12px; padding:12px 16px; border-radius:12px;` |
| **Card icon** | `.reorder-img` | `width:44px; height:44px; border-radius:10px; flex-shrink:0` (bg pastel màu khác nhau) |
| **Card info** | `.reorder-info` | `flex:1` |
| **Card name** | `.reorder-name` | `font-size:14px; font-weight:600; color:var(--fs-dark)` |
| **Card sub** | `.reorder-restaurant` | `font-size:12px; color:var(--fs-muted)` |
| **Arrow** | `.reorder-btn` | `color:var(--fs-muted-soft)` |

### 5.6 Restaurant List Section

Phần chính của trang chủ — danh sách quán ăn với filter + grid cards:

```
┌── max-width: 1200px; margin: auto ────────────────────────────┐
│                                                               │
│  [Section Title] "Quán ăn nổi bật tại TP.HCM"                │
│  [Section Subtitle] "Khám phá văn hoá ẩm thực Sài Gòn..."   │
│  [Xem tất cả →] (hidden mobile, visible desktop)              │
│                                                               │
│  ┌── FilterBar ViewComponent (Dual Filter) ──────────────┐   │
│  │  [🔍 Tìm kiếm...] [🍽 Tất cả] [⭐ Đánh giá] [💰 Giá] │   │
│  │  Selected chips: [Đánh giá ⬆] [Giá ⬇] [✕]            │   │
│  └────────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌── Category Pills ─────────────────────────────────────┐   │
│  │  [🍽 Tất cả] [🍚 Cơm] [🍜 Phở] [🥘 Lẩu] [🍝 Bún]...  │   │
│  │  .fs-category-row: flex-wrap (desktop) / scroll ngang  │   │
│  │  .active pill: bg green, text white                     │   │
│  └────────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌── Restaurant Cards Grid (row g-3 g-md-4 mt-1) ────────┐  │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐     │  │
│  │  │  img    │ │  img    │ │  img    │ │  img    │     │  │
│  │  │ 4:3 ar  │ │ 4:3 ar  │ │ 4:3 ar  │ │ 4:3 ar  │     │  │
│  │  │ Tên quán│ │ Tên quán│ │ Tên quán│ │ Tên quán│     │  │
│  │  │ Địa chỉ │ │ Địa chỉ │ │ Địa chỉ │ │ Địa chỉ │     │  │
│  │  │ ⭐ 4.5  │ │ ⭐ 4.2  │ │ ⭐ 4.8  │ │ ⭐ 4.3  │     │  │
│  │  │ 30 bl   │ │ 15 bl   │ │ 45 bl   │ │ 22 bl   │     │  │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘     │  │
│  │  grid: col-xl-3 col-lg-4 col-6                        │  │
│  │  (4 cols desktop, 3 cols tablet, 2 cols mobile)      │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  [Xem tất cả] (mobile only, .d-md-none)                      │
└───────────────────────────────────────────────────────────────┘
```

#### 5.6.1 Restaurant Card (Product Item)

```
┌── .product-item ────────────────────────────────────────────┐
│  ┌── .product-img-wrap (aspect-ratio 4/3, overflow hidden) ┐│
│  │  <img> loading="lazy" onerror="fallback"               ││
│  │  hover: scale(1.08) + overlay gradient                  ││
│  │                                                         ││
│  │  Badge: [🔴 Đang thịnh hành] (.fs-order-count-badge)    ││
│  │  Badge: [🤖 AI] (.fs-ai-badge, cho combo section)      ││
│  └─────────────────────────────────────────────────────────┘│
│  ┌── .product-body ────────────────────────────────────────┐│
│  │  .product-title: 16px/600, line-clamp 2                ││
│  │  .product-address: 12px, muted, map-marker icon        ││
│  └─────────────────────────────────────────────────────────┘│
│  ┌── .product-footer ──────────────────────────────────────┐│
│  │  ⭐ 4.5 (left)          💬 30 bình luận (right)        ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
     border-radius: 12px; overflow: hidden;
     box-shadow: var(--fs-shadow-sm);
     transition: transform .3s ease, box-shadow .3s ease;
     hover: translateY(-6px) + box-shadow elevated
     cursor: pointer (toàn card là link)
```

| Thành phần | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Card container** | `.product-item` | `background:#fff; border-radius:12px; overflow:hidden; box-shadow; hover translateY(-6px)` |
| **Image wrapper** | `.product-img-wrap` | `overflow:hidden; aspect-ratio:4/3; position:relative` |
| **Image** | `<img>` | `width:100%; height:100%; object-fit:cover; transition:transform .5s ease` + `hover scale(1.08)` |
| **Trending badge** | `.fs-order-count-badge` | `position:absolute; top:12px; left:12px; bg:rgba(0,0,0,.65); color:#fff; font-size:11px; padding:4px 10px; border-radius:20px;` |
| **Trending dot** | `.fs-trending-dot` | `display:inline-block; width:6px; height:6px; background:#ea4335; border-radius:50%; animation:pulse 1.5s infinite` |
| **Body** | `.product-body` | `padding:16px 16px 8px` |
| **Title** | `.product-title` | `font-size:16px; font-weight:600; color:var(--fs-dark); line-height:1.3; overflow:hidden; text-overflow:ellipsis; white-space:nowrap` |
| **Address** | `.product-address` | `font-size:12px; color:var(--fs-muted); margin-top:4px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis` |
| **Footer** | `.product-footer` | `display:flex; justify-content:space-between; align-items:center; padding:8px 16px 14px; border-top:1px solid var(--fs-border-soft)` |
| **Star rating** | `.pf-cell .fa-star` | `color:#f39c12; font-size:13px; margin-right:4px; display:inline-flex; align-items:center; gap:4px` |
| **Comment count** | `.pf-cell` | `font-size:12px; color:var(--fs-muted)` |

#### 5.6.2 Category Pills

```
┌── .fs-category-row ──────────────────────────────────────────┐
│  [🍽 Tất cả] [🍚 Cơm] [🍜 Phở] [🥘 Lẩu] [🍝 Bún] [🥗 Salad]│
│  flex-wrap: wrap (desktop) → overflow-x: auto (mobile)       │
│  gap: 10px; padding: 12px 0                                  │
└──────────────────────────────────────────────────────────────┘
```

| State | CSS | Mô tả |
|-------|-----|-------|
| **Default** | `.fs-category-pill` | `padding:8px 18px; border-radius:30px; font-size:13px; font-weight:500; border:1.5px solid var(--fs-border); background:#fff; color:var(--fs-muted); transition:all .2s ease; text-decoration:none; display:inline-flex; align-items:center;` |
| **Hover** | `&:hover` | `border-color:var(--fs-green); color:var(--fs-green); background:var(--fs-green-bg)` |
| **Active** | `.active` | `background:var(--fs-green); color:#fff; border-color:var(--fs-green); font-weight:600` |

#### 5.6.3 Empty State

Khi không có quán ăn nào khớp filter:
```
┌──────────────────────────────────────────────┐
│           🥄 (fa fa-utensils 48px)            │
│    "Không tìm thấy quán ăn phù hợp"           │
│    "Thử tìm kiếm với từ khoá khác..."          │
│                                              │
│    [Xem tất cả quán] (.btn-success.rounded-pill)│
└──────────────────────────────────────────────┘
```

### 5.7 Apriori AI Combo Section

Gợi ý món ăn từ thuật toán Apriori (dữ liệu từ `ViewBag.AprioriCombo`):

```
┌── Section: "🤖 Gợi ý Combo từ AI hôm nay" ──────────────────┐
│  "Dựa trên phân tích hàng ngàn đơn hàng thực tế..."         │
│                                                               │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐     │
│  │ img  │ │ img  │ │ img  │ │ img  │ │ img  │ │ img  │     │
│  │ 🤖AI │ │ 🤖AI │ │ 🤖AI │ │ 🤖AI │ │ 🤖AI │ │ 🤖AI │     │
│  │ Tên  │ │ Tên  │ │ Tên  │ │ Tên  │ │ Tên  │ │ Tên  │     │
│  │ Quán │ │ Quán │ │ Quán │ │ Quán │ │ Quán │ │ Quán │     │
│  │ Giá  │ │ Giá  │ │ Giá  │ │ Giá  │ │ Giá  │ │ Giá  │     │
│  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘     │
│  grid: col-xl-2 col-lg-3 col-md-4 col-6                     │
└──────────────────────────────────────────────────────────────┘
```

| Component | CSS | Chi tiết |
|-----------|-----|----------|
| **Section title** | `.fs-section-title-sm` | `18px/700`, icon robot `#f39c12` |
| **AI Badge** | `.fs-ai-badge` | `position:absolute; top:8px; right:8px; bg:rgba(243,156,18,.9); color:#fff; font-size:10px; padding:2px 8px; border-radius:20px; font-weight:600; backdrop-filter:blur(4px)` |
| **Price** | `.fs-price` | `font-size:13px; font-weight:600; color:var(--fs-green); margin-top:4px` |
| **Body compact** | `.product-body-compact` | `padding:12px 14px 10px` |

### 5.8 Highlight Band

Band marketing full-width với nền gradient/ảnh:

```
┌── .fs-highlight-band ────────────────────────────────────────┐
│  ┌── CỘT TRÁI (col-lg-7) ──┐ ┌── CỘT PHẢI (col-lg-5) ────┐│
│  │ [MÙA HÈ SÀI GÒN 2026]   │ │ ┌──────┐ ┌──────┐        ││
│  │ badge (12px, green bg)   │ │ │🍴Cơm │ │☕Trà  │        ││
│  │                         │ │ │Bún   │ │Cà phê│        ││
│  │ "Hôm nay có gì ngon?"    │ │ └──────┘ └──────┘        ││
│  │ h2 text-white            │ │ ┌──────┐ ┌──────┐        ││
│  │                         │ │ │💼Cơm  │ │🏍️30′ │        ││
│  │ [Khám phá thực đơn →]   │ │ │VP    │ │Giao  │        ││
│  │ btn-outline-light       │ │ └──────┘ └──────┘        ││
│  └─────────────────────────┘ └────────────────────────────┘│
└──────────────────────────────────────────────────────────────┘
     max-width: 100%; padding: 60px 0;
     background: linear-gradient(135deg, #1a1a2e, #16213e)
```

| Component | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Section** | `.fs-highlight-band` | `background:linear-gradient(135deg,#1a1a2e,#16213e); color:#fff; padding:60px 0` |
| **Badge** | `.fs-section-badge` | `display:inline-block; padding:4px 14px; border-radius:20px; background:var(--fs-green); color:#fff; font-size:12px; font-weight:600; letter-spacing:1px; margin-bottom:12px` |
| **Heading** | `h2.text-white` | `font-size:32px; font-weight:800` |
| **Paragraph** | Inline `<p>` | `font-size:15px; opacity:.8; line-height:1.7` |
| **Grid** | `.fs-highlight-grid` | `display:grid; grid-template-columns:1fr 1fr; gap:12px` |
| **Grid item** | `.fs-highlight-grid-item` | `bg:rgba(255,255,255,.1); border-radius:12px; padding:20px 16px; text-align:center; backdrop-filter:blur(8px)` |
| **Grid icon** | `.hg-icon` | `font-size:32px; margin-bottom:8px; color:var(--fs-green)` |
| **Grid label** | `.hg-label` | `font-size:11px; font-weight:500; text-transform:uppercase; letter-spacing:0.5px; opacity:.8` |

### 5.9 How It Works Section

3-step process cards, giải thích quy trình đặt món:

```
┌── "Đặt món dễ như thế này" ─────────────────────────────────┐
│  "Ba bước đơn giản để có bữa ăn ngon tại nhà"               │
│                                                               │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │    🔍        │ │    🛍️        │ │    🏍️        │         │
│  │              │ │              │ │              │         │
│  │ 1. Chọn quán │ │ 2. Thêm vào  │ │ 3. Nhận hàng │         │
│  │              │ │    giỏ       │ │              │         │
│  │ Duyệt hàng   │ │ Chọn món,   │ │ Shipper giao │         │
│  │ trăm quán    │ │ điều chỉnh  │ │ tận tay      │         │
│  │ tại TP.HCM   │ │ số lượng    │ │ trong 30′    │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│                                                               │
│  [Đăng ký miễn phí →]                                         │
└───────────────────────────────────────────────────────────────┘
```

| Component | CSS Class | Chi tiết |
|-----------|-----------|----------|
| **Section** | `.fs-how-section` | `padding:60px 0; background:var(--fs-light)` |
| **Title** | `.fs-section-title` | `font-size:28px; font-weight:700; color:var(--fs-dark)` |
| **Subtitle** | `.fs-section-sub` | `font-size:15px; color:var(--fs-muted); max-width:500px; margin:8px auto 0` |
| **Card** | `.fs-how-card` | `text-align:center; padding:32px 24px; border-radius:var(--fs-radius); background:var(--fs-white); box-shadow:var(--fs-shadow-sm); transition:transform .3s ease; hover:translateY(-4px)` |
| **Icon** | `.fs-how-icon` | `width:72px; height:72px; border-radius:50%; background:var(--fs-green-bg); display:flex; align-items:center; justify-content:center; margin:0 auto 16px; font-size:28px; color:var(--fs-green)` |
| **Step title** | `h5` | `font-size:16px; font-weight:600; margin-bottom:8px` |
| **Value prop** | `.fs-how-value` | `font-size:13px; color:var(--fs-muted); line-height:1.7` |
| **Highlight** | `.fs-how-highlight` | `font-weight:600; color:var(--fs-dark)` |
| **CTA** | `.btn-success.rounded-pill` | `font-size:15px; padding:12px 40px; box-shadow:var(--fs-shadow-btn)` |

### 5.10 Scroll-Reveal Animations

Trang chủ sử dụng **IntersectionObserver** (vanilla JS, không WOW.js) cho hiệu ứng scroll-reveal:

```html
<div class="fs-reveal" style="--fs-i:1">
    <!-- Nội dung sẽ fade-in + slide-up khi xuất hiện trong viewport -->
</div>
```

| Attribute | Giá trị | Hiệu ứng |
|-----------|---------|----------|
| `.fs-reveal` | class | `opacity:0; transform:translateY(30px); transition:all .6s cubic-bezier(.25,.46,.45,.94)` |
| `.fs-reveal.revealed` | class (JS thêm) | `opacity:1; transform:translateY(0)` |
| `--fs-i` | Số thứ tự (1, 2, 3...) | Stagger delay: `transition-delay: calc(var(--fs-i) * 80ms)` |
| `.fs-counter` | `data-count="N"` | Đếm từ 0→N khi section vào viewport (IntersectionObserver) |

**JS Implementation** (in `main.js` hoặc inline):
```javascript
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('revealed');
            // Trigger counter animation if .fs-counter exists
            const counter = entry.target.querySelector('.fs-counter');
            if (counter) animateCounter(counter);
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.1 });

document.querySelectorAll('.fs-reveal').forEach(el => observer.observe(el));
```

### 5.11 Layout Flow Summary

```
DOM load → Skeleton overlay hiện (0ms)
    │
    ├── HTML render: Promo → Hero → Stats → Re-order → Categories → Cards → AI → Band → How
    │
    ├── 100ms delay → Skeleton fadeOut (250ms)
    │   → OwlCarousel re-trigger + WOW.js init
    │   → Carousel caption animation (slideInDown/Up)
    │
    └── User scroll → IntersectionObserver:
        → .fs-reveal elements: fade-in staggered
        → .fs-counter elements: đếm từ 0→N
        → Header shadow: toggle .scrolled khi scroll >10px
```

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
│ [Tất cả] [Cơm] [Phở…]│ ← scroll ngang (sticky)
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
│                      │
│         ┌────┐       │ ← FAB + Badge
│         │ ☰  │       │
│         │ 5  │       │
│         └────┘       │
└──────────────────────┘
```

### 6.5 Bottom Sheet + FAB + Sticky Category Bar (v3.3)

#### 6.5.1 Vấn đề

Thanh danh mục món ăn trên mobile dạng scroll ngang (`overflow-x: auto`). Với quán có >6 danh mục, người dùng phải vuốt ngang liên tục, mỏi tay và mất góc nhìn tổng quan.

#### 6.5.2 Giải pháp kết hợp: Sticky Bar + FAB + Bottom Sheet

Hệ thống 3 lớp điều hướng danh mục:

```
LỚP 1 — STICKY CATEGORY BAR (luôn visible khi scroll)
┌─────────────────────────────────────────────┐
│ [Tất cả] [Cơm] [Phở] [Lẩu] [Bún] ... [⋮]    │ ← scroll ngang
│ position: sticky; top: 68px (60px mobile)    │
│ z-index: 50 + box-shadow khi scroll >100px   │
└─────────────────────────────────────────────┘

LỚP 2 — FLOATING ACTION BUTTON (góc dưới phải)
                ┌────┐
                │ ☰  │  ← 56px (48px mobile)
                │ 5  │  ← Badge: tổng số danh mục
                └────┘
         bottom:24px; right:16px; z-index:999

LỚP 3 — BOTTOM SHEET (khi bấm FAB)
┌──────────────────────────────────────────────┐
│  ──── ──── ──── (drag handle, 32×4px)       │
│  📂 Danh mục món ăn                  [✕]     │
├──────────────────────────────────────────────┤
│  🍽 Tất cả                                    │
│  🍚 Cơm                                       │
│  🍜 Phở                                       │
│  🥘 Lẩu                          ← active    │
│  🍝 Bún                                       │
│  🥗 Salad                           60% maxH │
├──────────────────────────────────────────────┤
│ (scroll nội bộ nếu nhiều danh mục)            │
└──────────────────────────────────────────────┘
    ↓ Click item → đóng sheet + navigate đến danh mục đó
```

#### 6.5.3 Chi tiết kỹ thuật

**Sticky Category Bar**:
```css
.menu-restaurant-category.is-sticky {
    position: sticky;
    top: 68px; /* dưới header */
    z-index: 50;
    background: #fff;
    border-bottom: 1px solid #e5e7eb;
    box-shadow: 0 2px 8px rgba(0,0,0,.05);
    padding: 0 16px;
    margin: 0 -16px; /* full-width visual */
}
.menu-restaurant-category.is-sticky.scrolled {
    box-shadow: 0 4px 16px rgba(0,0,0,.1); /* shadow khi scroll >100px */
}
@media (max-width: 576px) {
    .menu-restaurant-category.is-sticky { top: 60px; }
}
```

**FAB Button**:
```css
.fs-category-fab {
    position: fixed;
    bottom: 24px; right: 16px;
    z-index: 999;
    width: 56px; height: 56px;
    border-radius: 50%;
    background: linear-gradient(135deg, #3CB815, #2ea310);
    box-shadow: 0 4px 16px rgba(60,184,21,.35);
    cursor: pointer;
    display: none; /* desktop: hidden */
    align-items: center;
    justify-content: center;
}
@media (max-width: 768px) { .fs-category-fab { display: flex; } }
@media (max-width: 576px) { .fs-category-fab { width: 48px; height: 48px; bottom: 16px; } }
```

**Bottom Sheet**:
```css
.fs-bottom-sheet {
    position: fixed;
    left: 0; right: 0; bottom: 0;
    z-index: 9999;
    max-height: 60vh;
    background: #fff;
    border-radius: 20px 20px 0 0;
    transform: translateY(100%);
    transition: transform .35s cubic-bezier(.32,.72,0,1);
    display: flex;
    flex-direction: column;
}
.fs-bottom-sheet.active { transform: translateY(0); }

.fs-bottom-sheet-overlay {
    position: fixed; inset: 0;
    z-index: 9998;
    background: rgba(0,0,0,.45);
    opacity: 0; visibility: hidden;
    transition: opacity .3s ease, visibility .3s ease;
}
.fs-bottom-sheet-overlay.active { opacity: 1; visibility: visible; }
```

#### 6.5.4 Category Icon Mapping (getCategoryIcon)

Hàm JS tự động gán icon emoji dựa trên tên danh mục, xử lý tiếng Việt có dấu bằng `normalize('NFD')`:

| Tên danh mục | Icon | Logic match |
|-------------|------|-------------|
| Tất cả | 🍽 | `n === 'tat ca'` |
| Cơm | 🍚 | `includes('com')` |
| Phở | 🍜 | `includes('pho')` |
| Mì, Bún | 🍝 | `includes('mi') || includes('bun')` |
| Lẩu | 🥘 | `includes('lau')` |
| Bánh | 🥟 | `includes('banh')` |
| Salad, Rau | 🥗 | `includes('salad')` |
| Sushi | 🍣 | `includes('sushi')` |
| Thịt | 🥩 | `includes('thit')` |
| Cá, Hải sản | 🦐 | `includes('ca')` |
| Trà, Cafe | 🧋 | `includes('tra')` |
| Nước, Uống | 🥤 | `includes('nuoc')` |
| Tráng miệng | 🍰 | `includes('trang mieng')` |
| Khai vị | 🥟 | `includes('khai vi')` |
| Khác | 📂 | default |

```javascript
function stripDia(s) {
    return s.toLowerCase()
        .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
        .replace(/d/g, 'd');
}
```

#### 6.5.5 Files thay đổi

| File | Thay đổi |
|------|----------|
| `DetailRestaurant.cshtml` | CSS sticky bar + FAB + Bottom Sheet; HTML FAB + Bottom Sheet; JS buildCategorySheet(), open/closeCategorySheet(), getCategoryIcon() |

---

### 6.6 Chi Tiết Sản Phẩm (ChiTietSanPham)

**File**: `Views/Home/ChiTietSanPham.cshtml`  
**Layout**: `_LayoutPageHome.cshtml`  
**CSS**: `details.css`, `base.css` + inline styles  
**Route**: `/Home/ChiTietSanPham/{id}`

#### 6.6.1 Layout Desktop

Trang chi tiết sản phẩm (món ăn) với hero section 2 cột + similar items grid + reviews:

```
┌───────────── max-width: 1200px; margin: 100px auto 40px ──────────────┐
│                                                                       │
│  Breadcrumb: Trang chủ › Quán ăn › Tên món (13px, flex-wrap)         │
│                                                                       │
│  ┌─────────── HERO SECTION (flex, gap:40px, padding:32px) ─────────┐ │
│  │ ┌── CỘT TRÁI (flex:0 0 420px) ──┐ ┌── CỘT PHẢI (flex:1) ────┐ │ │
│  │ │ ┌───────────────────────────┐ │ │ [🏪 Tên quán] (link quán)  │ │ │
│  │ │ │        IMG 4:3            │ │ │                            │ │ │
│  │ │ │  aspect-ratio: 4/3        │ │ │ 🍕 Tên món (26px, 800)   │ │ │
│  │ │ │  hover: scale(1.05)       │ │ │ 🏷 Danh mục (12px pill)    │ │ │
│  │ │ │                           │ │ │                            │ │ │
│  │ │ │  Badge: -20% (góc TL)     │ │ │ Mô tả (15px, border-left  │ │ │
│  │ │ │  Badge: Đã mua (góc TR)   │ │ │ 4px green, bg light)      │ │ │
│  │ │ └───────────────────────────┘ │ │                            │ │ │
│  │ └─────────────────────────────┘ │ │ 💰 Giá: 35.000đ (32px,    │ │ │
│  │                                 │ │     đỏ, 800)               │ │ │
│  │                                 │ │     Giá cũ: 45.000đ (gạch │ │ │
│  │                                 │ │     ngang)                 │ │ │
│  │                                 │ │                            │ │ │
│  │                                 │ │ 🎯 Size variants (chips)  │ │ │
│  │                                 │ │ [M - 35k✓] [L - 45k]      │ │ │
│  │                                 │ │ [XL - 55k]                 │ │ │
│  │                                 │ │                            │ │ │
│  │                                 │ │ 🛒 Add to cart area (bg    │ │ │
│  │                                 │ │     light, border-radius)  │ │ │
│  │                                 │ │ [−][1][+] [🛒 Thêm vào    │ │ │
│  │                                 │ │          giỏ hàng]         │ │ │
│  │                                 │ │                            │ │ │
│  │                                 │ │ ✅ Còn hàng | 👍 100+ đã   │ │ │
│  │                                 │ │ đặt (13px, muted)          │ │ │
│  │                                 └──────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                                                                       │
│  ┌── CÙNG QUÁN (similar items grid) ──────────────────────────────┐  │
│  │  Các món khác từ <Tên quán>                                     │  │
│  │  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐                    │  │
│  │  │ img140 │ │ img140 │ │ img140 │ │ img140 │                    │  │
│  │  │ Tên món│ │ Tên món│ │ Tên món│ │ Tên món│                    │  │
│  │  │ 35kđ   │ │ 45kđ   │ │ 50kđ   │ │ 30kđ   │                    │  │
│  │  └────────┘ └────────┘ └────────┘ └────────┘                    │  │
│  │  grid: repeat(auto-fill, minmax(220px, 1fr))                    │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌── ĐÁNH GIÁ (reviews, paginated) ──────────────────────────────┐   │
│  │  ⭐ Đánh giá             4.5 ★★★★☆ (30 đánh giá)              │   │
│  │  ┌────────────────────────────────────────────────────────┐   │   │
│  │  │ [LK] Linh Ka             ★★★★★  10/07/2026              │   │   │
│  │  │ "Pizza ngon, đóng gói cẩn thận, giao nhanh!"            │   │   │
│  │  │ 🏷 Pizza hải sản                                        │   │   │
│  │  └────────────────────────────────────────────────────────┘   │   │
│  │  ┌────────────────────────────────────────────────────────┐   │   │
│  │  │ [MT] Minh Tú             ★★★★☆  09/07/2026              │   │   │
│  │  │ "Ngon nhưng hơi mặn"                                    │   │   │
│  │  │ 🏷 Pizza bò băm                                         │   │   │
│  │  └────────────────────────────────────────────────────────┘   │   │
│  │  [Xem thêm]                                                  │   │
│  └──────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────┘
```

#### 6.6.2 Layout Mobile (< 900px)

```
┌────────────────────────────────┐
│ Breadcrumb: Trang chủ › Quán…  │
├────────────────────────────────┤
│ ┌────────────────────────────┐ │
│ │      IMG (16:9/4:3)       │ │
│ │  Badge: -20% | Đã mua      │ │
│ └────────────────────────────┘ │
│ 🏪 Tên quán                    │
│ 🍕 Tên món (22px/18px)        │
│ Mô tả (14px)                   │
│ 💰 Giá: 35.000đ (26px/22px)   │
│                                │
│ Size: [M] [L] [XL]            │
│                                │
│ [−] [1] [+]                    │
│ [🛒 Thêm vào giỏ hàng] (full) │
│                                │
├════════════════════════════════┤
│ Các món khác                   │
│ ┌─────┐ ┌─────┐               │
│ │ img │ │ img │               │
│ │ Tên  │ │ Tên  │               │
│ └─────┘ └─────┘               │
├════════════════════════════════┤
│ ⭐ Đánh giá (1 cột)           │
└────────────────────────────────┘
```

#### 6.6.3 Các thành phần chính

| Thành phần | CSS Class / Selector | Mô tả |
|-----------|---------------------|-------|
| **Breadcrumb** | `.fs-breadcrumb` | flex, gap 8px, 13px, link xanh dương |
| **Hero section** | `.pd-hero` | flex row, gap 40px, padding 32px, shadow, border-radius 16px |
| **Image wrapper** | `.pd-image-wrap` | flex 0 0 420px, aspect-ratio 4/3, border-radius 12px, hover scale(1.05) |
| **Badge** | `.pd-badge` | absolute top-left, red/warning/green variants, box-shadow, 12px font |
| **Link quán** | `.pd-restaurant-link` | inline-flex, 13px, bg light, rounded pill, hover green |
| **Tên món** | `.pd-name` | 26px/800/font dark, line-height 1.3 |
| **Mô tả** | `.pd-desc` | 15px, border-left 4px green, bg light, border-radius 10px |
| **Price area** | `.pd-price-area` | flex, align-items baseline, flex-wrap |
| **Giá hiện tại** | `.pd-current-price` | 32px/800/đỏ, .unit sub 16px/400 |
| **Giá cũ** | `.pd-old-price` | 18px, text-decoration line-through, muted-soft |
| **Size chip** | `.pd-variant-chip` | border 2px, border-radius 24px, padding 8px 20px, active: bg green |
| **Add to cart area** | `.pd-cart-area` | bg light, border-radius 12px, padding 20px, flex-wrap |
| **Quantity** | `.pd-qty` | flex, border 1.5px, border-radius 10px, button 40×40px, input 56×40px |
| **Add button** | `.pd-btn-add` | gradient green, border-radius 10px, 15px/700, hover translateY(-1px) |
| **Similar grid** | `.pd-similar-grid` | grid auto-fill, minmax 220px, gap 16px |
| **Similar card** | `.pd-similar-card` | border 1.5px, border-radius 12px, hover shadow + translateY(-2px) |
| **Review card** | (inline) | border 1.5px, border-radius 12px, padding 16px, avatar 36px circle |
| **Load more** | `#pdLoadMoreWrap` | text-align center, button border 2px green, border-radius 30px |

#### 6.6.4 JS Interactions

| Function | Trigger | Mô tả |
|----------|---------|-------|
| `selectPdSize(btn)` | Click size chip | highlight chip active, cập nhật `#pdMaMonAn` + giá hiển thị |
| `adjustQty(delta)` | Click −/+ | Tăng/giảm số lượng (1-99), blur validate |
| `addToCartPd()` | Click add button | AJAX `ApiThemMonAn` hoặc `FastShipCart.addUnauth` (anonymous) |
| `pdLoadReviews(reset)` | DOM ready, load more | AJAX `GetReviews` (page, pageSize), render review cards |
| `pdLoadMore()` | Click "Xem thêm" | Tăng page + `pdLoadReviews(false)` append |

#### 6.6.5 Flow

```
SanPham (grid)              DetailRestaurant (menu)
     │                            │
     └──────────┬─────────────────┘
                ▼
     ┌─────────────────────┐
     │ ChiTietSanPham      │
     │  breadcrumb ← quán  │
     │  Ảnh + info         │
     │  Size variant chip  │
     │  ────────           │
     │  Thêm giỏ hàng      │
     │  (AJAX / local)     │
     ├─────────────────────┤
     │  Cùng quán (grid)   │
     ├─────────────────────┤
     │  Đánh giá (paginated)│
     └─────────────────────┘
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

### 7.2 Cart Item Responsive (v3.2 → v3.3)

#### Vấn đề mobile (v3.1)

Trên màn hình < 576px, cart item ép 5 phần tử (Ảnh 80px + Info + Qty control + Total + Delete) trên **cùng 1 hàng ngang** — quá chật, dễ bấm nhầm.

#### Giải pháp v3.3: Multi-row Card Layout

Phân rã cart item thành **2 hàng riêng biệt**:

```
ROW 1 — Nhận diện & Xóa:
┌──────────────────────────────────────┐
│ ┌──────┐ ┌───────────────────┐  🗑   │
│ │ 60px │ │ Tên món (14px)    │ (44px)│
│ │ img  │ │ Giá: 35.000đ/phần │ touch │
│ └──────┘ └───────────────────┘       │
└──────────────────────────────────────┘

ROW 2 — Thao tác & Thanh toán:
┌──────────────────────────────────────┐
│ 35.000đ              [─] [2] [+]    │
│ (giá bên trái)        (qty bên phải) │
│                       44×44px touch  │
└──────────────────────────────────────┘
```

#### CSS Implementation

```css
@@media (max-width: 576px) {
    .cart-item {
        flex-wrap: wrap;
        gap: 6px;
        padding: 10px 12px;
    }
    /* Row 1: Image (60px, order:1) + Name (flex:1, order:2) + Delete (right, order:3) */
    .cart-item img {
        width: 60px; height: 60px; order: 1;
    }
    .cart-item .item-info {
        order: 2; flex: 1; min-width: 0;
    }
    .cart-item .item-name {
        font-size: 14px;
        -webkit-line-clamp: 2; /* 2 dòng tối đa */
        overflow: hidden;
    }
    .delete-btn {
        order: 3; align-self: flex-start;
        min-width: 44px; min-height: 44px; /* touch target */
        display: flex; align-items: center; justify-content: center;
    }
    /* Row 2: Price (width:auto, order:4) + Qty (margin-left:auto, order:5) */
    .item-total {
        order: 4; width: auto; min-width: auto;
        font-size: 14px; font-weight: 700;
        padding-top: 6px; margin-top: 4px;
        border-top: 1px solid #f0f0f0;
    }
    .qty-control {
        order: 5; margin-left: auto;
        padding-top: 6px; margin-top: 4px;
        border-top: 1px solid #f0f0f0;
    }
    /* Touch targets: 44×44px (WCAG) */
    .qty-btn {
        width: 44px !important; height: 44px !important;
        border-radius: 10px !important; font-size: 18px !important;
    }
}
```

#### Touch Target Matrix (WCAG 2.1)

| Element | Kích thước | Khoảng cách tới element kế |
|---------|-----------|--------------------------|
| Nút Giảm `[−]` | 44×44px | 12px với số lượng |
| Số lượng `[2]` | 32px (read-only) | 12px với nút Tăng |
| Nút Tăng `[+]` | 44×44px | ≥24px với Delete |
| Nút Xoá `🗑` | 44×44px | Góc phải, riêng biệt |
| Khoảng cách tối thiểu | — | 8px (khuyến nghị 16px) |

#### Files thay đổi

| File | Thay đổi |
|------|----------|
| `Cart/Index.cshtml` | Rewrite mobile CSS: multi-row, width:auto, 44px touch targets |

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

### 9.6 Viewport Wrapper — Fix Scroll Rác (v3.3)

#### 9.6.1 Vấn đề

Trang Login/Signup/Forgot dùng `min-height: 100vh` + `padding: 80px 20px 20px` (cho fixed header 60px).
Vì `box-sizing` mặc định là `content-box`, padding được **cộng thêm** vào chiều cao → tổng chiều cao > 100vh → thanh cuộn dọc xuất hiện vô lý.

```diff
- ❌ min-height: 100vh + padding: 80px 20px 20px → overflow!
+ ✅ height: 100vh + overflow: hidden → khít viewport
```

#### 9.6.2 Giải pháp: Flexbox Viewport Wrapper

Cấu trúc mới: **Bao khung Viewport (Viewport Wrapper)** — body `height:100vh; overflow:hidden`, bên trong flex layout với header trong flow.

```
┌── BODY (height:100vh; overflow:hidden) ───────────┐
│ ┌── AUTH-PAGE-WRAPPER (flex-column) ───────────┐  │
│ │                                               │  │
│ │ ┌── HEADER (flex-shrink:0, height:60px) ───┐  │  │
│ │ │ [F]ast[ship]    [← Trang chủ] [Đăng ký]  │  │  │
│ │ └───────────────────────────────────────────┘  │  │
│ │                                               │  │
│ │ ┌── MAIN (flex:1, flex-center, overflow-y:auto) │ │
│ │ │     ┌──────────────────────────┐             │  │
│ │ │     │    LOGIN FORM            │             │  │
│ │ │     │    (margin:auto)         │             │  │
│ │ │     │                          │             │  │
│ │ │     │    [Username]            │             │  │
│ │ │     │    [Password]            │             │  │
│ │ │     │    [   Đăng nhập   ]    │             │  │
│ │ │     └──────────────────────────┘             │  │
│ │ └──────────────────────────────────────────────┘  │
│ └──────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

#### 9.6.3 Nguyên lý hoạt động

| Thành phần | CSS | Mục đích |
|-----------|-----|----------|
| `body` | `height:100vh; overflow:hidden` | Khóa cứng viewport, cấm scroll toàn trang |
| `.auth-page-wrapper` | `display:flex; flex-direction:column; height:100%; max-height:100vh; overflow:hidden` | Container flex chiếm trọn viewport |
| `header.auth-header` | `flex-shrink:0` | Header chiếm chiều cao tự nhiên (60px), không co |
| `main.auth-main` | `flex:1; display:flex; align-items:center; justify-content:center; overflow-y:auto` | Phần còn lại, căn giữa form, cho phép scroll nội bộ nếu form quá dài (Signup) |
| `.login-container` | `margin:auto` | Form tự động căn giữa, không cần padding/margin thủ công |

#### 9.6.4 Thay đổi so với v3.2

| Thuộc tính | v3.2 (cũ) | v3.3 (mới) |
|-----------|-----------|-----------|
| `body` height | `min-height:100vh` (gây overflow) | `height:100vh; overflow:hidden` (khít) |
| `body` padding | `80px 20px 20px` (cộng vào height) | Không có padding (header trong flow) |
| Header position | `position:fixed` (tách khỏi flow) | `flex-shrink:0` (trong flow) |
| Form centering | `padding-top:80px` thủ công | `flex:1 + flex center` tự động |
| Overflow | `min-height:100vh + padding` → scroll | `overflow:hidden` trên wrapper + `overflow-y:auto` trên main |

#### 9.6.5 Ưu điểm

1. **Không scroll rác** — body khóa cứng 100vh, main scroll nội bộ nếu cần
2. **Form luôn căn giữa** — không phụ thuộc vào chiều cao nội dung
3. **Không padding thủ công** — flex layout tự động xử lý khoảng cách
4. **Semantic HTML** — `<header>` + `<main>` thay vì `div` lồng nhau

#### 9.6.6 Files thay đổi

| File | Thay đổi |
|------|----------|
| `Login.cshtml` | body → viewport wrapper, header → `flex-shrink:0`, main → flex center |
| `Signup.cshtml` | Same pattern (8 fields vẫn vừa nhờ `overflow-y:auto` trên main) |
| `Forgot.cshtml` | Same pattern (form ngắn, căn giữa hoàn hảo) |

---

### 9.7 Logout UI (All Roles)

#### 9.7.1 Customer Logout

**Trigger**: All customer pages (Home, Cart, etc.) via:
- User dropdown in navbar (avatar + username) → "Đăng xuất" link
- Footer links (visible khi đã đăng nhập)
- Route: `GET /Home/Logout`

**Behavior**: `HomeController.Logout()` clears session (`HttpContext.Session.Clear()`), removes authentication cookie, redirects to `/Home`.

**UX**:
```
[Avatar] ▼
──────────────
Xin chào, <username>
──────────────
📊 Lịch sử đơn hàng
🏪 Quản lý quán (nếu là Quán ăn)
❌ Đăng xuất          ← text-danger
```

**File**: `_LayoutPageHome.cshtml` (dropdown items), `HomeController.cs` (Logout action)

#### 9.7.2 Admin Logout

**Trigger**: Header user dropdown → "Đăng xuất" icon button
- SVG logout icon (door with arrow)
- Route: `@Url.Action("Logout","Home")` → `/Home/Logout`
- Same server-side session clear + redirect

**File**: `_LayoutPageAmin.cshtml`

#### 9.7.3 Restaurant Logout

**Trigger**: Header user dropdown → "Đăng xuất" icon button
- Same SVG style as Admin
- Route: `~/Home/Logout`
- Session clear + redirect về trang chủ

**File**: `_LayoutPageRestaurant.cshtml`

#### 9.7.4 Shipper Logout

**Trigger**: Header user dropdown → "Đăng xuất" icon button
- Dropdown also contains "Hồ sơ" link (profile settings)
- Route: `@Url.Action("Logout","Home")`
- Session clear + redirect

**File**: `_LayoutPageShipper.cshtml`

---

### 9.8 Google OAuth Role Selection (v5.3)

#### 9.8.1 Overview

When a user logs in with Google OAuth for the first time, instead of auto-creating an account with the default "Khách hàng" role, the system now redirects to a **role selection page** (`/Home/SelectRoleGoogle`) where the user chooses their account type.

**Files**: `HomeController.cs`, `Views/Home/SelectRoleGoogle.cshtml`

#### 9.8.2 Flow

```
Google OAuth Callback → Email not found in tbUser?
    → Save email/name to Session
    → Redirect /Home/SelectRoleGoogle
    → User selects role + enters phone + address (if Quán ăn/Shipper)
    → POST /Home/CompleteGoogleRegistration
    → Backend: validate → create tbUser + role record → SetSessionUser → redirect to dashboard
```

#### 9.8.3 SelectRoleGoogle UI

**3 Role Cards**: Khách hàng (👤), Đối tác Quán ăn (🏪), Tài xế Shipper (🏍️)
- Styled with `--fs-green`, `--fs-radius`, `--fs-shadow` design tokens
- **Conditional Address**: Chỉ hiện khi role = Quán ăn hoặc Shipper
- **Phone validation**: Regex `^0[1-9][0-9]{8,9}$`
- **Anti-forgery**: `[ValidateAntiForgeryToken]` trên POST

#### 9.8.4 Backend Logic

| Step | Mô tả |
|------|-------|
| Validate role | `loaitaikhoan` phải là `Khách hàng`, `Quán ăn`, hoặc `Shipper` |
| Validate phone | Regex + kiểm tra trùng SĐT |
| Validate address | Bắt buộc nếu role là Quán ăn hoặc Shipper |
| Password | Sinh ngẫu nhiên `GG_{Guid}` (plain-text) |
| Create tbUser | Insert với role đã chọn |
| Create record | `tbKhachHang` / `tbQuanAn` / `tbShipper` tuỳ role |
| Redirect | Role-based: `/Home`, `/Restaurant/Dashboard`, `/Shipper` |

#### 9.8.5 Files changed

| File | Thay đổi |
|------|----------|
| `HomeController.cs` | `GoogleResponse()`: save Session + redirect thay vì auto-create |
| `HomeController.cs` | **New** `SelectRoleGoogle()` GET |
| `HomeController.cs` | **New** `CompleteGoogleRegistration()` POST |
| `Views/Home/SelectRoleGoogle.cshtml` | **New**: 3 role cards, phone input, conditional address |

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

## 11. Dashboard Restaurant

**Layout**: `_LayoutPageRestaurant.cshtml`  
**CSS**: `style-restaurant.css`, `fastship-design-tokens.css`, inline override  
**Home view**: `Views/Restaurant/Index.cshtml`

### 11.1 Layout Desktop

```
┌──────────────────────────────────────────────────────┐
│ HEADER: ☰ Hamburger | Logo | 🔍 Search | 🔔 Bell | 👤 Avatar  │
├──────────┬───────────────────────────────────────────┤
│ SIDEBAR  │  MAIN CONTENT                             │
│ (dark)   │                                           │
│          │  ┌── Page Title ───────────────────────┐  │
│ 🏠 Home  │  │  Thống kê (h2)                     │  │
│ 📋 Đơn   │  │  Xin chào quản lí <Tên quán>        │  │
│ 🍕 Món   │  └─────────────────────────────────────┘  │
│ 📊 Báo   │                                           │
│ cáo      │  ┌── APRIORI AI INSIGHTS ─────────────┐  │
│ 💬 Chat  │  │  🤖 Chiến lược bán chéo từ dữ liệu │  │
│ 👤 Hồ sơ │  │  Phân tích Apriori trên N đơn hàng │  │
│          │  │  ┌──────┐ ┌──────┐ ┌──────┐       │  │
│          │  │  │Cơm sườn│ │Bò né│ │Pizza │       │  │
│          │  │  │+       │ │+     │ │+     │       │  │
│          │  │  │Trà đá│ │Coca │ │Nước│       │  │
│          │  │  │78%    │ │65%   │ │82%   │       │  │
│          │  │  └──────┘ └──────┘ └──────┘       │  │
│          │  └─────────────────────────────────────┘  │
│          │                                           │
│          │  ┌── KPI ROW (4 columns) ──────────────┐  │
│          │  │  📦 Đơn hôm nay    💰 Doanh thu    │  │
│          │  │  👥 Khách mới      ⭐ Đánh giá     │  │
│          │  └─────────────────────────────────────┘  │
│          │                                           │
│          │  ┌── RECENT ORDERS TABLE ──────────────┐  │
│          │  │  DataTable with search/sort         │  │
│          │  │  |Mã|Khách|Món|Tổng|TT|Action|     │  │
│          │  │  |---|-----|---|----|--|------|     │  │
│          │  │  ...                                 │  │
│          │  └─────────────────────────────────────┘  │
├──────────┴───────────────────────────────────────────┤
└──────────────────────────────────────────────────────┘
```

### 11.2 Apriori AI Insights Card

Phân tích dữ liệu đơn hàng hoàn thành bằng thuật toán Apriori, hiển thị gợi ý bán chéo:

| Element | Style |
|---------|-------|
| **Header** | Gradient xanh `#3CB815→#27a001`, text trắng, icon 🤖, badge "AI" |
| **Card body** | Padding 20px 24px, grid `repeat(auto-fill, minmax(280px, 1fr))` |
| **Insight item** | bg `#f8f9fa`, border-radius 12px, padding 14px, hover border green + shadow |
| **Confidence** | Màu đỏ `#e74c3c`, bold, hiển thị % khách mua A cũng mua B |
| **Support** | 11px, muted, hiển thị % support + số đơn pair |

### 11.3 Chức năng chính

| Menu Item | View | Route |
|-----------|------|-------|
| Dashboard | `Index.cshtml` | `/Restaurant` |
| Quản lý đơn hàng | `OrderList.cshtml` | `/Restaurant/OrderList` |
| Quản lý món ăn | `Product.cshtml` | `/Restaurant/Product` |
| Thêm món ăn | `AddProduct.cshtml` | `/Restaurant/AddProduct` |
| Báo cáo doanh thu | `Report.cshtml` | `/Restaurant/Report` |
| Đánh giá | `Review.cshtml` | `/Restaurant/Review` |
| Chat hỗ trợ | `NhanTin.cshtml` | `/Restaurant/NhanTin` |
| Hồ sơ quán | `Profile.cshtml` | `/Restaurant/Profile` |

### 11.4 Real-time Features

| Trigger | SignalR Event | Group |
|---------|---------------|-------|
| Có đơn mới | `newOrder` → `restaurant_{maquan}` | Toast + reload table |
| Nhận đơn (nhandon) | `orderStatusChanged` → `order_{id}` | Customer nghe |
| Hủy đơn (huydon) | `orderStatusChanged` → `order_{id}` | Customer nghe |
| Chuẩn bị xong (hoantatdon) | `orderStatusChanged` + `newPickupOrder` | Customer + Shipper |
| Xác nhận lấy hàng (Delivery Scan) | `deliveryScanned` → `order_{id}` | Customer |

---

## 12. Dashboard Shipper

**Layout**: `_LayoutPageShipper.cshtml`  
**CSS**: `style-shiper.css`, `fastship-design-tokens.css`, inline override  
**Home view**: `Views/Shipper/Index.cshtml`

### 12.1 Layout Desktop — Split-screen

```
┌──── SHIPPER-SPLIT (grid: 380px 1fr, max-width: 1440px) ──────────┐
│ ┌── LEFT PANEL (flex-col, gap:20px) ──┐ ┌── RIGHT PANEL ──────┐ │
│ │                                      │ │                      │ │
│ │ ┌── PROFILE CARD ─────────────────┐ │ │ 📋 Đơn hàng           │ │
│ │ │  gradient dark bg               │ │ │                      │ │
│ │ │  ┌────┐                         │ │ │ [FREE-PICK] [ĐƠN HÀNG]│ │
│ │ │  │ 64 │  Nguyễn Văn A           │ │ │  (tab group)          │ │
│ │ │  │ px │  🟢 Đang hoạt động     │ │ │                      │ │
│ │ │  └────┘  [Bật/Tắt]              │ │ │ ┌── ORDER CARDS ──┐ │ │
│ │ └──────────────────────────────┘ │ │ │ ┌──────────────┐ │ │
│ │                                      │ │ │ #42 FREE-PICK │ │ │
│ │ ┌── QUICK STATS (2×2 grid) ────┐ │ │ │ 🏪 Koneko      │ │ │
│ │ │ 📦 Hôm nay  💰 Thu nhập     │ │ │ │ 📍 48 Cao Thắng │ │ │
│ │ │    5 đơn     120,000đ       │ │ │ │ 💵 Thu: 120k   │ │ │
│ │ │ 🚚 Đang giao 📋 FREE-PICK   │ │ │ │ [Nhận đơn] [👁]│ │ │
│ │ │    2 đơn       3 đơn        │ │ │ └──────────────┘ │ │
│ │ └──────────────────────────────┘ │ │ ┌──────────────┐ │ │
│ │                                      │ │ #41 Đang giao  │ │ │
│ │ ┌── MAP CARD ────────────────────┐ │ │ 🏪 Cơm 1990    │ │ │
│ │ │  📍 Vị trí của bạn             │ │ │ 💰 Ship: 15k  │ │ │
│ │ │  ┌────────────────────────┐   │ │ │ [Chi tiết]    │ │ │
│ │ │  │   Leaflet Map         │   │ │ └──────────────┘ │ │ │
│ │ │  │   260px height        │   │ │                      │ │
│ │ │  │   marker: vị trí bạn  │   │ │ [🔄 Làm mới]         │ │
│ │ │  └────────────────────────┘   │ │                      │ │
│ │ └──────────────────────────────┘ │ │                      │ │
│ └──────────────────────────────────┘ └──────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
```

### 12.2 Component Detail

| Component | CSS | Mô tả |
|-----------|-----|-------|
| **Split layout** | `.shipper-split` | `display:grid; grid-template-columns:380px 1fr; gap:24px` |
| **Profile card** | `.profile-card` | `background: linear-gradient(135deg, #1a1a2e, #16213e)`, border-radius 16px, padding 24px, trắng |
| **Avatar** | `.profile-avatar` | 64×64px, border-radius 50%, border 3px trắng mờ |
| **Status dot** | `.status-dot` | 8×8px, `.online` = green + box-shadow, `.offline` = red |
| **Quick stats** | `.quick-stats` | `grid-template-columns: 1fr 1fr; gap: 12px` |
| **Stat item** | `.stat-item` | bg trắng, border-radius 12px, padding 16px, border `#f0f0f0`, hover translateY(-1px) |
| **Map card** | `.map-card` | bg trắng, border-radius 16px, overflow hidden, border `#f0f0f0` |
| **Tab group** | `.tab-group` | bg `#f3f4f6`, border-radius 10px, padding 4px, `.tab-btn.active`: bg trắng + shadow |
| **Order cards** | `.order-card` | bg trắng, border-radius 14px, padding 20px, border `#f0f0f0`, hover translateY(-2px) + shadow |
| **Order grid** | `.order-grid` | `grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 16px` |
| **Accept btn** | `.btn-accept` | bg `#3CB815`, trắng, hover `#34a013` |
| **Empty state** | `.empty-state` | text-align center, padding 60px, icon 48px, muted text |

### 12.3 Order Card Detail

```
┌── ORDER CARD ───────────────────────────────┐
│ #42                              🟡 FREE-PICK│
│                                               │
│ 🏪 Koneko Pizza                               │
│ 📍 Quán: 48 Cao Thắng → Giao: Nguyễn Trãi    │
│ 💵 Thu: 120,000đ | Ship: 15,000đ              │
│ 📅 15/07 14:30                                │
│                                               │
│ ┌──────────┐ ┌────┐                          │
│ │ ✅ Nhận  │ │ 👁 │                          │
│ │   đơn    │ │    │                          │
│ └──────────┘ └────┘                          │
└──────────────────────────────────────────────┘
```

### 12.4 SignalR Real-time

| Event | Hành động |
|-------|-----------|
| `newPickupOrder` | 🔊 Phát âm thanh (triangle wave 660Hz) + reload trang sau 1s |
| `orderAccepted` | Ẩn card đơn (opacity→0, translateX→40px) + remove sau 350ms |
| `JoinShipperGroup` | Kết nối vào group nhận đơn FREE-PICK mới |
| Geolocation | `navigator.geolocation.watchPosition` → `UpdateLocation` (nếu có currentOrderId) |

---

---

## 13. Chat Widget (AI + Support)

### 13.1 Widget Overview

Floating chat bubble (bottom-right, z-index 9999):
- **Closed state**: Green circle (56px) with comment icon + unread badge
- **Open state**: 360×520px popup with two tabs: AI Chat + Support

### 13.2 Chat Widget Modern Minimalist Restyle (v4.3)

Toàn bộ inline CSS trong `_ChatWidget.cshtml` đã được viết lại theo phong cách Modern Minimalist:

| Thành phần | Before (v4.2) | After (v4.3) |
|-----------|---------------|-------------|
| **Màu chủ đạo** | `#28a745` (old green) | `var(--fs-green, #3CB815)` — design tokens |
| **Header background** | `#28a745` solid | `linear-gradient(135deg, var(--fs-green), var(--fs-green-dark))` |
| **Font** | Không khai báo | `var(--fs-font, 'Inter', sans-serif)` khắp file |
| **Border-radius** | 12px | `var(--fs-radius, 12px)` — flexible |
| **Box-shadow** | `0 8px 40px rgba(0,0,0,0.15)` | `0 8px 40px rgba(0,0,0,0.12)` — nhẹ hơn |
| **Tab active** | `#28a745` | `var(--fs-green, #3CB815)` |
| **Tab bg** | `#f5f6fa` | `var(--fs-light, #f8f9fa)` |
| **User message** | `#28a745` | `var(--fs-green, #3CB815)` |
| **Typing dots** | `#bbb` | `var(--fs-green, #3CB815)` — pulse animation |
| **Quick reply** | `#e8f0fe` blue | `var(--fs-green-bg)` + green border |
| **Admin status** | `#f0f4ff` blue bg | `var(--fs-green-bg)` + `@keyframes fsPulse` dot |
| **Scrollbar** | Mặc định | Custom 4px thin scrollbar |
| **Keyframes** | `@@keyframes typing` | `@@keyframes fsTyping` + `@@keyframes fsPulse` |
| **Animation `display:none`** | `chat-box { display:none }` (block transition) | `transform: scale(0.9); opacity:0; pointer-events:none` (scale-in tự layout-sg.css) |

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

### 14.5 Real-time Geolocation Streaming (v4.3)

Khi Shipper bấm "Chấp nhận đơn" (`Shipper/OrderDetail`), hệ thống tự động kích hoạt:

```javascript
// Shipper OrderDetail.cshtml — geolocation streaming
navigator.geolocation.watchPosition(function(pos) {
    var lat = pos.coords.latitude;
    var lng = pos.coords.longitude;
    
    // Update map center
    map.setView([lat, lng], 15);
    
    // Stream to SignalR — order_{madh} group
    conn.invoke('UpdateLocation', orderId, lat, lng);
}, function(err) { /* fallback */ }, {
    enableHighAccuracy: true,
    maximumAge: 5000,
    timeout: 15000
});

// Cleanup on page unload
window.addEventListener('beforeunload', function() {
    navigator.geolocation.clearWatch(window._geoWatchId);
});
```

**SignalR Hub** (`Chats.cs`):
```csharp
public async Task UpdateLocation(int orderId, double lat, double lng)
{
    await Clients.Group($"order_{orderId}").SendAsync("shipperLocationUpdate", orderId, lat, lng);
}
```

**Leaflet Marker smooth transition**:
```javascript
// ChiTietDonHang.cshtml — sau khi tạo shipperMarker
shipperMarker._icon.style.transition = 'transform 0.5s ease';
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

### 16.1 Custom Keyframes

FastShip dinh nghia CSS @keyframes sau trong `layout-sg.css` va inline trong views:

| Keyframe | File | Target |
|----------|------|--------|
| `fs-shimmer` | `layout-sg.css` | Skeleton loading shimmer |
| `slideInDown` | `layout-sg.css` | Carousel caption h1 tu tren xuong |
| `slideInUp` | `layout-sg.css` | Carousel caption buttons tu duoi len |
| `typing` | `_ChatWidget.cshtml` | Chat AI typing indicator |
| `pulse` | `AdminChat/Index.cshtml` | Unread indicator, online status |
| `slideUp` | `SuccessView.cshtml`, `FailureView.cshtml` | Ket qua thanh toan hien thi |
| `popIn` | `Checkout.cshtml` | Popup scale-in effect |

### 16.2 Animation Inventory

| Element | Animation | Duration | Easing | Trigger | File |
|---------|-----------|----------|--------|--------|------|
| Product card hover | `translateY(-6px)` + box-shadow | 0.3s | ease | Hover | `layout-sg.css` |
| Product image hover | `scale(1.08)` | 0.5s | ease | Card hover | `layout-sg.css` |
| Button hover | `translateY(-1px)` | 0.2s | ease | Hover | `layout-sg.css` |
| Cart item hover | bg highlight | 0.15s | ease | Hover | `Cart/Index.cshtml` |
| Cart qty button | border color + bg | 0.15s | ease | Hover | `Cart/Index.cshtml` |
| Delete button | color red + bg pink | 0.15s | ease | Hover | `Cart/Index.cshtml` |
| Checkout gradient btn | opacity | 0.2s | ease | Hover | `Cart/Checkout.cshtml` |
| Payment option | border highlight | 0.2s | ease | Click/Hover | `Cart/Checkout.cshtml` |
| Address card | border + shadow | 0.2s | ease | Selected | `Cart/Checkout.cshtml` |
| Carousel crossfade | carousel-fade class | 0.6s | CSS transition | Slide activate | `main.js` |
| Carousel caption | slideInDown/Up | 0.7s (0.4s mobile) | ease both | Slide activate + skeleton callback | `layout-sg.css` + `main.js` |
| Skeleton shimmer | fs-shimmer | 1.5s infinite | linear | Page load | `layout-sg.css` |
| Navbar scroll shadow | class toggle | 0.3s | ease | Scroll >10px | `layout-sg.css` |
| Nav fixed on scroll (v3.6) | fs-nav-fixed class | Instant | -- | Scroll > topbarH | `main.js` |
| Chat toggle hover | scale + shadow | 0.2s | ease | Hover | `_ChatWidget.cshtml` |
| Chat typing dots | typing keyframes | 1.4s | ease-in-out | AI responding | `_ChatWidget.cshtml` |
| Toast dismiss | jQuery fadeOut | 0.4s | -- | After 3.5s | Inline JS |
| Bottom sheet slide | translateY | 0.35s | cubic-bezier(.32,.72,0,1) | Click FAB | `DetailRestaurant.cshtml` |
| Bottom sheet overlay | opacity fade | 0.3s | ease | Click FAB | `DetailRestaurant.cshtml` |
| Category pill hover | border + background | 0.2s | ease | Hover | `layout-sg.css` |
| Social link hover | border + bg + color | 0.2s | ease | Hover | `layout-sg.css` |
| KPI icon hover | scale + rotate | 0.3s | ease | Hover | `Admin/Dashboard.cshtml` |
| Admin chat transitions | all properties | 0.15s | ease | Various | `AdminChat/Index.cshtml` |
| Review card hover | box-shadow | 0.2s | ease | Hover | `Restaurant/Review.cshtml` |
| Search autocomplete | bg highlight | 0.15s | ease | Hover | `_LayoutPageHome.cshtml` |
| Success/Failure page | slideUp keyframes | 0.5s | ease | Page load | `SuccessView.cshtml` |
| Checkout popup | popIn scale | 0.3s | ease | Payment result | `Checkout.cshtml` |
| Map marker update | setLatLng() | Instant | -- | SignalR event | `ChiTietDonHang.cshtml` |
| Skeleton fadeOut | jQuery fadeOut(250) | 0.25s | -- | 100ms after DOM + carousel re-trigger | `main.js` |
| **Carousel re-trigger (skeleton callback)** | Carousel.cycle() + triggerCaptionAnim() | — | — | After skeleton fadeOut | `main.js` |
| **Promo band compact** | negative margin for hero | — | — | Resize | `layout-sg.css` |
| Back-to-top fade | fadeIn/fadeOut | slow | -- | Scroll >300px | `main.js` |
| Back-to-top scroll | animate scrollTop | 1500ms | easeInOutExpo | Click | `main.js` |

### 16.3 Reduced Motion

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

### 17.3 Mobile Responsive Matrix (v3.2 → v3.3 — Tất cả các trang)

| Trang | Vấn đề | Fix v3.2 | Fix v3.3 (NEW) | Files |
|-------|--------|----------|---------------|-------|
| **Login/Signup/Forgot** | Không có header, mất navigation | Thêm fixed header | ⭐ **Viewport Wrapper**: `height:100vh;overflow:hidden`, header trong flex-flow, form flex center | `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml` |
| **Login/Signup/Forgot** | Container 420px tràn mobile | Padding giảm | Input font-size 16px (iOS zoom) | `login.css` |
| **Login/Signup/Forgot** | Scroll rác do `min-height:100vh + padding` | — | ⭐ **Xoá scroll hoàn toàn**: body `overflow:hidden`, main `overflow-y:auto` | `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml` |
| **DetailRestaurant** | Ảnh/info fixed width | 100% width, items xếp dọc | ⭐ **Sticky bar + FAB + Bottom Sheet**: `position:sticky` cho category, FAB 56px, bottom sheet 60vh | `DetailRestaurant.cshtml` |
| **DetailRestaurant** | Nhiều danh mục khó duyệt | Scroll ngang | ⭐ **Bottom Sheet duyệt nhanh**: overlay mờ, slide-up .35s cubic-bezier, icon emoji cho từng DM | `DetailRestaurant.cshtml` |
| **Cart/Index** | 5 elements trên 1 hàng ngang | Flex-wrap, ảnh 48px | ⭐ **Multi-row layout**: Row 1 (ảnh 60px+tên+xoá), Row 2 (giá trái + qty phải), touch 44×44px | `Cart/Index.cshtml` |
| **Header/Skeleton** | Skeleton che header | Header z-index 10000 | — | `layout-sg.css` |
| **ChiTietDonHang** | margin-top 200px | Giảm 200px→130px/80px | — | `ChiTietDonHang.cshtml` |
| **LichSuDatHang** | margin-top 150px | Giảm + DataTables responsive | — | `LichSuDatHang.cshtml` |
| **Thanh toán** | Payment/coupon không responsive | CSS responsive | — | `layout-sg.css` |
| **Chat Widget** | Toggle 56px cố định | Toggle 48px, box full-width | — | `layout-sg.css` |
| **Nhắn tin** | Header quá to | Giảm padding 12rem→5rem | — | `NhanTin.cshtml` |
| **DanhMuc/SanPham** | Ảnh height:250px cứng | object-fit:cover | ⭐ **aspect-ratio 4/3 thay height cố định**, `.category-card-img` class | `DanhMuc.cshtml`, `SanPham.cshtml`, `layout-sg.css` |
| **Page Header** | padding-top 12rem | 7rem/5rem mobile | — | `layout-sg.css` |

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
| Promo band | Font 13px (12px v4.2), padding 10px (6px v4.2) 36px |
| Chat toggle | 48px, bottom: 16px, right: 16px |
| Chat box | Full width (left: 8px, right: 8px) |
| Cart item | Flex-wrap, ảnh 48px, font 12px |
| Carousel | Caption position relative, background rgba(0,0,0,.45); **crossfade** (carousel-fade class) |
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
| **aria-label on icon buttons** | Chat toggle, cart delete, AJAX toggle buttons | ✅ v3.4 |

### 18.2 Areas for Improvement

- [ ] Implement skip-to-content link
- [ ] Add ARIA live regions for dynamic content updates
- [ ] Add keyboard support for star rating (arrow keys)
- [ ] Ensure sufficient color contrast in dashboard gradient KPI cards

---

## 19. Icons & Iconography

### 19.1 Icon Libraries Inventory

FastShip đã **đồng bộ hoá** toàn bộ icon libraries về **chỉ 2 nguồn**: Font Awesome 5 (CDN) và Emojis hệ thống. Tất cả các icon fonts thừa (Bootstrap Icons, Flaticon, LineIcons, Line Awesome, Simple Line Icons, Themify, Material Design Iconic, Avasta, Icomoon, Font Awesome Old) đã được xoá khỏi CSS @import và thư mục vật lý.

| Theme | Libraries | Nguồn | Usage |
|-------|-----------|-------|-------|
| **Home (Customer)** | Font Awesome 5 (`fa`, `fas`, `fab`) | CDN 5.10.0 | Header, footer, cards, buttons, social links |
| **Admin Dashboard** | Font Awesome 5 (`fa`, `fas`, `fab`) + SVG inline | CDN 5.10.0 | Sidebar icons, KPI cards, tables, action buttons |
| **Restaurant Dashboard** | Font Awesome 5 (`fa`, `fas`, `fab`) + SVG inline | CDN 5.10.0 | Sidebar menu, KPI cards, product management |
| **Shipper Dashboard** | Font Awesome 5 (`fa`, `fas`, `fab`) + SVG inline | CDN 5.10.0 | Sidebar, order list, wallet, notifications |
| **Cart/Checkout** | Font Awesome 5 (`fa`, `fas`, `fab`) + Elegant Icons | CDN + local | Cart items, payment icons, action buttons |
| **Category pills** | Emojis hệ thống | Hệ thống | Danh mục món ăn (🍽, 🍚, 🍜, etc.) |

> ⚡ **Icon Cleanup v4.1**: Đã xoá hoàn toàn Bootstrap Icons, Flaticon, LineIcons, Line Awesome, Simple Line Icons, Themify, Material Design Iconic Font, Avasta, Icomoon, Font Awesome Old — gồm CSS @import + thư mục icons vật lý (~173k dòng, ~50MB).

### 19.2 Icon Usage Patterns

#### Home Theme

| Location | Icon Class | Style | Color |
|----------|-----------|-------|-------|
| Top bar social links | `fab fa-facebook-f`, `fab fa-instagram`, `fab fa-tiktok`, `fab fa-youtube` | Regular | `var(--fs-muted)`, hover `var(--fs-green)` |
| Top bar contact | `fa fa-map-marker-alt`, `fa fa-envelope` | Solid | `var(--fs-muted)` |
| Search button | `fa fa-search` | Solid | White on green bg |
| Cart button | `fa fa-shopping-bag` | Solid | `var(--fs-dark)`, hover white |
| User menu | `fa fa-user`, `fa fa-sign-in-alt`, `fa fa-user-plus`, `fa fa-sign-out-alt` | Solid | `var(--fs-dark)` |
| Order history | `fa fa-history` | Solid | `text-muted` |
| Restaurant store link | `fa fa-store` | Solid | `text-muted` |
| Category pills | Inline emojis (🍽, 🍚, 🍜, 🥘, etc.) | Emoji | Natural |
| Star rating | `fa fa-star` | Solid | `#f39c12` (gold) |
| Address marker | `fa fa-map-marker-alt` | Solid | `var(--fs-green)` |
| How-it-works icons | `fa fa-search`, `fa fa-shopping-bag`, `fa fa-motorcycle` | Solid | `var(--fs-green)` on light green bg |
| Footer social | `fab fa-facebook-f`, `fab fa-instagram`, `fab fa-tiktok`, `fab fa-youtube` | Brand | `var(--fs-muted)`, hover white on green |
| Footer contact | `fa fa-map-marker-alt`, `fa fa-phone-alt`, `fa fa-envelope`, `fa fa-clock` | Solid | `var(--fs-green)` |
| Back to top | `fa fa-angle-up` | Solid | White on green |
| Carousel arrows | `bi bi-chevron-left`, `bi bi-chevron-right` | Bootstrap Icons | White |

#### Dashboard Themes (Admin, Restaurant, Shipper)

| Location | Icon Set | Typical Color |
|----------|---------|---------------|
| Sidebar menu items | Font Awesome 5 (`fas fa-home`, `fas fa-store`, etc.) + SVG inline | `#fff` on dark bg, active `var(--fs-green)` |
| KPI stat cards | Font Awesome (`fas fa-dollar-sign`, `fas fa-shopping-cart`, `fas fa-users`, `fas fa-percent`, `fas fa-trophy`) | White on `var(--fs-green-bg)` flat |
| Action buttons | Font Awesome (`fas fa-edit`, `fas fa-trash`, `fas fa-eye`, `fas fa-check`, `fas fa-times`) | Theme-specific |
| Data tables status | Font Awesome (`fas fa-check`, `fas fa-redo`, `fas fa-ban`, `fas fa-stream`, `fas fa-spinner`) | Contextual colors |
| User avatar | SVG inline (`icon-user1`, `icon-logout`) | `text-primary`, `text-danger` |
| Notification bell | `fas fa-bell`, SVG bell icon | `#fff` on header |
| Search | `fas fa-search` | `var(--text)` |

> **Lưu ý**: Flaticon (`flaticon-381-*`), LineIcons (`lni`), Line Awesome (`las`/`la`), Themify (`ti-*`), Material Design (`mdi`) đã được thay thế hoàn toàn bằng FA5 tương đương. Các icon classes cũ đã được chuyển đổi trong tất cả views (Admin, Shipper, Restaurant).

### 19.3 Icon Color Conventions

| Context | Color | HEX |
|---------|-------|-----|
| Active/Selected | Primary Green | `#3CB815` |
| Muted/Inactive | Muted Gray | `#6b7280` |
| Star rating | Gold | `#f39c12` |
| Success/Completed | Green | `#28a745` |
| Danger/Delete | Red | `#dc3545` / `#f72b50` |
| Warning | Orange | `#ff6d4d` |
| Info | Blue | `#2781d5` / `#4285F4` |
| Social brand | Brand-specific | `#1877F2` (FB), `#E1306C` (IG), etc. |
| Text default | Dark | `#1a1a2e` / `#111111` |

### 19.4 Emoji Usage for Categories

Trang DetailRestaurant sử dụng emoji mapping cho danh mục món ăn (hàm `getCategoryIcon` JS):

| Danh mục | Emoji | Logic |
|----------|-------|-------|
| Tất cả | 🍽 | `n === 'tat ca'` |
| Cơm | 🍚 | `includes('com')` |
| Phở | 🍜 | `includes('pho')` |
| Mỳ, Bún | 🍝 | `includes('mi') or includes('bun')` |
| Lẩu | 🥘 | `includes('lau')` |
| Bánh | 🥟 | `includes('banh')` |
| Salad, Rau | 🥗 | `includes('salad')` |
| Sushi | 🍣 | `includes('sushi')` |
| Thịt | 🥩 | `includes('thit')` |
| Cá, Hải sản | 🦐 | `includes('ca')` |
| Trà, Cafe | 🧋 | `includes('tra')` |
| Nước, Uống | 🥤 | `includes('nuoc')` |
| Tráng miệng | 🍰 | `includes('trang mieng')` |
| Khai vị | 🥟 | `includes('khai vi')` |
| Khác | 📂 | default |

Xử lý tiếng Việt không dấu qua `normalize('NFD') + replace(/[\u0300-\u036f]/g, '')`.

### 19.5 Icon Sizing Guidelines

| Context | Size |
|---------|------|
| Top bar social | ~12px |
| Navbar links | ~14px inline with text |
| Sidebar menu | 18-24px |
| KPI stat cards | 24px inside 56px container |
| Action buttons | 14-16px |
| Section headers | 16-20px inline |
| Star ratings | 14-16px |
| Cart delete | 16-18px |
| Chat widget | 20-24px toggle, 14px inline |
| Back to top | 16px |
| Footer icons | 13-14px |

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

### 21.1 Customer Flow (Updated — Real-time Order Pipeline v4.3)

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
  │                    PaymentController.ProcessPayment
  │                    (mock success/failure + coupon apply)
  │                          /            \
  │                        ✅            ❌
  │                  Success popup    Error popup
  │                        │
  │              ┌─────────┴─────────┐
  │              ▼                   ▼
  │      ChiTietDonHang      Giỏ hàng giữ nguyên
  │      (SignalR listener)     (thử lại)
  │              │
  │     ┌────────┴────────┐
  │     ▼                 ▼
  │  PaymentController         Restaurant OrderList
  │  broadcast newOrder ──────→ SignalR JoinRestaurantGroup
  │  → restaurant_{maquan}    → thông báo + reload
  │                              │
  │                   ┌──────────┴──────────┐
  │                   ▼                     ▼
  │              Nhận đơn              Hủy đơn
  │              (Đã xác nhận)
  │                   │
  │                   ▼
  │            Chuẩn bị xong
  │         (Chờ shipper lấy hàng)
  │              + broadcast
  │              → group "shippers"
  │                   │
  │           ┌───────┴───────┐
  │           ▼               ▼
  │     Shipper Index      Shipper Index (FREE-PICK)
  │     SignalR nhận        + reload trang
  │     newPickupOrder           │
  │     + âm thanh               │
  │           ┌──────────────────┘
  │           ▼
  │    Chấp nhận đơn → OrderDetail
  │     + setLatLng map
  │     + JoinOrderGroup(madh)
  │     + Geolocation watchPosition
  │           │
  │           ▼
  │    UpdateLocation(madh, lat, lng)
  │    → SignalR hub → order_{madh} group
  │           │
  │           ▼
  │    ChiTietDonHang.cshtml (Customer)
  │    shipperLocationUpdate event
  │    → Leaflet marker.setLatLng + map.setView
  │    → Marker lướt mượt (transition: transform 0.5s)
  │           │
  │           ▼
  │    Lấy hàng → Hoàn thành
  │    (UpdateDonHang AJAX)
```

### 21.2 Shipper Flow (Updated v4.3 — Real-time Pickup + Geolocation)

```
LOGIN ──→ DASHBOARD (with LIVE MAP)
              │
              ├── FREEPICK tab: raw SQL (đơn chưa có shipper)
              │    + SignalR JoinShipperGroup()
              │    + Lắng nghe newPickupOrder
              │    + Âm thanh + reload khi có đơn mới
              │    + Expose window.shipperConn
              │
              ▼
      Chấp nhận đơn → OrderDetail
              │
              ├── Map: Leaflet.js with pickup + delivery markers
              ├── SignalR: JoinOrderGroup(orderId)
              ├── Geolocation: watchPosition(enableHighAccuracy)
              │    → UpdateLocation(orderId, lat, lng)
              │    → group "order_{madh}" → Customer map
              └── Cleanup: clearWatch trên beforeunload
              │
              ▼
      [Lấy hàng] [Hoàn thành]
      (AJAX UpdateDonHang)

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

### 22.3 ✅ Completed in v3.3 — Layout Architecture Overhaul

| Task | Status | Files | Chi tiết |
|------|--------|-------|----------|
| **Viewport Wrapper Auth** | ✅ | `Login.cshtml`, `Signup.cshtml`, `Forgot.cshtml` | body `height:100vh;overflow:hidden`, header trong flex-flow, main flex center, form tự căn giữa |
| **Cart multi-row mobile** | ✅ | `Cart/Index.cshtml` | Row 1 (ảnh 60px + tên + xoá), Row 2 (giá trái + qty phải), touch 44×44px |
| **Sticky category bar** | ✅ | `DetailRestaurant.cshtml` | `position:sticky; top:68px/60px`, scrolled shadow, `.is-sticky` class |
| **FAB button** | ✅ | `DetailRestaurant.cshtml` | 56px (48px mobile), gradient green, badge đếm danh mục, `display:none` desktop |
| **Bottom Sheet category** | ✅ | `DetailRestaurant.cshtml` | 60vh, slide-up `.35s cubic-bezier`, overlay `.45 opacity`, đóng bằng Escape |
| **Category icon mapping** | ✅ | `DetailRestaurant.cshtml` | `getCategoryIcon()` xử lý tiếng Việt qua `normalize('NFD')` + 14 loại icon emoji |
| **aspect-ratio images** | ✅ | `DanhMuc.cshtml`, `SanPham.cshtml`, `layout-sg.css` | `.category-card-img` với `aspect-ratio: 4/3` (16/9 mobile), thay `height:250px` cứng |
| **UI-UX.md update** | ✅ | `UI-UX.md` | v3.3: 8 sections mới, responsive matrix cập nhật, backlog bổ sung |

### 22.4 ✅ Completed in v3.5 — UI Cleanup, 429 Handler, Pagination

| Task | Status | Details |
|------|--------|---------|
| **Remove legacy Bootstrap 3** | ✅ | Xóa 8 file CSS khỏi `wwwroot/Content/` (-7,472 lines), giữ `Site.css` |
| **Server-side pagination GetReviews** | ✅ | `IQueryable.Skip().Take()` trước ToList(), COUNT riêng, EF Core push-down |
| **429 Frontend handler (Login)** | ✅ | Chuyển form Login sang AJAX, parse JSON 429, hiển thị Retry-After |
| **429 Frontend handler (Checkout)** | ✅ | Thêm `xhr.status === 429` trong error callback, parse JSON + Retry-After header |
| **Overflow-x body fix** | ✅ | Tách `overflow-x:hidden` khỏi `<body>` trong `layout-sg.css`, chỉ giữ container |
| **WOW/OwlCarousel skeleton timing** | ✅ | Dời init vào callback `fadeOut()` của skeleton, tránh chạy khi overlay còn |
| **Bootstrap 3 _Layout.cshtml reference** | ✅ | Xóa `<link href="~/Content/bootstrap.css">` khỏi layout mặc định |

### 22.5 ✅ Completed in v4.8 — AdBlock Bypass, Hero Horizontal Slide, Coupon Popup & CSS Refinements

| Task | Status | Files | Chi tiết |
|------|--------|-------|----------|
| **AdBlock Bypass Icon SVG** | ✅ | `layout-sg.css`, `_LayoutPageHome.cshtml` | `fs-icon-anchor-f/i` class trung lập, SVG data URI, `inline-flex 28×28px`, `vertical-align:middle`, `no-repeat center/contain` |
| **Hero Carousel Horizontal Smooth Slide** | ✅ | `layout-sg.css`, `main.js`, `Index.cshtml` | Loại bỏ Ken Burns zoom + fade crossfade; `cubic-bezier(0.645,0.045,0.355,1)` horizontal slide; caption `translateX(30px)→0`; buttons delay 150ms |
| **Coupon Selection Popup** | ✅ | `CartController.cs`, `Checkout.cshtml` | `GetAvailableCoupons` endpoint query tbKhuyenMai; popup modal với coupon cards; click auto-apply → CheckCoupon |
| **CSS Refinements** | ✅ | `layout-sg.css` | Xoá `:contains()` không hợp lệ; chuẩn hoá background-image shorthand với `no-repeat center/contain`; fix `.fs-icon-anchor-i` |

### 22.7 Future Improvements

- [ ] **Dark mode**: Add CSS custom properties swap
- [ ] **Smooth page transitions**: View transitions API
- [ ] **Infinite scroll**: Replace pagination with Intersection Observer
- [ ] **Drag & drop**: For cart item reordering
- [ ] **Bottom sheet**: Replace popups with bottom sheets on mobile
- [ ] **Pull-to-refresh**: For order history on mobile
- [x] ✅ **Search autocomplete** (v3.4)
- [x] ✅ **AJAX Toggle 1-Click Hết hàng** (v3.4)
- [x] ✅ **aria-label icon-only buttons** (v3.4)
- [x] ✅ **SignalR payment broadcast** (v3.4)
- [x] ✅ **Soft delete tbMonAn**: isDeleted + FK RESTRICT, bảo toàn lịch sử hóa đơn
- [x] ✅ **tbLichSuSuDungKhuyenMai**: Lưu vết tần suất dùng mã giảm giá
- [x] ✅ **Redis IDistributedCache thay ConcurrentDictionary**: Connection state persistence, chịu restart container
- [x] ✅ **Force re-read giá từ DB**: PaymentController không tin frontend, chống sửa giá client
- [x] ✅ **Idempotency Lock Checkout**: Disable nút + spinner, chống double-submit
- [x] ✅ **Optimistic UI**: ToggleConHang + Add to Cart update ngay, rollback nếu fail
- [x] ✅ **Mobile Leaflet fix**: dragging:false, scrollWheelZoom:"center", giải phóng scroll dọc
- [x] ✅ **Payment error detail**: Inner exception + trace ID, không generic error
- [x] ✅ **AutoPreparingService tối ưu**: AsNoTracking + Attach + batch query
- [ ] **Real payment**: Replace mock Vietcombank with Stripe/PayPal/ZaloPay
- [ ] **Unit tests**: Add frontend component tests (Jest/Cypress)
- [ ] **Image optimization**: WebP format with `<picture>` fallback
- [ ] **Critical CSS**: Inline above-fold styles
- [ ] **Service Worker**: Offline support for order tracking
- [x] ✅ **Remove legacy Bootstrap 3** (v3.5)
- [x] ✅ **Server-side pagination reviews** (v3.5)
- [x] ✅ **429 frontend handler login+checkout** (v3.5)
- [x] ✅ **Overflow-x body fix** (v3.5)
- [x] ✅ **Skeleton+WOW/OwlCarousel timing** (v3.5)
- [ ] **Add `data-label` attributes to all dashboard tables**: Hiện tại stacked cards dùng CSS generic selector, nên thêm data-label cụ thể
- [x] ✅ **Dashboard mobile optimization**: Responsive sidebar, charts, KPI cards cho Admin/Restaurant/Shipper — **đã làm trong v4.0**
- [ ] **Google OAuth deployment test**: Kiểm tra đăng nhập Google trên Render production

### 22.6 ✅ Completed in v4.0 — Global Design System & Theme Unification

| Task | Status | Details |
|------|--------|---------|
| **Global Design Tokens** | ✅ | `fastship-design-tokens.css` — 350+ dòng, 16 component systems, unified `:root` variables |
| **Inter font enforcement** | ✅ | Font-family cascade trên mọi element (`!important`), FontAwesome exception cho icon fonts |
| **Dashboard KPI flat colors** | ✅ | Gradient rực rỡ → flat `var(--fs-green-bg)` + `var(--fs-shadow)` |
| **Dashboard sidebar clean** | ✅ | `.deznav .metismenu` border-radius 8px, hover xanh, active xanh full |
| **Dashboard tables modern** | ✅ | Uppercase 12px headers, `var(--fs-light)` bg, hover row green |
| **Dashboard buttons sync** | ✅ | `border-radius: 12px`, gradient xanh, font-weight 600 |
| **Dashboard form controls** | ✅ | `border-radius: 12px`, `border: 1.5px solid var(--fs-border)`, focus ring xanh |
| **Cart/Checkout border-radius sync** | ✅ | `border-radius: 12px !important`, `box-shadow: 0 4px 20px rgba(0,0,0,.07)` |
| **Auth pages mobile keyboard fix** | ✅ | `height:100vh` → `min-height:100vh;height:auto` + flexbox |
| **All 4 layouts linked** | ✅ | Home, Admin, Restaurant, Shipper đều load `fastship-design-tokens.css` |

### 22.7 ✅ Completed in v4.1 — Icon Cleanup & Library Consolidation

| Task | Status | Details |
|------|--------|---------|
| **Remove Bootstrap Icons CDN** | ✅ | Xoá `bootstrap-icons@1.4.1` khỏi Login, Signup, Forgot, `_LayoutPageHome` |
| **Remove LineIcons CDN** | ✅ | Xoá `lineicons.com/2.0` khỏi Admin, Restaurant, Shipper layouts |
| **Unify FA version** | ✅ | Admin + Shipper: FA6 beta → **FA5 5.10.0** (đồng bộ với Home) |
| **Remove .lni exception** | ✅ | Xoá `.lni` font-family exception khỏi `fastship-design-tokens.css` |
| **Replace Flaticon → FA5** | ✅ | `flaticon-381-location-4` → `fas fa-map-marker-alt` (Admin/OrderDetail) |
| **Replace Line Awesome → FA5** | ✅ | `las la-phone` → `fas fa-phone`, `las la-check-square` → `fas fa-check-square`, `las la-times-circle` → `fas fa-times-circle`, `la la-angle-left/right` → `fas fa-angle-left/right` |
| **Replace Themify → FA5** | ✅ | `ti-reload` → `fas fa-sync` (Shipper/ThongBao) |
| **Replace Material Design → FA5** | ✅ | `mdi mdi-file-document-box` → `fas fa-file-alt` (Shipper/ThongBao) |
| **Replace LNI → FA5** | ✅ | `lni lni-user` → `fas fa-user`, `lni lni-facebook-messenger` → `fab fa-facebook-messenger` (Shipper/OrderDetail) |
| **Remove @import icon CSS** | ✅ | Xoá 8 @import dòng icon fonts khỏi 6 files: `style-admin.css`, `style-restaurant.css`, `style-shiper.css`, `Admin/scss/main.css`, `Restaurant/scss/main.css`, `Shipper/scss/main.css` |
| **Delete icon directories** | ✅ | Xoá `Source/Admin/icons/`, `Source/Restaurant/icons/`, `Source/Shipper/icons/` (~173k dòng, ~50MB) |

### 22.8 ✅ Completed in v4.2 — Carousel-fade Crossfade & Hero Layout Optimization

| Task | Status | Files | Chi tiết |
|------|--------|-------|----------|
| **Carousel-fade crossfade** | ✅ | `main.js` | Chuyển carousel từ slide animation → fade crossfade (carousel-fade class), mượt mà hơn, không bị giật |
| **Carousel re-trigger trong skeleton callback** | ✅ | `main.js` | `Carousel.cycle()` + `triggerCaptionAnim()` sau khi skeleton fadeOut, đảm bảo auto-play và caption animation hoạt động |
| **Extract triggerCaptionAnim helper** | ✅ | `main.js` | Hàm reset animation (none → reflow → '') dùng chung cho skeleton callback và slid.bs.carousel event |
| **Cleanup WOW.js re-init khỏi Index.cshtml** | ✅ | `Home/Index.cshtml` | Xoá WOW re-init và carousel window.load handler khỏi Index.cshtml, chuyển logic vào main.js |
| **Promo band compact** | ✅ | `layout-sg.css` | Padding 14px→8px, font-size 15px→13px (mobile: 6px, 12px), line-height 1.3 |
| **Topbar compact** | ✅ | `layout-sg.css` | Height 38px→34px (--fs-topbar-h), font-size 12.5px→11.5px, padding 0 |
| **Negative margin hero** | ✅ | `layout-sg.css` | `#header-carousel { margin-top: -29px }` desktop / `-24px` mobile — kéo hero lên sát header, tạo 100vh effect |
| **Google OAuth auto-create** | ✅ | `HomeController.cs` | Khi email chưa tồn tại: tự động tạo tbUser (GG_Guid, email truncate 50) + tbKhachHang + redirect |
| **MySQL Server Version fix** | ✅ | `Program.cs` | `MariaDbServerVersion(10,6)` → `MySqlServerVersion(8,0,20)` — tắt RETURNING clause |
| **DateTime? ToString fix** | ✅ | `HomeController.cs` | `((DateTime)value).ToString()` thay vì `DateTime?.ToString()` tránh CS1501 build error |
| **fastship-design-tokens.css sync** | ✅ | `fastship-design-tokens.css` | `--fs-topbar-h: 38px→34px` đồng bộ với layout-sg.css (lần 2: file tokens global) |

---

> **Document Version**: 5.3 (Full)  
> **Cập nhật**: Tháng 7, 2026  
> **Based on**: Project.md  
> **Key changes v5.3**: Google OAuth Role Selection (SelectRoleGoogle), VietQR Bank Transfer (BankWebhook + OrderTracking QR Card), QA Fixes (XHR → jQuery ajaxSuccess, Idempotency Lock, Multi-device Cart Check)

---

## 23. Dual-Filter Bar & Cart Persistence (Grab UI)

### 23.1 FilterBar ViewComponent

**File**: `ViewComponents/FilterBarViewComponent.cs` + `Views/Shared/Components/FilterBar/Default.cshtml`

ViewComponent nhận `categoryId` và `q` (search query) từ ViewBag, render:

- **Horizontal scroll chip bar**: Các chip filter (Khuyến mãi, Bán chạy, Gần đây, Đánh giá tốt, $, $$, $$$, $$$$)
- **Nút "Bộ lọc"**: Kích hoạt Bottom Sheet
- **Chip active**: Xanh lá (`var(--fs-green)`), có icon ✕ để remove

```
┌──────────────────────────────────────────────────┐
│ [🍕 Tất cả] [🔥 Khuyến mãi] [⭐ Đánh giá tốt]    │
│ [💰 $] [💰 $$] [💵 $$$]     [☰ Bộ lọc]          │ ← horizontal scroll
└──────────────────────────────────────────────────┘
```

### 23.2 Bottom Sheet Filter UI

**File**: `wwwroot/js/filter.js`

Bottom Sheet với 4 sections:

```
┌──────────────────────────────────────────┐
│  ──── ──── ──── (drag handle)     [✕]   │
│  🔍 Bộ lọc tìm kiếm                      │
├──────────────────────────────────────────┤
│  📊 Sắp xếp theo (Radio)                 │
│  ○ Gợi ý  ○ Đánh giá  ○ Phí ship         │
│                                           │
│  🏷️ Tùy chọn (Checkbox)                  │
│  ☑ Khuyến mãi  ☐ Bán chạy  ☐ Gần đây     │
│                                           │
│  🍽 Loại ẩm thực (Grid)                  │
│  [Tất cả🍽] [Đồ ăn🍚] [Đồ uống🧋] [Chay🥗]│
│  [Bánh🥟] [Tráng miệng🍰] ...             │
│                                           │
│  💰 Khoảng giá (Price levels)            │
│  [$ 0-20k] [$$ 20-50k] [$$$ 50-100k]     │
│  [$$$$ 100k+]                             │
├──────────────────────────────────────────┤
│  [Áp dụng]                 [Đặt lại]     │
└──────────────────────────────────────────┘
```

**Two-way sync**: Chip ↔ Bottom Sheet state đồng bộ 2 chiều — click chip mở sheet và highlight đúng mục, chọn trong sheet tự động cập nhật chip.

### 23.3 Cart LocalStorage Persistence

**File**: `wwwroot/js/cart-local.js`

- Tự động lưu giỏ hàng vào `localStorage` khi có thay đổi
- Khi load trang: khôi phục cart từ localStorage qua API `RestoreFromLocal`
- Dùng XHR monkey-patch để phát hiện AJAX thay đổi số lượng
- Fallback: Session cart được ưu tiên nếu tồn tại

---

## 24. MoMo Payment Integration

### 24.1 MoMoService

**File**: `Services/MoMoService.cs`

Service tích hợp MoMo sandbox API với:

| Method | Mô tả |
|--------|-------|
| `CreatePaymentAsync(orderId, amount, orderInfo)` | Tạo request thanh toán MoMo, trả về payUrl |
| `QueryTransactionAsync(orderId)` | Kiểm tra trạng thái giao dịch |
| `ComputeHmacSha256(message, secretKey)` | HMAC-SHA256 signature cho MoMo requests |

**Security**: Toàn bộ keys đọc từ `Environment.GetEnvironmentVariable()` (MOMO_PARTNER_CODE, MOMO_ACCESS_KEY, MOMO_SECRET_KEY) — throws `InvalidOperationException` nếu thiếu.

### 24.2 Payment Flow

```
Customer Checkout ──→ PaymentController.ProcessPayment
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
              MoMo (payUrl)          COD (immediate)
                    │                   │
                    ▼                   ▼
        MoMoIpn callback ←───    SuccessView
        (cập nhật trạng thái)
```

---

## 25. RoleGuard Middleware

**File**: `Middleware/RoleGuardMiddleware.cs`

Middleware toàn cục chặn truy cập chéo role:

| Route prefix | Role required |
|-------------|---------------|
| `/Admin` | `Admin` |
| `/Restaurant` | `Quán ăn` |
| `/Shipper` | `Shipper` |

**Cơ chế**:
- Đọc `loaitaikhoan` từ session user
- So sánh với route map (case-insensitive)
- Nếu sai role → 403 + redirect về dashboard phù hợp
- Middleware được đăng ký **sau** `UseSession()` trong pipeline

---

## 26. Order Tracking & Live Map

### 26.1 OrderTracking View

**File**: `Views/Cart/OrderTracking.cshtml`

Trang theo dõi đơn hàng real-time:

```
┌──────────────────────────────────────────────┐
│  📦 Chi tiết đơn hàng #123                    │
│  ┌──────────────────────────────────────┐    │
│  │  Quán: Koneko Pizza                  │    │
│  │  Món: Pizza thập cẩm ×2              │    │
│  │  Địa chỉ: 48 Cao Thắng, Quận 3       │    │
│  │  Tổng: 120.000đ                       │    │
│  └──────────────────────────────────────┘    │
│                                              │
│  ┌── 7-STEP PROGRESS BAR ───────────────┐    │
│  │  ✅ ── ✅ ── ⏳ ── ○ ── ○ ── ○ ── ○  │    │
│  │  Đã   Xác  Chuẩn Chờ   Đã   Đang  Hoàn│    │
│  │  đặt  nhận bị    Shipper Lấy Giao  thành│    │
│  └──────────────────────────────────────┘    │
│                                              │
│  ┌── LEAFLET LIVE MAP ─────────────────┐     │
│  │  🗺️ [Map hiển thị vị trí shipper]   │     │
│  │  📍 Shipper: đang di chuyển          │     │
│  │  🏪 Quán ăn: [marker]               │     │
│  │  🏠 Giao đến: [marker]               │     │
│  └──────────────────────────────────────┘     │
└──────────────────────────────────────────────┘
```

### 26.2 7-Step Progress Bar

| Step | Trạng thái | Icon |
|------|-----------|------|
| 0 | Đã đặt | 📋 |
| 1 | Đã xác nhận | ✅ |
| 2 | Đang chuẩn bị | 👨‍🍳 |
| 3 | Chờ shipper lấy hàng | 📍 |
| 3.5 | Đã thanh toán (MoMo) | 💳 |
| 4 | Đã lấy | 📦 |
| 5 | Đang giao | 🚚 |
| 6 | Hoàn thành | 🎉 |

Mỗi step có: icon, label, đường kết nối (completed = xanh, active = pulse, pending = xám)

### 26.3 map.js Shared Module

**File**: `wwwroot/js/map.js`

Module JS chia sẻ cho toàn bộ tracking map:

```javascript
var FastShipTracking = {
    STATUS_FLOW: [...],  // 8 trạng thái
    connection: null,    // SignalR hub connection
    map: null,           // Leaflet map instance
    shipperMarker: null, // Moving marker
    
    initMap: function(elementId, lat, lng) { ... },
    initSignalR: function(orderId) { ... },
    renderProgressBar: function(currentStatus) { ... },
    getStatusStep: function(status) { ... },
    init: function(opts) { ... }  // One-call setup
};
```

**SignalR events**:
- `orderStatusChanged(orderId, status)` → cập nhật progress bar
- `shipperLocationUpdate(orderId, lat, lng)` → di chuyển marker trên map
- `shipperAssigned(orderId, shipperName)` → hiển thị tên shipper

---

## 27. Dashboard Real-time Updates

### 27.1 Admin Dashboard SignalR

**File**: `Views/Admin/Order.cshtml`

Admin quản lý đơn hàng với real-time notifications:

```javascript
// Admin/Order.cshtml
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/nhantin')
    .withAutomaticReconnect()
    .build();

connection.on('newOrder', function(orderId) {
    // Hiển thị toast notification + reload table
    showNotification('📦 Đơn hàng mới #' + orderId);
    $('#orderTable').DataTable().ajax.reload();
});

connection.on('orderStatusChanged', function(orderId, status) {
    // Cập nhật badge trạng thái + reload
    $('#orderTable').DataTable().ajax.reload();
});

connection.start();
```

### 27.2 Restaurant Real-time Broadcasts

| Controller Action | SignalR Event | Group |
|------------------|---------------|-------|
| `nhandon` (nhận đơn) | `orderStatusChanged` → `order_{id}` | Customer nghe |
| `huydon` (hủy đơn) | `orderStatusChanged` → `order_{id}` | Customer nghe |
| `hoantatdon` (chuẩn bị xong) | `orderStatusChanged` → `order_{id}` + `newPickupOrder` → shippers | Customer + Shipper |

### 27.3 Shipper Real-time Broadcasts

| Controller Action | SignalR Event | Group |
|------------------|---------------|-------|
| `UpdateDonHang` (cập nhật trạng thái) | `orderStatusChanged` → `order_{id}` | Customer nghe |
| Map location stream | `shipperLocationUpdate` → `order_{id}` | Customer map update |

---

## 28. VietQR Bank Transfer Payment (v5.3)
## 29. Lịch sử & Chi tiết Đơn hàng (All Roles)

### 29.1 Customer Order History (Cart/LichSuDatHang)

**File**: `Views/Cart/LichSuDatHang.cshtml`

**Layout**: `_LayoutPageHome.cshtml`

Trang chi tiết tất cả đơn hàng của khách hàng, có phân trang, tìm kiếm:

```
┌───────────────────────────────────────────────────────┐
│  📊 Lịch sử đơn hàng                                         │
│  Theo dõi tình trạng đơn hàng của bạn                           │
│                                                                              │
│  | Mã ĐH | Ngày đặt | Quán ăn | Món | Tổng tiền | Trạng thái | Thao tác |  │
│  |-------|--------|---------|-----|----------|-----------|--------|  │
│  | #42   | 15/07  | Koneko  | Pizza x2 | 120,000đ | 📋 Đã đặt  | [Xem]  |  │
│  | #41   | 14/07  | Cơm 1990| Cơm sườn| 45,000đ  | ✅ Hoàn thành | [Xem]  |  │
└───────────────────────────────────────────────────────┘
```

**Features**:
| Feature | Details |
|---------|---------|
| **DataTable** | jQuery DataTables với phân trang, tìm kiếm, sắp xếp |
| **Responsive** | Stacked cards trên mobile (`data-label`), margin-top giảm 150px→100px/80px |
| **Status badges** | 7 màu sắc khác nhau (xanh, cam, tím, teal, xanh lá, đỏ) |
| **Empty state** | Icon box + text + CTA "Đặt món ngay" nếu chưa có đơn |
| **Item preview** | Hiển thị 2 món đầu + số lượng (VD: "Pizza, Cơm... (3 món)") |
| **Actions** | Nút "Chi tiết" (✎) + "Theo dõi" (📍) cho mỗi đơn |

**Status color mapping**:
| Trạng thái | CSS Class | Màu nền |
|-----------|-----------|-----------|
| Đã đặt | `da-dat` | Xanh dương nhạt (#e3f2fd) |
| Đã xác nhận | `da-xac-nhan` | Xanh lá nhạt (#e8f5e9) |
| Đang chuẩn bị | `dang-chuan-bi` | Cam nhạt (#fff3e0) |
| Đã lấy | `da-lay` | Tím nhạt (#f3e5f5) |
| Đang giao | `dang-giao` | Teal nhạt (#e0f7fa) |
| Hoàn thành | `hoan-thanh` | Xanh lá đậm (#e8f5e9) |
| Đã hủy | `da-huy` | Đỏ nhạt (#ffebee) |

**Navigation**: Truy cập từ navbar (user dropdown → "Lịch sử đơn hàng"), footer (“Đơn hàng của tôi”), hoặc trang chủ (Re-order section).

### 29.2 Customer Order Detail (Cart/ChiTietDonHang)

**File**: `Views/Cart/ChiTietDonHang.cshtml`

**Layout**: `_LayoutPageHome.cshtml`

Trang chi tiết một đơn hàng cụ thể:

```
┌────────────┐ ┌──────────────────┐
│ Thông tin        │ │ Đơn hàng           │
│ Họ tên: ...     │ │ (Đã đặt)         │
│ Địa chỉ: ...    │ │ Món A x2   70,000đ │
│ SĐT: ...         │ │ Món B x1   35,000đ │
│ Ghi chú: ...     │ │ Tạm tính  105,000đ │
│ Shipper: ...      │ │ Phí ship   15,000đ │
│ [Chat]            │ │ Tổng:    120,000đ │
│ [Map] (nếu     │ │ Thanh toán: COD  │
│  đang giao)      │ │ [Theo dõi]       │
└────────────┘ └──────────────────┘
```

**Features**:
- **Invoice-style display** (thay vì input read-only): flat layout với label + value
- **Live Map**: Leaflet.js + SignalR shipper tracking (chỉ khi trạng thái = "Đang giao")
- **Chat button**: "Chat ngay!" dẫn đến trang NhanTin
- **Role-based actions**: Restaurant thấy nút Nhận đơn/Hủy đơn, Customer thấy nút Theo dõi
- **SignalR connection**: Auto-join group `order_{madh}` nhận shipper location updates

### 29.3 Customer Order Tracking (Cart/OrderTracking)

**File**: `Views/Cart/OrderTracking.cshtml`

**Layout**: `_LayoutPageHome.cshtml`

[Chi tiết đã mô tả ở Section 26 — Order Tracking & Live Map]

Bổ sung trong v5.3: QR code card cho đơn hàng chờ thanh toán (VietQR).

### 29.4 Admin Order History (Admin/Order)

**File**: `Views/Admin/Order.cshtml`

**Layout**: `_LayoutPageAmin.cshtml`

Dashboard quản lý đơn hàng toàn hệ thống:

| Feature | Details |
|---------|---------|
| **DataTable** | Phân trang server-side, tìm kiếm theo mã đơn / tên khách / quán |
| **Filters** | Lọc theo trạng thái, khoảng ngày |
| **Real-time** | SignalR `newOrder` + `orderStatusChanged` tự động reload DataTable |
| **Actions** | Xem chi tiết, cập nhật trạng thái, hủy đơn |

**Navigation**: Sidebar → "Xem đơn hàng" (`/Admin/Order`)

### 29.5 Restaurant Order List (Restaurant/OrderList)

**File**: `Views/Restaurant/OrderList.cshtml`

**Layout**: `_LayoutPageRestaurant.cshtml`

Dashboard quản lý đơn hàng của quán ăn:

| Feature | Details |
|---------|---------|
| **DataTable** | Danh sách đơn hàng của quán ăn hiện tại |
| **Status actions** | Nhận đơn (Đã xác nhận), Chuẩn bị xong (Chờ shipper), Hủy đơn |
| **SignalR** | `newOrder` event khi có đơn mới từ khách hàng |
| **KPI Cards** | Số liệu thống kê: tổng đơn, đơn hôm nay, doanh thu |

**Navigation**: Sidebar → Dashboard → "Danh sách đơn hàng" (`/Restaurant/OrderList`)

### 29.6 Shipper Order History (Shipper/LichSu)

**File**: `Views/Shipper/LichSu.cshtml`

**Layout**: `_LayoutPageShipper.cshtml`

Dashboard quản lý đơn hàng của shipper:

| Feature | Details |
|---------|---------|
| **DataTable** | Danh sách đơn đã/ đang giao của shipper |
| **Income tracking** | Hiển thị số tiền ship, thu hộ |
| **Status flow** | Chờ lấy hàng → Đã lấy → Đang giao → Hoàn thành |
| **Actions** | Xem chi tiết đơn, cập nhật trạng thái |

**Navigation**: Sidebar → "Lịch sử đơn hàng" (`/Shipper/LichSu`)

### 29.7 Shipper Index (FREE-PICK Dashboard)

**File**: `Views/Shipper/Index.cshtml`

**Layout**: `_LayoutPageShipper.cshtml`

Giao diện chính của shipper với danh sách đơn chờ nhận:

| Feature | Details |
|---------|---------|
| **FREE-PICK tab** | Raw SQL truy vấn đơn chưa có shipper |
| **Live Map** | Leaflet.js hiển thị vị trí quán ăn + khách hàng |
| **SignalR** | JoinShipperGroup + lắng nghe `newPickupOrder` + reload tự động |
| **Sound alert** | Phát âm thanh khi có đơn mới |
| **Accept action** | Nút "Chấp nhận đơn" race-condition safe |

**Navigation**: Default route sau khi shipper đăng nhập (`/Shipper`)

---

## 29.8 Summary: Login/Register/Logout/Order History by Role

| Role | Login Page | Register Page | Forgot? | Logout Trigger | Order History |
|------|-----------|--------------|---------|---------------|--------------|
| **Khách hàng** | `/Home/Login` (standalone) | `/Home/Signup` (standalone) | `/Home/Forgot` | User dropdown + Footer | `/Cart/LichSuDatHang` |
| **Admin** | `/Home/Login` (shared) | Không (do Admin tạo) | Không | Header dropdown | `/Admin/Order` |
| **Quán ăn** | `/Home/Login` (shared) | `/Home/Signup` (role=Quán ăn) | `/Home/Forgot` | Header dropdown | `/Restaurant/OrderList` |
| **Shipper** | `/Home/Login` (shared) | `/Home/Signup` (role=Shipper) | `/Home/Forgot` | Header dropdown | `/Shipper/LichSu` |

**Key insight**: Tất cả 4 role dùng chung 3 trang auth (Login, Signup, Forgot) và cùng 1 Logout action.
Sự khác biệt chỉ ở giao diện dashboard sau khi đăng nhập (role-based redirect) và sidebar navigation.



### 28.1 Architecture & Flow

```
CHECKOUT                         PAYMENT CONTROLLER               FRONTEND
User Chon CK → POST ProcessPayment → Create order (trangthai='Cho thanh toan')
                                         → Generate VietQR URL
                                         → Return qrCodeUrl + bankInfo
                                         → Frontend redirect /Cart/OrderTracking/{id}

WEBHOOK (Casso/SePay/PayOS) → POST /Payment/BankWebhook
    → Parse memo + amount
    → Extract FASTSHIP{madh}
    → Verify amount ±1000đ
    → Update trangthai = "Da dat"
    → SignalR: paymentConfirmed → order group
    → SignalR: newOrder → restaurant group

FALLBACK: User clicks "Toi da chuyen khoan"
    → GET /Payment/VerifyBankTransaction?madh={id}
    → If 15min passed: auto-confirm + SignalR broadcast
```

### 28.2 Payment Changes

| Change | File | Details |
|--------|------|---------|
| `trangthai` for bank transfer | `PaymentController.cs` | Set `"Chờ thanh toán"` thay vì `"Đã đặt"` |
| VietQR URL generation | `PaymentController.cs` | `img.vietqr.io/image/{BANK_ID}-{BANK_ACCOUNT_NO}-compact2.png?amount=...&addInfo=FASTSHIP{madh}` |
| `IsBankTransferMethod` | `PaymentController.cs` | Helper kiểm tra tên phương thức từ DB |
| Cart not cleared | `PaymentController.cs` | Bank transfer không xóa cart (user dễ đặt lại) |
| QR code generation | `CartController.cs` | OrderTracking action tự sinh VietQR URL khi order ở trạng thái "Chờ thanh toán" |
| IConfiguration injection | `CartController.cs` | Thêm `IConfiguration` để đọc BANK_ID, BANK_ACCOUNT_NO, BANK_ACCOUNT_NAME |

### 28.3 BankWebhook Endpoint

**Endpoint**: `POST /Payment/BankWebhook` (`[AllowAnonymous]`)

| Feature | Details |
|---------|---------|
| **Auth** | Bearer token trong `Authorization` header |
| **Body parse** | Hỗ trợ Casso (`data[0].description`), SePay (`transferDesc`), PayOS (`description`) |
| **Memo parse** | Regex `FASTSHIP(\d+)` |
| **Amount check** | ±1000đ tolerance |
| **Status guard** | Chỉ xử lý nếu `trangthai == "Chờ thanh toán"` |
| **SignalR** | `paymentConfirmed` → order group + `newOrder` → restaurant group |

### 28.4 OrderTracking - QR Code Card UI

**File**: `Views/Cart/OrderTracking.cshtml`

When `trangthai == "Chờ thanh toán"` + payment method is bank transfer:

- QR code image (220×220px) with **shimmer overlay** animation
- Bank info table: Ngân hàng, Số TK, Chủ TK, Số tiền, Nội dung CK
- Warning: "Giữ nguyên Số tiền + Nội dung chuyển khoản"
- Loading spinner: "Hệ thống đang chờ xác nhận..."
- Fallback button: "Tôi đã chuyển khoản thành công"
- **SignalR**: `onPaymentConfirmed` callback tự động ẩn QR

### 28.5 Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `BANK_ID` | `Vietcombank` | Mã ngân hàng nhận tiền |
| `BANK_ACCOUNT_NO` | `1234567890` | Số tài khoản nhận |
| `BANK_ACCOUNT_NAME` | `FASTSHIP CO., LTD` | Tên chủ tài khoản |
| `BANK_WEBHOOK_TOKEN` | `""` | Bearer token cho webhook auth (optional) |

---

### v5.0 — Phase 1-5: Dual-Filter Bar, MoMo, RoleGuard, Real-time Tracking, Dashboard (NEW)

| Task | Status | Files | Chi tiết |
|------|--------|-------|----------|
| **Dual-Filter Bar** | ✅ | `FilterBarViewComponent.cs`, `Default.cshtml` | ViewComponent with horizontal chip bar, bottom sheet, Grab-like UX |
| **MenuSearch API** | ✅ | `HomeController.cs` | Dynamic SQL: LEFT JOIN khuyến mãi, WHERE rating ≥ 4.4, price range, ORDER BY |
| **filter.js** | ✅ | `wwwroot/js/filter.js` | Bottom sheet, chip-sheet two-way sync, AJAX search, price level, category grid |
| **Cart localStorage** | ✅ | `cart-local.js`, `CartController.cs` | Auto-save/restore cart, RestoreFromLocal endpoint, XHR monkey-patch |
| **MoMo Service** | ✅ | `MoMoService.cs`, `PaymentController.cs` | HMAC-SHA256 sandbox, create/query payment, IPN callback, env var config |
| **RoleGuard Middleware** | ✅ | `RoleGuardMiddleware.cs`, `Program.cs` | Route permission map, 403 redirect, after UseSession |
| **OrderTracking view** | ✅ | `OrderTracking.cshtml`, `CartController.cs` | 7-step progress bar, Leaflet live map, SignalR orderStatusChanged |
| **map.js shared module** | ✅ | `wwwroot/js/map.js` | Leaflet + SignalR hub, STATUS_FLOW, renderProgressBar, getStatusStep, FastShipTracking |
| **Restaurant broadcasts** | ✅ | `RestaurantController.cs` | nhandon/huydon/hoantatdon async SendAsync to order_{id} group |
| **Shipper broadcasts** | ✅ | `ShipperController.cs` | UpdateDonHang async, status change broadcast to order group |
| **Admin real-time** | ✅ | `Admin/Order.cshtml` | SignalR newOrder notification, orderStatusChanged refresh |
| **Navigation links** | ✅ | `ChiTietDonHang.cshtml`, `LichSuDatHang.cshtml` | Tracking link cho customer + restaurant |

---

### v4.8 (previous)
> - 🔄 **SignalR Real-time Order Pipeline** — Customer thanh toán → broadcast `newOrder` đến Restaurant; Restaurant "Chuẩn bị xong" → broadcast `newPickupOrder` đến Shipper; Shipper geolocation → stream `UpdateLocation` đến Customer map  
> - 💬 **Chat Widget Modern Minimalist** — `var(--fs-green)` thay `#28a745`, Inter font, gradient header, 12px radius, `--fs-border` tokens, admin dot pulse animation  
> - 📍 **Shipper Geolocation Streaming** — `navigator.geolocation.watchPosition()` (enableHighAccuracy) → signalR `UpdateLocation()` → group `order_{madh}` → Leaflet marker lướt mượt  
> - 🏪 **Restaurant "Chuẩn bị xong" button** — OrderList thêm nút cho đơn `Đã xác nhận`, link đến `hoantatdon/{id}`, broadcast đến shippers group  
> - 🐛 **Payment status fix** — `PaymentController` đổi `"Đang xử lý"` → `"Đã đặt"` đồng bộ với `CartController.SuccessView` và OrderList button logic  
> - 🧹 **Fix 1 — Footer social icons alignment** — `display:inline-flex; width:36px; height:34px; margin:0 6px; line-height:1` trong `layout-sg.css`  
> - 🧹 **Fix 1 — Chat toggle close không được** — `toggleChat()` bulletproofed (null checks, jQuery handlers), click-outside-to-close, `scale(0.9) translateY(20px)` animation  
> - 🧹 **Fix 2 — Cart multi-restaurant** — `ApiThemMonAn` + `ApiForceSwitchRestaurant` endpoints, confirm dialog khi thêm món khác quán  
> - 🧹 **Fix 3 — Geo throttling 5s** — `sendLocationThrottled()` trong Shipper/OrderDetail, `_throttleInterval=5000`, chỉ gửi tọa độ 5s/lần  
> - 🧹 **Fix 3 — OnDisconnectedAsync cleanup** — `Chats.cs` broadcast `shipperOffline` khi shipper mất kết nối  
> - 🧹 **Fix 4 — Race condition Shipper** — Kiểm tra `mashipper != null` + `trangthai` trước khi assign, TempData error nếu đơn đã có shipper khác  
> - 📐 UI-UX.md: v4.5 — 5 fixes backlog, cập nhật Section 13 Chat + 7 Cart + 14 Live Tracking  
> - 📐 Project.md: Cập nhật v2.9 — 5 logic fixes  

