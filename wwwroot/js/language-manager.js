class LanguageManager {
    constructor() {
        this.currentLanguage = document.documentElement.lang || 'ar';
        this.supportedLanguages = ['ar', 'en', 'ku'];
        this.isChanging = false;

        console.log('✅ تم تحميل مدير اللغات - اللغة الحالية:', this.currentLanguage);
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadKurdishStyleIfNeeded();
        this.debugCookies();
    }

    debugCookies() {
        console.log('🍪 الكوكيز الحالية:', document.cookie);
        const cultureCookie = this.getCookie('.AspNetCore.Culture');
        console.log('🌐 كوكي اللغة:', cultureCookie);
    }

    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    setupEventListeners() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.attachFormListeners());
        } else {
            this.attachFormListeners();
        }
    }

    attachFormListeners() {
        const forms = document.querySelectorAll('.language-form');
        console.log('📝 عدد نماذج اللغة المكتشفة:', forms.length);

        forms.forEach((form, index) => {
            const culture = form.querySelector('input[name="culture"]')?.value;
            const button = form.querySelector('button[type="submit"]');

            console.log(`نموذج ${index + 1}:`, {
                culture,
                hasButton: !!button,
                formAction: form.action
            });

            form.addEventListener('submit', (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('🔄 محاولة تغيير اللغة إلى:', culture);
                this.changeLanguage(form);
            });
        });
    }

    loadKurdishStyleIfNeeded() {
        if (this.currentLanguage === 'ku') {
            if (!document.querySelector('link[href*="kurdish-rtl"]')) {
                const link = document.createElement('link');
                link.rel = 'stylesheet';
                link.href = '/css/kurdish-rtl.css';
                document.head.appendChild(link);
                console.log('✅ تم تحميل ملف CSS الكردي');
            }
        }
    }

    async changeLanguage(form) {
        if (this.isChanging) {
            console.warn('⚠️ تغيير اللغة قيد التنفيذ بالفعل');
            return;
        }

        this.isChanging = true;

        const formData = new FormData(form);
        const culture = formData.get('culture');
        const returnUrl = formData.get('returnUrl') || window.location.href;
        const token = formData.get('__RequestVerificationToken');

        console.log('📤 بيانات الطلب:', {
            culture,
            returnUrl,
            hasToken: !!token,
            currentLanguage: this.currentLanguage,
            formAction: form.action
        });

        if (!this.supportedLanguages.includes(culture)) {
            console.error('❌ لغة غير مدعومة:', culture);
            this.showError('اللغة غير مدعومة');
            this.isChanging = false;
            return;
        }

        if (culture === this.currentLanguage) {
            console.log('ℹ️ اللغة مختارة بالفعل');
            this.isChanging = false;
            return;
        }

        try {
            this.showLoading(form, culture);

            console.log('🚀 إرسال الطلب إلى:', form.action);

            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            console.log('📥 الاستجابة:', {
                status: response.status,
                ok: response.ok,
                statusText: response.statusText,
                headers: Object.fromEntries(response.headers.entries())
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('❌ خطأ في الاستجابة:', errorText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const contentType = response.headers.get('content-type');
            console.log('📋 نوع المحتوى:', contentType);

            let data;
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
                console.log('📊 البيانات المستلمة:', data);
            } else {
                const text = await response.text();
                console.log('📄 نص الاستجابة:', text.substring(0, 200));
                throw new Error('الاستجابة ليست JSON');
            }

            if (data.success) {
                console.log('✅ نجح تغيير اللغة! إعادة التوجيه إلى:', data.redirectUrl);

                // التحقق من الكوكي قبل إعادة التحميل
                setTimeout(() => {
                    console.log('🍪 الكوكيز بعد التغيير:', document.cookie);
                    window.location.href = data.redirectUrl || returnUrl;
                }, 100);
            } else {
                throw new Error(data.message || 'فشل تغيير اللغة');
            }

        } catch (error) {
            console.error('❌ خطأ في تغيير اللغة:', {
                message: error.message,
                stack: error.stack
            });

            this.showError(this.getErrorMessage(error.message));
            this.hideLoading(form);
            this.isChanging = false;
        }
    }

    showLoading(form, culture) {
        const button = form.querySelector('button[type="submit"]');
        if (!button) return;

        button.disabled = true;
        button.dataset.original = button.innerHTML;

        const loadingText = {
            'ar': 'جاري التحميل...',
            'en': 'Loading...',
            'ku': 'بارکردن...'
        }[culture] || 'جاري التحميل...';

        button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i>${loadingText}`;

        document.querySelectorAll('.language-form button').forEach(btn => {
            btn.disabled = true;
        });
    }

    hideLoading(form) {
        const button = form.querySelector('button[type="submit"]');
        if (button && button.dataset.original) {
            button.innerHTML = button.dataset.original;
            button.disabled = false;
            delete button.dataset.original;
        }

        document.querySelectorAll('.language-form button').forEach(btn => {
            btn.disabled = false;
        });
    }

    getErrorMessage(specificError = '') {
        const messages = {
            'ar': specificError || 'حدث خطأ في تغيير اللغة. يرجى المحاولة مرة أخرى.',
            'en': specificError || 'Error changing language. Please try again.',
            'ku': specificError || 'هەڵەیەک ڕوویدا. تکایە دووبارە هەوڵ بدەرەوە.'
        };
        return messages[this.currentLanguage] || messages['ar'];
    }

    showError(message) {
        document.querySelectorAll('.language-error').forEach(el => el.remove());

        const alert = document.createElement('div');
        alert.className = 'alert alert-danger alert-dismissible fade show position-fixed language-error';
        alert.style.cssText = 'top: 80px; right: 20px; left: 20px; z-index: 9999; max-width: 500px; margin: 0 auto; box-shadow: 0 4px 20px rgba(0,0,0,0.3);';
        alert.innerHTML = `
            <i class="fas fa-exclamation-circle me-2"></i>
            <strong>خطأ:</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(alert);

        setTimeout(() => {
            if (alert.parentNode) {
                alert.classList.remove('show');
                setTimeout(() => alert.remove(), 150);
            }
        }, 5000);
    }

    getCurrentLanguage() {
        return this.currentLanguage;
    }
}

// التهيئة
console.log('🚀 بدء تحميل مدير اللغات...');

let languageManager;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        languageManager = new LanguageManager();
        window.languageManager = languageManager;
        console.log('✅ تم تهيئة مدير اللغات عند DOMContentLoaded');
    });
} else {
    languageManager = new LanguageManager();
    window.languageManager = languageManager;
    console.log('✅ تم تهيئة مدير اللغات مباشرة');
}

window.LanguageManager = LanguageManager;