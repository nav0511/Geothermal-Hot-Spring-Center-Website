using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GHCW_BE.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySetting> config)
        {
            var account = new CloudinaryDotNet.Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageResult(IFormFile r)
        {
            if (r == null || r.Length == 0)
                throw new Exception("No file uploaded.");
            var uploadResult = new ImageUploadResult();
            string folderName = "TestPRM";

            if (r.Length > 0)
            {
                await using var stream = r.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(r.FileName, stream),
                    Folder = folderName
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString();
        }
    }
}
