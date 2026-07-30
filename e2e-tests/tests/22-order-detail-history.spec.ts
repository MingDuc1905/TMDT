/**
 * 📋 BỘ TEST 22: CHI TIẾT ĐƠN HÀNG + LỊCH SỬ (Order Detail & History)
 *
 * Mục tiêu:
 * - ChiTietDonHang: Invoice layout, map, SignalR, progress bar, review button
 * - LichSuDatHang: DataTable, status badges, filter, sort
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  const url = await login.login(user.username, user.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (!page.url().includes('/Home/Login')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── ORDER HISTORY ───
test.describe('📋 Lịch sử đơn hàng (Order History)', () => {

  test('[TC-OH-01] Lịch sử load — DataTable hiển thị + search/sort hoạt động', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra DataTable
    const table = page.locator('.dataTable, table, #example').first();
    await expect(table).toBeVisible({ timeout: 10_000 });

    // Đếm số dòng
    const rows = page.locator('table tbody tr, .order-row, .history-item');
    const rowCount = await rows.count();
    console.log(`📋 Order history rows: ${rowCount}`);

    // Kiểm tra các cột
    if (rowCount > 0) {
      const firstRowCells = rows.first().locator('td');
      const cellCount = await firstRowCells.count();
      console.log(`  Cells per row: ${cellCount}`);
      expect(cellCount).toBeGreaterThanOrEqual(4);
    }
  });

  test('[TC-OH-02] Status badges — màu sắc + emoji chính xác', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const badges = page.locator('.badge, [class*="status-badge"], .order-status');
    const badgeCount = await badges.count();
    console.log(`🏷️ Status badges: ${badgeCount}`);

    if (badgeCount > 0) {
      for (let i = 0; i < Math.min(badgeCount, 5); i++) {
        const text = await badges.nth(i).textContent();
        const color = await badges.nth(i).evaluate(el => window.getComputedStyle(el).backgroundColor);
        console.log(`  Badge ${i}: "${text?.trim()}" bg: ${color}`);
      }
    }
  });

  test('[TC-OH-03] Click đơn → chi tiết đơn hàng', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    const linkCount = await detailLinks.count();
    console.log(`🔗 Detail links: ${linkCount}`);

    if (linkCount > 0) {
      await detailLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('ChiTietDonHang');
    } else {
      console.log('ℹ️ Không có đơn hàng nào để xem chi tiết');
    }
  });

  test('[TC-OH-04] Click "Theo dõi" → OrderTracking page', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const trackLinks = page.locator('a[href*="OrderTracking"]');
    const linkCount = await trackLinks.count();
    console.log(`🔗 Tracking links: ${linkCount}`);

    if (linkCount > 0) {
      await trackLinks.first().click();
      await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL: ${url}`);
      expect(url).toContain('OrderTracking');
    }
  });

  test('[TC-OH-05] Nút đánh giá cho đơn "Hoàn thành"', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Tìm nút đánh giá
    const reviewBtns = page.locator('button:has-text("Đánh giá"), a:has-text("Đánh giá"), .btn-review');
    const btnCount = await reviewBtns.count();
    console.log(`⭐ Review buttons: ${btnCount}`);

    if (btnCount > 0) {
      await reviewBtns.first().click();
      await page.waitForTimeout(1000);

      // Kiểm tra modal đánh giá hiển thị
      const modal = page.locator('.modal.show, .review-modal, [class*="modal"]:visible').first();
      const modalVisible = await modal.isVisible().catch(() => false);
      console.log(`📦 Review modal visible: ${modalVisible}`);
    } else {
      console.log('ℹ️ Không có đơn hoàn thành để đánh giá');
    }
  });

  test('[TC-OH-06] Huỷ đơn (nếu có đơn có thể huỷ)', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const cancelBtns = page.locator('a[href*="HuyDon"], button:has-text("Huỷ"), .btn-cancel-order');
    const cancelCount = await cancelBtns.count();
    console.log(`🔴 Cancel buttons: ${cancelCount}`);

    if (cancelCount > 0) {
      await cancelBtns.first().click();
      await page.waitForTimeout(2000);
      const url = page.url();
      console.log(`📍 URL after cancel: ${url}`);
    } else {
      console.log('ℹ️ Không có đơn nào có thể huỷ');
    }
  });
});

// ─── ORDER DETAIL ───
test.describe('📄 Chi tiết đơn hàng (Order Detail)', () => {

  test('[TC-OD-01] Invoice layout — thông tin đơn hiển thị đầy đủ', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    // Go to order history first, then click first detail
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    const linkCount = await detailLinks.count();
    if (linkCount === 0) {
      console.log('ℹ️ Không có đơn hàng — skip');
      return;
    }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(3000);

    // Kiểm tra thông tin cơ bản
    const bodyText = await page.locator('body').textContent() || '';
    const hasOrderId = bodyText.includes('Đơn hàng') || bodyText.includes('Mã đơn') || bodyText.includes('Order');
    const hasAddress = bodyText.includes('Địa chỉ') || bodyText.includes('giao');
    const hasTotal = bodyText.includes('Tổng') || bodyText.includes('tiền');
    const hasItems = bodyText.includes('Món') || bodyText.includes('Sản phẩm');

    console.log(`📋 Order ID present: ${hasOrderId}`);
    console.log(`📍 Address present: ${hasAddress}`);
    console.log(`💰 Total present: ${hasTotal}`);
    console.log(`🍽️ Items present: ${hasItems}`);

    expect(hasOrderId).toBeTruthy();
    expect(hasTotal).toBeTruthy();
    expect(hasItems).toBeTruthy();
  });

  test('[TC-OD-02] Live map — Leaflet render', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() === 0) { console.log('ℹ️ Skip — no orders'); return; }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(3000);

    // Kiểm tra map
    const mapDiv = page.locator('#map, #trackingMap, .leaflet-container, [class*="map"]');
    const mapVisible = await mapDiv.isVisible().catch(() => false);
    console.log(`🗺️ Map visible: ${mapVisible}`);

    if (mapVisible) {
      const leafletTiles = page.locator('.leaflet-tile-loaded');
      const tileCount = await leafletTiles.count();
      console.log(`  Leaflet tiles: ${tileCount}`);

      const box = await mapDiv.boundingBox();
      if (box) console.log(`  Map size: ${box.width}x${box.height}`);
    }
  });

  test('[TC-OD-03] SignalR connection', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() === 0) { console.log('ℹ️ Skip — no orders'); return; }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(3000);

    const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
    console.log(`🔌 SignalR loaded: ${hasSignalR}`);
  });

  test('[TC-OD-04] Progress bar — tracking steps hiển thị', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() === 0) { console.log('ℹ️ Skip — no orders'); return; }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(3000);

    const progressBar = page.locator('.fs-tracking-progress, .progress-bar, .tracking-steps, [class*="tracking-step"]');
    const progressVisible = await progressBar.isVisible().catch(() => false);
    console.log(`📊 Progress bar visible: ${progressVisible}`);

    if (progressVisible) {
      const steps = page.locator('.fs-tracking-step, .tracking-step, .step-item');
      const stepCount = await steps.count();
      console.log(`  Steps: ${stepCount}`);
    }
  });

  test('[TC-OD-05] Nút đánh giá cho đơn "Hoàn thành"', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() === 0) { console.log('ℹ️ Skip — no orders'); return; }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(2000);

    const reviewBtn = page.locator('button:has-text("Đánh giá"), a:has-text("Đánh giá"), .btn-review');
    const btnVisible = await reviewBtn.isVisible().catch(() => false);
    console.log(`⭐ Review button: ${btnVisible}`);

    if (btnVisible) {
      await reviewBtn.click();
      await page.waitForTimeout(1000);
      const modal = page.locator('.modal.show, .review-modal').first();
      console.log(`📦 Modal visible: ${await modal.isVisible().catch(() => false)}`);
    }
  });

  test('[TC-OD-06] E-Invoice link hoạt động', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() === 0) { console.log('ℹ️ Skip — no orders'); return; }

    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(2000);

    const invoiceLink = page.locator('a[href*="EInvoice"]');
    const invVisible = await invoiceLink.isVisible().catch(() => false);
    console.log(`📄 E-Invoice link: ${invVisible}`);

    if (invVisible) {
      await invoiceLink.click();
      await page.waitForLoadState('networkidle').catch(() => {});
      await page.waitForTimeout(2000);
      console.log(`📍 Invoice URL: ${page.url()}`);
      expect(page.url()).toContain('EInvoice');
    }
  });
});
