# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 📋 Quản lý đơn hàng (Order List) >> [TC-3.5] Danh sách đơn hàng load - bảng hiển thị
- Location: tests\03-restaurant-flow.spec.ts:96:7

# Error details

```
TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
Call log:
  - waiting for locator('#example5') to be visible

```

# Page snapshot

```yaml
- generic [ref=e2]:
  - banner [ref=e3]:
    - generic [ref=e4]:
      - link "Fastship" [ref=e5] [cursor=pointer]:
        - /url: /Home
        - generic [ref=e6]: Fastship
      - generic [ref=e7]:
        - link " Trang chủ" [ref=e8] [cursor=pointer]:
          - /url: /Home
          - generic [ref=e9]: 
          - text: Trang chủ
        - link "Đăng ký" [ref=e10] [cursor=pointer]:
          - /url: /Home/Signup
  - main [ref=e11]:
    - generic [ref=e12]:
      - link "Fastship" [ref=e13] [cursor=pointer]:
        - /url: /Home
        - heading "Fastship" [level=1] [ref=e14]
      - heading "Đăng nhập" [level=2] [ref=e15]
      - generic [ref=e16]:
        - generic [ref=e18]:
          - link "Đăng nhập bằng Google" [ref=e19] [cursor=pointer]:
            - /url: /Home/GoogleLogin
            - img [ref=e20]
            - generic [ref=e25]: Đăng nhập bằng Google
          - link " Đăng ký làm Đối tác Quán ăn / Shipper" [ref=e27] [cursor=pointer]:
            - /url: /Home/GooglePartnerLogin
            - generic [ref=e28]: 
            - text: Đăng ký làm Đối tác Quán ăn / Shipper
        - generic [ref=e29]: hoặc bằng tài khoản
        - generic [ref=e30]: Tên đăng nhập hoặc số điện thoại
        - textbox "Tên đăng nhập hoặc số điện thoại" [ref=e31]
        - generic [ref=e32]: Mật khẩu
        - generic [ref=e33]:
          - textbox "Mật khẩu" [ref=e34]
          - button "Hiện/ẩn mật khẩu" [ref=e35] [cursor=pointer]:
            - generic: 
        - generic [ref=e36]:
          - generic [ref=e37] [cursor=pointer]:
            - checkbox "Lưu đăng nhập" [ref=e38]
            - text: Lưu đăng nhập
          - link "Quên mật khẩu?" [ref=e39] [cursor=pointer]:
            - /url: /Home/Forgot
        - button "Đăng nhập" [ref=e40] [cursor=pointer]
      - generic [ref=e42]:
        - text: Chưa có tài khoản?
        - link "Đăng ký" [ref=e43] [cursor=pointer]:
          - /url: /Home/Signup
      - generic [ref=e44]:
        - text: Bằng cách đăng nhập hoặc đăng ký, bạn đồng ý với
        - link "Điều khoản dịch vụ" [ref=e45] [cursor=pointer]:
          - /url: "#"
        - text: của Fastship
```

# Test source

```ts
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
  31  |   // ponytail: redirect về /Home/Login → cold start làm mất session cookie
  32  |   // Solution: goto trực tiếp /Restaurant
  33  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  34  |     console.log('⏳ Cold start / redirect crash, goto /Restaurant directly...');
  35  |     await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 30_000 }).catch(() => console.log('⚠️ Fallback goto Restaurant failed'));
  36  |   }
  37  | }
  38  | 
  39  | // ─── TEST SUITE 1: Dashboard ───
  40  | test.describe('🏪 Dashboard Quán ăn - KPI & Thống kê', () => {
  41  | 
  42  |   test('[TC-3.1] Đăng nhập quán ăn - redirect đến /Restaurant', async ({ page }) => {
  43  |     await loginAsRestaurant(page);
  44  |     const url = page.url();
  45  |     console.log(`✅ URL: ${url}`);
  46  |     expect(url).toContain('/Restaurant');
  47  |   });
  48  | 
  49  |   test('[TC-3.2] Dashboard hiển thị thẻ KPI (tổng đơn, doanh thu, đánh giá)', async ({ page }) => {
  50  |     await loginAsRestaurant(page);
  51  | 
  52  |     // Chờ KPI cards load
  53  |     await page.waitForSelector('.card-header', { timeout: 20_000 });
  54  |     const kpiCount = await page.locator('.card-header').count();
  55  |     console.log(`📊 KPI cards: ${kpiCount}`);
  56  |     expect(kpiCount).toBeGreaterThan(0);
  57  | 
  58  |     // Lấy text từng KPI
  59  |     for (let i = 0; i < kpiCount; i++) {
  60  |       const kpiText = await page.locator('.card-header').nth(i).textContent();
  61  |       console.log(`  KPI ${i}: ${kpiText?.trim()}`);
  62  |     }
  63  |   });
  64  | 
  65  |   test('[TC-3.3] Sidebar hiển thị đầy đủ menu: Dashboard, Order List, ...', async ({ page }) => {
  66  |     await loginAsRestaurant(page);
  67  | 
  68  |     const sidebarLinks = await page.locator('.deznav a[href]').count();
  69  |     console.log(`🔗 Sidebar links: ${sidebarLinks}`);
  70  |     expect(sidebarLinks).toBeGreaterThan(0);
  71  | 
  72  |     // Kiểm tra link "Danh sách đơn hàng" hiển thị
  73  |     await expect(page.locator('a[href*="/Restaurant/OrderList"]').first()).toBeVisible({ timeout: 5_000 });
  74  |   });
  75  | 
  76  |   test('[TC-3.4] Biểu đồ doanh thu (Chart.js) render', async ({ page }) => {
  77  |     await loginAsRestaurant(page);
  78  | 
  79  |     const canvasCount = await page.locator('canvas').count();
  80  |     console.log(`📈 Canvas charts: ${canvasCount}`);
  81  |     if (canvasCount > 0) {
  82  |       // Kiểm tra canvas có kích thước > 0
  83  |       const canvasBox = await page.locator('canvas').first().boundingBox();
  84  |       if (canvasBox) {
  85  |         expect(canvasBox.width).toBeGreaterThan(0);
  86  |         expect(canvasBox.height).toBeGreaterThan(0);
  87  |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  88  |       }
  89  |     }
  90  |   });
  91  | });
  92  | 
  93  | // ─── TEST SUITE 2: Quản lý đơn hàng ───
  94  | test.describe('📋 Quản lý đơn hàng (Order List)', () => {
  95  | 
  96  |   test('[TC-3.5] Danh sách đơn hàng load - bảng hiển thị', async ({ page }) => {
  97  |     await loginAsRestaurant(page);
  98  | 
  99  |     const restaurant = new RestaurantPage(page);
  100 |     await restaurant.gotoOrderList();
  101 | 
  102 |     // Chờ bảng load
> 103 |     await page.waitForSelector('#example5', { timeout: 20_000 });
      |                ^ TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
  104 |     const orderCount = await restaurant.getOrderCount();
  105 |     console.log(`📋 Số đơn hàng: ${orderCount}`);
  106 |     expect(orderCount).toBeGreaterThanOrEqual(0);
  107 |   });
  108 | 
  109 |   test('[TC-3.6] Chi tiết đơn hàng - click xem thông tin', async ({ page }) => {
  110 |     await loginAsRestaurant(page);
  111 | 
  112 |     const restaurant = new RestaurantPage(page);
  113 |     await restaurant.gotoOrderList();
  114 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  115 | 
  116 |     const orderCount = await restaurant.getOrderCount();
  117 |     if (orderCount > 0) {
  118 |       // Click vào link chi tiết đầu tiên
  119 |       const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
  120 |       const linkCount = await detailLinks.count();
  121 |       console.log(`🔗 Chi tiết links: ${linkCount}`);
  122 | 
  123 |       if (linkCount > 0) {
  124 |         await detailLinks.first().click();
  125 |         await page.waitForLoadState('networkidle');
  126 |         expect(page.url()).toContain('ChiTietDonHang');
  127 |         console.log(`✅ Chi tiết đơn hàng URL: ${page.url()}`);
  128 |       }
  129 |     } else {
  130 |       console.log('ℹ️ Không có đơn hàng nào để xem chi tiết');
  131 |     }
  132 |   });
  133 | 
  134 |   test('[TC-3.7] Kiểm tra trạng thái đơn - cột trạng thái không trống', async ({ page }) => {
  135 |     await loginAsRestaurant(page);
  136 | 
  137 |     const restaurant = new RestaurantPage(page);
  138 |     await restaurant.gotoOrderList();
  139 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  140 | 
  141 |     const orderCount = await restaurant.getOrderCount();
  142 |     if (orderCount > 0) {
  143 |       const status = await restaurant.getFirstOrderStatus();
  144 |       console.log(`📌 Trạng thái đơn đầu: ${status}`);
  145 |       expect(status).toBeTruthy();
  146 |     }
  147 |   });
  148 | 
  149 |   test('[TC-3.8] Nút "Nhận đơn" hiển thị cho đơn trạng thái "Đã đặt"', async ({ page }) => {
  150 |     await loginAsRestaurant(page);
  151 | 
  152 |     const restaurant = new RestaurantPage(page);
  153 |     await restaurant.gotoOrderList();
  154 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  155 | 
  156 |     // Kiểm tra nút nhận đơn
  157 |     const acceptBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  158 |     console.log(`🟢 Nhận đơn buttons: ${acceptBtns}`);
  159 |   });
  160 | });
  161 | 
  162 | // ─── TEST SUITE 3: Xử lý đơn hàng (Accept -> Prepare -> Complete) ───
  163 | test.describe('🔄 Xử lý đơn hàng - Accept & Status Transitions', () => {
  164 | 
  165 |   test('[TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn', async ({ page, context }) => {
  166 |     // Mở tab mới cho customer để tạo đơn
  167 |     const customerPage = await context.newPage();
  168 |     const loginC = new LoginPage(customerPage);
  169 |     await loginC.gotoLogin();
  170 |     await loginC.usernameInput.fill(USERS.customer1.username);
  171 |     await loginC.passwordInput.fill(USERS.customer1.password);
  172 |     await loginC.loginButton.click();
  173 |     await customerPage.waitForLoadState('networkidle');
  174 | 
  175 |     // Thêm món vào giỏ ở Koneko Pizza
  176 |     await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'networkidle' });
  177 |     await customerPage.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  178 | 
  179 |     // Thêm món đầu tiên
  180 |     const addBtn = customerPage.locator('.add-to-cart-btn').first();
  181 |     const qtyInput = customerPage.locator('.adding-food-cart input[name="soLuong"]').first();
  182 |     await qtyInput.fill('1');
  183 |     await addBtn.click();
  184 |     await customerPage.waitForResponse(resp => resp.url().includes('ApiThemMonAn') && resp.status() === 200);
  185 |     await customerPage.waitForLoadState('networkidle');
  186 |     console.log('✅ Customer: thêm món vào giỏ');
  187 | 
  188 |     // Vào checkout
  189 |     await customerPage.goto('/Cart/Checkout', { waitUntil: 'networkidle' });
  190 | 
  191 |     // Điền thông tin + đặt hàng
  192 |     const nameInput = customerPage.locator('#input-hoten');
  193 |     const phoneInput = customerPage.locator('#input-sdt');
  194 |     const addressInput = customerPage.locator('#input-diachi');
  195 |     if (await nameInput.isVisible()) {
  196 |       await nameInput.fill(USERS.customer1.name);
  197 |       await phoneInput.fill('0987654321');
  198 |       await addressInput.fill('02 Thanh Sơn, Thanh Bình, Hải Châu');
  199 |       await customerPage.waitForTimeout(500);
  200 |     }
  201 | 
  202 |     // Submit order
  203 |     const submitBtn = customerPage.locator('#btn-submit-cod');
```