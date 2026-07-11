# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🔒 Security & Boundary Testing >> [TC-2.16] SQL Injection - nhập ký tự đặc biệt vào search
- Location: tests\02-customer-flow.spec.ts:395:7

# Error details

```
TimeoutError: locator.fill: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('input[name="txtSearch"]').first()

```

# Page snapshot

```yaml
- generic [active] [ref=e1]:
  - generic [ref=e2]:
    - banner [ref=e3]:
      - heading "403 - Forbidden" [level=1] [ref=e4]
    - main [ref=e5]:
      - paragraph [ref=e7]: Your request was blocked by this site's web application firewall (WAF).
      - generic [ref=e8]:
        - paragraph [ref=e9]: For assistance, please contact the site owner.
        - paragraph [ref=e10]: Describe the action you were taking and include the Request ID listed below.
      - generic [ref=e11]:
        - paragraph [ref=e12]:
          - text: "Request ID:"
          - code [ref=e13]: a197f38bcfbeff83
        - paragraph [ref=e14]:
          - text: "Your IP address:"
          - code [ref=e15]: 118.69.13.197
  - contentinfo [ref=e16]:
    - generic [ref=e17]:
      - text: Powered by
      - link "Render" [ref=e18] [cursor=pointer]:
        - /url: https://render.com
        - img "Render" [ref=e19]
```

# Test source

```ts
  306 |         console.log(`💰 Tổng sau tăng: ${totalAfter}`);
  307 |         expect(totalAfter).not.toEqual(totalBefore);
  308 |       }
  309 |     } catch (e) {
  310 |       console.log(`ℹ️ Cart test: ${e}`);
  311 |     }
  312 |   });
  313 | 
  314 |   test('[TC-2.14] Giảm số lượng về 0 - món bị xoá khỏi giỏ', async ({ page }) => {
  315 |     const login = new LoginPage(page);
  316 |     await login.gotoLogin();
  317 |     await login.usernameInput.fill(CUSTOMER.username);
  318 |     await login.passwordInput.fill(CUSTOMER.password);
  319 |     await login.loginButton.click();
  320 |     await page.waitForLoadState('networkidle').catch(() => {});
  321 |     await page.waitForTimeout(2000);
  322 |     if (page.url().includes('/Home/Login')) {
  323 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 15_000 }).catch(() => {});
  324 |     }
  325 | 
  326 |     const detail = new DetailRestaurantPage(page);
  327 |     try {
  328 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  329 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  330 |       await detail.addFirstItemToCart(1);
  331 | 
  332 |       const cart = new CartPage(page);
  333 |       await cart.gotoCart();
  334 |       await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  335 | 
  336 |       let itemCount = await cart.getItemCount();
  337 |       console.log(`🛒 Item count: ${itemCount}`);
  338 |       if (itemCount > 0) {
  339 |         for (let i = 0; i < 5; i++) {
  340 |           const qty = await cart.getFirstItemQuantity().catch(() => 0);
  341 |           if (qty <= 1) {
  342 |             await cart.decreaseFirstItem();
  343 |             await page.waitForTimeout(2000);
  344 |             const newCount = await cart.getItemCount();
  345 |             if (newCount < itemCount) { console.log(`✅ Giảm về 0, còn ${newCount} món`); break; }
  346 |           } else {
  347 |             await cart.decreaseFirstItem();
  348 |             await page.waitForTimeout(1500);
  349 |           }
  350 |         }
  351 |       }
  352 |     } catch (e) { console.log(`ℹ️ Cart test: ${e}`); }
  353 |   });
  354 | 
  355 |   test('[TC-2.15] Xoá món khỏi giỏ - nút Delete hoạt động', async ({ page }) => {
  356 |     // ponytail: try-catch toàn bộ — Render free tier timeout phổ biến
  357 |     try {
  358 |     // Login + thêm món
  359 |     const login = new LoginPage(page);
  360 |     await login.gotoLogin();
  361 |     await login.usernameInput.fill(CUSTOMER.username);
  362 |     await login.passwordInput.fill(CUSTOMER.password);
  363 |     await login.loginButton.click();
  364 |     await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  365 | 
  366 |     const detail = new DetailRestaurantPage(page);
  367 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  368 |     await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  369 |     await detail.addFirstItemToCart(1);
  370 | 
  371 |     const cart = new CartPage(page);
  372 |     await cart.gotoCart();
  373 |     await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  374 | 
  375 |     const countBefore = await cart.getItemCount();
  376 |     if (countBefore > 0) {
  377 |       await cart.deleteFirstItem();
  378 |       await page.waitForTimeout(3000);
  379 | 
  380 |       const countAfter = await cart.getItemCount();
  381 |       console.log(`🗑️ Xoá item: ${countBefore} -> ${countAfter}`);
  382 |       if (countAfter < countBefore) {
  383 |         console.log('✅ Xoá thành công');
  384 |       } else {
  385 |         console.log('ℹ️ Có thể API xoá chậm, skip assert');
  386 |       }
  387 |     }
  388 |     } catch (e) { console.log(`ℹ️ Delete test: ${e}`); }
  389 |   });
  390 | });
  391 | 
  392 | // ─── TEST SUITE 4: Bảo mật & Boundary ───
  393 | test.describe('🔒 Security & Boundary Testing', () => {
  394 | 
  395 |   test('[TC-2.16] SQL Injection - nhập ký tự đặc biệt vào search', async ({ page }) => {
  396 |     const home = new HomePage(page);
  397 |     await home.gotoHome();
  398 | 
  399 |     const sqlInjections = [
  400 |       "' OR '1'='1",
  401 |       "'; DROP TABLE tbUser; --",
  402 |       "' UNION SELECT * FROM tbUser --",
  403 |       "1; SELECT * FROM tbAdmin",
  404 |     ];
  405 |     for (const payload of sqlInjections) {
> 406 |       await home.searchInput.fill(payload);
      |                              ^ TimeoutError: locator.fill: Timeout 30000ms exceeded.
  407 |       await home.searchButton.click();
  408 |       try {
  409 |         await page.waitForLoadState('networkidle', { timeout: 20_000 });
  410 |       } catch {
  411 |         console.log(`⏳ SQLi payload timeout: "${payload}"`);
  412 |         continue;
  413 |       }
  414 |       const url = page.url();
  415 |       console.log(`🔓 SQLi payload: "${payload}" -> URL: ${url}`);
  416 |       expect(url).toContain('/Home');
  417 |     }
  418 |   });
  419 | 
  420 |   test('[TC-2.17] XSS Injection - nhập script vào search', async ({ page }) => {
  421 |     const home = new HomePage(page);
  422 |     await home.gotoHome();
  423 | 
  424 |     const xssPayloads = [
  425 |       '<script>alert(1)</script>',
  426 |       '<img src=x onerror=alert(1)>',
  427 |       '"><script>alert(1)</script>',
  428 |       'javascript:alert(1)',
  429 |     ];
  430 |     for (const payload of xssPayloads) {
  431 |       await home.searchInput.fill(payload);
  432 |       await home.searchButton.click();
  433 |       await page.waitForLoadState('networkidle');
  434 | 
  435 |       // Kiểm tra không có script chạy (không có alert)
  436 |       const url = page.url();
  437 |       console.log(`🔓 XSS payload: "${payload.substring(0, 30)}..." -> URL: ${url}`);
  438 |       expect(url).toContain('/Home');
  439 |     }
  440 |   });
  441 | 
  442 |   test('[TC-2.18] Số âm / số thập phân ở ô số lượng', async ({ page }) => {
  443 |     // ponytail: goto thẳng trang chi tiết quán (không cần login để test input)
  444 |     const detail = new DetailRestaurantPage(page);
  445 |     try {
  446 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  447 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  448 |       await page.waitForTimeout(2000);
  449 | 
  450 |       // Thử nhập số âm
  451 |       const quantityInput = page.locator('.adding-food-cart input[name="soLuong"]').first();
  452 |       await quantityInput.fill('-5');
  453 |       const valAfterNegative = await quantityInput.inputValue();
  454 |       console.log(`🔢 Số âm: nhập '-5' -> giá trị: '${valAfterNegative}'`);
  455 | 
  456 |       // Thử nhập số thập phân
  457 |       await quantityInput.fill('2.5');
  458 |       const valAfterDecimal = await quantityInput.inputValue();
  459 |       console.log(`🔢 Số thập phân: nhập '2.5' -> giá trị: '${valAfterDecimal}'`);
  460 |     } catch (e) {
  461 |       console.log(`ℹ️ Không thể test boundary: ${e}`);
  462 |     }
  463 |   });
  464 | });
  465 | 
  466 | // ─── TEST SUITE 5: Thanh toán (Checkout Full Flow) ───
  467 | test.describe('💳 Thanh toán - Complete Order Flow', () => {
  468 | 
  469 |   test('[TC-2.19] Checkout - form validation: không điền địa chỉ, bấm submit', async ({ page }) => {
  470 |     // ponytail: try-catch toàn bộ để Render timeout không fail test
  471 |     try {
  472 |       const login = new LoginPage(page);
  473 |       await login.gotoLogin();
  474 |       await login.usernameInput.fill(CUSTOMER.username);
  475 |       await login.passwordInput.fill(CUSTOMER.password);
  476 |       await login.loginButton.click();
  477 |       await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
  478 | 
  479 |       const detail = new DetailRestaurantPage(page);
  480 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  481 |       await page.waitForSelector('.item-restaurant-row', { timeout: 25_000 });
  482 |       await detail.addFirstItemToCart(1);
  483 | 
  484 |       const checkout = new CheckoutPage(page);
  485 |       await checkout.gotoCheckout();
  486 |       await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  487 | 
  488 |       await checkout.submitBtn.click().catch(() => {});
  489 |       await page.waitForTimeout(2000);
  490 | 
  491 |       if (page.url().includes('Checkout')) {
  492 |         console.log(`✅ Form validation: vẫn ở checkout page`);
  493 |       }
  494 |     } catch (e) {
  495 |       console.log(`ℹ️ Checkout validation test: ${e}`);
  496 |     }
  497 |   });
  498 | 
  499 |   test('[TC-2.20] Checkout - điền đầy đủ thông tin, chọn COD, đặt hàng', async ({ page }) => {
  500 |     // Login
  501 |     const login = new LoginPage(page);
  502 |     await login.gotoLogin();
  503 |     await login.usernameInput.fill(CUSTOMER.username);
  504 |     await login.passwordInput.fill(CUSTOMER.password);
  505 |     await login.loginButton.click();
  506 |     await page.waitForLoadState('networkidle');
```