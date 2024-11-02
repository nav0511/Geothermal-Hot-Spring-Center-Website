using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace GHCW_FE.Pages.Admin
{
    public class EditNewsModel : PageModel
    {
        private NewsService _newsService = new NewsService();

        public NewsDTO News { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            News = await _newsService.GetNewsById(id);

            if (News == null)
            {
                ModelState.AddModelError(string.Empty, "Tin tức không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID tin tức không hợp lệ.");
                return Page();
            }

            News = await _newsService.GetNewsById(id);
            if (News == null)
            {
                ModelState.AddModelError(string.Empty, "Tin tức không tồn tại.");
                return NotFound();
            }

            News.Title = Request.Form["title"];
            News.UploadDate = Convert.ToDateTime(Request.Form["uploadDate"]);
            News.IsActive = Request.Form["isActive"] == "on";
            News.Description = Request.Form["description"];
            News.Image = "/images/" + Request.Form["image"].ToString();



            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newsDto = new NewsDTO
            {
                Id = News.Id,
                Title = News.Title,
                UploadDate= News.UploadDate,
                IsActive = News.IsActive,
                Description = News.Description,
                Image = News.Image,
            };

            var statusCode = await _newsService.UpdateNews(newsDto);

            if (statusCode == HttpStatusCode.NoContent)
            {
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật tin tức.");
                return Page();
            }
        }
    }
}
