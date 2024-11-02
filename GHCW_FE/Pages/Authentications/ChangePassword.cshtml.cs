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

        [BindProperty]
        public ChangePassRequest changePassRequest { get; set; }

        public ChangePasswordModel(HttpClient httpClient, AccountService accService)
        {
            _httpClient = httpClient;
            _accService = accService;
        }

        public void OnGet()
        {
            var userAccountJson = HttpContext.Session.GetString("acc");
            if (userAccountJson != null)
            {
                var userAccount = JsonConvert.DeserializeObject<AccountDTO>(userAccountJson);

                // Gán UserId vào changePassRequest.Id
                changePassRequest = new ChangePassRequest
                {
                    Id = userAccount.Id
                };
            }
            else
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đổi mật khẩu.";
                RedirectToPage("/Authentications/Login");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại";
                return Page();
            }
            var statusCode = await _accService.ChangePassword(changePassRequest);
            if (statusCode == HttpStatusCode.OK)
            {
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
    }
}
