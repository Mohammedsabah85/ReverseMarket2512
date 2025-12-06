using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ReverseMarket.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public static async Task<string?> SaveImageAsync(IFormFile file, string uploadPath, IWebHostEnvironment environment)
        {
            try
            {
                if (!IsValidImage(file))
                    return null;

                // إنشاء مجلد الرفع إذا لم يكن موجوداً
                var fullUploadPath = Path.Combine(environment.WebRootPath, "uploads", uploadPath);
                Directory.CreateDirectory(fullUploadPath);

                // إنشاء اسم ملف فريد
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                var filePath = Path.Combine(fullUploadPath, fileName);

                // حفظ الصورة مع تحسين الجودة
                using (var inputStream = file.OpenReadStream())
                using (var image = await Image.LoadAsync(inputStream))
                {
                    // تغيير حجم الصورة إذا كانت كبيرة
                    if (image.Width > 1200 || image.Height > 1200)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1200, 1200),
                            Mode = ResizeMode.Max
                        }));
                    }

                    // حفظ الصورة بجودة محسنة
                    await image.SaveAsync(filePath, new JpegEncoder
                    {
                        Quality = 85
                    });
                }

                return $"/uploads/{uploadPath}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في حفظ الصورة: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<string>> SaveMultipleImagesAsync(List<IFormFile> files, string uploadPath, IWebHostEnvironment environment, int maxCount = 3)
        {
            var savedPaths = new List<string>();

            foreach (var file in files.Take(maxCount))
            {
                var savedPath = await SaveImageAsync(file, uploadPath, environment);
                if (!string.IsNullOrEmpty(savedPath))
                {
                    savedPaths.Add(savedPath);
                }
            }

            return savedPaths;
        }

        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // التحقق من حجم الملف
            if (file.Length > MaxFileSize)
                return false;

            // التحقق من امتداد الملف
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;

            // التحقق من نوع المحتوى
            var allowedMimeTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public static bool DeleteImage(string imagePath, IWebHostEnvironment environment)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في حذف الصورة: {ex.Message}");
                return false;
            }
        }

        public static string GetImageUrl(string? imagePath, string defaultImage = "/images/default-image.jpg")
        {
            if (string.IsNullOrEmpty(imagePath))
                return defaultImage;

            return imagePath;
        }

        public static long GetMaxFileSize() => MaxFileSize;

        public static string[] GetAllowedExtensions() => AllowedExtensions;

        public static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 بايت";

            string[] sizes = { "بايت", "كيلوبايت", "ميجابايت", "جيجابايت" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        public static async Task<string?> CreateThumbnailAsync(string imagePath, IWebHostEnvironment environment, int width = 300, int height = 300)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return null;

                var fullImagePath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));

                if (!File.Exists(fullImagePath))
                    return null;

                var fileName = Path.GetFileNameWithoutExtension(fullImagePath);
                var extension = Path.GetExtension(fullImagePath);
                var directory = Path.GetDirectoryName(fullImagePath);

                var thumbnailFileName = $"{fileName}_thumb_{width}x{height}{extension}";
                var thumbnailPath = Path.Combine(directory, thumbnailFileName);

                using (var image = await Image.LoadAsync(fullImagePath))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Crop
                    }));

                    await image.SaveAsync(thumbnailPath, new JpegEncoder
                    {
                        Quality = 80
                    });
                }

                // إرجاع المسار النسبي للصورة المصغرة
                var relativePath = imagePath.Replace(Path.GetFileName(imagePath), thumbnailFileName);
                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في إنشاء الصورة المصغرة: {ex.Message}");
                return null;
            }
        }
    }
}