using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class NewsManagementModel : PageModel
    {
        private NewsService _newsService = new NewsService();

        public List<NewsDTO> NewsDTOs { get; set; } = new List<NewsDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 7;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            int totalNewsCount = _newsService.GetTotalRegularNews().Result;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            NewsDTOs = await _newsService.GetNews($"News?$filter=DiscountId eq null&top={PageSize}&$skip={skip}");
        }

        public async Task<IActionResult> OnPostDeleteNews(int id)
        {
            var responseStatus = await _newsService.DeleteNews(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {

                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa tin tức.");
                return Page();
            }
        }
    }
}
