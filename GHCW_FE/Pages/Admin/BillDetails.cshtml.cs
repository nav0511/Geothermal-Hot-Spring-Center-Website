using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class BillDetailsModel : PageModel
    {
        private BillService _billService;
        private DiscountService _discountService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public BillDetailsModel(TokenService tokenService, AuthenticationService authService, BillService billService, AccountService accService, DiscountService discountService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _accService = accService;
            _billService = billService;
            _discountService = discountService;
        }

        public int BillId { get; set; }
        public decimal TotalAmount { get; set; }

        public List<BillDetailDTO> BillDetails { get; set; } = new List<BillDetailDTO>();
        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

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

            BillId = id;

            var (statusCode0, billDetails) = await _billService.GetBillDetailsById(id);
            var (statusCode1, discountList) = await _discountService.GetDiscounts("Discount");

            BillDetails = billDetails.ToList();
            DiscountDTOs = discountList.ToList();

            var discountCode = BillDetails.FirstOrDefault()?.Bill?.DiscountCode;

            var discount = discountList.FirstOrDefault(d => d.Code == discountCode);

            int discountValue = 0;
            if (discount != null)
            {
                discountValue = discount?.Value ?? 0;
            }

            TotalAmount = BillDetails.Sum(td => td.Price * td.Quantity);

            if (discount != null)
            {
                if (discount.Value > 0)
                {
                    TotalAmount -= TotalAmount * discountValue / 100;
                }
            }

            return Page();
        }
    }
}
