import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * RestaurantPage — Page Object cho dashboard Quán ăn Fastship
 * URL: /Restaurant/*
 *
 * Kiểm tra: danh sách đơn hàng, xác nhận/hủy đơn, chuyển trạng thái
 */
export class RestaurantPage extends BasePage {
  readonly sidebar: Locator;
  readonly orderListLink: Locator;
  readonly orderTable: Locator;
  readonly orderRows: Locator;
  readonly acceptOrderBtn: Locator;
  readonly cancelOrderBtn: Locator;
  readonly readyBtn: Locator;
  readonly detailBtn: Locator;
  readonly kpiCards: Locator;

  constructor(page: Page) {
    super(page);
    this.sidebar = page.locator('.deznav');
    this.orderListLink = page.locator('a[href*="/Restaurant/OrderList"]');
    this.orderTable = page.locator('#example5');
    this.orderRows = page.locator('#example5 tbody tr');
    this.acceptOrderBtn = page.locator('a[href*="/Restaurant/nhandon/"]');
    this.cancelOrderBtn = page.locator('a[href*="/Restaurant/huydon/"]');
    this.readyBtn = page.locator('a[href*="/Restaurant/hoantatdon/"]');
    this.detailBtn = page.locator('a[href*="/Cart/ChiTietDonHang"]');
    this.kpiCards = page.locator('.card-header');
  }

  /** Mở trang dashboard quán ăn */
  async gotoDashboard() {
    await this.goto('/Restaurant');
  }

  /** Mở danh sách đơn hàng */
  async gotoOrderList() {
    await this.goto('/Restaurant/OrderList');
  }

  /** Lấy số dòng trong bảng đơn hàng */
  async getOrderCount(): Promise<number> {
    return await this.orderRows.count();
  }

  /** Lấy text cột trạng thái của đơn đầu tiên */
  async getFirstOrderStatus(): Promise<string | null> {
    return await this.orderRows.first().locator('td').nth(3).textContent();
  }

  /** Lấy mã đơn hàng đầu tiên */
  async getFirstOrderId(): Promise<string> {
    const link = this.orderRows.first().locator('a[href*="ChiTietDonHang"]');
    const href = await link.getAttribute('href');
    return href?.match(/id=(\d+)/)?.[1] || '';
  }

  /** Click nút Nhận đơn cho đơn đầu tiên (trạng thái: Đã đặt) */
  async acceptFirstOrder() {
    await this.acceptOrderBtn.first().click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /** Click nút Chuẩn bị xong (trạng thái: Đã xác nhận) */
  async markAsReady() {
    await this.readyBtn.first().click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /** Kiểm tra nút Nhận đơn còn hiển thị không */
  async isAcceptBtnVisible(): Promise<boolean> {
    try {
      return await this.acceptOrderBtn.first().isVisible({ timeout: 3_000 });
    } catch {
      return false;
    }
  }
}
