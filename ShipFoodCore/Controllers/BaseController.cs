// ============================================================
// ⚙️ BaseController — Lớp nền tảng cho tất cả Controllers | Base Class
// ============================================================
// Ý nghĩa: Lớp abstract cha của mọi Controller, cung cấp các chức năng dùng chung
// Chức năng: Session management (get/set user, cart), auth cookie (Remember Me),
//            role checking (CheckLogin, CheckRoleJson), session restore từ cookie
// KEYWORDS: base, session, cookie, auth, authentication, role check,
//           login, remember me, user session, cart session
// ============================================================
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ShipFood.Models;

namespace ShipFood.Controllers;

public abstract class BaseController : Controller
{
    protected dbFoodyEntities db = null!;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Kiểm tra đăng nhập: ưu tiên Session, fallback Cookie Auth.
    /// Nếu session mất (do restart) nhưng cookie auth còn → tự động phục hồi session.
    /// </summary>
    protected bool CheckLogin()
    {
        if (HttpContext.Session.GetString("user") != null)
            return true;

        // Fallback: kiểm tra auth cookie (User.Identity.IsAuthenticated)
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            RestoreSessionFromClaims();
            return HttpContext.Session.GetString("user") != null;
        }

        return false;
    }

    /// <summary>
    /// Lấy user hiện tại: ưu tiên Session, fallback Cookie Auth.
    /// </summary>
    protected tbUser? GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString("user");
        if (userJson != null)
            return JsonSerializer.Deserialize<tbUser>(userJson, _jsonOptions);

        // Fallback: nếu có auth cookie, phục hồi session và trả về user
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            RestoreSessionFromClaims();
            userJson = HttpContext.Session.GetString("user");
            if (userJson != null)
                return JsonSerializer.Deserialize<tbUser>(userJson, _jsonOptions);
        }

        return null;
    }

    /// <summary>
    /// Phục hồi session từ Cookie Auth claims.
    /// Được gọi khi session mất (restart) nhưng auth cookie còn.
    /// ponytail: sync method, callers (CheckLogin, GetCurrentUser) không async được.
    /// Session auto-commit ở cuối request, CommitAsync chỉ là safety.
    /// </summary>
    private void RestoreSessionFromClaims()
    {
        var userIdClaim = HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return;

        if (!int.TryParse(userIdClaim, out var userId))
            return;

        try
        {
            var user = db.tbUser.Find(userId);
            if (user != null && user.trangthai == 1)
            {
                SetSessionUser(user);
                // Session auto-commit khi response kết thúc
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<BaseController>>();
                logger.LogInformation("Session restored from auth cookie for user {UserId} ({Username})", userId, user.username);
            }
        }
        catch
        {
            // DB unavailable — không thể phục hồi
        }
    }

    /// <summary>
    /// Set session user + tạo auth cookie (dùng cho cả manual + OAuth login).
    /// Cookie có thời hạn 30 ngày, sliding expiration → tồn tại qua restart.
    /// </summary>
    protected async Task SetSessionAndCookieAsync(tbUser user, bool rememberMe = false)
    {
        // 1. Set session
        SetSessionUser(user);

        // 2. Tạo auth cookie với claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.userid.ToString()),
            new Claim(ClaimTypes.Name, user.username ?? ""),
            new Claim(ClaimTypes.Role, user.loaitaikhoan ?? "Khách hàng"),
            new Claim("loaitaikhoan", user.loaitaikhoan ?? "Khách hàng"),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(1),
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        // 3. Commit session để đảm bảo session cookie được ghi
        await HttpContext.Session.CommitAsync();
    }

    protected void SetSessionUser(tbUser user)
    {
        var userJson = JsonSerializer.Serialize(user, _jsonOptions);
        HttpContext.Session.SetString("user", userJson);
    }

    protected Cart? GetCart()
    {
        var cartJson = HttpContext.Session.GetString("cart");
        if (cartJson == null) return null;
        return JsonSerializer.Deserialize<Cart>(cartJson, _jsonOptions);
    }

    protected void SetCart(Cart cart)
    {
        var cartJson = JsonSerializer.Serialize(cart, _jsonOptions);
        HttpContext.Session.SetString("cart", cartJson);
    }

    /// <summary>
    /// Kiểm tra quyền truy cập cho JSON API endpoints.
    /// Nếu không đăng nhập hoặc sai role → trả về JsonResult 403 Forbidden
    /// </summary>
    protected JsonResult? CheckRoleJson(string requiredRole)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            Response.StatusCode = 403;
            return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục" });
        }
        if (!user.loaitaikhoan.Equals(requiredRole))
        {
            Response.StatusCode = 403;
            return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này" });
        }
        return null; // OK
    }
}
