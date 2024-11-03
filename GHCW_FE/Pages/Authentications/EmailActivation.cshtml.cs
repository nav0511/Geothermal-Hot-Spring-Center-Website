using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class EmailActivationModel : PageModel
    {
        private readonly AccountService _accService;

        public EmailActivationModel(AccountService accountService)
        {
            _accService = accountService;
        }

        public async Task<IActionResult> OnGetAsync(string email, string code)
        {
            var activationRequest = new ActivationCode
            {
                Email = email,
                Code = code
            };

            // Call the API to activate the account
            var statusCode = await _accService.AccountActivation(activationRequest);

            // Check the response and set the appropriate message
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
