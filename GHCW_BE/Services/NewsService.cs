using GHCW_BE.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class NewsService
    {
        private readonly GHCWContext _context;

        public NewsService(GHCWContext context)
        {
            _context = context;
        }

        public IQueryable<News> GetListNews()
        {
            return _context.News.Include(n => n.Discount).AsQueryable();
        }

        

    }
}
