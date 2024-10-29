using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddProductModel : PageModel
    {
        private ProductService _poductService = new ProductService();
        private CategoryService _categoryService = new CategoryService();
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetCategory("Category");
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = new ProductDTO
            {
                Name = Request.Form["name"],
                Price = Convert.ToDouble(Request.Form["price"]),
                Description = Request.Form["description"],
                Image = "/images/" + Request.Form["image"].ToString(),
                CategoryId = Convert.ToInt32(Request.Form["categoryId"]),
                Size = Request.Form["size"],
                IsForRent = Request.Form["isForRent"] == "on",
                Quantity = Convert.ToInt32(Request.Form["quantity"]),
                IsAvailable = Request.Form["isAvailable"] == "on"
            };


            var response = await _poductService.CreateProduct(product);

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/ProductManagement");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm dịch vụ.");
                return Page();
            }
        }
    }
}
