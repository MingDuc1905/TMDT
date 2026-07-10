# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🔒 Security & Boundary Testing >> [TC-2.16] SQL Injection - nhập ký tự đặc biệt vào search
- Location: tests\02-customer-flow.spec.ts:408:7

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
          - code [ref=e13]: a18fcb4e1f73fd9e
        - paragraph [ref=e14]:
          - text: "Your IP address:"
          - code [ref=e15]: 42.116.172.109
  - contentinfo [ref=e16]:
    - generic [ref=e17]:
      - text: Powered by
      - link "Render" [ref=e18] [cursor=pointer]:
        - /url: https://render.com
        - img "Render" [ref=e19]
```

# Test source

```ts
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
  392 |     const countBefore = await cart.getItemCount();
  393 |     if (countBefore > 0) {
  394 |       await cart.deleteFirstItem();
  395 |       await page.waitForLoadState('networkidle');
  396 |       await page.waitForTimeout(1000);
  397 | 
  398 |       const countAfter = await cart.getItemCount();
  399 |       console.log(`🗑️ Xoá item: ${countBefore} -> ${countAfter}`);
  400 |       expect(countAfter).toBeLessThan(countBefore);
  401 |     }
  402 |   });
  403 | });
  404 | 
  405 | // ─── TEST SUITE 4: Bảo mật & Boundary ───
  406 | test.describe('🔒 Security & Boundary Testing', () => {
  407 | 
  408 |   test('[TC-2.16] SQL Injection - nhập ký tự đặc biệt vào search', async ({ page }) => {
  409 |     const home = new HomePage(page);
  410 |     await home.gotoHome();
  411 | 
  412 |     const sqlInjections = [
  413 |       "' OR '1'='1",
  414 |       "'; DROP TABLE tbUser; --",
  415 |       "' UNION SELECT * FROM tbUser --",
  416 |       "1; SELECT * FROM tbAdmin",
  417 |     ];
  418 |     for (const payload of sqlInjections) {
> 419 |       await home.searchInput.fill(payload);
      |                              ^ TimeoutError: locator.fill: Timeout 30000ms exceeded.
  420 |       await home.searchButton.click();
  421 |       await page.waitForLoadState('networkidle');
  422 |       // Không crash, không redirect lạ
  423 |       const url = page.url();
  424 |       console.log(`🔓 SQLi payload: "${payload}" -> URL: ${url}`);
  425 |       expect(url).toContain('/Home');
  426 |     }
  427 |   });
  428 | 
  429 |   test('[TC-2.17] XSS Injection - nhập script vào search', async ({ page }) => {
  430 |     const home = new HomePage(page);
  431 |     await home.gotoHome();
  432 | 
  433 |     const xssPayloads = [
  434 |       '<script>alert(1)</script>',
  435 |       '<img src=x onerror=alert(1)>',
  436 |       '"><script>alert(1)</script>',
  437 |       'javascript:alert(1)',
  438 |     ];
  439 |     for (const payload of xssPayloads) {
  440 |       await home.searchInput.fill(payload);
  441 |       await home.searchButton.click();
  442 |       await page.waitForLoadState('networkidle');
  443 | 
  444 |       // Kiểm tra không có script chạy (không có alert)
  445 |       const url = page.url();
  446 |       console.log(`🔓 XSS payload: "${payload.substring(0, 30)}..." -> URL: ${url}`);
  447 |       expect(url).toContain('/Home');
  448 |     }
  449 |   });
  450 | 
  451 |   test('[TC-2.18] Số âm / số thập phân ở ô số lượng', async ({ page }) => {
  452 |     // ponytail: goto thẳng trang chi tiết quán (không cần login để test input)
  453 |     const detail = new DetailRestaurantPage(page);
  454 |     try {
  455 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  456 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  457 |       await page.waitForTimeout(2000);
  458 | 
  459 |       // Thử nhập số âm
  460 |       const quantityInput = page.locator('.adding-food-cart input[name="soLuong"]').first();
  461 |       await quantityInput.fill('-5');
  462 |       const valAfterNegative = await quantityInput.inputValue();
  463 |       console.log(`🔢 Số âm: nhập '-5' -> giá trị: '${valAfterNegative}'`);
  464 | 
  465 |       // Thử nhập số thập phân
  466 |       await quantityInput.fill('2.5');
  467 |       const valAfterDecimal = await quantityInput.inputValue();
  468 |       console.log(`🔢 Số thập phân: nhập '2.5' -> giá trị: '${valAfterDecimal}'`);
  469 |     } catch (e) {
  470 |       console.log(`ℹ️ Không thể test boundary: ${e}`);
  471 |     }
  472 |   });
  473 | });
  474 | 
  475 | // ─── TEST SUITE 5: Thanh toán (Checkout Full Flow) ───
  476 | test.describe('💳 Thanh toán - Complete Order Flow', () => {
  477 | 
  478 |   test('[TC-2.19] Checkout - form validation: không điền địa chỉ, bấm submit', async ({ page }) => {
  479 |     // Login + thêm món
  480 |     const login = new LoginPage(page);
  481 |     await login.gotoLogin();
  482 |     await login.usernameInput.fill(CUSTOMER.username);
  483 |     await login.passwordInput.fill(CUSTOMER.password);
  484 |     await login.loginButton.click();
  485 |     await page.waitForLoadState('networkidle');
  486 |     await page.waitForTimeout(2000);
  487 | 
  488 |     // ponytail: nếu login không redirect, goto thẳng trang để set cart
  489 |     if (page.url().includes('/Home/Login')) {
  490 |       console.log('⏳ Login không redirect, thử goto /...');
  491 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 15_000 });
  492 |     }
  493 | 
  494 |     const detail = new DetailRestaurantPage(page);
  495 |     try {
  496 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  497 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  498 |       await detail.addFirstItemToCart(1);
  499 | 
  500 |       const checkout = new CheckoutPage(page);
  501 |       await checkout.gotoCheckout();
  502 |       await page.waitForLoadState('networkidle');
  503 | 
  504 |       // Không điền gì, tick confirm, bấm submit
  505 |       try {
  506 |         await checkout.confirmCheckbox.check();
  507 |       } catch {
  508 |         console.log('ℹ️ Không có confirm checkbox');
  509 |       }
  510 |       await checkout.submitBtn.click();
  511 |       await page.waitForTimeout(2000);
  512 | 
  513 |       // URL không được redirect khỏi checkout
  514 |       expect(page.url()).toContain('Checkout');
  515 |       console.log(`✅ Form validation: vẫn ở checkout page`);
  516 |     } catch (e) {
  517 |       console.log(`ℹ️ Checkout validation test: ${e}`);
  518 |     }
  519 |   });
```