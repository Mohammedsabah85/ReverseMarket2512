// wwwroot/js/notifications.js
// نظام الإشعارات الفورية

class NotificationManager {
    constructor() {
        this.connection = null;
        this.init();
    }

    async init() {
        // Initialize SignalR connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationhub")
            .withAutomaticReconnect()
            .build();

        // Handle incoming notifications
        this.connection.on("ReceiveNotification", (notification) => {
            this.handleNewNotification(notification);
        });

        // Start connection
        try {
            await this.connection.start();
            console.log("✅ NotificationHub connected");
            this.loadNotificationData();
        } catch (err) {
            console.error("❌ NotificationHub connection error:", err);
            setTimeout(() => this.init(), 5000); // Retry after 5 seconds
        }

        // Handle connection closed
        this.connection.onclose(() => {
            console.log("⚠️ NotificationHub disconnected");
        });
    }

    handleNewNotification(notification) {
        // Update badge count
        this.updateBadgeCount();

        // Add to dropdown list
        this.addToDropdownList(notification);

        // Show toast notification
        this.showToast(notification);

        // Play notification sound (optional)
        this.playNotificationSound();
    }

    async loadNotificationData() {
        await this.updateBadgeCount();
        await this.loadLatestNotifications();
    }

    async updateBadgeCount() {
        try {
            const response = await fetch('/Notifications/GetUnreadCount');
            const data = await response.json();

            const badge = document.getElementById('notification-badge');
            if (badge) {
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.style.display = 'inline-block';
                } else {
                    badge.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error updating notification count:', error);
        }
    }

    async loadLatestNotifications() {
        try {
            const response = await fetch('/Notifications/GetLatestNotifications?take=5');
            const data = await response.json();

            const listContainer = document.getElementById('notification-list');
            if (!listContainer) return;

            if (data.notifications.length === 0) {
                listContainer.innerHTML = `
                    <div class="text-center p-3 text-muted">
                        <i class="far fa-bell-slash"></i>
                        <p class="mb-0">لا توجد إشعارات جديدة</p>
                    </div>
                `;
            } else {
                listContainer.innerHTML = data.notifications.map(n => this.createNotificationItem(n)).join('');
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    createNotificationItem(notification) {
        const typeIcon = this.getNotificationIcon(notification.type);
        const typeColor = this.getNotificationColor(notification.type);

        return `
            <a href="${notification.link || '#'}" 
               class="dropdown-item notification-item ${!notification.isRead ? 'unread' : ''}" 
               onclick="notificationManager.markAsRead(${notification.id}); return true;">
                <div class="d-flex align-items-start">
                    <div class="notification-icon me-3">
                        <i class="fas ${typeIcon}" style="color: ${typeColor}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <strong class="d-block">${this.escapeHtml(notification.title)}</strong>
                        <small class="text-muted d-block">${this.truncate(notification.message, 60)}</small>
                        <small class="text-muted"><i class="far fa-clock"></i> ${notification.createdAt}</small>
                    </div>
                    ${!notification.isRead ? '<span class="badge bg-primary ms-2">جديد</span>' : ''}
                </div>
            </a>
        `;
    }

    addToDropdownList(notification) {
        const listContainer = document.getElementById('notification-list');
        if (!listContainer) return;

        const newItem = this.createNotificationItem(notification);
        
        if (listContainer.querySelector('.text-muted')) {
            listContainer.innerHTML = newItem;
        } else {
            listContainer.insertAdjacentHTML('afterbegin', newItem);
        }

        // Keep only latest 5 notifications
        const items = listContainer.querySelectorAll('.notification-item');
        if (items.length > 5) {
            items[items.length - 1].remove();
        }
    }

    async markAsRead(notificationId) {
        try {
            await fetch(`/Notifications/MarkAsRead?id=${notificationId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            this.updateBadgeCount();
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    async markAllAsRead() {
        if (!confirm('هل تريد تحديد جميع الإشعارات كمقروءة؟')) {
            return;
        }

        try {
            await fetch('/Notifications/MarkAllAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            this.loadNotificationData();
            location.reload();
        } catch (error) {
            console.error('Error marking all as read:', error);
        }
    }

    showToast(notification) {
        // Using Bootstrap Toast (requires Bootstrap 5)
        const toastContainer = document.getElementById('toast-container') || this.createToastContainer();

        const toast = document.createElement('div');
        toast.className = 'toast align-items-center border-0';
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        const typeColor = this.getNotificationColor(notification.type);
        const typeIcon = this.getNotificationIcon(notification.type);

        toast.innerHTML = `
            <div class="d-flex" style="background-color: ${typeColor}15; border-right: 4px solid ${typeColor}">
                <div class="toast-body">
                    <div class="d-flex align-items-center">
                        <i class="fas ${typeIcon} fa-lg me-2" style="color: ${typeColor}"></i>
                        <div>
                            <strong class="d-block">${this.escapeHtml(notification.title)}</strong>
                            <small>${this.escapeHtml(notification.message)}</small>
                        </div>
                    </div>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        toastContainer.appendChild(toast);

        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 5000
        });

        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
        return container;
    }

    playNotificationSound() {
        // Optional: Add notification sound
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.5;
        audio.play().catch(e => console.log('Could not play notification sound'));
    }

    getNotificationIcon(type) {
        const icons = {
            'RequestApproved': 'fa-check-circle',
            'RequestRejected': 'fa-times-circle',
            'NewRequestForStore': 'fa-shopping-cart',
            'AdminAnnouncement': 'fa-bullhorn',
            'StoreApproved': 'fa-store',
            'StoreRejected': 'fa-store-slash',
            'UrlChangeApproved': 'fa-link',
            'UrlChangeRejected': 'fa-unlink',
            'SystemNotification': 'fa-cog',
            'General': 'fa-bell'
        };
        return icons[type] || 'fa-bell';
    }

    getNotificationColor(type) {
        const colors = {
            'RequestApproved': '#28a745',
            'RequestRejected': '#dc3545',
            'NewRequestForStore': '#007bff',
            'AdminAnnouncement': '#ffc107',
            'StoreApproved': '#28a745',
            'StoreRejected': '#dc3545',
            'UrlChangeApproved': '#28a745',
            'UrlChangeRejected': '#dc3545',
            'SystemNotification': '#6c757d',
            'General': '#17a2b8'
        };
        return colors[type] || '#6c757d';
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    truncate(text, length) {
        if (text.length <= length) return text;
        return text.substring(0, length) + '...';
    }
}

// Initialize notification manager when DOM is ready
let notificationManager;

document.addEventListener('DOMContentLoaded', function() {
    if (document.getElementById('notification-badge')) {
        notificationManager = new NotificationManager();
    }
});
