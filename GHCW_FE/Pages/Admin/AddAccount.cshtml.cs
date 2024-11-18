using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddAccountModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;

        public AddAccountModel(AccountService accountService, TokenService tokenService, AuthenticationService authService)
        {
            _accService = accountService;
            _tokenService = tokenService;
            _authService = authService;
        }

        [BindProperty]
        public AddRequest AddRequest { get; set; }


        public async Task<IActionResult> OnGet()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) > 0)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng thử lại.";
                return Page();
            }
            try
            {

                var statusCode = await _accService.AddUser(accessToken, AddRequest);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Thêm người dùng thành công.";
                    return Page();
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thêm người dùng.";
                    return Page();
                }
                else if (statusCode == HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "Email đã tồn tại, vui lòng sử dụng email khác để đăng ký.";
                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = "Thêm người dùng mới thất bại, vui lòng thử lại.";
                    return Page();
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
