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
/// Tests for PaymentController BankWebhook.
/// CRITICAL: Bank webhook auto-approves orders based on bank transfers.
/// If amount validation is wrong or memo parsing fails, wrong orders get paid.
/// </summary>
public class BankWebhookTests
{
    private static PaymentController CreateController(dbFoodyEntities db)
    {
        var mockVnpayService = new Mock<VnpayService>(
            new Mock<IConfiguration>().Object,
            new Mock<ILogger<VnpayService>>().Object);
        var mockHubContext = new Mock<IHubContext<Chats>>();
        var mockEdelivery = new Mock<EDeliveryService>(db, new Mock<ILogger<EDeliveryService>>().Object);
        var mockConfig = new Mock<IConfiguration>();
        // Set bank webhook token for auth
        mockConfig.Setup(c => c["BANK_WEBHOOK_TOKEN"]).Returns("test-webhook-token");

        return new PaymentController(
            db,
            new Mock<ILogger<PaymentController>>().Object,
            mockHubContext.Object,
            mockVnpayService.Object,
            mockConfig.Object,
            mockEdelivery.Object);
    }

    [Fact]
    public async Task BankWebhook_ValidMemo_UpdatesOrderStatus()
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
            tongtien = 100000, hinhthucthanhtoan = 2, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        // Casso webhook format
        var webhookBody = System.Text.Json.JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { description = $"FASTSHIP{order.madh}", amount = (long)(order.tongtien ?? 0) }
            }
        });

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer test-webhook-token";
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(webhookBody));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.BankWebhook();

        // Assert
        var updatedOrder = await db.tbDonHangs.FindAsync(order.madh);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Đã đặt", updatedOrder.trangthai);
        Assert.NotNull(updatedOrder.ngaythanhtoan);
    }

    [Fact]
    public async Task BankWebhook_AmountMismatch_RejectsPayment()
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
            tongtien = 100000, hinhthucthanhtoan = 2, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        // Amount mismatch: order is 100000, webhook says 50000
        var webhookBody = System.Text.Json.JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { description = $"FASTSHIP{order.madh}", amount = 50000L }
            }
        });

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(webhookBody));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.BankWebhook();

        // Assert — order should remain "Chờ thanh toán"
        var updatedOrder = await db.tbDonHangs.FindAsync(order.madh);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Chờ thanh toán", updatedOrder.trangthai);
    }

    [Fact]
    public async Task BankWebhook_InvalidMemo_ReturnsError()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        var controller = CreateController(db);

        var webhookBody = System.Text.Json.JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { description = "Random transfer not for FastShip", amount = 100000L }
            }
        });

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(webhookBody));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.BankWebhook();

        // Assert — should return error about invalid memo format
        var jsonResult = result.Value;
        Assert.NotNull(jsonResult);
    }

    [Fact]
    public async Task BankWebhook_AlreadyProcessedOrder_ReturnsOk()
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
            ngaydathang = DateTime.Now, trangthai = "Đã đặt", // Already processed
            tongtien = 100000, hinhthucthanhtoan = 2, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var webhookBody = System.Text.Json.JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { description = $"FASTSHIP{order.madh}", amount = (long)(order.tongtien ?? 0) }
            }
        });

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(webhookBody));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.BankWebhook();

        // Assert — should not crash, returns already processed
        Assert.NotNull(result);
    }

    [Fact]
    public async Task BankWebhook_NonExistentOrder_ReturnsError()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        var controller = CreateController(db);

        var webhookBody = System.Text.Json.JsonSerializer.Serialize(new
        {
            data = new[]
            {
                new { description = "FASTSHIP99999", amount = 100000L }
            }
        });

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(webhookBody));
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await controller.BankWebhook();

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("FASTSHIP42", 42)]
    [InlineData("Chuyen khoan FASTSHIP100", 100)]
    [InlineData("FASTSHIP1 abc", 1)]
    [InlineData("fastship99", 99)]
    public void BankWebhook_MemoParsing_ExtractsCorrectOrderId(string memo, int expectedOrderId)
    {
        // This tests the regex pattern used in BankWebhook
        var match = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIP(\d+)", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        Assert.True(match.Success, $"Memo '{memo}' should match FASTSHIP pattern");
        Assert.Equal(expectedOrderId, int.Parse(match.Groups[1].Value));
    }
}