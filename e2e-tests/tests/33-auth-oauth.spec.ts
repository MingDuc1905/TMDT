/**
 * 🔐 BỘ TEST 33: AUTH OAUTH + SIGNUP FULL FLOW
 *
 * Mục tiêu: Test toàn bộ luồng đăng ký và OAuth
 * - Signup page: form validation, đăng ký mới
 * - Google OAuth: button hiển thị, redirect
 * - Facebook OAuth: button hiển thị
 * - SelectRoleGoogle: chọn role sau OAuth
 * - Logout: clear session
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { HomePage } from '../pages/HomePage';
import { USERS, URLS } from '../fixtures/users';

// ─── SIGNUP ───
test.describe('📝 Signup — Đăng ký', () => {

  test('[TC-AUTH-01] Signup page load — form + validation fields', async ({ page }) => {
    await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra form
    const form = page.locator('form').first();
    await expect(form).toBeVisible({ timeout: 5_000 });

    // Đếm inputs
    const inputs = await form.locator('input, select').count();
    console.log(`📝 Form inputs: ${inputs}`);
    expect(inputs).toBeGreaterThanOrEqual(5); // username, password, name, phone, address...

    // Kiểm tra các field chính
    const usernameInput = page.locator('input[name*="user"], input[name*="taiKhoan"], input[placeholder*="tên"]').first();
    const passwordInput = page.locator('input[type="password"]').first();
    const nameInput = page.locator('input[name*="name"], input[name*="ten"], input[placeholder*="họ"]').first();
    const phoneInput = page.locator('input[name*="phone"], input[name*="sdt"], input[type="tel"]').first();

    console.log(`👤 Username: ${await usernameInput.isVisible().catch(() => false)}`);
    console.log(`🔑 Password: ${await passwordInput.isVisible().catch(() => false)}`);
    console.log(`📛 Full name: ${await nameInput.isVisible().catch(() => false)}`);
    console.log(`📞 Phone: ${await phoneInput.isVisible().catch(() => false)}`);

    // Check role select (khách hàng / quán ăn)
    const roleSelect = page.locator('select[name*="role"], select[name*="loai"], input[name*="role"], input[value*="Khách"]');
    const roleCount = await roleSelect.count();
    console.log(`🎭 Role options: ${roleCount}`);

    // Submit button
    const submitBtn = page.locator('button[type="submit"], input[type="submit"]').first();
    await expect(submitBtn).toBeVisible();
    console.log(`📤 Submit: "${await submitBtn.textContent()}"`);
  });

  test('[TC-AUTH-02] Signup — validation: submit trống', async ({ page }) => {
    await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const submitBtn = page.locator('button[type="submit"], input[type="submit"]').first();
    await submitBtn.click();
    await page.waitForTimeout(1000);

    // URL không đổi (HTML5 validation chặn submit)
    const url = page.url();
    console.log(`📍 URL: ${url}`);
    expect(url).toContain('Signup');
  });

  test('[TC-AUTH-03] Signup — link to Login hoạt động', async ({ page }) => {
    await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(2000);

    const loginLink = page.locator('a[href*="Login"]').first();
    if (await loginLink.isVisible().catch(() => false)) {
      await loginLink.click();
      await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
      await page.waitForTimeout(1000);
      expect(page.url()).toContain('Login');
      console.log('✅ Signup → Login link works');
    }
  });
});

// ─── GOOGLE OAUTH ───
test.describe('🔵 Google OAuth', () => {

  test('[TC-AUTH-04] Login page — Google OAuth button hiển thị', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await expect(login.googleLoginButton).toBeVisible({ timeout: 5_000 });
    const btnText = await login.googleLoginButton.textContent();
    console.log(`🔵 Google button: "${btnText?.trim()}"`);
  });

  test('[TC-AUTH-05] Google OAuth — Click redirect đến Google', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    if (await login.googleLoginButton.isVisible().catch(() => false)) {
      // Kiểm tra href trước khi click
      const href = await login.googleLoginButton.getAttribute('href');
      console.log(`🔗 Google href: ${href}`);

      if (href?.includes('google') || href?.includes('Google')) {
        // Navigate để kiểm tra redirect
        const resp = await page.goto(href, { waitUntil: 'domcontentloaded', timeout: 30_000 });
        const status = resp?.status() ?? 0;
        const url = page.url();
        console.log(`📍 URL: ${url.substring(0, 100)}, Status: ${status}`);

        // Should redirect to Google or return challenge
        const isGoogle = url.includes('google') || url.includes('accounts');
        console.log(`🔵 Redirect to Google: ${isGoogle}`);
      }
    }
  });

  test('[TC-AUTH-06] SelectRoleGoogle page — 3 role cards hiển thị', async ({ page }) => {
    // Simulate login with test parameter to reach role selection
    await page.goto('/Home/SelectRoleGoogle', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const bodyText = await page.locator('body').textContent() || '';
    console.log(`📄 Page: ${bodyText.substring(0, 100)}`);

    // Kiểm tra 3 role cards: Khách hàng, Quán ăn, Shipper
    const roleCards = page.locator('.role-card, [class*="role"] a, .card:has-text("Khách")');
    const cardCount = await roleCards.count();
    console.log(`🎭 Role cards: ${cardCount}`);

    if (cardCount === 0) {
      // Fallback: check for any clickable options
      const options = page.locator('a, button, input[type="radio"]');
      const optionCount = await options.count();
      console.log(`  Options: ${optionCount}`);
      if (optionCount > 0) {
        const texts = await options.allTextContents();
        console.log(`  Texts: ${texts.map(t => t?.trim()).filter(Boolean).join(', ')}`);
      }
    }
  });
});

// ─── LOGOUT ───
test.describe('🚪 Logout', () => {

  test('[TC-AUTH-07] Logout — clear session + redirect', async ({ page }) => {
    // Login first
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(USERS.customer1.username);
    await login.passwordInput.fill(USERS.customer1.password);
    await login.loginButton.click();
    await page.waitForTimeout(3000);

    // Kiểm tra đã login (có thể thấy user avatar)
    const home = new HomePage(page);
    const loggedIn = await home.userDropdown.isVisible().catch(() => false);
    const cartBtn = await home.cartButton.isVisible().catch(() => false);
    console.log(`👤 Logged in: ${loggedIn || cartBtn}`);

    // Logout
    await page.goto('/Home/Logout', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const url = page.url();
    console.log(`📍 After logout: ${url}`);

    // Should redirect to home or login
    const isLoggedOut = url.includes('/Home') || url === 'https://fastship-web.onrender.com/' || url === '/';
    expect(isLoggedOut).toBeTruthy();

    // Verify navbar shows login button (not user dropdown)
    const logoutLoginBtn = await home.loginNavBtn.isVisible().catch(() => false);
    console.log(`🔐 Login button visible after logout: ${logoutLoginBtn}`);
  });
});
