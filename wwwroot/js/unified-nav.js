/**
 * Unified Navigation System - نظام التنقل الموحد المحسّن
 * Version: 7.0 - مع إصلاحات شاملة للموبايل والوضع المظلم
 */

(function () {
    'use strict';

    // انتظار تحميل DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initUnifiedNav);
    } else {
        initUnifiedNav();
    }

    function initUnifiedNav() {
        console.log('🚀 تهيئة نظام التنقل الموحد v7.0...');

        // تهيئة جميع المكونات
        initNavbarToggler();
        initLanguageDropdown();
        initUserDropdown();
        initNotificationDropdown();
        initOutsideClickHandler();
        initKeyboardHandler();
        initScrollHandler();
        initResizeHandler();

        console.log('✅ تم تهيئة نظام التنقل بنجاح');
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

        newToggler.addEventListener('click', function (e) {
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

        closeAllDropdowns();
        collapse.classList.add('show');

        // منع التمرير على الصفحة
        if (window.innerWidth <= 991) {
            document.body.classList.add('nav-open');
            document.body.style.overflow = 'hidden';
        }

        console.log('📂 تم فتح القائمة الرئيسية');
    }

    /**
     * إغلاق القائمة الرئيسية
     */
    function closeNavbar() {
        const collapse = document.querySelector('.navbar-collapse');
        if (!collapse) return;

        collapse.classList.remove('show');
        document.body.classList.remove('nav-open');
        document.body.style.overflow = '';
        closeAllDropdowns();

        console.log('📁 تم إغلاق القائمة الرئيسية');
    }

    /**
     * تهيئة قائمة اللغات
     */
    function initLanguageDropdown() {
        const languageToggle = document.querySelector('#languageMenu, .language-dropdown');

        if (!languageToggle) {
            console.log('⚠️ لم يتم العثور على زر اللغة');
            return;
        }

        // إزالة المستمعات السابقة
        const newToggle = languageToggle.cloneNode(true);
        languageToggle.parentNode.replaceChild(newToggle, languageToggle);

        newToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            console.log('🌐 تم النقر على زر اللغة');

            const parent = this.closest('.dropdown, .language-selector, .nav-item');
            const menu = parent ? parent.querySelector('.dropdown-menu') : null;

            if (menu) {
                const isOpen = menu.classList.contains('show');

                // إغلاق القوائم الأخرى (ما عدا الإشعارات)
                closeOtherDropdowns(menu);

                if (!isOpen) {
                    menu.classList.add('show');
                    this.setAttribute('aria-expanded', 'true');
                    
                    // التمرير إلى القائمة إذا كانت خارج الشاشة
                    scrollToElement(menu);
                    
                    console.log('📂 تم فتح قائمة اللغات');
                } else {
                    menu.classList.remove('show');
                    this.setAttribute('aria-expanded', 'false');
                    console.log('📁 تم إغلاق قائمة اللغات');
                }
            }
        });

        // تهيئة نماذج اللغة
        initLanguageForms();

        console.log('✅ تم تهيئة قائمة اللغات');
    }

    /**
     * تهيئة نماذج تغيير اللغة
     */
    function initLanguageForms() {
        document.querySelectorAll('.language-form').forEach(form => {
            const newForm = form.cloneNode(true);
            form.parentNode.replaceChild(newForm, form);

            newForm.addEventListener('submit', function (e) {
                const button = this.querySelector('button');
                if (button && !button.disabled) {
                    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>جاري التغيير...';
                    button.disabled = true;
                }
                console.log('🔄 جاري تغيير اللغة...');
            });
        });
    }

    /**
     * تهيئة قائمة المستخدم
     */
    function initUserDropdown() {
        const userToggle = document.querySelector('#userMenu, .user-dropdown');

        if (!userToggle) {
            console.log('ℹ️ لم يتم العثور على قائمة المستخدم');
            return;
        }

        const newToggle = userToggle.cloneNode(true);
        userToggle.parentNode.replaceChild(newToggle, userToggle);

        newToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            console.log('👤 تم النقر على قائمة المستخدم');

            const parent = this.closest('.dropdown, .nav-item');
            const menu = parent ? parent.querySelector('.dropdown-menu') : this.nextElementSibling;

            if (menu && menu.classList.contains('dropdown-menu')) {
                const isOpen = menu.classList.contains('show');

                closeOtherDropdowns(menu);

                if (!isOpen) {
                    menu.classList.add('show');
                    this.setAttribute('aria-expanded', 'true');
                    scrollToElement(menu);
                    console.log('📂 تم فتح قائمة المستخدم');
                } else {
                    menu.classList.remove('show');
                    this.setAttribute('aria-expanded', 'false');
                    console.log('📁 تم إغلاق قائمة المستخدم');
                }
            }
        });

        console.log('✅ تم تهيئة قائمة المستخدم');
    }

    /**
     * تهيئة قائمة الإشعارات
     */
    function initNotificationDropdown() {
        const notificationBell = document.getElementById('notificationDropdown');
        const notificationMenu = document.getElementById('notification-menu');

        if (!notificationBell || !notificationMenu) {
            console.log('ℹ️ لم يتم العثور على عناصر الإشعارات');
            return;
        }

        // إزالة المستمعات السابقة
        const newBell = notificationBell.cloneNode(true);
        notificationBell.parentNode.replaceChild(newBell, notificationBell);

        newBell.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            console.log('🔔 تم النقر على جرس الإشعارات');

            const isOpen = notificationMenu.style.display === 'block';

            // إغلاق القوائم الأخرى
            closeOtherDropdowns(notificationMenu);

            if (isOpen) {
                notificationMenu.style.display = 'none';
                this.setAttribute('aria-expanded', 'false');
                console.log('📁 تم إغلاق قائمة الإشعارات');
            } else {
                notificationMenu.style.display = 'block';
                this.setAttribute('aria-expanded', 'true');
                scrollToElement(notificationMenu);
                console.log('📂 تم فتح قائمة الإشعارات');
            }
        });

        // منع إغلاق القائمة عند النقر داخلها
        notificationMenu.addEventListener('click', function (e) {
            e.stopPropagation();

            // السماح بالانتقال للروابط
            if (e.target.tagName === 'A' && e.target.href) {
                if (e.target.href.includes('/Notifications')) {
                    setTimeout(() => {
                        notificationMenu.style.display = 'none';
                    }, 100);
                }
            }
        });

        // منع أحداث الماوس من إغلاق القائمة
        ['mouseenter', 'mousemove', 'mouseover', 'mouseleave'].forEach(event => {
            notificationMenu.addEventListener(event, function (e) {
                e.stopPropagation();
            });
        });

        console.log('✅ تم تهيئة قائمة الإشعارات');
    }

    /**
     * إغلاق القوائم الأخرى (باستثناء القائمة المحددة)
     */
    function closeOtherDropdowns(exceptMenu) {
        document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
            if (menu !== exceptMenu && menu.id !== 'notification-menu') {
                menu.classList.remove('show');
            }
        });

        // إغلاق قائمة الإشعارات إذا لم تكن هي المستثناة
        const notificationMenu = document.getElementById('notification-menu');
        if (notificationMenu && notificationMenu !== exceptMenu) {
            notificationMenu.style.display = 'none';
        }

        document.querySelectorAll('[aria-expanded="true"]').forEach(toggle => {
            if (!toggle.id || (toggle.id !== 'notificationDropdown' && !toggle.closest('.dropdown')?.contains(exceptMenu))) {
                toggle.setAttribute('aria-expanded', 'false');
            }
        });
    }

    /**
     * إغلاق جميع القوائم المنسدلة
     */
    function closeAllDropdowns() {
        document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
            menu.classList.remove('show');
        });

        const notificationMenu = document.getElementById('notification-menu');
        if (notificationMenu) {
            notificationMenu.style.display = 'none';
        }

        document.querySelectorAll('[aria-expanded="true"]').forEach(toggle => {
            toggle.setAttribute('aria-expanded', 'false');
        });
    }

    /**
     * التمرير إلى عنصر معين
     */
    function scrollToElement(element) {
        if (!element || window.innerWidth > 991) return;

        const collapse = document.querySelector('.navbar-collapse');
        if (!collapse) return;

        setTimeout(() => {
            const elementRect = element.getBoundingClientRect();
            const collapseRect = collapse.getBoundingClientRect();

            if (elementRect.bottom > collapseRect.bottom) {
                element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }
        }, 100);
    }

    /**
     * معالج النقر خارج القائمة
     */
    function initOutsideClickHandler() {
        document.addEventListener('click', function (e) {
            const navbar = document.querySelector('.main-navbar, .navbar');
            const toggler = document.querySelector('.navbar-toggler');
            const notificationBell = document.getElementById('notificationDropdown');
            const notificationMenu = document.getElementById('notification-menu');

            // استثناء عناصر الإشعارات
            if (e.target.closest('#notificationDropdown') ||
                e.target.closest('#notification-menu') ||
                e.target.closest('.notification-dropdown')) {
                return;
            }

            if (!navbar) return;

            // استثناء زر القائمة
            if (toggler && toggler.contains(e.target)) {
                return;
            }

            // استثناء أزرار القوائم
            if (e.target.closest('.dropdown-toggle, .language-dropdown, .user-dropdown, #languageMenu, #userMenu')) {
                return;
            }

            // استثناء محتوى القوائم
            const dropdownMenu = e.target.closest('.dropdown-menu');
            if (dropdownMenu) {
                if (e.target.closest('a.dropdown-item') || e.target.closest('button:not([type="submit"])')) {
                    setTimeout(closeAllDropdowns, 100);
                }
                return;
            }

            // إغلاق كل شيء عند النقر خارج النافبار
            if (!navbar.contains(e.target)) {
                closeNavbar();
                closeAllDropdowns();
                
                // إغلاق قائمة الإشعارات أيضاً
                if (notificationMenu) {
                    notificationMenu.style.display = 'none';
                }
                if (notificationBell) {
                    notificationBell.setAttribute('aria-expanded', 'false');
                }
                return;
            }

            // إغلاق القائمة عند النقر على رابط عادي
            const clickedLink = e.target.closest('.nav-link:not(.dropdown-toggle):not(.language-dropdown):not(.user-dropdown):not(.notification-bell)');
            if (clickedLink) {
                setTimeout(() => {
                    if (window.innerWidth <= 991) {
                        closeNavbar();
                    }
                    closeAllDropdowns();
                }, 100);
            }
        });

        console.log('✅ تم تهيئة معالج النقر الخارجي');
    }

    /**
     * معالج لوحة المفاتيح
     */
    function initKeyboardHandler() {
        document.addEventListener('keydown', function (e) {
            // إغلاق القوائم عند الضغط على Escape
            if (e.key === 'Escape') {
                closeNavbar();
                closeAllDropdowns();
                
                const notificationMenu = document.getElementById('notification-menu');
                if (notificationMenu) {
                    notificationMenu.style.display = 'none';
                }
            }
        });

        console.log('✅ تم تهيئة معالج لوحة المفاتيح');
    }

    /**
     * معالج التمرير
     */
    function initScrollHandler() {
        let lastScrollY = window.scrollY;
        let ticking = false;

        window.addEventListener('scroll', function () {
            if (!ticking) {
                window.requestAnimationFrame(function () {
                    const currentScrollY = window.scrollY;

                    // إغلاق القوائم عند التمرير الكبير
                    if (Math.abs(currentScrollY - lastScrollY) > 100) {
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
     * معالج تغيير حجم الشاشة
     */
    function initResizeHandler() {
        let resizeTimeout;

        window.addEventListener('resize', function () {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function () {
                closeAllDropdowns();

                if (window.innerWidth > 991) {
                    closeNavbar();
                }

                console.log('📐 تم تغيير حجم الشاشة:', window.innerWidth);
            }, 250);
        });

        console.log('✅ تم تهيئة معالج تغيير الحجم');
    }

    // تصدير الدوال للاستخدام الخارجي
    window.UnifiedNav = {
        openNavbar: openNavbar,
        closeNavbar: closeNavbar,
        closeAllDropdowns: closeAllDropdowns,
        reinit: initUnifiedNav
    };

})();
