# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🛒 Vòng đời giỏ hàng (Cart Lifecycle) >> [TC-2.12] Thêm món vào giỏ (đã login) - kiểm tra giỏ có item
- Location: tests\02-customer-flow.spec.ts:262:7

# Error details

```
Error: expect(received).toBeGreaterThan(expected)

Expected: > 0
Received:   0
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
  191 |     const count = await home.getRestaurantCount();
  192 |     console.log(`🏷️ Category "Đồ ăn": ${count} quán`);
  193 |   });
  194 | 
  195 |   test('[TC-2.9] Click vào quán ăn - vào trang chi tiết thực đơn', async ({ page }) => {
  196 |     const home = new HomePage(page);
  197 |     await home.gotoHome();
  198 | 
  199 |     // ponytail: Render free chậm — timeout lâu hơn
  200 |     try {
  201 |       await page.waitForSelector('.product-item', { timeout: 25_000 });
  202 |       const count = await home.getRestaurantCount();
  203 |       expect(count).toBeGreaterThan(0);
  204 |       await home.clickFirstRestaurant();
  205 |       await page.waitForURL('**/DetailRestaurant**', { timeout: 25_000 });
  206 |     } catch {
  207 |       // Fallback: goto trực tiếp Koneko Pizza
  208 |       console.log('⏳ product-item/click timeout, thử goto trực tiếp...');
  209 |       await page.goto('/Home/DetailRestaurant?id=' + SEED.restaurantIds.konekoPizza, {
  210 |         waitUntil: 'networkidle',
  211 |         timeout: 20_000
  212 |       });
  213 |     }
  214 |     await page.waitForLoadState('networkidle');
  215 |     expect(page.url()).toContain('DetailRestaurant');
  216 |     console.log(`✅ DetailRestaurant URL: ${page.url()}`);
  217 | 
  218 |     const count = await home.getRestaurantCount();
  219 |     expect(count).toBeGreaterThan(0);
  220 | 
  221 |     await home.clickFirstRestaurant();
  222 |     await page.waitForURL('**/DetailRestaurant**', { timeout: 40_000 });
  223 |     await page.waitForLoadState('networkidle');
  224 | 
  225 |     expect(page.url()).toContain('DetailRestaurant');
  226 |     console.log(`✅ DetailRestaurant URL: ${page.url()}`);
  227 |   });
  228 | 
  229 |   test('[TC-2.10] Xem chi tiết quán - thực đơn có món ăn', async ({ page }) => {
  230 |     const detail = new DetailRestaurantPage(page);
  231 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  232 | 
  233 |     await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  234 |     const itemCount = await detail.getMenuItemCount();
  235 |     console.log(`🍕 Koneko Pizza: ${itemCount} món`);
  236 |     expect(itemCount).toBeGreaterThan(0);
  237 | 
  238 |     const name = await detail.getRestaurantName();
  239 |     expect(name).toBeTruthy();
  240 |     console.log(`🏪 Quán: ${name}`);
  241 |   });
  242 | });
  243 | 
  244 | // ─── TEST SUITE 3: Giỏ hàng ───
  245 | test.describe('🛒 Vòng đời giỏ hàng (Cart Lifecycle)', () => {
  246 | 
  247 |   test('[TC-2.11] Giỏ hàng trống - thông báo "trống" hiển thị', async ({ page }) => {
  248 |     const cart = new CartPage(page);
  249 |     await cart.gotoCart();
  250 |     await page.waitForLoadState('networkidle');
  251 | 
  252 |     try {
  253 |       await expect(cart.emptyCartMessage).toBeVisible({ timeout: 5_000 });
  254 |       console.log('✅ Giỏ trống - thông báo hiển thị');
  255 |     } catch {
  256 |       // Nếu không trống, kiểm tra có item
  257 |       const itemCount = await cart.getItemCount();
  258 |       console.log(`ℹ️ Giỏ có ${itemCount} món`);
  259 |     }
  260 |   });
  261 | 
  262 |   test('[TC-2.12] Thêm món vào giỏ (đã login) - kiểm tra giỏ có item', async ({ page }) => {
  263 |     // Login
  264 |     const login = new LoginPage(page);
  265 |     await login.gotoLogin();
  266 |     await login.usernameInput.fill(CUSTOMER.username);
  267 |     await login.passwordInput.fill(CUSTOMER.password);
  268 |     await login.loginButton.click();
  269 |     await page.waitForLoadState('networkidle');
  270 |     await page.waitForTimeout(1000);
  271 | 
  272 |     // Vào quán ăn
  273 |     const detail = new DetailRestaurantPage(page);
  274 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  275 |     await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  276 | 
  277 |     const itemCountBefore = await detail.getMenuItemCount();
  278 |     expect(itemCountBefore).toBeGreaterThan(0);
  279 | 
  280 |     // Thêm món đầu tiên
  281 |     await detail.addFirstItemToCart(1);
  282 |     console.log('✅ Đã thêm món vào giỏ');
  283 | 
  284 |     // Kiểm tra giỏ hàng
  285 |     const cart = new CartPage(page);
  286 |     await cart.gotoCart();
  287 |     await page.waitForLoadState('networkidle');
  288 | 
  289 |     const cartCount = await cart.getItemCount();
  290 |     console.log(`🛒 Số món trong giỏ: ${cartCount}`);
> 291 |     expect(cartCount).toBeGreaterThan(0);
      |                       ^ Error: expect(received).toBeGreaterThan(expected)
  292 |   });
  293 | 
  294 |   test('[TC-2.13] Tăng số lượng - tổng tiền thay đổi', async ({ page }) => {
  295 |     // Login + thêm món
  296 |     const login = new LoginPage(page);
  297 |     await login.gotoLogin();
  298 |     await login.usernameInput.fill(CUSTOMER.username);
  299 |     await login.passwordInput.fill(CUSTOMER.password);
  300 |     await login.loginButton.click();
  301 |     await page.waitForLoadState('networkidle').catch(() => {});
  302 |     await page.waitForTimeout(2000);
  303 |     if (page.url().includes('/Home/Login')) {
  304 |       console.log('ℹ️ Login không redirect — goto / để set session');
  305 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 15_000 }).catch(() => {});
  306 |     }
  307 | 
  308 |     const detail = new DetailRestaurantPage(page);
  309 |     try {
  310 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  311 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  312 |       await detail.addFirstItemToCart(1);
  313 | 
  314 |       const cart = new CartPage(page);
  315 |       await cart.gotoCart();
  316 |       await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  317 | 
  318 |       const itemCount = await cart.getItemCount();
  319 |       if (itemCount > 0) {
  320 |         const totalBefore = await cart.getTotalText();
  321 |         console.log(`💰 Tổng trước: ${totalBefore}`);
  322 |         await cart.increaseFirstItem();
  323 |         await page.waitForTimeout(2000);
  324 |         const totalAfter = await cart.getTotalText();
  325 |         console.log(`💰 Tổng sau tăng: ${totalAfter}`);
  326 |         expect(totalAfter).not.toEqual(totalBefore);
  327 |       }
  328 |     } catch (e) {
  329 |       console.log(`ℹ️ Cart test: ${e}`);
  330 |     }
  331 |   });
  332 | 
  333 |   test('[TC-2.14] Giảm số lượng về 0 - món bị xoá khỏi giỏ', async ({ page }) => {
  334 |     const login = new LoginPage(page);
  335 |     await login.gotoLogin();
  336 |     await login.usernameInput.fill(CUSTOMER.username);
  337 |     await login.passwordInput.fill(CUSTOMER.password);
  338 |     await login.loginButton.click();
  339 |     await page.waitForLoadState('networkidle').catch(() => {});
  340 |     await page.waitForTimeout(2000);
  341 |     if (page.url().includes('/Home/Login')) {
  342 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 15_000 }).catch(() => {});
  343 |     }
  344 | 
  345 |     const detail = new DetailRestaurantPage(page);
  346 |     try {
  347 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  348 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  349 |       await detail.addFirstItemToCart(1);
  350 | 
  351 |       const cart = new CartPage(page);
  352 |       await cart.gotoCart();
  353 |       await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  354 | 
  355 |       let itemCount = await cart.getItemCount();
  356 |       console.log(`🛒 Item count: ${itemCount}`);
  357 |       if (itemCount > 0) {
  358 |         for (let i = 0; i < 5; i++) {
  359 |           const qty = await cart.getFirstItemQuantity().catch(() => 0);
  360 |           if (qty <= 1) {
  361 |             await cart.decreaseFirstItem();
  362 |             await page.waitForTimeout(2000);
  363 |             const newCount = await cart.getItemCount();
  364 |             if (newCount < itemCount) { console.log(`✅ Giảm về 0, còn ${newCount} món`); break; }
  365 |           } else {
  366 |             await cart.decreaseFirstItem();
  367 |             await page.waitForTimeout(1500);
  368 |           }
  369 |         }
  370 |       }
  371 |     } catch (e) { console.log(`ℹ️ Cart test: ${e}`); }
  372 |   });
  373 | 
  374 |   test('[TC-2.15] Xoá món khỏi giỏ - nút Delete hoạt động', async ({ page }) => {
  375 |     // Login + thêm món
  376 |     const login = new LoginPage(page);
  377 |     await login.gotoLogin();
  378 |     await login.usernameInput.fill(CUSTOMER.username);
  379 |     await login.passwordInput.fill(CUSTOMER.password);
  380 |     await login.loginButton.click();
  381 |     await page.waitForLoadState('networkidle');
  382 | 
  383 |     const detail = new DetailRestaurantPage(page);
  384 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  385 |     await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  386 |     await detail.addFirstItemToCart(1);
  387 | 
  388 |     const cart = new CartPage(page);
  389 |     await cart.gotoCart();
  390 |     await page.waitForLoadState('networkidle');
  391 | 
```