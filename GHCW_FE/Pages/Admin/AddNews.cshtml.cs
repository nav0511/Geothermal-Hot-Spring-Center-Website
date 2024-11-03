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

            var news = new NewsDTOForAdd
            {
                Title = Request.Form["title"],
                UploadDate = DateTime.Now,
                IsActive = Request.Form["isActive"] == "on",
                Description = Request.Form["description"],
                Image = Request.Form.Files["image"]
            };


            var response = await _newsService.CreateNews(news, "multipart/form-data");

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
