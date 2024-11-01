using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Services
{
    public class DetailModel : PageModel
    {
        private readonly ServicesService _servicesService = new ServicesService();

        public ServiceDTO Service { get; set; }
        public List<ServiceDTO> ServiceDTOs { get; set; } = new List<ServiceDTO>();

        public async Task<IActionResult> OnGet(int id)
        {
            Service = await _servicesService.GetServiceById(id);
            if (Service == null)
            {
                return NotFound();
            }
            ServiceDTOs = await _servicesService.GetServices($"Service?$top=5&$filter=Id ne {id}");

            return Page();
        }
    }
}
