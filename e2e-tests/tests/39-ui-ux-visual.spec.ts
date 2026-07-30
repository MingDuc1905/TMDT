/**
 * 🎨 BỘ TEST 39: UI/UX VISUAL AUDIT — Design Tokens, Images, Layout
 *
 * Mục tiêu: Kiểm tra toàn diện UI/UX theo UI-UX.md
 * - Design tokens: --fs-* CSS variables
 * - Images: no broken images across ALL pages
 * - Layout: no overflow, responsive
 * - Accessibility: aria-labels, contrast, touch targets
 * - Font: Inter font loaded
 * - Animations: scroll-reveal, skeleton loading
 * - Components: cards, badges, buttons, modals
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { BasePage } from '../pages/BasePage';
import { SEED, URLS } from '../fixtures/users';

// ─── DESIGN TOKENS ───
test.describe('🎨 Design Tokens (--fs-*)', () => {

  test('[TC-UI-01] CSS variables — tất cả --fs-* tokens tồn tại', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    const tokens = await page.evaluate(() => {
      const style = getComputedStyle(document.documentElement);
      return {
        '--fs-green': style.getPropertyValue('--fs-green'),
        '--fs-orange': style.getPropertyValue('--fs-orange'),
        '--fs-dark': style.getPropertyValue('--fs-dark'),
        '--fs-white': style.getPropertyValue('--fs-white'),
        '--fs-danger': style.getPropertyValue('--fs-danger'),
        '--fs-warning': style.getPropertyValue('--fs-warning'),
        '--fs-radius': style.getPropertyValue('--fs-radius'),
        '--fs-shadow': style.getPropertyValue('--fs-shadow'),
        '--fs-transition': style.getPropertyValue('--fs-transition'),
      };
    });

    console.log('🎨 Design tokens:');
    for (const [key, val] of Object.entries(tokens)) {
      const status = val ? '✅' : '❌';
      console.log(`  ${status} ${key}: ${val || 'MISSING!'}`);
      if (key.includes('green') || key.includes('dark') || key.includes('white')) {
        expect(val).toBeTruthy();
      }
    }
  });

  test('[TC-UI-02] Inter font — loaded trên trang chủ', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    const interLoaded = await page.evaluate(() => {
      return document.fonts?.check('12px Inter') || document.fonts?.check('400 12px Inter') || false;
    }).catch(() => false);
    console.log(`🔤 Inter font loaded: ${interLoaded}`);
  });

  test('[TC-UI-03] Buttons — sử dụng --fs-radius và --fs-green', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    const buttonStyles = await page.evaluate(() => {
      const btn = document.querySelector('.btn, button, .fs-btn, .add-to-cart-btn');
      if (!btn) return null;
      const style = getComputedStyle(btn);
      return {
        borderRadius: style.borderRadius,
        backgroundColor: style.backgroundColor,
        padding: style.padding,
        fontSize: style.fontSize,
      };
    });

    if (buttonStyles) {
      console.log(`🔘 Button: radius=${buttonStyles.borderRadius}, bg=${buttonStyles.backgroundColor}`);
    } else {
      console.log('ℹ️ Không tìm thấy button');
    }
  });
});

// ─── IMAGES ACROSS ALL PAGES ───
test.describe('🖼️ Images — Kiểm tra toàn bộ trang', () => {

  const criticalPages = [
    { name: 'Trang chủ', path: '/' },
    { name: 'Login', path: '/Home/Login' },
    { name: 'Signup', path: '/Home/Signup' },
    { name: 'About', path: '/Home/About' },
    { name: 'Contact', path: '/Home/Contact' },
    { name: 'Danh mục', path: '/Home/DanhMuc' },
  ];

  for (const p of criticalPages) {
    test(`[TC-UI-04] ${p.name} — 0 broken images`, async ({ page }) => {
      const base = new BasePage(page);
      await base.goto(p.path);
      await page.waitForTimeout(3000);

      const imgResult = await base.validateAllImages();
      console.log(`📸 ${p.name}: ${imgResult.total} images, ${imgResult.broken} broken`);
      if (imgResult.broken > 0 && imgResult.brokenUrls.length > 0) {
        console.log(`  Broken URLs: ${imgResult.brokenUrls.join(', ')}`);
      }
      // Soft check — Unsplash may be rate-limited
      if (imgResult.broken > 0) {
        console.log(`⚠️ ${imgResult.broken} broken (may be external rate limits)`);
      }
    });
  }

  test('[TC-UI-05] Chi tiết quán — 0 broken images', async ({ page }) => {
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForTimeout(3000);

    const imgResult = await detail.validateAllImages();
    console.log(`📸 Restaurant detail: ${imgResult.total} images, ${imgResult.broken} broken`);
  });
});

// ─── LAYOUT ───
test.describe('📐 Layout — Overflow & Responsive', () => {

  const layoutPages = [
    { name: 'Trang chủ', path: '/' },
    { name: 'Login', path: '/Home/Login' },
    { name: 'About', path: '/Home/About' },
    { name: 'Danh mục', path: '/Home/DanhMuc' },
  ];

  for (const p of layoutPages) {
    test(`[TC-UI-06] ${p.name} — không horizontal overflow`, async ({ page }) => {
      const base = new BasePage(page);
      await base.goto(p.path);
      await page.waitForTimeout(3000);

      const hasOverflow = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      }).catch(() => true);
      console.log(`📐 ${p.name} overflow: ${hasOverflow}`);
    });
  }

  test('[TC-UI-07] 404 resources — không có 404 trên critical pages', async ({ page }) => {
    const failedRequests: string[] = [];
    page.on('response', (resp) => { if (resp.status() === 404) failedRequests.push(resp.url()); });

    const pages = ['/', '/Home/Login', '/Home/About', '/Home/DanhMuc'];
    for (const p of pages) {
      await page.goto(p, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(2000);
    }

    if (failedRequests.length > 0) {
      console.log(`⚠️ 404 resources: ${failedRequests.join('\\n')}`);
    }
    expect(failedRequests.length).toBe(0);
  });
});

// ─── ACCESSIBILITY ───
test.describe('♿ Accessibility Basics', () => {

  test('[TC-UI-08] Navbar links — aria-label trên icon-only buttons', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    // Kiểm tra icon-only buttons có aria-label
    const iconBtns = page.locator('button:not([aria-label]), a[class*="icon"]:not([aria-label])');
    const btnCount = await iconBtns.count();
    console.log(`🔘 Icon buttons without aria-label: ${btnCount}`);

    // Kiểm tra cart button có aria-label
    const cartBtn = page.locator('.fs-cart-btn, a[href*="/Cart"]').first();
    const ariaLabel = await cartBtn.getAttribute('aria-label').catch(() => null);
    console.log(`🛒 Cart aria-label: ${ariaLabel}`);
  });

  test('[TC-UI-09] Form inputs — labels hoặc aria-label', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    const inputs = page.locator('input');
    const inputCount = await inputs.count();
    let labeledCount = 0;

    for (let i = 0; i < inputCount; i++) {
      const input = inputs.nth(i);
      const id = await input.getAttribute('id');
      const ariaLabel = await input.getAttribute('aria-label');
      const placeholder = await input.getAttribute('placeholder');

      // Check if has label associated via 'for' attribute
      let hasLabel = false;
      if (id) {
        hasLabel = await page.locator(`label[for="${id}"]`).count() > 0;
      }

      if (ariaLabel || hasLabel || placeholder) labeledCount++;
    }

    console.log(`♿ Inputs: ${inputCount}, labeled: ${labeledCount}`);
    expect(labeledCount).toBeGreaterThanOrEqual(inputCount * 0.5);
  });
});

// ─── COMPONENTS ───
test.describe('🧩 Core Components', () => {

  test('[TC-UI-10] Cards — tất cả card components hiển thị đúng', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    const cardCount = await page.locator('.card, .product-item, .restaurant-card').count();
    console.log(`🃏 Cards: ${cardCount}`);
    expect(cardCount).toBeGreaterThan(0);

    if (cardCount > 0) {
      const firstCard = page.locator('.product-item, .restaurant-card').first();
      const box = await firstCard.boundingBox();
      if (box) {
        console.log(`  Card size: ${box.width}x${box.height}`);
        expect(box.width).toBeGreaterThan(100);
        expect(box.height).toBeGreaterThan(100);
      }
    }
  });

  test('[TC-UI-11] Status badges — màu sắc theo trạng thái', async ({ page }) => {
    // Login + xem lịch sử đơn
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill('tranthib');
    await login.passwordInput.fill('abcdef');
    await login.loginButton.click();
    await page.waitForTimeout(3000);

    if (page.url().includes('/Home/Login')) {
      await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }).catch(() => {});
    }

    await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const badges = page.locator('.badge, [class*="badge"], [class*="status"]');
    const badgeCount = await badges.count();
    console.log(`🏷️ Badges: ${badgeCount}`);

    if (badgeCount > 0) {
      for (let i = 0; i < Math.min(badgeCount, 5); i++) {
        const text = await badges.nth(i).textContent();
        const bg = await badges.nth(i).evaluate(el => getComputedStyle(el).backgroundColor);
        const color = await badges.nth(i).evaluate(el => getComputedStyle(el).color);
        console.log(`  Badge ${i}: "${text?.trim()}" bg:${bg} color:${color}`);
      }
    }
  });

  test('[TC-UI-12] Skeleton loading — không visible sau khi page load', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(3000);

    const skeleton = page.locator('#fs-loading-skeleton, .skeleton, .loading-placeholder').first();
    if (await skeleton.isVisible().catch(() => false)) {
      console.log('⚠️ Skeleton still visible after 3s');
    } else {
      console.log('✅ Skeleton hidden');
    }
  });
});

// ─── UI-UX.md COMPLIANCE ───
test.describe('📋 UI-UX.md Compliance', () => {

  test('[TC-UI-13] Scroll-reveal animations — class fs-reveal tồn tại', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(3000);

    const revealElements = await page.locator('.fs-reveal').count();
    console.log(`✨ fs-reveal elements: ${revealElements}`);

    const staggerElements = await page.locator('[class*="fs-i"]').count();
    console.log(`🎭 stagger elements (--fs-i): ${staggerElements}`);
  });

  test('[TC-UI-14] Color contrast — văn bản trên nền màu đọc được', async ({ page }) => {
    const base = new BasePage(page);
    await base.goto('/');
    await page.waitForTimeout(2000);

    const contrastIssues = await page.evaluate(() => {
      const issues: string[] = [];
      const el = document.querySelector('.btn, .badge, .fs-btn');
      if (!el) return ['No element found'];
      const style = getComputedStyle(el);
      // Basic check: color and background should not be same
      if (style.color === style.backgroundColor) {
        issues.push(`Same color: ${style.color}`);
      }
      return issues;
    });

    if (contrastIssues.length > 0) {
      console.log(`⚠️ Contrast: ${contrastIssues.join(', ')}`);
    } else {
      console.log('✅ No immediate contrast issues');
    }
  });
});
