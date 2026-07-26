// ============================================================
// 📦 EDeliveryController — Vận đơn điện tử QR Code | E-Delivery QR
// ============================================================
// Ý nghĩa: Quản lý vận đơn điện tử dùng QR Code cho cả 4 role
// Chức năng: Tạo QR, quét QR xác nhận lấy hàng, bypass admin, delivery logs, merchant scanner
// KEYWORDS: edelivery, qr, scan, delivery, vận đơn, qr code, bypass, delivery logs
// ============================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;
using System.Security.Cryptography;
using System.Text;

namespace ShipFood.Controllers;

public class EDeliveryController : BaseController
{
    private readonly IHubContext<Chats> _hubContext;
    private readonly EDeliveryService _eDelivery;
    // ponytail: QR_HASH_SECRET tu env var, fallback secure default
    private static string QR_HASH_SECRET => Environment.GetEnvironmentVariable("QR_HASH_SECRET") ?? "FastShip2024!EDelivery@Secure";

    public EDeliveryController(
        dbFoodyEntities context,
        IHubContext<Chats> hubContext,
        EDeliveryService eDelivery)
    {
        db = context;
        _hubContext = hubContext;
        _eDelivery = eDelivery;
    }

    // ════════════════════════════════════════════════════════════════
    // 1. GENERATE QR CODE (PNG) — GET /edelivery/qr/{orderId}
    // ════════════════════════════════════════════════════════════════
    public IActionResult GenerateQR(int orderId)
    {
        try
        {
            var token = GenerateSecureToken(orderId);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var qrContent = $"{baseUrl}/edelivery/scan/{token}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var darkRgb = new byte[] { 0x1a, 0x1a, 0x2e };
            var lightRgb = new byte[] { 0xff, 0xff, 0xff };
            var qrBytes = qrCode.GetGraphic(20, darkRgb, lightRgb);

            return File(qrBytes, "image/png");
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EDeliveryController>>();
            logger.LogError(ex, "[EDelivery] QR generation failed for order #{OrderId}", orderId);
            return NotFound();
        }
    }

    // ════════════════════════════════════════════════════════════════
    // 2. SCAN LANDING PAGE — GET /edelivery/scan/{token}
    // ════════════════════════════════════════════════════════════════
    public async Task<IActionResult> ScanQR(string token)
    {
        var orderId = ValidateToken(token);
        if (orderId == null)
        {
            ViewBag.Error = "Mã QR không hợp lệ hoặc đã hết hạn.";
            return View("ScanResult");
        }

        var donHang = await db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .FirstOrDefaultAsync(d => d.madh == orderId.Value);

        if (donHang == null)
        {
            ViewBag.Error = "Đơn hàng không tồn tại.";
            return View("ScanResult");
        }

        ViewBag.Order = donHang;
        ViewBag.Token = token;
        return View("ScanResult");
    }

    // ════════════════════════════════════════════════════════════════
    // 3. CONFIRM SCAN — POST /edelivery/confirm-scan
    // ════════════════════════════════════════════════════════════════
    [HttpPost]
    public async Task<JsonResult> ConfirmScan([FromBody] ScanRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.Token))
                return Json(new { success = false, message = "Thiếu mã xác thực." });

            var orderIdNullable = ValidateToken(request.Token);
            if (orderIdNullable == null)
                return Json(new { success = false, message = "Mã QR không hợp lệ." });

            var orderId = orderIdNullable.Value;

            var donHang = await db.tbDonHang
                .Include(d => d.tbQuanAn)
                .Include(d => d.tbThongTinDatHang)
                .Include(d => d.tbShipper)
                .FirstOrDefaultAsync(d => d.madh == orderId);

            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại." });

            if (donHang.trangthai != "Đã xác nhận" && donHang.trangthai != "Chờ shipper lấy hàng")
                return Json(new { success = false, message = $"Đơn hàng đang ở trạng thái '{donHang.trangthai}', không thể quét." });

            donHang.trangthai = "Đã lấy";
            donHang.ngaygiaohang = DateTime.Now;
            await db.SaveChangesAsync();

            try { await _eDelivery.GenerateEInvoice(orderId); } catch { }

            var now = DateTime.Now.ToString("HH:mm");
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("orderDeliveryScanned", orderId, donHang.trangthai, now);
            await _hubContext.Clients.Group("shippers").SendAsync("orderDeliveryScanned", orderId, donHang.trangthai, now);
            await _hubContext.Clients.Group("admins").SendAsync("deliveryScanEvent", new
            {
                orderId,
                trangthai = donHang.trangthai,
                restaurantName = donHang.tbQuanAn?.tenquanan ?? "N/A",
                customerName = donHang.tbThongTinDatHang?.tennguoinhan ?? "N/A",
                time = now,
                shipperName = donHang.tbShipper?.tenshipper ?? "N/A"
            });

            return Json(new
            {
                success = true,
                message = $"✅ Xác nhận đơn hàng #{orderId} đã được lấy thành công!",
                orderId,
                trangthai = donHang.trangthai
            });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EDeliveryController>>();
            logger.LogError(ex, "[EDelivery] ConfirmScan failed");
            return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    // ════════════════════════════════════════════════════════════════
    // 4. BYPASS — Admin ép trạng thái — POST /edelivery/bypass
    // ════════════════════════════════════════════════════════════════
    [HttpPost]
    public async Task<JsonResult> Bypass([FromBody] BypassRequest request)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null || user.loaitaikhoan != "Admin")
                return Json(new { success = false, message = "Chỉ Admin mới có quyền bypass." });

            if (request?.OrderId == null)
                return Json(new { success = false, message = "Thiếu mã đơn hàng." });

            var orderId = request.OrderId.Value;
            var donHang = await db.tbDonHang
                .Include(d => d.tbQuanAn)
                .Include(d => d.tbThongTinDatHang)
                .Include(d => d.tbShipper)
                .FirstOrDefaultAsync(d => d.madh == orderId);

            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại." });
            if (donHang.trangthai == "Hoàn thành" || donHang.trangthai == "Đã hủy")
                return Json(new { success = false, message = $"Đơn hàng đã ở trạng thái '{donHang.trangthai}', không thể bypass." });

            var statusTarget = request.TargetStatus ?? "Đã lấy";
            donHang.trangthai = statusTarget;
            if (statusTarget == "Hoàn thành") donHang.ngaythanhtoan = DateTime.Now;
            await db.SaveChangesAsync();

            var now = DateTime.Now.ToString("HH:mm");
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("deliveryBypassed", orderId, statusTarget, now);
            await _hubContext.Clients.Group("admins").SendAsync("deliveryBypassed", orderId, statusTarget, now);

            return Json(new
            {
                success = true,
                message = $"✅ Đã xử lý thủ công đơn hàng #{orderId} → '{statusTarget}'",
                orderId,
                trangthai = statusTarget
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    // ════════════════════════════════════════════════════════════════
    // 5. DELIVERY LOGS — Admin — GET /edelivery/delivery-logs
    // ════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<ActionResult> DeliveryLogs()
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Admin")
            return RedirectToAction("Login", "Home");

        ViewBag.AdminUserId = user.userid;

        var donHangs = await db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper)
            .OrderByDescending(d => d.ngaydathang)
            .Take(50)
            .ToListAsync();

        return View("~/Views/Admin/DeliveryLogs.cshtml", donHangs);
    }

    // ════════════════════════════════════════════════════════════════
    // 6. MERCHANT SCANNER — GET /edelivery/merchant-scan
    // ════════════════════════════════════════════════════════════════
    [HttpGet]
    public ActionResult MerchantScan()
    {
        var user = GetCurrentUser();
        if (user == null || (user.loaitaikhoan != "Quán ăn" && user.loaitaikhoan != "Admin"))
            return RedirectToAction("Login", "Home");
        return View("~/Views/Restaurant/Scanner.cshtml");
    }

    // ════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════

    private string GenerateSecureToken(int orderId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var raw = $"{orderId}|{timestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(QR_HASH_SECRET));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLower()[..8];

        var tokenRaw = $"{orderId}.{timestamp}.{hash}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenRaw))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private int? ValidateToken(string token)
    {
        try
        {
            var padded = token.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var parts = decoded.Split('.');
            if (parts.Length != 3) return null;

            if (!int.TryParse(parts[0], out int orderId)) return null;
            if (!long.TryParse(parts[1], out long timestamp)) return null;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - timestamp > 86400) return null;

            var raw = $"{orderId}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(QR_HASH_SECRET));
            var expectedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLower()[..8];
            if (parts[2] != expectedHash) return null;

            return orderId;
        }
        catch { return null; }
    }
}

public class ScanRequest
{
    public string? Token { get; set; }
}

public class BypassRequest
{
    public int? OrderId { get; set; }
    public string? TargetStatus { get; set; }
}
