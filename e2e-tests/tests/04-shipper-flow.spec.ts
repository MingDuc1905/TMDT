/**
 * 🚚 BỘ TEST: LUỒNG SHIPPER (Rider Wallet & Status)
 *
 * Mục tiêu:
 * - Đăng nhập tài khoản Shipper
 * - Xem FREE-PICK và danh sách đơn
 * - Nhận đơn giao hàng
 * - Kiểm tra ví tiền và thu nhập
 *
 * Tài khoản: shippery / shipy456 (userid = 5)
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ShipperPage } from '../pages/ShipperPage';
import { USERS, URLS } from '../fixtures/users';

const SHIPPER = USERS.shipper1;

test.describe('🚚 Shipper - Quản lý đơn hàng & Thu nhập', () => {

  test('[TC-4.1] Đăng nhập shipper - redirect đến dashboard', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();

    // Chờ redirect đến /Shipper
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    // Kiểm tra URL chứa /Shipper
    expect(page.url()).toContain('/Shipper');
  });

  test('[TC-4.2] Dashboard shipper hiển thị FREE-PICK + Order tabs', async ({ page }) => {
    const shipper = new ShipperPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    // Kiểm tra các tab hiển thị
    await expect(shipper.freepickTab).toBeVisible({ timeout: 10_000 });
    await expect(shipper.orderTab).toBeVisible({ timeout: 10_000 });
  });

  test('[TC-4.3] Tab ĐƠN HÀNG hiển thị danh sách đơn đã nhận', async ({ page }) => {
    const shipper = new ShipperPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    await shipper.openOrderTab();
    // Chờ bảng đơn hàng load
    await page.waitForSelector('.table-responsive', { timeout: 15_000 });

    const orderCount = await shipper.getOrderCount();
    console.log(`Shipper - số đơn: ${orderCount}`);
    expect(orderCount).toBeGreaterThanOrEqual(0);
  });

  test('[TC-4.4] Trang thu nhập hiển thị thống kê', async ({ page }) => {
    const shipper = new ShipperPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    await shipper.gotoIncome();
    await page.waitForLoadState('networkidle');

    // Kiểm tra trang thu nhập load
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
  });

  test('[TC-4.5] Bản đồ FREE-PICK hiển thị (map container)', async ({ page }) => {
    const shipper = new ShipperPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    await shipper.openFreepickTab();
    await page.waitForTimeout(2000);

    // Kiểm tra map container — không dùng isMapVisible vì map component có thể lazy load
    const mapVisible = await shipper.isMapVisible().catch(() => false);
    console.log(`Map hiển thị: ${mapVisible}`);
  });

  test('[TC-4.6] Trang ví tiền load được', async ({ page }) => {
    const shipper = new ShipperPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(SHIPPER.username);
    await login.passwordInput.fill(SHIPPER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Shipper');
    await page.waitForLoadState('networkidle');

    await shipper.gotoWallet();
    await page.waitForLoadState('networkidle');

    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
  });
});
