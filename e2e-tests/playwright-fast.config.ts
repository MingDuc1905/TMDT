import { defineConfig, devices } from '@playwright/test';

/**
 * Fast config — tối ưu tốc độ cho 643 tests
 * - workers=4: chạy 4 tests song song
 * - Desktop-only: bỏ mobile project
 * - timeout=45s: giảm từ 60s
 */
export default defineConfig({
  testDir: './tests',
  timeout: 45_000,
  expect: {
    timeout: 10_000,
  },
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 4,

  reporter: [
    ['list'],
    ['json', { outputFile: 'test-results-fast.json' }],
  ],

  use: {
    baseURL: 'https://fastship-web.onrender.com',
    actionTimeout: 20_000,
    navigationTimeout: 25_000,
    screenshot: 'only-on-failure',
    video: 'off',
    trace: 'off',
  },

  projects: [
    {
      name: 'Desktop Chromium Fast',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1920, height: 1080 },
      },
    },
  ],
});
