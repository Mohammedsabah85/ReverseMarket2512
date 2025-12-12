using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Models
{
    // ViewModel للطلبات
    public class MyRequestsViewModel
    {
        public List<Request> Requests { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // ViewModel لتعديل الملف
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }

        // الصورة الشخصية
        public string? ProfileImage { get; set; }
        public IFormFile? ProfileImageFile { get; set; }

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string City { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        public string District { get; set; }

        public string? Location { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        public string Gender { get; set; }

        // نوع المستخدم - للعرض فقط (لا يتم إرساله أو تعديله في النموذج)
        public UserType? UserType { get; set; }

        // للبائعين فقط
        public string? StoreName { get; set; }
        public string? StoreDescription { get; set; }
        public string? WebsiteUrl1 { get; set; }
        public string? WebsiteUrl2 { get; set; }
        public string? WebsiteUrl3 { get; set; }
        public bool HasPendingUrlChanges { get; set; }
        public string? PendingWebsiteUrl1 { get; set; }
        public string? PendingWebsiteUrl2 { get; set; }
        public string? PendingWebsiteUrl3 { get; set; }

        // فئات المتجر للبائعين
        public string? StoreCategories { get; set; } // JSON string of selected SubCategory2 IDs
        public List<StoreCategoryDisplay>? CurrentStoreCategories { get; set; } // للعرض
        public string? PendingUrl1Status { get; set; }
        public string? PendingUrl2Status { get; set; }
        public string? PendingUrl3Status { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════
        // ✅ تفضيلات التواصل (للمشترين)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// السماح بالاتصال الهاتفي المباشر
        /// </summary>
        [Display(Name = "السماح بالاتصال الهاتفي")]
        public bool AllowPhoneCall { get; set; } = true;

        /// <summary>
        /// السماح بالتواصل عبر واتساب
        /// </summary>
        [Display(Name = "السماح بالتواصل عبر واتساب")]
        public bool AllowWhatsApp { get; set; } = true;

        /// <summary>
        /// السماح بالتواصل عبر البريد الإلكتروني
        /// </summary>
        [Display(Name = "السماح بالتواصل عبر البريد الإلكتروني")]
        public bool AllowEmail { get; set; } = true;

        /// <summary>
        /// السماح بالتواصل عبر الرسائل النصية SMS
        /// </summary>
        [Display(Name = "السماح بالرسائل النصية SMS")]
        public bool AllowSMS { get; set; } = false;

        /// <summary>
        /// السماح بالتواصل عبر الدردشة الداخلية
        /// </summary>
        [Display(Name = "السماح بالدردشة الداخلية")]
        public bool AllowInAppChat { get; set; } = true;

        /// <summary>
        /// ملاحظات إضافية للتواصل
        /// </summary>
        [Display(Name = "ملاحظات التواصل")]
        [StringLength(500, ErrorMessage = "ملاحظات التواصل لا يجب أن تزيد عن 500 حرف")]
        public string? ContactNotes { get; set; }

        /// <summary>
        /// رقم واتساب بديل
        /// </summary>
        [Display(Name = "رقم واتساب بديل")]
        [StringLength(20, ErrorMessage = "رقم الواتساب لا يجب أن يزيد عن 20 رقم")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "رقم الواتساب غير صحيح")]
        public string? AlternativeWhatsApp { get; set; }

        /// <summary>
        /// أفضل وقت للاتصال
        /// </summary>
        [Display(Name = "أفضل وقت للاتصال")]
        [StringLength(100, ErrorMessage = "أفضل وقت للاتصال لا يجب أن يزيد عن 100 حرف")]
        public string? PreferredContactTime { get; set; }
    }

    // ViewModel لعرض فئات المتجر الحالية
    public class StoreCategoryDisplay
    {
        public int SubCategory2Id { get; set; }
        public string CategoryName { get; set; } = "";
        public string SubCategory1Name { get; set; } = "";
        public string SubCategory2Name { get; set; } = "";
        public string FullPath => $"{CategoryName} > {SubCategory1Name} > {SubCategory2Name}";
    }

    // ViewModel لتحديث معلومات المتجر
    public class UpdateStoreViewModel
    {
        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [StringLength(255, ErrorMessage = "اسم المتجر لا يجب أن يزيد عن 255 حرف")]
        public string StoreName { get; set; } = "";

        [StringLength(1000, ErrorMessage = "وصف المتجر لا يجب أن يزيد عن 1000 حرف")]
        public string? StoreDescription { get; set; }

        [Url(ErrorMessage = "الرابط غير صحيح")]
        [StringLength(500)]
        public string? WebsiteUrl1 { get; set; }

        [Url(ErrorMessage = "الرابط غير صحيح")]
        [StringLength(500)]
        public string? WebsiteUrl2 { get; set; }

        [Url(ErrorMessage = "الرابط غير صحيح")]
        [StringLength(500)]
        public string? WebsiteUrl3 { get; set; }

        // فئات المتجر - قائمة SubCategory2 IDs
        public List<int>? StoreCategories { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // ✅ ViewModel لإعدادات التواصل فقط
    // ═══════════════════════════════════════════════════════════════════════════════
    public class ContactPreferencesViewModel
    {
        [Display(Name = "السماح بالاتصال الهاتفي")]
        public bool AllowPhoneCall { get; set; } = true;

        [Display(Name = "السماح بالتواصل عبر واتساب")]
        public bool AllowWhatsApp { get; set; } = true;

        [Display(Name = "السماح بالتواصل عبر البريد الإلكتروني")]
        public bool AllowEmail { get; set; } = true;

        [Display(Name = "السماح بالرسائل النصية SMS")]
        public bool AllowSMS { get; set; } = false;

        [Display(Name = "السماح بالدردشة الداخلية")]
        public bool AllowInAppChat { get; set; } = true;

        [Display(Name = "ملاحظات التواصل")]
        [StringLength(500, ErrorMessage = "ملاحظات التواصل لا يجب أن تزيد عن 500 حرف")]
        public string? ContactNotes { get; set; }

        [Display(Name = "رقم واتساب بديل")]
        [StringLength(20, ErrorMessage = "رقم الواتساب لا يجب أن يزيد عن 20 رقم")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "رقم الواتساب غير صحيح")]
        public string? AlternativeWhatsApp { get; set; }

        [Display(Name = "أفضل وقت للاتصال")]
        [StringLength(100, ErrorMessage = "أفضل وقت للاتصال لا يجب أن يزيد عن 100 حرف")]
        public string? PreferredContactTime { get; set; }

        // للتحقق من وجود وسيلة تواصل واحدة على الأقل
        public bool HasAtLeastOneContactMethod()
        {
            return AllowPhoneCall || AllowWhatsApp || AllowEmail || AllowSMS || AllowInAppChat;
        }
    }
}