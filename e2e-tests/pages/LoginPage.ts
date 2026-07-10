import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * LoginPage — Page Object cho trang đăng nhập Fastship
 * URL: /Home/Login
 * 
 * Các selectors được ưu tiên dùng page.getByRole() hoặc locator css rõ ràng
 */
export class LoginPage extends BasePage {
  // ─── Form Inputs ───
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly rememberMeCheckbox: Locator;
  readonly forgotPasswordLink: Locator;
  readonly registerLink: Locator;

  // ─── Google OAuth ───
  readonly googleLoginButton: Locator;
  readonly googlePartnerLink: Locator;

  // ─── Error / Messages ───
  readonly errorAlert: Locator;
  readonly successMessage: Locator;

  constructor(page: Page) {
    super(page);

    // Dùng getByRole ưu tiên, fallback css nếu cần
    this.usernameInput = page.getByRole('textbox', { name: /tên đăng nhập/i });
    this.passwordInput = page.locator('#login-pwd');
    this.loginButton = page.getByRole('button', { name: /đăng nhập/i });
    this.rememberMeCheckbox = page.locator('input[name="rememberMe"]');
    this.forgotPasswordLink = page.getByRole('link', { name: /quên mật khẩu/i });
    // ponytail: .first() — navbar + footer đều có link Đăng ký
    this.registerLink = page.getByRole('link', { name: /đăng ký/i }).first();
    this.googleLoginButton = page.getByRole('link', { name: /đăng nhập bằng google/i });
    this.googlePartnerLink = page.locator('a[href*="GooglePartnerLogin"]');
    this.errorAlert = page.locator('.alert-danger');
    this.successMessage = page.locator('.alert-success');
  }

  /** Điều hướng đến trang login */
  async gotoLogin() {
    await this.goto('/Home/Login');
    await this.page.waitForSelector('.auth-card', { timeout: 15_000 });
  }

  /** Đăng nhập với username/password — chờ redirect hoàn tất */
  async login(username: string, password: string): Promise<string> {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.loginButton.click();

    // Chờ phản hồi từ server (form POST hoặc AJAX)
    try {
      await this.page.waitForLoadState('networkidle', { timeout: 20_000 });
    } catch {
      // Timeout là bình thường nếu form POST redirect
    }
    await this.page.waitForTimeout(2000);

    // Trả về URL hiện tại (sau redirect)
    return this.page.url();
  }

  /** Đăng nhập và chờ redirect về trang chủ */
  async loginAsCustomer(username: string, password: string) {
    await this.login(username, password);
    // Nếu login thành công, sẽ redirect về trang chủ
    // Nếu thất bại, URL vẫn là /Home/Login và error alert hiện ra
    await this.page.waitForLoadState('networkidle');
  }

  /** Lấy nội dung lỗi nếu có */
  async getErrorMessage(): Promise<string | null> {
    try {
      await this.errorAlert.waitFor({ state: 'visible', timeout: 5_000 });
      return await this.errorAlert.textContent();
    } catch {
      return null;
    }
  }

  /** Kiểm tra có đang ở trang login không */
  async isOnLoginPage(): Promise<boolean> {
    return this.page.url().includes('/Home/Login');
  }

  /** Click nút Google Login */
  async clickGoogleLogin() {
    await this.googleLoginButton.click();
    await this.page.waitForTimeout(2000);
  }
}
