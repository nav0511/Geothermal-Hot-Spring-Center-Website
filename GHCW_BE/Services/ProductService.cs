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
        private readonly CloudinaryService _cloudinary;

        public ProductService(GHCWContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinary = cloudinaryService;
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
            return await _cloudinary.UploadImageResult(r);
        }
    }
}
