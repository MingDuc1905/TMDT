-- ═══ FastShip DB Indexes (7.4) ═══
-- Ch?y 1 l?n trên production:
--   psql "$DATABASE_URL" -f scripts/db-indexes.sql

-- tbDonHang: index cho queries ph? bi?n
CREATE INDEX IF NOT EXISTS idx_donhang_trangthai ON "tbDonHang" ("trangthai");
CREATE INDEX IF NOT EXISTS idx_donhang_ngaydathang ON "tbDonHang" ("ngaydathang");
CREATE INDEX IF NOT EXISTS idx_donhang_mattdh ON "tbDonHang" ("mattdh");
CREATE INDEX IF NOT EXISTS idx_donhang_maquan ON "tbDonHang" ("maquan");
CREATE INDEX IF NOT EXISTS idx_donhang_mashipper ON "tbDonHang" ("mashipper");
-- Composite index cho dashboard (th?ng kê doanh thu theo th?i gian + tr?ng thái)
CREATE INDEX IF NOT EXISTS idx_donhang_trangthai_ngay ON "tbDonHang" ("trangthai", "ngaydathang");

-- tbChiTietDonHang
CREATE INDEX IF NOT EXISTS idx_chitietdonhang_madh ON "tbChiTietDonHang" ("madh");
CREATE INDEX IF NOT EXISTS idx_chitietdonhang_mamon ON "tbChiTietDonHang" ("mamon");

-- tbMonAn
CREATE INDEX IF NOT EXISTS idx_monan_maquanan ON "tbMonAn" ("maquanan");
CREATE INDEX IF NOT EXISTS idx_monan_madanhmuc ON "tbMonAn" ("madanhmuc");

-- tbUser
CREATE INDEX IF NOT EXISTS idx_user_loaitaikhoan ON "tbUser" ("loaitaikhoan");
CREATE INDEX IF NOT EXISTS idx_user_trangthai ON "tbUser" ("trangthai");
CREATE INDEX IF NOT EXISTS idx_user_sdt ON "tbUser" ("sdt");
CREATE INDEX IF NOT EXISTS idx_user_email ON "tbUser" ("email");

-- tbThongTinDatHang
CREATE INDEX IF NOT EXISTS idx_ttdh_userid ON "tbThongTinDatHang" ("userid");

-- tbTinNhan
CREATE INDEX IF NOT EXISTS idx_tinnhan_makh ON "tbTinNhan" ("makh");
CREATE INDEX IF NOT EXISTS idx_tinnhan_madh ON "tbTinNhan" ("madh");

-- tbBienTheMonAn
CREATE INDEX IF NOT EXISTS idx_bienthe_monan_mamon ON "tbBienTheMonAn" ("mamon");

-- tbDanhGia
CREATE INDEX IF NOT EXISTS idx_danhgia_maquan ON "tbDanhGia" ("maquan");
CREATE INDEX IF NOT EXISTS idx_danhgia_mamon ON "tbDanhGia" ("mamon");
