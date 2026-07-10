# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 04-shipper-flow.spec.ts >> 🚚 Dashboard Shipper - Tabs & Navigation >> [TC-4.3] Tab FREE-PICK - danh sách đơn chờ nhận load
- Location: tests\04-shipper-flow.spec.ts:58:7

# Error details

```
TimeoutError: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('#orders-all-tab')

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
  1   | import { Page, Locator } from '@playwright/test';
  2   | import { BasePage } from './BasePage';
  3   | 
  4   | /**
  5   |  * ShipperPage — Page Object cho dashboard Shipper Fastship
  6   |  * URL: /Shipper/*
  7   |  *
  8   |  * Kiểm tra: FREE-PICK, danh sách đơn, nhận đơn, cập nhật trạng thái, ví tiền
  9   |  */
  10  | export class ShipperPage extends BasePage {
  11  |   readonly freepickTab: Locator;
  12  |   readonly orderTab: Locator;
  13  |   readonly orderTable: Locator;
  14  |   readonly orderRows: Locator;
  15  |   readonly acceptOrderBtn: Locator;
  16  |   readonly detailLink: Locator;
  17  |   readonly walletLink: Locator;
  18  |   readonly incomeLink: Locator;
  19  |   readonly historyLink: Locator;
  20  |   readonly refreshBtn: Locator;
  21  |   readonly liveMap: Locator;
  22  | 
  23  |   constructor(page: Page) {
  24  |     super(page);
  25  | 
  26  |     this.freepickTab = page.locator('#orders-all-tab');
  27  |     this.orderTab = page.locator('#orders-paid-tab');
  28  |     this.orderTable = page.locator('.table-responsive table');
  29  |     this.orderRows = page.locator('.table-responsive tbody tr');
  30  |     this.acceptOrderBtn = page.locator('a[href*="/Shipper/OrderDetail/"]');
  31  |     this.detailLink = page.locator('a[href*="/Shipper/OrderDetail/"]');
  32  |     this.walletLink = page.locator('a[href*="/Shipper/ViTien"]');
  33  |     this.incomeLink = page.locator('a[href*="/Shipper/ThuNhap"]');
  34  |     this.historyLink = page.locator('a[href*="/Shipper/LichSu"]');
  35  |     this.refreshBtn = page.locator('a:has-text("Làm mới")');
  36  |     this.liveMap = page.locator('#shipper-map');
  37  |   }
  38  | 
  39  |   /** Mở dashboard shipper */
  40  |   async gotoDashboard() {
  41  |     await this.goto('/Shipper');
  42  |   }
  43  | 
  44  |   /** Mở tab FREE-PICK */
  45  |   async openFreepickTab() {
> 46  |     await this.freepickTab.click();
      |                            ^ TimeoutError: locator.click: Timeout 30000ms exceeded.
  47  |     await this.page.waitForTimeout(1000);
  48  |   }
  49  | 
  50  |   /** Mở tab ĐƠN HÀNG */
  51  |   async openOrderTab() {
  52  |     await this.orderTab.click();
  53  |     await this.page.waitForTimeout(1000);
  54  |   }
  55  | 
  56  |   /** Mở trang ví tiền */
  57  |   async gotoWallet() {
  58  |     await this.walletLink.click();
  59  |     await this.page.waitForLoadState('networkidle');
  60  |   }
  61  | 
  62  |   /** Mở trang thu nhập */
  63  |   async gotoIncome() {
  64  |     await this.goto('/Shipper/ThuNhap');
  65  |   }
  66  | 
  67  |   /** Mở trang lịch sử */
  68  |   async gotoHistory() {
  69  |     await this.goto('/Shipper/LichSu');
  70  |   }
  71  | 
  72  |   /** Click nút Chấp nhận đơn đầu tiên */
  73  |   async acceptFirstOrder() {
  74  |     await this.detailLink.first().click();
  75  |     await this.page.waitForLoadState('networkidle');
  76  |     await this.page.waitForTimeout(2000);
  77  |   }
  78  | 
  79  |   /** Lấy text trạng thái của đơn đầu tiên */
  80  |   async getFirstOrderStatus(): Promise<string | null> {
  81  |     return await this.orderRows.first().locator('td').nth(6).textContent();
  82  |   }
  83  | 
  84  |   /** Lấy số dư ví */
  85  |   async getWalletBalance(): Promise<string | null> {
  86  |     try {
  87  |       return await this.page.locator('.vi-tien-balance, [class*="balance"]').first().textContent();
  88  |     } catch {
  89  |       return null;
  90  |     }
  91  |   }
  92  | 
  93  |   /** Đếm số đơn trong bảng */
  94  |   async getOrderCount(): Promise<number> {
  95  |     return await this.orderRows.count();
  96  |   }
  97  | 
  98  |   /** Kiểm tra map có hiển thị không */
  99  |   async isMapVisible(): Promise<boolean> {
  100 |     try {
  101 |       return await this.liveMap.isVisible({ timeout: 3_000 });
  102 |     } catch {
  103 |       return false;
  104 |     }
  105 |   }
  106 | }
  107 | 
```