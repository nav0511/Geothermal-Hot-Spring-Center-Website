using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        [HttpGet("Total/{hasDiscount}")]
        public async Task<IActionResult> GetTotalNews(bool hasDiscount)
        {
            var list = _newsService.GetListNews();
            if (hasDiscount)
            {
                var result = list.Where(n => n.DiscountId != null);
                return Ok(await result.CountAsync());
            }
            else
            {
                var result = list.Where(n => n.DiscountId == null);
                return Ok(await result.CountAsync());
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetNewsById(int id)
        {
            var news = await _newsService.GetNewsById(id);
            if (news == null) return NotFound();
            var result = _mapper.Map<NewsDTO>(news);
            return Ok(result);
        }

        [HttpGet("GetByDiscountCode/{code}")]
        public async Task<IActionResult> GetNewsByDiscountCode(string code)
        {
            var news = await _newsService.GetNewsByDiscountCode(code);
            if (news == null) return NotFound();
            var result = _mapper.Map<NewsDTO>(news);
            return Ok(result);
        }

    }
}
