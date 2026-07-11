# 🎨 Kế hoạch Redesign — FastShip v2

> Tài liệu này phác thảo kế hoạch thiết kế lại giao diện toàn diện.
> **Lưu ý**: Style cụ thể (màu sắc, font chữ) do người dùng quyết định và cập nhật sau.

---

## 📋 Mục tiêu

Thiết kế lại **toàn bộ giao diện** của FastShip web app, bao gồm:
- Người dùng (Home/Customer): Trang chủ, Detail quán, Cart, Checkout
- Chủ quán (Restaurant Dashboard): Toàn bộ trang quản lý
- Shipper (Shipper Dashboard): Toàn bộ trang
- Admin (Admin Dashboard): Toàn bộ trang
- Shared components: Layouts, Header, Footer, Chat, Notification

---

## 🗺 Phạm vi chi tiết

### Tổng số views cần redesign: ~50+ views

| STT | Nhóm | Số views | Layout | Ưu tiên |
|-----|------|----------|--------|---------|
| 1 | **Home (Customer)** | 12 views | `_LayoutPageHome` | 🔴 P0 |
| 2 | **Cart / Checkout** | 8 views | `_LayoutPageHome` | 🔴 P0 |
| 3 | **Restaurant Dashboard** | 10 views | `_LayoutPageRestaurant` | 🟡 P1 |
| 4 | **Shipper Dashboard** | 8 views | `_LayoutPageShipper` | 🟡 P1 |
| 5 | **Admin Dashboard** | 13 views | `_LayoutPageAmin` | 🟢 P2 |
| 6 | **Shared/Layouts** | 9 files | — | 🔴 P0 |
| 7 | **Chat/Notification** | 3 views | — | 🟢 P2 |

---

## 📂 File mapping — từng view

### Phase 1: Core Layout + Home (P0)

#### Layouts cần sửa
| File | Ghi chú |
|------|---------|
| `Views/Shared/_LayoutPageHome.cshtml` | Layout public chính |
| `Views/Shared/_Layout.cshtml` | Fallback |
| `Views/Shared/_LayoutAuth.cshtml` | Layout login/signup |
| `Views/Shared/_ChatWidget.cshtml` | Chat widget partial |

#### Home pages
| File | Ghi chú |
|------|---------|
| `Views/Home/Index.cshtml` | 🏠 Trang chủ — hero, danh sách quán, gợi ý |
| `Views/Home/DetailRestaurant.cshtml` | 🍽 Chi tiết quán — menu, filter bar, cart side |
| `Views/Home/SanPham.cshtml` | Chi tiết sản phẩm |
| `Views/Home/DanhMuc.cshtml` | Xem theo danh mục |
| `Views/Home/Login.cshtml` | Đăng nhập |
| `Views/Home/Signup.cshtml` | Đăng ký |
| `Views/Home/Forgot.cshtml` | Quên mật khẩu |
| `Views/Home/NhanTin.cshtml` | Chat |
| `Views/Home/About.cshtml` | Giới thiệu |
| `Views/Home/Contact.cshtml` | Liên hệ |
| `Views/Home/ChiTietSanPham.cshtml` | Chi tiết SP (khác) |
| `Views/Home/SelectRoleGoogle.cshtml` | Chọn role sau Google login |

#### CSS/JS core
| File | Ghi chú |
|------|---------|
| `wwwroot/Source/Shared/css/fastship-design-tokens.css` | Design tokens — **sửa đầu tiên** |
| `wwwroot/Source/Home/css/style.css` | Style Home template |
| `wwwroot/Source/Home/css/bootstrap.css` | Bootstrap override |
| `wwwroot/js/cart-local.js` | Cart JS |
| `wwwroot/js/filter.js` | Filter JS |
| `wwwroot/js/lucide-icons.js` | Icon system |
| `wwwroot/js/map.js` | Map JS (Leaflet) |

### Phase 2: Cart & Checkout (P0)

| File | Ghi chú |
|------|---------|
| `Views/Cart/Index.cshtml` | Giỏ hàng |
| `Views/Cart/Checkout.cshtml` | **One-page checkout** — priority |
| `Views/Cart/OrderTracking.cshtml` | Theo dõi đơn (SignalR + progress bar) |
| `Views/Cart/LichSuDatHang.cshtml` | Lịch sử đặt hàng |
| `Views/Cart/ChiTietDonHang.cshtml` | Chi tiết đơn hàng |
| `Views/Cart/EInvoice.cshtml` | Hoá đơn điện tử |
| `Views/Cart/SuccessView.cshtml` | Thanh toán thành công |
| `Views/Cart/FailureView.cshtml` | Thanh toán thất bại |
| `wwwroot/Source/Cart/` | Cart template CSS |

### Phase 3: Restaurant Dashboard (P1)

| File | Ghi chú |
|------|---------|
| `Views/Restaurant/Index.cshtml` | Dashboard tổng quan |
| `Views/Restaurant/OrderList.cshtml` | Danh sách đơn hàng |
| `Views/Restaurant/ProductList.cshtml` | Quản lý món ăn |
| `Views/Restaurant/ProductDetail.cshtml` | Thêm/sửa món |
| `Views/Restaurant/Analytics.cshtml` | Thống kê (Chart.js) |
| `Views/Restaurant/GeneralCustomer.cshtml` | Khách hàng |
| `Views/Restaurant/Discount.cshtml` | Khuyến mãi |
| `Views/Restaurant/Review.cshtml` | Đánh giá |
| `Views/Restaurant/Profile.cshtml` | Thông tin quán |
| `Views/Restaurant/Wallet.cshtml` | Ví tiền |
| `Views/Shared/_LayoutPageRestaurant.cshtml` | Layout restaurant |
| `wwwroot/Source/Restaurant/` | Restaurant template CSS |

### Phase 4: Shipper Dashboard (P1)

| File | Ghi chú |
|------|---------|
| `Views/Shipper/Index.cshtml` | Dashboard — nhận đơn + bản đồ |
| `Views/Shipper/OrderDetail.cshtml` | Chi tiết đơn giao |
| `Views/Shipper/ThuNhap.cshtml` | Thu nhập |
| `Views/Shipper/ViTien.cshtml` | Ví tiền |
| `Views/Shipper/LichSu.cshtml` | Lịch sử |
| `Views/Shipper/NhanTin.cshtml` | Chat |
| `Views/Shipper/ThongBao.cshtml` | Thông báo |
| `Views/Shipper/CaiDat.cshtml` | Cài đặt |
| `Views/Shared/_LayoutPageShipper.cshtml` | Layout shipper |
| `Views/Shared/LayoutPageShipper.cshtml` | Layout shipper (old) |
| `wwwroot/Source/Shipper/` | Shipper template CSS |

### Phase 5: Admin Dashboard (P2)

| File | Ghi chú |
|------|---------|
| `Views/Admin/Dashboard.cshtml` | Dashboard tổng quan |
| `Views/Admin/Index.cshtml` | Danh sách |
| `Views/Admin/QuanLyQuanAn.cshtml` | Quản lý quán |
| `Views/Admin/QuanLyKhachHang.cshtml` | Quản lý khách |
| `Views/Admin/QuanLyShipper.cshtml` | Quản lý shipper |
| `Views/Admin/QuanLyQuanTriVien.cshtml` | Quản lý admin |
| `Views/Admin/Order.cshtml` | Đơn hàng |
| `Views/Admin/OrderDetail.cshtml` | Chi tiết đơn |
| `Views/Admin/Category.cshtml` | Danh mục |
| `Views/Admin/CreateCategory.cshtml` | Thêm danh mục |
| `Views/Admin/EditCategory.cshtml` | Sửa danh mục |
| `Views/Admin/PostTaiKhoan.cshtml` | Tài khoản |
| `Views/Shared/_LayoutPageAmin.cshtml` | Layout admin (lưu ý typo: 'Amin') |
| `wwwroot/Source/Admin/` | Admin template CSS |

---

## 📐 Quy tắc thiết kế

### Layout Hierarchy (giữ nguyên cấu trúc)
```
_LayoutPage*.cshtml  ← chứa <head>, <body>, nav, footer
    └── View.cshtml  ← chỉ RenderBody(), giữ nguyên @section
```

### File cần thay đổi khi redesign 1 view:
1. **View `.cshtml`** — HTML structure
2. **Template CSS** — `wwwroot/Source/<Role>/css/style.css`
3. **Design tokens** — `wwwroot/Source/Shared/css/fastship-design-tokens.css` (nếu cần global)
4. **Layout** — `Views/Shared/_LayoutPage*.cshtml` (nếu cần thay đổi cấu trúc)

### Lưu ý khi redesign:
- ⚠️ Không thay đổi Model/Controller/Service (chỉ UI)
- ⚠️ Giữ nguyên `@section`, `@RenderBody()`, `@await Component.InvokeAsync()`
- ⚠️ Giữ nguyên `id`/`class` quan trọng cho JS (cart, filter, map)
- ⚠️ Kiểm tra icon class trước khi đổi — FA vs Lucide

---

## 🔍 Các tính năng cần kiểm tra sau redesign

| Tính năng | Vị trí | Kiểm tra |
|-----------|--------|----------|
| 🔍 Filter thực đơn | `DetailRestaurant.cshtml` + `filter.js` | Click category → lọc đúng |
| 🛒 Cart localStorage | `cart-local.js` | Thêm món → persist → checkout |
| 📍 Order Tracking | `OrderTracking.cshtml` + SignalR | Progress bar + map |
| 💬 Chat | `NhanTin.cshtml` + `/nhantin` hub | Gửi tin nhắn real-time |
| 🗺 Map | `map.js` + Shipper | Hiển thị vị trí |
| 🤖 Gemini Chatbot | `_ChatWidget.cshtml` + `ChatbotController` | Hỏi đáp AI |
| 💳 MoMo Payment | `PaymentController` + `MoMoService` | Thanh toán thành công |
| ✅ E-Invoice | `EDeliveryService.cs` + `EInvoice.cshtml` | Tạo hoá đơn sau thanh toán |

---

## ✅ Checklist — từng view

Khi redesign 1 view, hoàn thành checklist này:

- [ ] HTML structure redesign
- [ ] CSS styling
- [ ] Responsive (mobile-first)
- [ ] Icons kiểm tra (FA/Lucide không bị lỗi)
- [ ] JS functionality còn hoạt động (cart, filter, map)
- [ ] Test các nút, link, form
- [ ] Test trên mobile (Chrome DevTools responsive)
