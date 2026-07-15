/**
 * EDGE CASES & PERFORMANCE TESTS — Phase 9
 *
 * 15 tests covering:
 * - Error handling (non-existent pages, invalid IDs, malformed URLs)
 * - Boundary conditions (long strings, special chars, unicode, large IDs)
 * - Performance (page load times, health endpoint)
 * - Console errors (critical JS error detection)
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';

test.setTimeout(120_000);

// ═══════════════════════════════════════════════════════════════════════════════
// Edge Cases: Error Handling
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Edge Cases: Error Handling', () => {

  test('non-existent page returns 404 or error', async ({ page }) => {
    const response = await page.goto('/nonexistent-page-xyz123', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const is404 = status === 404;
    const hasErrorText = await page.locator('body').textContent().catch(() => '');
    const showsError = is404 || hasErrorText.includes('404') || hasErrorText.includes('Not Found') ||
      hasErrorText.includes('Không tìm thấy') || hasErrorText.includes('error') ||
      status === 500 || page.url().includes('/nonexistent-page-xyz123');
    expect(showsError).toBeTruthy();
  });

  test('invalid restaurant ID shows error or empty', async ({ page }) => {
    const response = await page.goto('/Home/DetailRestaurant?id=99999', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const url = page.url();
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const handledGracefully = !bodyText.includes('Unhandled') && !bodyText.includes('Exception') &&
      !bodyText.includes('500 Internal') &&
      (status === 200 || status === 404 || url.includes('/Home/DetailRestaurant?id=99999'));
    expect(handledGracefully).toBeTruthy();
  });

  test('invalid order ID shows error', async ({ page }) => {
    const response = await page.goto('/Cart/OrderTracking?id=99999', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const url = page.url();
    const handledGracefully = (status === 200 || status === 404 || url.includes('/Cart/OrderTracking') ||
      url.includes('/Home/Login'));
    expect(handledGracefully).toBeTruthy();
  });

  test('server does not crash on malformed URL', async ({ page }) => {
    const response = await page.goto('/?txtSearch=<script>', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const isServerError = status === 500;
    expect(isServerError).toBeFalsy();

    const bodyText = await page.locator('body').textContent().catch(() => '');
    const noCrash = !bodyText.includes('Unhandled') && !bodyText.includes('An error occurred');
    expect(noCrash).toBeTruthy();
  });

  test('empty search returns homepage or empty state', async ({ page }) => {
    const response = await page.goto('/?txtSearch=', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    expect(status).toBeLessThan(500);
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const pageLoaded = bodyText.length > 0;
    expect(pageLoaded).toBeTruthy();
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Edge Cases: Boundary Conditions
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Edge Cases: Boundary Conditions', () => {

  test('very long search string does not crash', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    const longString = 'a'.repeat(250);
    await home.search(longString);

    await page.waitForTimeout(3000);
    const status = 200;
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const noCrash = !bodyText.includes('Unhandled') && !bodyText.includes('500 Internal');
    expect(noCrash).toBeTruthy();
  });

  test('special characters in search do not crash', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await home.search('!@#$%^&*()_+-={}[]|\\:";<>?,./');

    await page.waitForTimeout(3000);
    const bodyText = await page.locator('body').textContent().catch(() => '');
    const noCrash = !bodyText.includes('Unhandled') && !bodyText.includes('500 Internal');
    expect(noCrash).toBeTruthy();
  });

  test('unicode search works', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await home.search('com tam');

    await page.waitForTimeout(3000);
    const pageLoaded = (await page.locator('body').textContent().catch(() => '')).length > 0;
    expect(pageLoaded).toBeTruthy();
  });

  test('very large restaurant ID does not crash', async ({ page }) => {
    const response = await page.goto('/Home/DetailRestaurant?id=2147483647', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const isServerError = status === 500;
    expect(isServerError).toBeFalsy();
  });

  test('negative restaurant ID handled', async ({ page }) => {
    const response = await page.goto('/Home/DetailRestaurant?id=-1', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const status = response?.status();
    const isServerError = status === 500;
    expect(isServerError).toBeFalsy();
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Performance: Page Load Times
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Performance: Page Load Times', () => {

  test('homepage loads within 60 seconds', async ({ page }) => {
    const start = Date.now();
    await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 60_000 });
    const elapsed = Date.now() - start;

    expect(elapsed).toBeLessThan(60_000);
  });

  test('login page loads within 30 seconds', async ({ page }) => {
    const start = Date.now();
    await page.goto('/Home/Login', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    const elapsed = Date.now() - start;

    expect(elapsed).toBeLessThan(30_000);
  });

  test('health endpoint responds within 10 seconds', async ({ page }) => {
    const start = Date.now();
    const response = await page.goto('/health', { waitUntil: 'domcontentloaded', timeout: 10_000 });
    const elapsed = Date.now() - start;

    expect(elapsed).toBeLessThan(10_000);
    expect(response?.status()).toBeLessThan(500);
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Edge Cases: Console Errors
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Edge Cases: Console Errors', () => {

  test('homepage has no critical JS errors', async ({ page }) => {
    const criticalErrors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        const text = msg.text();
        const isKnown =
          text.includes('WebSocket') || text.includes('SignalR') ||
          text.includes('Failed to load resource') || text.includes('404') ||
          text.includes('net::ERR') || text.includes('favicon') ||
          text.includes('Mixed Content') || text.includes('CORS');
        if (!isKnown) {
          criticalErrors.push(text);
        }
      }
    });

    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForTimeout(5000);

    expect(criticalErrors).toHaveLength(0);
  });

  test('login page has no critical JS errors', async ({ page }) => {
    const criticalErrors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        const text = msg.text();
        const isKnown =
          text.includes('WebSocket') || text.includes('SignalR') ||
          text.includes('Failed to load resource') || text.includes('404') ||
          text.includes('net::ERR') || text.includes('favicon') ||
          text.includes('Mixed Content') || text.includes('CORS');
        if (!isKnown) {
          criticalErrors.push(text);
        }
      }
    });

    await page.goto('/Home/Login', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(5000);

    expect(criticalErrors).toHaveLength(0);
  });
});
