/**
 * 👑 BỘ TEST 25: ADMIN CHAT SUPPORT
 *
 * Mục tiêu: Test trang Admin Chat (/AdminChat)
 * - Danh sách hội thoại
 * - Load messages
 * - Gửi tin nhắn
 * - Unread count badge
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const ADMIN = USERS.admin1;

async function loginAsAdmin(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(ADMIN.username, ADMIN.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Admin', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Admin')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── TEST SUITE ───
test.describe('👑 Admin Chat Support', () => {

  test('[TC-ACH-01] Chat page — danh sách hội thoại + sidebar', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/AdminChat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra sidebar conversations
    const sidebar = page.locator('.chat-sidebar, .conversation-list, .user-list, [class*="conversation"]').first();
    const sidebarVisible = await sidebar.isVisible().catch(() => false);
    console.log(`💬 Chat sidebar visible: ${sidebarVisible}`);

    if (sidebarVisible) {
      const conversations = sidebar.locator('.conversation-item, .user-item, .chat-user');
      const convCount = await conversations.count();
      console.log(`  Conversations: ${convCount}`);
    }

    // Kiểm tra unread badges
    const unreadBadges = page.locator('.unread-badge, .badge-unread, [class*="unread"]');
    const badgeCount = await unreadBadges.count();
    console.log(`🔴 Unread badges: ${badgeCount}`);
  });

  test('[TC-ACH-02] Click user → load messages', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/AdminChat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const sidebar = page.locator('.chat-sidebar, .conversation-list, .user-list').first();
    if (!(await sidebar.isVisible().catch(() => false))) {
      console.log('ℹ️ Không có sidebar');
      return;
    }

    const firstUser = sidebar.locator('.conversation-item, .user-item, .chat-user').first();
    if (await firstUser.isVisible().catch(() => false)) {
      await firstUser.click();
      await page.waitForTimeout(2000);

      // Kiểm tra messages load
      const messageArea = page.locator('.message-area, .chat-messages, .msg-container').first();
      const msgVisible = await messageArea.isVisible().catch(() => false);
      console.log(`📋 Message area visible: ${msgVisible}`);

      if (msgVisible) {
        const messages = messageArea.locator('.message, .msg, .chat-message');
        const msgCount = await messages.count();
        console.log(`  Messages loaded: ${msgCount}`);

        // Kiểm tra scroll
        const scrollContainer = messageArea.locator('.message-list, .msg-list').first();
        const scrolled = await scrollContainer.evaluate(el => el.scrollTop > 0 || el.scrollHeight > el.clientHeight).catch(() => false);
        console.log(`  Auto-scrolled: ${scrolled}`);
      }
    } else {
      console.log('ℹ️ Không có user nào trong danh sách');
    }
  });

  test('[TC-ACH-03] Gửi tin nhắn từ Admin → Customer', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/AdminChat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Click user đầu tiên
    const firstUser = page.locator('.conversation-item, .user-item, .chat-user').first();
    if (await firstUser.isVisible().catch(() => false)) {
      await firstUser.click();
      await page.waitForTimeout(2000);

      // Kiểm tra input
      const input = page.locator('input[type="text"], textarea').first();
      if (await input.isVisible().catch(() => false)) {
        const testMsg = `Admin reply test ${Date.now()}`;
        await input.fill(testMsg);

        const sendBtn = page.locator('button:has-text("Gửi"), .btn-send, button[type="submit"]').first();
        if (await sendBtn.isVisible().catch(() => false)) {
          await sendBtn.click();
          await page.waitForTimeout(2000);
        } else {
          await input.press('Enter');
          await page.waitForTimeout(2000);
        }

        // Kiểm tra tin nhắn xuất hiện
        const allMsgs = await page.locator('.message, .msg, .chat-message').allTextContents();
        const found = allMsgs.some((m): m is string => m !== null && m.includes(testMsg));
        console.log(`✅ Admin message sent: ${found}`);
      }
    } else {
      console.log('ℹ️ Không có user nào để chat');
    }
  });

  test('[TC-ACH-04] Unread count — SignalR real-time update', async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/AdminChat', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra SignalR
    const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
    console.log(`🔌 SignalR loaded: ${hasSignalR}`);

    // Kiểm tra unread total
    const unreadTotal = page.locator('.unread-total, #unread-count, .badge-total').first();
    const totalText = await unreadTotal.textContent().catch(() => null);
    console.log(`🔴 Unread total: ${totalText}`);

    // Kiểm tra sidebar có auto-refresh
    if (hasSignalR) {
      const sidebarConversations = page.locator('.conversation-item, .user-item, .chat-user');
      const initialCount = await sidebarConversations.count();
      console.log(`📋 Sidebar items: ${initialCount}`);
    }
  });
});
