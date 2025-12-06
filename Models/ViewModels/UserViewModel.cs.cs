using ReverseMarket.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReverseMarket.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImage { get; set; }
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Location { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string StoreName { get; set; }
        public string StoreDescription { get; set; }
        public string WebsiteUrl1 { get; set; }
        public string WebsiteUrl2 { get; set; }
        public string WebsiteUrl3 { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsStoreApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<string> Roles { get; set; }
        public string GetFullName() => $"{FirstName} {LastName}".Trim();
        public List<string> StoreCategories { get; set; }

        public static UserViewModel FromApplicationUser(ApplicationUser user, IList<string> roles, List<string> storeCategories = null)
        {
            if (user == null) return null;

            return new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
                UserType = user.UserType,
                City = user.City,
                District = user.District,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                StoreName = user.StoreName,
                StoreDescription = user.StoreDescription,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,
                IsPhoneVerified = user.IsPhoneVerified,
                IsEmailVerified = user.IsEmailVerified,
                IsStoreApproved = user.IsStoreApproved,
                CreatedAt = user.CreatedAt,
                Roles = roles ?? new List<string>(),
                StoreCategories = storeCategories ?? new List<string>()
            };
        }

        public static List<UserViewModel> FromApplicationUsers(List<ApplicationUser> users, Dictionary<string, IList<string>> userRoles)
        {
            if (users == null) return new List<UserViewModel>();

            return users.Select(user => new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
                UserType = user.UserType,
                City = user.City,
                District = user.District,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                StoreName = user.StoreName,
                StoreDescription = user.StoreDescription,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,
                IsPhoneVerified = user.IsPhoneVerified,
                IsEmailVerified = user.IsEmailVerified,
                IsStoreApproved = user.IsStoreApproved,
                CreatedAt = user.CreatedAt,
                Roles = userRoles.ContainsKey(user.Id) ? userRoles[user.Id] : new List<string>()
            }).ToList();
        }
    }
}