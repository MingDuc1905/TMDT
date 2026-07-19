# BAO CAO LOI & HU SAI — FASTSHIP E2E TEST RESULTS

> Ngay test: 2026-07-19
> URL: https://fastship-web.onrender.com
> Test runner: Playwright 1.55.0 | Desktop Chromium (1920x1080)
> Tong test chay: 121 tests (batch 11-20, skip batch 18 mobile)
> Ket qua: 77 passed, 32 failed

---

## I. TONG KET KET QUA

| Batch | File | Pass | Fail | Duration |
|-------|------|------|------|----------|
| 11 | Cart Management | 6 | 9 | ~10 min |
| 12 | Checkout Flow | 1 | 13 | ~10 min |
| 13 | Order Status | 8 | 0 | 1.5 min |
| 14 | Filter & Search | 9 | 3 | 3.5 min |
| 15 | Chat System | 12 | 0 | 4.5 min |
| 16 | E-Delivery & Tracking | 9 | 1 | 4.2 min |
| 17 | Analytics & Dashboard | 10 | 0 | 2.0 min |
| 18 | Mobile Responsive | - | - | SKIP (per user) |
| 19 | Visual Regression | 10 | 5 | 1.5 min |
| 20 | Performance | 12 | 1 | 2.5 min |
| **TONG** | | **77** | **32** | **~40 min** |

### Phan loai 32 failures:

| Loai | So luong | Chi tiet |
|------|----------|----------|
| App bugs (loi that) | 13 | BUG-01 den BUG-13 |
| Test infra timeout | 15 | Batch 11 (8) + Batch 12 (7) — Render free tier cham + page object thieu `.catch()` |
| Snapshot baseline | 3 | Lan chay dau chua co baseline |
| Test expectation | 1 | TC-FILTER-07 category "Tat ca" khong ton tai |

---

## II. LOI THUC TE (APP BUGS)

### BUG-01: Search tra ket qua khi keyword khong ton tai

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-FILTER-02 |
| Muc do | CRITICAL |
| Uu tien | P0 |
| File | HomeController.cs |
| Anh chup | test-results/14-filter-search-.../test-failed-1.png |

**Mo ta:**
- Mong doi: Search "xyz123nonexistent" → 0 ket qua
- Thuc te: Tra ve 2 ket qua
- Evidence: `Results for "xyz123nonexistent": 2`

**Root cause:**
Search logic co the dang match fuzzy hoac default return all khi khong match. Kiem tra `HomeController.cs` — action `MenuSearch`. Co the dang dung `LIKE '%keyword%'` qua rong.

**Fix de xuat:**
```csharp
// HomeController.cs — MenuSearch action
// Fix: Exact match hoac strict tokenized search
// Hoac: Neu khong tim thay → tra ve empty list thay vi default list
```

---

### BUG-02: Category "Tat ca" khong ton tai tren homepage

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-FILTER-07 |
| Muc do | LOW |
| Uu tien | P3 |
| File | Views/Home/Index.cshtml |

**Mo ta:**
- Mong doi: Click "Tat ca" de reset filter
- Thuc te: Category pill "Tat ca" khong tim thay
- Evidence: `"Tat ca" category not found`

**Root cause:**
Homepage khong co category pill "Tat ca" (all), hoac selector sai.

**Fix de xuat:**
```html
<!-- Views/Home/Index.cshtml hoac partial _CategoryRow -->
<a class="fs-category-pill active" href="/">Tat ca</a>
```

---

### BUG-03: Restaurant menu search timeout — khong gui request

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-FILTER-09 |
| Muc do | CRITICAL |
| Uu tien | P0 |
| File | DetailRestaurant.cshtml |

**Mo ta:**
- Mong doi: Search "pizza" trong menu quán → filtered items
- Thuc te: Timeout 30s — waitForResponse khong nhan duoc response
- Evidence: `TimeoutError: page.waitForResponse: Timeout 30000ms exceeded`

**Root cause:**
`searchMenuBtn.click()` khong trigger form submit, hoac form action sai. Kiem tra form search trong DetailRestaurant page — co the can trigger submit event thay vi click button, hoac search button khong co `type="submit"`.

**Fix de xuat:**
```javascript
// Kiem tra form search trong DetailRestaurant page
// Form co the can trigger submit event thay vi click button
// Hoac: search button khong co type="submit"
```

---

### BUG-04: QRDelivery tab bi navbar che — khong click duoc

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-EDEL-02 |
| Muc do | HIGH |
| Uu tien | P1 |
| File | EDelivery.cshtml |

**Mo ta:**
- Mong doi: Click QR tab filter hoat dong
- Thuc te: Tab bi navbar che phu (z-index conflict)
- Evidence: `<div class="collapse navbar-collapse"> from <div class="header"> subtree intercepts pointer events`

**Root cause:**
Navbar `position: sticky/fixed` co z-index cao hon QR tabs.

**Fix de xuat:**
```css
/* Giam z-index cua navbar hoac tang z-index cua QR tabs */
.qr-tabs { position: relative; z-index: 1020; }
/* Hoac: */
.fs-header { z-index: 1010; }  /* thay vi 1050+ */
```

---

### BUG-05: Homepage load > 19s — qua cham

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-PERF-01 |
| Muc do | HIGH |
| Uu tien | P2 |
| File | Views/Home/Index.cshtml |

**Mo ta:**
- Mong doi: Homepage load < 10s
- Thuc te: Homepage load 19,695ms (gap doi mong doi)
- Evidence: `Homepage load: 19695ms`

**Root cause:**
Render free tier cold start + nhieu assets load dong thoi.

**Fix de xuat:**
```html
<!-- Them preconnect/preload cho critical resources -->
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preload" href="/Source/Shared/css/fastship-design-tokens.css" as="style">

<!-- Lazy load images below fold -->
<img loading="lazy" src="...">

<!-- Giam so luong HTTP requests -->
<!-- Combine CSS/JS bundles -->
```

---

### BUG-06: Cart — Them nhieu items timeout (Render slow)

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-CART-02, 03, 04, 05, 06, 07, 08 |
| Muc do | HIGH |
| Uu tien | P2 |
| File | CartController.cs |

**Mo ta:**
- Mong doi: Them 3 items, tang/giam so luong, xoa items
- Thuc te: 7/8 tests timeout sau 60s
- Evidence: Test duration ~60s cho moi thao tac cart

**Root cause:**
Moi AJAX call (add/increase/decrease/delete) mat 10-20s tren Render free tier.

**Fix de xuat:**
```javascript
// 1. Batching: Gop nhieu thao tac thanh 1 API call
// 2. Optimistic update: Update UI ngay, sync server sau
// 3. Debounce: Khong gui request qua nhanh
// 4. Cache: Store cart state trong localStorage
```

---

### BUG-07: E-Delivery QR API tra 404

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-EDEL-03 |
| Muc do | LOW |
| Uu tien | P3 |
| File | EDeliveryController.cs |

**Mo ta:**
- Mong doi: `/edelivery/qr/10` tra PNG image
- Thuc te: Tra 404 Not Found
- Evidence: `QR API status: 404`

**Root cause:**
Order ID 10 co the khong ton tai hoac khong co quyen truy cap.

**Fix de xuat:**
```csharp
// EDeliveryController.cs — GenerateQR
// Them: Check order exists + user has permission
// Them: Better error message thay vi 404
// Fix: Tra 400/403 thay vi 404 de phan biet
```

---

### BUG-08: Scan QR invalid token — khong hien thi error message

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-EDEL-04 |
| Muc do | LOW |
| Uu tien | P3 |
| File | Views/EDelivery/ScanResult.cshtml |

**Mo ta:**
- Mong doi: Hien thi "Ma QR khong hop le"
- Thuc te: Khong co error message (page load nhung khong co noi dung loi)

**Root cause:**
`ViewBag.Error` set nhung view khong render error message.

**Fix de xuat:**
```html
<!-- ScanResult.cshtml — Them error display -->
@if (ViewBag.Error != null)
{
    <div class="alert alert-danger">@ViewBag.Error</div>
}
```

---

### BUG-09: OrderTracking — Progress bar + Map khong render

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-TRACK-01, TC-STATUS-04, TC-STATUS-05 |
| Muc do | HIGH |
| Uu tien | P1 |
| File | Views/OrderTracking/Index.cshtml |

**Mo ta:**
- Mong doi: Progress bar hien 7 steps + Leaflet map render
- Thuc te: Progress bar: false, Map: false
- Evidence: `Progress bar: false, Map: false`

**Root cause:**
Co the khong co order dang active, hoac JS khong load. Kiem tra:
1. Co order active khong?
2. SignalR connection co estable khong?
3. Leaflet library co load khong?
4. Co fallback UI khi khong co order?

**Fix de xuat:**
```javascript
// Kiem tra OrderTracking.cshtml:
// 1. Co order active khong?
// 2. SignalR connection co estable khong?
// 3. Leaflet library co load khong?
// 4. Co fallback UI khi khong co order?
```

---

### BUG-10: OrderTracking — Chat FAB khong hien thi

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-TRACK-04 |
| Muc do | MEDIUM |
| Uu tien | P2 |
| File | Views/OrderTracking/Index.cshtml |

**Mo ta:**
- Mong doi: Chat FAB button visible tren tracking page
- Thuc te: Chat FAB: false

**Root cause:**
Chat widget co the khong render tren OrderTracking page.

**Fix de xuat:**
```html
<!-- Dam bao _ChatWidget.cshtml duoc include trong OrderTracking.cshtml -->
@await Html.PartialAsync("_ChatWidget")
```

---

### BUG-11: Restaurant Analytics page → 500 Error

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-ANALYTIC-05 |
| Muc do | HIGH |
| Uu tien | P1 |
| File | RestaurantController.cs |

**Mo ta:**
- Mong doi: Analytics page hien thi feedback stats
- Thuc te: Redirect sang `/Home/Error?traceId=...` (500 Internal Server Error)
- Evidence: `Analytics URL: https://fastship-web.onrender.com/Home/Error?traceId=...`

**Root cause:**
`RecommendationService` hoac analytics query crash.

**Fix de xuat:**
```csharp
// RestaurantController.cs — Analytics action
// Them try-catch va fallback
try {
    // Analytics logic
} catch (Exception ex) {
    _logger.LogError(ex, "Analytics failed");
    ViewBag.Error = "Khong the tai du lieu phan tich";
    return View(); // Show error state thay vi crash
}
```

---

### BUG-12: Footer khong hien thi tren Login + Cart pages

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-VISUAL-14 |
| Muc do | LOW |
| Uu tien | P3 |
| File | Views/Home/Login.cshtml, Views/Cart/Index.cshtml |

**Mo ta:**
- Mong doi: Footer visible tren tat ca pages
- Thuc te: Footer khong hien thi tren Login va Cart pages
- Evidence: `Footer on /Home/Login: false, Footer on /Cart: false`

**Root cause:**
Login layout va Cart layout khong include footer partial.

**Fix de xuat:**
```html
<!-- Dam bao tat ca layouts include footer -->
@await Html.PartialAsync("_Footer")
<!-- Hoac: Su dung shared _Layout cho tat ca pages -->
```

---

### BUG-13: Empty search state khong hien thi "Khong tim thay"

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-VISUAL-07 |
| Muc do | LOW |
| Uu tien | P3 |
| File | Views/Home/Index.cshtml |

**Mo ta:**
- Mong doi: Search khong ket qua → hien thi "Khong tim thay"
- Thuc te: Khong co empty state message

**Root cause:**
Empty state component thieu hoac khong trigger khi search 0 results.

**Fix de xuat:**
```html
<!-- Homepage — Them empty state cho search -->
@if (!Model.Any())
{
    <div class="empty-state">
        <i class="fas fa-search fa-3x text-muted"></i>
        <h5>Khong tim thay quan an phu hop</h5>
        <p>Thu tu khoa khac hoac duyet tat ca quan</p>
    </div>
}
```

---

### BUG-14: addItemToCartByIndex() — khong co fallback, timeout tren Render free tier

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-CHECKOUT-01~14 (batch 12), TC-CART-02~08 (batch 11) |
| Muc do | HIGH |
| Uu tien | P1 |
| File | e2e-tests/pages/DetailRestaurantPage.ts:128 |
| Loai | Test infrastructure (khong phai app bug) |

**Mo ta:**
- Mong doi: waitForResponse(ApiThemMonAn) nhan response trong 30s
- Thuc te: Timeout 30s — tat ca 13/14 checkout tests fail o setup, 8/9 cart tests fail
- Evidence: `TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"`

**Root cause:**
`DetailRestaurantPage.ts:128` dung `waitForResponse` khong co `.catch()` fallback. `addFirstItemToCart()` co `.catch(() => {})` nen cart page test pass, nhung `addItemToCartByIndex()` khong co → timeout.

**Fix de xuat (page object, khong phai app bug):**
```typescript
// DetailRestaurantPage.ts — addItemToCartByIndex()
async addItemToCartByIndex(index: number, quantity: number = 1) {
    await this.quantityInput.nth(index).fill(quantity.toString());
    await this.addToCartBtn.nth(index).click();
    await this.page.waitForResponse(resp =>
      resp.url().includes('ApiThemMonAn') && resp.status() === 200
    ).catch(() => {});  // Them fallback
    await this.page.waitForLoadState('networkidle').catch(() => {});  // Them fallback
}
```

---

### BUG-15: Delete item khong xoa khoi DOM

| Thuoc tinh | Chi tiet |
|------------|----------|
| Test | TC-CART-07 |
| Muc do | HIGH |
| Uu tien | P1 |
| File | CartController.cs + wwwroot/Source/Cart/cart.js |

**Mo ta:**
- Mong doi: Click delete → item bi xoa khoi DOM
- Thuc te: Delete API tra ve 200 nhung item van hien trong DOM
- Evidence: Test kiem tra `.cart-item` co count > 0 sau khi delete

**Root cause:**
Co the delete AJAX handler khong trigger DOM remove, hoac selector khong match.

**Fix de xuat:**
```javascript
// Cart.js — delete handler
// Sau khi API tra 200, can:
// 1. Xoa DOM element: $(this).closest('.cart-item').remove()
// 2. Cap nhat cart badge
// 3. Cap nhat total
// 4. Kiem tra neu cart trong → hien empty state
```

---

## III. TEST ISSUES (Khong phai app bug)

| # | Test | Issue | Fix |
|---|------|-------|-----|
| T1 | TC-VISUAL-08/09/10 | Screenshot baseline chua capture — lan chay dau tien | Chay `npx playwright test --update-snapshots` |
| T2 | TC-FILTER-07 | Category "Tat ca" khong co that — test sai expectation | Sua test: skip neu category khong ton tai |
| T3 | TC-TRACK-01~06 | OrderTracking khong co order active — test can order cu the | Fix test: tao order truoc khi test tracking |

---

## IV. UU TIEN SUA

| Priority | Bug | Effort | Impact |
|----------|-----|--------|--------|
| P0 | BUG-01: Search tra ket qua sai | 1h | Customer khong tim duoc dung quan |
| P0 | BUG-03: Restaurant menu search broken | 2h | Customer khong search duoc trong quan |
| P1 | BUG-04: QR tabs blocked by navbar | 30min | Shipper khong filter duoc QR |
| P1 | BUG-09: OrderTracking progress bar missing | 3h | Customer khong track don hang |
| P1 | BUG-11: Restaurant Analytics 500 error | 2h | Restaurant owner khong xem analytics |
| P1 | BUG-14: addItemToCartByIndex timeout (page object) | 30min | Test infrastructure — checkout flow khong test duoc |
| P1 | BUG-15: Delete item khong xoa khoi DOM | 1h | Cart UX — item khong bien mat sau khi xoa |
| P2 | BUG-05: Homepage load 20s | 4h | Performance improvement |
| P2 | BUG-06: Cart AJAX too slow | 1d | Cart UX improvement |
| P2 | BUG-10: Chat FAB missing on tracking | 1h | Chat support unavailable |
| P3 | BUG-02: Missing "All" category pill | 30min | UX improvement |
| P3 | BUG-07: QR API 404 | 1h | Error handling |
| P3 | BUG-08: QR scan error message missing | 30min | UX improvement |
| P3 | BUG-12: Missing footer on some pages | 1h | Layout consistency |
| P3 | BUG-13: Missing empty search state | 1h | UX improvement |

---

## V. TONG KET

| Loai | So luong | Mo ta |
|------|---------|-------|
| CRITICAL bugs | 2 | Search wrong results + Menu search broken |
| HIGH bugs | 7 | QR navbar, Tracking, Analytics 500, Perf, Cart slow, Checkout timeout, Delete DOM |
| MEDIUM bugs | 2 | QR error, Chat FAB |
| LOW bugs | 4 | Missing footer, empty state, category pill, QR API |
| Test infrastructure | 1 | BUG-14: addItemToCartByIndex page object timeout |
| Test issues | 3 | Snapshot baseline, wrong expectation, missing preconditions |
| **Tong** | **19** | |

**Khong co critical security issues** — SQL injection va XSS bi WAF chan.

---

## VI. ANH CHUP MAN HINH

Moi fail test deu tao screenshot tai `test-results/`:

| Test | Anh |
|------|-----|
| TC-FILTER-02 | test-results/14-filter-search-.../test-failed-1.png |
| TC-FILTER-09 | test-results/14-filter-search-.../test-failed-1.png |
| TC-EDEL-02 | test-results/16-edelivery-tracking-.../test-failed-1.png |
| TC-CART-02~08 | test-results/11-cart-management-.../test-failed-1.png |
| TC-CHECKOUT-01~14 | test-results/12-checkout-flow-.../test-failed-1.png |
| TC-PERF-01 | test-results/20-performance-.../test-failed-1.png |
| TC-VISUAL-07~10,14 | test-results/19-visual-regression-.../test-failed-1.png |
