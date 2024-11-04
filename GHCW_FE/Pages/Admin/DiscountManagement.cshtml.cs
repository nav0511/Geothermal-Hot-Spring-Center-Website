using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class DiscountManagementModel : PageModel
    {
        private DiscountService _discountService = new DiscountService();

        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var(statusCode, TotalNewsCount) = _discountService.GetTotalDiscounts().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, discounts) = await _discountService.GetDiscounts($"Discount?$top={PageSize}&$skip={skip}");
            DiscountDTOs = discounts;
        }

        public async Task<IActionResult> OnPostDeleteDiscount(string id)
        {
            var responseStatus = await _discountService.DeleteDiscount(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {

                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa phiếu giảm giá.");
                return Page();
            }
        }
    }
}
