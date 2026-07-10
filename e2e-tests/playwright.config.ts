import { defineConfig, devices } from '@playwright/test';

/**
 * Cấu hình Playwright cho Fastship E2E Tests
 * URL: https://fastship-web.onrender.com
 * 
 * Vì Render free tier có độ trễ lớn, tất cả timeouts đều được set 60s+
 */
export default defineConfig({
  testDir: './tests',
  // ponytail: Render free tier rất chậm. retries=0 để chạy nhanh hơn.
  // Dashboard tests (03,04,05) fail vì backend crash, không phải test code — retry cũng vô ích.
  timeout: 60_000,             // Giảm từ 120s → 60s
  expect: {
    timeout: 15_000,            // Giảm từ 30s → 15s
    toHaveScreenshot: {
      maxDiffPixels: 100,
    },
  },
  // ponytail: 1 worker để tránh rate limit login (5 POST/5ph trên Render)
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,                   // retries=0: không retry, test nhanh gấp đôi
  workers: 1,                   // 1 worker: tránh rate limit login
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
  ],

  use: {
    baseURL: 'https://fastship-web.onrender.com',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 30_000,      // Giảm từ 60s → 30s
    navigationTimeout: 30_000,  // Giảm từ 60s → 30s
  },

  projects: [
    {
      name: 'Desktop Chromium',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1920, height: 1080 },
      },
    },
    {
      name: 'Mobile Chrome',
      use: {
        ...devices['Pixel 5'],
        viewport: { width: 375, height: 812 },
      },
    },
  ],
});
