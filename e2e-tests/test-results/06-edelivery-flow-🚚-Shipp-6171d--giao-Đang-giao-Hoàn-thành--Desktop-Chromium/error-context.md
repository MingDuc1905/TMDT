# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 06-edelivery-flow.spec.ts >> 🚚 Shipper — QR Code Display >> [TC-6.2] QRDelivery page — tab filter hoạt động (Chờ giao / Đang giao / Hoàn thành)
- Location: tests\06-edelivery-flow.spec.ts:71:7

# Error details

```
TimeoutError: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('.qr-tab-btn').first()
    - locator resolved to <button data-tab="pending" class="qr-tab-btn active">…</button>
  - attempting click action
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div class="collapse navbar-collapse justify-content-between">…</div> from <div class="header">…</div> subtree intercepts pointer events
    - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div class="collapse navbar-collapse justify-content-between">…</div> from <div class="header">…</div> subtree intercepts pointer events
    - retrying click action
      - waiting 100ms
    57 × waiting for element to be visible, enabled and stable
       - element is visible, enabled and stable
       - scrolling into view if needed
       - done scrolling
       - <div class="collapse navbar-collapse justify-content-between">…</div> from <div class="header">…</div> subtree intercepts pointer events
     - retrying click action
       - waiting 500ms

```

# Page snapshot

```yaml
- generic [ref=e2]:
  - link [ref=e4] [cursor=pointer]:
    - /url: /Shipper
  - navigation [ref=e12]:
    - list [ref=e14]:
      - listitem [ref=e15]:
        - link [ref=e16] [cursor=pointer]:
          - /url: "#"
          - img [ref=e17]
      - listitem [ref=e22]:
        - generic [ref=e23]: Trạng thái làm việc
      - listitem [ref=e24]:
        - generic [ref=e26] [cursor=pointer]:
          - link "Đóng":
            - /url: /Shipper/updateStatus
            - generic [ref=e27]: Đóng
      - listitem [ref=e28]:
        - link "" [ref=e29] [cursor=pointer]:
          - /url: /Home/Logout
          - generic [ref=e30]: 
      - listitem [ref=e31]:
        - button "Xin chào, Nguyen Thi Z" [ref=e32] [cursor=pointer]:
          - generic [ref=e34]:
            - text: Xin chào,
            - strong [ref=e35]: Nguyen Thi Z
        - text:   
  - list [ref=e38]:
    - listitem [ref=e39]:
      - link " QR Giao hàng" [ref=e40] [cursor=pointer]:
        - /url: /Shipper/QRDelivery
        - generic [ref=e41]: 
        - text: QR Giao hàng
    - listitem [ref=e42]:
      - link "Ví" [ref=e43] [cursor=pointer]:
        - /url: /Shipper/ViTien
    - listitem [ref=e44]:
      - link "Lịch sử đơn hàng" [ref=e45] [cursor=pointer]:
        - /url: /Shipper/LichSu
    - listitem [ref=e46]:
      - link "Cài đặt" [ref=e47] [cursor=pointer]:
        - /url: /Shipper/CaiDat
  - generic [ref=e48]:
    - heading " Mã QR Giao hàng" [level=2] [ref=e49]:
      - generic [ref=e50]: 
      - text: Mã QR Giao hàng
    - generic [ref=e51]:
      - button " Chờ giao" [ref=e52] [cursor=pointer]:
        - generic [ref=e53]: 
        - text: Chờ giao
      - button " Đang giao" [ref=e54] [cursor=pointer]:
        - generic [ref=e55]: 
        - text: Đang giao
      - button " Hoàn thành" [ref=e56] [cursor=pointer]:
        - generic [ref=e57]: 
        - text: Hoàn thành
    - generic [ref=e59]:
      - generic [ref=e60]: 📭
      - paragraph [ref=e61]: Không có đơn hàng nào cần quét mã
      - text: Khi nhận đơn mới, QR sẽ hiển thị tại đây
```

# Test source

```ts
  1   | /**
  2   |  * 📸 BỘ TEST 06: E-DELIVERY QR CODE FULL FLOW
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Shipper: Xem QR code trên OrderDetail + QRDelivery list
  6   |  * - Merchant: Quét QR bằng camera (html5-qrcode)
  7   |  * - Customer: Nhận real-time notification khi QR được scan
  8   |  * - Admin: Delivery Logs Matrix + Bypass modal + pastel badges
  9   |  *
  10  |  * Flow chính:
  11  |  * 1. Customer tạo đơn → Merchant nhận đơn → Shipper nhận đơn
  12  |  * 2. Shipper vào OrderDetail → QR code hiển thị
  13  |  * 3. Merchant quét QR → API ConfirmScan → SignalR broadcast
  14  |  * 4. Customer nhận notification → giao diện cập nhật
  15  |  * 5. Admin bypass (nếu cần) → chuyển trạng thái thủ công
  16  |  */
  17  | 
  18  | import { test, expect } from '@playwright/test';
  19  | import { LoginPage } from '../pages/LoginPage';
  20  | import { USERS, SEED } from '../fixtures/users';
  21  | 
  22  | const CUSTOMER = USERS.customer1;
  23  | const RESTAURANT = USERS.restaurant1;
  24  | const SHIPPER = USERS.shipper2;
  25  | const ADMIN = USERS.admin1;
  26  | 
  27  | // ─── Helpers ───
  28  | async function loginAs(page: any, user: { username: string; password: string }) {
  29  |   const login = new LoginPage(page);
  30  |   const url = await login.login(user.username, user.password);
  31  |   if (url.includes('/Home/Error') || url.includes('/Home/Login')) {
  32  |     await page.waitForTimeout(2000);
  33  |     for (let retry = 0; retry < 2; retry++) {
  34  |       try {
  35  |         await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 20_000 });
  36  |         if (!page.url().includes('/Home/Login')) break;
  37  |       } catch { await page.waitForTimeout(1000); }
  38  |     }
  39  |   }
  40  | }
  41  | 
  42  | // ─── TEST SUITE 1: Shipper — QR Code trên OrderDetail ───
  43  | test.describe('🚚 Shipper — QR Code Display', () => {
  44  | 
  45  |   test('[TC-6.1] Shipper OrderDetail — QR code image load', async ({ page }) => {
  46  |     await loginAs(page, SHIPPER);
  47  |     // Vào QRDelivery page để xem QR
  48  |     await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  49  |     await page.waitForTimeout(3000);
  50  | 
  51  |     // Kiểm tra QR images có load không
  52  |     const qrImages = page.locator('img[alt*="QR"]');
  53  |     const qrCount = await qrImages.count();
  54  |     console.log(`📸 QR images found: ${qrCount}`);
  55  | 
  56  |     if (qrCount > 0) {
  57  |       // Kiểm tra ảnh QR không bị lỗi
  58  |       for (let i = 0; i < Math.min(qrCount, 3); i++) {
  59  |         const img = qrImages.nth(i);
  60  |         const src = await img.getAttribute('src');
  61  |         console.log(`  QR #${i}: ${src?.substring(0, 80)}...`);
  62  |         // Kiểm tra ảnh load thành công (naturalWidth > 0)
  63  |         const valid = await img.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0);
  64  |         if (!valid) console.log(`  ⚠️ QR #${i} failed to load, checking fallback...`);
  65  |       }
  66  |     } else {
  67  |       console.log('ℹ️ No QR orders found — shipper may have no pending orders');
  68  |     }
  69  |   });
  70  | 
  71  |   test('[TC-6.2] QRDelivery page — tab filter hoạt động (Chờ giao / Đang giao / Hoàn thành)', async ({ page }) => {
  72  |     await loginAs(page, SHIPPER);
  73  |     await page.goto('/Shipper/QRDelivery', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  74  |     await page.waitForTimeout(3000);
  75  | 
  76  |     // Kiểm tra tab buttons
  77  |     const tabBtns = page.locator('.qr-tab-btn');
  78  |     const tabCount = await tabBtns.count();
  79  |     console.log(`📋 QR tabs: ${tabCount}`);
  80  | 
  81  |     if (tabCount > 0) {
  82  |       // Click từng tab và kiểm tra content thay đổi
  83  |       for (let i = 0; i < tabCount; i++) {
  84  |         const tabText = await tabBtns.nth(i).textContent();
> 85  |         await tabBtns.nth(i).click();
      |                              ^ TimeoutError: locator.click: Timeout 30000ms exceeded.
  86  |         await page.waitForTimeout(500);
  87  |         console.log(`  Tab ${i}: "${tabText?.trim()}" — clicked`);
  88  |       }
  89  |     }
  90  |   });
  91  | 
  92  |   test('[TC-6.3] OrderDetail page — QR glassmorphism card hiển thị', async ({ page }) => {
  93  |     await loginAs(page, SHIPPER);
  94  |     await page.goto('/Shipper', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  95  |     await page.waitForTimeout(3000);
  96  | 
  97  |     // Vào order detail đầu tiên
  98  |     const detailLinks = page.locator('a[href*="/Shipper/OrderDetail/"]');
  99  |     const linkCount = await detailLinks.count();
  100 |     if (linkCount > 0) {
  101 |       await detailLinks.first().click();
  102 |       await page.waitForLoadState('networkidle');
  103 |       await page.waitForTimeout(2000);
  104 | 
  105 |       // Check QR card với glassmorphism class
  106 |       const qrCard = page.locator('.card:has(.fa-qrcode)').first();
  107 |       const cardVisible = await qrCard.isVisible().catch(() => false);
  108 |       console.log(`🃏 QR card visible: ${cardVisible}`);
  109 | 
  110 |       if (cardVisible) {
  111 |         const qrImg = qrCard.locator('img[alt*="QR"]');
  112 |         const imgLoaded = await qrImg.evaluate((el: HTMLImageElement) => el.complete && el.naturalWidth > 0).catch(() => false);
  113 |         console.log(`📸 QR image loaded: ${imgLoaded}`);
  114 | 
  115 |         // Kiểm tra nút tải QR
  116 |         const downloadBtn = qrCard.locator('a:has-text("Tải QR")');
  117 |         console.log(`⬇️ Download QR btn: ${await downloadBtn.isVisible().catch(() => false)}`);
  118 |       }
  119 |     } else {
  120 |       console.log('ℹ️ No order detail links — skip');
  121 |     }
  122 |   });
  123 | });
  124 | 
  125 | // ─── TEST SUITE 2: Merchant — html5-qrcode Scanner ───
  126 | test.describe('🏪 Merchant — QR Scanner', () => {
  127 | 
  128 |   test('[TC-6.4] Merchant scan page — html5-qrcode library load', async ({ page }) => {
  129 |     await loginAs(page, RESTAURANT);
  130 |     await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  131 |     await page.waitForTimeout(3000);
  132 | 
  133 |     // Kiểm tra scanner container
  134 |     const scannerDiv = page.locator('#qr-reader');
  135 |     const scannerExists = await scannerDiv.count();
  136 |     console.log(`📷 QR-reader container: ${scannerExists > 0}`);
  137 | 
  138 |     // Kiểm tra camera controls
  139 |     const startBtn = page.locator('#btnStartScan');
  140 |     const stopBtn = page.locator('#btnStopScan');
  141 |     const switchBtn = page.locator('#btnSwitchCamera');
  142 |     console.log(`  Start btn: ${await startBtn.isVisible().catch(() => false)}`);
  143 |     console.log(`  Stop btn: ${await stopBtn.isVisible().catch(() => false)}`);
  144 |     console.log(`  Switch cam: ${await switchBtn.isVisible().catch(() => false)}`);
  145 | 
  146 |     // Kiểm tra scan history section
  147 |     const historyContainer = page.locator('#scanHistory');
  148 |     await expect(historyContainer).toBeVisible({ timeout: 5_000 });
  149 |     const historyText = await historyContainer.textContent();
  150 |     console.log(`📋 Scan history: "${historyText?.substring(0, 50)}"`);
  151 |   });
  152 | 
  153 |   test('[TC-6.5] Merchant scan page — API call khi scan thành công', async ({ page }) => {
  154 |     await loginAs(page, RESTAURANT);
  155 |     await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  156 |     await page.waitForTimeout(2000);
  157 | 
  158 |     // Verify API endpoint tồn tại
  159 |     const apiResponse = await page.request.post('/EDelivery/ConfirmScan', {
  160 |       data: { token: 'invalid-token-test' },
  161 |       headers: { 'X-Requested-With': 'XMLHttpRequest' }
  162 |     });
  163 |     const responseJson = await apiResponse.json();
  164 |     console.log(`📡 API response: ${JSON.stringify(responseJson)}`);
  165 |     expect(responseJson.success).toBe(false); // Token invalid
  166 |     expect(responseJson.message).toContain('Mã QR');
  167 |   });
  168 | 
  169 |   test('[TC-6.6] Scan history — localStorage lưu và hiển thị', async ({ page }) => {
  170 |     await loginAs(page, RESTAURANT);
  171 |     await page.goto('/EDelivery/MerchantScan', { waitUntil: 'domcontentloaded', timeout: 30_000 });
  172 |     await page.waitForTimeout(2000);
  173 | 
  174 |     // Kiểm tra localStorage
  175 |     const history = await page.evaluate(() => localStorage.getItem('restaurantScanHistory'));
  176 |     console.log(`📦 localStorage scanHistory: ${history ? history.substring(0, 100) : 'null'}`);
  177 | 
  178 |     // Xóa history
  179 |     await page.evaluate(() => localStorage.removeItem('restaurantScanHistory'));
  180 |     const afterClear = await page.evaluate(() => localStorage.getItem('restaurantScanHistory'));
  181 |     console.log(`🗑️ After clear: ${afterClear}`);
  182 |   });
  183 | });
  184 | 
  185 | // ─── TEST SUITE 3: Customer — Real-time Notification ───
```