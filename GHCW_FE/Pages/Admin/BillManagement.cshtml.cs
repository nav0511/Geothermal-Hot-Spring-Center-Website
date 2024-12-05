using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class BillManagementModel : PageModel
    {
        private readonly BillService _billService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public BillManagementModel(TokenService tokenService, AuthenticationService authService, BillService billService, AccountService accService)
        {
            _authService = authService;
            _billService = billService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<BillDTO> BillDTOs { get; set; } = new List<BillDTO>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, string? searchTerm = null, int orderOption = 0, int sortOption = 0)
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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 1 && int.Parse(roleClaim.Value) != 4)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 4 || userProfile?.Role == 2 || userProfile?.Role == 3)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            SearchTerm = searchTerm;
            OrderOption = orderOption;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, bills) = await _billService.GetBillList("Bill", userProfile.Role, userProfile.Id);
            if (statusCode1 != HttpStatusCode.OK || bills == null)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách vé.";
                bills = new List<BillDTO>();
                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                bills = bills
                        .Where(d => d.Customer != null &&
                                    d.Customer.Name != null &&
                                    d.Customer.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }

            bills = OrderOption switch
            {
                1 => bills.OrderBy(d => d.OrderDate).ToList(),
                2 => bills.OrderByDescending(d => d.OrderDate).ToList(),
                _ => bills.ToList(),
            };

            bills = SortOption switch
            {
                1 => bills.Where(b => b.PaymentStatus == 1).ToList(),
                2 => bills.Where(b => b.PaymentStatus == 0).ToList(),
                _ => bills.ToList(),
            };

            var totalBills = bills?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalBills / PageSize);
            BillDTOs = bills?.Skip(skip).Take(PageSize).ToList() ?? new List<BillDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostBillActivation(int nId)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _billService.BillActivation(accessToken, nId);
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
