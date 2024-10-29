using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ServiceManagementModel : PageModel
    {
        private ServicesService _servicesService = new ServicesService();

        public List<ServiceDTO> ServiceDTOs { get; set; } = new List<ServiceDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            int totalNewsCount = _servicesService.GetTotalServices().Result;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            ServiceDTOs = await _servicesService.GetServices($"Service?$top={PageSize}&$skip={skip}");
        }

        public async Task<IActionResult> OnPostDeleteService(int id)
        {
            var responseStatus = await _servicesService.DeleteService(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {
                
                return RedirectToPage(); 
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa dịch vụ.");
                return Page(); 
            }
        }
    }
}
