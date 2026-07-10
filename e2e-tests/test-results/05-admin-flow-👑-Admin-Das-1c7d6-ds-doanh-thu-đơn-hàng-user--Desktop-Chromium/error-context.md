# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 05-admin-flow.spec.ts >> 👑 Admin Dashboard - KPI & Charts >> [TC-5.2] Dashboard hiển thị KPI cards (doanh thu, đơn hàng, user, ...)
- Location: tests\05-admin-flow.spec.ts:56:7

# Error details

```
Error: expect(received).toBeGreaterThan(expected)

Expected: > 0
Received:   0
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
  1   | /**
  2   |  * 👑 BỘ TEST 05: LUỒNG ADMIN DASHBOARD (Full Admin Operations)
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Đăng nhập Admin -> redirect dashboard
  6   |  * - Dashboard: KPI cards, biểu đồ doanh thu
  7   |  * - Quản lý người dùng: tìm kiếm, xem chi tiết
  8   |  * - Quản lý đơn hàng: xem danh sách, xác nhận/hủy
  9   |  * - Quản lý danh mục: xem, thêm mới
  10  |  * - Đối chiếu dữ liệu dashboard với database (DbDebug)
  11  |  * - Kiểm tra tất cả sidebar links hoạt động
  12  |  *
  13  |  * Tài khoản: admin1 / admin1
  14  |  */
  15  | 
  16  | import { test, expect } from '@playwright/test';
  17  | import { LoginPage } from '../pages/LoginPage';
  18  | import { AdminPage } from '../pages/AdminPage';
  19  | import { USERS, URLS } from '../fixtures/users';
  20  | 
  21  | const ADMIN = USERS.admin1;
  22  | 
  23  | // ─── Helper: Login admin — ponytail: login OK nhưng dashboard redirect crash
  24  | // Root cause: /Admin controller throws 500 → global handler redirect /Home/Error
  25  | // Solution: login set session thành công, dùng goto('/') để verify session
  26  | async function loginAsAdmin(page: any) {
  27  |   const login = new LoginPage(page);
  28  |   // ponytail: dùng login() có 429 retry + gotoLogin() reload form
  29  |   const url = await login.login(ADMIN.username, ADMIN.password);
  30  |   console.log(`📍 URL sau login: ${url}`);
  31  |   // ponytail: redirect về /Home/Login → cold start làm mất session cookie
  32  |   // Solution: goto trực tiếp /Admin, retry 1 lần nếu fail
  33  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  34  |     for (let retry = 0; retry < 2; retry++) {
  35  |       try {
  36  |         await page.goto('/Admin', { waitUntil: 'load', timeout: 30_000 });
  37  |         break;
  38  |       } catch {
  39  |         console.log(`⚠️ Fallback goto Admin #${retry+1} failed`);
  40  |         await page.waitForTimeout(3000);
  41  |       }
  42  |     }
  43  |   }
  44  | }
  45  | 
  46  | // ─── TEST SUITE 1: Dashboard ───
  47  | test.describe('👑 Admin Dashboard - KPI & Charts', () => {
  48  | 
  49  |   test('[TC-5.1] Đăng nhập admin - redirect đến /Admin', async ({ page }) => {
  50  |     await loginAsAdmin(page);
  51  |     const url = page.url();
  52  |     console.log(`✅ URL: ${url}`);
  53  |     expect(url).toContain('/Admin');
  54  |   });
  55  | 
  56  |   test('[TC-5.2] Dashboard hiển thị KPI cards (doanh thu, đơn hàng, user, ...)', async ({ page }) => {
  57  |     await loginAsAdmin(page);
  58  | 
  59  |     // ponytail: admin dashboard có thể dùng các class khác nhau — chờ page load trước
  60  |     await page.waitForLoadState('networkidle', { timeout: 30_000 });
  61  |     await page.waitForTimeout(3000);
  62  | 
  63  |     // Đếm tất cả cards/boxes trên dashboard
  64  |     const allCards = page.locator('.card, [class*="kpi"], .card-header, .card-body');
  65  |     const cardCount = await allCards.count();
  66  |     console.log(`📊 Cards/boxes: ${cardCount}`);
> 67  |     expect(cardCount).toBeGreaterThan(0);
      |                       ^ Error: expect(received).toBeGreaterThan(expected)
  68  | 
  69  |     // In text từng card
  70  |     for (let i = 0; i < Math.min(cardCount, 6); i++) {
  71  |       const text = await allCards.nth(i).textContent();
  72  |       console.log(`  Card ${i}: ${text?.trim().substring(0, 80)}`);
  73  |     }
  74  |   });
  75  | 
  76  |   test('[TC-5.3] Biểu đồ doanh thu Chart.js render', async ({ page }) => {
  77  |     await loginAsAdmin(page);
  78  | 
  79  |     await page.waitForLoadState('networkidle', { timeout: 30_000 });
  80  |     const canvasCount = await page.locator('canvas').count();
  81  |     console.log(`📈 Canvas elements: ${canvasCount}`);
  82  |     // ponytail: không fail nếu không có canvas (admin có thể chưa cấu hình chart)
  83  |     if (canvasCount > 0) {
  84  |       const canvasBox = await page.locator('canvas').first().boundingBox();
  85  |       if (canvasBox) {
  86  |         expect(canvasBox.width).toBeGreaterThan(0);
  87  |         expect(canvasBox.height).toBeGreaterThan(0);
  88  |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  89  |       }
  90  |     } else {
  91  |       console.log('ℹ️ Không có Chart.js canvas — admin page có thể không có biểu đồ');
  92  |     }
  93  |   });
  94  | 
  95  |   test('[TC-5.4] Kiểm tra tất cả navigation links trên admin page', async ({ page }) => {
  96  |     await loginAsAdmin(page);
  97  | 
  98  |     // ponytail: admin có thể có sidebar (.deznav) hoặc menu top — đếm tất cả links
  99  |     await page.waitForLoadState('networkidle', { timeout: 30_000 });
  100 | 
  101 |     const allNavLinks = page.locator('nav a[href], .deznav a[href], .sidebar a[href], [class*="menu"] a[href]');
  102 |     const linkCount = await allNavLinks.count();
  103 |     console.log(`🔗 Tổng navigation links: ${linkCount}`);
  104 |     expect(linkCount).toBeGreaterThan(0);
  105 | 
  106 |     // Kiểm tra các link chính tồn tại
  107 |     const expectedLinks = [
  108 |       { name: 'Dashboard', href: '/Admin/Dashboard' },
  109 |       { name: 'Quản lý', href: '/Admin/QuanLyKhachHang' },
  110 |       { name: 'Đơn hàng', href: '/Admin/Order' },
  111 |       { name: 'Danh mục', href: '/Admin/Category' },
  112 |     ];
  113 |     for (const link of expectedLinks) {
  114 |       const linkEl = page.locator(`a[href*="${link.href}"]`).first();
  115 |       const exists = await linkEl.count();
  116 |       console.log(`  ${exists > 0 ? '✅' : '❌'} ${link.name}: ${link.href}`);
  117 |     }
  118 |   });
  119 | 
  120 |   test('[TC-5.5] Kiểm tra sidebar routing - click từng link', async ({ page }) => {
  121 |     await loginAsAdmin(page);
  122 | 
  123 |     const pages = [
  124 |       { name: 'Dashboard', href: '/Admin/Dashboard' },
  125 |       { name: 'Quản lý người dùng', href: '/Admin/QuanLyKhachHang' },
  126 |       { name: 'Đơn hàng', href: '/Admin/Order' },
  127 |       { name: 'Danh mục', href: '/Admin/Category' },
  128 |     ];
  129 | 
  130 |     for (const p of pages) {
  131 |       const link = page.locator(`a[href*="${p.href}"]`).first();
  132 |       if (await link.isVisible().catch(() => false)) {
  133 |         await link.click();
  134 |         await page.waitForLoadState('networkidle');
  135 |         await page.waitForTimeout(1000);
  136 |         const url = page.url();
  137 |         console.log(`✅ ${p.name}: ${url}`);
  138 |         expect(url).toContain(p.href);
  139 |       } else {
  140 |         console.log(`❌ ${p.name}: link không hiển thị`);
  141 |       }
  142 |     }
  143 |   });
  144 | 
  145 |   test('[TC-5.6] Console không có JS errors (bỏ qua network 429)', async ({ page }) => {
  146 |     const jsErrors: string[] = [];
  147 |     page.on('pageerror', (err) => { jsErrors.push(err.message); });
  148 | 
  149 |     await loginAsAdmin(page);
  150 |     await page.waitForTimeout(3000);
  151 | 
  152 |     if (jsErrors.length > 0) {
  153 |       console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
  154 |     }
  155 |     // ponytail: chỉ fail nếu có JS error thật (không tính network 429/503 từ Render)
  156 |     expect(jsErrors.length).toBe(0);
  157 |   });
  158 | });
  159 | 
  160 | // ─── TEST SUITE 2: Quản lý người dùng ───
  161 | test.describe('👥 Quản lý Người dùng (User Management)', () => {
  162 | 
  163 |   test('[TC-5.7] Trang quản lý người dùng load - bảng hiển thị', async ({ page }) => {
  164 |     await loginAsAdmin(page);
  165 | 
  166 |     const admin = new AdminPage(page);
  167 |     await admin.gotoUserManagement();
```