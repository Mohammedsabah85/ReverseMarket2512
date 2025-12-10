/**
 * Unified Navigation System - نظام التنقل الموحد المحسّن
 * Version: 9.0 - مع مؤشر التمرير التفاعلي
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
        console.log('🚀 تهيئة نظام التنقل الموحد v9.0...');

        // تهيئة جميع المكونات
        initNavbarToggler();
        initLanguageDropdown();
        initUserDropdown();
        initNotificationDropdown();
        createScrollIndicator();
        initOutsideClickHandler();
        initKeyboardHandler();
        initScrollHandler();
        initResizeHandler();

        console.log('✅ تم تهيئة نظام التنقل بنجاح');
    }

    /**
     * إنشاء مؤشر التمرير
     */
    function createScrollIndicator() {
        // إزالة أي مؤشر سابق
        const existingIndicator = document.querySelector('.scroll-indicator');
        if (existingIndicator) {
            existingIndicator.remove();
        }

        // إنشاء مؤشر جديد
        const indicator = document.createElement('div');
        indicator.className = 'scroll-indicator';
        indicator.innerHTML = '<i class="fas fa-chevron-down"></i>';
        indicator.setAttribute('aria-label', 'اضغط للتمرير للأسفل');
        indicator.setAttribute('role', 'button');
        indicator.setAttribute('tabindex', '0');
        document.body.appendChild(indicator);

        // إضافة حدث النقر
        indicator.addEventListener('click', scrollToBottom);
        indicator.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                scrollToBottom();
            }
        });

        console.log('✅ تم إنشاء مؤشر التمرير');
    }

    /**
     * التمرير للأسفل
     */
    function scrollToBottom() {
        const collapse = document.querySelector('.navbar-collapse.show');
        if (!collapse) return;

        const scrollable = getScrollableElement(collapse);
        if (!scrollable) return;

        // حساب المسافة المتبقية
        const scrollRemaining = scrollable.scrollHeight - scrollable.scrollTop - scrollable.clientHeight;
        
        // التمرير بمقدار نصف الشاشة أو إلى النهاية
        const scrollAmount = Math.min(scrollRemaining, scrollable.clientHeight * 0.7);
        
        scrollable.scrollBy({
            top: scrollAmount,
            behavior: 'smooth'
        });
    }

    /**
     * الحصول على العنصر القابل للتمرير
     */
    function getScrollableElement(collapse) {
        // البحث عن العنصر القابل للتمرير
        const candidates = [
            collapse.querySelector('.navbar-nav'),
            collapse.querySelector('.d-flex'),
            collapse.querySelector('ul'),
            collapse.querySelector('div'),
            collapse
        ];

        for (const el of candidates) {
            if (el && el.scrollHeight > el.clientHeight) {
                return el;
            }
        }

        return collapse;
    }

    /**
     * تحديث حالة مؤشر التمرير
     */
    function updateScrollIndicator() {
        const indicator = document.querySelector('.scroll-indicator');
        const collapse = document.querySelector('.navbar-collapse.show');
        
        if (!indicator) return;

        // إخفاء المؤشر إذا كانت القائمة مغلقة أو على شاشة كبيرة
        if (!collapse || window.innerWidth > 991) {
            indicator.classList.remove('visible');
            return;
        }

        const scrollable = getScrollableElement(collapse);
        if (!scrollable) {
            indicator.classList.remove('visible');
            return;
        }

        // حساب ما إذا كان هناك محتوى للتمرير
        const scrollRemaining = scrollable.scrollHeight - scrollable.scrollTop - scrollable.clientHeight;
        
        // إظهار المؤشر إذا كان هناك محتوى أسفل (أكثر من 50 بكسل)
        if (scrollRemaining > 50) {
            indicator.classList.add('visible');
        } else {
            indicator.classList.remove('visible');
        }
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

        // إضافة مستمع التمرير للقائمة
        initMenuScrollListener(collapse);

        console.log('✅ تم تهيئة زر القائمة');
    }

    /**
     * تهيئة مستمع التمرير للقائمة
     */
    function initMenuScrollListener(collapse) {
        const scrollable = getScrollableElement(collapse);
        if (!scrollable) return;

        // إزالة المستمعات السابقة
        scrollable.removeEventListener('scroll', updateScrollIndicator);
        
        // إضافة مستمع جديد
        scrollable.addEventListener('scroll', updateScrollIndicator, { passive: true });
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

        // تهيئة مستمع التمرير وتحديث المؤشر
        setTimeout(() => {
            initMenuScrollListener(collapse);
            updateScrollIndicator();
        }, 100);

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

        // إخفاء مؤشر التمرير
        const indicator = document.querySelector('.scroll-indicator');
        if (indicator) {
            indicator.classList.remove('visible');
        }

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

                // إغلاق القوائم الأخرى
                closeOtherDropdowns(menu);

                if (!isOpen) {
                    menu.classList.add('show');
                    this.setAttribute('aria-expanded', 'true');
                    
                    // التمرير إلى القائمة
                    scrollToElement(menu);
                    
                    // تحديث مؤشر التمرير
                    setTimeout(updateScrollIndicator, 150);
                    
                    console.log('📂 تم فتح قائمة اللغات');
                } else {
                    menu.classList.remove('show');
                    this.setAttribute('aria-expanded', 'false');
                    setTimeout(updateScrollIndicator, 150);
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
                    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>جاري التغيير...';
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
                    setTimeout(updateScrollIndicator, 150);
                    console.log('📂 تم فتح قائمة المستخدم');
                } else {
                    menu.classList.remove('show');
                    this.setAttribute('aria-expanded', 'false');
                    setTimeout(updateScrollIndicator, 150);
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
                console.log('📂 تم فتح قائمة الإشعارات');
            }
        });

        // منع إغلاق القائمة عند النقر داخلها
        notificationMenu.addEventListener('click', function (e) {
            e.stopPropagation();

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

        const scrollable = getScrollableElement(collapse);
        if (!scrollable) return;

        setTimeout(() => {
            const elementRect = element.getBoundingClientRect();
            const containerRect = scrollable.getBoundingClientRect();

            // إذا كان العنصر خارج منطقة الرؤية
            if (elementRect.bottom > containerRect.bottom || elementRect.top < containerRect.top) {
                // التمرير إلى العنصر
                const scrollTop = element.offsetTop - scrollable.offsetTop - 20;
                scrollable.scrollTo({
                    top: scrollTop,
                    behavior: 'smooth'
                });
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
            const scrollIndicator = document.querySelector('.scroll-indicator');

            // استثناء مؤشر التمرير
            if (scrollIndicator && scrollIndicator.contains(e.target)) {
                return;
            }

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
     * معالج التمرير على الصفحة
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
        }, { passive: true });

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

                updateScrollIndicator();

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
        scrollToBottom: scrollToBottom,
        updateScrollIndicator: updateScrollIndicator,
        reinit: initUnifiedNav
    };

})();
