// ============================================================
// 🤖 OpenAIService — Chatbot AI (OpenAI-compatible)
// ============================================================
// Ý nghĩa: Kết nối OpenAI API (ZenMux) cho chatbot tư vấn món ăn
// Chức năng: Gọi OpenAI chat completions với system prompt tiếng Việt, lịch sử hội thoại
// KEYWORDS: openai, chatbot, ai, chatgpt, zenmux, hỏi đáp, system prompt, gpt, assistant
//
// LUỒNG DỮ LIỆU:
//   HomeController.ChatAI ⭢ ChatbotController.SendMessage ⭢ gọi OpenAIService.SendMessageAsync()
//   OpenAIService.SendMessageAsync ⭢ gọi OpenAI API (hoặc ZenMux endpoint)
//   OpenAI trả về text response ⭢ ChatbotController trả về JSON ⭢ Home/Index.cshtml hiển thị
//   Nếu chưa cấu hình OPENAI_API_KEY ⭢ trả về null ⭢ ChatbotController fallback rule-based
//
// FILES LIÊN QUAN:
//   CALLED BY:  ChatbotController.cs (SendMessage action)
//   CALLS:      OpenAI API (api.openai.com hoặc ZenMux custom endpoint)
//   LIÊN QUAN:  HomeController.cs (gọi ChatbotController qua fetch API)
//   LIÊN QUAN:  Views/Home/Index.cshtml (UI chatbot box)
//   LIÊN QUAN:  tbTinNhan.cs (lưu lịch sử chat vào DB)
//   DEPENDS ON: Biến môi trường: OPENAI_API_KEY, OPENAI_API_BASE, OPENAI_MODEL
// ============================================================
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace ShipFood.Services;

public class OpenAIService
{
    private readonly ChatClient? _chatClient;
    private readonly ILogger<OpenAIService> _logger;

    // ponytail: System prompt gi? nguyên n?i dung g?c, ch? thay tên AI assistant
    private const string SystemPrompt = """
Bạn là trợ lý FastShip - nền tảng giao đồ ăn tại Việt Nam.

QUY TẮC:
1. Trả lời tiếng Việt, tối đa 2-3 câu, đi thẳng vấn đề. Không dài dòng, không màu mè, không nịnh khách hàng.
2. Chỉ trả lời các câu hỏi liên quan đến FastShip (đặt món, giao hàng, thanh toán, khuyến mãi, quán ăn). Nếu hỏi ngoài chủ đề, từ chối nhẹ nhàng và gợi ý quay lại chủ đề FastShip.
3. Nếu hỏi về đơn hàng: bảo họ gửi mã đơn (#123).
4. Nếu muốn gợi ý món: bảo họ gõ "gợi ý món ăn".
5. Giọng điệu: tự nhiên, dân dã, như người bình thường nói chuyện với nhau. Không khách sáo, không xu nịnh, không dùng từ hoa mỹ.
""";

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _logger = logger;

        // ponytail: ??c bi?n môi tr??ng OPENAI_API_KEY và OPENAI_API_BASE (vi?t hoa) t? Render
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? configuration["OpenAI:ApiKey"];
        var baseUrl = Environment.GetEnvironmentVariable("OPENAI_API_BASE")
                   ?? configuration["OpenAI:ApiBase"];
        var modelName = Environment.GetEnvironmentVariable("OPENAI_MODEL")
                     ?? configuration["OpenAI:Model"]
                     ?? "gpt-4o-mini";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY")
        {
            _logger.LogWarning("OpenAIService: OPENAI_API_KEY not configured — AI chatbot disabled");
            _chatClient = null;
            return;
        }

        try
        {
            var credential = new ApiKeyCredential(apiKey);

            if (!string.IsNullOrEmpty(baseUrl))
            {
                // ponytail: custom base URL (ZenMux ho?c b?t k? OpenAI-compatible endpoint nào)
                var options = new OpenAIClientOptions
                {
                    Endpoint = new Uri(baseUrl)
                };
                _chatClient = new ChatClient(modelName, credential, options);

                _logger.LogInformation(
                    "OpenAIService initialized with custom endpoint: {BaseUrl}, model: {Model}",
                    baseUrl, modelName);
            }
            else
            {
                // fallback: dùng endpoint m?c ??nh c?a OpenAI (api.openai.com)
                _chatClient = new ChatClient(modelName, credential);

                _logger.LogInformation(
                    "OpenAIService initialized with default OpenAI endpoint, model: {Model}",
                    modelName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAIService failed to initialize");
            _chatClient = null;
        }
    }

    /// <summary>
    /// Kiểm tra service đã được cấu hình đầy đủ chưa
    /// </summary>
    public bool IsConfigured => _chatClient != null;

    /// <summary>
    /// Gửi tin nhắn đến OpenAI-compatible API và nhận phản hồi.
    /// Trả về null nếu chưa cấu hình hoặc có lỗi nghiêm trọng.
    /// Trả về string thông báo lỗi thân thiện nếu gặp lỗi tạm thời.
    /// history: danh sách tin nhắn user + bot xen kẽ để duy trì hội thoại.
    /// </summary>
    public async Task<string?> SendMessageAsync(string message, List<string>? history = null)
    {
        if (!IsConfigured) return null;

        try
        {
            // ─── Xây d?ng danh sách tin nh?n ───
            var messages = new List<ChatMessage>
            {
                // System prompt
                new SystemChatMessage(SystemPrompt)
            };

            // Lịch sử hội thoại (user + assistant xen kẽ)
            if (history != null)
            {
                for (int i = 0; i < history.Count; i++)
                {
                    if (i % 2 == 0)
                        messages.Add(new UserChatMessage(history[i]));
                    else
                        messages.Add(new AssistantChatMessage(history[i]));
                }
            }

            // Tin nhắn hiện tại
            messages.Add(new UserChatMessage(message));

            // ─── G?i API ───
            var completionOptions = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 800
            };

            var completion = await _chatClient!.CompleteChatAsync(messages, completionOptions);

            var text = completion?.Value?.Content?.FirstOrDefault()?.Text;

            return string.IsNullOrEmpty(text)
                ? "Xin lỗi, tôi chưa thể trả lời câu hỏi này ngay. Bạn có thể thử hỏi lại nhé!"
                : text;
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogWarning("OpenAI API 429 Too Many Requests: {Message}", ex.Message);
            return "⚠️ Hệ thống AI đang quá tải do lượt truy cập cao vào giờ cao điểm, vui lòng thử lại sau 1 phút.";
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("OpenAI API request timeout after 30s");
            return "⏱️ Hệ thống AI phản hồi chậm do kết nối mạng, vui lòng thử lại sau vài giây.";
        }
        catch (ClientResultException ex)
        {
            // ponytail: Log ??y ?? thông tin l?i ?? debug
            var errorDetail = ex.Message ?? "(no message)";
            var innerDetail = ex.InnerException?.Message ?? "(no inner)";
            _logger.LogError(ex,
                "OpenAI API client error (status {Status}). Message={Message}, Inner={Inner}",
                ex.Status, errorDetail, innerDetail);

            // ponytail: Phân lo?i l?i d?a trên status code
            if (ex.Status == 400)
                return "❌ API trả về lỗi 400 (Bad Request). Vui lòng kiểm tra cấu hình model và endpoint.";
            if (ex.Status == 401)
                return "🔑 API key không hợp lệ (401 Unauthorized). Vui lòng kiểm tra OPENAI_API_KEY.";
            if (ex.Status == 403)
                return "🔒 API key không có quyền truy cập model này (403 Forbidden).";
            if (ex.Status == 404)
                return "🔍 Endpoint hoặc model không tồn tại (404 Not Found). Vui lòng kiểm tra OPENAI_API_BASE và model name.";
            if (ex.Status >= 500)
                return "🔧 Dịch vụ AI đang gặp lỗi máy chủ (" + ex.Status + "). Vui lòng thử lại sau.";

            // ponytail: Các l?i khác — log chi ti?t response body n?u có
            return $"🔌 Không thể kết nối đến dịch vụ AI (HTTP {ex.Status}). Vui lòng kiểm tra OPENAI_API_KEY và OPENAI_API_BASE.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenAI API unexpected error: {Message}. Inner: {Inner}",
                ex.Message, ex.InnerException?.Message ?? "(no inner)");
            return null; // Fallback: chatbot s? dùng rule-based
        }
    }
}
