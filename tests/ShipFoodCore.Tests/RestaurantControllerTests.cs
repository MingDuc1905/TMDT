using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ShipFood.Controllers;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Tests;

public class RestaurantControllerTests
{
    /// <summary>
    /// Create a fresh InMemory database context for testing.
    /// </summary>
    private static dbFoodyEntities CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<dbFoodyEntities>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new dbFoodyEntities(options);
    }

    /// <summary>
    /// Seed basic data: 1 restaurant user, 2 food items with variants, 1 completed order.
    /// </summary>
    private static int SeedRestaurantData(dbFoodyEntities context)
    {
        // User (Quán ăn)
        var user = new tbUser
        {
            userid = 10,
            username = "testquan",
            pwd = "test123",
            loaitaikhoan = "Quán ăn",
            sdt = "0912345678",
            email = "test@quan.com",
            trangthai = 1
        };
        context.tbUser.Add(user);

        // Quán ăn
        var quanAn = new tbQuanAn
        {
            userid = 10,
            tenquanan = "Test Quán",
            diachi = "123 Test St",
            trangthai = "Đang mở cửa",
            hinhanh = "test.jpg",
            tbUser = user
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
            maquanan = 10,
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
            maquanan = 10,
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

        // Đơn hàng hoàn thành
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
            maquan = 10,
            mattdh = 1,
            ngaydathang = DateTime.Now,
            trangthai = "Hoàn thành",
            tongtien = 110000
        };
        context.tbDonHang.Add(dh);

        // Chi tiết đơn hàng (mamon = tbBienTheMonAn.id)
        var ct1 = new tbChiTietDonHang
        {
            mactdh = 1,
            madh = 1,
            mamon = 1,
            soluong = 2,
            dongia = 50000
        };
        context.tbChiTietDonHang.Add(ct1);

        var ct2 = new tbChiTietDonHang
        {
            mactdh = 2,
            madh = 1,
            mamon = 2,
            soluong = 1,
            dongia = 60000
        };
        context.tbChiTietDonHang.Add(ct2);

        context.SaveChanges();
        return 10; // userid of restaurant
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: getQuanAn — KHÔNG được crash vì [NotMapped] Include
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void getQuanAn_ShouldNotCrash_WhenDataExists()
    {
        // Arrange
        var context = CreateContext("GetQuanAn_WithData");
        SeedRestaurantData(context);
        
        // Create controller with mocked HttpContext to provide session
        var controller = CreateControllerWithSession(context, userId: 10, role: "Quán ăn");

        // Act
        var result = controller.PublicGetQuanAn();

        // Assert — should NOT throw InvalidOperationException
        Assert.NotNull(result);
        Assert.Equal("Test Quán", result.tenquanan);
        Assert.NotEmpty(result.tbMonAns);
    }

    [Fact]
    public void getQuanAn_TbMonAn_ShouldHaveBienTheMonAns()
    {
        // Arrange
        var context = CreateContext("GetQuanAn_BienThe");
        SeedRestaurantData(context);
        var controller = CreateControllerWithSession(context, userId: 10, role: "Quán ăn");

        // Act
        var result = controller.PublicGetQuanAn();

        // Assert — tbMonAns should have tbBienTheMonAns loaded via correct Include
        Assert.NotNull(result);
        foreach (var mon in result.tbMonAns)
        {
            Assert.NotEmpty(mon.tbBienTheMonAns);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST: Index — trang dashboard quán ăn không crash
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task Index_ShouldNotCrash()
    {
        // Arrange
        var context = CreateContext("RestaurantIndex");
        SeedRestaurantData(context);
        var controller = CreateControllerWithSession(context, userId: 10, role: "Quán ăn");

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert — should NOT throw
        Assert.NotNull(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a RestaurantController with an in-memory context and mocked HttpContext/Session.
    /// </summary>
    private static RestaurantController CreateControllerWithSession(
        dbFoodyEntities context, int userId, string role)
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(".");

        var hubMock = new Mock<Microsoft.AspNetCore.SignalR.IHubContext<Hubs.Chats>>();
        var hubClientsMock = new Mock<Microsoft.AspNetCore.SignalR.IHubClients>();
        var clientProxyMock = new Mock<Microsoft.AspNetCore.SignalR.IClientProxy>();
        hubMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);
        hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        // Dùng real RecommendationService (không mock) — method GetRestaurantAprioriInsights không phải virtual
        var reco = new RecommendationService(context);

        var controller = new RestaurantController(context, envMock.Object, hubMock.Object, reco);

        // Setup HttpContext with Session
        var httpContext = new DefaultHttpContext();
        var session = new Mock<ISession>();
        var userJson = System.Text.Json.JsonSerializer.Serialize(new tbUser
        {
            userid = userId,
            username = "testquan",
            loaitaikhoan = role,
            sdt = "0912345678",
            email = "test@test.com",
            pwd = "test123",
            trangthai = 1
        });

        byte[] userBytes = System.Text.Encoding.UTF8.GetBytes(userJson);
        session.Setup(s => s.TryGetValue("user", out It.Ref<byte[]>.IsAny!))
            .Returns((string key, out byte[] value) =>
            {
                value = userBytes;
                return true;
            });

        httpContext.Session = session.Object;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }
}

/// <summary>
/// Extension methods to expose private/protected members for testing.
/// RestaurantController.getQuanAn() is a public method.
/// </summary>
public static class RestaurantControllerExtensions
{
    public static tbQuanAn PublicGetQuanAn(this RestaurantController controller)
    {
        return controller.getQuanAn();
    }
}
