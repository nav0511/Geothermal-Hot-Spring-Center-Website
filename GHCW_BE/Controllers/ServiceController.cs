using GHCW_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

﻿using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private IMapper _mapper;
        private ServicesService _servicesService;
        private CloudinaryService _cloudinary;
        public ServiceController(IMapper mapper, ServicesService servicesService, CloudinaryService cloudinary)
        {
            _mapper = mapper;
            _servicesService = servicesService;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetServices()
        {
            var list = _servicesService.GetListServices();
            var projectedQuery = _mapper.ProjectTo<ServiceDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalServices()
        {
            var list = _servicesService.GetListServices();
            return Ok(list.Count());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceByID(int id)
        {
            var service = await _servicesService.GetListServices()
                                                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            var serviceDTO = _mapper.Map<ServiceDTO>(service);
            return Ok(serviceDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDTO serviceDto)
        {
            if (id != serviceDto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var existingService = await _servicesService.GetListServices()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingService == null)
            {
                return NotFound();
            }

            _mapper.Map(serviceDto, existingService);

            try
            {
                await _servicesService.UpdateService(existingService);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (existingService == null)
                {
                    return NotFound("Dịch vụ không tồn tại.");
                }
                throw;
            }

            return Ok("Cập nhật thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var existingService = await _servicesService.GetListServices()
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (existingService == null)
            {
                return NotFound("Dịch vụ không tồn tại.");
            }

            try
            {
                await _servicesService.DeleteService(existingService);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa dịch vụ: {ex.Message}");
            }

            return Ok("Xóa thành công");
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromForm] ServiceDTO2 serviceDto)
        {
            if (serviceDto == null)
            {
                return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
            }

            var service = _mapper.Map<Service>(serviceDto);
            service.Image = await _cloudinary.UploadImageResult(serviceDto.Image);

           
           await _servicesService.AddService(service);
           

            return Ok("Thêm thành công");
           
        }

       

    }
}
