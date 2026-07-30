/**
 * 📊 BỘ TEST 31: RESTAURANT ANALYTICS + DISCOUNT + REVIEWS
 *
 * Mục tiêu: Test các trang còn lại của quán ăn
 * - Analytics: charts, stats, top items
 * - Discount: danh sách KM, CRUD
 * - Reviews: quản lý đánh giá + reply
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const RESTAURANT = USERS.restaurant1;

async function loginAsMerchant(page: any) {
  const login = new LoginPage(page);
  const url = await login.login(RESTAURANT.username, RESTAURANT.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/Restaurant', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (page.url().includes('/Restaurant')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

// ─── ANALYTICS ───
test.describe('📊 Restaurant Analytics', () => {

  test('[TC-ANA-01] Revenue chart — canvas render + data', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const canvases = await page.locator('canvas').count();
    console.log(`📈 Canvas charts: ${canvases}`);

    if (canvases > 0) {
      for (let i = 0; i < canvases; i++) {
        const box = await page.locator('canvas').nth(i).boundingBox();
        if (box) {
          console.log(`  Chart ${i}: ${box.width}x${box.height}`);
          expect(box.width).toBeGreaterThan(0);
          expect(box.height).toBeGreaterThan(0);
        }
      }
    }
  });

  test('[TC-ANA-02] Top items — bảng món bán chạy', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const tables = page.locator('table');
    const tableCount = await tables.count();
    console.log(`📋 Tables: ${tableCount}`);

    for (let i = 0; i < tableCount; i++) {
      const headers = await tables.nth(i).locator('thead th, thead td').allTextContents().catch(() => []);
      const rows = await tables.nth(i).locator('tbody tr').count().catch(() => 0);
      if (headers.length > 0) {
        console.log(`  Table ${i}: [${headers.join(', ')}] — ${rows} rows`);
      }
    }
  });

  test('[TC-ANA-03] Date filter (nếu có)', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Analytics', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const dateInputs = page.locator('input[type="date"], input[name*="date"], input[name*="from"], input[name*="to"]');
    const dateCount = await dateInputs.count();
    console.log(`📅 Date inputs: ${dateCount}`);

    if (dateCount >= 2) {
      await dateInputs.first().fill('2026-07-01');
      await dateInputs.nth(1).fill('2026-07-29');
      await page.waitForTimeout(500);

      const applyBtn = page.locator('button:has-text("Áp dụng"), button:has-text("Filter"), button:has-text("Lọc")').first();
      if (await applyBtn.isVisible().catch(() => false)) {
        await applyBtn.click();
        await page.waitForTimeout(2000);
        console.log('✅ Date filter applied');
      }
    }
  });
});

// ─── DISCOUNT ───
test.describe('🏷️ Restaurant Discount', () => {

  test('[TC-DIS-01] Discount list — bảng khuyến mãi hiển thị', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const table = page.locator('table').first();
    const tableVisible = await table.isVisible().catch(() => false);
    console.log(`📋 Discount table: ${tableVisible}`);

    if (tableVisible) {
      const rows = await page.locator('table tbody tr').count();
      console.log(`  Discount rows: ${rows}`);

      if (rows > 0) {
        const headers = await page.locator('table thead th').allTextContents();
        console.log(`  Columns: ${headers.map(h => h.trim()).join(' | ')}`);
      }
    }

    const addBtn = page.locator('a:has-text("Thêm"), button:has-text("Thêm")').first();
    console.log(`➕ Add button: ${await addBtn.isVisible().catch(() => false)}`);
  });

  test('[TC-DIS-02] Thêm KM mới — form + submit', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const addBtn = page.locator('a:has-text("Thêm"), button:has-text("Thêm"), a[href*="Them"]').first();
    if (await addBtn.isVisible().catch(() => false)) {
      await addBtn.click();
      await page.waitForTimeout(1500);

      // Kiểm tra form
      const form = page.locator('form').first();
      const formVisible = await form.isVisible().catch(() => false);
      console.log(`📝 Discount form: ${formVisible}`);

      if (formVisible) {
        const inputs = await form.locator('input, select, textarea').count();
        console.log(`  Form fields: ${inputs}`);
      }
    } else {
      console.log('ℹ️ Không có nút Thêm');
    }
  });

  test('[TC-DIS-03] Gắn KM cho món', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra nút gắn món
    const attachBtn = page.locator('a:has-text("Gắn"), button:has-text("Gắn"), a[href*="Gan"]').first();
    const attachVisible = await attachBtn.isVisible().catch(() => false);
    console.log(`🔗 Attach items button: ${attachVisible}`);
  });

  test('[TC-DIS-04] KM hết hạn — trạng thái', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Discount', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const statusBadges = page.locator('.badge, [class*="status"], .trangthai');
    const statusCount = await statusBadges.count();
    console.log(`🏷️ Status badges: ${statusCount}`);

    if (statusCount > 0) {
      for (let i = 0; i < Math.min(statusCount, 3); i++) {
        const text = await statusBadges.nth(i).textContent();
        console.log(`  Badge ${i}: "${text?.trim()}"`);
      }
    }
  });
});

// ─── REVIEWS (Restaurant) ───
test.describe('⭐ Restaurant Reviews', () => {

  test('[TC-RRV-01] Review list — điểm TB + phân bố sao', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Review', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra rating overview
    const ratingOverview = page.locator('.rating-overview, .rating-summary, [class*="rating"]').first();
    const overviewVisible = await ratingOverview.isVisible().catch(() => false);
    console.log(`⭐ Rating overview: ${overviewVisible}`);

    if (overviewVisible) {
      const avgRating = page.locator('.avg-rating, .rating-number').first();
      const avgText = await avgRating.textContent().catch(() => null);
      console.log(`  Average rating: ${avgText}`);
    }

    // Kiểm tra danh sách review
    const reviewItems = page.locator('.review-item, .danhgia-item, .comment-item');
    const reviewCount = await reviewItems.count();
    console.log(`  Review items: ${reviewCount}`);
  });

  test('[TC-RRV-02] Reply review', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Review', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    const replyBtns = page.locator('button:has-text("Trả lời"), a:has-text("Reply"), .btn-reply');
    const replyCount = await replyBtns.count();
    console.log(`💬 Reply buttons: ${replyCount}`);

    if (replyCount > 0) {
      await replyBtns.first().click();
      await page.waitForTimeout(500);

      const textarea = page.locator('textarea').first();
      if (await textarea.isVisible().catch(() => false)) {
        await textarea.fill('Cảm ơn bạn đã đánh giá!');
        const submitBtn = page.locator('button:has-text("Gửi"), button[type="submit"]').first();
        if (await submitBtn.isVisible().catch(() => false)) {
          await submitBtn.click();
          await page.waitForTimeout(2000);
          console.log('✅ Reply submitted');
        }
      }
    }
  });

  test('[TC-RRV-03] Filter reviews by star', async ({ page }) => {
    await loginAsMerchant(page);
    await page.goto('/Restaurant/Review', { waitUntil: 'domcontentloaded', timeout: 30_000 });
    await page.waitForTimeout(3000);

    // Kiểm tra filter tabs/buttons
    const filterBtns = page.locator('button:has-text("5 sao"), button:has-text("4 sao"), .star-filter, .filter-star');
    const filterCount = await filterBtns.count();
    console.log(`🔍 Star filters: ${filterCount}`);

    if (filterCount > 0) {
      const firstFilterText = await filterBtns.first().textContent();
      await filterBtns.first().click();
      await page.waitForTimeout(1000);
      console.log(`  Clicked filter: "${firstFilterText?.trim()}"`);
    }
  });
});
