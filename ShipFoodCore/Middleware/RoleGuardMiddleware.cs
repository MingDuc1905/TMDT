using ShipFood.Models;
using System.Text.Json;

namespace ShipFood.Middleware;

/// <summary>
/// RoleGuard Middleware — chặn truy cập chéo trang giữa các vai trò
/// Kiểm tra mọi request, nếu user đăng nhập nhưng không đúng role → redirect
/// </summary>
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
        "/nhantin",  // SignalR hub
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

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // ═══ Bỏ qua các đường dẫn bypass (không cần kiểm tra) ═══
        foreach (var bp in BypassPaths)
        {
            if (path.StartsWith(bp, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }

        // ═══ Xác định role yêu cầu cho route hiện tại ═══
        string? requiredRole = null;
        foreach (var kvp in RouteRoleMap)
        {
            if (path.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                requiredRole = kvp.Value;
                break;
            }
        }

        // ═══ Không cần role → cho qua (kể cả chưa đăng nhập) ═══
        // Giúp 404, CSS/JS, static files không bị redirect về Login
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

        // ═══ Chưa đăng nhập → redirect về login ═══
        if (user == null)
        {
            // NẾU ĐANG Ở /Home/Login → KHÔNG redirect nữa (tránh loop)
            if (path.StartsWith("/home/login", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // AJAX → JSON 401
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
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

            // Tránh redirect loop
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
