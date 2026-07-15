// ====================================================================
// FastShip — Playwright Config for Lightpanda Browser (CDP)
// ====================================================================
// Kết nối tới Lightpanda CDP server thay vì Chromium built-in.
// Yêu cầu: Docker container Lightpanda đang chạy
//   docker compose up -d
//
// Chạy:
//   npx playwright test --config=lightpanda.config.ts
//   npm run test:lightpanda
// ====================================================================

import { defineConfig } from '@playwright/test';

/**
 * Lightpanda CDP Config
 *
 * Khác với playwright.config.ts (dùng Chromium built-in của Playwright),
 * config này kết nối tới Lightpanda CDP server thông qua chromium.connectOverCDP().
 *
 * Vì Lightpanda đang ở giai đoạn Beta, một số Web APIs có thể chưa hỗ trợ đầy đủ.
 * Xem: https://github.com/lightpanda-io/browser
 */
export default defineConfig({
  testDir: './tests',
  // ponytail: Lightpanda nhanh hơn Chrome 9x → giảm timeout
  timeout: 90_000,              // 90s — Render free tier 23-25s/page + Lightpanda Beta overhead
  expect: {
    timeout: 30_000,             // 30s
  },
  fullyParallel: false,          // Render free tier rate limit → serial
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,

  reporter: [
    ['html', { outputFolder: 'playwright-report-lightpanda' }],
    ['list'],
  ],

  use: {
    baseURL: process.env.BASE_URL || 'https://fastship-web.onrender.com',
    actionTimeout: 30_000,       // 30s
    navigationTimeout: 60_000,   // 60s — Render cold start can take 25s+
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry',
  },

  projects: [
    {
      name: 'Lightpanda Desktop',
    },
  ],
});
