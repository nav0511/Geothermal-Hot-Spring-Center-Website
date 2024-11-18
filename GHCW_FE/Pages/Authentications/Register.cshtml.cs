using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class RegisterModel : PageModel
    {
        private readonly AuthenticationService _authService;

        public RegisterModel(AuthenticationService authenticationService)
        {
            _authService = authenticationService;
        }

        [BindProperty]
        public RegisterDTO Account { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng thử lại.";
                return Page();
            }
            var statusCode = await _authService.Register(Account);
            if (statusCode == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công, vui lòng kiểm tra email để kích hoạt tài khoản";
                return Page();
            }
            else if (statusCode == HttpStatusCode.Conflict)
            {
                TempData["ErrorMessage"] = "Email đã được sử dụng, vui lòng dùng email khác để đăng ký";
                return Page();
            }
            else
            {
                TempData["ErrorMessage"] = "Đăng ký thất bại, vui lòng thử lại.";
                return Page();
            }
        }
    }
}
