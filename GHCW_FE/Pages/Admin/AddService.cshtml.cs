using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddServiceModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public AddServiceModel(TokenService tokenService, AuthenticationService authService, ServicesService servicesService, AccountService accService)
        {
            _authService = authService;
            _servicesService = servicesService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public ServiceDTO Service { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) > 1)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }

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
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 0)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            _servicesService.SetAccessToken(accessToken);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var service = new ServiceDTO2
            {
                Name = Request.Form["name"],
                Price = Convert.ToDouble(Request.Form["price"]),
                Time = Request.Form["time"],
                Description = Request.Form["description"],
                Image = Request.Form.Files["image"],
                IsActive = Request.Form["isActive"] == "true"
            };

            
            var response = await _servicesService.CreateService(service, accessToken, "multipart/form-data");

            if (response == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Thêm dịch vụ thành công";
                return RedirectToPage("/Admin/ServiceManagement"); 
            }
            else
            {
                TempData["ErrorMessage"] =( "Có lỗi xảy ra khi thêm dịch vụ."); 
                return Page();
            }
        }
    }
}
