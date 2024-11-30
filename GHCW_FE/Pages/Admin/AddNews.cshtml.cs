using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var promotion = new NewsDTOForAdd
            {
                Title = Request.Form["title"],
                DiscountId = Request.Form["discountId"],
                UploadDate = DateTime.Now,
                IsActive = Request.Form["isActive"] == "on",
                Description = Request.Form["description"],
                Image = Request.Form.Files["image"],
            };


            var response = await _newsService.CreateNews(promotion, "multipart/form-data");

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm tin tức.");
                return Page();
            }
        }
    }
}
