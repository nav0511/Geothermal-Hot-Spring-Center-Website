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

        public async Task<News> GetNewsById(int id)
        {
            return await _context.News.Include(n => n.Discount).FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News> GetNewsByDiscountCode(string code)
        {
            return await _context.News.Include(n => n.Discount).FirstOrDefaultAsync(n => n.DiscountId == code);
        }

        public async Task UpdateNews(News news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNews(News news)
        {
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
        }

        public async Task AddNews(News news)
        {

            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

    }
}
