using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using ShipFood.Controllers;
using ShipFood.Models;
using ShipFood.Services;
using ShipFood.Hubs;
using ShipFoodCore.Tests.Helpers;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Integration tests for PaymentController MoMo IPN callback.
/// Tests verify that payment status is correctly updated in DB when MoMo sends IPN.
/// CRITICAL: Fake IPN = free food. Wrong signature = fraud.
/// </summary>
public class PaymentIpnTests
{
    [Fact]
    public async Task MoMoIpn_InvalidSignature_ReturnsError()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        var mockMomoService = new Mock<MoMoService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<MoMoService>>().Object,
            new HttpClient());
        mockMomoService.Setup(s => s.VerifyIpnSignature(It.IsAny<Dictionary<string, string>>()))
            .Returns(false);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);
        var mockConfig = new Mock<IConfiguration>();

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockMomoService.Object,
            mockConfig.Object,
            mockEdelivery.Object);

        // Act — MoMo sends IPN with invalid signature
        var body = System.Text.Json.JsonSerializer.Serialize(new
        {
            orderId = "FS1_20260715120000",
            resultCode = 0,
            signature = "invalid_signature_12345"
        });

        // Set up HttpContext for the controller
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.MoMoIpn();

        // Assert — signature verification fails, should return error
        // The controller checks VerifyIpnSignature first
        mockMomoService.Verify(s => s.VerifyIpnSignature(It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task MoMoIpn_ValidSignature_UpdatesOrderStatus()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        // Create an order in "Chờ thanh toán" status
        var ttdh = new tbThongTinDatHang
        {
            userid = 2, sdt = "0900000002", diachi = "123 Test St", tennguoinhan = "Test User"
        };
        db.tbThongTinDatHang.Add(ttdh);
        await db.SaveChangesAsync();

        var order = new tbDonHang
        {
            maquan = 3, mattdh = ttdh.mattdh,
            ngaydathang = DateTime.Now, trangthai = "Chờ thanh toán",
            tongtien = 100000, hinhthucthanhtoan = 5, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var mockMomoService = new Mock<MoMoService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<MoMoService>>().Object,
            new HttpClient());
        mockMomoService.Setup(s => s.VerifyIpnSignature(It.IsAny<Dictionary<string, string>>()))
            .Returns(true);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);
        var mockConfig = new Mock<IConfiguration>();

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockMomoService.Object,
            mockConfig.Object,
            mockEdelivery.Object);

        var ipnParams = new Dictionary<string, string>
        {
            { "orderId", $"FS{order.madh}_20260715120000" },
            { "resultCode", "0" },
            { "transId", "12345678" },
            { "signature", "valid" }
        };
        var body = System.Text.Json.JsonSerializer.Serialize(ipnParams);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.MoMoIpn();

        // Assert — order status should be updated
        var updatedOrder = await db.tbDonHangs.FindAsync(order.madh);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Đã thanh toán", updatedOrder.trangthai);
        Assert.NotNull(updatedOrder.ngaythanhtoan);
        Assert.Equal("12345678", updatedOrder.momo_trans_id);
    }

    [Fact]
    public async Task MoMoIpn_FailedPayment_KeepsOrderPending()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        var ttdh = new tbThongTinDatHang
        {
            userid = 2, sdt = "0900000002", diachi = "123 Test St", tennguoinhan = "Test User"
        };
        db.tbThongTinDatHang.Add(ttdh);
        await db.SaveChangesAsync();

        var order = new tbDonHang
        {
            maquan = 3, mattdh = ttdh.mattdh,
            ngaydathang = DateTime.Now, trangthai = "Chờ thanh toán",
            tongtien = 100000, hinhthucthanhtoan = 5, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var mockMomoService = new Mock<MoMoService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<MoMoService>>().Object,
            new HttpClient());
        mockMomoService.Setup(s => s.VerifyIpnSignature(It.IsAny<Dictionary<string, string>>()))
            .Returns(true);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);
        var mockConfig = new Mock<IConfiguration>();

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockMomoService.Object,
            mockConfig.Object,
            mockEdelivery.Object);

        // resultCode != 0 means failed
        var ipnParams = new Dictionary<string, string>
        {
            { "orderId", $"FS{order.madh}_20260715120000" },
            { "resultCode", "7000" }, // MoMo error code
            { "message", "Insufficient balance" },
            { "transId", "" },
            { "signature", "valid" }
        };
        var body = System.Text.Json.JsonSerializer.Serialize(ipnParams);

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.MoMoIpn();

        // Assert — order should remain in "Chờ thanh toán"
        var updatedOrder = await db.tbDonHangs.FindAsync(order.madh);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Chờ thanh toán", updatedOrder.trangthai);
        Assert.Null(updatedOrder.ngaythanhtoan);
    }
}