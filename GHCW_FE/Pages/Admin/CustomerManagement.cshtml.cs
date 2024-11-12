using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class CustomerManagementModel : PageModel
    {
        private readonly CustomerService _cusService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        [BindProperty]
        public List<CustomerDTO>? Customers { get; set; }
        public CustomerManagementModel(CustomerService cusService, TokenService tokenService, AuthenticationService authService)
        {
            _cusService = cusService;
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
            _cusService.SetAccessToken(accessToken);

            var (statusCode, customers) = await _cusService.ListCustomer("Authentication/CustomerList", accessToken);
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
            Customers = customers;

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;
            if (customers != null)
            {
                var total = customers.Count();
                TotalPages = (int)Math.Ceiling((double)total / PageSize);
                (statusCode, customers) = await _cusService.ListCustomer($"Authentication/CustomerList?$top={PageSize}&$skip={skip}", accessToken);
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
                Customers = customers;
            }
            return Page();
        }
    }
}
