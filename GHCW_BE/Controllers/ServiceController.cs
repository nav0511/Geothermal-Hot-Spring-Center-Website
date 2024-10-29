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

        public ServiceController(IMapper mapper, ServicesService servicesService)
        {
            _mapper = mapper;
            _servicesService = servicesService;
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

            return NoContent();
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

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceDTO2 serviceDto)
        {
            if (serviceDto == null)
            {
                return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
            }

            //var service = _mapper.Map<Service>(serviceDto);
            var service = new Service
            {
                Name = serviceDto.Name,
                Price = serviceDto.Price ?? 0,
                Time = serviceDto.Time,
                Description = serviceDto.Description,
                Image = serviceDto.Image
            };

           
           await _servicesService.AddService(service);
           

            return Ok("Add Success");
           
        }

       

    }
}
