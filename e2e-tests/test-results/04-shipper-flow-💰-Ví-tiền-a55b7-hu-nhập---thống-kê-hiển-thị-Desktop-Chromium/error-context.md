# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 💰 Ví tiền & Thu nhập Shipper >> [TC-4.10] Trang thu nhập - thống kê hiển thị
- Location: tests\04-shipper-flow.spec.ts:197:7

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
  153 |     await page.waitForTimeout(2000);
  154 | 
  155 |     const orderRows = page.locator('.table-responsive tbody tr');
  156 |     const rowCount = await orderRows.count();
  157 | 
  158 |     if (rowCount > 0) {
  159 |       // Click vào chi tiết đơn đầu
  160 |       const firstRow = orderRows.first();
  161 |       const firstCellText = await firstRow.locator('td').first().textContent();
  162 |       console.log(`📋 Đơn hàng đầu: ${firstCellText?.trim()}`);
  163 | 
  164 |       // Click vào link chi tiết (nếu có)
  165 |       const detailLink = firstRow.locator('a[href*="OrderDetail"]');
  166 |       if (await detailLink.count() > 0) {
  167 |         await detailLink.first().click();
  168 |         await page.waitForLoadState('networkidle');
  169 |         console.log(`📍 URL: ${page.url()}`);
  170 |       }
  171 |     } else {
  172 |       console.log('ℹ️ Không có đơn nào');
  173 |     }
  174 |   });
  175 | });
  176 | 
  177 | // ─── TEST SUITE 3: Ví tiền & Thu nhập ───
  178 | test.describe('💰 Ví tiền & Thu nhập Shipper', () => {
  179 | 
  180 |   test('[TC-4.9] Trang ví tiền load - số dư hiển thị', async ({ page }) => {
  181 |     await loginAsShipper(page);
  182 |     const shipper = new ShipperPage(page);
  183 | 
  184 |     await shipper.gotoWallet();
  185 |     await page.waitForLoadState('networkidle');
  186 | 
  187 |     // Kiểm tra số dư
  188 |     const balance = await shipper.getWalletBalance();
  189 |     console.log(`💰 Số dư ví: ${balance}`);
  190 | 
  191 |     // Kiểm tra các giao dịch
  192 |     const transactionRows = page.locator('table tbody tr, .transaction-item');
  193 |     const txCount = await transactionRows.count().catch(() => 0);
  194 |     console.log(`📋 Giao dịch: ${txCount}`);
  195 |   });
  196 | 
  197 |   test('[TC-4.10] Trang thu nhập - thống kê hiển thị', async ({ page }) => {
  198 |     await loginAsShipper(page);
  199 |     const shipper = new ShipperPage(page);
  200 | 
  201 |     await shipper.gotoIncome();
  202 |     await page.waitForLoadState('networkidle');
  203 | 
  204 |     // Kiểm tra thống kê thu nhập
  205 |     const incomeStats = page.locator('.card-header, [class*="stat"], [class*="income"]');
  206 |     const statCount = await incomeStats.count();
  207 |     console.log(`📊 Thu nhập stats: ${statCount}`);
> 208 |     expect(statCount).toBeGreaterThan(0);
      |                       ^ Error: expect(received).toBeGreaterThan(expected)
  209 | 
  210 |     // Lấy text thống kê
  211 |     for (let i = 0; i < Math.min(statCount, 4); i++) {
  212 |       const text = await incomeStats.nth(i).textContent();
  213 |       console.log(`  Stat ${i}: ${text?.trim()}`);
  214 |     }
  215 |   });
  216 | 
  217 |   test('[TC-4.11] Trang lịch sử giao hàng load', async ({ page }) => {
  218 |     await loginAsShipper(page);
  219 |     const shipper = new ShipperPage(page);
  220 | 
  221 |     await shipper.gotoHistory();
  222 |     await page.waitForLoadState('networkidle');
  223 | 
  224 |     const bodyText = await page.locator('body').textContent();
  225 |     expect(bodyText).toBeTruthy();
  226 |     console.log('✅ Lịch sử giao hàng load');
  227 | 
  228 |     // Kiểm tra bảng lịch sử
  229 |     const tableRows = page.locator('table tbody tr');
  230 |     const rowCount = await tableRows.count().catch(() => 0);
  231 |     console.log(`📋 Lịch sử: ${rowCount} dòng`);
  232 |   });
  233 | 
  234 |   test('[TC-4.12] So sánh số dư ví trước và sau khi giao hàng (nếu có)', async ({ page }) => {
  235 |     await loginAsShipper(page);
  236 | 
  237 |     // Lấy số dư hiện tại
  238 |     const shipper = new ShipperPage(page);
  239 |     await shipper.gotoWallet();
  240 |     await page.waitForLoadState('networkidle');
  241 |     const balanceText = await shipper.getWalletBalance();
  242 |     console.log(`💰 Số dư hiện tại: ${balanceText}`);
  243 |   });
  244 | });
  245 | 
  246 | // ─── TEST SUITE 4: Visual & Console ───
  247 | test.describe('🖼️ Shipper Visual Checks', () => {
  248 | 
  249 |   test('[TC-4.13] Tất cả ảnh trên dashboard shipper không vỡ', async ({ page }) => {
  250 |     await loginAsShipper(page);
  251 | 
  252 |     const imgResult = await page.evaluate(() => {
  253 |       const imgs = Array.from(document.querySelectorAll('img'));
  254 |       let broken = 0;
  255 |       imgs.forEach((img) => {
  256 |         if (!img.complete || img.naturalWidth === 0) broken++;
  257 |       });
  258 |       return { total: imgs.length, broken };
  259 |     });
  260 |     console.log(`📸 Ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
  261 |     expect(imgResult.broken).toBe(0);
  262 |   });
  263 | 
  264 |   test('[TC-4.14] Console không có lỗi trên dashboard shipper', async ({ page }) => {
  265 |     const errors: string[] = [];
  266 |     page.on('console', (msg) => {
  267 |       if (msg.type() === 'error') errors.push(msg.text());
  268 |     });
  269 | 
  270 |     await loginAsShipper(page);
  271 |     await page.waitForTimeout(3000);
  272 | 
  273 |     if (errors.length > 0) {
  274 |       console.log(`❌ Console errors: ${errors.join(' | ')}`);
  275 |     }
  276 |     expect(errors.length).toBe(0);
  277 |   });
  278 | 
  279 |   test('[TC-4.15] Desktop layout - không bị overflow', async ({ page }) => {
  280 |     await loginAsShipper(page);
  281 | 
  282 |     const hasOverflow = await page.evaluate(() => {
  283 |       return document.documentElement.scrollWidth > document.documentElement.clientWidth;
  284 |     });
  285 |     expect(hasOverflow).toBe(false);
  286 |     console.log(`📐 Horizontal overflow: ${hasOverflow}`);
  287 |   });
  288 | });
  289 | 
```