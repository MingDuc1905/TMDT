using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ShipFood.Controllers;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Tests;

public class HomeControllerTest
{
    /// <summary>
    /// Seed an in-memory DbContext with test data for reviews (tbDanhGia)
    /// with a controlled number of reviews across multiple restaurants.
    /// Pass an optional existingContext to seed into an existing database
    /// (useful for multi-restaurant isolation tests).
    /// </summary>
    private static (dbFoodyEntities Context, int QuanId) CreateSeedData(
        int reviewCount,
        dbFoodyEntities? existingContext = null,
        int? quanIdOverride = null)
    {
        var context = existingContext ?? new dbFoodyEntities(
            new DbContextOptionsBuilder<dbFoodyEntities>()
                .UseInMemoryDatabase(databaseName: $"GetReviews_{Guid.NewGuid()}")
                .Options);

        var quanId = quanIdOverride ?? 1001;

        // ── Khách hàng ──
        var kh = new tbKhachHang { userid = quanId * 10 + 1, tenkh = "Nguyễn Văn A" };

        // ── Thông tin đặt hàng ──
        var ttdh = new tbThongTinDatHang
        {
            mattdh = quanId * 10 + 1,
            sdt = "0912345678",
            diachi = "123 Test St",
            tennguoinhan = "Nguyễn Văn A",
            userid = quanId * 10 + 1,
            tbKhachHang = kh
        };

        // ── Đơn hàng ──
        var dh = new tbDonHang
        {
            madh = quanId * 10 + 1,
            maquan = quanId,
            mattdh = quanId * 10 + 1,
            ngaydathang = new DateTime(2025, 6, 15),
            trangthai = "Hoàn thành",
            tbThongTinDatHang = ttdh
        };

        // ── Món ăn ──
        var monAn = new tbMonAn
        {
            mamon = quanId * 10 + 1,
            tenmon = "Phở bò",
            hinhanh = "pho-bo.jpg",
            maquanan = quanId
        };

        // ── Chi tiết đơn hàng + Đánh giá ──
        var chiTietList = new List<tbChiTietDonHang>();
        var danhGiaList = new List<tbDanhGia>();

        for (int i = 0; i < reviewCount; i++)
        {
            var mactdh = quanId * 1000 + i + 1;
            var chiTiet = new tbChiTietDonHang
            {
                mactdh = mactdh,
                madh = quanId * 10 + 1,
                mamon = quanId * 10 + 1,
                soluong = 1,
                tbDonHang = dh,
                tbMonAn = monAn
            };
            chiTietList.Add(chiTiet);

            var dg = new tbDanhGia
            {
                madg = reviewCount - i + quanId * 100,  // unique madg per restaurant
                mactdh = mactdh,
                diemdanhgia = (i % 5) + 1, // scores 1..5 cycling
                nhanxet = $"Review #{mactdh}"
            };
            danhGiaList.Add(dg);
        }

        // Link reviews back to their chi-tiet
        for (int i = 0; i < reviewCount; i++)
        {
            chiTietList[i].tbDanhGias.Add(danhGiaList[i]);
        }

        context.tbKhachHang.Add(kh);
        context.tbThongTinDatHang.Add(ttdh);
        context.tbDonHang.Add(dh);
        context.tbMonAn.Add(monAn);
        foreach (var ct in chiTietList) context.tbChiTietDonHang.Add(ct);
        foreach (var dg in danhGiaList) context.tbDanhGia.Add(dg);

        context.SaveChanges();

        return (context, quanId);
    }

    /// <summary>
    /// Create a HomeController hooked up to the given in-memory context
    /// with a mocked RecommendationService (unused by GetReviews).
    /// </summary>
    private static HomeController CreateController(dbFoodyEntities context)
    {
        var mockReco = new Mock<RecommendationService>(null!);
        var controller = new HomeController(context, mockReco.Object);

        // GetReviews does NOT use HttpContext.Session, so no further setup needed.

        return controller;
    }

    // ──────────────────────────────────────────────
    // TESTS
    // ──────────────────────────────────────────────

    [Fact]
    public void GetReviews_Page1_ReturnsFirstPageItems()
    {
        // Arrange: 10 reviews total, pageSize = 3
        var (context, quanId) = CreateSeedData(10);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 1, pageSize: 3) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Equal(10, (int)data.total);               // total across all pages
        Assert.Equal(3, ((IEnumerable<object>)data.reviews).Count()); // page has 3 items
        Assert.Equal(1, (int)data.page);
        Assert.Equal(3, (int)data.pageSize);
    }

    [Fact]
    public void GetReviews_Page2_ReturnsRemainingItems()
    {
        // Arrange: 10 reviews, pageSize = 3 → page 2 has items 4,5,6
        var (context, quanId) = CreateSeedData(10);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 2, pageSize: 3) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Equal(3, ((IEnumerable<object>)data.reviews).Count());
        Assert.Equal(2, (int)data.page);
    }

    [Fact]
    public void GetReviews_LastPage_ReturnsPartialPage()
    {
        // Arrange: 10 reviews, pageSize = 4 → pages: [1..4], [5..8], [9,10]
        var (context, quanId) = CreateSeedData(10);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 3, pageSize: 4) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Equal(2, ((IEnumerable<object>)data.reviews).Count()); // last page has 2
        Assert.Equal(3, (int)data.page);
        Assert.Equal(4, (int)data.pageSize);
    }

    [Fact]
    public void GetReviews_PageBeyondMax_ReturnsEmpty()
    {
        // Arrange: 5 reviews, pageSize = 5 → page 2 should be empty
        var (context, quanId) = CreateSeedData(5);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 2, pageSize: 5) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Empty(data.reviews);                        // empty list
        Assert.Equal(5, (int)data.total);                  // total still 5
        Assert.Equal(2, (int)data.page);
    }

    [Fact]
    public void GetReviews_ZeroReviews_ReturnsSuccessWithEmptyAndZeroAvg()
    {
        // Arrange: seed with 0 reviews
        var (context, quanId) = CreateSeedData(0);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 1, pageSize: 5) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Empty(data.reviews);
        Assert.Equal(0, (int)data.total);
        Assert.Equal(0, (double)data.avgDiem);
    }

    [Fact]
    public void GetReviews_NonExistentQuanId_ReturnsSuccessWithEmpty()
    {
        // Arrange
        var (context, _ /* realQuanId we ignore */) = CreateSeedData(5);
        var controller = CreateController(context);

        // Act — use quanId that does NOT exist in seed
        var result = controller.GetReviews(quanId: 9999, page: 1, pageSize: 5) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Empty(data.reviews);
        Assert.Equal(0, (int)data.total);
        Assert.Equal(0, (double)data.avgDiem);
    }

    [Fact]
    public void GetReviews_ReturnsCorrectTotalCount()
    {
        // Arrange: 7 reviews
        var (context, quanId) = CreateSeedData(7);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 1, pageSize: 10) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.Equal(7, (int)data.total);
        Assert.Equal(7, ((IEnumerable<object>)data.reviews).Count()); // all on one page
    }

    [Fact]
    public void GetReviews_ReturnsCorrectAverageRating()
    {
        // Arrange: 10 reviews with scores cycling 1..5 (sum = 1+2+3+4+5+1+2+3+4+5 = 30, avg = 3.0)
        var (context, quanId) = CreateSeedData(10);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 1, pageSize: 10) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        // Scores: 1,2,3,4,5,1,2,3,4,5 → sum=30, avg=3.0
        Assert.Equal(3.0, (double)data.avgDiem, precision: 1);
    }

    [Fact]
    public void GetReviews_OrdersByMadgDescending()
    {
        // Arrange: 5 reviews — madg will be 5+quanId*100, 4+quanId*100, ..., 1+quanId*100
        var (context, quanId) = CreateSeedData(5);
        var controller = CreateController(context);

        // Act
        var result = controller.GetReviews(quanId, page: 1, pageSize: 5) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert — should be in descending madg order
        Assert.NotNull(data);
        var reviews = ((IEnumerable<object>)data.reviews).Cast<dynamic>().ToList();
        Assert.Equal(5, reviews.Count);

        var madgList = reviews.Select(r => (int)r.madg).ToList();
        // Seed creates madg: 5+offset, 4+offset, 3+offset, 2+offset, 1+offset
        var offset = quanId * 100;
        Assert.Equal([5 + offset, 4 + offset, 3 + offset, 2 + offset, 1 + offset], madgList);
    }

    [Fact]
    public void GetReviews_DefaultPageSizeIs5()
    {
        // Arrange: 12 reviews, call without explicit pageSize
        var (context, quanId) = CreateSeedData(12);
        var controller = CreateController(context);

        // Act — use default parameters (page=1, pageSize=5)
        var result = controller.GetReviews(quanId, page: 1) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert
        Assert.NotNull(data);
        Assert.Equal(12, (int)data.total);
        Assert.Equal(5, ((IEnumerable<object>)data.reviews).Count()); // default pageSize=5
        Assert.Equal(5, (int)data.pageSize);
    }

    [Fact]
    public void GetReviews_MultipleRestaurants_DoesNotLeakReviews()
    {
        // Arrange: seed both restaurants into the SAME context using existingContext parameter
        var options = new DbContextOptionsBuilder<dbFoodyEntities>()
            .UseInMemoryDatabase(databaseName: $"GetReviews_Isolation_{Guid.NewGuid()}")
            .Options;

        var context = new dbFoodyEntities(options);

        // Restaurant 1 (quanId 1001) — 3 reviews
        var (_, _) = CreateSeedData(3, existingContext: context, quanIdOverride: 1001);

        // Restaurant 2 (quanId 2002) — 2 reviews
        var (_, _) = CreateSeedData(2, existingContext: context, quanIdOverride: 2002);

        var controller = CreateController(context);

        // Act — get reviews for Restaurant 2 only
        var result = controller.GetReviews(quanId: 2002, page: 1, pageSize: 10) as JsonResult;
        var data = result?.Value as dynamic;

        // Assert — only 2 reviews for Restaurant 2 should be returned
        Assert.NotNull(data);
        Assert.True((bool)data.success);
        Assert.Equal(2, (int)data.total);
        Assert.Equal(2, ((IEnumerable<object>)data.reviews).Count());

        // Also verify Restaurant 1 still has 3 reviews (data integrity)
        var result2 = controller.GetReviews(quanId: 1001, page: 1, pageSize: 10) as JsonResult;
        var data2 = result2?.Value as dynamic;
        Assert.Equal(3, (int)data2.total);
    }

    [Fact]
    public void GetReviews_AvgDiemIsConsistentAcrossPages()
    {
        // Arrange: 10 reviews with scores 1..5 cycling, avg = 3.0
        var (context, quanId) = CreateSeedData(10);
        var controller = CreateController(context);

        // Act — get page 1 and page 2
        var result1 = controller.GetReviews(quanId, page: 1, pageSize: 3) as JsonResult;
        var data1 = result1?.Value as dynamic;

        var result2 = controller.GetReviews(quanId, page: 2, pageSize: 3) as JsonResult;
        var data2 = result2?.Value as dynamic;

        // Assert — avgDiem should be the SAME across pages (computed from all reviews, not page subset)
        Assert.NotNull(data1);
        Assert.NotNull(data2);
        Assert.Equal((double)data1.avgDiem, (double)data2.avgDiem, precision: 1);
        Assert.Equal(3.0, (double)data1.avgDiem, precision: 1);
    }
}
