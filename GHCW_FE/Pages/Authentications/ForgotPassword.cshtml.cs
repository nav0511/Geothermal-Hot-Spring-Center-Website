using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Authentications
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly string _changePassApiUrl;
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            var baseUrlAPI = _configuration.GetValue<string>("API:Url");
            _changePassApiUrl = $"{baseUrlAPI}api/Authentication/ForgotPassword"; ;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            var requestBody = new { email };

            var response = await _httpClient.PostAsJsonAsync(_changePassApiUrl, requestBody);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Gửi thành công, vui lòng kiểm tra email để lấy tài khoản mới của bạn!";
                return RedirectToPage("Login"); // Redirect to the Login page after a successful email send
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }

    }
}
