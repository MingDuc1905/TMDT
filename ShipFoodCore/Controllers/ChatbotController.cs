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
    private const int MaxHistoryLength = 20; // Giữ tối đa 20 tin nhắn gần nhất cho hội thoại tự nhiên hơn

    public ChatbotController(dbFoodyEntities context, GeminiService gemini)
    {
        db = context;
        _gemini = gemini;
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
            var history = GetConversationHistory();
            var geminiReply = await _gemini.SendMessageAsync(message, history);
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

        // Gợi ý món ăn
        var recommendResult = HandleRecommendation(msg);
        if (recommendResult != null) return recommendResult;

        return null;
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
            .Where(ct => ct.tbMonAn != null)
            .GroupBy(ct => new { ct.mamon, ten = ct.tbMonAn!.tenmon, gia = ct.tbMonAn!.giatien })
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
}
