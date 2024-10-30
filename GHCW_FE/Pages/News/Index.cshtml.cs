using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GHCW_FE.Services;
using GHCW_FE.DTOs;

namespace GHCW_FE.Pages.News
{
    public class IndexModel : PageModel
    {
        private NewsService _newsService = new NewsService();

        public List<NewsDTO> NewsDtos { get; set; } = new List<NewsDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            int totalNewsCount = _newsService.GetTotalNews().Result;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            NewsDtos = _newsService.GetNews($"News?$orderby=UploadDate desc&$top={PageSize}&$skip={skip}").Result;
        }
    }
}
