using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Areas.Admin.Models;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdvertisementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdvertisementsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var advertisements = await _context.Advertisements
                .OrderBy(a => a.Type)
                .ThenBy(a => a.DisplayOrder)
                .ToListAsync();

            return View(advertisements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdvertisementViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = "";

                if (model.Image != null)
                {
                    imagePath = await SaveImageAsync(model.Image);
                }

                var advertisement = new Advertisement
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = imagePath,
                    LinkUrl = model.LinkUrl,
                    Type = model.Type,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };

                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // إضافة ميثود Edit GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }

            var model = new EditAdvertisementViewModel
            {
                Id = advertisement.Id,
                Title = advertisement.Title,
                Description = advertisement.Description,
                CurrentImagePath = advertisement.ImagePath,
                LinkUrl = advertisement.LinkUrl,
                Type = advertisement.Type,
                DisplayOrder = advertisement.DisplayOrder,
                IsActive = advertisement.IsActive,
                StartDate = advertisement.StartDate,
                EndDate = advertisement.EndDate
            };

            return View(model);
        }

        // إضافة ميثود Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAdvertisementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var advertisement = await _context.Advertisements.FindAsync(model.Id);
                if (advertisement == null)
                {
                    return NotFound();
                }

                advertisement.Title = model.Title;
                advertisement.Description = model.Description;
                advertisement.LinkUrl = model.LinkUrl;
                advertisement.Type = model.Type;
                advertisement.DisplayOrder = model.DisplayOrder;
                advertisement.IsActive = model.IsActive;
                advertisement.StartDate = model.StartDate;
                advertisement.EndDate = model.EndDate;

                // معالجة الصورة الجديدة إذا تم رفعها
                if (model.NewImage != null)
                {
                    // حذف الصورة القديمة
                    if (!string.IsNullOrEmpty(advertisement.ImagePath))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, advertisement.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // حفظ الصورة الجديدة
                    advertisement.ImagePath = await SaveImageAsync(model.NewImage);
                }

                _context.Update(advertisement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث الإعلان بنجاح";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "advertisements");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return $"/uploads/advertisements/{fileName}";
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement != null)
            {
                // حذف الصورة من الخادم
                if (!string.IsNullOrEmpty(advertisement.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, advertisement.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }

    // إضافة ViewModel للتعديل
    public class EditAdvertisementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الإعلان مطلوب")]
        [StringLength(255, ErrorMessage = "عنوان الإعلان لا يجب أن يزيد عن 255 حرف")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "وصف الإعلان لا يجب أن يزيد عن 1000 حرف")]
        public string? Description { get; set; }

        public string? CurrentImagePath { get; set; }

        public IFormFile? NewImage { get; set; }

        [Url(ErrorMessage = "رابط الإعلان غير صحيح")]
        public string? LinkUrl { get; set; }

        [Required(ErrorMessage = "نوع الإعلان مطلوب")]
        public AdvertisementType Type { get; set; }

        [Range(0, 999, ErrorMessage = "ترتيب العرض يجب أن يكون بين 0 و 999")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}