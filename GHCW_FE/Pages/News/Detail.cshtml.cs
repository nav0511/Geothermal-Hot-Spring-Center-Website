using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.News
{
    public class DetailModel : PageModel
    {
        private readonly NewsService _newsService = new NewsService();

        public NewsDTO News { get; set; }
        public List<NewsDTO> NewsDtos { get; set; } = new List<NewsDTO>();
        public string Message {  get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var (statusCode2, news) = await _newsService.GetNewsById(id);
                News = news;
                if (News == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Message = "Không có tin tức";
            }
            var (statusCode, newsDTOs) = _newsService.GetNews($"News?$orderby=UploadDate desc&$top=5&$filter=Id ne {id} and DiscountId eq null").Result;
            NewsDtos = newsDTOs;
            return Page();
        }
    }
}
