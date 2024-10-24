﻿using GHCW_FE.DTOs;

namespace GHCW_FE.Services
{
    public class DiscountService : BaseService
    {
        public async Task<List<DiscountDTO>?> GetDiscounts(string url)
        {
            return await GetData<List<DiscountDTO>>(url);
        }

        public async Task<int> GetTotalDiscounts()
        {
            string url = "Discount/Total";
            return await GetData<int>(url);
        }
    }
}
