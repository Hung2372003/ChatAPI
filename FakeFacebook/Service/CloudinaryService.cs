using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Security.Cryptography;

namespace FakeFacebook.Service
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        // Hàm tính SHA1 hash
        private static string ComputeSHA1(IFormFile file)
        {
            using var sha1 = SHA1.Create();
            using var stream = file.OpenReadStream();
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file,string? folder)
        {
            var publicId = "images/" + ComputeSHA1(file);
            try
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = publicId,
                    Overwrite = false // Không ghi đè
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return (uploadResult.SecureUrl?.ToString() ?? "", uploadResult.PublicId);
            }
            catch (Exception)
            {
                // Nếu trùng file => upload fail -> lấy file đã có sẵn
                var existing = await _cloudinary.GetResourceAsync(publicId);
                return (existing.SecureUrl ?? "", existing.PublicId);
            }
        }

        public async Task<(string Url, string PublicId)> UploadVideoAsync(IFormFile file,string? folder)
        {
            var publicId = "videos/" + ComputeSHA1(file);
            try
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = publicId,
                    Overwrite = false,
                    Folder = folder
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return (uploadResult.SecureUrl?.ToString() ?? "", uploadResult.PublicId);
            }
            catch (Exception)
            {
                var existing = await _cloudinary.GetResourceAsync(publicId);
                return (existing.SecureUrl ?? "", existing.PublicId);
            }
        }

        public async Task DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return;
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
