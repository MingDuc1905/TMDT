# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 💳 Thanh toán - Complete Order Flow >> [TC-2.20] Checkout - điền đầy đủ thông tin, chọn COD, đặt hàng
- Location: tests\02-customer-flow.spec.ts:493:7

# Error details

```
TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
```

# Page snapshot

```yaml
- generic [active] [ref=e1]:
  - link "Bỏ qua điều hướng, đến nội dung chính" [ref=e2] [cursor=pointer]:
    - /url: "#main-content"
  - banner [ref=e3]:
    - generic [ref=e6]:
      - generic [ref=e7]:
        - generic [ref=e8]:
          - generic [ref=e9]: 
          - text: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
        - generic [ref=e10]:
          - generic [ref=e11]: 
          - text: fastship@contact.com
      - generic [ref=e13]:
        - generic [ref=e14]: "Theo dõi chúng tôi:"
        - link "Facebook" [ref=e15] [cursor=pointer]:
          - /url: https://facebook.com/fastship
          - generic [ref=e16]: 
        - link "Instagram" [ref=e17] [cursor=pointer]:
          - /url: https://instagram.com/fastship
          - generic [ref=e18]: 
        - link "TikTok" [ref=e19] [cursor=pointer]:
          - /url: https://tiktok.com/@fastship
        - link "YouTube" [ref=e21] [cursor=pointer]:
          - /url: https://youtube.com/@fastship
          - generic [ref=e22]: 
    - navigation "Điều hướng chính" [ref=e23]:
      - generic [ref=e24]:
        - link "Fastship trang chủ" [ref=e25] [cursor=pointer]:
          - /url: /Home
          - text: Fastship
        - text: 
        - search "Tìm quán ăn" [ref=e27]:
          - combobox "Chọn danh mục" [ref=e28] [cursor=pointer]:
            - option "Tất cả" [selected]
            - option "Đồ ăn"
            - option "Đồ uống"
            - option "Đồ chay"
            - option "Bánh kem"
            - option "Tráng miệng"
            - option "Homemade"
            - option "Vỉa hè"
            - option "Pizza/Burger"
            - option "Món gà"
            - option "Món lẩu"
            - option "Sushi"
            - option "Mì phở"
            - option "Cơm hộp"
          - textbox "Từ khoá tìm kiếm" [ref=e29]:
            - /placeholder: Tìm món ăn, quán ăn...
          - button "Tìm kiếm" [ref=e30] [cursor=pointer]:
            - generic [ref=e31]: 
            - text: Tìm
        - generic [ref=e32]:
          - text: 
          - generic [ref=e33]:
            - link "Trang chủ" [ref=e34] [cursor=pointer]:
              - /url: /Home
            - link "Menu ẩm thực" [ref=e35] [cursor=pointer]:
              - /url: /Home/DanhMuc
            - link "T tranthib " [ref=e37] [cursor=pointer]:
              - /url: "#"
              - generic [ref=e38]: T
              - generic [ref=e39]: tranthib
              - text: 
            - link "Giỏ hàng" [ref=e40] [cursor=pointer]:
              - /url: /Cart
              - generic [ref=e41]: 
  - main [ref=e42]:
    - generic [ref=e45]:
      - generic [ref=e46]:
        - generic [ref=e47]:
          - heading "Giỏ hàng" [level=3] [ref=e48]:
            - img [ref=e49]
            - text: Giỏ hàng
          - generic [ref=e51]:
            - img [ref=e52]
            - text: Koneko Pizza
        - generic [ref=e55]:
          - img "Trà tắc" [ref=e56]
          - generic [ref=e57]:
            - generic [ref=e58]: Trà tắc
            - generic [ref=e59]: 10,000 đ / phần
          - generic [ref=e60]:
            - button "Giảm" [ref=e61] [cursor=pointer]:
              - img [ref=e62]
            - generic [ref=e63]: "1"
            - button "Tăng" [ref=e64] [cursor=pointer]:
              - img [ref=e65]
          - generic [ref=e66]: 10,000 đ
          - link "Xóa món Trà tắc khỏi giỏ hàng" [ref=e67] [cursor=pointer]:
            - /url: /Cart/XoaMon?maMonAn=1
            - img [ref=e68]
      - generic [ref=e71]:
        - generic [ref=e72]:
          - generic [ref=e73]: 
          - text: Có thể bạn đã bỏ quên
          - generic [ref=e74]: AI
        - paragraph [ref=e75]: Khách hàng thường đặt kèm những món này
        - generic [ref=e76]:
          - img "Pizza thập cẩm" [ref=e77]
          - generic [ref=e78]:
            - generic [ref=e79]: Pizza thập cẩm
            - generic [ref=e80]: Koneko Pizza
            - generic [ref=e81]: 80,000đ
          - link "+ Thêm" [ref=e82] [cursor=pointer]:
            - /url: /Cart/ThemMonAn?maMonAn=2&soLuong=1
      - generic [ref=e84]:
        - generic [ref=e85]:
          - img [ref=e86]
          - text: Tóm tắt đơn hàng
        - generic [ref=e88]:
          - generic [ref=e89]: Tổng món ăn
          - generic [ref=e90]: 10,000 đ
        - generic [ref=e91]:
          - generic [ref=e92]: Phí giao hàng
          - generic [ref=e93]: 15,000 đ
        - generic [ref=e94]:
          - generic [ref=e95]: Tổng cộng
          - generic [ref=e96]: 25,000 đ
        - generic [ref=e97]:
          - generic [ref=e98]:
            - generic [ref=e99]: 
            - text: "Mã giảm giá gợi ý:"
          - generic [ref=e100]:
            - generic [ref=e101] [cursor=pointer]:
              - generic [ref=e102]: 
              - text: Khuyến mãi sinh nhật (-30%)
            - generic [ref=e103] [cursor=pointer]: Khuyến mãi mùa hè (-20%)
        - link "Tiến hành thanh toán" [ref=e104] [cursor=pointer]:
          - /url: /Cart/Checkout
          - img [ref=e105]
          - text: Tiến hành thanh toán
        - link "← Tiếp tục mua sắm" [ref=e107] [cursor=pointer]:
          - /url: /
  - contentinfo [ref=e108]:
    - generic [ref=e111]:
      - generic [ref=e112]:
        - heading "Nhận ưu đãi & thực đơn mới nhất" [level=5] [ref=e113]
        - paragraph [ref=e114]: Đăng ký để không bỏ lỡ món mới, khuyến mãi hấp dẫn.
      - generic [ref=e117]:
        - textbox "Email đăng ký nhận tin" [ref=e118]:
          - /placeholder: Email của bạn...
        - button "Đăng ký" [ref=e119] [cursor=pointer]
    - generic [ref=e122]:
      - generic [ref=e123]:
        - link "Fastship" [ref=e124] [cursor=pointer]:
          - /url: /Home
        - paragraph [ref=e125]: Nền tảng đặt món và giao hàng nhanh tại TP. Hồ Chí Minh. Hàng trăm quán ăn ngon, giao trong 30 phút.
        - generic [ref=e126]:
          - link "Facebook" [ref=e127] [cursor=pointer]:
            - /url: https://facebook.com/fastship
            - generic [ref=e128]: 
          - link "Instagram" [ref=e129] [cursor=pointer]:
            - /url: https://instagram.com/fastship
            - generic [ref=e130]: 
          - link "TikTok" [ref=e131] [cursor=pointer]:
            - /url: https://tiktok.com/@fastship
          - link "YouTube" [ref=e132] [cursor=pointer]:
            - /url: https://youtube.com/@fastship
            - generic [ref=e133]: 
      - generic [ref=e134]:
        - heading "Khám phá" [level=6] [ref=e135]
        - list [ref=e136]:
          - listitem [ref=e137]:
            - link "Trang chủ" [ref=e138] [cursor=pointer]:
              - /url: /Home
          - listitem [ref=e139]:
            - link "Menu ẩm thực" [ref=e140] [cursor=pointer]:
              - /url: /Home/DanhMuc
          - listitem [ref=e141]:
            - link "Giỏ hàng" [ref=e142] [cursor=pointer]:
              - /url: /Cart
          - listitem [ref=e143]:
            - link "Lịch sử đơn" [ref=e144] [cursor=pointer]:
              - /url: /Cart/LichSuDatHang
      - generic [ref=e145]:
        - heading "Tài khoản" [level=6] [ref=e146]
        - list [ref=e147]:
          - listitem [ref=e148]:
            - link "Đơn hàng của tôi" [ref=e149] [cursor=pointer]:
              - /url: /Cart/LichSuDatHang
          - listitem [ref=e150]:
            - link "Đăng xuất" [ref=e151] [cursor=pointer]:
              - /url: /Home/Logout
      - generic [ref=e152]:
        - heading "Hỗ trợ" [level=6] [ref=e153]
        - list [ref=e154]:
          - listitem [ref=e155]:
            - link "Liên hệ" [ref=e156] [cursor=pointer]:
              - /url: /Home/Contact
          - listitem [ref=e157]:
            - link "Về chúng tôi" [ref=e158] [cursor=pointer]:
              - /url: /Home/About
          - listitem [ref=e159]:
            - link "Chính sách bảo mật" [ref=e160] [cursor=pointer]:
              - /url: "#"
          - listitem [ref=e161]:
            - link "Điều khoản sử dụng" [ref=e162] [cursor=pointer]:
              - /url: "#"
      - generic [ref=e163]:
        - heading "Liên hệ" [level=6] [ref=e164]
        - list [ref=e165]:
          - listitem [ref=e166]:
            - generic [ref=e167]: 
            - text: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
          - listitem [ref=e168]:
            - generic [ref=e169]: 
            - link "1900 1234" [ref=e170] [cursor=pointer]:
              - /url: tel:19001234
          - listitem [ref=e171]:
            - generic [ref=e172]: 
            - link "fastship@contact.com" [ref=e173] [cursor=pointer]:
              - /url: mailto:fastship@contact.com
          - listitem [ref=e174]:
            - generic [ref=e175]: 
            - text: 7:00 — 23:00, Thứ 2 — Chủ nhật
    - generic [ref=e178]:
      - generic [ref=e179]: © 2026 Fastship. Bảo lưu mọi quyền.
      - generic [ref=e181]:
        - text: Được làm với
        - generic [ref=e182]: 
        - text: tại Sài Gòn |
        - link "Chính sách bảo mật" [ref=e183] [cursor=pointer]:
          - /url: "#"
        - text: "|"
        - link "Điều khoản" [ref=e184] [cursor=pointer]:
          - /url: "#"
  - link "Lên đầu trang" [ref=e185] [cursor=pointer]:
    - /url: "#"
    - generic [ref=e186]: 
  - button "Mở chat hỗ trợ FastShip" [ref=e187] [cursor=pointer]:
    - generic [ref=e188]: 
  - generic:
    - generic:
      - generic:
        - heading " FastShip" [level=6]:
          - generic: 
          - text: FastShip
        - text: Trực tuyến 24/7
      - generic: ×
    - generic:
      - button "AI Chat"
      - button "Hỗ trợ"
    - generic:
      - generic:
        - generic: 👋 Chào bạn! Tôi là trợ lý FastShip. Có thể giúp gì cho bạn?
      - generic:
        - textbox "Nhập tin nhắn..."
        - button "":
          - generic: 
    - text: 
```

# Test source

```ts
  13  |   readonly restaurantAddress: Locator;
  14  |   readonly restaurantRating: Locator;
  15  |   readonly restaurantStatus: Locator;
  16  | 
  17  |   // ─── Menu Items ───
  18  |   readonly menuItems: Locator;
  19  |   readonly itemName: Locator;
  20  |   readonly itemPrice: Locator;
  21  |   readonly itemDesc: Locator;
  22  | 
  23  |   // ─── Add to Cart ───
  24  |   readonly quantityInput: Locator;
  25  |   readonly addToCartBtn: Locator;
  26  |   readonly searchMenuInput: Locator;
  27  |   readonly searchMenuBtn: Locator;
  28  | 
  29  |   // ─── Category ───
  30  |   readonly categoryPills: Locator;
  31  |   readonly categoryAll: Locator;
  32  | 
  33  |   // ─── Reviews ───
  34  |   readonly reviewList: Locator;
  35  | 
  36  |   constructor(page: Page) {
  37  |     super(page);
  38  | 
  39  |     this.restaurantName = page.locator('.name-restaurant');
  40  |     this.restaurantAddress = page.locator('.address-restaurant');
  41  |     this.restaurantRating = page.locator('.rating');
  42  |     this.restaurantStatus = page.locator('.status-restaurant');
  43  | 
  44  |     // Mỗi dòng món ăn là .item-restaurant-row
  45  |     this.menuItems = page.locator('.item-restaurant-row');
  46  |     this.itemName = page.locator('.item-restaurant-name');
  47  |     this.itemPrice = page.locator('.current-price');
  48  |     this.itemDesc = page.locator('.item-restaurant-desc');
  49  | 
  50  |     // Form thêm vào giỏ: input số lượng + nút Thêm
  51  |     this.quantityInput = page.locator('.adding-food-cart input[name="soLuong"]');
  52  |     this.addToCartBtn = page.locator('.add-to-cart-btn');
  53  | 
  54  |     // Tìm kiếm trong menu
  55  |     this.searchMenuInput = page.locator('input[name="searchKey"]');
  56  |     this.searchMenuBtn = page.locator('.search-items button[type="submit"]');
  57  | 
  58  |     // Danh mục thực đơn (category pills)
  59  |     this.categoryPills = page.locator('.list-category .item .item-link');
  60  |     this.categoryAll = page.locator('.list-category .item .item-link').first();
  61  | 
  62  |     // Review section
  63  |     this.reviewList = page.locator('#review-list');
  64  |   }
  65  | 
  66  |   /** Mở trang chi tiết quán ăn theo mã quán */
  67  |   async gotoRestaurant(quanId: number) {
  68  |     await this.goto(`/Home/DetailRestaurant?id=${quanId}`);
  69  |     await this.waitForPageReady();
  70  |   }
  71  | 
  72  |   /** Lấy tên quán ăn */
  73  |   async getRestaurantName(): Promise<string | null> {
  74  |     return await this.restaurantName.textContent();
  75  |   }
  76  | 
  77  |   /** Đếm số món trong thực đơn */
  78  |   async getMenuItemCount(): Promise<number> {
  79  |     return await this.menuItems.count();
  80  |   }
  81  | 
  82  |   /** Lấy danh sách tên món ăn */
  83  |   async getItemNames(): Promise<string[]> {
  84  |     return await this.itemName.allTextContents();
  85  |   }
  86  | 
  87  |   /** Lấy tên món đầu tiên */
  88  |   async getFirstItemName(): Promise<string | null> {
  89  |     return await this.itemName.first().textContent();
  90  |   }
  91  | 
  92  |   /** Tìm kiếm món trong menu của quán */
  93  |   async searchMenu(keyword: string) {
  94  |     await this.searchMenuInput.fill(keyword);
  95  |     await this.searchMenuBtn.click();
  96  |     await this.page.waitForResponse(resp =>
  97  |       resp.url().includes('DetailRestaurant') && resp.status() === 200
  98  |     );
  99  |   }
  100 | 
  101 |   /**
  102 |    * Thêm món đầu tiên vào giỏ hàng
  103 |    * - Đặt số lượng (mặc định 1)
  104 |    * - Click nút Thêm
  105 |    * - Chờ response API /Cart/ApiThemMonAn
  106 |    */
  107 |   async addFirstItemToCart(quantity: number = 1) {
  108 |     // Đặt số lượng
  109 |     await this.quantityInput.first().fill(quantity.toString());
  110 |     // Click nút Thêm — dùng optimistic AJAX
  111 |     await this.addToCartBtn.first().click();
  112 |     // Chờ API response (optimistic add-to-cart via AJAX)
> 113 |     await this.page.waitForResponse(resp =>
      |                     ^ TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
  114 |       resp.url().includes('ApiThemMonAn') && resp.status() === 200
  115 |     );
  116 |     // Chờ UI cập nhật
  117 |     await this.page.waitForLoadState('networkidle');
  118 |   }
  119 | 
  120 |   /**
  121 |    * Thêm món thứ index vào giỏ hàng
  122 |    */
  123 |   async addItemToCartByIndex(index: number, quantity: number = 1) {
  124 |     await this.quantityInput.nth(index).fill(quantity.toString());
  125 |     await this.addToCartBtn.nth(index).click();
  126 |     await this.page.waitForResponse(resp =>
  127 |       resp.url().includes('ApiThemMonAn') && resp.status() === 200
  128 |     );
  129 |     await this.page.waitForLoadState('networkidle');
  130 |   }
  131 | 
  132 |   /** Click vào category pill để lọc món */
  133 |   async clickCategory(categoryName: string) {
  134 |     const pill = this.categoryPills.filter({ hasText: categoryName }).first();
  135 |     await pill.click();
  136 |     await this.page.waitForLoadState('networkidle');
  137 |   }
  138 | 
  139 |   /** Kiểm tra review section có hiển thị không */
  140 |   async isReviewSectionVisible(): Promise<boolean> {
  141 |     try {
  142 |       return await this.reviewList.isVisible({ timeout: 5_000 });
  143 |     } catch {
  144 |       return false;
  145 |     }
  146 |   }
  147 | }
  148 | 
```