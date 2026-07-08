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
        if (res.justOrdered) {
            // User vừa đặt hàng thành công trên thiết bị khác → xóa localStorage cũ
            clearCartLocal();
            return;
        }
        if (res.success && res.redirect) {
            window.location.href = res.redirect;
        }
    })
    .catch(function(err) {
        console.warn('Cart restore failed:', err);
    });
}

// ─── Helper: trích xuất giỏ hàng từ DOM (dùng chung) ───
function extractCartFromDOM() {
    var cartTotalEl = document.querySelector('#cart-total');
    if (!cartTotalEl) return null;

    var data = {
        tongTien: parseFloat(cartTotalEl.textContent.replace(/[^0-9]/g, '')) || 0,
        items: []
    };

    document.querySelectorAll('.cart-item').forEach(function(item) {
        var name = item.querySelector('.item-name')?.textContent?.trim() || '';
        var priceText = item.querySelector('.item-price')?.textContent?.trim() || '0';
        var price = parseFloat(priceText.replace(/[^0-9]/g, '')) || 0;
        var qtyText = item.querySelector('.qty-num')?.textContent?.trim() || '0';
        var qty = parseInt(qtyText) || 0;
        var mamon = item.querySelector('[data-mamon]')?.dataset?.mamon;
        var img = item.querySelector('img')?.getAttribute('src') || '';

        if (mamon) {
            data.items.push({
                mamon: parseInt(mamon),
                tenmon: name,
                giatien: price,
                soLuong: qty,
                hinhanh: img
            });
        }
    });

    return data;
}

// ─── Save cart to localStorage after cart API calls ───
// Dùng jQuery ajaxSuccess (có filter) thay vì monkey-patch XHR.prototype.open
// giúp tránh memory leak và ảnh hưởng đến các AJAX request khác
function patchCartActions() {
    if (typeof $ === 'undefined') return;

    $(document).ajaxSuccess(function(event, xhr, settings) {
        var url = settings.url || '';
        // Chỉ xử lý khi là API cart
        if (url.indexOf('ApiTangSoLuong') === -1 &&
            url.indexOf('ApiGiamSoLuong') === -1 &&
            url.indexOf('ApiThemMonAn') === -1 &&
            url.indexOf('ApiForceSwitchRestaurant') === -1) {
            return;
        }

        var cartData = extractCartFromDOM();
        if (cartData && (cartData.items.length > 0 || cartData.tongTien > 0)) {
            saveCartToLocal(cartData);
        } else {
            clearCartLocal();
        }
    });
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
