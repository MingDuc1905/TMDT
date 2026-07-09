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
    $(window).scroll(function () {
        var scrollY = $(this).scrollTop();
        var topbar = $('.fs-topbar');
        var topbarH = topbar.length ? topbar.outerHeight() : 34; /* v4.2: 38→34 compact */
        var $nav = $('.fs-nav');
        var $body = $('body');

        if (scrollY > topbarH) {
            $nav.addClass('fs-nav-fixed');
            $body.addClass('fs-body-padded');
        } else {
            $nav.removeClass('fs-nav-fixed');
            $body.removeClass('fs-body-padded');
        }
    });
    
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
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

    // ── AOS (Animate On Scroll) — thay thế WOW cũ ──
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 600,
            once: true,
            offset: 40,
            easing: 'ease-out-cubic'
        });
    }

    // ── Typed.js — Typing Text Effect cho Hero Slogan ──
    var typedInstances = [];
    function initTypedEffect() {
        if (typeof Typed === 'undefined') return;
        var els = document.querySelectorAll('.fs-typed-text');
        els.forEach(function(el) {
            if (el.dataset.typedInited) return;
            el.dataset.typedInited = '1';
            var strings = el.dataset.strings ? JSON.parse(el.dataset.strings) : ['']; 
            var inst = new Typed(el, {
                strings: strings,
                typeSpeed: 50,
                backSpeed: 30,
                backDelay: 2000,
                startDelay: 500,
                loop: true,
                showCursor: true,
                cursorChar: '|',
                autoInsertCss: true
            });
            typedInstances.push(inst);
        });
    }

    // ── CountUp.js — Số đếm Stats Row ──
    var countUpInited = false;
    function initCountUp() {
        if (typeof CountUp === 'undefined') return;
        if (countUpInited) return;
        countUpInited = true;
        document.querySelectorAll('.fs-count-up').forEach(function(el) {
            var target = parseFloat(el.dataset.target) || 0;
            var suffix = el.dataset.suffix || '';
            var prefix = el.dataset.prefix || '';
            var decimals = el.dataset.decimals ? parseInt(el.dataset.decimals) : 0;
            var duration = el.dataset.duration ? parseFloat(el.dataset.duration) : 2;
            
            try {
                var cu = new CountUp(el, 0, target, decimals, duration, {
                    prefix: prefix,
                    suffix: suffix,
                    enableScrollSpy: false
                });
                if (!cu.error) {
                    cu.start();
                } else {
                    console.error('CountUp error:', cu.error);
                }
            } catch(e) {
                // Fallback: show raw number
                el.textContent = prefix + target.toLocaleString('vi-VN') + suffix;
            }
        });
    }

    // ── IntersectionObserver cho CountUp ──
    var countUpObserver = new IntersectionObserver(function(entries) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                initCountUp();
                countUpObserver.disconnect();
            }
        });
    }, { threshold: 0.3 });

    var statsRow = document.querySelector('.fs-stats-row');
    if (statsRow) countUpObserver.observe(statsRow);

    // ── 3D Card Hover — nghiêng theo chuột ──
    function init3DCards() {
        document.querySelectorAll('.product-item').forEach(function(card) {
            card.addEventListener('mousemove', function(e) {
                var rect = card.getBoundingClientRect();
                var x = e.clientX - rect.left;
                var y = e.clientY - rect.top;
                var centerX = rect.width / 2;
                var centerY = rect.height / 2;
                var rotateX = ((y - centerY) / centerY) * -8;  // max 8 độ
                var rotateY = ((x - centerX) / centerX) * 8;   // max 8 độ
                card.style.transform = 
                    'perspective(800px) rotateX(' + rotateX + 'deg) rotateY(' + rotateY + 'deg) translateY(-8px)';
            });
            card.addEventListener('mouseleave', function() {
                card.style.transform = '';
            });
        });
    }

    // ── Khởi tạo trên DOMContentLoaded ──
    document.addEventListener('DOMContentLoaded', function() {
        initTypedEffect();
        init3DCards();
    });

    // ── Tái khởi tạo Typed khi carousel slide thay đổi ──
    var carouselEl = document.getElementById('header-carousel');
    if (carouselEl) {
        carouselEl.addEventListener('slid.bs.carousel', function() {
            // Destroy old instances và tạo lại để text effect chạy đúng slide
            typedInstances.forEach(function(t) { t.destroy(); });
            typedInstances = [];
            setTimeout(initTypedEffect, 100);
        });
    }

})(jQuery);

