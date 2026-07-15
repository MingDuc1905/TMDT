/**
 * SECURITY TESTS — Phase 8
 *
 * 15 tests covering:
 * - Authentication bypass (unauthenticated access to protected routes)
 * - Role-based access control (wrong role access to restricted areas)
 * - Session & cookie attributes (HttpOnly, SameSite, Secure)
 * - Input validation (SQL injection, XSS in login form)
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS, URLS } from '../fixtures/users';

test.setTimeout(120_000);

const BASE = 'https://fastship-web.onrender.com';

// ═══════════════════════════════════════════════════════════════════════════════
// Helper
// ═══════════════════════════════════════════════════════════════════════════════

/** Returns true if page was redirected away from the restricted path */
async function wasRedirectedAway(page: import('@playwright/test').Page, restrictedPath: string): Promise<boolean> {
  await page.waitForTimeout(3000);
  const url = page.url();
  const onRestricted = url.includes(restrictedPath) && !url.includes('/Home/Login');
  return !onRestricted;
}

// ═══════════════════════════════════════════════════════════════════════════════
// Security: Authentication Bypass
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Security: Authentication Bypass', () => {

  test('unauthenticated access to /Cart redirects to login', async ({ page }) => {
    await page.goto(`${BASE}${URLS.cart}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const url = page.url();
    expect(url).toContain('/Home/Login');
  });

  test('unauthenticated access to /Cart/Checkout redirects to login', async ({ page }) => {
    await page.goto(`${BASE}${URLS.checkout}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const url = page.url();
    expect(url).toContain('/Home/Login');
  });

  test('unauthenticated access to /Restaurant redirects to login', async ({ page }) => {
    await page.goto(`${BASE}${URLS.restaurant}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const url = page.url();
    expect(url).toContain('/Home/Login');
  });

  test('unauthenticated access to /Shipper redirects to login', async ({ page }) => {
    await page.goto(`${BASE}${URLS.shipper}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const url = page.url();
    expect(url).toContain('/Home/Login');
  });

  test('unauthenticated access to /Admin redirects to login', async ({ page }) => {
    await page.goto(`${BASE}${URLS.admin}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const url = page.url();
    expect(url).toContain('/Home/Login');
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Security: Role-Based Access Control
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Security: Role-Based Access Control', () => {

  test('customer cannot access /Restaurant (gets redirected)', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);
    expect(page.url()).not.toContain('/Home/Login');

    await page.goto(`${BASE}${URLS.restaurant}`, { waitUntil: 'domcontentloaded' });
    const redirected = await wasRedirectedAway(page, '/Restaurant');
    expect(redirected).toBeTruthy();
  });

  test('customer cannot access /Shipper', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);
    expect(page.url()).not.toContain('/Home/Login');

    await page.goto(`${BASE}${URLS.shipper}`, { waitUntil: 'domcontentloaded' });
    const redirected = await wasRedirectedAway(page, '/Shipper');
    expect(redirected).toBeTruthy();
  });

  test('customer cannot access /Admin', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);
    expect(page.url()).not.toContain('/Home/Login');

    await page.goto(`${BASE}${URLS.admin}`, { waitUntil: 'domcontentloaded' });
    const redirected = await wasRedirectedAway(page, '/Admin');
    expect(redirected).toBeTruthy();
  });

  test('restaurant cannot access /Admin', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.restaurant1.username, USERS.restaurant1.password);
    await page.waitForTimeout(2000);
    expect(page.url()).toContain('/Restaurant');

    await page.goto(`${BASE}${URLS.admin}`, { waitUntil: 'domcontentloaded' });
    const redirected = await wasRedirectedAway(page, '/Admin');
    expect(redirected).toBeTruthy();
  });

  test('shipper cannot access /Admin', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.shipper1.username, USERS.shipper1.password);
    await page.waitForTimeout(2000);
    expect(page.url()).toContain('/Shipper');

    await page.goto(`${BASE}${URLS.admin}`, { waitUntil: 'domcontentloaded' });
    const redirected = await wasRedirectedAway(page, '/Admin');
    expect(redirected).toBeTruthy();
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Security: Session & Cookie
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Security: Session & Cookie', () => {

  test('session cookie is HttpOnly', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);

    const cookies = await page.context().cookies();
    const sessionCookie = cookies.find(c => c.name === '.AspNetCore.Session');
    expect(sessionCookie).toBeTruthy();
    expect(sessionCookie!.httpOnly).toBeTruthy();
  });

  test('session cookie has SameSite=Lax', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);

    const cookies = await page.context().cookies();
    const sessionCookie = cookies.find(c => c.name === '.AspNetCore.Session');
    expect(sessionCookie).toBeTruthy();
    expect(sessionCookie!.sameSite).toBe('Lax');
  });

  test('session cookie has Secure flag on HTTPS', async ({ page }) => {
    const login = new LoginPage(page);
    await login.login(USERS.customer1.username, USERS.customer1.password);
    await page.waitForTimeout(2000);

    const cookies = await page.context().cookies();
    const sessionCookie = cookies.find(c => c.name === '.AspNetCore.Session');
    expect(sessionCookie).toBeTruthy();
    expect(sessionCookie!.secure).toBeTruthy();
  });
});

// ═══════════════════════════════════════════════════════════════════════════════
// Security: Input Validation
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('Security: Input Validation', () => {

  test('login with SQL injection in username does not crash server', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill("admin'; DROP TABLE tbUser;--");
    await login.passwordInput.fill('test123');
    await login.loginButton.click();

    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);

    const statusCode = page.url();
    const bodyText = await page.locator('body').textContent().catch(() => '');

    const noServerError =
      !bodyText.includes('500 Internal Server Error') &&
      !bodyText.includes('An error occurred') &&
      !bodyText.includes('Exception') &&
      !bodyText.includes('Stack Trace');

    const stayedOnLogin = statusCode.includes('/Home/Login');
    const gotNormalResponse = noServerError;

    expect(stayedOnLogin || gotNormalResponse).toBeTruthy();
  });

  test('login with XSS in username does not render script', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    let alertTriggered = false;
    page.on('dialog', async (dialog) => {
      alertTriggered = true;
      await dialog.dismiss();
    });

    const xssPayload = "<script>alert('xss')</script>";
    await login.usernameInput.fill(xssPayload);
    await login.passwordInput.fill('test123');
    await login.loginButton.click();

    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);

    expect(alertTriggered).toBeFalsy();

    const bodyHtml = await page.locator('body').innerHTML().catch(() => '');
    const noScriptInjected =
      !bodyHtml.includes("<script>alert('xss')</script>") &&
      !bodyHtml.includes('alert(&#39;xss&#39;)');
    expect(noScriptInjected).toBeTruthy();

    const noServerError =
      !bodyHtml.includes('500 Internal Server Error') &&
      !bodyHtml.includes('An error occurred');
    expect(noServerError).toBeTruthy();
  });
});
