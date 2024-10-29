using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;


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
            var product = await _productService.GetListProducts()
                                                .FirstOrDefaultAsync(s => s.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productDTO = _mapper.Map<ProductDTO>(product);
            return Ok(productDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var existingProduct = await _productService.GetListProducts()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            _mapper.Map(productDto, existingProduct);

            try
            {
                await _productService.UpdateProduct(existingProduct);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (existingProduct == null)
                {
                    return NotFound("Sản phẩm không tồn tại.");
                }
                throw;
            }

            return NoContent();
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

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO2 productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
            }

            var product = _mapper.Map<Product>(productDto);
            


            await _productService.AddProduct(product);


            return Ok("Add Success");

        }
    }
}
