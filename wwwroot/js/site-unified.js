/**
 * REVERSE MARKET - JavaScript الموحد
 * Version: 4.0 Final
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 تهيئة الموقع...');
    
    // تهيئة جميع المكونات
    initNavbar();
    initBackToTop();
    initCarousel();
    initDropdowns();
    initAlerts();
    initAnimations();
    initForms();
    
    console.log('✅ تم تهيئة الموقع بنجاح');
});

/**
 * تهيئة النافبار والقائمة المتنقلة
 */
function initNavbar() {
    const toggler = document.querySelector('.navbar-toggler');
    const collapse = document.querySelector('.navbar-collapse');
    
    if (toggler && collapse) {
        toggler.addEventListener('click', function() {
            collapse.classList.toggle('show');
            this.classList.toggle('active');
        });
        
        // إغلاق القائمة عند النقر خارجها
        document.addEventListener('click', function(e) {
            if (!toggler.contains(e.target) && !collapse.contains(e.target)) {
                collapse.classList.remove('show');
                toggler.classList.remove('active');
            }
        });
    }
    
    // تأثير الظل عند التمرير
    const header = document.querySelector('.main-header');
    if (header) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                header.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.1)';
            } else {
                header.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.07)';
            }
        });
    }
}

/**
 * زر العودة للأعلى
 */
function initBackToTop() {
    const btn = document.getElementById('backToTop');
    if (!btn) return;
    
    window.addEventListener('scroll', function() {
        if (window.scrollY > 300) {
            btn.style.display = 'flex';
        } else {
            btn.style.display = 'none';
        }
    });
    
    btn.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

/**
 * تهيئة الـ Carousel للإعلانات
 */
function initCarousel() {
    const carousels = document.querySelectorAll('.carousel');
    
    carousels.forEach(carousel => {
        const items = carousel.querySelectorAll('.carousel-item');
        const indicators = carousel.querySelectorAll('.carousel-indicators button');
        const prevBtn = carousel.querySelector('.carousel-control-prev');
        const nextBtn = carousel.querySelector('.carousel-control-next');
        
        if (items.length === 0) return;
        
        let currentIndex = 0;
        let autoplayInterval;
        
        function showSlide(index) {
            items.forEach((item, i) => {
                item.classList.remove('active');
                if (indicators[i]) indicators[i].classList.remove('active');
            });
            
            items[index].classList.add('active');
            if (indicators[index]) indicators[index].classList.add('active');
            currentIndex = index;
        }
        
        function nextSlide() {
            let next = currentIndex + 1;
            if (next >= items.length) next = 0;
            showSlide(next);
        }
        
        function prevSlide() {
            let prev = currentIndex - 1;
            if (prev < 0) prev = items.length - 1;
            showSlide(prev);
        }
        
        function startAutoplay() {
            autoplayInterval = setInterval(nextSlide, 5000);
        }
        
        function stopAutoplay() {
            clearInterval(autoplayInterval);
        }
        
        // أزرار التحكم
        if (nextBtn) nextBtn.addEventListener('click', function() {
            nextSlide();
            stopAutoplay();
            startAutoplay();
        });
        
        if (prevBtn) prevBtn.addEventListener('click', function() {
            prevSlide();
            stopAutoplay();
            startAutoplay();
        });
        
        // المؤشرات
        indicators.forEach((indicator, i) => {
            indicator.addEventListener('click', function() {
                showSlide(i);
                stopAutoplay();
                startAutoplay();
            });
        });
        
        // إيقاف عند التمرير فوقه
        carousel.addEventListener('mouseenter', stopAutoplay);
        carousel.addEventListener('mouseleave', startAutoplay);
        
        // دعم اللمس
        let touchStartX = 0;
        carousel.addEventListener('touchstart', function(e) {
            touchStartX = e.touches[0].clientX;
            stopAutoplay();
        }, { passive: true });
        
        carousel.addEventListener('touchend', function(e) {
            const touchEndX = e.changedTouches[0].clientX;
            const diff = touchStartX - touchEndX;
            
            if (Math.abs(diff) > 50) {
                if (diff > 0) nextSlide();
                else prevSlide();
            }
            startAutoplay();
        }, { passive: true });
        
        // بدء التشغيل التلقائي
        startAutoplay();
    });
}

/**
 * تهيئة القوائم المنسدلة
 */
function initDropdowns() {
    const dropdowns = document.querySelectorAll('.dropdown');
    
    dropdowns.forEach(dropdown => {
        const toggle = dropdown.querySelector('.dropdown-toggle');
        const menu = dropdown.querySelector('.dropdown-menu');
        
        if (!toggle || !menu) return;
        
        // للموبايل - نقرة
        toggle.addEventListener('click', function(e) {
            if (window.innerWidth <= 992) {
                e.preventDefault();
                menu.classList.toggle('show');
            }
        });
    });
    
    // إغلاق القوائم عند النقر خارجها
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                menu.classList.remove('show');
            });
        }
    });
}

/**
 * تهيئة التنبيهات - إغلاق تلقائي
 */
function initAlerts() {
    const alerts = document.querySelectorAll('.alert');
    
    alerts.forEach(alert => {
        // زر الإغلاق
        const closeBtn = alert.querySelector('.btn-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', function() {
                alert.style.animation = 'fadeOut 0.3s ease';
                setTimeout(() => alert.remove(), 300);
            });
        }
        
        // إغلاق تلقائي بعد 5 ثواني
        setTimeout(() => {
            if (alert.parentElement) {
                alert.style.animation = 'fadeOut 0.3s ease';
                setTimeout(() => alert.remove(), 300);
            }
        }, 5000);
    });
}

/**
 * تهيئة الحركات
 */
function initAnimations() {
    // حركة الظهور عند التمرير
    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    // تطبيق على البطاقات
    const animatedElements = document.querySelectorAll('.category-card, .request-card, .step-card, .card-custom');
    animatedElements.forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = `all 0.5s ease ${index * 0.1}s`;
        observer.observe(el);
    });
}

/**
 * تهيئة النماذج
 */
function initForms() {
    // تحقق من صحة النماذج
    const forms = document.querySelectorAll('form[data-validate]');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            const requiredFields = form.querySelectorAll('[required]');
            
            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    isValid = false;
                    field.classList.add('is-invalid');
                } else {
                    field.classList.remove('is-invalid');
                }
            });
            
            if (!isValid) {
                e.preventDefault();
                showToast('يرجى ملء جميع الحقول المطلوبة', 'warning');
            }
        });
    });
    
    // تحسين حقول الإدخال
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused');
        });
    });
}

/**
 * عرض رسالة Toast
 */
function showToast(message, type = 'info') {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }
    
    const colors = {
        success: '#38A169',
        danger: '#E53E3E',
        warning: '#DD6B20',
        info: '#3182CE'
    };
    
    const icons = {
        success: 'fa-check-circle',
        danger: 'fa-times-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };
    
    const toast = document.createElement('div');
    toast.className = 'toast align-items-center border-0 show';
    toast.style.cssText = `
        background: white;
        border-right: 4px solid ${colors[type]};
        margin-bottom: 10px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.15);
        border-radius: 8px;
        min-width: 300px;
    `;
    
    toast.innerHTML = `
        <div class="d-flex align-items-center p-3">
            <i class="fas ${icons[type]} me-3" style="color: ${colors[type]}; font-size: 1.25rem;"></i>
            <div class="flex-grow-1">${message}</div>
            <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    container.appendChild(toast);
    
    // إزالة تلقائية
    setTimeout(() => {
        toast.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

/**
 * تأكيد الحذف
 */
function confirmDelete(message = 'هل أنت متأكد من الحذف؟') {
    return confirm(message);
}

/**
 * تبديل حالة العنصر
 */
function toggleStatus(element, activeClass = 'active') {
    element.classList.toggle(activeClass);
}

/**
 * نسخ للحافظة
 */
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showToast('تم النسخ بنجاح!', 'success');
    }).catch(() => {
        showToast('فشل النسخ', 'danger');
    });
}

/**
 * تنسيق الأرقام
 */
function formatNumber(num) {
    return new Intl.NumberFormat('ar-IQ').format(num);
}

/**
 * تنسيق التاريخ
 */
function formatDate(date) {
    return new Intl.DateTimeFormat('ar-IQ', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// جعل الدوال متاحة عالمياً
window.showToast = showToast;
window.confirmDelete = confirmDelete;
window.toggleStatus = toggleStatus;
window.copyToClipboard = copyToClipboard;
window.formatNumber = formatNumber;
window.formatDate = formatDate;

// إضافة CSS للـ fadeOut
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        from { opacity: 1; transform: translateY(0); }
        to { opacity: 0; transform: translateY(-20px); }
    }
    
    .is-invalid {
        border-color: #E53E3E !important;
        box-shadow: 0 0 0 3px rgba(229, 62, 62, 0.15) !important;
    }
    
    .focused label {
        color: #C9A227;
    }
`;
document.head.appendChild(style);

console.log('📦 تم تحميل جميع الدوال بنجاح');
