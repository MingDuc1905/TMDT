# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 03-restaurant-flow.spec.ts >> 🍽️ Quản lý Món ăn >> [TC-3.15] Console không có JS errors (bỏ qua network 429)
- Location: tests\03-restaurant-flow.spec.ts:339:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 0
Received: 2
```

# Page snapshot

```yaml
- generic [active] [ref=e1]:
  - generic [ref=e2]:
    - link [ref=e4] [cursor=pointer]:
      - /url: index.html
    - navigation [ref=e12]:
      - generic [ref=e13]:
        - img [ref=e17] [cursor=pointer]
        - list [ref=e19]:
          - listitem [ref=e20]:
            - link [ref=e21] [cursor=pointer]:
              - /url: "#"
              - img [ref=e22]
          - listitem [ref=e27]:
            - button [ref=e28] [cursor=pointer]:
              - img [ref=e29]
          - listitem [ref=e32]:
            - generic [ref=e34] [cursor=pointer]:
              - link "Đóng":
                - /url: /Restaurant/updateStatus
                - generic [ref=e35]: Đóng
          - listitem [ref=e36]:
            - button "Xin chào, konekopizza" [ref=e37] [cursor=pointer]:
              - generic [ref=e39]:
                - text: Xin chào,
                - strong [ref=e40]: konekopizza
    - generic [ref=e42]:
      - list [ref=e43]:
        - listitem [ref=e44]:
          - link " Dashboard" [ref=e45] [cursor=pointer]:
            - /url: javascript:void()
            - generic [ref=e46]: 
            - text: Dashboard
          - list [ref=e47]:
            - listitem [ref=e48]:
              - link "Dashboard" [ref=e49] [cursor=pointer]:
                - /url: /Restaurant
            - listitem [ref=e50]:
              - link "Phân tích" [ref=e51] [cursor=pointer]:
                - /url: /Restaurant/Analytics
            - listitem [ref=e52]:
              - link "Đánh giá" [ref=e53] [cursor=pointer]:
                - /url: /Restaurant/Review
            - listitem [ref=e54]:
              - link "Danh sách đơn hàng" [ref=e55] [cursor=pointer]:
                - /url: /Restaurant/OrderList
        - listitem [ref=e56]:
          - link " Apps" [ref=e57] [cursor=pointer]:
            - /url: javascript:void()
            - generic [ref=e58]: 
            - text: Apps
      - generic [ref=e59]:
        - paragraph [ref=e60]: Sắp xếp các menu của bạn thông qua nút bên dưới
        - link "+ Thêm thực đơn" [ref=e61] [cursor=pointer]:
          - /url: /Restaurant/ProductDetail
    - generic [ref=e63]:
      - generic [ref=e65]:
        - heading "Thống kê" [level=2] [ref=e66]
        - paragraph [ref=e67]: Xin chào quản lí Koneko Pizza
      - generic [ref=e70]:
        - generic [ref=e72]:
          - generic [ref=e73]: 🤖
          - generic [ref=e74]:
            - heading "Chiến lược bán chéo từ dữ liệu" [level=4] [ref=e75]
            - paragraph [ref=e76]: Phân tích Apriori trên 27 đơn hàng hoàn thành
          - generic [ref=e77]: AI
        - generic [ref=e78]:
          - paragraph [ref=e79]:
            - generic [ref=e80]: 
            - text: Những cặp món sau thường được khách đặt cùng nhau. Hãy tạo
            - strong [ref=e81]: Combo khuyến mãi
            - text: cho các cặp này để tăng doanh thu!
          - generic [ref=e83]:
            - generic [ref=e84]:
              - generic [ref=e85]: Trà tắc
              - generic [ref=e86]: +
              - generic [ref=e87]: Pizza thập cẩm
            - generic [ref=e89]:
              - strong [ref=e90]: 100%
              - text: khách mua Trà tắc cũng mua Pizza thập cẩm
            - generic [ref=e91]:
              - generic [ref=e92]:
                - text: Support
                - strong [ref=e93]: 3.7%
              - generic [ref=e94]: 1 đơn
  - img
```

# Test source

```ts
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
> 349 |     expect(jsErrors.length).toBe(0);
      |                             ^ Error: expect(received).toBe(expected) // Object.is equality
  350 |   });
  351 | });
  352 | 
```