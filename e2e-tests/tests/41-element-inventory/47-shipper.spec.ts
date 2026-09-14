/**
 * 🚚 BỘ TEST 47: SHIPPER ELEMENT INVENTORY (Element-level QA theo vai trò SHIPPER)
 *
 * Mục tiêu: Kiểm tra element-level trên toàn bộ pages của vai trò SHIPPER
 * - Dashboard (Index): profile card, tabs FREE-PICK/ĐƠN HÀNG, quick stats, Leaflet map #shipper-map, order cards, nút Chi tiết
 * - OrderDetail: thông tin đơn, items table, payment blocks, nút trạng thái (Lấy hàng/Hoàn thành), QR card, live map, chat
 * - ThuNhap: bank card, 4 stats cards, Chart.js canvas, badge 30 ngày
 * - ViTien: số dư VND format, mini stats, lịch sử giao dịch + status badges
 * - LichSu: bảng + status badges
 * - QRDelivery: tab filters + QR images load (naturalWidth > 0)
 * - CaiDat: profile form
 * - ThongBao: list thông báo
 * - NhanTin: chat container
 *
 * Tài khoản: shipperz / shipz789 (USERS.shipper2)
 * Convention: KHÔNG dùng networkidle (SignalR giữ kết nối), nav timeout 30s, test timeout 90s
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pages/LoginPage';
import { USERS } from '../../fixtures/users';

const SHIPPER = USERS.shipper2;

// Test timeout toàn bộ suite: 90s
test.setTimeout(90_000);

// ─── Helper: Login shipper2 + redirect /Shipper ───
async function loginAs(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(SHIPPER.username, SHIPPER.password);
  // ponytail: login() có 429 retry; cold start đôi khi mất session → fallback goto /Shipper
  if (!url.includes('/Shipper')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try {
        await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 20_000 });
        if (page.url().includes('/Shipper')) break;
      } catch {
        await page.waitForTimeout(1000);
      }
    }
  }
  await expect(page).toHaveURL(/\/Shipper/, { timeout: 30_000 });
}

// ─── Helper: Nav tới page shipper (domcontentloaded, KHÔNG networkidle) ───
async function navTo(page: any, path: string) {
  await page.goto(path, { waitUntil: 'domcontentloaded', timeout: 30_000 });
  await page.waitForTimeout(3000);
}

// ─── Helper: Lấy madh đầu tiên từ dashboard (link OrderDetail) ───
async function getFirstOrderId(page: any): Promise<string | null> {
  const href = await page.locator('a[href*="/Shipper/OrderDetail/"]').first().getAttribute('href').catch(() => null);
  const m = href?.match(/OrderDetail\/(\d+)/);
  return m ? m[1] : null;
}

// ─── TEST SUITE 1: Dashboard ───
test.describe('Shipper Dashboard - element inventory', () => {

  test('[TC-47-01] Dashboard - profile, tabs, quick stats and Leaflet map', async ({ page }) => {
    await loginAs(page);

    // Profile card
    const profileName = page.locator('.profile-card .profile-name');
    await expect(profileName).toBeVisible({ timeout: 10_000 });
    console.log(`👤 Shipper: ${(await profileName.textContent())?.trim()}`);
    expect(await page.locator('.profile-card .profile-avatar').count()).toBeGreaterThanOrEqual(1);
    expect(await page.locator('.profile-card .status-dot').count()).toBeGreaterThanOrEqual(1);

    // Tabs FREE-PICK / ĐƠN HÀNG
    const tabs = page.locator('.tab-group .tab-btn');
    const tabCount = await tabs.count();
    const tabTexts = (await tabs.allTextContents()).map(t => t.trim());
    console.log(`📑 Tabs (${tabCount}): ${tabTexts.join(' | ')}`);
    expect(tabCount).toBeGreaterThanOrEqual(2);
    expect(tabTexts.join(' ').toUpperCase()).toContain('FREE-PICK');
    expect(tabTexts.join(' ').toUpperCase()).toContain('ĐƠN HÀNG');
    expect(await page.locator('#tab-freepick').count()).toBeGreaterThanOrEqual(1);
    expect(await page.locator('#tab-orders').count()).toBeGreaterThanOrEqual(1);

    // Quick stats — 4 stat items
    const stats = page.locator('.quick-stats .stat-item');
    const statCount = await stats.count();
    console.log(`📊 Quick stats: ${statCount}`);
    expect(statCount).toBeGreaterThanOrEqual(4);
    for (let i = 0; i < Math.min(statCount, 4); i++) {
      const label = await stats.nth(i).locator('.label').textContent().catch(() => '');
      const value = await stats.nth(i).locator('.value').textContent().catch(() => '');
      console.log(`  Stat ${i}: ${label?.trim()} = ${value?.replace(/\s+/g, ' ').trim()}`);
    }

    // Leaflet map #shipper-map
    expect(await page.locator('#shipper-map').count()).toBe(1);
    const mapContainer = page.locator('#shipper-map.leaflet-container, #shipper-map').first();
    const mapVisible = await mapContainer.isVisible().catch(() => false);
    console.log(`🗺️ #shipper-map visible: ${mapVisible}`);
    if (mapVisible) {
      const tileCount = await page.locator('#shipper-map img.leaflet-tile').count();
      console.log(`  Leaflet tiles: ${tileCount}`);
    }
  });

  test('[TC-47-02] Dashboard - order cards, action buttons and refresh', async ({ page }) => {
    await loginAs(page);

    // Order cards
    const cards = page.locator('.order-grid .order-card');
    const cardCount = await cards.count();
    console.log(`📦 Order cards: ${cardCount}`);
    if (cardCount > 0) {
      const first = cards.first();
      const orderId = await first.locator('.order-id span').first().textContent();
      const badge = await first.locator('.order-id .badge').textContent();
      const infoRows = await first.locator('.order-info .info-row').count();
      console.log(`  #${orderId?.trim()} [${badge?.trim()}] — info rows: ${infoRows}`);
      expect(orderId).toMatch(/^#\d+/);
      expect(await first.locator('.order-actions').count()).toBeGreaterThanOrEqual(1);
    } else {
      console.log('ℹ️ Không có order cards — check empty state');
      expect(await page.locator('.empty-state').count()).toBeGreaterThanOrEqual(1);
    }

    // Action buttons: Nhận đơn + Chi tiết
    const acceptBtns = page.locator('.order-card .btn-accept');
    const detailLinks = page.locator('.order-card a[href*="/Shipper/OrderDetail/"]');
    console.log(`🟢 Nhận đơn: ${await acceptBtns.count()} | 🔍 Chi tiết: ${await detailLinks.count()}`);
    expect(await acceptBtns.count() + await detailLinks.count()).toBeGreaterThanOrEqual(1);

    // Refresh button
    expect(await page.locator('a:has-text("Làm mới")').count()).toBeGreaterThanOrEqual(1);
  });
});

// ─── TEST SUITE 2: OrderDetail ───
test.describe('Shipper OrderDetail - element inventory', () => {

  test('[TC-47-03] OrderDetail - order info, items table, payment and status buttons', async ({ page }) => {
    await loginAs(page);
    const orderId = await getFirstOrderId(page);
    if (!orderId) {
      console.log('ℹ️ Không tìm thấy đơn hàng nào để mở chi tiết');
      return;
    }
    await navTo(page, `/Shipper/OrderDetail/${orderId}`);

    // Tiêu đề đơn + breadcrumb
    const heading = page.locator('.form-head h2, h2:has-text("Mã đơn hàng")').first();
    await expect(heading).toBeVisible({ timeout: 10_000 });
    console.log(`📄 ${(await heading.textContent())?.trim()}`);
    expect(await heading.textContent()).toContain(`#${orderId}`);

    // Items table
    const itemRows = page.locator('.items-table tbody tr');
    const itemCount = await itemRows.count();
    console.log(`🍽️ Món trong đơn: ${itemCount}`);
    if (itemCount > 0) {
      expect(itemCount).toBeGreaterThanOrEqual(1);
      const firstItem = (await itemRows.first().textContent())?.replace(/\s+/g, ' ').trim();
      console.log(`  ${firstItem?.substring(0, 80)}`);
    }

    // Payment blocks (Trả quán / Phí ship / Thu khách)
    const payments = page.locator('.thanhtoan');
    const payCount = await payments.count();
    console.log(`💵 Payment blocks: ${payCount}`);
    for (let i = 0; i < Math.min(payCount, 4); i++) {
      console.log(`  ${(await payments.nth(i).textContent())?.replace(/\s+/g, ' ').trim()}`);
    }

    // Nút trạng thái: Nhận đơn (chưa nhận) hoặc Lấy hàng / Đang giao / Hoàn thành
    const claimBtn = page.locator('#btnClaim');
    if (await claimBtn.count() > 0) {
      console.log('🟢 Đơn chưa nhận — nút "Nhận đơn này" hiển thị');
      expect(await claimBtn.isVisible().catch(() => false)).toBeTruthy();
    } else {
      for (const sel of ['#btnPickup', '#btnDelivering', '#btnComplete']) {
        const count = await page.locator(sel).count();
        const disabled = count > 0 ? await page.locator(sel).isDisabled().catch(() => false) : false;
        console.log(`  ${sel}: count=${count}, disabled=${disabled}`);
      }
      expect(await page.locator('#btnPickup, #btnDelivering, #btnComplete').count()).toBeGreaterThanOrEqual(1);
    }
  });

  test('[TC-47-04] OrderDetail - QR card, live map and chat panel', async ({ page }) => {
    await loginAs(page);
    const orderId = await getFirstOrderId(page);
    if (!orderId) {
      console.log('ℹ️ Không tìm thấy đơn hàng nào để mở chi tiết');
      return;
    }
    await navTo(page, `/Shipper/OrderDetail/${orderId}`);

    // QR card — chờ img complete (display:none tới khi onload)
    const qrImg = page.locator('img[src*="GenerateQR"]').first();
    await qrImg.waitFor({ state: 'attached', timeout: 15_000 });
    await page.waitForFunction(() => {
      const img = document.querySelector('img[src*="GenerateQR"]') as HTMLImageElement | null;
      return !!img && img.complete;
    }, undefined, { timeout: 15_000 }).catch(() => console.log('ℹ️ QR image chưa complete trong 15s'));
    const qrOk = await qrImg.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
    console.log(`📸 QR image load (naturalWidth>0): ${qrOk}`);
    expect(qrOk).toBeTruthy();
    expect(await page.locator('a:has-text("Tải QR")').count()).toBeGreaterThanOrEqual(1);
    expect(await page.locator('a:has-text("DS QR")').count()).toBeGreaterThanOrEqual(1);

    // Live map (Leaflet #live-map)
    expect(await page.locator('#live-map').count()).toBeGreaterThanOrEqual(1);
    const liveMapVisible = await page.locator('#live-map').first().isVisible().catch(() => false);
    console.log(`🗺️ #live-map visible: ${liveMapVisible}`);

    // Chat panel
    expect(await page.locator('#shipperChatFab').count()).toBeGreaterThanOrEqual(1);
    expect(await page.locator('#shipperChatPanel').count()).toBeGreaterThanOrEqual(1);
    const msgCount = await page.locator('#shipperChatMsgs .s-msg').count();
    const quickReplies = await page.locator('.shipper-quick-replies .shipper-qr-btn').count();
    console.log(`💬 Chat msgs: ${msgCount} | ⚡ Quick replies: ${quickReplies}`);
    expect(await page.locator('#shipperChatInput').count()).toBe(1);

    // Customer info blocks
    const iconboxes = await page.locator('.iconbox').count();
    console.log(`👤 Customer info blocks: ${iconboxes}`);
    expect(iconboxes).toBeGreaterThanOrEqual(3);
  });
});

// ─── TEST SUITE 3: ThuNhap ───
test.describe('Shipper ThuNhap - element inventory', () => {

  test('[TC-47-05] ThuNhap - bank card, 4 stat cards and Chart.js canvas', async ({ page }) => {
    await loginAs(page);
    await navTo(page, '/Shipper/ThuNhap');

    // Bank card — balance VND format
    const balanceEl = page.locator('.bank-card .balance-amount');
    await expect(balanceEl).toBeVisible({ timeout: 10_000 });
    const balanceText = (await balanceEl.textContent())?.trim() || '';
    console.log(`💳 Tổng thu nhập 30 ngày: ${balanceText}`);
    expect(balanceText).toMatch(/\d/);
    expect(balanceText).toContain('đ');

    const cardNumber = ((await page.locator('.bank-card .card-number').textContent()) || '').replace(/\s+/g, ' ').trim();
    console.log(`  Card: ${cardNumber}`);
    expect(cardNumber).toMatch(/^\*{4} \*{4} \*{4} \d{4}$/);

    // 4 stats cards
    const stats = page.locator('.income-stats .income-stat-card');
    const statCount = await stats.count();
    console.log(`📊 Income stats: ${statCount}`);
    expect(statCount).toBeGreaterThanOrEqual(4);
    for (let i = 0; i < Math.min(statCount, 4); i++) {
      const label = await stats.nth(i).locator('.label').textContent().catch(() => '');
      const value = await stats.nth(i).locator('.value').textContent().catch(() => '');
      console.log(`  ${label?.trim()}: ${value?.trim()}`);
    }

    // Chart.js canvas
    expect(await page.locator('#incomeChart').count()).toBe(1);
    const canvasVisible = await page.locator('#incomeChart').isVisible().catch(() => false);
    console.log(`📈 #incomeChart visible: ${canvasVisible}`);
    expect(canvasVisible).toBeTruthy();

    // Badge 30 ngày
    const badgeText = (await page.locator('.chart-section .badge').textContent())?.trim() || '';
    console.log(`📅 Chart badge: ${badgeText}`);
    expect(badgeText).toContain('30 ngày');
  });
});

// ─── TEST SUITE 4: ViTien ───
test.describe('Shipper ViTien - element inventory', () => {

  test('[TC-47-06] ViTien - balance VND format, mini stats and transactions', async ({ page }) => {
    await loginAs(page);
    await navTo(page, '/Shipper/ViTien');

    // Wallet card — số dư khả dụng
    const balanceEl = page.locator('.wallet-card .wallet-balance');
    await expect(balanceEl).toBeVisible({ timeout: 10_000 });
    const balanceText = (await balanceEl.textContent())?.trim() || '';
    console.log(`👛 Số dư: ${balanceText}`);
    expect(balanceText).toMatch(/\d/);
    expect(balanceText).toContain('đ');
    const walletLabel = (await page.locator('.wallet-card .wallet-label').textContent())?.trim();
    console.log(`  Label: ${walletLabel}`);
    expect(walletLabel).toContain('Số dư');

    // Action buttons Nạp / Rút
    const actions = page.locator('.wallet-actions .btn-action');
    expect(await actions.count()).toBeGreaterThanOrEqual(2);
    console.log(`🔘 Actions: ${(await actions.allTextContents()).map(t => t.trim()).join(' | ')}`);

    // Mini stats
    const miniStats = page.locator('.wallet-mini-stats .wallet-mini-stat');
    const miniCount = await miniStats.count();
    console.log(`📊 Mini stats: ${miniCount}`);
    expect(miniCount).toBeGreaterThanOrEqual(2);
    for (let i = 0; i < Math.min(miniCount, 2); i++) {
      console.log(`  ${(await miniStats.nth(i).textContent())?.replace(/\s+/g, ' ').trim()}`);
    }

    // Transactions list
    const txItems = page.locator('.tx-list .tx-item');
    const txCount = await txItems.count();
    console.log(`📋 Transactions: ${txCount}`);
    if (txCount > 0) {
      expect(await txItems.first().locator('.tx-title').textContent()).toContain('Đơn hàng');
      expect(await txItems.first().locator('.tx-amount').textContent()).toMatch(/đ$/);
      const statusCount = await page.locator('.tx-status').count();
      expect(statusCount).toBeGreaterThanOrEqual(1);
      console.log(`  Statuses: ${(await page.locator('.tx-status').allTextContents()).map(s => s.trim()).join(' | ')}`);
    } else {
      console.log('ℹ️ Không có giao dịch — check empty state');
      expect(await page.locator('.empty-wallet').count()).toBeGreaterThanOrEqual(1);
    }
  });
});

// ─── TEST SUITE 5: LichSu ───
test.describe('Shipper LichSu - element inventory', () => {

  test('[TC-47-07] LichSu - history table and status badges', async ({ page }) => {
    await loginAs(page);
    await navTo(page, '/Shipper/LichSu');

    const heading = await page.locator('.welcome-text h4').textContent();
    console.log(`📄 ${heading?.trim()}`);
    expect(heading).toContain('Lịch sử');

    // Bảng lịch sử
    const table = page.locator('.table-responsive table tbody#orders');
    expect(await table.count()).toBeGreaterThanOrEqual(1);
    const rows = table.locator('tr');
    const rowCount = await rows.count();
    console.log(`📋 History rows: ${rowCount}`);

    if (rowCount > 0) {
      const firstText = (await rows.first().textContent()) || '';
      expect(firstText).toMatch(/#\d+/);
      expect(firstText).toContain('VND');
      console.log(`  ${firstText.replace(/\s+/g, ' ').trim().substring(0, 120)}`);
    }

    // Status badges
    const badges = page.locator('tbody#orders .badge');
    const badgeCount = await badges.count();
    console.log(`🏷️ Status badges: ${badgeCount}`);
    if (badgeCount > 0) {
      for (let i = 0; i < Math.min(badgeCount, 4); i++) {
        console.log(`  ${(await badges.nth(i).textContent())?.replace(/\s+/g, ' ').trim()}`);
      }
    }

    // Detail links
    console.log(`🔗 Detail links: ${await page.locator('a[href*="OrderDetail"]').count()}`);
  });
});

// ─── TEST SUITE 6: QRDelivery ───
test.describe('Shipper QRDelivery - element inventory', () => {

  test('[TC-47-08] QRDelivery - QR tab filters and QR images load', async ({ page }) => {
    await loginAs(page);
    await navTo(page, '/Shipper/QRDelivery');

    const h2 = await page.locator('.qr-page h2').textContent();
    console.log(`📄 ${h2?.trim()}`);
    expect(h2).toContain('Mã QR');

    // Tab filters (Chờ giao / Đang giao / Hoàn thành)
    const tabs = page.locator('.qr-tabs .qr-tab-btn');
    const tabCount = await tabs.count();
    console.log(`🔘 QR tabs (${tabCount}): ${(await tabs.allTextContents()).map(t => t.trim()).join(' | ')}`);
    expect(tabCount).toBeGreaterThanOrEqual(3);
    await page.locator('.qr-tab-btn[data-tab="completed"]').click();
    await page.waitForTimeout(500);
    const completedVisible = await page.locator('.qr-tab-content[data-tab="completed"]').isVisible().catch(() => false);
    console.log(`✅ Completed tab visible: ${completedVisible}`);

    // QR cards
    const cardCount = await page.locator('.qr-order-card').count();
    console.log(`🃏 QR cards: ${cardCount}`);
    expect(await page.locator('.qr-order-grid, .qr-empty').count()).toBeGreaterThanOrEqual(1);

    // QR images load (naturalWidth > 0)
    const qrImgs = page.locator('img[src*="GenerateQR"]');
    const qrCount = await qrImgs.count();
    let loaded = 0;
    for (let i = 0; i < Math.min(qrCount, 10); i++) {
      const img = qrImgs.nth(i);
      await page.waitForFunction((sel) => {
        const els = Array.from(document.querySelectorAll(sel));
        return els.length === 0 || els.every((el) => (el as HTMLImageElement).complete);
      }, 'img[src*="GenerateQR"]', { timeout: 15_000 }).catch(() => {});
      const ok = await img.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0).catch(() => false);
      if (ok) loaded++;
    }
    console.log(`📸 QR loaded: ${loaded}/${qrCount}`);
    if (qrCount > 0) {
      expect(loaded).toBeGreaterThanOrEqual(1);
    } else {
      console.log('ℹ️ Không có QR code — check empty state');
      expect(await page.locator('.qr-empty').count()).toBeGreaterThanOrEqual(1);
    }
  });
});

// ─── TEST SUITE 7: CaiDat ───
test.describe('Shipper CaiDat - element inventory', () => {

  test('[TC-47-09] CaiDat - profile stats and settings form', async ({ page }) => {
    await loginAs(page);
    await navTo(page, '/Shipper/CaiDat');

    // Profile name
    const profileName = page.locator('.profile-name h4, .profile-details .profile-name h4').first();
    expect(await profileName.count()).toBeGreaterThanOrEqual(1);
    console.log(`👤 Profile: ${(await profileName.textContent())?.trim()}`);

    // 4 cột thống kê (Điểm đánh giá / Số lượt đánh giá / SĐT / Tổng đơn)
    const cols = page.locator('.row .col h3.m-b-0');
    const colCount = await cols.count();
    console.log(`📊 Profile stats: ${colCount}`);
    for (let i = 0; i < Math.min(colCount, 4); i++) {
      console.log(`  ${(await cols.nth(i).textContent())?.trim()}`);
    }

    // Settings form
    const form = page.locator('.settings-form form, form');
    expect(await form.count()).toBeGreaterThanOrEqual(1);
    const diachiInput = page.locator('input[name="diachi"]');
    const pwdInput = page.locator('input#pwd, input[name="pwd"]');
    const sdtInput = page.locator('input#sdt, input[name="sdt"]');
    console.log(`📍 diachi: ${await diachiInput.count()} | 🔒 pwd: ${await pwdInput.count()} | 📞 sdt: ${await sdtInput.count()}`);
    expect(await diachiInput.count()).toBe(1);
    expect(await pwdInput.count()).toBe(1);
    expect(await sdtInput.count()).toBe(1);

    const submitBtn = page.locator('.settings-form button[type="submit"]');
    expect(await submitBtn.count()).toBeGreaterThanOrEqual(1);
    console.log(`💾 Submit: ${(await submitBtn.textContent())?.trim()}`);
  });
});

// ─── TEST SUITE 8: ThongBao + NhanTin ───
test.describe('Shipper ThongBao + NhanTin - element inventory', () => {

  test('[TC-47-10] ThongBao notifications and NhanTin chat container', async ({ page }) => {
    await loginAs(page);

    // ── ThongBao ──
    await navTo(page, '/Shipper/ThongBao');
    const header = await page.locator('.notif-header h2').textContent();
    console.log(`📄 ${header?.trim()}`);
    expect(header).toContain('Thông báo');

    const notifItems = page.locator('.notif-feed .notif-item');
    const notifCount = await notifItems.count();
    console.log(`🔔 Notifications: ${notifCount}`);
    if (notifCount > 0) {
      const first = notifItems.first();
      console.log(`  Title: ${(await first.locator('.notif-title').textContent())?.trim()}`);
      expect(await first.locator('.notif-title').count()).toBe(1);
      expect(await first.locator('.notif-desc').count()).toBe(1);
      expect(await first.locator('.notif-time').count()).toBe(1);
      expect(await first.locator('.notif-icon').count()).toBe(1);
      console.log(`  Unread items: ${await page.locator('.notif-item.unread').count()}`);
    } else {
      console.log('ℹ️ Không có thông báo — check empty state');
      expect(await page.locator('.notif-empty').count()).toBeGreaterThanOrEqual(1);
    }

    // ── NhanTin ──
    await navTo(page, '/Shipper/NhanTin');
    const conversationList = page.locator('#conversationList');
    await expect(conversationList).toBeVisible({ timeout: 10_000 });
    const convItems = await page.locator('#conversationList .list-group-item').count();
    console.log(`💬 Conversations: ${convItems}`);
    expect(await page.locator('#chatMessages').count()).toBe(1);
    console.log(`🧑 Partner: ${(await page.locator('#chatPartnerName').textContent())?.trim()}`);

    // Input area ẩn khi chưa chọn hội thoại
    expect(await page.locator('#chatInputArea').count()).toBe(1);
    const inputVisible = await page.locator('#chatInputArea').isVisible().catch(() => false);
    console.log(`⌨️ Chat input visible (trước chọn hội thoại): ${inputVisible}`);
    expect(await page.locator('#msgInput').count()).toBe(1);
  });
});
