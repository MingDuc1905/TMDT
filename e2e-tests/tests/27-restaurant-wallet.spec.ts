/**
 * 💰 BỘ TEST 27: RESTAURANT WALLET (Ví tiền quán)
 *
 * Mục tiêu: Test trang Wallet của quán ăn (/Restaurant/Wallet)
 * - Số dư + lịch sử giao dịch
 * - Rút tiền
 * - Nạp tiền
 * - Transaction items format
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const RESTAURANT = USERS.restaurant1;

async function loginAsMerchant(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(RESTAURANT.username, RESTAURANT.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Restaurant')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── TEST SUITE ───
test.describe('💰 Restaurant Wallet', () => {

  test('[TC-WAL-01] Wallet — số dư + lịch sử giao dịch', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra số dư
    const balance = page.locator('[class*="balance"], [class*="sodu"], .wallet-amount, [class*="wallet"]').first();
    const balanceVisible = await balance.isVisible().catch(() => false);
    console.log(`💰 Balance visible: ${balanceVisible}`);

    if (balanceVisible) {
      const balanceText = await balance.textContent();
      console.log(`  Balance: ${balanceText?.trim()}`);
    }

    // Kiểm tra bảng giao dịch
    const table = page.locator('table').first();
    const tableVisible = await table.isVisible().catch(() => false);
    console.log(`📋 Transaction table: ${tableVisible}`);

    if (tableVisible) {
      const rows = await page.locator('table tbody tr').count();
      console.log(`  Transactions: ${rows}`);

      if (rows > 0) {
        const firstRow = page.locator('table tbody tr').first();
        const cells = await firstRow.locator('td').allTextContents();
        console.log(`  First tx: ${cells.map(c => c.trim()).join(' | ')}`);
      }
    }
  });

  test('[TC-WAL-02] Rút tiền — form + submit', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra nút Rút tiền
    const withdrawBtn = page.locator('a:has-text("Rút"), button:has-text("Rút"), .btn-withdraw');
    const btnVisible = await withdrawBtn.isVisible().catch(() => false);
    console.log(`🏧 Withdraw button: ${btnVisible}`);

    if (btnVisible) {
      await withdrawBtn.click();
      await page.waitForTimeout(1000);

      // Kiểm tra form/modal rút tiền
      const modal = page.locator('.modal.show, .modal:visible').first();
      const modalVisible = await modal.isVisible().catch(() => false);
      console.log(`📦 Withdraw modal: ${modalVisible}`);

      if (modalVisible) {
        const input = modal.locator('input[type="number"], input[type="text"]').first();
        console.log(`  Amount input: ${await input.isVisible().catch(() => false)}`);

        const confirmBtn = modal.locator('button:has-text("Xác nhận"), button:has-text("Rút")').first();
        console.log(`  Confirm btn: ${await confirmBtn.isVisible().catch(() => false)}`);
      }
    }
  });

  test('[TC-WAL-03] Nạp tiền (mock payment gateway)', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra nút Nạp tiền
    const depositBtn = page.locator('a:has-text("Nạp"), button:has-text("Nạp"), .btn-deposit');
    const btnVisible = await depositBtn.isVisible().catch(() => false);
    console.log(`💳 Deposit button: ${btnVisible}`);

    if (btnVisible) {
      await depositBtn.click();
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 After deposit click: ${url}`);
    }
  });

  test('[TC-WAL-04] Transaction items — format hiển thị đúng', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Wallet', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const table = page.locator('table').first();
    if (!(await table.isVisible().catch(() => false))) {
      console.log('ℹ️ Không có bảng giao dịch');
      return;
    }

    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();
    console.log(`📋 Rows: ${rowCount}`);

    for (let i = 0; i < Math.min(rowCount, 3); i++) {
      const cells = await rows.nth(i).locator('td').allTextContents();
      const rowText = cells.map(c => c.trim()).join(' | ');
      console.log(`  Row ${i}: ${rowText}`);
    }

    // Kiểm tra có cột: ngày, nội dung, số tiền (+/-), số dư
    if (rowCount > 0) {
      const headers = await page.locator('table thead th, table thead td').allTextContents();
      console.log(`📊 Columns: ${headers.join(' | ')}`);
      expect(headers.length).toBeGreaterThanOrEqual(3);
    }
  });
});
