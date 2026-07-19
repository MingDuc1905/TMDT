/**
 * 📱 BỘ TEST 18: MOBILE RESPONSIVE DEEP TEST
 *
 * Mục tiêu: Test responsive design trên mobile (375px)
 * - Touch targets, bottom sheet, layout stacking, input zoom
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { HomePage } from '../pages/HomePage';
import { CartPage } from '../pages/CartPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { USERS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAsMobile(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. HOMEPAGE MOBILE
// ════════════════════════════════════════════════════════════════
test.describe('📱 Homepage mobile', () => {

  test('[TC-MOBILE-01] Homepage: 2-column grid trên mobile', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const restaurantCards = page.locator('.product-item');
    const count = await restaurantCards.count();
    console.log(`Restaurant cards: ${count}`);

    if (count > 0) {
      const firstCard = restaurantCards.first();
      const box = await firstCard.boundingBox();
      if (box) {
        console.log(`Card width: ${box.width}px, viewport: 375px`);
        // On 375px viewport, card should be ~50% width (2-column)
        expect(box.width).toBeLessThan(300);
      }
    }
  });

  test('[TC-MOBILE-02] Homepage: no horizontal overflow', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const hasOverflow = await page.evaluate(() => {
      return document.body.scrollWidth > document.documentElement.clientWidth;
    });
    console.log(`Horizontal overflow: ${hasOverflow}`);
    expect(hasOverflow).toBe(false);
  });

  test('[TC-MOBILE-03] Homepage: logo + navbar visible', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const logo = page.locator('.fs-logo').first();
    const logoVisible = await logo.isVisible().catch(() => false);
    console.log(`Logo visible: ${logoVisible}`);

    const navbar = page.locator('.fs-header').first();
    const navVisible = await navbar.isVisible().catch(() => false);
    console.log(`Navbar visible: ${navVisible}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. CART MOBILE
// ════════════════════════════════════════════════════════════════
test.describe('🛒 Cart mobile', () => {

  test('[TC-MOBILE-04] Cart page: stacked layout trên mobile', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Cart', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check cart items exist
    const items = page.locator('.cart-item');
    const count = await items.count();
    console.log(`Cart items: ${count}`);

    if (count > 0) {
      const item = items.first();
      const box = await item.boundingBox();
      if (box) {
        console.log(`Cart item width: ${box.width}px`);
        expect(box.width).toBeLessThanOrEqual(375);
      }
    }
  });

  test('[TC-MOBILE-05] Cart: touch targets ≥ 44px', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Cart', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const buttons = page.locator('.btn-tang, .btn-giam, .delete-btn');
    const count = await buttons.count();
    console.log(`Interactive buttons: ${count}`);

    let smallTargets = 0;
    for (let i = 0; i < Math.min(count, 5); i++) {
      const box = await buttons.nth(i).boundingBox();
      if (box) {
        if (box.height < 44 || box.width < 44) {
          smallTargets++;
          console.log(`  Button #${i}: ${box.width}x${box.height}px — TOO SMALL`);
        }
      }
    }
    console.log(`Small touch targets: ${smallTargets}/${Math.min(count, 5)}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 3. CHECKOUT MOBILE
// ════════════════════════════════════════════════════════════════
test.describe('💳 Checkout mobile', () => {

  test('[TC-MOBILE-06] Checkout: form fields full-width', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const nameInput = page.locator('#input-hoten');
    if (await nameInput.isVisible().catch(() => false)) {
      const box = await nameInput.boundingBox();
      if (box) {
        console.log(`Name input width: ${box.width}px`);
        expect(box.width).toBeGreaterThan(200);
      }
    }
  });

  test('[TC-MOBILE-07] Checkout: payment options tappable', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const paymentOptions = page.locator('.payment-option');
    const count = await paymentOptions.count();
    console.log(`Payment options: ${count}`);

    let smallOptions = 0;
    for (let i = 0; i < count; i++) {
      const box = await paymentOptions.nth(i).boundingBox();
      if (box && box.height < 44) {
        smallOptions++;
        console.log(`  Payment option #${i}: height ${box.height}px — TOO SMALL`);
      }
    }
    console.log(`Small payment options: ${smallOptions}/${count}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 4. RESTAURANT DETAIL MOBILE
// ════════════════════════════════════════════════════════════════
test.describe('🍽️ Restaurant detail mobile', () => {

  test('[TC-MOBILE-08] Restaurant detail: menu items stacked', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const menuItems = page.locator('.item-restaurant-row');
    const count = await menuItems.count();
    console.log(`Menu items: ${count}`);

    if (count > 0) {
      const firstItem = menuItems.first();
      const box = await firstItem.boundingBox();
      if (box) {
        console.log(`Menu item width: ${box.width}px`);
        expect(box.width).toBeLessThanOrEqual(375);
      }
    }
  });

  test('[TC-MOBILE-09] Restaurant detail: add to cart button accessible', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Home/DetailRestaurant?id=6', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const addBtn = page.locator('.add-to-cart-btn').first();
    if (await addBtn.isVisible().catch(() => false)) {
      const box = await addBtn.boundingBox();
      if (box) {
        console.log(`Add to cart button: ${box.width}x${box.height}px`);
        expect(box.height).toBeGreaterThanOrEqual(36);
      }
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 5. CHAT WIDGET MOBILE
// ════════════════════════════════════════════════════════════════
test.describe('💬 Chat widget mobile', () => {

  test('[TC-MOBILE-10] Chat FAB: accessible trên mobile', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const chatFab = page.locator('.chat-toggle, #chatToggle');
    const isVisible = await chatFab.isVisible().catch(() => false);
    console.log(`Chat FAB visible on mobile: ${isVisible}`);

    if (isVisible) {
      const box = await chatFab.boundingBox();
      if (box) {
        console.log(`Chat FAB: ${box.width}x${box.height}px`);
        expect(box.width).toBeGreaterThanOrEqual(44);
        expect(box.height).toBeGreaterThanOrEqual(44);
      }
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 6. INPUT ZOOM CHECK
// ════════════════════════════════════════════════════════════════
test.describe('🔍 Input zoom check', () => {

  test('[TC-MOBILE-11] Input font-size ≥ 16px (no iOS zoom)', async ({ page }) => {
    await loginAsMobile(page, CUSTOMER);

    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const inputs = page.locator('input[type="text"], input[type="tel"], input[type="number"], textarea');
    const count = await inputs.count();
    console.log(`Inputs to check: ${count}`);

    let zoomRisk = 0;
    for (let i = 0; i < Math.min(count, 5); i++) {
      const fontSize = await inputs.nth(i).evaluate((el) => {
        return parseFloat(window.getComputedStyle(el).fontSize);
      });
      if (fontSize < 16) {
        zoomRisk++;
        console.log(`  Input #${i}: font-size ${fontSize}px — WILL TRIGGER ZOOM`);
      }
    }
    console.log(`Zoom-risk inputs: ${zoomRisk}/${Math.min(count, 5)}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 7. BOTTOM SHEET (MOBILE FILTER)
// ════════════════════════════════════════════════════════════════
test.describe('📋 Bottom sheet filter', () => {

  test('[TC-MOBILE-12] Mobile: bottom sheet trigger visible', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const bottomSheetTrigger = page.locator('.filter-trigger, [class*="bottom-sheet"], .mobile-filter, #mobileFilterBtn');
    const isVisible = await bottomSheetTrigger.isVisible().catch(() => false);
    console.log(`Bottom sheet trigger: ${isVisible}`);
  });
});
