(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner(0);


    // Fixed Navbar
    $(window).scroll(function () {
        if ($(window).width() < 992) {
            if ($(this).scrollTop() > 55) {
                $('.fixed-top').addClass('shadow');
            } else {
                $('.fixed-top').removeClass('shadow');
            }
        } else {
            if ($(this).scrollTop() > 55) {
                $('.fixed-top').addClass('shadow').css('top', -55);
            } else {
                $('.fixed-top').removeClass('shadow').css('top', 0);
            }
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


    // vegetable carousel
    $(".vegetable-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1500,
        center: false,
        dots: true,
        loop: true,
        margin: 25,
        nav : true,
        navText : [
            '<i class="bi bi-arrow-left"></i>',
            '<i class="bi bi-arrow-right"></i>'
        ],
        responsiveClass: true,
        responsive: {
            0:{
                items:1
            },
            576:{
                items:1
            },
            768:{
                items:2
            },
            992:{
                items:3
            },
            1200:{
                items:4
            }
        }
    });


    // Modal Video
    $(document).ready(function () {
        var $videoSrc;
        $('.btn-play').click(function () {
            $videoSrc = $(this).data("src");
        });

        $('#videoModal').on('shown.bs.modal', function (e) {
            $("#video").attr('src', $videoSrc + "?autoplay=1&amp;modestbranding=1&amp;showinfo=0");
        })

        $('#videoModal').on('hide.bs.modal', function (e) {
            $("#video").attr('src', $videoSrc);
        })
    });

    // =========================================================
    // GreenBasket Global UI Utilities & AppState Integration
    // =========================================================
    window.GB = window.GB || {};

    // Custom Misfits Market Organic Toast Notification System
    window.GB.showToast = function (message, type = 'success') {
        let container = document.getElementById('gb-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'gb-toast-container';
            container.style.cssText = 'position: fixed; bottom: 24px; right: 24px; z-index: 99999 !important; display: flex; flex-direction: column; gap: 12px; pointer-events: none; max-width: 420px; width: calc(100% - 48px);';
            document.body.appendChild(container);
        } else {
            container.style.zIndex = '99999';
        }

        const toast = document.createElement('div');
        const toastClass = type === 'success' ? 'gb-toast-success' : (type === 'danger' ? 'gb-toast-danger' : 'gb-toast-info');
        const iconHtml = type === 'success' 
            ? '<i class="fas fa-leaf text-success me-2 fs-5"></i>' 
            : (type === 'danger' 
                ? '<i class="fas fa-exclamation-triangle text-warning me-2 fs-5"></i>' 
                : '<i class="fas fa-info-circle text-info me-2 fs-5"></i>');

        toast.className = `gb-misfits-toast ${toastClass}`;
        toast.innerHTML = `
            ${iconHtml}
            <div class="gb-toast-content" style="flex-grow: 1; line-height: 1.4;">${message}</div>
            <button type="button" class="gb-toast-close" title="Close"><i class="fas fa-times"></i></button>
        `;

        // Click to close
        toast.querySelector('.gb-toast-close').onclick = function () {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(10px) scale(0.95)';
            setTimeout(() => toast.remove(), 250);
        };

        container.appendChild(toast);
        setTimeout(() => {
            if (toast.parentElement) {
                toast.style.opacity = '0';
                toast.style.transform = 'translateY(10px) scale(0.95)';
                setTimeout(() => toast.remove(), 250);
            }
        }, 4000);
    };

    // Custom Misfits Market Confirmation Modal System
    window.GB.confirm = function (message, onConfirm, title = 'GreenBasket Confirmation') {
        let modalEl = document.getElementById('gb-confirm-modal');
        if (!modalEl) {
            modalEl = document.createElement('div');
            modalEl.id = 'gb-confirm-modal';
            modalEl.className = 'modal fade';
            modalEl.tabIndex = -1;
            modalEl.style.zIndex = '10800';
            modalEl.innerHTML = `
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content" style="background-color: #F9EDDE; border: 3px solid #2B2118; border-radius: 24px; box-shadow: 8px 8px 0px #2B2118;">
                        <div class="modal-header border-bottom border-dark pb-3">
                            <h4 class="fw-bold mb-0" id="gb-confirm-title" style="color: #1C3F2B;">
                                <i class="fas fa-leaf text-success me-2"></i>GreenBasket Notice
                            </h4>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body p-4 fs-5 text-dark fw-bold" id="gb-confirm-message">
                        </div>
                        <div class="modal-footer border-0 pt-0 gap-2">
                            <button type="button" class="btn rounded-pill px-4 py-2 fw-bold" data-bs-dismiss="modal" style="background-color: #e2e8f0; color: #2B2118; border: 2px solid #2B2118;">
                                Cancel
                            </button>
                            <button type="button" id="gb-btn-confirm-action" class="btn rounded-pill px-4 py-2 fw-bold text-white" style="background-color: #1C3F2B; border: 2px solid #2B2118; box-shadow: 2px 2px 0px #2B2118;">
                                Confirm <i class="fas fa-check-circle ms-1"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `;
            document.body.appendChild(modalEl);
        }

        document.getElementById('gb-confirm-title').innerHTML = `<i class="fas fa-leaf text-success me-2"></i>${title}`;
        document.getElementById('gb-confirm-message').innerHTML = message;

        const confirmBtn = document.getElementById('gb-btn-confirm-action');
        const newConfirmBtn = confirmBtn.cloneNode(true);
        confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

        const bsModal = new bootstrap.Modal(modalEl);

        newConfirmBtn.onclick = function () {
            bsModal.hide();
            if (typeof onConfirm === 'function') {
                onConfirm();
            }
        };

        bsModal.show();
    };

    // Override global alert() so native browser popups like "127.0.0.1 says Incorrect password."
    // are automatically intercepted & converted into Misfits Market Toasts
    window.alert = function (msg) {
        if (window.GB && window.GB.showToast) {
            window.GB.showToast(msg || 'Notice', 'danger');
        }
    };

    // Update Header UI Elements (Cart Count Badge, User Role, Auth Sign Out)
    function updateHeaderUI() {
        if (!window.AppState) return;

        // Update Cart Count
        const count = window.AppState.getCartCount();
        const $cartBadges = $('.gb-cart-count');
        if ($cartBadges.length) {
            $cartBadges.text(count);
        }

        const role = window.AppState.getUserRole();
        const user = window.AppState.getAuthUser();
        const isStaffOrAdmin = role === 'Staff / Admin';

        // 1. Show/Hide Staff Portal links based on actual role
        const $staffLinks = $('.gb-staff-link, a[href="admin.html"]');
        if ($staffLinks.length) {
            if (isStaffOrAdmin) {
                $staffLinks.show();
            } else {
                $staffLinks.hide();
            }
        }

        // 2. Hide Role Badge container if present
        $('.gb-role-badge').hide();

        // 3. Update dedicated auth container if present
        const $authContainer = $('#gb-header-auth-user');
        if ($authContainer.length) {
            if (user) {
                $authContainer.html(`
                    <span class="text-dark me-2 small fw-bold"><i class="fas fa-user-circle me-1 text-success"></i>${user.name}</span>
                    <button type="button" class="btn btn-sm btn-outline-danger rounded-pill px-3 py-1 btn-signout fw-bold"><i class="fas fa-sign-out-alt me-1"></i>Sign Out</button>
                `);
            } else {
                $authContainer.empty();
            }
        }

        // 4. Automatically inject Sign Out into Topbar (.top-link) across index.html, shop.html, etc.
        const $topLink = $('.top-link');
        if ($topLink.length && user) {
            if ($('#topbar-signout-btn').length === 0) {
                $topLink.append(`
                    <span class="text-white ms-3 me-2 small fw-bold"><i class="fas fa-user-circle me-1 text-warning"></i>${user.name}</span>
                    <button type="button" id="topbar-signout-btn" class="btn btn-sm btn-light rounded-pill px-3 py-0 text-dark fw-bold btn-signout ms-1" style="font-size: 0.8rem; border: 1.5px solid #2B2118;" title="Sign Out"><i class="fas fa-sign-out-alt me-1 text-danger"></i>Sign Out</button>
                `);
            }
        }

        // 5. Landing page Header Buttons auto-switch when logged in
        const $landingAuthContainer = $('#landing-auth-btns');
        if ($landingAuthContainer.length && user) {
            $landingAuthContainer.html(`
                <span class="fw-bold me-2 text-dark"><i class="fas fa-user-circle me-1 text-success fs-5"></i>Hi, <strong>${user.name}</strong></span>
                <button type="button" class="btn btn-sm btn-outline-danger rounded-pill px-3 py-2 fw-bold btn-signout" title="Sign Out"><i class="fas fa-sign-out-alt me-1"></i>Sign Out</button>
            `);
        }
    }

    // Auth Guard Route Check for Unauthenticated & Non-Staff Visitors
    function checkAuthGuard() {
        const currentPage = window.location.pathname.split('/').pop() || 'index.html';
        const publicPages = ['landing.html'];

        if (!publicPages.includes(currentPage) && window.AppState && !window.AppState.isLoggedIn()) {
            console.warn('Unauthenticated access attempt to ' + currentPage + '. Redirecting to landing.html...');
            window.location.href = 'landing.html';
            return;
        }

        // Staff / Admin Only Route Protection for admin.html
        if (currentPage === 'admin.html' && window.AppState) {
            const role = window.AppState.getUserRole();
            if (role !== 'Staff / Admin') {
                console.warn('Access denied to admin.html for non-staff user');
                window.location.href = 'index.html';
            }
        }
    }

    // =========================================================
    // EXPANDABLE FULL-WIDTH NAVBAR SEARCH OVERLAY WITH LIVE RESULTS
    // =========================================================
    function initSearchOverlay() {
        if ($('#gb-search-overlay').length > 0) return;

        const overlayHtml = `
            <div id="gb-search-overlay" style="position: fixed; top: 0; left: 0; width: 100%; z-index: 10500; background: #1C3F2B; border-bottom: 3.5px solid #2B2118; box-shadow: 0 10px 30px rgba(0,0,0,0.4); display: none; padding: 18px 20px;">
                <div class="container position-relative">
                    <div class="d-flex align-items-center">
                        <i class="fas fa-search me-3 fs-3" style="color: #FBFA86;"></i>
                        <input type="text" id="gb-top-search-input" class="form-control form-control-lg rounded-pill px-4 py-3" placeholder="Search spinach, grapes, carrots, farm origin..." style="font-weight: 700; border: 2.5px solid #2B2118 !important; box-shadow: 3px 3px 0px #2B2118; background: #ffffff; color: #2B2118;">
                        <button id="gb-close-top-search" class="btn rounded-circle ms-3 d-flex align-items-center justify-content-center" style="width: 48px; height: 48px; background: #FBFA86; border: 2.5px solid #2B2118; box-shadow: 2px 2px 0px #2B2118; flex-shrink: 0;" title="Close Search">
                            <i class="fas fa-times text-dark fs-5"></i>
                        </button>
                    </div>
                    <!-- Live Instant Search Results Dropdown -->
                    <div id="gb-top-search-results" class="position-absolute start-0 w-100 bg-white rounded-3 p-3 shadow-lg" style="top: 100%; margin-top: 12px; border: 2.5px solid #2B2118; max-height: 420px; overflow-y: auto; display: none; z-index: 10600; box-shadow: 6px 6px 0px #2B2118 !important;">
                    </div>
                </div>
            </div>
        `;

        $('body').append(overlayHtml);

        // Open Search Overlay
        window.GB.openSearchOverlay = function(initialQuery = '') {
            $('#searchModal').modal('hide');
            $('#gb-search-overlay').slideDown(250, function() {
                $('#gb-top-search-input').val(initialQuery).focus();
                if (initialQuery) {
                    performLiveSearch(initialQuery);
                }
            });
        };

        // Close Search Overlay
        window.GB.closeSearchOverlay = function() {
            $('#gb-search-overlay').slideUp(200);
            $('#gb-top-search-results').hide().empty();
        };

        // Close Button Event
        $(document).on('click', '#gb-close-top-search', function() {
            window.GB.closeSearchOverlay();
        });

        // Close on Esc key
        $(document).keyup(function(e) {
            if (e.key === "Escape") {
                window.GB.closeSearchOverlay();
            }
        });

        // Live Search Input Typing Event
        $(document).on('input', '#gb-top-search-input', function() {
            const query = $(this).val().trim();
            performLiveSearch(query);
        });

        // Perform Live Filter & Render Results
        function performLiveSearch(query) {
            const $results = $('#gb-top-search-results');
            if (!query || !window.AppState) {
                $results.hide().empty();
                return;
            }

            const products = window.AppState.getProducts();
            const q = query.toLowerCase();
            const filtered = products.filter(p => 
                p.name.toLowerCase().includes(q) || 
                p.categoryName.toLowerCase().includes(q) || 
                p.farmOrigin.toLowerCase().includes(q)
            );

            $results.empty().show();

            if (filtered.length === 0) {
                $results.html(`
                    <div class="text-center py-4">
                        <i class="fas fa-search-minus text-muted fs-2 mb-2"></i>
                        <p class="text-dark fw-bold mb-0">No produce items found matching "${query}"</p>
                    </div>
                `);
                return;
            }

            let resultsHtml = `<div class="mb-2 px-2 pb-2 border-bottom d-flex justify-content-between align-items-center">
                <span class="text-dark fw-bold small"><i class="fas fa-leaf text-success me-1"></i>Found ${filtered.length} matching items</span>
                <small class="text-muted">Instant Catalog Search</small>
            </div><div class="row g-2">`;

            filtered.forEach(p => {
                resultsHtml += `
                    <div class="col-md-6 col-lg-4">
                        <div class="p-2 border rounded-3 d-flex align-items-center bg-light" style="border: 1.5px solid #2B2118 !important;">
                            <img src="${p.image}" class="rounded me-3" style="width: 55px; height: 55px; object-fit: cover; border: 1px solid #2B2118;" alt="${p.name}">
                            <div class="flex-grow-1 overflow-hidden me-2">
                                <h6 class="fw-bold text-dark mb-0 text-truncate" style="font-size: 0.9rem;">${p.name}</h6>
                                <small class="text-muted d-block text-truncate" style="font-size: 0.75rem;"><i class="fas fa-map-marker-alt text-success me-1"></i>${p.farmOrigin}</small>
                                <span class="fw-bold text-dark" style="font-size: 0.85rem;">$${p.price.toFixed(2)} / ${p.unit}</span>
                            </div>
                            <button class="btn btn-sm btn-warning btn-add-cart rounded-pill px-2 py-1" data-product-id="${p.id}" style="font-size: 0.75rem; border: 1.5px solid #2B2118; box-shadow: 1px 1px 0px #2B2118;">
                                <i class="fas fa-plus"></i> Add
                            </button>
                        </div>
                    </div>
                `;
            });

            resultsHtml += `</div>`;
            $results.html(resultsHtml);
        }
    }

    // Global Document Event Listeners
    $(document).ready(function () {
        checkAuthGuard();
        updateHeaderUI();
        initSearchOverlay();

        // Sign Out Delegate
        $(document).on('click', '.btn-signout', function(e) {
            e.preventDefault();
            if (window.AppState) {
                window.AppState.logoutUser();
                window.GB.showToast('You have signed out.', 'info');
                setTimeout(() => {
                    window.location.href = 'landing.html';
                }, 800);
            }
        });

        // Listen for AppState changes across components
        window.addEventListener('gb_state_change', function () {
            updateHeaderUI();
        });

        // Intercept all search triggers to open top search overlay without page navigation
        $(document).on('click', '.btn-search, [data-bs-target="#searchModal"]', function(e) {
            e.preventDefault();
            window.GB.openSearchOverlay();
        });

        // Intercept form submits for search
        $(document).on('submit', '#gb-search-form, .gb-search-form, form[action="shop.html"]', function (e) {
            e.preventDefault();
            const query = $(this).find('input[type="search"], input[type="text"]').val().trim();
            window.GB.openSearchOverlay(query);
        });

        // Global delegate for 'Add to Cart' buttons
        $(document).on('click', '.btn-add-cart', function (e) {
            e.preventDefault();
            const productId = $(this).data('product-id');
            const qty = parseInt($(this).data('qty') || 1, 10);
            if (productId && window.AppState) {
                const product = window.AppState.getProductById(productId);
                if (product && product.stockStatus === 'Out of Stock') {
                    window.GB.showToast(`Sorry, ${product.name} is currently out of stock!`, 'danger');
                    return;
                }
                const success = window.AppState.addToCart(productId, qty);
                if (success && product) {
                    window.GB.showToast(`Added <strong>${product.name}</strong> to your cart!`, 'success');
                }
            }
        });

        // Initialize Holographic Cards on document ready
        window.GB.initHoloCards();
    });

    // =========================================================
    // HOLOGRAPHIC FOIL & BRAND SECURITY SEAL MODULE (NO LAG 2D)
    // =========================================================
    window.GB.initHoloCards = function (targetSelector = '.fruite-item') {
        $(targetSelector).each(function () {
            const $card = $(this);
            $card.addClass('gb-holo-card');

            // Inject holographic elements if missing
            if ($card.find('.gb-holo-foil').length === 0) {
                $card.append('<div class="gb-holo-foil"></div>');
            }
            if ($card.find('.gb-holo-glare').length === 0) {
                $card.append('<div class="gb-holo-glare"></div>');
            }
            if ($card.find('.gb-holo-security-seal').length === 0) {
                $card.append(`
                    <div class="gb-holo-security-seal" title="Anti-Counterfeit Seal - GreenBasket Certified Produce">
                        <div class="gb-seal-banner">
                            <i class="fas fa-shield-alt gb-seal-shield"></i>
                            <div class="gb-seal-text">
                                <span class="gb-seal-brand"><i class="fas fa-leaf me-1"></i>GREENBASKET</span>
                                <small class="gb-seal-sub">AUTHENTIC ORGANIC</small>
                            </div>
                            <i class="fas fa-check-circle gb-seal-check"></i>
                        </div>
                    </div>
                `);
            }
        });
    };

    // Smooth RequestAnimationFrame Mouse Tracking (No 3D Tilt, Maximum Smoothness)
    let holoRaf = null;
    $(document).on('mousemove touchmove', '.fruite-item.gb-holo-card', function (e) {
        const card = this;
        let clientX = e.clientX;
        let clientY = e.clientY;

        if (e.originalEvent && e.originalEvent.touches && e.originalEvent.touches.length > 0) {
            clientX = e.originalEvent.touches[0].clientX;
            clientY = e.originalEvent.touches[0].clientY;
        }

        if (clientX === undefined || clientY === undefined) return;

        if (holoRaf) cancelAnimationFrame(holoRaf);

        holoRaf = requestAnimationFrame(() => {
            const rect = card.getBoundingClientRect();
            const x = clientX - rect.left;
            const y = clientY - rect.top;

            const percentX = Math.max(0, Math.min(100, (x / rect.width) * 100));
            const percentY = Math.max(0, Math.min(100, (y / rect.height) * 100));

            card.style.setProperty('--mouse-x', `${percentX.toFixed(1)}%`);
            card.style.setProperty('--mouse-y', `${percentY.toFixed(1)}%`);
            card.style.setProperty('--holo-opacity', '1');
            card.style.setProperty('--glare-opacity', '0.6');
        });
    });

    // Reset card state on mouseleave / touchend
    $(document).on('mouseleave touchend touchcancel', '.fruite-item.gb-holo-card', function () {
        const card = this;
        card.style.setProperty('--holo-opacity', '0');
        card.style.setProperty('--glare-opacity', '0');
    });

})(jQuery);


