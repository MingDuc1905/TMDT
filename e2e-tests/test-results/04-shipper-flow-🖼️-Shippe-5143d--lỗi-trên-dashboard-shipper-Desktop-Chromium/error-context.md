# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🖼️ Shipper Visual Checks >> [TC-4.14] Console không có lỗi trên dashboard shipper
- Location: tests\04-shipper-flow.spec.ts:265:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 0
Received: 1
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
  177 | 
  178 | // ─── TEST SUITE 3: Ví tiền & Thu nhập ───
  179 | test.describe('💰 Ví tiền & Thu nhập Shipper', () => {
  180 | 
  181 |   test('[TC-4.9] Trang ví tiền load - số dư hiển thị', async ({ page }) => {
  182 |     await loginAsShipper(page);
  183 |     const shipper = new ShipperPage(page);
  184 | 
  185 |     await shipper.gotoWallet();
  186 |     await page.waitForLoadState('networkidle');
  187 | 
  188 |     // Kiểm tra số dư
  189 |     const balance = await shipper.getWalletBalance();
  190 |     console.log(`💰 Số dư ví: ${balance}`);
  191 | 
  192 |     // Kiểm tra các giao dịch
  193 |     const transactionRows = page.locator('table tbody tr, .transaction-item');
  194 |     const txCount = await transactionRows.count().catch(() => 0);
  195 |     console.log(`📋 Giao dịch: ${txCount}`);
  196 |   });
  197 | 
  198 |   test('[TC-4.10] Trang thu nhập - thống kê hiển thị', async ({ page }) => {
  199 |     await loginAsShipper(page);
  200 |     const shipper = new ShipperPage(page);
  201 | 
  202 |     await shipper.gotoIncome();
  203 |     await page.waitForLoadState('networkidle');
  204 | 
  205 |     // Kiểm tra thống kê thu nhập
  206 |     const incomeStats = page.locator('.card-header, [class*="stat"], [class*="income"]');
  207 |     const statCount = await incomeStats.count();
  208 |     console.log(`📊 Thu nhập stats: ${statCount}`);
  209 |     expect(statCount).toBeGreaterThan(0);
  210 | 
  211 |     // Lấy text thống kê
  212 |     for (let i = 0; i < Math.min(statCount, 4); i++) {
  213 |       const text = await incomeStats.nth(i).textContent();
  214 |       console.log(`  Stat ${i}: ${text?.trim()}`);
  215 |     }
  216 |   });
  217 | 
  218 |   test('[TC-4.11] Trang lịch sử giao hàng load', async ({ page }) => {
  219 |     await loginAsShipper(page);
  220 |     const shipper = new ShipperPage(page);
  221 | 
  222 |     await shipper.gotoHistory();
  223 |     await page.waitForLoadState('networkidle');
  224 | 
  225 |     const bodyText = await page.locator('body').textContent();
  226 |     expect(bodyText).toBeTruthy();
  227 |     console.log('✅ Lịch sử giao hàng load');
  228 | 
  229 |     // Kiểm tra bảng lịch sử
  230 |     const tableRows = page.locator('table tbody tr');
  231 |     const rowCount = await tableRows.count().catch(() => 0);
  232 |     console.log(`📋 Lịch sử: ${rowCount} dòng`);
  233 |   });
  234 | 
  235 |   test('[TC-4.12] So sánh số dư ví trước và sau khi giao hàng (nếu có)', async ({ page }) => {
  236 |     await loginAsShipper(page);
  237 | 
  238 |     // Lấy số dư hiện tại
  239 |     const shipper = new ShipperPage(page);
  240 |     await shipper.gotoWallet();
  241 |     await page.waitForLoadState('networkidle');
  242 |     const balanceText = await shipper.getWalletBalance();
  243 |     console.log(`💰 Số dư hiện tại: ${balanceText}`);
  244 |   });
  245 | });
  246 | 
  247 | // ─── TEST SUITE 4: Visual & Console ───
  248 | test.describe('🖼️ Shipper Visual Checks', () => {
  249 | 
  250 |   test('[TC-4.13] Tất cả ảnh trên dashboard shipper không vỡ', async ({ page }) => {
  251 |     await loginAsShipper(page);
  252 | 
  253 |     const imgResult = await page.evaluate(() => {
  254 |       const imgs = Array.from(document.querySelectorAll('img'));
  255 |       let broken = 0;
  256 |       imgs.forEach((img) => {
  257 |         if (!img.complete || img.naturalWidth === 0) broken++;
  258 |       });
  259 |       return { total: imgs.length, broken };
  260 |     });
  261 |     console.log(`📸 Ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
  262 |     expect(imgResult.broken).toBe(0);
  263 |   });
  264 | 
  265 |   test('[TC-4.14] Console không có lỗi trên dashboard shipper', async ({ page }) => {
  266 |     const errors: string[] = [];
  267 |     page.on('console', (msg) => {
  268 |       if (msg.type() === 'error') errors.push(msg.text());
  269 |     });
  270 | 
  271 |     await loginAsShipper(page);
  272 |     await page.waitForTimeout(3000);
  273 | 
  274 |     if (errors.length > 0) {
  275 |       console.log(`❌ Console errors: ${errors.join(' | ')}`);
  276 |     }
> 277 |     expect(errors.length).toBe(0);
      |                           ^ Error: expect(received).toBe(expected) // Object.is equality
  278 |   });
  279 | 
  280 |   test('[TC-4.15] Desktop layout - không bị overflow', async ({ page }) => {
  281 |     await loginAsShipper(page);
  282 | 
  283 |     const hasOverflow = await page.evaluate(() => {
  284 |       return document.documentElement.scrollWidth > document.documentElement.clientWidth;
  285 |     });
  286 |     expect(hasOverflow).toBe(false);
  287 |     console.log(`📐 Horizontal overflow: ${hasOverflow}`);
  288 |   });
  289 | });
  290 | 
```