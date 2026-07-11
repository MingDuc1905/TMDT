# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 📋 Quản lý đơn hàng (Order List) >> [TC-3.7] Kiểm tra trạng thái đơn - cột trạng thái không trống
- Location: tests\03-restaurant-flow.spec.ts:143:7

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
  48  | // ─── TEST SUITE 1: Dashboard ───
  49  | test.describe('🏪 Dashboard Quán ăn - KPI & Thống kê', () => {
  50  | 
  51  |   test('[TC-3.1] Đăng nhập quán ăn - redirect đến /Restaurant', async ({ page }) => {
  52  |     await loginAsRestaurant(page);
  53  |     const url = page.url();
  54  |     console.log(`✅ URL: ${url}`);
  55  |     expect(url).toContain('/Restaurant');
  56  |   });
  57  | 
  58  |   test('[TC-3.2] Dashboard hiển thị thẻ KPI (tổng đơn, doanh thu, đánh giá)', async ({ page }) => {
  59  |     await loginAsRestaurant(page);
  60  | 
  61  |     // Chờ KPI cards load
  62  |     await page.waitForSelector('.card-header', { timeout: 20_000 });
  63  |     const kpiCount = await page.locator('.card-header').count();
  64  |     console.log(`📊 KPI cards: ${kpiCount}`);
  65  |     expect(kpiCount).toBeGreaterThan(0);
  66  | 
  67  |     // Lấy text từng KPI
  68  |     for (let i = 0; i < kpiCount; i++) {
  69  |       const kpiText = await page.locator('.card-header').nth(i).textContent();
  70  |       console.log(`  KPI ${i}: ${kpiText?.trim()}`);
  71  |     }
  72  |   });
  73  | 
  74  |   test('[TC-3.3] Sidebar hiển thị đầy đủ menu: Dashboard, Order List, ...', async ({ page }) => {
  75  |     await loginAsRestaurant(page);
  76  | 
  77  |     const sidebarLinks = await page.locator('.deznav a[href]').count();
  78  |     console.log(`🔗 Sidebar links: ${sidebarLinks}`);
  79  |     expect(sidebarLinks).toBeGreaterThan(0);
  80  | 
  81  |     // Kiểm tra link "Danh sách đơn hàng" hiển thị
  82  |     await expect(page.locator('a[href*="/Restaurant/OrderList"]').first()).toBeVisible({ timeout: 5_000 });
  83  |   });
  84  | 
  85  |   test('[TC-3.4] Biểu đồ doanh thu (Chart.js) render', async ({ page }) => {
  86  |     await loginAsRestaurant(page);
  87  | 
  88  |     const canvasCount = await page.locator('canvas').count();
  89  |     console.log(`📈 Canvas charts: ${canvasCount}`);
  90  |     if (canvasCount > 0) {
  91  |       // Kiểm tra canvas có kích thước > 0
  92  |       const canvasBox = await page.locator('canvas').first().boundingBox();
  93  |       if (canvasBox) {
  94  |         expect(canvasBox.width).toBeGreaterThan(0);
  95  |         expect(canvasBox.height).toBeGreaterThan(0);
  96  |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  97  |       }
  98  |     }
  99  |   });
  100 | });
  101 | 
  102 | // ─── TEST SUITE 2: Quản lý đơn hàng ───
  103 | test.describe('📋 Quản lý đơn hàng (Order List)', () => {
  104 | 
  105 |   test('[TC-3.5] Danh sách đơn hàng load - bảng hiển thị', async ({ page }) => {
  106 |     await loginAsRestaurant(page);
  107 | 
  108 |     const restaurant = new RestaurantPage(page);
  109 |     await restaurant.gotoOrderList();
  110 | 
  111 |     // Chờ bảng load
  112 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  113 |     const orderCount = await restaurant.getOrderCount();
  114 |     console.log(`📋 Số đơn hàng: ${orderCount}`);
  115 |     expect(orderCount).toBeGreaterThanOrEqual(0);
  116 |   });
  117 | 
  118 |   test('[TC-3.6] Chi tiết đơn hàng - click xem thông tin', async ({ page }) => {
  119 |     await loginAsRestaurant(page);
  120 | 
  121 |     const restaurant = new RestaurantPage(page);
  122 |     await restaurant.gotoOrderList();
  123 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  124 | 
  125 |     const orderCount = await restaurant.getOrderCount();
  126 |     if (orderCount > 0) {
  127 |       // Click vào link chi tiết đầu tiên
  128 |       const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
  129 |       const linkCount = await detailLinks.count();
  130 |       console.log(`🔗 Chi tiết links: ${linkCount}`);
  131 | 
  132 |       if (linkCount > 0) {
  133 |         await detailLinks.first().click();
  134 |         await page.waitForLoadState('networkidle');
  135 |         expect(page.url()).toContain('ChiTietDonHang');
  136 |         console.log(`✅ Chi tiết đơn hàng URL: ${page.url()}`);
  137 |       }
  138 |     } else {
  139 |       console.log('ℹ️ Không có đơn hàng nào để xem chi tiết');
  140 |     }
  141 |   });
  142 | 
  143 |   test('[TC-3.7] Kiểm tra trạng thái đơn - cột trạng thái không trống', async ({ page }) => {
  144 |     await loginAsRestaurant(page);
  145 | 
  146 |     const restaurant = new RestaurantPage(page);
  147 |     await restaurant.gotoOrderList();
> 148 |     await page.waitForSelector('#example5', { timeout: 20_000 });
      |                ^ TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
  149 | 
  150 |     const orderCount = await restaurant.getOrderCount();
  151 |     if (orderCount > 0) {
  152 |       const status = await restaurant.getFirstOrderStatus();
  153 |       console.log(`📌 Trạng thái đơn đầu: ${status}`);
  154 |       expect(status).toBeTruthy();
  155 |     }
  156 |   });
  157 | 
  158 |   test('[TC-3.8] Nút "Nhận đơn" hiển thị cho đơn trạng thái "Đã đặt"', async ({ page }) => {
  159 |     await loginAsRestaurant(page);
  160 | 
  161 |     const restaurant = new RestaurantPage(page);
  162 |     await restaurant.gotoOrderList();
  163 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  164 | 
  165 |     // Kiểm tra nút nhận đơn
  166 |     const acceptBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  167 |     console.log(`🟢 Nhận đơn buttons: ${acceptBtns}`);
  168 |   });
  169 | });
  170 | 
  171 | // ─── TEST SUITE 3: Xử lý đơn hàng (Accept -> Prepare -> Complete) ───
  172 | test.describe('🔄 Xử lý đơn hàng - Accept & Status Transitions', () => {
  173 | 
  174 |   test('[TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn', async ({ page, context }) => {
  175 |     // Mở tab mới cho customer để tạo đơn
  176 |     const customerPage = await context.newPage();
  177 |     const loginC = new LoginPage(customerPage);
  178 |     await loginC.gotoLogin();
  179 |     await loginC.usernameInput.fill(USERS.customer1.username);
  180 |     await loginC.passwordInput.fill(USERS.customer1.password);
  181 |     await loginC.loginButton.click();
  182 |     await customerPage.waitForLoadState('networkidle');
  183 | 
  184 |     // Thêm món vào giỏ ở Koneko Pizza
  185 |     await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'networkidle' });
  186 |     await customerPage.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  187 | 
  188 |     // Thêm món đầu tiên
  189 |     const addBtn = customerPage.locator('.add-to-cart-btn').first();
  190 |     const qtyInput = customerPage.locator('.adding-food-cart input[name="soLuong"]').first();
  191 |     await qtyInput.fill('1');
  192 |     await addBtn.click();
  193 |     await customerPage.waitForResponse(resp => resp.url().includes('ApiThemMonAn') && resp.status() === 200);
  194 |     await customerPage.waitForLoadState('networkidle');
  195 |     console.log('✅ Customer: thêm món vào giỏ');
  196 | 
  197 |     // Vào checkout
  198 |     await customerPage.goto('/Cart/Checkout', { waitUntil: 'networkidle' });
  199 | 
  200 |     // Điền thông tin + đặt hàng
  201 |     const nameInput = customerPage.locator('#input-hoten');
  202 |     const phoneInput = customerPage.locator('#input-sdt');
  203 |     const addressInput = customerPage.locator('#input-diachi');
  204 |     if (await nameInput.isVisible()) {
  205 |       await nameInput.fill(USERS.customer1.name);
  206 |       await phoneInput.fill('0987654321');
  207 |       await addressInput.fill('02 Thanh Sơn, Thanh Bình, Hải Châu');
  208 |       await customerPage.waitForTimeout(500);
  209 |     }
  210 | 
  211 |     // Submit order
  212 |     const submitBtn = customerPage.locator('#btn-submit-cod');
  213 |     if (await submitBtn.isVisible()) {
  214 |       try {
  215 |         const confirmCb = customerPage.locator('#diff-acc');
  216 |         if (await confirmCb.isVisible()) await confirmCb.check();
  217 |       } catch {}
  218 |       await submitBtn.click();
  219 |       await customerPage.waitForTimeout(3000);
  220 |       await customerPage.waitForLoadState('networkidle');
  221 |       console.log(`✅ Customer: submitted order, URL: ${customerPage.url()}`);
  222 |     }
  223 |     await customerPage.close();
  224 | 
  225 |     // Quay lại tab quán ăn -> kiểm tra danh sách đơn
  226 |     const restaurant = new RestaurantPage(page);
  227 |     await loginAsRestaurant(page);
  228 |     await restaurant.gotoOrderList();
  229 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  230 | 
  231 |     const orderCount = await restaurant.getOrderCount();
  232 |     console.log(`📋 Số đơn sau khi tạo: ${orderCount}`);
  233 |   });
  234 | 
  235 |   test('[TC-3.10] Nhận đơn -> chuyển trạng thái "Đã xác nhận"', async ({ page }) => {
  236 |     await loginAsRestaurant(page);
  237 | 
  238 |     const restaurant = new RestaurantPage(page);
  239 |     await restaurant.gotoOrderList();
  240 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  241 | 
  242 |     // Kiểm tra có đơn và nút nhận đơn
  243 |     const acceptBtns = page.locator('a[href*="/Restaurant/nhandon/"]');
  244 |     const btnCount = await acceptBtns.count();
  245 | 
  246 |     if (btnCount > 0) {
  247 |       // Get order info before accepting
  248 |       const firstRow = page.locator('#example5 tbody tr').first();
```