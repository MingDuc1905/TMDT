# Fastship (ShipFood) — UI/UX Documentation (Full)

> **Phiên bản**: 4.3 — SignalR Real-time Order Pipeline, Chat Widget Modern Minimalist, Shipper Geolocation Streaming  
> **Cập nhật**: Tháng 7, 2026  
> **Mô tả**: Tài liệu thiết kế giao diện & trải nghiệm người dùng toàn diện cho nền tảng đặt đồ ăn Fastship  
> **Tài liệu liên quan**: Project.md — Tổng quan kiến trúc & phát triển

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
- Sidebar collapses to hamburger overlay

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

### 22.5 Future Improvements

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
- [ ] **Google OAuth deployment test**: Kiểm tra đăng nhập Google trên Railway production

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

> **Document Version**: 4.3 (Full)  
> **Cập nhật**: Tháng 7, 2026  
> **Based on**: Actual source code analysis of 8 Controllers, 30+ Views, 15+ Models, 10+ CSS files, 5 Layout files, 1 SignalR Hub, 4 sessions of responsive mobile fixes + 1 theme unification update + 1 icon cleanup session  
> **Key changes v4.3**:  
> - 🔄 **SignalR Real-time Order Pipeline** — Customer thanh toán → broadcast `newOrder` đến Restaurant; Restaurant "Chuẩn bị xong" → broadcast `newPickupOrder` đến Shipper; Shipper geolocation → stream `UpdateLocation` đến Customer map  
> - 💬 **Chat Widget Modern Minimalist** — `var(--fs-green)` thay `#28a745`, Inter font, gradient header, 12px radius, `--fs-border` tokens, admin dot pulse animation  
> - 📍 **Shipper Geolocation Streaming** — `navigator.geolocation.watchPosition()` (enableHighAccuracy) → signalR `UpdateLocation()` → group `order_{madh}` → Leaflet marker lướt mượt  
> - 🏪 **Restaurant "Chuẩn bị xong" button** — OrderList thêm nút cho đơn `Đã xác nhận`, link đến `hoantatdon/{id}`, broadcast đến shippers group  
> - 🐛 **Payment status fix** — `PaymentController` đổi `"Đang xử lý"` → `"Đã đặt"` đồng bộ với `CartController.SuccessView` và OrderList button logic  
> - 📐 UI-UX.md: Section 13 Chat Widget, 14 Live Tracking, 21 User Flows, 22.9 ✅ v4.3 backlog  
> - 📐 Project.md: Cập nhật v2.7 SignalR pipeline + API endpoints  

