using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShipFood.Services;

public class GeminiService
{
    private readonly string? _apiKey;
    private readonly string _modelName;

    private const string SystemPrompt = """
Bạn là trợ lý FastShip - một nền tảng giao đồ ăn tại Việt Nam.

THÔNG TIN VỀ FASTSHIP:
- FastShip là nền tảng giao đồ ăn online, kết nối khách hàng với các quán ăn.
- Phí vận chuyển: 15,000 VND cố định, miễn phí ship cho đơn từ 100,000 VND.
- Thời gian giao: 30-45 phút.
- Hỗ trợ thanh toán: Tiền mặt (COD), Chuyển khoản, PayPal.
- Giờ hoạt động: 7:00 - 21:30.
- Liên hệ: Fastship@contact.com, 48 Cao Thắng, Hải Châu, Đà Nẵng.

QUY TẮC:
1. Trả lời bằng tiếng Việt, thân thiện, tự nhiên.
2. Nếu người dùng hỏi về đơn hàng cụ thể, bảo họ gửi mã đơn hàng (ví dụ: #123).
3. Nếu người dùng muốn gợi ý món ăn, hãy đề xuất họ gõ "gợi ý món ăn".
4. Nếu người dùng muốn đặt hàng, hướng dẫn họ vào trang chủ để chọn quán và thêm món vào giỏ.
5. Giữ câu trả lời ngắn gọn, dễ hiểu, tối đa 3-4 câu.
""";

    public GeminiService(string? apiKey, string? modelName = null)
    {
        _apiKey = string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY" ? null : apiKey;
        _modelName = modelName ?? "gemini-2.0-flash";
    }

    /// <summary>
    /// Gửi tin nhắn đến Gemini AI và nhận phản hồi.
    /// Trả về null nếu có lỗi (để fallback về rule-based).
    /// Lưu ý: history là list các tin nhắn user + bot xen kẽ để duy trì hội thoại.
    /// </summary>
    public async Task<string?> SendMessageAsync(string message, List<string>? history = null)
    {
        if (!IsConfigured) return null;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", _apiKey);

            // Build contents array for Gemini API
            var contents = new List<object>();

            // System prompt as first turn
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = $"{SystemPrompt}\n\nHãy ghi nhớ những quy tắc trên và trả lời bằng tiếng Việt." } }
            });
            contents.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Tôi đã ghi nhớ. Tôi là trợ lý FastShip sẵn sàng hỗ trợ khách hàng!" } }
            });

            // Add conversation history (user + bot messages alternating)
            if (history != null)
            {
                for (int i = 0; i < history.Count; i++)
                {
                    contents.Add(new
                    {
                        role = i % 2 == 0 ? "user" : "model",
                        parts = new[] { new { text = history[i] } }
                    });
                }
            }

            // Current message
            contents.Add(new { role = "user", parts = new[] { new { text = message } } });

            var requestBody = new
            {
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 500,
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent",
                httpContent);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var text = responseJson?.candidates?[0]?.content?.parts?[0]?.text;

            return string.IsNullOrEmpty(text)
                ? "Xin lỗi, tôi chưa thể trả lời câu hỏi này ngay. Bạn có thể thử hỏi lại nhé!"
                : text;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Gemini API error: {ex.Message}");
            return null; // Fallback to rule-based
        }
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    // JSON response models
    private class GeminiResponse
    {
        public Candidate[]? candidates { get; set; }
    }

    private class Candidate
    {
        public Content? content { get; set; }
    }

    private class Content
    {
        public Part[]? parts { get; set; }
    }

    private class Part
    {
        public string? text { get; set; }
    }
}
