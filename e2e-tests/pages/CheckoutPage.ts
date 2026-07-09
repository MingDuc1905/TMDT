import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * CheckoutPage — Page Object cho trang thanh toán Fastship
 * URL: /Cart/Checkout
 * 
 * Kiểm tra: địa chỉ, phương thức thanh toán, coupon, xác nhận đặt hàng
 */
export class CheckoutPage extends BasePage {
  // ─── Address ───
  readonly newAddressTab: Locator;
  readonly savedAddressTab: Locator;
  readonly locationTab: Locator;
  readonly nameInput: Locator;
  readonly phoneInput: Locator;
  readonly addressInput: Locator;
  readonly districtSelect: Locator;

  // ─── Payment ───
  readonly paymentOptions: Locator;
  readonly codOption: Locator;
  readonly transferOption: Locator;

  // ─── Coupon ───
  readonly couponInput: Locator;
  readonly applyCouponBtn: Locator;
  readonly couponResult: Locator;

  // ─── Confirmation ───
  readonly confirmCheckbox: Locator;
  readonly submitBtn: Locator;

  // ─── Order Summary ───
  readonly orderSummary: Locator;
  readonly orderTotal: Locator;
  readonly orderDiscount: Locator;

  // ─── Result Popup ───
  readonly resultPopup: Locator;

  constructor(page: Page) {
    super(page);

    this.newAddressTab = page.locator('#tab-new');
    this.savedAddressTab = page.locator('#tab-available');
    this.locationTab = page.locator('#tab-current');
    this.nameInput = page.locator('#input-hoten');
    this.phoneInput = page.locator('#input-sdt');
    this.addressInput = page.locator('#input-diachi');
    this.districtSelect = page.locator('select[name="quan"]');

    this.paymentOptions = page.locator('.payment-option');
    this.codOption = page.locator('.payment-option').first();
    this.transferOption = page.locator('.payment-option').nth(1);

    this.couponInput = page.locator('#coupon-code');
    this.applyCouponBtn = page.locator('#btn-apply-coupon');
    this.couponResult = page.locator('#coupon-result');

    this.confirmCheckbox = page.locator('#diff-acc');
    this.submitBtn = page.locator('#btn-submit-cod');

    this.orderSummary = page.locator('.order-summary');
    this.orderTotal = page.locator('#order-total');
    this.orderDiscount = page.locator('#order-discount');

    this.resultPopup = page.locator('#payment-popup-overlay');
  }

  /** Mở trang checkout */
  async gotoCheckout() {
    await this.goto('/Cart/Checkout');
  }

  /** Điền thông tin giao hàng */
  async fillShippingInfo(name: string, phone: string, address: string) {
    await this.nameInput.fill(name);
    await this.phoneInput.fill(phone);
    await this.addressInput.fill(address);
    // Chọn quận mặc định (quận đầu tiên)
    await this.districtSelect.selectOption({ index: 1 });
  }

  /** Chọn phương thức thanh toán theo index */
  async selectPaymentMethod(index: number = 0) {
    await this.paymentOptions.nth(index).click();
    await this.page.waitForTimeout(500);
  }

  /** Chọn COD (tiền mặt) */
  async selectCOD() {
    await this.codOption.click();
    await this.page.waitForTimeout(500);
  }

  /** Nhập mã giảm giá */
  async applyCoupon(code: string) {
    await this.couponInput.fill(code);
    await this.applyCouponBtn.click();
    await this.page.waitForTimeout(1000);
  }

  /** Lấy kết quả áp dụng coupon */
  async getCouponResult(): Promise<string | null> {
    try {
      await this.couponResult.waitFor({ state: 'visible', timeout: 5_000 });
      return await this.couponResult.textContent();
    } catch {
      return null;
    }
  }

  /** Tick xác nhận đơn hàng */
  async confirmOrder() {
    await this.confirmCheckbox.check();
    await this.page.waitForTimeout(200);
  }

  /** Click nút xác nhận đặt hàng */
  async submitOrder() {
    await this.submitBtn.click();
    await this.page.waitForTimeout(3000);
  }

  /** Kiểm tra popup kết quả hiển thị */
  async isResultPopupVisible(): Promise<boolean> {
    try {
      await this.resultPopup.waitFor({ state: 'visible', timeout: 10_000 });
      return true;
    } catch {
      return false;
    }
  }

  /** Lấy text từ popup kết quả */
  async getResultPopupText(): Promise<string | null> {
    try {
      return await this.resultPopup.locator('.popup-box').textContent();
    } catch {
      return null;
    }
  }

  /** Kiểm tra tổng đơn hàng */
  async getOrderTotalText(): Promise<string | null> {
    return await this.orderTotal.textContent();
  }

  /** Kiểm tra nút submit có bị disabled không */
  async isSubmitDisabled(): Promise<boolean> {
    return await this.submitBtn.isDisabled();
  }
}
