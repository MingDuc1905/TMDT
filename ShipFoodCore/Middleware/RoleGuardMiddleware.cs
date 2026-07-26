// ============================================================
// 🛡️ RoleGuardMiddleware — Bảo vệ truy cập chéo role
// ============================================================
// Ý nghĩa: Chặn user không đúng role truy cập trang của role khác
// Chức năng: Kiểm tra session, fallback auth cookie, phục hồi session tự động
// KEYWORDS: middleware, role guard, security, authorization, session, redirect, bảo vệ
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Trigger: Mọi request HTTP đến server (middleware pipeline)
//   Gọi bởi: Program.cs (app.UseMiddleware<RoleGuardMiddleware>())
//   Gọi đến: BaseController (RestoreSessionFromClaims — phục hồi session)
//            HomeController (Login/Logout redirect)
//            RestaurantController, ShipperController, AdminController (redirect)
//   Bypass Paths: /health, /Home/Login, /Home/Error, /Home/Signup, /nhantin
//   Route Map: /admin → Admin, /restaurant → Quán ăn, /shipper → Shipper
//   Flow: Request → Check BypassPaths → Xác định Route →
//        Lấy session → null? → Restore từ auth cookie →
//        Có user? → Đúng role? → OK → Next
//        Không user → 401 JSON (AJAX) hoặc redirect /Home/Login
//        Sai role → 403 JSON (AJAX) hoặc redirect về dashboard của role đó
//   JSON API: Auto-detect → 401/403 JSON thay vì redirect (cho AJAX)
//   Security: Boundary check → /admin KHÔNG match /adminchat (tránh false positive)
// ============================================================
using System.Security.Claims;
using ShipFood.Models;
using System.Text.Json;

namespace ShipFood.Middleware;

public class RoleGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleGuardMiddleware> _logger;

    // Danh sách các đường dẫn được bỏ qua hoàn toàn (không cần đăng nhập)
    private static readonly HashSet<string> BypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/Home/Error",
        "/Home/Login",
        "/Home/GoogleLogin",
        "/Home/GoogleResponse",
        "/Home/GooglePartnerLogin",
        "/Home/SelectRoleGoogle",
        "/Home/CompleteGoogleRegistration",
        "/Home/Logout",
        "/Home/Signup",
        "/Home/DbDebug",
        "/Home/FixPasswords",
        "/Home/SeedDb",
        "/home/menusearch",   // search autocomplete JSON API
        "/nhantin",  // SignalR hub
    };

    /// <summary>
    /// Phục hồi session từ auth cookie claims (dùng khi session mất do app restart).
    /// Gọi CommitAsync để ghi ngay session xuống Redis/InMemory — tránh mất session.
    /// </summary>
    private static async Task RestoreSessionFromCookieAsync(HttpContext context, dbFoodyEntities db)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return;

        try
        {
            var user = db.tbUser.Find(userId);
            if (user != null && user.trangthai == 1)
            {
                var userJson = JsonSerializer.Serialize(user);
                context.Session.SetString("user", userJson);
                // ═══ FIX: Commit session ngay lập tức để Redis/InMemory lưu ═══
                // Tránh tình trạng session bị mất do chưa kịp persist
                await context.Session.CommitAsync();
            }
        }
        catch { /* DB unavailable — không thể phục hồi */ }
    }

    // ═══ JSON API patterns — các endpoint trả về JSON cần tự động xử lý AJAX ═══
    private static readonly HashSet<string> JsonApiPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Admin/GetDashboardStats",
        "/Admin/GetRevenueChart",
        "/Admin/GetTopRestaurants",
        "/Admin/GetOrderStatusPie",
        "/Admin/MockPaymentWebhook",
        "/Shipper/UpdateDonHang",
        "/EDelivery/ConfirmScan",
        "/EDelivery/Bypass",
        "/Cart/",
        "/Payment/",
        "/Chatbot/",
        "/AdminChat/",
        "/Home/GetReviews",
        "/Home/SubmitReview",
        "/Home/GetReviewableItems",
        "/Restaurant/ToggleConHang",
        "/Restaurant/ReplyReview",
    };

    // Route → Role mapping
    private static readonly Dictionary<string, string> RouteRoleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "/admin", "Admin" },
        { "/restaurant", "Quán ăn" },
        { "/shipper", "Shipper" },
    };

    public RoleGuardMiddleware(RequestDelegate next, ILogger<RoleGuardMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, dbFoodyEntities db)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // ═══ Bỏ qua các đường dẫn bypass ═══
        foreach (var bp in BypassPaths)
        {
            if (path.StartsWith(bp, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }

        // ═══ Xác định role yêu cầu cho route hiện tại ═══
        // ponytail: boundary check — "/admin" phải match "/admin/dashboard" NHƯNG KHÔNG match "/adminchat"
        string? requiredRole = null;
        foreach (var kvp in RouteRoleMap)
        {
            if (path == kvp.Key || path.StartsWith(kvp.Key + "/", StringComparison.OrdinalIgnoreCase))
            {
                requiredRole = kvp.Value;
                break;
            }
        }

        if (requiredRole == null)
        {
            await _next(context);
            return;
        }

        // ═══ Lấy user từ session ═══
        var userJson = context.Session.GetString("user");
        tbUser? user = null;
        if (!string.IsNullOrEmpty(userJson))
        {
            try { user = JsonSerializer.Deserialize<tbUser>(userJson); }
            catch { }
        }

        // ═══ Chưa đăng nhập → thử phục hồi từ auth cookie ═══
        if (user == null)
        {
            if (context.User?.Identity?.IsAuthenticated == true && !path.Equals("/home/logout", StringComparison.OrdinalIgnoreCase))
            {
                await RestoreSessionFromCookieAsync(context, db);
                userJson = context.Session.GetString("user");
                if (!string.IsNullOrEmpty(userJson))
                {
                    try { user = JsonSerializer.Deserialize<tbUser>(userJson); } catch { }
                    if (user != null)
                    {
                        _logger.LogInformation("RoleGuard: Session restored from auth cookie for user {UserId}", user.userid);
                    }
                }
            }

            // NẾU ĐANG Ở /Home/ → KHÔNG redirect (tránh loop)
            if (path.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ═══ Auto-detect JSON API endpoints ═══
            var isJsonApi = false;
            foreach (var prefix in JsonApiPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    isJsonApi = true;
                    break;
                }
            }

            if (isJsonApi ||
                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.",
                    redirectUrl = "/Home/Login"
                }));
                return;
            }

            context.Response.Redirect("/Home/Login");
            return;
        }

        // ═══ Sai role → redirect về trang phù hợp ═══
        if (user.loaitaikhoan != requiredRole)
        {
            _logger.LogWarning("RoleGuard: User {UserId} ({Role}) tried to access {Path} (required: {RequiredRole})",
                user.userid, user.loaitaikhoan, path, requiredRole);

            var redirectUrl = user.loaitaikhoan switch
            {
                "Khách hàng" => "/Home",
                "Quán ăn" => "/Restaurant",
                "Shipper" => "/Shipper",
                "Admin" => "/Admin",
                _ => "/Home/Login"
            };

            if (path.StartsWith(redirectUrl.ToLowerInvariant()))
            {
                _logger.LogWarning("RoleGuard redirect loop detected for User {UserId} — redirecting to /Home", user.userid);
                redirectUrl = "/Home";
            }

            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Bạn không có quyền truy cập trang này.",
                    redirectUrl
                }));
                return;
            }

            context.Response.Redirect(redirectUrl);
            return;
        }

        await _next(context);
    }
}
