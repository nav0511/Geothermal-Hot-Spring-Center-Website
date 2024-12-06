using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
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

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;

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

            var (statusCode1, products) = await _productService.GetProducts("Product");
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                products = products?.Where(d => d.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            products = OrderOption switch
            {
                1 => products.OrderBy(d => d.Id).ToList(),
                2 => products.OrderByDescending(d => d.Id).ToList(),
                3 => products.OrderBy(d => d.Price).ToList(),
                4 => products.OrderByDescending(d => d.Price).ToList(),
                _ => products.ToList(),
            };

            products = SortOption switch
            {
                1 => products.Where(p => p.IsAvailable).ToList(),
                2 => products.Where(p => !p.IsAvailable).ToList(),
                3 => products.Where(p => p.Size == "XS").ToList(),
                4 => products.Where(p => p.Size == "S").ToList(),
                5 => products.Where(p => p.Size == "M").ToList(),
                6 => products.Where(p => p.Size == "L").ToList(),
                7 => products.Where(p => p.Size == "XL").ToList(),
                8 => products.Where(p => p.Size == "XXL").ToList(),
                _ => products.ToList(),
            };

            var totalProducts = products?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalProducts / PageSize);
            ProductDTOs = products?.Skip(skip).Take(PageSize).ToList() ?? new List<ProductDTO>();

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
