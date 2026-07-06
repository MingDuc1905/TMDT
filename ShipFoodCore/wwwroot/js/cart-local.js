/**
 * FastShip Cart Persistence - localStorage
 * Saves cart to localStorage on every change
 * Restores cart on page load
 */

var CART_STORAGE_KEY = 'fastship_cart';

// ─── Save cart to localStorage ───
function saveCartToLocal(cartData) {
    try {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cartData));
    } catch (e) {
        console.warn('Failed to save cart to localStorage:', e);
    }
}

// ─── Kiểm tra trạng thái đăng nhập ───
// Kiểm tra qua cookie ASP.NET + session indicator trong DOM
function isUserLoggedIn() {
    // Ưu tiên kiểm tra cookie ASP.NET Authentication
    // Chỉ check .AspNetCore.Cookies (không check .Session vì session cookie tồn tại cho cả visitor chưa login)
    var hasAuthCookie = document.cookie.indexOf('.AspNetCore.Cookies') !== -1;
    
    // Fallback: kiểm tra các element DOM chỉ hiện khi đã login
    var hasUserElement = document.querySelector('#user-info') !== null
        || document.querySelector('.user-avatar') !== null
        || document.querySelector('.nav-user-dropdown') !== null;
    
    return hasAuthCookie || hasUserElement;
}

// ─── Load cart from localStorage ───
function loadCartFromLocal() {
    try {
        var data = localStorage.getItem(CART_STORAGE_KEY);
        return data ? JSON.parse(data) : null;
    } catch (e) {
        console.warn('Failed to load cart from localStorage:', e);
        return null;
    }
}

// ─── Clear cart from localStorage ───
function clearCartLocal() {
    try {
        localStorage.removeItem(CART_STORAGE_KEY);
    } catch (e) {
        console.warn('Failed to clear cart from localStorage:', e);
    }
}

// ─── Sync: localStorage → session (on page load) ───
function syncCartFromLocal() {
    var localCart = loadCartFromLocal();
    if (!localCart || !localCart.items || localCart.items.length === 0) return;

    // ⚠️ Mục 3: Kiểm tra trạng thái đăng nhập trước khi restore
    // Nếu user đã checkout ở thiết bị khác, xóa localStorage cũ
    if (!isUserLoggedIn()) {
        clearCartLocal();
        return;
    }

    // Check if session cart is empty, then restore from local
    var hasSessionCart = document.querySelector('#cart-items-container') !== null;
    if (hasSessionCart) {
        // Session already has cart, prefer it
        return;
    }

    // Try to restore via API
    var restoreUrl = '/Cart/RestoreFromLocal';
    fetch(restoreUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify(localCart)
    })
    .then(function(r) { return r.json(); })
    .then(function(res) {
        if (res.success && res.redirect) {
            window.location.href = res.redirect;
        }
    })
    .catch(function(err) {
        console.warn('Cart restore failed:', err);
    });
}

// ─── Monkey-patch cart actions to also save to localStorage ───
function patchCartActions() {
    var origOpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function() {
        this.addEventListener('load', function() {
            // After any cart API call, save current cart state to localStorage
            var cartTotalEl = document.querySelector('#cart-total');
            if (cartTotalEl) {
                var cartData = {
                    tongTien: parseFloat(cartTotalEl.textContent.replace(/[^0-9]/g, '')) || 0,
                    items: []
                };
                // Parse items from DOM
                document.querySelectorAll('.cart-item').forEach(function(item) {
                    var name = item.querySelector('.item-name')?.textContent?.trim() || '';
                    var priceText = item.querySelector('.item-price')?.textContent?.trim() || '0';
                    var price = parseFloat(priceText.replace(/[^0-9]/g, '')) || 0;
                    var qtyText = item.querySelector('.qty-num')?.textContent?.trim() || '0';
                    var qty = parseInt(qtyText) || 0;
                    var mamon = item.querySelector('[data-mamon]')?.dataset?.mamon;
                    var img = item.querySelector('img')?.getAttribute('src') || '';

                    if (mamon) {
                        cartData.items.push({
                            mamon: parseInt(mamon),
                            tenmon: name,
                            giatien: price,
                            soLuong: qty,
                            hinhanh: img
                        });
                    }
                });

                if (cartData.items.length > 0 || cartData.tongTien > 0) {
                    saveCartToLocal(cartData);
                }
            }
        });
        origOpen.apply(this, arguments);
    };
}

// ─── Initialize ───
document.addEventListener('DOMContentLoaded', function() {
    // Try to restore cart from localStorage on load
    // Only if we're on cart-related pages
    var isCartPage = window.location.pathname.toLowerCase().includes('/cart');
    var isCheckoutPage = window.location.pathname.toLowerCase().includes('/checkout');

    if (isCartPage || isCheckoutPage) {
        syncCartFromLocal();
    }

    // Patch cart saves
    patchCartActions();
});

// ─── Export for use in other scripts ───
window.FastShipCart = {
    save: saveCartToLocal,
    load: loadCartFromLocal,
    clear: clearCartLocal,
    sync: syncCartFromLocal
};
