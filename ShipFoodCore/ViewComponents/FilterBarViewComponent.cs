using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.ViewComponents;

/// <summary>
/// ViewComponent: Dual-Filter Bar (Grab-like)
/// Renders horizontal scroll chip bar + Bottom Sheet trigger
/// </summary>
public class FilterBarViewComponent : ViewComponent
{
    private readonly dbFoodyEntities _db;

    public FilterBarViewComponent(dbFoodyEntities db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        int? categoryId = null,
        string? sortBy = null,
        bool? isPromo = null,
        bool? isBestSeller = null,
        bool? isNearMe = null,
        string? maxPriceLevel = null,
        string? maxDiet = null,
        string? mode = null,
        string? q = null)
    {
        var categories = await _db.tbDanhMuc.ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.ActiveCategoryId = categoryId;
        ViewBag.SortBy = sortBy ?? "suggest";
        ViewBag.IsPromo = isPromo ?? false;
        ViewBag.IsBestSeller = isBestSeller ?? false;
        ViewBag.IsNearMe = isNearMe ?? false;
        ViewBag.MaxPriceLevel = maxPriceLevel;
        ViewBag.MaxDiet = maxDiet;
        ViewBag.Mode = mode ?? "delivery";
        ViewBag.Query = q;

        return View();
    }
}
