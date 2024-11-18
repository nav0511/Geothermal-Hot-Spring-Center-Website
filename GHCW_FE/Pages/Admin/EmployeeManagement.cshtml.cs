using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
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
            _accService.SetAccessToken(accessToken);

            var (statusCode, employees) = await _accService.ListEmployee("Account/employeelist", accessToken);
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
                employees = employees?.Where(e =>
                    (e.Name != null && e.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Email != null && e.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            switch (SortOption)
            {
                case 0:
                    employees = employees?.ToList();
                    break;
                case 1:
                    employees = employees?.Where(e => e.Gender == true).ToList(); // Lọc giới tính Nam
                    break;
                case 2:
                    employees = employees?.Where(e => e.Gender != true).ToList(); // Lọc giới tính Nữ
                    break;
                case 3:
                    employees = employees?.Where(e => e.IsActive).ToList(); // Lọc Active
                    break;
                case 4:
                    employees = employees?.Where(e => !e.IsActive).ToList(); // Lọc Inactive
                    break;
                case 5:
                    employees = employees?.Where(e => e.Role == 2).ToList(); // Lọc theo role = sale
                    break;
                case 6:
                    employees = employees?.Where(e => e.Role == 3).ToList(); // Lọc theo role = marketing
                    break;
                case 7:
                    employees = employees?.Where(e => e.Role == 4).ToList(); // Lọc theo role = lễ tân
                    break;
                case 8:
                    employees = employees?.Where(e => e.Role == 1).ToList(); // Lọc theo role = quản lí
                    break;
                case 9:
                    employees = employees?.Where(e => e.Role == 0).ToList(); // Lọc theo role = admin
                    break;
            }

            switch (OrderOption)
            {
                case 1:
                    employees = employees?.OrderBy(e => e.Id).ToList(); // Sắp xếp ID tăng dần
                    break;
                case 2:
                    employees = employees?.OrderByDescending(e => e.Id).ToList(); // Sắp xếp ID giảm dần
                    break;
                case 3:
                    employees = employees?.OrderBy(e => e.Name).ToList(); // Sắp xếp tên A-Z
                    break;
                case 4:
                    employees = employees?.OrderByDescending(e => e.Name).ToList(); // Sắp xếp tên Z-A
                    break;
            }

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;
            TotalPages = (int)Math.Ceiling((double)employees.Count() / PageSize);

            Employees = employees.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();
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
