using ShipFood.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShipFood.Middleware;

/// <summary>
/// RoleGuard Middleware — chặn truy cập chéo trang giữa các vai trò
/// Kiểm tra mọi request, nếu user đăng nhập nhưng không đúng role → redirect
/// </summary>
public class RoleGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleGuardMiddleware> _logger;

    // Route → Role mapping
    private static readonly Dictionary<string, string> RouteRoleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "/admin", "Admin" },
        { "/restaurant", "Quán ăn" },
        { "/shipper", "Shipper" },
        // Cart/Checkout dành cho Khách hàng (hoặc đã đăng nhập)
    };

    public RoleGuardMiddleware(RequestDelegate next, ILogger<RoleGuardMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Chỉ kiểm tra các route cần bảo vệ
        string? requiredRole = null;
        foreach (var kvp in RouteRoleMap)
        {
            if (path.StartsWith(kvp.Key))
            {
                requiredRole = kvp.Value;
                break;
            }
        }

        if (requiredRole != null)
        {
            // Lấy user từ session
            var userJson = context.Session.GetString("user");
            tbUser? user = null;
            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    user = JsonSerializer.Deserialize<tbUser>(userJson);
                }
                catch { }
            }

            if (user == null)
            {
                // Chưa đăng nhập → redirect về login
                context.Response.Redirect("/Home/Login");
                return;
            }

            if (user.loaitaikhoan != requiredRole)
            {
                _logger.LogWarning("RoleGuard: User {UserId} ({Role}) tried to access {Path} (required: {RequiredRole})",
                    user.userid, user.loaitaikhoan, path, requiredRole);

                // Redirect user to their correct dashboard
                var redirectUrl = user.loaitaikhoan switch
                {
                    "Khách hàng" => "/Home",
                    "Quán ăn" => "/Restaurant",
                    "Shipper" => "/Shipper",
                    "Admin" => "/Admin",
                    _ => "/Home"
                };

                // For AJAX requests, return JSON
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
        }

        await _next(context);
    }
}
