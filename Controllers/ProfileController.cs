using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.Services;
using System.Text.Json;

namespace ReverseMarket.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProfileController> _logger;
        private readonly IFileService _fileService;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ProfileController> logger,
            IFileService fileService) : base(context)
        {
            _userManager = userManager;
            _logger = logger;
            _fileService = fileService;
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

            // ✅ جلب فئات المتجر الحالية للبائعين
            List<StoreCategoryDisplay>? currentStoreCategories = null;
            if (user.UserType == UserType.Seller && user.StoreCategories != null && user.StoreCategories.Any())
            {
                currentStoreCategories = user.StoreCategories.Select(sc => new StoreCategoryDisplay
                {
                    SubCategory2Id = sc.SubCategory2Id ?? 0,
                    CategoryName = sc.Category?.Name ?? "",
                    SubCategory1Name = sc.SubCategory1?.Name ?? "",
                    SubCategory2Name = sc.SubCategory2?.Name ?? ""
                }).ToList();
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImage = user.ProfileImage,
                City = user.City,
                District = user.District,
                Location = user.Location,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                UserType = user.UserType,

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
                PendingWebsiteUrl3 = user.PendingWebsiteUrl3,

                // حالة كل رابط
                PendingUrl1Status = user.PendingUrl1Status,
                PendingUrl2Status = user.PendingUrl2Status,
                PendingUrl3Status = user.PendingUrl3Status,

                // فئات المتجر الحالية
                CurrentStoreCategories = currentStoreCategories,

                // ═══════════════════════════════════════════════════════════════════════════════
                // ✅ تفضيلات التواصل (للمشترين)
                // ═══════════════════════════════════════════════════════════════════════════════
                AllowPhoneCall = user.AllowPhoneCall,
                AllowWhatsApp = user.AllowWhatsApp,
                AllowEmail = user.AllowEmail,
                AllowSMS = user.AllowSMS,
                AllowInAppChat = user.AllowInAppChat,
                ContactNotes = user.ContactNotes,
                AlternativeWhatsApp = user.AlternativeWhatsApp,
                PreferredContactTime = user.PreferredContactTime
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

            // ✅ معالجة رفع الصورة الشخصية
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                try
                {
                    // حذف الصورة القديمة إذا كانت موجودة
                    if (!string.IsNullOrEmpty(user.ProfileImage))
                    {
                        await _fileService.DeleteImageAsync(user.ProfileImage);
                    }

                    // رفع الصورة الجديدة
                    var imagePath = await _fileService.SaveImageAsync(model.ProfileImageFile, "profiles");
                    user.ProfileImage = imagePath;

                    _logger.LogInformation("✅ تم تحديث الصورة الشخصية للمستخدم: {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ خطأ في رفع الصورة الشخصية");
                    ModelState.AddModelError("ProfileImageFile", "حدث خطأ أثناء رفع الصورة. حاول مرة أخرى.");
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    model.UserType = user.UserType;
                    return View(model);
                }
            }

            // ✅ الحماية: منع المشتري من إضافة معلومات متجر
            if (user.UserType == UserType.Buyer)
            {
                if (!string.IsNullOrEmpty(model.StoreName) ||
                    !string.IsNullOrEmpty(model.StoreDescription) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl1) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl2) ||
                    !string.IsNullOrEmpty(model.WebsiteUrl3))
                {
                    _logger.LogWarning("⚠️ محاولة إضافة معلومات متجر من قبل مشتري! UserId: {UserId}", userId);

                    TempData["ErrorMessage"] = "المشترون لا يمكنهم إضافة معلومات متجر. نوع حسابك هو: مشتري.";
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    model.UserType = user.UserType;
                    return View(model);
                }

                // ✅ حماية إضافية: مسح أي معلومات متجر
                user.StoreName = null;
                user.StoreDescription = null;
                user.WebsiteUrl1 = null;
                user.WebsiteUrl2 = null;
                user.WebsiteUrl3 = null;
                user.PendingWebsiteUrl1 = null;
                user.PendingWebsiteUrl2 = null;
                user.PendingWebsiteUrl3 = null;
                user.PendingUrl1Status = null;
                user.PendingUrl2Status = null;
                user.PendingUrl3Status = null;
                user.HasPendingUrlChanges = false;
                user.IsStoreApproved = true;

                var existingStoreCategories = await _context.StoreCategories
                    .Where(sc => sc.UserId == userId)
                    .ToListAsync();

                if (existingStoreCategories.Any())
                {
                    _context.StoreCategories.RemoveRange(existingStoreCategories);
                    _logger.LogWarning("⚠️ تم حذف {Count} فئة متجر من حساب مشتري: {UserId}",
                        existingStoreCategories.Count, userId);
                }

                // ═══════════════════════════════════════════════════════════════════════════════
                // ✅ حفظ تفضيلات التواصل (للمشترين فقط)
                // ═══════════════════════════════════════════════════════════════════════════════

                // التحقق من وجود وسيلة تواصل واحدة على الأقل
                if (!model.AllowPhoneCall && !model.AllowWhatsApp && !model.AllowEmail &&
                    !model.AllowSMS && !model.AllowInAppChat)
                {
                    ModelState.AddModelError("", "يجب تفعيل وسيلة تواصل واحدة على الأقل");
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    model.UserType = user.UserType;
                    return View(model);
                }

                user.AllowPhoneCall = model.AllowPhoneCall;
                user.AllowWhatsApp = model.AllowWhatsApp;
                user.AllowEmail = model.AllowEmail;
                user.AllowSMS = model.AllowSMS;
                user.AllowInAppChat = model.AllowInAppChat;
                user.ContactNotes = model.ContactNotes?.Trim();
                user.AlternativeWhatsApp = model.AlternativeWhatsApp?.Trim();
                user.PreferredContactTime = model.PreferredContactTime;

                _logger.LogInformation("✅ تم تحديث تفضيلات التواصل للمشتري: {UserId}. " +
                    "Phone: {Phone}, WhatsApp: {WhatsApp}, Email: {Email}, SMS: {SMS}, Chat: {Chat}",
                    userId, model.AllowPhoneCall, model.AllowWhatsApp, model.AllowEmail,
                    model.AllowSMS, model.AllowInAppChat);
            }

            // ✅ تحديث البيانات الأساسية
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.City = model.City;
            user.District = model.District;
            user.Location = model.Location;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;

            // ✅ تحديث معلومات المتجر (فقط للبائعين)
            int urlsChangedCount = 0;
            if (user.UserType == UserType.Seller)
            {
                if (string.IsNullOrEmpty(model.StoreName))
                {
                    ModelState.AddModelError("StoreName", "اسم المتجر مطلوب للبائعين");
                    ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    model.UserType = user.UserType;
                    await LoadCurrentStoreCategories(model, userId);
                    return View(model);
                }

                user.StoreName = model.StoreName;
                user.StoreDescription = model.StoreDescription;

                // معالجة الروابط بشكل منفصل
                if (model.WebsiteUrl1 != user.WebsiteUrl1)
                {
                    if (model.WebsiteUrl1 != user.PendingWebsiteUrl1 || user.PendingUrl1Status != "Pending")
                    {
                        user.PendingWebsiteUrl1 = model.WebsiteUrl1;
                        user.PendingUrl1Status = "Pending";
                        user.PendingUrl1SubmittedAt = DateTime.Now;
                        urlsChangedCount++;
                    }
                }

                if (model.WebsiteUrl2 != user.WebsiteUrl2)
                {
                    if (model.WebsiteUrl2 != user.PendingWebsiteUrl2 || user.PendingUrl2Status != "Pending")
                    {
                        user.PendingWebsiteUrl2 = model.WebsiteUrl2;
                        user.PendingUrl2Status = "Pending";
                        user.PendingUrl2SubmittedAt = DateTime.Now;
                        urlsChangedCount++;
                    }
                }

                if (model.WebsiteUrl3 != user.WebsiteUrl3)
                {
                    if (model.WebsiteUrl3 != user.PendingWebsiteUrl3 || user.PendingUrl3Status != "Pending")
                    {
                        user.PendingWebsiteUrl3 = model.WebsiteUrl3;
                        user.PendingUrl3Status = "Pending";
                        user.PendingUrl3SubmittedAt = DateTime.Now;
                        urlsChangedCount++;
                    }
                }

                user.HasPendingUrlChanges =
                    user.PendingUrl1Status == "Pending" ||
                    user.PendingUrl2Status == "Pending" ||
                    user.PendingUrl3Status == "Pending";

                // ✅ تحديث فئات المتجر
                if (!string.IsNullOrWhiteSpace(model.StoreCategories))
                {
                    try
                    {
                        var categoryIds = JsonSerializer.Deserialize<List<int>>(model.StoreCategories);

                        if (categoryIds != null && categoryIds.Any())
                        {
                            var oldCategories = await _context.StoreCategories
                                .Where(sc => sc.UserId == userId)
                                .ToListAsync();
                            _context.StoreCategories.RemoveRange(oldCategories);

                            foreach (var subCategory2Id in categoryIds)
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

                            _logger.LogInformation("✅ تم تحديث فئات المتجر للبائع {UserId}. عدد الفئات: {Count}",
                                userId, categoryIds.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ خطأ في معالجة فئات المتجر");
                    }
                }
            }

            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (result.Succeeded)
            {
                if (user.UserType == UserType.Seller && urlsChangedCount > 0)
                {
                    TempData["WarningMessage"] = $"تم حفظ تعديلاتك بنجاح. {urlsChangedCount} رابط/روابط جديدة بانتظار موافقة الإدارة.";
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
            model.UserType = user.UserType;
            await LoadCurrentStoreCategories(model, userId);
            return View(model);
        }

        /// <summary>
        /// جلب فئات المتجر الحالية للعرض
        /// </summary>
        private async Task LoadCurrentStoreCategories(EditProfileViewModel model, string userId)
        {
            var storeCategories = await _context.StoreCategories
                .Where(sc => sc.UserId == userId)
                .Include(sc => sc.Category)
                .Include(sc => sc.SubCategory1)
                .Include(sc => sc.SubCategory2)
                .ToListAsync();

            if (storeCategories.Any())
            {
                model.CurrentStoreCategories = storeCategories.Select(sc => new StoreCategoryDisplay
                {
                    SubCategory2Id = sc.SubCategory2Id ?? 0,
                    CategoryName = sc.Category?.Name ?? "",
                    SubCategory1Name = sc.SubCategory1?.Name ?? "",
                    SubCategory2Name = sc.SubCategory2?.Name ?? ""
                }).ToList();
            }
        }

        #endregion

        #region ═══════════════════════════════════════════════════════════════════════════════
        // ✅ صفحة إعدادات التواصل المنفصلة (اختياري)
        #endregion

        /// <summary>
        /// عرض صفحة إعدادات التواصل - للمشترين فقط
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ContactSettings()
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

            // ✅ متاح للمشترين فقط
            if (user.UserType != UserType.Buyer)
            {
                TempData["ErrorMessage"] = "إعدادات التواصل متاحة للمشترين فقط.";
                return RedirectToAction("Index");
            }

            var model = new ContactPreferencesViewModel
            {
                AllowPhoneCall = user.AllowPhoneCall,
                AllowWhatsApp = user.AllowWhatsApp,
                AllowEmail = user.AllowEmail,
                AllowSMS = user.AllowSMS,
                AllowInAppChat = user.AllowInAppChat,
                ContactNotes = user.ContactNotes,
                AlternativeWhatsApp = user.AlternativeWhatsApp,
                PreferredContactTime = user.PreferredContactTime
            };

            return View(model);
        }

        /// <summary>
        /// حفظ إعدادات التواصل
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactSettings(ContactPreferencesViewModel model)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.UserType != UserType.Buyer)
            {
                TempData["ErrorMessage"] = "إعدادات التواصل متاحة للمشترين فقط.";
                return RedirectToAction("Index");
            }

            // التحقق من وجود وسيلة واحدة على الأقل
            if (!model.HasAtLeastOneContactMethod())
            {
                ModelState.AddModelError("", "يجب تفعيل وسيلة تواصل واحدة على الأقل");
                return View(model);
            }

            user.AllowPhoneCall = model.AllowPhoneCall;
            user.AllowWhatsApp = model.AllowWhatsApp;
            user.AllowEmail = model.AllowEmail;
            user.AllowSMS = model.AllowSMS;
            user.AllowInAppChat = model.AllowInAppChat;
            user.ContactNotes = model.ContactNotes?.Trim();
            user.AlternativeWhatsApp = model.AlternativeWhatsApp?.Trim();
            user.PreferredContactTime = model.PreferredContactTime;
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("✅ تم تحديث إعدادات التواصل للمشتري: {UserId}", userId);
                TempData["SuccessMessage"] = "تم حفظ إعدادات التواصل بنجاح";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

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

            if (user.UserType != UserType.Seller)
            {
                _logger.LogWarning("⚠️ محاولة تحديث متجر غير مصرح بها! UserId: {UserId}, UserType: {UserType}",
                    userId, user.UserType);
                TempData["ErrorMessage"] = "غير مصرح لك بتحديث معلومات المتجر.";
                return RedirectToAction("Index");
            }

            user.StoreName = model.StoreName;
            user.StoreDescription = model.StoreDescription;

            int urlsChangedCount = 0;

            if (model.WebsiteUrl1 != user.WebsiteUrl1)
            {
                if (model.WebsiteUrl1 != user.PendingWebsiteUrl1 || user.PendingUrl1Status != "Pending")
                {
                    user.PendingWebsiteUrl1 = model.WebsiteUrl1;
                    user.PendingUrl1Status = "Pending";
                    user.PendingUrl1SubmittedAt = DateTime.Now;
                    urlsChangedCount++;
                }
            }

            if (model.WebsiteUrl2 != user.WebsiteUrl2)
            {
                if (model.WebsiteUrl2 != user.PendingWebsiteUrl2 || user.PendingUrl2Status != "Pending")
                {
                    user.PendingWebsiteUrl2 = model.WebsiteUrl2;
                    user.PendingUrl2Status = "Pending";
                    user.PendingUrl2SubmittedAt = DateTime.Now;
                    urlsChangedCount++;
                }
            }

            if (model.WebsiteUrl3 != user.WebsiteUrl3)
            {
                if (model.WebsiteUrl3 != user.PendingWebsiteUrl3 || user.PendingUrl3Status != "Pending")
                {
                    user.PendingWebsiteUrl3 = model.WebsiteUrl3;
                    user.PendingUrl3Status = "Pending";
                    user.PendingUrl3SubmittedAt = DateTime.Now;
                    urlsChangedCount++;
                }
            }

            user.HasPendingUrlChanges =
                user.PendingUrl1Status == "Pending" ||
                user.PendingUrl2Status == "Pending" ||
                user.PendingUrl3Status == "Pending";

            if (model.StoreCategories != null && model.StoreCategories.Any())
            {
                var oldCategories = await _context.StoreCategories
                    .Where(sc => sc.UserId == userId)
                    .ToListAsync();
                _context.StoreCategories.RemoveRange(oldCategories);

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
                if (urlsChangedCount > 0)
                {
                    TempData["WarningMessage"] = $"تم تحديث معلومات المتجر بنجاح. {urlsChangedCount} رابط/روابط جديدة بانتظار موافقة الإدارة.";
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

        #region API Endpoints للفئات

        /// <summary>
        /// جلب الفئات الفرعية الأولى حسب الفئة الرئيسية
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubCategories1(int categoryId)
        {
            var subCategories = await _context.SubCategories1
                .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }

        /// <summary>
        /// جلب الفئات الفرعية الثانية حسب الفئة الفرعية الأولى
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSubCategories2(int subCategory1Id)
        {
            var subCategories = await _context.SubCategories2
                .Where(sc => sc.SubCategory1Id == subCategory1Id && sc.IsActive)
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }

        #endregion
    }
}