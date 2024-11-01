using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GHCW_BE.Services
{
    public class ProductService
    {
        private readonly GHCWContext _context;
        private readonly Cloudinary _cloudinary;

        public ProductService(GHCWContext context, IOptions<CloudinarySetting> config)
        {
            _context = context;
            var account = new CloudinaryDotNet.Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public IQueryable<Product> GetListProducts()
        {
            return _context.Products.AsQueryable();
        }


        public async Task UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task AddProduct(Product product)
        {

            await _context.Products.AddAsync(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
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
