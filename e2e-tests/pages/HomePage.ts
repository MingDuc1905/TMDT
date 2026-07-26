import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * HomePage — Page Object cho trang chủ Fastship
 * URL: /
 * Gồm: Hero carousel, danh sách quán ăn, category pills, thanh tìm kiếm
 */
export class HomePage extends BasePage {
  // ─── Navbar ───
  readonly logo: Locator;
  readonly searchInput: Locator;
  readonly searchButton: Locator;
  readonly cartButton: Locator;
  readonly loginNavBtn: Locator;
  readonly registerNavBtn: Locator;
  readonly userDropdown: Locator;
  readonly logoutLink: Locator;

  // ─── Hero Carousel ───
  readonly carousel: Locator;
  readonly carouselPrevBtn: Locator;
  readonly carouselNextBtn: Locator;

  // ─── Category Pills (replaced by FilterBar chips) ───
  readonly categoryAll: Locator;
  readonly categoryRow: Locator;
  readonly filterChips: Locator;

  // ─── Restaurant Grid ───
  readonly restaurantCards: Locator;
  readonly emptyState: Locator;
  readonly emptyStateMessage: Locator;

  // ─── Promo Band ───
  readonly promoBand: Locator;
  readonly promoDismissBtn: Locator;

  // ─── Stats Row ───
  readonly statsRow: Locator;

  constructor(page: Page) {
    super(page);

    // ponytail: .first() — có thể có 2 elements do responsive layout
    this.logo = page.locator('.fs-logo').first();
    // ponytail: .first() — navbar có 2 search inputs (desktop + mobile)
    this.searchInput = page.locator('input[name="txtSearch"]').first();
    // ponytail: match cả desktop (aria-label) và mobile (icon-only button trong form mobile)
    this.searchButton = page.locator('button[aria-label="Tìm kiếm"], .fs-search-form-mobile button[type="submit"]').first();
    // ponytail: .first() — responsive layout có 2 cart buttons (desktop + mobile)
    this.cartButton = page.locator('.fs-cart-btn').first();
    this.loginNavBtn = page.locator('a[href*="/Home/Login"]').first();
    this.registerNavBtn = page.locator('a[href*="/Home/Signup"]').first();
    this.userDropdown = page.locator('.dropdown-toggle .fs-avatar-xs');
    this.logoutLink = page.locator('a[href*="/Home/Logout"]');

    this.carousel = page.locator('#header-carousel');
    this.carouselPrevBtn = page.locator('.carousel-control-prev');
    this.carouselNextBtn = page.locator('.carousel-control-next');

    this.categoryAll = page.locator('.fs-chip').first();
    this.categoryRow = page.locator('#filterChips');
    this.filterChips = page.locator('#filterChips');

    this.restaurantCards = page.locator('.product-item');
    this.emptyState = page.locator('.col-12.text-center.py-5');
    this.emptyStateMessage = page.locator('h5:has-text("Không tìm thấy")');

    this.promoBand = page.locator('#promoBand');
    this.promoDismissBtn = page.locator('#promoDismissBtn');
    this.statsRow = page.locator('.fs-stats-row');
  }

  /** Load trang chủ */
  async gotoHome() {
    await this.goto('/');
  }

  /** Tìm kiếm quán ăn hoặc món ăn */
  async search(keyword: string) {
    // ponytail: navigate trực tiếp thay vì click form — tránh mobile visibility issues
    await this.page.goto(`/?txtSearch=${encodeURIComponent(keyword)}`, { waitUntil: 'domcontentloaded' });
    await this.page.waitForLoadState('networkidle');
  }

  /** Click vào một filter chip */
  async clickCategory(categoryName: string) {
    const category = this.filterChips.locator(`button.fs-chip:has-text("${categoryName}")`);
    await category.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Lấy danh sách tên các quán ăn đang hiển thị */
  async getRestaurantNames(): Promise<string[]> {
    return await this.restaurantCards.locator('.product-title').allTextContents();
  }

  /** Click vào quán ăn đầu tiên trong danh sách */
  async clickFirstRestaurant() {
    await this.restaurantCards.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Click vào quán ăn theo tên */
  async clickRestaurantByName(name: string) {
    const card = this.restaurantCards.locator(`.product-title:has-text("${name}")`).first();
    await card.click();
    await this.page.waitForLoadState('networkidle');
  }

  /** Kiểm tra có hiển thị quán ăn không */
  async hasRestaurants(): Promise<boolean> {
    const count = await this.restaurantCards.count();
    return count > 0;
  }

  /** Đếm số quán ăn hiển thị */
  async getRestaurantCount(): Promise<number> {
    return await this.restaurantCards.count();
  }

  /** Dismiss promo band */
  async dismissPromo() {
    try {
      await this.promoDismissBtn.click({ timeout: 3_000 });
      await this.page.waitForTimeout(500);
    } catch {
      // Promo band có thể đã bị dismiss trước đó
    }
  }

  /** Kiểm tra navbar hiển thị đúng */
  async isNavbarVisible(): Promise<boolean> {
    return await this.navbar.isVisible();
  }

  /** Lấy text từ stat item */
  async getStatValue(index: number): Promise<string | null> {
    const stat = this.statsRow.locator('.fs-stat-item').nth(index);
    return await stat.locator('.stat-num').textContent();
  }
}
