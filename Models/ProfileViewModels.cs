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

        // ✅ الصورة الشخصية
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

        // ✅ نوع المستخدم - للعرض فقط (لا يتم إرساله أو تعديله في النموذج)
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

        // ✅ فئات المتجر للبائعين
        public string? StoreCategories { get; set; } // JSON string of selected SubCategory2 IDs
        public List<StoreCategoryDisplay>? CurrentStoreCategories { get; set; } // للعرض
    }

    // ✅ ViewModel لعرض فئات المتجر الحالية
    public class StoreCategoryDisplay
    {
        public int SubCategory2Id { get; set; }
        public string CategoryName { get; set; } = "";
        public string SubCategory1Name { get; set; } = "";
        public string SubCategory2Name { get; set; } = "";
        public string FullPath => $"{CategoryName} > {SubCategory1Name} > {SubCategory2Name}";
    }

    // ✅ ViewModel لتحديث معلومات المتجر
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
}