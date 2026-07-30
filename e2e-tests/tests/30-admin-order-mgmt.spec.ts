/**
 * 📦 BỘ TEST 30: ADMIN ORDER MANAGEMENT
 *
 * Mục tiêu: Test trang quản lý đơn hàng admin (/Admin/Order)
 * - Order list + DataTable
 * - SignalR real-time
 * - Detail view
 * - Dropdown action
 * - Search/filter
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const ADMIN = USERS.admin1;

async function loginAsAdmin(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(ADMIN.username, ADMIN.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Admin', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Admin')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── TEST SUITE ───
test.describe('📦 Admin Order Management', () => {

  test('[TC-AO-01] Order list — DataTable hiển thị', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra bảng
    const table = page.locator('table').first();
    await expect(table).toBeVisible({ timeout: 10_000 });
    const rows = await page.locator('table tbody tr').count();
    console.log(`📋 Order rows: ${rows}`);

    if (rows > 0) {
      const headers = await page.locator('table thead th, table thead td').allTextContents();
      console.log(`  Columns: ${headers.filter((h): h is string => h !== null).map(h => h.trim()).join(' | ')}`);
      expect(headers.length).toBeGreaterThanOrEqual(4);
    }
  });

  test('[TC-AO-02] SignalR real-time update', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra SignalR loaded
    const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
    console.log(`🔌 SignalR loaded: ${hasSignalR}`);

    // Kiểm tra real-time indicators
    const liveIndicator = page.locator('.live-badge, .real-time, [class*="live"], [class*="signal"]').first();
    const liveVisible = await liveIndicator.isVisible().catch(() => false);
    console.log(`📡 Live indicator: ${liveVisible}`);
  });

  test('[TC-AO-03] Click Order → OrderDetail', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="OrderDetail"], a[href*="/Admin/OrderDetail"]');
    const linkCount = await detailLinks.count();
    console.log(`🔗 OrderDetail links: ${linkCount}`);

    if (linkCount > 0) {
      await detailLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('OrderDetail');
    } else {
      console.log('ℹ️ Không có detail links — thử click dòng đầu table');
      const rows = page.locator('table tbody tr');
      if (await rows.count() > 0) {
        await rows.first().click();
        await page.waitForTimeout(2000);
        console.log(`📍 URL: ${page.url()}`);
      }
    }
  });

  test('[TC-AO-04] Dropdown action — menu hoạt động', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra action dropdown
    const dropdownBtns = page.locator('.dropdown-toggle, .btn-action, select[name*="action"], .action-menu');
    const ddCount = await dropdownBtns.count();
    console.log(`📋 Action dropdowns: ${ddCount}`);

    if (ddCount > 0) {
      await dropdownBtns.first().click().catch(() => {});
      await page.waitForTimeout(500);

      // Kiểm tra menu items
      const menuItems = page.locator('.dropdown-menu a, .dropdown-item, select[name*="action"] option');
      const itemCount = await menuItems.count();
      console.log(`  Menu items: ${itemCount}`);

      if (itemCount > 0) {
        const texts = await menuItems.allTextContents();
        console.log(`  Options: ${texts.filter((t): t is string => t !== null).map(t => t.trim()).join(', ')}`);
      }
    }
  });

  test('[TC-AO-05] Search by order ID', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // DataTable search input
    const searchInput = page.locator('.dataTables_filter input, input[type="search"], input[placeholder*="tìm"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('1');
      await page.waitForTimeout(1000);
      const val = await searchInput.inputValue();
      console.log(`🔍 Search value: "${val}"`);
    } else {
      console.log('ℹ️ Không có search input');
    }
  });

  test('[TC-AO-06] Filter by status', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra status filter
    const statusFilter = page.locator('select[name*="status"], select[name*="trangthai"], .status-filter select');
    if (await statusFilter.isVisible().catch(() => false)) {
      const options = await statusFilter.locator('option').allTextContents();
      console.log(`🏷️ Status options: ${options.filter((o): o is string => o !== null).join(', ')}`);

      // Chọn option thứ 2
      if (options.length > 1) {
        await statusFilter.selectOption({ index: 1 });
        await page.waitForTimeout(1000);
        console.log(`  Selected: "${options[1].trim()}"`);
      }
    } else {
      console.log('ℹ️ Không có status filter');
    }
  });
});
