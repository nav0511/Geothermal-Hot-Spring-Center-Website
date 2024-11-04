using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Admin
{
    public class UserManagementModel : PageModel
    {
        private readonly AccountService _accService;
        [BindProperty]
        public AccountDTO Account { get; set; }
        public List<AccountDTO> Accounts { get; set; }

        public UserManagementModel(AccountService accountService)
        {
            _accService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("accessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem hồ sơ.";
                return RedirectToPage("/Authentications/Login");
            }
            Accounts = await _accService.ListAccount(accessToken);
            return Page();
        }
    }
}
