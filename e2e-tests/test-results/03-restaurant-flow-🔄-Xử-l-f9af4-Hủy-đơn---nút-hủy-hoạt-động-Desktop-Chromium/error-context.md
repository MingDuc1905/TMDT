# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🔄 Xử lý đơn hàng - Accept & Status Transitions >> [TC-3.11] Hủy đơn - nút hủy hoạt động
- Location: tests\03-restaurant-flow.spec.ts:267:7

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
> 272 |     await page.waitForSelector('#example5', { timeout: 20_000 });
      |                ^ TimeoutError: page.waitForSelector: Timeout 20000ms exceeded.
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
  294 |     const readyBtns = page.locator('a[href*="/Restaurant/hoantatdon/"]');
  295 |     const btnCount = await readyBtns.count();
  296 |     console.log(`✅ Đã chuẩn bị xong buttons: ${btnCount}`);
  297 | 
  298 |     if (btnCount > 0) {
  299 |       await readyBtns.first().click();
  300 |       await page.waitForLoadState('networkidle');
  301 |       await page.waitForTimeout(2000);
  302 |       console.log('✅ Đã chuyển trạng thái "Hoàn tất"');
  303 |     }
  304 |   });
  305 | });
  306 | 
  307 | // ─── TEST SUITE 4: Quản lý món ăn & Danh mục ───
  308 | test.describe('🍽️ Quản lý Món ăn', () => {
  309 | 
  310 |   test('[TC-3.13] Dashboard quán - kiểm tra thông tin quán', async ({ page }) => {
  311 |     await loginAsRestaurant(page);
  312 | 
  313 |     // Kiểm tra header/avatar quán
  314 |     const restaurantName = page.locator('.fs-avatar-xl + span, .name-restaurant').first();
  315 |     try {
  316 |       await expect(restaurantName).toBeVisible({ timeout: 5_000 });
  317 |       const name = await restaurantName.textContent();
  318 |       console.log(`🏪 Tên quán: ${name}`);
  319 |     } catch {
  320 |       console.log('ℹ️ Không tìm thấy tên quán trên header');
  321 |     }
  322 |   });
  323 | 
  324 |   test('[TC-3.14] Kiểm tra tất cả ảnh trên dashboard quán không bị vỡ', async ({ page }) => {
  325 |     await loginAsRestaurant(page);
  326 | 
  327 |     const imgResult = await page.evaluate(() => {
  328 |       const imgs = Array.from(document.querySelectorAll('img'));
  329 |       let broken = 0;
  330 |       imgs.forEach((img) => {
  331 |         if (!img.complete || img.naturalWidth === 0) broken++;
  332 |       });
  333 |       return { total: imgs.length, broken };
  334 |     });
  335 |     console.log(`📸 Dashboard quán - Ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
  336 |     expect(imgResult.broken).toBe(0);
  337 |   });
  338 | 
  339 |   test('[TC-3.15] Console không có JS errors (bỏ qua network 429)', async ({ page }) => {
  340 |     const jsErrors: string[] = [];
  341 |     page.on('pageerror', (err) => { jsErrors.push(err.message); });
  342 | 
  343 |     await loginAsRestaurant(page);
  344 |     await page.waitForTimeout(3000);
  345 | 
  346 |     if (jsErrors.length > 0) {
  347 |       console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
  348 |     }
  349 |     expect(jsErrors.length).toBe(0);
  350 |   });
  351 | });
  352 | 
```