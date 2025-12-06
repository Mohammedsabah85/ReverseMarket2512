using System.ComponentModel.DataAnnotations;
using ReverseMarket.Models.Identity; // استخدام UserType من Identity

namespace ReverseMarket.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "رقم الهاتف يجب أن يكون 10 أرقام")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "رمز البلد مطلوب")]
        public string CountryCode { get; set; } = "+964";

        [Required(ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
        public bool AcceptTerms { get; set; }

        // خاصية للحصول على الرقم الكامل
        public string FullPhoneNumber => CountryCode + PhoneNumber;
    }

    public class VerifyOTPViewModel
    {
        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "رمز التحقق يجب أن يكون 4 أرقام")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "رمز التحقق يجب أن يحتوي على أرقام فقط")]
        public string OTP { get; set; } = "";
    }

    public class VerifyPhoneViewModel
    {
        [Required(ErrorMessage = "رمز التأكيد مطلوب")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "رمز التأكيد يجب أن يكون 4 أرقام")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "رمز التأكيد يجب أن يحتوي على أرقام فقط")]
        public string VerificationCode { get; set; } = "";
    }

    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم الأول يجب أن يكون بين 2 و 100 حرف")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z\s]+$", ErrorMessage = "الاسم الأول يجب أن يحتوي على أحرف عربية أو إنجليزية فقط")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم العائلة يجب أن يكون بين 2 و 100 حرف")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z\s]+$", ErrorMessage = "اسم العائلة يجب أن يحتوي على أحرف عربية أو إنجليزية فقط")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        public string Gender { get; set; } = "";

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المحافظة لا يجب أن يزيد عن 100 حرف")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المنطقة لا يجب أن يزيد عن 100 حرف")]
        public string District { get; set; } = "";

        [StringLength(255, ErrorMessage = "العنوان التفصيلي لا يجب أن يزيد عن 255 حرف")]
        public string? Location { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(255, ErrorMessage = "البريد الإلكتروني لا يجب أن يزيد عن 255 حرف")]
        public string? Email { get; set; }

        public IFormFile? ProfileImage { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        [Range(1, 2, ErrorMessage = "نوع الحساب غير صحيح")]
        public UserType UserType { get; set; }

        // Store fields (for sellers)
        [StringLength(255, ErrorMessage = "اسم المتجر لا يجب أن يزيد عن 255 حرف")]
        public string? StoreName { get; set; }

        [StringLength(1000, ErrorMessage = "وصف المتجر لا يجب أن يزيد عن 1000 حرف")]
        public string? StoreDescription { get; set; }

        [Url(ErrorMessage = "رابط الموقع الأول غير صحيح")]
        public string? WebsiteUrl1 { get; set; }

        [Url(ErrorMessage = "رابط الموقع الثاني غير صحيح")]
        public string? WebsiteUrl2 { get; set; }

        [Url(ErrorMessage = "رابط الموقع الثالث غير صحيح")]
        public string? WebsiteUrl3 { get; set; }

        // ⚠️⚠️⚠️ التغيير الأساسي هنا ⚠️⚠️⚠️
        // تم تغيير النوع من List<int>? إلى string?
        // السبب: JavaScript يرسل البيانات كـ JSON string: "[10,11,12]"
        // سيتم تحويله إلى List<int> في Controller باستخدام:
        // var categoryIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(model.StoreCategories);
        public string? StoreCategories { get; set; }

        // خاصية للتحقق من صحة العمر
        public bool IsValidAge()
        {
            var age = DateTime.Today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            return age >= 18 && age <= 100;
        }

        // خاصية للتحقق من صحة بيانات المتجر
        public bool IsValidStoreData()
        {
            if (UserType == UserType.Seller)
            {
                // تحقق من أن StoreCategories ليس فارغاً وأنه JSON صالح
                if (string.IsNullOrWhiteSpace(StoreName))
                    return false;

                if (string.IsNullOrWhiteSpace(StoreCategories))
                    return false;

                try
                {
                    var categories = System.Text.Json.JsonSerializer.Deserialize<List<int>>(StoreCategories);
                    return categories != null && categories.Any();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }

    // إضافة ViewModel لصفحة النجاح
    public class AccountCreatedViewModel
    {
        public string UserName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public UserType UserType { get; set; }
        public string? StoreName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}