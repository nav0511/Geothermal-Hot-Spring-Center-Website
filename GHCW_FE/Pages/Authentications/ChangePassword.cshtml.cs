using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class ChangePasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;

        [BindProperty]
        public ChangePassRequest changePassRequest { get; set; }

        public ChangePasswordModel(HttpClient httpClient, AccountService accService, TokenService tokenService, AuthenticationService authService)
        {
            _httpClient = httpClient;
            _accService = accService;
            _tokenService = tokenService;
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem hồ sơ.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);
            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập hồ sơ này.";
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
            changePassRequest = new ChangePassRequest
            {
                Id = userProfile.Id
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại";
                await OnGetAsync();
                return Page();
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);
                var statusCode = await _accService.ChangePassword(changePassRequest, accessToken);
                if (statusCode == HttpStatusCode.OK)
                {
                    await _authService.LogoutAsync();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công, vui lòng đăng nhập lại.";
                    return RedirectToPage("/Authentications/Login");
                }
                else if (statusCode == HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "Mật khẩu cũ không chính xác, vui lòng nhập lại";
                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = "Đổi mật khẩu thất bại, vui lòng thử lại";
                    return Page();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }
        }
    }
}
