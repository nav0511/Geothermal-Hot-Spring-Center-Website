using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly AccountService _accService;

        public ForgotPasswordModel(HttpClient httpClient, AccountService accService)
        {
            _httpClient = httpClient;
            _accService = accService;
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
                TempData["ErrorMessage"] = "Gửi email thất bại. Vui lòng thử lại.";
                return Page();
            }

            var statusCode = await _accService.ForgotPassword(Email);

            if (statusCode == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Mật khẩu mới đã được gửi đến email của bạn.";
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
