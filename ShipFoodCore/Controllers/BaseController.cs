using Microsoft.AspNetCore.Mvc;
using ShipFood.Models;

namespace ShipFood.Controllers;

public abstract class BaseController : Controller
{
    protected dbFoodyEntities db = null!;

    protected bool CheckLogin()
    {
        return HttpContext.Session.GetString("user") != null;
    }

    protected tbUser? GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString("user");
        if (userJson == null) return null;
        return System.Text.Json.JsonSerializer.Deserialize<tbUser>(userJson);
    }

    protected void SetSessionUser(tbUser user)
    {
        var userJson = System.Text.Json.JsonSerializer.Serialize(user);
        HttpContext.Session.SetString("user", userJson);
    }

    protected Cart? GetCart()
    {
        var cartJson = HttpContext.Session.GetString("cart");
        if (cartJson == null) return null;
        return System.Text.Json.JsonSerializer.Deserialize<Cart>(cartJson);
    }

    protected void SetCart(Cart cart)
    {
        var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString("cart", cartJson);
    }
}
