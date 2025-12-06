using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class HomeViewModel
    {
        public List<Advertisement> Advertisements { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Request> RecentRequests { get; set; } = new();
        public SiteSettings? SiteSettings { get; set; }
    }
}