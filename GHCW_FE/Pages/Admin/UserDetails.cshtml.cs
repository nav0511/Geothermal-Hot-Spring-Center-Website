using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Admin
{
    public class UserDetailsModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;

        public UserDetailsModel(AccountService accountService, TokenService tokenService)
        {
            _accService = accountService;
            _tokenService = tokenService;
        }

        [BindProperty]
        public EditRequest EditRequest { get; set; }

        [BindProperty]
        public AccountDTO UserProfile { get; set; }

        public void OnGet()
        {
        }
    }
}
