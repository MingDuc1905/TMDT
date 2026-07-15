import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * AdminPage — Page Object cho dashboard Admin Fastship
 * URL: /Admin/*
 *
 * Kiểm tra: Dashboard KPI, charts, quản lý user, quản lý đơn hàng
 */
export class AdminPage extends BasePage {
  readonly sidebar: Locator;
  readonly dashboardLink: Locator;
  readonly userManagementLink: Locator;
  readonly orderManagementLink: Locator;
  readonly categoryManagementLink: Locator;
  readonly kpiCards: Locator;
  readonly revenueChart: Locator;
  readonly orderTable: Locator;

  constructor(page: Page) {
    super(page);

    this.sidebar = page.locator('.deznav');
    this.dashboardLink = page.locator('a[href*="/Admin/Dashboard"], a[href*="/Admin/Index"]');
    this.userManagementLink = page.locator('a[href*="/Admin/QuanLyKhachHang"]');
    this.orderManagementLink = page.locator('a[href*="/Admin/Order"]');
    this.categoryManagementLink = page.locator('a[href*="/Admin/Category"]');
    this.kpiCards = page.locator('.stat-card');
    this.revenueChart = page.locator('canvas');
    this.orderTable = page.locator('.table-responsive table');
  }

  /** Mở dashboard admin */
  async gotoDashboard() {
    await this.goto('/Admin');
  }

  /** Mở trang quản lý khách hàng */
  async gotoUserManagement() {
    await this.goto('/Admin/QuanLyKhachHang');
  }

  /** Mở trang quản lý đơn hàng */
  async gotoOrderManagement() {
    await this.goto('/Admin/Order');
  }

  /** Mở trang quản lý danh mục */
  async gotoCategoryManagement() {
    await this.goto('/Admin/Category');
  }

  /** Đếm số KPI cards trên dashboard */
  async getKpiCount(): Promise<number> {
    return await this.kpiCards.count();
  }

  /** Kiểm tra biểu đồ có render không */
  async isChartVisible(): Promise<boolean> {
    try {
      return await this.revenueChart.first().isVisible({ timeout: 5_000 });
    } catch {
      return false;
    }
  }

  /** Tìm kiếm user trong bảng quản lý */
  async searchUser(keyword: string) {
    const searchInput = this.page.locator('#searchInput').first();
    await searchInput.fill(keyword);
    await this.page.waitForTimeout(1000);
  }
}
