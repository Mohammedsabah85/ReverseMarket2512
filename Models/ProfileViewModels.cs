using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReverseMarket.Models;

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

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string City { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        public string District { get; set; }

        public string? Location { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        public string Gender { get; set; }

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
    }
}