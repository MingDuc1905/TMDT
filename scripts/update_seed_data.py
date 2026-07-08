#!/usr/bin/env python3
"""Update mysql_utf8.sql with Unsplash images, fix variants, fix order totals."""
import re

with open("mysql_utf8.sql", "r", encoding="utf-8") as f:
    sql = f.read()

changes = 0

# ============================================================
# 1. Replace local restaurant images with Unsplash URLs
# ============================================================
restaurant_images = {
    "'koneko.jpg'":            "'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&h=300&fit=crop'",
    "'com1990.jpg'":           "'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop'",
    "'bundaugiadi.jpg'":       "'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'",
    "'quanchayanlactam.jpg'":  "'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop'",
    "'changanuongbahong.jpg'": "'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop'",
    "'tralong.jpg'":           "'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop'",
    "'bunmambadong.jpg'":      "'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop'",
    "'danghoang_gatre.jpg'":   "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'sushitotoro.jpg'":       "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'43_bakery.jpg'":         "'https://images.unsplash.com/photo-1509365465985-25d11c17e812?w=400&h=300&fit=crop'",
}

for old, new in restaurant_images.items():
    if old in sql:
        sql = sql.replace(old, new)
        changes += 1

# ============================================================
# 2. Replace local dish images with Unsplash URLs
# ============================================================
dish_images = {
    # Quán 6: Koneko Pizza
    "'tratac.jpg'":             "'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop'",
    "'banh_mi_op_la.jpg'":      "'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&h=300&fit=crop'",
    # Rest of dishes use generic food images
    "'pizza.jpg'":              "'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop'",
    "'comdaunhoithit.jpg'":     "'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop'",
    "'comsuonxao.jpg'":         "'https://images.unsplash.com/photo-1559847844-5315695dadae?w=400&h=300&fit=crop'",
    "'combo_comga.jpg'":        "'https://images.unsplash.com/photo-1580477667995-05b94f944cd3?w=400&h=300&fit=crop'",
    "'comdaurau.jpg'":          "'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop'",
    "'coca.jpg'":               "'https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop'",
    # Bún Đậu
    "'metabc.jpg'":             "'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'",
    "'metnemnuongcuon.jpg'":    "'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop'",
    "'metcuontala.jpg'":        "'https://images.unsplash.com/photo-1603073163308-9654c3fb70b5?w=400&h=300&fit=crop'",
    # Chay
    "'cha_bong_chay.jpg'":      "'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop'",
    "'nam_rim.jpg'":            "'https://images.unsplash.com/photo-1559847844-5315695dadae?w=400&h=300&fit=crop'",
    "'com_ngoc_bich.jpg'":      "'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&h=300&fit=crop'",
    "'nam_sot_bo_toi.jpg'":     "'https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=400&h=300&fit=crop'",
    "'mi_quang_chay.jpg'":      "'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop'",
    # Chân gà
    "'chan_ga.jpg'":            "'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop'",
    "'canh_ga.jpg'":            "'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop'",
    "'thit_xien_nuong.jpg'":    "'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'",
    "'chim_cut_nuong.jpg'":     "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'ech_nuong.jpg'":          "'https://images.unsplash.com/photo-1559847844-5315695dadae?w=400&h=300&fit=crop'",
    # Trà Long
    "'ts_dac_biet.jpg'":        "'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop'",
    "'tra_mang_cau.jpg'":       "'https://images.unsplash.com/photo-1571934811356-5cc061b6821f?w=400&h=300&fit=crop'",
    "'tra_trai_cay.jpg'":       "'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop'",
    "'st_thot_not.jpg'":        "'https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=400&h=300&fit=crop'",
    "'tra_sen.jpg'":            "'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop'",
    "'tra_dau_tam.jpg'":        "'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&h=300&fit=crop'",
    "'tra_trai_cay_dbiet.jpg'": "'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop'",
    "'ts_phomai_cunang.jpg'":   "'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop'",
    "'ts_thai_xanh.jpg'":       "'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400&h=300&fit=crop'",
    # Bún Mắm
    "'bun_mam_heo_quay.jpg'":   "'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop'",
    "'bun_mam_thap_cam.jpg'":   "'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=300&fit=crop'",
    "'nem.jpg'":                "'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'",
    "'heo_quay.jpg'":           "'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400&h=300&fit=crop'",
    # Đàng Hoàng
    "'ga_xe_xoi.jpg'":          "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'ga_quay.jpg'":            "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'ga_hap_hanh.jpg'":        "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'ga_rang_muoi.jpg'":       "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'combo_ga_xoi.jpg'":       "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    # Sushi
    "'sashimi_kieu_thai.jpg'":  "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'set_cahoi_luon.jpg'":     "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'set_ca_hoi.jpg'":         "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'set_7.jpg'":              "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'gung_rong_nho.jpg'":      "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'set_take_away.jpg'":      "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    # 43 Bakery
    "'bento.jpg'":              "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'banh_kem_dthuong.jpg'":   "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'bong_lan_trung_muoi.jpg'":"'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'tiramisu_9hop.jpg'":      "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'banh_kem_tre_em.jpg'":    "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'banh_kem_trai_cay.jpg'":  "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'set_hoa_banh.jpg'":       "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    # Category & review images
    "'do_an.jpg'":              "'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&h=300&fit=crop'",
    "'do_uong.jpg'":            "'https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400&h=300&fit=crop'",
    "'do_chay.jpg'":            "'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=300&fit=crop'",
    "'banh_kem.jpg'":           "'https://images.unsplash.com/photo-1558636508-e0db3814bd1d?w=400&h=300&fit=crop'",
    "'trang_mieng.jpg'":        "'https://images.unsplash.com/photo-1551024601-bec78aea704b?w=400&h=300&fit=crop'",
    "'homemade.jpg'":           "'https://images.unsplash.com/photo-1495521821757-a1efb6729352?w=400&h=300&fit=crop'",
    "'via_he.jpg'":             "'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=400&h=300&fit=crop'",
    "'pizza.jpg'":              "'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400&h=300&fit=crop'",
    "'mon_ga.jpg'":             "'https://images.unsplash.com/photo-1598103442097-8b74394b95c6?w=400&h=300&fit=crop'",
    "'mon_lau.jpg'":            "'https://images.unsplash.com/photo-1546737013-1ac7f1f35b10?w=400&h=300&fit=crop'",
    "'sushi.jpg'":              "'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=400&h=300&fit=crop'",
    "'mi_pho.jpg'":             "'https://images.unsplash.com/photo-1552611052-33e04de1b100?w=400&h=300&fit=crop'",
    "'com_hop.jpg'":            "'https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400&h=300&fit=crop'",
    # Review images
    "'danhgia1.jpg'":           "'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=200&h=200&fit=crop'",
    "'danhgia2.jpg'":           "'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=200&h=200&fit=crop'",
    # Shipper images
    "'shipper_y.jpg'":          "'https://images.unsplash.com/photo-1599566150163-29194dcaad36?w=100&h=100&fit=crop'",
    "'shipper_z.jpg'":          "'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&h=100&fit=crop'",
    # Empty images (.jpg with no content)
    "'.jpg'":                   "''",
}

for old, new in dish_images.items():
    count = sql.count(old)
    if count > 0:
        sql = sql.replace(old, new)
        changes += count

# ============================================================
# 3. Fix Trà Long size variants - all drinks should have M/L
# ============================================================
# Current: many Trà Long items only have 1 variant
# We need to add M size for items that only have L size
# Items 26 (Trà Mãng Cầu - đặc biệt), 27 (Trà Mãng Cầu), 28 (Trà trái cây Nhiệt đới)
# 29 (Trà Long nhãn), 30 (Sữa Tươi Thốt Nốt) already have M

# Fix item 30 (Sữa Tươi Thốt Nốt Rim) - should have M as 35k 
# Already has M=35000, that's correct

# Add L size for items that only have 1 size:
# Item 31 (Trà Sen) - add L
new_variants = """
(31, 'L',    39000),
(32, 'L',    30000),
(33, 'M',    25000),   -- Trà trái cây 1lit hạng M
(34, 'L',    39000),
(35, 'L',    38000),
"""
# Insert after existing Trà Long variants (line with (35, NULL, 28000))
insert_point = "(35, NULL,   28000),"
if insert_point in sql:
    sql = sql.replace(insert_point, insert_point + new_variants)
    changes += 1

# ============================================================
# 4. Add M size for items that only have L size
# ============================================================
# Items 26, 27, 28, 29 have only L - add M
m_sizes = """
(26, 'M',    28000),
(27, 'M',    23000),
(28, 'M',    23000),
(29, 'M',    23000),
"""
insert_after = "(30, 'M',    35000),"
if insert_after in sql:
    sql = sql.replace(insert_after, insert_after + m_sizes)
    changes += 1

# ============================================================
# 5. Fix order totals to match actual variant prices
# ============================================================
# Order 1: Trà tắc (id=1) 10000 x1 + Pizza thập cẩm M (id=2) 80000 x1 = 90000
# Current: tongtien=100000 ❌ → should be 90000
sql = sql.replace(
    "100000.0000, 1, 'Ghi chú đơn hàng', NULL, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3),\r\n(2, 6, 1, '2024-05-16 08:00:00', 'Đã đặt', 90000.0000, 1, 'Ghi chú đơn hàng',",
    "90000.0000, 1, 'Ghi chú đơn hàng', NULL, 0.0000, 5000.0000, '2024-05-20 08:00:00', '2024-05-20 08:00:00', 3),\r\n(2, 6, 1, '2024-05-16 08:00:00', 'Đã đặt', 65000.0000, 1, 'Ghi chú đơn hàng',"
)

# Fix ChiTietDonHang dongia
# Order 1: Trà tắc (bienthe id=1) giatien=10000, Pizza thập cẩm M (id=2) giatien=80000
# Current: dongia=50000 (wrong), 80000 (correct)
sql = sql.replace(
    "(1, 1, 1, 1, 50000.0000),   -- Trà tắc (bienthe id=1)",
    "(1, 1, 1, 1, 10000.0000),   -- Trà tắc (bienthe id=1)")
changes += 1

# Order 2: Pizza thập cẩm M (id=2) x1 = 80000
# Current: tongtien=90000, dongia=60000 (wrong)
sql = sql.replace(
    "(3, 2, 2, 1, 60000.0000);   -- Pizza thập cẩm M (bienthe id=2)",
    "(3, 2, 2, 1, 80000.0000);   -- Pizza thập cẩm M (bienthe id=2)")
changes += 1

# ============================================================
# 6. Add phanHoiCuaQuan column to tbDanhGia
# ============================================================
sql = sql.replace(
    "hinhanh VARCHAR(100) NULL,\r\n    PRIMARY KEY (madg)",
    "hinhanh VARCHAR(100) NULL,\r\n    phanHoiCuaQuan VARCHAR(500) NULL COMMENT 'Phản hồi của quán ăn dành cho đánh giá',\r\n    PRIMARY KEY (madg)")

# ============================================================
# 7. Write back
# ============================================================
with open("mysql_utf8.sql", "w", encoding="utf-8") as f:
    f.write(sql)

print(f"✅ Updated {changes} items in mysql_utf8.sql")
