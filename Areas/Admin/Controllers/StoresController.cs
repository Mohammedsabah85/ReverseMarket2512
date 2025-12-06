using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models.Identity;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Models;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WhatsAppService _whatsAppService;
        private readonly ILogger<StoresController> _logger;

        public StoresController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            WhatsAppService whatsAppService,
            ILogger<StoresController> logger)
        {
            _context = context;
            _userManager = userManager;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        // 📋 عرض جميع المتاجر
        public async Task<IActionResult> Index(string searchTerm, bool? isActive, bool? isApproved)
        {
            var query = _userManager.Users
                .Where(u => u.UserType == UserType.Seller)
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.Category)
                .AsQueryable();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.StoreName.Contains(searchTerm) || 
                                        u.FirstName.Contains(searchTerm) || 
                                        u.LastName.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(u => u.IsStoreApproved == isApproved.Value);
            }

            var stores = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.IsActive = isActive;
            ViewBag.IsApproved = isApproved;

            return View(stores);
        }

        // 📝 صفحة التعديل
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var store = await _userManager.Users
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Seller);

            if (store == null)
            {
                return NotFound();
            }

            // جلب جميع الفئات المتاحة
            ViewBag.AllCategories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(store);
        }

        // 💾 حفظ التعديلات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model, string[] selectedCategories)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var store = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (store == null)
            {
                return NotFound();
            }

            try
            {
                // تحديث البيانات الأساسية
                store.StoreName = model.StoreName;
                store.StoreDescription = model.StoreDescription;
                store.FirstName = model.FirstName;
                store.LastName = model.LastName;
                store.PhoneNumber = model.PhoneNumber;
                store.Email = model.Email;
               
                store.City = model.City;
                store.WebsiteUrl1 = model.WebsiteUrl1;
                store.WebsiteUrl2 = model.WebsiteUrl2;
                store.WebsiteUrl3 = model.WebsiteUrl3;
                store.UpdatedAt = DateTime.Now;

                // تحديث الفئات
                if (selectedCategories != null && selectedCategories.Length > 0)
                {
                    // حذف الفئات القديمة
                    _context.StoreCategories.RemoveRange(store.StoreCategories);

                    // إضافة الفئات الجديدة
                    foreach (var categoryId in selectedCategories)
                    {
                        if (int.TryParse(categoryId, out int catId))
                        {
                            store.StoreCategories.Add(new StoreCategory
                            {
                                UserId = store.Id,
                                CategoryId = catId
                            });
                        }
                    }
                }

                await _userManager.UpdateAsync(store);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"تم تحديث بيانات متجر {store.StoreName} بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المتجر");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث المتجر";
                
                ViewBag.AllCategories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                return View(model);
            }
        }

        // 🔄 تفعيل/إيقاف المتجر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var store = await _userManager.FindByIdAsync(id);
            if (store == null || store.UserType != UserType.Seller)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction(nameof(Index));
            }

            store.IsActive = !store.IsActive;
            store.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(store);

            if (result.Succeeded)
            {
                var status = store.IsActive ? "تفعيل" : "إيقاف";
                await NotifyStoreStatusChangeAsync(store, store.IsActive);
                TempData["SuccessMessage"] = $"تم {status} متجر {store.StoreName} بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تغيير حالة المتجر";
            }

            return RedirectToAction(nameof(Index));
        }

        // 🗑️ حذف المتجر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var store = await _userManager.Users
                .Include(u => u.StoreCategories)
                .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Seller);

            if (store == null)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // حذف الفئات المرتبطة
                _context.StoreCategories.RemoveRange(store.StoreCategories);

                // حذف المتجر
                var result = await _userManager.DeleteAsync(store);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"تم حذف متجر {store.StoreName} بنجاح";
                }
                else
                {
                    TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المتجر";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المتجر");
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المتجر";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PendingApproval()
        {
            var pendingStores = await _userManager.Users
                .Where(u => u.UserType == UserType.Seller && !u.IsStoreApproved)
                .Include(u => u.StoreCategories)
                .ThenInclude(sc => sc.Category)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(pendingStores);
        }

        // ✅ صفحة جديدة لمراجعة الروابط المعلقة
        public async Task<IActionResult> PendingUrlChanges()
        {
            var storesWithPendingUrls = await _userManager.Users
                .Where(u => u.UserType == UserType.Seller && u.HasPendingUrlChanges)
                .OrderByDescending(u => u.UpdatedAt)
                .ToListAsync();

            return View(storesWithPendingUrls);
        }

        // ✅ الموافقة على الروابط الجديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUrlChanges(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.UserType != UserType.Seller)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction("PendingUrlChanges");
            }

            // نقل الروابط من Pending إلى الروابط الفعلية
            user.WebsiteUrl1 = user.PendingWebsiteUrl1;
            user.WebsiteUrl2 = user.PendingWebsiteUrl2;
            user.WebsiteUrl3 = user.PendingWebsiteUrl3;

            // إعادة تعيين الحقول المعلقة
            user.PendingWebsiteUrl1 = null;
            user.PendingWebsiteUrl2 = null;
            user.PendingWebsiteUrl3 = null;
            user.HasPendingUrlChanges = false;
            user.UrlsLastApprovedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // إرسال إشعار بالموافقة
                await NotifyUrlApprovalAsync(user);

                TempData["SuccessMessage"] = $"تم اعتماد الروابط الجديدة لمتجر {user.StoreName}";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء اعتماد الروابط";
            }

            return RedirectToAction("PendingUrlChanges");
        }

        // ✅ رفض الروابط الجديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUrlChanges(string id, string reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction("PendingUrlChanges");
            }

            // حذف الروابط المعلقة
            user.PendingWebsiteUrl1 = null;
            user.PendingWebsiteUrl2 = null;
            user.PendingWebsiteUrl3 = null;
            user.HasPendingUrlChanges = false;

            await _userManager.UpdateAsync(user);

            // إرسال إشعار بالرفض
            await NotifyUrlRejectionAsync(user, reason);

            TempData["SuccessMessage"] = $"تم رفض الروابط الجديدة لمتجر {user.StoreName}";
            return RedirectToAction("PendingUrlChanges");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.UserType != UserType.Seller)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction("PendingApproval");
            }

            user.IsStoreApproved = true;
            user.StoreApprovedAt = DateTime.Now;
            user.StoreApprovedBy = User.Identity.Name;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await NotifyStoreApprovalAsync(user);
                TempData["SuccessMessage"] = $"تم اعتماد متجر {user.StoreName} بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء اعتماد المتجر";
            }

            return RedirectToAction("PendingApproval");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStore(string id, string reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المتجر غير موجود";
                return RedirectToAction("PendingApproval");
            }

            await NotifyStoreRejectionAsync(user, reason);

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"تم رفض متجر {user.StoreName}";
            return RedirectToAction("PendingApproval");
        }

        // 📧 إشعار تغيير حالة المتجر
        private async Task NotifyStoreStatusChangeAsync(ApplicationUser store, bool isActive)
        {
            try
            {
                if (!string.IsNullOrEmpty(store.PhoneNumber))
                {
                    var status = isActive ? "تفعيل" : "إيقاف";
                    var message = $"مرحباً {store.StoreName}!\n\n" +
                                 $"تم {status} متجرك في السوق العكسي.\n\n";

                    if (isActive)
                    {
                        message += "يمكنك الآن استقبال الطلبات والتواصل مع العملاء.\n\n";
                    }
                    else
                    {
                        message += "في حالة وجود استفسار، يرجى التواصل معنا.\n\n";
                    }

                    message += "شكراً لك - السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = store.PhoneNumber,
                        message = message
                    };

                    await _whatsAppService.SendMessageAsync(whatsAppRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار تغيير حالة المتجر");
            }
        }

        // ✅ إرسال إشعار الموافقة على الروابط
        private async Task NotifyUrlApprovalAsync(ApplicationUser store)
        {
            try
            {
                if (!string.IsNullOrEmpty(store.PhoneNumber))
                {
                    var message = $"مرحباً {store.StoreName}!\n\n" +
                                 $"تم اعتماد تحديثات الروابط الخاصة بمتجرك ✅\n\n" +
                                 $"الروابط الجديدة:\n";

                    if (!string.IsNullOrEmpty(store.WebsiteUrl1))
                        message += $"• {store.WebsiteUrl1}\n";
                    if (!string.IsNullOrEmpty(store.WebsiteUrl2))
                        message += $"• {store.WebsiteUrl2}\n";
                    if (!string.IsNullOrEmpty(store.WebsiteUrl3))
                        message += $"• {store.WebsiteUrl3}\n";

                    message += "\nشكراً لك - السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = store.PhoneNumber,
                        message = message
                    };

                    var result = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                    if (result.Success)
                    {
                        _logger.LogInformation("✅ تم إرسال إشعار الموافقة على الروابط إلى {PhoneNumber}",
                            store.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الموافقة على الروابط");
            }
        }

        // ✅ إرسال إشعار رفض الروابط
        private async Task NotifyUrlRejectionAsync(ApplicationUser store, string reason)
        {
            try
            {
                if (!string.IsNullOrEmpty(store.PhoneNumber))
                {
                    var message = $"مرحباً {store.StoreName}!\n\n" +
                                 $"نأسف لإبلاغك بأن الروابط الجديدة لم تتم الموافقة عليها.\n\n";

                    if (!string.IsNullOrEmpty(reason))
                    {
                        message += $"السبب: {reason}\n\n";
                    }

                    message += "يمكنك إعادة المحاولة بروابط أخرى.\n\n" +
                              "شكراً لتفهمك - السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = store.PhoneNumber,
                        message = message
                    };

                    await _whatsAppService.SendMessageAsync(whatsAppRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض الروابط");
            }
        }

        private async Task NotifyStoreApprovalAsync(ApplicationUser store)
        {
            try
            {
                if (!string.IsNullOrEmpty(store.PhoneNumber))
                {
                    var message = $"🎉 تهانينا {store.StoreName}!\n\n" +
                                 $"تم اعتماد متجرك في السوق العكسي بنجاح! ✅\n\n" +
                                 $"يمكنك الآن:\n" +
                                 $"• استقبال الطلبات الجديدة\n" +
                                 $"• التواصل مع العملاء\n" +
                                 $"• تقديم عروضك الخاصة\n\n" +
                                 $"نتمنى لك تجربة موفقة معنا!\n\n" +
                                 $"السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = store.PhoneNumber,
                        message = message
                    };

                    var result = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                    if (result.Success)
                    {
                        _logger.LogInformation("✅ تم إرسال إشعار الموافقة على المتجر بنجاح إلى {PhoneNumber}",
                            store.PhoneNumber);
                    }
                    else
                    {
                        _logger.LogError("❌ فشل إرسال إشعار الموافقة على المتجر إلى {PhoneNumber}: {Error}",
                            store.PhoneNumber, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الموافقة على المتجر");
            }
        }

        private async Task NotifyStoreRejectionAsync(ApplicationUser store, string reason)
        {
            try
            {
                if (!string.IsNullOrEmpty(store.PhoneNumber))
                {
                    var message = $"مرحباً {store.StoreName}!\n\n" +
                                 $"نأسف لإبلاغك بأن طلب اعتماد متجرك لم تتم الموافقة عليه.\n\n";

                    if (!string.IsNullOrEmpty(reason))
                    {
                        message += $"السبب: {reason}\n\n";
                    }

                    message += "يمكنك التواصل معنا لمزيد من التفاصيل.\n\n" +
                              "شكراً لتفهمك - السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = store.PhoneNumber,
                        message = message
                    };

                    var result = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                    if (result.Success)
                    {
                        _logger.LogInformation("✅ تم إرسال إشعار رفض المتجر بنجاح إلى {PhoneNumber}",
                            store.PhoneNumber);
                    }
                    else
                    {
                        _logger.LogError("❌ فشل إرسال إشعار رفض المتجر إلى {PhoneNumber}: {Error}",
                            store.PhoneNumber, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار رفض المتجر");
            }
        }
    }
}
