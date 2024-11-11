using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class DiscountManagementModel : PageModel
    {
        private readonly DiscountService _discountService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public DiscountManagementModel(TokenService tokenService, AuthenticationService authService, DiscountService discountService, AccountService accService)
        {
            _authService = authService;
            _discountService = discountService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

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

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 3)
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

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var(statusCode1, TotalNewsCount) = _discountService.GetTotalDiscounts().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, discounts) = await _discountService.GetDiscounts($"Discount?$top={PageSize}&$skip={skip}");
            DiscountDTOs = discounts;
            return Page();
        }

        public async Task<IActionResult> OnPostDiscountActivation(string code)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _discountService.DiscountActivation(accessToken, code);
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


        public async Task<IActionResult> OnPostDeleteDiscount(string id)
        {
            var responseStatus = await _discountService.DeleteDiscount(id);
            if (responseStatus == HttpStatusCode.OK)
            {

                TempData["SuccessMessage"] = "Xóa phiếu giảm giá thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phiếu giảm giá.";
            }
            return RedirectToPage();
        }
    }
}
