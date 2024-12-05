using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Booking
{
    public class IndexModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;

        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();
        public Dictionary<int, int> SelectedServices { get; set; } = new Dictionary<int, int>();

        [BindProperty]
        public DateTime BookingDate { get; set; }

        public IndexModel(ServicesService servicesService, TokenService tokenService, AuthenticationService authService)
        {
            _servicesService = servicesService;
            _tokenService = tokenService;
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) != 5)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }

            (HttpStatusCode StatusCode, List<ServiceDTO>? ListServices) = await _servicesService.GetServices($"Service?$filter=IsActive eq true");
            AvailableServices = ListServices;
            return Page();
        }

    }
}
