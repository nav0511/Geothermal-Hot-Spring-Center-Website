using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Promotions
{
    public class DetailModel : PageModel
    {
        private readonly DiscountService _discountService = new DiscountService();
        private readonly NewsService _newsService = new NewsService();

        public DiscountDTO Discount { get; set; }
        public NewsDTO News { get; set; }
        public List<DiscountDTO> DiscountDtos { get; set; } = new List<DiscountDTO>();
        public string Message { get; set; }

        public async Task<IActionResult> OnGet(string code)
        {
            try
            {
                Discount = await _discountService.GetDiscountByCode(code);
                if (Discount == null)
                {
                    throw new Exception();
                }
                News = await _newsService.GetNewsByDiscountCode(code);

            }
            catch (Exception ex)
            {
                Message = "Khuyến mãi không có hoặc đã hết hiệu lực";
            }

            DiscountDtos = _discountService.GetDiscounts($"Discount?$top=5&$orderBy=StartDate desc&$filter=code ne '{code}'").Result;

            return Page();
        }
    }
}
