using System;
using System.Collections.Generic;
using System.Linq;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Models
{
    public class User
    {
        // تغيير Id من int إلى string لمطابقة ASP.NET Identity
        public string Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Location { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public UserType UserType { get; set; }

        // Store Information
        public string StoreName { get; set; }
        public string StoreDescription { get; set; }
        public string WebsiteUrl1 { get; set; }
        public string WebsiteUrl2 { get; set; }
        public string WebsiteUrl3 { get; set; }
        public string ProfileImage { get; set; }

        // Account Status
        public bool IsActive { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relations
        public List<StoreCategory> StoreCategories { get; set; }

        // Constructor
        public User()
        {
            StoreCategories = new List<StoreCategory>();
        }

        // Static method to convert ApplicationUser to User
        public static User FromApplicationUser(ApplicationUser appUser)
        {
            if (appUser == null)
            {
                return null;
            }

            return new User
            {
                // نسخ Id كـ string
                Id = appUser.Id,
                FirstName = appUser.FirstName ?? "",
                LastName = appUser.LastName ?? "",
                Email = appUser.Email,
                PhoneNumber = appUser.PhoneNumber ?? "",
                City = appUser.City ?? "",
                District = appUser.District ?? "",
                Location = appUser.Location,
                DateOfBirth = appUser.DateOfBirth,
                Gender = appUser.Gender ?? "",
                UserType = appUser.UserType,
                StoreName = appUser.StoreName,
                StoreDescription = appUser.StoreDescription,
                WebsiteUrl1 = appUser.WebsiteUrl1,
                WebsiteUrl2 = appUser.WebsiteUrl2,
                WebsiteUrl3 = appUser.WebsiteUrl3,
                ProfileImage = appUser.ProfileImage,
                IsActive = appUser.IsActive,
                IsPhoneVerified = appUser.IsPhoneVerified,
                PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
                CreatedAt = appUser.CreatedAt,
                UpdatedAt = appUser.UpdatedAt,
                StoreCategories = appUser.StoreCategories?.ToList() ?? new List<StoreCategory>()
            };
        }

        // Static method to convert list of ApplicationUsers to list of Users
        public static List<User> FromApplicationUsers(IEnumerable<ApplicationUser> appUsers)
        {
            if (appUsers == null)
            {
                return new List<User>();
            }

            var userList = new List<User>();
            foreach (var appUser in appUsers)
            {
                var user = FromApplicationUser(appUser);
                if (user != null)
                {
                    // Log للتشخيص
                    Console.WriteLine($"Converting User: Id={user.Id}, Name={user.FirstName} {user.LastName}");
                    userList.Add(user);
                }
            }

            return userList;
        }

        // Helper method to get full name
        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        // Helper method to check if user is admin
        public bool IsAdmin()
        {
            return PhoneNumber == "+9647805006974";
        }

        // Override ToString for debugging
        public override string ToString()
        {
            return $"User[Id={Id}, Name={GetFullName()}, Phone={PhoneNumber}, Type={UserType}]";
        }
    }
}
