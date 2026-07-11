# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🔐 Đăng nhập - Negative & Positive >> [TC-2.1] Đăng nhập sai mật khẩu - hiển thị lỗi "Mật khẩu không đúng"
- Location: tests\02-customer-flow.spec.ts:27:7

# Error details

```
Error: expect(received).toContain(expected) // indexOf

Expected substring: "mật khẩu"
Received string:    "⚠️ bạn đã gửi quá nhiều yêu cầu. vui lòng thử lại sau 60 giây. thử lại sau 60 giây."
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
        - textbox "Tên đăng nhập hoặc số điện thoại" [ref=e31]: tranthib
        - generic [ref=e32]: Mật khẩu
        - generic [ref=e33]:
          - textbox "Mật khẩu" [ref=e34]: sai_mat_khau_123
          - button "Hiện/ẩn mật khẩu" [ref=e35] [cursor=pointer]:
            - generic: 
        - alert [ref=e36]:
          - generic [ref=e37]: 
          - text: ⚠️ Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau 60 giây.
          - strong [ref=e38]: Thử lại sau 60 giây.
        - generic [ref=e39]:
          - generic [ref=e40] [cursor=pointer]:
            - checkbox "Lưu đăng nhập" [ref=e41]
            - text: Lưu đăng nhập
          - link "Quên mật khẩu?" [ref=e42] [cursor=pointer]:
            - /url: /Home/Forgot
        - button "Đăng nhập" [ref=e43] [cursor=pointer]
      - generic [ref=e45]:
        - text: Chưa có tài khoản?
        - link "Đăng ký" [ref=e46] [cursor=pointer]:
          - /url: /Home/Signup
      - generic [ref=e47]:
        - text: Bằng cách đăng nhập hoặc đăng ký, bạn đồng ý với
        - link "Điều khoản dịch vụ" [ref=e48] [cursor=pointer]:
          - /url: "#"
        - text: của Fastship
```

# Test source

```ts
  1   | /**
  2   |  * 🛍️ BỘ TEST 02: LUỒNG KHÁCH HÀNG (Customer E2E Flow)
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Kiểm thử đăng nhập sai/đúng
  6   |  * - Tìm kiếm, lọc danh mục, xem chi tiết quán
  7   |  * - Thêm món vào giỏ, tăng/giảm/xoá
  8   |  * - Checkout: form validation, COD payment, order ID
  9   |  * - Security: XSS, SQL Injection, boundary values
  10  |  * - Kiểm tra trạng thái đơn dưới database
  11  |  */
  12  | 
  13  | import { test, expect } from '@playwright/test';
  14  | import { HomePage } from '../pages/HomePage';
  15  | import { LoginPage } from '../pages/LoginPage';
  16  | import { CartPage } from '../pages/CartPage';
  17  | import { CheckoutPage } from '../pages/CheckoutPage';
  18  | import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
  19  | import { USERS, URLS, SHIPPING, INVALID_CREDENTIALS, SEED } from '../fixtures/users';
  20  | 
  21  | const CUSTOMER = USERS.customer1;
  22  | const RESTAURANT_CRED = USERS.restaurant1;
  23  | 
  24  | // ─── TEST SUITE 1: Đăng nhập ───
  25  | test.describe('🔐 Đăng nhập - Negative & Positive', () => {
  26  | 
  27  |   test('[TC-2.1] Đăng nhập sai mật khẩu - hiển thị lỗi "Mật khẩu không đúng"', async ({ page }) => {
  28  |     const login = new LoginPage(page);
  29  |     await login.gotoLogin();
  30  | 
  31  |     await login.usernameInput.fill(INVALID_CREDENTIALS.wrongPassword.username);
  32  |     await login.passwordInput.fill(INVALID_CREDENTIALS.wrongPassword.password);
  33  | 
  34  |     // Lấy page content trước khi click để compare
  35  |     await login.loginButton.click();
  36  | 
  37  |     // Chờ page ổn định (form submit rồi render lại)
  38  |     await page.waitForLoadState('networkidle');
  39  |     await page.waitForTimeout(2000);
  40  | 
  41  |     // Kiểm tra error alert
  42  |     const errorMsg = await login.getErrorMessage();
  43  |     console.log(`❌ Lỗi: ${errorMsg}`);
  44  |     expect(errorMsg).toBeTruthy();
> 45  |     expect(errorMsg?.toLowerCase()).toContain('mật khẩu');
      |                                     ^ Error: expect(received).toContain(expected) // indexOf
  46  |     // URL vẫn là /Home/Login
  47  |     expect(page.url()).toContain('/Home/Login');
  48  |   });
  49  | 
  50  |   test('[TC-2.2] Đăng nhập tài khoản không tồn tại - lỗi "không tồn tại"', async ({ page }) => {
  51  |     const login = new LoginPage(page);
  52  |     await login.gotoLogin();
  53  | 
  54  |     await login.usernameInput.fill(INVALID_CREDENTIALS.nonExistent.username);
  55  |     await login.passwordInput.fill(INVALID_CREDENTIALS.nonExistent.password);
  56  |     await login.loginButton.click();
  57  | 
  58  |     await page.waitForLoadState('networkidle');
  59  |     await page.waitForTimeout(2000);
  60  | 
  61  |     const errorMsg = await login.getErrorMessage();
  62  |     console.log(`❌ Lỗi: ${errorMsg}`);
  63  |     expect(errorMsg).toBeTruthy();
  64  |     expect(errorMsg?.toLowerCase()).toContain('không tồn tại');
  65  |   });
  66  | 
  67  |   test('[TC-2.3] Đăng nhập để trống username - validation', async ({ page }) => {
  68  |     const login = new LoginPage(page);
  69  |     await login.gotoLogin();
  70  | 
  71  |     await login.passwordInput.fill('somepassword');
  72  |     await login.loginButton.click();
  73  |     await page.waitForTimeout(1000);
  74  | 
  75  |     // HTML5 validation: field required, không submit được
  76  |     const urlAfter = page.url();
  77  |     console.log(`URL sau khi submit form trống: ${urlAfter}`);
  78  |     expect(urlAfter).toContain('/Home/Login');
  79  |   });
  80  | 
  81  |   test('[TC-2.4] Đăng nhập đúng - redirect về trang chủ', async ({ page }) => {
  82  |     const login = new LoginPage(page);
  83  |     await login.gotoLogin();
  84  | 
  85  |     await login.usernameInput.fill(CUSTOMER.username);
  86  |     await login.passwordInput.fill(CUSTOMER.password);
  87  |     await login.loginButton.click();
  88  | 
  89  |     // Chờ redirect — ponytail: nếu timeout, thử goto thẳng trang chủ
  90  |     try {
  91  |       await page.waitForLoadState('networkidle', { timeout: 15_000 });
  92  |     } catch {
  93  |       console.log('⏳ Login POST timeout, thử goto thẳng /Home...');
  94  |     }
  95  |     await page.waitForTimeout(3000);
  96  | 
  97  |     const currentUrl = page.url();
  98  |     console.log(`📍 URL sau login: ${currentUrl}`);
  99  |     // ponytail: nếu vẫn ở /Home/Login, thử goto / trực tiếp (session đã được set)
  100 |     if (currentUrl.includes('/Home/Login')) {
  101 |       console.log('⏳ Vẫn ở login page, thử goto /...');
  102 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 20_000 });
  103 |       const newUrl = page.url();
  104 |       console.log(`📍 URL sau goto /: ${newUrl}`);
  105 |       expect(newUrl).not.toContain('/Home/Login');
  106 |       // Xác nhận đã login: user dropdown hiển thị
  107 |       const home = new HomePage(page);
  108 |       try {
  109 |         await expect(home.userDropdown).toBeVisible({ timeout: 5_000 });
  110 |         console.log('✅ User dropdown visible — đã login');
  111 |       } catch {
  112 |         console.log('ℹ️ User dropdown không visible (có thể UI khác)');
  113 |       }
  114 |     } else {
  115 |       expect(currentUrl).not.toContain('/Home/Login');
  116 |     }
  117 |   });
  118 | 
  119 |   test('[TC-2.5] Đăng nhập với Remember Me - session còn sau redirect', async ({ page }) => {
  120 |     const login = new LoginPage(page);
  121 |     await login.gotoLogin();
  122 | 
  123 |     await login.usernameInput.fill(CUSTOMER.username);
  124 |     await login.passwordInput.fill(CUSTOMER.password);
  125 |     await login.rememberMeCheckbox.check();
  126 |     await login.loginButton.click();
  127 | 
  128 |     await page.waitForLoadState('networkidle');
  129 |     await page.waitForTimeout(2000);
  130 | 
  131 |     // Kiểm tra đã login - user dropdown hiển thị
  132 |     const home = new HomePage(page);
  133 |     try {
  134 |       await expect(home.userDropdown).toBeVisible({ timeout: 8_000 });
  135 |       console.log('✅ Remember Me - user dropdown visible');
  136 |     } catch {
  137 |       console.log('ℹ️ User dropdown không visible (có thể do UI khác)');
  138 |     }
  139 |   });
  140 | });
  141 | 
  142 | // ─── TEST SUITE 2: Tìm kiếm & Duyệt ───
  143 | test.describe('🔍 Tìm kiếm & Duyệt danh sách', () => {
  144 | 
  145 |   test('[TC-2.6] Tìm kiếm "pizza" - hiển thị ít nhất 1 kết quả', async ({ page }) => {
```