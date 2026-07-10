# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 📋 Quản lý đơn hàng (Order List) >> [TC-3.8] Nút "Nhận đơn" hiển thị cho đơn trạng thái "Đã đặt"
- Location: tests\03-restaurant-flow.spec.ts:148:7

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
  146 |   });
  147 | 
  148 |   test('[TC-3.8] Nút "Nhận đơn" hiển thị cho đơn trạng thái "Đã đặt"', async ({ page }) => {
  149 |     await loginAsRestaurant(page);
  150 | 
  151 |     const restaurant = new RestaurantPage(page);
  152 |     await restaurant.gotoOrderList();
> 153 |     await page.waitForSelector('#example5', { timeout: 20_000 });
      |                ^ TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
  154 | 
  155 |     // Kiểm tra nút nhận đơn
  156 |     const acceptBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  157 |     console.log(`🟢 Nhận đơn buttons: ${acceptBtns}`);
  158 |   });
  159 | });
  160 | 
  161 | // ─── TEST SUITE 3: Xử lý đơn hàng (Accept -> Prepare -> Complete) ───
  162 | test.describe('🔄 Xử lý đơn hàng - Accept & Status Transitions', () => {
  163 | 
  164 |   test('[TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn', async ({ page, context }) => {
  165 |     // Mở tab mới cho customer để tạo đơn
  166 |     const customerPage = await context.newPage();
  167 |     const loginC = new LoginPage(customerPage);
  168 |     await loginC.gotoLogin();
  169 |     await loginC.usernameInput.fill(USERS.customer1.username);
  170 |     await loginC.passwordInput.fill(USERS.customer1.password);
  171 |     await loginC.loginButton.click();
  172 |     await customerPage.waitForLoadState('networkidle');
  173 | 
  174 |     // Thêm món vào giỏ ở Koneko Pizza
  175 |     await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'networkidle' });
  176 |     await customerPage.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  177 | 
  178 |     // Thêm món đầu tiên
  179 |     const addBtn = customerPage.locator('.add-to-cart-btn').first();
  180 |     const qtyInput = customerPage.locator('.adding-food-cart input[name="soLuong"]').first();
  181 |     await qtyInput.fill('1');
  182 |     await addBtn.click();
  183 |     await customerPage.waitForResponse(resp => resp.url().includes('ApiThemMonAn') && resp.status() === 200);
  184 |     await customerPage.waitForLoadState('networkidle');
  185 |     console.log('✅ Customer: thêm món vào giỏ');
  186 | 
  187 |     // Vào checkout
  188 |     await customerPage.goto('/Cart/Checkout', { waitUntil: 'networkidle' });
  189 | 
  190 |     // Điền thông tin + đặt hàng
  191 |     const nameInput = customerPage.locator('#input-hoten');
  192 |     const phoneInput = customerPage.locator('#input-sdt');
  193 |     const addressInput = customerPage.locator('#input-diachi');
  194 |     if (await nameInput.isVisible()) {
  195 |       await nameInput.fill(USERS.customer1.name);
  196 |       await phoneInput.fill('0987654321');
  197 |       await addressInput.fill('02 Thanh Sơn, Thanh Bình, Hải Châu');
  198 |       await customerPage.waitForTimeout(500);
  199 |     }
  200 | 
  201 |     // Submit order
  202 |     const submitBtn = customerPage.locator('#btn-submit-cod');
  203 |     if (await submitBtn.isVisible()) {
  204 |       try {
  205 |         const confirmCb = customerPage.locator('#diff-acc');
  206 |         if (await confirmCb.isVisible()) await confirmCb.check();
  207 |       } catch {}
  208 |       await submitBtn.click();
  209 |       await customerPage.waitForTimeout(3000);
  210 |       await customerPage.waitForLoadState('networkidle');
  211 |       console.log(`✅ Customer: submitted order, URL: ${customerPage.url()}`);
  212 |     }
  213 |     await customerPage.close();
  214 | 
  215 |     // Quay lại tab quán ăn -> kiểm tra danh sách đơn
  216 |     const restaurant = new RestaurantPage(page);
  217 |     await loginAsRestaurant(page);
  218 |     await restaurant.gotoOrderList();
  219 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  220 | 
  221 |     const orderCount = await restaurant.getOrderCount();
  222 |     console.log(`📋 Số đơn sau khi tạo: ${orderCount}`);
  223 |   });
  224 | 
  225 |   test('[TC-3.10] Nhận đơn -> chuyển trạng thái "Đã xác nhận"', async ({ page }) => {
  226 |     await loginAsRestaurant(page);
  227 | 
  228 |     const restaurant = new RestaurantPage(page);
  229 |     await restaurant.gotoOrderList();
  230 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  231 | 
  232 |     // Kiểm tra có đơn và nút nhận đơn
  233 |     const acceptBtns = page.locator('a[href*="/Restaurant/nhandon/"]');
  234 |     const btnCount = await acceptBtns.count();
  235 | 
  236 |     if (btnCount > 0) {
  237 |       // Get order info before accepting
  238 |       const firstRow = page.locator('#example5 tbody tr').first();
  239 |       const orderIdCell = firstRow.locator('td').first();
  240 |       const orderId = await orderIdCell.textContent();
  241 |       console.log(`📋 Nhận đơn #${orderId?.trim()}`);
  242 | 
  243 |       // Click nhận đơn
  244 |       await acceptBtns.first().click();
  245 |       await page.waitForLoadState('networkidle');
  246 |       await page.waitForTimeout(2000);
  247 |       console.log(`✅ Đã nhận đơn #${orderId?.trim()}`);
  248 | 
  249 |       // Kiểm tra nút nhận đơn không còn hiển thị (đã chuyển trạng thái)
  250 |       const remainingBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  251 |       console.log(`🔄 Nhận đơn buttons còn: ${remainingBtns}`);
  252 |     } else {
  253 |       console.log('ℹ️ Không có đơn nào để nhận');
```