﻿using GHCW_BE.Models;

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
    }
}
