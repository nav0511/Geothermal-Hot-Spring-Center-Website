using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
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

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        [BindProperty(SupportsGet = true)]
        public int OrderOption { get; set; }
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) > 3)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            _cusService.SetAccessToken(accessToken);

            var (statusCode, customers) = await _cusService.ListCustomer("Customer/CustomerList", accessToken);
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
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                customers = customers?.Where(e =>
                    (e.FullName != null && e.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Email != null && e.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            switch (SortOption)
            {
                case 0:
                    customers = customers?.ToList();
                    break;
                case 1:
                    customers = customers?.Where(e => e.AccountId != null).ToList(); // Đã đăng ký tài khoản
                    break;
                case 2:
                    customers = customers?.Where(e => e.AccountId == null).ToList(); // Chưa đăng ký tài khoản
                    break;
                case 3:
                    customers = customers?.Where(e => e.Gender == true).ToList(); // Lọc Nam
                    break;
                case 4:
                    customers = customers?.Where(e => !e.Gender == true).ToList(); // Lọc Nữ
                    break;
                case 5:
                    customers = customers?.Where(e => e.IsEmailNotify == true).ToList(); // Lọc Nam
                    break;
                case 6:
                    customers = customers?.Where(e => !e.IsEmailNotify == true).ToList(); // Lọc Nữ
                    break;
            }

            switch (OrderOption)
            {
                case 1:
                    customers = customers?.OrderBy(e => e.Id).ToList(); // Sắp xếp ID tăng dần
                    break;
                case 2:
                    customers = customers?.OrderByDescending(e => e.Id).ToList(); // Sắp xếp ID giảm dần
                    break;
                case 3:
                    customers = customers?.OrderBy(e => e.FullName).ToList(); // Sắp xếp tên A-Z
                    break;
                case 4:
                    customers = customers?.OrderByDescending(e => e.FullName).ToList(); // Sắp xếp tên Z-A
                    break;
            }

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;
            TotalPages = (int)Math.Ceiling((double)customers.Count() / PageSize);

            Customers = customers.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();
            return Page();
        }
    }
}
