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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromBody] NewsDTO newsDto)
        {
            if (id != newsDto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var existingNews = await _newsService.GetListNews()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingNews == null)
            {
                return NotFound();
            }

            _mapper.Map(newsDto, existingNews);

            try
            {
                await _newsService.UpdateNews(existingNews);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (existingNews == null)
                {
                    return NotFound("Tin tức không tồn tại.");
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var existingNews = await _newsService.GetListNews()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingNews == null)
            {
                return NotFound("Tin tức không tồn tại.");
            }

            try
            {
                await _newsService.DeleteNews(existingNews);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa tin tức: {ex.Message}");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews([FromBody] NewsDTO2 newsDto)
        {
            if (newsDto == null)
            {
                return BadRequest("Dữ liệu tin tức không hợp lệ.");
            }

            var news = _mapper.Map<News>(newsDto);



            await _newsService.AddNews(news);


            return Ok("Add Success");

        }
    }
}
