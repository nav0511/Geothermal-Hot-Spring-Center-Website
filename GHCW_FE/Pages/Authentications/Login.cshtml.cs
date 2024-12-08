using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace GHCW_FE.Pages.Authentications
{
    public class LoginModel : PageModel
    {
        private readonly AuthenticationService _authService;
        private readonly TokenService _tokenService;
        private readonly AccountService _accService;

        public LoginModel(AuthenticationService authService, TokenService tokenService, AccountService accService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _accService = accService;
        }

        [BindProperty]
        public LoginDTO Account { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/Index", new { reload = true });
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ, vui lòng thử lại.";
                return Page();
            }

            if (await _authService.AuthenticateUserAsync(Account))
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["ErrorMessage"] = "Không lấy được token đăng nhập.";
                    return RedirectToPage("/Authentications/Login");
                }
                _authService.SetAccessToken(accessToken);

                var (statusCode, userAccount) = await _accService.UserProfile(accessToken);
                if (statusCode == HttpStatusCode.OK && userAccount != null)
                {
                    // Lưu thông tin tài khoản vào session
                    HttpContext.Session.SetString("acc", System.Text.Json.JsonSerializer.Serialize(userAccount));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Điều hướng người dùng dựa trên loại tài khoản
                    return userAccount.Role switch
                    {
                        0 => RedirectToPage("/Admin/Dashboard"),
                        1 => RedirectToPage("/Admin/Dashboard"),
                        2 => RedirectToPage("/Admin/NewsManagement"),
                        3 => RedirectToPage("/Admin/TicketManagement"),
                        4 => RedirectToPage("/StaffBooking/BookTicket"),
                        5 => RedirectToPage("/Index"),
                        _ => RedirectToPage("/Index")
                    };
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin người dùng. Vui lòng thử lại.";
                    return RedirectToPage("/Authentications/Login");
                }
            }

            TempData["ErrorMessage"] = "Đăng nhập thất bại, vui lòng kiểm tra lại email hoặc mật khẩu.";
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["ErrorMessage"] = "Không lấy được token đăng nhập.";
                return RedirectToPage("/Authentications/Login");
            }
            _authService.SetAccessToken(accessToken);

            await _authService.LogoutAsync();
            return RedirectToPage("/Authentications/Login");
        }
    }
}
