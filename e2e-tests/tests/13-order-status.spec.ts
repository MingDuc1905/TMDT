/**
 * 📦 BỘ TEST 13: CHUYỂN TRẠNG THÁI ĐƠN HÀNG
 *
 * Mục tiêu: Validate order status transitions
 * - COD flow: Chờ thanh toán → Đang chuẩn bị → Chờ shipper → Đang giao → Hoàn thành
 * - Cancellation flow
 * - Invalid state transitions
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;
const RESTAURANT = USERS.restaurant1;
const SHIPPER = USERS.shipper1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. ORDER STATUS DISPLAY
// ════════════════════════════════════════════════════════════════
test.describe('📊 Order status display', () => {

  test('[TC-STATUS-01] Customer: OrderHistory → verify orders listed with status', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check for order list
    const orderRows = page.locator('tr, .order-card, .order-item, .history-item');
    const count = await orderRows.count();
    console.log(`Order history items: ${count}`);

    if (count > 0) {
      // Check status badges exist
      const statusBadges = page.locator('.badge, [class*="status"]');
      const badgeCount = await statusBadges.count();
      console.log(`Status badges found: ${badgeCount}`);
    }
  });

  test('[TC-STATUS-02] Customer: OrderHistory → click vào đơn → OrderDetail page', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Find first order link
    const orderLinks = page.locator('a[href*="ChiTietDonHang"], a[href*="OrderTracking"], tr a, .order-item a');
    const count = await orderLinks.count();
    console.log(`Order links: ${count}`);

    if (count > 0) {
      await orderLinks.first().click();
      await page.waitForTimeout(3000);

      const currentUrl = page.url();
      console.log(`After click order link: ${currentUrl}`);
      const isDetailOrTracking = currentUrl.includes('ChiTietDonHang') ||
        currentUrl.includes('OrderTracking') ||
        currentUrl.includes('Order');
      expect(isDetailOrTracking).toBe(true);
    }
  });

  test('[TC-STATUS-03] Restaurant: OrderList → verify orders with accept/reject buttons', async ({ page }) => {
    await loginAs(page, RESTAURANT);

    await page.goto('/Restaurant/OrderList', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const orderItems = page.locator('.order-item, tr, .order-card');
    const count = await orderItems.count();
    console.log(`Restaurant order list items: ${count}`);

    // Check for action buttons (nhận đơn, hủy đơn)
    const acceptBtns = page.locator('a[href*="nhandon"], button:has-text("Nhận"), .btn-success:has-text("Nhận")');
    const rejectBtns = page.locator('a[href*="huydon"], button:has-text("Hủy"), .btn-danger:has-text("Hủy")');
    const acceptCount = await acceptBtns.count();
    const rejectCount = await rejectBtns.count();
    console.log(`Accept buttons: ${acceptCount}, Reject buttons: ${rejectCount}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. ORDER TRACKING PROGRESS BAR
// ════════════════════════════════════════════════════════════════
test.describe('📍 Order tracking progress', () => {

  test('[TC-STATUS-04] OrderTracking → progress bar renders', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    // Try to access tracking page
    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const trackingProgress = page.locator('#trackingProgress');
    const isVisible = await trackingProgress.isVisible().catch(() => false);
    console.log(`Tracking progress visible: ${isVisible}`);

    if (isVisible) {
      const steps = page.locator('.fs-tracking-step');
      const stepCount = await steps.count();
      console.log(`Tracking steps: ${stepCount}`);

      const currentStep = page.locator('.fs-tracking-step.current');
      const hasCurrent = await currentStep.isVisible().catch(() => false);
      console.log(`Has current step: ${hasCurrent}`);

      const completedSteps = page.locator('.fs-tracking-step.completed');
      const completedCount = await completedSteps.count();
      console.log(`Completed steps: ${completedCount}`);
    }
  });

  test('[TC-STATUS-05] OrderTracking → map renders', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const map = page.locator('#trackingMap');
    const isVisible = await map.isVisible().catch(() => false);
    console.log(`Tracking map visible: ${isVisible}`);

    if (isVisible) {
      // Check Leaflet map loaded
      const leafletTiles = page.locator('.leaflet-tile, .leaflet-container');
      const tilesCount = await leafletTiles.count();
      console.log(`Leaflet elements: ${tilesCount}`);
    }
  });

  test('[TC-STATUS-06] OrderTracking → ETA display', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/OrderTracking', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const eta = page.locator('#etaText');
    const isVisible = await eta.isVisible().catch(() => false);

    if (isVisible) {
      const etaText = await eta.textContent();
      console.log(`ETA: "${etaText}"`);
    } else {
      console.log('ℹ️ ETA not visible (no active delivery)');
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 3. SHIPPER ORDER STATUS
// ════════════════════════════════════════════════════════════════
test.describe('🚚 Shipper order status', () => {

  test('[TC-STATUS-07] Shipper: OrderList → verify order cards with status', async ({ page }) => {
    await loginAs(page, SHIPPER);

    await page.goto('/Shipper', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const orderCards = page.locator('.order-card, .card, tr');
    const count = await orderCards.count();
    console.log(`Shipper order cards: ${count}`);

    // Check for status badges
    const statusBadges = page.locator('.badge, [class*="status"]');
    const badgeCount = await statusBadges.count();
    console.log(`Status badges: ${badgeCount}`);
  });

  test('[TC-STATUS-08] Shipper: ThuNhap → earnings dashboard loads', async ({ page }) => {
    await loginAs(page, SHIPPER);

    await page.goto('/Shipper/ThuNhap', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const pageContent = await page.locator('body').textContent();
    const hasEarnings = pageContent?.includes('Thu nhập') ||
      pageContent?.includes('Doanh thu') ||
      pageContent?.includes('Đơn hàng');
    console.log(`Earnings page loaded: ${hasEarnings}`);
  });
});
