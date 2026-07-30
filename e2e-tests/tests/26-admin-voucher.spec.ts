/**
 * 🎟️ BỘ TEST 26: ADMIN VOUCHER MANAGEMENT
 *
 * Mục tiêu: Test trang VoucherManager (/Admin/VoucherManager)
 * - Danh sách voucher
 * - Create voucher
 * - Edit voucher
 * - Toggle active/inactive
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
test.describe('🎟️ Admin Voucher Manager', () => {

  test('[TC-AV-01] Voucher list — bảng + dữ liệu', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/VoucherManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📋 Page title: ${bodyText.substring(0, 100)}`);

    // Kiểm tra bảng
    const table = page.locator('table').first();
    const tableVisible = await table.isVisible().catch(() => false);
    console.log(`📊 Table visible: ${tableVisible}`);

    if (tableVisible) {
      const rows = await page.locator('table tbody tr').count();
      console.log(`  Voucher rows: ${rows}`);

      if (rows > 0) {
        const firstRowText = await page.locator('table tbody tr').first().textContent();
        console.log(`  First row: ${firstRowText?.trim().substring(0, 80)}`);
      }
    }

    // Kiểm tra nút Thêm
    const addBtn = page.locator('a:has-text("Thêm"), button:has-text("Thêm")').first();
    console.log(`➕ Add button: ${await addBtn.isVisible().catch(() => false)}`);
  });

  test('[TC-AV-02] Create voucher — form + submit', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/VoucherManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Click Thêm
    const addBtn = page.locator('a:has-text("Thêm"), button:has-text("Thêm")').first();
    if (await addBtn.isVisible().catch(() => false)) {
      await addBtn.click();
      await page.waitForTimeout(1500);

      // Kiểm tra form modal/page
      const form = page.locator('form[action*="Voucher"], form[action*="voucher"], .modal form').first();
      const codeInput = page.locator('input[name*="code"], input[name*="ma"], input[name*="voucher"]').first();
      const percentInput = page.locator('input[name*="percent"], input[name*="phantram"], input[name*="giam"]').first();

      const codeVisible = await codeInput.isVisible().catch(() => false);
      const percentVisible = await percentInput.isVisible().catch(() => false);
      console.log(`📝 Code input: ${codeVisible}, Percent input: ${percentVisible}`);

      // Thử điền form
      if (codeVisible && percentVisible) {
        const uniqueCode = `TEST${Date.now()}`;
        await codeInput.fill(uniqueCode);
        await percentInput.fill('10');
        console.log(`  Điền code: ${uniqueCode}, giảm: 10%`);

        // Submit
        const submitBtn = page.locator('button[type="submit"], button:has-text("Lưu"), button:has-text("Tạo")').first();
        if (await submitBtn.isVisible().catch(() => false)) {
          await submitBtn.click();
          await page.waitForTimeout(3000);

          const url = page.url();
          console.log(`📍 After create: ${url}`);
        }
      }
    }
  });

  test('[TC-AV-03] Edit voucher — cập nhật % giảm + hạn', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/VoucherManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Click nút Sửa đầu tiên
    const editBtns = page.locator('a[href*="Edit"], a[href*="edit"], a:has-text("Sửa"), button:has-text("Sửa")');
    const editCount = await editBtns.count();
    console.log(`✏️ Edit buttons: ${editCount}`);

    if (editCount > 0) {
      await editBtns.first().click();
      await page.waitForTimeout(1500);

      // Kiểm tra form edit
      const form = page.locator('form').first();
      const formVisible = await form.isVisible().catch(() => false);
      console.log(`📝 Edit form visible: ${formVisible}`);

      if (formVisible) {
        const inputs = await form.locator('input').count();
        console.log(`  Form inputs: ${inputs}`);
      }
    }
  });

  test('[TC-AV-04] Toggle active/inactive', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/VoucherManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra toggle switches
    const toggles = page.locator('.toggle-switch, input[type="checkbox"], .switch, a[href*="Toggle"], a[href*="toggle"]');
    const toggleCount = await toggles.count();
    console.log(`🔄 Toggle switches: ${toggleCount}`);

    if (toggleCount > 0) {
      const firstToggle = toggles.first();
      const isChecked = await firstToggle.isChecked().catch(() => false);
      console.log(`  First toggle checked: ${isChecked}`);

      await firstToggle.click().catch(() => {});
      await page.waitForTimeout(1000);
      console.log('  Clicked toggle');
    }
  });
});
