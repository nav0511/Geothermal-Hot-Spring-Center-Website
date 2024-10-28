using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Promotions
{
    public class IndexModel : PageModel
    {
        private DiscountService _discountService = new DiscountService();

        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            int totalNewsCount = _discountService.GetTotalDiscounts().Result;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            DiscountDTOs = _discountService.GetDiscounts($"Discount?$top={PageSize}&$skip={skip}").Result;
        }
    }
}
