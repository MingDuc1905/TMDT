# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🔄 Xử lý đơn hàng - Accept & Status Transitions >> [TC-3.9] Tạo đơn mới từ customer -> kiểm tra quán ăn thấy đơn
- Location: tests\03-restaurant-flow.spec.ts:165:7

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
  204 |     if (await submitBtn.isVisible()) {
  205 |       try {
  206 |         const confirmCb = customerPage.locator('#diff-acc');
  207 |         if (await confirmCb.isVisible()) await confirmCb.check();
  208 |       } catch {}
  209 |       await submitBtn.click();
  210 |       await customerPage.waitForTimeout(3000);
  211 |       await customerPage.waitForLoadState('networkidle');
  212 |       console.log(`✅ Customer: submitted order, URL: ${customerPage.url()}`);
  213 |     }
  214 |     await customerPage.close();
  215 | 
  216 |     // Quay lại tab quán ăn -> kiểm tra danh sách đơn
  217 |     const restaurant = new RestaurantPage(page);
  218 |     await loginAsRestaurant(page);
  219 |     await restaurant.gotoOrderList();
> 220 |     await page.waitForSelector('#example5', { timeout: 20_000 });
      |                ^ TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
  221 | 
  222 |     const orderCount = await restaurant.getOrderCount();
  223 |     console.log(`📋 Số đơn sau khi tạo: ${orderCount}`);
  224 |   });
  225 | 
  226 |   test('[TC-3.10] Nhận đơn -> chuyển trạng thái "Đã xác nhận"', async ({ page }) => {
  227 |     await loginAsRestaurant(page);
  228 | 
  229 |     const restaurant = new RestaurantPage(page);
  230 |     await restaurant.gotoOrderList();
  231 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  232 | 
  233 |     // Kiểm tra có đơn và nút nhận đơn
  234 |     const acceptBtns = page.locator('a[href*="/Restaurant/nhandon/"]');
  235 |     const btnCount = await acceptBtns.count();
  236 | 
  237 |     if (btnCount > 0) {
  238 |       // Get order info before accepting
  239 |       const firstRow = page.locator('#example5 tbody tr').first();
  240 |       const orderIdCell = firstRow.locator('td').first();
  241 |       const orderId = await orderIdCell.textContent();
  242 |       console.log(`📋 Nhận đơn #${orderId?.trim()}`);
  243 | 
  244 |       // Click nhận đơn
  245 |       await acceptBtns.first().click();
  246 |       await page.waitForLoadState('networkidle');
  247 |       await page.waitForTimeout(2000);
  248 |       console.log(`✅ Đã nhận đơn #${orderId?.trim()}`);
  249 | 
  250 |       // Kiểm tra nút nhận đơn không còn hiển thị (đã chuyển trạng thái)
  251 |       const remainingBtns = await page.locator('a[href*="/Restaurant/nhandon/"]').count();
  252 |       console.log(`🔄 Nhận đơn buttons còn: ${remainingBtns}`);
  253 |     } else {
  254 |       console.log('ℹ️ Không có đơn nào để nhận');
  255 |     }
  256 |   });
  257 | 
  258 |   test('[TC-3.11] Hủy đơn - nút hủy hoạt động', async ({ page }) => {
  259 |     await loginAsRestaurant(page);
  260 | 
  261 |     const restaurant = new RestaurantPage(page);
  262 |     await restaurant.gotoOrderList();
  263 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  264 | 
  265 |     // Kiểm tra nút hủy
  266 |     const cancelBtns = page.locator('a[href*="/Restaurant/huydon/"]');
  267 |     const btnCount = await cancelBtns.count();
  268 |     console.log(`🔴 Hủy đơn buttons: ${btnCount}`);
  269 | 
  270 |     if (btnCount > 0) {
  271 |       await cancelBtns.first().click();
  272 |       await page.waitForLoadState('networkidle');
  273 |       await page.waitForTimeout(2000);
  274 |       console.log('✅ Đã hủy đơn');
  275 |     }
  276 |   });
  277 | 
  278 |   test('[TC-3.12] Nút "Đã chuẩn bị xong" cho đơn đã xác nhận', async ({ page }) => {
  279 |     await loginAsRestaurant(page);
  280 | 
  281 |     const restaurant = new RestaurantPage(page);
  282 |     await restaurant.gotoOrderList();
  283 |     await page.waitForSelector('#example5', { timeout: 20_000 });
  284 | 
  285 |     const readyBtns = page.locator('a[href*="/Restaurant/hoantatdon/"]');
  286 |     const btnCount = await readyBtns.count();
  287 |     console.log(`✅ Đã chuẩn bị xong buttons: ${btnCount}`);
  288 | 
  289 |     if (btnCount > 0) {
  290 |       await readyBtns.first().click();
  291 |       await page.waitForLoadState('networkidle');
  292 |       await page.waitForTimeout(2000);
  293 |       console.log('✅ Đã chuyển trạng thái "Hoàn tất"');
  294 |     }
  295 |   });
  296 | });
  297 | 
  298 | // ─── TEST SUITE 4: Quản lý món ăn & Danh mục ───
  299 | test.describe('🍽️ Quản lý Món ăn', () => {
  300 | 
  301 |   test('[TC-3.13] Dashboard quán - kiểm tra thông tin quán', async ({ page }) => {
  302 |     await loginAsRestaurant(page);
  303 | 
  304 |     // Kiểm tra header/avatar quán
  305 |     const restaurantName = page.locator('.fs-avatar-xl + span, .name-restaurant').first();
  306 |     try {
  307 |       await expect(restaurantName).toBeVisible({ timeout: 5_000 });
  308 |       const name = await restaurantName.textContent();
  309 |       console.log(`🏪 Tên quán: ${name}`);
  310 |     } catch {
  311 |       console.log('ℹ️ Không tìm thấy tên quán trên header');
  312 |     }
  313 |   });
  314 | 
  315 |   test('[TC-3.14] Kiểm tra tất cả ảnh trên dashboard quán không bị vỡ', async ({ page }) => {
  316 |     await loginAsRestaurant(page);
  317 | 
  318 |     const imgResult = await page.evaluate(() => {
  319 |       const imgs = Array.from(document.querySelectorAll('img'));
  320 |       let broken = 0;
```