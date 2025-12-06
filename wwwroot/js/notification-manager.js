// wwwroot/js/notification-manager.js
const notificationManager = {
    connection: null,
    badge: null,
    list: null,

    init: function () {
        console.log('🔔 تهيئة مدير الإشعارات...');

        this.badge = document.getElementById('notification-badge');
        this.list = document.getElementById('notification-list');

        if (!this.badge || !this.list) {
            console.warn('⚠️ عناصر الإشعارات غير موجودة في الصفحة');
            return;
        }

        // تحميل الإشعارات الأولية
        this.loadNotifications();

        // تحديث العداد
        this.updateBadgeCount();

        // الاتصال بـ SignalR
        this.connectSignalR();

        // تحديث دوري كل دقيقة
        setInterval(() => {
            this.updateBadgeCount();
        }, 60000);
    },

    connectSignalR: function () {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.on("ReceiveNotification", (notification) => {
            console.log('📬 إشعار جديد:', notification);
            this.handleNewNotification(notification);
        });

        this.connection.start()
            .then(() => {
                console.log('✅ تم الاتصال بـ SignalR بنجاح');
            })
            .catch(err => {
                console.error('❌ خطأ في الاتصال بـ SignalR:', err);
                setTimeout(() => this.connectSignalR(), 5000);
            });
    },

    loadNotifications: function () {
        console.log('📥 تحميل الإشعارات...');

        fetch('/Notifications/GetLatestNotifications?take=5')
            .then(response => {
                console.log('📡 استجابة الخادم:', response.status);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 البيانات المستلمة:', data);
                if (data.notifications && data.notifications.length > 0) {
                    this.displayNotifications(data.notifications);
                } else {
                    this.list.innerHTML = `
                        <div class="text-center p-4 text-muted">
                            <i class="fas fa-bell-slash fa-2x mb-2"></i>
                            <p class="mb-0">لا توجد إشعارات</p>
                        </div>
                    `;
                }
            })
            .catch(err => {
                console.error('❌ خطأ في تحميل الإشعارات:', err);
                this.list.innerHTML = `
                    <div class="text-center p-4 text-danger">
                        <i class="fas fa-exclamation-circle fa-2x mb-2"></i>
                        <p class="mb-0">خطأ في تحميل الإشعارات</p>
                        <button class="btn btn-sm btn-outline-primary mt-2" onclick="notificationManager.loadNotifications()">
                            <i class="fas fa-redo"></i> إعادة المحاولة
                        </button>
                    </div>
                `;
            });
    },

    displayNotifications: function (notifications) {
        if (!notifications || notifications.length === 0) {
            this.list.innerHTML = `
                <div class="text-center p-4 text-muted">
                    <i class="fas fa-bell-slash fa-2x mb-2"></i>
                    <p class="mb-0">لا توجد إشعارات</p>
                </div>
            `;
            return;
        }

        this.list.innerHTML = notifications.map(n => `
            <div class="notification-item ${!n.isRead ? 'unread' : ''}" 
                 data-notification-id="${n.id}">
                <div class="d-flex align-items-start p-3">
                    <div class="notification-icon me-3">
                        <i class="fas ${this.getNotificationIcon(n.type)}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start mb-1">
                            <h6 class="mb-0">
                                ${!n.isRead ? '<span class="badge bg-primary me-1">جديد</span>' : ''}
                                ${n.title}
                            </h6>
                            <small class="text-muted ms-2">${this.getRelativeTime(n.createdAt)}</small>
                        </div>
                        <p class="mb-2 text-muted small">${n.message}</p>
                        ${n.link ? `
                            <a href="${n.link}" class="btn btn-sm btn-outline-primary" 
                               onclick="notificationManager.markAsRead(${n.id})">
                                <i class="fas fa-external-link-alt"></i> عرض
                            </a>
                        ` : ''}
                    </div>
                </div>
            </div>
        `).join('');
    },

    handleNewNotification: function (notification) {
        this.loadNotifications();
        this.updateBadgeCount();
        this.showToast(notification);
        this.playNotificationSound();
    },

    showToast: function (notification) {
        const toastHtml = `
            <div class="toast align-items-center text-white bg-primary border-0" 
                 role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <strong>${notification.title}</strong><br>
                        ${notification.message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                            data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    },

    playNotificationSound: function () {
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.5;
            audio.play().catch(e => console.log('لا يمكن تشغيل الصوت:', e));
        } catch (e) {
            console.log('خطأ في تشغيل الصوت:', e);
        }
    },

    updateBadgeCount: function () {
        fetch('/Notifications/GetUnreadCount')
            .then(response => response.json())
            .then(data => {
                const count = data.count || 0;
                if (this.badge) {
                    if (count > 0) {
                        this.badge.textContent = count > 99 ? '99+' : count;
                        this.badge.style.display = 'flex';
                    } else {
                        this.badge.style.display = 'none';
                    }
                }
            })
            .catch(err => console.error('خطأ في تحديث العداد:', err));
    },

    markAsRead: function (notificationId) {
        if (!notificationId) return;

        fetch(`/Notifications/MarkAsRead?id=${notificationId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        }).then(() => {
            const item = document.querySelector(`[data-notification-id="${notificationId}"]`);
            if (item) {
                item.classList.remove('unread');
                const badge = item.querySelector('.badge.bg-primary');
                if (badge) badge.remove();
            }
            this.updateBadgeCount();
        }).catch(err => console.error('خطأ في تحديد الإشعار كمقروء:', err));
    },

    markAllAsRead: function () {
        fetch('/Notifications/MarkAllAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        }).then(() => {
            this.loadNotifications();
            this.updateBadgeCount();
        }).catch(err => console.error('خطأ في تحديد جميع الإشعارات كمقروءة:', err));
    },

    getNotificationIcon: function (type) {
        const icons = {
            'RequestApproved': 'fa-check-circle text-success',
            'RequestRejected': 'fa-times-circle text-danger',
            'NewRequestForStore': 'fa-shopping-cart text-primary',
            'AdminAnnouncement': 'fa-bullhorn text-warning',
            'StoreApproved': 'fa-store text-success',
            'StoreRejected': 'fa-store-slash text-danger',
            'UrlChangeApproved': 'fa-link text-success',
            'UrlChangeRejected': 'fa-unlink text-danger',
            'SystemNotification': 'fa-cog text-secondary'
        };
        return icons[type] || 'fa-bell text-info';
    },

    getRelativeTime: function (dateStr) {
        const date = new Date(dateStr);
        const now = new Date();
        const seconds = Math.floor((now - date) / 1000);

        if (seconds < 60) return 'الآن';
        if (seconds < 3600) return `منذ ${Math.floor(seconds / 60)} دقيقة`;
        if (seconds < 86400) return `منذ ${Math.floor(seconds / 3600)} ساعة`;
        if (seconds < 604800) return `منذ ${Math.floor(seconds / 86400)} يوم`;
        if (seconds < 2592000) return `منذ ${Math.floor(seconds / 604800)} أسبوع`;
        if (seconds < 31536000) return `منذ ${Math.floor(seconds / 2592000)} شهر`;
        return `منذ ${Math.floor(seconds / 31536000)} سنة`;
    }
};

// تهيئة عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('notification-badge')) {
        notificationManager.init();
    }
});