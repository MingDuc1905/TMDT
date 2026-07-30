/**
 * 👑 BỘ TEST 37: ADMIN REMAINING — EditOrder, WalletManager, EditCategory
 *
 * Mục tiêu: Test các trang admin còn thiếu
 * - EditOrder: sửa đơn hàng (form, status change)
 * - WalletManager: quản lý ví admin
 * - EditCategory: sửa danh mục
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

// ─── EDIT ORDER ───
test.describe('📝 Admin EditOrder', () => {

  test('[TC-ADM-01] EditOrder page load — form fields', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/EditOrder?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 120)}`);

    // Kiểm tra form
    const form = page.locator('form').first();
    const formVisible = await form.isVisible().catch(() => false);
    console.log(`📝 Form: ${formVisible}`);

    if (formVisible) {
      const inputs = await form.locator('input, select, textarea').count();
      console.log(`  Fields: ${inputs}`);

      // Kiểm tra các field chính
      const statusSelect = form.locator('select[name*="status"], select[name*="trangthai"]').first();
      console.log(`  Status select: ${await statusSelect.isVisible().catch(() => false)}`);

      // Submit button
      const submitBtn = form.locator('button[type="submit"]').first();
      console.log(`  Submit: ${await submitBtn.isVisible().catch(() => false)}`);
    }
  });

  test('[TC-ADM-02] EditOrder — link từ Order List', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Order', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const editLinks = page.locator('a[href*="EditOrder"], a[href*="edit-order"]');
    const linkCount = await editLinks.count();
    console.log(`🔗 Edit links in Order List: ${linkCount}`);

    if (linkCount > 0) {
      await editLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('EditOrder');
    }
  });

  test('[TC-ADM-03] EditOrder — navigate from OrderDetail', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/OrderDetail?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra nút "Sửa" / "Edit" trên OrderDetail
    const editBtn = page.locator('a[href*="EditOrder"], button:has-text("Sửa"), a:has-text("Sửa")').first();
    const btnVisible = await editBtn.isVisible().catch(() => false);
    console.log(`✏️ Edit button on OrderDetail: ${btnVisible}`);
  });

  test('[TC-ADM-04] EditOrder — API submit form (thử)', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/EditOrder?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const form = page.locator('form').first();
    if (!(await form.isVisible().catch(() => false))) {
      console.log('ℹ️ No edit form');
      return;
    }

    const statusSelect = form.locator('select[name*="status"], select[name*="trangthai"]').first();
    if (await statusSelect.isVisible().catch(() => false)) {
      // Chọn trạng thái khác (nếu có)
      const options = await statusSelect.locator('option').allTextContents();
      console.log(`  Status options: ${options.map(o => o.trim()).join(', ')}`);

      if (options.length > 1) {
        await statusSelect.selectOption({ index: 1 });
        await page.waitForTimeout(300);

        const submitBtn = form.locator('button[type="submit"]').first();
        if (await submitBtn.isVisible().catch(() => false)) {
          await submitBtn.click();
          await page.waitForTimeout(2000);
          console.log(`📍 After submit: ${page.url()}`);
        }
      }
    }
  });
});

// ─── WALLET MANAGER ───
test.describe('💰 Admin WalletManager', () => {

  test('[TC-ADM-05] WalletManager page load', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/WalletManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page title: ${bodyText.substring(0, 100)}`);

    // Kiểm tra bảng
    const table = page.locator('table').first();
    const tableVisible = await table.isVisible().catch(() => false);
    console.log(`📊 Table: ${tableVisible}`);

    if (tableVisible) {
      const rows = await page.locator('table tbody tr').count();
      console.log(`  Rows: ${rows}`);

      if (rows > 0) {
        const headers = await page.locator('table thead th').allTextContents();
        console.log(`  Columns: ${headers.filter((h): h is string => h !== null).map(h => h.trim()).join(', ')}`);
      }
    }

    // Kiểm tra balance
    const balance = page.locator('[class*="balance"], [class*="total"], .wallet-summary').first();
    console.log(`💰 Balance: ${await balance.isVisible().catch(() => false)}`);
  });

  test('[TC-ADM-06] WalletManager — nút thao tác chính', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/WalletManager', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra các nút
    const findBtns = [
      'a:has-text("Cộng tiền"), button:has-text("Cộng tiền")',
      'a:has-text("Trừ tiền"), button:has-text("Trừ tiền")',
      'a[href*="Export"], button:has-text("Xuất")',
    ];

    for (const selector of findBtns) {
      const btn = page.locator(selector).first();
      const visible = await btn.isVisible().catch(() => false);
      if (visible) {
        const text = await btn.textContent();
        console.log(`  Button: ${text?.trim()}`);
      }
    }

    // Kiểm tra sidebar nav link
    const navLink = page.locator('a[href*="WalletManager"]').first();
    console.log(`🔗 Nav link: ${await navLink.isVisible().catch(() => false)}`);
  });
});

// ─── EDIT CATEGORY ───
test.describe('📂 Admin EditCategory', () => {

  test('[TC-ADM-07] EditCategory page — form load', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/EditCategory?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    const form = page.locator('form').first();
    const formVisible = await form.isVisible().catch(() => false);
    console.log(`📝 Form: ${formVisible}`);

    if (formVisible) {
      const inputs = await form.locator('input').count();
      console.log(`  Inputs: ${inputs}`);

      const nameInput = page.locator('input[name*="name"], input[name*="ten"]').first();
      console.log(`  Name: ${await nameInput.isVisible().catch(() => false)}`);

      if (await nameInput.isVisible().catch(() => false)) {
        const currentValue = await nameInput.inputValue();
        console.log(`  Current value: "${currentValue}"`);
      }

      const submitBtn = form.locator('button[type="submit"]').first();
      console.log(`  Submit: ${await submitBtn.isVisible().catch(() => false)}`);
    }
  });

  test('[TC-ADM-08] EditCategory — navigate from Category list', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/Category', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const editLinks = page.locator('a[href*="EditCategory"]');
    const linkCount = await editLinks.count();
    console.log(`🔗 Edit category links: ${linkCount}`);

    if (linkCount > 0) {
      await editLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('EditCategory');
    }
  });

  test('[TC-ADM-09] EditCategory — file upload cho icon', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/EditCategory?id=1', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const fileInput = page.locator('input[type="file"]').first();
    const fileVisible = await fileInput.isVisible().catch(() => false);
    console.log(`📁 File upload: ${fileVisible}`);

    const previewImg = page.locator('img[src*="danhmuc"], img[src*="Category"]').first();
    console.log(`🖼️ Preview: ${await previewImg.isVisible().catch(() => false)}`);
  });
});

// ─── POST TAI KHOAN ───
test.describe('👤 Admin PostTaiKhoan (Account Management)', () => {

  test('[TC-ADM-10] PostTaiKhoan page load', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/Admin/PostTaiKhoan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    const form = page.locator('form').first();
    const formVisible = await form.isVisible().catch(() => false);
    console.log(`📝 Form: ${formVisible}`);

    if (formVisible) {
      const inputs = await form.locator('input, select').count();
      console.log(`  Fields: ${inputs}`);
    }
  });
});
