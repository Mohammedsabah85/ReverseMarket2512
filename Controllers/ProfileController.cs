using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ProfileController> logger) : base(context)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // عرض الملف الشخصي
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // جلب طلبات المستخدم
            var requests = await _context.Requests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.Images)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.TotalRequests = requests.Count;
            ViewBag.ApprovedRequests = requests.Count(r => r.Status == RequestStatus.Approved);
            ViewBag.PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending);
            ViewBag.RejectedRequests = requests.Count(r => r.Status == RequestStatus.Rejected);

            return View(user);
        }

        // عرض طلباتي
        public async Task<IActionResult> MyRequests(int page = 1)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var pageSize = 10;
            var query = _context.Requests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.Images)
                .OrderByDescending(r => r.CreatedAt);

            var totalRequests = await query.CountAsync();
            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new MyRequestsViewModel
            {
                Requests = requests,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize)
            };

            return View(model);
        }

        // تعديل الملف الشخصي
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.City,
                District = user.District,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                StoreName = user.StoreName,
                StoreDescription = user.StoreDescription,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,
                // إضافة الروابط المعلقة إن وجدت
                HasPendingUrlChanges = user.HasPendingUrlChanges,
                PendingWebsiteUrl1 = user.PendingWebsiteUrl1,
                PendingWebsiteUrl2 = user.PendingWebsiteUrl2,
                PendingWebsiteUrl3 = user.PendingWebsiteUrl3, 
 
    };

            return View(model);
        }

        // حفظ التعديلات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // تحديث البيانات الأساسية (لا تحتاج موافقة)
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.City = model.City;
            user.District = model.District;
            user.Location = model.Location;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;

            if (user.UserType == UserType.Seller)
            {
                user.StoreName = model.StoreName;
                user.StoreDescription = model.StoreDescription;

                // ✅ معالجة الروابط - تحتاج موافقة الإدارة
                bool urlsChanged = false;

                // التحقق من تغيير الروابط
                if (model.WebsiteUrl1 != user.WebsiteUrl1 ||
                    model.WebsiteUrl2 != user.WebsiteUrl2 ||
                    model.WebsiteUrl3 != user.WebsiteUrl3)
                {
                    // حفظ الروابط الجديدة كـ pending
                    user.PendingWebsiteUrl1 = model.WebsiteUrl1;
                    user.PendingWebsiteUrl2 = model.WebsiteUrl2;
                    user.PendingWebsiteUrl3 = model.WebsiteUrl3;
                    user.HasPendingUrlChanges = true;
                    urlsChanged = true;
                }
            }

            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (user.UserType == UserType.Seller && user.HasPendingUrlChanges)
                {
                    TempData["WarningMessage"] = "تم حفظ تعديلاتك بنجاح. الروابط الجديدة بانتظار موافقة الإدارة.";
                }
                else
                {
                    TempData["SuccessMessage"] = "تم تحديث ملفك الشخصي بنجاح";
                }
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}