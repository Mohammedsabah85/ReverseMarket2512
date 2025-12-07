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

        #region عرض الملف الشخصي

        /// <summary>
        /// عرض الملف الشخصي للمستخدم
        /// </summary>
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
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory1)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory2)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // ✅ جلب الطلبات فقط للمشترين
            if (user.UserType == UserType.Buyer)
            {
                var requests = await _context.Requests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .Include(r => r.Images)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                ViewBag.Requests = requests;
                ViewBag.TotalRequests = requests.Count;
                ViewBag.ApprovedRequests = requests.Count(r => r.Status == RequestStatus.Approved);
                ViewBag.PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending);
                ViewBag.RejectedRequests = requests.Count(r => r.Status == RequestStatus.Rejected);
            }

            return View(user);
        }

        /// <summary>
        /// عرض طلباتي - متاح للمشترين فقط
        /// </summary>
        public async Task<IActionResult> MyRequests(int page = 1)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);

            // ✅ حماية: التحقق من أن المستخدم مشتري
            if (user == null || user.UserType != UserType.Buyer)
            {
                _logger.LogWarning("محاولة وصول غير مصرح بها لصفحة الطلبات. UserId: {UserId}, UserType: {UserType}",
                    userId, user?.UserType);
                TempData["ErrorMessage"] = "عذراً، هذه الصفحة متاحة للمشترين فقط.";
                return RedirectToAction("Index");
            }

            var pageSize = 10;
            var query = _context.Requests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
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

        #endregion

        #region تعديل الملف الشخصي

        /// <summary>
        /// عرض صفحة تعديل الملف الشخصي
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory1)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory2)
                .FirstOrDefaultAsync(u => u.Id == userId);

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
                UserType = user.UserType, // ✅ للقراءة فقط - لا يمكن تغييره

                // معلومات المتجر (فقط للبائعين)
                StoreName = user.StoreName,
                StoreDescription = user.StoreDescription,
                WebsiteUrl1 = user.WebsiteUrl1,
                WebsiteUrl2 = user.WebsiteUrl2,
                WebsiteUrl3 = user.WebsiteUrl3,

                // الروابط المعلقة
                HasPendingUrlChanges = user.HasPendingUrlChanges,
                PendingWebsiteUrl1 = user.PendingWebsiteUrl1,
                PendingWebsiteUrl2 = user.PendingWebsiteUrl2,
                PendingWebsiteUrl3 = user.PendingWebsiteUrl3
            };

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// حفظ تعديلات الملف الشخصي
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // ✅ الحماية الأولى: منع تغيير نوع المستخدم نهائياً
            if (model.UserType != user.UserType)
            {
                _logger.LogWarning("⚠️ محاولة تغيير نوع الحساب! UserId: {UserId}, النوع الحالي: {CurrentType}, النوع المطلوب: {RequestedType}",
                    userId, user.UserType, model.UserType);

                TempData["ErrorMessage"] = "لا يمكن تغيير نوع الحساب. إذا كنت تريد تغيير نوع حسابك، يرجى التواصل مع الإدارة.";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            // ✅ الحماية الثانية: منع المشتري من إضافة معلومات متجر
            if (user.UserType == UserType.Buyer)
            {
                // التحقق من محاولة إضافة معلومات متجر
                if (!string.IsNullOrEmpty(model.StoreName) ||
                    !string.IsNullOrEmpty(model.StoreDescription) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl1) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl2) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl3))
                {
                    _logger.LogWarning("⚠️ محاولة إضافة معلومات متجر من قبل مشتري! UserId: {UserId}", userId);

                    TempData["ErrorMessage"] = "المشترون لا يمكنهم إضافة معلومات متجر. نوع حسابك هو: مشتري.";
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    return View(model);
                }

                // ✅ حماية إضافية: مسح أي معلومات متجر قد تكون موجودة في قاعدة البيانات
                user.StoreName = null;
                user.StoreDescription = null;
                user.WebsiteUrl1 = null;
                user.WebsiteUrl2 = null;
                user.WebsiteUrl3 = null;
                user.PendingWebsiteUrl1 = null;
                user.PendingWebsiteUrl2 = null;
                user.PendingWebsiteUrl3 = null;
                user.HasPendingUrlChanges = false;
                user.IsStoreApproved = true; // المشترون دائماً approved

                // حذف أي فئات متجر قد تكون موجودة (حماية إضافية)
                var existingStoreCategories = await _context.StoreCategories
                    .Where(sc => sc.UserId == userId)
                    .ToListAsync();

                if (existingStoreCategories.Any())
                {
                    _context.StoreCategories.RemoveRange(existingStoreCategories);
                    _logger.LogWarning("⚠️ تم حذف {Count} فئة متجر من حساب مشتري: {UserId}",
                        existingStoreCategories.Count, userId);
                }
            }

            // ✅ تحديث البيانات الأساسية (متاح للجميع)
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.City = model.City;
            user.District = model.District;
            user.Location = model.Location;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;

            // ✅ تحديث معلومات المتجر (فقط للبائعين)
            bool urlsChanged = false;
            if (user.UserType == UserType.Seller)
            {
                // التحقق من وجود اسم المتجر
                if (string.IsNullOrEmpty(model.StoreName))
                {
                    ModelState.AddModelError("StoreName", "اسم المتجر مطلوب للبائعين");
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    return View(model);
                }

                user.StoreName = model.StoreName;
                user.StoreDescription = model.StoreDescription;

                // ✅ معالجة الروابط - تحتاج موافقة الإدارة
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

                    _logger.LogInformation("✅ البائع {UserId} قام بتعديل روابط المتجر. في انتظار موافقة الإدارة.", userId);
                }
            }

            user.UpdatedAt = DateTime.Now;

            // حفظ التغييرات
            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (result.Succeeded)
            {
                if (user.UserType == UserType.Seller && urlsChanged)
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

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        #endregion

        #region إدارة المتجر (للبائعين فقط)

        /// <summary>
        /// عرض صفحة إدارة المتجر - متاح للبائعين فقط
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManageStore()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory1)
                .Include(u => u.StoreCategories)
                    .ThenInclude(sc => sc.SubCategory2)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // ✅ حماية: التحقق من أن المستخدم بائع
            if (user.UserType != UserType.Seller)
            {
                _logger.LogWarning("⚠️ محاولة وصول غير مصرح بها لصفحة إدارة المتجر! UserId: {UserId}, UserType: {UserType}",
                    userId, user.UserType);
                TempData["ErrorMessage"] = "عذراً، هذه الصفحة متاحة للبائعين فقط.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(user);
        }

        /// <summary>
        /// تحديث معلومات المتجر - متاح للبائعين فقط
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStore(UpdateStoreViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View("ManageStore", model);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // ✅ حماية: التحقق من أن المستخدم بائع
            if (user.UserType != UserType.Seller)
            {
                _logger.LogWarning("⚠️ محاولة تحديث متجر غير مصرح بها! UserId: {UserId}, UserType: {UserType}",
                    userId, user.UserType);
                TempData["ErrorMessage"] = "غير مصرح لك بتحديث معلومات المتجر.";
                return RedirectToAction("Index");
            }

            // تحديث معلومات المتجر
            user.StoreName = model.StoreName;
            user.StoreDescription = model.StoreDescription;

            // معالجة الروابط - تحتاج موافقة الإدارة
            bool urlsChanged = false;
            if (model.WebsiteUrl1 != user.WebsiteUrl1 ||
                model.WebsiteUrl2 != user.WebsiteUrl2 ||
                model.WebsiteUrl3 != user.WebsiteUrl3)
            {
                user.PendingWebsiteUrl1 = model.WebsiteUrl1;
                user.PendingWebsiteUrl2 = model.WebsiteUrl2;
                user.PendingWebsiteUrl3 = model.WebsiteUrl3;
                user.HasPendingUrlChanges = true;
                urlsChanged = true;

                _logger.LogInformation("✅ البائع {UserId} قام بتعديل روابط المتجر في صفحة ManageStore.", userId);
            }

            // تحديث فئات المتجر إذا تم تعديلها
            if (model.StoreCategories != null && model.StoreCategories.Any())
            {
                // حذف الفئات القديمة
                var oldCategories = await _context.StoreCategories
                    .Where(sc => sc.UserId == userId)
                    .ToListAsync();
                _context.StoreCategories.RemoveRange(oldCategories);

                // إضافة الفئات الجديدة
                foreach (var subCategory2Id in model.StoreCategories)
                {
                    var subCategory2 = await _context.SubCategories2
                        .Include(sc2 => sc2.SubCategory1)
                        .FirstOrDefaultAsync(sc2 => sc2.Id == subCategory2Id);

                    if (subCategory2 != null)
                    {
                        var storeCategory = new StoreCategory
                        {
                            UserId = userId,
                            CategoryId = subCategory2.SubCategory1.CategoryId,
                            SubCategory1Id = subCategory2.SubCategory1Id,
                            SubCategory2Id = subCategory2.Id,
                            CreatedAt = DateTime.Now
                        };
                        _context.StoreCategories.Add(storeCategory);
                    }
                }
            }

            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (result.Succeeded)
            {
                if (urlsChanged)
                {
                    TempData["WarningMessage"] = "تم تحديث معلومات المتجر بنجاح. الروابط الجديدة بانتظار موافقة الإدارة.";
                }
                else
                {
                    TempData["SuccessMessage"] = "تم تحديث معلومات المتجر بنجاح";
                }
                return RedirectToAction("ManageStore");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View("ManageStore", await _userManager.FindByIdAsync(userId));
        }

        #endregion
    }
}

/*
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * ✅ ملخص التحسينات والحمايات المطبقة:
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * 
 * 1. ✅ تنظيم الكود في 3 Regions واضحة
 * 2. ✅ إضافة XML Comments لكل Method
 * 3. ✅ منع تغيير UserType بشكل نهائي مع تسجيل المحاولة
 * 4. ✅ منع المشتري من إضافة معلومات متجر مع رسالة خطأ واضحة
 * 5. ✅ مسح تلقائي لأي معلومات متجر من حساب المشتري
 * 6. ✅ حذف فئات المتجر من حساب المشتري تلقائياً
 * 7. ✅ MyRequests متاح للمشترين فقط
 * 8. ✅ Index يعرض الطلبات للمشترين فقط
 * 9. ✅ ManageStore متاح للبائعين فقط
 * 10. ✅ UpdateStore متاح للبائعين فقط
 * 11. ✅ تسجيل جميع المحاولات المشبوهة في Logs
 * 12. ✅ رسائل خطأ واضحة بالعربية
 * 13. ✅ معالجة الروابط المعلقة للبائعين
 * 
 * الحمايات المطبقة:
 * ─────────────────
 * ✓ حماية على مستوى Controller (Authorization)
 * ✓ حماية على مستوى Model Validation
 * ✓ حماية على مستوى Database (cleanup)
 * ✓ حماية على مستوى Logging (تسجيل المحاولات)
 * 
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 */

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ReverseMarket.Data;
//using ReverseMarket.Models;
//using ReverseMarket.Models.Identity;

//namespace ReverseMarket.Controllers
//{
//    [Authorize]
//    public class ProfileController : BaseController
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ILogger<ProfileController> _logger;

//        public ProfileController(
//            ApplicationDbContext context,
//            UserManager<ApplicationUser> userManager,
//            ILogger<ProfileController> logger) : base(context)
//        {
//            _userManager = userManager;
//            _logger = logger;
//        }

//        // عرض الملف الشخصي
//        public async Task<IActionResult> Index()
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var user = await _userManager.Users
//                .Include(u => u.StoreCategories)
//                    .ThenInclude(sc => sc.Category)
//                .FirstOrDefaultAsync(u => u.Id == userId);

//            if (user == null)
//            {
//                return NotFound();
//            }

//            // جلب طلبات المستخدم
//            var requests = await _context.Requests
//                .Where(r => r.UserId == userId)
//                .Include(r => r.Category)
//                .Include(r => r.SubCategory1)
//                .Include(r => r.Images)
//                .OrderByDescending(r => r.CreatedAt)
//                .ToListAsync();

//            ViewBag.Requests = requests;
//            ViewBag.TotalRequests = requests.Count;
//            ViewBag.ApprovedRequests = requests.Count(r => r.Status == RequestStatus.Approved);
//            ViewBag.PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending);
//            ViewBag.RejectedRequests = requests.Count(r => r.Status == RequestStatus.Rejected);

//            return View(user);
//        }

//        // عرض طلباتي
//        public async Task<IActionResult> MyRequests(int page = 1)
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var pageSize = 10;
//            var query = _context.Requests
//                .Where(r => r.UserId == userId)
//                .Include(r => r.Category)
//                .Include(r => r.SubCategory1)
//                .Include(r => r.Images)
//                .OrderByDescending(r => r.CreatedAt);

//            var totalRequests = await query.CountAsync();
//            var requests = await query
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();

//            var model = new MyRequestsViewModel
//            {
//                Requests = requests,
//                CurrentPage = page,
//                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize)
//            };

//            return View(model);
//        }

//        // تعديل الملف الشخصي
//        [HttpGet]
//        public async Task<IActionResult> Edit()
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound();
//            }

//            var model = new EditProfileViewModel
//            {
//                FirstName = user.FirstName,
//                LastName = user.LastName,
//                Email = user.Email,
//                PhoneNumber = user.PhoneNumber,
//                City = user.City,
//                District = user.District,
//                Location = user.Location,
//                DateOfBirth = user.DateOfBirth,
//                Gender = user.Gender,
//                StoreName = user.StoreName,
//                StoreDescription = user.StoreDescription,
//                WebsiteUrl1 = user.WebsiteUrl1,
//                WebsiteUrl2 = user.WebsiteUrl2,
//                WebsiteUrl3 = user.WebsiteUrl3,
//                // إضافة الروابط المعلقة إن وجدت
//                HasPendingUrlChanges = user.HasPendingUrlChanges,
//                PendingWebsiteUrl1 = user.PendingWebsiteUrl1,
//                PendingWebsiteUrl2 = user.PendingWebsiteUrl2,
//                PendingWebsiteUrl3 = user.PendingWebsiteUrl3, 

//    };

//            return View(model);
//        }

//        // حفظ التعديلات
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(EditProfileViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound();
//            }

//            // تحديث البيانات الأساسية (لا تحتاج موافقة)
//            user.FirstName = model.FirstName;
//            user.LastName = model.LastName;
//            user.Email = model.Email;
//            user.City = model.City;
//            user.District = model.District;
//            user.Location = model.Location;
//            user.DateOfBirth = model.DateOfBirth;
//            user.Gender = model.Gender;

//            if (user.UserType == UserType.Seller)
//            {
//                user.StoreName = model.StoreName;
//                user.StoreDescription = model.StoreDescription;

//                // ✅ معالجة الروابط - تحتاج موافقة الإدارة
//                bool urlsChanged = false;

//                // التحقق من تغيير الروابط
//                if (model.WebsiteUrl1 != user.WebsiteUrl1 ||
//                    model.WebsiteUrl2 != user.WebsiteUrl2 ||
//                    model.WebsiteUrl3 != user.WebsiteUrl3)
//                {
//                    // حفظ الروابط الجديدة كـ pending
//                    user.PendingWebsiteUrl1 = model.WebsiteUrl1;
//                    user.PendingWebsiteUrl2 = model.WebsiteUrl2;
//                    user.PendingWebsiteUrl3 = model.WebsiteUrl3;
//                    user.HasPendingUrlChanges = true;
//                    urlsChanged = true;
//                }
//            }

//            user.UpdatedAt = DateTime.Now;

//            var result = await _userManager.UpdateAsync(user);
//            if (result.Succeeded)
//            {
//                if (user.UserType == UserType.Seller && user.HasPendingUrlChanges)
//                {
//                    TempData["WarningMessage"] = "تم حفظ تعديلاتك بنجاح. الروابط الجديدة بانتظار موافقة الإدارة.";
//                }
//                else
//                {
//                    TempData["SuccessMessage"] = "تم تحديث ملفك الشخصي بنجاح";
//                }
//                return RedirectToAction("Index");
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError("", error.Description);
//            }

//            return View(model);
//        }
//    }
//}