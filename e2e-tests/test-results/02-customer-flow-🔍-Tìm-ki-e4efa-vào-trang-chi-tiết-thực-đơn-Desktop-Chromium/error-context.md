# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-customer-flow.spec.ts >> 🔍 Tìm kiếm & Duyệt danh sách >> [TC-2.9] Click vào quán ăn - vào trang chi tiết thực đơn
- Location: tests\02-customer-flow.spec.ts:195:7

# Error details

```
Error: expect(received).toBeGreaterThan(expected)

Expected: > 0
Received:   0
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
            - /placeholder: Tìm quán ăn, món ăn...
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
    - generic [ref=e47]:
      - generic [ref=e48]:
        - generic [ref=e49]:
          - img "Không tìm thấy ảnh" [ref=e51]
          - generic [ref=e52]:
            - navigation "breadcrumb" [ref=e53]:
              - list [ref=e54]:
                - listitem [ref=e55]:
                  - link "Trang chủ" [ref=e56] [cursor=pointer]:
                    - /url: /
                  - text: »
                - listitem [ref=e57]:
                  - text: /
                  - link "TP. Hồ Chí Minh" [ref=e58] [cursor=pointer]:
                    - /url: /Home
                  - text: »
                - listitem [ref=e59]: / Koneko Pizza »
            - generic [ref=e60]:
              - generic [ref=e61] [cursor=pointer]:
                - generic [ref=e62]: 
                - text: Yêu thích
              - generic [ref=e63]:
                - text: Quán ăn
                - generic [ref=e64]:
                  - text: "-"
                  - link "Chi nhánh" [ref=e65] [cursor=pointer]:
                    - /url: /thuong-hieu/com-tho-thien-phuc
            - heading "Koneko Pizza" [level=1] [ref=e66]
            - generic [ref=e67]: K57H10/12 Bà Bang Nhãn, P. Hòa Hải, Quận Bình Thạnh, TP. Hồ Chí Minh
            - generic [ref=e68]:
              - generic [ref=e69]:
                - generic [ref=e71]: 
                - generic [ref=e73]: 
                - generic [ref=e75]: 
                - generic [ref=e77]: 
                - generic [ref=e79]: 
              - generic [ref=e80]: "75"
              - text: đánh giá trên Fastship
            - link "Xem thêm lượt đánh giá từ Fastship" [ref=e82] [cursor=pointer]:
              - /url: "#"
            - generic [ref=e83]:
              - generic "Đang mở cửa" [ref=e85] [cursor=pointer]
              - generic [ref=e86]:
                - generic [ref=e87]: 
                - text: 07:30 - 21:30
            - generic [ref=e88]:
              - generic [ref=e89]: 
              - text: 35.000 - 75.000
            - generic [ref=e91]:
              - generic [ref=e92]:
                - generic [ref=e93]: Phí dịch vụ
                - generic [ref=e94]: 0.0% Phí phục vụ
              - generic [ref=e95]:
                - generic [ref=e96]: Dịch vụ bởi
                - generic [ref=e97]: Fastship
        - generic [ref=e99]:
          - generic [ref=e101]: Thực đơn
          - generic:
            - generic:
              - generic [ref=e104]:
                - link "Tất cả" [ref=e105] [cursor=pointer]:
                  - /url: /Home/DetailRestaurant?id=6
                  - generic "Tất cả" [ref=e107]
                - link "Đồ uống" [ref=e108] [cursor=pointer]:
                  - /url: ~/Home/DetailRestaurant?idDM=2&id=6
                  - generic "Đồ uống" [ref=e110]
                - link "Pizza/Burger" [ref=e111] [cursor=pointer]:
                  - /url: ~/Home/DetailRestaurant?idDM=8&id=6
                  - generic "Pizza/Burger" [ref=e113]
              - generic [ref=e115]:
                - paragraph [ref=e118]:
                  - generic [ref=e119]: 
                  - textbox "Tìm món" [ref=e120]
                  - button "Tìm" [ref=e121] [cursor=pointer]:
                    - strong [ref=e122]: Tìm
                - strong [ref=e125]: Danh sách món
                - generic [ref=e127]:
                  - generic [ref=e128]:
                    - button "Không tìm thấy ảnh" [ref=e129] [cursor=pointer]:
                      - img "Không tìm thấy ảnh" [ref=e130]
                    - generic [ref=e131]: "-20%"
                  - generic [ref=e132]:
                    - heading "Trà tắc" [level=2] [ref=e133] [cursor=pointer]
                    - generic [ref=e134]: Trà và tắc.
                    - generic [ref=e135]:
                      - text: Đã được đặt 100+ lần |
                      - generic [ref=e136]: 
                      - text: "3"
                  - generic [ref=e138]:
                    - generic [ref=e139]:
                      - generic [ref=e140]: 10,000đ
                      - generic [ref=e141]: 8,000đ
                    - generic [ref=e143]:
                      - spinbutton [ref=e144]: "1"
                      - button "Thêm" [ref=e145] [cursor=pointer]
                - generic [ref=e147]:
                  - generic [ref=e148]:
                    - button "Không tìm thấy ảnh" [ref=e149] [cursor=pointer]:
                      - img "Không tìm thấy ảnh" [ref=e150]
                    - generic [ref=e151]: "-30%"
                  - generic [ref=e152]:
                    - heading "Pizza thập cẩm" [level=2] [ref=e153] [cursor=pointer]
                    - generic [ref=e154]: Thịt, xúc xích, ớt chuông, bắp và phô mai
                    - generic [ref=e155]:
                      - text: Đã được đặt 100+ lần |
                      - generic [ref=e156]: 
                      - text: "3"
                  - generic [ref=e158]:
                    - generic [ref=e159]:
                      - generic [ref=e160]: 80,000đ
                      - generic [ref=e161]: 56,000đ
                    - generic [ref=e163]:
                      - spinbutton [ref=e164]: "1"
                      - button "Thêm" [ref=e165] [cursor=pointer]
                - generic [ref=e167]:
                  - generic [ref=e168]:
                    - button "Không tìm thấy ảnh" [ref=e169] [cursor=pointer]:
                      - img "Không tìm thấy ảnh" [ref=e170]
                    - generic [ref=e171]: "-10%"
                  - generic [ref=e172]:
                    - heading "Pizza Bò" [level=2] [ref=e173] [cursor=pointer]
                    - generic [ref=e174]: Bò, ớt chuông, bắp và phô mai
                    - generic [ref=e175]:
                      - text: Đã được đặt 100+ lần |
                      - generic [ref=e176]: 
                      - text: "3"
                  - generic [ref=e178]:
                    - generic [ref=e179]:
                      - generic [ref=e180]: 70,000đ
                      - generic [ref=e181]: 63,000đ
                    - generic [ref=e183]:
                      - spinbutton [ref=e184]: "1"
                      - button "Thêm" [ref=e185] [cursor=pointer]
                - generic [ref=e187]:
                  - button "Không tìm thấy ảnh" [ref=e189] [cursor=pointer]:
                    - img "Không tìm thấy ảnh" [ref=e190]
                  - generic [ref=e191]:
                    - heading "Pizza xúc xích" [level=2] [ref=e192] [cursor=pointer]
                    - generic [ref=e193]: Xúc xích, bắp, phô mai và ớt chuông
                    - generic [ref=e194]:
                      - text: Đã được đặt 100+ lần |
                      - generic [ref=e195]: 
                      - text: "3"
                  - generic [ref=e197]:
                    - generic [ref=e199]: 70,000đ
                    - generic [ref=e201]:
                      - spinbutton [ref=e202]: "1"
                      - button "Thêm" [ref=e203] [cursor=pointer]
                - generic [ref=e205]:
                  - button "Không tìm thấy ảnh" [ref=e207] [cursor=pointer]:
                    - img "Không tìm thấy ảnh" [ref=e208]
                  - generic [ref=e209]:
                    - heading "Pizza hải sản" [level=2] [ref=e210] [cursor=pointer]
                    - generic [ref=e211]: Tôm, mực, ớt chuông, bắp, phô mai
                    - generic [ref=e212]:
                      - text: Đã được đặt 100+ lần |
                      - generic [ref=e213]: 
                      - text: "3"
                  - generic [ref=e215]:
                    - generic [ref=e217]: 95,000đ
                    - generic [ref=e219]:
                      - spinbutton [ref=e220]: "1"
                      - button "Thêm" [ref=e221] [cursor=pointer]
                - generic [ref=e222]:
                  - strong [ref=e224]:
                    - generic [ref=e225]: 
                    - text: Thường được mua kèm
                  - generic [ref=e226]:
                    - img [ref=e228]
                    - generic [ref=e229]:
                      - strong [ref=e230]: Pizza thập cẩm
                      - generic [ref=e231]: 80,000đ
                    - link "Xem" [ref=e233] [cursor=pointer]:
                      - /url: /Home/DetailRestaurant/6
                - generic [ref=e234]:
                  - strong [ref=e236]:
                    - generic [ref=e237]: 
                    - text: Gợi ý cho khung giờ này
                  - generic [ref=e238]:
                    - generic [ref=e239]:
                      - img [ref=e240]
                      - strong [ref=e242]: Cơm trắng + đậu nhồi thịt sốt cà chua
                      - generic [ref=e243]: 40,000đ
                      - link "Đặt ngay" [ref=e244] [cursor=pointer]:
                        - /url: /Home/DetailRestaurant/7
                    - generic [ref=e245]:
                      - img [ref=e246]
                      - strong [ref=e248]: Cơm trắng + sườn xào chua ngọt
                      - generic [ref=e249]: 40,000đ
                      - link "Đặt ngay" [ref=e250] [cursor=pointer]:
                        - /url: /Home/DetailRestaurant/7
                    - generic [ref=e251]:
                      - img [ref=e252]
                      - strong [ref=e254]: Combo cơm gà rang xả ớt + nước
                      - generic [ref=e255]: 50,000đ
                      - link "Đặt ngay" [ref=e256] [cursor=pointer]:
                        - /url: /Home/DetailRestaurant/7
                    - generic [ref=e257]:
                      - img [ref=e258]
                      - strong [ref=e260]: Cơm trắng + đậu nhồi thịt + rau xào theo ngày
                      - generic [ref=e261]: 40,000đ
                      - link "Đặt ngay" [ref=e262] [cursor=pointer]:
                        - /url: /Home/DetailRestaurant/7
      - generic [ref=e263]:
        - generic [ref=e265]:
          - heading " Danh mục món ăn" [level=3] [ref=e266]:
            - generic [ref=e267]: 
            - text: Danh mục món ăn
          - button "Đóng" [ref=e268] [cursor=pointer]: ✕
        - generic [ref=e269]:
          - link "🍽 Tất cả" [ref=e270] [cursor=pointer]:
            - /url: /Home/DetailRestaurant?id=6
            - generic [ref=e271]: 🍽
            - generic [ref=e272]: Tất cả
          - link "🥤 Đồ uống" [ref=e273] [cursor=pointer]:
            - /url: ~/Home/DetailRestaurant?idDM=2&id=6
            - generic [ref=e274]: 🥤
            - generic [ref=e275]: Đồ uống
          - link "📂 Pizza/Burger" [ref=e276] [cursor=pointer]:
            - /url: ~/Home/DetailRestaurant?idDM=8&id=6
            - generic [ref=e277]: 📂
            - generic [ref=e278]: Pizza/Burger
      - generic [ref=e280]:
        - generic [ref=e281]:
          - generic [ref=e282]:
            - heading "⭐ Đánh giá từ khách hàng" [level=3] [ref=e283]
            - paragraph [ref=e284]: Nhận xét thực tế từ những người đã đặt hàng
          - generic [ref=e286]:
            - generic [ref=e287]: "4.5"
            - generic [ref=e288]: ★★★★½
            - generic [ref=e289]: 2 đánh giá
        - generic [ref=e290]:
          - generic [ref=e291]:
            - generic [ref=e292]:
              - generic [ref=e293]: T
              - generic [ref=e294]:
                - generic [ref=e295]: Tran Thi B
                - generic [ref=e296]: 16/05/2024
              - generic [ref=e297]: ★★★★☆
            - paragraph [ref=e298]: "\"Pizza ngon, phô mai nhiều\""
            - generic [ref=e299]:
              - img [ref=e300]
              - generic [ref=e301]: Pizza thập cẩm
          - generic [ref=e302]:
            - generic [ref=e303]:
              - generic [ref=e304]: T
              - generic [ref=e305]:
                - generic [ref=e306]: Tran Thi B
                - generic [ref=e307]: 16/05/2024
              - generic [ref=e308]: ★★★★★
            - paragraph [ref=e309]: "\"Món ăn ngon, giao hàng nhanh\""
            - generic [ref=e310]:
              - img [ref=e311]
              - generic [ref=e312]: Trà tắc
        - generic [ref=e313]:
          - paragraph [ref=e314]: Đăng nhập để viết đánh giá
          - link "Đăng nhập" [ref=e315] [cursor=pointer]:
            - /url: /Home/Login
  - contentinfo [ref=e316]:
    - generic [ref=e319]:
      - generic [ref=e320]:
        - heading "Nhận ưu đãi & thực đơn mới nhất" [level=5] [ref=e321]
        - paragraph [ref=e322]: Đăng ký để không bỏ lỡ món mới, khuyến mãi hấp dẫn.
      - generic [ref=e325]:
        - textbox "Email đăng ký nhận tin" [ref=e326]:
          - /placeholder: Email của bạn...
        - button "Đăng ký" [ref=e327] [cursor=pointer]
    - generic [ref=e330]:
      - generic [ref=e331]:
        - link "Fastship" [ref=e332] [cursor=pointer]:
          - /url: /Home
        - paragraph [ref=e333]: Nền tảng đặt món và giao hàng nhanh tại TP. Hồ Chí Minh. Hàng trăm quán ăn ngon, giao trong 30 phút.
        - generic [ref=e334]:
          - link "Facebook" [ref=e335] [cursor=pointer]:
            - /url: https://facebook.com/fastship
            - generic [ref=e336]: 
          - link "Instagram" [ref=e337] [cursor=pointer]:
            - /url: https://instagram.com/fastship
            - generic [ref=e338]: 
          - link "TikTok" [ref=e339] [cursor=pointer]:
            - /url: https://tiktok.com/@fastship
          - link "YouTube" [ref=e340] [cursor=pointer]:
            - /url: https://youtube.com/@fastship
            - generic [ref=e341]: 
      - generic [ref=e342]:
        - heading "Khám phá" [level=6] [ref=e343]
        - list [ref=e344]:
          - listitem [ref=e345]:
            - link "Trang chủ" [ref=e346] [cursor=pointer]:
              - /url: /Home
          - listitem [ref=e347]:
            - link "Menu ẩm thực" [ref=e348] [cursor=pointer]:
              - /url: /Home/DanhMuc
          - listitem [ref=e349]:
            - link "Giỏ hàng" [ref=e350] [cursor=pointer]:
              - /url: /Cart
          - listitem [ref=e351]:
            - link "Lịch sử đơn" [ref=e352] [cursor=pointer]:
              - /url: /Cart/LichSuDatHang
      - generic [ref=e353]:
        - heading "Tài khoản" [level=6] [ref=e354]
        - list [ref=e355]:
          - listitem [ref=e356]:
            - link "Đăng nhập" [ref=e357] [cursor=pointer]:
              - /url: /Home/Login
          - listitem [ref=e358]:
            - link "Đăng ký" [ref=e359] [cursor=pointer]:
              - /url: /Home/Signup
      - generic [ref=e360]:
        - heading "Hỗ trợ" [level=6] [ref=e361]
        - list [ref=e362]:
          - listitem [ref=e363]:
            - link "Liên hệ" [ref=e364] [cursor=pointer]:
              - /url: /Home/Contact
          - listitem [ref=e365]:
            - link "Về chúng tôi" [ref=e366] [cursor=pointer]:
              - /url: /Home/About
          - listitem [ref=e367]:
            - link "Chính sách bảo mật" [ref=e368] [cursor=pointer]:
              - /url: "#"
          - listitem [ref=e369]:
            - link "Điều khoản sử dụng" [ref=e370] [cursor=pointer]:
              - /url: "#"
      - generic [ref=e371]:
        - heading "Liên hệ" [level=6] [ref=e372]
        - list [ref=e373]:
          - listitem [ref=e374]:
            - generic [ref=e375]: 
            - text: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
          - listitem [ref=e376]:
            - generic [ref=e377]: 
            - link "1900 1234" [ref=e378] [cursor=pointer]:
              - /url: tel:19001234
          - listitem [ref=e379]:
            - generic [ref=e380]: 
            - link "fastship@contact.com" [ref=e381] [cursor=pointer]:
              - /url: mailto:fastship@contact.com
          - listitem [ref=e382]:
            - generic [ref=e383]: 
            - text: 7:00 — 23:00, Thứ 2 — Chủ nhật
    - generic [ref=e386]:
      - generic [ref=e387]: © 2026 Fastship. Bảo lưu mọi quyền.
      - generic [ref=e389]:
        - text: Được làm với
        - generic [ref=e390]: 
        - text: tại Sài Gòn |
        - link "Chính sách bảo mật" [ref=e391] [cursor=pointer]:
          - /url: "#"
        - text: "|"
        - link "Điều khoản" [ref=e392] [cursor=pointer]:
          - /url: "#"
  - text: 
  - button "Mở chat hỗ trợ FastShip" [ref=e393] [cursor=pointer]:
    - generic [ref=e394]: 
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
  119 |   test('[TC-2.5] Đăng nhập với Remember Me - session còn sau redirect', async ({ page }) => {
  120 |     const login = new LoginPage(page);
  121 |     await login.gotoLogin();
  122 | 
  123 |     await login.usernameInput.fill(CUSTOMER.username);
  124 |     await login.passwordInput.fill(CUSTOMER.password);
  125 |     await login.rememberMeCheckbox.check();
  126 |     await login.loginButton.click();
  127 | 
  128 |     await page.waitForLoadState('networkidle');
  129 |     await page.waitForTimeout(2000);
  130 | 
  131 |     // Kiểm tra đã login - user dropdown hiển thị
  132 |     const home = new HomePage(page);
  133 |     try {
  134 |       await expect(home.userDropdown).toBeVisible({ timeout: 8_000 });
  135 |       console.log('✅ Remember Me - user dropdown visible');
  136 |     } catch {
  137 |       console.log('ℹ️ User dropdown không visible (có thể do UI khác)');
  138 |     }
  139 |   });
  140 | });
  141 | 
  142 | // ─── TEST SUITE 2: Tìm kiếm & Duyệt ───
  143 | test.describe('🔍 Tìm kiếm & Duyệt danh sách', () => {
  144 | 
  145 |   test('[TC-2.6] Tìm kiếm "pizza" - hiển thị ít nhất 1 kết quả', async ({ page }) => {
  146 |     const home = new HomePage(page);
  147 |     await home.gotoHome();
  148 | 
  149 |     await home.searchInput.fill('pizza');
  150 |     await home.searchButton.click();
  151 |     // ponytail: không dùng waitForResponse vì search là form GET, không fetch API
  152 |     try {
  153 |       await page.waitForLoadState('networkidle', { timeout: 30_000 });
  154 |     } catch {}
  155 |     await page.waitForTimeout(2000);
  156 | 
  157 |     const hasResults = await home.hasRestaurants();
  158 |     if (hasResults) {
  159 |       const names = await home.getRestaurantNames();
  160 |       console.log(`🔍 Tìm "pizza": ${names.length} kết quả`);
  161 |       expect(names.length).toBeGreaterThan(0);
  162 |     } else {
  163 |       console.log('🔍 Không có kết quả cho "pizza"');
  164 |     }
  165 |   });
  166 | 
  167 |   test('[TC-2.7] Tìm kiếm không có kết quả - hiển thị thông báo "Không tìm thấy"', async ({ page }) => {
  168 |     const home = new HomePage(page);
  169 |     await home.gotoHome();
  170 | 
  171 |     await home.search('xyzkhôngcókếtquả123456');
  172 |     await page.waitForLoadState('networkidle');
  173 | 
  174 |     const hasResults = await home.hasRestaurants();
  175 |     if (!hasResults) {
  176 |       try {
  177 |         await expect(home.emptyStateMessage).toBeVisible({ timeout: 5_000 });
  178 |         console.log('✅ Hiển thị "Không tìm thấy"');
  179 |       } catch {
  180 |         console.log('ℹ️ Không có empty state message');
  181 |       }
  182 |     }
  183 |   });
  184 | 
  185 |   test('[TC-2.8] Click category pill - lọc danh sách quán', async ({ page }) => {
  186 |     const home = new HomePage(page);
  187 |     await home.gotoHome();
  188 | 
  189 |     await home.clickCategory('Đồ ăn');
  190 |     await page.waitForLoadState('networkidle');
  191 |     const count = await home.getRestaurantCount();
  192 |     console.log(`🏷️ Category "Đồ ăn": ${count} quán`);
  193 |   });
  194 | 
  195 |   test('[TC-2.9] Click vào quán ăn - vào trang chi tiết thực đơn', async ({ page }) => {
  196 |     const home = new HomePage(page);
  197 |     await home.gotoHome();
  198 | 
  199 |     // ponytail: Render free chậm — timeout lâu hơn
  200 |     try {
  201 |       await page.waitForSelector('.product-item', { timeout: 25_000 });
  202 |       const count = await home.getRestaurantCount();
  203 |       expect(count).toBeGreaterThan(0);
  204 |       await home.clickFirstRestaurant();
  205 |       await page.waitForURL('**/DetailRestaurant**', { timeout: 25_000 });
  206 |     } catch {
  207 |       // Fallback: goto trực tiếp Koneko Pizza
  208 |       console.log('⏳ product-item/click timeout, thử goto trực tiếp...');
  209 |       await page.goto('/Home/DetailRestaurant?id=' + SEED.restaurantIds.konekoPizza, {
  210 |         waitUntil: 'networkidle',
  211 |         timeout: 20_000
  212 |       });
  213 |     }
  214 |     await page.waitForLoadState('networkidle');
  215 |     expect(page.url()).toContain('DetailRestaurant');
  216 |     console.log(`✅ DetailRestaurant URL: ${page.url()}`);
  217 | 
  218 |     const count = await home.getRestaurantCount();
> 219 |     expect(count).toBeGreaterThan(0);
      |                   ^ Error: expect(received).toBeGreaterThan(expected)
  220 | 
  221 |     await home.clickFirstRestaurant();
  222 |     await page.waitForURL('**/DetailRestaurant**', { timeout: 40_000 });
  223 |     await page.waitForLoadState('networkidle');
  224 | 
  225 |     expect(page.url()).toContain('DetailRestaurant');
  226 |     console.log(`✅ DetailRestaurant URL: ${page.url()}`);
  227 |   });
  228 | 
  229 |   test('[TC-2.10] Xem chi tiết quán - thực đơn có món ăn', async ({ page }) => {
  230 |     const detail = new DetailRestaurantPage(page);
  231 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  232 | 
  233 |     await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  234 |     const itemCount = await detail.getMenuItemCount();
  235 |     console.log(`🍕 Koneko Pizza: ${itemCount} món`);
  236 |     expect(itemCount).toBeGreaterThan(0);
  237 | 
  238 |     const name = await detail.getRestaurantName();
  239 |     expect(name).toBeTruthy();
  240 |     console.log(`🏪 Quán: ${name}`);
  241 |   });
  242 | });
  243 | 
  244 | // ─── TEST SUITE 3: Giỏ hàng ───
  245 | test.describe('🛒 Vòng đời giỏ hàng (Cart Lifecycle)', () => {
  246 | 
  247 |   test('[TC-2.11] Giỏ hàng trống - thông báo "trống" hiển thị', async ({ page }) => {
  248 |     const cart = new CartPage(page);
  249 |     await cart.gotoCart();
  250 |     await page.waitForLoadState('networkidle');
  251 | 
  252 |     try {
  253 |       await expect(cart.emptyCartMessage).toBeVisible({ timeout: 5_000 });
  254 |       console.log('✅ Giỏ trống - thông báo hiển thị');
  255 |     } catch {
  256 |       // Nếu không trống, kiểm tra có item
  257 |       const itemCount = await cart.getItemCount();
  258 |       console.log(`ℹ️ Giỏ có ${itemCount} món`);
  259 |     }
  260 |   });
  261 | 
  262 |   test('[TC-2.12] Thêm món vào giỏ (đã login) - kiểm tra giỏ có item', async ({ page }) => {
  263 |     // Login
  264 |     const login = new LoginPage(page);
  265 |     await login.gotoLogin();
  266 |     await login.usernameInput.fill(CUSTOMER.username);
  267 |     await login.passwordInput.fill(CUSTOMER.password);
  268 |     await login.loginButton.click();
  269 |     await page.waitForLoadState('networkidle');
  270 |     await page.waitForTimeout(1000);
  271 | 
  272 |     // Vào quán ăn
  273 |     const detail = new DetailRestaurantPage(page);
  274 |     await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  275 |     await page.waitForSelector('.item-restaurant-row', { timeout: 20_000 });
  276 | 
  277 |     const itemCountBefore = await detail.getMenuItemCount();
  278 |     expect(itemCountBefore).toBeGreaterThan(0);
  279 | 
  280 |     // Thêm món đầu tiên
  281 |     await detail.addFirstItemToCart(1);
  282 |     console.log('✅ Đã thêm món vào giỏ');
  283 | 
  284 |     // Kiểm tra giỏ hàng
  285 |     const cart = new CartPage(page);
  286 |     await cart.gotoCart();
  287 |     await page.waitForLoadState('networkidle');
  288 | 
  289 |     const cartCount = await cart.getItemCount();
  290 |     console.log(`🛒 Số món trong giỏ: ${cartCount}`);
  291 |     expect(cartCount).toBeGreaterThan(0);
  292 |   });
  293 | 
  294 |   test('[TC-2.13] Tăng số lượng - tổng tiền thay đổi', async ({ page }) => {
  295 |     // Login + thêm món
  296 |     const login = new LoginPage(page);
  297 |     await login.gotoLogin();
  298 |     await login.usernameInput.fill(CUSTOMER.username);
  299 |     await login.passwordInput.fill(CUSTOMER.password);
  300 |     await login.loginButton.click();
  301 |     await page.waitForLoadState('networkidle').catch(() => {});
  302 |     await page.waitForTimeout(2000);
  303 |     if (page.url().includes('/Home/Login')) {
  304 |       console.log('ℹ️ Login không redirect — goto / để set session');
  305 |       await page.goto('/', { waitUntil: 'networkidle', timeout: 15_000 }).catch(() => {});
  306 |     }
  307 | 
  308 |     const detail = new DetailRestaurantPage(page);
  309 |     try {
  310 |       await detail.gotoRestaurant(SEED.restaurantIds.konekoPizza);
  311 |       await page.waitForSelector('.item-restaurant-row', { timeout: 30_000 });
  312 |       await detail.addFirstItemToCart(1);
  313 | 
  314 |       const cart = new CartPage(page);
  315 |       await cart.gotoCart();
  316 |       await page.waitForLoadState('networkidle', { timeout: 20_000 }).catch(() => {});
  317 | 
  318 |       const itemCount = await cart.getItemCount();
  319 |       if (itemCount > 0) {
```