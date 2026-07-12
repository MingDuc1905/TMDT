-- ===============================================================
-- FASTSHIP - PostgreSQL Database Seed Data
-- For use with Render PostgreSQL or local development
-- Tables are created by EF Core EnsureCreated() / Migrate()
-- This file contains only INSERT data statements
-- ===============================================================

-- ==================== tbUser ====================
INSERT INTO "tbUser" ("userid", "username", "pwd", "loaitaikhoan", "sdt", "vitien", "email", "trangthai") VALUES
(1,  'tranthib',       'abcdef',        'Khách hàng', '0987654321', 2000.0000,    'tranthib@example.com', 1),
(2,  'levanc',         'qwerty',        'Khách hàng', '0901234567', 1500.0000,    'levanc@example.com', 1),
(3,  'shippery',       'shipy456',      'Shipper',     '0955555555', 1000.0000,    'shippery@gmail.com', 1),
(4,  'shipperz',       'shipz789',      'Shipper',     '0966666666', 1200.0000,    'shipperz@gmail.com', 1),
(5,  'phamthid',       'xyz123',        'Khách hàng', '0977777777', 1800.0000,    'phamthid@example.com', 1),
(6,  'konekopizza',    'konekopizza',   'Quán ăn',    '0922227262', 10000.0000,   'konekopizza@gmail.com', 1),
(7,  'com1990nvs',     'com1990nvs',    'Quán ăn',    '0632563451', 10000000.0000, 'com1990@gmail.com', 1),
(8,  'bundaugiadi',    'bundaugiadi',   'Quán ăn',    '0632586299', 50000000.0000, 'bundaugiadi@gmail.com', 1),
(9,  'quanchayanlactam', 'quanchayanlactam', 'Quán ăn', '0986123487', 4000000.0000,  'chayanlactam@gmail.com', 1),
(10, 'changanuongbahong', 'changanuongbahong', 'Quán ăn', '0728973833', 130000000.0000, 'changanuongbahong@gmail.com', 1),
(11, 'tralong',        'tralong',       'Quán ăn',    '0286472897', 130000000.0000, 'tralong@gmail.com', 1),
(12, 'bunmambadong',   'bunmambadong',  'Quán ăn',    '0905816478', 130000000.0000, 'bunmambadong@gmail.com', 1),
(13, 'danghoanggatre', 'danghoanggatre', 'Quán ăn',   '0902123654', 130000000.0000, 'danghoanggatre@gmail.com', 1),
(14, 'sushitotoro',    'sushitotoro',   'Quán ăn',    '0286677897', 130000000.0000, 'sushitotoro@gmail.com', 1),
(15, '43bakery',       '43bakery',      'Quán ăn',    '0164982356', 30000000.0000,  '43bakery@gmail.com', 1),
(16, 'admin1',         'admin1',        'Admin',       '0902122309', 0.0000,        'admin1@gmail.com', 1),
(17, 'admin2',         'admin2',        'Admin',       '0286673478', 0.0000,        'admin2@gmail.com', 1),
(18, 'admin3',         'admin3',        'Admin',       '0383766899', 0.0000,        'admin3@gmail.com', 1);

-- ==================== tbAdmin ====================
INSERT INTO "tbAdmin" ("userid", "tenadmin") VALUES
(16, 'Admin 1'),
(17, 'Admin 2'),
(18, 'Admin 3');

-- ==================== tbKhachHang ====================
INSERT INTO "tbKhachHang" ("userid", "tenkh") VALUES
(1, 'Tran Thi B'),
(2, 'Le Van C'),
(5, 'Pham Thi D');

-- ==================== tbShipper ====================
INSERT INTO "tbShipper" ("userid", "tenshipper", "diachi", "toado", "diemdanhgia", "soluotdanhgia", "trangthai", "hinhanh") VALUES
(3, 'Le Van Y', '48 Cao Thắng, P.Thanh Bình, Quận 3, TP. Hồ Chí Minh', NULL, 4, 80, 'Không hoạt động', 'https://images.unsplash.com/photo-1599566150163-29194dcaad36?w=100&h=100&fit=crop'),
(4, 'Nguyen Thi Z', '80 Huỳnh Ngọc Huệ,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 5, 120, 'Đang hoạt động', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&h=100&fit=crop');

-- ==================== tbQuanAn ====================
INSERT INTO "tbQuanAn" ("userid", "tenquanan", "diachi", "toado", "soluotdanhgia", "diemdanhgia", "trangthai", "hinhanh") VALUES
(6, 'Koneko Pizza', 'K57H10/12 Bà Bang Nhãn, P. Hòa Hải,  Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 75, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&h=300&fit=crop'),
(7, 'Cơm 1990 - Ngô Văn Sở', '61 Ngô Văn Sở, P.Hòa Khánh Nam,  Quận 12, TP. Hồ Chí Minh', NULL, 300, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop'),
(8, 'Bún Đậu Mắm Tôm Gia Di - Nguyễn Văn Thoại', '100 Nguyễn Văn Thoại, P. Mỹ An,  Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'),
(9, 'Quán Chay An Lạc Tâm - Phan Đăng Lưu', '117 Phan Đăng Lưu, P. Hòa Cường Bắc,  Quận 3, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop'),
(10, 'Chân Gà Nướng Bà Hồng - Trần Cao Vân', '151 Trần Cao Vân, P. Tam Thuận,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 400, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop'),
(11, 'Trà Long - Trà Trái Cây', '149/11 Lê Đình Lý,  Quận 3, TP. Hồ Chí Minh', NULL, 300, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop'),
(12, 'Bún Mắm Bà Đông', '145 Huỳnh Thúc Kháng, P. Bình Hiên,  Quận 3, TP. Hồ Chí Minh', NULL, 70, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop'),
(13, 'Đàng Hoàng - Gà Tre Đèo Le', '90 Huỳnh Ngọc Huệ,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'),
(14, 'Sushi Totoro - Sushi Của Người Việt', '51 Châu Thị Vĩnh Tế, P. Bắc Mỹ Phú, Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 200, 4, 'Đang mở cửa', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'),
(15, '43 Bakery - Bánh Mì & Bánh Kem - Ngũ Hành Sơn', '149 Ngũ Hành Sơn, P. Mỹ An, Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 200, 5, 'Đang mở cửa', 'https://images.unsplash.com/photo-1509365465985-25d11c17e812?w=400&h=300&fit=crop');

-- ==================== tbDanhMuc ====================
INSERT INTO "tbDanhMuc" ("madanhmuc", "tendanhmuc", "mota", "hinhanh") VALUES
(1, 'Đồ ăn', 'Các món ăn khác nhau từ nhiều nền văn hóa và phong cách ẩm thực khác nhau.', 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&h=300&fit=crop'),
(2, 'Đồ uống', 'Các loại đồ uống như nước ngọt, nước trái cây, nước ép, nước lọc, trà, cà phê, cocktail, và nhiều loại đồ uống khác.', 'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop'),
(3, 'Đồ chay', 'Các món ăn không chứa thịt hoặc bất kỳ sản phẩm động vật nào.', 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop'),
(4, 'Bánh kem', 'Bánh kem là một loại bánh ngọt được làm từ các thành phần như bột mì, đường, trứng và sữa, thường được trang trí với kem và nhiều loại topping khác nhau.', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'),
(5, 'Tráng miệng', 'Các món như bánh ngọt, kem, hoa quả hoặc các loại đặc sản ngọt khác.', 'https://images.unsplash.com/photo-1551024601-bec78aea704b?w=400&h=300&fit=crop'),
(6, 'Homemade', 'Đồ ăn tự làm được chế biến tại nhà hoặc do các nhà hàng địa phương chế biến một cách thủ công và truyền thống.', 'https://images.unsplash.com/photo-1495521821757-a1efb6729352?w=400&h=300&fit=crop'),
(7, 'Vỉa hè', 'Các loại thức ăn đường phố hoặc các món ăn nhanh phổ biến', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'),
(8, 'Pizza/Burger', 'Các loại thức ăn nhanh phổ biến, thường được làm từ bánh mì, thịt, rau cải và các loại sốt.', 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop'),
(9, 'Món gà', 'Các món ăn chế biến từ thịt gà, bao gồm cả các món ăn nhanh và các món truyền thống.', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'),
(10, 'Món lẩu', 'Một loại món ăn được chế biến trong một nồi lớn có nước dùng nấu sôi, thường bao gồm thịt, hải sản, rau cải và các loại gia vị.', 'https://images.unsplash.com/photo-1546737013-1ac7f1f35b10?w=400&h=300&fit=crop'),
(11, 'Sushi', 'Món sushi thường được chế biến từ cơm trộn giấm kết hợp với các nguyên liệu khác nhau như hải sản, cá, hải sản tươi sống.', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'),
(12, 'Mì phở', 'Mì phở bao gồm một tô nước dùng sôi và bún mì phở.', 'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop'),
(13, 'Cơm hộp', 'Cơm hộp thường một phần cơm trắng, kèm theo các loại thức ăn khác nhau được sắp xếp ngăn nắp trong một hộp đựng cơm.', 'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop');

-- ==================== tbMonAn ====================
INSERT INTO "tbMonAn" ("mamon", "tenmon", "mota", "hinhanh", "maquanan", "madanhmuc", "conhang") VALUES
(1, 'Trà tắc', 'Trà và tắc.', 'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop', 6, 2, true),
(2, 'Pizza thập cẩm', 'Thịt, xúc xích, ớt chuông, bắp và phô mai', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&h=300&fit=crop', 6, 8, true),
(3, 'Pizza Bò', 'Bò, ớt chuông, bắp và phô mai', 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop', 6, 8, true),
(4, 'Pizza xúc xích', 'Xúc xích, bắp, phô mai và ớt chuông', 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop', 6, 8, true),
(5, 'Pizza hải sản', 'Tôm, mực, ớt chuông, bắp, phô mai', 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop', 6, 8, true),
(6, 'Cơm trắng + đậu nhồi thịt sốt cà chua', 'Món phụ ăn kèm', 'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop', 7, 13, true),
(7, 'Cơm trắng + sườn xào chua ngọt', 'Món phụ ăn kèm', 'https://images.unsplash.com/photo-1559847844-5315695dadae?w=400&h=300&fit=crop', 7, 13, true),
(8, 'Combo cơm gà rang xả ớt + nước', 'Coca hoặc trà tắc hoặc nước khoáng lạt', 'https://images.unsplash.com/photo-1580477667995-05b94f944cd3?w=400&h=300&fit=crop', 7, 1, true),
(9, 'Cơm trắng + đậu nhồi thịt + rau xào theo ngày', 'Rau theo mùa', 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop', 7, 13, true),
(10, 'Coca', 'Nước ngọt', 'https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop', 7, 2, true),
(11, 'Mẹt A', 'Bao gồm: thịt luộc + đậu khuôn + chả quế + chả cốm', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop', 8, 12, true),
(12, 'Mẹt B', 'Bao gồm: Bún, đậu khuôn, thịt, chả cốm, chả quế, nem rán, phèo luộc', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop', 8, 12, true),
(13, 'Mẹt C', 'Bao gồm: bún, đậu khuôn, thịt, chả cốm, dồi, phèo luộc, lưỡi, nem rán', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop', 8, 12, true),
(14, 'Mẹt nem nướng cuốn', 'Nem nướng Nha Trang', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop', 8, 1, true),
(15, 'Mẹt cuốn tá lá', 'Ram tôm đất + Nem nướng Nha Trang', 'https://images.unsplash.com/photo-1603073163308-9654c3fb70b5?w=400&h=300&fit=crop', 8, 1, true),
(16, 'Chà Bông Chay', 'Trộn với cơm trắng hoặc thêm rong biển sấy và đậu phộng muối', 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop', 9, 3, true),
(17, 'Nấm rim mè', 'Có thể trộn gỏi, ăn cùng cơm trắng hoặc ăn vặt đều ngon', 'https://images.unsplash.com/photo-1559847844-5315695dadae?w=400&h=300&fit=crop', 9, 13, true),
(18, 'Cơm Ngọc Bích', 'Cơm trộn nước cốt dừa và cải bó xôi xay nhuyễn', 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop', 9, 3, true),
(19, 'Nấm Sốt Bơ Tỏi', 'Nấm đùi gà sốt bơ tỏi, ăn kèm bánh mì', 'https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=400&h=300&fit=crop', 9, 3, true),
(20, 'Mì quảng', 'Hương vị đậm đà từ rau củ, nghệ, đậu phộng và bánh tráng', 'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop', 9, 3, true),
(21, 'Chân gà nướng', '3 cặp', 'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop', 10, 7, true),
(22, 'Cánh gà nướng', '2 cánh', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 10, 7, true),
(23, 'Thịt xiên nướng', '5 xiên', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop', 10, 7, true),
(24, 'Chim cút nướng', '2 con', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 10, 7, true),
(25, 'Ếch nướng', '2 con', 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop', 10, 7, true),
(26, 'Trà Mãng Cầu', 'Bánh flan, khúc bạch, pudding, thạch pho mai, củ năng, trân châu', 'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop', 11, 2, true),
(27, 'Trà Mãng Cầu', 'Chỉ có 1 size', 'https://images.unsplash.com/photo-1571934811356-5cc061b6821f?w=400&h=300&fit=crop', 11, 2, true),
(28, 'Trà trái cây Nhiệt đới', 'Chỉ có 1 size', 'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop', 11, 2, true),
(29, 'Trà Long nhãn thái lan', 'Trà long nhãn + trân châu trắng thanh mát', 'https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=400&h=300&fit=crop', 11, 2, true),
(30, 'Sữa Tươi Thốt Nốt Rim', 'Ko điều chỉnh được lượng đường', 'https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=400&h=300&fit=crop', 11, 2, true),
(31, 'Trà Sen Highland', '', 'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop', 11, 2, true),
(32, 'Trà Dâu Tằm', '', 'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop', 11, 2, true),
(33, 'Đặc biệt Trà Trái cây ly 1lit', 'Trà đào, dâu tằm, ổi, xoài, dâu, kiwi', 'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop', 11, 2, true),
(34, 'Trà sữa Pho mai viên + củ năng', '3 viên pho mai, củ năng, trân châu', 'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop', 11, 2, true),
(35, 'Trà sữa Thái Xanh', 'Trà sữa + trân châu + thạch nhà làm + Flan', 'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop', 11, 2, true),
(36, 'Bún mắm thịt heo quay', 'Bún mắm + heo quay', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop', 12, 12, true),
(37, 'Bún mắm thập cẩm', 'Bún mắm thập cẩm', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop', 12, 12, true),
(38, 'Nem', '1 cây', 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop', 12, 12, true),
(39, 'Bún heo quay đặc biệt', '', 'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop', 12, 12, true),
(40, 'Bún mắm nem - chả', '', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop', 12, 12, true),
(41, 'Heo quay cúng thần tài (100gr)', 'Để nguyên hoặc chặt miếng nhỏ', 'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop', 12, 1, true),
(42, 'Nước mía', '', 'https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop', 12, 2, true),
(43, 'Thịt heo quay thêm (100gram)', '', 'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop', 12, 1, true),
(44, 'Bún mắm thịt heo quay - nem', '', 'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop', 12, 12, true),
(45, 'Bún heo quay đặc biệt', '', 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop', 12, 12, true),
(46, 'Gà Xé Kèm Xôi Xéo', 'Gà hấp + xôi', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 13, 9, true),
(47, 'Gà Quay Đặc Biệt', 'Gà quay nguyên con', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 13, 9, true),
(48, 'Gà hấp hành', 'Chặt theo yêu cầu', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 13, 9, true),
(49, 'Gà rang muối', 'Chặt miếng', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 13, 9, true),
(50, 'Combo 0,5 Gà quay + Xôi', 'Nửa con gà + xôi', 'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop', 13, 9, true),
(51, 'Sashimi Mix Sốt Cay Kiểu Thái Lan', 'Cá hồi, cá ngừ, cá trắng sốt cay kiểu Thái', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(52, 'Set cá hồi vs lươn tươi ngon', 'Sushi cá hồi + sushi lươn', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(53, 'Set cá hồi tươi ngon', 'Sashimi cá hồi + cơm cuộn cá hồi bơ + sushi cá hồi chín sốt cay', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(54, 'Set Ngon - Bổ - Rẻ 7', 'Những miếng sashimi tươi ngon, béo ngậy', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(55, 'Gừng + Rong Nho', 'Gừng đỏ và rong nho', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(56, 'Set Take Away A', 'Maki trứng tôm 8 viên, sushi thanh cua, sushi cá hồi chín sốt cay', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop', 14, 11, true),
(57, 'Bento cake', '', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(58, 'Bánh kem decor dễ thương', 'Ngẫu nhiên', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(59, 'Bông lan trứng muối size 16', 'Ngẫu nhiên', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(60, 'Tiramisu mix bông lan trứng muối - set 9 hộp', '9 hộp', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(61, 'Bánh kem trẻ em', 'Ngẫu nhiên', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(62, 'Bánh kem trái cây s16', 'Size 16, ngẫu nhiên', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true),
(63, 'Set hoa và bánh', 'Hộp gồm bánh và hoa trang trí', 'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop', 15, 4, true);

-- ==================== tbBienTheMonAn ====================
INSERT INTO "tbBienTheMonAn" ("mamon", "size", "giatien") VALUES
-- Quán 6: Koneko Pizza
(1,  NULL,   10000),
(2,  'M',    80000),
(2,  'L',    120000),
(3,  'M',    70000),
(3,  'L',    110000),
(4,  'M',    70000),
(4,  'L',    110000),
(5,  'M',    95000),
(5,  'L',    135000),
-- Quán 7: Cơm 1990
(6,  NULL,   40000),
(7,  NULL,   40000),
(8,  NULL,   50000),
(9,  NULL,   40000),
(10, NULL,   10000),
-- Quán 8: Bún Đậu Mắm Tôm Gia Di
(11, NULL,   40000),
(12, NULL,   50000),
(13, NULL,   75000),
(14, NULL,   70000),
(15, NULL,   75000),
-- Quán 9: Quán Chay An Lạc Tâm
(16, NULL,   35000),
(17, NULL,   40000),
(18, NULL,   38000),
(19, NULL,   58000),
(20, NULL,   29000),
-- Quán 10: Chân Gà Nướng Bà Hồng
(21, NULL,   39000),
(22, NULL,   38000),
(23, NULL,   60000),
(24, NULL,   64000),
(25, NULL,   56000),
-- Quán 11: Trà Long
(26, 'L',    33000),
(27, 'L',    28000),
(28, 'L',    28000),
(29, 'L',    28000),
(30, 'M',    35000),
(26, 'M',    28000),
(27, 'M',    23000),
(28, 'M',    23000),
(29, 'M',    23000),
(31, NULL,   29000),
(32, NULL,   20000),
(33, NULL,   30000),
(34, NULL,   29000),
(35, NULL,   28000),
(31, 'L',    39000),
(32, 'L',    30000),
(33, 'M',    25000),
(34, 'L',    39000),
(35, 'L',    38000),
-- Quán 12: Bún Mắm Bà Đông
(36, NULL,   35000),
(37, NULL,   45000),
(38, NULL,   5000),
(39, NULL,   45000),
(40, NULL,   35000),
(41, NULL,   40000),
(42, NULL,   10000),
(43, NULL,   40000),
(44, NULL,   40000),
(45, NULL,   45000),
-- Quán 13: Đàng Hoàng
(46, NULL,   275000),
(47, NULL,   235000),
(48, NULL,   225000),
(49, NULL,   265000),
(50, NULL,   190000),
-- Quán 14: Sushi Totoro
(51, NULL,   104000),
(52, NULL,   199000),
(53, NULL,   198000),
(54, NULL,   189000),
(55, NULL,   57000),
(56, NULL,   95000),
-- Quán 15: 43 Bakery
(57, NULL,   110000),
(58, NULL,   250000),
(59, NULL,   220000),
(60, NULL,   300000),
(61, NULL,   270000),
(62, NULL,   320000),
(63, NULL,   350000);

-- ==================== tbKhuyenMai ====================
INSERT INTO "tbKhuyenMai" ("makm", "tenkm", "mota", "loaikm", "phantramgiam", "dieukien", "ngaybatdau", "ngayketthuc") VALUES
(1, 'Khuyến mãi mùa hè', 'Giảm giá 20% cho tất cả sản phẩm mùa hè', 'Giảm giá', 20, 'Sản phẩm mùa hè', '2026-06-01 00:00:00', '2026-08-31 00:00:00'),
(2, 'Khuyến mãi sinh nhật', 'Giảm 30% cho khách hàng sinh nhật trong tháng', 'Giảm giá', 30, 'Khách hàng sinh nhật', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(3, 'Khuyến mãi mua hàng lớn', 'Giảm giá 10% cho hóa đơn từ 1 triệu trở lên', 'Giảm giá', 10, 'Hóa đơn từ 1 triệu', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
-- ═══ TIME-SLOT VOUCHERS (tự động áp dụng theo khung giờ) ═══
(4, 'SÁNG KHOẺ - Giảm 15%', 'Giảm 15% cho đơn hàng từ 50K, áp dụng 6:00-10:00', 'Giảm giá', 15, 'Đơn từ 50.000đ, khung giờ 6:00-10:00', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(5, 'TRƯA NGON - Giảm 25%', 'Giảm 25% cho đơn từ 50K, áp dụng 10:00-14:00', 'Giảm giá', 25, 'Đơn từ 50.000đ, khung giờ 10:00-14:00', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(6, 'XẾ MÊ - Giảm 10%', 'Giảm 10% cho đơn từ 30K, áp dụng 14:00-17:00', 'Giảm giá', 10, 'Đơn từ 30.000đ, khung giờ 14:00-17:00', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(7, 'TỐI VUI - Giảm 25%', 'Giảm 25% cho đơn từ 100K, áp dụng 17:00-22:00', 'Giảm giá', 25, 'Đơn từ 100.000đ, khung giờ 17:00-22:00', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(8, 'KHUYA - Giảm 30%', 'Giảm 30% cho đơn từ 50K, áp dụng 22:00-06:00', 'Giảm giá', 30, 'Đơn từ 50.000đ, khung giờ 22:00-06:00', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(9, 'ĐẶT LẦN ĐẦU - Giảm 40%', 'Giảm 40% cho đơn đầu tiên, tối đa 50.000đ', 'Giảm giá', 40, 'Đơn hàng đầu tiên, tối đa giảm 50.000đ', '2026-01-01 00:00:00', '2026-12-31 00:00:00'),
(10, 'MIỄN PHÍ SHIP', 'Miễn phí giao hàng cho đơn từ 50K', 'Miễn phí ship', 0, 'Đơn từ 50.000đ, miễn phí ship 15.000đ', '2026-01-01 00:00:00', '2026-12-31 00:00:00');

-- ==================== tbMonAnKhuyenMai ====================
INSERT INTO "tbMonAnKhuyenMai" ("makm", "mamon", "soluong", "trangthai", "phantramgiam") VALUES
(1, 1, 48, 'Còn hạn', 20),
(2, 2, 30, 'Còn hạn', 30),
(2, 4, 20, 'Còn hạn', 10);

-- ==================== tbLoaiHinhThanhToan ====================
INSERT INTO "tbLoaiHinhThanhToan" ("mahttt", "tenhinhthuc", "mota") VALUES
(1, 'Tiền mặt', 'Thanh toán khi nhận hàng'),
(2, 'Tài khoản ngân hàng', 'Liên kết tài khoản ngân hàng'),
(3, 'ZaloPay', 'Tài khoản ZaloPay liên kết'),
(4, 'Paypal', ''),
(5, 'Momo', '');

-- ==================== tbThongTinDatHang ====================
INSERT INTO "tbThongTinDatHang" ("sdt", "diachi", "toado", "tennguoinhan", "userid") VALUES
('0987654321', '02 Thanh Sơn, Thanh Bình, Hải Châu, TP. Hồ Chí Minh', NULL, 'Trần Thị B', 1),
('0901234567', '48 Cao Thắng, Thanh Bình, Hải Châu, TP. Hồ Chí Minh', NULL, 'Lê Văn C', 2);

-- ==================== Tránh duplicate ALL tables ====================
-- ⚠️ Nếu chạy seed nhiều lần, các INSERT sẽ tạo bản ghi trùng lặp!
-- Cách fix: Xoá toàn bộ dữ liệu cũ trước khi seed lại (thường dùng cho dev):
--
--   TRUNCATE "tbChiTietDonHang", "tbDanhGia", "tbTinNhan", "tbDonHang",
--            "tbThongTinDatHang", "tbMonAnKhuyenMai", "tbKhuyenMai",
--            "tbBienTheMonAn", "tbMonAn", "tbDanhMuc", "tbQuanAn",
--            "tbShipper", "tbKhachHang", "tbAdmin", "tbUser" CASCADE;
--
-- Hoặc dùng lệnh sau để xoá user gốc (chạy trước seed):
--   DELETE FROM "tbUser" WHERE "userid" BETWEEN 1 AND 18;
--
-- Cách fix nhẹ hơn: thêm ON CONFLICT DO NOTHING vào mỗi INSERT,
-- nhưng yêu cầu unique constraint trên các cột.
--
-- Phòng ngừa duplicate:
--   CREATE UNIQUE INDEX IF NOT EXISTS idx_tbuser_username ON "tbUser"("username");
--   CREATE UNIQUE INDEX IF NOT EXISTS idx_tbshipper_userid ON "tbShipper"("userid");
--   CREATE UNIQUE INDEX IF NOT EXISTS idx_tbquanan_userid ON "tbQuanAn"("userid");
--   CREATE UNIQUE INDEX IF NOT EXISTS idx_tbkhachhang_userid ON "tbKhachHang"("userid");
--   CREATE UNIQUE INDEX IF NOT EXISTS idx_tbadmin_userid ON "tbAdmin"("userid");

-- Nếu unique constraint đã có, các INSERT sau sẽ báo lỗi duplicate key.
-- Đó là hành vi ĐÚNG — không tạo duplicate.

-- Xoá đơn hàng rác (không có chi tiết) để tránh tích luỹ khi seed lại:
DELETE FROM "tbDonHang" WHERE "madh" NOT IN (SELECT DISTINCT "madh" FROM "tbChiTietDonHang");

-- ==================== tbDonHang ====================

INSERT INTO "tbDonHang" ("maquan", "mattdh", "ngaydathang", "trangthai", "tongtien", "hinhthucthanhtoan", "ghichu", "makhuyenmai", "phiship", "phidichvu", "ngaygiaohang", "ngaythanhtoan", "mashipper") VALUES
(6, 1, '2024-05-16 08:00:00', 'Hoàn thành', 100000.0000, 1, 'Ghi chú đơn hàng', NULL, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3),
(6, 1, '2024-05-16 08:00:00', 'Đã đặt', 90000.0000, 1, 'Ghi chú đơn hàng', 1, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3);

-- ==================== tbChiTietDonHang ====================
INSERT INTO "tbChiTietDonHang" ("madh", "mamon", "soluong", "dongia") VALUES
(1, 1, 1, 10000.0000),
(1, 2, 1, 80000.0000),
(2, 2, 1, 80000.0000);

-- ==================== tbDanhGia ====================
INSERT INTO "tbDanhGia" ("mactdh", "diemdanhgia", "nhanxet", "hinhanh") VALUES
(1, 5, 'Món ăn ngon, giao hàng nhanh', 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=200&h=200&fit=crop'),
(2, 4, 'Pizza ngon, phô mai nhiều', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=200&h=200&fit=crop');

-- ==================== tbTinNhan ====================
INSERT INTO "tbTinNhan" ("madh", "noidung", "mashipper", "makh") VALUES
(1, 'Đơn hàng của bạn đã được xác nhận', 3, 1),
(1, 'Đơn hàng của bạn đang được vận chuyển', 3, 1),
(1, 'Giao hàng đã thành công', 3, 1),
(2, 'Đơn hàng của bạn đã được xác nhận', 3, 1),
(2, 'Đơn hàng của bạn đang được vận chuyển', 3, 1),
(2, 'Giao hàng đã thành công', 3, 1);
