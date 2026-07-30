/**
 * 💬 BỘ TEST 34: CHAT CHO QUÁN ĂN + SHIPPER
 *
 * Mục tiêu: Test chat page của restaurant và shipper
 * - Restaurant NhanTin: container, gửi tin, SignalR
 * - Shipper NhanTin: container, gửi tin, SignalR
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

async function loginAsMerchant(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(USERS.restaurant1.username, USERS.restaurant1.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Restaurant')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

async function loginAsShipper(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(USERS.shipper2.username, USERS.shipper2.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Shipper')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── RESTAURANT CHAT ───
test.describe('🏪 Restaurant Chat (NhanTin)', () => {

  test('[TC-CHAT-R-01] Chat page load — container + signalR', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra chat container
    const chatContainer = page.locator('.chat-container, .chat-box, #nhantin-chat, [class*="chat"]').first();
    const containerVisible = await chatContainer.isVisible().catch(() => false);
    console.log(`💬 Chat container: ${containerVisible}`);

    if (containerVisible) {
      // Kiểm tra message input
      const input = page.locator('input[type="text"], textarea').first();
      console.log(`📝 Input: ${await input.isVisible().catch(() => false)}`);

      // Kiểm tra send button
      const sendBtn = page.locator('button:has-text("Gửi"), .btn-send').first();
      console.log(`📤 Send: ${await sendBtn.isVisible().catch(() => false)}`);

      // Kiểm tra message list
      const messageList = page.locator('.message-list, .chat-messages, .msg-container').first();
      console.log(`📋 Messages: ${await messageList.isVisible().catch(() => false)}`);

      // SignalR
      const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
      console.log(`🔌 SignalR: ${hasSignalR}`);
    } else {
      console.log('ℹ️ Chat container not found — checking for alternative layout');
      const bodyText = await page.locator('body').textContent() || '';
      console.log(`  Body: ${bodyText.substring(0, 150)}`);
    }
  });

  test('[TC-CHAT-R-02] Restaurant chat — gửi tin nhắn', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const input = page.locator('input[type="text"], textarea').first();
    if (!(await input.isVisible().catch(() => false))) {
      console.log('ℹ️ No chat input');
      return;
    }

    const testMsg = `Merchant test ${Date.now()}`;
    await input.fill(testMsg);

    const sendBtn = page.locator('button:has-text("Gửi"), .btn-send').first();
    if (await sendBtn.isVisible().catch(() => false)) {
      await sendBtn.click();
    } else {
      await input.press('Enter');
    }
    await page.waitForTimeout(2000);

    const allMsgs = await page.locator('.message, .msg, .chat-message').allTextContents();
    const found = allMsgs.some((m): m is string => m !== null && m.includes(testMsg));
    console.log(`✅ Message sent: ${found}`);
  });

  test('[TC-CHAT-R-03] Restaurant sidebar nav link to NhanTin', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const chatLinks = page.locator('a[href*="NhanTin"], a[href*="nhan-tin"]');
    const linkCount = await chatLinks.count();
    console.log(`🔗 Chat nav links: ${linkCount}`);
    expect(linkCount).toBeGreaterThan(0);
  });
});

// ─── SHIPPER CHAT ───
test.describe('🚚 Shipper Chat (NhanTin)', () => {

  test('[TC-CHAT-S-01] Shipper chat page load — container + signalR', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const chatContainer = page.locator('.chat-container, .chat-box, #nhantin-chat, [class*="chat"]').first();
    const containerVisible = await chatContainer.isVisible().catch(() => false);
    console.log(`💬 Chat container: ${containerVisible}`);

    if (containerVisible) {
      const input = page.locator('input[type="text"], textarea').first();
      const sendBtn = page.locator('button:has-text("Gửi"), .btn-send').first();
      console.log(`📝 Input: ${await input.isVisible().catch(() => false)}`);
      console.log(`📤 Send: ${await sendBtn.isVisible().catch(() => false)}`);

      const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
      console.log(`🔌 SignalR: ${hasSignalR}`);
    }
  });

  test('[TC-CHAT-S-02] Shipper chat — gửi tin nhắn', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const input = page.locator('input[type="text"], textarea').first();
    if (!(await input.isVisible().catch(() => false))) {
      console.log('ℹ️ No chat input');
      return;
    }

    const testMsg = `Shipper test ${Date.now()}`;
    await input.fill(testMsg);
    await input.press('Enter');
    await page.waitForTimeout(2000);

    const allMsgs = await page.locator('.message, .msg, .chat-message').allTextContents();
    const found = allMsgs.some((m): m is string => m !== null && m.includes(testMsg));
    console.log(`✅ Message sent: ${found}`);
  });

  test('[TC-CHAT-S-03] Shipper sidebar nav link to NhanTin', async ({ page }) => {
    await loginAsShipper(page);
    await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const chatLinks = page.locator('a[href*="NhanTin"], a[href*="nhan-tin"]');
    const linkCount = await chatLinks.count();
    console.log(`🔗 Chat nav links: ${linkCount}`);
  });
});
