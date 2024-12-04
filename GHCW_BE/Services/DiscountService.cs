﻿using GHCW_BE.Models;
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

        public async Task<(bool isSuccess, string message)> UpdateDiscount(Discount discount)
        {
            try
            {
                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật Mã giảm giá thành công.");

            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình cập nhật Mã giảm giá, vui lòng thử lại.");
            }
        }

        public async Task DeleteDiscount(Discount discount)
        {
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool isSuccess, string message)> AddDiscount(Discount discount)
        {
            try
            {
                await _context.Discounts.AddAsync(discount);
                await _context.SaveChangesAsync();
                return (true, "Thêm Mã giảm giá mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm Mã giảm giá, vui lòng thử lại.");
            }
            
        }

        public async Task<Discount?> CheckDiscountExsit(string code)
        {
            var checkDiscount = await _context.Discounts.FirstOrDefaultAsync(x => x.Code.Equals(code));
            return checkDiscount;
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
