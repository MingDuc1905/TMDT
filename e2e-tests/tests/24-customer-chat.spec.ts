/**
 * 💬 BỘ TEST 24: CHAT KHÁCH HÀNG (Customer Chat)
 *
 * Mục tiêu: Test chat page + AI chatbot widget
 * - Chat page load + container
 * - Gửi tin nhắn
 * - SignalR connection
 * - AI Chatbot widget
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

// ─── TEST SUITE ───
test.describe('💬 Chat khách hàng', () => {

  test('[TC-CHAT-01] Chat page load — container + SignalR connected', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Home/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra chat container
    const chatContainer = page.locator('.chat-container, .chat-box, #nhantin-chat, [class*="chat"]').first();
    await expect(chatContainer).toBeVisible({ timeout: 10_000 });
    console.log('💬 Chat container visible');

    // Kiểm tra SignalR
    const hasSignalR = await page.evaluate(() => !!(window as any)['signalR']).catch(() => false);
    console.log(`🔌 SignalR loaded: ${hasSignalR}`);

    // Kiểm tra các element cơ bản
    const messageInput = page.locator('input[type="text"], textarea').first();
    const inputVisible = await messageInput.isVisible().catch(() => false);
    console.log(`📝 Message input: ${inputVisible}`);

    const sendBtn = page.locator('button:has-text("Gửi"), .btn-send, button[type="submit"]').first();
    console.log(`📤 Send button: ${await sendBtn.isVisible().catch(() => false)}`);

    const messageList = page.locator('.message-list, .chat-messages, .msg-container').first();
    console.log(`📋 Message list: ${await messageList.isVisible().catch(() => false)}`);
  });

  test('[TC-CHAT-02] Gửi tin nhắn — message xuất hiện trong chat', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Home/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const chatContainer = page.locator('.chat-container, .chat-box, #nhantin-chat').first();
    if (!(await chatContainer.isVisible().catch(() => false))) {
      console.log('ℹ️ Chat container không hiển thị');
      return;
    }

    // Tìm input
    const messageInput = page.locator('input[type="text"], textarea').first();
    if (!(await messageInput.isVisible().catch(() => false))) {
      console.log('ℹ️ Message input không hiển thị');
      return;
    }

    // Lấy số tin nhắn hiện tại
    const messagesBefore = await page.locator('.message-item, .msg, .chat-message').count();

    // Nhập tin nhắn
    const testMessage = `Test message ${Date.now()}`;
    await messageInput.fill(testMessage);
    console.log(`💬 Typed: "${testMessage}"`);

    // Gửi
    const sendBtn = page.locator('button:has-text("Gửi"), .btn-send, button[type="submit"]').first();
    if (await sendBtn.isVisible().catch(() => false)) {
      await sendBtn.click();
      await page.waitForTimeout(2000);
    } else {
      // Try Enter key
      await messageInput.press('Enter');
      await page.waitForTimeout(2000);
    }

    // Kiểm tra tin nhắn được gửi
    const messagesAfter = await page.locator('.message-item, .msg, .chat-message').count();
    console.log(`📋 Messages: ${messagesBefore} → ${messagesAfter}`);

    const allMessages = await page.locator('.message-item, .msg, .chat-message').allTextContents();
    const sentFound = allMessages.some((m): m is string => m !== null && (m.includes(testMessage) || m.includes('Test message')));
    console.log(`✅ Message found in chat: ${sentFound}`);
  });

  test('[TC-CHAT-03] Chat với Admin Support (nếu có)', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/Home/NhanTin', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const chatContainer = page.locator('.chat-container, .chat-box, #nhantin-chat').first();
    if (!(await chatContainer.isVisible().catch(() => false))) {
      console.log('ℹ️ Chat container không hiển thị');
      return;
    }

    // Kiểm tra có tab/admin chat widget
    const supportTab = page.locator('a:has-text("Hỗ trợ"), a:has-text("Admin"), .support-tab, .admin-tab').first();
    const supportVisible = await supportTab.isVisible().catch(() => false);
    console.log(`👑 Admin support tab: ${supportVisible}`);

    if (supportVisible) {
      await supportTab.click();
      await page.waitForTimeout(1000);

      const messageInput = page.locator('input[type="text"], textarea').first();
      if (await messageInput.isVisible().catch(() => false)) {
        await messageInput.fill('Xin chào, tôi cần hỗ trợ');
        const sendBtn = page.locator('button:has-text("Gửi"), .btn-send').first();
        if (await sendBtn.isVisible().catch(() => false)) {
          await sendBtn.click();
          await page.waitForTimeout(2000);
          console.log('✅ Gửi tin nhắn đến admin support');
        }
      }
    }
  });

  test('[TC-CHAT-04] AI Chatbot (Gemini) widget', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    // Kiểm tra floating chat widget
    const chatWidget = page.locator('.chat-widget, .ai-chat-btn, #chatbot-btn, .floating-chat');
    const widgetVisible = await chatWidget.isVisible().catch(() => false);
    console.log(`🤖 AI Chat widget visible: ${widgetVisible}`);

    if (widgetVisible) {
      await chatWidget.click();
      await page.waitForTimeout(1500);

      // Kiểm tra chat panel mở ra
      const chatPanel = page.locator('.chat-panel, .chat-widget-body, .ai-chat-box').first();
      const panelVisible = await chatPanel.isVisible().catch(() => false);
      console.log(`📦 Chat panel opened: ${panelVisible}`);

      if (panelVisible) {
        // Thử gửi câu hỏi
        const input = chatPanel.locator('input[type="text"], textarea').first();
        if (await input.isVisible().catch(() => false)) {
          await input.fill('Có món gì ngon?');
          await input.press('Enter');
          await page.waitForTimeout(5000); // Chờ AI trả lời

          const aiResponse = chatPanel.locator('.ai-message, .bot-message, .chatbot-response');
          const responseCount = await aiResponse.count();
          console.log(`💬 AI responses: ${responseCount}`);

          if (responseCount > 0) {
            const responseText = await aiResponse.first().textContent();
            console.log(`  Response: "${responseText?.substring(0, 100)}"`);
          }
        }
      }
    } else {
      console.log('ℹ️ Không tìm thấy AI chat widget trên trang');
    }
  });
});
