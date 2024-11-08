using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Admin
{
    public class PromotionManagementModel : PageModel
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

            var (statusCode, TotalNewsCount) = _newsService.GetTotalPromotionNews().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, newsDTOs) = await _newsService.GetNews($"News?$filter=DiscountId ne null&top={PageSize}&$skip={skip}");
            NewsDTOs = newsDTOs;
        }
    }
}
