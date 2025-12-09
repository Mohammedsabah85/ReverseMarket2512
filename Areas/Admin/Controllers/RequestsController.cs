using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
using ReverseMarket.Areas.Admin.Models;
using ReverseMarket.CustomWhatsappService;
using ReverseMarket.Services;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<RequestsController> _logger;
        private readonly WhatsAppService _whatsAppService;
        private readonly INotificationService _notificationService;

        public RequestsController(
            ApplicationDbContext context,
            ILogger<RequestsController> logger,
            WhatsAppService whatsAppService,
            INotificationService notificationService)
        {
            _dbContext = context;
            _logger = logger;
            _whatsAppService = whatsAppService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(RequestStatus? status = null, int page = 1)
        {
            var pageSize = 20;

            var query = _dbContext.Requests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var totalRequests = await query.CountAsync();
            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new AdminRequestsViewModel
            {
                Requests = requests,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize),
                StatusFilter = status
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _dbContext.Requests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status, string? adminNotes = null)
        {
            try
            {
                var request = await _dbContext.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                {
                    TempData["ErrorMessage"] = "الطلب غير موجود";
                    return RedirectToAction("Index");
                }

                if (!Enum.IsDefined(typeof(RequestStatus), status))
                {
                    TempData["ErrorMessage"] = "حالة الطلب غير صحيحة";
                    return RedirectToAction("Details", new { id });
                }

                var requestStatus = (RequestStatus)status;
                request.Status = requestStatus;
                request.AdminNotes = adminNotes;

                if (requestStatus == RequestStatus.Approved)
                {
                    request.ApprovedAt = DateTime.Now;

                    // حفظ التغييرات أولاً
                    await _dbContext.SaveChangesAsync();

                    // ✅ إرسال إشعار للمستخدم بالموافقة
                    await SendApprovalNotificationAsync(request);

                    // ✅ إرسال إشعار للمتاجر المتخصصة بنفس الفئة الفرعية الثانية
                    await SendStoreNotificationsAsync(request);
                }
                else if (requestStatus == RequestStatus.Rejected)
                {
                    await _dbContext.SaveChangesAsync();

                    // ✅ إرسال إشعار بالرفض
                    await SendRejectionNotificationAsync(request);
                }
                else
                {
                    await _dbContext.SaveChangesAsync();
                }

                var statusText = requestStatus switch
                {
                    RequestStatus.Approved => "تم اعتماد",
                    RequestStatus.Rejected => "تم رفض",
                    RequestStatus.Postponed => "تم تأجيل",
                    _ => "تم تحديث"
                };

                TempData["SuccessMessage"] = $"{statusText} الطلب بنجاح";

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث حالة الطلب: {RequestId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث حالة الطلب";

                return RedirectToAction("Details", new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _dbContext.Requests
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _dbContext.Categories.Where(c => c.IsActive).ToListAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Request model)
        {
            try
            {
                var request = await _dbContext.Requests.FindAsync(model.Id);
                if (request == null)
                {
                    return NotFound();
                }

                request.Title = model.Title;
                request.Description = model.Description;
                request.CategoryId = model.CategoryId;
                request.SubCategory1Id = model.SubCategory1Id;
                request.SubCategory2Id = model.SubCategory2Id;
                request.City = model.City;
                request.District = model.District;
                request.Location = model.Location;
                request.AdminNotes = model.AdminNotes;

                _dbContext.Update(request);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث الطلب بنجاح";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الطلب: {RequestId}", model.Id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث الطلب";

                ViewBag.Categories = await _dbContext.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var request = await _dbContext.Requests
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                {
                    TempData["ErrorMessage"] = "الطلب غير موجود";
                    return RedirectToAction("Index");
                }

                if (request.Images != null && request.Images.Any())
                {
                    _dbContext.RequestImages.RemoveRange(request.Images);
                }

                _dbContext.Requests.Remove(request);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم حذف الطلب بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الطلب: {RequestId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الطلب";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRequestStatus(int id)
        {
            try
            {
                var request = await _dbContext.Requests.FindAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "الطلب غير موجود";
                    return RedirectToAction("Index");
                }

                if (request.Status == RequestStatus.Approved)
                {
                    request.Status = RequestStatus.Postponed;
                    TempData["SuccessMessage"] = "تم إيقاف الطلب";
                }
                else if (request.Status == RequestStatus.Postponed)
                {
                    request.Status = RequestStatus.Approved;
                    request.ApprovedAt = DateTime.Now;
                    TempData["SuccessMessage"] = "تم تفعيل الطلب";
                }

                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تبديل حالة الطلب: {RequestId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تبديل حالة الطلب";
                return RedirectToAction("Details", new { id });
            }
        }

        #region ✅ نظام الإشعارات المحسن

        /// <summary>
        /// إرسال إشعار موافقة للمشتري عبر جميع القنوات (نظام + إيميل + واتساب)
        /// </summary>
        private async Task SendApprovalNotificationAsync(Request request)
        {
            try
            {
                // ✅ بناء رابط الطلب الكامل
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var requestLink = $"/Requests/Details/{request.Id}";
                var fullRequestUrl = $"{baseUrl}{requestLink}";

                var title = "🎉 تم اعتماد طلبك!";
                var message = $"مرحباً {request.User?.FirstName}!\n\n" +
                             $"يسعدنا إبلاغك بأنه تم اعتماد طلبك:\n\n" +
                             $"📋 العنوان: {request.Title}\n\n" +
                             $"سيتم الآن عرض طلبك للمتاجر المتخصصة وستتلقى عروضاً قريباً.\n\n" +
                             $"🔗 لمشاهدة طلبك:\n{fullRequestUrl}\n\n" +
                             $"شكراً لاستخدامك السوق العكسي! 🛒";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestApproved,
                    userId: request.UserId,
                    requestId: request.Id,
                    link: requestLink,
                    isFromAdmin: true,
                    adminId: User.Identity?.Name
                );

                await _notificationService.SendNotificationAsync(notification,
                    sendEmail: true,
                    sendWhatsApp: true,
                    sendInApp: true);

                _logger.LogInformation("✅ تم إرسال إشعار الموافقة للمشتري {UserId} للطلب #{RequestId}",
                    request.UserId, request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في إرسال إشعار الموافقة للطلب #{RequestId}", request.Id);
            }
        }

        /// <summary>
        /// إرسال إشعار رفض للمشتري عبر جميع القنوات
        /// </summary>
        private async Task SendRejectionNotificationAsync(Request request)
        {
            try
            {
                var title = "تحديث حول طلبك";
                var message = $"مرحباً {request.User?.FirstName}!\n\n" +
                             $"نأسف لإبلاغك بأن طلبك: \"{request.Title}\" لم تتم الموافقة عليه.\n\n";

                if (!string.IsNullOrEmpty(request.AdminNotes))
                {
                    message += $"السبب: {request.AdminNotes}\n\n";
                }

                message += "يمكنك إضافة طلب جديد في أي وقت.\n\n" +
                          "شكراً لتفهمك - السوق العكسي 🛒";

                var notification = await _notificationService.CreateNotificationAsync(
                    title: title,
                    message: message,
                    type: NotificationType.RequestRejected,
                    userId: request.UserId,
                    requestId: request.Id,
                    isFromAdmin: true,
                    adminId: User.Identity?.Name
                );

                await _notificationService.SendNotificationAsync(notification,
                    sendEmail: true,
                    sendWhatsApp: true,
                    sendInApp: true);

                _logger.LogInformation("✅ تم إرسال إشعار الرفض للمشتري {UserId} للطلب #{RequestId}",
                    request.UserId, request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في إرسال إشعار الرفض للطلب #{RequestId}", request.Id);
            }
        }

        /// <summary>
        /// ✅ إرسال إشعارات للمتاجر المتخصصة حول طلب جديد معتمد
        /// يتم الإرسال فقط للمتاجر التي لديها نفس الفئة الفرعية الثانية (SubCategory2)
        /// </summary>
        private async Task SendStoreNotificationsAsync(Request request)
        {
            try
            {
                _logger.LogInformation("🔍 بدء إرسال إشعارات للمتاجر المتخصصة للطلب #{RequestId}", request.Id);

                // جلب بيانات الطلب كاملة
                var fullRequest = await _dbContext.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (fullRequest == null)
                {
                    _logger.LogError("❌ لم يتم العثور على الطلب #{RequestId}", request.Id);
                    return;
                }

                // ✅ التحقق من وجود SubCategory2Id
                if (!fullRequest.SubCategory2Id.HasValue)
                {
                    _logger.LogWarning("⚠️ الطلب #{RequestId} لا يحتوي على فئة فرعية ثانية. لن يتم إرسال إشعارات للمتاجر.", request.Id);
                    return;
                }

                // البحث عن المتاجر المتخصصة بنفس الفئة الفرعية الثانية فقط
                var relevantStores = await GetRelevantStoresBySubCategory2Async(fullRequest.SubCategory2Id.Value);

                if (!relevantStores.Any())
                {
                    _logger.LogWarning("⚠️ لم يتم العثور على متاجر متخصصة في الفئة الفرعية الثانية للطلب #{RequestId}", request.Id);
                    return;
                }

                _logger.LogInformation("✅ تم العثور على {Count} متجر متخصص في الفئة الفرعية الثانية: {SubCategory2Name}",
                    relevantStores.Count, fullRequest.SubCategory2?.Name);

                // ✅ بناء مسار الفئات الكامل
                var categoryPath = fullRequest.Category?.Name ?? "غير محدد";
                if (fullRequest.SubCategory1 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory1.Name}";
                }
                if (fullRequest.SubCategory2 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory2.Name}";
                }

                // ✅ بناء رابط الطلب الكامل
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var requestLink = $"/Requests/Details/{fullRequest.Id}";
                var fullRequestUrl = $"{baseUrl}{requestLink}";

                // ✅ تحضير معلومات الصور
                var hasImages = fullRequest.Images != null && fullRequest.Images.Any();
                var imagesCount = hasImages ? fullRequest.Images.Count : 0;

                var successCount = 0;
                var failureCount = 0;

                // إرسال إشعار لكل متجر
                foreach (var store in relevantStores)
                {
                    try
                    {
                        // ✅ بناء عنوان الإشعار
                        var title = "🛒 طلب جديد في تخصصك!";

                        // ✅ بناء رسالة مفصلة للإشعار داخل التطبيق
                        var inAppMessage = $"طلب جديد متاح في فئتك المتخصصة:\n\n" +
                                          $"📋 العنوان: {fullRequest.Title}\n" +
                                          $"📂 الفئة: {categoryPath}\n" +
                                          $"📍 الموقع: {fullRequest.City} - {fullRequest.District}\n" +
                                          $"👤 المشتري: {fullRequest.User?.FirstName} {fullRequest.User?.LastName}\n\n" +
                                          $"اضغط للاطلاع على التفاصيل وتقديم عرضك!";

                        // ✅ بناء رسالة مفصلة للواتساب والإيميل
                        var detailedMessage = $"🔔 مرحباً {store.StoreName ?? store.FirstName}!\n\n" +
                                             $"تم نشر طلب جديد يتوافق مع تخصص متجرك:\n\n" +
                                             $"━━━━━━━━━━━━━━━━━━━━\n" +
                                             $"📋 العنوان: {fullRequest.Title}\n" +
                                             $"━━━━━━━━━━━━━━━━━━━━\n\n" +
                                             $"📂 الفئة: {categoryPath}\n\n" +
                                             $"📝 التفاصيل:\n{fullRequest.Description}\n\n" +
                                             $"📍 الموقع: {fullRequest.City} - {fullRequest.District}";

                        if (!string.IsNullOrEmpty(fullRequest.Location))
                        {
                            detailedMessage += $" ({fullRequest.Location})";
                        }

                        detailedMessage += $"\n\n👤 معلومات المشتري:\n" +
                                          $"   • الاسم: {fullRequest.User?.FirstName} {fullRequest.User?.LastName}\n" +
                                          $"   • الهاتف: {fullRequest.User?.PhoneNumber}\n";

                        if (hasImages)
                        {
                            detailedMessage += $"\n📸 الصور: {imagesCount} صورة مرفقة\n";
                        }

                        detailedMessage += $"\n📅 تاريخ الطلب: {fullRequest.CreatedAt:yyyy-MM-dd HH:mm}\n\n" +
                                          $"🔗 لمشاهدة الطلب الكامل والتفاصيل:\n{fullRequestUrl}\n\n" +
                                          $"━━━━━━━━━━━━━━━━━━━━\n" +
                                          $"بادر بالتواصل مع المشتري وتقديم عرضك!\n" +
                                          $"━━━━━━━━━━━━━━━━━━━━\n\n" +
                                          $"🛒 السوق العكسي - نوصلك بالعملاء";

                        // ✅ إنشاء الإشعار داخل التطبيق أولاً
                        var notification = await _notificationService.CreateNotificationAsync(
                            title: title,
                            message: inAppMessage,
                            type: NotificationType.NewRequestForStore,
                            userId: store.Id,
                            requestId: fullRequest.Id,
                            link: requestLink,
                            isFromAdmin: true,
                            adminId: User.Identity?.Name
                        );

                        // ✅ إرسال الإشعار داخل التطبيق
                        await _notificationService.SendNotificationAsync(notification,
                            sendEmail: false,
                            sendWhatsApp: false,
                            sendInApp: true);

                        // ✅ إرسال رسالة مفصلة عبر الواتساب
                        if (!string.IsNullOrEmpty(store.PhoneNumber))
                        {
                            var whatsAppRequest = new WhatsAppMessageRequest
                            {
                                recipient = store.PhoneNumber,
                                message = detailedMessage,
                                type = "whatsapp",
                                lang = "ar",
                                sender_id = "AliJamal"
                            };

                            var whatsAppResult = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                            if (whatsAppResult.Success)
                            {
                                _logger.LogInformation("✅ تم إرسال واتساب للمتجر {StoreName} - {Phone}",
                                    store.StoreName ?? store.FirstName, store.PhoneNumber);
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ فشل إرسال واتساب للمتجر {StoreName}: {Error}",
                                    store.StoreName ?? store.FirstName, whatsAppResult.Message);
                            }
                        }

                        // ✅ إرسال إيميل مفصل (إذا كان متوفراً)
                        if (!string.IsNullOrEmpty(store.Email))
                        {
                            // تحديث رسالة الإشعار للإيميل
                            notification.Message = detailedMessage;
                            await _notificationService.SendNotificationAsync(notification,
                                sendEmail: true,
                                sendWhatsApp: false,
                                sendInApp: false);
                        }

                        successCount++;
                        _logger.LogInformation("✅ تم إرسال إشعار للمتجر {StoreName} ({StoreId}) - الهاتف: {Phone}",
                            store.StoreName ?? store.FirstName, store.Id, store.PhoneNumber);

                        // تأخير قصير بين الإشعارات لتجنب Rate Limiting
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, "❌ خطأ في إرسال إشعار للمتجر {StoreName} ({StoreId})",
                            store.StoreName ?? store.FirstName, store.Id);
                    }
                }

                _logger.LogInformation("✅ تم الانتهاء من إرسال إشعارات المتاجر للطلب #{RequestId} - نجح: {Success}، فشل: {Failed}",
                    request.Id, successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ عام في إرسال إشعارات المتاجر للطلب #{RequestId}", request.Id);
            }
        }

        /// <summary>
        /// ✅ البحث عن المتاجر المتخصصة بناءً على الفئة الفرعية الثانية فقط
        /// </summary>
        private async Task<List<ApplicationUser>> GetRelevantStoresBySubCategory2Async(int subCategory2Id)
        {
            try
            {
                // ✅ البحث عن المتاجر التي لديها نفس SubCategory2Id بالضبط
                var storeUserIds = await _dbContext.StoreCategories
                    .Include(sc => sc.User)
                    .Where(sc =>
                        sc.SubCategory2Id == subCategory2Id &&
                        sc.User.UserType == UserType.Seller &&
                        sc.User.IsActive &&
                        sc.User.IsStoreApproved &&
                        !string.IsNullOrEmpty(sc.User.PhoneNumber))
                    .Select(sc => sc.UserId)
                    .Distinct()
                    .ToListAsync();

                if (!storeUserIds.Any())
                {
                    _logger.LogWarning("⚠️ لا توجد متاجر متخصصة في الفئة الفرعية الثانية: {SubCategory2Id}", subCategory2Id);
                    return new List<ApplicationUser>();
                }

                var stores = await _dbContext.Users
                    .Where(u => storeUserIds.Contains(u.Id))
                    .ToListAsync();

                _logger.LogInformation("🔍 تم العثور على {Count} متجر متخصص في الفئة الفرعية الثانية: {SubCategory2Id}",
                    stores.Count, subCategory2Id);

                return stores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في البحث عن المتاجر المتخصصة للفئة الفرعية الثانية: {SubCategory2Id}", subCategory2Id);
                return new List<ApplicationUser>();
            }
        }

        /// <summary>
        /// البحث عن المتاجر المتخصصة بناءً على فئة الطلب (للتوافقية مع الكود القديم)
        /// </summary>
        private async Task<List<ApplicationUser>> GetRelevantStoresAsync(Request request)
        {
            // إذا كان هناك SubCategory2Id، استخدم البحث المحدد
            if (request.SubCategory2Id.HasValue)
            {
                return await GetRelevantStoresBySubCategory2Async(request.SubCategory2Id.Value);
            }

            // للتوافقية: البحث بناءً على SubCategory1 أو Category
            try
            {
                IQueryable<StoreCategory> relevantStoresQuery = _dbContext.StoreCategories
                    .Include(sc => sc.User)
                    .Where(sc =>
                        sc.User.UserType == UserType.Seller &&
                        sc.User.IsActive &&
                        sc.User.IsStoreApproved &&
                        !string.IsNullOrEmpty(sc.User.PhoneNumber));

                if (request.SubCategory1Id.HasValue)
                {
                    var subCategory2Ids = await _dbContext.SubCategories2
                        .Where(sc2 => sc2.SubCategory1Id == request.SubCategory1Id)
                        .Select(sc2 => sc2.Id)
                        .ToListAsync();

                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.SubCategory1Id == request.SubCategory1Id ||
                        (sc.SubCategory2Id.HasValue && subCategory2Ids.Contains(sc.SubCategory2Id.Value)));
                }
                else
                {
                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.CategoryId == request.CategoryId);
                }

                var storeUserIds = await relevantStoresQuery
                    .Select(sc => sc.UserId)
                    .Distinct()
                    .ToListAsync();

                var stores = await _dbContext.Users
                    .Where(u => storeUserIds.Contains(u.Id))
                    .ToListAsync();

                return stores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في البحث عن المتاجر المتخصصة للطلب #{RequestId}", request.Id);
                return new List<ApplicationUser>();
            }
        }

        #endregion

        #region الدوال القديمة (للمرجعية)

        // ✅ إرسال إشعار للمستخدم بالموافقة على الطلب (دالة قديمة للمرجعية)
        private async Task NotifyUserAboutApprovalAsync(Request request)
        {
            try
            {
                if (request.User != null && !string.IsNullOrEmpty(request.User.PhoneNumber))
                {
                    var requestUrl = $"{Request.Scheme}://{Request.Host}/Requests/Details/{request.Id}";

                    var messageText = $"مرحبا {request.User.FirstName}!\n\n" +
                                     $"تم الموافقة على طلبك: {request.Title}\n\n" +
                                     $"سيتم اشعار المتاجر المتخصصة وستبدا بتلقي العروض قريبا.\n\n" +
                                     $"🔗 لمشاهدة طلبك:\n{requestUrl}\n\n" +
                                     $"شكرا لاستخدامك السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = request.User.PhoneNumber,
                        message = messageText,
                        type = "whatsapp",
                        lang = "ar"
                    };

                    var result = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                    if (result.Success)
                    {
                        _logger.LogInformation("✅ تم إرسال إشعار الموافقة بنجاح إلى {PhoneNumber}",
                            request.User.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الموافقة");
            }
        }

        // ✅ إرسال إشعار للمستخدم برفض الطلب (دالة قديمة للمرجعية)
        private async Task NotifyUserAboutRejectionAsync(Request request)
        {
            try
            {
                if (request.User != null && !string.IsNullOrEmpty(request.User.PhoneNumber))
                {
                    var messageText = $"مرحبا {request.User.FirstName}!\n\n" +
                                     $"ناسف لابلاغك بان طلبك: {request.Title}\n" +
                                     $"لم تتم الموافقة عليه.\n\n";

                    if (!string.IsNullOrEmpty(request.AdminNotes))
                    {
                        messageText += $"السبب: {request.AdminNotes}\n\n";
                    }

                    messageText += "يمكنك اضافة طلب جديد في اي وقت.\n\n" +
                              "شكرا لتفهمك - السوق العكسي";

                    var whatsAppRequest = new WhatsAppMessageRequest
                    {
                        recipient = request.User.PhoneNumber,
                        message = messageText,
                        type = "whatsapp",
                        lang = "ar"
                    };

                    await _whatsAppService.SendMessageAsync(whatsAppRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الرفض");
            }
        }

        #endregion
    }
}