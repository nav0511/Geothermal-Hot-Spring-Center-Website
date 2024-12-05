using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddProductModel : PageModel
    {
        private ProductService _productService;
        private CategoryService _categoryService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public AddProductModel(TokenService tokenService, AuthenticationService authService, ProductService productService, AccountService accService, CategoryService categoryService)
        {
            _authService = authService;
            _productService = productService;
            _categoryService = categoryService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public List<string> Sizes { get; } = new List<string> { "XS", "S", "M", "L", "XL", "XXL"};

        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

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
            if (roleClaim != null && int.Parse(roleClaim.Value) > 1)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }

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

            var (statusCode1, categories) = await _categoryService.GetCategory("Category");
            Categories = categories;
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
            _productService.SetAccessToken(accessToken);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = new ProductDTOImg
            {
                Name = Request.Form["name"],
                Price = Convert.ToDouble(Request.Form["price"]),
                Description = Request.Form["description"],
                Image = "/images/" + Request.Form["image"].ToString(),
                CategoryId = Convert.ToInt32(Request.Form["categoryId"]),
                Size = Request.Form["size"],
                IsForRent = Request.Form["isForRent"] == "on",
                Quantity = Convert.ToInt32(Request.Form["quantity"]),
                IsAvailable = Request.Form["isAvailable"] == "on",
                Img = Request.Form.Files["image"]
            };


            var response = await _productService.CreateProduct(product, accessToken, "multipart/form-data");

            if (response == HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công";
                return RedirectToPage("/Admin/ProductManagement");
            }
            else
            {
                @TempData["ErrorMessage"] = ("Có lỗi xảy ra khi thêm sản phẩm.");
                return Page();
            }
        }
    }
}
