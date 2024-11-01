using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class DiscountService
    {
        private readonly GHCWContext _context;

        public DiscountService(GHCWContext context)
        {
            _context = context;
        }

        public IQueryable<Discount> GetListDiscounts()
        {
            return _context.Discounts.AsQueryable();
        }

        public Discount GetDiscount(string code)
        {
            return _context.Discounts.FirstOrDefault(x => x.Code == code);
        }

        public async Task UpdateDiscount(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiscount(Discount discount)
        {
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
        }

        public async Task AddDiscount(Discount discount)
        {

            await _context.Discounts.AddAsync(discount);
            await _context.SaveChangesAsync();
        }

        //public async Task<string> GenerateUniqueDiscountCode()
        //{
        //    string code;
        //    var random = new Random();
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        //    do
        //    {
        //        code = new string(Enumerable.Repeat(chars, 6)
        //          .Select(s => s[random.Next(s.Length)]).ToArray());
        //    }
        //    while (await _context.Discounts.AnyAsync(d => d.Code == code));

        //    return code;
        //}
    }
}
