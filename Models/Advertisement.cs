// Models/Advertisement.cs
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public string? LinkUrl { get; set; }

        public AdvertisementType Type { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum AdvertisementType
    {
        Banner = 1,
        Slide = 2,
        Logo = 3
    }
}