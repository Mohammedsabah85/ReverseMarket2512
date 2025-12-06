using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSettings();
            return View(settings);
        }

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSettings model, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();

                if (settings == null)
                {
                    settings = new SiteSettings();
                    _context.SiteSettings.Add(settings);
                }

                // Upload Logo if provided
                if (logoFile != null)
                {
                    var logoPath = await SaveLogoAsync(logoFile);
                    settings.SiteLogo = logoPath;
                }

                // About Us - Multilingual
                settings.AboutUs = model.AboutUs;
                settings.AboutUsEn = model.AboutUsEn;
                settings.AboutUsKu = model.AboutUsKu;

                // Contact Information
                settings.ContactPhone = model.ContactPhone;
                settings.ContactWhatsApp = model.ContactWhatsApp;
                settings.ContactEmail = model.ContactEmail;

                // Social Media URLs
                settings.FacebookUrl = model.FacebookUrl;
                settings.InstagramUrl = model.InstagramUrl;
                settings.TwitterUrl = model.TwitterUrl;
                settings.YouTubeUrl = model.YouTubeUrl;

                // Copyright Info - Multilingual
                settings.CopyrightInfo = model.CopyrightInfo;
                settings.CopyrightInfoEn = model.CopyrightInfoEn;
                settings.CopyrightInfoKu = model.CopyrightInfoKu;

                // Privacy Policy - Multilingual
                settings.PrivacyPolicy = model.PrivacyPolicy;
                settings.PrivacyPolicyEn = model.PrivacyPolicyEn;
                settings.PrivacyPolicyKu = model.PrivacyPolicyKu;

                // Terms of Use - Multilingual
                settings.TermsOfUse = model.TermsOfUse;
                settings.TermsOfUseEn = model.TermsOfUseEn;
                settings.TermsOfUseKu = model.TermsOfUseKu;

                // Intellectual Property - Multilingual
                settings.IntellectualProperty = model.IntellectualProperty;
                settings.IntellectualPropertyEn = model.IntellectualPropertyEn;
                settings.IntellectualPropertyKu = model.IntellectualPropertyKu;

                // Update timestamp
                settings.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // Helper method to save logo file
        private async Task<string> SaveLogoAsync(IFormFile logo)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "site");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = "logo" + Path.GetExtension(logo.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await logo.CopyToAsync(fileStream);
            }

            return $"/uploads/site/{fileName}";
        }
    }
}