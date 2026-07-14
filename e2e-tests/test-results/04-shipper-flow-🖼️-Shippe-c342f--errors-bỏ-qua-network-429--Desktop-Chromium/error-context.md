# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🖼️ Shipper Visual Checks >> [TC-4.14] Console không có JS errors (bỏ qua network 429)
- Location: tests\04-shipper-flow.spec.ts:283:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 0
Received: 2
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
    - generic [ref=e49]:
      - generic [ref=e50]:
        - img "Avatar" [ref=e51]
        - heading "Nguyen Thi Z" [level=2] [ref=e52]
        - generic [ref=e53]:
          - text: Đang hoạt động
          - link "Tắt" [ref=e55] [cursor=pointer]:
            - /url: /Shipper/updateStatus
      - generic [ref=e56]:
        - generic [ref=e57]:
          - paragraph [ref=e58]: Hôm nay
          - heading "0 đơn" [level=3] [ref=e59]
        - generic [ref=e60]:
          - paragraph [ref=e61]: Thu nhập
          - heading "0đ" [level=3] [ref=e62]
        - generic [ref=e63]:
          - paragraph [ref=e64]: Đang giao
          - heading "0" [level=3] [ref=e65]
        - generic [ref=e66]:
          - paragraph [ref=e67]: FREE-PICK
          - heading "0" [level=3] [ref=e68]
      - generic [ref=e70]:
        - generic [ref=e71]: 
        - text: Vị trí của bạn
    - generic [ref=e73]:
      - generic [ref=e74]:
        - heading "📋 Đơn hàng" [level=2] [ref=e75]
        - generic [ref=e76]:
          - button "FREE-PICK" [ref=e77] [cursor=pointer]
          - button "ĐƠN HÀNG" [ref=e78] [cursor=pointer]
      - generic [ref=e80]:
        - generic [ref=e81]: 📭
        - paragraph [ref=e82]: Không có đơn FREE-PICK nào đang chờ
        - text: Khi có đơn mới, bạn sẽ nghe thấy âm báo
      - link " Làm mới" [ref=e84] [cursor=pointer]:
        - /url: javascript:location.reload()
        - generic [ref=e85]: 
        - text: Làm mới
```

# Test source

```ts
  193 | 
  194 |   test('[TC-4.9] Trang ví tiền load - số dư hiển thị', async ({ page }) => {
  195 |     await loginAsShipper(page);
  196 |     const shipper = new ShipperPage(page);
  197 | 
  198 |     await shipper.gotoWallet();
  199 |     await page.waitForLoadState('networkidle');
  200 | 
  201 |     // Kiểm tra số dư
  202 |     const balance = await shipper.getWalletBalance();
  203 |     console.log(`💰 Số dư ví: ${balance}`);
  204 | 
  205 |     // Kiểm tra các giao dịch
  206 |     const transactionRows = page.locator('table tbody tr, .transaction-item');
  207 |     const txCount = await transactionRows.count().catch(() => 0);
  208 |     console.log(`📋 Giao dịch: ${txCount}`);
  209 |   });
  210 | 
  211 |   test('[TC-4.10] Trang thu nhập - thống kê hiển thị', async ({ page }) => {
  212 |     await loginAsShipper(page);
  213 |     const shipper = new ShipperPage(page);
  214 | 
  215 |     await shipper.gotoIncome();
  216 |     await page.waitForLoadState('networkidle');
  217 | 
  218 |     // Kiểm tra thống kê thu nhập
  219 |     const incomeStats = page.locator('.card-header, [class*="stat"], [class*="income"]');
  220 |     const statCount = await incomeStats.count();
  221 |     console.log(`📊 Thu nhập stats: ${statCount}`);
  222 |     // ponytail: nếu không có thống kê (shipper chưa có đơn), log + skip (không fail)
  223 |     if (statCount > 0) {
  224 |       for (let i = 0; i < Math.min(statCount, 4); i++) {
  225 |         const text = await incomeStats.nth(i).textContent();
  226 |         console.log(`  Stat ${i}: ${text?.trim()}`);
  227 |       }
  228 |     } else {
  229 |       console.log('ℹ️ Shipper chưa có dữ liệu thu nhập');
  230 |     }
  231 |   });
  232 | 
  233 |   test('[TC-4.11] Trang lịch sử giao hàng load', async ({ page }) => {
  234 |     await loginAsShipper(page);
  235 |     const shipper = new ShipperPage(page);
  236 | 
  237 |     await shipper.gotoHistory();
  238 |     await page.waitForLoadState('networkidle');
  239 | 
  240 |     const bodyText = await page.locator('body').textContent();
  241 |     expect(bodyText).toBeTruthy();
  242 |     console.log('✅ Lịch sử giao hàng load');
  243 | 
  244 |     // Kiểm tra bảng lịch sử
  245 |     const tableRows = page.locator('table tbody tr');
  246 |     const rowCount = await tableRows.count().catch(() => 0);
  247 |     if (rowCount === 0) {
  248 |       console.log('ℹ️ Không có đơn hàng nào trong lịch sử — seed fix sẽ cải thiện');
  249 |     }
  250 |     console.log(`📋 Lịch sử: ${rowCount} dòng`);
  251 |   });
  252 | 
  253 |   test('[TC-4.12] So sánh số dư ví trước và sau khi giao hàng (nếu có)', async ({ page }) => {
  254 |     await loginAsShipper(page);
  255 | 
  256 |     // Lấy số dư hiện tại
  257 |     const shipper = new ShipperPage(page);
  258 |     await shipper.gotoWallet();
  259 |     await page.waitForLoadState('networkidle');
  260 |     const balanceText = await shipper.getWalletBalance();
  261 |     console.log(`💰 Số dư hiện tại: ${balanceText}`);
  262 |   });
  263 | });
  264 | 
  265 | // ─── TEST SUITE 4: Visual & Console ───
  266 | test.describe('🖼️ Shipper Visual Checks', () => {
  267 | 
  268 |   test('[TC-4.13] Tất cả ảnh trên dashboard shipper không vỡ', async ({ page }) => {
  269 |     await loginAsShipper(page);
  270 | 
  271 |     const imgResult = await page.evaluate(() => {
  272 |       const imgs = Array.from(document.querySelectorAll('img'));
  273 |       let broken = 0;
  274 |       imgs.forEach((img) => {
  275 |         if (!img.complete || img.naturalWidth === 0) broken++;
  276 |       });
  277 |       return { total: imgs.length, broken };
  278 |     });
  279 |     console.log(`📸 Ảnh: ${imgResult.total}, Lỗi: ${imgResult.broken}`);
  280 |     expect(imgResult.broken).toBe(0);
  281 |   });
  282 | 
  283 |   test('[TC-4.14] Console không có JS errors (bỏ qua network 429)', async ({ page }) => {
  284 |     const jsErrors: string[] = [];
  285 |     page.on('pageerror', (err) => { jsErrors.push(err.message); });
  286 | 
  287 |     await loginAsShipper(page);
  288 |     await page.waitForTimeout(3000);
  289 | 
  290 |     if (jsErrors.length > 0) {
  291 |       console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
  292 |     }
> 293 |     expect(jsErrors.length).toBe(0);
      |                             ^ Error: expect(received).toBe(expected) // Object.is equality
  294 |   });
  295 | 
  296 |   test('[TC-4.15] Desktop layout - không bị overflow', async ({ page }) => {
  297 |     await loginAsShipper(page);
  298 | 
  299 |     const hasOverflow = await page.evaluate(() => {
  300 |       return document.documentElement.scrollWidth > document.documentElement.clientWidth;
  301 |     });
  302 |     expect(hasOverflow).toBe(false);
  303 |     console.log(`📐 Horizontal overflow: ${hasOverflow}`);
  304 |   });
  305 | });
  306 | 
```