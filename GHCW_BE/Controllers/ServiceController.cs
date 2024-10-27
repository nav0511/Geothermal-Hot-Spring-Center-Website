<<<<<<< HEAD
﻿using GHCW_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
=======
﻿using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
>>>>>>> main

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
<<<<<<< HEAD
        GHCWContext context = new GHCWContext();

        [HttpGet]
        public ActionResult GetAllService()
        {
            try
            {
                var services = context.Services.ToList();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching data.");
            }
        }



=======
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
>>>>>>> main
    }
}
