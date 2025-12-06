using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.SignalR;

namespace ReverseMarket.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(
            string title,
            string message,
            NotificationType type,
            string? userId = null,
            UserType? targetUserType = null,
            int? requestId = null,
            string? link = null,
            bool isFromAdmin = false,
            string? adminId = null);

        Task SendNotificationAsync(Notification notification, bool sendEmail = true, bool sendWhatsApp = true, bool sendInApp = true);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int take = 50);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteNotificationAsync(int notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        //private readonly WhatsAppService _whatsAppService;
        private readonly IWhatsAppService _whatsAppService; // ✅ غيّر هنا

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
             //WhatsAppService whatsAppService,
             IWhatsAppService whatsAppService,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _hubContext = hubContext;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(
            string title,
            string message,
            NotificationType type,
            string? userId = null,
            UserType? targetUserType = null,
            int? requestId = null,
            string? link = null,
            bool isFromAdmin = false,
            string? adminId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                UserId = userId,
                TargetUserType = targetUserType,
                RequestId = requestId,
                Link = link,
                IsFromAdmin = isFromAdmin,
                AdminId = adminId,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task SendNotificationAsync(
            Notification notification,
            bool sendEmail = true,
            bool sendWhatsApp = true,
            bool sendInApp = true)
        {
            try
            {
                // تحديد المستخدمين المستهدفين
                List<ApplicationUser> targetUsers;

                if (!string.IsNullOrEmpty(notification.UserId))
                {
                    // إشعار لمستخدم محدد
                    var user = await _userManager.FindByIdAsync(notification.UserId);
                    targetUsers = user != null ? new List<ApplicationUser> { user } : new List<ApplicationUser>();
                }
                else if (notification.TargetUserType.HasValue)
                {
                    // إشعار لنوع معين من المستخدمين
                    targetUsers = await _userManager.Users
                        .Where(u => u.UserType == notification.TargetUserType.Value && u.IsActive)
                        .ToListAsync();
                }
                else
                {
                    // إشعار لجميع المستخدمين
                    targetUsers = await _userManager.Users
                        .Where(u => u.IsActive)
                        .ToListAsync();
                }

                if (!targetUsers.Any())
                {
                    _logger.LogWarning("لم يتم العثور على مستخدمين لإرسال الإشعار #{NotificationId}", notification.Id);
                    return;
                }

                // إنشاء نسخة من الإشعار لكل مستخدم (إذا كان إشعار عام)
                if (string.IsNullOrEmpty(notification.UserId) && targetUsers.Count > 1)
                {
                    foreach (var user in targetUsers)
                    {
                        var userNotification = new Notification
                        {
                            Title = notification.Title,
                            Message = notification.Message,
                            Type = notification.Type,
                            UserId = user.Id,
                            TargetUserType = notification.TargetUserType,
                            RequestId = notification.RequestId,
                            Link = notification.Link,
                            IsFromAdmin = notification.IsFromAdmin,
                            AdminId = notification.AdminId,
                            CreatedAt = notification.CreatedAt
                        };

                        _context.Notifications.Add(userNotification);
                    }
                    await _context.SaveChangesAsync();
                }

                // إرسال الإشعارات عبر القنوات المختلفة
                foreach (var user in targetUsers)
                {
                    // 1. إشعار داخل التطبيق (SignalR)
                    if (sendInApp)
                    {
                        await SendInAppNotificationAsync(user.UserName!, notification);
                    }

                    // 2. إرسال عبر الإيميل
                    if (sendEmail && !string.IsNullOrEmpty(user.Email))
                    {
                        await SendEmailNotificationAsync(user, notification);
                    }

                    // 3. إرسال عبر الواتساب
                    if (sendWhatsApp && !string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        await SendWhatsAppNotificationAsync(user, notification);
                    }

                    await Task.Delay(200); // تأخير بسيط بين كل مستخدم
                }

                _logger.LogInformation("تم إرسال الإشعار #{NotificationId} إلى {Count} مستخدم",
                    notification.Id, targetUsers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال الإشعار #{NotificationId}", notification.Id);
            }
        }

        private async Task SendInAppNotificationAsync(string username, Notification notification)
        {
            try
            {
                await _hubContext.Clients.User(username)
                    .SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type.ToString(),
                        link = notification.Link,
                        createdAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    });

                _logger.LogInformation("✅ تم إرسال إشعار داخل التطبيق إلى {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال الإشعار داخل التطبيق إلى {Username}", username);
            }
        }

        private async Task SendEmailNotificationAsync(ApplicationUser user, Notification notification)
        {
            try
            {
                var subject = $"🔔 {notification.Title}";
                var body = BuildEmailBody(user, notification);

                var result = await _emailService.SendEmailAsync(user.Email!, subject, body);

                if (result)
                {
                    notification.EmailSent = true;
                    notification.EmailSentAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ تم إرسال إشعار بالإيميل إلى {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال الإشعار بالإيميل إلى {Email}", user.Email);
            }
        }

        private async Task SendWhatsAppNotificationAsync(ApplicationUser user, Notification notification)
        {
            try
            {
                var message = BuildWhatsAppMessage(user, notification);

                // WhatsAppService does not have SendMessageAsync, so use an available method.
                // For notification messages, SendWelcomeMessageAsync is the closest match.
                // You may want to adjust the parameters as needed for your use case.
                //var result = await _whatsAppService.SendWelcomeMessageAsync(
                //    user.PhoneNumber!,
                //    user.FirstName,
                //    user.UserType.ToString()
                //);
                var result = await _whatsAppService.SendWhatsAppNotificationAsync(
            user.PhoneNumber!,
            message
        );
                if (result)
                {
                    notification.WhatsAppSent = true;
                    notification.WhatsAppSentAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ تم إرسال إشعار بالواتساب إلى {Phone}", user.PhoneNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال الإشعار بالواتساب إلى {Phone}", user.PhoneNumber);
            }
        }

        private string BuildEmailBody(ApplicationUser user, Notification notification)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .notification-type {{ background-color: #f8f9fa; padding: 10px; border-radius: 5px; margin: 20px 0; text-align: center; font-weight: bold; }}
        .message {{ background-color: #fff3cd; padding: 20px; border-radius: 5px; margin: 20px 0; border-right: 4px solid #ffc107; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 إشعار جديد</h1>
            <p>السوق العكسي</p>
        </div>
        <div class='content'>
            <p>مرحباً {user.FirstName} {user.LastName}،</p>
            
            <div class='notification-type'>
                {GetNotificationTypeText(notification.Type)}
            </div>
            
            <h2>{notification.Title}</h2>
            
            <div class='message'>
                {notification.Message.Replace("\n", "<br>")}
            </div>
            
            {(string.IsNullOrEmpty(notification.Link) ? "" : $"<a href='{notification.Link}' class='button'>عرض التفاصيل</a>")}
            
            <p style='margin-top: 30px; color: #666;'>
                التاريخ: {notification.CreatedAt:yyyy-MM-dd HH:mm}
            </p>
        </div>
        <div class='footer'>
            <p>هذا الإشعار تم إرساله من السوق العكسي</p>
            <p>© 2025 جميع الحقوق محفوظة</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildWhatsAppMessage(ApplicationUser user, Notification notification)
        {
            var typeEmoji = GetNotificationTypeEmoji(notification.Type);
            var message = $"{typeEmoji} *{notification.Title}*\n\n";
            message += $"مرحباً {user.FirstName}!\n\n";
            message += $"{notification.Message}\n\n";

            if (!string.IsNullOrEmpty(notification.Link))
            {
                message += $"للمزيد من التفاصيل: {notification.Link}\n\n";
            }

            message += $"📅 {notification.CreatedAt:yyyy-MM-dd HH:mm}\n\n";
            message += "السوق العكسي 🛒";

            return message;
        }

        private string GetNotificationTypeText(NotificationType type)
        {
            return type switch
            {
                NotificationType.RequestApproved => "✅ تم اعتماد الطلب",
                NotificationType.RequestRejected => "❌ تم رفض الطلب",
                NotificationType.NewRequestForStore => "🛒 طلب جديد متاح",
                NotificationType.AdminAnnouncement => "📢 إعلان من الإدارة",
                NotificationType.StoreApproved => "✅ تم اعتماد المتجر",
                NotificationType.StoreRejected => "❌ تم رفض المتجر",
                NotificationType.UrlChangeApproved => "✅ تم اعتماد الروابط",
                NotificationType.UrlChangeRejected => "❌ تم رفض الروابط",
                NotificationType.SystemNotification => "⚙️ إشعار النظام",
                _ => "📬 إشعار عام"
            };
        }

        private string GetNotificationTypeEmoji(NotificationType type)
        {
            return type switch
            {
                NotificationType.RequestApproved => "✅",
                NotificationType.RequestRejected => "❌",
                NotificationType.NewRequestForStore => "🛒",
                NotificationType.AdminAnnouncement => "📢",
                NotificationType.StoreApproved => "✅",
                NotificationType.StoreRejected => "❌",
                NotificationType.UrlChangeApproved => "✅",
                NotificationType.UrlChangeRejected => "❌",
                NotificationType.SystemNotification => "⚙️",
                _ => "🔔"
            };
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int take = 50)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Request)
                .OrderByDescending(n => n.CreatedAt);

            if (unreadOnly)
            {
                return await query.Where(n => !n.IsRead).Take(take).ToListAsync();
            }

            return await query.Take(take).ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}