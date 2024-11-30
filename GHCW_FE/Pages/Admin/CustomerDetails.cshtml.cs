using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class CustomerDetailsModel : PageModel
    {
        private readonly AccountService _accService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly CustomerService _customerService;

        public CustomerDetailsModel(AccountService accountService, TokenService tokenService, AuthenticationService authService, CustomerService customerService)
        {
            _accService = accountService;
            _tokenService = tokenService;
            _authService = authService;
            _customerService = customerService;
        }

        [BindProperty]
        public CustomerDTO EditRequest { get; set; }

        public CustomerDTO? CustomerProfile { get; set; }

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

            var (statusCode, customer) = await _customerService.GetCustomerById(accessToken, id);

            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Khách hàng không tồn tại.";
                return RedirectToPage("/Admin/CustomerManagement");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin tài khoản.";
                return Page();
            }
            CustomerProfile = customer;
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

                var (statusCode1, account) = await _accService.GetUserByEmail(accessToken, EditRequest.Email);

                if (statusCode1 == HttpStatusCode.OK && account != null)
                {
                    EditRequest.AccountId = account.Id;
                }
                else if (statusCode1 != HttpStatusCode.NotFound)
                {
                    EditRequest.AccountId = null;
                }

                var statusCode = await _customerService.EditCustomer(accessToken, EditRequest);
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
