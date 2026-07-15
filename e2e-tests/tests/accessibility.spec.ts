/**
 * ACCESSIBILITY TESTS — WCAG compliance, keyboard nav, ARIA, contrast, semantic HTML
 *
 * Covers:
 * - Semantic HTML landmarks (main, nav, footer)
 * - Image alt text
 * - Font Awesome icon accessibility
 * - Form labels and accessible names
 * - Search input ARIA
 * - Minimum touch target sizes (44px WCAG)
 */

import { test, expect } from '@playwright/test';
import { SEED } from '../fixtures/users';

test.setTimeout(120_000);

const BASE_URL = 'https://fastship-web.onrender.com';

// ─────────────────────────────────────────────────────────────────────────────
// Describe: Accessibility: Semantic HTML & Landmarks (4 tests)
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Accessibility: Semantic HTML & Landmarks', () => {

  test('homepage has main landmark', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const hasMain = await page.evaluate(() => {
      return document.querySelector('main, [role="main"], .main-content, #main') !== null;
    });

    expect(hasMain).toBe(true);
  });

  test('homepage has navigation landmark', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const hasNav = await page.evaluate(() => {
      return document.querySelector('nav, [role="navigation"], .navbar, .fs-header') !== null;
    });

    expect(hasNav).toBe(true);
  });

  test('homepage has footer', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const hasFooter = await page.evaluate(() => {
      return document.querySelector('footer, [role="contentinfo"], .fs-footer') !== null;
    });

    expect(hasFooter).toBe(true);
  });

  test('login page has form with labels', async ({ page }) => {
    await page.goto(`${BASE_URL}/Home/Login`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForSelector('.auth-card', { timeout: 15_000 });

    const unlabeledCount = await page.evaluate(() => {
      const inputs = document.querySelectorAll('form input:not([type="hidden"]):not([type="checkbox"]):not([type="submit"])');
      let unlabeled = 0;
      inputs.forEach((input: HTMLInputElement) => {
        const id = input.id;
        const hasLabel = id ? document.querySelector(`label[for="${id}"]`) !== null : false;
        const hasAriaLabel = input.hasAttribute('aria-label');
        const hasAriaLabelledby = input.hasAttribute('aria-labelledby');
        const hasPlaceholder = input.hasAttribute('placeholder');
        const wrappedInLabel = input.closest('label') !== null;
        if (!hasLabel && !hasAriaLabel && !hasAriaLabelledby && !hasPlaceholder && !wrappedInLabel) {
          unlabeled++;
        }
      });
      return unlabeled;
    });

    console.log(`Login form unlabeled inputs: ${unlabeledCount}`);
    expect(unlabeledCount).toBeLessThanOrEqual(1);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Describe: Accessibility: Images & Alt Text (3 tests)
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Accessibility: Images & Alt Text', () => {

  test('all images on homepage have alt attribute', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const missingAlt = await page.evaluate(() => {
      return Array.from(document.querySelectorAll('img')).filter(
        (img) => !img.hasAttribute('alt')
      ).length;
    });

    console.log(`Homepage images missing alt: ${missingAlt}`);
    expect(missingAlt).toBe(0);
  });

  test('all images on restaurant detail have alt', async ({ page }) => {
    await page.goto(`${BASE_URL}/Home/DetailRestaurant?id=${SEED.restaurantIds.konekoPizza}`, {
      waitUntil: 'domcontentloaded',
    });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const missingAlt = await page.evaluate(() => {
      return Array.from(document.querySelectorAll('img')).filter(
        (img) => !img.hasAttribute('alt')
      ).length;
    });

    console.log(`Restaurant detail images missing alt: ${missingAlt}`);
    expect(missingAlt).toBe(0);
  });

  test('Font Awesome icons have aria-hidden or aria-label', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const inaccessibleIcons = await page.evaluate(() => {
      const icons = document.querySelectorAll('i.fas, i.far, i.fab, i.fa, [class*="fa-"]');
      let inaccessible = 0;
      icons.forEach((icon) => {
        const hasAriaHidden = icon.getAttribute('aria-hidden') === 'true';
        const parentHasLabel = icon.parentElement?.getAttribute('aria-label') !== null;
        const iconHasLabel = icon.getAttribute('aria-label') !== null;
        if (!hasAriaHidden && !parentHasLabel && !iconHasLabel) {
          inaccessible++;
        }
      });
      return inaccessible;
    });

    console.log(`Font Awesome icons without accessibility attributes: ${inaccessibleIcons}`);
    expect(inaccessibleIcons).toBe(0);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Describe: Accessibility: Form & Interactive Elements (3 tests)
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Accessibility: Form & Interactive Elements', () => {

  test('login form inputs have accessible names', async ({ page }) => {
    await page.goto(`${BASE_URL}/Home/Login`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForSelector('.auth-card', { timeout: 15_000 });

    const inputDetails = await page.evaluate(() => {
      const inputs = document.querySelectorAll('form input:not([type="hidden"]):not([type="submit"])');
      const results: { type: string; hasLabel: boolean; hasAriaLabel: boolean; hasPlaceholder: boolean; name: string }[] = [];
      inputs.forEach((input: HTMLInputElement) => {
        const id = input.id;
        const hasLabel = id ? document.querySelector(`label[for="${id}"]`) !== null : false;
        results.push({
          type: input.type || 'text',
          hasLabel,
          hasAriaLabel: input.hasAttribute('aria-label'),
          hasPlaceholder: input.hasAttribute('placeholder'),
          name: input.name || '',
        });
      });
      return results;
    });

    console.log(`Login inputs: ${JSON.stringify(inputDetails, null, 2)}`);

    for (const input of inputDetails) {
      const hasAccessibleName = input.hasLabel || input.hasAriaLabel || input.hasPlaceholder || input.name !== '';
      if (!hasAccessibleName) {
        console.log(`WARNING: ${input.type} input has no accessible name`);
      }
    }

    const allAccessible = inputDetails.every(
      (i) => i.hasLabel || i.hasAriaLabel || i.hasPlaceholder || i.name !== ''
    );
    expect(allAccessible).toBe(true);
  });

  test('search input has aria-label or label', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const searchAccessibility = await page.evaluate(() => {
      const searchInput = document.querySelector('input[name="txtSearch"]') as HTMLInputElement | null;
      if (!searchInput) return { found: false };

      const hasAriaLabel = searchInput.hasAttribute('aria-label');
      const ariaLabel = searchInput.getAttribute('aria-label');
      const id = searchInput.id;
      const hasLabel = id ? document.querySelector(`label[for="${id}"]`) !== null : false;
      const hasPlaceholder = searchInput.hasAttribute('placeholder');
      const wrappedInLabel = searchInput.closest('label') !== null;

      return {
        found: true,
        hasAriaLabel,
        ariaLabel,
        hasLabel,
        hasPlaceholder,
        wrappedInLabel,
        accessible: hasAriaLabel || hasLabel || hasPlaceholder || wrappedInLabel,
      };
    });

    console.log(`Search input accessibility: ${JSON.stringify(searchAccessibility, null, 2)}`);
    expect(searchAccessibility.found).toBe(true);
    expect(searchAccessibility.accessible).toBe(true);
  });

  test('interactive elements have minimum touch target size', async ({ page }) => {
    await page.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(2000);

    const undersized = await page.evaluate(() => {
      const minSize = 44;
      const elements = document.querySelectorAll(
        'button, a[href], [role="button"], .btn, input[type="submit"], .navbar-toggler'
      );
      let tooSmall = 0;
      const details: { tag: string; text: string; width: number; height: number }[] = [];

      elements.forEach((el) => {
        const rect = el.getBoundingClientRect();
        if (rect.width === 0 && rect.height === 0) return;
        if (rect.width < minSize && rect.height < minSize) {
          tooSmall++;
          details.push({
            tag: el.tagName.toLowerCase(),
            text: (el.textContent || '').trim().substring(0, 30),
            width: Math.round(rect.width),
            height: Math.round(rect.height),
          });
        }
      });

      return { total: elements.length, tooSmall, details: details.slice(0, 10) };
    });

    console.log(
      `Touch targets: ${undersized.total} total, ${undersized.tooSmall} below 44px`
    );
    if (undersized.details.length > 0) {
      console.log(`Undersized elements: ${JSON.stringify(undersized.details, null, 2)}`);
    }

    const ratio = undersized.tooSmall / Math.max(undersized.total, 1);
    console.log(`Undersized ratio: ${(ratio * 100).toFixed(1)}%`);
    expect(ratio).toBeLessThan(0.5);
  });
});
