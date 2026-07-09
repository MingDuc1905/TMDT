import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * CartPage — Page Object cho trang giỏ hàng Fastship
 * URL: /Cart
 * 
 * Kiểm tra: danh sách món, nút tăng/giảm, xoá, tổng tiền
 */
export class CartPage extends BasePage {
  readonly cartContainer: Locator;
  readonly cartItems: Locator;
  readonly emptyCartMessage: Locator;
  readonly checkoutButton: Locator;
  readonly continueShoppingLink: Locator;
  readonly cartTotal: Locator;
  readonly cartGrandTotal: Locator;

  constructor(page: Page) {
    super(page);

    this.cartContainer = page.locator('.cart-wrapper');
    this.cartItems = page.locator('.cart-item');
    this.emptyCartMessage = page.locator('h4:has-text("trống")');
    this.checkoutButton = page.locator('.btn-checkout-go');
    this.continueShoppingLink = page.locator('a:has-text("tiếp tục mua sắm")');
    this.cartTotal = page.locator('#cart-total');
    this.cartGrandTotal = page.locator('#cart-grandtotal');
  }

  /** Mở trang giỏ hàng */
  async gotoCart() {
    await this.goto('/Cart');
  }

  /** Đếm số món trong giỏ hàng */
  async getItemCount(): Promise<number> {
    return await this.cartItems.count();
  }

  /** Click nút tăng số lượng cho món đầu tiên */
  async increaseFirstItem() {
    const increaseBtn = this.cartItems.first().locator('.btn-tang');
    await increaseBtn.click();
    await this.page.waitForTimeout(1000);
  }

  /** Click nút giảm số lượng cho món đầu tiên */
  async decreaseFirstItem() {
    const decreaseBtn = this.cartItems.first().locator('.btn-giam');
    await decreaseBtn.click();
    await this.page.waitForTimeout(1000);
  }

  /** Xoá món đầu tiên khỏi giỏ hàng */
  async deleteFirstItem() {
    const deleteBtn = this.cartItems.first().locator('.delete-btn');
    await deleteBtn.click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Lấy số lượng hiện tại của món đầu tiên */
  async getFirstItemQuantity(): Promise<number> {
    const qtyText = await this.cartItems.first().locator('.qty-num').textContent();
    return parseInt(qtyText || '0', 10);
  }

  /** Lấy tổng tiền giỏ hàng (text) */
  async getTotalText(): Promise<string | null> {
    return await this.cartTotal.textContent();
  }

  /** Kiểm tra giỏ hàng có trống không */
  async isEmpty(): Promise<boolean> {
    try {
      await this.emptyCartMessage.waitFor({ state: 'visible', timeout: 5_000 });
      return true;
    } catch {
      return false;
    }
  }

  /** Click nút thanh toán */
  async clickCheckout() {
    await this.checkoutButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Kiểm tra nút thanh toán có bị disabled không */
  async isCheckoutDisabled(): Promise<boolean> {
    return await this.checkoutButton.isDisabled();
  }

  /** Lấy tên các món trong giỏ */
  async getItemNames(): Promise<string[]> {
    return await this.cartItems.locator('.item-name').allTextContents();
  }
}
