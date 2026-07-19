using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFoodCore.Tests.Helpers;

/// <summary>
/// Factory that creates fresh InMemory DbContext instances for each test.
/// Uses unique database names to ensure test isolation.
/// </summary>
public static class TestDbContextFactory
{
    public static dbFoodyEntities Create(string? dbName = null)
    {
        dbName ??= $"ShipFoodTest_{Guid.NewGuid():N}";
        
        var options = new DbContextOptionsBuilder<dbFoodyEntities>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new dbFoodyEntities(options);
    }

    /// <summary>
    /// Creates a DbContext and seeds it with common test data:
    /// - 1 Admin user, 1 Customer user, 1 Restaurant user, 1 Shipper user
    /// - 1 Restaurant, 1 Category, 2 Dishes with variants
    /// - 1 Payment method (COD), 1 Payment method (Bank Transfer)
    /// - 1 Active voucher
    /// </summary>
    public static (dbFoodyEntities db, tbUser admin, tbUser customer, tbUser restaurantUser, tbUser shipper)
        CreateSeeded(string? dbName = null)
    {
        var db = Create(dbName);

        // ─── Users ───
        var admin = new tbUser { userid = 1, username = "admin", pwd = "Admin@123", loaitaikhoan = "Admin", sdt = "0900000001", email = "admin@test.com", trangthai = 1 };
        var customer = new tbUser { userid = 2, username = "customer1", pwd = "Customer@123", loaitaikhoan = "Khách hàng", sdt = "0900000002", email = "customer@test.com", trangthai = 1 };
        var restaurantUser = new tbUser { userid = 3, username = "restaurant1", pwd = "Restaurant@123", loaitaikhoan = "Quán ăn", sdt = "0900000003", email = "restaurant@test.com", trangthai = 1 };
        var shipper = new tbUser { userid = 4, username = "shipper1", pwd = "Shipper@123", loaitaikhoan = "Shipper", sdt = "0900000004", email = "shipper@test.com", trangthai = 1 };

        db.tbUser.AddRange(admin, customer, restaurantUser, shipper);
        db.SaveChanges();

        // ─── Restaurant profile ───
        var restaurant = new tbQuanAn
        {
            userid = 3,
            tenquanan = "Test Restaurant",
            diachi = "123 Test Street, HCM",
            trangthai = "Hoạt động",
            hinhanh = "/img/test.jpg"
        };
        db.tbQuanAn.Add(restaurant);
        db.SaveChanges();

        // ─── Category ───
        var category = new tbDanhMuc { madanhmuc = 1, tendanhmuc = "Phở", mota = "Phở và mì" };
        db.tbDanhMuc.Add(category);
        db.SaveChanges();

        // ─── Dishes ───
        var dish1 = new tbMonAn { mamon = 1, tenmon = "Phở Bò", maquanan = 3, madanhmuc = 1, conhang = true, isDeleted = false };
        var dish2 = new tbMonAn { mamon = 2, tenmon = "Bún Chả", maquanan = 3, madanhmuc = 1, conhang = true, isDeleted = false };
        db.tbMonAn.AddRange(dish1, dish2);
        db.SaveChanges();

        // ─── Variants (prices) ───
        var variant1 = new tbBienTheMonAn { id = 1, mamon = 1, size = null, giatien = 45000 };
        var variant2 = new tbBienTheMonAn { id = 2, mamon = 1, size = "L", giatien = 55000 };
        var variant3 = new tbBienTheMonAn { id = 3, mamon = 2, size = null, giatien = 40000 };
        db.tbBienTheMonAn.AddRange(variant1, variant2, variant3);
        db.SaveChanges();

        // ─── Payment methods ───
        var codMethod = new tbLoaiHinhThanhToan { mahttt = 1, tenhinhthuc = "Tiền mặt" };
        var bankMethod = new tbLoaiHinhThanhToan { mahttt = 2, tenhinhthuc = "Chuyển khoản ngân hàng" };
        var momoMethod = new tbLoaiHinhThanhToan { mahttt = 5, tenhinhthuc = "Ví MoMo" };
        var vnpayMethod = new tbLoaiHinhThanhToan { mahttt = 6, tenhinhthuc = "VNPAY", mota = "Thanh toán qua cổng VNPAY (ATM, Visa, Mastercard)" };
        db.tbLoaiHinhThanhToan.AddRange(codMethod, bankMethod, momoMethod, vnpayMethod);
        db.SaveChanges();

        // ─── Active voucher ───
        var voucher = new tbKhuyenMai
        {
            makm = 1,
            tenkm = "GIAM10%",
            mota = "Giảm 10% cho đơn hàng đầu tiên",
            loaikm = "Phần trăm",
            phantramgiam = 10,
            ngaybatdau = DateTime.Now.AddDays(-1),
            ngayketthuc = DateTime.Now.AddDays(30)
        };
        db.tbKhuyenMai.Add(voucher);
        db.SaveChanges();

        return (db, admin, customer, restaurantUser, shipper);
    }
}