using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;

namespace ReverseMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories1)
                .ThenInclude(sc => sc.SubCategories2)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new Category
                    {
                        Name = model.Name,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.Now
                    };

                    if (model.Image != null)
                    {
                        var imagePath = await SaveCategoryImageAsync(model.Image);
                        if (imagePath != null)
                        {
                            category.ImagePath = imagePath;
                        }
                    }

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إضافة الفئة بنجاح";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطأ في إنشاء الفئة: {ex.Message}");
                    TempData["ErrorMessage"] = "حدث خطأ أثناء إضافة الفئة";
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new EditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CurrentImagePath = category.ImagePath,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories.FindAsync(model.Id);
                    if (category == null)
                    {
                        return NotFound();
                    }

                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.IsActive = model.IsActive;

                    if (model.RemoveImage && !string.IsNullOrEmpty(category.ImagePath))
                    {
                        DeleteCategoryImage(category.ImagePath);
                        category.ImagePath = null;
                    }
                    else if (model.NewImage != null)
                    {
                        if (!string.IsNullOrEmpty(category.ImagePath))
                        {
                            DeleteCategoryImage(category.ImagePath);
                        }

                        var imagePath = await SaveCategoryImageAsync(model.NewImage);
                        if (imagePath != null)
                        {
                            category.ImagePath = imagePath;
                        }
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم تحديث الفئة بنجاح";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"خطأ في تحديث الفئة: {ex.Message}");
                    TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث الفئة";
                }
            }

            return View(model);
        }

        // إصلاح حذف الفئة الأساسية
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories1)
                    .ThenInclude(sc => sc.SubCategories2)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category != null)
                {
                    // حذف الصورة المرتبطة
                    if (!string.IsNullOrEmpty(category.ImagePath))
                    {
                        DeleteCategoryImage(category.ImagePath);
                    }

                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم حذف الفئة بنجاح";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في حذف الفئة: {ex.Message}");
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الفئة";
            }

            return RedirectToAction("Index");
        }

        #region Private Methods for Image Handling

        private async Task<string?> SaveCategoryImageAsync(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return null;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return null;
                }

                if (image.Length > 2 * 1024 * 1024)
                {
                    return null;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "categories");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"/uploads/categories/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في حفظ صورة الفئة: {ex.Message}");
                return null;
            }
        }

        private void DeleteCategoryImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في حذف صورة الفئة: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateSubCategory1(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Category = category;
            var model = new SubCategory1 { CategoryId = categoryId };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSubCategory2(int subCategory1Id)
        {
            var subCategory1 = await _context.SubCategories1
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == subCategory1Id);

            if (subCategory1 == null)
            {
                return NotFound();
            }

            ViewBag.SubCategory1 = subCategory1;
            var model = new SubCategory2 { SubCategory1Id = subCategory1Id };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditSubCategory1(int id)
        {
            var subCategory1 = await _context.SubCategories1
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (subCategory1 == null)
            {
                return NotFound();
            }

            ViewBag.Category = subCategory1.Category;
            return View(subCategory1);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubCategory1(SubCategory1 subCategory)
        {
            //if (ModelState.IsValid)
            //{
                _context.Update(subCategory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث الفئة الفرعية الأولى بنجاح";
                return RedirectToAction("Index");
            //}

            var category = await _context.Categories.FindAsync(subCategory.CategoryId);
            ViewBag.Category = category;
            return View(subCategory);
        }

        [HttpGet]
        public async Task<IActionResult> EditSubCategory2(int id)
        {
            var subCategory2 = await _context.SubCategories2
                .Include(sc => sc.SubCategory1)
                .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (subCategory2 == null)
            {
                return NotFound();
            }

            ViewBag.SubCategory1 = subCategory2.SubCategory1;
            return View(subCategory2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubCategory2(SubCategory2 subCategory)
        {
            //if (ModelState.IsValid)
            //{
                _context.Update(subCategory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث الفئة الفرعية الثانية بنجاح";
                return RedirectToAction("Index");
            //}

            var subCategory1 = await _context.SubCategories1
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == subCategory.SubCategory1Id);

            ViewBag.SubCategory1 = subCategory1;
            return View(subCategory);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubCategory1(int id)
        {
            var subCategory1 = await _context.SubCategories1.FindAsync(id);
            if (subCategory1 != null)
            {
                _context.SubCategories1.Remove(subCategory1);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الفئة الفرعية الأولى بنجاح";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubCategory2(int id)
        {
            var subCategory2 = await _context.SubCategories2.FindAsync(id);
            if (subCategory2 != null)
            {
                _context.SubCategories2.Remove(subCategory2);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الفئة الفرعية الثانية بنجاح";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory1(SubCategory1 subCategory)
        {
            try
            {
                ModelState.Remove("Category");

                if (ModelState.IsValid)
                {
                    var parentCategory = await _context.Categories.FindAsync(subCategory.CategoryId);
                    if (parentCategory == null)
                    {
                        TempData["ErrorMessage"] = "الفئة الرئيسية غير موجودة";
                        return RedirectToAction("Index");
                    }

                    var existingSubCategory = await _context.SubCategories1
                        .FirstOrDefaultAsync(sc => sc.Name == subCategory.Name && sc.CategoryId == subCategory.CategoryId);

                    if (existingSubCategory != null)
                    {
                        TempData["ErrorMessage"] = "يوجد فئة فرعية بنفس الاسم في هذه الفئة";
                        ViewBag.Category = parentCategory;
                        return View(subCategory);
                    }

                    subCategory.CreatedAt = DateTime.Now;
                    subCategory.IsActive = true;

                    _context.SubCategories1.Add(subCategory);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إضافة الفئة الفرعية الأولى بنجاح";
                    return RedirectToAction("Index");
                }

                var category = await _context.Categories.FindAsync(subCategory.CategoryId);
                ViewBag.Category = category;

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                return View(subCategory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في إنشاء الفئة الفرعية: {ex.Message}");
                TempData["ErrorMessage"] = "حدث خطأ أثناء إضافة الفئة الفرعية";

                var category = await _context.Categories.FindAsync(subCategory.CategoryId);
                ViewBag.Category = category;
                return View(subCategory);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory2(SubCategory2 subCategory)
        {
            try
            {
                ModelState.Remove("SubCategory1");

                if (ModelState.IsValid)
                {
                    var parentSubCategory = await _context.SubCategories1
                        .Include(sc => sc.Category)
                        .FirstOrDefaultAsync(sc => sc.Id == subCategory.SubCategory1Id);

                    if (parentSubCategory == null)
                    {
                        TempData["ErrorMessage"] = "الفئة الفرعية الأولى غير موجودة";
                        return RedirectToAction("Index");
                    }

                    var existingSubCategory = await _context.SubCategories2
                        .FirstOrDefaultAsync(sc => sc.Name == subCategory.Name && sc.SubCategory1Id == subCategory.SubCategory1Id);

                    if (existingSubCategory != null)
                    {
                        TempData["ErrorMessage"] = "يوجد فئة فرعية ثانية بنفس الاسم في هذه الفئة";
                        ViewBag.SubCategory1 = parentSubCategory;
                        return View(subCategory);
                    }

                    subCategory.CreatedAt = DateTime.Now;
                    subCategory.IsActive = true;

                    _context.SubCategories2.Add(subCategory);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إضافة الفئة الفرعية الثانية بنجاح";
                    return RedirectToAction("Index");
                }

                var subCategory1 = await _context.SubCategories1
                    .Include(sc => sc.Category)
                    .FirstOrDefaultAsync(sc => sc.Id == subCategory.SubCategory1Id);

                ViewBag.SubCategory1 = subCategory1;

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                return View(subCategory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في إنشاء الفئة الفرعية الثانية: {ex.Message}");
                TempData["ErrorMessage"] = "حدث خطأ أثناء إضافة الفئة الفرعية الثانية";

                var subCategory1 = await _context.SubCategories1
                    .Include(sc => sc.Category)
                    .FirstOrDefaultAsync(sc => sc.Id == subCategory.SubCategory1Id);

                ViewBag.SubCategory1 = subCategory1;
                return View(subCategory);
            }
        }
        #endregion
    }
}