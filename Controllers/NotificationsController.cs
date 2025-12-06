using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReverseMarket.Services;

namespace ReverseMarket.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            Data.ApplicationDbContext context,
            INotificationService notificationService) : base(context)
        {
            _notificationService = notificationService;
        }

        // عرض جميع الإشعارات
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, false, 100);
            return View(notifications);
        }

        // عرض الإشعارات غير المقروءة فقط
        public async Task<IActionResult> Unread()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, true, 50);
            return View("Index", notifications);
        }

        // الحصول على عدد الإشعارات غير المقروءة (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        // الحصول على آخر الإشعارات (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetLatestNotifications(int take = 5)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { notifications = new List<object>() });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, false, take);

            var result = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type.ToString(),
                isRead = n.IsRead,
                createdAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                link = n.Link
            });

            return Json(new { notifications = result });
        }

        // تحديد إشعار كمقروء (AJAX)
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = true });
        }

        // تحديد جميع الإشعارات كمقروءة
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "جلسة منتهية" });
            }

            await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = true });
        }

        // حذف إشعار
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            TempData["SuccessMessage"] = "تم حذف الإشعار بنجاح";
            return RedirectToAction("Index");
        }
    }
}