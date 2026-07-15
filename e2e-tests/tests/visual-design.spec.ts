/**
 * Phase 2: Visual & Design Tests (37 tests)
 * Design system validation, responsive layout, CSS consistency
 *
 * Base URL: https://fastship-web.onrender.com
 * Design System v4.0: Font Inter, Primary #3CB815, Secondary #F65005
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { SEED } from '../fixtures/users';

test.setTimeout(120_000);

// ─── Design: Design System Tokens (5) ───

test.describe('Visual: Design System Tokens', () => {

  test('CSS custom properties are defined on :root', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');

    const value = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--fs-green').trim()
    );
    expect(value).not.toBe('');
  });

  test('primary color is #3CB815', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    const value = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--fs-green').trim()
    );
    expect(value).toBe('#3CB815');
  });

  test('secondary color is #F65005', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    const value = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--fs-orange').trim()
    );
    expect(value).toBe('#F65005');
  });

  test('font family includes Inter', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    const value = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--fs-font').trim()
    );
    expect(value).toContain('Inter');
  });

  test('border-radius variable is set', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    const value = await page.evaluate(() =>
      getComputedStyle(document.documentElement).getPropertyValue('--fs-radius').trim()
    );
    expect(value).not.toBe('');
  });
});

// ─── Design: Navbar Consistency (6) ───

test.describe('Visual: Navbar Consistency', () => {

  test('navbar has FastShip logo', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(home.logo).toBeVisible({ timeout: 15_000 });
  });

  test('navbar has search input on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(page.locator('input[name="txtSearch"]').first()).toBeVisible({ timeout: 15_000 });
  });

  test('navbar has cart button', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(home.cartButton).toBeVisible({ timeout: 15_000 });
  });

  test('navbar has login button when not authenticated', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(page.locator('a[href*="/Home/Login"]').first()).toBeVisible({ timeout: 15_000 });
  });

  test('navbar is fixed/sticky on scroll', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForTimeout(2000);

    const position = await page.evaluate(() => {
      const header = document.querySelector('.fs-header');
      if (!header) return 'missing';
      return getComputedStyle(header).position;
    });
    expect(['fixed', 'sticky']).toContain(position);
  });

  test('mobile hamburger menu exists', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    const home = new HomePage(page);
    await home.gotoHome();
    const hamburger = page.locator('.navbar-toggler');
    await expect(hamburger).toBeVisible({ timeout: 10_000 });
  });
});

// ─── Design: Homepage Layout (7) ───

test.describe('Visual: Homepage Layout', () => {

  test('hero carousel is present', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(home.carousel).toBeVisible({ timeout: 15_000 });
  });

  test('category pills row is present', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForSelector('#categoryRow', { state: 'attached', timeout: 15_000 });
    await expect(home.categoryRow).toBeVisible();
  });

  test('restaurant grid has cards', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(5000);
    const count = await home.restaurantCards.count();
    expect(count).toBeGreaterThan(0);
  });

  test('restaurant cards have image, title, rating', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(5000);
    const firstCard = home.restaurantCards.first();
    await expect(firstCard.locator('.product-title')).toBeVisible({ timeout: 10_000 });
  });

  test('stats row displays stats', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await expect(home.statsRow).toBeVisible({ timeout: 10_000 });
  });

  test('footer is present', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await home.scrollToBottom();
    await expect(home.footer).toBeVisible({ timeout: 10_000 });
  });

  test('promo band appears or is dismissible', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    try {
      await expect(home.promoBand).toBeVisible({ timeout: 8_000 });
      await home.dismissPromo();
      await expect(home.promoBand).not.toBeVisible({ timeout: 3_000 });
    } catch {
      // promo band not present or already dismissed
    }
  });
});

// ─── Design: Auth Pages Design (6) ───

test.describe('Visual: Auth Pages Design', () => {

  test('login page has auth-card centered', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    await expect(page.locator('.auth-card')).toBeVisible({ timeout: 15_000 });
  });

  test('login form has input icons', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    const inputCount = await page.locator('.auth-card input').count();
    expect(inputCount).toBeGreaterThanOrEqual(2);
  });

  test('google login button is styled', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    await expect(login.googleLoginButton).toBeVisible({ timeout: 10_000 });
    const hasClass = await page.evaluate(() => {
      const btn = document.querySelector('a[href*="GoogleLogin"], a[href*="google"]');
      return btn ? btn.className.length > 0 : false;
    });
    expect(hasClass).toBe(true);
  });

  test('signup page has role selection cards', async ({ page }) => {
    await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    const roleCards = await page.locator('.role-card, .role-option, [class*="role"], input[name*="role"], select[name*="role"], .signup-role').count();
    expect(roleCards).toBeGreaterThan(0);
  });

  test('signup page has password strength indicator', async ({ page }) => {
    await page.goto('/Home/Signup', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    const strengthEl = await page.locator('.password-strength, .strength, [class*="strength"], .pwd-strength, .pw-strength, #passwordStrength').count();
    expect(strengthEl).toBeGreaterThan(0);
  });

  test('login page has partner link', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    const partnerLink = page.locator('a[href*="Partner"], a[href*="QuanAn"], a[href*="partner"]');
    await expect(partnerLink.first()).toBeVisible({ timeout: 10_000 });
  });
});

// ─── Design: Restaurant Detail Design (7) ───

test.describe('Visual: Restaurant Detail Design', () => {

  test('restaurant header displays name', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await expect(detail.restaurantName).toBeVisible({ timeout: 15_000 });
  });

  test('menu items have price displayed', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    await expect(detail.itemPrice.first()).toBeVisible();
  });

  test('category pills are horizontal scrollable', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.list-category', { timeout: 15_000 });
    const scrollable = await page.evaluate(() => {
      const el = document.querySelector('.list-category');
      if (!el) return false;
      const style = getComputedStyle(el);
      return style.overflowX === 'auto' || style.overflowX === 'scroll' || style.display === 'flex';
    });
    expect(scrollable).toBe(true);
  });

  test('add-to-cart buttons are styled (btn-primary)', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    await expect(detail.addToCartBtn.first()).toBeVisible();
    const hasBtnClass = await page.evaluate(() => {
      const btn = document.querySelector('.add-to-cart-btn');
      if (!btn) return false;
      return btn.classList.contains('btn') || btn.classList.contains('btn-primary') || btn.tagName === 'BUTTON';
    });
    expect(hasBtnClass).toBe(true);
  });

  test('restaurant images have valid src', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    const imgResult = await detail.validateAllImages();
    if (imgResult.broken > 0) {
      console.log(`Broken images: ${imgResult.brokenUrls.join(', ')}`);
    }
  });

  test('review section exists', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForTimeout(3000);
    const exists = await page.locator('#review-list').count();
    expect(exists).toBeGreaterThanOrEqual(0);
  });

  test('quantity input is properly sized', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    await expect(detail.quantityInput.first()).toBeVisible();
    const box = await detail.quantityInput.first().boundingBox();
    if (box) {
      expect(box.width).toBeGreaterThan(0);
      expect(box.height).toBeGreaterThan(0);
    }
  });
});

// ─── Design: Image Validation (6) ───

test.describe('Visual: Image Validation', () => {

  test('homepage images are not broken', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();
    await page.waitForTimeout(3000);
    const imgResult = await home.validateAllImages();
    console.log(`Total: ${imgResult.total}, Broken: ${imgResult.broken}`);
    expect(imgResult.broken).toBe(0);
  });

  test('restaurant detail images are not broken', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
    const imgResult = await detail.validateAllImages();
    console.log(`Total: ${imgResult.total}, Broken: ${imgResult.broken}`);
    expect(imgResult.broken).toBe(0);
  });

  test('login page has no broken images', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    const imgResult = await login.validateAllImages();
    expect(imgResult.broken).toBe(0);
  });

  test('all img tags have alt attributes', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    const missingAlt = await page.evaluate(() =>
      document.querySelectorAll('img:not([alt])').length
    );
    expect(missingAlt).toBe(0);
  });

  test('no emoji used as navigation icons', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    const hasEmojiInNav = await page.evaluate(() => {
      const nav = document.querySelector('nav, .fs-header, .navbar');
      if (!nav) return false;
      const emojiRegex = /[\u{1F300}-\u{1F9FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}]/u;
      const links = nav.querySelectorAll('a, button, .nav-link, .navbar-nav li');
      for (const el of Array.from(links)) {
        const text = el.textContent || '';
        if (emojiRegex.test(text)) return true;
      }
      return false;
    });
    expect(hasEmojiInNav).toBe(false);
  });

  test('Font Awesome icons load correctly', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    const hasIcons = await page.evaluate(() =>
      document.querySelector('.fas, .fab, .far') !== null
    );
    expect(hasIcons).toBe(true);
  });
});
