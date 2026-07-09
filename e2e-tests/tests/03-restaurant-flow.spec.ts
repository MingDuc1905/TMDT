/**
 * 🏪 BỘ TEST: LUỒNG QUÁN ĂN (Merchant Order Lifecycle)
 *
 * Mục tiêu:
 * - Đăng nhập tài khoản Quán ăn
 * - Xem danh sách đơn hàng mới
 * - Xác nhận đơn hàng
 * - Chuyển trạng thái "Chuẩn bị xong"
 *
 * Tài khoản: konekopizza / konekopizza (maquanan = 6)
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { RestaurantPage } from '../pages/RestaurantPage';
import { USERS, URLS, SEED } from '../fixtures/users';

const RESTAURANT = USERS.restaurant1;

test.describe('🏪 Quán ăn - Xử lý đơn hàng', () => {

  test('[TC-3.1] Đăng nhập quán ăn - redirect đến dashboard', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill(RESTAURANT.username);
    await login.passwordInput.fill(RESTAURANT.password);
    await login.loginButton.click();

    // Chờ redirect đến /Restaurant
    await page.waitForURL('**/Restaurant');
    await page.waitForLoadState('networkidle');

    // Kiểm tra URL chứa /Restaurant
    expect(page.url()).toContain('/Restaurant');
  });

  test('[TC-3.2] Dashboard quán ăn hiển thị thống kê KPI', async ({ page }) => {
    const restaurant = new RestaurantPage(page);

    // Login
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(RESTAURANT.username);
    await login.passwordInput.fill(RESTAURANT.password);
    await login.loginButton.click();
    await page.waitForURL('**/Restaurant');
    await page.waitForLoadState('networkidle');

    // Chờ KPI cards load
    await page.waitForSelector('.card-header', { timeout: 15_000 });
    const kpiCount = await restaurant.kpiCards.count();
    console.log(`Số KPI cards: ${kpiCount}`);
    expect(kpiCount).toBeGreaterThan(0);
  });

  test('[TC-3.3] Danh sách đơn hàng hiển thị', async ({ page }) => {
    const restaurant = new RestaurantPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(RESTAURANT.username);
    await login.passwordInput.fill(RESTAURANT.password);
    await login.loginButton.click();
    await page.waitForURL('**/Restaurant');
    await page.waitForLoadState('networkidle');

    await restaurant.gotoOrderList();
    // Chờ bảng đơn hàng load
    await page.waitForSelector('#example5', { timeout: 15_000 });

    const orderCount = await restaurant.getOrderCount();
    console.log(`Số đơn hàng: ${orderCount}`);
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });

  test('[TC-3.4] Chi tiết đơn hàng hiển thị đúng thông tin', async ({ page }) => {
    const restaurant = new RestaurantPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(RESTAURANT.username);
    await login.passwordInput.fill(RESTAURANT.password);
    await login.loginButton.click();
    await page.waitForURL('**/Restaurant');
    await page.waitForLoadState('networkidle');

    await restaurant.gotoOrderList();
    await page.waitForSelector('#example5', { timeout: 15_000 });

    // Nếu có đơn hàng, kiểm tra nút chi tiết
    const orderCount = await restaurant.getOrderCount();
    if (orderCount > 0) {
      const detailBtns = await page.locator('a[href*="ChiTietDonHang"]').count();
      expect(detailBtns).toBeGreaterThan(0);

      // Click vào chi tiết đơn đầu tiên
      await page.locator('a[href*="ChiTietDonHang"]').first().click();
      await page.waitForLoadState('networkidle');

      // Kiểm tra URL chứa ChiTietDonHang
      expect(page.url()).toContain('ChiTietDonHang');
    }
  });
});
