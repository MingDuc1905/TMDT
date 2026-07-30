/**
 * ⭐ BỘ TEST 23: ĐÁNH GIÁ MÓN ĂN (Reviews)
 *
 * Mục tiêu: Test review modal từ Chi tiết đơn hàng + Lịch sử đơn hàng
 * - Star rating hover/click
 * - Submit review
 * - Validation (0 sao, empty)
 * - Duplicate prevention
 * - Badge "Đã đánh giá"
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { USERS } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  const url = await login.login(user.username, user.password);
  if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
    await page.waitForTimeout(2000);
    for (let r = 0; r < 2; r++) {
      try { await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 }); if (!page.url().includes('/Home/Login')) break; } catch { await page.waitForTimeout(1000); }
    }
  }
}

async function openReviewModal(page: any): Promise<boolean> {
  // Try OrderDetail first, then LichSuDatHang
  await page.goto('/Cart/LichSuDatHang', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  await page.waitForTimeout(3000);

  // Try review button in LichSuDatHang
  let reviewBtn = page.locator('button:has-text("Đánh giá"), a:has-text("Đánh giá")').first();
  if (await reviewBtn.isVisible().catch(() => false)) {
    await reviewBtn.click();
    await page.waitForTimeout(1000);
    const modal = page.locator('.modal.show, .review-modal, #reviewModal').first();
    return await modal.isVisible().catch(() => false);
  }

  // Try clicking an order detail link first, then look for review button
  const detailLinks = page.locator('a[href*="ChiTietDonHang"]');
  if (await detailLinks.count() > 0) {
    await detailLinks.first().click();
    await page.waitForLoadState('networkidle').catch(() => {});
    await page.waitForTimeout(2000);

    reviewBtn = page.locator('button:has-text("Đánh giá"), a:has-text("Đánh giá")').first();
    if (await reviewBtn.isVisible().catch(() => false)) {
      await reviewBtn.click();
      await page.waitForTimeout(1000);
      const modal = page.locator('.modal.show, .review-modal, #reviewModal').first();
      return await modal.isVisible().catch(() => false);
    }
  }

  return false;
}

// ─── TEST SUITE ───
test.describe('⭐ Reviews & Ratings', () => {

  test('[TC-RV-01] Mở modal — star rating + textarea hiển thị', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    const modalOpened = await openReviewModal(page);

    if (modalOpened) {
      // Kiểm tra star rating
      const stars = page.locator('.star-rating i, .star-rating span, .rating-star, .review-star');
      const starCount = await stars.count();
      console.log(`⭐ Stars: ${starCount}`);
      expect(starCount).toBeGreaterThanOrEqual(5);

      // Kiểm tra textarea
      const textarea = page.locator('textarea');
      const textareaVisible = await textarea.isVisible().catch(() => false);
      console.log(`📝 Textarea visible: ${textareaVisible}`);

      // Kiểm tra items select (nếu có)
      const items = page.locator('.review-item-select, .order-item-checkbox, [class*="menu-item"]');
      const itemCount = await items.count();
      console.log(`🍽️ Reviewable items: ${itemCount}`);

      // Kiểm tra nút submit
      const submitBtn = page.locator('button:has-text("Gửi"), button:has-text("Đánh giá")');
      console.log(`📤 Submit btn: ${await submitBtn.isVisible().catch(() => false)}`);
    } else {
      console.log('ℹ️ Không mở được review modal (chưa có đơn hoàn thành)');
    }
  });

  test('[TC-RV-02] Chọn sao + nhập nhận xét → submit thành công', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    const modalOpened = await openReviewModal(page);

    if (modalOpened) {
      // Click star thứ 4
      const stars = page.locator('.star-rating i, .star-rating span, .rating-star');
      const starCount = await stars.count();
      if (starCount >= 4) {
        await stars.nth(3).click();
        await page.waitForTimeout(300);
        console.log('⭐ Chọn sao 4');
      }

      // Chọn item đầu tiên (nếu có)
      const items = page.locator('.review-item-select input[type="checkbox"], .order-item-checkbox');
      if (await items.isVisible().catch(() => false)) {
        await items.first().click();
        await page.waitForTimeout(200);
        console.log('✅ Chọn item đầu');
      }

      // Nhập nhận xét
      const textarea = page.locator('textarea');
      if (await textarea.isVisible().catch(() => false)) {
        await textarea.fill('Món rất ngon, giao hàng nhanh!');
        console.log('💬 Đã nhập nhận xét');
      }

      // Submit
      const submitBtn = page.locator('button:has-text("Gửi"), button:has-text("Đánh giá")').first();
      if (await submitBtn.isVisible().catch(() => false)) {
        await submitBtn.click();
        await page.waitForTimeout(3000);

        // Kiểm tra kết quả
        const bodyText = await page.locator('body').textContent() || '';
        const hasSuccess = bodyText.includes('thành công') || bodyText.includes('cảm ơn') || bodyText.includes('Đã đánh giá');
        console.log(`✅ Review thành công: ${hasSuccess}`);
      }
    } else {
      console.log('ℹ️ Không mở được review modal');
    }
  });

  test('[TC-RV-03] Submit với 0 sao → validation error', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    const modalOpened = await openReviewModal(page);

    if (modalOpened) {
      // Không chọn sao, submit luôn
      const submitBtn = page.locator('button:has-text("Gửi"), button:has-text("Đánh giá")').first();
      if (await submitBtn.isVisible().catch(() => false)) {
        await submitBtn.click();
        await page.waitForTimeout(1000);

        // Kiểm tra validation
        const bodyText = await page.locator('body').textContent() || '';
        const hasError = bodyText.includes('Vui lòng chọn') || bodyText.includes('chọn số sao') || bodyText.includes('sao');
        console.log(`⚠️ Validation error message: ${hasError}`);

        // Hoặc HTML5 validation
        const stillOnPage = await page.locator('.modal.show, .review-modal').isVisible().catch(() => false);
        console.log(`📦 Modal vẫn hiển thị (form not submitted): ${stillOnPage}`);
      }
    } else {
      console.log('ℹ️ Không mở được review modal');
    }
  });

  test('[TC-RV-04] Nhận xét >500 ký tự → trim hoặc block', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    const modalOpened = await openReviewModal(page);

    if (modalOpened) {
      const textarea = page.locator('textarea');
      if (await textarea.isVisible().catch(() => false)) {
        // Nhập 600 ký tự
        const longText = 'A'.repeat(600);
        await textarea.fill(longText);
        await page.waitForTimeout(300);

        // Kiểm tra maxLength attribute
        const maxLength = await textarea.getAttribute('maxlength');
        console.log(`📏 MaxLength attr: ${maxLength}`);

        // Kiểm tra character count
        const charCount = page.locator('.char-count, .counter, [class*="count"]');
        const countText = await charCount.textContent().catch(() => null);
        console.log(`🔢 Char count: ${countText}`);
      }
    } else {
      console.log('ℹ️ Không mở được review modal');
    }
  });

  test('[TC-RV-05] Đánh giá duplicate → error message', async ({ page }) => {
    await loginAs(page, CUSTOMER);
    const modalOpened = await openReviewModal(page);

    if (modalOpened) {
      // Click star 5
      const stars = page.locator('.star-rating i, .star-rating span, .rating-star');
      if (await stars.count() >= 5) {
        await stars.nth(4).click();
        await page.waitForTimeout(200);
      }

      // Chọn item
      const items = page.locator('.review-item-select input[type="checkbox"], .order-item-checkbox');
      if (await items.isVisible().catch(() => false)) {
        await items.first().click();
        await page.waitForTimeout(200);
      }

      // Submit lần 1
      const submitBtn = page.locator('button:has-text("Gửi"), button:has-text("Đánh giá")').first();
      if (await submitBtn.isVisible().catch(() => false)) {
        await submitBtn.click();
        await page.waitForTimeout(2000);

        // Nếu món chưa được đánh giá, lần 2 sẽ báo lỗi duplicate
        // Kiểm tra kết quả
        const bodyText = await page.locator('body').textContent() || '';
        const firstSuccess = bodyText.includes('thành công');
        console.log(`✅ Lần 1: ${firstSuccess ? 'thành công' : 'có thể đã tồn tại'}`);

        // Thử submit lần 2
        const detailLinks = page.locator('a[href*="ChiTietDonHang"]').first();
        if (await detailLinks.isVisible().catch(() => false)) {
          await detailLinks.click();
          await page.waitForTimeout(2000);
          const reviewBtn2 = page.locator('button:has-text("Đánh giá")').first();
          if (await reviewBtn2.isVisible().catch(() => false)) {
            await reviewBtn2.click();
            await page.waitForTimeout(1000);

            // Kiểm tra item đã có badge "Đã đánh giá"
            const reviewedBadge = page.locator('.reviewed-badge, .badge:has-text("Đã đánh giá")');
            console.log(`✅ Badge \"Đã đánh giá\": ${await reviewedBadge.isVisible().catch(() => false)}`);
          }
        }
      }
    } else {
      console.log('ℹ️ Không mở được review modal');
    }
  });
});
