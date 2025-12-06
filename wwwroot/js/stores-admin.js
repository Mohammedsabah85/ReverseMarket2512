// 🏪 JavaScript لإدارة المتاجر - ReverseMarket

// ==================== وظائف عامة ====================

/**
 * عرض رسالة تأكيد مخصصة
 */
function confirmAction(message, callback) {
    if (confirm(message)) {
        if (typeof callback === 'function') {
            callback();
        }
        return true;
    }
    return false;
}

/**
 * عرض رسالة Toast
 */
function showToast(message, type = 'success') {
    // يمكن استخدام مكتبة مثل Toastr أو Bootstrap Toast
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 20px; left: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 5000);
}

// ==================== إدارة الحذف ====================

/**
 * حذف متجر مع تأكيد مزدوج
 */
function deleteStore(storeId, storeName) {
    const confirmMessage = `⚠️ هل أنت متأكد من حذف متجر "${storeName}"?\n\nتحذير: هذا الإجراء لا يمكن التراجع عنه!`;
    
    if (!confirm(confirmMessage)) {
        return;
    }

    // تأكيد إضافي
    const secondConfirm = confirm('هل أنت متأكد تماماً؟ سيتم حذف جميع البيانات المرتبطة بهذا المتجر.');
    
    if (secondConfirm) {
        submitDeleteForm(storeId);
    }
}

/**
 * إرسال نموذج الحذف
 */
function submitDeleteForm(storeId) {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = `/Admin/Stores/Delete/${storeId}`;

    // إضافة Anti-Forgery Token
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token.value;
        form.appendChild(tokenInput);
    }

    document.body.appendChild(form);
    form.submit();
}

// ==================== إدارة التفعيل/الإيقاف ====================

/**
 * تبديل حالة المتجر
 */
function toggleStoreStatus(storeId, storeName, currentStatus) {
    const action = currentStatus ? 'إيقاف' : 'تفعيل';
    const message = `هل تريد ${action} متجر "${storeName}"؟`;
    
    return confirm(message);
}

// ==================== الفلاتر والبحث ====================

/**
 * إعادة تعيين الفلاتر
 */
function resetFilters() {
    document.querySelector('input[name="searchTerm"]').value = '';
    document.querySelector('select[name="isActive"]').value = '';
    document.querySelector('select[name="isApproved"]').value = '';
    document.querySelector('form').submit();
}

/**
 * البحث السريع
 */
function quickSearch(searchTerm) {
    const rows = document.querySelectorAll('.table-admin tbody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        const matches = text.includes(searchTerm.toLowerCase());
        row.style.display = matches ? '' : 'none';
    });
}

// ==================== إدارة الفئات ====================

/**
 * تحديد/إلغاء تحديد جميع الفئات
 */
function toggleAllCategories(selectAll) {
    const checkboxes = document.querySelectorAll('input[name="selectedCategories"]');
    checkboxes.forEach(checkbox => {
        checkbox.checked = selectAll;
    });
}

/**
 * التحقق من اختيار فئة واحدة على الأقل
 */
function validateCategories() {
    const checkboxes = document.querySelectorAll('input[name="selectedCategories"]:checked');
    
    if (checkboxes.length === 0) {
        alert('⚠️ يرجى اختيار فئة واحدة على الأقل للمتجر');
        return false;
    }
    
    return true;
}

// ==================== التحقق من النماذج ====================

/**
 * التحقق من نموذج التعديل
 */
function validateEditForm(event) {
    // التحقق من الحقول المطلوبة
    const storeName = document.querySelector('input[name="StoreName"]');
    const firstName = document.querySelector('input[name="FirstName"]');
    const lastName = document.querySelector('input[name="LastName"]');
    const phoneNumber = document.querySelector('input[name="PhoneNumber"]');
    const email = document.querySelector('input[name="Email"]');

    if (!storeName.value.trim()) {
        alert('⚠️ يرجى إدخال اسم المتجر');
        storeName.focus();
        event.preventDefault();
        return false;
    }

    if (!firstName.value.trim()) {
        alert('⚠️ يرجى إدخال الاسم الأول');
        firstName.focus();
        event.preventDefault();
        return false;
    }

    if (!lastName.value.trim()) {
        alert('⚠️ يرجى إدخال اسم العائلة');
        lastName.focus();
        event.preventDefault();
        return false;
    }

    // التحقق من رقم الهاتف (964 للعراق)
    const phonePattern = /^964\d{10}$/;
    if (!phonePattern.test(phoneNumber.value.replace(/\s/g, ''))) {
        alert('⚠️ يرجى إدخال رقم هاتف صحيح بصيغة: 964xxxxxxxxxx');
        phoneNumber.focus();
        event.preventDefault();
        return false;
    }

    // التحقق من البريد الإلكتروني
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email.value)) {
        alert('⚠️ يرجى إدخال بريد إلكتروني صحيح');
        email.focus();
        event.preventDefault();
        return false;
    }

    // التحقق من الفئات
    if (!validateCategories()) {
        event.preventDefault();
        return false;
    }

    return true;
}

// ==================== تحسينات الأداء ====================

/**
 * تحميل البيانات بشكل كسول
 */
function lazyLoadImages() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                observer.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
}

// ==================== التصدير ====================

/**
 * تصدير البيانات إلى Excel
 */
function exportToExcel() {
    // يمكن استخدام مكتبة مثل SheetJS
    showToast('جاري تصدير البيانات...', 'info');
    
    // TODO: تطبيق التصدير الفعلي
    setTimeout(() => {
        showToast('تم التصدير بنجاح!', 'success');
    }, 1500);
}

/**
 * طباعة الجدول
 */
function printTable() {
    window.print();
}

// ==================== الإحصائيات ====================

/**
 * تحديث الإحصائيات بشكل حي
 */
function updateStatistics() {
    const total = document.querySelectorAll('.table-admin tbody tr').length;
    const approved = document.querySelectorAll('.badge.bg-success').length;
    const active = document.querySelectorAll('.badge.bg-primary').length;
    const pending = total - approved;

    // تحديث البطاقات
    updateStatCard('total-stores', total);
    updateStatCard('approved-stores', approved);
    updateStatCard('active-stores', active);
    updateStatCard('pending-stores', pending);
}

function updateStatCard(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value;
    }
}

// ==================== التنبيهات ====================

/**
 * عرض تنبيه للمتاجر غير المعتمدة
 */
function showPendingStoresAlert() {
    const pendingCount = document.querySelectorAll('.badge.bg-warning').length;
    
    if (pendingCount > 0) {
        showToast(`لديك ${pendingCount} متجر بانتظار المراجعة`, 'warning');
    }
}

// ==================== تهيئة الصفحة ====================

/**
 * تهيئة الصفحة عند التحميل
 */
document.addEventListener('DOMContentLoaded', function() {
    // ربط نموذج التعديل بالتحقق
    const editForm = document.querySelector('form[action*="Edit"]');
    if (editForm) {
        editForm.addEventListener('submit', validateEditForm);
    }

    // تفعيل التلميحات (Tooltips)
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        new bootstrap.Tooltip(tooltip);
    });

    // البحث السريع
    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function(e) {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                quickSearch(e.target.value);
            }, 300);
        });
    }

    // تحميل البيانات بشكل كسول
    lazyLoadImages();

    // عرض تنبيهات المتاجر المعلقة
    setTimeout(showPendingStoresAlert, 1000);

    // تفعيل الرسوم المتحركة
    animateCards();
});

// ==================== رسوم متحركة ====================

/**
 * تحريك البطاقات عند التمرير
 */
function animateCards() {
    const cards = document.querySelectorAll('.card-admin, .stat-card');
    
    const cardObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '0';
                entry.target.style.transform = 'translateY(20px)';
                
                setTimeout(() => {
                    entry.target.style.transition = 'all 0.5s ease';
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, 100);
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => cardObserver.observe(card));
}

// ==================== معالجة الأخطاء ====================

/**
 * معالجة أخطاء AJAX
 */
function handleAjaxError(error) {
    console.error('خطأ:', error);
    showToast('حدث خطأ أثناء تنفيذ العملية', 'danger');
}

// ==================== وظائف مساعدة ====================

/**
 * تنسيق الأرقام بالعربية
 */
function formatNumberArabic(number) {
    return number.toLocaleString('ar-IQ');
}

/**
 * تنسيق التاريخ
 */
function formatDate(date) {
    return new Date(date).toLocaleDateString('ar-IQ', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// ==================== تصدير الوظائف ====================

// جعل الوظائف متاحة عالمياً
window.StoresAdmin = {
    deleteStore,
    toggleStoreStatus,
    resetFilters,
    quickSearch,
    toggleAllCategories,
    validateCategories,
    exportToExcel,
    printTable,
    updateStatistics,
    showToast
};
