using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private NewsService _newsService = new NewsService();
        private ServicesService _servicesService = new ServicesService();

        public List<NewsDTO> NewsDtos { get; set; } = new List<NewsDTO>();
        public List<ServiceDTO> ServiceDtos { get; set; } = new List<ServiceDTO>();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            var (statusCode, newsDTOs) = _newsService.GetNews("News?$orderby=UploadDate desc&$top=3&$filter=DiscountId eq null and IsActive eq true").Result;
            NewsDtos = newsDTOs;
            var (statusCode2, serviceDTOs) = _servicesService.GetServices("Service?$top=3").Result;
            ServiceDtos = serviceDTOs;
        }
    }
}
