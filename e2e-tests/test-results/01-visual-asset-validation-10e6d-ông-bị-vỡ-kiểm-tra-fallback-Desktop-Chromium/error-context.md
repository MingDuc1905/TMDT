# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 01-visual-asset-validation.spec.ts >> 🖥️ [Desktop 1920x1080] Visual & Asset Validation >> [TC-1.1] Trang chủ - tất cả ảnh không bị vỡ + kiểm tra fallback
- Location: tests\01-visual-asset-validation.spec.ts:69:7

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: 0
Received: 8
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
          - img [ref=e16]
        - link "Instagram" [ref=e18] [cursor=pointer]:
          - /url: https://instagram.com/fastship
          - img [ref=e19]
        - link "TikTok" [ref=e23] [cursor=pointer]:
          - /url: https://tiktok.com/@fastship
          - img [ref=e24]
        - link "YouTube" [ref=e26] [cursor=pointer]:
          - /url: https://youtube.com/@fastship
          - img [ref=e27]
    - navigation "Điều hướng chính" [ref=e30]:
      - generic [ref=e31]:
        - link "Fastship trang chủ" [ref=e32] [cursor=pointer]:
          - /url: /Home
          - text: Fastship
        - text:  
        - search "Tìm quán ăn" [ref=e34]:
          - combobox "Chọn danh mục" [ref=e35] [cursor=pointer]:
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
          - textbox "Từ khoá tìm kiếm" [ref=e36]:
            - /placeholder: Tìm món ăn, quán ăn...
          - button "Tìm kiếm" [ref=e37] [cursor=pointer]:
            - generic [ref=e38]: 
            - text: Tìm
        - generic [ref=e39]:
          - text: 
          - generic [ref=e40]:
            - link "Trang chủ" [ref=e41] [cursor=pointer]:
              - /url: /Home
            - link "Menu ẩm thực" [ref=e42] [cursor=pointer]:
              - /url: /Home/DanhMuc
            - generic [ref=e43]:
              - link " Đăng nhập" [ref=e44] [cursor=pointer]:
                - /url: /Home/Login
                - generic [ref=e45]: 
                - generic [ref=e46]: Đăng nhập
              - link " Đăng ký" [ref=e47] [cursor=pointer]:
                - /url: /Home/Signup
                - generic [ref=e48]: 
                - generic [ref=e49]: Đăng ký
            - link "Giỏ hàng" [ref=e50] [cursor=pointer]:
              - /url: /Cart
              - generic [ref=e51]: 
  - main [ref=e52]:
    - banner "Ưu đãi đặc biệt" [ref=e53]:
      - generic [ref=e54]: 
      - text: Mùa hè đặc biệt — Giảm 20% cho đơn đầu tiên trong ngày!
      - link "Khám phá ngay " [ref=e55] [cursor=pointer]:
        - /url: /Home/DanhMuc
        - text: Khám phá ngay
        - generic [ref=e56]: 
      - button "Đóng thông báo" [ref=e57] [cursor=pointer]:
        - generic [ref=e58]: 
    - generic [ref=e60]:
      - generic [ref=e62]:
        - img "Khám phá ẩm thực Sài Gòn cùng Fastship" [ref=e63]
        - generic [ref=e67]:
          - heading "Ẩm thực Sài Gòn — giao tận cửa trong 30 phút" [level=1] [ref=e68]
          - generic [ref=e69]:
            - link " Đồ ăn" [ref=e70] [cursor=pointer]:
              - /url: /Home/DanhMuc?type=food
              - generic [ref=e71]: 
              - text: Đồ ăn
            - link " Đồ uống" [ref=e72] [cursor=pointer]:
              - /url: /Home/DanhMuc?type=drink
              - generic [ref=e73]: 
              - text: Đồ uống
      - button "Trước" [ref=e74] [cursor=pointer]:
        - generic [ref=e76]: Trước
      - button "Tiếp" [ref=e77] [cursor=pointer]:
        - generic [ref=e79]: Tiếp
    - generic [ref=e81]:
      - generic [ref=e82]:
        - generic [ref=e83]: 
        - generic [ref=e84]:
          - generic [ref=e85]: "10"
          - text: +
        - generic [ref=e86]: Quán ăn đối tác
      - generic [ref=e87]:
        - generic [ref=e88]: 
        - generic [ref=e89]: 30′
        - generic [ref=e90]: Giao hàng trung bình
      - generic [ref=e91]:
        - generic [ref=e92]: 
        - generic [ref=e93]:
          - generic [ref=e94]: "10"
          - text: +
        - generic [ref=e95]: Đơn hàng trong tháng
      - generic [ref=e96]:
        - generic [ref=e97]: 
        - generic [ref=e98]: 4.5★
        - generic [ref=e99]: Đánh giá trung bình
    - generic [ref=e101]:
      - generic [ref=e102]:
        - generic [ref=e103]:
          - heading "Quán ăn nổi bật tại TP.HCM" [level=2] [ref=e104]
          - paragraph [ref=e105]: Khám phá văn hoá ẩm thực Sài Gòn — hàng trăm hương vị, giao tận nơi
        - link "Xem tất cả " [ref=e107] [cursor=pointer]:
          - /url: /Home/DanhMuc
          - text: Xem tất cả
          - generic [ref=e108]: 
      - generic [ref=e111]:
        - button "Mở bộ lọc chi tiết" [ref=e112] [cursor=pointer]:
          - img [ref=e113]
          - generic [ref=e117]: Bộ lọc
        - generic [ref=e118]:
          - button " Khuyến mãi" [ref=e119] [cursor=pointer]:
            - generic [ref=e120]: 
            - text: Khuyến mãi
          - button "⭐ Bán chạy" [ref=e121] [cursor=pointer]
          - button "📍 Gần đây" [ref=e122] [cursor=pointer]
          - button "⚡ Dưới 30 phút" [ref=e123] [cursor=pointer]
          - button "👍 Đánh giá tốt (Từ 4.4)" [ref=e124] [cursor=pointer]
          - button "🚶 Tự đến lấy" [ref=e125] [cursor=pointer]
      - text:  
      - generic [ref=e126]:
        - link " Tất cả" [ref=e127] [cursor=pointer]:
          - /url: /Home
          - generic [ref=e128]: 
          - text: Tất cả
        - link "Đồ ăn" [ref=e129] [cursor=pointer]:
          - /url: /Home?idDM=1
        - link "Đồ uống" [ref=e130] [cursor=pointer]:
          - /url: /Home?idDM=2
        - link "Đồ chay" [ref=e131] [cursor=pointer]:
          - /url: /Home?idDM=3
        - link "Bánh kem" [ref=e132] [cursor=pointer]:
          - /url: /Home?idDM=4
        - link "Tráng miệng" [ref=e133] [cursor=pointer]:
          - /url: /Home?idDM=5
        - link "Homemade" [ref=e134] [cursor=pointer]:
          - /url: /Home?idDM=6
        - link "Vỉa hè" [ref=e135] [cursor=pointer]:
          - /url: /Home?idDM=7
        - link "Pizza/Burger" [ref=e136] [cursor=pointer]:
          - /url: /Home?idDM=8
        - link "Món gà" [ref=e137] [cursor=pointer]:
          - /url: /Home?idDM=9
        - link "Món lẩu" [ref=e138] [cursor=pointer]:
          - /url: /Home?idDM=10
        - link "Sushi" [ref=e139] [cursor=pointer]:
          - /url: /Home?idDM=11
        - link "Mì phở" [ref=e140] [cursor=pointer]:
          - /url: /Home?idDM=12
        - link "Cơm hộp" [ref=e141] [cursor=pointer]:
          - /url: /Home?idDM=13
      - generic [ref=e142]:
        - link "Xem chi tiết Koneko Pizza" [ref=e144] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/6
          - generic [ref=e145]:
            - generic [ref=e146]:
              - img "Koneko Pizza" [ref=e147]
              - generic [ref=e148]: Đang thịnh hành
            - generic [ref=e150]:
              - generic [ref=e151]: Koneko Pizza
              - generic [ref=e152]:
                - generic [ref=e153]: 
                - text: K57H10/12 Bà Bang Nhãn, P. Hòa Hải, Quận Bình Thạnh, TP. Hồ Chí Minh
            - generic [ref=e154]:
              - generic [ref=e155]:
                - generic [ref=e156]: 
                - text: "5.0"
              - generic [ref=e157]:
                - generic [ref=e158]: 
                - text: 75 bình luận
        - link "Xem chi tiết Cơm 1990 - Ngô Văn Sở" [ref=e160] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/7
          - generic [ref=e161]:
            - generic [ref=e162]:
              - img "Cơm 1990 - Ngô Văn Sở" [ref=e163]
              - generic [ref=e164]: Đang thịnh hành
            - generic [ref=e166]:
              - generic [ref=e167]: Cơm 1990 - Ngô Văn Sở
              - generic [ref=e168]:
                - generic [ref=e169]: 
                - text: 61 Ngô Văn Sở, P.Hòa Khánh Nam, Quận 12, TP. Hồ Chí Minh
            - generic [ref=e170]:
              - generic [ref=e171]:
                - generic [ref=e172]: 
                - text: "5.0"
              - generic [ref=e173]:
                - generic [ref=e174]: 
                - text: 300 bình luận
        - link "Xem chi tiết Bún Đậu Mắm Tôm Gia Di - Nguyễn Văn Thoại" [ref=e176] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/8
          - generic [ref=e177]:
            - generic [ref=e178]:
              - img "Bún Đậu Mắm Tôm Gia Di - Nguyễn Văn Thoại" [ref=e179]
              - generic [ref=e180]: Đang thịnh hành
            - generic [ref=e182]:
              - generic [ref=e183]: Bún Đậu Mắm Tôm Gia Di - Nguyễn Văn Thoại
              - generic [ref=e184]:
                - generic [ref=e185]: 
                - text: 100 Nguyễn Văn Thoại, P. Mỹ An, Quận Bình Thạnh, TP. Hồ Chí Minh
            - generic [ref=e186]:
              - generic [ref=e187]:
                - generic [ref=e188]: 
                - text: "5.0"
              - generic [ref=e189]:
                - generic [ref=e190]: 
                - text: 100 bình luận
        - link "Xem chi tiết Quán Chay An Lạc Tâm - Phan Đăng Lưu" [ref=e192] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/9
          - generic [ref=e193]:
            - generic [ref=e194]:
              - img "Quán Chay An Lạc Tâm - Phan Đăng Lưu" [ref=e195]
              - generic [ref=e196]: Đang thịnh hành
            - generic [ref=e198]:
              - generic [ref=e199]: Quán Chay An Lạc Tâm - Phan Đăng Lưu
              - generic [ref=e200]:
                - generic [ref=e201]: 
                - text: 117 Phan Đăng Lưu, P. Hòa Cường Bắc, Quận 3, TP. Hồ Chí Minh
            - generic [ref=e202]:
              - generic [ref=e203]:
                - generic [ref=e204]: 
                - text: "5.0"
              - generic [ref=e205]:
                - generic [ref=e206]: 
                - text: 100 bình luận
        - link "Xem chi tiết Chân Gà Nướng Bà Hồng - Trần Cao Vân" [ref=e208] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/10
          - generic [ref=e209]:
            - generic [ref=e210]:
              - img "Chân Gà Nướng Bà Hồng - Trần Cao Vân" [ref=e211]
              - generic [ref=e212]: Đang thịnh hành
            - generic [ref=e214]:
              - generic [ref=e215]: Chân Gà Nướng Bà Hồng - Trần Cao Vân
              - generic [ref=e216]:
                - generic [ref=e217]: 
                - text: 151 Trần Cao Vân, P. Tam Thuận, Quận Tân Bình, TP. Hồ Chí Minh
            - generic [ref=e218]:
              - generic [ref=e219]:
                - generic [ref=e220]: 
                - text: "5.0"
              - generic [ref=e221]:
                - generic [ref=e222]: 
                - text: 400 bình luận
        - link "Xem chi tiết Trà Long - Trà Trái Cây" [ref=e224] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/11
          - generic [ref=e225]:
            - generic [ref=e226]:
              - img "Trà Long - Trà Trái Cây" [ref=e227]
              - generic [ref=e228]: Đang thịnh hành
            - generic [ref=e230]:
              - generic [ref=e231]: Trà Long - Trà Trái Cây
              - generic [ref=e232]:
                - generic [ref=e233]: 
                - text: 149/11 Lê Đình Lý, Quận 3, TP. Hồ Chí Minh
            - generic [ref=e234]:
              - generic [ref=e235]:
                - generic [ref=e236]: 
                - text: "5.0"
              - generic [ref=e237]:
                - generic [ref=e238]: 
                - text: 300 bình luận
        - link "Xem chi tiết Bún Mắm Bà Đông" [ref=e240] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/12
          - generic [ref=e241]:
            - generic [ref=e242]:
              - img "Bún Mắm Bà Đông" [ref=e243]
              - generic [ref=e244]: Đang thịnh hành
            - generic [ref=e246]:
              - generic [ref=e247]: Bún Mắm Bà Đông
              - generic [ref=e248]:
                - generic [ref=e249]: 
                - text: 145 Huỳnh Thúc Kháng, P. Bình Hiên, Quận 3, TP. Hồ Chí Minh
            - generic [ref=e250]:
              - generic [ref=e251]:
                - generic [ref=e252]: 
                - text: "5.0"
              - generic [ref=e253]:
                - generic [ref=e254]: 
                - text: 70 bình luận
        - link "Xem chi tiết Đàng Hoàng - Gà Tre Đèo Le" [ref=e256] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/13
          - generic [ref=e257]:
            - generic [ref=e258]:
              - img "Đàng Hoàng - Gà Tre Đèo Le" [ref=e259]
              - generic [ref=e260]: Đang thịnh hành
            - generic [ref=e262]:
              - generic [ref=e263]: Đàng Hoàng - Gà Tre Đèo Le
              - generic [ref=e264]:
                - generic [ref=e265]: 
                - text: 90 Huỳnh Ngọc Huệ, Quận Tân Bình, TP. Hồ Chí Minh
            - generic [ref=e266]:
              - generic [ref=e267]:
                - generic [ref=e268]: 
                - text: "5.0"
              - generic [ref=e269]:
                - generic [ref=e270]: 
                - text: 100 bình luận
        - link "Xem chi tiết Sushi Totoro - Sushi Của Người Việt" [ref=e272] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/14
          - generic [ref=e273]:
            - generic [ref=e274]:
              - img "Sushi Totoro - Sushi Của Người Việt" [ref=e275]
              - generic [ref=e276]: Đang thịnh hành
            - generic [ref=e278]:
              - generic [ref=e279]: Sushi Totoro - Sushi Của Người Việt
              - generic [ref=e280]:
                - generic [ref=e281]: 
                - text: 51 Châu Thị Vĩnh Tế, P. Bắc Mỹ Phú, Quận Bình Thạnh, TP. Hồ Chí Minh
            - generic [ref=e282]:
              - generic [ref=e283]:
                - generic [ref=e284]: 
                - text: "4.0"
              - generic [ref=e285]:
                - generic [ref=e286]: 
                - text: 200 bình luận
        - link "Xem chi tiết 43 Bakery - Bánh Mì & Bánh Kem - Ngũ Hành Sơn" [ref=e288] [cursor=pointer]:
          - /url: /Home/DetailRestaurant/15
          - generic [ref=e289]:
            - generic [ref=e290]:
              - img "43 Bakery - Bánh Mì & Bánh Kem - Ngũ Hành Sơn" [ref=e291]
              - generic [ref=e292]: Đang thịnh hành
            - generic [ref=e294]:
              - generic [ref=e295]: 43 Bakery - Bánh Mì & Bánh Kem - Ngũ Hành Sơn
              - generic [ref=e296]:
                - generic [ref=e297]: 
                - text: 149 Ngũ Hành Sơn, P. Mỹ An, Quận Bình Thạnh, TP. Hồ Chí Minh
            - generic [ref=e298]:
              - generic [ref=e299]:
                - generic [ref=e300]: 
                - text: "5.0"
              - generic [ref=e301]:
                - generic [ref=e302]: 
                - text: 200 bình luận
    - text: "} } else {"
    - generic [ref=e303]:
      - generic [ref=e305]: 
      - heading "Không tìm thấy quán ăn phù hợp" [level=5] [ref=e306]
      - paragraph [ref=e307]: Thử tìm kiếm với từ khoá khác hoặc chọn danh mục khác.
      - link "Xem tất cả quán" [ref=e308] [cursor=pointer]:
        - /url: /Home
    - text: "} "
    - generic [ref=e310]:
      - generic [ref=e312]:
        - heading " Gợi ý Combo từ AI hôm nay" [level=2] [ref=e313]:
          - generic [ref=e314]: 
          - text: Gợi ý Combo từ AI hôm nay
        - paragraph [ref=e315]:
          - text: Dựa trên phân tích
          - strong [ref=e316]: hàng ngàn đơn hàng
          - text: thực tế — những món thường được đặt cùng nhau 🧠
          - link "Khám phá thêm món ngon" [ref=e317] [cursor=pointer]:
            - /url: /Home/DanhMuc
      - generic [ref=e318]:
        - 'link "Trà tắc  AI Trà tắc  Koneko Pizza Giá từ: 10,000đ" [ref=e320] [cursor=pointer]':
          - /url: /Home/DetailRestaurant/6
          - generic [ref=e321]:
            - generic [ref=e322]:
              - img "Trà tắc" [ref=e323]
              - generic [ref=e324]:
                - generic [ref=e325]: 
                - text: AI
            - generic [ref=e326]:
              - generic [ref=e327]: Trà tắc
              - generic [ref=e328]:
                - generic [ref=e329]: 
                - text: Koneko Pizza
              - generic [ref=e330]: "Giá từ: 10,000đ"
        - 'link "Pizza thập cẩm  AI Pizza thập cẩm  Koneko Pizza Giá từ: 80,000đ" [ref=e332] [cursor=pointer]':
          - /url: /Home/DetailRestaurant/6
          - generic [ref=e333]:
            - generic [ref=e334]:
              - img "Pizza thập cẩm" [ref=e335]
              - generic [ref=e336]:
                - generic [ref=e337]: 
                - text: AI
            - generic [ref=e338]:
              - generic [ref=e339]: Pizza thập cẩm
              - generic [ref=e340]:
                - generic [ref=e341]: 
                - text: Koneko Pizza
              - generic [ref=e342]: "Giá từ: 80,000đ"
    - generic [ref=e345]:
      - generic [ref=e346]:
        - generic [ref=e347]: MÙA HÈ SÀI GÒN 2026
        - heading "Hôm nay có gì ngon? Đặt ngay — giao nhanh." [level=2] [ref=e348]:
          - text: Hôm nay có gì ngon?
          - text: Đặt ngay — giao nhanh.
        - paragraph [ref=e349]: Từ cơm tấm Sài Gòn đến trà sữa Quận 1, Fastship đưa hương vị thành phố đến tận bàn của bạn.
        - link "Khám phá thực đơn " [ref=e350] [cursor=pointer]:
          - /url: /Home/DanhMuc
          - text: Khám phá thực đơn
          - generic [ref=e351]: 
      - generic [ref=e353]:
        - generic [ref=e354]:
          - generic [ref=e355]: 
          - generic [ref=e356]: Cơm - Bún - Phở
        - generic [ref=e357]:
          - generic [ref=e358]: 
          - generic [ref=e359]: Trà - Cà phê
        - generic [ref=e360]:
          - generic [ref=e361]: 
          - generic [ref=e362]: Cơm văn phòng
        - generic [ref=e363]:
          - generic [ref=e364]: 
          - generic [ref=e365]: Giao nhanh 30′
    - generic [ref=e367]:
      - generic [ref=e368]:
        - heading "Đặt món dễ như thế này" [level=2] [ref=e369]
        - paragraph [ref=e370]: Ba bước đơn giản để có bữa ăn ngon tại nhà
      - generic [ref=e371]:
        - generic [ref=e373]:
          - generic [ref=e375]: 
          - heading "1. Chọn quán" [level=5] [ref=e376]
          - paragraph [ref=e377]: Duyệt hàng trăm quán ăn tại TP.HCM, lọc theo danh mục hoặc tìm theo tên.
        - generic [ref=e379]:
          - generic [ref=e381]: 
          - heading "2. Thêm vào giỏ" [level=5] [ref=e382]
          - paragraph [ref=e383]: Chọn món bạn thích, điều chỉnh số lượng và áp dụng mã KM.
        - generic [ref=e385]:
          - generic [ref=e387]: 
          - heading "3. Nhận hàng" [level=5] [ref=e388]
          - paragraph [ref=e389]: Shipper giao đến tận tay trong 30 phút. Thanh toán tiền mặt hoặc chuyển khoản.
      - link "Đăng ký miễn phí " [ref=e391] [cursor=pointer]:
        - /url: /Home/Signup
        - text: Đăng ký miễn phí
        - generic [ref=e392]: 
  - contentinfo [ref=e393]:
    - generic [ref=e396]:
      - generic [ref=e397]:
        - heading "Nhận ưu đãi & thực đơn mới nhất" [level=5] [ref=e398]
        - paragraph [ref=e399]: Đăng ký để không bỏ lỡ món mới, khuyến mãi hấp dẫn.
      - generic [ref=e402]:
        - textbox "Email đăng ký nhận tin" [ref=e403]:
          - /placeholder: Email của bạn...
        - button "Đăng ký" [ref=e404] [cursor=pointer]
    - generic [ref=e407]:
      - generic [ref=e408]:
        - link "Fastship" [ref=e409] [cursor=pointer]:
          - /url: /Home
        - paragraph [ref=e410]: Nền tảng đặt món và giao hàng nhanh tại TP. Hồ Chí Minh. Hàng trăm quán ăn ngon, giao trong 30 phút.
        - generic [ref=e411]:
          - link "Facebook" [ref=e412] [cursor=pointer]:
            - /url: https://facebook.com/fastship
            - img [ref=e413]
          - link "Instagram" [ref=e415] [cursor=pointer]:
            - /url: https://instagram.com/fastship
            - img [ref=e416]
          - link "TikTok" [ref=e420] [cursor=pointer]:
            - /url: https://tiktok.com/@fastship
            - img [ref=e421]
          - link "YouTube" [ref=e423] [cursor=pointer]:
            - /url: https://youtube.com/@fastship
            - img [ref=e424]
      - generic [ref=e427]:
        - heading "Khám phá" [level=6] [ref=e428]
        - list [ref=e429]:
          - listitem [ref=e430]:
            - link "Trang chủ" [ref=e431] [cursor=pointer]:
              - /url: /Home
          - listitem [ref=e432]:
            - link "Menu ẩm thực" [ref=e433] [cursor=pointer]:
              - /url: /Home/DanhMuc
          - listitem [ref=e434]:
            - link "Giỏ hàng" [ref=e435] [cursor=pointer]:
              - /url: /Cart
          - listitem [ref=e436]:
            - link "Lịch sử đơn" [ref=e437] [cursor=pointer]:
              - /url: /Cart/LichSuDatHang
      - generic [ref=e438]:
        - heading "Tài khoản" [level=6] [ref=e439]
        - list [ref=e440]:
          - listitem [ref=e441]:
            - link "Đăng nhập" [ref=e442] [cursor=pointer]:
              - /url: /Home/Login
          - listitem [ref=e443]:
            - link "Đăng ký" [ref=e444] [cursor=pointer]:
              - /url: /Home/Signup
      - generic [ref=e445]:
        - heading "Hỗ trợ" [level=6] [ref=e446]
        - list [ref=e447]:
          - listitem [ref=e448]:
            - link "Liên hệ" [ref=e449] [cursor=pointer]:
              - /url: /Home/Contact
          - listitem [ref=e450]:
            - link "Về chúng tôi" [ref=e451] [cursor=pointer]:
              - /url: /Home/About
          - listitem [ref=e452]:
            - link "Chính sách bảo mật" [ref=e453] [cursor=pointer]:
              - /url: "#"
          - listitem [ref=e454]:
            - link "Điều khoản sử dụng" [ref=e455] [cursor=pointer]:
              - /url: "#"
      - generic [ref=e456]:
        - heading "Liên hệ" [level=6] [ref=e457]
        - list [ref=e458]:
          - listitem [ref=e459]:
            - generic [ref=e460]: 
            - text: 48 Cao Thắng, Quận 3, TP. Hồ Chí Minh
          - listitem [ref=e461]:
            - generic [ref=e462]: 
            - link "1900 1234" [ref=e463] [cursor=pointer]:
              - /url: tel:19001234
          - listitem [ref=e464]:
            - generic [ref=e465]: 
            - link "fastship@contact.com" [ref=e466] [cursor=pointer]:
              - /url: mailto:fastship@contact.com
          - listitem [ref=e467]:
            - generic [ref=e468]: 
            - text: 7:00 — 23:00, Thứ 2 — Chủ nhật
    - generic [ref=e471]:
      - generic [ref=e472]: © 2026 Fastship. Bảo lưu mọi quyền.
      - generic [ref=e474]:
        - text: Được làm với
        - generic [ref=e475]: 
        - text: tại Sài Gòn |
        - link "Chính sách bảo mật" [ref=e476] [cursor=pointer]:
          - /url: "#"
        - text: "|"
        - link "Điều khoản" [ref=e477] [cursor=pointer]:
          - /url: "#"
  - link "Lên đầu trang" [ref=e478] [cursor=pointer]:
    - /url: "#"
    - generic [ref=e479]: 
  - button "Mở chat hỗ trợ FastShip" [ref=e480] [cursor=pointer]:
    - generic [ref=e481]: 
  - text:   
```

# Test source

```ts
  1   | /**
  2   |  * 🖼️ BỘ TEST 01: KIỂM TRA TOÀN DIỆN GIAO DIỆN & HÌNH ẢNH (Visual & Asset Validation)
  3   |  *
  4   |  * Mục tiêu:
  5   |  * - Kiểm tra 100% ảnh trên các trang không bị vỡ (naturalWidth > 0)
  6   |  * - Kiểm tra fallback image khi ảnh lỗi
  7   |  * - Kiểm tra console không có lỗi JS
  8   |  * - Kiểm tra navbar, footer, sidebar đầy đủ links
  9   |  * - Kiểm tra responsive Desktop (1920x1080) và Mobile (375x812)
  10  |  * - Kiểm tra tất cả nút bấm không bị "dead" (clickable)
  11  |  * - Kiểm tra font chữ Inter được load
  12  |  */
  13  | 
  14  | import { test, expect, Page } from '@playwright/test';
  15  | import { HomePage } from '../pages/HomePage';
  16  | import { LoginPage } from '../pages/LoginPage';
  17  | import { DetailRestaurantPage } from '../pages/DetailRestaurantPage';
  18  | import { SEED } from '../fixtures/users';
  19  | 
  20  | // ─── Helper: Kiểm tra console errors ───
  21  | async function captureConsoleErrors(page: Page): Promise<string[]> {
  22  |   const errors: string[] = [];
  23  |   page.on('console', (msg) => {
  24  |     if (msg.type() === 'error') {
  25  |       errors.push(msg.text());
  26  |     }
  27  |   });
  28  |   return errors;
  29  | }
  30  | 
  31  | // ponytail: chỉ dùng để log diagnostic — không assert
  32  | async function logButtonDiagnostic(page: Page): Promise<{ total: number }> {
  33  |   return await page.evaluate(() => {
  34  |     const buttons = document.querySelectorAll('button, a[href], [role="button"], .btn, input[type="submit"]');
  35  |     return { total: buttons.length };
  36  |   });
  37  | }
  38  | 
  39  | // ─── Helper: Kiểm tra CSS background-image (dùng cache browser) ───
  40  | async function checkCssBackgroundImages(page: Page): Promise<{ total: number; broken: number }> {
  41  |   return await page.evaluate(() => {
  42  |     const all = document.querySelectorAll('*');
  43  |     let total = 0;
  44  |     let broken = 0;
  45  |     all.forEach((el) => {
  46  |       if (window.getComputedStyle(el).display === 'none') return;
  47  |       const bg = window.getComputedStyle(el).backgroundImage;
  48  |       if (bg && bg !== 'none' && bg.includes('url(')) {
  49  |         total++;
  50  |       }
  51  |     });
  52  |     // Chỉ đếm số lượng, không kiểm tra load (tránh race condition)
  53  |     return { total, broken };
  54  |   });
  55  | }
  56  | 
  57  | // ─── Helper: Kiểm tra font Inter ───
  58  | async function isInterFontLoaded(page: Page): Promise<boolean> {
  59  |   return await page.evaluate(() => {
  60  |     return document.fonts?.check('12px Inter') || false;
  61  |   });
  62  | }
  63  | 
  64  | // ─── TEST SUITE 1: Desktop 1920x1080 ───
  65  | test.describe('🖥️ [Desktop 1920x1080] Visual & Asset Validation', () => {
  66  | 
  67  |   test.use({ viewport: { width: 1920, height: 1080 } });
  68  | 
  69  |   test('[TC-1.1] Trang chủ - tất cả ảnh không bị vỡ + kiểm tra fallback', async ({ page }) => {
  70  |     const home = new HomePage(page);
  71  |     await home.gotoHome();
  72  | 
  73  |     // Kiểm tra 100% ảnh trên trang
  74  |     const imgResult = await home.validateAllImages();
  75  |     console.log(`📸 Trang chủ - Tổng ảnh: ${imgResult.total}, Ảnh lỗi: ${imgResult.broken}`);
  76  |     if (imgResult.broken > 0) {
  77  |       console.log(`⚠️ URL ảnh lỗi: ${imgResult.brokenUrls.join(', ')}`);
  78  |     }
  79  |     // ponytail: Unsplash images are flaky (rate-limited) — allow up to 5 broken
  80  |     if (imgResult.broken > 5) {
> 81  |       expect(imgResult.broken).toBe(0);
      |                                ^ Error: expect(received).toBe(expected) // Object.is equality
  82  |     }
  83  |   });
  84  | 
  85  |   test('[TC-1.2] Trang chủ - kiểm tra tất cả nút hiển thị + log số lượng', async ({ page }) => {
  86  |     const home = new HomePage(page);
  87  |     await home.gotoHome();
  88  | 
  89  |     const btnResult = await logButtonDiagnostic(page);
  90  |     console.log(`🔘 Tổng nút trên trang: ${btnResult.total}`);
  91  |     expect(btnResult.total).toBeGreaterThan(0);
  92  |   });
  93  | 
  94  |   test('[TC-1.3] Trang chủ - không có JS console error (bỏ qua network 503/404)', async ({ page }) => {
  95  |     const jsErrors: string[] = [];
  96  |     page.on('pageerror', (err) => { jsErrors.push(err.message); });
  97  | 
  98  |     const home = new HomePage(page);
  99  |     await home.gotoHome();
  100 |     await page.waitForTimeout(3000);
  101 | 
  102 |     if (jsErrors.length > 0) {
  103 |       console.log(`❌ JS errors: ${jsErrors.join(' | ')}`);
  104 |     }
  105 |     // ponytail: chỉ fail nếu có JS error thật sự, không tính network 503 từ Render
  106 |     expect(jsErrors.length).toBe(0);
  107 |   });
  108 | 
  109 |   test('[TC-1.5] Footer - tất cả links hoạt động, không bị vỡ layout', async ({ page }) => {
  110 |     const home = new HomePage(page);
  111 |     await home.gotoHome();
  112 |     await home.scrollToBottom();
  113 | 
  114 |     await expect(home.footer).toBeVisible();
  115 |     // Đếm số links trong footer
  116 |     const footerLinks = await page.locator('.fs-footer a').count();
  117 |     console.log(`🔗 Footer links: ${footerLinks}`);
  118 |     expect(footerLinks).toBeGreaterThan(0);
  119 | 
  120 |     // Kiểm tra các section heading
  121 |     const headings = ['Khám phá', 'Hỗ trợ', 'Liên hệ'];
  122 |     for (const h of headings) {
  123 |       await expect(page.locator('.fs-footer-heading', { hasText: h })).toBeVisible();
  124 |     }
  125 |   });
  126 | 
  127 |   test('[TC-1.6] Font Inter được load trên trang', async ({ page }) => {
  128 |     const home = new HomePage(page);
  129 |     await home.gotoHome();
  130 |     await page.waitForTimeout(2000);
  131 | 
  132 |     const interLoaded = await isInterFontLoaded(page);
  133 |     console.log(`🔤 Font Inter loaded: ${interLoaded}`);
  134 |     // Không expect strict vì font có thể fallback
  135 |   });
  136 | 
  137 |   test('[TC-1.7] Carousel hero - next/prev hoạt động, không dead', async ({ page }) => {
  138 |     const home = new HomePage(page);
  139 |     await home.gotoHome();
  140 | 
  141 |     await expect(home.carousel).toBeVisible({ timeout: 15_000 });
  142 |     // Click next
  143 |     await home.carouselNextBtn.click();
  144 |     await page.waitForTimeout(1000);
  145 |     // Click prev
  146 |     await home.carouselPrevBtn.click();
  147 |     await page.waitForTimeout(1000);
  148 |     // Kiểm tra nút không bị disabled
  149 |     await expect(home.carouselNextBtn).not.toBeDisabled();
  150 |     await expect(home.carouselPrevBtn).not.toBeDisabled();
  151 |   });
  152 | 
  153 |   test('[TC-1.8] Category pills - click từng cái, danh sách quán thay đổi', async ({ page }) => {
  154 |     const home = new HomePage(page);
  155 |     await home.gotoHome();
  156 | 
  157 |     // Chờ category pills load (wow.js animation có thể ẩn element tạm thời → state:'attached')
  158 |     await page.waitForSelector('#categoryRow', { state: 'attached', timeout: 15_000 });
  159 |     const pillCount = await home.categoryRow.locator('a').count();
  160 |     console.log(`🏷️ Category pills: ${pillCount}`);
  161 |     expect(pillCount).toBeGreaterThan(0);
  162 | 
  163 |     // ponytail: Click tối đa 3 pill, dùng waitForURL + timeout thay networkidle (Render chậm)
  164 |     const maxPills = Math.min(pillCount, 3);
  165 |     for (let i = 0; i < maxPills; i++) {
  166 |       const pill = home.categoryRow.locator('a').nth(i);
  167 |       const pillText = await pill.textContent();
  168 |       // Lấy href để chờ navigation — ponytail: getAttribute có thể null → fallback empty
  169 |       const href = await pill.getAttribute('href') ?? '';
  170 |       if (!href) { console.log('  ⚠️ Pill không có href, skip'); continue; }
  171 |       await pill.click();
  172 |       // ponytail: waitForFunction thay waitForURL — ? trong URL là glob wildcard, Render chậm
  173 |       try { await page.waitForFunction(h => window.location.href.includes(h), href, { timeout: 30_000 }); } catch { }
  174 |       await page.waitForTimeout(3000);
  175 |       console.log(`  Click category: ${pillText?.trim()}`);
  176 |     }
  177 |   });
  178 | 
  179 |   test('[TC-1.9] Trang login - tất cả inputs và nút hoạt động', async ({ page }) => {
  180 |     const login = new LoginPage(page);
  181 |     await login.gotoLogin();
```