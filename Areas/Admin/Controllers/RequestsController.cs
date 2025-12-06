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

                    // ✅ إرسال إشعار للمستخدم بالموافقة باستخدام النظام المحسن
                    await SendApprovalNotificationAsync(request);

                    // ✅ إرسال إشعار للمتاجر المتخصصة
                    await SendStoreNotificationsAsync(request);
                }
                else if (requestStatus == RequestStatus.Rejected)
                {
                    await _dbContext.SaveChangesAsync();
                    
                    // ✅ إرسال إشعار بالرفض باستخدام النظام المحسن
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

        // ✅ إرسال إشعار للمستخدم بالموافقة على الطلب
        private async Task NotifyUserAboutApprovalAsync(Request request)
        {
            try
            {
                if (request.User != null && !string.IsNullOrEmpty(request.User.PhoneNumber))
                {
                    // ✅ بناء رابط مباشر للطلب
                    var requestUrl = $"{Request.Scheme}://{Request.Host}/Requests/Details/{request.Id}";
                    
                    var messageText = $"مرحبا {request.User.FirstName}!\n\n" +
                                     $"تم الموافقة على طلبك: {request.Title}\n\n" +
                                     $"سيتم اشعار المتاجر المتخصصة وستبدا بتلقي العروض قريبا.\n\n" +
                                     $"🔗 لمشاهدة طلبك:\n{requestUrl}\n\n" +
                                     $"شكرا لاستخدامك السوق العكسي";

                    _logger.LogInformation("📤 إرسال رسالة موافقة للمشتري {Phone}:\n{Message}",
                        request.User.PhoneNumber, messageText);

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
                    else
                    {
                        _logger.LogError("❌ فشل إرسال إشعار الموافقة إلى {PhoneNumber}: {Error}",
                            request.User.PhoneNumber, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الموافقة");
            }
        }

        // ✅ إرسال إشعار للمستخدم برفض الطلب
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

                    _logger.LogInformation("📤 إرسال رسالة رفض للمشتري {Phone}:\n{Message}",
                        request.User.PhoneNumber, messageText);

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
                        _logger.LogInformation("✅ تم إرسال إشعار الرفض بنجاح إلى {PhoneNumber}",
                            request.User.PhoneNumber);
                    }
                    else
                    {
                        _logger.LogError("❌ فشل إرسال إشعار الرفض إلى {PhoneNumber}: {Error}",
                            request.User.PhoneNumber, result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرسال إشعار الرفض");
            }
        }

        // ✅✅✅ النسخة المصححة الكاملة - إرسال إشعار للمتاجر المتخصصة بعد موافقة الأدمن
        private async Task NotifyStoresAboutApprovedRequestAsync(Request request)
        {
            try
            {
                _logger.LogInformation("🔍 بدء عملية البحث عن المتاجر المتخصصة للطلب المعتمد #{RequestId}", request.Id);

                // ✅ جلب بيانات الطلب كاملة مع جميع العلاقات
                var fullRequest = await _dbContext.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (fullRequest == null)
                {
                    _logger.LogError("❌ لم يتم العثور على الطلب #{RequestId} في قاعدة البيانات", request.Id);
                    return;
                }

                _logger.LogInformation("✅ تم جلب بيانات الطلب: Category={CategoryId}, SubCat1={SubCat1Id}, SubCat2={SubCat2Id}",
                    fullRequest.CategoryId, fullRequest.SubCategory1Id, fullRequest.SubCategory2Id);

                // ✅ البحث عن المتاجر المتخصصة بنفس الفئة
                IQueryable<StoreCategory> relevantStoresQuery = _dbContext.StoreCategories
                    .Include(sc => sc.User)
                    .Where(sc =>
                        sc.User.UserType == UserType.Seller &&
                        sc.User.IsActive &&
                        sc.User.IsStoreApproved &&
                        !string.IsNullOrEmpty(sc.User.PhoneNumber));

                // ✅ تطبيق الفلتر الصحيح بناءً على مستوى الفئة
                if (fullRequest.SubCategory2Id.HasValue)
                {
                    // البحث عن المتاجر التي لديها نفس SubCategory2 بالضبط
                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.SubCategory2Id == fullRequest.SubCategory2Id);

                    _logger.LogInformation("🔍 البحث بناءً على SubCategory2: {SubCat2Id}", fullRequest.SubCategory2Id);
                }
                else if (fullRequest.SubCategory1Id.HasValue)
                {
                    // ✅ البحث عن المتاجر التي لديها نفس SubCategory1 
                    // أو أي SubCategory2 تنتمي لنفس SubCategory1
                    var subCategory2IdsUnderSubCategory1 = await _dbContext.SubCategories2
                        .Where(sc2 => sc2.SubCategory1Id == fullRequest.SubCategory1Id)
                        .Select(sc2 => sc2.Id)
                        .ToListAsync();

                    _logger.LogInformation("🔍 تم العثور على {Count} فئة فرعية ثانية تحت SubCategory1: {SubCat1Id}",
                        subCategory2IdsUnderSubCategory1.Count, fullRequest.SubCategory1Id);

                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.SubCategory1Id == fullRequest.SubCategory1Id ||
                        (sc.SubCategory2Id.HasValue && subCategory2IdsUnderSubCategory1.Contains(sc.SubCategory2Id.Value)));

                    _logger.LogInformation("🔍 البحث بناءً على SubCategory1: {SubCat1Id}", fullRequest.SubCategory1Id);
                }
                else
                {
                    // البحث عن المتاجر التي لديها نفس Category فقط
                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.CategoryId == fullRequest.CategoryId);

                    _logger.LogInformation("🔍 البحث بناءً على Category: {CategoryId}", fullRequest.CategoryId);
                }

                // ✅ الحصول على IDs المستخدمين بشكل فريد أولاً
                var storeUserIds = await relevantStoresQuery
                    .Select(sc => sc.UserId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("🔍 تم العثور على {Count} معرف مستخدم فريد", storeUserIds.Count);

                if (!storeUserIds.Any())
                {
                    _logger.LogWarning("⚠️ لا توجد متاجر متخصصة لهذا الطلب!");
                    _logger.LogWarning("⚠️ يرجى التحقق من أن المتاجر قد أضافت الفئات الصحيحة في ملفاتهم الشخصية");
                    return;
                }

                // ✅ جلب المستخدمين بناءً على الـ IDs
                var relevantStores = await _dbContext.Users
                    .Where(u => storeUserIds.Contains(u.Id))
                    .ToListAsync();

                _logger.LogInformation("✅ تم العثور على {Count} متجر متخصص للطلب المعتمد #{RequestId}",
                    relevantStores.Count, request.Id);

                // ✅ إعداد مسار الفئات للعرض
                var categoryPath = fullRequest.Category?.Name ?? "غير محدد";
                if (fullRequest.SubCategory1 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory1.Name}";
                }
                if (fullRequest.SubCategory2 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory2.Name}";
                }

                var successCount = 0;
                var failureCount = 0;

                // ✅ إرسال الإشعارات لكل متجر
                foreach (var store in relevantStores)
                {
                    try
                    {
                        // ✅ بناء رابط مباشر للطلب
                        var requestUrl = $"{Request.Scheme}://{Request.Host}/Requests/Details/{fullRequest.Id}";
                        
                        // ✅ بناء رسالة مفصلة وواضحة مع إيموجي للوضوح
                        var messageText = $"🔔 طلب جديد معتمد في تخصصك!\n\n" +
                                         $"مرحباً {store.StoreName ?? store.FirstName}!\n\n" +
                                         $"📌 عنوان الطلب: {fullRequest.Title}\n\n" +
                                         $"📂 الفئة: {categoryPath}\n\n" +
                                         $"📍 الموقع: {fullRequest.City} - {fullRequest.District}\n\n" +
                                         $"📝 التفاصيل:\n{fullRequest.Description}\n\n" +
                                         $"👤 المشتري: {fullRequest.User?.FirstName} {fullRequest.User?.LastName}\n" +
                                         $"📞 للتواصل: {fullRequest.User?.PhoneNumber}\n\n" +
                                         $"📅 تاريخ الطلب: {fullRequest.CreatedAt:yyyy-MM-dd}\n\n" +
                                         $"🔗 للمشاهدة الكاملة والتفاصيل:\n{requestUrl}\n\n" +
                                         $"🛒 السوق العكسي";

                        _logger.LogInformation("📤 إرسال رسالة للمتجر {StoreName} ({StoreId}) - {Phone}",
                            store.StoreName ?? store.FirstName, store.Id, store.PhoneNumber);

                        var whatsAppRequest = new WhatsAppMessageRequest
                        {
                            recipient = store.PhoneNumber,
                            message = messageText,
                            type = "whatsapp",
                            lang = "ar",
                            sender_id = "AliJamal"
                        };

                        var result = await _whatsAppService.SendMessageAsync(whatsAppRequest);

                        if (result.Success)
                        {
                            successCount++;
                            _logger.LogInformation("✅ تم إرسال إشعار بنجاح إلى المتجر {StoreName} - {PhoneNumber}",
                                store.StoreName ?? store.FirstName, store.PhoneNumber);
                        }
                        else
                        {
                            failureCount++;
                            _logger.LogError("❌ فشل إرسال إشعار إلى المتجر {StoreName} - {PhoneNumber}: {Error}",
                                store.StoreName ?? store.FirstName, store.PhoneNumber, result.Message ?? "غير معروف");
                        }

                        // تأخير قصير بين الرسائل لتجنب Rate Limiting
                        await Task.Delay(500);
                    }
                    catch (Exception storeEx)
                    {
                        failureCount++;
                        _logger.LogError(storeEx, "❌ خطأ في إرسال إشعار للمتجر {StoreName} ({StoreId})",
                            store.StoreName ?? store.FirstName, store.Id);
                    }
                }

                _logger.LogInformation("✅ تم الانتهاء من إرسال الإشعارات: {SuccessCount} نجحت، {FailureCount} فشلت",
                    successCount, failureCount);

                if (successCount == 0 && failureCount > 0)
                {
                    _logger.LogError("❌ فشل إرسال جميع الإشعارات! يرجى التحقق من إعدادات WhatsApp API");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ عام في إرسال إشعارات المتاجر للطلب #{RequestId}", request.Id);
            }
        }

        // ✅ دوال الإشعارات المحسنة باستخدام INotificationService

        /// <summary>
        /// إرسال إشعار موافقة للمشتري عبر جميع القنوات (نظام + إيميل + واتساب)
        /// </summary>
        private async Task SendApprovalNotificationAsync(Request request)
        {
            try
            {
                var title = "تم اعتماد طلبك! 🎉";
                var message = $"مرحباً {request.User?.FirstName}!\n\n" +
                             $"يسعدنا إبلاغك بأنه تم اعتماد طلبك: \"{request.Title}\"\n\n" +
                             $"سيتم الآن عرض طلبك للمتاجر المتخصصة وستتلقى عروضاً قريباً.\n\n" +
                             $"شكراً لاستخدامك السوق العكسي! 🛒";

                var requestLink = $"/Requests/Details/{request.Id}";

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
        /// إرسال إشعارات للمتاجر المتخصصة حول طلب جديد معتمد
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
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (fullRequest == null) return;

                // البحث عن المتاجر المتخصصة
                var relevantStores = await GetRelevantStoresAsync(fullRequest);

                if (!relevantStores.Any())
                {
                    _logger.LogWarning("⚠️ لم يتم العثور على متاجر متخصصة للطلب #{RequestId}", request.Id);
                    return;
                }

                var title = "طلب جديد متاح! 🛒";
                var message = $"طلب جديد في فئتك المتخصصة:\n\n" +
                             $"📋 العنوان: {fullRequest.Title}\n" +
                             $"📂 الفئة: {fullRequest.Category?.Name}\n" +
                             $"📍 المدينة: {fullRequest.City}\n\n" +
                             $"اضغط للاطلاع على التفاصيل وتقديم عرضك!";

                var requestLink = $"/Requests/Details/{fullRequest.Id}";

                // إرسال إشعار لكل متجر
                foreach (var store in relevantStores)
                {
                    try
                    {
                        var notification = await _notificationService.CreateNotificationAsync(
                            title: title,
                            message: message,
                            type: NotificationType.NewRequestForStore,
                            userId: store.Id,
                            requestId: fullRequest.Id,
                            link: requestLink,
                            isFromAdmin: true,
                            adminId: User.Identity?.Name
                        );

                        await _notificationService.SendNotificationAsync(notification, 
                            sendEmail: true, 
                            sendWhatsApp: true, 
                            sendInApp: true);

                        _logger.LogInformation("✅ تم إرسال إشعار للمتجر {StoreName} ({StoreId})", 
                            store.StoreName ?? store.FirstName, store.Id);

                        // تأخير قصير بين الإشعارات
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ خطأ في إرسال إشعار للمتجر {StoreId}", store.Id);
                    }
                }

                _logger.LogInformation("✅ تم الانتهاء من إرسال إشعارات المتاجر للطلب #{RequestId} - تم إرسال {Count} إشعار", 
                    request.Id, relevantStores.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ عام في إرسال إشعارات المتاجر للطلب #{RequestId}", request.Id);
            }
        }

        /// <summary>
        /// البحث عن المتاجر المتخصصة بناءً على فئة الطلب
        /// </summary>
        private async Task<List<ApplicationUser>> GetRelevantStoresAsync(Request request)
        {
            try
            {
                IQueryable<StoreCategory> relevantStoresQuery = _dbContext.StoreCategories
                    .Include(sc => sc.User)
                    .Where(sc =>
                        sc.User.UserType == UserType.Seller &&
                        sc.User.IsActive &&
                        sc.User.IsStoreApproved &&
                        !string.IsNullOrEmpty(sc.User.PhoneNumber));

                // تطبيق الفلتر بناءً على مستوى الفئة
                if (request.SubCategory2Id.HasValue)
                {
                    relevantStoresQuery = relevantStoresQuery.Where(sc =>
                        sc.SubCategory2Id == request.SubCategory2Id);
                }
                else if (request.SubCategory1Id.HasValue)
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

                // الحصول على المستخدمين بشكل فريد
                var storeUserIds = await relevantStoresQuery
                    .Select(sc => sc.UserId)
                    .Distinct()
                    .ToListAsync();

                var stores = await _dbContext.Users
                    .Where(u => storeUserIds.Contains(u.Id))
                    .ToListAsync();

                _logger.LogInformation("🔍 تم العثور على {Count} متجر متخصص للطلب #{RequestId}", 
                    stores.Count, request.Id);

                return stores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في البحث عن المتاجر المتخصصة للطلب #{RequestId}", request.Id);
                return new List<ApplicationUser>();
            }
        }
    }
}