// ====================================================================
// Lightpanda Browser — Playwright Custom Fixture
// ====================================================================
// Kết nối Playwright tới Lightpanda CDP server (Docker) thay vì
// dùng Chromium build-in. Chạy nhanh hơn 9x, ít RAM hơn 16x.
//
// Usage:
//   import { test, expect } from '../fixtures/lightpanda-fixture';
//
//   test('homepage loads', async ({ page }) => {
//     await page.goto('/');
//     await expect(page.locator('h1')).toBeVisible();
//   });
//
// Yêu cầu: Lightpanda Docker container đang chạy
//   docker compose up -d
// ====================================================================

import { test as base, expect, type Page, type Browser } from '@playwright/test';
import { chromium } from 'playwright';

// ─── Types ──────────────────────────────────────────────────────────
type LightpandaFixtures = {
  /** Page kết nối qua Lightpanda CDP */
  page: Page;
};

type LightpandaWorkerFixtures = {
  /** Browser instance kết nối tới Lightpanda */
  lightpandaBrowser: Browser;
};

// ─── CDP Endpoint ───────────────────────────────────────────────────
const LIGHTPANDA_CDP_URL = process.env.LIGHTPANDA_CDP_URL || 'http://127.0.0.1:9222';

// ─── Worker-scoped fixture: 1 browser cho cả worker ─────────────────
const test = base.extend<LightpandaFixtures, LightpandaWorkerFixtures>({
  lightpandaBrowser: [
    async ({ }, use) => {
      // Connect to Lightpanda via Chrome DevTools Protocol (CDP)
      const browser = await chromium.connectOverCDP(LIGHTPANDA_CDP_URL);
      await use(browser);
      await browser.close();
    },
    { scope: 'worker' }
  ],

  // ─── Test-scoped fixture: page mới cho mỗi test ─────────────────
  page: async ({ lightpandaBrowser }, use) => {
    // Lấy context đầu tiên hoặc tạo mới
    const contexts = lightpandaBrowser.contexts();
    const context = contexts[0] || await lightpandaBrowser.newContext({
      viewport: { width: 1920, height: 1080 },
      locale: 'vi-VN',
    });

    // Lấy page đầu tiên hoặc tạo mới
    const pages = context.pages();
    const page = pages[0] || await context.newPage();
    await use(page);

    // Cleanup: đóng page sau test
    await page.close();
  },
});

export { test, expect };
export type { Page, Browser };
