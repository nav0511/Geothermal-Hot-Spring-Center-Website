using GHCW_BE.Models;

namespace GHCW_BE.Services
{
    public class ProductService
    {
        private readonly GHCWContext _context;

        public ProductService(GHCWContext context)
        {
            _context = context;
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
            await _context.SaveChangesAsync();
        }
    }
}
