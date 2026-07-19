using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using ShipFood.Controllers;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;
using ShipFoodCore.Tests.Helpers;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Integration tests for PaymentController VNPAY IPN callback.
/// Tests verify that payment status is correctly updated in DB when VNPAY sends IPN.
/// </summary>
public class PaymentIpnTests
{
    [Fact]
    public async Task VnpayIpn_ValidSignature_UpdatesOrderStatus()
    {
        // Arrange — create a pending order in DB
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
            ngaydathang = DateTime.Now,
            trangthai = "Chờ thanh toán",
            tongtien = 50000,
            hinhthucthanhtoan = 1
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();
        var createdOrderId = order.madh;

        var mockVnpayService = new Mock<VnpayService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<VnpayService>>().Object);
        mockVnpayService.Setup(s => s.VerifySignature(It.IsAny<IDictionary<string, string>>()))
            .Returns(true);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockVnpayService.Object,
            new Mock<IConfiguration>().Object,
            mockEdelivery.Object
        );

        // Simulate valid VNPAY IPN with query params
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString(
            $"?vnp_ResponseCode=00&vnp_TxnRef={createdOrderId}&vnp_TransactionNo=123456&vnp_Amount=5000000&vnp_SecureHash=validhash");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act — VNPAY sends valid IPN
        var result = await controller.VnpayIPN();

        // Assert — Order should now be "Đã đặt"
        var updatedOrder = await db.tbDonHangs.FindAsync(createdOrderId);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Đã đặt", updatedOrder.trangthai);
        Assert.NotNull(updatedOrder.ngaythanhtoan);
    }

    [Fact]
    public async Task VnpayIpn_InvalidSignature_ReturnsError()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        var mockVnpayService = new Mock<VnpayService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<VnpayService>>().Object);
        mockVnpayService.Setup(s => s.VerifySignature(It.IsAny<IDictionary<string, string>>()))
            .Returns(false);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockVnpayService.Object,
            new Mock<IConfiguration>().Object,
            mockEdelivery.Object
        );

        // Simulate VNPAY IPN with invalid signature
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString(
            "?vnp_ResponseCode=00&vnp_TxnRef=1&vnp_SecureHash=invalid");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.VnpayIPN();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        dynamic value = jsonResult.Value!;
        string rspCode = value.GetType().GetProperty("RspCode")?.GetValue(value, null) as string;
        Assert.Equal("97", rspCode);
    }

    [Fact]
    public async Task VnpayIpn_DuplicateCallback_DoesNotDoubleProcess()
    {
        // Arrange — create an already-paid order
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
            ngaydathang = DateTime.Now,
            trangthai = "Đã đặt",
            tongtien = 50000,
            hinhthucthanhtoan = 1
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();
        var createdOrderId = order.madh;

        var mockVnpayService = new Mock<VnpayService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<VnpayService>>().Object);
        mockVnpayService.Setup(s => s.VerifySignature(It.IsAny<IDictionary<string, string>>()))
            .Returns(true);

        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);

        var controller = new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockVnpayService.Object,
            new Mock<IConfiguration>().Object,
            mockEdelivery.Object
        );

        // Simulate duplicate VNPAY IPN
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString(
            $"?vnp_ResponseCode=00&vnp_TxnRef={createdOrderId}&vnp_TransactionNo=999999&vnp_Amount=5000000&vnp_SecureHash=validhash");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act — duplicate IPN
        var result = await controller.VnpayIPN();

        // Assert — Order status should still be "Đã đặt"
        var unchangedOrder = await db.tbDonHangs.FindAsync(createdOrderId);
        Assert.NotNull(unchangedOrder);
        Assert.Equal("Đã đặt", unchangedOrder.trangthai);
    }
}
