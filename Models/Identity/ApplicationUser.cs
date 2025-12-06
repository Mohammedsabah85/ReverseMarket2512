// Models/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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

        // Store properties (if user is seller)
        [StringLength(255)]
        public string? StoreName { get; set; }

        [StringLength(1000)]
        public string? StoreDescription { get; set; }

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
        // Navigation properties
        //public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
        //public virtual ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();

        public string? PendingWebsiteUrl1 { get; set; }
        public string? PendingWebsiteUrl2 { get; set; }
        public string? PendingWebsiteUrl3 { get; set; }

        public bool HasPendingUrlChanges { get; set; }


        public DateTime? UrlsLastApprovedAt { get; set; }

        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
        public virtual ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();
        // Full name property
        public string FullName => $"{FirstName} {LastName}";

        // Display name based on user type
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