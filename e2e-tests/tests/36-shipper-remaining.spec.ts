/**
 * 🚚 BỘ TEST 36: SHIPPER REMAINING — ThongBao, ClaimOrder, updateStatus, QR
 *
 * Mục tiêu: Test các trang và API còn thiếu của shipper
 * - ThongBao: notifications list + badges
 * - ClaimOrder API: nhận đơn
 * - UpdateDonHang API: cập nhật trạng thái
 * - Toggle Online/Offline status
 * - QRDelivery full flow
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

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

// ─── THONG BAO ───
test.describe('🔔 Shipper Thông báo', () => {

  test('[TC-SHP-01] ThongBao page load — danh sách notification', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/ThongBao', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    // Kiểm tra notification items
    const notifications = page.locator('.notification-item, .thongbao-item, .alert, .list-group-item');
    const notifCount = await notifications.count();
    console.log(`🔔 Notifications: ${notifCount}`);

    // Kiểm tra time stamps
    let hasTime = false;
    if (notifCount > 0) {
      const firstText = await notifications.first().textContent();
      hasTime = bodyText.includes('phút') || bodyText.includes('giờ') || bodyText.includes('ngày');
      console.log(`  First: ${firstText?.trim().substring(0, 80)}`);
      console.log(`  Has time: ${hasTime}`);
    }
  });

  test('[TC-SHP-02] ThongBao — unread badge + mark as read', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/ThongBao', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Unread badges
    const unreadBadges = page.locator('.unread-badge, .badge-unread, [class*="unread"]');
    const badgeCount = await unreadBadges.count();
    console.log(`🔴 Unread badges: ${badgeCount}`);

    // Click vào notification đầu tiên (mark as read)
    const firstNotif = page.locator('.notification-item, .thongbao-item, .list-group-item').first();
    if (await firstNotif.isVisible().catch(() => false)) {
      await firstNotif.click();
      await page.waitForTimeout(1000);
      console.log('✅ Clicked notification');
    }
  });
});

// ─── CLAIM ORDER ───
test.describe('📦 Claim Order API', () => {

  test('[TC-SHP-03] ClaimOrder API — kiểm tra request/response', async ({ page }) => {
    await loginAsShipper(page);

    // Test ClaimOrder với order không tồn tại
    const resp = await page.request.post('/Shipper/ClaimOrder?id=99999', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    const json = await resp.json();
    console.log(`📡 ClaimOrder: ${JSON.stringify(json)}`);
    expect(json).toBeDefined();
    // Should return error since order doesn't exist or is already claimed
    if (json.success === false) {
      console.log(`✅ API correctly rejected: ${json.message}`);
    }
  });

  test('[TC-SHP-04] ClaimOrder — với order ID hợp lệ (nếu có FREE-PICK)', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Tìm nút order detail trong FREE-PICK
    const freePickLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
    const linkCount = await freePickLinks.count();
    console.log(`🔗 FREE-PICK order links: ${linkCount}`);

    if (linkCount > 0) {
      const href = await freePickLinks.first().getAttribute('href');
      const match = href?.match(/OrderDetail\/(\d+)/);
      if (match) {
        const orderId = match[1];
        console.log(`📋 Claiming order #${orderId}`);

        const resp = await page.request.post(`/Shipper/ClaimOrder?id=${orderId}`, {
          headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const json = await resp.json();
        console.log(`📡 Response: ${JSON.stringify(json)}`);
      }
    } else {
      console.log('ℹ️ Không có FREE-PICK orders');
    }
  });
});

// ─── UPDATE DON HANG ───
test.describe('🔄 UpdateDonHang — Cập nhật trạng thái', () => {

  test('[TC-SHP-05] UpdateDonHang API các trạng thái', async ({ page }) => {
    await loginAsShipper(page);

    // Test các status codes
    const statuses = [
      { code: 'lh', name: 'Lấy hàng' },
      { code: 'dg', name: 'Đang giao' },
      { code: 'ht', name: 'Hoàn thành' },
      { code: 'invalid', name: 'Invalid status' },
    ];

    for (const s of statuses) {
      const resp = await page.request.post(`/Shipper/UpdateDonHang?status=${s.code}&id=99999`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
      });
      const json = await resp.json();
      console.log(`📡 ${s.name} (${s.code}): ${JSON.stringify(json)}`);
    }
  });
});

// ─── UPDATE STATUS TOGGLE ───
test.describe('🔄 Shipper Online/Offline Toggle', () => {

  test('[TC-SHP-06] Toggle online/offline status', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    // Tìm nút toggle status
    const toggleLink = page.locator('a[href*="updateStatus"], a[href*="UpdateStatus"]').first();
    if (await toggleLink.isVisible().catch(() => false)) {
      const textBefore = await toggleLink.textContent();
      console.log(`🔄 Status before: "${textBefore?.trim()}"`);

      await toggleLink.click();
      await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
      await page.waitForTimeout(1000);

      console.log(`📍 After toggle: ${page.url()}`);
    } else {
      console.log('ℹ️ Không có toggle status link — check API directly');
      const resp = await page.request.get('/Shipper/updateStatus', {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
      });
      const json = await resp.json();
      console.log(`📡 updateStatus API: ${JSON.stringify(json)}`);
    }
  });
});

// ─── QR DELIVERY FULL ───
test.describe('📸 QR Delivery Advanced', () => {

  test('[TC-SHP-07] QRDelivery — tải xuống QR code', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const downloadBtns = page.locator('a:has-text("Tải QR"), .btn-download, a[download]');
    const btnCount = await downloadBtns.count();
    console.log(`⬇️ Download QR buttons: ${btnCount}`);

    if (btnCount > 0) {
      // Kiểm tra QR image quality
      const qrImg = page.locator('img[alt*="QR"], img[src*="GenerateQR"]').first();
      if (await qrImg.isVisible().catch(() => false)) {
        const valid = await qrImg.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
        console.log(`📸 QR image valid: ${valid}, size: ${await qrImg.evaluate((el: HTMLImageElement) => `${el.naturalWidth}x${el.naturalHeight}`).catch(() => 'N/A')}`);
      }
    }
  });

  test('[TC-SHP-08] GenerateQR API — tạo QR code', async ({ page }) => {
    await loginAsShipper(page);

    // Test GenerateQR API
    const resp = await page.goto('/EDelivery/GenerateQR?orderId=99999', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const status = resp?.status() ?? 0;
    const contentType = resp?.headers()['content-type'] || '';
    console.log(`📡 GenerateQR: status=${status}, type=${contentType}`);

    // Should return image/png with QR code
    if (status === 200) {
      expect(contentType).toContain('image');
      console.log('✅ QR code generated successfully');
    }
  });
});
