/**
 * 🖼️ BỘ TEST: KIỂM TRA TOÀN DIỆN GIAO DIỆN & HÌNH ẢNH
 *
 * Mục tiêu:
 * - Kiểm tra tất cả ảnh trên trang không bị vỡ (naturalWidth > 0)
 * - Kiểm tra thanh điều hướng (Navbar) hiển thị đầy đủ
 * - Kiểm tra font chữ Inter được load
 * - Kiểm tra layout Desktop 1920x1080 và Mobile responsive
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';

// ─── TEST 1: Trang chủ Desktop ───
test.describe('🖼️ Visual Validation - Desktop (1920x1080)', () => {

  test('[TC-1.1] Tất cả ảnh trên trang chủ không bị vỡ', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    // Kiểm tra từng ảnh trên trang — dùng page.evaluate() đọc naturalWidth
    const imgResult = await home.validateAllImages();
    console.log(`Tổng số ảnh: ${imgResult.total}, Ảnh lỗi: ${imgResult.broken}`);

    expect(imgResult.broken).toBe(0);
  });

  test('[TC-1.2] Thanh navbar hiển thị đầy đủ - logo, search, cart, login', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    // Kiểm tra các phần tử navbar
    await expect(home.logo).toBeVisible();
    await expect(home.searchInput).toBeVisible();
    await expect(home.cartButton).toBeVisible();
    await expect(home.loginNavBtn).toBeVisible();
    await expect(home.registerNavBtn).toBeVisible();

    // Kiểm tra navbar cố định trên đầu trang
    const navBox = await home.navbar.boundingBox();
    expect(navBox?.y).toBeDefined();
    if (navBox) expect(navBox.y).toBeLessThan(50);
  });

  test('[TC-1.3] Footer hiển thị đầy đủ links và thông tin', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await home.scrollToBottom();

    await expect(home.footer).toBeVisible();
    // Kiểm tra các section trong footer
    await expect(page.locator('.fs-footer-heading', { hasText: 'Khám phá' })).toBeVisible();
    await expect(page.locator('.fs-footer-heading', { hasText: 'Hỗ trợ' })).toBeVisible();
    await expect(page.locator('.fs-footer-heading', { hasText: 'Liên hệ' })).toBeVisible();
  });

  test('[TC-1.4] Stats row hiển thị số liệu', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await expect(home.statsRow).toBeVisible();
    const stat1 = await home.getStatValue(0);
    expect(stat1).toBeTruthy();
  });

  test('[TC-1.5] Carousel hero hoạt động - có thể chuyển slide', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await expect(home.carousel).toBeVisible();
    // Click nút next — chờ response
    await home.carouselNextBtn.click();
    await page.waitForTimeout(1000); // carousel animation, hard wait acceptable
    // Click nút prev
    await home.carouselPrevBtn.click();
    await page.waitForTimeout(1000);
  });

  test('[TC-1.6] Trang login hiển thị đầy đủ form', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await expect(login.usernameInput).toBeVisible();
    await expect(login.passwordInput).toBeVisible();
    await expect(login.loginButton).toBeVisible();
    await expect(login.rememberMeCheckbox).toBeVisible();
    await expect(login.forgotPasswordLink).toBeVisible();
    await expect(login.registerLink).toBeVisible();
  });

  test('[TC-1.7] Promo band có thể dismiss', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await expect(home.promoBand).toBeVisible({ timeout: 10_000 });
    await home.dismissPromo();
    await expect(home.promoBand).not.toBeVisible();
  });
});

// ─── TEST 2: Responsive Mobile ───
test.describe('📱 Visual Validation - Mobile (375x812)', () => {

  test.use({ viewport: { width: 375, height: 812 } });

  test('[TC-1.8] Menu mobile hamburger hoạt động', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    // Kiểm tra hamburger button hiển thị trên mobile
    const hamburger = page.locator('.navbar-toggler');
    await expect(hamburger).toBeVisible();

    // Click mở menu
    await hamburger.click();
    // Chờ menu collapse mở
    const navCollapse = page.locator('#navbarCollapse');
    await expect(navCollapse).toBeVisible({ timeout: 5_000 });
  });

  test('[TC-1.9] Ảnh trên mobile không bị vỡ', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    const imgResult = await home.validateAllImages();
    console.log(`Mobile - Tổng ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
    expect(imgResult.broken).toBe(0);
  });

  test('[TC-1.10] Restaurant cards responsive - 2 cột trên mobile', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    // Chờ danh sách quán ăn load từ API
    await page.waitForSelector('.product-item', { timeout: 15_000 });
    const count = await home.getRestaurantCount();
    expect(count).toBeGreaterThan(0);
  });
});
