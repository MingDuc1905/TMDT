# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🔄 Xử lý đơn hàng - Accept & Status Transitions >> [TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn
- Location: tests\03-restaurant-flow.spec.ts:164:7

# Error details

```
Error: page.goto: net::ERR_ABORTED at https://fastship-web.onrender.com/Home/DetailRestaurant?id=6
Call log:
  - navigating to "https://fastship-web.onrender.com/Home/DetailRestaurant?id=6", waiting until "networkidle"

```

# Test source

```ts
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
  153 |     await page.waitForSelector('#example5', { timeout: 20_000 });
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
> 175 |     await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'networkidle' });
      |                        ^ Error: page.goto: net::ERR_ABORTED at https://fastship-web.onrender.com/Home/DetailRestaurant?id=6
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
  254 |     }
  255 |   });
  256 | 
  257 |   test('[TC-3.11] Hủy đơn - nút hủy hoạt động', async ({ page }) => {
  258 |     await loginAsRestaurant(page);
  259 | 
  260 |     const restaurant = new RestaurantPage(page);
  261 |     await restaurant.gotoOrderList();
  262 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  263 | 
  264 |     // Kiểm tra nút hủy
  265 |     const cancelBtns = page.locator('a[href*="/Restaurant/huydon/"]');
  266 |     const btnCount = await cancelBtns.count();
  267 |     console.log(`🔴 Hủy đơn buttons: ${btnCount}`);
  268 | 
  269 |     if (btnCount > 0) {
  270 |       await cancelBtns.first().click();
  271 |       await page.waitForLoadState('networkidle');
  272 |       await page.waitForTimeout(2000);
  273 |       console.log('✅ Đã hủy đơn');
  274 |     }
  275 |   });
```