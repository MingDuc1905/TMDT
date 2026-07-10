# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 💰 Ví tiền & Thu nhập Shipper >> [TC-4.11] Trang lịch sử giao hàng load
- Location: tests\04-shipper-flow.spec.ts:233:7

# Error details

```
TimeoutError: page.goto: Timeout 30000ms exceeded.
Call log:
  - navigating to "https://fastship-web.onrender.com/Home/Login", waiting until "networkidle"

```

# Page snapshot

```yaml
- generic [ref=e2]:
  - banner [ref=e3]:
    - generic [ref=e4]:
      - link "Fastship" [ref=e5] [cursor=pointer]:
        - /url: /Home
        - generic [ref=e6]: Fastship
      - generic [ref=e7]:
        - link " Trang chủ" [ref=e8] [cursor=pointer]:
          - /url: /Home
          - generic [ref=e9]: 
          - text: Trang chủ
        - link "Đăng ký" [ref=e10] [cursor=pointer]:
          - /url: /Home/Signup
  - main [ref=e11]:
    - generic [ref=e12]:
      - link "Fastship" [ref=e13] [cursor=pointer]:
        - /url: /Home
        - heading "Fastship" [level=1] [ref=e14]
      - heading "Đăng nhập" [level=2] [ref=e15]
      - generic [ref=e16]:
        - generic [ref=e18]:
          - link "Đăng nhập bằng Google" [ref=e19] [cursor=pointer]:
            - /url: /Home/GoogleLogin
            - img [ref=e20]
            - generic [ref=e25]: Đăng nhập bằng Google
          - link " Đăng ký làm Đối tác Quán ăn / Shipper" [ref=e27] [cursor=pointer]:
            - /url: /Home/GooglePartnerLogin
            - generic [ref=e28]: 
            - text: Đăng ký làm Đối tác Quán ăn / Shipper
        - generic [ref=e29]: hoặc bằng tài khoản
        - generic [ref=e30]: Tên đăng nhập hoặc số điện thoại
        - textbox "Tên đăng nhập hoặc số điện thoại" [ref=e31]
        - generic [ref=e32]: Mật khẩu
        - generic [ref=e33]:
          - textbox "Mật khẩu" [ref=e34]
          - button "Hiện/ẩn mật khẩu" [ref=e35] [cursor=pointer]:
            - generic: 
        - generic [ref=e36]:
          - generic [ref=e37] [cursor=pointer]:
            - checkbox "Lưu đăng nhập" [ref=e38]
            - text: Lưu đăng nhập
          - link "Quên mật khẩu?" [ref=e39] [cursor=pointer]:
            - /url: /Home/Forgot
        - button "Đăng nhập" [ref=e40] [cursor=pointer]
      - generic [ref=e42]:
        - text: Chưa có tài khoản?
        - link "Đăng ký" [ref=e43] [cursor=pointer]:
          - /url: /Home/Signup
      - generic [ref=e44]:
        - text: Bằng cách đăng nhập hoặc đăng ký, bạn đồng ý với
        - link "Điều khoản dịch vụ" [ref=e45] [cursor=pointer]:
          - /url: "#"
        - text: của Fastship
```

# Test source

```ts
  1   | import { Page, Locator, expect } from '@playwright/test';
  2   | 
  3   | /**
  4   |  * BasePage — Lớp cơ sở cho tất cả Page Objects
  5   |  * Chứa các method dùng chung: navigation, wait, screenshot, toast, ...
  6   |  */
  7   | export class BasePage {
  8   |   readonly page: Page;
  9   | 
  10  |   // Common elements xuất hiện trên hầu hết các trang
  11  |   readonly navbar: Locator;
  12  |   readonly footer: Locator;
  13  |   readonly loadingSkeleton: Locator;
  14  |   readonly backToTopBtn: Locator;
  15  | 
  16  |   constructor(page: Page) {
  17  |     this.page = page;
  18  | 
  19  |     this.navbar = page.locator('.fs-header');
  20  |     this.footer = page.locator('.fs-footer');
  21  |     this.loadingSkeleton = page.locator('#fs-loading-skeleton');
  22  |     this.backToTopBtn = page.locator('.back-to-top');
  23  |   }
  24  | 
  25  |   // ─── Navigation ───
  26  | 
  27  |   /** Điều hướng đến URL tương đối */
  28  |   async goto(path: string = '/') {
> 29  |     await this.page.goto(path, { waitUntil: 'networkidle' });
      |                     ^ TimeoutError: page.goto: Timeout 30000ms exceeded.
  30  |     // Chờ skeleton loading biến mất (nếu có)
  31  |     await this.waitForSkeletonHidden();
  32  |   }
  33  | 
  34  |   /** Chờ skeleton loading ẩn đi */
  35  |   async waitForSkeletonHidden() {
  36  |     try {
  37  |       await this.loadingSkeleton.waitFor({ state: 'hidden', timeout: 15_000 });
  38  |     } catch {
  39  |       // Nếu không có skeleton (ví dụ trang dashboard), bỏ qua
  40  |     }
  41  |   }
  42  | 
  43  |   /** Chờ trang load hoàn tất — network idle + DOM ready */
  44  |   async waitForPageReady() {
  45  |     await this.page.waitForLoadState('networkidle');
  46  |     await this.page.waitForLoadState('domcontentloaded');
  47  |   }
  48  | 
  49  |   /** Lấy URL hiện tại */
  50  |   getCurrentUrl(): string {
  51  |     return this.page.url();
  52  |   }
  53  | 
  54  |   /** Kiểm tra URL hiện tại có chứa đoạn path không */
  55  |   async expectUrlContains(path: string) {
  56  |     await expect(this.page).toHaveURL(new RegExp(path));
  57  |   }
  58  | 
  59  |   /** Kiểm tra title của trang */
  60  |   async expectTitleContains(text: string) {
  61  |     await expect(this.page).toHaveTitle(new RegExp(text, 'i'));
  62  |   }
  63  | 
  64  |   // ─── Visual Checks ───
  65  | 
  66  |   /** Chụp màn hình toàn trang */
  67  |   async takeFullScreenshot(name: string) {
  68  |     await this.page.screenshot({ path: `screenshots/${name}.png`, fullPage: true });
  69  |   }
  70  | 
  71  |   /** Kiểm tra toàn bộ ảnh <img> trên trang — tự nhiên có width > 0? */
  72  |   async validateAllImages(): Promise<{ broken: number; total: number; brokenUrls: string[] }> {
  73  |     const result = await this.page.evaluate(() => {
  74  |       const imgs = Array.from(document.querySelectorAll('img'));
  75  |       let broken = 0;
  76  |       const brokenUrls: string[] = [];
  77  |       imgs.forEach((img) => {
  78  |         // Bỏ qua ảnh placeholder
  79  |         if (img.src.includes('placeholder')) return;
  80  |         if (!img.complete || img.naturalWidth === 0) {
  81  |           broken++;
  82  |           brokenUrls.push(img.src || '(no src)');
  83  |         }
  84  |       });
  85  |       return { total: imgs.length, broken, brokenUrls };
  86  |     });
  87  |     return result;
  88  |   }
  89  | 
  90  |   /** Kiểm tra console có lỗi không */
  91  |   async getConsoleErrors(): Promise<string[]> {
  92  |     const errors: string[] = [];
  93  |     this.page.on('console', (msg) => {
  94  |       if (msg.type() === 'error') errors.push(msg.text());
  95  |     });
  96  |     return errors;
  97  |   }
  98  | 
  99  |   // ─── Toast / Notification ───
  100 | 
  101 |   /** Chờ và lấy nội dung toast message */
  102 |   async getToastMessage(): Promise<string | null> {
  103 |     try {
  104 |       const toast = this.page.locator('[class*="toast"], [class*="Toast"]').first();
  105 |       await toast.waitFor({ state: 'visible', timeout: 5_000 });
  106 |       return await toast.textContent();
  107 |     } catch {
  108 |       return null;
  109 |     }
  110 |   }
  111 | 
  112 |   // ─── Scroll ───
  113 | 
  114 |   /** Scroll đến cuối trang */
  115 |   async scrollToBottom() {
  116 |     await this.page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  117 |     await this.page.waitForTimeout(500);
  118 |   }
  119 | 
  120 |   /** Scroll đến phần tử */
  121 |   async scrollToElement(locator: Locator) {
  122 |     await locator.scrollIntoViewIfNeeded();
  123 |     await this.page.waitForTimeout(200);
  124 |   }
  125 | 
  126 |   // ─── Utilities ───
  127 | 
  128 |   /** Lấy text từ một element, trả về null nếu không tìm thấy */
  129 |   async getText(locator: Locator): Promise<string | null> {
```