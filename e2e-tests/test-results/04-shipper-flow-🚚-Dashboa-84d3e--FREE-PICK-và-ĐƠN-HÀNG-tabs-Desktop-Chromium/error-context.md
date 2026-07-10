# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🚚 Dashboard Shipper - Tabs & Navigation >> [TC-4.2] Dashboard hiển thị FREE-PICK và ĐƠN HÀNG tabs
- Location: tests\04-shipper-flow.spec.ts:61:7

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: locator('#orders-paid-tab, [data-tab="orders"], a:has-text("ĐƠN HÀNG"), a:has-text("Đã nhận")')
Expected: visible
Error: strict mode violation: locator('#orders-paid-tab, [data-tab="orders"], a:has-text("ĐƠN HÀNG"), a:has-text("Đã nhận")') resolved to 2 elements:
    1) <a aria-expanded="false" href="/Shipper/LichSu" class="has-arrow ai-icon">…</a> aka getByRole('link', { name: 'Lịch sử đơn hàng' })
    2) <a role="tab" href="#danhsach" data-toggle="tab" id="orders-paid-tab" aria-selected="false" class="flex-sm-fill text-sm-center nav-link">ĐƠN HÀNG</a> aka getByRole('tab', { name: 'ĐƠN HÀNG' })

Call log:
  - Expect "toBeVisible" with timeout 10000ms
  - waiting for locator('#orders-paid-tab, [data-tab="orders"], a:has-text("ĐƠN HÀNG"), a:has-text("Đã nhận")')

```

# Page snapshot

```yaml
- generic [ref=e7]:
  - link [ref=e9] [cursor=pointer]:
    - /url: /Shipper
  - navigation [ref=e17]:
    - list [ref=e19]:
      - listitem [ref=e20]:
        - link [ref=e21] [cursor=pointer]:
          - /url: "#"
          - img [ref=e22]
      - listitem [ref=e27]:
        - generic [ref=e28]: Trạng thái làm việc
      - listitem [ref=e29]:
        - generic [ref=e31] [cursor=pointer]:
          - link "Đóng":
            - /url: /Shipper/updateStatus
            - generic [ref=e32]: Đóng
      - listitem [ref=e33]:
        - button "Xin chào, Nguyen Thi Z" [ref=e34] [cursor=pointer]:
          - generic [ref=e36]:
            - text: Xin chào,
            - strong [ref=e37]: Nguyen Thi Z
  - list [ref=e40]:
    - listitem [ref=e41]:
      - link "Ví" [ref=e42] [cursor=pointer]:
        - /url: /Shipper/ViTien
    - listitem [ref=e43]:
      - link "Lịch sử đơn hàng" [ref=e44] [cursor=pointer]:
        - /url: /Shipper/LichSu
    - listitem [ref=e45]:
      - link "Cài đặt" [ref=e46] [cursor=pointer]:
        - /url: /Shipper/CaiDat
  - generic [ref=e49]:
    - navigation [ref=e50]:
      - tab "FREE-PICK" [selected] [ref=e51] [cursor=pointer]
      - tab "ĐƠN HÀNG" [ref=e52] [cursor=pointer]
    - link "Làm mới" [ref=e58] [cursor=pointer]:
      - /url: javascript:void()
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
  32  |   // Solution: goto trực tiếp /Shipper, retry nhanh với domcontentloaded
  33  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  34  |     await page.waitForTimeout(2000); // chờ session cookie settle
  35  |     for (let retry = 0; retry < 3; retry++) {
  36  |       try {
  37  |         await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 15_000 });
  38  |         if (page.url().includes('/Shipper')) break;
  39  |       } catch {
  40  |         console.log(`⚠️ Fallback goto Shipper #${retry+1} failed`);
  41  |         await page.waitForTimeout(1000);
  42  |       }
  43  |     }
  44  |   }
  45  |   // ponytail: safety net nếu retry không kịp
  46  |   if (!page.url().includes('/Shipper')) {
  47  |     await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 15_000 }).catch(() => {});
  48  |   }
  49  | }
  50  | 
  51  | // ─── TEST SUITE 1: Dashboard & Tabs ───
  52  | test.describe('🚚 Dashboard Shipper - Tabs & Navigation', () => {
  53  | 
  54  |   test('[TC-4.1] Đăng nhập shipper - redirect đến /Shipper', async ({ page }) => {
  55  |     await loginAsShipper(page);
  56  |     const url = page.url();
  57  |     console.log(`✅ URL: ${url}`);
  58  |     expect(url).toContain('/Shipper');
  59  |   });
  60  | 
  61  |   test('[TC-4.2] Dashboard hiển thị FREE-PICK và ĐƠN HÀNG tabs', async ({ page }) => {
  62  |     await loginAsShipper(page);
  63  |     const shipper = new ShipperPage(page);
  64  | 
  65  |     await expect(shipper.freepickTab).toBeVisible({ timeout: 10_000 });
> 66  |     await expect(shipper.orderTab).toBeVisible({ timeout: 10_000 });
      |                                    ^ Error: expect(locator).toBeVisible() failed
  67  |     console.log('✅ FREE-PICK và ĐƠN HÀNG tabs hiển thị');
  68  |   });
  69  | 
  70  |   test('[TC-4.3] Tab FREE-PICK - danh sách đơn chờ nhận load', async ({ page }) => {
  71  |     await loginAsShipper(page);
  72  |     const shipper = new ShipperPage(page);
  73  | 
  74  |     await shipper.openFreepickTab();
  75  |     await page.waitForTimeout(2000);
  76  | 
  77  |     // Kiểm tra có đơn trong FREE-PICK
  78  |     const orderCount = await shipper.getOrderCount();
  79  |     console.log(`📋 FREE-PICK orders: ${orderCount}`);
  80  |   });
  81  | 
  82  |   test('[TC-4.4] Tab ĐƠN HÀNG - danh sách đơn đã nhận', async ({ page }) => {
  83  |     await loginAsShipper(page);
  84  |     const shipper = new ShipperPage(page);
  85  | 
  86  |     await shipper.openOrderTab();
  87  |     await page.waitForSelector('.table-responsive', { timeout: 15_000 });
  88  | 
  89  |     const orderCount = await shipper.getOrderCount();
  90  |     console.log(`📋 Đơn đã nhận: ${orderCount}`);
  91  |   });
  92  | 
  93  |   test('[TC-4.5] Bản đồ FREE-PICK hiển thị', async ({ page }) => {
  94  |     await loginAsShipper(page);
  95  |     const shipper = new ShipperPage(page);
  96  | 
  97  |     await shipper.openFreepickTab();
  98  |     await page.waitForTimeout(3000);
  99  | 
  100 |     const mapVisible = await shipper.isMapVisible().catch(() => false);
  101 |     const mapDiv = await page.locator('#shipper-map, #map, [class*="map"]').count();
  102 |     console.log(`🗺️ Map container: ${mapDiv}, Visible: ${mapVisible}`);
  103 |   });
  104 | });
  105 | 
  106 | // ─── TEST SUITE 2: Nhận đơn & Giao hàng ───
  107 | test.describe('📦 Nhận đơn & Quy trình giao hàng', () => {
  108 | 
  109 |   test('[TC-4.6] Click "Chi tiết" / "Nhận đơn" đầu tiên', async ({ page }) => {
  110 |     await loginAsShipper(page);
  111 |     const shipper = new ShipperPage(page);
  112 | 
  113 |     // Mở FREE-PICK
  114 |     await shipper.openFreepickTab();
  115 |     await page.waitForTimeout(2000);
  116 | 
  117 |     // Kiểm tra link chi tiết
  118 |     const detailLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
  119 |     const linkCount = await detailLinks.count();
  120 |     console.log(`🔗 Order detail links: ${linkCount}`);
  121 | 
  122 |     if (linkCount > 0) {
  123 |       await detailLinks.first().click();
  124 |       await page.waitForLoadState('networkidle');
  125 |       await page.waitForTimeout(2000);
  126 | 
  127 |       const url = page.url();
  128 |       console.log(`📍 URL sau click: ${url}`);
  129 |       expect(url).toContain('OrderDetail');
  130 |     } else {
  131 |       console.log('ℹ️ Không có đơn trong FREE-PICK');
  132 |     }
  133 |   });
  134 | 
  135 |   test('[TC-4.7] Cập nhật trạng thái giao hàng (nếu có nút)', async ({ page }) => {
  136 |     await loginAsShipper(page);
  137 | 
  138 |     // Vào tab ĐƠN HÀNG để xem đơn đã nhận
  139 |     const shipper = new ShipperPage(page);
  140 |     await shipper.openOrderTab();
  141 |     await page.waitForTimeout(2000);
  142 | 
  143 |     // Kiểm tra các nút cập nhật trạng thái
  144 |     const statusUpdateBtns = [
  145 |       { label: 'Đã lấy hàng', selector: 'a[href*="danggiaohang"], a[href*="layhang"]' },
  146 |       { label: 'Đang giao', selector: 'a[href*="danggiao"]' },
  147 |       { label: 'Giao thành công', selector: 'a[href*="dagiao"], a[href*="hoantat"]' },
  148 |     ];
  149 | 
  150 |     for (const btn of statusUpdateBtns) {
  151 |       const btnCount = await page.locator(btn.selector).count();
  152 |       if (btnCount > 0) {
  153 |         console.log(`🟢 Nút "${btn.label}": ${btnCount}`);
  154 |       } else {
  155 |         console.log(`⚪ Nút "${btn.label}": không có`);
  156 |       }
  157 |     }
  158 |   });
  159 | 
  160 |   test('[TC-4.8] Chi tiết đơn hàng đã nhận - thông tin hiển thị đầy đủ', async ({ page }) => {
  161 |     await loginAsShipper(page);
  162 | 
  163 |     // Vào ĐƠN HÀNG
  164 |     const shipper = new ShipperPage(page);
  165 |     await shipper.openOrderTab();
  166 |     await page.waitForTimeout(2000);
```