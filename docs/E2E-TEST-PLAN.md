# 📋 Kế Hoạch Kiểm Thử E2E — FastShip (ShipFood)

> **Phiên bản**: 1.0  
> **Ngày**: 29/07/2026  
> **Môi trường**: Desktop (1920×1080) — bỏ qua Mobile  
> **Framework**: Playwright + Lightpanda Browser  
> **Base URL**: `https://fastship-web.onrender.com`

---

## 🎯 Mục Tiêu

Kiểm thử **toàn bộ 55+ trang** của FastShip, bao gồm **4 roles**:

| Role | Số trang | Mô tả |
|------|----------|-------|
| 🛍️ **Khách hàng** | ~15 trang | Trang chủ, chi tiết quán, giỏ hàng, checkout, lịch sử, chat |
| 🏪 **Quán ăn** | ~12 trang | Dashboard, đơn hàng, sản phẩm, khuyến mãi, analytics |
| 🚚 **Shipper** | ~9 trang | Dashboard, FREE-PICK, ví tiền, thu nhập, QR delivery |
| 👑 **Admin** | ~14 trang | Dashboard, quản lý user/đơn hàng/danh mục, chat, delivery logs |

**Phạm vi kiểm thử**:
- ✅ Chức năng (từng nút bấm, form, API)
- ✅ Hiển thị dữ liệu (ảnh, số liệu, bảng biểu)
- ✅ Luồng nghiệp vụ (đặt hàng → thanh toán → giao hàng → đánh giá)
- ✅ Lỗi 404/500, console errors, broken images
- ❌ Bỏ qua bảo mật chuyên sâu (SQLi, XSS — đã có trong test cũ)
- ❌ Bỏ qua Mobile/responsive

---

## 🧪 Kiến Trúc Test

### Test Runner
- **Playwright** (v1.61.1) — chạy trên Desktop Chromium
- **Lightpanda** (tùy chọn) — browser nhẹ hơn 9x cho CI/CD

### Test Data
```
Tài khoản seed từ seed.sql:
  Khách hàng: tranthib / abcdef
  Quán ăn:    konekopizza / konekopizza
  Shipper:    shipperz / shipz789
  Admin:      admin1 / admin1
```

### Page Object Model
```
pages/
├── BasePage.ts          — Navigation, screenshot, image validation, toast
├── HomePage.ts          — Trang chủ, tìm kiếm, filter chips
├── LoginPage.ts         — Đăng nhập, error messages
├── CartPage.ts          — Giỏ hàng (items, quantity, delete)
├── CheckoutPage.ts      — Thanh toán (address, payment, coupon, submit)
├── DetailRestaurantPage.ts — Chi tiết quán (menu, add-to-cart, reviews)
├── RestaurantPage.ts    — Dashboard quán (KPI, orders, sidebar)
├── ShipperPage.ts       — Dashboard shipper (FREE-PICK, wallet, map)
└── AdminPage.ts         — Dashboard admin (KPI, charts, user management)
```

---

## 📊 Ma Trận Kiểm Thử Chi Tiết

### 🛍️ PHẦN 1: KHÁCH HÀNG (Customer) — ~80 Test Cases

#### 1A. Trang chủ (`/`) — 10 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-H-01 | Hero carousel hiển thị + auto-play | Load trang → chờ 3s → kiểm tra slide active | Carousel có `#header-carousel`, >0 slides, nút prev/next click được | Cao |
| TC-H-02 | Filter chips — click từng cái | Load trang → click chip "Đồ ăn" | URL chứa `?idDM=`, danh sách quán thay đổi theo category | Cao |
| TC-H-03 | Search "pizza" → kết quả chứa "Koneko Pizza" | Nhập "pizza" vào search → submit | URL chứa `?txtSearch=pizza`, danh sách quán > 0 | Cao |
| TC-H-04 | Search không có kết quả | Nhập "xyznonexist123" → submit | Hiển thị thông báo "Không tìm thấy" | Trung |
| TC-H-05 | Stats row hiển thị số liệu | Load trang → scroll xuống stats | 4 stat items: "Đối tác", "Món ăn", "Đơn hàng", "Khách hàng" — mỗi cái có số > 0 | Cao |
| TC-H-06 | Restaurant grid — click vào quán | Click vào card quán đầu | Redirect đến `/Home/DetailRestaurant?id=X` | Cao |
| TC-H-07 | Promo band dismiss | Click nút X trên promo band | Promo band ẩn đi | Thấp |
| TC-H-08 | Footer — tất cả links hoạt động | Scroll xuống footer → click từng link | Footer hiển thị 3 sections: "Khám phá", "Hỗ trợ", "Liên hệ" | Trung |
| TC-H-09 | Ảnh không bị broken | Load trang → check all `<img>` | 0 broken images (naturalWidth > 0) | Cao |
| TC-H-10 | 0 JS errors | Load trang → capture console errors | 0 page errors | Cao |

#### 1B. Đăng nhập (`/Home/Login`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-L-01 | Login UI — tất cả elements hiển thị | Load trang login | Username input, password input, Login button, Remember Me checkbox, Google OAuth button, Register link đều hiển thị | Cao |
| TC-L-02 | Login sai mật khẩu | Nhập đúng username, sai password → click Login | Error alert "Mật khẩu không đúng", URL vẫn là `/Home/Login` | Cao |
| TC-L-03 | Login tài khoản không tồn tại | Nhập username không có → click Login | Error alert "không tồn tại" | Cao |
| TC-L-04 | Login bỏ trống username | Để trống username → click Login | HTML5 validation: "Please fill out this field", không submit được | Cao |
| TC-L-05 | Login đúng → redirect | Nhập `tranthib` / `abcdef` → Login | Redirect về trang chủ, user dropdown (avatar) hiển thị | Critical |
| TC-L-06 | Remember Me — session persist | Login với Remember Me → redirect | User vẫn đăng nhập sau redirect | Trung |
| TC-L-07 | Forgot password link hoạt động | Click "Quên mật khẩu" | Redirect đến `/Home/Forgot` | Thấp |
| TC-L-08 | Register link hoạt động | Click "Đăng ký" | Redirect đến `/Home/Signup` | Thấp |

#### 1C. Chi tiết quán (`/Home/DetailRestaurant?id=X`) — 10 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-DR-01 | Trang load — tên quán + địa chỉ | Mở Koneko Pizza (id=6) | Tên quán hiển thị, địa chỉ hiển thị, rating hiển thị | Critical |
| TC-DR-02 | Menu items hiển thị | Load trang → đếm `.item-restaurant-row` | Số món > 0, mỗi món có tên + giá + ảnh | Critical |
| TC-DR-03 | Giá món hiển thị đúng format | Kiểm tra `.current-price` text | Format "40,000₫" hoặc "50.000₫" (VND) | Cao |
| TC-DR-04 | Category pills lọc menu | Click category "Món chính" | Menu items lọc theo category | Cao |
| TC-DR-05 | Tìm kiếm trong menu | Nhập "pizza" vào search menu → submit | Menu items lọc theo tên | Trung |
| TC-DR-06 | Thêm món vào giỏ (chưa login) | Điền số lượng = 2 → click "Thêm vào giỏ" | AJAX success response, cart badge cập nhật | Critical |
| TC-DR-07 | Discount badge hiển thị (nếu có KM) | Load quán có khuyến mãi | Badge "-20%" góc ảnh, giá cũ gạch ngang + giá mới màu đỏ | Cao |
| TC-DR-08 | Out-of-stock badge + disabled button | Tìm món có `soluong == 0` | Badge "Cháy hàng", nút Thêm disabled | Cao |
| TC-DR-09 | Review section — hiển thị đánh giá | Scroll đến review list | Danh sách review có avatar + tên + sao + nội dung | Trung |
| TC-DR-10 | "Đã mua" filter | Click "Đã mua" | Chỉ hiển thị món đã từng mua | Trung |

#### 1D. Chi tiết sản phẩm (`/Home/ChiTietSanPham?id=X`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SP-01 | Product hero — ảnh + tên + giá | Mở chi tiết sản phẩm | Ảnh lớn, tên món, giá, mô tả hiển thị | Cao |
| TC-SP-02 | Size chips (M/L/XL) | Check size selector | 3 size chips click được, giá thay đổi theo size | Cao |
| TC-SP-03 | Add to cart từ chi tiết | Chọn size → nhập số lượng → click Thêm | Thêm vào giỏ thành công | Critical |
| TC-SP-04 | Similar items — gợi ý món tương tự | Scroll xuống "Món tương tự" | Hiển thị ≥2 món cùng danh mục | Trung |
| TC-SP-05 | Reviews — xem thêm paginate | Click "Xem thêm" | Load thêm 6 review mỗi lần | Trung |
| TC-SP-06 | Submit review từ chi tiết | Chọn sao + nhập nhận xét → gửi | Review hiển thị trong danh sách | Cao |

#### 1E. Giỏ hàng (`/Cart`) — 12 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-CRT-01 | Giỏ trống — empty state | Mở cart khi chưa có item | Hiển thị "Giỏ hàng trống" + nút "Khám phá quán ăn" | Cao |
| TC-CRT-02 | Thêm 1 món → cart badge = 1 | Login → thêm món | Badge `#navCartBadge` hiển thị count ≥ 1 | Critical |
| TC-CRT-03 | Thêm 3 món → tổng tiền hiển thị | Thêm 3 món khác nhau → vào cart | 3 items trong cart, total > 0 | Critical |
| TC-CRT-04 | Tăng số lượng 1→3 → total tăng | Click nút "+" 2 lần | Số lượng hiển thị = 3, tổng tăng gấp 3 | Cao |
| TC-CRT-05 | Giảm số lượng về 0 → item xoá | Click nút "-" khi qty=1 | Item biến mất khỏi cart | Cao |
| TC-CRT-06 | Xoá item | Click nút "Xoá" | Item biến mất, cart count giảm | Cao |
| TC-CRT-07 | Xoá tất cả → empty state | Xoá hết items | Empty state hiển thị | Trung |
| TC-CRT-08 | Tổng tiền format VND | Kiểm tra `.cart-grandtotal` text | Format "45,000₫" hoặc "50.000₫" | Trung |
| TC-CRT-09 | Nút "Thanh toán" enabled khi có item | Cart có items | Button check-out không disabled | Critical |
| TC-CRT-10 | Session persist — navigate away → quay lại | Về trang chủ → quay lại cart | Items vẫn còn | Cao |
| TC-CRT-11 | Refresh page → items vẫn giữ | F5 reload cart page | Items không mất | Cao |
| TC-CRT-12 | Multi-restaurant validation | Thêm món từ 2 quán | Cart cảnh báo hoặc block | Trung |

#### 1F. Checkout (`/Cart/Checkout`) — 12 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-CHK-01 | Checkout load — form elements | Cart có items → vào checkout | Tab "Địa chỉ mới" active, payment options hiển thị | Critical |
| TC-CHK-02 | Order summary — items + total | Load checkout | Items list + tổng tiền khớp với cart | Critical |
| TC-CHK-03 | Điền form địa chỉ mới | Nhập tên, SĐT, địa chỉ, chọn quận | Form fields fill được, lưu được | Cao |
| TC-CHK-04 | Tab "Địa chỉ đã lưu" | Click tab → chọn địa chỉ có sẵn | Address load từ DB, auto-fill | Trung |
| TC-CHK-05 | Chọn COD | Click payment option COD | COD được selected (class active) | Critical |
| TC-CHK-06 | Chọn Chuyển khoản | Click option chuyển khoản | Hiển thị thông tin tài khoản + QR | Cao |
| TC-CHK-07 | Submit button disabled khi chưa confirm | Chưa tick checkbox confirm | Button disabled | Trung |
| TC-CHK-08 | Nhập coupon hợp lệ | Nhập "SALE10" → Apply | Coupon áp dụng, discount hiển thị | Cao |
| TC-CHK-09 | Nhập coupon không hợp lệ | Nhập "KHONGTONTAI999" → Apply | Error message hiển thị | Cao |
| TC-CHK-10 | Browse coupons popup | Click "Xem mã giảm giá" | Popup hiển thị danh sách coupon cards | Trung |
| TC-CHK-11 | COD Checkout — fill full form → submit | Fill address → COD → confirm → submit | Result popup "Đặt hàng thành công" hoặc redirect | Critical |
| TC-CHK-12 | Checkout không có items → redirect | Vào /Cart/Checkout khi giỏ trống | Redirect về /Cart | Cao |

#### 1G. Lịch sử đơn hàng (`/Cart/LichSuDatHang`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-OH-01 | Lịch sử load — DataTable hiển thị | Login → mở lịch sử | Bảng có ≥0 dòng, search/sort hoạt động | Critical |
| TC-OH-02 | Status badges — màu sắc + emoji | Xem cột trạng thái | Badge màu xanh "Hoàn thành", đỏ "Đã hủy", cam "Đang giao" | Cao |
| TC-OH-03 | Click vào đơn → chi tiết | Click link "Chi tiết" | Redirect đến `/Cart/ChiTietDonHang?id=X` | Cao |
| TC-OH-04 | Click vào tracking | Click link "Theo dõi" | Redirect đến `/Cart/OrderTracking?id=X` | Cao |
| TC-OH-05 | Nút "⭐ Đánh giá" cho đơn "Hoàn thành" | Tìm đơn hoàn thành | Nút đánh giá hiển thị, click mở modal | Cao |
| TC-OH-06 | Huỷ đơn (nếu trạng thái cho phép) | Click nút Huỷ | Confirm → đơn chuyển "Đã hủy" | Trung |

#### 1H. Chi tiết đơn hàng (`/Cart/ChiTietDonHang?id=X`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-OD-01 | Invoice layout — thông tin đơn | Mở chi tiết đơn | Mã đơn, ngày đặt, địa chỉ, trạng thái, items, tổng tiền hiển thị | Critical |
| TC-OD-02 | Live map — Leaflet render | Scroll đến map | Map có container #map, Leaflet tiles load | Cao |
| TC-OD-03 | SignalR connection | Load page | SignalR hub `/nhantin` connected | Trung |
| TC-OD-04 | Progress bar — tracking steps | Check progress section | 7 bước: "Đã đặt" → "Đã xác nhận" → … → "Hoàn thành" | Cao |
| TC-OD-05 | Nút "⭐ Đánh giá" cho đơn "Hoàn thành" | Nếu order hoàn thành | Modal đánh giá với star rating + comment | Cao |
| TC-OD-06 | E-Invoice link | Click "Xuất hóa đơn" | Redirect đến `/Cart/EInvoice?id=X` | Trung |

#### 1I. Order Tracking (`/Cart/OrderTracking?id=X`) — 5 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-TR-01 | Progress bar — 7 steps | Load tracking page | 7 steps với icons, step hiện tại highlight | Critical |
| TC-TR-02 | Leaflet map với markers | Check map container | Map hiển thị, có marker cho quán + shipper | Cao |
| TC-TR-03 | ETA display | Check ETA section | "Dự kiến giao: XX phút" hoặc tương tự | Cao |
| TC-TR-04 | SignalR connection | Load page | SignalR connected, sẵn sàng nhận update real-time | Trung |
| TC-TR-05 | FastShipTracking hub callbacks | Evaluate JS window | `window.FastShipTracking.createHubConnection` tồn tại | Trung |

#### 1J. Review Modal (từ Chi tiết đơn + Lịch sử) — 5 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-RV-01 | Mở modal — star rating hiển thị | Click "⭐ Đánh giá" | Modal với 5 sao (hover hiệu ứng), textarea nhận xét | Cao |
| TC-RV-02 | Chọn sao + nhập nhận xét → submit | Chọn 4 sao → nhập "Ngon!" → Gửi | API success, badge "Đã đánh giá" xuất hiện | Critical |
| TC-RV-03 | Submit với 0 sao → validation error | Click Gửi khi chưa chọn sao | Error "Vui lòng chọn số sao" | Cao |
| TC-RV-04 | Nhận xét >500 ký tự → trim | Nhập 600 ký tự → submit | Chỉ gửi 500 ký tự đầu | Trung |
| TC-RV-05 | Đánh giá duplicate → error | Submit review lần 2 cho cùng món | Error "Bạn đã đánh giá món này rồi" | Cao |

#### 1K. Chat (`/Home/NhanTin`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-CHAT-01 | Chat page load | Mở `/Home/NhanTin` | Chat container hiển thị, SignalR connected | Cao |
| TC-CHAT-02 | Gửi tin nhắn | Nhập text → click Gửi | Tin nhắn hiển thị trong chat box | Cao |
| TC-CHAT-03 | Nhận tin nhắn từ admin/shipper | Chờ admin reply | Tin nhắn đến real-time, scroll auto | Trung |
| TC-CHAT-04 | AI Chatbot (Gemini) widget | Click chat widget → hỏi "Có món gì ngon?" | AI reply với gợi ý món ăn | Trung |

---

### 🏪 PHẦN 2: QUÁN ĂN (Restaurant) — ~50 Test Cases

#### 2A. Dashboard (`/Restaurant`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-RS-01 | Login quán ăn → redirect | Login `konekopizza` → redirect | URL chứa `/Restaurant`, sidebar visible | Critical |
| TC-RS-02 | KPI cards — doanh thu, đơn hàng | Load dashboard | 4 KPI cards: "Tổng đơn", "Doanh thu", "Đánh giá", "Khách hàng" | Critical |
| TC-RS-03 | Chart.js canvas — kích thước > 0 | Check `<canvas>` elements | Canvas có width, height > 0 | Cao |
| TC-RS-04 | Sidebar — tất cả links hiển thị | Load dashboard | Sidebar có: Dashboard, Đơn hàng, Món ăn, Phân tích, Khuyến mãi, Đánh giá | Critical |
| TC-RS-05 | Sidebar routing — click "Danh sách đơn hàng" | Click Order List link | Redirect đến `/Restaurant/OrderList` | Cao |
| TC-RS-06 | Sidebar routing — "Quản lý món" | Click Product List | Redirect đến `/Restaurant/ProductList` | Cao |
| TC-RS-07 | Ảnh dashboard không broken | Check all `<img>` | 0 broken images | Cao |
| TC-RS-08 | Console 0 JS errors | Capture errors | 0 page errors | Cao |

#### 2B. Order List (`/Restaurant/OrderList`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-ROL-01 | DataTable load | Mở Order List | Bảng `#example5` hiển thị ≥0 dòng | Critical |
| TC-ROL-02 | Cột trạng thái — không trống | Check từng dòng | Mỗi dòng có badge trạng thái | Cao |
| TC-ROL-03 | Nút "Nhận đơn" cho đơn "Đã đặt" | Tìm đơn có trạng thái "Đã đặt" | Link `a[href*="nhandon"]` hiển thị | Critical |
| TC-ROL-04 | Click "Nhận đơn" → chuyển trạng thái | Click Nhận → reload page | Trạng thái chuyển "Đã xác nhận", nút Nhận biến mất | Critical |
| TC-ROL-05 | Nút "Hủy đơn" hoạt động | Click Hủy | Confirm → trạng thái "Đã hủy" | Cao |
| TC-ROL-06 | Nút "Đã chuẩn bị xong" cho đơn "Đã xác nhận" | Click hoantatdon | Trạng thái chuyển "Chờ shipper" | Cao |
| TC-ROL-07 | Chi tiết đơn → click xem | Click link Chi tiết | Redirect `/Cart/ChiTietDonHang?id=X` | Cao |
| TC-ROL-08 | Search/filter date | Nhập fromDate → toDate → search | Bảng lọc theo ngày | Trung |

#### 2C. Product Management (`/Restaurant/ProductList`, `/Restaurant/ProductDetail`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-PROD-01 | Product list — danh sách món | Mở Product List | Bảng danh sách món + ảnh + giá + trạng thái | Critical |
| TC-PROD-02 | Edit link → ProductDetail | Click "Sửa" | Form edit với dữ liệu món đã điền sẵn | Cao |
| TC-PROD-03 | Delete link → xoá món | Click "Xoá" | Món bị xoá (soft delete), redirect list | Cao |
| TC-PROD-04 | Add new — form fields | Mở ProductDetail không có id | Form trống: Tên món, Danh mục, Giá, Size M/L/XL, Ảnh upload, Mô tả | Critical |
| TC-PROD-05 | Size variant inputs (M/L/XL) | Check form | 3 input cho size M, L, XL, mỗi cái có price riêng | Cao |
| TC-PROD-06 | File upload — chọn ảnh + preview | Click "Chọn ảnh" → pick file | Preview image hiển thị | Cao |
| TC-PROD-07 | Submit form trống → validation | Click Submit khi chưa điền | HTML5 validation: required fields báo lỗi | Cao |
| TC-PROD-08 | Submit đầy đủ → thêm món thành công | Điền đủ → Submit | Món mới xuất hiện trong Product List | Critical |

#### 2D. Discount (`/Restaurant/Discount`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-DIS-01 | Discount list — bảng khuyến mãi | Mở Discount | Bảng danh sách KM: Tên, % giảm, Số lượng, Trạng thái | Critical |
| TC-DIS-02 | Thêm KM mới | Click "Thêm" → form → submit | KM mới xuất hiện trong bảng | Cao |
| TC-DIS-03 | Gắn KM cho món | Chọn món → gắn % giảm | Món hiển thị discount badge trên menu | Trung |
| TC-DIS-04 | Hết hạn → tự động tắt | KM có ngày kết thúc < hôm nay | Trạng thái "Hết hạn" | Trung |

#### 2E. Analytics (`/Restaurant/Analytics`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-ANA-01 | Revenue chart — canvas render | Mở Analytics | Biểu đồ doanh thu theo danh mục (Chart.js) | Cao |
| TC-ANA-02 | Top items — bảng món bán chạy | Check section | Bảng top 10 món: tên, số lượng, doanh thu | Cao |
| TC-ANA-03 | Doanh thu theo category | Check section | Pie/bar chart doanh thu theo category | Trung |
| TC-ANA-04 | Date filter | Chọn from → to → Apply | Charts + tables cập nhật theo ngày | Trung |

#### 2F. Reviews (`/Restaurant/Review`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-RRV-01 | Review list — điểm + phân bố sao | Mở Review | Tổng điểm TB + 5 bar phân bố sao | Cao |
| TC-RRV-02 | Reply review | Click "Trả lời" → nhập → submit | Reply hiển thị dưới review | Trung |
| TC-RRV-03 | Filter reviews | Chọn filter "5 sao" | Chỉ hiển thị review 5 sao | Trung |
| TC-RRV-04 | Filter "Có bình luận" / "Chưa reply" | Chọn filter | Lọc đúng | Trung |

#### 2G. Profile (`/Restaurant/Profile`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-PRF-01 | Profile form — thông tin quán | Mở Profile | Form có: Tên quán, Địa chỉ, SĐT, Giờ mở cửa, Avatar | Cao |
| TC-PRF-02 | Cập nhật thông tin | Sửa tên → Submit | Lưu thành công, thông tin cập nhật | Cao |
| TC-PRF-03 | Toggle trạng thái mở/đóng cửa | Click toggle | Badge chuyển "Đang mở" ↔ "Đóng cửa" | Cao |
| TC-PRF-04 | Đổi mật khẩu | Nhập mật khẩu cũ + mới → Submit | Đổi mật khẩu thành công | Trung |

#### 2H. Wallet (`/Restaurant/Wallet`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-WAL-01 | Wallet — số dư + lịch sử | Mở Wallet | Số dư hiển thị, bảng giao dịch | Cao |
| TC-WAL-02 | Rút tiền | Nhập số tiền → Rút | Confirm → giao dịch tạo | Trung |
| TC-WAL-03 | Nạp tiền (mock) | Click Nạp | Redirect đến payment gateway | Trung |
| TC-WAL-04 | Transaction items format | Check từng dòng | Mỗi giao dịch: ngày, nội dung, số tiền (+/-), số dư | Trung |

#### 2I. Scanner (`/Restaurant/Scanner`) — 3 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SCN-01 | html5-qrcode container | Mở Scanner | `#qr-reader` container hiển thị | Cao |
| TC-SCN-02 | Camera controls: Start/Stop/Switch | Check buttons | Start, Stop, Switch Camera buttons visible | Trung |
| TC-SCN-03 | Scan history | Check `#scanHistory` | History container visible | Thấp |

---

### 🚚 PHẦN 3: SHIPPER — ~35 Test Cases

#### 3A. Dashboard (`/Shipper`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SH-01 | Login shipper → redirect | Login `shipperz` | URL chứa `/Shipper`, sidebar visible | Critical |
| TC-SH-02 | FREE-PICK tab — danh sách đơn chờ | Click FREE-PICK tab | Bảng đơn chưa có shipper | Critical |
| TC-SH-03 | ĐƠN HÀNG tab — đơn đã nhận | Click ĐƠN HÀNG tab | Bảng đơn đã nhận | Critical |
| TC-SH-04 | Leaflet map trên FREE-PICK | Check map container | `#shipper-map` hiển thị, tiles loaded | Cao |
| TC-SH-05 | Click "Chi tiết" → OrderDetail | Click link đầu | Redirect `/Shipper/OrderDetail?id=X` | Cao |
| TC-SH-06 | Ảnh dashboard không broken | Check all `<img>` | 0 broken images | Cao |
| TC-SH-07 | Desktop layout — không overflow | Check scroll | No horizontal scroll | Trung |
| TC-SH-08 | Console 0 JS errors | Capture errors | 0 page errors | Cao |

#### 3B. Order Detail (`/Shipper/OrderDetail?id=X`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SOD-01 | Order info — thông tin đơn | Mở OrderDetail | Mã đơn, địa chỉ, items, tổng tiền hiển thị | Critical |
| TC-SOD-02 | Nút "Đã lấy hàng" (Pickup) | Click `#btnPickup` | Trạng thái chuyển "Đang giao" | Critical |
| TC-SOD-03 | Nút "Hoàn thành" (Complete) | Click `#btnComplete` | Trạng thái chuyển "Hoàn thành" | Critical |
| TC-SOD-04 | QR code — glassmorphism card | Check QR section | Card QR với mã QR image + nút "Tải QR" | Cao |
| TC-SOD-05 | Leaflet map | Check map | Live map với route từ quán → khách | Cao |
| TC-SOD-06 | Geolocation update (nếu có) | Check SignalR | Tọa độ shipper gửi lên server real-time | Trung |

#### 3C. Wallet (`/Shipper/ViTien`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SW-01 | Số dư ví hiển thị | Mở ViTien | Balance text hiển thị format VND | Critical |
| TC-SW-02 | Lịch sử giao dịch | Check transaction table | Bảng: ngày, nội dung, +/- số tiền, số dư | Cao |
| TC-SW-03 | Giao dịch gần nhất | Check dòng đầu | Dòng hoàn thành gần nhất đúng thông tin | Trung |
| TC-SW-04 | Rút tiền | Click "Rút tiền" | Confirm → giao dịch tạo | Thấp |

#### 3D. Income (`/Shipper/ThuNhap`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SI-01 | Thống kê 30 ngày | Mở ThuNhap | Cards: "Hoàn thành", "Đã hủy", "Tổng thu", "Hôm nay" | Critical |
| TC-SI-02 | Chart — biểu đồ thu nhập | Check `<canvas>` | Chart.js bar chart thu nhập theo ngày | Trung |
| TC-SI-03 | Bảng đơn hàng 30 ngày | Check table | Danh sách đơn trong 30 ngày: mã, ngày, tiền, trạng thái | Cao |
| TC-SI-04 | So sánh số dư ví vs thu nhập | Check numbers | Sum of completed orders ≈ income stats | Trung |

#### 3E. History (`/Shipper/LichSu`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SHIST-01 | Lịch sử giao hàng — bảng | Mở LichSu | Bảng đơn đã giao + đã hủy | Cao |
| TC-SHIST-02 | Status badges | Check cột trạng thái | Badge màu: xanh="Hoàn thành", đỏ="Đã hủy" | Trung |
| TC-SHIST-03 | Sort by date | Click cột "Ngày" | Sort asc/desc | Thấp |
| TC-SHIST-04 | Search | Nhập mã đơn | Filter đúng | Thấp |

#### 3F. Settings (`/Shipper/CaiDat`) — 3 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SSET-01 | Profile settings tab | Click tab Profile | Form: tên, SĐT, email, avatar | Trung |
| TC-SSET-02 | Đổi mật khẩu | Nhập mật khẩu cũ + mới → Submit | Đổi thành công | Thấp |
| TC-SSET-03 | Cập nhật profile | Sửa SĐT → Submit | Lưu thành công | Thấp |

#### 3G. QR Delivery (`/Shipper/QRDelivery`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-SQR-01 | QR list — images load | Mở QRDelivery | QR images hiển thị (complete + naturalWidth > 0) | Cao |
| TC-SQR-02 | Tab filter: Chờ giao / Đang giao / Hoàn thành | Click từng tab | Bảng QR lọc theo trạng thái | Cao |
| TC-SQR-03 | SignalR auto-refresh | Load page | SignalR connected, QR tự động refresh | Trung |
| TC-SQR-04 | Download QR | Click "Tải QR" | Download PNG file | Thấp |

---

### 👑 PHẦN 4: ADMIN — ~40 Test Cases

#### 4A. Dashboard (`/Admin`, `/Admin/Dashboard`) — 8 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-AD-01 | Login admin → redirect | Login `admin1` | URL chứa `/Admin`, sidebar visible | Critical |
| TC-AD-02 | KPI cards — 4 thẻ | Load Dashboard | 4 KPI: Tổng đơn, Doanh thu, User, Đánh giá — mỗi thẻ có số > 0 | Critical |
| TC-AD-03 | Revenue line chart | Check canvas | Chart.js line chart: doanh thu theo tháng | Cao |
| TC-AD-04 | Order status pie chart | Check canvas | Pie chart: tỉ lệ trạng thái đơn | Cao |
| TC-AD-05 | Top items chart | Check section | Bar chart: top 10 món bán chạy | Cao |
| TC-AD-06 | Date filter — từ ngày → đến ngày | Nhập date → Apply | Charts + tables cập nhật | Trung |
| TC-AD-07 | Sidebar — tất cả nav links | Load dashboard | Links: Dashboard, Quản lý Quán/Shipper/Khách hàng/Admin, Đơn hàng, Danh mục, Chat, Voucher | Critical |
| TC-AD-08 | Console 0 JS errors | Capture errors | 0 page errors | Cao |

#### 4B. Quản lý người dùng (4 pages) — 10 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-AU-01 | Quản lý Khách hàng | Mở `/Admin/QuanLyKhachHang` | Bảng customers: tên, SĐT, email, trạng thái | Critical |
| TC-AU-02 | Quản lý Quán ăn | Mở `/Admin/QuanLyQuanAn` | Bảng: tên quán, chủ, doanh thu, trạng thái | Critical |
| TC-AU-03 | Quản lý Shipper | Mở `/Admin/QuanLyShipper` | Bảng: tên, SĐT, số đơn, trạng thái | Critical |
| TC-AU-04 | Quản lý Admin | Mở `/Admin/QuanLyQuanTriVien` | Bảng: tên, email, ngày tạo | Trung |
| TC-AU-05 | Duyệt user | Click "Duyệt" trên user pending | User active, trạng thái chuyển | Cao |
| TC-AU-06 | Khóa/Mở khóa user | Click "Khóa" | User bị khóa, không login được | Cao |
| TC-AU-07 | Search user | Nhập tên → search | Bảng lọc theo keyword | Trung |
| TC-AU-08 | Filter by role | Click tab "Quán ăn" | Chỉ hiển thị quán | Trung |
| TC-AU-09 | Click user → chi tiết | Click vào dòng | Modal/user detail với thông tin đầy đủ | Trung |
| TC-AU-10 | Không thể khóa admin cuối cùng | Thử khóa admin cuối cùng | Error "Không thể khóa admin cuối cùng" | Cao |

#### 4C. Quản lý đơn hàng (`/Admin/Order`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-AO-01 | Order list — DataTable | Mở `/Admin/Order` | Bảng: mã, khách hàng, quán, tổng tiền, trạng thái | Critical |
| TC-AO-02 | SignalR real-time update | Chờ đơn mới | Đơn mới xuất hiện auto, không cần refresh | Cao |
| TC-AO-03 | Click Order → OrderDetail | Click link Chi tiết | Redirect đến `/Admin/OrderDetail?id=X` | Cao |
| TC-AO-04 | Dropdown action (Nhận/Hủy/Xác nhận) | Click action dropdown | Menu actions hiển thị đúng theo trạng thái | Cao |
| TC-AO-05 | Search by order ID | Nhập mã → search | Filter đúng 1 đơn | Trung |
| TC-AO-06 | Filter by status | Chọn trạng thái trong dropdown | Bảng lọc theo trạng thái | Trung |

#### 4D. Quản lý danh mục (`/Admin/Category`) — 6 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-AC-01 | Category list — bảng danh mục | Mở `/Admin/Category` | Bảng: tên DM, icon, số món, trạng thái | Cao |
| TC-AC-02 | Create category | Click "Thêm" → form → submit | Category mới trong bảng | Cao |
| TC-AC-03 | Edit category | Click "Sửa" → sửa tên → submit | Tên category cập nhật | Cao |
| TC-AC-04 | Delete category (không có ràng buộc) | Click "Xoá" | Category xoá thành công | Cao |
| TC-AC-05 | Delete category (có ràng buộc món) | Thử xoá category có món | Error "Không thể xoá danh mục có món ăn" | Cao |
| TC-AC-06 | Upload icon cho category | Chọn file ảnh → Submit | Icon cập nhật | Trung |

#### 4E. Admin Chat (`/AdminChat`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-ACH-01 | Chat page — danh sách hội thoại | Mở AdminChat | Sidebar: danh sách user đang chat, badge unread | Cao |
| TC-ACH-02 | Click user → load messages | Click vào user | Messages load, scroll xuống cuối | Cao |
| TC-ACH-03 | Gửi tin nhắn | Nhập text → Enter/Gửi | Tin nhắn xuất hiện, SignalR broadcast | Critical |
| TC-ACH-04 | Unread count — badge cập nhật real-time | Chờ tin nhắn mới | Badge số unread tăng | Trung |

#### 4F. Voucher Manager (`/Admin/VoucherManager`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-AV-01 | Voucher list — bảng | Mở VoucherManager | Bảng: mã, % giảm, số lượng, hạn sử dụng, trạng thái | Cao |
| TC-AV-02 | Create voucher | Click "Thêm" → form → submit | Voucher mới trong bảng | Cao |
| TC-AV-03 | Edit voucher — % giảm + hạn | Click "Sửa" → update | Voucher cập nhật | Trung |
| TC-AV-04 | Toggle active/inactive | Click toggle | Voucher active ↔ inactive | Trung |

#### 4G. Delivery Logs (`/EDelivery/DeliveryLogs`) — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-ADL-01 | Delivery Logs — stats cards | Mở DeliveryLogs | Thẻ: Tổng đã giao, Đang giao, Hoàn thành | Cao |
| TC-ADL-02 | Pastel badges table | Check table | Table không có vertical borders, badges màu pastel | Cao |
| TC-ADL-03 | Bypass modal — open + close | Click "Bypass" | Modal: status select + Cancel + Confirm | Cao |
| TC-ADL-04 | Bypass API call | POST /EDelivery/Bypass với order 99999 | Response: `{success: false, message: "..."}` | Trung |

---

#### 4H. Database Validation — 4 tests

| # | Test Case | Steps | Expected Output | Priority |
|---|-----------|-------|-----------------|----------|
| TC-DB-01 | DbDebug endpoint | GET `/Home/DbDebug` | JSON: `{success: true, database: {tbUser: >0, tbQuanAn: >0, ...}}` | Critical |
| TC-DB-02 | Seed data integrity | Verify từng bảng | tbUser ≥ 15 users, tbQuanAn ≥ 10 quán, tbMonAn ≥ 50 món | Cao |
| TC-DB-03 | Foreign key integrity | Check FK relationships | All tbChiTietDonHang.mamon → tbBienTheMonAn.id tồn tại | Trung |
| TC-DB-04 | Order status transitions hợp lệ | Check DB | No invalid status combinations | Trung |

---

## 🔄 LUỒNG NGHIỆP VỤ END-TO-END (Smoke Tests)

### Flow 1: Khách hàng đặt hàng COD — Full life cycle

```
Đăng nhập → Tìm quán → Xem menu → Thêm món → 
Checkout → Điền địa chỉ → Chọn COD → Xác nhận → 
Xem lịch sử → Xem chi tiết đơn → Theo dõi giao hàng → 
Nhận hàng → Đánh giá
```

**Expected**: Mỗi bước thành công, dữ liệu nhất quán giữa các trang.

### Flow 2: Quản lý đơn — Quán ăn + Shipper

```
Customer đặt → Restaurant nhận đơn → 
Shipper nhận FREE-PICK → Shipper lấy hàng → 
Shipper giao → Hoàn thành
```

**Expected**: Trạng thái đơn chuyển qua từng bước, không skip.

### Flow 3: Admin quản lý toàn diện

```
Login admin → Xem Dashboard KPI → 
Quản lý user (Duyệt) → Quản lý đơn (Xem) → 
Quản lý danh mục (Thêm mới) → Chat hỗ trợ
```

**Expected**: Mỗi thao tác thành công, dữ liệu cập nhật.

---

## ⚙️ CẤU HÌNH TEST

### File cấu hình: `e2e-tests/playwright.config.ts`

```typescript
timeout: 60_000,
expect.timeout: 15_000,
workers: 1,        // Serial để tránh rate limit
retries: 0,
use: {
  baseURL: 'https://fastship-web.onrender.com',
  viewport: { width: 1920, height: 1080 },
}
```

### Lệnh chạy

```bash
# Chạy toàn bộ test
cd e2e-tests && npx playwright test

# Chạy theo role
npx playwright test tests/02-customer-flow.spec.ts   # Khách hàng
npx playwright test tests/03-restaurant-flow.spec.ts  # Quán ăn
npx playwright test tests/04-shipper-flow.spec.ts     # Shipper
npx playwright test tests/05-admin-flow.spec.ts       # Admin

# Chạy visual + asset
npx playwright test tests/01-visual-asset-validation.spec.ts

# Chạy nhanh (Desktop only, timeout thấp)
npm run test:fast

# Với Lightpanda Browser
npm run test:lightpanda
```

---

## 📈 MA TRẬN COVERAGE

| Phần | Role | Số test case | Existing | Cần thêm | Priority |
|------|------|-------------|----------|---------|----------|
| 1A | Customer - Home | 10 | 15 (TC-1.x, TC-2.x) | 0 | ✅ Đủ |
| 1B | Customer - Login | 8 | 5 (TC-2.1-2.5) | 3 | Trung |
| 1C | Customer - DetailRestaurant | 10 | 8 (TC-2.6-2.10) | 2 | Thấp |
| 1D | Customer - ProductDetail | 6 | 0 | 6 | **Cao** |
| 1E | Customer - Cart | 12 | 10 (TC-2.11-2.15, TC-CART-*) | 2 | Thấp |
| 1F | Customer - Checkout | 12 | 14 (TC-CHECKOUT-*) | 0 | ✅ Đủ |
| 1G | Customer - OrderHistory | 6 | 2 (TC-2.21) | 4 | Trung |
| 1H | Customer - OrderDetail | 6 | 0 | 6 | **Cao** |
| 1I | Customer - Tracking | 5 | 4 (TC-STATUS-*) | 1 | Thấp |
| 1J | Customer - Reviews | 5 | 0 | 5 | **Cao** |
| 1K | Customer - Chat | 4 | 0 | 4 | **Cao** |
| 2A | Restaurant - Dashboard | 8 | 5 (TC-3.1-3.4) | 3 | Thấp |
| 2B | Restaurant - OrderList | 8 | 8 (TC-3.5-3.12) | 0 | ✅ Đủ |
| 2C | Restaurant - Products | 8 | 6 (TC-3.16-3.20) | 2 | Thấp |
| 2D | Restaurant - Discount | 4 | 1 (TC-3.22) | 3 | Trung |
| 2E | Restaurant - Analytics | 4 | 1 (TC-3.23) | 3 | Trung |
| 2F | Restaurant - Reviews | 4 | 1 (TC-8.10) | 3 | Trung |
| 2G | Restaurant - Profile | 4 | 2 (TC-3.21, 8.6-8.7) | 2 | Thấp |
| 2H | Restaurant - Wallet | 4 | 0 | 4 | Trung |
| 2I | Restaurant - Scanner | 3 | 2 (TC-3.24-3.25) | 1 | Thấp |
| 3A | Shipper - Dashboard | 8 | 5 (TC-4.1-4.5) | 3 | Thấp |
| 3B | Shipper - OrderDetail | 6 | 2 (TC-4.6, 9.1) | 4 | Trung |
| 3C | Shipper - Wallet | 4 | 3 (TC-4.9, 9.4) | 1 | Thấp |
| 3D | Shipper - Income | 4 | 2 (TC-4.10, 9.5) | 2 | Thấp |
| 3E | Shipper - History | 4 | 2 (TC-4.11, 9.6) | 2 | Thấp |
| 3F | Shipper - Settings | 3 | 1 (TC-9.7) | 2 | Thấp |
| 3G | Shipper - QR Delivery | 4 | 2 (TC-6.1-6.2, 9.8) | 2 | Thấp |
| 4A | Admin - Dashboard | 8 | 6 (TC-5.1-5.6) | 2 | Thấp |
| 4B | Admin - User Mgmt | 10 | 3 (TC-5.7-5.9) | 7 | **Cao** |
| 4C | Admin - Order Mgmt | 6 | 2 (TC-5.10-5.11) | 4 | Trung |
| 4D | Admin - Category Mgmt | 6 | 2 (TC-5.12-5.13) | 4 | Trung |
| 4E | Admin - Chat | 4 | 0 | 4 | **Cao** |
| 4F | Admin - Voucher | 4 | 0 | 4 | **Cao** |
| 4G | Admin - Delivery Logs | 4 | 4 (TC-6.10-6.13) | 0 | ✅ Đủ |
| 4H | Admin - Database | 4 | 1 (TC-5.14) | 3 | Trung |

### Tổng quan

| Mục | Số lượng |
|-----|---------|
| **Tổng test cases** | ~230 |
| **Đã có sẵn** | ~130 (56%) |
| **Cần bổ sung** | ~100 (44%) |
| **Priority thấp** | ~50 |
| **Priority trung** | ~25 |
| **Priority cao** | ~25 |

---

## 🚨 LƯU Ý QUAN TRỌNG

1. **Render Free Tier**: 23-25s cold start → timeout 60s, retry pattern
2. **Rate Limit**: 5 POST/5ph → workers=1, login có 429 retry
3. **SignalR**: Không dùng `waitForLoadState('networkidle')` — WebSocket giữ kết nối mãi
4. **Unsplash Images**: Render IP bị Unsplash rate limit → ảnh external có thể bị 403
5. **Session Cookie**: HttpOnly cookie → JS không đọc được → Server-side `isLoggedIn` flag
6. **Date/Time**: Dùng `DateTime.Now` (không UTC) — server timezone = UTC+7

---

## 📋 DANH SÁCH FILE TEST CẦN VIẾT MỚI (Priority Cao)

| File | Nội dung | Test cases |
|------|---------|-----------|
| `tests/xx-product-detail.spec.ts` | Chi tiết sản phẩm | 6 |
| `tests/xx-order-detail.spec.ts` | Chi tiết đơn hàng + tracking | 6 |
| `tests/xx-reviews.spec.ts` | Review modal full flow | 5 |
| `tests/xx-customer-chat.spec.ts` | Chat + AI chatbot | 4 |
| `tests/xx-admin-user-mgmt.spec.ts` | User CRUD + duyệt/khóa | 10 |
| `tests/xx-admin-chat.spec.ts` | Admin chat support | 4 |
| `tests/xx-admin-voucher.spec.ts` | Voucher CRUD | 4 |
| `tests/xx-restaurant-wallet.spec.ts` | Wallet + thu nhập | 4 |
| `tests/xx-smoke-e2e.spec.ts` | Full flow E2E smoke | 3 |

---

## 📊 BÁO CÁO TEST

Sau khi chạy, test report sẽ được generate tại:
- HTML: `e2e-tests/playwright-report/` (chạy `npx playwright show-report`)
- JSON: `e2e-tests/test-results-fast.json` (chạy với `npm run test:fast`)

Mỗi test case log đầy đủ:
- ✅ PASS: Test case + expected output + actual output
- ❌ FAIL: Test case + error message + screenshot + stack trace
- ℹ️ SKIP: Test case + lý do (precondition fail)
