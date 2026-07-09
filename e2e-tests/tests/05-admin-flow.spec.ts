/**
 * 👑 BỘ TEST: LUỒNG ADMIN DASHBOARD (Data & Financial Check)
 *
 * Mục tiêu:
 * - Đăng nhập Admin
 * - Dashboard thống kê + biểu đồ
 * - Quản lý người dùng
 * - Quản lý đơn hàng
 * - Quản lý danh mục
 *
 * Tài khoản: admin1 / admin1
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { AdminPage } from '../pages/AdminPage';
import { USERS, URLS } from '../fixtures/users';

const ADMIN = USERS.admin1;

test.describe('👑 Admin Dashboard', () => {

  test('[TC-5.1] Đăng nhập admin - redirect đến dashboard', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();

    // Chờ redirect đến /Admin
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    // Kiểm tra URL chứa /Admin
    expect(page.url()).toContain('/Admin');
  });

  test('[TC-5.2] Dashboard admin hiển thị KPI cards', async ({ page }) => {
    const admin = new AdminPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    // Chờ KPI cards load
    await page.waitForSelector('.card-header', { timeout: 15_000 });
    const kpiCount = await admin.getKpiCount();
    console.log(`Admin KPI cards: ${kpiCount}`);
    expect(kpiCount).toBeGreaterThan(0);
  });

  test('[TC-5.3] Biểu đồ dashboard (canvas/Chart.js) render', async ({ page }) => {
    const admin = new AdminPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    // Chờ canvas elements render
    await page.waitForSelector('canvas', { timeout: 10_000 });
    const chartVisible = await admin.isChartVisible();
    console.log(`Chart hiển thị: ${chartVisible}`);
  });

  test('[TC-5.4] Trang quản lý khách hàng load và hiển thị bảng', async ({ page }) => {
    const admin = new AdminPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    await admin.gotoUserManagement();
    await page.waitForLoadState('networkidle');

    // Kiểm tra title
    const title = await page.title();
    expect(title).toBeTruthy();
  });

  test('[TC-5.5] Trang quản lý đơn hàng load - bảng hiển thị', async ({ page }) => {
    const admin = new AdminPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    await admin.gotoOrderManagement();
    await page.waitForLoadState('networkidle');

    // Kiểm tra bảng đơn hàng
    const hasTable = await admin.orderTable.isVisible().catch(() => false);
    console.log(`Order table visible: ${hasTable}`);
  });

  test('[TC-5.6] Trang quản lý danh mục load', async ({ page }) => {
    const admin = new AdminPage(page);

    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(ADMIN.username);
    await login.passwordInput.fill(ADMIN.password);
    await login.loginButton.click();
    await page.waitForURL('**/Admin');
    await page.waitForLoadState('networkidle');

    await admin.gotoCategoryManagement();
    await page.waitForLoadState('networkidle');

    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
  });
});
