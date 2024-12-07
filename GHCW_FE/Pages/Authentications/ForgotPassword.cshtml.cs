using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationService _authService;

        public ForgotPasswordModel(HttpClient httpClient, AuthenticationService authenticationService)
        {
            _httpClient = httpClient;
            _authService = authenticationService;
        }

        [BindProperty]
        public string Email { get; set; }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                TempData["ErrorMessage"] = "Email không được để trống.";
                return Page();
            }

            var statusCode = await _authService.ForgotPassword(Email);

            if (statusCode == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Mật khẩu mới đã được gửi đến email của bạn.";
                return Page();
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Không tìm thấy email, vui lòng thử lại.";
                return Page();
            }
            else
            {
                TempData["ErrorMessage"] = "Gửi email thất bại. Vui lòng thử lại.";
                return Page();
            }
        }

    }
}
