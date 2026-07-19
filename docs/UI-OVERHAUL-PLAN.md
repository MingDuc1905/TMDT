# PLAN: FastShip UI Overhaul - 4 Roles (Customer + 3 Dashboards)

> Ngay tao: 2026-07-19
> Pham vi: Customer-facing + Admin, Shipper, Restaurant dashboards
> Muc tieu: Nang cap giao dien chuyen nghiep, adopt design system, cleanup CSS/vendor
> Trang thai: PLAN MODE - Chua code

---

## I. TINH TRANG HIEN TAI

### 1.1 Van de cot loi

| Van de | Muc do | Chi tiet |
|--------|--------|----------|
| Design system ton tai nhung khong dung | CRITICAL | fastship-design-tokens.css (641 dong) defines .fs-card, .fs-btn, .fs-table, .fs-kpi... nhung views dung inline styles / Bootstrap classes cu |
| 3 the he CSS chong cheo | HIGH | Template CSS (44K dong) + inline style (100-250 dong/trang) + design tokens |
| Bootstrap 4 (dashboards) vs Bootstrap 5 (customer) | MEDIUM | Grid behavior, JS API, data attributes khac nhau |
| Vendor bundle nang | HIGH | 53 libs x 3 roles = 159 vendor dirs, phan lon khong dung |
| Inline styles !important tran lan | HIGH | Moi view tu define .card, .table, .btn, .form-control voi !important |
| HTML markup cu, placeholder content | MEDIUM | Typos, missing tags, hardcoded English text, emoji icons |

### 1.2 Files hien tai

**Layouts (3 files can refactor):**
- Views/Shared/_LayoutPageAmin.cshtml (483 dong) - Admin
- Views/Shared/_LayoutPageShipper.cshtml (358 dong) - Shipper
- Views/Shared/_LayoutPageRestaurant.cshtml (404 dong) - Restaurant

**CSS monolithic (can xoa/giu lai phan dung):**
- Source/Admin/css/style-admin.css (44,271 dong)
- Source/Restaurant/css/style-restaurant.css (44,203 dong)
- Source/Shipper/css/style-shiper.css (44,267 dong)
- Source/Admin/css/admin.css (35 dong)

**Views can refactor HTML:**
- Admin: 17 views
- Shipper: 9 views
- Restaurant: 13 views

---

## 0. CUSTOMER-FACING UI (Truoc khi 3 Dashboards)

### 0.1 Danh gia tinh trang

**Diem tot (da hon 3 dashboards):**
- Da co BS5 (bootstrap.min.css = BS5.0.0)
- Da dung design tokens (fastship-design-tokens.css)
- Co skeleton loading, dark mode, PWA, skip-link
- FilterBar ViewComponent voi bottom-sheet mobile UX
- ChatWidget voi Gemini AI + SignalR
- Search autocomplete voi debounce 300ms
- Responsive design (mobile-first)

**Van de can fix:**

| Van de | Muc do | Chi tiet |
|--------|--------|----------|
| Inline styles trong layout | MEDIUM | _LayoutPageHome.cshtml co ~90 dong inline style (dropdown, hover, dark mode) |
| CSS overlap: style.css + layout-sg.css | MEDIUM | 2 CSS files chong cheo, style.css la template cu |
| DetailRestaurant.cshtml uses details.css | MEDIUM | CSS rieng, khong dung design tokens |
| Signup.cshtml 793 lines, inline styles | MEDIUM | Form dai, nhieu inline CSS |
| Cart/Index.cshtml fully inline CSS | HIGH | Toan bo trang gio hang style inline |
| Checkout.cshtml inline styles | MEDIUM | Address tabs, payment methods inline |
| OrderTracking.cshtml 664 lines | MEDIUM | Mixed inline + external CSS |
| Some pages use details.css (old template) | LOW | Khong dung design tokens |
| jQuery 3.4.1 CDN (cu) | LOW | Nen upgrade 3.6+ |

### 0.2 CSS hien tai cua Customer-facing

```
Source/Home/css/
  bootstrap.min.css     (BS5.0.0 - GIU)
  style.css             (Template cu - GIU phan can, xoa phan trung)
  layout-sg.css         (Sweetgreen overrides - CHINH, ~55K chars)
  login.css             (Auth pages - GIU)
  details.css           (Restaurant detail - CAN REFACTOR)
  base.css              (Reset - GIU)
  chat.css              (Messenger - GIU)
  font/                 (Custom fonts)
```

**Libraries (GIU tat ca):**
- animate/ - scroll animations
- owlcarousel/ - hero carousel
- easing/ - scroll easing
- waypoints/ - scroll trigger
- wow/ - scroll reveal

### 0.3 Views can refactor

| File | Van de | Sua |
|------|--------|-----|
| _LayoutPageHome.cshtml | ~90 dong inline style | Extract vao layout-sg.css |
| Home/Signup.cshtml | 793 lines, inline styles | Extract CSS, clean form |
| Cart/Index.cshtml | Fully inline CSS | Extract vao fs-cart.css |
| Cart/Checkout.cshtml | Inline styles | Extract vao fs-checkout.css |
| Cart/OrderTracking.cshtml | 664 lines, mixed CSS | Extract vao fs-tracking.css |
| Home/DetailRestaurant.cshtml | Uses details.css (old) | Chuyen sang design tokens |
| Home/Profile.cshtml | Can kiem tra | Dung design tokens |
| Home/Wallet.cshtml | Can kiem tra | Dung design tokens |

### 0.4 Customer-facing Action Items

```
C1.1  Extract inline styles tu _LayoutPageHome.cshtml vao layout-sg.css
C1.2  Cart/Index.cshtml: extract inline CSS vao fs-cart.css
C1.3  Checkout.cshtml: extract inline CSS vao fs-checkout.css
C1.4  OrderTracking.cshtml: extract inline CSS vao fs-tracking.css
C1.5  DetailRestaurant.cshtml: chuyen tu details.css sang design tokens
C1.6  Signup.cshtml: clean 793 lines, extract form styles
C1.7  Upgrade jQuery 3.4.1 -> 3.7+ (CDN)
C1.8  Kiem tra tat ca pages dung design tokens thay hardcoded colors
```

---

## II. HUONG GIAI QUYET (3 Dashboards)

### 2.1 Bootstrap Unify -> Bootstrap 5.3

**Ly do nang len BS5:**
- Da dung BS5 o customer-facing -> dong bo
- BS5 khong can jQuery cho JS components
- Better utility classes (gap-*, fs-*, etc.)
- data-bs-* attributes thay data-*

**Thay doi can thiet:**

```
BS4 -> BS5 migration:
- data-toggle="dropdown" -> data-bs-toggle="dropdown"
- data-target="#id" -> data-bs-target="#id"
- .ml-* -> .ms-* (margin-left -> margin-start)
- .mr-* -> .me-*
- .pl-* -> .ps-*
- .pr-* -> .pe-*
- .text-left -> .text-start
- .text-right -> .text-end
- .float-left -> .float-start
- .float-right -> .float-end
- .font-weight-* -> .fw-*
- .font-italic -> .fst-italic
- jQuery('.dropdown').dropdown() -> bootstrap.Dropdown (vanilla JS)
- <span class="badge badge-success"> -> <span class="badge bg-success">
- .form-group -> .mb-3 (BS5 removed .form-group)
- .custom-control -> .form-check
- .custom-select -> .form-select
```

**Source BS5 CSS/JS:** Dung CDN hoac local copy tu ~/Source/Home/css/bootstrap.min.css (da co BS5).

### 2.2 Design System Adopt

**Chien luoc:** Giu fastship-design-tokens.css lam core, them fs-dashboard-overrides.css cho shared dashboard styles.

**File CSS moi:**
```
Source/Shared/css/
  fastship-design-tokens.css      (GIU - 641 dong, core tokens)
  fastship-animations.css         (GIU - 117 dong)
  fs-dashboard-overrides.css     (TAO MOI - ~200 dong, shared cho ca 3 roles)
  fs-admin.css                   (TAO MOI - ~150 dong, admin-specific)
  fs-shipper.css                 (TAO MOI - ~150 dong, shipper-specific)
  fs-restaurant.css              (TAO MOI - ~150 dong, restaurant-specific)
```

### 2.3 Vendor Cleanup

**GIU (dung thuc su):**

| Vendor | Danh boi | Ly do |
|--------|-----------|-------|
| global/ (jQuery + Bootstrap bundle) | Tat ca | Core dependency |
| bootstrap-select/ | Tat ca | Enhanced dropdowns |
| chart.js/ | Admin, Restaurant | Dashboard charts |
| datatables/ | Restaurant, Shipper | Table sorting/pagination |
| metismenu/ | Tat ca | Sidebar navigation |
| waypoints/ + jquery.counterup/ | Admin, Restaurant | Counter animations |
| peity/ | Admin | Inline donut charts |
| perfect-scrollbar/ | Tat ca | Sidebar scroll |
| sweetalert2/ | Tat ca | Confirmation dialogs |
| toastr/ | Tat ca | Toast notifications |

**XOA (khong dung): 43 libs**

| Vendor | Ly do xoa |
|--------|-----------|
| amcharts/ | Khong dung - Chart.js thay the |
| animate/ | Khong dung - CSS animations thay the |
| aos/ | Khong dung - WOW.js thay the o Home |
| apexchart/ | Chi load nhung functions trong |
| bootstrap-daterangepicker/ | Khong dung |
| bootstrap-datetimepicker/ | Khong dung |
| bootstrap-material-datetimepicker/ | Khong dung |
| bootstrap-multiselect/ | Khong dung |
| bootstrap-tagsinput/ | Khong dung |
| bootstrap-touchspin/ | Khong dung |
| bootstrap-v4-rtl/ | Khong dung - RTL khong can |
| bootstrap4-notify/ | Khong dung - Toastr thay the |
| chartist/ + chartist-plugin-tooltips/ | Khong dung - Chart.js thay the |
| clockpicker/ | Khong dung |
| deznav/ | CSS da inline vao layout |
| dropzone/ | Khong dung |
| flot/ + flot-spline/ | Khong dung |
| fullcalendar/ | Khong dung |
| highlightjs/ | Khong dung |
| jquery-asColor/ + asColorPicker/ + asGradient/ | Khong dung |
| jquery-sparkline/ | Khong dung |
| jquery-steps/ | Khong dung |
| jquery-validation/ | Dung client-side validation cua BS5 |
| jqueryui/ | Khong dung |
| jqvmap/ | Khong dung |
| moment/ | Khong dung |
| morris/ | Khong dung |
| nestable2/ | Khong dung |
| nouislider/ | Khong dung |
| owl-carousel/ | Khong dung o dashboards |
| pickadate/ | Khong dung |
| raphael/ | Khong dung |
| select2/ | bootstrap-select thay the |
| summernote/ | Khong dung |
| svganimation/ | Khong dung |
| wnumb/ | Khong dung |

**Tom tat:** Giu 10 libs, xoa 43 libs -> giam ~80% vendor bundle.

**Luu y:** Xoa theo thu tu: xoa CSS/JS references trong layout TRUOC, roi xoa files. Kiem tra grep toan bo views truoc khi xoa.

### 2.4 Shared Dashboard Overrides (fs-dashboard-overrides.css)

Extract tu inline style hien tai - phan giong nhau giua 3 layouts:

```css
/* fs-dashboard-overrides.css - Shared for Admin/Restaurant/Shipper */

/* Card system - dung design tokens */
.fs-card {
    background: var(--fs-white);
    border: none !important;
    border-radius: var(--fs-radius) !important;
    box-shadow: var(--fs-shadow) !important;
}
.fs-card:hover {
    transform: translateY(-2px);
    box-shadow: var(--fs-shadow-lg) !important;
}

/* Table system */
/* Sidebar navigation */
/* Badge/status system */
/* Form controls */
/* Page header pattern */
/* Responsive table -> card conversion */
```

### 2.5 HTML Cleanup

**Admin views can sua:**

| File | Van de | Sua |
|------|--------|-----|
| _LayoutPageAmin.cshtml | Typo filename "Amin" | Rename -> _LayoutPageAdmin.cshtml |
| PostTaiKhoan.cshtml | Inline style="width:45%", .blog_area class | Dung BS5 grid + .fs-card |
| CreateCategory.cshtml | .blog_area + .form-horizontal, title sai | Dung BS5 form + .fs-card |
| EditCategory.cshtml | Giong tren | Giong tren |
| _ListCategory.cshtml | .table-striped .table-bordered, inline links | Dung .fs-table + .fs-btn |
| QuanLyQuanTriVien.cshtml | Class undefined, inline width | Dung .fs-table + BS5 classes |
| Dashboard.cshtml | Inline gradient colors | Dung var(--fs-*) tokens |
| DeliveryLogs.cshtml | Emoji in option | Thay boi text |
| Category.cshtml | Mixed class patterns | Unify voi design system |

**Shipper views can sua:**

| File | Van de | Sua |
|------|--------|-----|
| LichSu.cshtml | Raw Bootstrap table | Dung .fs-table |
| CaiDat.cshtml | Template-like, hardcoded stats | Modernize |
| ThuNhap.cshtml | Emoji icons | Font Awesome icons |
| ViTien.cshtml | Emoji | Font Awesome icons |
| _LayoutPageShipper.cshtml | Sidebar chi 4 items | Them Income, Notifications, Chat |
| Avatar dropdown | Inline styles + onmouseenter | Dung .fs-avatar-toggle |
| Status toggle | Inline button styles | Dung .fs-btn |

**Restaurant views can sua:**

| File | Van de | Sua |
|------|--------|-----|
| Profile.cshtml | Hardcoded English, missing >, typo | Fix HTML + content |
| Analytics.cshtml | Tabs bi comment out | Complete or remove |
| GeneralCustomer.cshtml | Fake data, English names | Replace with real data |
| OrderList.cshtml | Non-functional filters | Fix or remove |
| Discount.cshtml | Table co ban | Dung .fs-table |
| _LayoutPageRestaurant.cshtml | Sidebar thieu Wallet, Discount, Chat | Them links |
| Index.cshtml (Dashboard) | Inline styles tren KPI cards | Dung .fs-kpi |
| dashboard-1.js | Functions trong (gutted) | Restore hoac xoa |

---

## III. IMPLEMENTATION PLAN

### Phase 0: Preparation (Day 1)

```
1.1  Backup toan bo code (git branch: feat/ui-overhaul)
1.2  Grep tat ca vendor references trong views -> confirm libs thuc su dung
1.3  Test hien tai: chay app, confirm 3 roles hoat dong OK
1.4  Chup anh screenshots 3 roles hien tai (trước khi thay doi)
```

### Phase 1: Vendor Cleanup + CSS Foundation (Day 1-2)

```
2.1  Xoa 43 vendor libs khong dung (Admin/Restaurant/Shipper)
     - Xoa CSS references trong layout TRUOC
     - Xoa JS references trong layout TRUOC
     - Xoa vendor directories SAU
     - Kiem tra grep toan bo views truoc khi xoa

2.2  Tao fs-dashboard-overrides.css
     - Extract shared styles tu 3 inline <style> blocks
     - Dung design tokens (--fs-*) thay hardcoded values

2.3  Upgrade Bootstrap 4 -> 5.3 trong 3 layouts
     - Thay BS4 CSS bang BS5 CDN hoac local copy
     - Update data attributes (data-toggle -> data-bs-toggle)
     - Update utility classes (ml-* -> ms-*, etc.)
     - Update form markup (.custom-control -> .form-check)
     - Test sidebar metisMenu co hoat dong khong

2.4  Cleanup inline <style> trong 3 layouts
     - Giu phan BS5 override can thiet
     - Bo phan trung lap voi fs-dashboard-overrides.css
     - Chuyen hardcoded colors -> var(--fs-*)
```

### Phase 2: Layout Refactor (Day 2-3)

```
3.1  Admin Layout (_LayoutPageAmin.cshtml)
     - Fix typo filename -> _LayoutPageAdmin.cshtml
     - Update all view references
     - Replace inline styles voi fs-dashboard-overrides
     - Add skip-link accessibility
     - Clean sidebar: fix metisMenu for BS5

3.2  Shipper Layout (_LayoutPageShipper.cshtml)
     - Sidebar: them Income, Notifications, Chat links
     - Avatar dropdown: dung .fs-avatar-toggle
     - Status toggle: dung .fs-btn
     - Remove DataTables init from layout (move to specific pages)

3.3  Restaurant Layout (_LayoutPageRestaurant.cshtml)
     - Sidebar: them Wallet, Discount, Chat links
     - Avatar dropdown: dung .fs-avatar-toggle
     - Status toggle: dung .fs-btn
     - Remove unused script loads (apexchart, highlightjs)
     - Fix logo link (currently points to index.html)
```

### Phase 3: View-by-View Refactor (Day 3-7)

Thuc hien parallel cho 3 roles. Moi view:
1. Replace BS4 classes -> BS5
2. Replace inline styles -> design tokens / fs-* classes
3. Fix HTML bugs (missing tags, typos, placeholder content)
4. Replace emoji icons -> Font Awesome
5. Add loading states where missing

**Admin (11 views):**
```
Day 3: Dashboard.cshtml, QuanLyKhachHang.cshtml, QuanLyShipper.cshtml
Day 4: QuanLyQuanAn.cshtml, QuanLyQuanTriVien.cshtml, Order.cshtml
Day 5: OrderDetail.cshtml, EditOrder.cshtml, Category.cshtml + CRUD
Day 6: VoucherManager.cshtml, WalletManager.cshtml, DeliveryLogs.cshtml
Day 7: PostTaiKhoan.cshtml, AdminChat/Index.cshtml
```

**Shipper (9 views):**
```
Day 3: Index.cshtml (Dashboard), QRDelivery.cshtml
Day 4: OrderDetail.cshtml, ThuNhap.cshtml, ViTien.cshtml
Day 5: LichSu.cshtml, CaiDat.cshtml, ThongBao.cshtml, NhanTin.cshtml
```

**Restaurant (13 views):**
```
Day 3: Index.cshtml (Dashboard), OrderList.cshtml
Day 4: ProductList.cshtml, ProductDetail.cshtml
Day 5: Review.cshtml, Profile.cshtml, Analytics.cshtml
Day 6: Discount.cshtml, Wallet.cshtml, Scanner.cshtml
Day 7: NhanTin.cshtml, GeneralCustomer.cshtml
```

### Phase 4: Verification (Day 7-8)

```
4.1  Chay dotnet build - Xac nhan khong co compile error
4.2  Test Admin: Dashboard, CRUD users, Orders, Categories, Vouchers
4.3  Test Shipper: Dashboard, QR delivery, Income, History, Settings
4.4  Test Restaurant: Dashboard, Orders, Menu CRUD, Reviews, Profile
4.5  Test Customer: Homepage, Search, Restaurant Detail, Cart, Checkout, Order Tracking
4.6  Test responsive tren mobile
4.7  Chay E2E tests (neu co) - Xac nhan pass
4.8  Kiem tra performance: page load time truoc/sau
4.9  Chup anh screenshots moi
```

---

## IV. RISK MITIGATION

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| MetisMenu khong hoat dong voi BS5 | HIGH | HIGH | Test ngay sau upgrade. Fallback: dung Bootstrap 5 Nav/ScrollSpy thay metisMenu |
| Sidebar CSS bi break | MEDIUM | HIGH | Extract deznav CSS vao shared file truoc khi xoa vendor |
| Vendor xoa nhung con references | MEDIUM | HIGH | Grep toan bo views 3 lan truoc khi xoa |
| Dark mode bi anh huong | LOW | MEDIUM | Test dark mode sau moi phase |
| DataTables khong hoat dong voi BS5 | MEDIUM | MEDIUM | DataTables 1.13+ hoat dong voi BS5. Kiem tra version |

---

## V. SUCCESS CRITERIA

| Criteria | Metric |
|----------|--------|
| Vendor size giam | 53 -> 10 libs per role (~80% reduction) |
| CSS size giam | ~132K lines -> ~1K lines custom + 641 tokens |
| Inline styles gianh | 0 inline style blocks trong layouts |
| Design system adoption | 100% views dung fs-* classes |
| Bootstrap unify | Tat ca roles dung BS5 |
| HTML cleanup | 0 typos, 0 missing tags, 0 placeholder content |
| Accessibility | Skip-link, aria-labels, 44px touch targets |
| Performance | Page load < 5s (thay vi 19s) |
| Visual consistency | 3 roles co giao dien dong nhat |

---

## VI. ESTIMATED EFFORT

| Phase | Effort | Days |
|-------|--------|------|
| Phase 0: Preparation | 2h | 0.5 |
| Phase 0.5: Customer-facing cleanup | 12h | 2 |
| Phase 1: Vendor + CSS (dashboards) | 8h | 1.5 |
| Phase 2: Layout Refactor (dashboards) | 12h | 2 |
| Phase 3: View Refactor (33 dashboard views) | 40h | 5 |
| Phase 4: Verification | 8h | 1.5 |
| **TONG** | **~82h** | **~12 days** |

**Luu y:** Neu dung subagents parallel, co the giam xuong ~6-7 ngay.

**Customer-facing nho hon dashboards** vi da co BS5 + design tokens. Chi can extract inline styles va fix CSS overlap.
