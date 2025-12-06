/**
 * Advertisement Manager V3.0
 * نظام إدارة الإعلانات - نسخة محسّنة
 */

class AdvertisementManager {
    constructor(options = {}) {
        this.options = {
            autoplayInterval: options.autoplayInterval || 5000,
            pauseOnHover: options.pauseOnHover !== false,
            lazyLoad: options.lazyLoad !== false,
            enableSwipe: options.enableSwipe !== false,
            ...options
        };

        this.carousels = [];
        this.init();
    }

    init() {
        console.log('🎬 تهيئة مدير الإعلانات...');
        
        this.initCarousels();
        this.initLazyLoading();
        this.initSwipeGestures();
        this.initClickHandlers();
        this.initViewTracking();
        
        console.log('✅ تم تهيئة مدير الإعلانات بنجاح');
    }

    /**
     * تهيئة الـ Carousels
     */
    initCarousels() {
        const carouselElements = document.querySelectorAll('.advertisement-carousel');
        
        carouselElements.forEach(element => {
            try {
                const carousel = new bootstrap.Carousel(element, {
                    interval: this.options.autoplayInterval,
                    pause: this.options.pauseOnHover ? 'hover' : false,
                    wrap: true,
                    touch: this.options.enableSwipe
                });

                this.carousels.push({
                    element,
                    instance: carousel
                });

                // إيقاف التشغيل التلقائي عند التفاعل
                if (this.options.pauseOnHover) {
                    element.addEventListener('mouseenter', () => {
                        carousel.pause();
                    });

                    element.addEventListener('mouseleave', () => {
                        carousel.cycle();
                    });
                }

                console.log('✅ تم تهيئة carousel:', element.id || 'unnamed');
            } catch (error) {
                console.error('❌ خطأ في تهيئة carousel:', error);
            }
        });
    }

    /**
     * Lazy Loading للصور
     */
    initLazyLoading() {
        if (!this.options.lazyLoad) return;

        const images = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        this.loadImage(img);
                        observer.unobserve(img);
                    }
                });
            }, {
                rootMargin: '50px 0px',
                threshold: 0.01
            });

            images.forEach(img => imageObserver.observe(img));
            console.log(`🖼️ تم تفعيل Lazy Loading لـ ${images.length} صورة`);
        } else {
            // Fallback للمتصفحات القديمة
            images.forEach(img => this.loadImage(img));
        }
    }

    loadImage(img) {
        const src = img.dataset.src;
        if (!src) return;

        img.src = src;
        img.classList.add('advertisement-loading');
        
        img.onload = () => {
            img.classList.remove('advertisement-loading');
            img.classList.add('loaded');
        };

        img.onerror = () => {
            img.classList.remove('advertisement-loading');
            img.src = '/images/placeholder-ad.png'; // صورة بديلة
        };
    }

    /**
     * دعم الـ Swipe للموبايل
     */
    initSwipeGestures() {
        if (!this.options.enableSwipe) return;

        this.carousels.forEach(({ element, instance }) => {
            let touchStartX = 0;
            let touchEndX = 0;

            element.addEventListener('touchstart', (e) => {
                touchStartX = e.changedTouches[0].screenX;
            }, { passive: true });

            element.addEventListener('touchend', (e) => {
                touchEndX = e.changedTouches[0].screenX;
                this.handleSwipe(touchStartX, touchEndX, instance);
            }, { passive: true });
        });

        console.log('👆 تم تفعيل دعم Swipe');
    }

    handleSwipe(startX, endX, carousel) {
        const minSwipeDistance = 50;
        const distance = endX - startX;

        if (Math.abs(distance) < minSwipeDistance) return;

        if (distance > 0) {
            carousel.prev(); // Swipe right
        } else {
            carousel.next(); // Swipe left
        }
    }

    /**
     * معالجات النقر على الإعلانات
     */
    initClickHandlers() {
        // معالجة نقرات البطاقات
        document.querySelectorAll('.advertisement-card').forEach(card => {
            card.addEventListener('click', (e) => {
                const link = card.dataset.link;
                if (link && !e.target.closest('button')) {
                    this.trackClick(card.dataset.adId, link);
                    window.location.href = link;
                }
            });
        });

        // معالجة نقرات الـ Banner
        document.querySelectorAll('.advertisement-banner').forEach(banner => {
            banner.addEventListener('click', (e) => {
                const link = banner.dataset.link;
                if (link && !e.target.closest('button')) {
                    this.trackClick(banner.dataset.adId, link);
                    window.location.href = link;
                }
            });
        });
    }

    /**
     * تتبع المشاهدات
     */
    initViewTracking() {
        const ads = document.querySelectorAll('[data-ad-id]');
        
        if ('IntersectionObserver' in window) {
            const viewObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const adId = entry.target.dataset.adId;
                        if (adId && !entry.target.dataset.viewed) {
                            this.trackView(adId);
                            entry.target.dataset.viewed = 'true';
                        }
                    }
                });
            }, {
                threshold: 0.5
            });

            ads.forEach(ad => viewObserver.observe(ad));
            console.log(`👁️ تم تفعيل تتبع المشاهدات لـ ${ads.length} إعلان`);
        }
    }

    /**
     * تتبع المشاهدة
     */
    trackView(adId) {
        if (!adId) return;

        fetch('/Advertisements/TrackView', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ adId })
        }).catch(err => {
            console.warn('⚠️ فشل تتبع المشاهدة:', err);
        });

        console.log('👁️ تم تتبع مشاهدة الإعلان:', adId);
    }

    /**
     * تتبع النقرة
     */
    trackClick(adId, link) {
        if (!adId) return;

        fetch('/Advertisements/TrackClick', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ adId, link })
        }).catch(err => {
            console.warn('⚠️ فشل تتبع النقرة:', err);
        });

        console.log('🖱️ تم تتبع نقرة على الإعلان:', adId);
    }

    /**
     * إيقاف جميع الـ Carousels
     */
    pauseAll() {
        this.carousels.forEach(({ instance }) => {
            instance.pause();
        });
    }

    /**
     * تشغيل جميع الـ Carousels
     */
    playAll() {
        this.carousels.forEach(({ instance }) => {
            instance.cycle();
        });
    }

    /**
     * تحديث إعلان معين
     */
    updateAd(adId, newData) {
        const adElement = document.querySelector(`[data-ad-id="${adId}"]`);
        if (!adElement) return;

        if (newData.image) {
            const img = adElement.querySelector('img');
            if (img) img.src = newData.image;
        }

        if (newData.title) {
            const title = adElement.querySelector('.advertisement-card-title, .carousel-caption h3');
            if (title) title.textContent = newData.title;
        }

        if (newData.description) {
            const desc = adElement.querySelector('.advertisement-card-description, .carousel-caption p');
            if (desc) desc.textContent = newData.description;
        }

        console.log('🔄 تم تحديث الإعلان:', adId);
    }

    /**
     * إضافة إعلان جديد
     */
    addAd(containerSelector, adData) {
        const container = document.querySelector(containerSelector);
        if (!container) return;

        const adHtml = this.createAdHTML(adData);
        container.insertAdjacentHTML('beforeend', adHtml);

        // إعادة تهيئة Lazy Loading للصور الجديدة
        this.initLazyLoading();

        console.log('➕ تم إضافة إعلان جديد');
    }

    /**
     * إنشاء HTML للإعلان
     */
    createAdHTML(data) {
        return `
            <div class="advertisement-card" data-ad-id="${data.id}" data-link="${data.link}">
                ${data.badge ? `<span class="advertisement-badge ${data.badgeType}">${data.badge}</span>` : ''}
                <img src="/images/placeholder.png" 
                     data-src="${data.image}" 
                     alt="${data.title}" 
                     class="advertisement-card-image">
                <div class="advertisement-card-overlay">
                    <h5 class="advertisement-card-title">${data.title}</h5>
                    <p class="advertisement-card-description">${data.description}</p>
                </div>
            </div>
        `;
    }

    /**
     * حذف إعلان
     */
    removeAd(adId) {
        const adElement = document.querySelector(`[data-ad-id="${adId}"]`);
        if (adElement) {
            adElement.remove();
            console.log('🗑️ تم حذف الإعلان:', adId);
        }
    }
}

/**
 * Utility Functions
 */
const AdvertisementUtils = {
    /**
     * تحميل الإعلانات من API
     */
    async loadAds(category = null, limit = 10) {
        try {
            const url = `/api/Advertisements?limit=${limit}${category ? `&category=${category}` : ''}`;
            const response = await fetch(url);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error('❌ خطأ في تحميل الإعلانات:', error);
            return [];
        }
    },

    /**
     * تصفية الإعلانات
     */
    filterAds(ads, filters = {}) {
        return ads.filter(ad => {
            if (filters.category && ad.category !== filters.category) return false;
            if (filters.active !== undefined && ad.isActive !== filters.active) return false;
            if (filters.minPrice && ad.price < filters.minPrice) return false;
            if (filters.maxPrice && ad.price > filters.maxPrice) return false;
            return true;
        });
    },

    /**
     * ترتيب الإعلانات
     */
    sortAds(ads, sortBy = 'date', order = 'desc') {
        return ads.sort((a, b) => {
            let comparison = 0;
            
            switch (sortBy) {
                case 'date':
                    comparison = new Date(a.createdAt) - new Date(b.createdAt);
                    break;
                case 'views':
                    comparison = a.views - b.views;
                    break;
                case 'clicks':
                    comparison = a.clicks - b.clicks;
                    break;
                case 'price':
                    comparison = a.price - b.price;
                    break;
                default:
                    comparison = 0;
            }
            
            return order === 'desc' ? -comparison : comparison;
        });
    },

    /**
     * تنسيق العدد
     */
    formatNumber(num) {
        if (num >= 1000000) {
            return (num / 1000000).toFixed(1) + 'M';
        }
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    },

    /**
     * التحقق من صحة URL الصورة
     */
    isValidImageUrl(url) {
        return /\.(jpg|jpeg|png|gif|webp|svg)$/i.test(url);
    }
};

// تهيئة تلقائية عند تحميل الصفحة
let advertisementManager;

document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.advertisement-carousel, .advertisement-card')) {
        advertisementManager = new AdvertisementManager({
            autoplayInterval: 5000,
            pauseOnHover: true,
            lazyLoad: true,
            enableSwipe: true
        });
        
        // إتاحة الوصول العام
        window.advertisementManager = advertisementManager;
        window.AdvertisementUtils = AdvertisementUtils;
    }
});

// معالجة الرؤية عند تغيير Tab
document.addEventListener('visibilitychange', () => {
    if (advertisementManager) {
        if (document.hidden) {
            advertisementManager.pauseAll();
        } else {
            advertisementManager.playAll();
        }
    }
});
