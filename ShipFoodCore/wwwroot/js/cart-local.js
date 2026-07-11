/**
 * FastShip Cart Persistence - localStorage
 * Saves cart to localStorage on EVERY change (kể cả khi chưa login)
 * Restores cart on page load + sau khi login
 */

var CART_STORAGE_KEY = 'fastship_cart';
var CART_COUNT_KEY = 'fastship_cart_count';

// ─── Save cart to localStorage (luôn lưu, kể cả anonymous) ───
function saveCartToLocal(cartData) {
    try {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cartData));
        // Cập nhật badge count
        if (cartData && cartData.items) {
            var total = cartData.items.reduce(function(sum, i) { return sum + (i.soLuong || 0); }, 0);
            localStorage.setItem(CART_COUNT_KEY, total);
            updateCartBadge(total);
        }
    } catch (e) {
        console.warn('Failed to save cart to localStorage:', e);
    }
}

// ─── Save cart count separately (cho badge) ───
function saveCartCount(count) {
    try {
        localStorage.setItem(CART_COUNT_KEY, count);
        updateCartBadge(count);
    } catch (e) {}
}

// ─── Get cart count from localStorage ───
function getCartCount() {
    try {
        return parseInt(localStorage.getItem(CART_COUNT_KEY)) || 0;
    } catch (e) { return 0; }
}

// ─── Update cart badge trên navbar ───
function updateCartBadge(count) {
    // Tìm tất cả cart badges
    document.querySelectorAll('.fs-cart-badge, .cart-count-badge').forEach(function(el) {
        if (count > 0) {
            el.textContent = count > 99 ? '99+' : count;
            el.style.display = 'flex';
        } else {
            el.style.display = 'none';
        }
    });
    
    // Fallback: tìm cart link và thêm badge nếu chưa có
    var cartLinks = document.querySelectorAll('a[href="/Cart"], a[href$="/Cart/Index"]');
    cartLinks.forEach(function(link) {
        var existingBadge = link.querySelector('.fs-cart-badge');
        if (!existingBadge && count > 0) {
            var badge = document.createElement('span');
            badge.className = 'fs-cart-badge';
            badge.style.cssText = 'position:absolute;top:-6px;right:-6px;background:#dc3545;color:#fff;width:20px;height:20px;border-radius:50%;font-size:11px;display:flex;align-items:center;justify-content:center;font-weight:700;';
            badge.textContent = count > 99 ? '99+' : count;
            link.style.position = 'relative';
            link.appendChild(badge);
        } else if (existingBadge) {
            if (count > 0) {
                existingBadge.textContent = count > 99 ? '99+' : count;
                existingBadge.style.display = 'flex';
            } else {
                existingBadge.style.display = 'none';
            }
        }
    });
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
        localStorage.removeItem(CART_COUNT_KEY);
        updateCartBadge(0);
    } catch (e) {
        console.warn('Failed to clear cart from localStorage:', e);
    }
}

// ─── Sync: localStorage → server session (sau khi login) ───
function syncCartFromLocal() {
    var localCart = loadCartFromLocal();
    if (!localCart || !localCart.items || localCart.items.length === 0) {
        // Vẫn update badge từ count
        updateCartBadge(getCartCount());
        return;
    }

    // Nếu đã login → restore lên server
    var hasAuthCookie = document.cookie.indexOf('.AspNetCore.Cookies') !== -1;
    if (!hasAuthCookie) {
        updateCartBadge(getCartCount());
        return; // Chưa login, chỉ hiển thị badge
    }

    // Check if session cart is empty, then restore from local
    var hasSessionCart = document.querySelector('#cart-items-container') !== null;
    if (hasSessionCart) {
        // Session already has cart, prefer it
        updateCartBadge(getCartCount());
        return;
    }

    // Try to restore via API
    var restoreUrl = '/Cart/RestoreFromLocal';
    fetch(restoreUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name=\"__RequestVerificationToken\"]')?.value || ''
        },
        body: JSON.stringify(localCart)
    })
    .then(function(r) { return r.json(); })
    .then(function(res) {
        if (res.justOrdered) {
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
function patchCartActions() {
    if (typeof $ === 'undefined') return;

    $(document).ajaxSuccess(function(event, xhr, settings) {
        var url = settings.url || '';
        if (url.indexOf('ApiTangSoLuong') === -1 &&
            url.indexOf('ApiGiamSoLuong') === -1 &&
            url.indexOf('ApiThemMonAn') === -1 &&
            url.indexOf('ApiForceSwitchRestaurant') === -1 &&
            url.indexOf('ApiAddToCartUnauth') === -1) {
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

// ─── Thêm món vào localStorage khi chưa login (Grab-style) ───
function addToCartUnauth(maMonAn, soLuong, tenmon, giatien, hinhanh, maquanan) {
    var cart = loadCartFromLocal() || { items: [], tongTien: 0 };
    
    // Tìm item có cùng mamon
    var existed = false;
    cart.items.forEach(function(i) {
        if (i.mamon === maMonAn) {
            i.soLuong = (i.soLuong || 0) + soLuong;
            existed = true;
        }
    });
    
    if (!existed) {
        cart.items.push({
            mamon: maMonAn,
            tenmon: tenmon || 'Món ăn',
            giatien: giatien || 0,
            soLuong: soLuong,
            hinhanh: hinhanh || '',
            maquanan: maquanan || null
        });
    }
    
    // Tính lại tổng tiền
    cart.tongTien = cart.items.reduce(function(sum, i) { return sum + (i.giatien || 0) * (i.soLuong || 0); }, 0);
    
    saveCartToLocal(cart);
    
    // Toast thông báo
    showCartToast('success', '✅ Đã thêm vào giỏ hàng!');
    
    return cart;
}

// ─── Toast notification ───
function showCartToast(type, msg) {
    var existing = document.querySelector('.fs-cart-toast');
    if (existing) existing.remove();
    
    var color = type === 'error' ? '#dc3545' : '#28a745';
    var toast = document.createElement('div');
    toast.className = 'fs-cart-toast';
    toast.style.cssText = 'position:fixed;bottom:24px;right:24px;z-index:99999;background:' + color + ';color:#fff;padding:14px 20px;border-radius:12px;box-shadow:0 4px 20px rgba(0,0,0,.2);font-size:14px;max-width:360px;animation:fsToastIn .3s ease;display:flex;align-items:center;gap:10px;';
    toast.innerHTML = msg;
    document.body.appendChild(toast);
    setTimeout(function(){ toast.style.transition = 'opacity .3s'; toast.style.opacity = '0'; setTimeout(function(){ toast.remove(); }, 300); }, 2500);
}

// ─── Initialize ───
document.addEventListener('DOMContentLoaded', function() {
    // Khởi tạo badge từ localStorage
    var count = getCartCount();
    updateCartBadge(count);
    
    // Trên trang Cart/Checkout → sync với server nếu đã login
    var isCartPage = window.location.pathname.toLowerCase().includes('/cart');
    var isCheckoutPage = window.location.pathname.toLowerCase().includes('/checkout');
    var isLoginPage = window.location.pathname.toLowerCase().includes('/login');
    var isSignupPage = window.location.pathname.toLowerCase().includes('/signup');
    
    if (isCartPage || isCheckoutPage) {
        syncCartFromLocal();
    }
    
    // Nếu vừa login → restore cart từ localStorage
    if (isLoginPage || isSignupPage) {
        // Khi chuyển trang, syncCartFromLocal sẽ tự chạy
    }

    patchCartActions();
    
    // Inject CSS animation
    if (!document.getElementById('fs-cart-anim-style')) {
        var style = document.createElement('style');
        style.id = 'fs-cart-anim-style';
        style.textContent = '@keyframes fsToastIn { 0% { opacity: 0; transform: translateY(20px); } 100% { opacity: 1; transform: translateY(0); } }';
        document.head.appendChild(style);
    }
});

// ─── Export ───
window.FastShipCart = {
    save: saveCartToLocal,
    load: loadCartFromLocal,
    clear: clearCartLocal,
    sync: syncCartFromLocal,
    addUnauth: addToCartUnauth,
    getCount: getCartCount,
    updateBadge: updateCartBadge
};
