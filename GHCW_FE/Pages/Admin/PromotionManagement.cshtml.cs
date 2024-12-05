using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class PromotionManagementModel : PageModel
    {
        private NewsService _newsService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public PromotionManagementModel(TokenService tokenService, AuthenticationService authService, NewsService newsService, AccountService accService)
        {
            _authService = authService;
            _newsService = newsService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public List<NewsDTO> NewsDTOs { get; set; } = new List<NewsDTO>();

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

            SearchTerm = searchTerm;
            OrderOption = orderOption;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, news) = await _newsService.GetNews($"News?$filter=DiscountId ne null");
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                news = news?.Where(d => d.Title?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            news = OrderOption switch
            {
                1 => news.OrderBy(d => d.Id).ToList(),
                2 => news.OrderByDescending(d => d.Id).ToList(),
                3 => news.OrderBy(d => d.UploadDate).ToList(),
                4 => news.OrderByDescending(d => d.UploadDate).ToList(),
                _ => news.ToList(),
            };

            news = SortOption switch
            {
                1 => news.Where(n => n.IsActive).ToList(),
                2 => news.Where(n => !n.IsActive).ToList(),
                _ => news.ToList(),
            };

            var totalNews = news?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalNews / PageSize);
            NewsDTOs = news?.Skip(skip).Take(PageSize).ToList() ?? new List<NewsDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostPromotionActivation(int nId)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _newsService.NewsActivation(accessToken, nId);
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

        public async Task<IActionResult> OnPostDeletePromotion(int id)
        {
            var responseStatus = await _newsService.DeleteNews(id);
            if (responseStatus == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Xóa tin khuyến mãi thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa tin khuyến mãi.";
            }
            return RedirectToPage();

        }
    }
}
