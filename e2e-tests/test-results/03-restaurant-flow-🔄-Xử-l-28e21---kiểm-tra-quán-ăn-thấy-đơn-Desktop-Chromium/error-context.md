# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🔄 Xử lý đơn hàng - Accept & Status Transitions >> [TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn
- Location: tests\03-restaurant-flow.spec.ts:174:7

# Error details

```
TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
```

# Test source

```ts
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
  148 |     await page.waitForSelector('#example5', { timeout: 20_000 });
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
> 193 |     await customerPage.waitForResponse(resp => resp.url().includes('ApiThemMonAn') && resp.status() === 200);
      |                        ^ TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
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
  249 |       const orderIdCell = firstRow.locator('td').first();
  250 |       const orderId = await orderIdCell.textContent();
  251 |       console.log(`📋 Nhận đơn #${orderId?.trim()}`);
  252 | 
  253 |       // Click nhận đơn
  254 |       await acceptBtns.first().click();
  255 |       await page.waitForLoadState('networkidle');
  256 |       await page.waitForTimeout(2000);
  257 |       console.log(`✅ Đã nhận đơn #${orderId?.trim()}`);
  258 | 
  259 |       // Kiểm tra nút nhận đơn không còn hiển thị (đã chuyển trạng thái)
  260 |       const remainingBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  261 |       console.log(`🔄 Nhận đơn buttons còn: ${remainingBtns}`);
  262 |     } else {
  263 |       console.log('ℹ️ Không có đơn nào để nhận');
  264 |     }
  265 |   });
  266 | 
  267 |   test('[TC-3.11] Hủy đơn - nút hủy hoạt động', async ({ page }) => {
  268 |     await loginAsRestaurant(page);
  269 | 
  270 |     const restaurant = new RestaurantPage(page);
  271 |     await restaurant.gotoOrderList();
  272 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  273 | 
  274 |     // Kiểm tra nút hủy
  275 |     const cancelBtns = page.locator('a[href*="/Restaurant/huydon/"]');
  276 |     const btnCount = await cancelBtns.count();
  277 |     console.log(`🔴 Hủy đơn buttons: ${btnCount}`);
  278 | 
  279 |     if (btnCount > 0) {
  280 |       await cancelBtns.first().click();
  281 |       await page.waitForLoadState('networkidle');
  282 |       await page.waitForTimeout(2000);
  283 |       console.log('✅ Đã hủy đơn');
  284 |     }
  285 |   });
  286 | 
  287 |   test('[TC-3.12] Nút "Đã chuẩn bị xong" cho đơn đã xác nhận', async ({ page }) => {
  288 |     await loginAsRestaurant(page);
  289 | 
  290 |     const restaurant = new RestaurantPage(page);
  291 |     await restaurant.gotoOrderList();
  292 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  293 | 
```