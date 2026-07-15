# FastShip Full Fix Implementation Plan

> **For agentic workers:** Use inline execution with checkpoints.

**Goal:** Fix all production bugs + implement SePay Webhook + wallet + admin UI + accessibility

**Architecture:** ASP.NET Core 8 MVC + PostgreSQL + SignalR. Each task produces independently testable changes.

**Memo format (SePay):** `SEVQR FASTSHIP{OrderId}`
**Password:** Skip (user said no)

---

### Task 1: SePay Webhook — Endpoint + Memo Format

**Files:**
- Modify: `ShipFoodCore/Controllers/PaymentController.cs` — BankWebhook (already has [Route], auth, regex, amount check, SignalR)
- Modify: `ShipFoodCore/Controllers/CartController.cs` — OrderTracking QR memo format
- Fix: Duplicate `pa` variable in BankWebhook method (already fixed)

**Changes needed:**
- [ ] Verify memo format is `SEVQR FASTSHIP{OrderId}` ✅
- [ ] Verify [Route("Payment/BankWebhook")] exists ✅
- [ ] Verify SePay 'content' field parsing ✅
- [ ] Verify authorization token check ✅
- [ ] Verify amount comparison ✅
- [ ] Verify SignalR broadcast ✅

### Task 2: Fix Shipper Page (P1 + P2)

**Files:**
- Modify: `ShipFoodCore/Views/Shared/_LayoutPageShipper.cshtml` — jQuery load trước global.min.js ✅
- Modify: `ShipFoodCore/Controllers/ShipperController.cs` — PostgreSQL quoting cho raw SQL ✅

### Task 3: Fix RemoveDiacritics (P6 + P7)

**Files:**
- Modify: `ShipFoodCore/Controllers/HomeController.cs` — null check + Đ→D, đ→d ✅

### Task 4: MoMoService Virtual + VoucherService (P8 + P9)

**Files:**
- Modify: `ShipFoodCore/Services/MoMoService.cs` — VerifyIpnSignature virtual ✅
- Modify: `ShipFoodCore/Services/VoucherService.cs` — free ship gate chỉ >=50K ✅

### Task 5: UI/CSS Fixes (P5 + P12 + P14 + P15 + Admin)

**Files:**
- Modify: `ShipFoodCore/wwwroot/Source/Home/css/layout-sg.css` — sticky header + category pills scroll ✅
- Modify: `ShipFoodCore/Views/Shared/_LayoutPageHome.cshtml` — login/logout visibility ✅
- NEW: Fix admin dashboard CSS text overlap
- NEW: Fix admin logout UI

### Task 6: Admin CRUD Khuyến Mãi

**Files:**
- Investigate: `ShipFoodCore/Views/Admin/VoucherManager.cshtml` — check if CRUD works
- Investigate: `ShipFoodCore/Controllers/AdminController.cs` — check khuyenmai actions

### Task 7: SignalR Auth (P4)

**Files:**
- Modify: `ShipFoodCore/Hubs/Chats.cs` — thêm auth check trên connections

### Task 8: Wallet + Deposit cho Customer

**Files:**
- NEW: `ShipFoodCore/Views/Home/Wallet.cshtml` — trang ví tiền (copy từ Shipper wallet)
- Modify: `ShipFoodCore/Views/Shared/_LayoutPageHome.cshtml` — thêm link ví tiền
- Modify: `ShipFoodCore/Controllers/HomeController.cs` — thêm Wallet/NapTien actions

### Task 9: Accessibility (P10 + P11)

**Files:**
- Modify: Restaurant detail views — thêm alt text cho 7 ảnh
- Modify: Layout files — thêm aria-hidden="true" cho FA icons

### Task 10: Build + Test

- Run: `dotnet build`
- Run: `dotnet test`

### Task 11: Code Review

- Spawn code-reviewer-deepseek-flash to review all changes
