using System.Text.Json;
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

    protected bool CheckLogin()
    {
        return HttpContext.Session.GetString("user") != null;
    }

    protected tbUser? GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString("user");
        if (userJson == null) return null;
        return JsonSerializer.Deserialize<tbUser>(userJson, _jsonOptions);
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
}
