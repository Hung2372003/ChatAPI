namespace FakeFacebook.Service
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file , string? folderPosition);
        Task<(string Url, string PublicId)> UploadVideoAsync(IFormFile file, string? folderPosition);
        Task DeleteFileAsync(string publicId);
    }
}
