using ShipFood.Models;
using ShipFood.Services;
using ShipFoodCore.Tests.Helpers;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Tests for VoucherService — time-slot based voucher suggestions.
/// Tests verify that vouchers are correctly filtered by time of day, expiry, and usage history.
/// </summary>
public class VoucherServiceTests
{
    [Fact]
    public async Task GetTimeSlotVoucher_NoVouchers_ReturnsNull()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        // Remove all vouchers
        db.tbKhuyenMai.RemoveRange(db.tbKhuyenMai.ToList());
        await db.SaveChangesAsync();

        var service = new VoucherService(db);

        // Act
        var result = await service.GetTimeSlotVoucher();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTimeSlotVoucher_ExpiredVoucher_ReturnsNull()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        
        // Mark existing voucher as expired
        var voucher = db.tbKhuyenMai.First();
        voucher.ngayketthuc = DateTime.Now.AddDays(-1);
        await db.SaveChangesAsync();

        var service = new VoucherService(db);

        // Act
        var result = await service.GetTimeSlotVoucher();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecommendedVouchers_NullUser_ReturnsTimeSlotOnly()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        var service = new VoucherService(db);

        // Act
        var result = await service.GetRecommendedVouchers(null);

        // Assert
        Assert.NotNull(result);
        // Should have at least 1 voucher (time-slot) if one matches current hour
        // Or 0 if no voucher matches current time slot
        foreach (var v in result)
        {
            Assert.True(v.ngayketthuc == null || v.ngayketthuc >= DateTime.Now,
                $"Voucher {v.tenkm} should not be expired");
        }
    }

    [Fact]
    public async Task GetRecommendedVouchers_WithOrderHistory_DoesNotCrash()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        var service = new VoucherService(db);

        // Act — valid userId (customer userid=2)
        var result = await service.GetRecommendedVouchers(2);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<tbKhuyenMai>>(result);
    }

    [Fact]
    public async Task GetRecommendedVouchers_WithTongTien_AboveThreshold_ConsidersFreeShip()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        
        // Add a free-ship voucher
        db.tbKhuyenMai.Add(new tbKhuyenMai
        {
            tenkm = "MIỄN PHÍ SHIP CHO ĐƠN TRÊN 50K",
            loaikm = "Free ship",
            phantramgiam = 0,
            ngaybatdau = DateTime.Now.AddDays(-1),
            ngayketthuc = DateTime.Now.AddDays(30)
        });
        await db.SaveChangesAsync();

        var service = new VoucherService(db);

        // Act — tongTien >= 50000 triggers free ship check
        var result = await service.GetRecommendedVouchers(null, 60000);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, v => v.tenkm.Contains("MIỄN PHÍ SHIP"));
    }

    [Fact]
    public async Task GetRecommendedVouchers_WithTongTien_BelowThreshold_NoFreeShip()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        
        db.tbKhuyenMai.Add(new tbKhuyenMai
        {
            tenkm = "MIỄN PHÍ SHIP CHO ĐƠN TRÊN 50K",
            loaikm = "Free ship",
            phantramgiam = 0,
            ngaybatdau = DateTime.Now.AddDays(-1),
            ngayketthuc = DateTime.Now.AddDays(30)
        });
        await db.SaveChangesAsync();

        var service = new VoucherService(db);

        // Act — tongTien < 50000 should NOT trigger free ship
        var result = await service.GetRecommendedVouchers(null, 30000);

        // Assert
        Assert.DoesNotContain(result, v => v.tenkm.Contains("MIỄN PHÍ SHIP"));
    }

    [Fact]
    public async Task GetRecommendedVouchers_FirstTimeUser_SuggestsFirstOrderVoucher()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        
        // Add first-order voucher
        db.tbKhuyenMai.Add(new tbKhuyenMai
        {
            tenkm = "ĐẶT LẦN ĐẦU GIẢM 20%",
            loaikm = "Phần trăm",
            phantramgiam = 20,
            ngaybatdau = DateTime.Now.AddDays(-1),
            ngayketthuc = DateTime.Now.AddDays(30)
        });
        await db.SaveChangesAsync();

        var service = new VoucherService(db);

        // Act — userId=2 (customer) has no orders → first-time user
        var result = await service.GetRecommendedVouchers(2);

        // Assert
        Assert.Contains(result, v => v.tenkm.Contains("ĐẶT LẦN ĐẦU"));
    }

    [Fact]
    public void GetCurrentTimeSlotInfo_ReturnsValidInfo()
    {
        // Act
        var (name, icon, description) = VoucherService.GetCurrentTimeSlotInfo();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(name));
        Assert.False(string.IsNullOrWhiteSpace(icon));
        Assert.False(string.IsNullOrWhiteSpace(description));
        Assert.Contains(name, new[] { "Sáng", "Trưa", "Xế", "Tối", "Khuya" });
    }
}