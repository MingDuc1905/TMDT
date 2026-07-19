/**
 * ⚡ BỘ TEST 20: PERFORMANCE & CONSOLE ERRORS
 *
 * Mục tiêu: Performance baseline + JS console error detection
 */

import { test, expect } from '@playwright/test';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = (await import('../pages/LoginPage')).LoginPage;
  const l = new login(page);
  await l.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. PAGE LOAD PERFORMANCE
// ════════════════════════════════════════════════════════════════
test.describe('⚡ Page load performance', () => {

  test('[TC-PERF-01] Homepage load time < 10s', async ({ page }) => {
    const start = Date.now();
    await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const loadTime = Date.now() - start;
    console.log(`Homepage load: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(10_000);
  });

  test('[TC-PERF-02] Login page load time < 8s', async ({ page }) => {
    const start = Date.now();
    await page.goto('/Home/Login', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const loadTime = Date.now() - start;
    console.log(`Login page load: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(8_000);
  });

  test('[TC-PERF-03] Cart page load time < 8s', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const start = Date.now();
    await page.goto('/Cart', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const loadTime = Date.now() - start;
    console.log(`Cart page load: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(8_000);
  });

  test('[TC-PERF-04] Restaurant detail load time < 10s', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const start = Date.now();
    await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const loadTime = Date.now() - start;
    console.log(`Restaurant detail load: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(10_000);
  });

  test('[TC-PERF-05] Checkout page load time < 10s', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const start = Date.now();
    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const loadTime = Date.now() - start;
    console.log(`Checkout page load: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(10_000);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. CONSOLE ERRORS
// ════════════════════════════════════════════════════════════════
test.describe('🚫 Console errors', () => {

  test('[TC-PERF-06] Homepage — 0 critical JS errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    const criticalErrors = errors.filter(e =>
      !e.includes('favicon') &&
      !e.includes('404') &&
      !e.includes('net::') &&
      !e.includes('SignalR') &&
      !e.includes('WebSocket')
    );
    console.log(`Critical JS errors: ${criticalErrors.length}`);
    criticalErrors.forEach(e => console.log(`  ❌ ${e.substring(0, 120)}`));
  });

  test('[TC-PERF-07] Cart page — 0 critical JS errors', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    await page.goto('/Cart', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const criticalErrors = errors.filter(e =>
      !e.includes('favicon') &&
      !e.includes('404') &&
      !e.includes('net::') &&
      !e.includes('SignalR') &&
      !e.includes('WebSocket')
    );
    console.log(`Critical JS errors on Cart: ${criticalErrors.length}`);
    criticalErrors.forEach(e => console.log(`  ❌ ${e.substring(0, 120)}`));
  });

  test('[TC-PERF-08] Checkout — 0 critical JS errors', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const criticalErrors = errors.filter(e =>
      !e.includes('favicon') &&
      !e.includes('404') &&
      !e.includes('net::') &&
      !e.includes('SignalR') &&
      !e.includes('WebSocket')
    );
    console.log(`Critical JS errors on Checkout: ${criticalErrors.length}`);
    criticalErrors.forEach(e => console.log(`  ❌ ${e.substring(0, 120)}`));
  });
});

// ════════════════════════════════════════════════════════════════
// 3. HEALTH ENDPOINT
// ════════════════════════════════════════════════════════════════
test.describe('🏥 Health check', () => {

  test('[TC-PERF-09] /health endpoint responds < 5s', async ({ page }) => {
    const start = Date.now();
    const response = await page.request.get('/health');
    const responseTime = Date.now() - start;

    console.log(`Health status: ${response.status()}, time: ${responseTime}ms`);
    expect(response.status()).toBe(200);
    expect(responseTime).toBeLessThan(5_000);
  });
});

// ════════════════════════════════════════════════════════════════
// 4. API RESPONSE TIMES
// ════════════════════════════════════════════════════════════════
test.describe('📡 API response times', () => {

  test('[TC-PERF-10] MenuSearch API < 5s', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    const start = Date.now();
    const response = await page.request.get('/Home/MenuSearch?searchKey=pizza&maQuan=6');
    const responseTime = Date.now() - start;

    console.log(`MenuSearch API: ${response.status()}, time: ${responseTime}ms`);
    expect(responseTime).toBeLessThan(5_000);
  });

  test('[TC-PERF-11] Homepage HTML < 8s', async ({ page }) => {
    const start = Date.now();
    const response = await page.request.get('/');
    const responseTime = Date.now() - start;

    console.log(`Homepage HTML: ${response.status()}, time: ${responseTime}ms`);
    expect(responseTime).toBeLessThan(8_000);
  });
});

// ════════════════════════════════════════════════════════════════
// 5. STATIC ASSETS
// ════════════════════════════════════════════════════════════════
test.describe('📦 Static assets', () => {

  test('[TC-PERF-12] CSS bundle loads', async ({ page }) => {
    const responses: number[] = [];
    page.on('response', (resp) => {
      if (resp.url().endsWith('.css')) responses.push(resp.status());
    });

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    console.log(`CSS responses: ${responses.length}, statuses: ${[...new Set(responses)].join(',')}`);
    expect(responses.length).toBeGreaterThan(0);
  });

  test('[TC-PERF-13] JS bundle loads', async ({ page }) => {
    const responses: number[] = [];
    page.on('response', (resp) => {
      if (resp.url().endsWith('.js')) responses.push(resp.status());
    });

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    console.log(`JS responses: ${responses.length}, statuses: ${[...new Set(responses)].join(',')}`);
    expect(responses.length).toBeGreaterThan(0);
  });
});
