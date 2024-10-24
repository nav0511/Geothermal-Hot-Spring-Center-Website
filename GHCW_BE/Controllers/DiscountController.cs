using AutoMapper;
using GHCW_BE.DTOs;
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
    }
}
