using ShipFood.Models;

namespace ShipFoodCore.Tests.Models;

/// <summary>
/// Tests for Cart model business logic.
/// The Cart class handles multi-restaurant support, add/remove/update quantity.
/// These tests verify the cart logic in isolation (no DB needed).
/// </summary>
public class CartTests
{
    [Fact]
    public void NewCart_IsEmpty()
    {
        // Act
        var cart = new Cart();

        // Assert
        Assert.Empty(cart.items);
        Assert.Equal(0, cart.tongTien);
        Assert.Null(cart.userid);
    }

    [Fact]
    public void ThemMon_AddsNewCartItem()
    {
        // Arrange
        var cart = new Cart();
        var item = new CartItem { mabienthe = 1, mamon = 1, tenmon = "Phở Bò", giatien = 45000, maquanan = 3 };

        // Act
        cart.themMon(item, 2);

        // Assert
        Assert.Single(cart.items);
        Assert.Equal(90000, cart.tongTien);
        Assert.Equal(2, cart.items[0].soLuong);
    }

    [Fact]
    public void ThemMon_SameItem_IncreasesQuantity()
    {
        // Arrange
        var cart = new Cart();
        var item = new CartItem { mabienthe = 1, mamon = 1, tenmon = "Phở Bò", giatien = 45000, maquanan = 3 };

        // Act
        cart.themMon(item, 1);
        cart.themMon(item, 2);

        // Assert
        Assert.Single(cart.items);
        Assert.Equal(3, cart.items[0].soLuong);
        Assert.Equal(135000, cart.tongTien);
    }

    [Fact]
    public void ThemMon_DifferentItems_KeepsSeparate()
    {
        // Arrange
        var cart = new Cart();
        var item1 = new CartItem { mabienthe = 1, mamon = 1, tenmon = "Phở Bò", giatien = 45000, maquanan = 3 };
        var item2 = new CartItem { mabienthe = 2, mamon = 2, tenmon = "Bún Chả", giatien = 40000, maquanan = 3 };

        // Act
        cart.themMon(item1, 1);
        cart.themMon(item2, 1);

        // Assert
        Assert.Equal(2, cart.items.Count);
        Assert.Equal(85000, cart.tongTien);
    }

    [Fact]
    public void XoaMon_RemovesExistingItem()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 2);

        // Act
        cart.xoaMon(1);

        // Assert
        Assert.Empty(cart.items);
        Assert.Equal(0, cart.tongTien);
    }

    [Fact]
    public void XoaMon_NonExistentItem_DoesNothing()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 2);

        // Act
        cart.xoaMon(999);

        // Assert
        Assert.Single(cart.items);
        Assert.Equal(90000, cart.tongTien);
    }

    [Fact]
    public void GiamMon_QuantityAboveOne_DecreasesQuantity()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 3);

        // Act
        cart.giamMon(1);

        // Assert
        Assert.Single(cart.items);
        Assert.Equal(2, cart.items[0].soLuong);
        Assert.Equal(90000, cart.tongTien);
    }

    [Fact]
    public void GiamMon_QuantityOne_RemovesItem()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 1);

        // Act
        cart.giamMon(1);

        // Assert
        Assert.Empty(cart.items);
        Assert.Equal(0, cart.tongTien);
    }

    [Fact]
    public void RestaurantIds_ReturnsUniqueIds()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 1);
        cart.themMon(new CartItem { mabienthe = 2, giatien = 40000, maquanan = 3 }, 1);
        cart.themMon(new CartItem { mabienthe = 3, giatien = 35000, maquanan = 5 }, 1);

        // Act
        var ids = cart.RestaurantIds;

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.Contains(3, ids);
        Assert.Contains(5, ids);
    }

    [Fact]
    public void CanAddFromRestaurant_EmptyCart_ReturnsTrue()
    {
        // Arrange
        var cart = new Cart();

        // Act & Assert
        Assert.True(cart.CanAddFromRestaurant(3));
    }

    [Fact]
    public void CanAddFromRestaurant_SameRestaurant_ReturnsTrue()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 1);

        // Act & Assert
        Assert.True(cart.CanAddFromRestaurant(3));
    }

    [Fact]
    public void CanAddFromRestaurant_DifferentRestaurant_ReturnsFalse()
    {
        // Arrange
        var cart = new Cart();
        cart.themMon(new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 }, 1);

        // Act & Assert
        Assert.False(cart.CanAddFromRestaurant(5));
    }

    [Fact]
    public void CanAddFromRestaurant_NullId_ReturnsFalse()
    {
        // Arrange
        var cart = new Cart();

        // Act & Assert
        Assert.False(cart.CanAddFromRestaurant(null));
    }

    [Fact]
    public void TongTien_CalculatesCorrectlyAcrossOperations()
    {
        // Arrange
        var cart = new Cart();
        var item1 = new CartItem { mabienthe = 1, giatien = 45000, maquanan = 3 };
        var item2 = new CartItem { mabienthe = 2, giatien = 55000, maquanan = 3 };

        // Act: Add 2 of item1 (90000), add 1 of item2 (55000) = 145000
        cart.themMon(item1, 2);
        cart.themMon(item2, 1);
        Assert.Equal(145000, cart.tongTien);

        // Reduce item1 by 1 => 1*45000 + 55000 = 100000
        cart.giamMon(1);
        Assert.Equal(100000, cart.tongTien);

        // Remove item2 => 45000
        cart.xoaMon(2);
        Assert.Equal(45000, cart.tongTien);

        // Add 3 more of item1 => 4*45000 = 180000
        cart.themMon(item1, 3);
        Assert.Equal(180000, cart.tongTien);
    }
}