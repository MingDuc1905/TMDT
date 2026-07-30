// ============================================================
// 🤖 ChatbotController — AI Chatbot thông minh | AI Assistant
// ============================================================
// Ý nghĩa: Xử lý hội thoại AI với khách hàng, hỗ trợ tra cứu đơn hàng
// Chức năng: Gửi tin nhắn đến OpenAI, tra cứu đơn hàng (#madh),
//            gợi ý món ăn từ DB, phân tích ngữ cảnh hệ thống,
//            lịch sử hội thoại (session), quick replies
// KEYWORDS: chatbot, AI, OpenAI, tro ly ao, tra cuu, order lookup,
//           goi y mon an, suggestion, hoi thoai
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Trigger: User gửi tin nhắn từ _ChatWidget.cshtml (floating chat)
//   Calls →: BaseController (GetCurrentUser)
//            OpenAIService (SendMessageAsync)
//            EDeliveryService (GetDocumentsByOrder cho tra invoice)
//            Models: tbDonHang, tbMonAn, tbChiTietDonHang
//   Called by ←: Views/Shared/_ChatWidget.cshtml (AJAX POST /Chatbot/SendMessage)
//   Flow: User nhập → POST SendMessage → 1. Rate limit check (5 req/min)
//        → 2. HandleDatabaseQueries: #123 → order lookup, "gợi ý" → top 5
//        → 3. Gọi OpenAI với lịch sử hội thoại + DB context cache
//        → 4. Fallback nếu AI lỗi: hướng dẫn dùng lệnh
//   DB Context: cached 5 phút — tránh query N+1 mỗi request
//   Quick Replies: động dựa trên từ khóa trong tin nhắn
// ============================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

[EnableRateLimiting("openai-policy")]
public class ChatbotController : BaseController
{
    private readonly OpenAIService _openAI;
    private readonly EDeliveryService _eDelivery;
    private readonly ILogger<ChatbotController> _logger;
    private const int MaxHistoryLength = 20; // Giữ tối đa 20 tin nhắn gần nhất cho hội thoại tự nhiên hơn

    public ChatbotController(dbFoodyEntities context, OpenAIService openAI, EDeliveryService eDelivery, ILogger<ChatbotController> logger)
    {
        db = context;
        _openAI = openAI;
        _eDelivery = eDelivery;
        _logger = logger;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<JsonResult> SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Json(new { reply = "Vui lòng nhập câu hỏi!", quickReplies = new string[] { } });

        // ponytail: security fix — giới hạn độ dài message
        if (message.Length > 2000)
            return Json(new { reply = "Tin nhắn quá dài. Vui lòng nhập ngắn hơn (tối đa 2000 ký tự).", quickReplies = new string[] { } });

        // ponytail: security fix — yêu cầu ?ang nh?p d? chat v?i AI
        var currentUser = GetCurrentUser();
        if (currentUser == null && message.Contains("#"))
        {
            // N?u khách vãng lai h?i v? don hàng/invoice/stats, yêu c?u ?ang nh?p
            return Json(new { reply = "🔒 Vui lòng đăng nhập để sử dụng tính năng tra cứu.", quickReplies = new[] { "Đăng nhập", "Gợi ý món ăn", "Phí ship thế nào?" } });
        }

        var lowerMsg = message.ToLower().Trim();

        // 1. X? lý các truy v?n c?n database (tra c?u don hàng, hóa don)
        // ponytail: ch? b?t pattern rõ ràng #123, không intercept t? khóa h?i tho?i t? nhiên
        var dbResult = await HandleDatabaseQueries(lowerMsg);
        if (dbResult != null)
        {
            SaveToHistory(message, (string)dbResult.GetType().GetProperty("reply")!.GetValue(dbResult)!);
            return Json(dbResult);
        }

        // 2. Dùng OpenAI-compatible AI trả lời TỰ DO
        if (_openAI.IsConfigured)
        {
            // ── Đồng bộ thời gian thực: query DB trước khi gửi lên AI ──
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
            var dbContext = GetDBContextSummary();
            // ponytail: format s?ch, d? ?c — không dùng prefix ?n ào
            var augmentedMessage = $"Thông tin hệ thống FastShip:\n{dbContext}\n\nNgười dùng hỏi: {message}";
            if (realtimeOrderStatus != null)
            {
                augmentedMessage += $"\n\n(Trạng thái thực tế: {realtimeOrderStatus})";
            }
            var aiReply = await _openAI.SendMessageAsync(augmentedMessage, history);
            if (!string.IsNullOrEmpty(aiReply))
            {
                SaveToHistory(message, aiReply);
                return Json(new
                {
                    reply = aiReply,
                    quickReplies = GetContextualQuickReplies(message)
                });
            }
            else
            {
                var errorReply = "❌ Xảy ra lỗi nội bộ khi kết nối đến AI. Vui lòng kiểm tra lại API key hoặc kết nối mạng.";
                SaveToHistory(message, errorReply);
                return Json(new
                {
                    reply = errorReply,
                    quickReplies = new[] { "Thử lại sau" }
                });
            }
        }

        // 3. Nếu AI không khả dụng (chưa cấu hình API key), thông báo nhẹ nhàng
        var configReply = "⚠️ **FastShip Chatbot cần được cấu hình API key để hoạt động.**\n\nVui lòng liên hệ quản trị viên để thiết lập kết nối OpenAI.\n\nTrong lúc chờ, bạn có thể dùng các lệnh sau:\n- `#123` - Tra cứu đơn hàng\n- `gợi ý món ăn` - Xem món bán chạy\n- `phí ship` - Xem thông tin phí vận chuyển";
        SaveToHistory(message, configReply);
        return Json(new
        {
            reply = configReply,
            quickReplies = new[] { "Tra cứu đơn hàng #", "Gợi ý món ăn", "Phí ship thế nào?", "Đăng ký bán hàng" }
        });
    }

    /// <summary>
    /// Ki?m tra tr?ng thái AI — không g?i API, không t?n credits, không pollute history
    /// </summary>
    [HttpGet]
    [IgnoreAntiforgeryToken]
    public JsonResult Status()
    {
        return Json(new
        {
            success = true,
            configured = _openAI.IsConfigured,
            model = _openAI.IsConfigured ? "connected" : "unconfigured"
        });
    }

    [HttpGet]
    [IgnoreAntiforgeryToken]
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
    /// X? lý các truy v?n database — ch? pattern rõ ràng, không intercept h?i tho?i t? nhiên
    /// ponytail: các câu h?i t? nhiên (g?i ý, phí ship, th?ng kê) do AI x? lý v?i DB context có s?n
    /// </summary>
    private async Task<object?> HandleDatabaseQueries(string msg)
    {
        // Tra cứu đơn hàng (#123)
        var orderResult = HandleOrderLookup(msg);
        if (orderResult != null) return orderResult;

        // Tra cứu hóa đơn / vận đơn điện tử
        var invoiceResult = await HandleInvoiceLookup(msg);
        if (invoiceResult != null) return invoiceResult;

        return null;
    }

    // ponytail: các hàm HandleStats, HandleLeast, HandleShip, HandleRestaurant, HandleRecommendation
    // dã du?c lo?i b? — AI x? lý các câu h?i t? nhiên này t?t hon v?i DB context có s?n

    private object? HandleOrderLookup(string msg)
    {
        // Chỉ match khi có dấu # hoặc từ khóa "mã", "đơn" đứng trước số - tránh bắt nhầm số trong câu hỏi thường
        var match = Regex.Match(msg, @"(?:#|mã\s+|đơn\s+|order\s+|tra\s+)(\d{2,8})");
        if (!match.Success) return null;

        int orderId = int.Parse(match.Groups[1].Value);

        // ponytail: security fix — b?t bu?c ?ang nh?p d? tra c?u don hàng
        var user = GetCurrentUser();
        if (user == null)
        {
            return new
            {
                reply = "🔒 Vui lòng đăng nhập để tra cứu đơn hàng.",
                quickReplies = new[] { "Gợi ý món ăn", "Phí ship thế nào?", "Liên hệ hỗ trợ" }
            };
        }
        var donHang = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper)
            .FirstOrDefault(d => d.madh == orderId && d.tbThongTinDatHang != null && d.tbThongTinDatHang.userid == user.userid);

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
    private async Task<object?> HandleInvoiceLookup(string msg)
    {
        bool isInvoiceQuery = ContainsAny(msg, "hóa đơn", "hoá đơn", "invoice", "vận đơn", "van don", "chứng từ", "chung tu", "e-invoice", "e-waybill");
        if (!isInvoiceQuery) return null;

        // ponytail: security fix — ch? cho phép user ?ã ?ang nh?p tra c?u hóa don
        var user = GetCurrentUser();
        if (user == null)
        {
            return new
            {
                reply = "🔒 Vui lòng đăng nhập để tra cứu hóa đơn/vận đơn điện tử.",
                quickReplies = new[] { "Đăng nhập", "Gợi ý món ăn", "Phí ship thế nào?" }
            };
        }

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
            // ponytail: Fix #4 — dùng await thay vì GetAwaiter().GetResult() tránh deadlock
            var docs = await _eDelivery.GetDocumentsByOrder(orderId);
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
            _logger.LogError(ex, "Invoice lookup failed for order #{OrderId}", orderId);
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

    // ponytail: HandleRecommendation dã du?c lo?i b? — AI x? lý g?i ý t?t hon

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
    /// Cache cho DB context summary — tránh query N+1 m?i l?n g?i tin nh?n
    /// Du?c refresh m?i 5 phút
    /// </summary>
    private static string? _cachedDbContext;
    private static DateTime _lastDbContextRefresh = DateTime.MinValue;
    private static readonly object _dbCacheLock = new();

    /// <summary>
    /// Tóm t?t DB context d? inject vào AI prompt — cache 5 phút
    /// ponytail: format d? ?c, d? hi?u cho AI
    /// </summary>
    private string GetDBContextSummary()
    {
        // ponytail: cache 5 phút tránh query N+1 m?i request
        if (_cachedDbContext != null && (DateTime.Now - _lastDbContextRefresh).TotalMinutes < 5)
            return _cachedDbContext;

        lock (_dbCacheLock)
        {
            if (_cachedDbContext != null && (DateTime.Now - _lastDbContextRefresh).TotalMinutes < 5)
                return _cachedDbContext;

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

                _cachedDbContext = $"- Quán ăn: {tongQuan} quán | Món ăn: {tongMon} món | Đơn hàng: {tongDon} | Hoàn thành: {donThanhCong}\n"
                    + $"- Top 5 bán chạy: {string.Join(", ", topMon)}.\n"
                    + $"- Quán: {string.Join(", ", listQuan)}.\n"
                    + $"- Phí ship: cố định 15.000đ, miễn phí từ 200.000đ. Thời gian giao: 20-40 phút.\n"
                    + $"- Thanh toán: COD, VNPAY QR, chuyển khoản.";
                _lastDbContextRefresh = DateTime.Now;
                return _cachedDbContext;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetDBContextSummary failed — fallback used");
                return "Dữ liệu FastShip: quán ăn, món ăn, đơn hàng.";
            }
        }
    }
}
