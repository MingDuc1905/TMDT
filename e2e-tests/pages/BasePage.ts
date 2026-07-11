import { Page, Locator, expect } from '@playwright/test';

/**
 * BasePage — Lớp cơ sở cho tất cả Page Objects
 * Chứa các method dùng chung: navigation, wait, screenshot, toast, ...
 */
export class BasePage {
  readonly page: Page;

  // Common elements xuất hiện trên hầu hết các trang
  readonly navbar: Locator;
  readonly footer: Locator;
  readonly loadingSkeleton: Locator;
  readonly backToTopBtn: Locator;

  constructor(page: Page) {
    this.page = page;

    this.navbar = page.locator('.fs-header');
    this.footer = page.locator('.fs-footer');
    this.loadingSkeleton = page.locator('#fs-loading-skeleton');
    this.backToTopBtn = page.locator('.back-to-top');
  }

  // ─── Navigation ───

  /** Điều hướng đến URL tương đối */
  async goto(path: string = '/') {
    // ponytail: dùng domcontentloaded thay networkidle — Render free tier rất chậm
    // nếu dùng networkidle sẽ timeout vì Render giữ kết nối lâu
    try {
      await this.page.goto(path, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    } catch {
      // Retry 1 lần nếu timeout (Render cold-start)
      console.log('⚠️ goto timeout, retrying...');
      await this.page.goto(path, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    }
    // Chờ skeleton loading biến mất (nếu có)
    await this.waitForSkeletonHidden();
    // Chờ thêm để network idle (không bắt buộc)
    try { await this.page.waitForLoadState('networkidle', { timeout: 30_000 }); } catch { }
  }

  /** Chờ skeleton loading ẩn đi */
  async waitForSkeletonHidden() {
    try {
      await this.loadingSkeleton.waitFor({ state: 'hidden', timeout: 15_000 });
    } catch {
      // Nếu không có skeleton (ví dụ trang dashboard), bỏ qua
    }
  }

  /** Chờ trang load hoàn tất — network idle + DOM ready */
  async waitForPageReady() {
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Lấy URL hiện tại */
  getCurrentUrl(): string {
    return this.page.url();
  }

  /** Kiểm tra URL hiện tại có chứa đoạn path không */
  async expectUrlContains(path: string) {
    await expect(this.page).toHaveURL(new RegExp(path));
  }

  /** Kiểm tra title của trang */
  async expectTitleContains(text: string) {
    await expect(this.page).toHaveTitle(new RegExp(text, 'i'));
  }

  // ─── Visual Checks ───

  /** Chụp màn hình toàn trang */
  async takeFullScreenshot(name: string) {
    await this.page.screenshot({ path: `screenshots/${name}.png`, fullPage: true });
  }

  /** Kiểm tra toàn bộ ảnh <img> trên trang — tự nhiên có width > 0? */
  async validateAllImages(): Promise<{ broken: number; total: number; brokenUrls: string[] }> {
    const result = await this.page.evaluate(() => {
      const imgs = Array.from(document.querySelectorAll('img'));
      let broken = 0;
      const brokenUrls: string[] = [];
      imgs.forEach((img) => {
        // Bỏ qua ảnh placeholder
        if (img.src.includes('placeholder')) return;
        if (!img.complete || img.naturalWidth === 0) {
          broken++;
          brokenUrls.push(img.src || '(no src)');
        }
      });
      return { total: imgs.length, broken, brokenUrls };
    });
    return result;
  }

  /** Kiểm tra console có lỗi không */
  async getConsoleErrors(): Promise<string[]> {
    const errors: string[] = [];
    this.page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    return errors;
  }

  // ─── Toast / Notification ───

  /** Chờ và lấy nội dung toast message */
  async getToastMessage(): Promise<string | null> {
    try {
      const toast = this.page.locator('[class*="toast"], [class*="Toast"]').first();
      await toast.waitFor({ state: 'visible', timeout: 5_000 });
      return await toast.textContent();
    } catch {
      return null;
    }
  }

  // ─── Scroll ───

  /** Scroll đến cuối trang */
  async scrollToBottom() {
    await this.page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await this.page.waitForTimeout(500);
  }

  /** Scroll đến phần tử */
  async scrollToElement(locator: Locator) {
    await locator.scrollIntoViewIfNeeded();
    await this.page.waitForTimeout(200);
  }

  // ─── Utilities ───

  /** Lấy text từ một element, trả về null nếu không tìm thấy */
  async getText(locator: Locator): Promise<string | null> {
    try {
      return await locator.textContent();
    } catch {
      return null;
    }
  }

  /** Đợi một khoảng thời gian (dùng khi Render cần thêm thời gian) */
  async waitForRender(ms: number = 2000) {
    await this.page.waitForTimeout(ms);
  }
}
