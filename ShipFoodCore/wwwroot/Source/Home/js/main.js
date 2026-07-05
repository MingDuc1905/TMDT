(function ($) {
    "use strict";

    // ── Helper: re-trigger caption animation (slideInDown / slideInUp) ──
    function triggerCaptionAnim(slideEl) {
        if (!slideEl) return;
        var els = slideEl.querySelectorAll('.carousel-caption h1, .carousel-caption .slideInUp');
        els.forEach(function (el) {
            if (!el) return;
            el.style.animation = 'none';
            el.offsetHeight; // force reflow
            el.style.animation = '';
        });
    }

    // ── Chuyển Bootstrap Carousel sang crossfade (thay vì slide) ──
    // Tương đương animateIn: 'fadeIn' / animateOut: 'fadeOut' của OwlCarousel
    document.addEventListener('DOMContentLoaded', function () {
        var carouselEl = document.getElementById('header-carousel');
        if (carouselEl) {
            carouselEl.classList.add('carousel-fade');
        }
    });

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

                // ── Re-trigger Bootstrap Carousel SAU KHI skeleton biến mất ──
                var carouselEl = document.getElementById('header-carousel');
                if (carouselEl) {
                    // Đảm bảo carousel auto-play hoạt động trở lại
                    var bsCarousel = bootstrap.Carousel.getInstance(carouselEl);
                    if (bsCarousel) {
                        bsCarousel.cycle();
                    }
                    // Re-trigger caption animation cho slide đang active
                    var activeSlide = carouselEl.querySelector('.carousel-item.active');
                    if (activeSlide) {
                        triggerCaptionAnim(activeSlide);
                    }
                }
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

})(jQuery);

