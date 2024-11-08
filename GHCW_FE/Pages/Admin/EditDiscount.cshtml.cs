using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EditDiscountModel : PageModel
    {
        private DiscountService _discountService = new DiscountService();

        public DiscountDTO Discount { get; set; }

        public async Task<IActionResult> OnGetAsync(string code)
        {
            var (statusCode, discount) = await _discountService.GetDiscountByCode(code);
            Discount = discount;
            if (Discount == null)
            {
                ModelState.AddModelError(string.Empty, "Phiếu giảm giá không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string code)
        {
            var (statusCode, discount) = await _discountService.GetDiscountByCode(code);
            Discount = discount;
            if (Discount == null)
            {
                ModelState.AddModelError(string.Empty, "Phiếu giảm giá không tồn tại.");
                return NotFound();
            }

            Discount.Code = Request.Form["code"];
            Discount.Name = Request.Form["name"];
            Discount.Value = Convert.ToInt32(Request.Form["value"]);
            Discount.StartDate = Convert.ToDateTime(Request.Form["startDate"]);
            Discount.EndDate = Convert.ToDateTime(Request.Form["endDate"]);
            Discount.Description = Request.Form["description"];
            Discount.IsAvailable = Request.Form["isAvailable"] == "on";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var discountDto = new DiscountDTO
            {
                Code = Discount.Code,
                Name = Discount.Name,
                Value = Discount.Value,
                StartDate = Discount.StartDate,
                EndDate = Discount.EndDate,
                Description = Discount.Description,
                IsAvailable = Discount.IsAvailable
            };

            statusCode = await _discountService.UpdateDiscount(discountDto);

            if (statusCode == HttpStatusCode.NoContent)
            {
                return RedirectToPage("/Admin/DiscountManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật phiếu giảm giá.");
                return Page();
            }
        }
    }
}
