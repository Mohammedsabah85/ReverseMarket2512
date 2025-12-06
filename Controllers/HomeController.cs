using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.ViewModels;
using ReverseMarket.Resources;
using ReverseMarket.Services;
using System.Diagnostics;
using Twilio.Types;
using static System.Net.Mime.MediaTypeNames;

namespace ReverseMarket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TwilioWhatsapp _twilio = new TwilioWhatsapp();
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IEmailService _emailService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            IStringLocalizer<SharedResource> localizer,
            IEmailService emailService,
            ILogger<HomeController> logger)
        {
            _context = context;
            _localizer = localizer;
            _emailService = emailService;
            _logger = logger;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // تحميل إعدادات الموقع وتمريرها لجميع الصفحات
            var siteSettings = _context.SiteSettings.FirstOrDefault();
            ViewBag.SiteSettings = siteSettings;
        }


        public async Task<IActionResult> Index()
        {
            // _twilio.SendWhatsAppMessage("+9647801861182", $"code is {"99998520"} do not share");

            var model = new ReverseMarket.Models.HomeViewModel
            {
                Advertisements = await _context.Advertisements
                    .Where(a => a.IsActive &&
                           a.StartDate <= DateTime.Now &&
                           (a.EndDate == null || a.EndDate >= DateTime.Now))
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync(),

                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Take(8)
                    .ToListAsync(),

                RecentRequests = await _context.Requests
                    .Where(r => r.Status == RequestStatus.Approved)
                    .Include(r => r.Category)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.ApprovedAt)
                    .Take(12)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> About()
        {
            //var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            //ViewBag.SiteSettings = siteSettings;
            //return View();
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();

            // جلب الإعلانات النشطة والمرتبة
            var advertisements = await _context.Advertisements
                .Where(a => a.IsActive &&
                            a.StartDate <= DateTime.Now &&
                            (a.EndDate == null || a.EndDate >= DateTime.Now))
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.CreatedAt)
                .ToListAsync();

            // إنشاء الموديل الموحد
            var model = new HomeViewModel
            {
                SiteSettings = siteSettings,
                Advertisements = advertisements
            };

            // تمرير الموديل إلى الواجهة
            return View(model);
        }

        //public async Task<IActionResult> Contact()
        //{
        //    //var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
        //    //ViewBag.SiteSettings = siteSettings;
        //    //return View();
        //    var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();

        //    // جلب الإعلانات النشطة والمرتبة
        //    var advertisements = await _context.Advertisements
        //        .Where(a => a.IsActive &&
        //                    a.StartDate <= DateTime.Now &&
        //                    (a.EndDate == null || a.EndDate >= DateTime.Now))
        //        .OrderBy(a => a.DisplayOrder)
        //        .ThenBy(a => a.CreatedAt)
        //        .ToListAsync();

        //    // إنشاء الموديل الموحد
        //    var model = new HomeViewModel
        //    {
        //        SiteSettings = siteSettings,
        //        Advertisements = advertisements
        //    };

        //    // تمرير الموديل إلى الواجهة
        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();

            var advertisements = await _context.Advertisements
                .Where(a => a.IsActive &&
                            a.StartDate <= DateTime.Now &&
                            (a.EndDate == null || a.EndDate >= DateTime.Now))
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.CreatedAt)
                .ToListAsync();

            var model = new ContactPageViewModel
            {
                SiteSettings = siteSettings,
                Advertisements = advertisements
            };

            return View(model);
        }


        public IActionResult IntellectualProperty()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Contact(ContactFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _logger.LogInformation("Attempting to send contact form email");

        //            // إرسال البريد الإلكتروني
        //            var result = await _emailService.SendContactFormEmailAsync(
        //                model.Name,
        //                model.Email,
        //                model.Phone,
        //                model.Subject,
        //                model.Message
        //            );

        //            if (result)
        //            {
        //                TempData["SuccessMessage"] = "تم إرسال رسالتك بنجاح! سنرد عليك قريباً.";
        //                _logger.LogInformation("Contact form email sent successfully");
        //                return RedirectToAction(nameof(Contact));
        //            }
        //            else
        //            {
        //                _logger.LogError("Failed to send contact form email");
        //                TempData["ErrorMessage"] = "عذراً، حدث خطأ في إرسال الرسالة. يرجى التأكد من إعدادات البريد الإلكتروني أو المحاولة لاحقاً.";
        //                return View(model);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // تسجيل الخطأ التفصيلي
        //            _logger.LogError($"Exception in Contact form: {ex.Message}");
        //            _logger.LogError($"Stack trace: {ex.StackTrace}");

        //            TempData["ErrorMessage"] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى لاحقاً.";
        //            return View(model);
        //        }
        //    }

        //    var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
        //    ViewBag.SiteSettings = siteSettings;
        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactPageViewModel model)
        {
            //if (ModelState.IsValid)
            //{
                try
                {
                    _logger.LogInformation("Attempting to send contact form email");

                    var result = await _emailService.SendContactFormEmailAsync(
                        model.ContactForm.Name,
                        model.ContactForm.Email,
                        model.ContactForm.Phone,
                        model.ContactForm.Subject,
                        model.ContactForm.Message
                    );

                    if (result)
                    {
                        TempData["SuccessMessage"] = "تم إرسال رسالتك بنجاح! سنرد عليك قريباً.";
                        return RedirectToAction(nameof(Contact));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "عذراً، حدث خطأ في إرسال الرسالة. حاول لاحقاً.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception in Contact form: {ex.Message}");
                    TempData["ErrorMessage"] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى لاحقاً.";
                }
            //}

            // في حال فشل ModelState أو إرسال البريد
            model.SiteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            model.Advertisements = await _context.Advertisements
                .Where(a => a.IsActive &&
                            a.StartDate <= DateTime.Now &&
                            (a.EndDate == null || a.EndDate >= DateTime.Now))
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.CreatedAt)
                .ToListAsync();

            return View(model);
        }

        public IActionResult Terms()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            ViewBag.SiteSettings = siteSettings;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ContactFormModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}