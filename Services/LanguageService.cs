using Microsoft.AspNetCore.Localization;

namespace ReverseMarket.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentLanguage()
        {
            var feature = _httpContextAccessor.HttpContext?
                .Features.Get<IRequestCultureFeature>();

            var currentCulture = feature?.RequestCulture.Culture.Name ?? "ar";
            return currentCulture.Split('-')[0].ToLower();
        }

        public string GetDirection()
        {
            var lang = GetCurrentLanguage();
            return (lang == "ar" || lang == "ku") ? "rtl" : "ltr";
        }

        public List<LanguageInfo> GetSupportedLanguages()
        {
            return new List<LanguageInfo>
            {
                new LanguageInfo { Code = "ar", Name = "Arabic", NativeName = "العربية", Flag = "🇮🇶" },
                new LanguageInfo { Code = "en", Name = "English", NativeName = "English", Flag = "🇬🇧" },
                new LanguageInfo { Code = "ku", Name = "Kurdish", NativeName = "کوردی", Flag = "KU" }
            };
        }
    }
}