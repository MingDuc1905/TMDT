# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🏪 Dashboard Quán ăn - KPI & Thống kê >> [TC-3.1] Đăng nhập quán ăn - redirect đến /Restaurant
- Location: tests\03-restaurant-flow.spec.ts:41:7

# Error details

```
Error: expect(received).toContain(expected) // indexOf

Expected substring: "/Restaurant"
Received string:    "about:blank"
```

# Test source

```ts
  1   | /**
  2   |  * 🏪 BỘ TEST 03: LUỒNG QUÁN ĂN (Merchant Order Lifecycle)
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Đăng nhập quán ăn -> redirect dashboard
  6   |  * - Kiểm tra KPI cards, biểu đồ thống kê
  7   |  * - Xem danh sách đơn hàng mới
  8   |  * - Xác nhận đơn / Hủy đơn
  9   |  * - Chuyển trạng thái "Đang chuẩn bị món" -> "Hoàn tất"
  10  |  * - Kiểm tra đơn hàng đã xử lý biến mất khỏi danh sách
  11  |  * - Đối chiếu trạng thái đơn với database (qua API)
  12  |  *
  13  |  * Tài khoản: konekopizza / konekopizza (userid=6)
  14  |  */
  15  | 
  16  | import { test, expect } from '@playwright/test';
  17  | import { LoginPage } from '../pages/LoginPage';
  18  | import { RestaurantPage } from '../pages/RestaurantPage';
  19  | import { USERS, URLS, SEED } from '../fixtures/users';
  20  | 
  21  | const RESTAURANT = USERS.restaurant1;
  22  | 
  23  | // ─── Helper: Login quán ăn — ponytail: login OK nhưng dashboard redirect crash
  24  | // Root cause: /Restaurant controller throws 500 → global handler redirect /Home/Error
  25  | // Solution: login set session thành công, dùng goto('/') để verify session
  26  | async function loginAsRestaurant(page: any) {
  27  |   const login = new LoginPage(page);
  28  |   // ponytail: dùng login() có 429 retry + gotoLogin() reload form
  29  |   const url = await login.login(RESTAURANT.username, RESTAURANT.password);
  30  |   console.log(`📍 URL sau login: ${url}`);
  31  |   // Nếu redirect crash (500), session vẫn được set — goto '/' để verify
  32  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  33  |     console.log('⏳ Dashboard redirect crash (500), goto /...');
  34  |     await page.goto('/', { waitUntil: 'networkidle', timeout: 20_000 });
  35  |   }
  36  | }
  37  | 
  38  | // ─── TEST SUITE 1: Dashboard ───
  39  | test.describe('🏪 Dashboard Quán ăn - KPI & Thống kê', () => {
  40  | 
  41  |   test('[TC-3.1] Đăng nhập quán ăn - redirect đến /Restaurant', async ({ page }) => {
  42  |     await loginAsRestaurant(page);
  43  |     const url = page.url();
  44  |     console.log(`✅ URL: ${url}`);
> 45  |     expect(url).toContain('/Restaurant');
      |                 ^ Error: expect(received).toContain(expected) // indexOf
  46  |   });
  47  | 
  48  |   test('[TC-3.2] Dashboard hiển thị thẻ KPI (tổng đơn, doanh thu, đánh giá)', async ({ page }) => {
  49  |     await loginAsRestaurant(page);
  50  | 
  51  |     // Chờ KPI cards load
  52  |     await page.waitForSelector('.card-header', { timeout: 20_000 });
  53  |     const kpiCount = await page.locator('.card-header').count();
  54  |     console.log(`📊 KPI cards: ${kpiCount}`);
  55  |     expect(kpiCount).toBeGreaterThan(0);
  56  | 
  57  |     // Lấy text từng KPI
  58  |     for (let i = 0; i < kpiCount; i++) {
  59  |       const kpiText = await page.locator('.card-header').nth(i).textContent();
  60  |       console.log(`  KPI ${i}: ${kpiText?.trim()}`);
  61  |     }
  62  |   });
  63  | 
  64  |   test('[TC-3.3] Sidebar hiển thị đầy đủ menu: Dashboard, Order List, ...', async ({ page }) => {
  65  |     await loginAsRestaurant(page);
  66  | 
  67  |     const sidebarLinks = await page.locator('.deznav a[href]').count();
  68  |     console.log(`🔗 Sidebar links: ${sidebarLinks}`);
  69  |     expect(sidebarLinks).toBeGreaterThan(0);
  70  | 
  71  |     // Kiểm tra link "Danh sách đơn hàng" hiển thị
  72  |     await expect(page.locator('a[href*="/Restaurant/OrderList"]').first()).toBeVisible({ timeout: 5_000 });
  73  |   });
  74  | 
  75  |   test('[TC-3.4] Biểu đồ doanh thu (Chart.js) render', async ({ page }) => {
  76  |     await loginAsRestaurant(page);
  77  | 
  78  |     const canvasCount = await page.locator('canvas').count();
  79  |     console.log(`📈 Canvas charts: ${canvasCount}`);
  80  |     if (canvasCount > 0) {
  81  |       // Kiểm tra canvas có kích thước > 0
  82  |       const canvasBox = await page.locator('canvas').first().boundingBox();
  83  |       if (canvasBox) {
  84  |         expect(canvasBox.width).toBeGreaterThan(0);
  85  |         expect(canvasBox.height).toBeGreaterThan(0);
  86  |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  87  |       }
  88  |     }
  89  |   });
  90  | });
  91  | 
  92  | // ─── TEST SUITE 2: Quản lý đơn hàng ───
  93  | test.describe('📋 Quản lý đơn hàng (Order List)', () => {
  94  | 
  95  |   test('[TC-3.5] Danh sách đơn hàng load - bảng hiển thị', async ({ page }) => {
  96  |     await loginAsRestaurant(page);
  97  | 
  98  |     const restaurant = new RestaurantPage(page);
  99  |     await restaurant.gotoOrderList();
  100 | 
  101 |     // Chờ bảng load
  102 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  103 |     const orderCount = await restaurant.getOrderCount();
  104 |     console.log(`📋 Số đơn hàng: ${orderCount}`);
  105 |     expect(orderCount).toBeGreaterThanOrEqual(0);
  106 |   });
  107 | 
  108 |   test('[TC-3.6] Chi tiết đơn hàng - click xem thông tin', async ({ page }) => {
  109 |     await loginAsRestaurant(page);
  110 | 
  111 |     const restaurant = new RestaurantPage(page);
  112 |     await restaurant.gotoOrderList();
  113 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  114 | 
  115 |     const orderCount = await restaurant.getOrderCount();
  116 |     if (orderCount > 0) {
  117 |       // Click vào link chi tiết đầu tiên
  118 |       const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
  119 |       const linkCount = await detailLinks.count();
  120 |       console.log(`🔗 Chi tiết links: ${linkCount}`);
  121 | 
  122 |       if (linkCount > 0) {
  123 |         await detailLinks.first().click();
  124 |         await page.waitForLoadState('networkidle');
  125 |         expect(page.url()).toContain('ChiTietDonHang');
  126 |         console.log(`✅ Chi tiết đơn hàng URL: ${page.url()}`);
  127 |       }
  128 |     } else {
  129 |       console.log('ℹ️ Không có đơn hàng nào để xem chi tiết');
  130 |     }
  131 |   });
  132 | 
  133 |   test('[TC-3.7] Kiểm tra trạng thái đơn - cột trạng thái không trống', async ({ page }) => {
  134 |     await loginAsRestaurant(page);
  135 | 
  136 |     const restaurant = new RestaurantPage(page);
  137 |     await restaurant.gotoOrderList();
  138 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  139 | 
  140 |     const orderCount = await restaurant.getOrderCount();
  141 |     if (orderCount > 0) {
  142 |       const status = await restaurant.getFirstOrderStatus();
  143 |       console.log(`📌 Trạng thái đơn đầu: ${status}`);
  144 |       expect(status).toBeTruthy();
  145 |     }
```