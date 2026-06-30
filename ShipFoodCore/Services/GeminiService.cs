using System.Text;
using System.Text.Json;

namespace ShipFood.Services;

public class GeminiService
{
    private readonly string? _apiKey;
    private readonly string _modelName;

    private const string SystemPrompt = """
Bạn là trợ lý FastShip - nền tảng giao đồ ăn tại Việt Nam.

THÔNG TIN:
- Phí ship: 15,000đ cố định, free ship nếu đơn từ 100,000đ
- Giao hàng: 30-45 phút
- Thanh toán: Tiền mặt, Chuyển khoản, PayPal
- Giờ: 7:00 - 21:30
- Liên hệ: Fastship@contact.com, 48 Cao Thắng, Hải Châu, Đà Nẵng

QUY TẮC:
1. Trả lời tiếng Việt, ngắn gọn (2-3 câu), đi thẳng vào vấn đề. KHÔNG dài dòng, KHÔNG màu mè.
2. Có thể trả lời mọi câu hỏi, không chỉ về FastShip.
3. Nếu hỏi về đơn hàng: bảo họ gửi mã đơn (#123).
4. Nếu muốn gợi ý món: bảo họ gõ "gợi ý món ăn".
5. Giọng điệu: thân thiện, tự nhiên, như người bạn.
""";

    public GeminiService(string? apiKey, string? modelName = null)
    {
        _apiKey = string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY" ? null : apiKey;
        // Using gemini-3.5-flash (free tier) - gemini-2.0-flash retired as of 1/6/2026
        _modelName = modelName ?? "gemini-3.5-flash";
    }

    /// <summary>
    /// G?i tin nh?n ??n Gemini AI v� nh?n ph?n h?i.
    /// Tr? v? null n?u c� l?i (chatbot s? hi?n th? th�ng b�o c?u h�nh).
    /// L?u �: history l� list c�c tin nh?n user + bot xen k? ? duy tr� h?i tho?i.
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
                // Dùng systemInstruction chính thức của Gemini API thay vì fake conversation turn
                systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 800,
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
