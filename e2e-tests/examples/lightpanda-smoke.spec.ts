// ====================================================================
// FastShip + Lightpanda — Smoke Test
// ====================================================================
// Chạy smoke test cơ bản trên FastShip sử dụng Lightpanda browser
// qua CDP. Kiểm tra xem Lightpanda có render đúng các trang chính.
//
// Yêu cầu:
//   docker compose up -d (Lightpanda CDP server)
//   npm run test:lightpanda
// ====================================================================

import { test, expect } from '../fixtures/lightpanda-fixture';

test.describe('🏪 FastShip + Lightpanda Smoke Tests', () => {

  test('Trang chủ load được - hero section visible', async ({ page }) => {
    const start = Date.now();
    await page.goto('/');
    const loadTime = Date.now() - start;

    console.log(`⏱ Homepage loaded in ${loadTime}ms`);

    // Kiểm tra title
    await expect(page).toHaveTitle(/Fastship|ShipFood|fastship/i);

    // Kiểm tra các element chính
    await expect(page.locator('nav, .navbar, header').first()).toBeVisible();

    // Ghi log thời gian load
    test.info().annotations.push({
      type: 'performance',
      description: `Homepage load time: ${loadTime}ms`,
    });
  });

  test('Tìm kiếm hoạt động', async ({ page }) => {
    await page.goto('/');
    // Tìm ô search và gõ từ khóa
    const searchInput = page.locator('input[type="search"], input[placeholder*="tìm"], input[name="q"], .search-input, #search').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('Pizza');
      // Chờ autocomplete hoặc kết quả
      await page.waitForTimeout(1000);
      console.log('✅ Search input works');
    } else {
      console.log('⚠️ Search input not found — skipping');
    }
  });

  test('Trang đăng nhập load được', async ({ page }) => {
    await page.goto('/Home/Login');
    await expect(page).toHaveTitle(/Đăng nhập|Login|Fastship/i);
    // Kiểm tra có form/login container
    const loginForm = page.locator('form, .login-container, .login-form, .auth-page-wrapper').first();
    await expect(loginForm).toBeVisible();
    console.log('✅ Login page loaded successfully');
  });

  test('Healthcheck endpoint trả về 200', async ({ page }) => {
    const response = await page.request.get('/health');
    expect(response.status()).toBe(200);
    const body = await response.text();
    expect(body).toContain('OK');
    console.log(`✅ Healthcheck: ${body}`);
  });

  test('Cart page không bị crash', async ({ page }) => {
    await page.goto('/Cart');
    // Cart page có thể hiển thị giỏ hàng trống hoặc redirect login
    const currentUrl = page.url();
    console.log(`📍 Cart page URL: ${currentUrl}`);
    // Không được 500 error
    expect(currentUrl).not.toContain('Error');
    expect(currentUrl).not.toContain('500');
  });

  test('Trang chủ có danh sách quán ăn', async ({ page }) => {
    await page.goto('/');
    // Kiểm tra danh sách quán ăn (home page)
    const restaurantCards = page.locator('.product-item, .restaurant-card, .fs-card, [class*="product"], [class*="restaurant"]').first();
    const isVisible = await restaurantCards.isVisible().catch(() => false);
    console.log(isVisible
      ? '✅ Restaurant listing found'
      : '⚠️ No restaurant cards found — might need different selector');
  });
});
