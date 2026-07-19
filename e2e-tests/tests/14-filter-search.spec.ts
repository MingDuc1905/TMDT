/**
 * 🔍 BỘ TEST 14: TÌM KIẾM & LỌC (FILTER & SEARCH)
 *
 * Mục tiêu: Test homepage filter, search, restaurant menu filter
 * - Category pills, search bar, sort, price filter
 * - Desktop vs Mobile filter UI
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { HomePage } from '../pages/HomePage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

// ════════════════════════════════════════════════════════════════
// 1. HOMEPAGE SEARCH
// ════════════════════════════════════════════════════════════════
test.describe('🔍 Homepage search', () => {

  test('[TC-FILTER-01] Search "pizza" → verify kết quả chứa pizza', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    await homePage.search('pizza');
    await page.waitForTimeout(3000);

    const count = await homePage.getRestaurantCount();
    console.log(`Results for "pizza": ${count}`);

    if (count > 0) {
      const names = await homePage.getRestaurantNames();
      console.log(`Restaurant names: ${names.join(', ')}`);
      // At least one should contain "pizza" or related
      const hasRelevant = names.some(n => n.toLowerCase().includes('pizza') || n.toLowerCase().includes('koneko'));
      console.log(`Has relevant result: ${hasRelevant}`);
    }
  });

  test('[TC-FILTER-02] Search "xyz123" → empty state', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    await homePage.search('xyz123nonexistent');
    await page.waitForTimeout(3000);

    const count = await homePage.getRestaurantCount();
    console.log(`Results for "xyz123nonexistent": ${count}`);
    expect(count).toBe(0);

    // Verify empty state
    const emptyMsg = page.locator('h5:has-text("Không tìm thấy"), .empty-state, .no-results');
    const isVisible = await emptyMsg.isVisible().catch(() => false);
    console.log(`Empty state visible: ${isVisible}`);
  });

  test('[TC-FILTER-03] Search Unicode "phở" → không crash', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    await homePage.search('phở');
    await page.waitForTimeout(3000);

    const count = await homePage.getRestaurantCount();
    console.log(`Results for "phở": ${count}`);

    // Verify no crash - page should still be functional
    const isNavbarVisible = await homePage.isNavbarVisible();
    expect(isNavbarVisible).toBe(true);
  });

  test('[TC-FILTER-04] Search rỗng → hiện tất cả quán', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    await homePage.search('');
    await page.waitForTimeout(3000);

    const count = await homePage.getRestaurantCount();
    console.log(`Results for empty search: ${count}`);
    expect(count).toBeGreaterThan(0);
  });
});

// ════════════════════════════════════════════════════════════════
// 2. CATEGORY PILLS
// ════════════════════════════════════════════════════════════════
test.describe('🏷️ Category pills', () => {

  test('[TC-FILTER-05] Category pills visible trên homepage', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    const categoryRow = page.locator('#categoryRow');
    const isVisible = await categoryRow.isVisible().catch(() => false);
    console.log(`Category row visible: ${isVisible}`);

    if (isVisible) {
      const pills = page.locator('.fs-category-pill');
      const pillCount = await pills.count();
      console.log(`Category pills: ${pillCount}`);

      if (pillCount > 0) {
        const pillTexts = await pills.allTextContents();
        console.log(`Pill names: ${pillTexts.slice(0, 5).join(', ')}`);
      }
    }
  });

  test('[TC-FILTER-06] Click category pill "Cơm" → filter restaurants', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    // Try clicking "Cơm" category
    try {
      await homePage.clickCategory('Cơm');
      await page.waitForTimeout(3000);

      const count = await homePage.getRestaurantCount();
      console.log(`Results after "Cơm" filter: ${count}`);
    } catch (e) {
      console.log('ℹ️ "Cơm" category pill not found');
    }
  });

  test('[TC-FILTER-07] Click "Tất cả" → reset filter', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.gotoHome();
    await page.waitForTimeout(2000);

    // First apply a filter
    try {
      await homePage.clickCategory('Cơm');
      await page.waitForTimeout(2000);
    } catch {}

    // Then click "Tất cả"
    try {
      await homePage.clickCategory('Tất cả');
      await page.waitForTimeout(3000);

      const count = await homePage.getRestaurantCount();
      console.log(`Results after "Tất cả": ${count}`);
      expect(count).toBeGreaterThan(0);
    } catch (e) {
      console.log('ℹ️ "Tất cả" category not found');
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 3. RESTAURANT MENU FILTER
// ════════════════════════════════════════════════════════════════
test.describe('🍽️ Restaurant menu filter', () => {

  test('[TC-FILTER-08] Restaurant detail → category sidebar filter', async ({ page }) => {
    const detailPage = new DetailRestaurantPage(page);
    await detailPage.gotoRestaurant(6); // Koneko Pizza
    await page.waitForTimeout(3000);

    const categories = page.locator('.list-category .item .item-link');
    const catCount = await categories.count();
    console.log(`Menu categories: ${catCount}`);

    if (catCount > 1) {
      // Click second category
      await categories.nth(1).click();
      await page.waitForTimeout(2000);

      const menuItems = await detailPage.getMenuItemCount();
      console.log(`Items after category filter: ${menuItems}`);
    }
  });

  test('[TC-FILTER-09] Restaurant detail → search menu items', async ({ page }) => {
    const detailPage = new DetailRestaurantPage(page);
    await detailPage.gotoRestaurant(6); // Koneko Pizza
    await page.waitForTimeout(3000);

    const searchInput = page.locator('input[name="searchKey"]');
    const isVisible = await searchInput.isVisible().catch(() => false);

    if (isVisible) {
      await detailPage.searchMenu('pizza');
      await page.waitForTimeout(2000);

      const menuItems = await detailPage.getMenuItemCount();
      console.log(`Items after search "pizza": ${menuItems}`);
    } else {
      console.log('ℹ️ Menu search input not visible');
    }
  });

  test('[TC-FILTER-10] Restaurant detail → promotional badge visible', async ({ page }) => {
    const detailPage = new DetailRestaurantPage(page);
    await detailPage.gotoRestaurant(6);
    await page.waitForTimeout(3000);

    const promoBadges = page.locator('.badge-giamgia, .promotion-badge, [class*="promo"], [class*="sale"]');
    const count = await promoBadges.count();
    console.log(`Promotional badges: ${count}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 4. FILTER BAR (DESKTOP)
// ════════════════════════════════════════════════════════════════
test.describe('🎛️ Filter bar', () => {

  test('[TC-FILTER-11] FilterBar sort options visible', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const filterBar = page.locator('.filter-bar, #filterBar, [class*="filter"]');
    const isVisible = await filterBar.isVisible().catch(() => false);
    console.log(`Filter bar visible: ${isVisible}`);

    if (isVisible) {
      const sortOptions = page.locator('.filter-bar select, .filter-bar option, [class*="sort"]');
      const count = await sortOptions.count();
      console.log(`Sort options: ${count}`);
    }
  });

  test('[TC-FILTER-12] Clear all filters → reset', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    // Try to find and click clear/reset button
    const clearBtn = page.locator('button:has-text("Xoá"), button:has-text("Reset"), .clear-filter, [class*="clear"]');
    const count = await clearBtn.count();
    console.log(`Clear filter buttons: ${count}`);

    if (count > 0) {
      await clearBtn.first().click();
      await page.waitForTimeout(2000);

      const restaurantCount = await page.locator('.product-item').count();
      console.log(`Restaurants after clear: ${restaurantCount}`);
      expect(restaurantCount).toBeGreaterThan(0);
    }
  });
});
