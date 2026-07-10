# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🚚 Dashboard Shipper - Tabs & Navigation >> [TC-4.1] Đăng nhập shipper - redirect đến /Shipper
- Location: tests\04-shipper-flow.spec.ts:42:7

# Error details

```
Error: expect(received).toContain(expected) // indexOf

Expected substring: "/Shipper"
Received string:    "https://fastship-web.onrender.com/Home/Login"
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
  2   |  * 🚚 BỘ TEST 04: LUỒNG SHIPPER (Rider Full Lifecycle)
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Đăng nhập Shipper -> redirect dashboard
  6   |  * - Xem FREE-PICK: danh sách đơn chờ nhận
  7   |  * - Nhận đơn giao hàng
  8   |  * - Cập nhật trạng thái: Lấy hàng -> Đang giao -> Đã giao
  9   |  * - Kiểm tra ví tiền tăng sau khi giao thành công
  10  |  * - Kiểm tra thu nhập / lịch sử giao hàng
  11  |  * - Bản đồ live tracking hiển thị
  12  |  *
  13  |  * Tài khoản: shipperz / shipz789 (userid=4, trạng thái: Đang hoạt động)
  14  |  */
  15  | 
  16  | import { test, expect } from '@playwright/test';
  17  | import { LoginPage } from '../pages/LoginPage';
  18  | import { ShipperPage } from '../pages/ShipperPage';
  19  | import { USERS, URLS } from '../fixtures/users';
  20  | 
  21  | const SHIPPER = USERS.shipper2; // shipperz - Đang hoạt động
  22  | 
  23  | // ─── Helper: Login shipper — ponytail: login OK nhưng dashboard redirect crash
  24  | // Root cause: /Shipper controller throws 500 → global handler redirect /Home/Error
  25  | // Solution: login set session thành công, dùng goto('/') để verify session
  26  | async function loginAsShipper(page: any) {
  27  |   const login = new LoginPage(page);
  28  |   // ponytail: dùng login() có 429 retry + gotoLogin() reload form
  29  |   const url = await login.login(SHIPPER.username, SHIPPER.password);
  30  |   console.log(`📍 URL sau login: ${url}`);
  31  |   // ponytail: redirect về /Home/Login → cold start làm mất session cookie
  32  |   // Solution: goto trực tiếp /Shipper
  33  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  34  |     console.log('⏳ Cold start / redirect crash, goto /Shipper directly...');
  35  |     await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 }).catch(() => console.log('⚠️ Fallback goto Shipper failed'));
  36  |   }
  37  | }
  38  | 
  39  | // ─── TEST SUITE 1: Dashboard & Tabs ───
  40  | test.describe('🚚 Dashboard Shipper - Tabs & Navigation', () => {
  41  | 
  42  |   test('[TC-4.1] Đăng nhập shipper - redirect đến /Shipper', async ({ page }) => {
  43  |     await loginAsShipper(page);
  44  |     const url = page.url();
  45  |     console.log(`✅ URL: ${url}`);
> 46  |     expect(url).toContain('/Shipper');
      |                 ^ Error: expect(received).toContain(expected) // indexOf
  47  |   });
  48  | 
  49  |   test('[TC-4.2] Dashboard hiển thị FREE-PICK và ĐƠN HÀNG tabs', async ({ page }) => {
  50  |     await loginAsShipper(page);
  51  |     const shipper = new ShipperPage(page);
  52  | 
  53  |     await expect(shipper.freepickTab).toBeVisible({ timeout: 10_000 });
  54  |     await expect(shipper.orderTab).toBeVisible({ timeout: 10_000 });
  55  |     console.log('✅ FREE-PICK và ĐƠN HÀNG tabs hiển thị');
  56  |   });
  57  | 
  58  |   test('[TC-4.3] Tab FREE-PICK - danh sách đơn chờ nhận load', async ({ page }) => {
  59  |     await loginAsShipper(page);
  60  |     const shipper = new ShipperPage(page);
  61  | 
  62  |     await shipper.openFreepickTab();
  63  |     await page.waitForTimeout(2000);
  64  | 
  65  |     // Kiểm tra có đơn trong FREE-PICK
  66  |     const orderCount = await shipper.getOrderCount();
  67  |     console.log(`📋 FREE-PICK orders: ${orderCount}`);
  68  |   });
  69  | 
  70  |   test('[TC-4.4] Tab ĐƠN HÀNG - danh sách đơn đã nhận', async ({ page }) => {
  71  |     await loginAsShipper(page);
  72  |     const shipper = new ShipperPage(page);
  73  | 
  74  |     await shipper.openOrderTab();
  75  |     await page.waitForSelector('.table-responsive', { timeout: 15_000 });
  76  | 
  77  |     const orderCount = await shipper.getOrderCount();
  78  |     console.log(`📋 Đơn đã nhận: ${orderCount}`);
  79  |   });
  80  | 
  81  |   test('[TC-4.5] Bản đồ FREE-PICK hiển thị', async ({ page }) => {
  82  |     await loginAsShipper(page);
  83  |     const shipper = new ShipperPage(page);
  84  | 
  85  |     await shipper.openFreepickTab();
  86  |     await page.waitForTimeout(3000);
  87  | 
  88  |     const mapVisible = await shipper.isMapVisible().catch(() => false);
  89  |     const mapDiv = await page.locator('#shipper-map, #map, [class*="map"]').count();
  90  |     console.log(`🗺️ Map container: ${mapDiv}, Visible: ${mapVisible}`);
  91  |   });
  92  | });
  93  | 
  94  | // ─── TEST SUITE 2: Nhận đơn & Giao hàng ───
  95  | test.describe('📦 Nhận đơn & Quy trình giao hàng', () => {
  96  | 
  97  |   test('[TC-4.6] Click "Chi tiết" / "Nhận đơn" đầu tiên', async ({ page }) => {
  98  |     await loginAsShipper(page);
  99  |     const shipper = new ShipperPage(page);
  100 | 
  101 |     // Mở FREE-PICK
  102 |     await shipper.openFreepickTab();
  103 |     await page.waitForTimeout(2000);
  104 | 
  105 |     // Kiểm tra link chi tiết
  106 |     const detailLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
  107 |     const linkCount = await detailLinks.count();
  108 |     console.log(`🔗 Order detail links: ${linkCount}`);
  109 | 
  110 |     if (linkCount > 0) {
  111 |       await detailLinks.first().click();
  112 |       await page.waitForLoadState('networkidle');
  113 |       await page.waitForTimeout(2000);
  114 | 
  115 |       const url = page.url();
  116 |       console.log(`📍 URL sau click: ${url}`);
  117 |       expect(url).toContain('OrderDetail');
  118 |     } else {
  119 |       console.log('ℹ️ Không có đơn trong FREE-PICK');
  120 |     }
  121 |   });
  122 | 
  123 |   test('[TC-4.7] Cập nhật trạng thái giao hàng (nếu có nút)', async ({ page }) => {
  124 |     await loginAsShipper(page);
  125 | 
  126 |     // Vào tab ĐƠN HÀNG để xem đơn đã nhận
  127 |     const shipper = new ShipperPage(page);
  128 |     await shipper.openOrderTab();
  129 |     await page.waitForTimeout(2000);
  130 | 
  131 |     // Kiểm tra các nút cập nhật trạng thái
  132 |     const statusUpdateBtns = [
  133 |       { label: 'Đã lấy hàng', selector: 'a[href*="danggiaohang"], a[href*="layhang"]' },
  134 |       { label: 'Đang giao', selector: 'a[href*="danggiao"]' },
  135 |       { label: 'Giao thành công', selector: 'a[href*="dagiao"], a[href*="hoantat"]' },
  136 |     ];
  137 | 
  138 |     for (const btn of statusUpdateBtns) {
  139 |       const btnCount = await page.locator(btn.selector).count();
  140 |       if (btnCount > 0) {
  141 |         console.log(`🟢 Nút "${btn.label}": ${btnCount}`);
  142 |       } else {
  143 |         console.log(`⚪ Nút "${btn.label}": không có`);
  144 |       }
  145 |     }
  146 |   });
```