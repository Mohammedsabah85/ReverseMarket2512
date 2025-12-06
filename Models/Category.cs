// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<SubCategory1> SubCategories1 { get; set; } = new List<SubCategory1>();
        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }

    public class SubCategory1
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<SubCategory2> SubCategories2 { get; set; } = new List<SubCategory2>();
        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }

    public class SubCategory2
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int SubCategory1Id { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual SubCategory1 SubCategory1 { get; set; }
        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }


    
    //// إضافة ViewModel لإنشاء الفئات
    //public class CreateCategoryViewModel
    //{
    //    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    //    [StringLength(100, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 100 حرف")]
    //    public string Name { get; set; } = "";

    //    [StringLength(500, ErrorMessage = "الوصف لا يجب أن يزيد عن 500 حرف")]
    //    public string? Description { get; set; }

    //    public IFormFile? Image { get; set; }

    //    public bool IsActive { get; set; } = true;
    //}

    //// إضافة ViewModel لتعديل الفئات
    //public class EditCategoryViewModel
    //{
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    //    [StringLength(100, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 100 حرف")]
    //    public string Name { get; set; } = "";

    //    [StringLength(500, ErrorMessage = "الوصف لا يجب أن يزيد عن 500 حرف")]
    //    public string? Description { get; set; }

    //    public string? CurrentImagePath { get; set; }

    //    public IFormFile? NewImage { get; set; }

    //    public bool RemoveImage { get; set; }

    //    public bool IsActive { get; set; } = true;

    //    public DateTime CreatedAt { get; set; }
    //}

    //public class CreateCategoryViewModel
    //{
    //    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    //    [StringLength(255, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 255 حرف")]
    //    public string Name { get; set; } = "";

    //    [StringLength(1000, ErrorMessage = "وصف الفئة لا يجب أن يزيد عن 1000 حرف")]
    //    public string? Description { get; set; }

    //    public IFormFile? Image { get; set; }

    //    public bool IsActive { get; set; } = true;
    //}
    //public class EditCategoryViewModel
    //{
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    //    [StringLength(255, ErrorMessage = "اسم الفئة لا يجب أن يزيد عن 255 حرف")]
    //    public string Name { get; set; } = "";

    //    [StringLength(1000, ErrorMessage = "وصف الفئة لا يجب أن يزيد عن 1000 حرف")]
    //    public string? Description { get; set; }

    //    public string? CurrentImagePath { get; set; }

    //    public IFormFile? NewImage { get; set; }

    //    public bool RemoveImage { get; set; }

    //    public bool IsActive { get; set; } = true;

    //    public DateTime CreatedAt { get; set; }
    //}
}