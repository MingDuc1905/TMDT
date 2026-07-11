# FastShip Bug Fix Plan — 11 Tasks

> **For agentic workers:** Use `subagent-driven-development` to implement tasks in parallel.
> Skills used: `ponytail`, `systematic-debugging`, `ui-ux-pro-max`, `dispatching-parallel-agents`

**Goal:** Fix 9 reported bugs + 2 feature improvements in FastShip food delivery platform

**Architecture:** ASP.NET Core 8 MVC (C# Razor) + PostgreSQL + SignalR

---

## Task 1: Reviews từ trang chủ (Bug #1)

**Files:**
- Modify: `ShipFoodCore/Views/Home/Index.cshtml` 
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Skills:** `ponytail`

**Changes:**
- Thêm anchor `#reviews-section` vào link "bình luận" trên thẻ quán ăn
- Thêm JS để auto-scroll xuống reviews section khi URL có hash
- Thêm `id="reviews-section"` vào container reviews trong DetailRestaurant.cshtml

---

## Task 2: Lọc thực đơn restaurant (Bug #2)

**Files:**
- Modify: `ShipFoodCore/Controllers/HomeController.cs`
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Skills:** `systematic-debugging`, `ponytail`

**Root cause:**
- DetailRestaurant action dùng `idDM` filter nhưng view dùng `thucdon.madanhmuc` trong href
- Cần kiểm tra `ViewBag.idDM == thucdon.madanhmuc` active state có đúng ko

**Changes:**
- Debug filter logic trong `HomeController.DetailRestaurant`
- Fix active class cho category pills

---

## Task 3: Badge % khuyến mãi & UI (Bug #3)

**Files:**
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Skills:** `ui-ux-pro-max`, `ponytail`

**Changes:**
- Cải thiện UI badge %: thêm tooltip "Giảm X% từ chương trình khuyến mãi"
- Thêm màu sắc nổi bật hơn cho badge
- Hiển thị "Giá gốc" + "Giá KM" rõ ràng hơn

---

## Task 4: Mất ảnh gợi ý món ăn (Bug #4)

**Files:**
- Modify: `ShipFoodCore/Utils/TinhToan.cs`
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Skills:** `ponytail`, `systematic-debugging`

**Root cause:**
- `TinhToan.HinhAnhUrl(null)` trả về data URI 1x1 transparent thay vì placeholder
- Recommendation items có `hinhanh` = null trong DB → ko hiện ảnh

**Changes:**
- Cập nhật `HinhAnhUrl()`: trả về placeholder image thay vì transparent 1x1
- Thêm `onerror` fallback cho img tags trong recommendations

---

## Task 5: Đặt nhiều quán trong 1 đơn (Bug #5)

**Files:**
- Modify: `ShipFoodCore/Models/Cart.cs`
- Modify: `ShipFoodCore/Controllers/CartController.cs`
- Modify: `ShipFoodCore/Controllers/BaseController.cs`
- Modify: `ShipFoodCore/Controllers/PaymentController.cs`
- Modify: `ShipFoodCore/Views/Cart/Checkout.cshtml`
- Modify: `ShipFoodCore/Views/Cart/Index.cshtml`
- Modify: `ShipFoodCore/Views/Home/DetailRestaurant.cshtml`

**Skills:** `brainstorming`, `writing-plans`, `ponytail`

**User requirement:** Gộp món từ nhiều quán vào 1 đơn, 1 shipper lấy giao 1 lần

**Design decisions:**
- Cart model: bỏ `maquanan` single-restaurant constraint
- CartItem đã có `maquanan` riêng → dùng để phân biệt quán
- Khi checkout: tạo 1 đơn với items từ nhiều quán, lưu thông tin quán trong `tbChiTietDonHang` (cần thêm cột `maquan` vào `tbChiTietDonHang`)
- Payment: tính ship phí riêng theo từng quán hoặc tính 1 lần
- Shipper view: hiển thị nhiều điểm lấy hàng trên map

**Changes:**
1. Cart.cs: loại bỏ single `maquanan`, hỗ trợ multi-restaurant
2. CartController: sửa `ThemMonAn`, `ApiThemMonAn` để ko conflict khi khác quán
3. PaymentController: sửa `ProcessPayment` để tạo 1 đơn với items từ nhiều quán
4. DbContext: thêm FK `tbChiTietDonHang.maquan → tbQuanAn.userid`
5. Checkout.cshtml: hiển thị items theo từng quán
6. Cart/Index.cshtml: hiển thị items theo quán

---

## Task 6: Search riêng món ăn (Bug #6)

**Files:**
- Modify: `ShipFoodCore/Views/Shared/_LayoutPageHome.cshtml`
- Modify: `ShipFoodCore/wwwroot/js/filter.js`
- Modify: `ShipFoodCore/Controllers/HomeController.cs`

**Skills:** `ui-ux-pro-max`, `ponytail`

**User requirement:** Search riêng món ăn (ko hiện quán)

**Changes:**
- Navbar: thêm tab/switch "Tìm quán" vs "Tìm món"
- `_LayoutPageHome.cshtml`: sửa search form gọi `MenuSearch` API khi ở chế độ "Tìm món"
- Hiển thị kết quả món ăn dạng grid cards (giống search results của filter.js)
- Autocomplete: gợi ý tên món khi gõ

---

## Task 7: Database trùng (Bug #7)

**Files:**
- Modify: `seed.sql`
- Modify: `ShipFoodCore/Controllers/CartController.cs`

**Skills:** `systematic-debugging`

**Root cause:**
- `tbThongTinDatHang` có 20+ bản ghi "Trần Thị B" giống hệt nhau do seed hoặc cách xử lý save address

**Changes:**
- Check seed.sql: xoá duplicate INSERT `tbThongTinDatHang`
- CartController: kiểm tra trùng trước khi thêm địa chỉ mới vào db
- Thêm message cho user khi địa chỉ đã tồn tại

---

## Task 8: Địa chỉ trùng ở checkout (Bug #8)

**Files:**
- Modify: `ShipFoodCore/Views/Cart/Checkout.cshtml`

**Skills:** `ponytail`

**Changes:**
- Gom nhóm địa chỉ trùng bằng Distinct() hoặc GroupBy
- Chỉ hiển thị 1 địa chỉ duy nhất
- Thêm `Distinct()` trong query `db.tbThongTinDatHang` hoặc xử lý client-side

---

## Task 9: QR chuyển khoản ko hiện (Bug #9)

**Files:**
- Modify: `ShipFoodCore/Controllers/PaymentController.cs`
- Modify: `ShipFoodCore/Views/Cart/Checkout.cshtml`

**Skills:** `ponytail`, `systematic-debugging`

**Root cause:**
- `PaymentController.ProcessPayment` tạo QR thành công trong response JSON
- Nhưng `showResultPopup` trong Checkout.cshtml chỉ hiển thị QR nếu response có `qrCodeUrl`
- Cần kiểm tra `isBankTransfer` flag và `BankId/BankAccountNo` config

**Changes:**
- Check `IsBankTransferMethod()` có detect đúng payment method ko
- Kiểm tra `BankId`, `BankAccountNo`, `BankAccountName` có được config trong appsettings.json ko
- Thêm config fallback rõ ràng

---

## Task 10: 🎫 Voucher như Grab/ShopeeFood (Feature)

**Files:**
- Modify: `ShipFoodCore/Views/Cart/Checkout.cshtml`
- Modify: `ShipFoodCore/Views/Cart/Index.cshtml`
- Modify: `ShipFoodCore/Controllers/CartController.cs`
- Modify: `ShipFoodCore/wwwroot/js/cart-local.js`

**Skills:** `ui-ux-pro-max`, `ponytail`

**User requirement:** Voucher chọn dễ như Grab/ShopeeFood — giá thay đổi real-time, tự tính khi thêm món

**Changes:**

### 10a. API: Cải thiện `CheckCoupon`
- `CartController.CheckCoupon`: trả về `newTotal` (tổng sau giảm) để front end ko cần tự tính
- Thêm `CartController.RemoveCoupon` API — bỏ chọn voucher, trả về giá gốc

### 10b. UI: Popup chọn voucher giống Grab
- Checkout.cshtml: thiết kế lại popup voucher:
  - Card từng voucher: % giảm, điều kiện (VD: "Đơn từ 50K"), hạn sử dụng
  - Checkbox/Radio tích chọn 1 voucher
  - Nút "Áp dụng" / "Bỏ chọn"
  - Hiển thị số tiền giảm ngay trong popup

### 10c. Real-time price update
- Khi chọn voucher → gọi API CheckCoupon → cập nhật `#order-total` ngay lập tức
- Khi bỏ voucher → gọi API RemoveCoupon → tổng về giá gốc
- Khi thêm/bớt món trong cart → tự động tính lại discount (gọi lại CheckCoupon)

### 10d. Cart/Index voucher suggestions
- Hiển thị voucher gợi ý ở trang Cart (đã có code `#cart-coupon-suggestions`)
- Click voucher → chuyển đến Checkout và tự động áp dụng

---

## Task 11: 🎨 Lucide Icons — Thay thế Font Awesome (Feature)

**Files:**
- Modify: ~20+ files (tất cả `.cshtml` views)
- Add: Lucide CDN vào layout files

**Skills:** `ponytail`, `ui-styling`

**User requirement:** Thay toàn bộ Font Awesome icons bằng Lucide Icons SVG từ unpkg CDN

**Icon mapping (FA → Lucide):**

| Font Awesome | Lucide Name | Usage Area |
|-------------|-------------|-----------|
| `fa-star` | `star` | Rating stars |
| `fa-shopping-bag` | `shopping-bag` | Cart icon |
| `fa-store` | `store` | Restaurant |
| `fa-motorcycle`, `fa-truck` | `bike`, `truck` | Delivery |
| `fa-map-marker-alt` | `map-pin` | Address/location |
| `fa-search` | `search` | Search |
| `fa-user` | `user` | User profile |
| `fa-phone` | `phone` | Phone contact |
| `fa-tag` | `tag` | Coupon/discount |
| `fa-arrow-right` | `arrow-right` | Navigation |
| `fa-chevron-right` | `chevron-right` | Expand |
| `fa-times` | `x` | Close |
| `fa-check` | `check` | Confirm |
| `fa-check-circle` | `check-circle` | Success |
| `fa-exclamation-circle` | `alert-circle` | Error/warning |
| `fa-spinner` | `loader` | Loading |
| `fa-info-circle` | `info` | Info |
| `fa-eye` | `eye` | View details |
| `fa-edit` | `edit` | Edit |
| `fa-trash` | `trash-2` | Delete |
| `fa-clock` | `clock` | Time |
| `fa-history` | `history` | History |
| `fa-comment`, `fa-comment-alt` | `message-circle` | Reviews |
| `fa-robot` | `bot` | AI suggestions |
| `fa-fire` | `flame` | Trending/hot |
| `fa-utensils`, `fa-coffee` | `utensils`, `coffee` | Food/drink categories |
| `fa-dollar-sign`, `fa-money-bill-wave` | `dollar-sign` | Payment |
| `fa-credit-card` | `credit-card` | Payment method |
| `fa-university` | `building` | Bank transfer |
| `fa-bell` | `bell` | Notification |
| `fa-sync`, `fa-redo`, `fa-refresh` | `refresh-cw` | Refresh |
| `fa-chart-line` | `trending-up` | Analytics |
| `fa-chart-pie` | `pie-chart` | Charts |
| `fa-trophy` | `trophy` | Top ranking |
| `fa-users` | `users` | Users management |
| `fa-cog`, `fa-settings` | `settings` | Settings |
| `fa-sign-out-alt` | `log-out` | Logout |
| `fa-paper-plane` | `send` | Send message |
| `fa-phone` | `phone` | Call |
| `fa-envelope` | `mail` | Email |
| `fa-home` | `home` | Home |
| `fa-th-list` | `list` | Category list |

**Changes:**

### 11a. Add Lucide CDN to layouts
- `_LayoutPageHome.cshtml`: thêm `<script src="https://unpkg.com/lucide@latest"></script>`
- `_LayoutPageAmin.cshtml`: thêm Lucide CDN
- `_LayoutPageRestaurant.cshtml`: thêm Lucide CDN
- `_LayoutPageShipper.cshtml`: thêm Lucide CDN

### 11b. Lucide helper function
Tạo file `wwwroot/js/lucide-icons.js` với helper:
```javascript
function li(name, size, color) {
    // <img src="https://unpkg.com/lucide-static@latest/icons/{name}.svg" 
    //      style="width:{size}px;height:{size}px" />
}
```

### 11c. Replace in all views
Dùng regex thay thế:
- `<i class="fa[s]?[rl]? ..."></i>` → `<img src="https://unpkg.com/lucide-static@latest/icons/{name}.svg" ... />`
- Thêm class `.lucide-icon` với CSS `width:16px;height:16px;vertical-align:middle`

### 11d. CSS utility classes
```css
.lucide-icon { 
  width: 16px; height: 16px; 
  vertical-align: middle; 
  display: inline-block;
}
.lucide-icon-sm { width: 12px; height: 12px; }
.lucide-icon-lg { width: 24px; height: 24px; }
.lucide-icon-xl { width: 32px; height: 32px; }
```

---

## Execution Order

1. **Phase 1 — Dọn dẹp dữ liệu**
   - Task 7 (DB trùng) + Task 8 (địa chỉ trùng)

2. **Phase 2 — UI fixes nhỏ**
   - Task 4 (Missing images)
   - Task 1 (Homepage reviews)
   - Task 2 (Menu filter)
   - Task 3 (Badge % cải thiện)

3. **Phase 3 — Thanh toán & Tìm kiếm**
   - Task 9 (QR code)
   - Task 6 (Food search)
   - Task 10 (Voucher như Grab/ShopeeFood)

4. **Phase 4 — Icon overhaul**
   - Task 11 (Lucide Icons thay FA) — làm cuối vì ảnh hưởng nhiều file nhất

5. **Phase 5 — Tính năng lớn**
   - Task 5 (Multi-restaurant) — làm cuối vì thay đổi kiến trúc
