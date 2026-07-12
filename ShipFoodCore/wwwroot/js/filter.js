/**
 * FastShip Filter Bar - JavaScript
 * Dual-Filter Bar with Bottom Sheet (Grab-like UI/UX)
 * Supports two-way sync between chips and sheet
 */

// ─── HinhAnhUrl helper (matches TinhToan.HinhAnhUrl in C#) ───
function hinhAnhUrl(hinhanh) {
    if (!hinhanh) return '/Source/Home/img/food-placeholder.png';
    if (hinhanh.indexOf('http://') === 0 || hinhanh.indexOf('https://') === 0) return hinhanh;
    return '/Source/images/MonAn/' + hinhanh;
}

// ─── State ───
var filterState = {
    categoryId: null,
    sortBy: 'suggest',
    isPromo: false,
    isBestSeller: false,
    isNearMe: false,
    maxPriceLevel: '',
    maxDiet: '',
    mode: 'delivery',
    q: ''
};

// ─── Bottom Sheet ───
function openFilterSheet() {
    document.getElementById('filterSheetOverlay').classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeFilterSheet(e) {
    if (e && e.target !== e.currentTarget) return;
    document.getElementById('filterSheetOverlay').classList.remove('open');
    document.body.style.overflow = '';
}

// ─── Chips ───
function toggleChip(name) {
    var chip = document.querySelector('.fs-chip[data-filter="' + name + '"]');
    if (!chip) return;

    chip.classList.toggle('active');
    var isActive = chip.classList.contains('active');

    // Sync state
    switch (name) {
        case 'promo':
            filterState.isPromo = isActive;
            syncSheetCheckbox('promo', isActive);
            break;
        case 'bestseller':
            filterState.isBestSeller = isActive;
            syncSheetCheckbox('bestseller', isActive);
            break;
        case 'nearme':
            filterState.isNearMe = isActive;
            break;
        case 'quick':
            // Quick filter: just a chip, no special state
            break;
        case 'rated':
            // Toggle rating filter
            break;
        case 'mode':
            filterState.mode = isActive ? 'pickup' : 'delivery';
            syncSheetRadio('filterMode', filterState.mode);
            break;
    }

    updateActiveBadge();
    triggerSearch();
}

// ─── Bottom Sheet ↔ Chip Sync ───
function syncSheetCheckbox(name, checked) {
    var sheetCheckbox = document.querySelector('.fs-sheet-body input[type="checkbox"]');
    if (!sheetCheckbox) return;
    // Find the right checkbox
    var checkboxes = document.querySelectorAll('.fs-sheet-body input[type="checkbox"]');
    checkboxes.forEach(function(cb) {
        var label = cb.closest('.fs-checkbox');
        if (!label) return;
        var text = label.textContent.trim();
        if ((name === 'promo' && text.includes('khuyến mãi')) ||
            (name === 'bestseller' && text.includes('Bán chạy'))) {
            cb.checked = checked;
            if (checked) label.classList.add('active');
            else label.classList.remove('active');
        }
    });
}

function syncSheetRadio(name, value) {
    var radios = document.querySelectorAll('.fs-sheet-body input[name="' + name + '"]');
    radios.forEach(function(r) {
        var label = r.closest('.fs-radio');
        if (r.value === value) {
            r.checked = true;
            if (label) label.classList.add('active');
        } else {
            r.checked = false;
            if (label) label.classList.remove('active');
        }
    });
}

function toggleSheetCheckbox(name, el) {
    var label = el.closest('.fs-checkbox');
    if (el.checked) label.classList.add('active');
    else label.classList.remove('active');

    // Sync to chip
    var chipName = name;
    if (name === 'vegetarian') chipName = 'vegetarian'; // no chip for this

    var chip = document.querySelector('.fs-chip[data-filter="' + chipName + '"]');
    if (chip) {
        if (el.checked) chip.classList.add('active');
        else chip.classList.remove('active');
    }

    // Update state
    if (name === 'promo') filterState.isPromo = el.checked;
    if (name === 'bestseller') filterState.isBestSeller = el.checked;
}

// ─── Price Level ───
function selectPriceLevel(value) {
    document.querySelectorAll('.fs-price-btn').forEach(function(btn) {
        if (btn.dataset.value === value) btn.classList.add('active');
        else btn.classList.remove('active');
    });
    filterState.maxPriceLevel = value;
}

// ─── Category ───
function selectCategory(value) {
    document.querySelectorAll('.fs-cat-btn').forEach(function(btn) {
        if (btn.dataset.value === String(value)) btn.classList.add('active');
        else btn.classList.remove('active');
    });
    filterState.categoryId = value ? parseInt(value) : null;
    updateActiveBadge();
}

// ─── Filter Management ───
function updateFilter() {
    // Read radio states
    var modeRadio = document.querySelector('input[name="filterMode"]:checked');
    if (modeRadio) filterState.mode = modeRadio.value;

    var sortRadio = document.querySelector('input[name="sortBy"]:checked');
    if (sortRadio) filterState.sortBy = sortRadio.value;

    updateActiveBadge();
}

function updateActiveBadge() {
    var count = 0;
    if (filterState.isPromo) count++;
    if (filterState.isBestSeller) count++;
    if (filterState.isNearMe) count++;
    if (filterState.categoryId) count++;
    if (filterState.maxPriceLevel) count++;
    if (filterState.sortBy !== 'suggest') count++;

    var badge = document.getElementById('filterActiveBadge');
    if (count > 0) {
        badge.textContent = count;
        badge.style.display = 'flex';
    } else {
        badge.style.display = 'none';
    }
}

function resetFilters() {
    filterState = {
        categoryId: null,
        sortBy: 'suggest',
        isPromo: false,
        isBestSeller: false,
        isNearMe: false,
        maxPriceLevel: '',
        maxDiet: '',
        mode: 'delivery',
        q: filterState.q
    };

    // Reset UI
    document.querySelectorAll('.fs-chip.active').forEach(function(c) { c.classList.remove('active'); });
    document.querySelectorAll('.fs-radio.active').forEach(function(r) { r.classList.remove('active'); });
    document.querySelectorAll('.fs-checkbox.active').forEach(function(c) { c.classList.remove('active'); });
    document.querySelectorAll('.fs-price-btn.active').forEach(function(b) { b.classList.remove('active'); });
    document.querySelectorAll('.fs-cat-btn.active').forEach(function(b) { b.classList.remove('active'); });

    // Set default radio
    var defaultMode = document.querySelector('input[name="filterMode"][value="delivery"]');
    if (defaultMode) { defaultMode.checked = true; defaultMode.closest('.fs-radio').classList.add('active'); }
    var defaultSort = document.querySelector('input[name="sortBy"][value="suggest"]');
    if (defaultSort) { defaultSort.checked = true; defaultSort.closest('.fs-radio').classList.add('active'); }

    updateActiveBadge();
    triggerSearch();
}

function applyFilters() {
    // Cập nhật filter state từ sheet (bao gồm sync chips)
    syncChipsFromSheet();
    updateFilter();
    closeFilterSheet();
    triggerSearch();
}

// ─── Search Trigger ───
function triggerSearch() {
    var params = new URLSearchParams();

    if (filterState.q) params.set('q', filterState.q);
    if (filterState.categoryId) params.set('categoryId', filterState.categoryId);
    if (filterState.sortBy !== 'suggest') params.set('sortBy', filterState.sortBy);
    if (filterState.isPromo) params.set('isPromo', 'true');
    if (filterState.isBestSeller) params.set('isBestSeller', 'true');
    if (filterState.isNearMe) params.set('isNearMe', 'true');
    if (filterState.maxPriceLevel) params.set('maxPriceLevel', filterState.maxPriceLevel);
    if (filterState.maxDiet) params.set('maxDiet', filterState.maxDiet);
    if (filterState.mode !== 'delivery') params.set('mode', filterState.mode);

    var qs = params.toString();
    var url = qs ? '/Home/MenuSearch?' + qs : '/Home/MenuSearch';

    // Fetch results via AJAX
    fetch(url, {
        headers: { 'Accept': 'application/json' }
    })
    .then(function(r) { return r.json(); })
    .then(function(data) {
        renderSearchResults(data);
    })
    .catch(function(err) {
        console.error('Filter search failed:', err);
    });
}

// ─── Sync filter chips with bottom sheet state ───
function syncChipsFromSheet() {
    // Sync mode radio
    var modeRadio = document.querySelector('input[name="filterMode"]:checked');
    if (modeRadio) {
        filterState.mode = modeRadio.value;
        var modeChip = document.querySelector('.fs-chip[data-filter="mode"]');
        if (modeChip) {
            if (filterState.mode === 'pickup') modeChip.classList.add('active');
            else modeChip.classList.remove('active');
        }
    }

    // Sync sort
    var sortRadio = document.querySelector('input[name="sortBy"]:checked');
    if (sortRadio) filterState.sortBy = sortRadio.value;

    // Sync checkboxes
    document.querySelectorAll('.fs-sheet-body input[type="checkbox"]').forEach(function(cb) {
        var label = cb.closest('.fs-checkbox');
        if (!label) return;
        var text = label.textContent.trim();
        if (text.includes('khuyến mãi') || text.includes('Khuyến mãi')) {
            filterState.isPromo = cb.checked;
            var promo = document.querySelector('.fs-chip[data-filter="promo"]');
            if (promo) { if (cb.checked) promo.classList.add('active'); else promo.classList.remove('active'); }
        }
        if (text.includes('Bán chạy')) {
            filterState.isBestSeller = cb.checked;
            var best = document.querySelector('.fs-chip[data-filter="bestseller"]');
            if (best) { if (cb.checked) best.classList.add('active'); else best.classList.remove('active'); }
        }
    });

    // Sync price level
    var activePrice = document.querySelector('.fs-price-btn.active');
    if (activePrice) filterState.maxPriceLevel = activePrice.dataset.value;
    else filterState.maxPriceLevel = '';

    // Sync category
    var activeCat = document.querySelector('.fs-cat-btn.active');
    if (activeCat) {
        var val = activeCat.dataset.value;
        filterState.categoryId = val ? parseInt(val) : null;
    }

    updateActiveBadge();
}

// ─── Render Results ───
function renderSearchResults(data) {
    var container = document.getElementById('searchResults');
    if (!container) return;

    if (!data || !data.items || data.items.length === 0) {
        container.innerHTML = '<div class="fs-empty-search"><i class="fa fa-search"></i><h5>Không tìm thấy món ăn phù hợp</h5><p class="text-muted">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p></div>';
        return;
    }

    var html = '<div class="row g-3" id="menuSearchResults">';
    data.items.forEach(function(item) {
        // ponytail: use hinhAnhUrl() to handle full URLs and relative paths (fix mất ảnh khi lọc)
        var imgSrc = hinhAnhUrl(item.hinhanh);
        var rating = item.avgRating ? '⭐ ' + item.avgRating.toFixed(1) : '⭐ Mới';
        var priceLabel = item.giaMin ? (item.giaMax && item.giaMax !== item.giaMin ? item.giaMin.toLocaleString('vi-VN') + ' - ' + item.giaMax.toLocaleString('vi-VN') : item.giaMin.toLocaleString('vi-VN')) : '';

        html += '<div class="col-xl-3 col-lg-4 col-md-4 col-6">';
        html += '<a href="/Home/DetailRestaurant/' + item.maquanan + '" class="text-decoration-none">';
        html += '<div class="product-item">';
        html += '<div class="product-img-wrap">';
        html += '<img src="' + imgSrc + '" alt="' + item.tenmon + '" loading="lazy" onerror="this.src=\'/Source/Home/img/food-placeholder.png\'">';
        if (item.isPromo) html += '<div class="fs-order-count-badge" style="background:#e74c3c;">🔥 Giảm</div>';
        html += '</div>';
        html += '<div class="product-body">';
        html += '<div class="product-title">' + item.tenmon + '</div>';
        html += '<div class="product-address"><i class="fa fa-store me-1 text-green"></i>' + (item.tenquanan || '') + '</div>';
        if (item.sizes && item.sizes.length > 0) {
            html += '<div class="mt-1">';
            item.sizes.forEach(function(s) {
                html += '<span class="badge bg-light text-dark me-1 border">' + (s.size || 'Mặc định') + ': ' + s.giatien.toLocaleString('vi-VN') + 'đ</span>';
            });
            html += '</div>';
        }
        html += '</div>';
        html += '<div class="product-footer">';
        html += '<div class="pf-cell"><i class="fa fa-star"></i> ' + rating + '</div>';
        html += '<div class="pf-cell"><span class="text-muted">' + priceLabel + '</span></div>';
        html += '</div>';
        html += '</div></a></div>';
    });
    html += '</div>';

    container.innerHTML = html;

    // Update count
    var countEl = document.getElementById('searchResultCount');
    if (countEl) countEl.textContent = data.total || data.items.length;
}

// ─── Initialize on DOM ready ───
// ponytail: CHỈ giữ các event listeners KHÔNG xung đột với HTML onclick attributes
// Các onclick/onchange trong Default.cshtml đã xử lý: toggleChip, closeFilterSheet,
// selectPriceLevel, selectCategory, updateFilter, toggleSheetCheckbox, resetFilters, applyFilters
// → KHÔNG thêm addEventListener trùng lặp
// → applyFilters() đã tích hợp syncChipsFromSheet() bên trong
document.addEventListener('DOMContentLoaded', function() {
    // Close sheet on Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            var sheet = document.getElementById('filterSheetOverlay');
            if (sheet.classList.contains('open')) closeFilterSheet();
        }
    });

    // Prevent sheet close when clicking inside sheet
    var sheet = document.getElementById('filterSheet');
    if (sheet) {
        sheet.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    }

    // Load initial filter state from URL params
    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('isPromo') === 'true') filterState.isPromo = true;
    if (urlParams.get('isBestSeller') === 'true') filterState.isBestSeller = true;
    if (urlParams.get('categoryId')) filterState.categoryId = parseInt(urlParams.get('categoryId'));
});
