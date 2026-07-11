# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🔍 Tìm kiếm & Duyệt danh sách >> [TC-2.8] Click category pill - lọc danh sách quán
- Location: tests\02-customer-flow.spec.ts:185:7

# Error details

```
TimeoutError: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('#categoryRow').locator('a:has-text("Đồ ăn")').first()
    - locator resolved to <a href="/Home?idDM=1" class="fs-category-pill ">↵                        Đồ ăn↵                  …</a>
  - attempting click action
    2 × waiting for element to be visible, enabled and stable
      - element is not visible
    - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is not visible
    - retrying click action
      - waiting 100ms
    57 × waiting for element to be visible, enabled and stable
       - element is not visible
     - retrying click action
       - waiting 500ms

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
            - generic [ref=e36]:
              - link " Đăng nhập" [ref=e37] [cursor=pointer]:
                - /url: /Home/Login
                - generic [ref=e38]: 
                - generic [ref=e39]: Đăng nhập
              - link " Đăng ký" [ref=e40] [cursor=pointer]:
                - /url: /Home/Signup
                - generic [ref=e41]: 
                - generic [ref=e42]: Đăng ký
            - link "Giỏ hàng" [ref=e43] [cursor=pointer]:
              - /url: /Cart
              - generic [ref=e44]: 
  - main [ref=e45]:
    - banner "Ưu đãi đặc biệt" [ref=e46]:
      - generic [ref=e47]: 
      - text: Mùa hè đặc biệt — Giảm 20% cho đơn đầu tiên trong ngày!
      - link "Khám phá ngay " [ref=e48] [cursor=pointer]:
        - /url: /Home/DanhMuc
        - text: Khám phá ngay
        - generic [ref=e49]: 
      - button "Đóng thông báo" [ref=e50] [cursor=pointer]:
        - generic [ref=e51]: 
    - generic [ref=e53]:
      - generic [ref=e55]:
        - img "Khám phá ẩm thực Sài Gòn cùng Fastship" [ref=e56]
        - generic [ref=e60]:
          - heading "Ẩm thực Sài Gòn — giao tận cửa trong 30 phút" [level=1] [ref=e61]
          - generic [ref=e62]:
            - link " Đồ ăn" [ref=e63] [cursor=pointer]:
              - /url: /Home/DanhMuc?type=food
              - generic [ref=e64]: 
              - text: Đồ ăn
            - link " Đồ uống" [ref=e65] [cursor=pointer]:
              - /url: /Home/DanhMuc?type=drink
              - generic [ref=e66]: 
              - text: Đồ uống
      - button "Trước" [ref=e67] [cursor=pointer]:
        - generic [ref=e69]: Trước
      - button "Tiếp" [ref=e70] [cursor=pointer]:
        - generic [ref=e72]: Tiếp
    - generic [ref=e74]:
      - generic [ref=e75]:
        - generic [ref=e76]: 
        - generic [ref=e77]: 200+
        - generic [ref=e78]: Quán ăn đối tác
      - generic [ref=e79]:
        - img [ref=e81]
        - generic [ref=e86]: 30′
        - generic [ref=e87]: Giao hàng trung bình
      - generic [ref=e88]:
        - img [ref=e90]
        - generic [ref=e93]: 50K+
        - generic [ref=e94]: Đơn hàng mỗi tháng
      - generic [ref=e95]:
        - img [ref=e97]
        - generic [ref=e99]: 4.8★
        - generic [ref=e100]: Đánh giá trung bình
    - generic [ref=e102]:
      - generic [ref=e103]:
        - generic [ref=e104]:
          - heading "Quán ăn nổi bật tại TP.HCM" [level=2] [ref=e105]
          - paragraph [ref=e106]: Khám phá văn hoá ẩm thực Sài Gòn — hàng trăm hương vị, giao tận nơi
        - link "Xem tất cả " [ref=e108] [cursor=pointer]:
          - /url: /Home/DanhMuc
          - text: Xem tất cả
          - generic [ref=e109]: 
      - generic [ref=e112]:
        - button "Mở bộ lọc chi tiết" [ref=e113] [cursor=pointer]:
          - img [ref=e114]
          - generic [ref=e118]: Bộ lọc
        - generic [ref=e119]:
          - button " Khuyến mãi" [ref=e120] [cursor=pointer]:
            - generic [ref=e121]: 
            - text: Khuyến mãi
          - button "⭐ Bán chạy" [ref=e122] [cursor=pointer]
          - button "📍 Gần đây" [ref=e123] [cursor=pointer]
          - button "⚡ Dưới 30 phút" [ref=e124] [cursor=pointer]
          - button "👍 Đánh giá tốt (Từ 4.4)" [ref=e125] [cursor=pointer]
          - button "🚶 Tự đến lấy" [ref=e126] [cursor=pointer]
      - text:  
    - text: "} } else {"
    - generic [ref=e128]:
      - generic [ref=e130]: 
      - heading "Không tìm thấy quán ăn phù hợp" [level=5] [ref=e131]
      - paragraph [ref=e132]: Thử tìm kiếm với từ khoá khác hoặc chọn danh mục khác.
      - link "Xem tất cả quán" [ref=e133] [cursor=pointer]:
        - /url: /Home
    - text: "} "
  - contentinfo [ref=e137]:
    - generic [ref=e140]:
      - generic [ref=e141]:
        - heading "Nhận ưu đãi & thực đơn mới nhất" [level=5] [ref=e142]
        - paragraph [ref=e143]: Đăng ký để không bỏ lỡ món mới, khuyến mãi hấp dẫn.
      - generic [ref=e146]:
        - textbox "Email đăng ký nhận tin" [ref=e147]:
          - /placeholder: Email của bạn...
        - button "Đăng ký" [ref=e148] [cursor=pointer]
    - generic [ref=e151]:
      - generic [ref=e152]:
        - link "Fastship" [ref=e153] [cursor=pointer]:
          - /url: /Home
        - paragraph [ref=e154]: Nền tảng đặt món và giao hàng nhanh tại TP. Hồ Chí Minh. Hàng trăm quán ăn ngon, giao trong 30 phút.
        - generic [ref=e155]:
          - link "Facebook" [ref=e156] [cursor=pointer]:
            - /url: https://facebook.com/fastship
            - generic [ref=e157]: 
          - link "Instagram" [ref=e158] [cursor=pointer]:
            - /url: https://instagram.com/fastship
            - generic [ref=e159]: 
          - link "TikTok" [ref=e160] [cursor=pointer]:
            - /url: https://tiktok.com/@fastship
          - link "YouTube" [ref=e161] [cursor=pointer]:
            - /url: https://youtube.com/@fastship
            - generic [ref=e162]: 
      - generic [ref=e163]:
        - heading "Khám phá" [level=6] [ref=e164]
        - list [ref=e165]:
          - listitem [ref=e166]:
            - link "Trang chủ" [ref=e167] [cursor=pointer]:
              - /url: /Home
          - listitem [ref=e168]:
            - link "Menu ẩm thực" [ref=e169] [cursor=pointer]:
              - /url: /Home/DanhMuc
          - listitem [ref=e170]:
            - link "Giỏ hàng" [ref=e171] [cursor=pointer]:
              - /url: /Cart
          - listitem [ref=e172]:
            - link "Lịch sử đơn" [ref=e173] [cursor=pointer]:
              - /url: /Cart/LichSuDatHang
      - generic [ref=e174]:
        - heading "Tài khoản" [level=6] [ref=e175]
        - list [ref=e176]:
          - listitem [ref=e177]:
            - link "Đăng nhập" [ref=e178] [cursor=pointer]:
              - /url: /Home/Login
          - listitem [ref=e179]:
            - link "Đăng ký" [ref=e180] [cursor=pointer]:
              - /url: /Home/Signup
      - generic [ref=e181]:
        - heading "Hỗ trợ" [level=6] [ref=e182]
        - list [ref=e183]:
          - listitem [ref=e184]:
            - link "Liên hệ" [ref=e185] [cursor=pointer]:
              - /url: /Home/Contact
          - listitem [ref=e186]:
            - link "Về chúng tôi" [ref=e187] [cursor=pointer]:
              - /url: /Home/About
          - listitem [ref=e188]:
            - link "Chính sách bảo mật" [ref=e189] [cursor=pointer]:
              - /url: "#"
          - listitem [ref=e190]:
            - link "Điều khoản sử dụng" [ref=e191] [cursor=pointer]:
              - /url: "#"
      - generic [ref=e192]:
        - heading "Liên hệ" [level=6] [ref=e193]
        - list [ref=e194]:
          - listitem [ref=e195]:
            - generic [ref=e196]: 
            - text: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
          - listitem [ref=e197]:
            - generic [ref=e198]: 
            - link "1900 1234" [ref=e199] [cursor=pointer]:
              - /url: tel:19001234
          - listitem [ref=e200]:
            - generic [ref=e201]: 
            - link "fastship@contact.com" [ref=e202] [cursor=pointer]:
              - /url: mailto:fastship@contact.com
          - listitem [ref=e203]:
            - generic [ref=e204]: 
            - text: 7:00 — 23:00, Thứ 2 — Chủ nhật
    - generic [ref=e207]:
      - generic [ref=e208]: © 2026 Fastship. Bảo lưu mọi quyền.
      - generic [ref=e210]:
        - text: Được làm với
        - generic [ref=e211]: 
        - text: tại Sài Gòn |
        - link "Chính sách bảo mật" [ref=e212] [cursor=pointer]:
          - /url: "#"
        - text: "|"
        - link "Điều khoản" [ref=e213] [cursor=pointer]:
          - /url: "#"
  - link "Lên đầu trang" [ref=e214] [cursor=pointer]:
    - /url: "#"
    - generic [ref=e215]: 
  - button "Mở chat hỗ trợ FastShip" [ref=e216] [cursor=pointer]:
    - generic [ref=e217]: 
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
    - generic:
      - generic:
        - generic: 👋 Chào bạn! Tôi là trợ lý FastShip. Có thể giúp gì cho bạn?
      - generic:
        - textbox "Nhập tin nhắn..."
        - button "":
          - generic: 
    - text: 
```

# Test source

```ts
  1   | import { Page, Locator } from '@playwright/test';
  2   | import { BasePage } from './BasePage';
  3   | 
  4   | /**
  5   |  * HomePage — Page Object cho trang chủ Fastship
  6   |  * URL: /
  7   |  * Gồm: Hero carousel, danh sách quán ăn, category pills, thanh tìm kiếm
  8   |  */
  9   | export class HomePage extends BasePage {
  10  |   // ─── Navbar ───
  11  |   readonly logo: Locator;
  12  |   readonly searchInput: Locator;
  13  |   readonly searchButton: Locator;
  14  |   readonly cartButton: Locator;
  15  |   readonly loginNavBtn: Locator;
  16  |   readonly registerNavBtn: Locator;
  17  |   readonly userDropdown: Locator;
  18  |   readonly logoutLink: Locator;
  19  | 
  20  |   // ─── Hero Carousel ───
  21  |   readonly carousel: Locator;
  22  |   readonly carouselPrevBtn: Locator;
  23  |   readonly carouselNextBtn: Locator;
  24  | 
  25  |   // ─── Category Pills ───
  26  |   readonly categoryAll: Locator;
  27  |   readonly categoryRow: Locator;
  28  | 
  29  |   // ─── Restaurant Grid ───
  30  |   readonly restaurantCards: Locator;
  31  |   readonly emptyState: Locator;
  32  |   readonly emptyStateMessage: Locator;
  33  | 
  34  |   // ─── Promo Band ───
  35  |   readonly promoBand: Locator;
  36  |   readonly promoDismissBtn: Locator;
  37  | 
  38  |   // ─── Stats Row ───
  39  |   readonly statsRow: Locator;
  40  | 
  41  |   constructor(page: Page) {
  42  |     super(page);
  43  | 
  44  |     // ponytail: .first() — có thể có 2 elements do responsive layout
  45  |     this.logo = page.locator('.fs-logo').first();
  46  |     // ponytail: .first() — navbar có 2 search inputs (desktop + mobile)
  47  |     this.searchInput = page.locator('input[name="txtSearch"]').first();
  48  |     this.searchButton = page.getByRole('button', { name: /tìm/i });
  49  |     // ponytail: .first() — responsive layout có 2 cart buttons (desktop + mobile)
  50  |     this.cartButton = page.locator('.fs-cart-btn').first();
  51  |     this.loginNavBtn = page.locator('a[href*="/Home/Login"]').first();
  52  |     this.registerNavBtn = page.locator('a[href*="/Home/Signup"]').first();
  53  |     this.userDropdown = page.locator('.dropdown-toggle .fs-avatar-xs');
  54  |     this.logoutLink = page.locator('a[href*="/Home/Logout"]');
  55  | 
  56  |     this.carousel = page.locator('#header-carousel');
  57  |     this.carouselPrevBtn = page.locator('.carousel-control-prev');
  58  |     this.carouselNextBtn = page.locator('.carousel-control-next');
  59  | 
  60  |     this.categoryAll = page.locator('.fs-category-pill').first();
  61  |     this.categoryRow = page.locator('#categoryRow');
  62  | 
  63  |     this.restaurantCards = page.locator('.product-item');
  64  |     this.emptyState = page.locator('.col-12.text-center.py-5');
  65  |     this.emptyStateMessage = page.locator('h5:has-text("Không tìm thấy")');
  66  | 
  67  |     this.promoBand = page.locator('#promoBand');
  68  |     this.promoDismissBtn = page.locator('#promoDismissBtn');
  69  |     this.statsRow = page.locator('.fs-stats-row');
  70  |   }
  71  | 
  72  |   /** Load trang chủ */
  73  |   async gotoHome() {
  74  |     await this.goto('/');
  75  |   }
  76  | 
  77  |   /** Tìm kiếm quán ăn hoặc món ăn */
  78  |   async search(keyword: string) {
  79  |     await this.searchInput.fill(keyword);
  80  |     await this.searchButton.click();
  81  |     await this.page.waitForLoadState('networkidle');
  82  |   }
  83  | 
  84  |   /** Click vào một category pill */
  85  |   async clickCategory(categoryName: string) {
  86  |     const category = this.categoryRow.locator(`a:has-text("${categoryName}")`);
> 87  |     await category.first().click();
      |                            ^ TimeoutError: locator.click: Timeout 30000ms exceeded.
  88  |     await this.page.waitForLoadState('networkidle');
  89  |   }
  90  | 
  91  |   /** Lấy danh sách tên các quán ăn đang hiển thị */
  92  |   async getRestaurantNames(): Promise<string[]> {
  93  |     return await this.restaurantCards.locator('.product-title').allTextContents();
  94  |   }
  95  | 
  96  |   /** Click vào quán ăn đầu tiên trong danh sách */
  97  |   async clickFirstRestaurant() {
  98  |     await this.restaurantCards.first().click();
  99  |     await this.page.waitForLoadState('networkidle');
  100 |   }
  101 | 
  102 |   /** Click vào quán ăn theo tên */
  103 |   async clickRestaurantByName(name: string) {
  104 |     const card = this.restaurantCards.locator(`.product-title:has-text("${name}")`).first();
  105 |     await card.click();
  106 |     await this.page.waitForLoadState('networkidle');
  107 |   }
  108 | 
  109 |   /** Kiểm tra có hiển thị quán ăn không */
  110 |   async hasRestaurants(): Promise<boolean> {
  111 |     const count = await this.restaurantCards.count();
  112 |     return count > 0;
  113 |   }
  114 | 
  115 |   /** Đếm số quán ăn hiển thị */
  116 |   async getRestaurantCount(): Promise<number> {
  117 |     return await this.restaurantCards.count();
  118 |   }
  119 | 
  120 |   /** Dismiss promo band */
  121 |   async dismissPromo() {
  122 |     try {
  123 |       await this.promoDismissBtn.click({ timeout: 3_000 });
  124 |       await this.page.waitForTimeout(500);
  125 |     } catch {
  126 |       // Promo band có thể đã bị dismiss trước đó
  127 |     }
  128 |   }
  129 | 
  130 |   /** Kiểm tra navbar hiển thị đúng */
  131 |   async isNavbarVisible(): Promise<boolean> {
  132 |     return await this.navbar.isVisible();
  133 |   }
  134 | 
  135 |   /** Lấy text từ stat item */
  136 |   async getStatValue(index: number): Promise<string | null> {
  137 |     const stat = this.statsRow.locator('.fs-stat-item').nth(index);
  138 |     return await stat.locator('.stat-num').textContent();
  139 |   }
  140 | }
  141 | 
```