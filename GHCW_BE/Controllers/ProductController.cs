using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IMapper _mapper;
        private ProductService _productService;

        public ProductController(IMapper mapper, ProductService productService)
        {
            _mapper = mapper;
            _productService = productService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetProducts()
        {
            var list = _productService.GetListProducts();
            var projectedQuery = _mapper.ProjectTo<ProductDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }


        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalProducts()
        {
            var list = _productService.GetListProducts();
            return Ok(list.Count());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByID(int id)
        {
            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            var productDTO = _mapper.Map<ProductDTO>(product);
            return Ok(productDTO);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTOForUpdate productDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                if (id != productDto.Id)
                {
                    return BadRequest("ID không khớp.");
                }

                var existingProduct = await _productService.GetProductById(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                _mapper.Map(productDto, existingProduct);

                string? imageUrl = null;

                if (productDto.Img != null && productDto.Img.Length > 0)
                {
                    imageUrl = await _productService.UploadImageResult(productDto.Img);
                }
                existingProduct.Image = imageUrl;

                var (isSuccess, message) = await _productService.UpdateProduct(existingProduct);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _productService.GetListProducts()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingProduct == null)
            {
                return NotFound("Sản phẩm không tồn tại.");
            }

            try
            {
                await _productService.DeleteProduct(existingProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa sản phẩm: {ex.Message}");
            }

            return Ok("Xóa thành công");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDTOImg productDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                if (productDto == null)
                {
                    return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
                }

                var product = _mapper.Map<Product>(productDto);

                string? imageUrl = null;

                if (productDto.Image != null && productDto.Image.Length > 0)
                {
                    imageUrl = await _productService.UploadImageResult(productDto.Img);
                }
                product.Image = imageUrl;

                var (isSuccess, message) = await _productService.AddProduct(product);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpDelete("ProductActivation/{id}")]
        public async Task<IActionResult> ProductActivation(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _productService.ProductActivation(id);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpPost("Image")]
        public async Task<IActionResult> AddImage(IFormFile img)
        {
            return Ok(await _productService.UploadImageResult(img));
        }
    }
}
