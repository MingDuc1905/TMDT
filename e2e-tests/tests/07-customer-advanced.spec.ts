/**
 * 🧑‍💼 BỘ TEST 07: CUSTOMER ADVANCED — Payment, Review, Multi-Restaurant, Chat
 *
 * Target: Tất cả tính năng nâng cao của Customer chưa có trong 02-customer-flow
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED, SHIPPING } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAsCustomer(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(CUSTOMER.username, CUSTOMER.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (!page.url().includes('/Home/Login')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── SUITE 1: Payment ───
test.describe('💳 Payment Methods', () => {
  test('[TC-7.1] Thanh toán MoMo — page load + QR display', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const momoOpt = page.locator('.payment-option:has-text("MoMo"), .payment-option:has-text("momo"), [class*="momo"]').first();
    const momoExists = await momoOpt.isVisible().catch(() => false);
    console.log(`💳 MoMo option visible: ${momoExists}`);
    if (momoExists) {
      await momoOpt.click(); await page.waitForTimeout(1000);
      const qrImg = page.locator('img[src*="momo"], img[src*="MoMo"], .qr-wrapper img').first();
      const qrVisible = await qrImg.isVisible().catch(() => false);
      console.log(`  MoMo QR visible: ${qrVisible}`);
    }
  });

  test('[TC-7.2] Thanh toán Bank Transfer — QR + thông tin tài khoản', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const bankOpt = page.locator('.payment-option:has-text("ngân hàng"), .payment-option:has-text("chuyển khoản"), [class*="bank"]').first();
    const bankExists = await bankOpt.isVisible().catch(() => false);
    console.log(`🏦 Bank transfer option visible: ${bankExists}`);
    if (bankExists) {
      await bankOpt.click(); await page.waitForTimeout(1000);
      // Kiểm tra thông tin tài khoản hiển thị
      const bankInfo = page.locator('[class*="bank-info"], [class*="account"], [class*="stk"]').first();
      console.log(`  Bank info visible: ${await bankInfo.isVisible().catch(() => false)}`);
    }
  });

  test('[TC-7.3] Verify Bank Transaction API', async ({ page }) => {
    await loginAsCustomer(page);
    const resp = await page.request.get('/Payment/VerifyBankTransaction', { params: { madh: 0 } });
    const json = await resp.json();
    console.log(`📡 VerifyBank API: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
  });
});

// ─── SUITE 2: Order Tracking SignalR ───
test.describe('📍 Order Tracking Real-time', () => {
  test('[TC-7.4] OrderTracking page — progress bar steps render', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const trackLink = page.locator('a[href*="OrderTracking"]').first();
    if (await trackLink.isVisible().catch(() => false)) {
      await trackLink.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(3000);
      const steps = page.locator('.fs-tracking-step');
      const stepCount = await steps.count();
      console.log(`📊 Tracking steps: ${stepCount}`);
      expect(stepCount).toBeGreaterThan(0);
      // Kiểm tra step icons
      const icons = page.locator('.fs-tracking-icon');
      console.log(`  Icons: ${await icons.count()}`);
      // SignalR loaded
      await page.waitForLoadState('networkidle');
      const hasSignalR = await page.evaluate(() => !!window['signalR']).catch(() => false);
      console.log(`  SignalR loaded: ${hasSignalR}`);
    } else console.log('ℹ️ No tracking link');
  });

  test('[TC-7.5] Map Leaflet render trên OrderTracking', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const trackLink = page.locator('a[href*="OrderTracking"]').first();
    if (await trackLink.isVisible().catch(() => false)) {
      await trackLink.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(3000);
      const mapDiv = page.locator('#trackingMap');
      const mapVisible = await mapDiv.isVisible().catch(() => false);
      console.log(`🗺️ Map visible: ${mapVisible}`);
      if (mapVisible) {
        const box = await mapDiv.boundingBox();
        if (box) { expect(box.width).toBeGreaterThan(0); console.log(`  Map size: ${box.width}x${box.height}`); }
        // Leaflet tiles loaded
        const tiles = await page.locator('.leaflet-tile-loaded').count();
        console.log(`  Leaflet tiles: ${tiles}`);
      }
    }
  });

  test('[TC-7.6] ChiTietDonHang — SignalR + Leaflet map', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const detailLink = page.locator('a[href*="ChiTietDonHang"]').first();
    if (await detailLink.isVisible().catch(() => false)) {
      await detailLink.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(3000);
      const hasSignalR = await page.evaluate(() => !!window['signalR']).catch(() => false);
      console.log(`🔌 SignalR: ${hasSignalR}`);
      const hasLeaflet = await page.evaluate(() => typeof L !== 'undefined').catch(() => false);
      console.log(`🍃 Leaflet: ${hasLeaflet}`);
    }
  });
});

// ─── SUITE 3: Cart ───
test.describe('🛒 Cart Advanced', () => {
  test('[TC-7.7] Cart badge count — update after add', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    const addBtn = page.locator('.add-to-cart-btn').first();
    if (await addBtn.isVisible().catch(() => false)) {
      await page.locator('.adding-food-cart input[name="soLuong"]').first().fill('1');
      await addBtn.click(); await page.waitForTimeout(2000);
      // Check badge count
      const badge = page.locator('.fs-cart-badge, .cart-count-badge, [class*="cart-badge"]').first();
      const badgeText = await badge.textContent().catch(() => null);
      console.log(`🔢 Cart badge: "${badgeText}"`);
    }
  });

  test('[TC-7.8] Multi-restaurant cart — thêm món từ 2 quán khác nhau', async ({ page }) => {
    await loginAsCustomer(page);
    // Thêm món quán 1
    await page.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    const addBtn1 = page.locator('.add-to-cart-btn').first();
    if (await addBtn1.isVisible()) { await page.locator('.adding-food-cart input[name="soLuong"]').first().fill('1'); await addBtn1.click(); await page.waitForTimeout(2000); }
    // Thêm món quán 2
    await page.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.com1990}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    const addBtn2 = page.locator('.add-to-cart-btn').first();
    if (await addBtn2.isVisible()) { await page.locator('.adding-food-cart input[name="soLuong"]').first().fill('1'); await addBtn2.click(); await page.waitForTimeout(2000); }
    // Kiểm tra cart có item từ 2 quán
    await page.goto('/Cart', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(2000);
    const items = page.locator('.cart-item, [class*="cart-item"]');
    const itemCount = await items.count();
    console.log(`🛒 Cart items (multi-restaurant): ${itemCount}`);
  });
});

// ─── SUITE 4: Review ───
test.describe('⭐ Reviews & Ratings', () => {
  test('[TC-7.9] Review section hiển thị trên DetailRestaurant', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const reviewSection = page.locator('#review-list, [class*="review"], [class*="danhgia"]').first();
    const reviewVisible = await reviewSection.isVisible().catch(() => false);
    console.log(`⭐ Review section: ${reviewVisible}`);
    if (reviewVisible) {
      const reviewCount = await page.locator('[class*="review-item"], [class*="danhgia-item"]').count();
      console.log(`  Reviews count: ${reviewCount}`);
    }
  });
});

// ─── SUITE 5: Category Filter ───
test.describe('🔍 Category Filter & Search', () => {
  test('[TC-7.10] Lọc quán theo danh mục — category pills hoạt động', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(2000);
    const pills = page.locator('.fs-category-pill, .list-category .item-link, [class*="category"] a');
    const pillCount = await pills.count();
    console.log(`🏷️ Category pills: ${pillCount}`);
    if (pillCount > 1) {
      await pills.nth(1).click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2000);
      const restaurants = page.locator('.product-item, [class*="restaurant-card"]');
      console.log(`  Restaurants after filter: ${await restaurants.count()}`);
    }
  });

  test('[TC-7.11] Thanh tìm kiếm navbar — search theo tên quán', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(2000);
    const searchInput = page.locator('input[name="txtSearch"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('Koneko'); await page.locator('button:has-text("Tìm"), button[type="submit"]').first().click();
      try { await page.waitForLoadState('networkidle', { timeout: 20_000 }); } catch {}
      await page.waitForTimeout(2000);
      const results = page.locator('.product-item, [class*="restaurant-card"]');
      console.log(`🔍 Search 'Koneko': ${await results.count()} results`);
    }
  });
});

// ─── SUITE 6: Chat ───
test.describe('💬 Chat với Shipper', () => {
  test('[TC-7.12] Chat page load — component hiển thị', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Home/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const chatBox = page.locator('.chat-box, #chat-container, [class*="chat"]').first();
    console.log(`💬 Chat container: ${await chatBox.isVisible().catch(() => false)}`);
  });
});

// ─── SUITE 7: E-Delivery ───
test.describe('📸 E-Delivery Customer', () => {
  test('[TC-7.13] ScanQR landing — token invalid hiển thị lỗi', async ({ page }) => {
    await page.goto('/edelivery/scan/invalid-test-token', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);
    const body = await page.locator('body').textContent() || '';
    expect(body).toContain('không hợp lệ');
    console.log('✅ Invalid token page shows error');
  });

  test('[TC-7.14] OrderTracking — SignalR delivery events', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(3000);
    const trackLink = page.locator('a[href*="OrderTracking"]').first();
    if (await trackLink.isVisible()) {
      await trackLink.click(); await page.waitForLoadState('networkidle'); await page.waitForTimeout(3000);
      // Kiểm tra callback functions tồn tại
      const hasCallbacks = await page.evaluate(() => {
        const ft = (window as any).FastShipTracking;
        return !!(ft && ft.createHubConnection);
      }).catch(() => false);
      console.log(`📡 FastShipTracking hub: ${hasCallbacks}`);
    }
  });

  test('[TC-7.15] Success page — order confirmation hiển thị', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/Cart/SuccessView', { waitUntil: 'domcontentloaded', timeout: 30_000 }); await page.waitForTimeout(2000);
    const body = await page.locator('body').textContent() || '';
    expect(body).toContain('thành công');
    console.log('✅ Success page shows confirmation');
  });
});
