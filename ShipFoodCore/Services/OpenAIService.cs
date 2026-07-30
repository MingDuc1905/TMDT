// ============================================================
// 🤖 OpenAIService — Chatbot AI (OpenAI-compatible / ZenMux)
// ============================================================
// Ý nghĩa: Kết nối OpenAI-compatible API (ZenMux) bằng HttpClient
// Chức năng: Gọi POST {OPENAI_API_BASE}/chat/completions, parse JSON response
// KEYWORDS: openai, zenmux, chatbot, ai, chatgpt, httpclient, completions
//
// LUỒNG DỮ LIỆU:
//   ChatbotController.SendMessage ⭢ OpenAIService.SendMessageAsync()
//   ⭢ HttpClient POST {OPENAI_API_BASE}/chat/completions
//   ⭢ Parse JSON response → extract content → return string
//
// BIẾN MÔI TRƯỜNG (Render):
//   OPENAI_API_KEY  — API key (NaraRouter: sk-nry-...)
//   OPENAI_API_BASE — Base URL: https://router.bynara.id/v1
//   OPENAI_MODEL    — Model: agnes-2.5-flash (free, 512K context)
//
// FILES LIÊN QUAN:
//   CALLED BY:  ChatbotController.cs
//   GIỐNG:     OpenAI chat completions format (tương thích ngược)
// ============================================================
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipFood.Services;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string? _apiKey;
    private readonly string? _baseUrl;
    private readonly string _model;

    // ponytail: System prompt — AI thân thi?n, t? nhiên, t?p trung vào FastShip
    private const string SystemPrompt = """
Bạn là trợ lý AI thân thiện của nền tảng FastShip (ShipFood) - giao đồ ăn tại TP.HCM.
Hãy trả lời tự nhiên như một người bạn am hiểu về ẩm thực và dịch vụ của FastShip.

QUY TẮC:
1. Trả lời tự nhiên, thân thiện, có thể chào hỏi nhẹ nhàng nếu phù hợp.
2. Luôn trung thực - nếu không biết thì nói không biết, đừng bịa thông tin.
3. Chỉ nói về FastShip (ShipFood) - không trả lời câu hỏi ngoài phạm vi.
4. Ưu tiên tiếng Việt, rõ ràng, dễ hiểu.
""";

    // ─── JSON Models cho request/response ───
    private class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 800;
    }

    private class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    private class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _logger = logger;

        // ─── ??c bi?n môi tr??ng t? Render ───
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? configuration["OpenAI:ApiKey"];
        _baseUrl = Environment.GetEnvironmentVariable("OPENAI_API_BASE")
                ?? configuration["OpenAI:ApiBase"];
        _model = Environment.GetEnvironmentVariable("OPENAI_MODEL")
              ?? configuration["OpenAI:Model"]
              ?? "agnes-2.5-flash";

        _logger.LogInformation("OpenAI config - BaseUrl: {BaseUrl}, Model: {Model}", _baseUrl ?? "(default)", _model);

        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_OPENAI_API_KEY")
        {
            _logger.LogWarning("OPENAI_API_KEY not configured — AI chatbot disabled");
            _httpClient = null!;
            return;
        }

        // ─── C?u hình HttpClient ───
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(60); // timeout 60s cho ZenMux/OpenAI
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _httpClient = client;

        _logger.LogInformation(
            "OpenAI-compatible client initialized: {BaseUrl}, model: {Model}",
            _baseUrl ?? "(default OpenAI)", _model);
    }

    /// <summary>
    /// Ki?m tra service dã du?c c?u hình d?y d?
    /// </summary>
    public bool IsConfigured => _httpClient != null;

    /// <summary>
    /// G?i OpenAI-compatible API b?ng HttpClient, parse JSON response.
    /// Tr? v? null n?u chua c?u hình ho?c l?i nghiêm tr?ng.
    /// history: user + bot xen k? (user, bot, user, bot...)
    /// </summary>
    public async Task<string?> SendMessageAsync(string message, List<string>? history = null)
    {
        if (!IsConfigured) return null;

        try
        {
            // ─── Xây d?ng messages array ───
            var messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt }
            };

            if (history != null)
            {
                for (int i = 0; i < history.Count; i++)
                {
                    messages.Add(new ChatMessage
                    {
                        Role = i % 2 == 0 ? "user" : "assistant",
                        Content = history[i]
                    });
                }
            }

            messages.Add(new ChatMessage { Role = "user", Content = message });

            // ─── Xây d?ng request body ───
            var requestBody = new ChatRequest
            {
                Model = _model,
                Messages = messages,
                // ponytail: temperature 0.7 cho câu tr? l?i t? nhiên, không quá sáng t?o
                Temperature = 0.7,
                // ponytail: maxTokens 600 ??? dài v?a ph?i, không quá d?i dòng
                MaxTokens = 600
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ─── Xác d?nh endpoint URL ───
            var defaultBase = "https://router.bynara.id/v1";
            var endpoint = string.IsNullOrEmpty(_baseUrl)
                ? $"{defaultBase}/chat/completions"
                : $"{_baseUrl.TrimEnd('/')}/chat/completions";

            _logger.LogDebug("Sending request to {Endpoint} with model {Model}", endpoint, _model);

            // ─── G?i API ───
            var response = await _httpClient.PostAsync(endpoint, content);

            // ─── X? lý HTTP error ───
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "API returned HTTP {StatusCode}: {ErrorBody}",
                    statusCode, errorBody);

                return statusCode switch
                {
                    429 => "⚠️ Hệ thống AI đang quá tải, vui lòng thử lại sau 1 phút.",
                    400 => "❌ API trả về lỗi 400 — kiểm tra model name hoặc endpoint.",
                    401 => "🔑 API key không hợp lệ (401). Kiểm tra OPENAI_API_KEY.",
                    403 => "🔒 API key không có quyền truy cập model (403).",
                    404 => "🔍 Endpoint hoặc model không tồn tại (404). Kiểm tra OPENAI_API_BASE và OPENAI_MODEL.",
                    _ when statusCode >= 500 => $"🔧 Dịch vụ AI đang lỗi ({statusCode}). Thử lại sau.",
                    _ => $"🔌 Lỗi kết nối AI (HTTP {statusCode}). Kiểm tra cấu hình."
                };
            }

            // ─── Parse JSON response ───
            var responseJson = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseJson);

            var reply = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

            if (!string.IsNullOrEmpty(reply))
                return reply;

            _logger.LogWarning("API returned empty content: {Response}", responseJson);
            return "Xin lỗi, mình chưa thể trả lời câu hỏi này. Bạn thử hỏi lại nhé!";
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("API request timed out after 60s");
            return "⏱️ Hệ thống AI phản hồi chậm, vui lòng thử lại sau vài giây.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Message}", ex.Message);
            return "🔌 Không thể kết nối đến dịch vụ AI. Kiểm tra kết nối mạng và OPENAI_API_BASE.";
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse API response JSON");
            return "❌ Lỗi xử lý phản hồi từ AI. Vui lòng thử lại.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
            return null; // Fallback v? rule-based
        }
    }
}
