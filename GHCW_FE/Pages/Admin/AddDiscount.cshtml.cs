using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddDiscountModel : PageModel
    {
        private DiscountService _discountService = new DiscountService();

        public DiscountDTO Discount { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var discount = new DiscountDTO
            {
                Name = Request.Form["name"],
                Value = Convert.ToInt32(Request.Form["value"]),
                StartDate = Convert.ToDateTime(Request.Form["startDate"]),
                EndDate = Convert.ToDateTime(Request.Form["endDate"]),
                Description = Request.Form["description"],
                IsAvailable  = Request.Form["isAvailable"] == "on"
            };


            var response = await _discountService.CreateDiscount(discount);

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/DiscountManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm dịch vụ.");
                return Page();
            }
        }
    }
}
