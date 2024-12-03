using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace GHCW_FE.Pages.Admin
{
    public class EditNewsModel : PageModel
    {
        private NewsService _newsService;
        private DiscountService _discountService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public EditNewsModel(TokenService tokenService, AuthenticationService authService, NewsService newsService, AccountService accService, DiscountService discountService)
        {
            _authService = authService;
            _newsService = newsService;
            _discountService = discountService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public List<DiscountDTO> Discounts { get; set; } = new List<DiscountDTO>();


        public NewsDTO News { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
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

            var (statusCode1, news) = await _newsService.GetNewsById(id);
            News = news;
            var (statusCode2, discounts) = await _discountService.GetDiscounts("Discount");
            Discounts = discounts;


            if (News == null)
            {
                ModelState.AddModelError(string.Empty, "Tin tức không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID tin tức không hợp lệ.");
                return Page();
            }

            var (statusCode, news) = await _newsService.GetNewsById(id);
            News = news;
            if (News == null)
            {
                ModelState.AddModelError(string.Empty, "Tin tức không tồn tại.");
                return NotFound();
            }

            News.Title = Request.Form["title"];
            News.DiscountId = Request.Form["discountId"];
            News.UploadDate = Convert.ToDateTime(Request.Form["uploadDate"]);
            News.IsActive = Request.Form["isActive"] == "on";
            News.Description = Request.Form["description"];
            News.Image = "/images/" + Request.Form["image"].ToString();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var promotion = new NewsDTO
            {
                Id = News.Id,
                Title = News.Title,
                DiscountId = News.DiscountId,
                UploadDate = News.UploadDate,
                IsActive = News.IsActive,
                Description = News.Description,
                Image = News.Image,
            };

            statusCode = await _newsService.UpdateNews(promotion);

            if (statusCode == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/NewsManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật tin tức.");
                return Page();
            }
        }
    }
}
