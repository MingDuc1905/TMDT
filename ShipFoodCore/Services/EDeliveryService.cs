// ============================================================
// 📄 EDeliveryService — Hóa đơn & Vận đơn điện tử
// ============================================================
// Ý nghĩa: Sinh E-Invoice (hóa đơn) và E-Waybill (vận đơn) cho đơn hàng thành công
// Chức năng: GenerateEInvoice, GenerateEWaybill, GetDocumentsByOrder, GetByNumber — idempotent
// KEYWORDS: invoice, waybill, e-invoice, e-waybill, hóa đơn điện tử, vận đơn điện tử, QR
//
// LUỒNG DỮ LIỆU:
//   Payment xong ⭢ PaymentController.MomoReturn/MockPaymentWebhook ⭢ EDeliveryController.Generate
//   EDeliveryController.Generate ⭢ gọi GenerateEInvoice() ⭢ tạo tbEInvoice mới (idempotent)
//   Shipper giao thành công ⭢ ShipperController.hoantatdon() ⭢ EDeliveryController.GenerateWaybill
//   GenerateEWaybill() ⭢ tạo tbEInvoice (loaichungtu="EWaybill") với QR code + chữ ký số
//   View order ⭢ EDeliveryController.Documents ⭢ GetDocumentsByOrder() ⭢ hiển thị E-Invoice/Waybill
//   ExportExcel ⭢ EDeliveryController.Export ⭢ xuất XML/PDF cho cơ quan thuế (mở rộng sau)
//
// FILES LIÊN QUAN:
//   CALLED BY:  EDeliveryController.cs (Generate, Documents, Export)
//   CALLED BY:  ShipperController.cs (hoantatdon — gọi GenerateEWaybill)
//   CALLS:      DbContext.tbEInvoices (CRUD), DbContext.tbDonHangs (order info)
//   LIÊN QUAN:  tbEInvoice.cs (model hóa đơn điện tử)
//   LIÊN QUAN:  tbDonHang.cs, tbQuanAn.cs, tbShipper.cs (thông tin đơn hàng)
// ============================================================
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Services;

public class EDeliveryService
{
    private readonly dbFoodyEntities _db;
    private readonly ILogger<EDeliveryService> _logger;

    public EDeliveryService(dbFoodyEntities db, ILogger<EDeliveryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Sinh hóa đơn điện tử khi đơn hàng được thanh toán thành công
    /// </summary>
    public async Task<tbEInvoice?> GenerateEInvoice(int orderId)
    {
        try
        {
            var donHang = await _db.tbDonHangs
                .Include(d => d.tbQuanAn)
                .Include(d => d.tbThongTinDatHang)
                .Include(d => d.tbLoaiHinhThanhToan)
                .FirstOrDefaultAsync(d => d.madh == orderId);

            if (donHang == null)
            {
                _logger.LogWarning("[EDelivery] Order #{OrderId} not found", orderId);
                return null;
            }

            // ponytail: check invoice đã tồn tại chưa (idempotent)
            var existing = await _db.tbEInvoices
                .FirstOrDefaultAsync(e => e.madh == orderId && e.loaichungtu == "EInvoice");
            if (existing != null)
            {
                _logger.LogInformation("[EDelivery] E-Invoice for #{OrderId} already exists, skip", orderId);
                return existing;
            }

            var now = DateTime.Now;
            var invoiceCount = await _db.tbEInvoices.CountAsync();
            var invoiceNumber = $"INV-FS-{now:yyyyMMdd}-{(invoiceCount + 1):D4}";

            // Tạo QR data: mã hóa thông tin hóa đơn
            var qrData = $"FASTSHIP|INVOICE|{donHang.madh}|{donHang.tongtien:N0}|{now:yyyyMMddHHmmss}";

            var invoice = new tbEInvoice
            {
                invoice_number = invoiceNumber,
                madh = orderId,
                ngayxuat = now,
                tongtien = donHang.tongtien ?? 0,
                is_digital_signed = true,
                qr_code_data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(qrData)),
                loaichungtu = "EInvoice"
            };

            _db.tbEInvoices.Add(invoice);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[EDelivery] E-Invoice #{InvNum} generated for order #{OrderId}", invoiceNumber, orderId);
            return invoice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EDelivery] Failed to generate E-Invoice for order #{OrderId}", orderId);
            return null;
        }
    }

    /// <summary>
    /// Sinh vận đơn điện tử khi shipper giao hàng thành công
    /// </summary>
    public async Task<tbEInvoice?> GenerateEWaybill(int orderId)
    {
        try
        {
            var donHang = await _db.tbDonHangs
                .Include(d => d.tbQuanAn)
                .Include(d => d.tbThongTinDatHang)
                .Include(d => d.tbShipper)
                .FirstOrDefaultAsync(d => d.madh == orderId);

            if (donHang == null)
            {
                _logger.LogWarning("[EDelivery] Order #{OrderId} not found for E-Waybill", orderId);
                return null;
            }

            // ponytail: check waybill đã tồn tại chưa (idempotent)
            var existing = await _db.tbEInvoices
                .FirstOrDefaultAsync(e => e.madh == orderId && e.loaichungtu == "EWaybill");
            if (existing != null)
            {
                _logger.LogInformation("[EDelivery] E-Waybill for #{OrderId} already exists, skip", orderId);
                return existing;
            }

            var now = DateTime.Now;
            var waybillCount = await _db.tbEInvoices.CountAsync();
            var waybillNumber = $"WB-FS-{now:yyyyMMdd}-{(waybillCount + 1):D4}";

            // QR: thông tin bàn giao hàng
            var qrData = $"FASTSHIP|WAYBILL|{donHang.madh}|{donHang.tbShipper?.tenshipper ?? "N/A"}|{now:yyyyMMddHHmmss}";

            var waybill = new tbEInvoice
            {
                invoice_number = waybillNumber,
                madh = orderId,
                ngayxuat = now,
                tongtien = donHang.phiship ?? 0,
                is_digital_signed = true,
                qr_code_data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(qrData)),
                loaichungtu = "EWaybill"
            };

            _db.tbEInvoices.Add(waybill);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[EDelivery] E-Waybill #{WbNum} generated for order #{OrderId}", waybillNumber, orderId);
            return waybill;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EDelivery] Failed to generate E-Waybill for order #{OrderId}", orderId);
            return null;
        }
    }

    /// <summary>
    /// Lấy thông tin hóa đơn/vận đơn theo mã đơn hàng
    /// </summary>
    public async Task<List<tbEInvoice>> GetDocumentsByOrder(int orderId)
    {
        return await _db.tbEInvoices
            .Where(e => e.madh == orderId)
            .OrderBy(e => e.loaichungtu)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin hóa đơn/vận đơn theo mã số
    /// </summary>
    public async Task<tbEInvoice?> GetByNumber(string invoiceNumber)
    {
        return await _db.tbEInvoices
            .FirstOrDefaultAsync(e => e.invoice_number == invoiceNumber);
    }
}
