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

        //public async Task<Discount> GetDiscountByCode(string code)
        //{
        //    return await _context.Discounts.FirstOrDefaultAsync(n => n.Code == code);
        //}

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

        public async Task<(bool isSuccess, string message)> DiscountActivation(string code)
        {
            var discount = GetDiscount(code);
            if (discount == null)
            {
                return (false, "Phiếu giảm giá không tồn tại.");
            }
            try
            {
                discount.IsAvailable = !discount.IsAvailable;
                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái phiếu giảm giá thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái phiếu giảm giá thất bại, vui lòng thử lại.");
            }
        }
    }
}
