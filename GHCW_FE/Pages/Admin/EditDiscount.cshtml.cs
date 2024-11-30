using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EditDiscountModel : PageModel
    {
        private DiscountService _discountService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public EditDiscountModel(TokenService tokenService, AuthenticationService authService, DiscountService discountService, AccountService accService)
        {
            _authService = authService;
            _discountService = discountService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public DiscountDTO Discount { get; set; }

        public async Task<IActionResult> OnGetAsync(string code)
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

            var (statusCode1, discount) = await _discountService.GetDiscountByCode(code);
            Discount = discount;
            if (Discount == null)
            {
                ModelState.AddModelError(string.Empty, "Phiếu giảm giá không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string code)
        {
            var (statusCode, discount) = await _discountService.GetDiscountByCode(code);
            Discount = discount;
            if (Discount == null)
            {
                ModelState.AddModelError(string.Empty, "Phiếu giảm giá không tồn tại.");
                return NotFound();
            }

            Discount.Code = Request.Form["code"];
            Discount.Name = Request.Form["name"];
            Discount.Value = Convert.ToInt32(Request.Form["value"]);
            Discount.StartDate = Convert.ToDateTime(Request.Form["startDate"]);
            Discount.EndDate = Convert.ToDateTime(Request.Form["endDate"]);
            Discount.Description = Request.Form["description"];
            Discount.IsAvailable = Request.Form["isAvailable"] == "on";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var discountDto = new DiscountDTO
            {
                Code = Discount.Code,
                Name = Discount.Name,
                Value = Discount.Value,
                StartDate = Discount.StartDate,
                EndDate = Discount.EndDate,
                Description = Discount.Description,
                IsAvailable = Discount.IsAvailable
            };

            statusCode = await _discountService.UpdateDiscount(discountDto);

            if (statusCode == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/DiscountManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật phiếu giảm giá.");
                return Page();
            }
        }
    }
}
