using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

/// <summary>
/// Một item trong giỏ hàng, đại diện cho 1 biến thể (size) của món ăn
/// </summary>
public class CartItem
{
    public int mabienthe { get; set; }       // FK → tbBienTheMonAn.id
    public int mamon { get; set; }            // FK → tbMonAn.mamon (để nhóm)
    public string tenmon { get; set; } = "";
    public string? size { get; set; }         // Ví dụ: "M", "L", null
    public decimal? giatien { get; set; }
    public string? hinhanh { get; set; }
    public int? maquanan { get; set; }
    public int soLuong { get; set; }
}

public class Cart
{
    public int? userid { get; set; }
    public decimal? tongTien { get; set; }
    public int? maquanan { get; set; }  // Legacy: first restaurant
    public int? maKM { get; set; }
    public List<CartItem> items { get; set; } = new();
    public tbThongTinDatHang? thongTinDatHang { get; set; }

    // ─── Multi-restaurant support ───
    /// <summary>
    /// Get unique restaurant IDs in cart
    /// </summary>
    [NotMapped]
    public List<int> RestaurantIds => items
        .Where(i => i.maquanan.HasValue)
        .Select(i => i.maquanan!.Value)
        .Distinct()
        .ToList();

    /// <summary>
    /// Check if adding item from another restaurant is allowed
    /// </summary>
    public bool CanAddFromRestaurant(int? restaurantId)
    {
        if (!restaurantId.HasValue) return false;
        if (!items.Any(i => i.maquanan.HasValue)) return true;
        return RestaurantIds.Contains(restaurantId.Value);
    }

    public Cart()
    {
        items = new List<CartItem>();
        tongTien = 0;
    }

    public void themMon(CartItem item, int soLuong)
    {
        foreach (var i in items)
        {
            if (i.mabienthe == item.mabienthe)
            {
                i.soLuong += soLuong;
                tongTien += i.giatien * soLuong;
                return;
            }
        }
        item.soLuong = soLuong;
        items.Add(item);
        tongTien += item.giatien * soLuong;
    }

    public void xoaMon(int mabienthe)
    {
        foreach (var i in items)
        {
            if (i.mabienthe == mabienthe)
            {
                items.Remove(i);
                tongTien -= i.giatien * i.soLuong;
                return;
            }
        }
    }

    public void giamMon(int? mabienthe)
    {
        foreach (var i in items)
        {
            if (i.mabienthe == mabienthe)
            {
                if (i.soLuong <= 1)
                {
                    items.Remove(i);
                    tongTien -= i.giatien;
                    break;
                }
                else
                {
                    i.soLuong -= 1;
                    tongTien -= i.giatien;
                    return;
                }
            }
        }
    }

    // ─── Backward-compatible properties ───
    [NotMapped]
    public List<tbMonAn> monAns
    {
        get
        {
            return items.Select(i => new tbMonAn
            {
                mamon = i.mamon,
                tenmon = i.tenmon,
                hinhanh = i.hinhanh,
                maquanan = i.maquanan,
                giatien = i.giatien,
                soLuong = i.soLuong
            }).ToList();
        }
        set { }
    }
}
