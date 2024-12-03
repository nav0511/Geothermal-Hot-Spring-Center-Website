using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ServiceManagementModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public ServiceManagementModel(TokenService tokenService, AuthenticationService authService, ServicesService servicesService, AccountService accService)
        {
            _authService = authService;
            _servicesService = servicesService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<ServiceDTO> ServiceDTOs { get; set; } = new List<ServiceDTO>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, string? searchTerm = null, int sortOption = 0)
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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 1)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 1)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            SearchTerm = searchTerm;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, services) = await _servicesService.GetServices("Service");
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                services = services?.Where(d => d.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            services = SortOption switch
            {
                1 => services.OrderBy(d => d.Name).ToList(),
                2 => services.OrderByDescending(d => d.Name).ToList(),
                3 => services.OrderBy(d => d.Price).ToList(),
                4 => services.OrderByDescending(d => d.Price).ToList(),
                _ => services.ToList(),
            };

            var totalServices = services?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalServices / PageSize);
            ServiceDTOs = services?.Skip(skip).Take(PageSize).ToList() ?? new List<ServiceDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostServiceActivation(int nId)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _servicesService.ServiceActivation(accessToken, nId);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đổi trạng thái thất bại, vui lòng thử lại sau.";
                await OnGetAsync();
                return Page();
            }
            else
            {
                TempData["SuccessMessage"] = "Đổi trạng thái thành công.";
                await OnGetAsync();
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteService(int id)
        {
            var responseStatus = await _servicesService.DeleteService(id);
            if (responseStatus == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Xóa dịch vụ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa dịch vụ.";
            }
            return RedirectToPage();

        }
    }
}
