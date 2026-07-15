(function ($) {
    "use strict";

    // ── Cấu hình Carousel trượt ngang horizontal smooth ──
    // Bootstrap 5 khởi tạo carousel tự động khi có data-bs-ride, nhưng ta init thủ công
    // để đảm bảo timing với skeleton loading
    function initHeroCarousel() {
        var carouselEl = document.getElementById('header-carousel');
        if (!carouselEl) return;
        
        // Check if already initialized
        var bsCarousel = bootstrap.Carousel.getInstance(carouselEl);
        if (bsCarousel) {
            bsCarousel.cycle();
            return;
        }
        
        // Khởi tạo mới với interval 4500ms
        new bootstrap.Carousel(carouselEl, {
            interval: 4500,
            ride: 'carousel',
            pause: false
        });
        
        // CSS @keyframes fsSlideLeft tự động chạy khi slide.active thay đổi
    }

    // Skeleton loading — fade out after page render
    var skeletonLoader = function () {
        setTimeout(function () {
            $('#fs-loading-skeleton').fadeOut(250, function() {
                $(this).remove();
                // ── Khởi tạo WOW.js & OwlCarousel SAU KHI skeleton biến mất ──
                if (typeof WOW !== 'undefined') {
                    new WOW({
                        offset: 0,
                        mobile:  true,
                        live:    true
                    }).init();
                }
                // OwlCarousel: chỉ init nếu phần tử tồn tại
                if ($(".testimonial-carousel").length && typeof $.fn.owlCarousel !== 'undefined') {
                    $(".testimonial-carousel").owlCarousel({
                        autoplay: true,
                        smartSpeed: 1000,
                        margin: 25,
                        loop: true,
                        center: true,
                        dots: false,
                        nav: true,
                        navText : [
                            '<i class="bi bi-chevron-left"></i>',
                            '<i class="bi bi-chevron-right"></i>'
                        ],
                        responsive: {
                            0:{
                                items:1
                            },
                            768:{
                                items:2
                            },
                            992:{
                                items:3
                            }
                        }
                    });
                }

                // ── Khởi tạo Bootstrap Carousel SAU KHI skeleton biến mất ──
                initHeroCarousel();
            });
        }, 100);
    };
    skeletonLoader();
    
    
    // Fixed Navbar on scroll — topbar scrolls away, nav becomes fixed at top
    var navScrollTicking = false;
    $(window).scroll(function () {
        if (!navScrollTicking) {
            requestAnimationFrame(function () {
                var scrollY = $(window).scrollTop();
                var topbar = $('.fs-topbar');
                var topbarH = topbar.length ? topbar.outerHeight() : 34;
                var $nav = $('.fs-nav');
                var $body = $('body');

                if (scrollY > topbarH) {
                    $nav.addClass('fs-nav-fixed');
                    $body.addClass('fs-body-padded');
                } else {
                    $nav.removeClass('fs-nav-fixed');
                    $body.removeClass('fs-body-padded');
                }

                // Back to top button
                if (scrollY > 300) {
                    $('.back-to-top').fadeIn('slow');
                } else {
                    $('.back-to-top').fadeOut('slow');
                }
                navScrollTicking = false;
            });
            navScrollTicking = true;
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });

    // ── Chat + Leaflet Map Overlap Fix ──
    // Auto-ẩn chat widget khi người dùng tương tác với Leaflet map
    $(document).on('click', '.leaflet-map, .leaflet-container, #shipper-map, #map', function() {
        var chatBox = document.getElementById('chatBox');
        var chatToggle = document.querySelector('.chat-toggle');
        if (chatBox && chatBox.classList.contains('active')) {
            chatBox.classList.remove('active');
            if (chatToggle) chatToggle.style.display = 'flex';
        }
    });

    // Auto-ẩn chat khi drag trên Leaflet map (touch devices)
    $(document).on('touchstart', '.leaflet-container', function() {
        var chatBox = document.getElementById('chatBox');
        var chatToggle = document.querySelector('.chat-toggle');
        if (chatBox && chatBox.classList.contains('active')) {
            // Delay nhẹ để không ảnh hưởng đến touch start của map
            setTimeout(function() {
                chatBox.classList.remove('active');
                if (chatToggle) chatToggle.style.display = 'flex';
            }, 100);
        }
    });

})(jQuery);

// ═══════════════════════════════════════════════════════════
// SCROLL REVEAL — IntersectionObserver (site-wide)
// ═══════════════════════════════════════════════════════════
(function() {
    'use strict';
    var revealEls = document.querySelectorAll('.fs-reveal');
    if (revealEls.length) {
        var observer = new IntersectionObserver(function(entries, obs) {
            entries.forEach(function(entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('fs-revealed');
                obs.unobserve(entry.target);
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });
        revealEls.forEach(function(el) { observer.observe(el); });
    }
})();

// ═══════════════════════════════════════════════════════════
// COUNTER ANIMATION — số đếm khi scroll vào viewport
// ═══════════════════════════════════════════════════════════
(function() {
    'use strict';
    var counters = document.querySelectorAll('.fs-counter[data-count]');
    if (!counters.length) return;

    // Ưu tiên requestAnimationFrame cho animation mượt
    var counterObserver = new IntersectionObserver(function(entries, obs) {
        entries.forEach(function(entry) {
            if (!entry.isIntersecting) return;
            var el = entry.target;
            var target = parseInt(el.getAttribute('data-count'), 10);
            if (isNaN(target) || target <= 0) { obs.unobserve(el); return; }
            var duration = Math.min(2000, target * 10); // max 2s
            var start = performance.now();
            function step(now) {
                var progress = Math.min((now - start) / duration, 1);
                // easeOutQuad
                var eased = 1 - (1 - progress) * (1 - progress);
                var current = Math.round(eased * target);
                el.textContent = current.toLocaleString('vi-VN');
                if (progress < 1) {
                    requestAnimationFrame(step);
                } else {
                    el.textContent = target.toLocaleString('vi-VN');
                }
            }
            requestAnimationFrame(step);
            obs.unobserve(el);
        });
    }, { threshold: 0.3 });

    counters.forEach(function(el) { counterObserver.observe(el); });
})();
