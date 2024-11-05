using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private IMapper _mapper;
        private TicketService _ticketService;
        private CloudinaryService _cloudinary;
        public TicketController(IMapper mapper, TicketService ticketService, CloudinaryService cloudinary)
        {
            _mapper = mapper;
            _ticketService = ticketService;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetBookingList()
        {
            var list = _ticketService.GetListBooking();
            var projectedQuery = _mapper.ProjectTo<TicketDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalBooking()
        {
            var list = _ticketService.GetListBooking();
            return Ok(list.Count());
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetServiceByID(int id)
        //{
        //    var service = await _servicesService.GetListServices()
        //                                        .FirstOrDefaultAsync(s => s.Id == id);

        //    if (service == null)
        //    {
        //        return NotFound();
        //    }

        //    var serviceDTO = _mapper.Map<ServiceDTO>(service);
        //    return Ok(serviceDTO);
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDTO serviceDto)
        //{
        //    if (id != serviceDto.Id)
        //    {
        //        return BadRequest("ID không khớp.");
        //    }

        //    var existingService = await _servicesService.GetListServices()
        //                                                .FirstOrDefaultAsync(s => s.Id == id);
        //    if (existingService == null)
        //    {
        //        return NotFound();
        //    }

        //    _mapper.Map(serviceDto, existingService);

        //    try
        //    {
        //        await _servicesService.UpdateService(existingService);
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (existingService == null)
        //        {
        //            return NotFound("Dịch vụ không tồn tại.");
        //        }
        //        throw;
        //    }

        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteService(int id)
        //{
        //    var existingService = await _servicesService.GetListServices()
        //                                                .FirstOrDefaultAsync(s => s.Id == id);
        //    if (existingService == null)
        //    {
        //        return NotFound("Dịch vụ không tồn tại.");
        //    }

        //    try
        //    {
        //        await _servicesService.DeleteService(existingService);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa dịch vụ: {ex.Message}");
        //    }

        //    return NoContent();
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateService([FromForm] ServiceDTO2 serviceDto)
        //{
        //    if (serviceDto == null)
        //    {
        //        return BadRequest("Dữ liệu dịch vụ không hợp lệ.");
        //    }

        //    var service = _mapper.Map<Service>(serviceDto);
        //    service.Image = await _cloudinary.UploadImageResult(serviceDto.Image);


        //    await _servicesService.AddService(service);


        //    return Ok("Add Success");

        //}
    }
}
