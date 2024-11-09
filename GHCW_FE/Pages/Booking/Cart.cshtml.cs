using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Booking
{
    public class CartModel : PageModel
    {
        private readonly ServicesService _servicesService;

        public CartModel(ServicesService servicesService)
        {
            _servicesService = servicesService;
        }
        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();

        public async Task OnGet()
        {
            (HttpStatusCode StatusCode, List<ServiceDTO>? ListServices) = await _servicesService.GetServices($"Service");
            AvailableServices = ListServices;
        }
    }
}
