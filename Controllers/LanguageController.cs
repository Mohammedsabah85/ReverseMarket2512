using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReverseMarket.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILogger<LanguageController> _logger;

        public LanguageController(ILogger<LanguageController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            try
            {
                _logger.LogInformation("تغيير اللغة مطلوب إلى: {Culture}", culture);

                // التحقق من وجود اللغة
                if (string.IsNullOrEmpty(culture))
                {
                    _logger.LogWarning("معامل اللغة فارغ");
                    return BadRequest(new { success = false, message = "معامل اللغة مطلوب" });
                }

                // التحقق من صحة اللغة
                var validCultures = new[] { "ar", "en", "ku" };
                culture = culture.ToLower().Trim();

                if (!validCultures.Contains(culture))
                {
                    _logger.LogWarning("لغة غير صالحة: {Culture}", culture);
                    return BadRequest(new { success = false, message = "لغة غير صالحة" });
                }

                // حفظ الكوكيز بشكل صحيح
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        Path = "/",
                        SameSite = SameSiteMode.Lax,
                        HttpOnly = false,
                        Secure = false // في الإنتاج، غيرها إلى true مع HTTPS
                    }
                );

                _logger.LogInformation("تم حفظ كوكيز اللغة بنجاح: {Culture}", culture);

                // التحقق من URL الرجوع
                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                {
                    returnUrl = "/";
                }

                // إذا كان طلب AJAX، إرجاع JSON مع redirect URL
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        culture = culture,
                        redirectUrl = returnUrl,
                        message = "تم تغيير اللغة بنجاح"
                    });
                }

                // إعادة التوجيه العادية
                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تغيير اللغة");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطأ في تغيير اللغة: " + ex.Message
                    });
                }

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetCurrentLanguage()
        {
            var feature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = feature?.RequestCulture.Culture.Name ?? "ar";

            _logger.LogInformation("اللغة الحالية المطلوبة: {Culture}", currentCulture);

            return Json(new
            {
                success = true,
                language = currentCulture.Split('-')[0],
                direction = (currentCulture.StartsWith("ar") || currentCulture.StartsWith("ku")) ? "rtl" : "ltr"
            });
        }
    }
}