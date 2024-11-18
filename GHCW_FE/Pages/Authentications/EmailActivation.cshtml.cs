using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class EmailActivationModel : PageModel
    {
        private readonly AuthenticationService _authService;

        public EmailActivationModel(AuthenticationService authenticationService)
        {
            _authService = authenticationService;
        }

        public async Task<IActionResult> OnGetAsync(string email, string code)
        {
            var activationRequest = new ActivationCode
            {
                Email = email,
                Code = code
            };

            var statusCode = await _authService.AccountActivation(activationRequest);

            if (statusCode == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Kích hoạt tài khoản thành công, vui lòng đăng nhập.";
                return Page();
                
            }
            else
            {
                TempData["ErrorMessage"] = "Kích hoạt tài khoản thất bại, vui lòng thử lại.";
                return Page();
            }
        }
    }
}
