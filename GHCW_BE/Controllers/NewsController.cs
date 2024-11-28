﻿using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private IMapper _mapper;
        private NewsService _newsService;
        private DiscountService _discountService;
        private CloudinaryService _cloudinaryService;

        public NewsController(IMapper mapper, NewsService newsService, DiscountService discountService, CloudinaryService cloudinaryService)
        {
            _mapper = mapper;
            _newsService = newsService;
            _discountService = discountService;
            _cloudinaryService = cloudinaryService;
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

            return Ok("Cập nhật thành công");
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

            return Ok("Xóa thành công");
        }

        [Authorize]
        [HttpDelete("NewsActivation/{id}")]
        public async Task<IActionResult> NewsActivation(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 3)
            {
                var (isSuccess, message) = await _newsService.NewsActivation(id);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }


        [HttpPost]
        public async Task<IActionResult> CreateNews([FromForm] NewsDTOForAdd newsDto)
        {
            if (newsDto == null)
            {
                return BadRequest("Dữ liệu tin tức không hợp lệ.");
            }

            var news = _mapper.Map<News>(newsDto);

            news.Image = await _cloudinaryService.UploadImageResult(newsDto.Image);

            await _newsService.AddNews(news);

            var fakeCustomers = new List<CustomerDTO>
        {
            new CustomerDTO
            {
                Id = 1,
                FullName = "Phong DB 05",
                Email = "phongdb05@gmail.com",
                PhoneNumber = "0901234567",
                DoB = new DateTime(1995, 5, 15),
                Gender = true, // Assuming true is male
                Address = "Hà Nội, Việt Nam",
                IsEmailNotify = true
            },
            new CustomerDTO
            {
                Id = 2,
                FullName = "Phong DB 02",
                Email = "phongdb02@gmail.com",
                PhoneNumber = "0907654321",
                DoB = new DateTime(1992, 3, 25),
                Gender = true, // Assuming true is male
                Address = "Hồ Chí Minh, Việt Nam",
                IsEmailNotify = true
            }
        };
            var users = fakeCustomers;

            var emailSent = await _newsService.SendNewsNotificationAsync(news, users);
            if (!emailSent)
            {
                return StatusCode(500, "Tin tức được thêm nhưng không thể gửi email thông báo.");
            }


            return Ok("Thêm thành công");

        }
    }
}
