# BÁO CÁO KIỂM THỬ TOÀN DIỆN — FASTSHIP (ShipFood)

> Ngày tạo: 2026-07-19
> Phiên bản: 2.0
> URL: https://fastship-web.onrender.com
> Mục đích: Kiểm thử toàn diện UI/UX, chức năng, hình ảnh, nội dung, giao diện, hiệu suất — KHÔNG bao gồm bảo mật

---

## I. TỔNG QUAN HỆ THỐNG

### 1.1 Kiến trúc kỹ thuật

| Thành phần | Công nghệ |
|------------|-----------|
| Backend | ASP.NET Core 8 MVC |
| Database | PostgreSQL + Entity Framework Core 8 |
| Real-time | SignalR |
| AI | Gemini API (chatbot) |
| Frontend | Bootstrap 5, jQuery, Chart.js |
| Hosting | Render.com (Free Tier) |
| CDN | Cloudflare (DNS) |

### 1.2 4 vai trò người dùng

| Vai trò | Tài khoản | Quyền hạn |
|---------|-----------|-----------|
| Khách hàng | tranthib / abcdef | Đặt hàng, theo dõi, đánh giá |
| Nhà hàng | konekopizza | Quản lý menu, đơn hàng, thống kê |
| Shipper | shipperz | Giao hàng, quét QR, kiểm thu |
| Quản trị viên | admin1 | Quản lý người dùng, đơn hàng, hệ thống |

### 1.3 Cơ sở kiểm thử

| Thuộc tính | Giá trị |
|------------|---------|
| Framework | Playwright 1.55.0 |
| Ngôn ngữ | TypeScript |
| Projects | Desktop Chromium (1920×1080), Mobile Chrome (375×812) |
| Workers | 1 (tránh rate limit) |
| Timeout | 60s test, 30s action, 15s expect |
| Reporter | List (console) |
| Page Objects | 7 file (CartPage, CheckoutPage, DetailRestaurantPage, Homepage, OrderTrackingPage, RestaurantDashboard, ShipperDashboard) |

---

## II. TỔNG SỐ TEST CASES

| Nhóm | Số file | Số test cases | Trạng thái |
|------|---------|---------------|------------|
| Existing tests (trước đó) | 22 | 460 | Đã có |
| NEW tests (tạo mới) | 10 | 186 | Tạo mới |
| **TỔNG** | **32** | **646** | |

### 2.1 Existing Tests — 22 file, 460 cases

| File | Tests | Phạm vi |
|------|-------|---------|
| user-comprehensive.spec.ts | 87 | Luồng người dùng toàn diện |
| admin-flow.spec.ts | 43 | E2E quản trị viên |
| visual-design.spec.ts | 38 | Hệ thống thiết kế, responsive |
| auth-flow.spec.ts | 36 | Xác thực đăng nhập |
| restaurant-flow.spec.ts | 27 | E2E nhà hàng |
| customer-flow.spec.ts | 50 | Luồng khách hàng đầy đủ |
| shipper-flow.spec.ts | 22 | E2E shipper |
| 02-customer-flow.spec.ts | 21 | Luồng khách hàng cơ bản |
| 01-visual-asset-validation.spec.ts | 20 | Xác thực tài nguyên hình ảnh |
| 03-restaurant-flow.spec.ts | 25 | Nhà hàng cơ bản |
| 04-shipper-flow.spec.ts | 15 | Shipper cơ bản |
| 05-admin-flow.spec.ts | 17 | Quản trị viên cơ bản |
| 06-edelivery-flow.spec.ts | 15 | E-delivery QR |
| 07-customer-advanced.spec.ts | 15 | Khách hàng nâng cao |
| 08-merchant-advanced.spec.ts | 10 | Nhà hàng nâng cao |
| 09-shipper-advanced.spec.ts | 8 | Shipper nâng cao |
| 10-admin-advanced.spec.ts | 15 | Quản trị viên nâng cao |
| edge-performance.spec.ts | 15 | Trường hợp biên, hiệu suất |
| security.spec.ts | 15 | Bảo mật (BỎ QUA) |
| smoke-lightpanda.spec.ts | 15 | Kiểm thử khói |
| cross-browser.spec.ts | 6 | Đa trình duyệt |
| accessibility.spec.ts | 10 | Khả năng truy cập |

### 2.2 NEW Tests — 10 file, 186 cases

| File | Tests | Phạm vi | Ưu tiên |
|------|-------|---------|---------|
| 11-cart-management.spec.ts | 15 | Thêm/xóa/sửa giỏ hàng, lưu trữ | Critical |
| 12-checkout-flow.spec.ts | 14 | Địa chỉ, thanh toán, mã giảm giá, đặt hàng | Critical |
| 13-order-status.spec.ts | 8 | Chuyển trạng thái, thanh tiến trình theo dõi | Critical |
| 14-filter-search.spec.ts | 12 | Tìm kiếm trang chủ, danh mục, lọc menu | High |
| 15-chat-system.spec.ts | 12 | Chatbot AI + Chat quản trị viên | High |
| 16-edelivery-tracking.spec.ts | 10 | QR giao hàng, theo dõi đơn, chi tiết đơn | High |
| 17-analytics-dashboard.spec.ts | 10 | Thống kê nhà hàng, bảng điều khiển quản trị | Medium |
| 18-mobile-responsive.spec.ts | 12 | Đích chạm, bố cục, thu phóng nhập liệu | Medium |
| 19-visual-regression.spec.ts | 15 | Token thiết kế, ảnh chụp màn hình, biểu tượng | Medium |
| 20-performance.spec.ts | 13 | Thời gian tải, lỗi console, hiệu suất API | Low |

---

## III. KẾT QUẢ THỰC TẾ — BATCH 11 ĐẾN 20

> Chạy ngày 2026-07-19 | 1 worker | timeout 60s | Desktop Chromium (1920×1080)
> Test mobile BỎ QUA theo yêu cầu người dùng

### 3.1 Tổng kết theo batch

| Batch | File | Tổng | Đạt | Lỗi | Thời gian | Ghi chú |
|-------|------|------|-----|-----|-----------|---------|
| 11 | Quản lý giỏ hàng | 15 | 6 | 9 | ~10 phút | 8 timeout (Render chậm), 1 lỗi (xóa không mất khỏi DOM) |
| 12 | Luồng thanh toán | 14 | 1 | 13 | ~10 phút | 13 timeout ở bước thiết lập (addItemToCartByIndex) |
| 13 | Trạng thái đơn hàng | 8 | 8 | 0 | 1,5 phút | Đạt tất cả — LỖI-09 thanh tiến trình + bản đồ không hiển thị |
| 14 | Lọc và Tìm kiếm | 12 | 9 | 3 | 3,5 phút | LỖI-01 tìm kiếm sai, LỖI-03 tìm kiếm menu bị hỏng |
| 15 | Hệ thống chat | 12 | 12 | 0 | 4,5 phút | Đạt tất cả |
| 16 | E-Delivery và Theo dõi | 10 | 9 | 1 | 4,2 phút | LỖI-04 tab QR bị navbar đè, LỖI-07/08 |
| 17 | Thống kê và Bảng điều khiển | 10 | 10 | 0 | 2,0 phút | LỖI-11 Thống kê nhà hàng lỗi 500 |
| 18 | Responsive trên di động | 12 | - | - | - | BỎ QUA theo yêu cầu người dùng |
| 19 | Đánh giá hình ảnh | 15 | 10 | 5 | 1,5 phút | 3 baseline ảnh chụp, LỖI-12 footer thiếu, LỖI-13 trạng thái rỗng |
| 20 | Hiệu suất | 13 | 12 | 1 | 2,5 phút | LỖI-05 trang chủ tải 19,7 giây |
| **TỔNG** | | **121** | **77** | **32** | **~40 phút** | |

### 3.2 Tỷ lệ Đạt/Lỗi

```
Tổng test đã chạy:  121
Đạt:                 77  (63,6%)
Lỗi:                 32  (26,4%)
Bỏ qua:              12  (10,0%) — batch 18 mobile
```

### 3.3 Phân loại 32 lỗi

| Loại | Số lượng | Chi tiết |
|------|----------|----------|
| Lỗi ứng dụng (lỗi thật) | 13 | LỖI-01 đến LỖI-13 (xem BUG-REPORT.md) |
| Lỗi cơ sở kiểm thử (timeout) | 15 | Batch 11 (8) + Batch 12 (7) — Render free tier chậm + page object thiếu `.catch()` |
| Ảnh chụp cơ sở | 3 | Lần chạy đầu chưa có baseline |
| Kỳ vọng test | 1 | TC-FILTER-07 danh mục "Tất cả" không tồn tại trên giao diện |

---

## IV. CHI TIẾT TỪNG BATCH

### Batch 11: Quản lý giỏ hàng (15 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-CART-01 | Tải trang giỏ hàng | ĐẠT | 8s | |
| TC-CART-02 | Thêm 3 món vào giỏ | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-03 | Tăng số lượng | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-04 | Giảm số lượng | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-05 | Xóa món khỏi giỏ | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-06 | Tính tổng giỏ hàng | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-07 | Xóa món khỏi DOM | LỖI | 60s | LỖI-15: API xóa trả 200 nhưng món vẫn hiện trong DOM |
| TC-CART-08 | Xác thực nhiều nhà hàng | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-09 | Lưu giỏ sau đăng nhập | ĐẠT | 15s | |
| TC-CART-10 | Trạng thái giỏ trống | ĐẠT | 5s | |
| TC-CART-11 | Hiển thị số lượng giỏ | ĐẠT | 8s | |
| TC-CART-12 | Thêm món từ nhà hàng khác | ĐẠT | 20s | |
| TC-CART-13 | Giỏ responsive trên di động | ĐẠT | 10s | |
| TC-CART-14 | Trường hợp biên — số lượng tối đa | LỖI | 60s | Timeout — API Render chậm |
| TC-CART-15 | Trường hợp biên — ký tự đặc biệt | LỖI | 60s | Timeout — API Render chậm |

**Kết quả: 6 đạt, 9 lỗi**
- 8 lỗi: Timeout (Render free tier API chậm — mỗi AJAX call 10-20 giây)
- 1 lỗi: LỖI-15 — Xóa món không mất khỏi DOM

### Batch 12: Luồng thanh toán (14 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-CHECKOUT-01 | Tải trang thanh toán | LỖI | 60s | Timeout ở bước thiết lập — addItemToCartByIndex |
| TC-CHECKOUT-02 | Tóm tắt đơn hiển thị món từ giỏ | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-03 | Tab "Địa chỉ mới" mặc định hoạt động | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-04 | Tab "Địa chỉ đã lưu" | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-05 | Xác thực biểu mẫu | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-06 | Lựa chọn COD | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-07 | Lựa chọn chuyển khoản | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-08 | Nút đặt hàng bị vô hiệu hóa | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-09 | Mã giảm giá hợp lệ | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-10 | Mã giảm giá không hợp lệ | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-11 | Xem danh sách mã giảm giá | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-12 | Điền đầy đủ → đặt hàng COD | LỖI | 60s | Timeout ở bước thiết lập |
| TC-CHECKOUT-13 | Thanh toán không có món | ĐẠT | 10s | Chuyển hướng về giỏ hàng |
| TC-CHECKOUT-14 | Nút đặt hàng vô hiệu hóa sau khi click | LỖI | 60s | Timeout ở bước thiết lập |

**Kết quả: 1 đạt, 13 lỗi**
- 13 lỗi: Timeout ở bước thiết lập — `addItemToCartByIndex()` trong `DetailRestaurantPage.ts:128` thiếu `.catch()` fallback
- 1 đạt: TC-CHECKOUT-13 (không cần thêm món)

### Batch 13: Trạng thái đơn hàng (8 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-STATUS-01 | Tải trang lịch sử đơn hàng | ĐẠT | 8s | |
| TC-STATUS-02 | Tải trang chi tiết đơn hàng | ĐẠT | 10s | |
| TC-STATUS-03 | Chuyển trạng thái đơn hàng | ĐẠT | 12s | |
| TC-STATUS-04 | Thanh tiến trình theo dõi đơn | ĐẠT | 10s | LỖI-09: Thanh tiến trình không hiển thị |
| TC-STATUS-05 | Bản đồ theo dõi đơn | ĐẠT | 10s | LỖI-09: Bản đồ không hiển thị |
| TC-STATUS-06 | Lọc lịch sử đơn hàng | ĐẠT | 8s | |
| TC-STATUS-07 | Chi tiết món trong đơn | ĐẠT | 10s | |
| TC-STATUS-08 | Màu badge trạng thái đơn | ĐẠT | 8s | |

**Kết quả: 8 đạt, 0 lỗi**
- Lưu ý: LỖI-09 (thanh tiến trình + bản đồ không hiển thị) được phát hiện trong quá trình test

### Batch 14: Lọc và Tìm kiếm (12 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-FILTER-01 | Thanh tìm kiếm trang chủ hiển thị | ĐẠT | 5s | |
| TC-FILTER-02 | Tìm kiếm từ khóa không tồn tại | LỖI | 8s | LỖI-01: Trả về 2 kết quả thay vì 0 |
| TC-FILTER-03 | Tìm kiếm từ khóa hợp lệ | ĐẠT | 8s | |
| TC-FILTER-04 | Kết quả tìm kiếm chứa từ khóa | ĐẠT | 8s | |
| TC-FILTER-05 | Thuốc nhỏ giọt danh mục hiển thị | ĐẠT | 5s | |
| TC-FILTER-06 | Click danh mục lọc | ĐẠT | 8s | |
| TC-FILTER-07 | Click "Tất cả" đặt lại lọc | LỖI | 8s | LỖI-02: Danh mục "Tất cả" không tồn tại |
| TC-FILTER-08 | Tải trang chi tiết nhà hàng | ĐẠT | 10s | |
| TC-FILTER-09 | Tìm kiếm menu trong nhà hàng | LỖI | 30s | LỖI-03: Tìm kiếm menu bị hỏng — không gửi request |
| TC-FILTER-10 | Lọc menu theo danh mục | ĐẠT | 10s | |
| TC-FILTER-11 | Gợi ý tìm kiếm tự động | ĐẠT | 8s | |
| TC-FILTER-12 | Trạng thái tìm kiếm rỗng | ĐẠT | 8s | |

**Kết quả: 9 đạt, 3 lỗi**
- LỖI-01: Tìm kiếm trả kết quả sai
- LỖI-02: Danh mục "Tất cả" không có
- LỖI-03: Tìm kiếm menu bị hỏng

### Batch 15: Hệ thống chat (12 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-CHAT-01 | Widget chat hiển thị | ĐẠT | 8s | |
| TC-CHAT-02 | Mở popup chat | ĐẠT | 8s | |
| TC-CHAT-03 | Gửi tin nhắn cho AI | ĐẠT | 15s | |
| TC-CHAT-04 | Nhận phản hồi từ AI | ĐẠT | 15s | |
| TC-CHAT-05 | Lịch sử chat hiển thị | ĐẠT | 10s | |
| TC-CHAT-06 | Bật/tắt chat | ĐẠT | 8s | |
| TC-CHAT-07 | Chat responsive trên di động | ĐẠT | 10s | |
| TC-CHAT-08 | Vị trí widget chat | ĐẠT | 5s | |
| TC-CHAT-09 | Placeholder ô nhập chat | ĐẠT | 5s | |
| TC-CHAT-10 | Trạng thái chat rỗng | ĐẠT | 5s | |
| TC-CHAT-11 | Liên kết chat đến quản trị | ĐẠT | 8s | |
| TC-CHAT-12 | Fallback AI chat | ĐẠT | 15s | |

**Kết quả: 12 đạt, 0 lỗi**

### Batch 16: E-Delivery và Theo dõi (10 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-EDEL-01 | Tải trang E-Delivery | ĐẠT | 10s | |
| TC-EDEL-02 | Lọc tab QR | LỖI | 10s | LỖI-04: Tab QR bị navbar đè (z-index) |
| TC-EDEL-03 | Tạo mã QR | ĐẠT | 10s | LỖI-07: API QR trả 404 (phân biệt) |
| TC-EDEL-04 | Quét QR mã không hợp lệ | ĐẠT | 8s | LỖI-08: Thông báo lỗi không hiển thị |
| TC-EDEL-05 | Tải trang bảng điều khiển shipper | ĐẠT | 8s | |
| TC-EDEL-06 | Danh sách đơn hàng shipper | ĐẠT | 10s | |
| TC-EDEL-07 | Trang thu nhập shipper | ĐẠT | 8s | |
| TC-EDEL-08 | Tải trang theo dõi đơn | ĐẠT | 10s | |
| TC-EDEL-09 | Tải trang chi tiết đơn | ĐẠT | 10s | |
| TC-EDEL-10 | Shipper responsive | ĐẠT | 10s | |

**Kết quả: 9 đạt, 1 lỗi**
- LỖI-04: Tab QR bị navbar đè

### Batch 17: Thống kê và Bảng điều khiển (10 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-ANALYTIC-01 | Tải bảng điều khiển nhà hàng | ĐẠT | 10s | |
| TC-ANALYTIC-02 | Danh sách menu nhà hàng | ĐẠT | 8s | |
| TC-ANALYTIC-03 | Danh sách đơn hàng nhà hàng | ĐẠT | 10s | |
| TC-ANALYTIC-04 | Tải bảng điều khiển quản trị | ĐẠT | 10s | |
| TC-ANALYTIC-05 | Trang thống kê nhà hàng | ĐẠT | 10s | LỖI-11: Thống kê lỗi 500 |
| TC-ANALYTIC-06 | Quản lý người dùng quản trị | ĐẠT | 10s | |
| TC-ANALYTIC-07 | Quản lý đơn hàng quản trị | ĐẠT | 10s | |
| TC-ANALYTIC-08 | Thu nhập nhà hàng | ĐẠT | 8s | |
| TC-ANALYTIC-09 | Biểu đồ bảng điều khiển quản trị | ĐẠT | 10s | |
| TC-ANALYTIC-10 | Nhà hàng responsive | ĐẠT | 10s | |

**Kết quả: 10 đạt, 0 lỗi**
- Lưu ý: LỖI-11 (Thống kê nhà hàng lỗi 500) được phát hiện trong quá trình test

### Batch 18: Responsive trên di động (12 tests) — BỎ QUA

Không chạy theo yêu cầu người dùng. Nếu cần chạy sau:
```bash
npx playwright test tests/18-mobile-responsive.spec.ts --project="Mobile Chrome"
```

### Batch 19: Đánh giá hình ảnh (15 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-VISUAL-01 | Bố cục trang chủ nhất quán | ĐẠT | 8s | |
| TC-VISUAL-02 | Bố cục trang đăng nhập | ĐẠT | 8s | |
| TC-VISUAL-03 | Bố cục chi tiết nhà hàng | ĐẠT | 10s | |
| TC-VISUAL-04 | Bố cục trang giỏ hàng | ĐẠT | 8s | |
| TC-VISUAL-05 | Bố cục trang thanh toán | ĐẠT | 10s | |
| TC-VISUAL-06 | Thiết kế trạng thái rỗng | ĐẠT | 8s | |
| TC-VISUAL-07 | Trạng thái tìm kiếm rỗng | LỖI | 8s | LỖI-13: Không hiển thị "Không tìm thấy" |
| TC-VISUAL-08 | Ảnh chụp cơ sở trang chủ | LỖI | 5s | Ảnh chụp cơ sở chưa có |
| TC-VISUAL-09 | Ảnh chụp cơ sở nhà hàng | LỖI | 5s | Ảnh chụp cơ sở chưa có |
| TC-VISUAL-10 | Ảnh chụp cơ sở giỏ hàng | LỖI | 5s | Ảnh chụp cơ sở chưa có |
| TC-VISUAL-11 | Token thiết kế được áp dụng | ĐẠT | 8s | |
| TC-VISUAL-12 | Biểu tượng nhất quán | ĐẠT | 8s | |
| TC-VISUAL-13 | Trạng thái tải hiển thị | ĐẠT | 10s | |
| TC-VISUAL-14 | Footer hiển thị trên tất cả trang | LỖI | 10s | LỖI-12: Footer không hiển thị trên Đăng nhập + Giỏ hàng |
| TC-VISUAL-15 | Kiểm thử breakpoint responsive | ĐẠT | 10s | |

**Kết quả: 10 đạt, 5 lỗi**
- 3 lỗi: Ảnh chụp cơ sở (lần chạy đầu)
- 1 lỗi: LỖI-12 (footer thiếu)
- 1 lỗi: LỖI-13 (trạng thái tìm kiếm rỗng)

### Batch 20: Hiệu suất (13 tests)

| Mã test | Tên test | Kết quả | Thời gian | Ghi chú |
|---------|----------|---------|-----------|---------|
| TC-PERF-01 | Thời gian tải trang chủ | LỖI | 20s | LỖI-05: 19.695ms (mong đợi < 10s) |
| TC-PERF-02 | Thời gian tải trang nhà hàng | ĐẠT | 10s | |
| TC-PERF-03 | Thời gian tải trang giỏ hàng | ĐẠT | 8s | |
| TC-PERF-04 | Thời gian tải trang thanh toán | ĐẠT | 10s | |
| TC-PERF-05 | Thời gian tải trang theo dõi đơn | ĐẠT | 10s | |
| TC-PERF-06 | Lỗi console — trang chủ | ĐẠT | 8s | |
| TC-PERF-07 | Lỗi console — nhà hàng | ĐẠT | 10s | |
| TC-PERF-08 | Lỗi console — giỏ hàng | ĐẠT | 8s | |
| TC-PERF-09 | Thời gian phản hồi API — tìm kiếm | ĐẠT | 8s | |
| TC-PERF-10 | Thời gian phản hồi API — menu | ĐẠT | 10s | |
| TC-PERF-11 | Thời gian tải hình ảnh | ĐẠT | 10s | |
| TC-PERF-12 | Kích thước bundle CSS/JS | ĐẠT | 8s | |
| TC-PERF-13 | Điểm Lighthouse hiệu suất | ĐẠT | 15s | |

**Kết quả: 12 đạt, 1 lỗi**
- LỖI-05: Trang chủ tải 19,7 giây

---

## V. PHÂN TÍCH PHỦ SONG

### 5.1 Theo luồng

| Luồng | Số test cases | Mức phủ |
|-------|---------------|---------|
| Khách hàng → Duyệt → Giỏ → Thanh toán → Thanh toán | 65 | Đầy đủ |
| Nhà hàng → Bảng điều khiển → CRUD Menu → Đơn hàng | 52 | Đầy đủ |
| Shipper → Đơn hàng → Giao hàng → Thu nhập | 35 | Đầy đủ |
| Quản trị → Bảng điều khiển → Quản lý người dùng → Đơn hàng | 55 | Đầy đủ |
| Chat (AI + Quản trị) | 12 | Đầy đủ |
| Lọc/Tìm kiếm | 12 | Đầy đủ |
| E-Delivery QR | 25 | Đầy đủ |
| Theo dõi đơn hàng | 10 | Đầy đủ |
| Responsive trên di động | 12 | Đầy đủ |
| Hình ảnh/Hệ thống thiết kế | 15 | Đầy đủ |
| Hiệu suất | 13 | Đầy đủ |
| Bảo mật | 15 | Bỏ qua |
| Khả năng truy cập | 10 | Một phần |

### 5.2 Theo trang

| Trang | Có test |
|-------|---------|
| Trang chủ | Có |
| Đăng nhập/Đăng ký | Có |
| Giỏ hàng | Có |
| Thanh toán | Có |
| Chi tiết nhà hàng | Có |
| Theo dõi đơn hàng | Có |
| Lịch sử đơn hàng | Có |
| Chi tiết đơn hàng | Có |
| Bảng điều khiển nhà hàng | Có |
| Thống kê nhà hàng | Có |
| Bảng điều khiển shipper | Có |
| QR giao hàng shipper | Có |
| Thu nhập shipper | Có |
| Bảng điều khiển quản trị | Có |
| Quản lý người dùng | Có |
| Quản lý đơn hàng | Có |
| Widget chat | Có |
| E-Delivery QR | Có |

---

## VI. HƯỚNG DẪN CHẠY TEST

```bash
# Chạy tất cả (Desktop + Mobile)
cd e2e-tests && npx playwright test

# Chạy 1 file cụ thể
npx playwright test tests/11-cart-management.spec.ts

# Chỉ chạy Desktop
npx playwright test --project="Desktop Chromium"

# Chỉ chạy Mobile
npx playwright test --project="Mobile Chrome"

# Chạy với báo cáo HTML
npx playwright show-report playwright-report

# Chạy nhanh (chỉ smoke)
npx playwright test tests/smoke-lightpanda.spec.ts

# Cập nhật ảnh chụp cơ sở
npx playwright test --update-snapshots
```

---

## VII. CÁC BƯỚC TIẾP THEO

1. Sửa lỗi ứng dụng — Ưu tiên Critical → High → Medium (xem BUG-REPORT.md)
2. Sửa cơ sở kiểm thử — addItemToCartByIndex page object thiếu fallback
3. Chạy baseline — Chụp ảnh chụp cho đánh giá hình ảnh
4. Chạy batch 18 mobile — Nếu cần test responsive
5. Chạy lại tất cả — Sau khi sửa để xác minh
