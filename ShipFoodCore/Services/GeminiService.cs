using System.Net;
using System.Text;
using System.Text.Json;

namespace ShipFood.Services;

public class GeminiService
{
    private readonly string? _apiKey;
    private readonly string _modelName;

    // ponytail: security fix — không hardcode thông tin n?i b? trong system prompt
    // Thông tin nh?y c?m (email, sdt, d?a ch?) không ???c hardcode trong prompt
    // Các thông tin này s? ???c inject t? DB context summary bên ChatbotController
    private const string SystemPrompt = """
Bạn là trợ lý FastShip - nền tảng giao đồ ăn tại Việt Nam.

QUY TẮC:
1. Trả lời tiếng Việt, tối đa 2-3 câu, đi thẳng vấn đề. Không dài dòng, không màu mè, không nịnh khách hàng.
2. Chỉ trả lời các câu hỏi liên quan đến FastShip (đặt món, giao hàng, thanh toán, khuyến mãi, quán ăn). Nếu hỏi ngoài chủ đề, từ chối nhẹ nhàng và gợi ý quay lại chủ đề FastShip.
3. Nếu hỏi về đơn hàng: bảo họ gửi mã đơn (#123).
4. Nếu muốn gợi ý món: bảo họ gõ "gợi ý món ăn".
5. Giọng điệu: tự nhiên, dân dã, như người bình thường nói chuyện với nhau. Không khách sáo, không xu nịnh, không dùng từ hoa mỹ.
""";

    public GeminiService(string? apiKey, string? modelName = null)
    {
        _apiKey = string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY" ? null : apiKey;
        // Use gemini-1.5-flash (latest stable, free tier) — model name can be overridden via env var
        _modelName = modelName ?? "gemini-1.5-flash";
    }

    /// <summary>
    /// Gửi tin nhắn đến Gemini AI và nhận phản hồi.
    /// Trả về null nếu có lỗi (chatbot sẽ hiển thị thông báo cấu hình).
    /// Trả về string thông báo lỗi thân thiện nếu HTTP 429 (Too Many Requests).
    /// Lưu ý: history là list các tin nhắn user + bot xen kẽ để duy trì hội thoại.
    /// </summary>
    public async Task<string?> SendMessageAsync(string message, List<string>? history = null)
    {
        if (!IsConfigured) return null;

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
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
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            Console.Error.WriteLine($"[Gemini] 429 Too Many Requests: {ex.Message}");
            return "⚠️ Hệ thống AI đang quá tải do lượt truy cập cao vào giờ cao điểm, vui lòng thử lại sau 1 phút.";
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine("[Gemini] Request timeout after 30s — Google AI Studio may be slow or unreachable from Render");
            return "⏱️ Hệ thống AI phản hồi chậm do kết nối mạng, vui lòng thử lại sau vài giây.";
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[Gemini] Network error: {ex.Message}");
            if (ex.InnerException != null)
                Console.Error.WriteLine($"[Gemini] Inner: {ex.InnerException.Message}");
            return "🔌 Mất kết nối đến dịch vụ AI, vui lòng kiểm tra mạng và thử lại.";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Gemini] Unexpected: {ex.GetType().Name}: {ex.Message}");
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
