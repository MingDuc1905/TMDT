# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🍽️ Quản lý Món ăn >> [TC-3.14] Kiểm tra tất cả ảnh trên dashboard quán không bị vỡ
- Location: tests\03-restaurant-flow.spec.ts:315:7

# Error details

```
Error: page.evaluate: Execution context was destroyed, most likely because of a navigation
```

# Page snapshot

```yaml
- generic [ref=e7]:
  - link [ref=e9] [cursor=pointer]:
    - /url: index.html
  - navigation [ref=e17]:
    - generic [ref=e18]:
      - img [ref=e22] [cursor=pointer]
      - list [ref=e24]:
        - listitem [ref=e25]:
          - link [ref=e26] [cursor=pointer]:
            - /url: "#"
            - img [ref=e27]
        - listitem [ref=e32]:
          - button [ref=e33] [cursor=pointer]:
            - img [ref=e34]
        - listitem [ref=e37]:
          - generic [ref=e39] [cursor=pointer]:
            - link "Đóng":
              - /url: /Restaurant/updateStatus
              - generic [ref=e40]: Đóng
        - listitem [ref=e41]:
          - button "Xin chào, konekopizza" [ref=e42] [cursor=pointer]:
            - generic [ref=e44]:
              - text: Xin chào,
              - strong [ref=e45]: konekopizza
  - generic [ref=e47]:
    - list [ref=e48]:
      - listitem [ref=e49]:
        - link " Dashboard" [ref=e50] [cursor=pointer]:
          - /url: javascript:void()
          - generic [ref=e51]: 
          - text: Dashboard
        - list [ref=e52]:
          - listitem [ref=e53]:
            - link "Dashboard" [ref=e54] [cursor=pointer]:
              - /url: /Restaurant
          - listitem [ref=e55]:
            - link "Phân tích" [ref=e56] [cursor=pointer]:
              - /url: /Restaurant/Analytics
          - listitem [ref=e57]:
            - link "Đánh giá" [ref=e58] [cursor=pointer]:
              - /url: /Restaurant/Review
          - listitem [ref=e59]:
            - link "Danh sách đơn hàng" [ref=e60] [cursor=pointer]:
              - /url: /Restaurant/OrderList
      - listitem [ref=e61]:
        - link " Apps" [ref=e62] [cursor=pointer]:
          - /url: javascript:void()
          - generic [ref=e63]: 
          - text: Apps
        - list [ref=e64]:
          - listitem [ref=e65]:
            - link "Hồ sơ" [ref=e66] [cursor=pointer]:
              - /url: /Restaurant/Profile
          - listitem [ref=e67]:
            - link "Cửa hàng" [ref=e68] [cursor=pointer]:
              - /url: javascript:void()
            - list [ref=e69]:
              - listitem [ref=e70]:
                - link "Danh sách thực đơn" [ref=e71] [cursor=pointer]:
                  - /url: /Restaurant/ProductList
              - listitem [ref=e72]:
                - link "Chi tiết món" [ref=e73] [cursor=pointer]:
                  - /url: /Restaurant/ProductDetail
    - generic [ref=e74]:
      - paragraph [ref=e75]: Sắp xếp các menu của bạn thông qua nút bên dưới
      - link "+ Thêm thực đơn" [ref=e76] [cursor=pointer]:
        - /url: /Restaurant/ProductDetail
  - generic [ref=e78]:
    - generic [ref=e80]:
      - heading "Thống kê" [level=2] [ref=e81]
      - paragraph [ref=e82]: Xin chào quản lí Koneko Pizza
    - generic [ref=e85]:
      - generic [ref=e87]:
        - generic [ref=e88]: 🤖
        - generic [ref=e89]:
          - heading "Chiến lược bán chéo từ dữ liệu" [level=4] [ref=e90]
          - paragraph [ref=e91]: Phân tích Apriori trên 27 đơn hàng hoàn thành
        - generic [ref=e92]: AI
      - generic [ref=e93]:
        - paragraph [ref=e94]:
          - generic [ref=e95]: 
          - text: Những cặp món sau thường được khách đặt cùng nhau. Hãy tạo
          - strong [ref=e96]: Combo khuyến mãi
          - text: cho các cặp này để tăng doanh thu!
        - generic [ref=e98]:
          - generic [ref=e99]:
            - generic [ref=e100]: Trà tắc
            - generic [ref=e101]: +
            - generic [ref=e102]: Pizza thập cẩm
          - generic [ref=e104]:
            - strong [ref=e105]: 100%
            - text: khách mua Trà tắc cũng mua Pizza thập cẩm
          - generic [ref=e106]:
            - generic [ref=e107]:
              - text: Support
              - strong [ref=e108]: 3.7%
            - generic [ref=e109]: 1 đơn
```

# Test source

```ts
  218 |     await loginAsRestaurant(page);
  219 |     await restaurant.gotoOrderList();
  220 |     await page.waitForSelector('#example5', { timeout: 20_000 });
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
> 318 |     const imgResult = await page.evaluate(() => {
      |                                  ^ Error: page.evaluate: Execution context was destroyed, most likely because of a navigation
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
  342 |     expect(errors.length).toBe(0);
  343 |   });
  344 | });
  345 | 
```