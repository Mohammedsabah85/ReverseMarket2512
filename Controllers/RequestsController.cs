using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.ViewModels;
using ReverseMarket.Services;
using Microsoft.AspNetCore.Identity;
using ReverseMarket.Models.Identity;
using ReverseMarket.CustomWhatsappService;

namespace ReverseMarket.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppService _whatsAppService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestsController> _logger;
        private readonly WhatsAppService _customWhatsAppService;

        public RequestsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppService whatsAppService,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestsController> logger,
            WhatsAppService customWhatsAppService)
            : base(context)
        {
            _webHostEnvironment = webHostEnvironment;
            _whatsAppService = whatsAppService;
            _userManager = userManager;
            _logger = logger;
            _customWhatsAppService = customWhatsAppService;
        }

        public async Task<IActionResult> Index(string search, int? categoryId, int? subCategory1Id, int? subCategory2Id, int page = 1)
        {
            var pageSize = 12;

            var query = _context.Requests
                .Where(r => r.Status == RequestStatus.Approved)
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .Include(r => r.User)
                .AsQueryable();

            // البحث النصي
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search));
            }

            // الفلترة حسب الفئات
            if (subCategory2Id.HasValue)
            {
                query = query.Where(r => r.SubCategory2Id == subCategory2Id.Value);
            }
            else if (subCategory1Id.HasValue)
            {
                query = query.Where(r => r.SubCategory1Id == subCategory1Id.Value);
            }
            else if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId.Value);
            }

            var totalRequests = await query.CountAsync();
            var requests = await query
                .OrderByDescending(r => r.ApprovedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // جلب الفئات الفرعية بناءً على الاختيارات
            var subCategories1 = new List<SubCategory1>();
            var subCategories2 = new List<SubCategory2>();

            if (categoryId.HasValue)
            {
                subCategories1 = await _context.SubCategories1
                    .Where(sc => sc.CategoryId == categoryId.Value && sc.IsActive)
                    .ToListAsync();
            }

            if (subCategory1Id.HasValue)
            {
                subCategories2 = await _context.SubCategories2
                    .Where(sc => sc.SubCategory1Id == subCategory1Id.Value && sc.IsActive)
                    .ToListAsync();
            }

            var model = new RequestsViewModel
            {
                Requests = requests,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                SubCategories1 = subCategories1,
                SubCategories2 = subCategories2,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalRequests / pageSize),
                Search = search,
                SelectedCategoryId = categoryId,
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id,
                Advertisements = await _context.Advertisements
                    .Where(a => a.IsActive &&
                           a.StartDate <= DateTime.Now &&
                           (a.EndDate == null || a.EndDate >= DateTime.Now))
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync(),
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.Requests
                .Include(r => r.Category)
                .Include(r => r.SubCategory1)
                .Include(r => r.SubCategory2)
                .Include(r => r.Images)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RequestStatus.Approved);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "جلسة المستخدم منتهية الصلاحية";
                    return RedirectToAction("Login", "Account");
                }

                try
                {
                    var request = new Request
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CategoryId = model.CategoryId,
                        SubCategory1Id = model.SubCategory1Id,
                        SubCategory2Id = model.SubCategory2Id,
                        City = model.City,
                        District = model.District,
                        Location = model.Location,
                        UserId = userId,
                        Status = RequestStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    _context.Requests.Add(request);
                    await _context.SaveChangesAsync();

                    // معالجة رفع الصور
                    if (model.Images != null && model.Images.Any())
                    {
                        await SaveRequestImagesAsync(request.Id, model.Images);
                    }

                    // إرسال إشعار للإدارة عن الطلب الجديد
                    await NotifyAdminAboutNewRequestAsync(request);

                    TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح! سيتم مراجعته والموافقة عليه في أقرب وقت.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في إنشاء الطلب");
                    TempData["ErrorMessage"] = "حدث خطأ أثناء إرسال الطلب. يرجى المحاولة مرة أخرى.";
                }
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        private async Task SaveRequestImagesAsync(int requestId, List<IFormFile> images)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "requests");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var maxFileSize = 5 * 1024 * 1024; // 5 MB

                foreach (var image in images.Take(3))
                {
                    if (image?.Length > 0)
                    {
                        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            continue;
                        }

                        if (image.Length > maxFileSize)
                        {
                            continue;
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var requestImage = new RequestImage
                        {
                            RequestId = requestId,
                            ImagePath = $"/uploads/requests/{fileName}",
                            CreatedAt = DateTime.Now
                        };

                        _context.RequestImages.Add(requestImage);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حفظ الصور");
            }
        }

        private async Task NotifyAdminAboutNewRequestAsync(Request request)
        {
            try
            {
                var adminPhone = "+9647805006974";

                var fullRequest = await _context.Requests
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Include(r => r.SubCategory1)
                    .Include(r => r.SubCategory2)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (fullRequest == null)
                {
                    _logger.LogError("❌ لم يتم العثور على الطلب #{RequestId}", request.Id);
                    return;
                }

                var categoryPath = fullRequest.Category?.Name ?? "غير محدد";
                if (fullRequest.SubCategory1 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory1.Name}";
                }
                if (fullRequest.SubCategory2 != null)
                {
                    categoryPath += $" > {fullRequest.SubCategory2.Name}";
                }

                var messageText = $"طلب جديد يحتاج مراجعة!\n\n" +
                                 $"المستخدم: {fullRequest.User?.FirstName} {fullRequest.User?.LastName}\n" +
                                 $"رقم الهاتف: {fullRequest.User?.PhoneNumber}\n\n" +
                                 $"عنوان الطلب: {fullRequest.Title}\n\n" +
                                 $"الفئة: {categoryPath}\n\n" +
                                 $"الموقع: {fullRequest.City} - {fullRequest.District}\n\n" +
                                 $"التفاصيل:\n{fullRequest.Description}\n\n" +
                                 $"التاريخ: {fullRequest.CreatedAt:yyyy-MM-dd HH:mm}\n\n" +
                                 $"يرجى مراجعة الطلب واعتماده\n\n" +
                                 $"السوق العكسي";

                _logger.LogInformation("📤 إرسال رسالة للأدمن:\n{Message}", messageText);

                var whatsAppRequest = new WhatsAppMessageRequest
                {
                    recipient = adminPhone,
                    message = messageText,
                    type = "whatsapp",
                    lang = "ar"
                };

                var result = await _customWhatsAppService.SendMessageAsync(whatsAppRequest);

                if (result.Success)
                {
                    _logger.LogInformation("✅ تم إرسال إشعار للإدارة عن طلب جديد #{RequestId}", request.Id);
                }
                else
                {
                    _logger.LogError("❌ فشل إرسال إشعار للإدارة: {Error}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ خطأ في إرسال إشعار للإدارة");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSubCategories1(int categoryId)
        {
            var subCategories = await _context.SubCategories1
                .Where(sc => sc.CategoryId == categoryId) // إزالة && sc.IsActive
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories2(int subCategory1Id)
        {
            var subCategories = await _context.SubCategories2
                .Where(sc => sc.SubCategory1Id == subCategory1Id) // إزالة && sc.IsActive
                .Select(sc => new { id = sc.Id, name = sc.Name })
                .ToListAsync();

            return Json(subCategories);
        }
        //[HttpGet]
        //public async Task<IActionResult> GetSubCategories1(int categoryId)
        //{
        //    var subCategories = await _context.SubCategories1
        //        .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
        //        .Select(sc => new { id = sc.Id, name = sc.Name })
        //        .ToListAsync();

        //    return Json(subCategories);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetSubCategories2(int subCategory1Id)
        //{
        //    var subCategories = await _context.SubCategories2
        //        .Where(sc => sc.SubCategory1Id == subCategory1Id && sc.IsActive)
        //        .Select(sc => new { id = sc.Id, name = sc.Name })
        //        .ToListAsync();

        //    return Json(subCategories);
        //}
    }
}

