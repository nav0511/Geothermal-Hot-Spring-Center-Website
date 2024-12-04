using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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


        public async Task<(bool isSuccess, string message)> UpdateProduct(Product product)
        {
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật Sản phẩm mới thành công.");

            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình cập nhật Sản phẩm, vui lòng thử lại.");
            }
        }

        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool isSuccess, string message)> AddProduct(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return (true, "Thêm Sản phẩm mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm Sản phẩm, vui lòng thử lại.");
            }

        }

        public async Task<(bool isSuccess, string message)> ProductActivation(int pid)
        {
            var product = await _context.Products.FindAsync(pid);
            if (product == null)
            {
                return (false, "Sản phẩm không tồn tại.");
            }
            try
            {
                product.IsAvailable = !product.IsAvailable;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái sản phẩm thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái sản phẩm thất bại, vui lòng thử lại.");
            }
        }

        public async Task<string> UploadImageResult(IFormFile r)
        {
            return await _cloudinary.UploadImageResult(r);
        }
    }
}
