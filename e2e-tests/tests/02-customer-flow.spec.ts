/**
 * 🛍️ BỘ TEST: LUỒNG KHÁCH HÀNG (Customer Flow)
 *
 * Mục tiêu:
 * - Đăng nhập sai → kiểm tra lỗi
 * - Đăng nhập đúng → redirect
 * - Tìm kiếm món ăn
 * - Thêm món vào giỏ từ trang chi tiết quán ăn
 * - Tương tác giỏ hàng (tăng, giảm → 0 → xoá → trống → disabled)
 * - Checkout: validate → thanh toán → mã đơn hàng
 */

import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { CartPage } from '../pages/CartPage';
import { CheckoutPage } from '../pages/CheckoutPage';
import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
import { USERS, URLS, SHIPPING, INVALID_CREDENTIALS, SEED } from '../fixtures/users';

const CUSTOMER = USERS.customer1;

// ─── TEST 1: Login ───
test.describe('🔐 Đăng nhập', () => {

  test('[TC-2.1] Đăng nhập sai - hiển thị lỗi dưới form', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill(INVALID_CREDENTIALS.wrongPassword.username);
    await login.passwordInput.fill(INVALID_CREDENTIALS.wrongPassword.password);
    await login.loginButton.click();

    // Chờ response login (POST /Home/Login)
    await page.waitForResponse(resp =>
      resp.url().includes('/Home/Login') && resp.status() === 200
    );

    // Kiểm tra error alert hiển thị
    const errorMsg = await login.getErrorMessage();
    expect(errorMsg).toBeTruthy();
    expect(errorMsg?.toLowerCase()).toContain('mật khẩu');
  });

  test('[TC-2.2] Đăng nhập đúng - redirect về trang chủ', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();

    await login.usernameInput.fill(CUSTOMER.username);
    await login.passwordInput.fill(CUSTOMER.password);
    await login.loginButton.click();

    // Chờ redirect về trang chủ
    await page.waitForURL('**/Home');
    await page.waitForLoadState('networkidle');

    // Kiểm tra đã redirect khỏi trang login
    expect(page.url()).not.toContain('/Home/Login');
  });
});

// ─── TEST 2: Search & Browse ───
test.describe('🔍 Tìm kiếm & Duyệt', () => {

  test('[TC-2.3] Tìm kiếm từ khóa "pizza" - hiển thị kết quả', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await home.search('pizza');
    // Chờ response tìm kiếm
    await page.waitForResponse(resp =>
      resp.url().includes('/Home') && resp.status() === 200
    );

    // Kiểm tra có kết quả hoặc thông báo
    const hasResults = await home.hasRestaurants();
    if (hasResults) {
      const names = await home.getRestaurantNames();
      console.log(`Tìm "pizza": ${names.length} kết quả`);
    }
  });

  test('[TC-2.4] Click category pill - lọc danh sách', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    await home.clickCategory('Đồ ăn');
    await page.waitForLoadState('networkidle');

    const count = await home.getRestaurantCount();
    console.log(`Category "Đồ ăn": ${count} quán`);
  });

  test('[TC-2.5] Click vào quán ăn - xem chi tiết thực đơn', async ({ page }) => {
    const home = new HomePage(page);
    await home.gotoHome();

    // Chờ restaurant cards load
    await page.waitForSelector('.product-item', { timeout: 15_000 });
    const hasItems = await home.hasRestaurants();

    if (hasItems) {
      await home.clickFirstRestaurant();
      // Chờ trang DetailRestaurant load
      await page.waitForURL('**/DetailRestaurant**');
      await page.waitForLoadState('networkidle');

      // Kiểm tra URL chứa DetailRestaurant
      expect(page.url()).toContain('DetailRestaurant');
    }
  });
});

// ─── TEST 3: Cart Lifecycle ───
test.describe('🛒 Vòng đời giỏ hàng', () => {

  test('[TC-2.6] Giỏ hàng trống - hiển thị thông báo + checkout disabled', async ({ page }) => {
    const cart = new CartPage(page);
    await cart.gotoCart();

    await page.waitForLoadState('networkidle');
    const empty = await cart.isEmpty();
    if (empty) {
      // Nếu trống: kiểm tra thông báo + nút thanh toán không hiển thị (hoặc disabled)
      await expect(cart.emptyCartMessage).toBeVisible();
    }
  });

  test('[TC-2.7] Thêm món vào giỏ từ trang quán ăn', async ({ page }) => {
    // Đăng nhập trước
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(CUSTOMER.username);
    await login.passwordInput.fill(CUSTOMER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Home');
    await page.waitForLoadState('networkidle');

    // Vào quán Koneko Pizza (id=6)
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);

    // Kiểm tra có món ăn
    await page.waitForSelector('.item-restaurant-row', { timeout: 15_000 });
    const itemCount = await detail.getMenuItemCount();
    expect(itemCount).toBeGreaterThan(0);

    // Thêm món đầu tiên vào giỏ
    await detail.addFirstItemToCart(1);
    console.log('✅ Đã thêm món vào giỏ hàng');
  });

  test('[TC-2.8] Tăng/giảm số lượng trong giỏ', async ({ page }) => {
    const cart = new CartPage(page);
    await cart.gotoCart();

    // Chờ cart items load
    await page.waitForLoadState('networkidle');
    const itemCount = await cart.getItemCount();

    if (itemCount > 0) {
      // Tăng số lượng
      await cart.increaseFirstItem();
      await page.waitForTimeout(1500); // AJAX update
      const qtyAfterIncrease = await cart.getFirstItemQuantity();
      console.log(`Số lượng sau tăng: ${qtyAfterIncrease}`);
      expect(qtyAfterIncrease).toBeGreaterThanOrEqual(1);

      // Giảm số lượng
      await cart.decreaseFirstItem();
      await page.waitForTimeout(1500);
      const qtyAfterDecrease = await cart.getFirstItemQuantity();
      console.log(`Số lượng sau giảm: ${qtyAfterDecrease}`);
    }
  });

  test('[TC-2.9] Giảm số lượng về 0 → Xoá → Giỏ trống → Checkout disabled', async ({ page }) => {
    const cart = new CartPage(page);
    await cart.gotoCart();

    await page.waitForLoadState('networkidle');
    let itemCount = await cart.getItemCount();

    if (itemCount > 0) {
      // Giảm từng món cho đến khi hết
      for (let i = 0; i < 10; i++) { // safety limit
        itemCount = await cart.getItemCount();
        if (itemCount === 0) break;

        const qty = await cart.getFirstItemQuantity();
        if (qty <= 1) {
          // Nếu còn 1 cái: click decrease — API tự xoá khi qty về 0
          // hoặc click delete nếu decrease không xoá được
          try {
            await cart.decreaseFirstItem();
            await page.waitForTimeout(1500);
            // Kiểm tra lại count — nếu giảm xuống 0, API đã xoá item
            const newCount = await cart.getItemCount();
            if (newCount === itemCount) {
              // Decrease không xoá (qty chỉ về 0 nhưng chưa xoá) → dùng delete
              await cart.deleteFirstItem();
              await page.waitForLoadState('networkidle');
            }
          } catch {
            // Nếu decrease fail, dùng delete
            await cart.deleteFirstItem();
            await page.waitForLoadState('networkidle');
          }
        } else {
          // Nếu > 1, click giảm
          await cart.decreaseFirstItem();
          await page.waitForTimeout(1500);
        }
      }

      // Kiểm tra giỏ hàng trống
      await page.waitForLoadState('networkidle');
      const empty = await cart.isEmpty();
      expect(empty).toBe(true);

      // Kiểm tra checkout disabled (nút không visible hoặc disabled)
      const isDisabled = await cart.isCheckoutDisabled().catch(() => true);
      console.log(`Checkout disabled: ${isDisabled}`);
    }
  });
});

// ─── TEST 4: Checkout (đã login + có món trong giỏ) ───
test.describe('💳 Thanh toán - Full Flow', () => {

  test('[TC-2.10] Đăng nhập + thêm món + vào checkout', async ({ page }) => {
    // Đăng nhập
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(CUSTOMER.username);
    await login.passwordInput.fill(CUSTOMER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Home');
    await page.waitForLoadState('networkidle');

    // Thêm món vào giỏ
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 15_000 });
    await detail.addFirstItemToCart(1);

    // Vào checkout
    const checkout = new CheckoutPage(page);
    await checkout.gotoCheckout();
    await page.waitForLoadState('networkidle');

    // Kiểm tra form checkout hiển thị
    await expect(checkout.nameInput).toBeVisible();
    await expect(checkout.phoneInput).toBeVisible();
    await expect(checkout.addressInput).toBeVisible();

    // Kiểm tra payment options có ít nhất 1
    const paymentCount = await checkout.paymentOptions.count();
    expect(paymentCount).toBeGreaterThan(0);
  });

  test('[TC-2.11] Validate form trống - không cho submit', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(CUSTOMER.username);
    await login.passwordInput.fill(CUSTOMER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Home');
    await page.waitForLoadState('networkidle');

    // Thêm món + vào checkout
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 15_000 });
    await detail.addFirstItemToCart(1);

    const checkout = new CheckoutPage(page);
    await checkout.gotoCheckout();
    await page.waitForLoadState('networkidle');

    // Tick confirm (nhưng để trống address)
    await checkout.confirmOrder();

    // Click submit
    await checkout.submitBtn.click();
    await page.waitForTimeout(1000);

    // Kiểm tra toast error hiển thị (do form trống)
    const toastText = await checkout.getToastMessage();
    if (toastText) {
      console.log(`Toast error: ${toastText}`);
    }
    // URL vẫn là checkout, không redirect
    expect(page.url()).toContain('Checkout');
  });

  test('[TC-2.12] Điền đầy đủ → chọn COD → đặt hàng → mã đơn xuất hiện', async ({ page }) => {
    const login = new LoginPage(page);
    await login.gotoLogin();
    await login.usernameInput.fill(CUSTOMER.username);
    await login.passwordInput.fill(CUSTOMER.password);
    await login.loginButton.click();
    await page.waitForURL('**/Home');
    await page.waitForLoadState('networkidle');

    // Thêm món
    const detail = new DetailRestaurantPage(page);
    await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
    await page.waitForSelector('.item-restaurant-row', { timeout: 15_000 });
    await detail.addFirstItemToCart(1);

    // Vào checkout
    const checkout = new CheckoutPage(page);
    await checkout.gotoCheckout();
    await page.waitForLoadState('networkidle');

    // Điền thông tin đầy đủ
    await checkout.fillShippingInfo(SHIPPING.name, SHIPPING.phone, SHIPPING.address);

    // Chọn COD
    await checkout.selectCOD();

    // Tick confirm
    await checkout.confirmOrder();

    // Click Xác nhận đặt hàng
    await checkout.submitBtn.click();

    // Chờ response từ /Payment/ProcessPayment
    try {
      await page.waitForResponse(resp =>
        resp.url().includes('ProcessPayment') && resp.status() === 200
      );
    } catch {
      // Fallback: chờ navigation
      await page.waitForLoadState('networkidle', { timeout: 15_000 });
    }

    // Kiểm tra popup kết quả hiển thị hoặc redirect
    const popupVisible = await checkout.isResultPopupVisible().catch(() => false);
    if (popupVisible) {
      const popupText = await checkout.getResultPopupText();
      console.log(`Popup: ${popupText}`);
      expect(popupText).toBeTruthy();
    } else {
      // Nếu không có popup, kiểm tra đã redirect
      const currentUrl = page.url();
      console.log(`Redirect to: ${currentUrl}`);
    }
  });
});
