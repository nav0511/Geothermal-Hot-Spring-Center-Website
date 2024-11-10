using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class NewsManagementModel : PageModel
    {
        private NewsService _newsService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public NewsManagementModel(TokenService tokenService, AuthenticationService authService, NewsService newsService, AccountService accService)
        {
            _authService = authService;
            _newsService = newsService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<NewsDTO> NewsDTOs { get; set; } = new List<NewsDTO>();

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

            var (statusCode1, TotalNewsCount) = _newsService.GetTotalRegularNews().Result;
            int totalNewsCount = TotalNewsCount;

            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, newsDTOs) = await _newsService.GetNews($"News?$filter=DiscountId eq null&top={PageSize}&$skip={skip}");

            NewsDTOs = newsDTOs;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteNews(int id)
        {
            var responseStatus = await _newsService.DeleteNews(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {
                TempData["SuccessMessage"] = "Xóa tin tức thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa tin tức.";
            }
            return RedirectToPage();
        }
    }
}
