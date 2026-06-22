using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class ChatbotController : BaseController
{
    private readonly GeminiService _gemini;
    private const int MaxHistoryLength = 10; // Giữ tối đa 10 tin nhắn gần nhất

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

        // 1. Kiểm tra các câu hỏi đặc biệt cần database
        var dbResult = HandleDatabaseQueries(lowerMsg);
        if (dbResult != null)
        {
            SaveToHistory(message, (string)dbResult.GetType().GetProperty("reply")!.GetValue(dbResult)!);
            return Json(dbResult);
        }

        // 2. Sử dụng Gemini AI để trả lời
        if (_gemini.IsConfigured)
        {
            // Lấy lịch sử hội thoại từ session (chỉ các tin nhắn cũ)
            var history = GetConversationHistory();
            // GeminiService tự thêm message hiện tại vào contents, không cần Add ở đây

            var geminiReply = await _gemini.SendMessageAsync(message, history);
            if (!string.IsNullOrEmpty(geminiReply))
            {
                SaveToHistory(message, geminiReply);
                return Json(new
                {
                    reply = geminiReply,
                    quickReplies = GetDefaultQuickReplies(lowerMsg)
                });
            }
        }

        // 3. Fallback: xử lý rule-based
        var fallbackResult = HandleFallback(lowerMsg);
        var replyText = (string)fallbackResult.GetType().GetProperty("reply")!.GetValue(fallbackResult)!;
        SaveToHistory(message, replyText);
        return Json(fallbackResult);
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
        var match = Regex.Match(msg, @"#?(\d{2,8})");
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
    /// Fallback rule-based khi Gemini không hoạt động
    /// </summary>
    private object HandleFallback(string msg)
    {
        if (ContainsAny(msg, "phí ship", "phí vận chuyển", "tiền ship", "free ship", "miễn phí"))
        {
            return new
            {
                reply = "🚚 **Phí vận chuyển FastShip:**" +
                        "\n- Phí ship cố định: **15,000đ**" +
                        "\n- Miễn phí ship cho đơn từ **100,000đ**" +
                        "\n- Thời gian giao: **30-45 phút**",
                quickReplies = new[] { "Đăng ký bán hàng", "Hủy đơn thế nào?", "Gợi ý món ăn" }
            };
        }

        if (ContainsAny(msg, "đăng ký", "bán hàng", "mở quán", "đăng ký bán"))
        {
            return new
            {
                reply = "🏪 **Đăng ký bán hàng trên FastShip:**" +
                        "\n1. Đăng ký tài khoản với loại **'Quán ăn'**" +
                        "\n2. Chờ Admin duyệt (thường trong 24h)" +
                        "\n3. Sau khi được duyệt, đăng nhập và quản lý thực đơn" +
                        "\n\n📧 Liên hệ: Fastship@contact.com",
                quickReplies = new[] { "Phí ship", "Chính sách hủy đơn", "Gợi ý món ăn" }
            };
        }

        if (ContainsAny(msg, "hủy đơn", "huỷ đơn", "cancel", "hủy", "huỷ"))
        {
            return new
            {
                reply = "📋 **Chính sách hủy đơn:**" +
                        "\n- Có thể hủy trong **5 phút** sau khi đặt" +
                        "\n- Nếu quán đã xác nhận, liên hệ quán để hủy" +
                        "\n- Đơn đã giao **không thể hủy**" +
                        "\n\n👉 Vào **Lịch sử đơn hàng** để hủy đơn",
                quickReplies = new[] { "Phí ship", "Đăng ký bán hàng", "Gợi ý món ăn" }
            };
        }

        if (ContainsAny(msg, "giờ", "thời gian", "mở cửa", "hoạt động"))
        {
            return new
            {
                reply = "⏰ **Giờ hoạt động:**" +
                        "\n- Các quán mở cửa: **7:00 - 21:30**" +
                        "\n- Giờ có thể khác nhau tùy quán" +
                        "\n- Xem chi tiết trên trang của từng quán",
                quickReplies = new[] { "Phí ship", "Đăng ký bán hàng", "Gợi ý món ăn" }
            };
        }

        // Mặc định
        return new
        {
            reply = "👋 **Chào bạn!** Tôi là trợ lý FastShip." +
                    "\n\nTôi có thể giúp bạn:" +
                    "\n- 📦 **Tra cứu đơn hàng**: Gửi mã đơn (ví dụ: #123)" +
                    "\n- 🔥 **Gợi ý món**: Gõ 'gợi ý món ăn'" +
                    "\n- 📞 **Hỗ trợ**: Các câu hỏi về phí ship, hủy đơn, đăng ký..." +
                    "\n- 🤖 **AI Chat**: Hỏi bất cứ điều gì!",
            quickReplies = new[] { "Phí ship thế nào?", "Gợi ý món ăn", "Đăng ký bán hàng" }
        };
    }

    private string[] GetDefaultQuickReplies(string msg)
    {
        if (ContainsAny(msg, "phí", "ship", "tiền"))
            return new[] { "Gợi ý món ăn", "Đăng ký bán hàng", "Tra cứu đơn hàng" };
        if (ContainsAny(msg, "món", "ăn", "bán chạy"))
            return new[] { "Phí ship thế nào?", "Đăng ký bán hàng", "Tra cứu đơn hàng" };
        if (ContainsAny(msg, "đăng ký", "bán"))
            return new[] { "Phí ship thế nào?", "Gợi ý món ăn", "Tra cứu đơn hàng" };
        return new[] { "Phí ship thế nào?", "Gợi ý món ăn", "Đăng ký bán hàng" };
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var k in keywords)
            if (text.Contains(k)) return true;
        return false;
    }
}
