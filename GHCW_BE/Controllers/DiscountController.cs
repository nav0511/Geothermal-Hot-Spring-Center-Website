using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : Controller
    {
        private IMapper _mapper;
        private DiscountService _discountService;

        public DiscountController(IMapper mapper, DiscountService discountService)
        {
            _mapper = mapper;
            _discountService = discountService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetDiscounts()
        {
            var list = _discountService.GetListDiscounts();
            var projectedQuery = _mapper.ProjectTo<DiscountDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalDiscount()
        {
            var list = _discountService.GetListDiscounts();
            return Ok(list.Count());
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetDiscountByCode(string code)
        {
            var discount = _discountService.GetDiscount(code);

            if (discount == null)
            {
                return NotFound();
            }

            var discountDTO = _mapper.Map<DiscountDTO>(discount);
            return Ok(discountDTO);
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> UpdateDiscount(string code, [FromBody] DiscountDTO discountDto)
        {
            if (!code.Equals(discountDto.Code))
            {
                return BadRequest("Code không khớp.");
            }

            var existingDiscount = _discountService.GetDiscount(code);
            if (existingDiscount == null)
            {
                return NotFound();
            }

            _mapper.Map(discountDto, existingDiscount);

            try
            {
                await _discountService.UpdateDiscount(existingDiscount);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (existingDiscount == null)
                {
                    return NotFound("Mã giảm giá không tồn tại.");
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteDiscount(string code)
        {
            var existingDiscount = _discountService.GetDiscount(code);
            if (existingDiscount == null)
            {
                return NotFound("Mã giảm giá không tồn tại.");
            }

            try
            {
                await _discountService.DeleteDiscount(existingDiscount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa mã giảm giá: {ex.Message}");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] DiscountDTO2 discountDto)
        {
            if (discountDto == null)
            {
                return BadRequest("Dữ liệu mã giảm giá không hợp lệ.");
            }

            var discountCode = await _discountService.GenerateUniqueDiscountCode();

            var discount = new Discount
            {
                Code = discountCode,
                Name = discountDto.Name,
                Value = discountDto.Value ?? 0, 
                StartDate = discountDto.StartDate ?? DateTime.Now,
                EndDate = discountDto.EndDate ?? DateTime.Now.AddDays(10),
                Description = discountDto.Description,
                IsAvailable = discountDto.IsAvailable ?? false
            };


            await _discountService.AddDiscount(discount);


            return Ok("Add Success");

        }

    }
}
