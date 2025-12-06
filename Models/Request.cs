// Models/Request.cs - Updated for Identity
using System.ComponentModel.DataAnnotations;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Models
{
    public class Request
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int CategoryId { get; set; }

        public int? SubCategory1Id { get; set; }

        public int? SubCategory2Id { get; set; }

        [Required]
        public string City { get; set; } = "";

        [Required]
        public string District { get; set; } = "";

        public string? Location { get; set; }

        [Required]
        public string UserId { get; set; } = ""; // Changed from int to string for Identity

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ApprovedAt { get; set; }

        public string? AdminNotes { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual SubCategory1? SubCategory1 { get; set; }
        public virtual SubCategory2? SubCategory2 { get; set; }
        public virtual ICollection<RequestImage> Images { get; set; } = new List<RequestImage>();
      
    }

    public class RequestImage
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = "";

        public int RequestId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Request Request { get; set; } = null!;
    }

    public enum RequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Postponed = 4
    }
}