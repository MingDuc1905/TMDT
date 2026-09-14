// ============================================================
// ⚙️ BaseController — Lớp nền tảng cho tất cả Controllers | Base Class
// ============================================================
// Ý nghĩa: Lớp abstract cha của mọi Controller, cung cấp các chức năng dùng chung
// Chức năng: Session management (get/set user, cart), auth cookie (Remember Me),
//            role checking (CheckLogin, CheckRoleJson), session restore từ cookie
// KEYWORDS: base, session, cookie, auth, authentication, role check,
//           login, remember me, user session, cart session
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Kế thừa bởi: HomeController, CartController, RestaurantController,
//                ShipperController, AdminController, PaymentController,
//                AdminChatController, ChatbotController, EDeliveryController
//   Calls →: HttpContext.Session (get/set user, cart)
//            HttpContext.SignInAsync (auth cookie với claims)
//            dbFoodyEntities (phục hồi session từ DB)
//            Models: tbUser, Cart
//   Called by ←: Tất cả Controller con gọi GetCurrentUser(), CheckLogin(), GetCart()
//   Flow: GetCurrentUser → Session "user" → nếu null → fallback cookie claims
//        → RestoreSessionFromClaims → Find DB → SetSessionUser
//        SetSessionAndCookieAsync → SetSessionUser + SignInAsync + CommitAsync
//        GetCart/SetCart → Session "cart" (JSON serialize/deserialize)
//        CheckRoleJson → kiểm tra role → trả về 403 JSON nếu sai
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
    // ════════════════════════════════════════════════════════════
    // 📦 KHỐI KHỞI TẠO (FIELDS) — cấu hình chung cho mọi controller con
    // ════════════════════════════════════════════════════════════
    // KEYWORDS: db, dbcontext, json options, ignore cycles, base controller
    // → FILE: mọi controller con (Home, Cart, Payment, Restaurant,
    //         Shipper, Admin, AdminChat, Chatbot, EDelivery) kế thừa
    //         và tự gán `db` trong constructor.
    // db: DbContext dùng chung (Models/DbContext.cs) — được DI inject
    //     ở từng controller con qua `dbFoodyEntities context`.
    // _jsonOptions: cấu hình JSON serialize — bỏ qua vòng lặp tham
    //     chiếu (ReferenceHandler.IgnoreCycles) khi lưu Cart/user
    //     vào Session (tránh lỗi cyclic reference khi model có navigation).
    protected dbFoodyEntities db = null!;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    // ════════════════════════════════════════════════════════════
    // 🔐 KHỐI XÁC THỰC (AUTH) — kiểm tra đăng nhập & phục hồi session
    // ════════════════════════════════════════════════════════════
    // KEYWORDS: auth, login check, session, cookie, restore session
    // → GỌI BỞI: CheckLogin()/GetCurrentUser() được gọi từ MỌI action
    //   của mọi controller con trước khi xử lý nghiệp vụ.
    // → FILE: RoleGuardMiddleware.cs cũng có logic phục hồi session
    //   tương tự (RestoreSessionFromCookieAsync) ở tầng middleware.

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
    // KEYWORDS: restore, claims, cookie auth, session recovery
    // → FILE: đọc tbUser (Models/tbUser.cs) từ DB theo userId claim.
    // → CHỈ chấp nhận user còn hoạt động (trangthai == 1).
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
    // KEYWORDS: set session, auth cookie, signin, remember me, claims
    // → FILE: gọi từ HomeController (Login, Facebook/Google OAuth, Signup)
    // → TẠO Claims: NameIdentifier, Name, Role, loaitaikhoan → dùng bởi
    //   RoleGuardMiddleware.cs và Chats.cs (GetCallerUserIdAsync) để
    //   xác định role/quyền của user hiện tại.
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

    // ════════════════════════════════════════════════════════════
    // 🛒 KHỐI GIỎ HÀNG (CART SESSION) — lưu trữ giỏ hàng trong Session
    // ════════════════════════════════════════════════════════════
    // KEYWORDS: cart, gio hang, session cart, get cart, set cart
    // → FILE: CartController.cs + HomeController.cs dùng để thêm/sửa
    //   món, checkout; PaymentController.cs đọc giỏ để tạo đơn.
    // → MODELS: Cart.cs (Models/Cart.cs) — đối tượng JSON lưu trong
    //   Session key "cart".
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
    // KEYWORDS: role check, authorization, json api, 403, quyen truy cap
    // → GỌI BỞI: các action JSON trả về dữ liệu (vd CartController.CheckCoupon,
    //   RestaurantController.ToggleConHang, ShipperController...) trước khi
    //   xử lý để đảm bảo chỉ role đúng mới được gọi API.
    // → KHÁC RoleGuardMiddleware: CheckRoleJson kiểm tra theo role STRING
    //   cụ thể, còn middleware kiểm tra theo ROUTE prefix.
    protected JsonResult? CheckRoleJson(string requiredRole)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            Response.StatusCode = 403;
            return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục" });
        }
        // ponytail: dung == thay .Equals() de tranh NullReferenceException khi loaitaikhoan null
        if (user.loaitaikhoan != requiredRole)
        {
            Response.StatusCode = 403;
            return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này" });
        }
        return null; // OK
    }
}
