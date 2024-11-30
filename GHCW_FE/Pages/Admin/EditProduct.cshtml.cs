using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace GHCW_FE.Pages.Admin
{
    public class EditProductModel : PageModel
    {
        private ProductService _productService;
        private CategoryService _categoryService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public EditProductModel(TokenService tokenService, AuthenticationService authService, ProductService productService, AccountService accService, CategoryService categoryService)
        {
            _authService = authService;
            _productService = productService;
            _categoryService = categoryService;
            _tokenService = tokenService;
            _accService = accService;
        }
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public List<string> Sizes { get; } = new List<string> { "XS", "S", "M", "L", "XL", "XXL" };


        public ProductDTO Product { get; set; }

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

            var (statusCode1, product) = await _productService.GetProductByID(id);
            Product = product;
            var (statusCode2, categories) = await _categoryService.GetCategory("Category");
            Categories = categories;


            if (Product == null)
            {
                ModelState.AddModelError(string.Empty, "Sản phẩm không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID dịch vụ không hợp lệ.");
                return Page();
            }

            var (statusCode, product) = await _productService.GetProductByID(id);
            Product = product;
            if (Product == null)
            {
                ModelState.AddModelError(string.Empty, "Dịch vụ không tồn tại.");
                return NotFound();
            }

            Product.Name = Request.Form["name"];
            Product.Price = Convert.ToDouble(Request.Form["price"]);
            Product.Description = Request.Form["description"];
            Product.Image = "/images/" + Request.Form["image"].ToString();
            Product.CategoryId = Convert.ToInt32(Request.Form["categoryId"]);
            Product.Size = Request.Form["size"];
            Product.IsForRent = Request.Form["isForRent"] == "on";
            Product.Quantity = Convert.ToInt32(Request.Form["quantity"]);
            Product.IsAvailable = Request.Form["isAvailable"] == "on";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var productDto = new ProductDTO
            {
                Id = Product.Id,
                Name = Product.Name,
                Price = Product.Price,
                Description = Product.Description,
                Image = Product.Image,
                CategoryId = Product.CategoryId,
                Size = Product.Size,
                IsForRent = Product.IsForRent,
                Quantity = Product.Quantity,
                IsAvailable = Product.IsAvailable
            };

            statusCode = await _productService.UpdateProduct(productDto);

            if (statusCode == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/ProductManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật dịch vụ.");
                return Page();
            }
        }
    }
}
