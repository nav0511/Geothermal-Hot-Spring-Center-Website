using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddPromotionModel : PageModel
    {
        private NewsService _newsService = new NewsService();
        private DiscountService _discountService = new DiscountService();

        public List<DiscountDTO> Discounts { get; set; } = new List<DiscountDTO>();

        public async Task OnGetAsync()
        {
            Discounts = await _discountService.GetDiscounts("Discount");
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var promotion = new NewsDTO
            {
                Title = Request.Form["title"],
                DiscountId = Request.Form["discountId"],
                UploadDate = Convert.ToDateTime(Request.Form["uploadDate"]),
                IsActive = Request.Form["isActive"] == "on",
                Description = Request.Form["description"],
                Image = "/images/" + Request.Form["image"].ToString(),
            };


            var response = await _newsService.CreateNews(promotion);

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/PromotionManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm dịch vụ.");
                return Page();
            }
        }
    }
}
