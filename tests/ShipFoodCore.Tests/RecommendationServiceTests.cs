using Microsoft.EntityFrameworkCore;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Tests;

public class RecommendationServiceTests
{
    /// <summary>
    /// Create a fresh InMemory database context with seed data for testing.
    /// </summary>
    private static dbFoodyEntities CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<dbFoodyEntities>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new dbFoodyEntities(options);
    }

    /// <summary>
    /// Seed basic test data: 1 restaurant, 2 food items (each with 1 variant), 
    /// and optionally some order history.
    /// </summary>
    private static void SeedBasicData(dbFoodyEntities context)
    {
        // Quán ăn
        var quanAn = new tbQuanAn
        {
            userid = 1,
            tenquanan = "Test Quán",
            diachi = "123 Test St",
            trangthai = "Đang mở cửa",
            hinhanh = "test.jpg"
        };
        context.tbQuanAn.Add(quanAn);

        // Danh mục
        var dm = new tbDanhMuc { madanhmuc = 1, tendanhmuc = "Món chính" };
        context.tbDanhMuc.Add(dm);

        // Món 1
        var mon1 = new tbMonAn
        {
            mamon = 1,
            tenmon = "Phở bò",
            maquanan = 1,
            madanhmuc = 1,
            hinhanh = "pho.jpg",
            conhang = true
        };
        context.tbMonAn.Add(mon1);

        // Biến thể Món 1
        var bt1 = new tbBienTheMonAn
        {
            id = 1,
            mamon = 1,
            size = "Vừa",
            giatien = 50000
        };
        context.tbBienTheMonAn.Add(bt1);

        // Món 2
        var mon2 = new tbMonAn
        {
            mamon = 2,
            tenmon = "Bún bò",
            maquanan = 1,
            madanhmuc = 1,
            hinhanh = "bun.jpg",
            conhang = true
        };
        context.tbMonAn.Add(mon2);

        // Biến thể Món 2
        var bt2 = new tbBienTheMonAn
        {
            id = 2,
            mamon = 2,
            size = "Lớn",
            giatien = 60000
        };
        context.tbBienTheMonAn.Add(bt2);

        // Đơn hàng hoàn thành (để Apriori/trending có dữ liệu)
        var ttdh = new tbThongTinDatHang
        {
            mattdh = 1,
            sdt = "0912345678",
            diachi = "456 Test Ave",
            tennguoinhan = "Test User",
            userid = 1
        };
        context.tbThongTinDatHang.Add(ttdh);

        var dh = new tbDonHang
        {
            madh = 1,
            maquan = 1,
            mattdh = 1,
            ngaydathang = DateTime.Now,
            trangthai = "Hoàn thành",
            tongtien = 110000
        };
        context.tbDonHang.Add(dh);

        // Chi tiết đơn hàng (liên kết với biến thể)
        var ct1 = new tbChiTietDonHang
        {
            mactdh = 1,
            madh = 1,
            mamon = 1,  // FK to tbBienTheMonAn.id
            soluong = 2,
            dongia = 50000
        };
        context.tbChiTietDonHang.Add(ct1);

        var ct2 = new tbChiTietDonHang
        {
            mactdh = 2,
            madh = 1,
            mamon = 2,  // FK to tbBienTheMonAn.id
            soluong = 1,
            dongia = 60000
        };
        context.tbChiTietDonHang.Add(ct2);

        context.SaveChanges();
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: GetTimeBasedRecommendations — KHÔNG được crash
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetTimeBasedRecommendations_ShouldNotCrash_WhenDataExists()
    {
        // Arrange
        var context = CreateContext("TimeBased_WithData");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act
        var result = await service.GetTimeBasedRecommendations(take: 4);

        // Assert — should return items WITHOUT throwing InvalidOperationException
        Assert.NotNull(result);
        Assert.NotEmpty(result);  // should find items matching time-based keywords
    }

    [Fact]
    public async Task GetTimeBasedRecommendations_ShouldReturnEmpty_WhenNoData()
    {
        // Arrange — empty database
        var context = CreateContext("TimeBased_Empty");
        var service = new RecommendationService(context);

        // Act
        var result = await service.GetTimeBasedRecommendations(take: 4);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTimeBasedRecommendations_ShouldRespectTakeParameter()
    {
        // Arrange
        var context = CreateContext("TimeBased_TakeParam");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act
        var result = await service.GetTimeBasedRecommendations(take: 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: GetPopularPairs — KHÔNG được crash
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetPopularPairs_ShouldNotCrash_WhenDataExists()
    {
        // Arrange
        var context = CreateContext("PopularPairs_WithData");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act
        var result = await service.GetPopularPairs(take: 6);

        // Assert — should return items WITHOUT crashing
        Assert.NotNull(result);
        // May be empty since we only have 1 order with 2 items, which IS ≥ 2
        // But we just check it doesn't throw
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: GetAprioriRecommendations — KHÔNG được crash
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAprioriRecommendations_ShouldNotCrash_WithValidInput()
    {
        // Arrange
        var context = CreateContext("Apriori_WithData");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act — pass a valid monId (1)
        var result = await service.GetAprioriRecommendations(new List<int> { 1 }, take: 4);

        // Assert — should NOT throw
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAprioriRecommendations_ShouldHandleEmptyInput()
    {
        // Arrange
        var context = CreateContext("Apriori_EmptyInput");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act — empty input should fallback to GetPopularPairs
        var result = await service.GetAprioriRecommendations(new List<int>(), take: 4);

        // Assert — should NOT throw
        Assert.NotNull(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: GetTrendingItems — KHÔNG được crash
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetTrendingItems_ShouldNotCrash_WhenDataExists()
    {
        // Arrange
        var context = CreateContext("Trending_WithData");
        SeedBasicData(context);
        var service = new RecommendationService(context);

        // Act
        var result = await service.GetTrendingItems(take: 6);

        // Assert — should NOT throw
        Assert.NotNull(result);
    }
}
