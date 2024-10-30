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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _servicesService.GetServiceById(id);
            if (service == null) return NotFound();
            var result = _mapper.Map<ServiceDTO>(service);
            return Ok(result);
        }
    }
}
