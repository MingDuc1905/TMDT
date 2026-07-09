/**
 * FastShip Smooth Page Transitions
 * Fade transitions giữa các trang nội bộ — cảm giác SPA mượt mà
 * Dùng Barba.js hoặc tự code intercept link clicks
 * Version: 1.0
 */

(function() {
    'use strict';

    // ─── Config ───
    var TRANSITION_DURATION = 350; // ms
    var EXCLUDE_SELECTOR = '[data-no-transition], .dropdown-item, [target="_blank"], a[href*="tel:"], a[href*="mailto:"], a[href^="#"], .navbar-toggler, .carousel-control-prev, .carousel-control-next, form a, button, input[type="submit"], .fs-category-pill, .fs-cat-btn';
    var SAME_HOST_PATTERN = /^\/[^\/]|^https?:\/\/[^\/]*fastship/i;

    // ─── State ───
    var isTransitioning = false;

    // ─── Progress Bar Element ───
    var progressBar = document.createElement('div');
    progressBar.id = 'fs-page-progress';
    progressBar.style.cssText = [
        'position:fixed',
        'top:0',
        'left:0',
        'width:0%',
        'height:3px',
        'background:linear-gradient(90deg, #3CB815, #27a001)',
        'z-index:99999',
        'transition:width .3s ease',
        'box-shadow:0 0 10px rgba(60,184,21,.5)'
    ].join(';') + ';';
    document.head.appendChild(progressBar);

    function showProgress() {
        progressBar.style.width = '30%';
    }

    function completeProgress() {
        progressBar.style.width = '100%';
        setTimeout(function() {
            progressBar.style.width = '0%';
        }, 400);
    }

    function failProgress() {
        progressBar.style.width = '0%';
    }

    // ─── Fade Transition ───
    function fadeOut(el, duration) {
        return new Promise(function(resolve) {
            el.style.transition = 'opacity ' + duration + 'ms ease';
            el.style.opacity = '0';
            setTimeout(resolve, duration);
        });
    }

    function fadeIn(el, duration) {
        return new Promise(function(resolve) {
            el.style.opacity = '0';
            el.style.transition = 'opacity ' + duration + 'ms ease';
            // Force reflow
            void el.offsetHeight;
            el.style.opacity = '1';
            setTimeout(resolve, duration);
        });
    }

    // ─── Load Page via Fetch ───
    function loadPage(url) {
        return fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(function(res) {
            if (!res.ok) throw new Error('Page load failed: ' + res.status);
            return res.text();
        })
        .then(function(html) {
            // Parse HTML để lấy nội dung chính
            var parser = new DOMParser();
            var doc = parser.parseFromString(html, 'text/html');
            var newContent = doc.getElementById('main-content');
            if (!newContent) throw new Error('No main-content found');
            return newContent.innerHTML;
        });
    }

    // ─── Handle Link Click ───
    function handleLinkClick(e) {
        var link = e.currentTarget;
        
        // Bỏ qua nếu đang transition
        if (isTransitioning) {
            e.preventDefault();
            return;
        }

        var href = link.getAttribute('href');
        if (!href || href === '#') return;

        // Chỉ xử lý link nội bộ
        var isInternal = href.startsWith('/');
        if (!isInternal) return;

        e.preventDefault();
        navigateTo(href);
    }

    // ─── Navigation Function ───
    function navigateTo(url) {
        if (isTransitioning) return;
        isTransitioning = true;

        var mainContent = document.getElementById('main-content');
        if (!mainContent) {
            isTransitioning = false;
            window.location.href = url;
            return;
        }

        showProgress();

        fadeOut(mainContent, TRANSITION_DURATION / 2)
            .then(function() {
                return loadPage(url);
            })
            .then(function(newHtml) {
                // Update content
                mainContent.innerHTML = newHtml;
                // Update URL
                window.history.pushState({ url: url }, '', url);
                // Update document title
                var title = document.title;
                var newTitle = document.querySelector('title');
                // Scroll lên đầu
                window.scrollTo({ top: 0, behavior: 'smooth' });
                // Fade in
                return fadeIn(mainContent, TRANSITION_DURATION / 2);
            })
            .then(function() {
                completeProgress();
                isTransitioning = false;

                // Re-init AOS cho content mới
                if (window.AOS) {
                    setTimeout(function() { AOS.refresh(); }, 100);
                }
                // Re-init Typed
                if (window.initTypedEffect) {
                    setTimeout(initTypedEffect, 200);
                }
            })
            .catch(function(err) {
                console.error('Page transition failed:', err);
                failProgress();
                isTransitioning = false;
                // Fallback: navigate thường
                window.location.href = url;
            });
    }

    // ─── Handle Back/Forward ───
    window.addEventListener('popstate', function(e) {
        if (e.state && e.state.url) {
            navigateTo(e.state.url);
        }
    });

    // ─── Bind Links ───
    function bindLinks() {
        document.querySelectorAll('a:not(' + EXCLUDE_SELECTOR + ')').forEach(function(a) {
            var href = a.getAttribute('href');
            if (href && href.startsWith('/')) {
                a.removeEventListener('click', handleLinkClick);
                a.addEventListener('click', handleLinkClick);
            }
        });
    }

    // ─── MutationObserver cho nội dung động ───
    var observer = new MutationObserver(function() {
        bindLinks();
    });

    // ─── Init ───
    document.addEventListener('DOMContentLoaded', function() {
        bindLinks();

        var mainContent = document.getElementById('main-content');
        if (mainContent) {
            observer.observe(mainContent, { childList: true, subtree: true });
        }

        // Fade in ngay khi load
        mainContent.style.opacity = '0';
        setTimeout(function() {
            fadeIn(mainContent, 400);
        }, 50);
    });

})();
