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
  testDir: './examples',
  // ponytail: Lightpanda nhanh hơn Chrome 9x → giảm timeout
  timeout: 30_000,              // 30s (vs 60s cho Chromium)
  expect: {
    timeout: 10_000,             // 10s (vs 15s cho Chromium)
  },
  fullyParallel: true,           // Lightpanda nhẹ → chạy parallel được
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: process.env.CI ? 2 : 4,

  reporter: [
    ['html', { outputFolder: 'playwright-report-lightpanda' }],
    ['list'],
  ],

  use: {
    baseURL: process.env.BASE_URL || 'https://fastship-web.onrender.com',
    actionTimeout: 15_000,       // 15s
    navigationTimeout: 15_000,   // 15s
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
