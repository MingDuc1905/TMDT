# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🔄 Xử lý đơn hàng - Accept & Status Transitions >> [TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn
- Location: tests\03-restaurant-flow.spec.ts:187:7

# Error details

```
TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
```

# Test source

```ts
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
  199 |     await customerPage.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  200 | 
  201 |     // Thêm món đầu tiên
  202 |     const addBtn = customerPage.locator('.add-to-cart-btn').first();
  203 |     const qtyInput = customerPage.locator('.adding-food-cart input[name="soLuong"]').first();
  204 |     await qtyInput.fill('1');
  205 |     await addBtn.click();
> 206 |     await customerPage.waitForResponse(resp => resp.url().includes('ApiThemMonAn') && resp.status() === 200);
      |                        ^ TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
  207 |     await customerPage.waitForLoadState('networkidle');
  208 |     console.log('✅ Customer: thêm món vào giỏ');
  209 | 
  210 |     // Vào checkout
  211 |     await customerPage.goto('/Cart/Checkout', { waitUntil: 'networkidle' });
  212 | 
  213 |     // Điền thông tin + đặt hàng
  214 |     const nameInput = customerPage.locator('#input-hoten');
  215 |     const phoneInput = customerPage.locator('#input-sdt');
  216 |     const addressInput = customerPage.locator('#input-diachi');
  217 |     if (await nameInput.isVisible()) {
  218 |       await nameInput.fill(USERS.customer1.name);
  219 |       await phoneInput.fill('0987654321');
  220 |       await addressInput.fill('02 Thanh Sơn, Thanh Bình, Hải Châu');
  221 |       await customerPage.waitForTimeout(500);
  222 |     }
  223 | 
  224 |     // Submit order
  225 |     const submitBtn = customerPage.locator('#btn-submit-cod');
  226 |     if (await submitBtn.isVisible()) {
  227 |       try {
  228 |         const confirmCb = customerPage.locator('#diff-acc');
  229 |         if (await confirmCb.isVisible()) await confirmCb.check();
  230 |       } catch {}
  231 |       await submitBtn.click();
  232 |       await customerPage.waitForTimeout(3000);
  233 |       await customerPage.waitForLoadState('networkidle');
  234 |       console.log(`✅ Customer: submitted order, URL: ${customerPage.url()}`);
  235 |     }
  236 |     await customerPage.close();
  237 | 
  238 |     // Quay lại tab quán ăn -> kiểm tra danh sách đơn
  239 |     const restaurant = new RestaurantPage(page);
  240 |     await loginAsRestaurant(page);
  241 |     await restaurant.gotoOrderList();
  242 |     await waitForOrderTable(page);
  243 | 
  244 |     const orderCount = await restaurant.getOrderCount();
  245 |     console.log(`📋 Số đơn sau khi tạo: ${orderCount}`);
  246 |   });
  247 | 
  248 |   test('[TC-3.10] Nhận đơn -> chuyển trạng thái "Đã xác nhận"', async ({ page }) => {
  249 |     await loginAsRestaurant(page);
  250 | 
  251 |     const restaurant = new RestaurantPage(page);
  252 |     await restaurant.gotoOrderList();
  253 |     await waitForOrderTable(page);
  254 | 
  255 |     const acceptBtns = page.locator('a[href*="/Restaurant/nhandon/"]');
  256 |     const btnCount = await acceptBtns.count();
  257 | 
  258 |     if (btnCount > 0) {
  259 |       // Get order info before accepting
  260 |       const firstRow = page.locator('#example5 tbody tr').first();
  261 |       const orderIdCell = firstRow.locator('td').first();
  262 |       const orderId = await orderIdCell.textContent();
  263 |       console.log(`📋 Nhận đơn #${orderId?.trim()}`);
  264 | 
  265 |       // Click nhận đơn
  266 |       await acceptBtns.first().click();
  267 |       await page.waitForLoadState('networkidle');
  268 |       await page.waitForTimeout(2000);
  269 |       console.log(`✅ Đã nhận đơn #${orderId?.trim()}`);
  270 | 
  271 |       // Kiểm tra nút nhận đơn không còn hiển thị (đã chuyển trạng thái)
  272 |       const remainingBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  273 |       console.log(`🔄 Nhận đơn buttons còn: ${remainingBtns}`);
  274 |     } else {
  275 |       console.log('ℹ️ Không có đơn nào để nhận');
  276 |     }
  277 |   });
  278 | 
  279 |   test('[TC-3.11] Hủy đơn - nút hủy hoạt động', async ({ page }) => {
  280 |     await loginAsRestaurant(page);
  281 | 
  282 |     const restaurant = new RestaurantPage(page);
  283 |     await restaurant.gotoOrderList();
  284 |     await waitForOrderTable(page);
  285 | 
  286 |     // Kiểm tra nút hủy
  287 |     const cancelBtns = page.locator('a[href*="/Restaurant/huydon/"]');
  288 |     const btnCount = await cancelBtns.count();
  289 |     console.log(`🔴 Hủy đơn buttons: ${btnCount}`);
  290 | 
  291 |     if (btnCount > 0) {
  292 |       await cancelBtns.first().click();
  293 |       await page.waitForLoadState('networkidle');
  294 |       await page.waitForTimeout(2000);
  295 |       console.log('✅ Đã hủy đơn');
  296 |     }
  297 |   });
  298 | 
  299 |   test('[TC-3.12] Nút "Đã chuẩn bị xong" cho đơn đã xác nhận', async ({ page }) => {
  300 |     await loginAsRestaurant(page);
  301 | 
  302 |     const restaurant = new RestaurantPage(page);
  303 |     await restaurant.gotoOrderList();
  304 |     await waitForOrderTable(page);
  305 | 
  306 |     const readyBtns = page.locator('a[href*="/Restaurant/hoantatdon/"]');
```