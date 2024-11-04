using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class UserManagementModel : PageModel
    {
        private readonly AccountService _accService;
        [BindProperty]
        public AccountDTO Account { get; set; }
        public List<AccountDTO>? Accounts { get; set; }

        public UserManagementModel(AccountService accountService)
        {
            _accService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("accessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            var (statusCode, accounts) = await _accService.ListAccount(accessToken);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách thông tin người dùng.";
                return Page();
            }
            Accounts = accounts;
            return Page();
        }
    }
}
