using ReverseMarket.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public NotificationType Type { get; set; }

        // المستخدم المستهدف (null = جميع المستخدمين)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // نوع المستخدمين المستهدفين (null = الجميع)
        public UserType? TargetUserType { get; set; }

        // الطلب المرتبط (للإشعارات المتعلقة بالطلبات)
        public int? RequestId { get; set; }
        public Request? Request { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReadAt { get; set; }

        // للإشعارات من الإدارة
        public bool IsFromAdmin { get; set; } = false;
        public string? AdminId { get; set; }

        // حالة الإرسال
        public bool EmailSent { get; set; } = false;
        public bool WhatsAppSent { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
        public DateTime? WhatsAppSentAt { get; set; }

        // رابط الإشعار (اختياري)
        public string? Link { get; set; }

        // أيقونة الإشعار (اختياري)
        public string? Icon { get; set; }
    }

    public enum NotificationType
    {
        RequestApproved,        // طلب تم اعتماده
        RequestRejected,        // طلب تم رفضه
        NewRequestForStore,     // طلب جديد للمتجر
        AdminAnnouncement,      // إعلان من الإدارة
        SystemNotification,     // إشعار نظام
        StoreApproved,          // متجر تم اعتماده
        StoreRejected,          // متجر تم رفضه
        UrlChangeApproved,      // تغيير الروابط تم اعتماده
        UrlChangeRejected,      // تغيير الروابط تم رفضه
        General                 // إشعار عام
    }
}