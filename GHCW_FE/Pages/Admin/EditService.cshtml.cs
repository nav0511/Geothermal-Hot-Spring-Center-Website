using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EditServiceModel : PageModel
    {
        private ServicesService _servicesService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public EditServiceModel(TokenService tokenService, AuthenticationService authService, ServicesService servicesService, AccountService accService)
        {
            _authService = authService;
            _servicesService = servicesService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public ServiceDTO Service { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
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

            var (statusCode1, service) = await _servicesService.GetServiceById(id);
            Service = service;

            if (Service == null)
            {
                ModelState.AddModelError(string.Empty, "Dịch vụ không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID dịch vụ không hợp lệ.");
                return Page();
            }

            var (statusCode, service) = await _servicesService.GetServiceById(id);
            Service = service;
            if (Service == null)
            {
                ModelState.AddModelError(string.Empty, "Dịch vụ không tồn tại.");
                return NotFound();
            }

            Service.Name = Request.Form["name"];
            Service.Price = Convert.ToDouble(Request.Form["price"]);
            Service.Time = Request.Form["time"];
            Service.Description = Request.Form["description"];
            Service.Image = "/images/" + Request.Form["image"];
            Service.IsActive = Request.Form["isActive"] == "on";


            if (!ModelState.IsValid)
            {
                return Page();
            }

            var serviceDto = new ServiceDTO
            {
                Id = Service.Id,
                Name = Service.Name,
                Price = Service.Price,
                Time = Service.Time,
                Description = Service.Description,
                Image = Service.Image,
                IsActive = Service.IsActive,
            };

            statusCode = await _servicesService.UpdateService(serviceDto);

            if (statusCode == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/ServiceManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật dịch vụ.");
                return Page();
            }
        }


    }
}
