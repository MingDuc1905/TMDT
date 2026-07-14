using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

[EnableRateLimiting("gemini-policy")]
public class ChatbotController : BaseController
{
    private readonly GeminiService _gemini;
    private readonly EDeliveryService _eDelivery;
    private const int MaxHistoryLength = 20; // Giữ tối đa 20 tin nhắn gần nhất cho hội thoại tự nhiên hơn

    public ChatbotController(dbFoodyEntities context, GeminiService gemini, EDeliveryService eDelivery)
    {
        db = context;
        _gemini = gemini;
        _eDelivery = eDelivery;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Json(new { reply = "Vui lòng nhập câu hỏi!", quickReplies = new string[] { } });

        var lowerMsg = message.ToLower().Trim();

        // 1. Xử lý các truy vấn cần database (tra cứu đơn hàng, gợi ý món ăn)
        // Chỉ xử lý khi có từ khóa rõ ràng - không ảnh hưởng đến hội thoại tự do với AI
        var dbResult = HandleDatabaseQueries(lowerMsg);
        if (dbResult != null)
        {
            SaveToHistory(message, (string)dbResult.GetType().GetProperty("reply")!.GetValue(dbResult)!);
            return Json(dbResult);
        }

        // 2. Dùng Gemini AI trả lời TỰ DO - không bị giới hạn bởi kịch bản lập trình sẵn
        if (_gemini.IsConfigured)
        {
            // ── Đồng bộ thời gian thực: query DB trước khi gửi lên Gemini ──
            // Tránh lệch pha giữa trạng thái đơn hàng trong DB vs Progress Bar UI
            // Lấy trạng thái mới nhất của đơn hàng nếu có đề cập trong message
            var orderMatch = Regex.Match(message, @"(?:#|mã\s+|đơn\s+|order\s+|tra\s+)(\d{2,8})");
            string? realtimeOrderStatus = null;
            if (orderMatch.Success)
            {
                int orderId = int.Parse(orderMatch.Groups[1].Value);
                var liveOrder = db.tbDonHang
                    .AsNoTracking()
                    .Where(d => d.madh == orderId)
                    .Select(d => new { d.trangthai, d.tongtien })
                    .FirstOrDefault();
                if (liveOrder != null)
                {
                    realtimeOrderStatus = $"[Trạng thái thực tế từ hệ thống] Đơn #{orderId}: {liveOrder.trangthai}, tổng tiền: {liveOrder.tongtien:N0}đ";
                }
            }

            var history = GetConversationHistory();
            // Inject realtime context + DB stats vào message trước khi gửi lên Gemini
            var augmentedMessage = message;
            var dbContext = GetDBContextSummary();
            augmentedMessage = $"[BỐI CẢNH HỆ THỐNG FASTSHIP]\n{dbContext}\n\n[CÂU HỎI CỦA KHÁCH HÀNG] {message}";
            if (realtimeOrderStatus != null)
            {
                augmentedMessage += $"\n\n(Dữ liệu hệ thống: {realtimeOrderStatus})";
            }
            var geminiReply = await _gemini.SendMessageAsync(augmentedMessage, history);
            if (!string.IsNullOrEmpty(geminiReply))
            {
                SaveToHistory(message, geminiReply);
                return Json(new
                {
                    reply = geminiReply,
                    quickReplies = GetContextualQuickReplies(message)
                });
            }
        }

        // 3. Nếu Gemini không khả dụng (chưa cấu hình API key), thông báo nhẹ nhàng
        var configReply = "⚠️ **FastShip Chatbot cần được cấu hình API key để hoạt động.**\n\nVui lòng liên hệ quản trị viên để thiết lập kết nối Gemini AI.\n\nTrong lúc chờ, bạn có thể dùng các lệnh sau:\n- `#123` - Tra cứu đơn hàng\n- `gợi ý món ăn` - Xem món bán chạy\n- `phí ship` - Xem thông tin phí vận chuyển";
        SaveToHistory(message, configReply);
        return Json(new
        {
            reply = configReply,
            quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Phí ship thế nào?", "Đăng ký bán hàng" }
        });
    }

    [HttpGet]
    public JsonResult GetAiHistory()
    {
        var rawHistory = GetConversationHistory();
        if (rawHistory.Count == 0)
            return Json(new { success = true, history = new List<object>() });

        // Chuy?n t? List<string> (lu?n phiên user/bot) sang List<{role, content}>
        var formatted = new List<object>();
        for (int i = 0; i < rawHistory.Count; i += 2)
        {
            if (i < rawHistory.Count)
                formatted.Add(new { role = "user", content = rawHistory[i] });
            if (i + 1 < rawHistory.Count)
                formatted.Add(new { role = "bot", content = rawHistory[i + 1] });
        }

        return Json(new { success = true, history = formatted });
    }

    /// <summary>
    /// Lưu tin nhắn vào lịch sử hội thoại (session)
    /// </summary>
    private void SaveToHistory(string userMsg, string botReply)
    {
        var history = GetConversationHistory();
        history.Add(userMsg);
        history.Add(botReply);

        // Giới hạn độ dài lịch sử
        if (history.Count > MaxHistoryLength)
        {
            history = history.Skip(history.Count - MaxHistoryLength).ToList();
        }

        HttpContext.Session.SetString("chat_history", JsonSerializer.Serialize(history));
    }

    /// <summary>
    /// Lấy lịch sử hội thoại từ session
    /// </summary>
    private List<string> GetConversationHistory()
    {
        var json = HttpContext.Session.GetString("chat_history");
        if (string.IsNullOrEmpty(json))
            return new List<string>();
        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    /// <summary>
    /// Xử lý các câu hỏi cần truy vấn database
    /// </summary>
    private object? HandleDatabaseQueries(string msg)
    {
        // Tra cứu đơn hàng (#123 hoặc 123)
        var orderResult = HandleOrderLookup(msg);
        if (orderResult != null) return orderResult;

        // Tra cứu hóa đơn / vận đơn điện tử
        var invoiceResult = HandleInvoiceLookup(msg);
        if (invoiceResult != null) return invoiceResult;

        // Thống kê / số liệu
        var statsResult = HandleStatsQueries(msg);
        if (statsResult != null) return statsResult;

        // Món ít người mua nhất
        var leastResult = HandleLeastPopular(msg);
        if (leastResult != null) return leastResult;

        // Phí ship
        var shipResult = HandleShipFee(msg);
        if (shipResult != null) return shipResult;

        // Danh sách quán
        var restaurantResult = HandleRestaurantList(msg);
        if (restaurantResult != null) return restaurantResult;

        // Gợi ý món ăn
        var recommendResult = HandleRecommendation(msg);
        if (recommendResult != null) return recommendResult;

        return null;
    }

    /// <summary>
    /// Thống kê tổng quát từ DB
    /// </summary>
    private object? HandleStatsQueries(string msg)
    {
        bool isStats = ContainsAny(msg, "thống kê", "bao nhiêu", "số lượng", "tổng số", "có bao nhiêu", "hiện có", "bao nhiêu quán", "bao nhiêu món", "bao nhiêu đơn");
        if (!isStats) return null;

        var tongQuan = db.tbQuanAn.Count();
        var tongMon = db.tbMonAn.Count();
        var tongDon = db.tbDonHang.Count();
        var donThanhCong = db.tbDonHang.Count(d => d.trangthai == "Hoàn thành");
        var tongNguoiDung = db.tbUser.Count();

        var replyText = "📊 **THỐNG KÊ FASTSHIP**\n"
            + $"- 🏪 Quán ăn: {tongQuan} quán\n"
            + $"- 🍽️ Món ăn: {tongMon} món\n"
            + $"- 📦 Tổng đơn hàng: {tongDon} đơn\n"
            + $"- ✅ Đơn thành công: {donThanhCong} đơn\n"
            + $"- 👥 Tài khoản: {tongNguoiDung} người dùng";

        return new
        {
            reply = replyText,
            quickReplies = new[] { "Gợi ý món ăn", "Món ít người mua", "Danh sách quán", "Phí ship thế nào?" }
        };
    }

    /// <summary>
    /// Món ít người đặt nhất
    /// </summary>
    private object? HandleLeastPopular(string msg)
    {
        bool isLeast = ContainsAny(msg, "ít người mua", "ít bán", "chưa có ai", "ít lượt", "ít đặt", "chậm bán", "ế");
        if (!isLeast) return null;

        var leastItems = db.tbChiTietDonHang
            .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null)
            .GroupBy(ct => new { ct.mamon, ten = ct.tbBienTheMonAn!.tbMonAn!.tenmon, gia = ct.tbBienTheMonAn!.tbMonAn!.giatien })
            .Select(g => new { ten = g.Key.ten, gia = g.Key.gia, soLuong = g.Sum(ct => ct.soluong ?? 0) })
            .OrderBy(g => g.soLuong)
            .Take(5)
            .ToList();

        if (leastItems.Count == 0)
        {
            return new
            {
                reply = "😅 Chưa có đủ dữ liệu để thống kê món ít người mua.",
                quickReplies = new[] { "Gợi ý món ăn", "Thống kê chung", "Danh sách quán" }
            };
        }

        var replyText = "📉 **TOP MÓN ÍT NGƯỜI MUA**\n";
        int i = 1;
        foreach (var m in leastItems)
        {
            replyText += $"\n{i}. **{m.ten}** - {m.gia?.ToString("N0") ?? "0"}đ (Chỉ bán {m.soLuong} suất)";
            i++;
        }
        replyText += "\n\n💡 Bạn có thể thử để ủng hộ các món này nhé!";

        return new
        {
            reply = replyText,
            quickReplies = new[] { "Gợi ý món ăn", "Thống kê chung", "Phí ship thế nào?" }
        };
    }

    /// <summary>
    /// Thông tin phí ship
    /// </summary>
    private object? HandleShipFee(string msg)
    {
        bool isShip = ContainsAny(msg, "phí ship", "phí vận chuyển", "ship bao nhiêu", "tiền ship", "giao hàng bao nhiêu", "phí giao", "bao phí");
        if (!isShip) return null;

        return new
        {
            reply = "🛵 **PHÍ VẬN CHUYỂN FASTSHIP**\n"
                + "- Phí ship cố định: **15.000đ** / đơn\n"
                + "- Miễn phí ship cho đơn từ **200.000đ**\n"
                + "- Khu vực giao hàng: nội thành (bán kính 8km)\n"
                + "- Thời gian giao: 20-40 phút\n\n"
                + "👉 Đặt đơn từ 200k để được MIỄN PHÍ ship!",
            quickReplies = new[] { "Gợi ý món ăn", "Danh sách quán", "Đặt hàng ngay" }
        };
    }

    /// <summary>
    /// Danh sách quán ăn
    /// </summary>
    private object? HandleRestaurantList(string msg)
    {
        bool isList = ContainsAny(msg, "danh sách quán", "quán nào", "quán ăn", "xem quán", "list quán", "quán gì");
        if (!isList) return null;

        var quans = db.tbQuanAn
            .OrderByDescending(q => q.tbDonHang.Count())
            .Take(6)
            .Select(q => new { q.tenquanan, q.diachi, soDon = q.tbDonHang.Count() })
            .ToList();

        if (quans.Count == 0)
        {
            return new
            {
                reply = "😅 Hiện tại chưa có quán ăn nào. Vui lòng thử lại sau!",
                quickReplies = new[] { "Gợi ý món ăn", "Phí ship thế nào?" }
            };
        }

        var replyText = "🏪 **TOP QUÁN ĂN**\n";
        int i = 1;
        foreach (var q in quans)
        {
            replyText += $"\n{i}. **{q.tenquanan}** - {q.diachi} ({q.soDon} đơn)";
            i++;
        }
        replyText += "\n\n👉 Truy cập trang chủ để xem menu chi tiết!";

        return new
        {
            reply = replyText,
            quickReplies = new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Thống kê chung" }
        };
    }

    private object? HandleOrderLookup(string msg)
    {
        // Chỉ match khi có dấu # hoặc từ khóa "mã", "đơn" đứng trước số - tránh bắt nhầm số trong câu hỏi thường
        var match = Regex.Match(msg, @"(?:#|mã\s+|đơn\s+|order\s+|tra\s+)(\d{2,8})");
        if (!match.Success) return null;

        int orderId = int.Parse(match.Groups[1].Value);

        var donHang = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper)
            .FirstOrDefault(d => d.madh == orderId);

        if (donHang == null)
        {
            return new
            {
                reply = "❌ Không tìm thấy đơn hàng mã #" + orderId + ". Vui lòng kiểm tra lại mã đơn hàng.",
                quickReplies = new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Liên hệ hỗ trợ" }
            };
        }

        var shipperText = donHang.tbShipper != null
            ? "\n- 🚀 Người giao: " + donHang.tbShipper.tenshipper
            : "\n- 🚀 Người giao: Đang tìm shipper";

        return new
        {
            reply = "📦 **ĐƠN HÀNG #" + donHang.madh + "**" +
                    "\n- 🏪 Quán: " + donHang.tbQuanAn?.tenquanan +
                    "\n- 👤 Người nhận: " + donHang.tbThongTinDatHang?.tennguoinhan +
                    "\n- 📍 Địa chỉ: " + donHang.tbThongTinDatHang?.diachi +
                    "\n- 💰 Tổng tiền: " + (donHang.tongtien?.ToString("N0") ?? "0") + "đ" +
                    "\n- 📌 Trạng thái: " + GetStatusEmoji(donHang.trangthai) + " " + donHang.trangthai +
                    shipperText +
                    "\n- 📅 Ngày đặt: " + donHang.ngaydathang?.ToString("dd/MM/yyyy HH:mm"),
            quickReplies = new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Đặt món mới" }
        };
    }

    /// <summary>
    /// Tra cứu hóa đơn / vận đơn điện tử
    /// </summary>
    private object? HandleInvoiceLookup(string msg)
    {
        bool isInvoiceQuery = ContainsAny(msg, "hóa đơn", "hoá đơn", "invoice", "vận đơn", "van don", "chứng từ", "chung tu", "e-invoice", "e-waybill");
        if (!isInvoiceQuery) return null;

        // Tìm mã đơn hàng trong câu hỏi
        var orderMatch = Regex.Match(msg, @"(?:#|mã\s+|đơn\s+|order\s+|đơn hàng\s+)(\d{2,8})");
        int orderId;
        if (orderMatch.Success)
        {
            orderId = int.Parse(orderMatch.Groups[1].Value);
        }
        else
        {
            return new
            {
                reply = "Vui lòng cung cấp mã đơn hàng để tra cứu hóa đơn. Ví dụ: `hóa đơn #123`",
                quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Phí ship thế nào?" }
            };
        }

        try
        {
            // ponytail: GetAwaiter().GetResult() tránh deadlock khi gọi async từ sync context
            var docs = _eDelivery.GetDocumentsByOrder(orderId).GetAwaiter().GetResult();
            if (docs.Count == 0)
            {
                return new
                {
                    reply = $"📄 Đơn hàng #{orderId} chưa có hóa đơn/vận đơn điện tử. Chứng từ sẽ được tạo tự động sau khi thanh toán và giao hàng thành công.",
                    quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Liên hệ hỗ trợ" }
                };
            }

            var replyText = $"📑 **CHỨNG TỪ ĐIỆN TỬ - ĐƠN HÀNG #{orderId}**\n\n";
            foreach (var doc in docs)
            {
                var emoji = doc.loaichungtu == "EInvoice" ? "🧾" : "📦";
                var name = doc.loaichungtu == "EInvoice" ? "Hóa đơn điện tử" : "Vận đơn điện tử";
                replyText += $"{emoji} **{name}**\n";
                replyText += $"  Số: `{doc.invoice_number}`\n";
                replyText += $"  Ngày xuất: {doc.ngayxuat:dd/MM/yyyy HH:mm}\n";
                replyText += $"  Giá trị: {doc.tongtien:N0}đ\n";
                replyText += $"  Đã ký số: {(doc.is_digital_signed ? "✅" : "❌")}\n";
                replyText += $"  🔗 Xem chi tiết: {GetBaseUrl()}/Cart/EInvoice?id={doc.einvoice_id}\n\n";
            }

            return new
            {
                reply = replyText,
                quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Liên hệ hỗ trợ" }
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Chatbot] Invoice lookup error: {ex.Message}");
            return new
            {
                reply = "❌ Không thể tra cứu hóa đơn lúc này. Vui lòng thử lại sau.",
                quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Liên hệ hỗ trợ" }
            };
        }
    }

    private string GetBaseUrl()
    {
        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}";
    }

    private string GetStatusEmoji(string? status)
    {
        return status switch
        {
            "Đã đặt" => "📋",
            "Đã xác nhận" => "✅",
            "Đang chuẩn bị" => "👨‍🍳",
            "Đã lấy" => "🛵",
            "Đang giao" => "🚀",
            "Hoàn thành" => "🎉",
            "Đã hủy" => "❌",
            _ => "📌"
        };
    }

    private object? HandleRecommendation(string msg)
    {
        bool isRecommend = ContainsAny(msg, "gợi ý", "gọi ý", "đề xuất", "nên ăn", "bán chạy", "hot", "ngon", "món gì");
        if (!isRecommend) return null;

        var topMonAn = db.tbChiTietDonHang
            .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null)
            .GroupBy(ct => new { ct.mamon, ten = ct.tbBienTheMonAn!.tbMonAn!.tenmon, gia = ct.tbBienTheMonAn!.tbMonAn!.giatien })
            .Select(g => new
            {
                ten = g.Key.ten,
                gia = g.Key.gia,
                soLuong = g.Sum(ct => ct.soluong ?? 0)
            })
            .OrderByDescending(g => g.soLuong)
            .Take(5)
            .ToList();

        if (topMonAn.Count == 0)
        {
            return new
            {
                reply = "😅 Hiện tại chưa có đủ dữ liệu bán chạy. Bạn hãy khám phá các quán ăn trên trang chủ nhé!",
                quickReplies = new[] { "Xem quán ăn", "Phí ship thế nào?", "Liên hệ hỗ trợ" }
            };
        }

        var replyText = "🔥 **TOP MÓN BÁN CHẠY** 🔥\n";
        int index = 1;
        foreach (var mon in topMonAn)
        {
            replyText += $"\n{index}. **{mon.ten}** - {mon.gia?.ToString("N0") ?? "0"}đ (Đã bán {mon.soLuong} suất)";
            index++;
        }
        replyText += "\n\n👉 Đặt ngay để thưởng thức!";

        return new
        {
            reply = replyText,
            quickReplies = new[] { "Xem quán ăn", "Phí ship thế nào?", "Tra cứu đơn hàng" }
        };
    }

    /// <summary>
    /// Gợi ý các quick reply dựa trên nội dung tin nhắn hiện tại
    /// (chỉ gợi ý định hướng chung, không ảnh hưởng đến nội dung AI trả lời)
    /// </summary>
    private string[] GetContextualQuickReplies(string msg)
    {
        var lower = msg.ToLower();
        if (lower.Contains("phí") || lower.Contains("ship") || lower.Contains("tiền"))
            return new[] { "Gợi ý món ăn", "Đăng ký bán hàng", "Tra cứu đơn hàng", "Liên hệ hỗ trợ" };
        if (lower.Contains("món") || lower.Contains("ăn") || lower.Contains("ngon") || lower.Contains("bán chạy") || lower.Contains("gợi ý"))
            return new[] { "Phí ship thế nào?", "Đăng ký bán hàng", "Tra cứu đơn hàng #", "Liên hệ hỗ trợ" };
        if (lower.Contains("đăng ký") || lower.Contains("bán hàng") || lower.Contains("mở quán"))
            return new[] { "Phí ship thế nào?", "Gợi ý món ăn", "Tra cứu đơn hàng #" };
        if (lower.Contains("chào") || lower.Contains("cảm ơn") || lower.Contains("hello"))
            return new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Tra cứu đơn hàng #", "Liên hệ hỗ trợ" };
        return new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Đăng ký bán hàng", "Liên hệ hỗ trợ" };
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var k in keywords)
            if (text.Contains(k)) return true;
        return false;
    }

    /// <summary>
    /// Tóm tắt DB context để inject vào Gemini prompt — giúp AI trả lời câu hỏi cụ thể
    /// </summary>
    private string GetDBContextSummary()
    {
        try
        {
            var tongQuan = db.tbQuanAn.Count();
            var tongMon = db.tbMonAn.Count();
            var tongDon = db.tbDonHang.Count();
            var donThanhCong = db.tbDonHang.Count(d => d.trangthai == "Hoàn thành");
            var topMon = db.tbChiTietDonHang
                .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null)
                .GroupBy(ct => ct.tbBienTheMonAn!.tbMonAn!.tenmon)
                .Select(g => new { ten = g.Key, soLuong = g.Sum(ct => ct.soluong ?? 0) })
                .OrderByDescending(g => g.soLuong)
                .Take(5).Select(g => $"{g.ten}({g.soLuong})").ToList();
            var listQuan = db.tbQuanAn.Select(q => q.tenquanan).Take(10).ToList();

            return $"Số quán ăn: {tongQuan}. Số món: {tongMon}. Tổng đơn: {tongDon}. Đơn thành công: {donThanhCong}. "
                + $"Top bán chạy: {string.Join(", ", topMon)}. "
                + $"Quán: {string.Join(", ", listQuan)}. "
                + "Phí ship cố định 15k, miễn phí ship từ 200k.";
        }
        catch { return "Dữ liệu hệ thống FastShip: quán ăn, món ăn, đơn hàng."; }
    }
}
