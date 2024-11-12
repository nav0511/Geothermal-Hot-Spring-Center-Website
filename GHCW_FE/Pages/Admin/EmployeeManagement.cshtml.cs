using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EmployeeManagementModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;

        [BindProperty]
        public List<AccountDTO>? Employees { get; set; }

        public EmployeeManagementModel(AccountService accountService, TokenService tokenService, AuthenticationService authService)
        {
            _accService = accountService;
            _tokenService = tokenService;
            _authService = authService;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, employees) = await _accService.ListEmployee("Authentication/employeelist",accessToken);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.Unauthorized)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách thông tin người dùng.";
                return Page();
            }
            Employees = employees;

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;
            if (employees != null)
            {
                var total = employees.Count();
                TotalPages = (int)Math.Ceiling((double)total / PageSize);
                (statusCode, employees) = await _accService.ListEmployee($"Authentication/employeelist?$top={PageSize}&$skip={skip}", accessToken);
                if (statusCode == HttpStatusCode.Forbidden)
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                    return RedirectToPage("/Authentications/Login");
                }
                else if (statusCode == HttpStatusCode.Unauthorized)
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn.";
                    return RedirectToPage("/Authentications/Login");
                }
                else if (statusCode != HttpStatusCode.OK)
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách thông tin người dùng.";
                    return Page();
                }
                Employees = employees;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAccountActivation(int uid)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _accService.AccountActivation(accessToken, uid);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đổi trạng thái thất bại, vui lòng thử lại sau.";
                await OnGetAsync();
                return Page();
            }
            else
            {
                TempData["SuccessMessage"] = "Đổi trạng thái thành công.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}
