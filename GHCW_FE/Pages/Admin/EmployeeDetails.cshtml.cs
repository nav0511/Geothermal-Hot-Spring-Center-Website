using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EmployeeDetailsModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        public EmployeeDetailsModel(AccountService accountService, TokenService tokenService, AuthenticationService authService)
        {
            _accService = accountService;
            _tokenService = tokenService;
            _authService = authService;
        }
        [BindProperty]
        public EditRequest EditRequest { get; set; }

        public AccountDTO? UserProfile { get; set; }
        public AccountDTO? EmployeeProfile { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, userInfo) = await _accService.GetUserById(accessToken, id);

            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin tài khoản này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToPage("/Admin/UserManagement");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin tài khoản.";
                return Page();
            }
            EmployeeProfile = userInfo;

            var (statusCode2, userProfile) = await _accService.UserProfile(accessToken);
            if (statusCode2 == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập hồ sơ này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode2 == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode2 != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }
            UserProfile = userProfile;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại";
                return await OnGetAsync(EditRequest.Id);
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);

                var statusCode = await _accService.EditUserInfo(accessToken, EditRequest);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật thông tin.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Cập nhật thông tin thất bại, vui lòng thử lại.";
                    return await OnGetAsync(EditRequest.Id);
                }
            }
            catch (UnauthorizedAccessException)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }
        }
    }
}
