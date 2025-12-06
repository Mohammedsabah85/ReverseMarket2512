using ReverseMarket.Models.Identity;

namespace ReverseMarket.Models
{
    public class StoresViewModel
    {
        public List<Advertisement> Advertisements { get; set; } = new();
        public List<ApplicationUser> Stores { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Search { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
    /// <summary>
    /// Extension methods for ApplicationUser to Store conversion
    /// </summary>
    public static class StoreExtensions
    {
        public static List<User> ToStoreList(this IEnumerable<ApplicationUser> applicationUsers)
        {
            return applicationUsers.Select(User.FromApplicationUser).ToList();
        }

        public static User ToStore(this ApplicationUser applicationUser)
        {
            return User.FromApplicationUser(applicationUser);
        }
    }
}