# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🛒 Vòng đời giỏ hàng (Cart Lifecycle) >> [TC-2.15] Xoá món khỏi giỏ - nút Delete hoạt động
- Location: tests\02-customer-flow.spec.ts:355:7

# Error details

```
TimeoutError: page.waitForResponse: Timeout 30000ms exceeded while waiting for event "response"
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