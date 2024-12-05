using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using GHCW_BE.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : Controller
    {
        private IMapper _mapper;
        private DiscountService _discountService;
        private NewsService _newsService;


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
            return Ok(await list.CountAsync());
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

        [Authorize]
        [HttpPut("{code}")]
        public async Task<IActionResult> UpdateDiscount(string code, [FromBody] DiscountDTO discountDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
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

                var (isSuccess, message) = await _discountService.UpdateDiscount(existingDiscount);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
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

            return Ok("Xóa thành công");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] DiscountDTO discountDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                if (discountDto == null)
                {
                    return BadRequest("Dữ liệu mã giảm giá không hợp lệ.");
                }
                var checkDisExist = await _discountService.CheckDiscountExsit(discountDto.Code);
                if (checkDisExist != null)
                {
                    return Conflict("Code đã được sử dụng, vui lòng dùng Code khác");
                }
                var discount = new Discount
                {
                    Code = discountDto.Code,
                    Name = discountDto.Name,
                    Value = discountDto.Value,
                    StartDate = discountDto.StartDate,
                    EndDate = discountDto.EndDate,
                    Description = discountDto.Description,
                    IsAvailable = discountDto.IsAvailable
                };
                var (isSuccess, message) = await _discountService.AddDiscount(discount);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpDelete("DiscountActivation/{code}")]
        public async Task<IActionResult> DiscountActivation(string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _discountService.DiscountActivation(code);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpGet("AvailableForNews")]
        public async Task<IActionResult> GetAvailableDiscountsForNews([FromQuery] string? id)
        {
            var discounts = await _discountService.GetAvailableDiscountsForNewsAsync(id);
            return Ok(discounts);
        }

    }
}
