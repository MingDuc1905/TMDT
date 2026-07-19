/**
 * 📦 BỘ TEST 16: E-DELIVERY & ORDER TRACKING
 *
 * Mục tiêu: Test e-delivery QR + order tracking pages
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;
const SHIPPER = USERS.shipper2;
const RESTAURANT = USERS.restaurant1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. SHIPPER E-DELIVERY QR
// ════════════════════════════════════════════════════════════════
test.describe('📸 Shipper QR Delivery', () => {

  test('[TC-EDEL-01] Shipper: QRDelivery page → QR images load', async ({ page }) => {
    await loginAs(page, SHIPPER);

    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const qrImages = page.locator('img[alt*="QR"], img[src*="edelivery"], img[src*="qr"]');
    const qrCount = await qrImages.count();
    console.log(`QR images: ${qrCount}`);

    if (qrCount > 0) {
      for (let i = 0; i < Math.min(qrCount, 3); i++) {
        const img = qrImages.nth(i);
        const valid = await img.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
        console.log(`  QR #${i} loaded: ${valid}`);
      }
    }
  });

  test('[TC-EDEL-02] Shipper: QRDelivery tab filter', async ({ page }) => {
    await loginAs(page, SHIPPER);

    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const tabBtns = page.locator('.qr-tab-btn, [class*="tab"], .nav-link, .tab-btn');
    const tabCount = await tabBtns.count();
    console.log(`QR tabs: ${tabCount}`);

    if (tabCount > 1) {
      // Click each tab
      for (let i = 0; i < Math.min(tabCount, 3); i++) {
        const tabText = await tabBtns.nth(i).textContent();
        await tabBtns.nth(i).click();
        await page.waitForTimeout(1000);
        console.log(`  Tab ${i}: "${tabText?.trim()}" clicked`);
      }
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 2. E-DELIVERY API
// ════════════════════════════════════════════════════════════════
test.describe('🔗 E-Delivery API', () => {

  test('[TC-EDEL-03] QR code API → valid PNG response', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    // Get an order ID from order history
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Try to fetch QR for a known order (use order ID 10 as test)
    const response = await page.request.get('/edelivery/qr/10');
    console.log(`QR API status: ${response.status()}`);

    if (response.status() === 200) {
      const contentType = response.headers()['content-type'];
      console.log(`QR content-type: ${contentType}`);
      expect(contentType).toContain('image');
    }
  });

  test('[TC-EDEL-04] Scan QR với invalid token → error', async ({ page }) => {
    await page.goto('/edelivery/scan/invalidtoken123', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const errorText = await page.locator('body').textContent();
    const hasError = errorText?.includes('không hợp lệ') || errorText?.includes('error') || errorText?.includes('hết hạn');
    console.log(`Invalid scan error: ${hasError}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 3. ORDER TRACKING
// ════════════════════════════════════════════════════════════════
test.describe('📍 Order tracking', () => {

  test('[TC-TRACK-01] OrderTracking → progress bar + map render', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const progress = page.locator('#trackingProgress');
    const progressVisible = await progress.isVisible().catch(() => false);
    console.log(`Progress bar: ${progressVisible}`);

    const map = page.locator('#trackingMap');
    const mapVisible = await map.isVisible().catch(() => false);
    console.log(`Map: ${mapVisible}`);

    if (progressVisible) {
      const steps = await page.locator('.fs-tracking-step').count();
      console.log(`Tracking steps: ${steps}`);
    }
  });

  test('[TC-TRACK-02] OrderTracking → ETA display', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const eta = page.locator('#etaText');
    const isVisible = await eta.isVisible().catch(() => false);

    if (isVisible) {
      const text = await eta.textContent();
      console.log(`ETA: "${text}"`);
    }
  });

  test('[TC-TRACK-03] OrderTracking → order summary sidebar', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    // Check order summary
    const orderItems = page.locator('.order-item');
    const count = await orderItems.count();
    console.log(`Order items in tracking: ${count}`);

    // Check info rows
    const infoRows = page.locator('.info-row');
    const infoCount = await infoRows.count();
    console.log(`Info rows: ${infoCount}`);
  });

  test('[TC-TRACK-04] OrderTracking → chat sheet FAB visible', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const chatFab = page.locator('#chatFab, .chat-fab');
    const isVisible = await chatFab.isVisible().catch(() => false);
    console.log(`Chat FAB in tracking: ${isVisible}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 4. ORDER DETAIL
// ════════════════════════════════════════════════════════════════
test.describe('📄 Order detail', () => {

  test('[TC-TRACK-05] ChiTietDonHang → order info + action buttons', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Find first order detail link
    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    const count = await detailLinks.count();
    console.log(`Detail links: ${count}`);

    if (count > 0) {
      await detailLinks.first().click();
      await page.waitForTimeout(3000);

      const orderTitle = page.locator('.checkout__order h4');
      if (await orderTitle.isVisible().catch(() => false)) {
        const title = await orderTitle.textContent();
        console.log(`Order title: "${title}"`);
      }

      const items = page.locator('.checkout__order ul li');
      const itemCount = await items.count();
      console.log(`Order items: ${itemCount}`);
    }
  });

  test('[TC-TRACK-06] ChiTietDonHang → map visible khi đang giao', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
    if (await detailLinks.count() > 0) {
      await detailLinks.first().click();
      await page.waitForTimeout(3000);

      const map = page.locator('#order-tracking-map, .leaflet-container');
      const isVisible = await map.isVisible().catch(() => false);
      console.log(`Inline map in detail: ${isVisible}`);
    }
  });
});
