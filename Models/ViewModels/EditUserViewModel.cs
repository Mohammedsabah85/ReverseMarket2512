using ReverseMarket.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(50)]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [StringLength(50)]
        [Display(Name = "الاسم الأخير")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }  // ✅ Nullable

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }  // ✅ Nullable

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }

        [Display(Name = "نوع المستخدم")]
        public UserType UserType { get; set; }

        [Display(Name = "المدينة")]
        public string? City { get; set; }  // ✅ Nullable

        [Display(Name = "الحي")]
        public string? District { get; set; }  // ✅ Nullable

        [Display(Name = "الموقع")]
        public string? Location { get; set; }  // ✅ Nullable

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "الجنس")]
        public string? Gender { get; set; }  // ✅ Nullable

        [Display(Name = "اسم المتجر")]
        public string? StoreName { get; set; }  // ✅ Nullable

        [Display(Name = "وصف المتجر")]
        public string? StoreDescription { get; set; }  // ✅ Nullable

        [Display(Name = "رابط الموقع 1")]
        [Url(ErrorMessage = "الرابط غير صحيح")]
        public string? WebsiteUrl1 { get; set; }  // ✅ Nullable

        [Display(Name = "رابط الموقع 2")]
        [Url(ErrorMessage = "الرابط غير صحيح")]
        public string? WebsiteUrl2 { get; set; }  // ✅ Nullable

        [Display(Name = "رابط الموقع 3")]
        [Url(ErrorMessage = "الرابط غير صحيح")]
        public string? WebsiteUrl3 { get; set; }  // ✅ Nullable

        [Display(Name = "الهاتف موثق")]
        public bool IsPhoneVerified { get; set; }

        [Display(Name = "البريد موثق")]
        public bool IsEmailVerified { get; set; }

        [Display(Name = "المتجر موافق عليه")]
        public bool IsStoreApproved { get; set; }

        [Display(Name = "فئات المتجر")]
        public List<int>? StoreCategories { get; set; }  // ✅ Nullable

        [Display(Name = "الفئات الحالية")]
        public List<string>? CurrentStoreCategories { get; set; }  // ✅ Nullable

        [Display(Name = "الأدوار")]
        public List<string>? SelectedRoles { get; set; }  // ✅ Nullable

        public List<string>? AvailableRoles { get; set; }  // ✅ Nullable
    }
}