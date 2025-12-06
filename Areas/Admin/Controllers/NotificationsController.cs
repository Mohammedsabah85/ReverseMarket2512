using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.Services;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        // عرض صفحة إرسال الإشعارات
        [HttpGet]
        public IActionResult Send()
        {
            return View(new SendNotificationViewModel());
        }

        // إرسال إشعار من الإدارة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendNotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // إنشاء الإشعار
                var notification = await _notificationService.CreateNotificationAsync(
                    title: model.Title,
                    message: model.Message,
                    type: NotificationType.AdminAnnouncement,
                    userId: model.TargetType == "specific" ? model.SpecificUserId : null,
                    targetUserType: model.TargetType == "buyers" ? UserType.Buyer :
                                   model.TargetType == "sellers" ? UserType.Seller : null,
                    link: model.Link,
                    isFromAdmin: true,
                    adminId: adminId
                );

                // إرسال الإشعار عبر جميع القنوات
                await _notificationService.SendNotificationAsync(
                    notification,
                    sendEmail: model.SendViaEmail,
                    sendWhatsApp: model.SendViaWhatsApp,
                    sendInApp: model.SendViaApp
                );

                TempData["SuccessMessage"] = "تم إرسال الإشعار بنجاح!";
                return RedirectToAction("Send");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال الإشعار");
                TempData["ErrorMessage"] = "حدث خطأ أثناء إرسال الإشعار";
                return View(model);
            }
        }

        // صفحة سجل الإشعارات المرسلة
        [HttpGet]
        public IActionResult History()
        {
            var notifications = _context.Notifications
                .Where(n => n.IsFromAdmin)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToList();

            return View(notifications);
        }
    }

    // ViewModel لإرسال الإشعارات
    public class SendNotificationViewModel
    {
        [Required(ErrorMessage = "عنوان الإشعار مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان لا يجب أن يزيد عن 200 حرف")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "محتوى الإشعار مطلوب")]
        [StringLength(1000, ErrorMessage = "المحتوى لا يجب أن يزيد عن 1000 حرف")]
        public string Message { get; set; } = "";

        [Required(ErrorMessage = "نوع المستهدفين مطلوب")]
        public string TargetType { get; set; } = "all"; // all, buyers, sellers, specific

        public string? SpecificUserId { get; set; }

        [Url(ErrorMessage = "الرابط غير صحيح")]
        public string? Link { get; set; }

        public bool SendViaEmail { get; set; } = true;
        public bool SendViaWhatsApp { get; set; } = true;
        public bool SendViaApp { get; set; } = true;
    }
}