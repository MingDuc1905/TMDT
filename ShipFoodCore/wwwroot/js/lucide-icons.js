/**
 * Lucide Icons Helper — Lightweight SVG icon system
 * Uses Lucide Static SVG from unpkg CDN
 * 
 * Usage:
 *   <i data-lucide="search" class="icon-sm"></i>
 *   or: <span class="li-search"></span>
 * 
 * After DOM change: LucideIcons.load() to re-render
 */
(function() {
    'use strict';

    const BASE_URL = 'https://unpkg.com/lucide-static@latest/icons/';
    
    // Icon name mapping (FA class → Lucide name)
    const ICON_MAP = {
        // Navigation & Actions
        'search': 'search',
        'shopping-bag': 'shopping-bag',
        'shopping-cart': 'shopping-cart',
        'user': 'user',
        'user-plus': 'user-plus',
        'sign-in-alt': 'log-in',
        'sign-out-alt': 'log-out',
        'times': 'x',
        'close': 'x',
        'check': 'check',
        'check-circle': 'check-circle',
        'exclamation-circle': 'alert-circle',
        'info-circle': 'info-circle',
        'question-circle': 'help-circle',
        'plus': 'plus',
        'minus': 'minus',
        'plus-circle': 'plus-circle',
        'minus-circle': 'minus-circle',
        'arrow-right': 'arrow-right',
        'arrow-left': 'arrow-left',
        'arrow-up': 'arrow-up',
        'arrow-down': 'arrow-down',
        'chevron-right': 'chevron-right',
        'chevron-left': 'chevron-left',
        'chevron-up': 'chevron-up',
        'chevron-down': 'chevron-down',
        'angle-up': 'chevron-up',
        'angle-down': 'chevron-down',
        'angle-right': 'chevron-right',
        'angle-left': 'chevron-left',
        'redo': 'refresh-cw',
        'sync-alt': 'refresh-cw',
        'spinner': 'loader',
        'eye': 'eye',
        'eye-slash': 'eye-off',
        
        // Location & Contact
        'map-marker-alt': 'map-pin',
        'map-pin': 'map-pin',
        'location-arrow': 'navigation',
        'phone': 'phone',
        'phone-alt': 'phone',
        'envelope': 'mail',
        'clock': 'clock',
        'calendar': 'calendar',
        
        // Food & Restaurant
        'store': 'store',
        'utensils': 'utensils-crossed',
        'coffee': 'coffee',
        'motorcycle': 'bike',
        'truck': 'truck',
        'shipping-fast': 'truck',
        'box-open': 'package',
        
        // Payments & Money
        'wallet': 'wallet',
        'credit-card': 'credit-card',
        'money-bill-wave': 'banknote',
        'receipt': 'receipt',
        'tag': 'tag',
        'tags': 'tags',
        'university': 'landmark',
        'flask': 'flask-conical',
        'star': 'star',
        'fire': 'flame',
        'bolt': 'zap',
        
        // UI Elements
        'heart': 'heart',
        'comment': 'message-square',
        'comment-alt': 'message-circle',
        'comments': 'message-circle',
        'comment-dots': 'message-circle',
        'share': 'share-2',
        'reply': 'reply',
        'paper-plane': 'send',
        'search-minus': 'search-minus',
        'thumbs-up': 'thumbs-up',
        'thumbs-down': 'thumbs-down',
        'history': 'clock',
        
        // Objects
        'gift': 'gift',
        'shield-alt': 'shield',
        'lock': 'lock',
        'bookmark': 'bookmark',
        'sticky-note': 'sticky-note',
        'copy': 'copy',
        'clipboard': 'clipboard',
        'inbox': 'inbox',
        'compass': 'compass',
        
        // Chart & Stats
        'chart-bar': 'bar-chart-3',
        'chart-pie': 'pie-chart',
        'chart-line': 'trending-up',
        'chart-area': 'area-chart',
        
        // Media
        'image': 'image',
        'camera': 'camera',
        'play': 'play',
        'pause': 'pause',
        'video': 'video',
        
        // Social (keep brand icons as-is, they don't have Lucide equivalents)
        'facebook-f': 'facebook',
        'facebook': 'facebook',
        'instagram': 'instagram',
        'twitter': 'twitter',
        'youtube': 'youtube',
        'tiktok': 'music',
        'facebook-messenger': 'message-circle',
        
        // Misc
        'home': 'home',
        'menu': 'menu',
        'more-horizontal': 'more-horizontal',
        'more-vertical': 'more-vertical',
        'settings': 'settings',
        'cog': 'settings',
        'trash': 'trash-2',
        'edit': 'edit-3',
        'pencil-alt': 'edit-3',
        'filter': 'filter',
        'sliders-h': 'sliders',
        'sort': 'arrow-up-down',
        'lightbulb': 'lightbulb',
        'bell': 'bell',
        'bell-slash': 'bell-off',
        'flag': 'flag',
        'globe': 'globe',
        'link': 'link',
        'external-link-alt': 'external-link',
        'download': 'download',
        'upload': 'upload',
        'print': 'printer',
        'robot': 'bot',
        'box': 'package',
        'tv': 'monitor',
        'mobile-alt': 'smartphone',
        'laptop': 'laptop',
        
        // File types
        'file': 'file',
        'file-alt': 'file-text',
        'file-pdf': 'file-text',
        'file-image': 'file-image',
        
        // Data & Analytics
        'database': 'database',
        'server': 'server',
        'cloud': 'cloud',
        'cloud-upload-alt': 'cloud-upload',
        'cloud-download-alt': 'cloud-download',
        
        // Misc extra
        'award': 'award',
        'badge': 'badge-check',
        'certificate': 'certificate',
        'crown': 'crown',
        'dollar-sign': 'dollar-sign',
        'percentage': 'percent',
        'ruler-combined': 'ruler',
        'palette': 'palette',
        'code': 'code',
        'terminal': 'terminal',
        'wifi': 'wifi',
        'battery-full': 'battery-full',
        'battery-half': 'battery-half',
        'battery-empty': 'battery-empty',
        'sun': 'sun',
        'moon': 'moon',
        'check-double': 'check-check',
    };

    // Aliases with 'fa-' prefix
    for (var key in ICON_MAP) {
        if (ICON_MAP.hasOwnProperty(key)) {
            var aliased = 'fa-' + key;
            if (!ICON_MAP[aliased]) {
                ICON_MAP[aliased] = ICON_MAP[key];
            }
        }
    }

    // Size presets
    const SIZE_MAP = {
        'icon-xs': 12,
        'icon-sm': 16,
        'icon-md': 20,
        'icon-lg': 24,
        'icon-xl': 32,
        'icon-2xl': 40,
        'fa-xs': 12,
        'fa-sm': 14,
        'fa-lg': 20,
        'fa-2x': 24,
        'fa-3x': 32,
        'fa-4x': 40,
        'fa-5x': 48,
    };

    /**
     * Fetch SVG content from Lucide CDN
     */
    function fetchIcon(name) {
        if (!name) return null;
        var mapped = ICON_MAP[name.toLowerCase()] || name;
        return fetch(BASE_URL + mapped + '.svg')
            .then(function(r) { return r.ok ? r.text() : null; })
            .catch(function() { return null; });
    }

    /**
     * Get icon size from element classes
     */
    function getSize(el) {
        var classes = el.className || '';
        for (var cls in SIZE_MAP) {
            if (classes.indexOf(cls) !== -1) {
                return SIZE_MAP[cls];
            }
        }
        return 18; // default size
    }

    /**
     * Get stroke color from element
     */
    function getColor(el) {
        var color = el.getAttribute('data-color');
        if (color) return color;
        
        // Try to get from style or computed
        var style = el.getAttribute('style');
        if (style) {
            var m = style.match(/color\s*:\s*([^;]+)/i);
            if (m) return m[1].trim();
        }
        
        return 'currentColor';
    }

    /**
     * Insert SVG icon into element
     */
    function renderIcon(el) {
        var name = el.getAttribute('data-lucide') || el.className.match(/li-(\w+)/)?.[1];
        // Also check fa-* classes
        if (!name) {
            var classes = el.className || '';
            for (var i = 0; i < classes.split(' ').length; i++) {
                var cls = classes.split(' ')[i];
                if (cls.indexOf('fa-') === 0 || cls.indexOf('fas fa-') === 0) {
                    var iconName = cls.replace('fa-', '').replace('fas ', '').trim().replace('fa-', '');
                    if (ICON_MAP[iconName]) {
                        name = ICON_MAP[iconName];
                        break;
                    }
                }
            }
        }
        
        if (!name) return;
        
        var mapped = ICON_MAP[name.toLowerCase()] || name;
        var size = getSize(el);
        var color = getColor(el);
        
        // Inline SVG svg
        var svg = '<svg xmlns="http://www.w3.org/2000/svg" ' +
            'width="' + size + '" height="' + size + '" ' +
            'viewBox="0 0 24 24" fill="none" stroke="' + color + '" ' +
            'stroke-width="2" stroke-linecap="round" stroke-linejoin="round" ' +
            'class="lucide lucide-' + mapped + '">';
        
        // Try fetching actual SVG path from CDN, fallback to inline
        var self = el;
        fetchIcon(name).then(function(svgContent) {
            if (svgContent) {
                // Extract path data from SVG
                var parser = new DOMParser();
                var doc = parser.parseFromString(svgContent, 'image/svg+xml');
                var paths = doc.querySelectorAll('path, circle, rect, line, polyline, polygon');
                var inner = '';
                paths.forEach(function(p) {
                    inner += p.outerHTML;
                });
                if (inner) {
                    self.innerHTML = svg + inner + '</svg>';
                    return;
                }
            }
            // Fallback: use generic SVG with icon data
            self.innerHTML = getFallbackSvg(mapped, size, color);
        });
    }

    /**
     * Fallback SVG for common icons (rendered inline, no network needed)
     */
    function getFallbackSvg(name, size, color) {
        var basicPaths = {
            'search': '<circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>',
            'shopping-bag': '<path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 01-8 0"/>',
            'map-pin': '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/>',
            'star': '<polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>',
            'store': '<path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>',
            'bike': '<circle cx="5.5" cy="17.5" r="3.5"/><circle cx="18.5" cy="17.5" r="3.5"/><path d="M15 6a1 1 0 100-2 1 1 0 000 2zm-3 11.5V14l-3-3 4-3 2 3h3"/>',
            'user': '<path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/>',
            'tag': '<path d="M20.59 13.41l-7.17 7.17a2 2 0 01-2.83 0L2 12V2h10l8.59 8.59a2 2 0 010 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/>',
            'clock': '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>',
            'phone': '<path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z"/>',
            'mail': '<path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>',
            'heart': '<path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z"/>',
            'check': '<polyline points="20 6 9 17 4 12"/>',
            'check-circle': '<path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>',
            'x': '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>',
            'alert-circle': '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
        };

        var paths = basicPaths[name] || basicPaths['check'];
        var svg = '<svg xmlns="http://www.w3.org/2000/svg" ' +
            'width="' + size + '" height="' + size + '" ' +
            'viewBox="0 0 24 24" fill="none" stroke="' + color + '" ' +
            'stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
            paths + '</svg>';
        return svg;
    }

    /**
     * Load all icons on the page
     * Supports: data-lucide="name", class="li-name", class="fa fa-name", class="fas fa-name"
     */
    function load() {
        // Single selector: data-lucide or li-* icons only
        // Skip FA (fa, fas, far) — they're handled by Font Awesome CSS
        var els = document.querySelectorAll('[data-lucide], [class*="li-"]');
        els.forEach(function(el) {
            if (el.querySelector('svg')) return;
            var cls = el.className || '';
            // Skip FA icons (handled by Font Awesome CSS)
            if (cls.indexOf('fa-') !== -1 || cls.indexOf('fas ') !== -1 || cls.indexOf('far ') !== -1 || cls.indexOf('fab ') !== -1) return;
            renderIcon(el);
        });
    }

    // Auto-load on DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', load);
    } else {
        load();
    }

    // Export for manual re-load after dynamic content
    window.LucideIcons = {
        load: load,
        map: ICON_MAP,
        fetchIcon: fetchIcon,
        render: renderIcon
    };
})();
