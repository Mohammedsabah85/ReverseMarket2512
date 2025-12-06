using SixLabors.ImageSharp.Formats.Jpeg;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ReverseMarket.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<FileService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (!IsValidImageFile(file))
                    throw new ArgumentException("Invalid image file");

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Resize image if it's too large
                await ResizeImageAsync(filePath, 1200, 1200);

                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image file");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image: {ImagePath}", imagePath);
                return false;
            }
        }

        public async Task<string> ResizeImageAsync(string imagePath, int maxWidth, int maxHeight)
        {
            try
            {
                using var image = await Image.LoadAsync(imagePath);

                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));

                    await image.SaveAsync(imagePath, new JpegEncoder { Quality = 85 });
                }

                return imagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resize image: {ImagePath}", imagePath);
                return imagePath;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = _configuration.GetSection("FileUploadSettings:AllowedImageExtensions")
                .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var maxSizeInMB = _configuration.GetValue<int>("FileUploadSettings:MaxFileSizeInMB", 5);
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension) && file.Length <= maxSizeInBytes;
        }
    }
}
