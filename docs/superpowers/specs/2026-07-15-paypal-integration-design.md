# PayPal Sandbox Integration — Design Doc

**Goal**: Cho phép khách hàng thanh toán đơn hàng qua PayPal Sandbox.

**Approach C (approved)**:
1. **Checkout**: User chọn PayPal → tạo đơn "Chờ thanh toán" → gọi `CreatePayPalOrder` → redirect sang PayPal
2. **OrderTracking**: Đơn "Chờ thanh toán" + PayPal → hiển thị nút "Thanh toán PayPal" → gọi `CreatePayPalOrder` → redirect
3. **Callback**: PayPal redirect về `/Payment/CapturePayPalOrder` → capture → update DB "Đã đặt" → SignalR → redirect OrderTracking

**Backend** (đã có từ commit a0cb0c4):
- `PayPalService.cs`: GetAccessToken, CreateOrder (VND→USD rate 25000), CaptureOrder
- `PaymentController.CreatePayPalOrder`: POST endpoint trả về approveLink
- `PaymentController.CapturePayPalOrder`: GET endpoint capture + update DB + SignalR

**Frontend** (cần làm):
- `Checkout.cshtml`: Thêm PayPal vào danh sách phương thức thanh toán + AJAX redirect
- `OrderTracking.cshtml`: Nút "Thanh toán PayPal" cho đơn "Chờ thanh toán"

**Fix kèm**:
- Login cart loss: sửa thứ tự SetCart vs Session.Clear trong HomeController.cs
