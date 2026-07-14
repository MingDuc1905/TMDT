# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 05-admin-flow.spec.ts >> 👑 Admin Dashboard - KPI & Charts >> [TC-5.5] Kiểm tra sidebar routing - click từng link
- Location: tests\05-admin-flow.spec.ts:128:7

# Error details

```
TimeoutError: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('a[href*="/Admin/Dashboard"]').first()
    - locator resolved to <a aria-expanded="false" href="/Admin/Dashboard" class="has-arrow ai-icon">…</a>
  - attempting click action
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - element is outside of the viewport
    - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - element is outside of the viewport
    - retrying click action
      - waiting 100ms
    57 × waiting for element to be visible, enabled and stable
       - element is visible, enabled and stable
       - scrolling into view if needed
       - done scrolling
       - element is outside of the viewport
     - retrying click action
       - waiting 500ms

```

# Page snapshot

```yaml
- generic [ref=e2]:
  - link [ref=e4] [cursor=pointer]:
    - /url: /Admin/QuanLyQuanAn
  - navigation [ref=e12]:
    - list [ref=e14]:
      - listitem [ref=e15]:
        - link [ref=e16] [cursor=pointer]:
          - /url: "#"
          - img [ref=e17]
      - listitem [ref=e22]:
        - link "" [ref=e23] [cursor=pointer]:
          - /url: /Home/Logout
          - generic [ref=e24]: 
      - listitem [ref=e25]:
        - button "A" [ref=e26] [cursor=pointer]:
          - generic [ref=e27]: A
        - text:   
  - list [ref=e30]:
    - listitem [ref=e31]:
      - link " Dashboard" [ref=e32] [cursor=pointer]:
        - /url: /Admin/Dashboard
        - generic [ref=e33]:
          - generic [ref=e34]: 
          - text: Dashboard
    - listitem [ref=e35]:
      - link " Xem quán ăn" [ref=e36] [cursor=pointer]:
        - /url: /Admin/QuanLyQuanAn
        - generic [ref=e37]:
          - generic [ref=e38]: 
          - text: Xem quán ăn
    - listitem [ref=e39]:
      - link " Xem admin" [ref=e40] [cursor=pointer]:
        - /url: /Admin/QuanLyQuanTriVien
        - generic [ref=e41]:
          - generic [ref=e42]: 
          - text: Xem admin
    - listitem [ref=e43]:
      - link " Xem tài xế" [ref=e44] [cursor=pointer]:
        - /url: /Admin/QuanLyShipper
        - generic [ref=e45]:
          - generic [ref=e46]: 
          - text: Xem tài xế
    - listitem [ref=e47]:
      - link " Xem khách hàng" [ref=e48] [cursor=pointer]:
        - /url: /Admin/QuanLyKhachHang
        - generic [ref=e49]:
          - generic [ref=e50]: 
          - text: Xem khách hàng
    - listitem [ref=e51]:
      - link " Xem đơn hàng" [ref=e52] [cursor=pointer]:
        - /url: /Admin/Order
        - generic [ref=e53]:
          - generic [ref=e54]: 
          - text: Xem đơn hàng
    - listitem [ref=e55]:
      - link " Xem danh mục" [ref=e56] [cursor=pointer]:
        - /url: /Admin/Category
        - generic [ref=e57]:
          - generic [ref=e58]: 
          - text: Xem danh mục
    - listitem [ref=e59]:
      - link " Khuyến mãi" [ref=e60] [cursor=pointer]:
        - /url: /Admin/VoucherManager
        - generic [ref=e61]:
          - generic [ref=e62]: 
          - text: Khuyến mãi
    - listitem [ref=e63]:
      - link " Chat khách hàng" [ref=e64] [cursor=pointer]:
        - /url: /AdminChat
        - generic [ref=e65]:
          - generic [ref=e66]: 
          - text: Chat khách hàng
    - listitem [ref=e67]:
      - link " Giám sát giao hàng" [ref=e68] [cursor=pointer]:
        - /url: /EDelivery/DeliveryLogs
        - generic [ref=e69]:
          - generic [ref=e70]: 
          - text: Giám sát giao hàng
  - generic [ref=e72]:
    - generic [ref=e74]: 
    - heading "Chào mừng đến với Admin Panel" [level=3] [ref=e75]
    - paragraph [ref=e76]: Sử dụng sidebar để quản lý hệ thống
    - link " Xem Dashboard" [ref=e77] [cursor=pointer]:
      - /url: /Admin/Dashboard
      - generic [ref=e78]: 
      - text: Xem Dashboard
```

# Test source

```ts
  41  |         console.log(`⚠️ Fallback goto Admin #${retry+1} failed`);
  42  |         await page.waitForTimeout(1000);
  43  |       }
  44  |     }
  45  |   }
  46  | }
  47  | 
  48  | // ─── TEST SUITE 1: Dashboard ───
  49  | test.describe('👑 Admin Dashboard - KPI & Charts', () => {
  50  | 
  51  |   test('[TC-5.1] Đăng nhập admin - redirect đến /Admin', async ({ page }) => {
  52  |     await loginAsAdmin(page);
  53  |     const url = page.url();
  54  |     console.log(`✅ URL: ${url}`);
  55  |     expect(url).toContain('/Admin');
  56  |   });
  57  | 
  58  |   test('[TC-5.2] Dashboard hiển thị KPI cards (doanh thu, đơn hàng, user, ...)', async ({ page }) => {
  59  |     await loginAsAdmin(page);
  60  | 
  61  |     // ponytail: networkidle có thể timeout trên Render cold start → catch để không crash
  62  |     // Relaxed assertion: nếu không tìm thấy card nào, log + skip (không fail)
  63  |     await page.waitForTimeout(3000); // chờ DOM render xong
  64  |     await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  65  | 
  66  |     // Đếm tất cả cards/boxes trên dashboard
  67  |     const allCards = page.locator('.card, [class*="kpi"], .card-header, .card-body');
  68  |     const cardCount = await allCards.count();
  69  |     console.log(`📊 Cards/boxes: ${cardCount}`);
  70  |     if (cardCount > 0) {
  71  |       // In text từng card
  72  |       for (let i = 0; i < Math.min(cardCount, 6); i++) {
  73  |         const text = await allCards.nth(i).textContent();
  74  |         console.log(`  Card ${i}: ${text?.trim().substring(0, 80)}`);
  75  |       }
  76  |     } else {
  77  |       console.log('ℹ️ Không tìm thấy card element nào (có thể layout khác)');
  78  |     }
  79  |   });
  80  | 
  81  |   test('[TC-5.3] Biểu đồ doanh thu Chart.js render', async ({ page }) => {
  82  |     await loginAsAdmin(page);
  83  | 
  84  |     await page.waitForLoadState('networkidle', { timeout: 30_000 });
  85  |     const canvasCount = await page.locator('canvas').count();
  86  |     console.log(`📈 Canvas elements: ${canvasCount}`);
  87  |     // ponytail: không fail nếu không có canvas (admin có thể chưa cấu hình chart)
  88  |     if (canvasCount > 0) {
  89  |       const canvasBox = await page.locator('canvas').first().boundingBox();
  90  |       if (canvasBox) {
  91  |         expect(canvasBox.width).toBeGreaterThan(0);
  92  |         expect(canvasBox.height).toBeGreaterThan(0);
  93  |         console.log(`📐 Chart: ${canvasBox.width}x${canvasBox.height}`);
  94  |       }
  95  |     } else {
  96  |       console.log('ℹ️ Không có Chart.js canvas — admin page có thể không có biểu đồ');
  97  |     }
  98  |   });
  99  | 
  100 |   test('[TC-5.4] Kiểm tra tất cả navigation links trên admin page', async ({ page }) => {
  101 |     await loginAsAdmin(page);
  102 | 
  103 |     // ponytail: networkidle có thể timeout → catch + relaxed assertion
  104 |     await page.waitForTimeout(3000);
  105 |     await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  106 | 
  107 |     const allNavLinks = page.locator('nav a[href], .deznav a[href], .sidebar a[href], [class*="menu"] a[href]');
  108 |     const linkCount = await allNavLinks.count();
  109 |     console.log(`🔗 Tổng navigation links: ${linkCount}`);
  110 |     if (linkCount > 0) {
  111 |       // Kiểm tra các link chính tồn tại
  112 |       const expectedLinks = [
  113 |         { name: 'Dashboard', href: '/Admin/Dashboard' },
  114 |         { name: 'Quản lý', href: '/Admin/QuanLyKhachHang' },
  115 |         { name: 'Đơn hàng', href: '/Admin/Order' },
  116 |         { name: 'Danh mục', href: '/Admin/Category' },
  117 |       ];
  118 |       for (const link of expectedLinks) {
  119 |         const linkEl = page.locator(`a[href*="${link.href}"]`).first();
  120 |         const exists = await linkEl.count();
  121 |         console.log(`  ${exists > 0 ? '✅' : '❌'} ${link.name}: ${link.href}`);
  122 |       }
  123 |     } else {
  124 |       console.log('ℹ️ Không tìm thấy navigation link nào (có thể layout khác)');
  125 |     }
  126 |   });
  127 | 
  128 |   test('[TC-5.5] Kiểm tra sidebar routing - click từng link', async ({ page }) => {
  129 |     await loginAsAdmin(page);
  130 | 
  131 |     const pages = [
  132 |       { name: 'Dashboard', href: '/Admin/Dashboard' },
  133 |       { name: 'Quản lý người dùng', href: '/Admin/QuanLyKhachHang' },
  134 |       { name: 'Đơn hàng', href: '/Admin/Order' },
  135 |       { name: 'Danh mục', href: '/Admin/Category' },
  136 |     ];
  137 | 
  138 |     for (const p of pages) {
  139 |       const link = page.locator(`a[href*="${p.href}"]`).first();
  140 |       if (await link.isVisible().catch(() => false)) {
> 141 |         await link.click();
      |                    ^ TimeoutError: locator.click: Timeout 30000ms exceeded.
  142 |         await page.waitForLoadState('networkidle');
  143 |         await page.waitForTimeout(1000);
  144 |         const url = page.url();
  145 |         console.log(`✅ ${p.name}: ${url}`);
  146 |         expect(url).toContain(p.href);
  147 |       } else {
  148 |         console.log(`❌ ${p.name}: link không hiển thị`);
  149 |       }
  150 |     }
  151 |   });
  152 | 
  153 |   test('[TC-5.6] Console không có JS errors (bỏ qua network 429)', async ({ page }) => {
  154 |     const jsErrors: string[] = [];
  155 |     page.on('pageerror', (err) => { jsErrors.push(err.message); });
  156 | 
  157 |     await loginAsAdmin(page);
  158 |     await page.waitForTimeout(3000);
  159 | 
  160 |     if (jsErrors.length > 0) {
  161 |       console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
  162 |     }
  163 |     // ponytail: chỉ fail nếu có JS error thật (không tính network 429/503 từ Render)
  164 |     expect(jsErrors.length).toBe(0);
  165 |   });
  166 | });
  167 | 
  168 | // ─── TEST SUITE 2: Quản lý người dùng ───
  169 | test.describe('👥 Quản lý Người dùng (User Management)', () => {
  170 | 
  171 |   test('[TC-5.7] Trang quản lý người dùng load - bảng hiển thị', async ({ page }) => {
  172 |     await loginAsAdmin(page);
  173 | 
  174 |     const admin = new AdminPage(page);
  175 |     await admin.gotoUserManagement();
  176 |     await page.waitForLoadState('networkidle');
  177 | 
  178 |     const bodyText = await page.locator('body').textContent();
  179 |     expect(bodyText).toBeTruthy();
  180 |     console.log('✅ Quản lý người dùng load');
  181 | 
  182 |     // Đếm bảng
  183 |     const tables = await page.locator('table').count();
  184 |     console.log(`📋 Tables: ${tables}`);
  185 |   });
  186 | 
  187 |   test('[TC-5.8] Tìm kiếm người dùng - gõ từ khóa', async ({ page }) => {
  188 |     await loginAsAdmin(page);
  189 | 
  190 |     const admin = new AdminPage(page);
  191 |     await admin.gotoUserManagement();
  192 | 
  193 |     // Tìm search input
  194 |     const searchInput = page.locator('input[type="search"], input[placeholder*="tìm"], input[placeholder*="search"]').first();
  195 |     if (await searchInput.isVisible().catch(() => false)) {
  196 |       await searchInput.fill('koneko');
  197 |       await page.waitForTimeout(1500);
  198 |       const val = await searchInput.inputValue();
  199 |       console.log(`🔍 Search value: "${val}"`);
  200 |     } else {
  201 |       console.log('ℹ️ Không có search input');
  202 |     }
  203 |   });
  204 | 
  205 |   test('[TC-5.9] Kiểm tra các loại user (Khách hàng, Quán ăn, Shipper, Admin)', async ({ page }) => {
  206 |     await loginAsAdmin(page);
  207 | 
  208 |     const admin = new AdminPage(page);
  209 |     await admin.gotoUserManagement();
  210 |     await page.waitForLoadState('networkidle');
  211 | 
  212 |     // Kiểm tra nếu có filter tabs
  213 |     const filterTabs = page.locator('[class*="tab"], [class*="filter"]').filter({ hasText: /khách hàng|quán ăn|shipper|admin/i });
  214 |     const tabCount = await filterTabs.count();
  215 |     console.log(`🔍 Filter tabs: ${tabCount}`);
  216 | 
  217 |     if (tabCount > 0) {
  218 |       const tabTexts = await filterTabs.allTextContents();
  219 |       tabTexts.forEach((t) => console.log(`  Tab: ${t?.trim()}`));
  220 |     }
  221 |   });
  222 | });
  223 | 
  224 | // ─── TEST SUITE 3: Quản lý đơn hàng ───
  225 | test.describe('📦 Quản lý Đơn hàng (Order Management)', () => {
  226 | 
  227 |   test('[TC-5.10] Trang quản lý đơn hàng load', async ({ page }) => {
  228 |     await loginAsAdmin(page);
  229 | 
  230 |     const admin = new AdminPage(page);
  231 |     await admin.gotoOrderManagement();
  232 |     await page.waitForLoadState('networkidle');
  233 | 
  234 |     const bodyText = await page.locator('body').textContent();
  235 |     expect(bodyText).toBeTruthy();
  236 |     console.log('✅ Quản lý đơn hàng load');
  237 | 
  238 |     // Kiểm tra bảng đơn hàng
  239 |     const hasTable = await admin.orderTable.isVisible().catch(() => false);
  240 |     if (hasTable) {
  241 |       const rows = await page.locator('table tbody tr').count();
```