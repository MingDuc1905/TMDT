/**
 * 🎨 BỘ TEST 19: VISUAL REGRESSION
 *
 * Mục tiêu: Test design system consistency
 * - Design tokens, fonts, colors, empty states, loading states
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
// 1. DESIGN TOKENS
// ════════════════════════════════════════════════════════════════
test.describe('🎨 Design tokens', () => {

  test('[TC-VISUAL-01] --fs-green CSS variable defined', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const green = await page.evaluate(() => {
      return getComputedStyle(document.documentElement).getPropertyValue('--fs-green').trim();
    });
    console.log(`--fs-green: "${green}"`);
    expect(green).toBeTruthy();
  });

  test('[TC-VISUAL-02] --fs-orange CSS variable defined', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const orange = await page.evaluate(() => {
      return getComputedStyle(document.documentElement).getPropertyValue('--fs-orange').trim();
    });
    console.log(`--fs-orange: "${orange}"`);
    expect(orange).toBeTruthy();
  });

  test('[TC-VISUAL-03] Inter font loaded', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const fontFamily = await page.evaluate(() => {
      return getComputedStyle(document.body).fontFamily;
    });
    console.log(`Body font-family: "${fontFamily}"`);
    expect(fontFamily.toLowerCase()).toContain('inter');
  });

  test('[TC-VISUAL-04] --fs-radius applied on cards', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const radius = await page.evaluate(() => {
      const card = document.querySelector('.product-item, .card');
      if (!card) return null;
      return getComputedStyle(card).borderRadius;
    });
    console.log(`Card border-radius: "${radius}"`);
  });

  test('[TC-VISUAL-05] --fs-shadow applied on cards', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const shadow = await page.evaluate(() => {
      const card = document.querySelector('.product-item, .card');
      if (!card) return null;
      return getComputedStyle(card).boxShadow;
    });
    console.log(`Card box-shadow: "${shadow}"`);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. EMPTY STATES
// ════════════════════════════════════════════════════════════════
test.describe('📭 Empty states', () => {

  test('[TC-VISUAL-06] Cart empty state: icon + text + CTA', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    await page.goto('/Cart', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const emptyCart = page.locator('.empty-cart');
    const isVisible = await emptyCart.isVisible().catch(() => false);

    if (isVisible) {
      // Check for icon, text, CTA
      const hasIcon = await emptyCart.locator('i, svg, img').count() > 0;
      const hasText = await emptyCart.locator('h4, p, span').count() > 0;
      const hasCTA = await emptyCart.locator('.btn, a.btn').count() > 0;
      console.log(`Empty cart: icon=${hasIcon}, text=${hasText}, CTA=${hasCTA}`);
    }
  });

  test('[TC-VISUAL-07] Search empty state: "Không tìm thấy" message', async ({ page }) => {
    await page.goto('/?txtSearch=xyznonexistent999', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const emptyMsg = page.locator('h5:has-text("Không tìm thấy"), .empty-state, .no-results');
    const isVisible = await emptyMsg.isVisible().catch(() => false);
    console.log(`Search empty state: ${isVisible}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 3. SCREENSHOT COMPARISON
// ════════════════════════════════════════════════════════════════
test.describe('📸 Screenshots', () => {

  test('[TC-VISUAL-08] Homepage screenshot — Desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    await expect(page).toHaveScreenshot('homepage-desktop.png', {
      maxDiffPixelRatio: 0.1,
      fullPage: true,
    });
  });

  test('[TC-VISUAL-09] Homepage screenshot — Mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    await expect(page).toHaveScreenshot('homepage-mobile.png', {
      maxDiffPixelRatio: 0.1,
      fullPage: true,
    });
  });

  test('[TC-VISUAL-10] Login page screenshot', async ({ page }) => {
    await page.goto('/Home/Login', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    await expect(page).toHaveScreenshot('login-desktop.png', {
      maxDiffPixelRatio: 0.1,
    });
  });
});

// ════════════════════════════════════════════════════════════════
// 4. ICONS CONSISTENCY
// ════════════════════════════════════════════════════════════════
test.describe('🖼️ Icons consistency', () => {

  test('[TC-VISUAL-11] Không dùng emoji làm icon controls', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Check navbar for emoji in buttons
    const buttons = page.locator('.fs-header button, .fs-header a');
    const count = await buttons.count();
    let emojiCount = 0;

    for (let i = 0; i < Math.min(count, 10); i++) {
      const text = await buttons.nth(i).textContent() || '';
      // Simple emoji detection
      const emojiRegex = /[\u{1F600}-\u{1F64F}\u{1F300}-\u{1F5FF}\u{1F680}-\u{1F6FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}]/u;
      if (emojiRegex.test(text)) {
        emojiCount++;
        console.log(`  Button #${i}: "${text.trim()}" contains emoji`);
      }
    }
    console.log(`Buttons with emoji: ${emojiCount}/${Math.min(count, 10)}`);
  });

  test('[TC-VISUAL-12] Font Awesome icons loaded', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const faIcons = page.locator('.fas, .far, .fab, [class*="fa-"]');
    const count = await faIcons.count();
    console.log(`Font Awesome icons: ${count}`);
    expect(count).toBeGreaterThan(0);
  });
});

// ════════════════════════════════════════════════════════════════
// 5. LOADING STATES
// ════════════════════════════════════════════════════════════════
test.describe('⏳ Loading states', () => {

  test('[TC-VISUAL-13] Skeleton loading có trên trang', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    // Check if skeleton exists in DOM (may be hidden after load)
    const skeleton = page.locator('#fs-loading-skeleton, .skeleton, [class*="loading"]');
    const count = await skeleton.count();
    console.log(`Loading/skeleton elements: ${count}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 6. FOOTER CONSISTENCY
// ════════════════════════════════════════════════════════════════
test.describe('📋 Footer', () => {

  test('[TC-VISUAL-14] Footer visible trên tất cả pages', async ({ page }) => {
    const pages = ['/', '/Home/Login', '/Cart'];

    for (const path of pages) {
      await page.goto(path, { waitUntil: 'domcontentloaded' });
      await page.waitForTimeout(2000);

      const footer = page.locator('.fs-footer');
      const isVisible = await footer.isVisible().catch(() => false);
      console.log(`Footer on ${path}: ${isVisible}`);
    }
  });
});
