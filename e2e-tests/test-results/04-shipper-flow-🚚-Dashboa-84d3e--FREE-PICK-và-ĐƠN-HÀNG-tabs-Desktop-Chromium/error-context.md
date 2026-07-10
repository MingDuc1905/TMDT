# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🚚 Dashboard Shipper - Tabs & Navigation >> [TC-4.2] Dashboard hiển thị FREE-PICK và ĐƠN HÀNG tabs
- Location: tests\04-shipper-flow.spec.ts:48:7

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: locator('#orders-all-tab')
Expected: visible
Timeout: 10000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" with timeout 10000ms
  - waiting for locator('#orders-all-tab')

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
  31  |   // Nếu redirect crash (500), session vẫn được set — goto '/' để verify
  32  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  33  |     console.log('⏳ Dashboard redirect crash (500), goto /...');
  34  |     await page.goto('/', { waitUntil: 'networkidle', timeout: 20_000 });
  35  |   }
  36  | }
  37  | 
  38  | // ─── TEST SUITE 1: Dashboard & Tabs ───
  39  | test.describe('🚚 Dashboard Shipper - Tabs & Navigation', () => {
  40  | 
  41  |   test('[TC-4.1] Đăng nhập shipper - redirect đến /Shipper', async ({ page }) => {
  42  |     await loginAsShipper(page);
  43  |     const url = page.url();
  44  |     console.log(`✅ URL: ${url}`);
  45  |     expect(url).toContain('/Shipper');
  46  |   });
  47  | 
  48  |   test('[TC-4.2] Dashboard hiển thị FREE-PICK và ĐƠN HÀNG tabs', async ({ page }) => {
  49  |     await loginAsShipper(page);
  50  |     const shipper = new ShipperPage(page);
  51  | 
> 52  |     await expect(shipper.freepickTab).toBeVisible({ timeout: 10_000 });
      |                                       ^ Error: expect(locator).toBeVisible() failed
  53  |     await expect(shipper.orderTab).toBeVisible({ timeout: 10_000 });
  54  |     console.log('✅ FREE-PICK và ĐƠN HÀNG tabs hiển thị');
  55  |   });
  56  | 
  57  |   test('[TC-4.3] Tab FREE-PICK - danh sách đơn chờ nhận load', async ({ page }) => {
  58  |     await loginAsShipper(page);
  59  |     const shipper = new ShipperPage(page);
  60  | 
  61  |     await shipper.openFreepickTab();
  62  |     await page.waitForTimeout(2000);
  63  | 
  64  |     // Kiểm tra có đơn trong FREE-PICK
  65  |     const orderCount = await shipper.getOrderCount();
  66  |     console.log(`📋 FREE-PICK orders: ${orderCount}`);
  67  |   });
  68  | 
  69  |   test('[TC-4.4] Tab ĐƠN HÀNG - danh sách đơn đã nhận', async ({ page }) => {
  70  |     await loginAsShipper(page);
  71  |     const shipper = new ShipperPage(page);
  72  | 
  73  |     await shipper.openOrderTab();
  74  |     await page.waitForSelector('.table-responsive', { timeout: 15_000 });
  75  | 
  76  |     const orderCount = await shipper.getOrderCount();
  77  |     console.log(`📋 Đơn đã nhận: ${orderCount}`);
  78  |   });
  79  | 
  80  |   test('[TC-4.5] Bản đồ FREE-PICK hiển thị', async ({ page }) => {
  81  |     await loginAsShipper(page);
  82  |     const shipper = new ShipperPage(page);
  83  | 
  84  |     await shipper.openFreepickTab();
  85  |     await page.waitForTimeout(3000);
  86  | 
  87  |     const mapVisible = await shipper.isMapVisible().catch(() => false);
  88  |     const mapDiv = await page.locator('#shipper-map, #map, [class*="map"]').count();
  89  |     console.log(`🗺️ Map container: ${mapDiv}, Visible: ${mapVisible}`);
  90  |   });
  91  | });
  92  | 
  93  | // ─── TEST SUITE 2: Nhận đơn & Giao hàng ───
  94  | test.describe('📦 Nhận đơn & Quy trình giao hàng', () => {
  95  | 
  96  |   test('[TC-4.6] Click "Chi tiết" / "Nhận đơn" đầu tiên', async ({ page }) => {
  97  |     await loginAsShipper(page);
  98  |     const shipper = new ShipperPage(page);
  99  | 
  100 |     // Mở FREE-PICK
  101 |     await shipper.openFreepickTab();
  102 |     await page.waitForTimeout(2000);
  103 | 
  104 |     // Kiểm tra link chi tiết
  105 |     const detailLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
  106 |     const linkCount = await detailLinks.count();
  107 |     console.log(`🔗 Order detail links: ${linkCount}`);
  108 | 
  109 |     if (linkCount > 0) {
  110 |       await detailLinks.first().click();
  111 |       await page.waitForLoadState('networkidle');
  112 |       await page.waitForTimeout(2000);
  113 | 
  114 |       const url = page.url();
  115 |       console.log(`📍 URL sau click: ${url}`);
  116 |       expect(url).toContain('OrderDetail');
  117 |     } else {
  118 |       console.log('ℹ️ Không có đơn trong FREE-PICK');
  119 |     }
  120 |   });
  121 | 
  122 |   test('[TC-4.7] Cập nhật trạng thái giao hàng (nếu có nút)', async ({ page }) => {
  123 |     await loginAsShipper(page);
  124 | 
  125 |     // Vào tab ĐƠN HÀNG để xem đơn đã nhận
  126 |     const shipper = new ShipperPage(page);
  127 |     await shipper.openOrderTab();
  128 |     await page.waitForTimeout(2000);
  129 | 
  130 |     // Kiểm tra các nút cập nhật trạng thái
  131 |     const statusUpdateBtns = [
  132 |       { label: 'Đã lấy hàng', selector: 'a[href*="danggiaohang"], a[href*="layhang"]' },
  133 |       { label: 'Đang giao', selector: 'a[href*="danggiao"]' },
  134 |       { label: 'Giao thành công', selector: 'a[href*="dagiao"], a[href*="hoantat"]' },
  135 |     ];
  136 | 
  137 |     for (const btn of statusUpdateBtns) {
  138 |       const btnCount = await page.locator(btn.selector).count();
  139 |       if (btnCount > 0) {
  140 |         console.log(`🟢 Nút "${btn.label}": ${btnCount}`);
  141 |       } else {
  142 |         console.log(`⚪ Nút "${btn.label}": không có`);
  143 |       }
  144 |     }
  145 |   });
  146 | 
  147 |   test('[TC-4.8] Chi tiết đơn hàng đã nhận - thông tin hiển thị đầy đủ', async ({ page }) => {
  148 |     await loginAsShipper(page);
  149 | 
  150 |     // Vào ĐƠN HÀNG
  151 |     const shipper = new ShipperPage(page);
  152 |     await shipper.openOrderTab();
```