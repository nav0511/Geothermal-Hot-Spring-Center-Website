using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace GHCW_FE.Pages.Authentications
{
    public class LoginModel : PageModel
    {
        private readonly AuthenticationService _authService;

        public LoginModel(AuthenticationService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginDTO Account { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ, vui lòng thử lại.";
                return Page();
            }

            if (await _authService.AuthenticateUserAsync(Account.Email, Account.Password))
            {
                var userAccount = await _authService.GetUserInfoAsync();
                if (userAccount != null)
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
                        2 => Redirect("../Index"),
                        3 => Redirect("../Index"),
                        4 => Redirect("../Index"),
                        5 => Redirect("../Index")
                    };
                }
            }

            TempData["ErrorMessage"] = "Đăng nhập thất bại, vui lòng kiểm tra lại email hoặc mật khẩu.";
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _authService.LogoutAsync();
            return RedirectToPage("/Authentications/Login");
        }
    }
}
