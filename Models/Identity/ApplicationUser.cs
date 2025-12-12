// Models/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReverseMarket.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string City { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string District { get; set; } = "";

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? ProfileImage { get; set; }

        [Required]
        public UserType UserType { get; set; }

        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════
        // Store properties (if user is seller)
        // ═══════════════════════════════════════════════════════════════════════════════

        [StringLength(255)]
        public string? StoreName { get; set; }

        [StringLength(1000)]
        public string? StoreDescription { get; set; }

        // الروابط الفعلية (المعتمدة)
        [StringLength(500)]
        public string? WebsiteUrl1 { get; set; }

        [StringLength(500)]
        public string? WebsiteUrl2 { get; set; }

        [StringLength(500)]
        public string? WebsiteUrl3 { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsStoreApproved { get; set; } = false;
        public DateTime? StoreApprovedAt { get; set; }
        public string? StoreApprovedBy { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════
        // الروابط المعلقة (في انتظار موافقة الإدارة)
        // ═══════════════════════════════════════════════════════════════════════════════

        [StringLength(500)]
        public string? PendingWebsiteUrl1 { get; set; }

        [StringLength(500)]
        public string? PendingWebsiteUrl2 { get; set; }

        [StringLength(500)]
        public string? PendingWebsiteUrl3 { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════
        // ✅ جديد: حالة كل رابط معلق بشكل منفصل
        // القيم المحتملة: null (لا يوجد تعديل) / "Pending" (في الانتظار)
        // ═══════════════════════════════════════════════════════════════════════════════

        [StringLength(20)]
        public string? PendingUrl1Status { get; set; }

        [StringLength(20)]
        public string? PendingUrl2Status { get; set; }

        [StringLength(20)]
        public string? PendingUrl3Status { get; set; }

        // ✅ جديد: تاريخ تقديم كل رابط
        public DateTime? PendingUrl1SubmittedAt { get; set; }
        public DateTime? PendingUrl2SubmittedAt { get; set; }
        public DateTime? PendingUrl3SubmittedAt { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// هل يوجد أي تعديل معلق على الروابط؟
        /// يُحسب تلقائياً من حالات الروابط الفردية
        /// </summary>
        public bool HasPendingUrlChanges { get; set; }

        public DateTime? UrlsLastApprovedAt { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════════
        // خصائص محسوبة (لا تُحفظ في قاعدة البيانات)
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// هل يوجد أي رابط معلق؟ (خاصية محسوبة)
        /// </summary>
        [NotMapped]
        public bool HasAnyPendingUrl =>
            PendingUrl1Status == "Pending" ||
            PendingUrl2Status == "Pending" ||
            PendingUrl3Status == "Pending";

        /// <summary>
        /// عدد الروابط المعلقة
        /// </summary>
        [NotMapped]
        public int PendingUrlsCount
        {
            get
            {
                int count = 0;
                if (PendingUrl1Status == "Pending") count++;
                if (PendingUrl2Status == "Pending") count++;
                if (PendingUrl3Status == "Pending") count++;
                return count;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // Navigation properties
        // ═══════════════════════════════════════════════════════════════════════════════

        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
        public virtual ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();

        // ═══════════════════════════════════════════════════════════════════════════════
        // Computed properties
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// الاسم الكامل
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// اسم العرض حسب نوع المستخدم
        /// </summary>
        public string DisplayName => UserType == UserType.Seller && !string.IsNullOrEmpty(StoreName)
            ? StoreName
            : FullName;
    }

    public enum UserType
    {
        Buyer = 1,
        Seller = 2
    }

    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}


//// Models/Identity/ApplicationUser.cs
//using Microsoft.AspNetCore.Identity;
//using System.ComponentModel.DataAnnotations;

//namespace ReverseMarket.Models.Identity
//{
//    public class ApplicationUser : IdentityUser
//    {
//        [Required]
//        [StringLength(50)]
//        public string FirstName { get; set; } = "";

//        [Required]
//        [StringLength(50)]
//        public string LastName { get; set; } = "";

//        [Required]
//        public DateTime DateOfBirth { get; set; }

//        [Required]
//        [StringLength(10)]
//        public string Gender { get; set; } = "";

//        [Required]
//        [StringLength(100)]
//        public string City { get; set; } = "";


//        [Required]
//        [StringLength(100)]
//        public string District { get; set; } = "";

//        [StringLength(255)]
//        public string? Location { get; set; }

//        [StringLength(500)]
//        public string? ProfileImage { get; set; }

//        [Required]
//        public UserType UserType { get; set; }

//        public bool IsPhoneVerified { get; set; }
//        public bool IsEmailVerified { get; set; }
//        public DateTime CreatedAt { get; set; } = DateTime.Now;
//        public DateTime? UpdatedAt { get; set; }

//        // Store properties (if user is seller)
//        [StringLength(255)]
//        public string? StoreName { get; set; }

//        [StringLength(1000)]
//        public string? StoreDescription { get; set; }

//        [StringLength(500)]
//        public string? WebsiteUrl1 { get; set; }

//        [StringLength(500)]
//        public string? WebsiteUrl2 { get; set; }

//        [StringLength(500)]
//        public string? WebsiteUrl3 { get; set; }

//        public bool IsActive { get; set; } = true;
//        public bool IsStoreApproved { get; set; } = false;
//        public DateTime? StoreApprovedAt { get; set; }
//        public string? StoreApprovedBy { get; set; }
//        // Navigation properties
//        //public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
//        //public virtual ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();

//        public string? PendingWebsiteUrl1 { get; set; }
//        public string? PendingWebsiteUrl2 { get; set; }
//        public string? PendingWebsiteUrl3 { get; set; }

//        public bool HasPendingUrlChanges { get; set; }


//        public DateTime? UrlsLastApprovedAt { get; set; }

//        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
//        public virtual ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();
//        // Full name property
//        public string FullName => $"{FirstName} {LastName}";

//        // Display name based on user type
//        public string DisplayName => UserType == UserType.Seller && !string.IsNullOrEmpty(StoreName)
//            ? StoreName
//            : FullName;


//    }

//    public enum UserType
//    {
//        Buyer = 1,
//        Seller = 2
//    }

//    public class ApplicationRole : IdentityRole
//    {
//        public string? Description { get; set; }
//        public DateTime CreatedAt { get; set; } = DateTime.Now;
//    }
//}