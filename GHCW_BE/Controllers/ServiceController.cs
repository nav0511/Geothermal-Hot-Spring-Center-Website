using GHCW_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromForm] ServiceDTOForUpdate serviceDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
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
                existingService.Image = await _cloudinary.UploadImageResult(serviceDto.Image);

                var (isSuccess, message) = await _servicesService.UpdateService(existingService);

                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateService([FromForm] ServiceDTO2 serviceDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                if (serviceDto == null)
                {
                    return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
                }

                var service = _mapper.Map<Service>(serviceDto);
                service.Image = await _cloudinary.UploadImageResult(serviceDto.Image);

                var (isSuccess, message) = await _servicesService.AddService(service);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpDelete("ServiceActivation/{id}")]
        public async Task<IActionResult> ServiceActivation(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _servicesService.ServiceActivation(id);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

    }
}
