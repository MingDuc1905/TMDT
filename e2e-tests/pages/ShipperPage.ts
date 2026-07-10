import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * ShipperPage — Page Object cho dashboard Shipper Fastship
 * URL: /Shipper/*
 *
 * Kiểm tra: FREE-PICK, danh sách đơn, nhận đơn, cập nhật trạng thái, ví tiền
 */
export class ShipperPage extends BasePage {
  readonly freepickTab: Locator;
  readonly orderTab: Locator;
  readonly orderTable: Locator;
  readonly orderRows: Locator;
  readonly acceptOrderBtn: Locator;
  readonly detailLink: Locator;
  readonly walletLink: Locator;
  readonly incomeLink: Locator;
  readonly historyLink: Locator;
  readonly refreshBtn: Locator;
  readonly liveMap: Locator;

  constructor(page: Page) {
    super(page);

    // ponytail: thử nhiều selector cho tab FREE-PICK (các version HTML khác nhau)
    this.freepickTab = page.locator('#orders-all-tab, [data-tab="freepick"], a:has-text("FREE-PICK"), a:has-text("Chờ nhận")');
    this.orderTab = page.locator('#orders-paid-tab, [data-tab="orders"], a:has-text("ĐƠN HÀNG"), a:has-text("Đã nhận")');
    this.orderTable = page.locator('.table-responsive table');
    this.orderRows = page.locator('.table-responsive tbody tr');
    this.acceptOrderBtn = page.locator('a[href*="/Shipper/OrderDetail/"]');
    this.detailLink = page.locator('a[href*="/Shipper/OrderDetail/"]');
    this.walletLink = page.locator('a[href*="/Shipper/ViTien"]');
    this.incomeLink = page.locator('a[href*="/Shipper/ThuNhap"]');
    this.historyLink = page.locator('a[href*="/Shipper/LichSu"]');
    this.refreshBtn = page.locator('a:has-text("Làm mới")');
    this.liveMap = page.locator('#shipper-map');
  }

  /** Mở dashboard shipper */
  async gotoDashboard() {
    await this.goto('/Shipper');
  }

  /** Mở tab FREE-PICK — ponytail: nếu tab không tồn tại (different HTML), log + skip */
  async openFreepickTab() {
    try {
      const count = await this.freepickTab.count();
      if (count === 0) {
        console.log('ℹ️ Tab FREE-PICK không tồn tại (selector không match)');
        return;
      }
      await this.freepickTab.click({ timeout: 5_000 });
      await this.page.waitForTimeout(1000);
    } catch {
      console.log('ℹ️ Tab FREE-PICK không clickable, skip');
    }
  }

  /** Mở tab ĐƠN HÀNG */
  async openOrderTab() {
    try {
      const count = await this.orderTab.count();
      if (count === 0) {
        console.log('ℹ️ Tab ĐƠN HÀNG không tồn tại (selector không match)');
        return;
      }
      await this.orderTab.click({ timeout: 5_000 });
      await this.page.waitForTimeout(1000);
    } catch {
      console.log('ℹ️ Tab ĐƠN HÀNG không clickable, skip');
    }
  }

  /** Mở trang ví tiền */
  async gotoWallet() {
    try {
      const count = await this.walletLink.count();
      if (count === 0) {
        console.log('ℹ️ Wallet link không tồn tại, goto direct /Shipper/ViTien');
        await this.goto('/Shipper/ViTien');
      } else {
        await this.walletLink.click({ timeout: 5_000 });
      }
      await this.page.waitForLoadState('networkidle');
    } catch {
      console.log('ℹ️ Wallet link click timeout, goto direct');
      await this.goto('/Shipper/ViTien');
    }
  }

  /** Mở trang thu nhập */
  async gotoIncome() {
    await this.goto('/Shipper/ThuNhap');
  }

  /** Mở trang lịch sử */
  async gotoHistory() {
    await this.goto('/Shipper/LichSu');
  }

  /** Click nút Chấp nhận đơn đầu tiên */
  async acceptFirstOrder() {
    await this.detailLink.first().click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(2000);
  }

  /** Lấy text trạng thái của đơn đầu tiên */
  async getFirstOrderStatus(): Promise<string | null> {
    return await this.orderRows.first().locator('td').nth(6).textContent();
  }

  /** Lấy số dư ví */
  async getWalletBalance(): Promise<string | null> {
    try {
      return await this.page.locator('.vi-tien-balance, [class*="balance"]').first().textContent();
    } catch {
      return null;
    }
  }

  /** Đếm số đơn trong bảng */
  async getOrderCount(): Promise<number> {
    return await this.orderRows.count();
  }

  /** Kiểm tra map có hiển thị không */
  async isMapVisible(): Promise<boolean> {
    try {
      return await this.liveMap.isVisible({ timeout: 3_000 });
    } catch {
      return false;
    }
  }
}
