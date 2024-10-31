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
    public class NewsController : ControllerBase
    {
        private IMapper _mapper;
        private NewsService _newsService;
        private DiscountService _discountService;

        public NewsController(IMapper mapper, NewsService newsService, DiscountService discountService)
        {
            _mapper = mapper;
            _newsService = newsService;
            _discountService = discountService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetNews()
        {
            var list = _newsService.GetListNews();
            var projectedQuery = _mapper.ProjectTo<NewsDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalNews()
        {
            var list = _newsService.GetListNews();
            return Ok(list.Count());
        }

        [HttpGet("Reguler")]
        public async Task<IActionResult> GetTotalRegularNews()
        {
            var list = _newsService.GetListNews().Where(r => r.DiscountId == null);
            return Ok(list.Count());
        }

        [HttpGet("Promotion")]
        public async Task<IActionResult> GetTotalPromotionNews()
        {
            var list = _newsService.GetListNews().Where(r => r.DiscountId != null);
            return Ok(list.Count());
        }


    }
}
