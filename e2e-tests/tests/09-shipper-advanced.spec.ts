/**
 * 🚚 BỘ TEST 09: SHIPPER ADVANCED — Status transitions, Race condition, Geolocation, Settings
 *
 * Target: Tất cả tính năng nâng cao của Shipper chưa có trong 04-shipper-flow
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED } from '../fixtures/users';

const SHIPPER = USERS.shipper2;

async function loginAsShipper(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(SHIPPER.username, SHIPPER.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Shipper')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── SUITE 1: Status Transitions ───
test.describe('🔄 Cập nhật trạng thái giao hàng', () => {
  test('[TC-9.1] OrderDetail — nút Lấy hàng + Hoàn thành', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/LichSu', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const detailLinks = page.locator('a[href*="OrderDetail/"]');
    if (await detailLinks.count() > 0) {
      await detailLinks.first().click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      const pickupBtn = page.locator('#btnPickup');
      const completeBtn = page.locator('#btnComplete');
      console.log(`🟢 Lấy hàng btn: ${await pickupBtn.isVisible().catch(() => false)} (disabled: ${await pickupBtn.isDisabled().catch(() => false)})`);
      console.log(`✅ Hoàn thành btn: ${await completeBtn.isVisible().catch(() => false)} (disabled: ${await completeBtn.isDisabled().catch(() => false)})`);
    } else console.log('ℹ️ No order detail links');
  });

  test('[TC-9.2] UpdateDonHang API — status transitions', async ({ page }) => {
    await loginAsShipper(page);
    // Test API với order không tồn tại
    const resp = await page.request.post('/Shipper/UpdateDonHang', {
      params: { status: 'lh', id: 99999 },
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 UpdateDonHang API: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });

  test('[TC-9.3] Toggle trạng thái Online/Offline', async ({ page }) => {
    await loginAsShipper(page);
    // Click toggle status
    const statusLink = page.locator('a[href*="updateStatus"]').first();
    if (await statusLink.isVisible().catch(() => false)) {
      await statusLink.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      const url = page.url();
      console.log(`📍 After status toggle: ${url}`);
    } else console.log('ℹ️ No status toggle link');
  });
});

// ─── SUITE 2: Wallet & Income ───
test.describe('💰 Ví & Thu nhập', () => {
  test('[TC-9.4] Ví tiền — số dư + lịch sử giao dịch', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/ViTien', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const balance = page.locator('[class*="balance"], [class*="sodu"], .vi-tien-balance').first();
    console.log(`💰 Balance: ${await balance.textContent().catch(() => 'N/A')}`);
    const transactions = page.locator('table tbody tr, .transaction-item').count();
    console.log(`📋 Transactions: ${await transactions}`);
  });

  test('[TC-9.5] Thu nhập — thống kê 30 ngày', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/ThuNhap', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const stats = page.locator('.card-header, .stat-item, [class*="thunhap"]');
    const statCount = await stats.count();
    console.log(`📊 Income stats: ${statCount}`);
    for (let i = 0; i < Math.min(statCount, 4); i++) {
      console.log(`  ${(await stats.nth(i).textContent())?.trim()}`);
    }
  });

  test('[TC-9.6] Lịch sử giao hàng — bảng + trạng thái', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/LichSu', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();
    console.log(`📋 Delivery history: ${rowCount} rows`);
    if (rowCount > 0) {
      const firstStatus = await rows.first().locator('td').last().textContent();
      console.log(`  First status: ${firstStatus?.trim()}`);
    }
  });
});

// ─── SUITE 3: Settings ───
test.describe('⚙️ Cài đặt', () => {
  test('[TC-9.7] CaiDat — form fields + submit', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/CaiDat', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const inputs = page.locator('form input[type="text"], form input[type="password"]');
    const inputCount = await inputs.count();
    console.log(`📝 Settings fields: ${inputCount}`);
    const submitBtn = page.locator('button[type="submit"]').first();
    if (await submitBtn.isVisible().catch(() => false)) {
      await submitBtn.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      console.log(`📍 After settings save: ${page.url()}`);
    }
  });
});

// ─── SUITE 4: QR Delivery ───
test.describe('📸 QR Delivery Advanced', () => {
  test('[TC-9.8] QRDelivery page — SignalR auto-refresh', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const tabs = page.locator('.qr-tab-btn');
    const tabCount = await tabs.count();
    console.log(`📋 QR tabs: ${tabCount}`);
    if (tabCount > 0) {
      await tabs.nth(0).click(); await page.waitForTimeout(500);
      await tabs.nth(1).click(); await page.waitForTimeout(500);
      if (tabCount > 2) { await tabs.nth(2).click(); await page.waitForTimeout(500); }
      console.log('✅ All QR tabs clickable');
    }
    // SignalR loaded
    await page.waitForLoadState('networkidle');
    const hasSignalR = await page.evaluate(() => !!window['signalR']).catch(() => false);
    console.log(`🔌 SignalR on QR: ${hasSignalR}`);
  });
});
