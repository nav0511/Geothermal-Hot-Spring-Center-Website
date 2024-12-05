using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class DiscountService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<DiscountDTO>? Discounts)> GetDiscounts(string url)
        {
            return await GetData<List<DiscountDTO>>(url);
        }

        public async Task<(HttpStatusCode StatusCode, int TotalDiscounts)> GetTotalDiscounts()
        {
            string url = "Discount/Total";
            return await GetData<int>(url);
        }

        public async Task<(HttpStatusCode StatusCode, List<DiscountDTO>? Discount)> GetUnsedDiscounts(string? currentDiscountId)
        {
            string url = $"Discount/AvailableForNews?id={currentDiscountId}";
            return await GetData<List<DiscountDTO>>(url);
        }

        public async Task<(HttpStatusCode StatusCode, DiscountDTO? Discount)> GetDiscountByCode(string code)
        {
            string url = $"Discount/{code}";
            return await GetData<DiscountDTO>(url);
        }

        public async Task<HttpStatusCode> UpdateDiscount(DiscountDTO discount, string accessToken)
        {
            string url = $"Discount/{discount.Code}";
            return await PutData(url, discount, null, accessToken);
        }

        public async Task<HttpStatusCode> DeleteDiscount(string code)
        {
            string url = $"Discount/{code}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateDiscount(DiscountDTO discount, string accessToken)
        {
            string url = "Discount";
            return await PushData(url, discount, null, accessToken);
        }

        public async Task<HttpStatusCode> DiscountActivation(string accessToken, string code)
        {
            var statusCode = await DeleteData($"Discount/DiscountActivation/{code}", accessToken);
            return statusCode;
        }
    }
}
