using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
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
            var(statusCode, service) = await _servicesService.GetServiceById(id);
            Service = service;
            if (Service == null)
            {
                return NotFound();
            }
            var(statusCode2, serviceDTOs) = await _servicesService.GetServices($"Service?$top=5&$filter=Id ne {id} and IsActive eq true");
            ServiceDTOs = serviceDTOs;
            return Page();
        }
    }
}
