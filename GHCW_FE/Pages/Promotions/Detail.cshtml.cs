using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Promotions
{
    public class DetailModel : PageModel
    {
        private readonly NewsService _newsService = new NewsService();

        public NewsDTO News { get; set; }
        public List<NewsDTO> NewsDtos { get; set; } = new List<NewsDTO>();
        public string Message { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var (statusCode, news) = await _newsService.GetNewsById(id);
                News = news;
                if (News == null || News.DiscountId == null)
                {
                    throw new Exception();
                }

            }
            catch (Exception)
            {
                Message = "Khuyến mãi không có hoặc đã hết hiệu lực";
            }
            var(statusCode2, newsDtos) = _newsService.GetNews($"News?$orderby=UploadDate desc&$top=5&$filter=Id ne {id} and DiscountId ne null and IsActive eq true").Result;
            NewsDtos = newsDtos;

            return Page();
        }
    }
}
