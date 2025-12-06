using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using System.Security.Claims;


namespace ReverseMarket.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static User FromApplicationUser(this ClaimsPrincipal principal, ApplicationUser appUser)
        {
            return User.FromApplicationUser(appUser);
        }

        public static List<User> FromApplicationUsers(this ClaimsPrincipal principal, IEnumerable<ApplicationUser> appUsers)
        {
            return User.FromApplicationUsers(appUsers);
        }
    }
}
