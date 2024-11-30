using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;

namespace GHCW_FE.Pages.Services
{
    public class IndexModel : PageModel
    {
        private ServicesService _servicesService = new ServicesService();

        public List<ServiceDTO> ServiceDTOs { get; set; } = new List<ServiceDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var(statusCode, TotalNewsCount) = _servicesService.GetTotalServices().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var(statusCode2, serviceDTOs) = await _servicesService.GetServices($"Service?$top={PageSize}&$skip={skip}&$filter=IsActive eq true");
            ServiceDTOs = serviceDTOs;
        }
    }
}
