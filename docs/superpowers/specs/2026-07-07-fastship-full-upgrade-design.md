# FastShip Full Upgrade Design Spec

> **Date:** 2026-07-07  
> **Author:** Buffy AI Agent  
> **Status:** Approved

## Overview

Comprehensive upgrade of FastShip food delivery platform covering search/filter, checkout/payment, role-based routing, real-time tracking, and cart persistence.

## Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Cart Persistence | Frontend localStorage | Simple, fast, no API needed |
| Payment Gateway | MoMo (real sandbox) | Most popular in Vietnam |
| Filter UI | ViewComponent | Clean ASP.NET architecture |
| Routing Guard | Middleware | Centralized, no role confusion |
| Real-time Level | Comprehensive (Live Map + Dashboard) | Full UX |
| Map Library | Leaflet.js | Open-source, no API key |

## Phase 1: 🔍 Advanced Search & Dual-Filter Bar

### FilterBarViewComponent
- Location: `ShipFoodCore/ViewComponents/FilterBarViewComponent.cs`
- Renders horizontal scroll chip bar + Bottom Sheet trigger
- Receives: categories, current filters from query params
- Returns: ViewComponent with filter chips and bottom sheet

### Filter API (HomeController)
- `GET /Home/MenuSearch` — Dynamic SQL search on tbMonAn
- Query params: `q` (search), `categoryId`, `sortBy`, `isPromo`, `isBestSeller`, `isNearMe`, `maxPriceLevel`, `maxDiet`
- Returns JSON with dish list including variants and avg rating

### Dynamic SQL logic:
- LEFT JOIN tbMonAnKhuyenMai when isPromo=true
- WHERE tbQuanAn.diemdanhgia >= 4.4 for "Đánh giá tốt"
- Price range filtering by $ levels (1$=0-20000, 2$=20000-50000, 3$=50000-100000, 4$=100000+)
- ORDER BY sortBy (gợi ý, đánh giá, phí ship, khoảng cách)

### Frontend (filter.js)
- localStorage for cart persistence
- Dual-Filter Bar: horizontal scroll with filter chips
- Bottom Sheet: slide-up panel with radio/checkbox/grid
- Two-way sync: chip ↔ bottom sheet state
- Quick filter chips: "Gần đây", "Dưới 30 phút", "Khuyến mãi", "Đánh giá tốt"

## Phase 2: 🛒 One-page Checkout + MoMo

### Cart Persistence (localStorage)
- Save cart to localStorage on every add/remove
- On page load, restore cart from localStorage
- Fallback: if Session cart exists, prefer Session

### One-page Checkout
- Single view: `Views/Cart/Checkout.cshtml` (refactored)
- Sections: order summary, default address, payment method → confirm
- Pre-filled default address from tbThongTinDatHang

### MoMo Integration
- Service: `Services/MoMoService.cs`
- Endpoints: Create payment request, check transaction status
- MoMo sandbox API (test mode)
- IPN callback handling for payment confirmation

### Error Handling
- try-catch in ProcessPayment with specific error codes
- Frontend displays detailed error message from API response

## Phase 3: 🚦 RoleGuard Middleware

### Middleware
- File: `Middleware/RoleGuardMiddleware.cs`
- Route mapping: /Admin → Admin, /Restaurant → Quán ăn, /Shipper → Shipper, default → Khách hàng
- On mismatch: return 403 + redirect to appropriate dashboard

### Changes:
- Add middleware to Program.cs pipeline
- Remove redundant checkLogin() from individual controllers (optional)
- Block cross-role access at middleware level

## Phase 4: 📦 Real-time Order Tracking

### SignalR Hub Extension
- Extend `Chats.cs` with `OrderStatusUpdate(orderId, status, timestamp)`
- Groups: `order_{orderId}` for customer tracking
- Events: `orderStatusChanged`, `shipperAssigned`, `shipperLocationUpdate`

### Order Tracking View
- File: `Views/Cart/OrderTracking.cshtml` (new)
- Progress bar with 5 steps: Đã đặt → Xác nhận → Chuẩn bị → Đang giao → Hoàn thành
- Live map showing shipper location (Leaflet.js)

### Status Flow
```
Đã đặt → Đã xác nhận → Đang chuẩn bị → Chờ shipper lấy → Đang giao → Hoàn thành
```

## Phase 5: 🗺 Live Map + Dashboard Real-time

### Live Map (Leaflet.js)
- Embedded in OrderTracking view
- Shipper broadcasts lat/lng via SignalR UpdateLocation
- Customer sees moving marker on map

### Admin Dashboard Real-time
- SignalR events for new orders, status changes
- Real-time stats update without page refresh
- Order notifications popup

---

## File Map

```
NEW FILES:
  ShipFoodCore/ViewComponents/FilterBarViewComponent.cs
  ShipFoodCore/Views/Shared/Components/FilterBar/Default.cshtml
  ShipFoodCore/Middleware/RoleGuardMiddleware.cs
  ShipFoodCore/Services/MoMoService.cs
  ShipFoodCore/wwwroot/js/filter.js
  ShipFoodCore/wwwroot/js/cart-local.js
  ShipFoodCore/wwwroot/js/map.js
  ShipFoodCore/Views/Cart/OrderTracking.cshtml

MODIFIED FILES:
  ShipFoodCore/Controllers/HomeController.cs      — MenuSearch API
  ShipFoodCore/Controllers/PaymentController.cs   — MoMo integration + try-catch
  ShipFoodCore/Controllers/CartController.cs       — localStorage support
  ShipFoodCore/Views/Cart/Checkout.cshtml          — One-page refactor
  ShipFoodCore/Views/Home/Index.cshtml             — Filter bar integration
  ShipFoodCore/Hubs/Chats.cs                      — Order status events
  ShipFoodCore/Program.cs                         — Middleware registration
  ShipFoodCore/Models/Cart.cs                     — JSON helper methods
```
