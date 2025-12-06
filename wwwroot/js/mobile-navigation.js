/**
 * تحسين التنقل في الموبايل
 * Mobile Navigation Enhancement
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 تحميل تحسينات التنقل للموبايل...');
    
    // تحسين القوائم المنسدلة
    initializeDropdowns();
    
    // تحسين جرس الإشعارات
    initializeNotificationBell();
    
    // تحسين قائمة اللغات
    initializeLanguageSelector();
    
    // تحسين قائمة المستخدم
    initializeUserMenu();
    
    // إضافة مستمعات الأحداث للموبايل
    addMobileEventListeners();
});

/**
 * تهيئة القوائم المنسدلة
 */
function initializeDropdowns() {
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    
    dropdowns.forEach(dropdown => {
        // إضافة مستمع النقر
        dropdown.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const dropdownMenu = this.nextElementSibling;
            if (dropdownMenu && dropdownMenu.classList.contains('dropdown-menu')) {
                toggleDropdown(dropdownMenu);
            }
        });
        
        // تحسين التفاعل مع اللمس
        dropdown.addEventListener('touchstart', function(e) {
            this.classList.add('touching');
        });
        
        dropdown.addEventListener('touchend', function(e) {
            this.classList.remove('touching');
        });
    });
}

/**
 * تبديل حالة القائمة المنسدلة
 */
function toggleDropdown(dropdownMenu) {
    const isOpen = dropdownMenu.classList.contains('show');
    
    // إغلاق جميع القوائم المفتوحة
    closeAllDropdowns();
    
    if (!isOpen) {
        // فتح القائمة المحددة
        dropdownMenu.classList.add('show');
        dropdownMenu.style.display = 'block';
        
        // إضافة خلفية شفافة
        addBackdrop(dropdownMenu);
        
        console.log('✅ تم فتح القائمة المنسدلة');
    }
}

/**
 * إغلاق جميع القوائم المنسدلة
 */
function closeAllDropdowns() {
    const openDropdowns = document.querySelectorAll('.dropdown-menu.show');
    
    openDropdowns.forEach(dropdown => {
        dropdown.classList.remove('show');
        dropdown.style.display = 'none';
    });
    
    // إزالة الخلفية الشفافة
    removeBackdrop();
    
    console.log('🔒 تم إغلاق جميع القوائم المنسدلة');
}

/**
 * إضافة خلفية شفافة
 */
function addBackdrop(dropdownMenu) {
    // إزالة الخلفية الموجودة إن وجدت
    removeBackdrop();
    
    const backdrop = document.createElement('div');
    backdrop.className = 'dropdown-backdrop';
    backdrop.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.3);
        z-index: 1040;
        backdrop-filter: blur(2px);
    `;
    
    // إغلاق القائمة عند النقر على الخلفية
    backdrop.addEventListener('click', closeAllDropdowns);
    backdrop.addEventListener('touchstart', closeAllDropdowns);
    
    document.body.appendChild(backdrop);
}

/**
 * إزالة الخلفية الشفافة
 */
function removeBackdrop() {
    const backdrop = document.querySelector('.dropdown-backdrop');
    if (backdrop) {
        backdrop.remove();
    }
}

/**
 * تهيئة جرس الإشعارات
 */
function initializeNotificationBell() {
    const notificationBell = document.querySelector('.notification-bell');
    
    if (notificationBell) {
        notificationBell.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            console.log('🔔 تم النقر على جرس الإشعارات');
            
            // تحديث الإشعارات
            if (typeof updateNotifications === 'function') {
                updateNotifications();
            }
            
            // فتح قائمة الإشعارات
            const notificationDropdown = document.querySelector('#notificationDropdown');
            if (notificationDropdown) {
                toggleDropdown(notificationDropdown);
            } else {
                // الانتقال إلى صفحة الإشعارات
                window.location.href = '/Notifications';
            }
        });
        
        // تحسين التفاعل البصري
        notificationBell.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.05)';
        });
        
        notificationBell.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    }
}

/**
 * تهيئة قائمة اللغات
 */
function initializeLanguageSelector() {
    const languageDropdown = document.querySelector('#languageMenu');
    
    if (languageDropdown) {
        languageDropdown.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const dropdownMenu = document.querySelector('#languageDropdown');
            if (dropdownMenu) {
                toggleDropdown(dropdownMenu);
            }
        });
    }
    
    // تحسين أزرار اللغة
    const languageButtons = document.querySelectorAll('.language-form button');
    languageButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            // إضافة تأثير التحميل
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جاري التغيير...';
            this.disabled = true;
            
            // إرسال النموذج
            setTimeout(() => {
                this.closest('form').submit();
            }, 100);
        });
    });
}

/**
 * تهيئة قائمة المستخدم
 */
function initializeUserMenu() {
    const userDropdown = document.querySelector('#userMenu');
    
    if (userDropdown) {
        userDropdown.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const dropdownMenu = this.nextElementSibling;
            if (dropdownMenu && dropdownMenu.classList.contains('dropdown-menu')) {
                toggleDropdown(dropdownMenu);
            }
        });
    }
}

/**
 * إضافة مستمعات الأحداث للموبايل
 */
function addMobileEventListeners() {
    // إغلاق القوائم عند النقر خارجها
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.dropdown')) {
            closeAllDropdowns();
        }
    });
    
    // إغلاق القوائم عند الضغط على Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeAllDropdowns();
        }
    });
    
    // تحسين التمرير
    let isScrolling = false;
    window.addEventListener('scroll', function() {
        if (!isScrolling) {
            closeAllDropdowns();
            isScrolling = true;
            
            setTimeout(() => {
                isScrolling = false;
            }, 100);
        }
    });
    
    // تحسين تغيير الاتجاه
    window.addEventListener('orientationchange', function() {
        setTimeout(() => {
            closeAllDropdowns();
        }, 100);
    });
    
    // تحسين تغيير حجم النافذة
    window.addEventListener('resize', function() {
        closeAllDropdowns();
    });
}

/**
 * تحسين أداء اللمس
 */
function optimizeTouchPerformance() {
    // تعطيل التمرير المطاطي في iOS
    document.addEventListener('touchmove', function(e) {
        if (e.target.closest('.dropdown-menu')) {
            e.preventDefault();
        }
    }, { passive: false });
    
    // تحسين استجابة اللمس
    const touchElements = document.querySelectorAll('.nav-link, .dropdown-item, .btn');
    touchElements.forEach(element => {
        element.style.webkitTapHighlightColor = 'transparent';
        element.style.touchAction = 'manipulation';
    });
}

/**
 * إضافة تأثيرات بصرية
 */
function addVisualEffects() {
    // تأثير الموجة عند النقر
    const clickableElements = document.querySelectorAll('.btn, .nav-link, .dropdown-item');
    
    clickableElements.forEach(element => {
        element.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            ripple.className = 'ripple-effect';
            
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                left: ${x}px;
                top: ${y}px;
                background: rgba(255, 255, 255, 0.3);
                border-radius: 50%;
                transform: scale(0);
                animation: ripple 0.6s ease-out;
                pointer-events: none;
            `;
            
            this.style.position = 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
    
    // إضافة CSS للتأثير
    const style = document.createElement('style');
    style.textContent = `
        @keyframes ripple {
            to {
                transform: scale(2);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
}

// تهيئة التحسينات الإضافية
document.addEventListener('DOMContentLoaded', function() {
    optimizeTouchPerformance();
    addVisualEffects();
});

// تصدير الدوال للاستخدام العام
window.MobileNavigation = {
    closeAllDropdowns,
    toggleDropdown,
    initializeDropdowns,
    initializeNotificationBell
};