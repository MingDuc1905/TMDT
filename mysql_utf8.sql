-- ===============================================================
-- FASTSHIP - MySQL Database Setup (Railway)
-- Creates all tables matching C# models + inserts seed data
-- ===============================================================

-- ===============================================================
-- Ép buộc MySQL nhập file theo chuẩn UTF-8
-- Giải quyết tận gốc lỗi font tiếng Việt bị thành ký tự rác
-- ===============================================================
SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET CHARACTER SET utf8mb4;
SET character_set_client = utf8mb4;
SET character_set_connection = utf8mb4;
SET character_set_results = utf8mb4;
SET SESSION collation_connection = utf8mb4_unicode_ci;

-- Drop foreign key checks temporarily to allow DROP TABLE
SET FOREIGN_KEY_CHECKS = 0;

-- Drop tables in reverse dependency order (if re-running)
DROP TABLE IF EXISTS tbDanhGia;
DROP TABLE IF EXISTS tbChiTietDonHang;
DROP TABLE IF EXISTS tbMonAnKhuyenMai;
DROP TABLE IF EXISTS tbTinNhan;
DROP TABLE IF EXISTS tbDonHang;
DROP TABLE IF EXISTS tbKhuyenMai;
DROP TABLE IF EXISTS tbBienTheMonAn;
DROP TABLE IF EXISTS tbMonAn;
DROP TABLE IF EXISTS tbDanhMuc;
DROP TABLE IF EXISTS tbThongTinDatHang;
DROP TABLE IF EXISTS tbLoaiHinhThanhToan;
DROP TABLE IF EXISTS tbQuanAn;
DROP TABLE IF EXISTS tbShipper;
DROP TABLE IF EXISTS tbKhachHang;
DROP TABLE IF EXISTS tbAdmin;
DROP TABLE IF EXISTS tbUser;

-- ===================== tbUser =====================
CREATE TABLE tbUser (
    userid INT AUTO_INCREMENT NOT NULL,
    username VARCHAR(50) NOT NULL,
    pwd VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    loaitaikhoan VARCHAR(50) NOT NULL,
    sdt VARCHAR(11) NOT NULL,
    vitien DECIMAL(19,4) NULL,
    email VARCHAR(50) NOT NULL,
    trangthai INT NOT NULL,
    PRIMARY KEY (userid),
    UNIQUE KEY uq_email (email),
    UNIQUE KEY uq_sdt (sdt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbAdmin =====================
CREATE TABLE tbAdmin (
    userid INT NOT NULL,
    tenadmin VARCHAR(50) NOT NULL,
    PRIMARY KEY (userid),
    CONSTRAINT fk_admin_user FOREIGN KEY (userid) REFERENCES tbUser(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbKhachHang =====================
CREATE TABLE tbKhachHang (
    userid INT NOT NULL,
    tenkh VARCHAR(50) NOT NULL,
    PRIMARY KEY (userid),
    CONSTRAINT fk_khachhang_user FOREIGN KEY (userid) REFERENCES tbUser(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbShipper =====================
CREATE TABLE tbShipper (
    userid INT NOT NULL,
    tenshipper VARCHAR(50) NOT NULL,
    diachi VARCHAR(250) NOT NULL,
    toado VARCHAR(100) NULL,
    diemdanhgia DECIMAL(2,1) NULL,
    soluotdanhgia INT NULL,
    trangthai VARCHAR(50) NULL,
    hinhanh VARCHAR(100) NULL,
    PRIMARY KEY (userid),
    CONSTRAINT fk_shipper_user FOREIGN KEY (userid) REFERENCES tbUser(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbQuanAn =====================
CREATE TABLE tbQuanAn (
    userid INT NOT NULL,
    tenquanan VARCHAR(100) NOT NULL,
    diachi VARCHAR(250) NOT NULL,
    toado VARCHAR(100) NULL,
    soluotdanhgia INT NULL,
    diemdanhgia DECIMAL(2,1) NULL,
    trangthai VARCHAR(50) NULL,
    hinhanh VARCHAR(100) NULL,
    PRIMARY KEY (userid),
    CONSTRAINT fk_quanan_user FOREIGN KEY (userid) REFERENCES tbUser(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbDanhMuc =====================
CREATE TABLE tbDanhMuc (
    madanhmuc INT AUTO_INCREMENT NOT NULL,
    tendanhmuc VARCHAR(100) NOT NULL,
    mota VARCHAR(250) NULL,
    hinhanh VARCHAR(100) NULL,
    PRIMARY KEY (madanhmuc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbMonAn =====================
-- Lưu tên gốc của món ăn, KHÔNG chứa size hay giá (chuyển sang tbBienTheMonAn)
CREATE TABLE tbMonAn (
    mamon INT AUTO_INCREMENT NOT NULL,
    tenmon VARCHAR(100) NOT NULL,
    mota VARCHAR(500) NULL,
    hinhanh VARCHAR(50) NULL,
    maquanan INT NULL,
    madanhmuc INT NULL,
    conhang BIT DEFAULT 1,
    PRIMARY KEY (mamon),
    CONSTRAINT fk_monan_quanan FOREIGN KEY (maquanan) REFERENCES tbQuanAn(userid) ON DELETE CASCADE,
    CONSTRAINT fk_monan_danhmuc FOREIGN KEY (madanhmuc) REFERENCES tbDanhMuc(madanhmuc) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbBienTheMonAn (MỚI) =====================
-- Lưu các biến thể size và giá tương ứng cho mỗi món ăn
CREATE TABLE tbBienTheMonAn (
    id INT AUTO_INCREMENT NOT NULL,
    mamon INT NOT NULL,
    size VARCHAR(10) NULL,
    giatien DECIMAL(19,4) NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_bienthe_monan FOREIGN KEY (mamon) REFERENCES tbMonAn(mamon) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbKhuyenMai =====================
CREATE TABLE tbKhuyenMai (
    makm INT AUTO_INCREMENT NOT NULL,
    tenkm VARCHAR(100) NOT NULL,
    mota VARCHAR(500) NULL,
    loaikm VARCHAR(200) NOT NULL,
    phantramgiam INT NULL,
    dieukien VARCHAR(500) NULL,
    ngaybatdau DATETIME NULL,
    ngayketthuc DATETIME NULL,
    PRIMARY KEY (makm)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbMonAnKhuyenMai =====================
-- mamon giờ là FK tới tbBienTheMonAn.id
CREATE TABLE tbMonAnKhuyenMai (
    id INT AUTO_INCREMENT NOT NULL,
    makm INT NULL,
    mamon INT NULL,
    soluong INT NULL,
    trangthai VARCHAR(50) NULL,
    phantramgiam INT NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_makm_khuyenmai FOREIGN KEY (makm) REFERENCES tbKhuyenMai(makm) ON DELETE CASCADE,
    CONSTRAINT fk_makm_bienthe FOREIGN KEY (mamon) REFERENCES tbBienTheMonAn(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbLoaiHinhThanhToan =====================
CREATE TABLE tbLoaiHinhThanhToan (
    mahttt INT AUTO_INCREMENT NOT NULL,
    tenhinhthuc VARCHAR(100) NOT NULL,
    mota VARCHAR(500) NULL,
    PRIMARY KEY (mahttt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbThongTinDatHang =====================
CREATE TABLE tbThongTinDatHang (
    mattdh INT AUTO_INCREMENT NOT NULL,
    sdt VARCHAR(11) NOT NULL,
    diachi VARCHAR(250) NOT NULL,
    toado VARCHAR(100) NULL,
    tennguoinhan VARCHAR(50) NOT NULL,
    userid INT NULL,
    PRIMARY KEY (mattdh),
    CONSTRAINT fk_ttdh_khachhang FOREIGN KEY (userid) REFERENCES tbKhachHang(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbDonHang =====================
CREATE TABLE tbDonHang (
    madh INT AUTO_INCREMENT NOT NULL,
    maquan INT NULL,
    mattdh INT NULL,
    ngaydathang DATETIME NULL,
    trangthai VARCHAR(50) NULL,
    tongtien DECIMAL(19,4) NULL,
    hinhthucthanhtoan INT NULL,
    ghichu VARCHAR(200) NULL,
    makhuyenmai INT NULL,
    phiship DECIMAL(19,4) NULL,
    phidichvu DECIMAL(19,4) NULL,
    ngaygiaohang DATETIME NULL,
    ngaythanhtoan DATETIME NULL,
    mashipper INT NULL,
    momo_trans_id VARCHAR(100) NULL,
    PRIMARY KEY (madh),
    CONSTRAINT fk_donhang_quanan FOREIGN KEY (maquan) REFERENCES tbQuanAn(userid) ON DELETE SET NULL,
    CONSTRAINT fk_donhang_ttdh FOREIGN KEY (mattdh) REFERENCES tbThongTinDatHang(mattdh) ON DELETE SET NULL,
    CONSTRAINT fk_donhang_httt FOREIGN KEY (hinhthucthanhtoan) REFERENCES tbLoaiHinhThanhToan(mahttt),
    CONSTRAINT fk_donhang_khuyenmai FOREIGN KEY (makhuyenmai) REFERENCES tbKhuyenMai(makm),
    CONSTRAINT fk_donhang_shipper FOREIGN KEY (mashipper) REFERENCES tbShipper(userid) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbChiTietDonHang =====================
-- mamon giờ là FK tới tbBienTheMonAn.id
CREATE TABLE tbChiTietDonHang (
    mactdh INT AUTO_INCREMENT NOT NULL,
    madh INT NULL,
    mamon INT NULL,
    soluong INT NULL,
    dongia DECIMAL(19,4) NULL,
    PRIMARY KEY (mactdh),
    CONSTRAINT fk_ctdh_donhang FOREIGN KEY (madh) REFERENCES tbDonHang(madh) ON DELETE CASCADE,
    CONSTRAINT fk_ctdh_bienthe FOREIGN KEY (mamon) REFERENCES tbBienTheMonAn(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbDanhGia =====================
CREATE TABLE tbDanhGia (
    madg INT AUTO_INCREMENT NOT NULL,
    mactdh INT NULL,
    diemdanhgia INT NULL,
    nhanxet VARCHAR(500) NULL,
    hinhanh VARCHAR(100) NULL,
    PRIMARY KEY (madg),
    CONSTRAINT fk_danhgia_ctdh FOREIGN KEY (mactdh) REFERENCES tbChiTietDonHang(mactdh)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================== tbTinNhan =====================
CREATE TABLE tbTinNhan (
    matn INT AUTO_INCREMENT NOT NULL,
    madh INT NULL,
    noidung VARCHAR(500) NULL,
    mashipper INT NULL,
    makh INT NULL,
    PRIMARY KEY (matn),
    CONSTRAINT fk_tinnhan_donhang FOREIGN KEY (madh) REFERENCES tbDonHang(madh),
    CONSTRAINT fk_tinnhan_khachhang FOREIGN KEY (makh) REFERENCES tbKhachHang(userid),
    CONSTRAINT fk_tinnhan_shipper FOREIGN KEY (mashipper) REFERENCES tbShipper(userid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================================
-- SEED DATA
-- =====================================================================

-- ==================== tbUser ====================
INSERT INTO tbUser (userid, username, pwd, loaitaikhoan, sdt, vitien, email, trangthai) VALUES
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
INSERT INTO tbAdmin (userid, tenadmin) VALUES
(16, 'Admin 1'),
(17, 'Admin 2'),
(18, 'Admin 3');

-- ==================== tbKhachHang ====================
INSERT INTO tbKhachHang (userid, tenkh) VALUES
(1, 'Tran Thi B'),
(2, 'Le Van C'),
(5, 'Pham Thi D');

-- ==================== tbShipper ====================
INSERT INTO tbShipper (userid, tenshipper, diachi, toado, diemdanhgia, soluotdanhgia, trangthai, hinhanh) VALUES
(3, 'Le Van Y', '48 Cao Thắng, P.Thanh Bình, Quận 3, TP. Hồ Chí Minh', NULL, 4, 80, 'Không hoạt động', 'shipper_y.jpg'),
(4, 'Nguyen Thi Z', '80 Huỳnh Ngọc Huệ,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 5, 120, 'Đang hoạt động', 'shipper_z.jpg');

-- ==================== tbQuanAn ====================
INSERT INTO tbQuanAn (userid, tenquanan, diachi, toado, soluotdanhgia, diemdanhgia, trangthai, hinhanh) VALUES
(6, 'Koneko Pizza', 'K57H10/12 Bà Bang Nhãn, P. Hòa Hải,  Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 75, 5, 'Đóng cửa', 'koneko.jpg'),
(7, 'Cơm 1990 - Ngô Văn Sở', '61 Ngô Văn Sở, P.Hòa Khánh Nam,  Quận 12, TP. Hồ Chí Minh', NULL, 300, 5, 'Đang mở cửa', 'com1990.jpg'),
(8, 'Bún Đậu Mắm Tôm Gia Di - Nguyễn Văn Thoại', '100 Nguyễn Văn Thoại, P. Mỹ An,  Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'bundaugiadi.jpg'),
(9, 'Quán Chay An Lạc Tâm - Phan Đăng Lưu', '117 Phan Đăng Lưu, P. Hòa Cường Bắc,  Quận 3, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'quanchayanlactam.jpg'),
(10, 'Chân Gà Nướng Bà Hồng - Trần Cao Vân', '151 Trần Cao Vân, P. Tam Thuận,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 400, 5, 'Đang mở cửa', 'changanuongbahong.jpg'),
(11, 'Trà Long - Trà Trái Cây', '149/11 Lê Đình Lý,  Quận 3, TP. Hồ Chí Minh', NULL, 300, 5, 'Đang mở cửa', 'tralong.jpg'),
(12, 'Bún Mắm Bà Đông', '145 Huỳnh Thúc Kháng, P. Bình Hiên,  Quận 3, TP. Hồ Chí Minh', NULL, 70, 5, 'Đang mở cửa', 'bunmambadong.jpg'),
(13, 'Đàng Hoàng - Gà Tre Đèo Le', '90 Huỳnh Ngọc Huệ,  Quận Tân Bình, TP. Hồ Chí Minh', NULL, 100, 5, 'Đang mở cửa', 'danghoang_gatre.jpg'),
(14, 'Sushi Totoro - Sushi Của Người Việt', '51 Châu Thị Vĩnh Tế, P. Bắc Mỹ Phú, Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 200, 4, 'Đang mở cửa', 'sushitotoro.jpg'),
(15, '43 Bakery - Bánh Mì & Bánh Kem - Ngũ Hành Sơn', '149 Ngũ Hành Sơn, P. Mỹ An, Quận Bình Thạnh, TP. Hồ Chí Minh', NULL, 200, 5, 'Đang mở cửa', '43_bakery.jpg');

-- ==================== tbDanhMuc ====================
INSERT INTO tbDanhMuc (madanhmuc, tendanhmuc, mota, hinhanh) VALUES
(1, 'Đồ ăn', 'Các món ăn khác nhau từ nhiều nền văn hóa và phong cách ẩm thực khác nhau.', 'do_an.jpg'),
(2, 'Đồ uống', 'Các loại đồ uống như nước ngọt, nước trái cây, nước ép, nước lọc, trà, cà phê, cocktail, và nhiều loại đồ uống khác.', 'do_uong.jpg'),
(3, 'Đồ chay', 'Các món ăn không chứa thịt hoặc bất kỳ sản phẩm động vật nào.', 'do_chay.jpg'),
(4, 'Bánh kem', 'Bánh kem là một loại bánh ngọt được làm từ các thành phần như bột mì, đường, trứng và sữa, thường được trang trí với kem và nhiều loại topping khác nhau.', 'banh_kem.jpg'),
(5, 'Tráng miệng', 'Các món như bánh ngọt, kem, hoa quả hoặc các loại đặc sản ngọt khác.', 'trang_mieng.jpg'),
(6, 'Homemade', 'Đồ ăn tự làm được chế biến tại nhà hoặc do các nhà hàng địa phương chế biến một cách thủ công và truyền thống.', 'homemade.jpg'),
(7, 'Vỉa hè', 'Các loại thức ăn đường phố hoặc các món ăn nhanh phổ biến', 'via_he.jpg'),
(8, 'Pizza/Burger', 'Các loại thức ăn nhanh phổ biến, thường được làm từ bánh mì, thịt, rau cải và các loại sốt.', 'pizza.jpg'),
(9, 'Món gà', 'Các món ăn chế biến từ thịt gà, bao gồm cả các món ăn nhanh và các món truyền thống.', 'mon_ga.jpg'),
(10, 'Món lẩu', 'Một loại món ăn được chế biến trong một nồi lớn có nước dùng nấu sôi, thường bao gồm thịt, hải sản, rau cải và các loại gia vị.', 'mon_lau.jpg'),
(11, 'Sushi', 'Món sushi thường được chế biến từ cơm trộn giấm kết hợp với các nguyên liệu khác nhau như hải sản, cá, hải sản tươi sống.', 'sushi.jpg'),
(12, 'Mì phở', 'Mì phở bao gồm một tô nước dùng sôi và bún mì phở.', 'mi_pho.jpg'),
(13, 'Cơm hộp', 'Cơm hộp thường một phần cơm trắng, kèm theo các loại thức ăn khác nhau được sắp xếp ngăn nắp trong một hộp đựng cơm.', 'com_hop.jpg');

-- ==================== tbMonAn (đã bỏ giatien + Size khỏi tên) ====================
INSERT INTO tbMonAn (mamon, tenmon, mota, hinhanh, maquanan, madanhmuc, conhang) VALUES
(1, 'Trà tắc', 'Trà và tắc.', 'tratac.jpg', 6, 2, 1),
(2, 'Pizza thập cẩm', 'Thịt, xúc xích, ớt chuông, bắp và phô mai', 'banh_mi_op_la.jpg', 6, 8, 1),
(3, 'Pizza Bò', 'Bò, ớt chuông, bắp và phô mai', 'pizza.jpg', 6, 8, 1),
(4, 'Pizza xúc xích', 'Xúc xích, bắp, phô mai và ớt chuông', 'pizza.jpg', 6, 8, 1),
(5, 'Pizza hải sản', 'Tôm, mực, ớt chuông, bắp, phô mai', 'pizza.jpg', 6, 8, 1),
(6, 'Cơm trắng + đậu nhồi thịt sốt cà chua', 'Món phụ ăn kèm', 'comdaunhoithit.jpg', 7, 13, 1),
(7, 'Cơm trắng + sườn xào chua ngọt', 'Món phụ ăn kèm', 'comsuonxao.jpg', 7, 13, 1),
(8, 'Combo cơm gà rang xả ớt + nước', 'Coca hoặc trà tắc hoặc nước khoáng lạt', 'combo_comga.jpg', 7, 1, 1),
(9, 'Cơm trắng + đậu nhồi thịt + rau xào theo ngày', 'Rau theo mùa', 'comdaurau.jpg', 7, 13, 1),
(10, 'Coca', 'Nước ngọt', 'coca.jpg', 7, 2, 1),
(11, 'Mẹt A', 'Bao gồm: thịt luộc + đậu khuôn + chả quế + chả cốm', 'metabc.jpg', 8, 12, 1),
(12, 'Mẹt B', 'Bao gồm: Bún, đậu khuôn, thịt, chả cốm, chả quế, nem rán, phèo luộc', 'metabc.jpg', 8, 12, 1),
(13, 'Mẹt C', 'Bao gồm: bún, đậu khuôn, thịt, chả cốm, dồi, phèo luộc, lưỡi, nem rán', 'metabc.jpg', 8, 12, 1),
(14, 'Mẹt nem nướng cuốn', 'Nem nướng Nha Trang', 'metnemnuongcuon.jpg', 8, 1, 1),
(15, 'Mẹt cuốn tá lá', 'Ram tôm đất + Nem nướng Nha Trang', 'metcuontala.jpg', 8, 1, 1),
(16, 'Chà Bông Chay', 'Trộn với cơm trắng hoặc thêm rong biển sấy và đậu phộng muối', 'cha_bong_chay.jpg', 9, 3, 1),
(17, 'Nấm rim mè', 'Có thể trộn gỏi, ăn cùng cơm trắng hoặc ăn vặt đều ngon', 'nam_rim.jpg', 9, 13, 1),
(18, 'Cơm Ngọc Bích', 'Cơm trộn nước cốt dừa và cải bó xôi xay nhuyễn', 'com_ngoc_bich.jpg', 9, 3, 1),
(19, 'Nấm Sốt Bơ Tỏi', 'Nấm đùi gà sốt bơ tỏi, ăn kèm bánh mì', 'nam_sot_bo_toi.jpg', 9, 3, 1),
(20, 'Mì quảng', 'Hương vị đậm đà từ rau củ, nghệ, đậu phộng và bánh tráng', 'mi_quang_chay.jpg', 9, 3, 1),
(21, 'Chân gà nướng', '3 cặp', 'chan_ga.jpg', 10, 7, 1),
(22, 'Cánh gà nướng', '2 cánh', 'canh_ga.jpg', 10, 7, 1),
(23, 'Thịt xiên nướng', '5 xiên', 'thit_xien_nuong.jpg', 10, 7, 1),
(24, 'Chim cút nướng', '2 con', 'chim_cut_nuong.jpg', 10, 7, 1),
(25, 'Ếch nướng', '2 con', 'ech_nuong.jpg', 10, 7, 1),
(26, 'Trà Mãng Cầu', 'Bánh flan, khúc bạch, pudding, thạch pho mai, củ năng, trân châu', 'ts_dac_biet.jpg', 11, 2, 1),
(27, 'Trà Mãng Cầu', 'Chỉ có 1 size', 'tra_mang_cau.jpg', 11, 2, 1),
(28, 'Trà trái cây Nhiệt đới', 'Chỉ có 1 size', 'tra_trai_cay.jpg', 11, 2, 1),
(29, 'Trà Long nhãn thái lan', 'Trà long nhãn + trân châu trắng thanh mát', 'st_thot_not.jpg', 11, 2, 1),
(30, 'Sữa Tươi Thốt Nốt Rim', 'Ko điều chỉnh được lượng đường', 'st_thot_not.jpg', 11, 2, 1),
(31, 'Trà Sen Highland', '', 'tra_sen.jpg', 11, 2, 1),
(32, 'Trà Dâu Tằm', '', 'tra_dau_tam.jpg', 11, 2, 1),
(33, 'Đặc biệt Trà Trái cây ly 1lit', 'Trà đào, dâu tằm, ổi, xoài, dâu, kiwi', 'tra_trai_cay_dbiet.jpg', 11, 2, 1),
(34, 'Trà sữa Pho mai viên + củ năng', '3 viên pho mai, củ năng, trân châu', 'ts_phomai_cunang.jpg', 11, 2, 1),
(35, 'Trà sữa Thái Xanh', 'Trà sữa + trân châu + thạch nhà làm + Flan', 'ts_thai_xanh.jpg', 11, 2, 1),
(36, 'Bún mắm thịt heo quay', 'Bún mắm + heo quay', 'bun_mam_heo_quay.jpg', 12, 12, 1),
(37, 'Bún mắm thập cẩm', 'Bún mắm thập cẩm', 'bun_mam_thap_cam.jpg', 12, 12, 1),
(38, 'Nem', '1 cây', 'nem.jpg', 12, 12, 1),
(39, 'Bún heo quay đặc biệt', '', '.jpg', 12, 12, 1),
(40, 'Bún mắm nem - chả', '', '.jpg', 12, 12, 1),
(41, 'Heo quay cúng thần tài (100gr)', 'Để nguyên hoặc chặt miếng nhỏ', 'heo_quay.jpg', 12, 1, 1),
(42, 'Nước mía', '', '.jpg', 12, 2, 1),
(43, 'Thịt heo quay thêm (100gram)', '', 'heo_quay.jpg', 12, 1, 1),
(44, 'Bún mắm thịt heo quay - nem', '', '.jpg', 12, 12, 1),
(45, 'Bún heo quay đặc biệt', '', '.jpg', 12, 12, 1),
(46, 'Gà Xé Kèm Xôi Xéo', 'Gà hấp + xôi', 'ga_xe_xoi.jpg', 13, 9, 1),
(47, 'Gà Quay Đặc Biệt', 'Gà quay nguyên con', 'ga_quay.jpg', 13, 9, 1),
(48, 'Gà hấp hành', 'Chặt theo yêu cầu', 'ga_hap_hanh.jpg', 13, 9, 1),
(49, 'Gà rang muối', 'Chặt miếng', 'ga_rang_muoi.jpg', 13, 9, 1),
(50, 'Combo 0,5 Gà quay + Xôi', 'Nửa con gà + xôi', 'combo_ga_xoi.jpg', 13, 9, 1),
(51, 'Sashimi Mix Sốt Cay Kiểu Thái Lan', 'Cá hồi, cá ngừ, cá trắng sốt cay kiểu Thái', 'sashimi_kieu_thai.jpg', 14, 11, 1),
(52, 'Set cá hồi vs lươn tươi ngon', 'Sushi cá hồi + sushi lươn', 'set_cahoi_luon.jpg', 14, 11, 1),
(53, 'Set cá hồi tươi ngon', 'Sashimi cá hồi + cơm cuộn cá hồi bơ + sushi cá hồi chín sốt cay', 'set_ca_hoi.jpg', 14, 11, 1),
(54, 'Set Ngon - Bổ - Rẻ 7', 'Những miếng sashimi tươi ngon, béo ngậy', 'set_7.jpg', 14, 11, 1),
(55, 'Gừng + Rong Nho', 'Gừng đỏ và rong nho', 'gung_rong_nho.jpg', 14, 11, 1),
(56, 'Set Take Away A', 'Maki trứng tôm 8 viên, sushi thanh cua, sushi cá hồi chín sốt cay', 'set_take_away.jpg', 14, 11, 1),
(57, 'Bento cake', '', 'bento.jpg', 15, 10, 1),
(58, 'Bánh kem decor dễ thương', 'Ngẫu nhiên', 'banh_kem_dthuong.jpg', 15, 10, 1),
(59, 'Bông lan trứng muối size 16', 'Ngẫu nhiên', 'bong_lan_trung_muoi.jpg', 15, 10, 1),
(60, 'Tiramisu mix bông lan trứng muối - set 9 hộp', '9 hộp', 'tiramisu_9hop.jpg', 15, 10, 1),
(61, 'Bánh kem trẻ em', 'Ngẫu nhiên', 'banh_kem_tre_em.jpg', 15, 10, 1),
(62, 'Bánh kem trái cây s16', 'Size 16, ngẫu nhiên', 'banh_kem_trai_cay.jpg', 15, 10, 1),
(63, 'Set hoa và bánh', 'Hộp gồm bánh và hoa trang trí', 'set_hoa_banh.jpg', 15, 10, 1);

-- ==================== tbBienTheMonAn (MỚI) ====================
-- Mỗi món có ít nhất 1 biến thể mặc định (size=NULL) với giá gốc
-- Món có nhiều size thì tạo nhiều dòng
INSERT INTO tbBienTheMonAn (mamon, size, giatien) VALUES
-- Quán 6: Koneko Pizza
(1,  NULL,   10000),   -- Trà tắc (1 size)
(2,  'M',    80000),   -- Pizza thập cẩm size M
(2,  'L',    120000),  -- Pizza thập cẩm size L (giả định)
(3,  'M',    70000),   -- Pizza Bò size M
(3,  'L',    110000),  -- Pizza Bò size L (giả định)
(4,  'M',    70000),   -- Pizza xúc xích size M
(4,  'L',    110000),  -- Pizza xúc xích size L
(5,  'M',    95000),   -- Pizza hải sản size M
(5,  'L',    135000),  -- Pizza hải sản size L
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
(31, NULL,   29000),
(32, NULL,   20000),
(33, NULL,   30000),
(34, NULL,   29000),
(35, NULL,   28000),
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
INSERT INTO tbKhuyenMai (makm, tenkm, mota, loaikm, phantramgiam, dieukien, ngaybatdau, ngayketthuc) VALUES
(1, 'Khuyến mãi mùa hè', 'Giảm giá 20% cho tất cả sản phẩm mùa hè', 'Giảm giá', 20, 'Sản phẩm mùa hè', '2024-06-01 00:00:00', '2024-08-31 00:00:00'),
(2, 'Khuyến mãi sinh nhật', 'Giảm 30% cho khách hàng sinh nhật trong tháng', 'Giảm giá', 30, 'Khách hàng sinh nhật', '2024-01-01 00:00:00', '2024-05-17 00:00:00'),
(3, 'Khuyến mãi mua hàng lớn', 'Giảm giá 10% cho hóa đơn từ 1 triệu trở lên', 'Giảm giá', 10, 'Hóa đơn từ 1 triệu', '2024-05-01 00:00:00', '2024-05-10 00:00:00');

-- ==================== tbMonAnKhuyenMai ====================
-- mamon giờ là id của tbBienTheMonAn
INSERT INTO tbMonAnKhuyenMai (id, makm, mamon, soluong, trangthai, phantramgiam) VALUES
(1, 1, 1, 48, 'Hết hạn', 20),    -- Trà tắc id=1
(2, 2, 2, 30, 'Hết hạn', 30),    -- Pizza thập cẩm M id=2
(3, 2, 4, 20, 'Hết hạn', 10);    -- Pizza Bò M id=4

-- ==================== tbLoaiHinhThanhToan ====================
INSERT INTO tbLoaiHinhThanhToan (mahttt, tenhinhthuc, mota) VALUES
(1, 'Tiền mặt', 'Thanh toán khi nhận hàng'),
(2, 'Tài khoản ngân hàng', 'Liên kết tài khoản ngân hàng'),
(3, 'ZaloPay', 'Tài khoản ZaloPay liên kết'),
(4, 'Paypal', ''),
(5, 'Momo', '');

-- ==================== tbThongTinDatHang ====================
INSERT INTO tbThongTinDatHang (mattdh, sdt, diachi, toado, tennguoinhan, userid) VALUES
(1, '0987654321', '02 Thanh Sơn, Thanh Bình, Hải Châu, TP. Hồ Chí Minh', NULL, 'Trần Thị B', 1),
(2, '0901234567', '48 Cao Thắng, Thanh Bình, Hải Châu, TP. Hồ Chí Minh', NULL, 'Lê Văn C', 2);

-- ==================== tbDonHang ====================
INSERT INTO tbDonHang (madh, maquan, mattdh, ngaydathang, trangthai, tongtien, hinhthucthanhtoan, ghichu, makhuyenmai, phiship, phidichvu, ngaygiaohang, ngaythanhtoan, mashipper) VALUES
(1, 6, 1, '2024-05-16 08:00:00', 'Hoàn thành', 100000.0000, 1, 'Ghi chú đơn hàng', NULL, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3),
(2, 6, 1, '2024-05-16 08:00:00', 'Đã đặt', 90000.0000, 1, 'Ghi chú đơn hàng', 1, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3);

-- ==================== tbChiTietDonHang ====================
-- mamon giờ là id của tbBienTheMonAn
INSERT INTO tbChiTietDonHang (mactdh, madh, mamon, soluong, dongia) VALUES
(1, 1, 1, 1, 50000.0000),   -- Trà tắc (bienthe id=1)
(2, 1, 2, 1, 80000.0000),   -- Pizza thập cẩm M (bienthe id=2)
(3, 2, 2, 1, 60000.0000);   -- Pizza thập cẩm M (bienthe id=2)

-- ==================== tbDanhGia ====================
INSERT INTO tbDanhGia (madg, mactdh, diemdanhgia, nhanxet, hinhanh) VALUES
(1, 1, 5, 'Món ăn ngon, giao hàng nhanh', 'danhgia1.jpg'),
(2, 2, 4, 'Pizza ngon, phô mai nhiều', 'danhgia2.jpg');

-- ==================== tbTinNhan ====================
INSERT INTO tbTinNhan (matn, madh, noidung, mashipper, makh) VALUES
(1, 1, 'Đơn hàng của bạn đã được xác nhận', 3, 1),
(2, 1, 'Đơn hàng của bạn đang được vận chuyển', 3, 1),
(3, 1, 'Giao hàng đã thành công', 3, 1),
(4, 2, 'Đơn hàng của bạn đã được xác nhận', 3, 1),
(5, 2, 'Đơn hàng của bạn đang được vận chuyển', 3, 1),
(6, 2, 'Giao hàng đã thành công', 3, 1);

-- ===============================================================
-- Khôi phục kiểm tra khóa ngoại
-- ===============================================================
SET FOREIGN_KEY_CHECKS = 1;
