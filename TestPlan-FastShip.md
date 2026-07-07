# 📋 FASTSHIP TEST PLAN — Chi Tiết Từng Chức Năng

> **Website**: https://shipfood.up.railway.app/
> **Version**: 5.1

---

## 📑 MỤC LỤC

1. [TEST CASE 1: Khách Hàng — Trang chủ & Tìm kiếm](#tc1)
2. [TEST CASE 2: Khách Hàng — Xem quán & Thêm giỏ hàng](#tc2)
3. [TEST CASE 3: Khách Hàng — Checkout & Thanh toán](#tc3)
4. [TEST CASE 4: Khách Hàng — Lịch sử & Đánh giá](#tc4)
5. [TEST CASE 5: Khách Hàng — Chat & Notification](#tc5)
6. [TEST CASE 6: Admin — Dashboard & Thống kê](#tc6)
7. [TEST CASE 7: Admin — CRUD Người dùng](#tc7)
8. [TEST CASE 8: Admin — CRUD Danh mục](#tc8)
9. [TEST CASE 9: Admin — Quản lý Đơn hàng & Chat](#tc9)
10. [TEST CASE 10: Quán Ăn — Dashboard & Quản lý món](#tc10)
11. [TEST CASE 11: Quán Ăn — Xử lý đơn hàng & Khuyến mãi](#tc11)
12. [TEST CASE 12: Shipper — FREE-PICK & Nhận đơn](#tc12)
13. [TEST CASE 13: Shipper — Map Tracking & Thu nhập](#tc13)
14. [TEST CASE 14: Cross-cutting — Responsive, Lỗi, Bảo mật](#tc14)

---

<a name="tc1"></a>
## TC1: Khách Hàng — Trang chủ & Tìm kiếm

**Mục tiêu**: Kiểm tra toàn bộ luồng duyệt web của khách hàng chưa đăng nhập

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 1.1 | Truy cập https://shipfood.up.railway.app/ | Trang chủ load trong < 5s, không console error | |
| 1.2 | Kiểm tra hero carousel | Carousel tự động chạy, 3+ slide, crossfade mượt | |
| 1.3 | Kiểm tra topbar | Hiển thị SĐT, email, social icons (FB, IG, TikTok, YouTube) | |
| 1.4 | Kiểm tra navbar | Logo "Fastship", search input, cart icon, user menu | |
| 1.5 | Kiểm tra danh sách quán ăn | Grid hiển thị 6+ quán, mỗi quán có ảnh, tên, địa chỉ, rating | |
| 1.6 | Kiểm tra skeleton loading | Khi load lần đầu, hiển thị shimmer skeleton thay vì spinner | |
| 1.7 | Kiểm tra footer | 4 cột: Về chúng tôi, Liên kết, Liên hệ, Newsletter + social | |
| 1.8 | Click vào quán ăn đầu tiên | Chuyển đến /Home/DetailRestaurant/{id} | |
| 1.9 | Kiểm tra breadcrumb | Breadcrumb: Trang chủ > Tên quán | |
| 1.10 | Nhập từ khóa vào search bar + Enter | Reload trang với kết quả lọc | |
| 1.11 | Nhập từ khóa không dấu (VD: "com") | Tìm được quán có tên "Cơm" (không dấu) | |
| 1.12 | Check thanh filter bar | Các chip filter: Tất cả, Khuyến mãi, Đánh giá tốt, $, $$, $$$ | |
| 1.13 | Click chip "Đánh giá tốt" | Lọc quán có rating >= 4.4 | |
| 1.14 | Click "Bộ lọc" → bottom sheet | Bottom sheet slide lên với 4 sections | |
| 1.15 | Chọn filter trong bottom sheet → "Áp dụng" | AJAX reload danh sách quán | |

---

<a name="tc2"></a>
## TC2: Khách Hàng — Xem quán & Thêm giỏ hàng

**Mục tiêu**: Kiểm tra chi tiết quán, menu, thêm vào giỏ

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 2.1 | Truy cập /Home/DetailRestaurant/{id} của quán đang mở cửa | Hiển thị ảnh quán, thông tin, rating, địa chỉ | |
| 2.2 | Kiểm tra sidebar danh mục | Danh sách danh mục món ăn (Cơm, Phở, Lẩu...) | |
| 2.3 | Click vào danh mục "Cơm" | Lọc chỉ hiển thị món Cơm, scroll đến section đó | |
| 2.4 | Nhập từ khóa vào search món | Lọc món theo tên (có hỗ trợ không dấu) | |
| 2.5 | Click nút "+" trên món ăn | Thêm vào giỏ hàng, badge cart tăng lên | |
| 2.6 | Click nút "Thêm vào giỏ" trên món khác quán | Hiện popup xác nhận "Đã có món từ quán khác" | |
| 2.7 | Xác nhận chuyển quán (ForceSwitch) | Giỏ hàng reset, chỉ còn món từ quán mới | |
| 2.8 | Kiểm tra FAB button (mobile) | Hiển thị ở góc dưới phải với badge đếm danh mục | |
| 2.9 | Click FAB → bottom sheet category | Bottom sheet hiển thị danh sách danh mục với icon emoji | |
| 2.10 | Chọn danh mục từ bottom sheet | Đóng sheet, scroll đến danh mục đã chọn | |
| 2.11 | Cuộn xuống phần đánh giá | Grid reviews, mỗi card có tên, rating sao, nhận xét | |
| 2.12 | Click "Xem thêm" đánh giá | Load thêm 5 review (server-side pagination) | |

---

<a name="tc3"></a>
## TC3: Khách Hàng — Checkout & Thanh toán

**Required**: Đã đăng nhập (tranthib/abcdef), giỏ hàng có món

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 3.1 | Truy cập /Cart | Hiển thị danh sách món trong giỏ, mỗi món có ảnh, tên, giá, số lượng, nút xoá | |
| 3.2 | Click nút Tăng số lượng [+] | Số lượng tăng, tổng tiền cập nhật real-time (AJAX) | |
| 3.3 | Click nút Giảm số lượng [-] | Số lượng giảm (không xuống dưới 1) | |
| 3.4 | Click nút Xoá món 🗑 | Món bị xoá khỏi giỏ, tổng tiền cập nhật | |
| 3.5 | Kiểm tra khuyến mãi (nếu có) | Hiển thị coupon popup với danh sách mã giảm giá khả dụng | |
| 3.6 | Click "Áp dụng mã" | Gọi API CheckCoupon, giảm giá cập nhật | |
| 3.7 | Click "Thanh toán" | Chuyển sang /Cart/Checkout | |
| 3.8 | Kiểm tra form Checkout | 3 sections: Địa chỉ giao hàng, Ghi chú & KM, Phương thức TT | |
| 3.9 | Chọn địa chỉ có sẵn | Address card highlight, tự động điền thông tin | |
| 3.10 | Nhập địa chỉ mới | Form validate họ tên (2-100), SĐT (10-11 số, bắt đầu 0) | |
| 3.11 | Chọn phương thức thanh toán COD | Radio selected, không redirect | |
| 3.12 | Thêm ghi chú | Text area lưu được | |
| 3.13 | Click "Xác nhận đặt hàng" | Xử lý success/failure, redirect đến SuccessView/FailureView | |
| 3.14 | Kiểm tra SuccessView | Hiển thị "Đặt hàng thành công! Mã đơn hàng: #XXX" | |
| 3.15 | Kiểm tra giỏ hàng sau đặt | Giỏ hàng trống (đã được clear) | |
| 3.16 | Kiểm tra nhận đơn real-time (SignalR) | Nếu đang ở dashboard Restaurant, đơn mới hiện lên real-time | |

---

<a name="tc4"></a>
## TC4: Khách Hàng — Lịch sử & Đánh giá

**Required**: Đã đăng nhập, đã từng đặt hàng

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 4.1 | Truy cập /Cart/LichSuDatHang | DataTable hiển thị danh sách đơn hàng (Mã ĐH, Ngày, Quán, Món, Tổng tiền, Trạng thái) | |
| 4.2 | Click vào một đơn hàng | Chuyển đến /Cart/ChiTietDonHang/{id} | |
| 4.3 | Kiểm tra ChiTietDonHang | Hiển thị thông tin quán, danh sách món (tên + số lượng + giá), tổng tiền, trạng thái | |
| 4.4 | Click "Theo dõi" trên đơn hàng đang giao | Chuyển đến /Cart/OrderTracking/{id} | |
| 4.5 | Kiểm tra OrderTracking (nếu đang giao) | 7-step progress bar + Leaflet map (nếu shipper đang stream location) | |
| 4.6 | Click "Đánh giá" trên đơn hoàn thành | Hiện form đánh giá với star picker + textarea | |
| 4.7 | Chọn 4 sao + nhập nhận xét | Gửi AJAX, thông báo "Cảm ơn bạn đã đánh giá!" | |
| 4.8 | Thử đánh giá lại món đã đánh giá | Báo lỗi "Bạn đã đánh giá món này rồi" | |
| 4.9 | Kiểm tra filter DataTable | Search, sort theo cột, phân trang hoạt động | |
| 4.10 | Check empty state (nếu chưa có đơn) | Hiển thị icon box + "Bạn chưa có đơn hàng nào" + CTA | |

---

<a name="tc5"></a>
## TC5: Khách Hàng — Chat Widget

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 5.1 | Click chat bubble (góc dưới phải) | Chat widget mở: 2 tabs "AI Chat" + "Support" | |
| 5.2 | Chọn tab "AI Chat" | Hiển thị khung chat với AI Gemini | |
| 5.3 | Gõ "gợi ý món ăn" → Send | AI trả lời với gợi ý món, typing indicator 3 dots animation | |
| 5.4 | Gõ "mã 1" (tra cứu đơn hàng) | AI trả về trạng thái đơn hàng #1 với emoji | |
| 5.5 | Click quick reply | Gửi tin nhắn mặc định, AI phản hồi | |
| 5.6 | Chọn tab "Support" | Hiển thị lịch sử chat với admin (nếu có) | |
| 5.7 | Gửi tin nhắn support | Message lưu DB, broadcast real-time đến admin | |
| 5.8 | Đóng chat widget | Widget thu nhỏ về bubble, animation scale-in/out | |
| 5.9 | Kiểm tra unread badge | Nếu có tin nhắn mới từ admin, bubble hiển thị badge đỏ | |

---

<a name="tc6"></a>
## TC6: Admin — Dashboard & Thống kê

**Required**: Login admin1/admin1

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 6.1 | Login admin1/admin1 → redirect /Admin | Dashboard load thành công | |
| 6.2 | Kiểm tra 4 KPI cards | Hiển thị: Doanh thu, Đơn hàng, Khách hàng mới, Quán ăn | |
| 6.3 | Kiểm tra biểu đồ Revenue Chart | Chart.js line chart, data 30 ngày gần nhất | |
| 6.4 | Kiểm tra Top Restaurants chart | Bar chart top 5 quán theo doanh thu | |
| 6.5 | Kiểm tra Order Status Pie chart | Doughnut chart: Hoàn thành, Đã hủy, Đang xử lý | |
| 6.6 | Chọn date filter → "Lọc" | Các KPI và biểu đồ cập nhật theo khoảng ngày | |
| 6.7 | Click "Export CSV" | Download file CSV với danh sách đơn hoàn thành | |
| 6.8 | Kiểm tra sidebar | Menu items: Dashboard, Quản lý user, Danh mục, Đơn hàng, Chat | |

---

<a name="tc7"></a>
## TC7: Admin — CRUD Người dùng

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 7.1 | Click "Quản lý Khách hàng" | /Admin/QuanLyKhachHang, danh sách khách hàng với DataTable | |
| 7.2 | Click "Quản lý Quán ăn" | /Admin/QuanLyQuanAn, danh sách quán ăn, có nút Duyệt/Hủy | |
| 7.3 | Click "Khóa" trên một Khách hàng | user.trangthai = 2, button chuyển thành "Mở khóa" | |
| 7.4 | Click "Mở khóa" | user.trangthai = 1 | |
| 7.5 | Thử khóa Admin cuối cùng | Báo lỗi "Không thể khóa tài khoản Admin cuối cùng" | |
| 7.6 | Click "Thêm" user mới | Form tạo user (username, pwd, email, SĐT, role) | |
| 7.7 | Điền form + submit | User được tạo, profile data tương ứng theo role | |

---

<a name="tc8"></a>
## TC8: Admin — CRUD Danh mục

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 8.1 | Click "Danh mục" | /Admin/Category, danh sách danh mục + search | |
| 8.2 | Click "Tạo danh mục" | Form tạo: tên, mô tả, hình ảnh | |
| 8.3 | Điền form + submit | Danh mục mới xuất hiện trong danh sách | |
| 8.4 | Click "Sửa" trên danh mục | /Admin/EditCategory/{id}, form pre-filled | |
| 8.5 | Sửa tên → submit | Tên danh mục cập nhật | |
| 8.6 | Click "Xóa" trên danh mục có món | Báo lỗi "Không thể xóa, vẫn còn món ăn" (ON DELETE RESTRICT) | |
| 8.7 | Xóa danh mục không có món | Xóa thành công | |

---

<a name="tc9"></a>
## TC9: Admin — Quản lý Đơn hàng & Chat

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 9.1 | Click "Đơn hàng" | /Admin/Order, danh sách đơn hàng (nếu fix đã hoạt động) | |
| 9.2 | Click "Chi tiết" trên 1 đơn | /Admin/OrderDetail/{id}, xem chi tiết đơn + dropdown action | |
| 9.3 | Click "Admin Chat" | /AdminChat, danh sách hội thoại khách hàng (nếu fix đã hoạt động) | |
| 9.4 | Chọn 1 khách hàng từ danh sách | Load lịch sử tin nhắn, input chat hiện ra | |
| 9.5 | Gõ tin nhắn → Send | Message lưu DB + broadcast SignalR đến khách hàng | |
| 9.6 | Kiểm tra unread dot | Khách hàng chưa đọc có chấm đỏ pulse animation | |

---

<a name="tc10"></a>
## TC10: Quán Ăn — Dashboard & Quản lý món

**Required**: Login konekopizza/konekopizza (nếu fix đã hoạt động)

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 10.1 | Login → redirect /Restaurant | Dashboard quán ăn load (nếu fix đã hoạt động) | |
| 10.2 | Kiểm tra 4 KPI cards | Số món, Doanh thu, Đơn hàng, Khách hàng | |
| 10.3 | Click "Quản lý món" | /Restaurant/ProductList, danh sách món ăn + tình trạng Còn hàng/Hết | |
| 10.4 | Click "Thêm món" | Form thêm món: tên, danh mục, giá (size), ảnh | |
| 10.5 | Điền form + submit | Món mới xuất hiện trong danh sách | |
| 10.6 | Toggle "Còn hàng" 1-click | AJAX toggle conhang, badge cập nhật real-time | |
| 10.7 | Click "Sửa món" | Form edit pre-filled | |
| 10.8 | Click "Xóa món" | Món bị xóa khỏi danh sách | |

---

<a name="tc11"></a>
## TC11: Quán Ăn — Xử lý đơn hàng & Khuyến mãi

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 11.1 | Click "Đơn hàng" | /Restaurant/OrderList, danh sách đơn kèm nút Nhận/Hủy | |
| 11.2 | Click "Nhận đơn" | Trạng thái → "Đã xác nhận", SignalR broadcast đến Customer | |
| 11.3 | Click "Chuẩn bị xong" | Trạng thái → "Chờ shipper lấy hàng", broadcast đến Shippers | |
| 11.4 | Click "Hủy đơn" | Trạng thái → "Đã hủy", MoMo Refund nếu thanh toán MoMo | |
| 11.5 | Click "Khuyến mãi" | /Restaurant/Discount, form thêm KM + danh sách KM hiện tại | |
| 11.6 | Thêm KM cho món: chọn món + chọn KM + % giảm | KM được gắn vào món, badge KM hiển thị | |
| 11.7 | Click "Phân tích" | /Restaurant/Analytics, doanh thu theo danh mục, top món bán chạy | |
| 11.8 | Click "Đánh giá" | /Restaurant/Review, danh sách đánh giá + thống kê sao | |
| 11.9 | Toggle trạng thái quán (Đóng/Mở) | 1 click toggle, badge cập nhật | |

---

<a name="tc12"></a>
## TC12: Shipper — FREE-PICK & Nhận đơn

**Required**: Login shippery/shipy456 (nếu fix đã hoạt động)

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 12.1 | Login → redirect /Shipper | Dashboard Shipper load (nếu fix đã hoạt động) | |
| 12.2 | Kiểm tra bảng FREE-PICK | Danh sách đơn chưa có shipper (raw SQL query) | |
| 12.3 | Click "Chấp nhận" trên đơn FREE-PICK | Chuyển đến /Shipper/OrderDetail/{id} | |
| 12.4 | Kiểm tra map OrderDetail | Leaflet map với 2 marker: pickup (quán) + delivery (khách) | |
| 12.5 | Click "Lấy hàng" | AJAX cập nhật trạng thái → "Đã lấy", SignalR broadcast | |
| 12.6 | Click "Hoàn thành" | AJAX → "Hoàn thành", SignalR đến customer + restaurant | |

---

<a name="tc13"></a>
## TC13: Shipper — Map Tracking & Thu nhập

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 13.1 | Kiểm tra geolocation streaming | watchPosition gửi tọa độ 5s/lần qua SignalR UpdateLocation | |
| 13.2 | Kiểm tra Leaflet marker smooth transition | Marker di chuyển mượt (transition: transform 0.5s) | |
| 13.3 | Click "Thu nhập" | /Shipper/ThuNhap, thống kê 30 ngày + hôm nay | |
| 13.4 | Click "Ví tiền" | /Shipper/ViTien, danh sách đơn + số dư | |
| 13.5 | Click "Lịch sử" | /Shipper/LichSu, danh sách đơn đã giao | |
| 13.6 | Click "Cài đặt" | /Shipper/CaiDat, form cập nhật thông tin cá nhân | |

---

<a name="tc14"></a>
## TC14: Cross-cutting — Responsive, Lỗi, Bảo mật

| Step | Thao tác | Expected Result | Actual |
|------|----------|----------------|--------|
| 14.1 | Resize màn hình xuống 375px (mobile) | Layout responsive: sidebar ẩn, cards xếp dọc, touch targets >= 44px | |
| 14.2 | Test keyboard navigation (Tab) | Focus visible ring (3px solid var(--fs-green)) | |
| 14.3 | Test prefers-reduced-motion | Animation tắt (duration 0.01ms) | |
| 14.4 | Truy cập /Admin khi chưa login | Redirect về /Home/Login | |
| 14.5 | Login customer → truy cập /Admin | RoleGuard redirect về /Home (403) | |
| 14.6 | Gửi form login sai 5 lần | Rate limiting 429: "Vui lòng thử lại sau X giây" | |
| 14.7 | Gửi AJAX request với token sai | 400 Bad Request (Antiforgery validation) | |
| 14.8 | Truy cập /health | {"status":"healthy"} 200 OK | |
| 14.9 | Kiểm tra console (F12) | Không có lỗi JS, chỉ accessibility warnings | |
