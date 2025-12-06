using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [StringLength(255, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 255 حرف")]
        public string Name { get; set; } = "";

        [StringLength(1000, ErrorMessage = "وصف الفئة لا يجب أن يزيد عن 1000 حرف")]
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [StringLength(255, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 255 حرف")]
        public string Name { get; set; } = "";

        [StringLength(1000, ErrorMessage = "وصف الفئة لا يجب أن يزيد عن 1000 حرف")]
        public string? Description { get; set; }

        public string? CurrentImagePath { get; set; }

        public IFormFile? NewImage { get; set; }

        public bool RemoveImage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}