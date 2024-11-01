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

        public NewsDTO News { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var news = new NewsDTO
            {
                Title = Request.Form["title"],
                UploadDate = Convert.ToDateTime(Request.Form["uploadDate"]),
                IsActive = Request.Form["isActive"] == "on",
                Description = Request.Form["description"],
                Image = "/images/" + Request.Form["image"].ToString(),
            };


            var response = await _newsService.CreateNews(news);

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm dịch vụ.");
                return Page();
            }
        }
    }
}
