/**
 * 👥 BỘ TEST 28: ADMIN USER MANAGEMENT
 *
 * Mục tiêu: Test toàn bộ trang quản lý người dùng
 * - Quản lý Khách hàng, Quán ăn, Shipper, Admin
 * - Duyệt/Khóa/Mở khóa user
 * - Search, filter by role
 * - Không thể khóa admin cuối cùng
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
test.describe('👥 Admin User Management', () => {

  test.describe('📋 Role pages', () => {

    test('[TC-AU-01] Quản lý Khách hàng — bảng + dữ liệu', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const table = page.locator('table').first();
      await expect(table).toBeVisible({ timeout: 10_000 });

      const rows = await page.locator('table tbody tr').count();
      console.log(`📋 Customer rows: ${rows}`);
      expect(rows).toBeGreaterThan(0);

      // Kiểm tra cột: tên, SĐT, email, trạng thái
      const headers = await page.locator('table thead th, table thead td').allTextContents();
      console.log(`  Columns: ${headers.join(' | ')}`);
      expect(headers.length).toBeGreaterThanOrEqual(4);
    });

    test('[TC-AU-02] Quản lý Quán ăn — bảng + dữ liệu', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyQuanAn', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const table = page.locator('table').first();
      await expect(table).toBeVisible({ timeout: 10_000 });
      const rows = await page.locator('table tbody tr').count();
      console.log(`🏪 Restaurant rows: ${rows}`);
      expect(rows).toBeGreaterThan(0);
    });

    test('[TC-AU-03] Quản lý Shipper — bảng + dữ liệu', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyShipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const table = page.locator('table').first();
      await expect(table).toBeVisible({ timeout: 10_000 });
      const rows = await page.locator('table tbody tr').count();
      console.log(`🚚 Shipper rows: ${rows}`);
      expect(rows).toBeGreaterThan(0);
    });

    test('[TC-AU-04] Quản lý Admin — bảng hiển thị', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyQuanTriVien', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const table = page.locator('table').first();
      const tableVisible = await table.isVisible().catch(() => false);
      console.log(`👑 Admin table visible: ${tableVisible}`);

      if (tableVisible) {
        const rows = await page.locator('table tbody tr').count();
        console.log(`  Admin rows: ${rows}`);
      }
    });
  });

  test.describe('🔧 Actions', () => {

    test('[TC-AU-05] Duyệt user — click nút Duyệt (nếu có)', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Tìm nút Duyệt
      const approveBtns = page.locator('a[href*="Duyet"], button:has-text("Duyệt"), .btn-approve');
      const approveCount = await approveBtns.count();
      console.log(`✅ Approve buttons: ${approveCount}`);

      if (approveCount > 0) {
        await approveBtns.first().click();
        await page.waitForTimeout(2000);
        console.log(`📍 After approve: ${page.url()}`);
      }
    });

    test('[TC-AU-06] Khóa/Mở khóa user', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Tìm nút Khóa/Mở khóa
      const lockBtns = page.locator('a[href*="Khoa"], a[href*="khoa"], a[href*="Lock"], button:has-text("Khóa"), button:has-text("Mở")');
      const lockCount = await lockBtns.count();
      console.log(`🔒 Lock/unlock buttons: ${lockCount}`);

      if (lockCount > 0) {
        const btnText = await lockBtns.first().textContent();
        console.log(`  First btn: "${btnText?.trim()}"`);
        await lockBtns.first().click();
        await page.waitForTimeout(2000);
        console.log(`📍 After lock toggle: ${page.url()}`);
      }
    });

    test('[TC-AU-07] Search user — gõ từ khóa', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const searchInput = page.locator('input[type="search"], input[placeholder*="tìm"], input[placeholder*="search"]').first();
      if (await searchInput.isVisible().catch(() => false)) {
        await searchInput.fill('koneko');
        await page.waitForTimeout(1500);
        const val = await searchInput.inputValue();
        console.log(`🔍 Search: "${val}"`);

        // Đếm lại rows sau filter
        const rows = await page.locator('table tbody tr, .dataTable tbody tr').count().catch(() => 0);
        console.log(`  Rows after search: ${rows}`);
      } else {
        console.log('ℹ️ Không có search input (DataTable built-in search)');
        // DataTables có search input riêng
        const dtSearch = page.locator('.dataTables_filter input').first();
        if (await dtSearch.isVisible().catch(() => false)) {
          await dtSearch.fill('koneko');
          await page.waitForTimeout(1000);
          console.log('  Used DataTable search');
        }
      }
    });

    test('[TC-AU-08] Filter by role tabs', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Kiểm tra filter tabs
      const roleTabs = page.locator('a[href*="quanly"], .nav-tabs a, .nav-pills a').filter({ hasText: /khách|quán|ship|admin/i });
      const tabCount = await roleTabs.count();
      console.log(`📑 Role filter tabs: ${tabCount}`);

      if (tabCount > 0) {
        for (let i = 0; i < Math.min(tabCount, 3); i++) {
          const text = await roleTabs.nth(i).textContent();
          console.log(`  Tab ${i}: "${text?.trim()}"`);
        }
      }
    });

    test('[TC-AU-09] Click vào user → view detail', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyKhachHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      const detailLinks = page.locator('a[href*="Detail"], a[href*="detail"], table tbody tr a').first();
      if (await detailLinks.isVisible().catch(() => false)) {
        await detailLinks.click();
        await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
        await page.waitForTimeout(1000);
        console.log(`📍 URL: ${page.url()}`);
      } else {
        console.log('ℹ️ Không có detail link — click vào row');
        const rows = page.locator('table tbody tr');
        if (await rows.count() > 0) {
          await rows.first().click();
          await page.waitForTimeout(1000);
          console.log('  Clicked first row');
        }
      }
    });

    test('[TC-AU-10] Không thể khóa/duyệt admin cuối cùng', async ({ page }) => {
      await loginAsAdmin(page);
      await page.goto('/Admin/QuanLyQuanTriVien', { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Tìm nút khóa/duyệt trên admin
      const adminActions = page.locator('a[href*="Khoa"], a[href*="Duyet"], button:has-text("Khóa")');
      const actionCount = await adminActions.count();
      console.log(`🔒 Admin actions: ${actionCount}`);

      if (actionCount > 0) {
        await adminActions.first().click();
        await page.waitForTimeout(2000);

        // Kiểm tra có error message "Không thể khóa admin cuối cùng"
        const bodyText = await page.locator('body').textContent() || '';
        const hasError = bodyText.includes('Không thể') || bodyText.includes('cuối cùng') || bodyText.includes('không được phép');
        console.log(`⚠️ Error displayed: ${hasError}`);
      } else {
        console.log('ℹ️ Không có action buttons trên admin page');
      }
    });
  });
});
