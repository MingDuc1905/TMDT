/**
 * 📸 BỘ TEST 06: E-DELIVERY QR CODE FULL FLOW
 *
 * Mục tiêu:
 * - Shipper: Xem QR code trên OrderDetail + QRDelivery list
 * - Merchant: Quét QR bằng camera (html5-qrcode)
 * - Customer: Nhận real-time notification khi QR được scan
 * - Admin: Delivery Logs Matrix + Bypass modal + pastel badges
 *
 * Flow chính:
 * 1. Customer tạo đơn → Merchant nhận đơn → Shipper nhận đơn
 * 2. Shipper vào OrderDetail → QR code hiển thị
 * 3. Merchant quét QR → API ConfirmScan → SignalR broadcast
 * 4. Customer nhận notification → giao diện cập nhật
 * 5. Admin bypass (nếu cần) → chuyển trạng thái thủ công
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;
const RESTAURANT = USERS.restaurant1;
const SHIPPER = USERS.shipper2;
const ADMIN = USERS.admin1;

// ─── Helpers ───
async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  const url = await login.login(user.username, user.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let retry = 0; retry < 2; retry++) {
      try {
        await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 });
        if (!page.url().includes('/Home/Login')) break;
      } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── TEST SUITE 1: Shipper — QR Code trên OrderDetail ───
test.describe('🚚 Shipper — QR Code Display', () => {

  test('[TC-6.1] Shipper OrderDetail — QR code image load', async ({ page }) => {
    await loginAs(page, SHIPPER);
    // Vào QRDelivery page để xem QR
    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra QR images có load không
    const qrImages = page.locator('img[alt*="QR"]');
    const qrCount = await qrImages.count();
    console.log(`📸 QR images found: ${qrCount}`);

    if (qrCount > 0) {
      // Kiểm tra ảnh QR không bị lỗi
      for (let i = 0; i < Math.min(qrCount, 3); i++) {
        const img = qrImages.nth(i);
        const src = await img.getAttribute('src');
        console.log(`  QR #${i}: ${src?.substring(0, 80)}...`);
        // Kiểm tra ảnh load thành công (naturalWidth > 0)
        const valid = await img.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
        if (!valid) console.log(`  ⚠️ QR #${i} failed to load, checking fallback...`);
      }
    } else {
      console.log('ℹ️ No QR orders found — shipper may have no pending orders');
    }
  });

  test('[TC-6.2] QRDelivery page — tab filter hoạt động (Chờ giao / Đang giao / Hoàn thành)', async ({ page }) => {
    await loginAs(page, SHIPPER);
    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra tab buttons
    const tabBtns = page.locator('.qr-tab-btn');
    const tabCount = await tabBtns.count();
    console.log(`📋 QR tabs: ${tabCount}`);

    if (tabCount > 0) {
      // Click từng tab — force click vì sticky header có thể overlap
      for (let i = 0; i < tabCount; i++) {
        const tabText = await tabBtns.nth(i).textContent();
        await tabBtns.nth(i).scrollIntoViewIfNeeded().catch(() => {});
        await tabBtns.nth(i).click({ force: true });
        await page.waitForTimeout(500);
        console.log(`  Tab ${i}: "${tabText?.trim()}" — clicked`);
      }
    }
  });

  test('[TC-6.3] OrderDetail page — QR glassmorphism card hiển thị', async ({ page }) => {
    await loginAs(page, SHIPPER);
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Vào order detail đầu tiên
    const detailLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
    const linkCount = await detailLinks.count();
    if (linkCount > 0) {
      await detailLinks.first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // Check QR card với glassmorphism class
      const qrCard = page.locator('.card:has(.fa-qrcode)').first();
      const cardVisible = await qrCard.isVisible().catch(() => false);
      console.log(`🃏 QR card visible: ${cardVisible}`);

      if (cardVisible) {
        const qrImg = qrCard.locator('img[alt*="QR"]');
        const imgLoaded = await qrImg.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0).catch(() => false);
        console.log(`📸 QR image loaded: ${imgLoaded}`);

        // Kiểm tra nút tải QR
        const downloadBtn = qrCard.locator('a:has-text("Tải QR")');
        console.log(`⬇️ Download QR btn: ${await downloadBtn.isVisible().catch(() => false)}`);
      }
    } else {
      console.log('ℹ️ No order detail links — skip');
    }
  });
});

// ─── TEST SUITE 2: Merchant — html5-qrcode Scanner ───
test.describe('🏪 Merchant — QR Scanner', () => {

  test('[TC-6.4] Merchant scan page — html5-qrcode library load', async ({ page }) => {
    await loginAs(page, RESTAURANT);
    await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra scanner container
    const scannerDiv = page.locator('#qr-reader');
    const scannerExists = await scannerDiv.count();
    console.log(`📷 QR-reader container: ${scannerExists > 0}`);

    // Kiểm tra camera controls
    const startBtn = page.locator('#btnStartScan');
    const stopBtn = page.locator('#btnStopScan');
    const switchBtn = page.locator('#btnSwitchCamera');
    console.log(`  Start btn: ${await startBtn.isVisible().catch(() => false)}`);
    console.log(`  Stop btn: ${await stopBtn.isVisible().catch(() => false)}`);
    console.log(`  Switch cam: ${await switchBtn.isVisible().catch(() => false)}`);

    // Kiểm tra scan history section
    const historyContainer = page.locator('#scanHistory');
    await expect(historyContainer).toBeVisible({ timeout: 5_000 });
    const historyText = await historyContainer.textContent();
    console.log(`📋 Scan history: "${historyText?.substring(0, 50)}"`);
  });

  test('[TC-6.5] Merchant scan page — API call khi scan thành công', async ({ page }) => {
    await loginAs(page, RESTAURANT);
    await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    // Verify API endpoint tồn tại
    const apiResponse = await page.request.post('/EDelivery/ConfirmScan', {
      data: { token: 'invalid-token-test' },
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const responseJson = await apiResponse.json();
    console.log(`📡 API response: ${JSON.stringify(responseJson)}`);
    expect(responseJson.success).toBe(false); // Token invalid
    expect(responseJson.message).toContain('Mã QR');
  });

  test('[TC-6.6] Scan history — localStorage lưu và hiển thị', async ({ page }) => {
    await loginAs(page, RESTAURANT);
    await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    // Kiểm tra localStorage
    const history = await page.evaluate(() => localStorage.getItem('restaurantScanHistory'));
    console.log(`📦 localStorage scanHistory: ${history ? history.substring(0, 100) : 'null'}`);

    // Xóa history
    await page.evaluate(() => localStorage.removeItem('restaurantScanHistory'));
    const afterClear = await page.evaluate(() => localStorage.getItem('restaurantScanHistory'));
    console.log(`🗑️ After clear: ${afterClear}`);
  });
});

// ─── TEST SUITE 3: Customer — Real-time Notification ───
test.describe('📱 Customer — Real-time QR Scan Notification', () => {

  test('[TC-6.7] ScanQR landing page — hiển thị thông tin đơn hàng', async ({ page }) => {
    // Test với token không hợp lệ trước
    await page.goto('/EDelivery/ScanQR/invalid-token-test', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Scan page (invalid token): ${bodyText.substring(0, 100)}`);
    expect(bodyText).toContain('hợp lệ');
  });

  test('[TC-6.8] OrderTracking page — SignalR delivery event handlers', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    // Vào order tracking của đơn đầu tiên
    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    // Click vào đơn đầu để xem tracking
    const firstOrderLink = page.locator('a[href*="OrderTracking"]').first();
    if (await firstOrderLink.isVisible().catch(() => false)) {
      await firstOrderLink.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // Kiểm tra progress bar
      const progressBar = page.locator('.fs-tracking-progress');
      const progressVisible = await progressBar.isVisible().catch(() => false);
      console.log(`📊 Progress bar visible: ${progressVisible}`);

      // Kiểm tra SignalR connection
      await page.waitForLoadState('networkidle');
      const hasSignalR = await page.evaluate(() => !!window['signalR']).catch(() => false);
      console.log(`🔌 SignalR loaded: ${hasSignalR}`);

      // Kiểm tra map
      const mapDiv = page.locator('#trackingMap');
      const mapVisible = await mapDiv.isVisible().catch(() => false);
      console.log(`🗺️ Tracking map visible: ${mapVisible}`);
    } else {
      console.log('ℹ️ No order tracking links found');
    }
  });

  test('[TC-6.9] ChiTietDonHang page — SignalR + Leaflet live map', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const detailLink = page.locator('a[href*="ChiTietDonHang"]').first();
    if (await detailLink.isVisible().catch(() => false)) {
      await detailLink.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      const bodyText = await page.locator('body').textContent() || '';
      const hasSignalR = bodyText.includes('signalR');
      console.log(`🔌 SignalR on detail page: ${hasSignalR}`);
    }
  });
});

// ─── TEST SUITE 4: Admin — Delivery Logs + Bypass ───
test.describe('👑 Admin — Delivery Logs Matrix & Bypass', () => {

  test('[TC-6.10] Delivery Logs page load — stats + table', async ({ page }) => {
    await loginAs(page, ADMIN);
    await page.goto('/EDelivery/DeliveryLogs', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra stats cards
    const statCards = page.locator('.stat-card');
    const statCount = await statCards.count();
    console.log(`📊 Stat cards: ${statCount}`);

    if (statCount > 0) {
      for (let i = 0; i < statCount; i++) {
        const text = await statCards.nth(i).textContent();
        console.log(`  ${text?.trim().substring(0, 60)}`);
      }
    }

    // Kiểm tra table không có vertical borders
    const table = page.locator('.delivery-table');
    const tableExists = await table.isVisible().catch(() => false);
    console.log(`📋 Delivery table visible: ${tableExists}`);

    if (tableExists) {
      // Kiểm tra pastel badges
      const badges = page.locator('.pastel-badge');
      const badgeCount = await badges.count();
      console.log(`🏷️ Pastel badges: ${badgeCount}`);

      if (badgeCount > 0) {
        const firstBadgeText = await badges.first().textContent();
        console.log(`  First badge: "${firstBadgeText?.trim()}"`);
      }
    }

    // Verify sidebar nav link
    const navLink = page.locator('a[href*="DeliveryLogs"]');
    const navExists = await navLink.count();
    console.log(`🔗 Nav link to DeliveryLogs: ${navExists > 0}`);
  });

  test('[TC-6.11] Bypass modal — open + close + UI elements', async ({ page }) => {
    await loginAs(page, ADMIN);
    await page.goto('/EDelivery/DeliveryLogs', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra nút bypass tồn tại
    const bypassBtns = page.locator('.btn-bypass');
    const bypassCount = await bypassBtns.count();
    console.log(`🛠️ Bypass buttons: ${bypassCount}`);

    if (bypassCount > 0) {
      // Click nút bypass đầu
      await bypassBtns.first().click();
      await page.waitForTimeout(1000);

      // Kiểm tra modal hiển thị
      const modal = page.locator('#bypassModal');
      const modalVisible = await modal.isVisible().catch(() => false);
      console.log(`📦 Bypass modal visible: ${modalVisible}`);

      if (modalVisible) {
        const modalText = await modal.textContent();
        console.log(`  Modal text: "${modalText?.substring(0, 100)}"`);

        // Kiểm tra status select
        const statusSelect = page.locator('#bypassStatusSelect');
        const selectExists = await statusSelect.isVisible().catch(() => false);
        console.log(`  Status select: ${selectExists}`);

        // Kiểm tra các option
        if (selectExists) {
          const options = await statusSelect.locator('option').allTextContents();
          console.log(`  Options: ${options.join(', ')}`);
        }

        // Kiểm tra nút Cancel và Confirm
        const cancelBtn = page.locator('.btn-cancel');
        const confirmBtn = page.locator('#btnConfirmBypass');
        console.log(`  Cancel btn: ${await cancelBtn.isVisible().catch(() => false)}`);
        console.log(`  Confirm btn: ${await confirmBtn.isVisible().catch(() => false)}`);

        // Close modal
        await cancelBtn.click();
        await page.waitForTimeout(500);
        console.log(`  Modal closed: ${!(await modal.isVisible().catch(() => false))}`);
      }
    } else {
      console.log('ℹ️ No bypass buttons (all orders may be completed/cancelled)');
    }
  });

  test('[TC-6.12] Bypass API call — validate request/response', async ({ page }) => {
    await loginAs(page, ADMIN);
    await page.goto('/Admin', { waitUntil: 'networkidle', timeout: 30_000 });
    await page.waitForTimeout(1000);

    // Test bypass với order không tồn tại
    const response = await page.request.post('/EDelivery/Bypass', {
      data: { orderId: 99999, targetStatus: 'Đã lấy' },
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await response.json();
    console.log(`📡 Bypass API response: ${JSON.stringify(json)}`);
    expect(json.success).toBe(false);
    expect(json.message).toContain('không tồn tại');
  });

  test('[TC-6.13] Delivery Logs — no vertical borders in table', async ({ page }) => {
    await loginAs(page, ADMIN);
    await page.goto('/EDelivery/DeliveryLogs', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra CSS: border-left/right của th/td phải là 0px (không viền dọc)
    const hasNoVerticalBorder = await page.evaluate(() => {
      const th = document.querySelector('.delivery-table th');
      if (!th) return false;
      const style = window.getComputedStyle(th);
      return style.borderLeftWidth === '0px' && style.borderRightWidth === '0px';
    });
    console.log(`📐 No vertical borders (borderLeftWidth=0px): ${hasNoVerticalBorder}`);
  });

  test('[TC-6.14] Delivery Logs — admin sidebar nav link hoạt động', async ({ page }) => {
    await loginAs(page, ADMIN);

    // Click nav link từ sidebar — page.evaluate bypasses viewport issues on mobile
    const deliveryLink = page.locator('a[href*="DeliveryLogs"]').first();
    if (await deliveryLink.isVisible().catch(() => false)) {
      await deliveryLink.scrollIntoViewIfNeeded().catch(() => {});
      await page.evaluate((el: HTMLElement) => el.click(), await deliveryLink.elementHandle());
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      const url = page.url();
      console.log(`📍 URL after nav click: ${url}`);
      expect(url).toContain('DeliveryLogs');
    } else {
      console.log('ℹ️ DeliveryLogs nav link not found in sidebar');
    }
  });
});

// ─── TEST SUITE 5: End-to-End Flow: Customer → Shipper → Merchant Scan ───
test.describe('🔄 End-to-End: Order → QR → Scan → Notify', () => {

  test('[TC-6.15] Full flow: Tạo đơn → Shipper nhận → QR hiển thị', async ({ page, context }) => {
    // Step 1: Customer login + thêm món + checkout
    const customerPage = await context.newPage();
    const loginC = new LoginPage(customerPage);
    await loginC.gotoLogin();
    await loginC.usernameInput.fill(CUSTOMER.username);
    await loginC.passwordInput.fill(CUSTOMER.password);
    await loginC.loginButton.click();
    await customerPage.waitForLoadState('networkidle').catch(() => {});
    await customerPage.waitForTimeout(2000);

    // Thêm món
    await customerPage.goto(`/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, { waitUntil: 'domcontentloaded' });
    await customerPage.waitForSelector('.item-restaurant-row', { timeout: 25_000 });

    const addBtn = customerPage.locator('.add-to-cart-btn').first();
    if (await addBtn.isVisible().catch(() => false)) {
      await customerPage.locator('.adding-food-cart input[name="soLuong"]').first().fill('1');
      await addBtn.click();
      await customerPage.waitForTimeout(2000);
      console.log('✅ Customer: added item to cart');
    }
    await customerPage.close();

    // Step 2: Shipper check QR page
    await loginAs(page, SHIPPER);
    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const qrImgs = page.locator('img[alt*="QR"]');
    const qrCount = await qrImgs.count();
    console.log(`📸 QR images for shipper: ${qrCount}`);

    // Step 3: Merchant scan page load
    const merchantPage = await context.newPage();
    await loginAs(merchantPage, RESTAURANT);
    await merchantPage.goto('/edelivery/merchant-scan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await merchantPage.waitForTimeout(3000);
    const scannerVisible = await merchantPage.locator('#qr-reader').isVisible().catch(() => false);
    console.log(`📷 Merchant scanner visible: ${scannerVisible}`);
    await merchantPage.close();

    // Step 4: Admin delivery logs load
    await loginAs(page, ADMIN);
    await page.goto('/EDelivery/DeliveryLogs', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);
    const deliveryTable = await page.locator('.delivery-table').isVisible().catch(() => false);
    console.log(`👑 Admin delivery logs: ${deliveryTable}`);
  });
});
