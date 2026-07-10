# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🍽️ Quản lý Món ăn >> [TC-3.15] Console không có lỗi trên dashboard quán
- Location: tests\03-restaurant-flow.spec.ts:330:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 0
Received: 1
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
  321 |       imgs.forEach((img) => {
  322 |         if (!img.complete || img.naturalWidth === 0) broken++;
  323 |       });
  324 |       return { total: imgs.length, broken };
  325 |     });
  326 |     console.log(`📸 Dashboard quán - Ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
  327 |     expect(imgResult.broken).toBe(0);
  328 |   });
  329 | 
  330 |   test('[TC-3.15] Console không có lỗi trên dashboard quán', async ({ page }) => {
  331 |     const errors: string[] = [];
  332 |     page.on('console', (msg) => {
  333 |       if (msg.type() === 'error') errors.push(msg.text());
  334 |     });
  335 | 
  336 |     await loginAsRestaurant(page);
  337 |     await page.waitForTimeout(3000);
  338 | 
  339 |     if (errors.length > 0) {
  340 |       console.log(`❌ Console errors: ${errors.join(' | ')}`);
  341 |     }
> 342 |     expect(errors.length).toBe(0);
      |                           ^ Error: expect(received).toBe(expected) // Object.is equality
  343 |   });
  344 | });
  345 | 
```