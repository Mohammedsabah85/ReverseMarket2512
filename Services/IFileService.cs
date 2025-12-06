namespace ReverseMarket.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string imagePath);
        Task<string> ResizeImageAsync(string imagePath, int maxWidth, int maxHeight);
        bool IsValidImageFile(IFormFile file);
    }
}