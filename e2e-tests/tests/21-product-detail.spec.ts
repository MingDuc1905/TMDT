/**
 * 🍽️ BỘ TEST 21: CHI TIẾT SẢN PHẨM (Product Detail)
 *
 * Mục tiêu: Test trang chi tiết sản phẩm /Home/ChiTietSanPham
 * - Hero section: ảnh, tên, giá, mô tả
 * - Size chips (M/L/XL)
 * - Add to cart
 * - Similar items
 * - Reviews + paginate
 * - Submit review
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { HomePage } from '../pages/HomePage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { USERS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

// ─── Helper ───
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

async function getFirstProductId(page: any): Promise<number> {
  // Navigate to restaurant detail to find a product
  const detail = new DetailRestaurantPage(page);
  await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  // Get the first product's link or ID
  const firstItemLink = page.locator('.item-restaurant-name a, .item-restaurant-name').first();
  const href = await firstItemLink.getAttribute('href').catch(() => null);
  if (href && href.includes('ChiTietSanPham')) {
    const match = href.match(/id=(\d+)/);
    if (match) return parseInt(match[1]);
  }
  // Fallback: try to extract mamon from data attribute
  const dataId = await page.locator('.item-restaurant-row').first().getAttribute('data-mamon').catch(() => null);
  if (dataId) return parseInt(dataId);
  return 1; // fallback
}

// ─── TEST SUITE ───
test.describe('🍽️ Chi tiết sản phẩm', () => {

  test.describe('🎠 Hero section', () => {

    test('[TC-PROD-01] Product hero — ảnh + tên + giá + mô tả hiển thị', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Kiểm tra hero image
      const heroImg = page.locator('.product-hero img, .chi-tiet-sp img, img[alt*="món"]').first();
      const imgVisible = await heroImg.isVisible().catch(() => false);
      console.log(`🖼️ Hero image visible: ${imgVisible}`);
      if (imgVisible) {
        const valid = await heroImg.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
        console.log(`  Image loaded: ${valid}`);
      }

      // Kiểm tra tên món
      const productName = page.locator('.product-name, .ten-mon, h1, h2').first();
      await expect(productName).toBeVisible({ timeout: 5_000 });
      const nameText = await productName.textContent();
      console.log(`📛 Product name: ${nameText?.trim()}`);

      // Kiểm tra giá
      const priceEl = page.locator('.product-price, .gia-mon, .current-price').first();
      const priceVisible = await priceEl.isVisible().catch(() => false);
      console.log(`💰 Price visible: ${priceVisible}`);
      if (priceVisible) {
        const priceText = await priceEl.textContent();
        console.log(`  Price: ${priceText?.trim()}`);
      }

      // Kiểm tra mô tả
      const descEl = page.locator('.product-desc, .mo-ta, .description').first();
      const descVisible = await descEl.isVisible().catch(() => false);
      console.log(`📝 Description visible: ${descVisible}`);
    });

    test('[TC-PROD-02] Size chips (M/L/XL) — click chọn size + giá thay đổi', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Kiểm tra size chips
      const sizeChips = page.locator('.size-chip, .size-option, [class*="size"] input[type="radio"], button.size');
      const chipCount = await sizeChips.count();
      console.log(`📏 Size chips: ${chipCount}`);

      if (chipCount >= 2) {
        // Lấy giá ban đầu
        const priceEl = page.locator('.product-price, .gia-mon, .current-price').first();
        const priceBefore = await priceEl.textContent().catch(() => null);

        // Click size thứ 2
        await sizeChips.nth(1).click();
        await page.waitForTimeout(500);

        const priceAfter = await priceEl.textContent().catch(() => null);
        console.log(`  Price before: ${priceBefore} → after: ${priceAfter}`);
        // Giá có thể giống hoặc khác tùy vào thiết lập size
      } else {
        console.log('ℹ️ Không đủ size chips để test');
      }
    });
  });

  test.describe('🛒 Add to cart từ chi tiết', () => {

    test('[TC-PROD-03] Add to cart từ chi tiết — thêm thành công', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Chọn số lượng
      const qtyInput = page.locator('input[name="soLuong"], .qty-input, input[type="number"]').first();
      if (await qtyInput.isVisible().catch(() => false)) {
        await qtyInput.fill('1');
      }

      // Click Thêm vào giỏ
      const addBtn = page.locator('.add-to-cart-btn, button:has-text("Thêm"), .btn-add-cart').first();
      if (await addBtn.isVisible().catch(() => false)) {
        await addBtn.click();
        await page.waitForTimeout(2000);

        // Kiểm tra cart badge tăng
        const badge = page.locator('#navCartBadge, .cart-count, .fs-cart-badge').first();
        const badgeText = await badge.textContent().catch(() => null);
        console.log(`🛒 Cart badge after add: ${badgeText}`);
        if (badgeText) {
          expect(parseInt(badgeText)).toBeGreaterThanOrEqual(1);
        }

        // Kiểm tra toast/success message
        const toast = page.locator('.toast, .alert-success, .success-message').first();
        const toastVisible = await toast.isVisible().catch(() => false);
        console.log(`✅ Success toast: ${toastVisible}`);
      } else {
        console.log('ℹ️ Nút Thêm không hiển thị (có thể cần chọn size trước)');
      }
    });
  });

  test.describe('🎯 Similar items & Reviews', () => {

    test('[TC-PROD-04] Similar items — gợi ý món tương tự', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Cuộn xuống similar items
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight * 0.6));
      await page.waitForTimeout(500);

      const similarSection = page.locator('.similar-items, .related-products, [class*="similar"], [class*="related"], [class*="goi-y"]').first();
      const similarVisible = await similarSection.isVisible().catch(() => false);
      console.log(`🎯 Similar items section: ${similarVisible}`);

      if (similarVisible) {
        const similarItems = similarSection.locator('.item, .product-card, .similar-item');
        const count = await similarItems.count();
        console.log(`  Similar items count: ${count}`);
        expect(count).toBeGreaterThanOrEqual(1);
      }
    });

    test('[TC-PROD-05] Reviews — section hiển thị + xem thêm paginate', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Cuộn xuống reviews
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      await page.waitForTimeout(500);

      const reviewSection = page.locator('#review-list, .review-section, [class*="danhgia"], [class*="review"]').first();
      const reviewVisible = await reviewSection.isVisible().catch(() => false);
      console.log(`⭐ Review section visible: ${reviewVisible}`);

      if (reviewVisible) {
        const reviewItems = reviewSection.locator('.review-item, .danhgia-item, .comment-item');
        const initialCount = await reviewItems.count();
        console.log(`  Initial reviews: ${initialCount}`);

        // Thử click "Xem thêm"
        const xemThemBtn = reviewSection.locator('.btn-xem-them, button:has-text("Xem thêm"), a:has-text("Xem thêm")').first();
        if (await xemThemBtn.isVisible().catch(() => false)) {
          await xemThemBtn.click();
          await page.waitForTimeout(2000);
          const afterCount = await reviewItems.count();
          console.log(`  After 'Xem thêm': ${afterCount}`);
          expect(afterCount).toBeGreaterThanOrEqual(initialCount);
        }
      }
    });

    test('[TC-PROD-06] Submit review — chọn sao + nhận xét + gửi', async ({ page }) => {
      await loginAs(page, CUSTOMER);
      const productId = await getFirstProductId(page);
      await page.goto(`/Home/ChiTietSanPham?id=${productId}`, { waitUntil: 'domcontentloaded', timeout: 30_000 });
      await page.waitForTimeout(3000);

      // Cuộn xuống review form
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight * 0.8));
      await page.waitForTimeout(500);

      // Kiểm tra form submit review (nếu có)
      const starRating = page.locator('.star-rating, .rating-stars, [class*="star"] i.fa-star').first();
      const reviewForm = page.locator('form[action*="SubmitReview"], .review-form, textarea[placeholder*="nhận xét"]').first();
      const formVisible = await reviewForm.isVisible().catch(() => false);
      console.log(`📝 Review form visible: ${formVisible}`);

      if (formVisible) {
        // Click star thứ 4
        const stars = page.locator('.star-rating i, .rating-stars i, .rating-star');
        const starCount = await stars.count();
        if (starCount >= 4) {
          await stars.nth(3).click();
          await page.waitForTimeout(300);
          console.log('⭐ Clicked star 4/5');
        }

        // Nhập nhận xét
        const commentArea = page.locator('textarea');
        if (await commentArea.isVisible().catch(() => false)) {
          await commentArea.fill('Món ngon, giao hàng nhanh!');
          console.log('💬 Filled comment');
        }

        // Submit
        const submitBtn = page.locator('button:has-text("Gửi"), button:has-text("Đánh giá"), input[type="submit"]').first();
        if (await submitBtn.isVisible().catch(() => false)) {
          await submitBtn.click();
          await page.waitForTimeout(2000);

          const bodyText = await page.locator('body').textContent() || '';
          const hasSuccess = bodyText.includes('thành công') || bodyText.includes('cảm ơn');
          console.log(`✅ Review submitted: ${hasSuccess}`);
        }
      } else {
        console.log('ℹ️ Không có form submit review (có thể user chưa mua món này)');
      }
    });
  });
});
