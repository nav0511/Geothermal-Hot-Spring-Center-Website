using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddNewsModel : PageModel
    {
        private NewsService _newsService;
        private DiscountService _discountService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public AddNewsModel(TokenService tokenService, AuthenticationService authService, NewsService newsService, AccountService accService, DiscountService discountService)
        {
            _authService = authService;
            _newsService = newsService;
            _discountService = discountService;
            _tokenService = tokenService;
            _accService = accService;
        }


        public List<DiscountDTO> Discounts { get; set; } = new List<DiscountDTO>();

        public async Task<IActionResult> OnGetAsync()
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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 3)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }

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

            var (statusCode1, discounts) = await _discountService.GetDiscounts("Discount");
            Discounts = discounts;
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 0)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            _newsService.SetAccessToken(accessToken);

            if (!ModelState.IsValid)
            {
                return Page();
            }
            var promotion = new NewsDTOForAdd();
            var discount = Request.Form["discountId"];
            if (discount == "0")
            {
                promotion = new NewsDTOForAdd
                {
                    Title = Request.Form["title"],
                    DiscountId = null,
                    UploadDate = DateTime.Now,
                    IsActive = Request.Form["isActive"] == "on",
                    Description = Request.Form["description"],
                    Image = Request.Form.Files["image"],
                };
            }
            else
            {
                promotion = new NewsDTOForAdd
                {
                    Title = Request.Form["title"],
                    DiscountId = discount,
                    UploadDate = DateTime.Now,
                    IsActive = Request.Form["isActive"] == "on",
                    Description = Request.Form["description"],
                    Image = Request.Form.Files["image"],
                };
            }

            var response = await _newsService.CreateNews(promotion, accessToken, "multipart/form-data");

            if (response == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Thêm tin tức thành công";
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                @TempData["ErrorMessage"] = ("Có lỗi xảy ra khi thêm tin tức.");
                return Page();
            }
        }
    }
}
