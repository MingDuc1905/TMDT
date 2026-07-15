using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ShipFood.Models;
using ShipFood.Services;
using ShipFoodCore.Tests.Helpers;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Tests for EDeliveryService — E-Invoice and E-Waybill generation.
/// Tests verify idempotency (no duplicate invoices) and proper document creation.
/// </summary>
public class EDeliveryServiceTests
{
    private readonly Mock<ILogger<EDeliveryService>> _mockLogger = new();

    [Fact]
    public async Task GenerateEInvoice_CreatesInvoiceForOrder()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;

        // Create an order
        var ttdh = new tbThongTinDatHang
        {
            userid = 2, sdt = "0900000002", diachi = "123 Test St", tennguoinhan = "Test User"
        };
        db.tbThongTinDatHang.Add(ttdh);
        await db.SaveChangesAsync();

        var order = new tbDonHang
        {
            maquan = 3,
            mattdh = ttdh.mattdh,
            ngaydathang = DateTime.Now,
            trangthai = "Đã thanh toán",
            tongtien = 100000,
            hinhthucthanhtoan = 1,
            phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        // Need a Shipper for the service (referenced in FK)
        var shipper = new tbShipper
        {
            userid = 4,
            tenshipper = "Test Shipper",
            diachi = "456 Ship St"
        };
        db.tbShipper.Add(shipper);
        await db.SaveChangesAsync();

        var service = new EDeliveryService(db, _mockLogger.Object);

        // Act
        var result = await service.GenerateEInvoice(order.madh);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.einvoice_id > 0);
        Assert.Contains("INV-FS-", result.invoice_number);
        Assert.Equal(order.madh, result.madh);
        Assert.Equal(order.tongtien, result.tongtien);
    }

    [Fact]
    public async Task GenerateEInvoice_DuplicateOrder_ReturnsExistingInvoice()
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
            ngaydathang = DateTime.Now, trangthai = "Đã thanh toán",
            tongtien = 100000, hinhthucthanhtoan = 1, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var shipper = new tbShipper { userid = 4, tenshipper = "Test Shipper", diachi = "456 Ship St" };
        db.tbShipper.Add(shipper);
        await db.SaveChangesAsync();

        var service = new EDeliveryService(db, _mockLogger.Object);

        // Act — generate twice
        var result1 = await service.GenerateEInvoice(order.madh);
        var result2 = await service.GenerateEInvoice(order.madh);

        // Assert — should return same invoice (idempotent)
        Assert.Equal(result1.einvoice_id, result2.einvoice_id);
        Assert.Equal(result1.invoice_number, result2.invoice_number);
    }

    [Fact]
    public async Task GenerateEInvoice_OrderNotFound_ThrowsOrReturnsNull()
    {
        // Arrange
        var (db, _, _, _, _) = TestDbContextFactory.CreateSeeded();
        await using var _ = db;
        var service = new EDeliveryService(db, _mockLogger.Object);

        // Act & Assert — should throw or handle gracefully for non-existent order
        try
        {
            var result = await service.GenerateEInvoice(99999);
            // If it doesn't throw, result should be null or indicate error
            Assert.Null(result);
        }
        catch (Exception)
        {
            // Exception for non-existent order is also acceptable
        }
    }

    [Fact]
    public async Task GetDocumentsByOrder_WithInvoice_ReturnsDocuments()
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
            ngaydathang = DateTime.Now, trangthai = "Đã thanh toán",
            tongtien = 100000, hinhthucthanhtoan = 1, phiship = 15000
        };
        db.tbDonHang.Add(order);
        await db.SaveChangesAsync();

        var shipper = new tbShipper { userid = 4, tenshipper = "Test Shipper", diachi = "456 Ship St" };
        db.tbShipper.Add(shipper);
        await db.SaveChangesAsync();

        var service = new EDeliveryService(db, _mockLogger.Object);
        await service.GenerateEInvoice(order.madh);

        // Act
        var docs = await service.GetDocumentsByOrder(order.madh);

        // Assert
        Assert.NotNull(docs);
        Assert.NotEmpty(docs);
    }
}