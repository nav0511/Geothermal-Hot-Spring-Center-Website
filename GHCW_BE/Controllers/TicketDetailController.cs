using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketDetailController : ControllerBase
    {
        private IMapper _mapper;
        private TicketDetailService _ticketDetailService;
        private CloudinaryService _cloudinary;
        public TicketDetailController(IMapper mapper, TicketDetailService ticketDetaiService, CloudinaryService cloudinary)
        {
            _mapper = mapper;
            _ticketDetailService = ticketDetaiService;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetBookingList()
        {
            var list = _ticketDetailService.GetListBookingDetails();
            if (list == null)
            {
                return NotFound("Không có danh sách vé nào.");
            }

            var projectedQuery = _mapper.ProjectTo<TicketDetailDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketDetailById(int id)
        {
            var details = await _ticketDetailService.GetBookingDetails(id); 
            if (details == null)
            {
                return NotFound();
            }

            var ticketDetailDTOs = _mapper.Map<List<TicketDetailDTO>>(details);
            return Ok(ticketDetailDTOs);
        }

    }
}
