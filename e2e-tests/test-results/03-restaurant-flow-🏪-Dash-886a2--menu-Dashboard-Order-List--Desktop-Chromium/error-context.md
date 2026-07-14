# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🏪 Dashboard Quán ăn - KPI & Thống kê >> [TC-3.3] Sidebar hiển thị đầy đủ menu: Dashboard, Order List, ...
- Location: tests\03-restaurant-flow.spec.ts:90:7

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator:  locator('a[href*="/Restaurant/OrderList"]').first()
Expected: visible
Received: hidden
Timeout:  5000ms

Call log:
  - Expect "toBeVisible" with timeout 5000ms
  - waiting for locator('a[href*="/Restaurant/OrderList"]').first()
    13 × locator resolved to <a href="/Restaurant/OrderList" class="dropdown-item py-2 px-3">…</a>
       - unexpected value "hidden"

```

```yaml
- link:
  - /url: index.html
- navigation:
  - img
  - list:
    - listitem:
      - link:
        - /url: "#"
        - img
    - listitem:
      - button:
        - img
    - listitem:
      - link "Mở":
        - /url: /Restaurant/updateStatus
    - listitem:
      - link "":
        - /url: /Home/Logout
    - listitem:
      - button "Xin chào, konekopizza":
        - text: Xin chào,
        - strong: konekopizza
- list:
  - listitem:
    - link " Dashboard":
      - /url: javascript:void()
    - list:
      - listitem:
        - link "Dashboard":
          - /url: /Restaurant
      - listitem:
        - link "Phân tích":
          - /url: /Restaurant/Analytics
      - listitem:
        - link "Đánh giá":
          - /url: /Restaurant/Review
      - listitem:
        - link "Danh sách đơn hàng":
          - /url: /Restaurant/OrderList
  - listitem:
    - link " Apps":
      - /url: javascript:void()
- paragraph: Sắp xếp các menu của bạn thông qua nút bên dưới
- link "+ Thêm thực đơn":
  - /url: /Restaurant/ProductDetail
- heading "Thống kê" [level=2]
- paragraph: Xin chào quản lí Koneko Pizza
- text: 🤖
- heading "Chiến lược bán chéo từ dữ liệu" [level=4]
- paragraph: Phân tích Apriori trên 30 đơn hàng hoàn thành
- text: AI
- paragraph:
  - text:  Những cặp món sau thường được khách đặt cùng nhau. Hãy tạo
  - strong: Combo khuyến mãi
  - text: cho các cặp này để tăng doanh thu!
- text: Trà tắc + Pizza thập cẩm
- strong: 100%
- text: khách mua Trà tắc cũng mua Pizza thập cẩm Support
- strong: 3.3%
- text: 1 đơn
- img
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
  23  | // ponytail: retry #example5 với page reload nếu DataTables chưa kịp render
  24  | async function waitForOrderTable(page: any) {
  25  |   for (let attempt = 0; attempt < 2; attempt++) {
  26  |     try {
  27  |       await page.waitForSelector('#example5', { timeout: 25_000 });
  28  |       return;
  29  |     } catch {
  30  |       console.log(`⏳ #example5 timeout lần ${attempt+1}, reload...`);
  31  |       await page.reload({ waitUntil: 'domcontentloaded', timeout: 30_000 }).catch(() => {});
  32  |       await page.waitForTimeout(3000);
  33  |     }
  34  |   }
  35  |   await page.waitForSelector('table', { timeout: 15_000 }).catch(() => {});
  36  | }
  37  | 
  38  | // ─── Helper: Login quán ăn — ponytail: login OK nhưng dashboard redirect crash
  39  | // Root cause: /Restaurant controller throws 500 → global handler redirect /Home/Error
  40  | // Solution: login set session thành công, dùng goto('/') để verify session
  41  | async function loginAsRestaurant(page: any) {
  42  |   const login = new LoginPage(page);
  43  |   // ponytail: dùng login() có 429 retry + gotoLogin() reload form
  44  |   const url = await login.login(RESTAURANT.username, RESTAURANT.password);
  45  |   console.log(`📍 URL sau login: ${url}`);
  46  |   // ponytail: redirect về /Home/Login → cold start làm mất session cookie
  47  |   // Solution: goto trực tiếp /Restaurant, retry nhanh với domcontentloaded
  48  |   // ponytail: cold start → goto /Restaurant với timeout vừa đủ, 2 retries
  49  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  50  |     await page.waitForTimeout(2000); // chờ session cookie settle
  51  |     for (let retry = 0; retry < 2; retry++) {
  52  |       try {
  53  |         await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 20_000 });
  54  |         if (page.url().includes('/Restaurant')) break;
  55  |       } catch {
  56  |         console.log(`⚠️ Fallback goto Restaurant #${retry+1} failed`);
  57  |         await page.waitForTimeout(1000);
  58  |       }
  59  |     }
  60  |   }
  61  |   await page.waitForSelector('.deznav', { timeout: 10_000 }).catch(() => {});
  62  | }
  63  | 
  64  | // ─── TEST SUITE 1: Dashboard ───
  65  | test.describe('🏪 Dashboard Quán ăn - KPI & Thống kê', () => {
  66  | 
  67  |   test('[TC-3.1] Đăng nhập quán ăn - redirect đến /Restaurant', async ({ page }) => {
  68  |     await loginAsRestaurant(page);
  69  |     const url = page.url();
  70  |     console.log(`✅ URL: ${url}`);
  71  |     expect(url).toContain('/Restaurant');
  72  |   });
  73  | 
  74  |   test('[TC-3.2] Dashboard hiển thị thẻ KPI (tổng đơn, doanh thu, đánh giá)', async ({ page }) => {
  75  |     await loginAsRestaurant(page);
  76  | 
  77  |     // Chờ KPI cards load
  78  |     await page.waitForSelector('.card-header', { timeout: 20_000 });
  79  |     const kpiCount = await page.locator('.card-header').count();
  80  |     console.log(`📊 KPI cards: ${kpiCount}`);
  81  |     expect(kpiCount).toBeGreaterThan(0);
  82  | 
  83  |     // Lấy text từng KPI
  84  |     for (let i = 0; i < kpiCount; i++) {
  85  |       const kpiText = await page.locator('.card-header').nth(i).textContent();
  86  |       console.log(`  KPI ${i}: ${kpiText?.trim()}`);
  87  |     }
  88  |   });
  89  | 
  90  |   test('[TC-3.3] Sidebar hiển thị đầy đủ menu: Dashboard, Order List, ...', async ({ page }) => {
  91  |     await loginAsRestaurant(page);
  92  | 
  93  |     const sidebarLinks = await page.locator('.deznav a[href]').count();
  94  |     console.log(`🔗 Sidebar links: ${sidebarLinks}`);
  95  |     expect(sidebarLinks).toBeGreaterThan(0);
  96  | 
  97  |     // Kiểm tra link "Danh sách đơn hàng" hiển thị
> 98  |     await expect(page.locator('a[href*="/Restaurant/OrderList"]').first()).toBeVisible({ timeout: 5_000 });
      |                                                                            ^ Error: expect(locator).toBeVisible() failed
  99  |   });
  100 | 
  101 |   test('[TC-3.4] Biểu đồ doanh thu (Chart.js) render', async ({ page }) => {
  102 |     await loginAsRestaurant(page);
  103 | 
  104 |     const canvasCount = await page.locator('canvas').count();
  105 |     console.log(`📈 Canvas charts: ${canvasCount}`);
  106 |     if (canvasCount > 0) {
  107 |       // Kiểm tra canvas có kích thước > 0
  108 |       const canvasBox = await page.locator('canvas').first().boundingBox();
  109 |       if (canvasBox) {
  110 |         expect(canvasBox.width).toBeGreaterThan(0);
  111 |         expect(canvasBox.height).toBeGreaterThan(0);
  112 |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  113 |       }
  114 |     }
  115 |   });
  116 | });
  117 | 
  118 | // ─── TEST SUITE 2: Quản lý đơn hàng ───
  119 | test.describe('📋 Quản lý đơn hàng (Order List)', () => {
  120 | 
  121 |   test('[TC-3.5] Danh sách đơn hàng load - bảng hiển thị', async ({ page }) => {
  122 |     await loginAsRestaurant(page);
  123 | 
  124 |     const restaurant = new RestaurantPage(page);
  125 |     await restaurant.gotoOrderList();
  126 |     await waitForOrderTable(page);
  127 | 
  128 |     const orderCount = await restaurant.getOrderCount();
  129 |     console.log(`📋 Số đơn hàng: ${orderCount}`);
  130 |     expect(orderCount).toBeGreaterThanOrEqual(0);
  131 |   });
  132 | 
  133 |   test('[TC-3.6] Chi tiết đơn hàng - click xem thông tin', async ({ page }) => {
  134 |     await loginAsRestaurant(page);
  135 | 
  136 |     const restaurant = new RestaurantPage(page);
  137 |     await restaurant.gotoOrderList();
  138 |     await waitForOrderTable(page);
  139 | 
  140 |     const orderCount = await restaurant.getOrderCount();
  141 |     if (orderCount > 0) {
  142 |       const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
  143 |       const linkCount = await detailLinks.count();
  144 |       console.log(`🔗 Chi tiết links: ${linkCount}`);
  145 | 
  146 |       if (linkCount > 0) {
  147 |         await detailLinks.first().click();
  148 |         await page.waitForLoadState('networkidle');
  149 |         expect(page.url()).toContain('ChiTietDonHang');
  150 |         console.log(`✅ Chi tiết đơn hàng URL: ${page.url()}`);
  151 |       }
  152 |     } else {
  153 |       console.log('ℹ️ Không có đơn hàng nào để xem chi tiết');
  154 |     }
  155 |   });
  156 | 
  157 |   test('[TC-3.7] Kiểm tra trạng thái đơn - cột trạng thái không trống', async ({ page }) => {
  158 |     await loginAsRestaurant(page);
  159 | 
  160 |     const restaurant = new RestaurantPage(page);
  161 |     await restaurant.gotoOrderList();
  162 |     await waitForOrderTable(page);
  163 | 
  164 |     const orderCount = await restaurant.getOrderCount();
  165 |     if (orderCount > 0) {
  166 |       const status = await restaurant.getFirstOrderStatus();
  167 |       console.log(`📌 Trạng thái đơn đầu: ${status}`);
  168 |       expect(status).toBeTruthy();
  169 |     }
  170 |   });
  171 | 
  172 |   test('[TC-3.8] Nút "Nhận đơn" hiển thị cho đơn trạng thái "Đã đặt"', async ({ page }) => {
  173 |     await loginAsRestaurant(page);
  174 | 
  175 |     const restaurant = new RestaurantPage(page);
  176 |     await restaurant.gotoOrderList();
  177 |     await waitForOrderTable(page);
  178 | 
  179 |     const acceptBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  180 |     console.log(`🟢 Nhận đơn buttons: ${acceptBtns}`);
  181 |   });
  182 | });
  183 | 
  184 | // ─── TEST SUITE 3: Xử lý đơn hàng (Accept -> Prepare -> Complete) ───
  185 | test.describe('🔄 Xử lý đơn hàng - Accept & Status Transitions', () => {
  186 | 
  187 |   test('[TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn', async ({ page, context }) => {
  188 |     // Mở tab mới cho customer để tạo đơn
  189 |     const customerPage = await context.newPage();
  190 |     const loginC = new LoginPage(customerPage);
  191 |     await loginC.gotoLogin();
  192 |     await loginC.usernameInput.fill(USERS.customer1.username);
  193 |     await loginC.passwordInput.fill(USERS.customer1.password);
  194 |     await loginC.loginButton.click();
  195 |     await customerPage.waitForLoadState('networkidle');
  196 | 
  197 |     // Thêm món vào giỏ ở Koneko Pizza
  198 |     await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'networkidle' });
```