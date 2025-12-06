// Controllers/DiagnosticController.cs - للمساعدة في تشخيص مشاكل الصور

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarket.Data;
using ReverseMarket.Models;
using ReverseMarket.Models.Identity;
namespace ReverseMarket.Controllers
{
    public class DiagnosticController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public DiagnosticController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _dbContext = context;
            _environment = environment;
        }

        // GET: /Diagnostic/Images
        public async Task<IActionResult> Images()
        {
            var diagnosticInfo = new ImageDiagnosticInfo();

            // فحص قاعدة البيانات
            try
            {
                diagnosticInfo.TotalImagesInDb = await _dbContext.RequestImages.CountAsync();
                diagnosticInfo.RequestsWithImages = await _dbContext.Requests
                    .Include(r => r.Images)
                    .Where(r => r.Images.Any())
                    .CountAsync();

                var sampleImages = await _dbContext.RequestImages
                    .Take(10)
                    .ToListAsync();

                diagnosticInfo.SampleImages = sampleImages;
            }
            catch (Exception ex)
            {
                diagnosticInfo.DatabaseError = ex.Message;
            }

            // فحص مجلدات الرفع
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                var requestsPath = Path.Combine(uploadsPath, "requests");

                diagnosticInfo.UploadsDirectoryExists = Directory.Exists(uploadsPath);
                diagnosticInfo.RequestsDirectoryExists = Directory.Exists(requestsPath);

                if (diagnosticInfo.RequestsDirectoryExists)
                {
                    var files = Directory.GetFiles(requestsPath);
                    diagnosticInfo.PhysicalFilesCount = files.Length;
                    diagnosticInfo.SamplePhysicalFiles = files.Take(10).Select(Path.GetFileName).ToList();
                }
            }
            catch (Exception ex)
            {
                diagnosticInfo.FileSystemError = ex.Message;
            }

            // فحص الأذونات
            try
            {
                var testPath = Path.Combine(_environment.WebRootPath, "uploads", "test.txt");
                await System.IO.File.WriteAllTextAsync(testPath, "test");
                System.IO.File.Delete(testPath);
                diagnosticInfo.HasWritePermissions = true;
            }
            catch
            {
                diagnosticInfo.HasWritePermissions = false;
            }

            return View(diagnosticInfo);
        }

        // GET: /Diagnostic/TestUpload
        public IActionResult TestUpload()
        {
            return View();
        }

        // POST: /Diagnostic/TestUpload
        [HttpPost]
        public async Task<IActionResult> TestUpload(IFormFile testImage)
        {
            var result = new UploadTestResult();

            if (testImage == null || testImage.Length == 0)
            {
                result.Error = "لم يتم اختيار ملف";
                return View(result);
            }

            try
            {
                // اختبار حفظ الملف
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "test");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"test_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(testImage.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await testImage.CopyToAsync(stream);
                }

                result.Success = true;
                result.FileName = fileName;
                result.FileSize = testImage.Length;
                result.FilePath = $"/uploads/test/{fileName}";
                result.FileExists = System.IO.File.Exists(filePath);

                // اختبار الوصول عبر URL
                result.WebAccessible = await TestWebAccessAsync(result.FilePath);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return View(result);
        }

        private async Task<bool> TestWebAccessAsync(string relativePath)
        {
            try
            {
                using var httpClient = new HttpClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fullUrl = $"{baseUrl}{relativePath}";

                var response = await httpClient.GetAsync(fullUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // GET: /Diagnostic/FixIssues
        public async Task<IActionResult> FixIssues()
        {
            var results = new List<string>();

            try
            {
                // إنشاء المجلدات المطلوبة
                var directories = new[]
                {
                    "uploads",
                    "uploads/requests",
                    "uploads/profiles",
                    "uploads/advertisements",
                    "uploads/site",
                    "uploads/test"
                };

                foreach (var dir in directories)
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, dir);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        results.Add($"تم إنشاء المجلد: {dir}");
                    }
                    else
                    {
                        results.Add($"المجلد موجود: {dir}");
                    }
                }

                // فحص الصور المفقودة
                var imagesWithMissingFiles = await _dbContext.RequestImages
                    .Where(img => !string.IsNullOrEmpty(img.ImagePath))
                    .ToListAsync();

                int missingCount = 0;
                foreach (var image in imagesWithMissingFiles)
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
                    if (!System.IO.File.Exists(fullPath))
                    {
                        missingCount++;
                    }
                }

                results.Add($"عدد الصور المفقودة: {missingCount}");

                ViewBag.Results = results;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }
    }

    // Models للتشخيص
    public class ImageDiagnosticInfo
    {
        public int TotalImagesInDb { get; set; }
        public int RequestsWithImages { get; set; }
        public bool UploadsDirectoryExists { get; set; }
        public bool RequestsDirectoryExists { get; set; }
        public int PhysicalFilesCount { get; set; }
        public bool HasWritePermissions { get; set; }
        public List<RequestImage> SampleImages { get; set; } = new();
        public List<string> SamplePhysicalFiles { get; set; } = new();
        public string? DatabaseError { get; set; }
        public string? FileSystemError { get; set; }
    }

    public class UploadTestResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? FilePath { get; set; }
        public bool FileExists { get; set; }
        public bool WebAccessible { get; set; }
        public string? Error { get; set; }
    }
}