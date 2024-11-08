using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Booking
{
    public class IndexModel : PageModel
    {
        private readonly ServicesService _servicesService;

        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();
        public Dictionary<int, int> SelectedServices { get; set; } = new Dictionary<int, int>();

        [BindProperty]
        public DateTime BookingDate { get; set; }

        public IndexModel(ServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            AvailableServices = await _servicesService.GetServices($"Service");
        }

        //public IActionResult OnPostAddToCart(int serviceId, int quantity)
        //{
        //    // Logic to add service to cart
        //}

        public IActionResult OnPostProceedToPayment()
        {
            // Redirect to payment page with cart details
            return RedirectToPage("Payment");
        }
    }
}
