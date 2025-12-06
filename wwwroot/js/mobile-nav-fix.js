/**
 * Mobile Navigation Fix - إصلاح التنقل في الموبايل
 * Version: 5.0
 * يعمل مع Bootstrap 5
 */

(function() {
    'use strict';

    // تأكد من تحميل DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initMobileNav);
    } else {
        initMobileNav();
    }

    function initMobileNav() {
        console.log('🚀 تهيئة نظام التنقل للموبايل...');

        // التحقق من حجم الشاشة
        if (window.innerWidth > 991) {
            console.log('📱 الشاشة كبيرة - تخطي تهيئة الموبايل');
            return;
        }

        initNavbarToggler();
        initLanguageDropdown();
        initUserDropdown();
        initNotificationDropdown();
        initOutsideClickHandler();
        initScrollHandler();

        console.log('✅ تم تهيئة نظام التنقل للموبايل بنجاح');
    }

    /**
     * تهيئة زر القائمة الرئيسية
     */
    function initNavbarToggler() {
        const toggler = document.querySelector('.navbar-toggler');
        const collapse = document.querySelector('.navbar-collapse');

        if (!toggler || !collapse) return;

        // إزالة المستمعات السابقة
        const newToggler = toggler.cloneNode(true);
        toggler.parentNode.replaceChild(newToggler, toggler);

        newToggler.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            const isOpen = collapse.classList.contains('show');

            if (isOpen) {
                closeNavbar();
            } else {
                openNavbar();
            }
        });

        console.log('✅ تم تهيئة زر القائمة');
    }

    /**
     * فتح القائمة الرئيسية
     */
    function openNavbar() {
        const collapse = document.querySelector('.navbar-collapse');
        if (!collapse) return;

        // إغلاق جميع القوائم الفرعية أولاً
        closeAllDropdowns();

        collapse.classList.add('show');
        document.body.style.overflow = 'hidden';
        
        console.log('📂 تم فتح القائمة الرئيسية');
    }

    /**
     * إغلاق القائمة الرئيسية
     */
    function closeNavbar() {
        const collapse = document.querySelector('.navbar-collapse');
        if (!collapse) return;

        collapse.classList.remove('show');
        document.body.style.overflow = '';

        // إغلاق جميع القوائم الفرعية
        closeAllDropdowns();

        console.log('📁 تم إغلاق القائمة الرئيسية');
    }

    /**
     * تهيئة قائمة اللغات
     */
    function initLanguageDropdown() {
        const languageToggle = document.querySelector('#languageMenu, .language-dropdown');
        const languageMenu = document.querySelector('#languageDropdown');

        if (!languageToggle) return;

        // إزالة المستمعات السابقة
        const newToggle = languageToggle.cloneNode(true);
        languageToggle.parentNode.replaceChild(newToggle, languageToggle);

        newToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            const menu = this.closest('.dropdown, .language-selector')?.querySelector('.dropdown-menu');
            if (menu) {
                toggleDropdown(menu);
            }
        });

        // تهيئة أزرار اللغة
        document.querySelectorAll('.language-form').forEach(form => {
            form.addEventListener('submit', function(e) {
                const button = form.querySelector('button');
                if (button && !button.disabled) {
                    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>جاري التغيير...';
                    button.disabled = true;
                }
            });
        });

        console.log('✅ تم تهيئة قائمة اللغات');
    }

    /**
     * تهيئة قائمة المستخدم
     */
    function initUserDropdown() {
        const userToggle = document.querySelector('#userMenu, .user-dropdown');

        if (!userToggle) return;

        // إزالة المستمعات السابقة
        const newToggle = userToggle.cloneNode(true);
        userToggle.parentNode.replaceChild(newToggle, userToggle);

        newToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            const menu = this.nextElementSibling || 
                         this.closest('.dropdown')?.querySelector('.dropdown-menu');
            if (menu && menu.classList.contains('dropdown-menu')) {
                toggleDropdown(menu);
            }
        });

        console.log('✅ تم تهيئة قائمة المستخدم');
    }

    /**
     * تهيئة قائمة الإشعارات
     */
    function initNotificationDropdown() {
        const notificationBell = document.querySelector('.notification-bell, #notification-dropdown-toggle');

        if (!notificationBell) return;

        // إزالة المستمعات السابقة
        const newBell = notificationBell.cloneNode(true);
        notificationBell.parentNode.replaceChild(newBell, notificationBell);

        newBell.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            // التحقق من وجود قائمة منسدلة
            const menu = this.nextElementSibling || 
                         this.closest('.dropdown')?.querySelector('.dropdown-menu');
            
            if (menu && menu.classList.contains('dropdown-menu')) {
                toggleDropdown(menu);
            } else {
                // الانتقال لصفحة الإشعارات إذا لم تكن هناك قائمة
                window.location.href = '/Notifications';
            }
        });

        console.log('✅ تم تهيئة جرس الإشعارات');
    }

    /**
     * تبديل حالة القائمة المنسدلة
     */
    function toggleDropdown(menu) {
        if (!menu) return;

        const isOpen = menu.classList.contains('show');

        // إغلاق جميع القوائم الأخرى
        closeAllDropdowns();

        if (!isOpen) {
            menu.classList.add('show');
            console.log('📂 تم فتح قائمة منسدلة');
        }
    }

    /**
     * إغلاق جميع القوائم المنسدلة
     */
    function closeAllDropdowns() {
        document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
            menu.classList.remove('show');
        });
    }

    /**
     * معالج النقر خارج القائمة
     */
    function initOutsideClickHandler() {
        document.addEventListener('click', function(e) {
            // تحقق إذا كان النقر خارج النافبار
            const navbar = document.querySelector('.main-navbar, .navbar');
            const collapse = document.querySelector('.navbar-collapse');
            const toggler = document.querySelector('.navbar-toggler');

            if (!navbar) return;

            // إذا كان النقر على زر التوجل، لا تفعل شيئاً
            if (toggler && toggler.contains(e.target)) {
                return;
            }

            // إذا كان النقر خارج النافبار بالكامل
            if (!navbar.contains(e.target)) {
                closeNavbar();
                closeAllDropdowns();
                return;
            }

            // إذا كان النقر على عنصر داخل القائمة المنسدلة (ليس dropdown-toggle)
            if (collapse && collapse.contains(e.target)) {
                // إذا كان النقر على رابط عادي (ليس dropdown-toggle)
                const clickedLink = e.target.closest('.nav-link:not(.dropdown-toggle)');
                const clickedDropdownItem = e.target.closest('.dropdown-item');

                if (clickedLink || clickedDropdownItem) {
                    // انتظر قليلاً ثم أغلق القائمة
                    setTimeout(() => {
                        closeNavbar();
                    }, 100);
                }
            }
        });

        console.log('✅ تم تهيئة معالج النقر الخارجي');
    }

    /**
     * معالج التمرير
     */
    function initScrollHandler() {
        let lastScrollY = window.scrollY;
        let ticking = false;

        window.addEventListener('scroll', function() {
            if (!ticking) {
                window.requestAnimationFrame(function() {
                    const currentScrollY = window.scrollY;
                    
                    // إغلاق القوائم عند التمرير
                    if (Math.abs(currentScrollY - lastScrollY) > 50) {
                        closeAllDropdowns();
                    }

                    lastScrollY = currentScrollY;
                    ticking = false;
                });
                ticking = true;
            }
        });

        console.log('✅ تم تهيئة معالج التمرير');
    }

    /**
     * إعادة التهيئة عند تغيير حجم الشاشة
     */
    let resizeTimeout;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            if (window.innerWidth <= 991) {
                initMobileNav();
            } else {
                // إغلاق كل شيء عند التبديل للشاشة الكبيرة
                closeNavbar();
                closeAllDropdowns();
            }
        }, 250);
    });

    // تصدير الدوال للاستخدام الخارجي
    window.MobileNav = {
        openNavbar: openNavbar,
        closeNavbar: closeNavbar,
        closeAllDropdowns: closeAllDropdowns,
        toggleDropdown: toggleDropdown,
        reinit: initMobileNav
    };

})();
