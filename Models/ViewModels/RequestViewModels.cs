using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Models
{
    public class RequestsViewModel
    {
        public List<Request> Requests { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<SubCategory1> SubCategories1 { get; set; } = new();
        public List<SubCategory2> SubCategories2 { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Search { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategory1Id { get; set; }
        public int? SelectedSubCategory2Id { get; set; }
        public List<Advertisement> Advertisements { get; set; } = new();
    }


    public class CreateRequestViewModel
    {
        [Required(ErrorMessage = "عنوان الطلب مطلوب")]
        [StringLength(255, ErrorMessage = "عنوان الطلب لا يجب أن يزيد عن 255 حرف")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "تفاصيل الطلب مطلوبة")]
        [StringLength(2000, ErrorMessage = "تفاصيل الطلب لا يجب أن تزيد عن 2000 حرف")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "الفئة الرئيسية مطلوبة")]
        public int CategoryId { get; set; }

        public int? SubCategory1Id { get; set; }

        public int? SubCategory2Id { get; set; }

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المحافظة لا يجب أن يزيد عن 100 حرف")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المنطقة لا يجب أن يزيد عن 100 حرف")]
        public string District { get; set; } = "";

        [StringLength(255, ErrorMessage = "العنوان التفصيلي لا يجب أن يزيد عن 255 حرف")]
        public string? Location { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}