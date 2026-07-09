import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * DetailRestaurantPage — Page Object cho trang chi tiết quán ăn Fastship
 * URL: /Home/DetailRestaurant?id=<maquan>
 *
 * Chức năng: xem thực đơn, tìm món, thêm món vào giỏ hàng
 */
export class DetailRestaurantPage extends BasePage {
  // ─── Restaurant Info ───
  readonly restaurantName: Locator;
  readonly restaurantAddress: Locator;
  readonly restaurantRating: Locator;
  readonly restaurantStatus: Locator;

  // ─── Menu Items ───
  readonly menuItems: Locator;
  readonly itemName: Locator;
  readonly itemPrice: Locator;
  readonly itemDesc: Locator;

  // ─── Add to Cart ───
  readonly quantityInput: Locator;
  readonly addToCartBtn: Locator;
  readonly searchMenuInput: Locator;
  readonly searchMenuBtn: Locator;

  // ─── Category ───
  readonly categoryPills: Locator;
  readonly categoryAll: Locator;

  // ─── Reviews ───
  readonly reviewList: Locator;

  constructor(page: Page) {
    super(page);

    this.restaurantName = page.locator('.name-restaurant');
    this.restaurantAddress = page.locator('.address-restaurant');
    this.restaurantRating = page.locator('.rating');
    this.restaurantStatus = page.locator('.status-restaurant');

    // Mỗi dòng món ăn là .item-restaurant-row
    this.menuItems = page.locator('.item-restaurant-row');
    this.itemName = page.locator('.item-restaurant-name');
    this.itemPrice = page.locator('.current-price');
    this.itemDesc = page.locator('.item-restaurant-desc');

    // Form thêm vào giỏ: input số lượng + nút Thêm
    this.quantityInput = page.locator('.adding-food-cart input[name="soLuong"]');
    this.addToCartBtn = page.locator('.add-to-cart-btn');

    // Tìm kiếm trong menu
    this.searchMenuInput = page.locator('input[name="searchKey"]');
    this.searchMenuBtn = page.locator('.search-items button[type="submit"]');

    // Danh mục thực đơn (category pills)
    this.categoryPills = page.locator('.list-category .item .item-link');
    this.categoryAll = page.locator('.list-category .item .item-link').first();

    // Review section
    this.reviewList = page.locator('#review-list');
  }

  /** Mở trang chi tiết quán ăn theo mã quán */
  async gotoRestaurant(quanId: number) {
    await this.goto(`/Home/DetailRestaurant?id=${quanId}`);
    await this.waitForPageReady();
  }

  /** Lấy tên quán ăn */
  async getRestaurantName(): Promise<string | null> {
    return await this.restaurantName.textContent();
  }

  /** Đếm số món trong thực đơn */
  async getMenuItemCount(): Promise<number> {
    return await this.menuItems.count();
  }

  /** Lấy danh sách tên món ăn */
  async getItemNames(): Promise<string[]> {
    return await this.itemName.allTextContents();
  }

  /** Lấy tên món đầu tiên */
  async getFirstItemName(): Promise<string | null> {
    return await this.itemName.first().textContent();
  }

  /** Tìm kiếm món trong menu của quán */
  async searchMenu(keyword: string) {
    await this.searchMenuInput.fill(keyword);
    await this.searchMenuBtn.click();
    await this.page.waitForResponse(resp =>
      resp.url().includes('DetailRestaurant') && resp.status() === 200
    );
  }

  /**
   * Thêm món đầu tiên vào giỏ hàng
   * - Đặt số lượng (mặc định 1)
   * - Click nút Thêm
   * - Chờ response API /Cart/ApiThemMonAn
   */
  async addFirstItemToCart(quantity: number = 1) {
    // Đặt số lượng
    await this.quantityInput.first().fill(quantity.toString());
    // Click nút Thêm — dùng optimistic AJAX
    await this.addToCartBtn.first().click();
    // Chờ API response (optimistic add-to-cart via AJAX)
    await this.page.waitForResponse(resp =>
      resp.url().includes('ApiThemMonAn') && resp.status() === 200
    );
    // Chờ UI cập nhật
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Thêm món thứ index vào giỏ hàng
   */
  async addItemToCartByIndex(index: number, quantity: number = 1) {
    await this.quantityInput.nth(index).fill(quantity.toString());
    await this.addToCartBtn.nth(index).click();
    await this.page.waitForResponse(resp =>
      resp.url().includes('ApiThemMonAn') && resp.status() === 200
    );
    await this.page.waitForLoadState('networkidle');
  }

  /** Click vào category pill để lọc món */
  async clickCategory(categoryName: string) {
    const pill = this.categoryPills.filter({ hasText: categoryName }).first();
    await pill.click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Kiểm tra review section có hiển thị không */
  async isReviewSectionVisible(): Promise<boolean> {
    try {
      return await this.reviewList.isVisible({ timeout: 5_000 });
    } catch {
      return false;
    }
  }
}
