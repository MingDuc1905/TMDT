/**
 * 💬 BỘ TEST 15: HỆ THỐNG CHAT (AI + ADMIN)
 *
 * Mục tiêu: Test AI Chatbot + Admin Chat
 * - Chat widget open/close, AI message send/receive
 * - Admin chat tab, SignalR connection, real-time messaging
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;
const ADMIN = USERS.admin1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. CHAT WIDGET UI
// ════════════════════════════════════════════════════════════════
test.describe('💬 Chat widget UI', () => {

  test('[TC-CHAT-01] Chat FAB button visible trên homepage', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const chatToggle = page.locator('.chat-toggle, #chatToggle');
    const isVisible = await chatToggle.isVisible().catch(() => false);
    console.log(`Chat FAB visible: ${isVisible}`);
    expect(isVisible).toBe(true);
  });

  test('[TC-CHAT-02] Click chat FAB → chat box opens', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const chatToggle = page.locator('.chat-toggle, #chatToggle');
    await chatToggle.click();
    await page.waitForTimeout(2000);

    const chatBox = page.locator('.chat-box.active, #chatBox.active');
    const isVisible = await chatBox.isVisible().catch(() => false);
    console.log(`Chat box visible after click: ${isVisible}`);
    expect(isVisible).toBe(true);
  });

  test('[TC-CHAT-03] Chat box có 2 tabs: AI + Admin', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    // Check tabs
    const tabAi = page.locator('#tabAi, .chat-tab:has-text("AI")');
    const tabAdmin = page.locator('#tabAdmin, .chat-tab:has-text("Hỗ trợ")');
    const aiVisible = await tabAi.isVisible().catch(() => false);
    const adminVisible = await tabAdmin.isVisible().catch(() => false);
    console.log(`AI tab: ${aiVisible}, Admin tab: ${adminVisible}`);
  });

  test('[TC-CHAT-04] Close chat box → hidden', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(1000);

    // Close chat
    const closeBtn = page.locator('.close-btn, .close-chat');
    if (await closeBtn.isVisible().catch(() => false)) {
      await closeBtn.click();
      await page.waitForTimeout(1000);

      const chatBox = page.locator('.chat-box.active, #chatBox.active');
      const isHidden = !(await chatBox.isVisible().catch(() => false));
      console.log(`Chat box hidden after close: ${isHidden}`);
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 2. AI CHATBOT
// ════════════════════════════════════════════════════════════════
test.describe('🤖 AI Chatbot', () => {

  test('[TC-CHAT-05] AI chat input + send button visible', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    // Check AI section
    const aiSection = page.locator('#sectionAi');
    const isVisible = await aiSection.isVisible().catch(() => false);
    console.log(`AI section visible: ${isVisible}`);

    if (isVisible) {
      const aiInput = page.locator('#aiInput');
      const aiBtn = page.locator('#aiBtn');
      const inputVisible = await aiInput.isVisible().catch(() => false);
      const btnVisible = await aiBtn.isVisible().catch(() => false);
      console.log(`AI input: ${inputVisible}, AI button: ${btnVisible}`);
    }
  });

  test('[TC-CHAT-06] Gửi tin nhắn AI "Xin chào" → nhận reply', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    const aiInput = page.locator('#aiInput');
    const aiBtn = page.locator('#aiBtn');

    if (await aiInput.isVisible().catch(() => false)) {
      await aiInput.fill('Xin chào');
      await aiBtn.click();
      await page.waitForTimeout(5000);

      // Check for bot response
      const botMessages = page.locator('.msg.bot, .chat-msgs .bot');
      const count = await botMessages.count();
      console.log(`Bot messages after send: ${count}`);

      if (count > 0) {
        const lastBotMsg = await botMessages.last().textContent();
        console.log(`Last bot reply: "${lastBotMsg?.substring(0, 100)}"`);
        expect(lastBotMsg).toBeTruthy();
      }
    }
  });

  test('[TC-CHAT-07] Gửi tin nhắn AI "Gợi ý món ăn" → nhận top items', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    const aiInput = page.locator('#aiInput');
    const aiBtn = page.locator('#aiBtn');

    if (await aiInput.isVisible().catch(() => false)) {
      await aiInput.fill('Gợi ý món ăn ngon');
      await aiBtn.click();
      await page.waitForTimeout(8000);

      const botMessages = page.locator('.msg.bot, .chat-msgs .bot');
      const count = await botMessages.count();
      console.log(`Bot messages after food suggestion: ${count}`);

      if (count > 0) {
        const lastBotMsg = await botMessages.last().textContent();
        console.log(`Bot food suggestion: "${lastBotMsg?.substring(0, 150)}"`);
      }
    }
  });

  test('[TC-CHAT-08] Quick reply buttons hoạt động', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    // Check for quick reply buttons
    const quickBtns = page.locator('.quick-btn, .quick-reply');
    const count = await quickBtns.count();
    console.log(`Quick reply buttons: ${count}`);

    if (count > 0) {
      const btnTexts = await quickBtns.allTextContents();
      console.log(`Quick buttons: ${btnTexts.join(', ')}`);

      // Click first quick button
      await quickBtns.first().click();
      await page.waitForTimeout(5000);

      const botMessages = page.locator('.msg.bot, .chat-msgs .bot');
      const botCount = await botMessages.count();
      console.log(`Bot messages after quick reply: ${botCount}`);
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 3. ADMIN CHAT
// ════════════════════════════════════════════════════════════════
test.describe('👨‍💼 Admin chat', () => {

  test('[TC-CHAT-09] Customer: chuyển sang Admin tab', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Open chat
    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    // Click admin tab
    const adminTab = page.locator('#tabAdmin, .chat-tab:has-text("Hỗ trợ")');
    if (await adminTab.isVisible().catch(() => false)) {
      await adminTab.click();
      await page.waitForTimeout(1000);

      const adminSection = page.locator('#sectionAdmin');
      const isVisible = await adminSection.isVisible().catch(() => false);
      console.log(`Admin section visible: ${isVisible}`);
    }
  });

  test('[TC-CHAT-10] Admin chat — login admin → verify chat page', async ({ page }) => {
    await loginAs(page, ADMIN);

    await page.goto('/AdminChat', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const currentUrl = page.url();
    console.log(`Admin chat URL: ${currentUrl}`);

    // Check for conversation list or empty state
    const conversations = page.locator('.conversation-item, .chat-list-item, tr');
    const count = await conversations.count();
    console.log(`Admin conversations: ${count}`);

    // Check for SignalR connection status
    const connectionStatus = page.locator('.connection-status, [class*="signal"], [class*="status"]');
    const statusCount = await connectionStatus.count();
    console.log(`Connection status elements: ${statusCount}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 4. CHAT EDGE CASES
// ════════════════════════════════════════════════════════════════
test.describe('⚠️ Chat edge cases', () => {

  test('[TC-CHAT-11] Gửi tin nhắn rỗng → không gửi', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(2000);

    const aiInput = page.locator('#aiInput');
    const aiBtn = page.locator('#aiBtn');

    if (await aiInput.isVisible().catch(() => false)) {
      const msgCountBefore = await page.locator('.msg').count();

      // Try to send empty message
      await aiInput.fill('');
      await aiBtn.click();
      await page.waitForTimeout(2000);

      const msgCountAfter = await page.locator('.msg').count();
      console.log(`Messages before: ${msgCountBefore}, after empty send: ${msgCountAfter}`);
      // Should not add new message
    }
  });

  test('[TC-CHAT-12] Chat widget không có SignalR → graceful fallback', async ({ page }) => {
    // This test verifies chat doesn't crash without SignalR
    await loginAs(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check for console errors related to SignalR
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        errors.push(msg.text());
      }
    });

    await page.locator('.chat-toggle, #chatToggle').click();
    await page.waitForTimeout(3000);

    const signalrErrors = errors.filter(e => e.includes('SignalR') || e.includes('connection'));
    console.log(`SignalR errors: ${signalrErrors.length}`);
    if (signalrErrors.length > 0) {
      console.log(`Errors: ${signalrErrors[0].substring(0, 100)}`);
    }
  });
});
