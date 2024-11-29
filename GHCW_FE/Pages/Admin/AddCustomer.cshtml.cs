using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddCustomerModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly CustomerService _customerService;

        public AddCustomerModel(AccountService accountService, TokenService tokenService, AuthenticationService authService, CustomerService customerService)
        {
            _accService = accountService;
            _tokenService = tokenService;
            _authService = authService;
            _customerService = customerService;
        }

        [BindProperty]
        public AddCustomerRequest AddRequest { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng thử lại.";
                return Page();
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);

                var (statusCode1, account) = await _accService.GetUserByEmail(accessToken, AddRequest.Email);

                if (statusCode1 == HttpStatusCode.OK && account != null)
                {
                    AddRequest.AccountId = account.Id;
                }
                else if (statusCode1 != HttpStatusCode.NotFound)
                {
                    AddRequest.AccountId = null;
                }

                var statusCode = await _customerService.AddCustomer(accessToken, AddRequest);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công.";
                    return Page();
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thêm khách hàng mới.";
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
