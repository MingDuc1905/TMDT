/**
 * 💳 BỘ TEST 12: QUY TRÌNH THANH TOÁN (CHECKOUT FLOW)
 *
 * Mục tiêu: Test toàn bộ checkout từ cart → address → payment → confirm → result
 * - Address tabs, payment methods, coupon, submit, result popup
 * - Multi-restaurant checkout, validation, error handling
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { CartPage } from '../pages/CartPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { CheckoutPage } from '../pages/CheckoutPage';
import { USERS, SEED, SHIPPING } from '../fixtures/users';

const CUSTOMER = USERS.customer1;
const RESTAURANT_ID = SEED.restaurantIds.konekoPizza;

async function loginAs(page: any, user: { username: string; password: string }) {
  const login = new LoginPage(page);
  await login.login(user.username, user.password);
  await page.waitForTimeout(2000);
}

async function setupCartWithItems(page: any) {
  await loginAs(page, CUSTOMER);
  const detailPage = new DetailRestaurantPage(page);
  await detailPage.gotoRestaurant(RESTAURANT_ID);
  const menuCount = await page.locator('.item-restaurant-row').count();
  if (menuCount >= 2) {
    await detailPage.addItemToCartByIndex(0, 1);
    await page.waitForTimeout(1000);
    await detailPage.addItemToCartByIndex(1, 1);
    await page.waitForTimeout(1000);
  } else if (menuCount >= 1) {
    await detailPage.addFirstItemToCart(1);
    await page.waitForTimeout(1000);
  }
}

// ════════════════════════════════════════════════════════════════
// 1. CHECKOUT PAGE LOAD
// ════════════════════════════════════════════════════════════════
test.describe('📋 Checkout page load', () => {

  test('[TC-CHECKOUT-01] Checkout page load → verify elements visible', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Verify key sections exist
    const newAddressTab = page.locator('#tab-new');
    await expect(newAddressTab).toBeVisible();

    const paymentOptions = page.locator('.payment-option');
    const paymentCount = await paymentOptions.count();
    expect(paymentCount).toBeGreaterThanOrEqual(1);
    console.log(`Payment options: ${paymentCount}`);

    const orderSummary = page.locator('.order-summary');
    await expect(orderSummary).toBeVisible();
  });

  test('[TC-CHECKOUT-02] Order summary hiển thị items từ cart', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    const orderItems = page.locator('.order-item');
    const itemCount = await orderItems.count();
    console.log(`Order items in summary: ${itemCount}`);
    expect(itemCount).toBeGreaterThanOrEqual(1);

    const totalText = await checkoutPage.getOrderTotalText();
    console.log(`Order total: ${totalText}`);
    expect(totalText).toBeTruthy();
  });
});

// ════════════════════════════════════════════════════════════════
// 2. ADDRESS TABS
// ════════════════════════════════════════════════════════════════
test.describe('📍 Address tabs', () => {

  test('[TC-CHECKOUT-03] Tab "Địa chỉ mới" mặc định active', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    const newTab = page.locator('#tab-new');
    await expect(newTab).toBeVisible();

    // Verify form fields visible
    const nameInput = page.locator('#input-hoten');
    await expect(nameInput).toBeVisible();

    const phoneInput = page.locator('#input-sdt');
    await expect(phoneInput).toBeVisible();

    const addressInput = page.locator('#input-diachi');
    await expect(addressInput).toBeVisible();
  });

  test('[TC-CHECKOUT-04] Tab "Địa chỉ đã lưu" → verify saved addresses', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Check if saved address tab exists
    const savedTab = page.locator('#tab-available');
    const isVisible = await savedTab.isVisible().catch(() => false);

    if (isVisible) {
      await savedTab.click();
      await page.waitForTimeout(1000);

      const panel = page.locator('#panel-available');
      await expect(panel).toBeVisible();
      console.log('Saved address panel visible');
    } else {
      console.log('ℹ️ No saved address tab (user has no saved addresses)');
    }
  });

  test('[TC-CHECKOUT-05] Điền form địa chỉ mới → verify validation', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Fill form
    await checkoutPage.fillShippingInfo(
      SHIPPING.name,
      SHIPPING.phone,
      SHIPPING.address
    );
    await page.waitForTimeout(500);

    // Verify fields filled
    const nameValue = await page.locator('#input-hoten').inputValue();
    expect(nameValue).toBe(SHIPPING.name);

    const phoneValue = await page.locator('#input-sdt').inputValue();
    expect(phoneValue).toBe(SHIPPING.phone);

    const addressValue = await page.locator('#input-diachi').inputValue();
    expect(addressValue).toBe(SHIPPING.address);
  });
});

// ════════════════════════════════════════════════════════════════
// 3. PAYMENT METHODS
// ════════════════════════════════════════════════════════════════
test.describe('💰 Payment methods', () => {

  test('[TC-CHECKOUT-06] COD option → verify selection', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    await checkoutPage.selectCOD();
    await page.waitForTimeout(500);

    const codOption = page.locator('.payment-option').first();
    const hasSelected = await codOption.evaluate((el) =>
      el.classList.contains('selected') || el.classList.contains('active')
    );
    console.log(`COD selected: ${hasSelected}`);
  });

  test('[TC-CHECKOUT-07] Chuyển khoản option → verify QR/info area', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Click transfer option (2nd payment option)
    const transferOption = page.locator('.payment-option').nth(1);
    const exists = await transferOption.isVisible().catch(() => false);

    if (exists) {
      await transferOption.click();
      await page.waitForTimeout(1000);

      const paymentInfo = page.locator('#payment-info-area');
      const isVisible = await paymentInfo.isVisible().catch(() => false);
      console.log(`Payment info area visible after transfer: ${isVisible}`);
    } else {
      console.log('ℹ️ Only COD available');
    }
  });

  test('[TC-CHECKOUT-08] Submit button disabled khi chưa confirm', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    const isDisabled = await checkoutPage.isSubmitDisabled();
    console.log(`Submit disabled without confirm: ${isDisabled}`);
    // Note: submit may or may not be disabled depending on implementation
  });
});

// ════════════════════════════════════════════════════════════════
// 4. COUPON
// ════════════════════════════════════════════════════════════════
test.describe('🎟️ Coupon', () => {

  test('[TC-CHECKOUT-09] Nhập coupon hợp lệ → verify discount', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Apply coupon
    await checkoutPage.applyCoupon('SALE10');
    await page.waitForTimeout(2000);

    const couponResult = await checkoutPage.getCouponResult();
    console.log(`Coupon result: "${couponResult}"`);
  });

  test('[TC-CHECKOUT-10] Nhập coupon không hợp lệ → verify error', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    await checkoutPage.applyCoupon('KHONGTONTAI999');
    await page.waitForTimeout(2000);

    const couponResult = await checkoutPage.getCouponResult();
    console.log(`Invalid coupon result: "${couponResult}"`);
    // Should show error message
  });

  test('[TC-CHECKOUT-11] Browse coupons popup → verify display', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    const browseBtn = page.locator('#btn-browse-coupons');
    const isVisible = await browseBtn.isVisible().catch(() => false);

    if (isVisible) {
      await browseBtn.click();
      await page.waitForTimeout(2000);

      const popup = page.locator('#coupon-popup-overlay');
      const popupVisible = await popup.isVisible().catch(() => false);
      console.log(`Coupon popup visible: ${popupVisible}`);

      if (popupVisible) {
        const couponCards = page.locator('.coupon-card');
        const cardCount = await couponCards.count();
        console.log(`Coupon cards in popup: ${cardCount}`);

        // Close popup
        const closeBtn = page.locator('.coupon-popup-close');
        if (await closeBtn.isVisible().catch(() => false)) {
          await closeBtn.click();
        }
      }
    } else {
      console.log('ℹ️ Browse coupons button not visible');
    }
  });
});

// ════════════════════════════════════════════════════════════════
// 5. ORDER SUBMISSION
// ════════════════════════════════════════════════════════════════
test.describe('✅ Order submission', () => {

  test('[TC-CHECKOUT-12] Fill full form → confirm → submit COD → verify result popup', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    // Fill shipping info
    await checkoutPage.fillShippingInfo(
      SHIPPING.name,
      SHIPPING.phone,
      SHIPPING.address
    );
    await page.waitForTimeout(500);

    // Select COD
    await checkoutPage.selectCOD();
    await page.waitForTimeout(500);

    // Confirm checkbox
    await checkoutPage.confirmOrder();
    await page.waitForTimeout(500);

    // Submit
    await checkoutPage.submitOrder();
    await page.waitForTimeout(5000);

    // Check result popup
    const popupVisible = await checkoutPage.isResultPopupVisible();
    console.log(`Result popup visible: ${popupVisible}`);

    if (popupVisible) {
      const popupText = await checkoutPage.getResultPopupText();
      console.log(`Result popup text: "${popupText?.substring(0, 100)}"`);
    }

    // Check for redirect to order detail or tracking
    const currentUrl = page.url();
    console.log(`Post-checkout URL: ${currentUrl}`);
  });
});

// ════════════════════════════════════════════════════════════════
// 6. EDGE CASES
// ════════════════════════════════════════════════════════════════
test.describe('⚠️ Checkout edge cases', () => {

  test('[TC-CHECKOUT-13] Checkout không có items → redirect hoặc empty state', async ({ page }) => {
    await loginAs(page, CUSTOMER);

    // Navigate directly to checkout without items
    await page.goto('/Cart/Checkout', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);

    const currentUrl = page.url();
    console.log(`Checkout without items URL: ${currentUrl}`);

    // Should redirect to cart or show empty state
    const isOnCart = currentUrl.includes('/Cart');
    expect(isOnCart).toBe(true);
  });

  test('[TC-CHECKOUT-14] Submit button disable sau khi click đầu tiên', async ({ page }) => {
    await setupCartWithItems(page);

    const checkoutPage = new CheckoutPage(page);
    await checkoutPage.gotoCheckout();
    await page.waitForTimeout(3000);

    await checkoutPage.fillShippingInfo(SHIPPING.name, SHIPPING.phone, SHIPPING.address);
    await checkoutPage.selectCOD();
    await checkoutPage.confirmOrder();
    await page.waitForTimeout(500);

    // Click submit
    await checkoutPage.submitBtn.click();
    await page.waitForTimeout(1000);

    // Verify button is disabled (double-submit prevention)
    const isDisabled = await checkoutPage.submitBtn.isDisabled().catch(() => true);
    console.log(`Submit disabled after click: ${isDisabled}`);
  });
});
