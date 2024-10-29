using GHCW_BE.Models;

namespace GHCW_BE.Services
{
    public class CategoryService
    {
        private readonly GHCWContext _context;

        public CategoryService(GHCWContext context)
        {
            _context = context;
        }

        public IQueryable<Category> GetListCategory()
        {
            return _context.Categories.AsQueryable();
        }
    }
}
