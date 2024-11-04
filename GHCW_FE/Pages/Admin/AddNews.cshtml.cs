using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddNewsModel : PageModel
    {
        private NewsService _newsService = new NewsService();
        private DiscountService _discountService = new DiscountService();

        public List<DiscountDTO> Discounts { get; set; } = new List<DiscountDTO>();

        public async Task OnGetAsync()
        {
            var (statusCode, discounts) = await _discountService.GetDiscounts("Discount");
            Discounts = discounts;
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var promotion = new NewsDTOForAdd
            {
                Title = Request.Form["title"],
                DiscountId = Request.Form["discountId"],
                UploadDate = DateTime.Now,
                IsActive = Request.Form["isActive"] == "on",
                Description = Request.Form["description"],
                Image = Request.Form.Files["image"],
            };


            var response = await _newsService.CreateNews(promotion, "multipart/form-data");

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm tin tức.");
                return Page();
            }
        }
    }
}
