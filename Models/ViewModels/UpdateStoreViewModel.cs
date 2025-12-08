using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    /// <summary>
    /// نموذج تحديث معلومات المتجر
    /// يستخدم في صفحة Profile/ManageStore
    /// ✅ متاح للبائعين فقط
    /// </summary>
    //public class UpdateStoreViewModel
    //{
    //    #region معلومات المتجر الأساسية

    //    /// <summary>
    //    /// اسم المتجر - مطلوب
    //    /// </summary>
    //    [Required(ErrorMessage = "اسم المتجر مطلوب")]
    //    [StringLength(100, MinimumLength = 3, ErrorMessage = "اسم المتجر يجب أن يكون بين 3 و 100 حرف")]
    //    [Display(Name = "اسم المتجر")]
    //    public string StoreName { get; set; }

    //    /// <summary>
    //    /// وصف المتجر - اختياري
    //    /// </summary>
    //    [StringLength(500, ErrorMessage = "وصف المتجر يجب ألا يتجاوز 500 حرف")]
    //    [Display(Name = "وصف المتجر")]
    //    public string? StoreDescription { get; set; }

    //    #endregion

    //    #region روابط المتجر (تحتاج موافقة الإدارة)

    //    /// <summary>
    //    /// الرابط الأول - اختياري
    //    /// ✅ يحتاج موافقة الإدارة عند التغيير
    //    /// </summary>
    //    [Url(ErrorMessage = "الرابط غير صحيح. مثال: https://example.com")]
    //    [StringLength(200, ErrorMessage = "الرابط يجب ألا يتجاوز 200 حرف")]
    //    [Display(Name = "رابط الموقع الإلكتروني 1")]
    //    public string? WebsiteUrl1 { get; set; }

    //    /// <summary>
    //    /// الرابط الثاني - اختياري
    //    /// ✅ يحتاج موافقة الإدارة عند التغيير
    //    /// </summary>
    //    [Url(ErrorMessage = "الرابط غير صحيح. مثال: https://example.com")]
    //    [StringLength(200, ErrorMessage = "الرابط يجب ألا يتجاوز 200 حرف")]
    //    [Display(Name = "رابط الموقع الإلكتروني 2")]
    //    public string? WebsiteUrl2 { get; set; }

    //    /// <summary>
    //    /// الرابط الثالث - اختياري
    //    /// ✅ يحتاج موافقة الإدارة عند التغيير
    //    /// </summary>
    //    [Url(ErrorMessage = "الرابط غير صحيح. مثال: https://example.com")]
    //    [StringLength(200, ErrorMessage = "الرابط يجب ألا يتجاوز 200 حرف")]
    //    [Display(Name = "رابط الموقع الإلكتروني 3")]
    //    public string? WebsiteUrl3 { get; set; }

    //    #endregion

    //    #region فئات التخصص

    //    /// <summary>
    //    /// فئات المتجر (SubCategory2 IDs)
    //    /// البائع يختار التخصصات التي يعمل فيها
    //    /// </summary>
    //    [Display(Name = "تخصصات المتجر")]
    //    public List<int>? StoreCategories { get; set; }

    //    #endregion

    //    #region معلومات إضافية (للعرض فقط)

    //    /// <summary>
    //    /// الفئات الحالية المحددة (للعرض في الواجهة)
    //    /// </summary>
    //    public List<string>? CurrentCategoriesDisplay { get; set; }

    //    /// <summary>
    //    /// هل توجد روابط معلقة؟
    //    /// </summary>
    //    public bool HasPendingUrlChanges { get; set; }

    //    #endregion
    //}
}

/*
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * 📝 ملاحظات الاستخدام:
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * 
 * الوصول:
 * ─────────
 * ✅ متاح للبائعين فقط (UserType.Seller)
 * ❌ المشترون لا يمكنهم الوصول لهذه الصفحة
 * ✅ محمي في ProfileController.ManageStore()
 * 
 * الحقول المطلوبة:
 * ────────────────
 * ✓ StoreName (اسم المتجر) - مطلوب دائماً
 * ✗ StoreDescription - اختياري
 * ✗ WebsiteUrl1/2/3 - اختيارية لكن تحتاج موافقة الإدارة عند التغيير
 * ✗ StoreCategories - اختياري لكن يُفضل اختيار تخصص واحد على الأقل
 * 
 * سير العمل:
 * ──────────
 * 1. البائع يملأ/يعدل معلومات المتجر
 * 2. إذا تم تعديل الروابط → تُحفظ كـ PendingWebsiteUrl في ApplicationUser
 * 3. الإدارة توافق على الروابط الجديدة
 * 4. بعد الموافقة → تنتقل من Pending إلى WebsiteUrl الفعلي
 * 
 * الحماية:
 * ────────
 * ✓ Controller يتحقق من UserType قبل السماح بالوصول
 * ✓ Controller يسجل أي محاولة وصول غير مصرح بها
 * ✓ Validation attributes تمنع البيانات الخاطئة
 * 
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 */