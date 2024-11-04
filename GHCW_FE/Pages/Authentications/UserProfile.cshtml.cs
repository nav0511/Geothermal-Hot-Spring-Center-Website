using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace GHCW_FE.Pages.Authentications
{
    public class UserProfileModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;

        public UserProfileModel(AccountService accService, TokenService tokenService)
        {
            _accService = accService;
            _tokenService = tokenService;
        }

        [BindProperty]
        public UpdateRequest UpdateRequest { get; set; } = new UpdateRequest();
        [BindProperty]
        public AccountDTO UserProfile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("accessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem hồ sơ.";
                return RedirectToPage("/Authentications/Login");
            }
            UserProfile = await _accService.UserProfile(accessToken);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UpdateRequest == null)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại";
                return await OnGetAsync();
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                _accService.SetAccessToken(accessToken);
                var statusCode = await _accService.UpdateProfile(UpdateRequest, accessToken);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
                    return await OnGetAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Cập nhật thông tin thất bại, vui lòng thử lại.";
                    return await OnGetAsync();
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }
        }
    }
}
