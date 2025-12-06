// Models/StoreCategory.cs
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Models
{
    public class StoreCategory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = ""; // Changed from int to string for Identity

        public int CategoryId { get; set; }

        public int? SubCategory1Id { get; set; }

        public int? SubCategory2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual Category Category { get; set; } = null!;
        public virtual SubCategory1? SubCategory1 { get; set; }
        public virtual SubCategory2? SubCategory2 { get; set; }
    }
}