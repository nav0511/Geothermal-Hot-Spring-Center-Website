using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ProductManagementModel : PageModel
    {
        private ProductService _productService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public ProductManagementModel(TokenService tokenService, AuthenticationService authService, ProductService productService, AccountService accService)
        {
            _authService = authService;
            _productService = productService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<ProductDTO> ProductDTOs { get; set; } = new List<ProductDTO>();

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
            if (userProfile?.Role > 1)
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

            var(statusCode1, TotalNewsCount) = _productService.GetTotalProducts().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, productDTOs) = await _productService.GetProducts($"Product?$top={PageSize}&$skip={skip}");
            ProductDTOs = productDTOs;

            return Page();
        }

        public async Task<IActionResult> OnPostProductActivation(int nId)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _productService.ProductActivation(accessToken, nId);
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

        public async Task<IActionResult> OnPostDeleteProduct(int id)
        {
            var responseStatus = await _productService.DeleteProduct(id);
            if (responseStatus == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phẩm.";
            }
            return RedirectToPage();
        }
    }
}
