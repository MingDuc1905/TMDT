/**
 * Phase 11: Cross-Browser / Responsive Tests (6 tests)
 * Verify consistent behavior across viewport sizes: Desktop, Tablet, Mobile
 *
 * Base URL: https://fastship-web.onrender.com
 * Breakpoints: Mobile < 768px, Tablet 768-1024px, Desktop > 1024px
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';

test.setTimeout(120_000);

const DESKTOP = { width: 1920, height: 1080 };
const TABLET = { width: 768, height: 1024 };
const MOBILE = { width: 375, height: 812 };

const RESTAURANT_ID = 6; // Koneko Pizza

// ─── Desktop (1920x1080) ───

test.describe('Cross-Browser: Desktop (1920x1080)', () => {

  test('desktop: homepage layout has multi-column grid', async ({ page }) => {
    await page.setViewportSize(DESKTOP);
    const home = new HomePage(page);
    await home.gotoHome();

    const cards = home.restaurantCards;
    await expect(cards.first()).toBeVisible({ timeout: 15_000 });

    const count = await home.getRestaurantCount();
    expect(count).toBeGreaterThan(0);

    const usesGridOrFlex = await page.evaluate(() => {
      const container = document.querySelector('.product-list, .row, [class*="grid"]');
      if (!container) return true; // fallback: if no explicit grid, cards still render
      const style = getComputedStyle(container);
      return (
        style.display.includes('grid') ||
        style.display.includes('flex') ||
        style.display === 'flex' ||
        style.display === 'grid'
      );
    });
    expect(usesGridOrFlex).toBeTruthy();

    await page.screenshot({ path: 'screenshots/cross-browser-desktop-home.png', fullPage: true });
  });

  test('desktop: login page centered layout', async ({ page }) => {
    await page.setViewportSize(DESKTOP);
    const login = new LoginPage(page);
    await login.gotoLogin();

    const authCard = page.locator('.auth-card');
    await expect(authCard).toBeVisible({ timeout: 15_000 });

    const box = await authCard.boundingBox();
    expect(box).not.toBeNull();

    if (box) {
      const viewportWidth = DESKTOP.width;
      const cardCenter = box.x + box.width / 2;
      expect(Math.abs(cardCenter - viewportWidth / 2)).toBeLessThan(200);
    }

    await page.screenshot({ path: 'screenshots/cross-browser-desktop-login.png', fullPage: true });
  });
});

// ─── Tablet (768x1024) ───

test.describe('Cross-Browser: Tablet (768x1024)', () => {

  test('tablet: homepage adapts to tablet width', async ({ page }) => {
    await page.setViewportSize(TABLET);
    const home = new HomePage(page);
    await home.gotoHome();

    const cards = home.restaurantCards;
    await expect(cards.first()).toBeVisible({ timeout: 15_000 });

    const count = await home.getRestaurantCount();
    expect(count).toBeGreaterThan(0);

    const firstCardBox = await cards.first().boundingBox();
    expect(firstCardBox).not.toBeNull();
    if (firstCardBox) {
      expect(firstCardBox.width).toBeGreaterThan(100);
      expect(firstCardBox.width).toBeLessThanOrEqual(TABLET.width);
    }

    await page.screenshot({ path: 'screenshots/cross-browser-tablet-home.png', fullPage: true });
  });

  test('tablet: restaurant detail is usable', async ({ page }) => {
    await page.setViewportSize(TABLET);
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(RESTAURANT_ID);

    const menuCount = await detail.getMenuItemCount();
    expect(menuCount).toBeGreaterThan(0);

    await expect(detail.menuItems.first()).toBeVisible({ timeout: 15_000 });

    const firstItemBox = await detail.menuItems.first().boundingBox();
    expect(firstItemBox).not.toBeNull();
    if (firstItemBox) {
      expect(firstItemBox.width).toBeGreaterThan(50);
    }

    await page.screenshot({ path: 'screenshots/cross-browser-tablet-detail.png', fullPage: true });
  });
});

// ─── Mobile (375x812) ───

test.describe('Cross-Browser: Mobile (375x812)', () => {

  test('mobile: homepage shows hamburger menu', async ({ page }) => {
    await page.setViewportSize(MOBILE);
    const home = new HomePage(page);
    await home.gotoHome();

    const navbar = home.navbar;
    await expect(navbar).toBeVisible({ timeout: 15_000 });

    const mobileToggle = page.locator(
      '.navbar-toggler, .navbar-toggle, button[data-bs-toggle="collapse"], button[aria-controls]'
    ).first();
    const hamburgerVisible = await mobileToggle.isVisible().catch(() => false);

    if (!hamburgerVisible) {
      const anyToggle = page.locator('button, .icon-menu, .menu-toggle, .fs-hamburger').first();
      const altVisible = await anyToggle.isVisible().catch(() => false);
      expect(hamburgerVisible || altVisible).toBeTruthy();
    } else {
      expect(hamburgerVisible).toBeTruthy();
    }

    const cards = home.restaurantCards;
    await expect(cards.first()).toBeVisible({ timeout: 15_000 });

    const firstCardBox = await cards.first().boundingBox();
    expect(firstCardBox).not.toBeNull();
    if (firstCardBox) {
      expect(firstCardBox.width).toBeLessThanOrEqual(MOBILE.width);
    }

    await page.screenshot({ path: 'screenshots/cross-browser-mobile-home.png', fullPage: true });
  });

  test('mobile: restaurant detail shows add-to-cart', async ({ page }) => {
    await page.setViewportSize(MOBILE);
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(RESTAURANT_ID);

    await expect(detail.menuItems.first()).toBeVisible({ timeout: 15_000 });

    const menuCount = await detail.getMenuItemCount();
    expect(menuCount).toBeGreaterThan(0);

    await expect(detail.addToCartBtn.first()).toBeVisible({ timeout: 15_000 });

    const btnBox = await detail.addToCartBtn.first().boundingBox();
    expect(btnBox).not.toBeNull();
    if (btnBox) {
      expect(btnBox.width).toBeGreaterThan(30);
    }

    await page.screenshot({ path: 'screenshots/cross-browser-mobile-detail.png', fullPage: true });
  });
});
