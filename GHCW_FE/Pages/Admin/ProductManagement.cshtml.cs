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
        private ProductService _productService = new ProductService();

        public List<ProductDTO> ProductDTOs { get; set; } = new List<ProductDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var(statusCode, TotalNewsCount) = _productService.GetTotalProducts().Result;
            int totalNewsCount = TotalNewsCount;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode2, productDTOs) = await _productService.GetProducts($"Product?$top={PageSize}&$skip={skip}");
            ProductDTOs = productDTOs;
        }

        public async Task<IActionResult> OnPostDeleteProduct(int id)
        {
            var responseStatus = await _productService.DeleteProduct(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {

                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa dịch vụ.");
                return Page();
            }
        }
    }
}
