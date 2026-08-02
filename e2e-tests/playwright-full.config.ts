import { defineConfig, devices } from '@playwright/test';

/**
 * Full config — chạy TẤT CẢ 32 test files
 * - 2 workers: tránh rate limit login
 * - 120s timeout: Render free tier rất chậm
 * - Desktop-only: tập trung vào desktop, mobile cũng chạy
 */
export default defineConfig({
  testDir: './tests',
  timeout: 120_000,
  expect: {
    timeout: 30_000,
  },
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 2,

  reporter: [
    ['list'],
    ['json', { outputFile: 'test-results-final.json' }],
  ],

  use: {
    baseURL: 'https://fastship-web.onrender.com',
    actionTimeout: 60_000,
    navigationTimeout: 60_000,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'Desktop Full',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1920, height: 1080 },
      },
    },
  ],
});
