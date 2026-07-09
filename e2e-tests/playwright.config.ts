import { defineConfig, devices } from '@playwright/test';

/**
 * Cấu hình Playwright cho Fastship E2E Tests
 * URL: https://fastship-web.onrender.com
 * 
 * Vì Render free tier có độ trễ lớn, tất cả timeouts đều được set 60s+
 */
export default defineConfig({
  testDir: './tests',
  timeout: 120_000,            // Mỗi test tối đa 120s
  expect: {
    timeout: 30_000,            // Expect timeout 30s
    toHaveScreenshot: {
      maxDiffPixels: 100,       // Cho phép sai khác nhỏ do font rendering
    },
  },
  fullyParallel: false,         // Chạy tuần tự để tránh conflict session/cart
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 1,
  workers: process.env.CI ? 1 : 1,
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
  ],

  use: {
    baseURL: 'https://fastship-web.onrender.com',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 60_000,      // Mỗi action tối đa 60s
    navigationTimeout: 60_000,  // Navigation tối đa 60s
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
