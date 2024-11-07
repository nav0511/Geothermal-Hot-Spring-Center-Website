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
            if (list == null)
            {
                return NotFound("Không có danh sách vé nào.");
            }

            var projectedQuery = _mapper.ProjectTo<TicketDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalBooking()
        {
            var list = _ticketService.GetListBooking();
            if (list == null)
            {
                return Ok(0); // Trả về 0 nếu không có dữ liệu
            }

            var count = await list.CountAsync();
            return Ok(count);
        }

        [HttpPut("Update-Checkin")]
        public async Task<IActionResult> UpdateCheckIn([FromBody] TicketDTO2 request)
        {
            if (request == null || request.Id == 0 || request.ReceptionistId == 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var ticket = await _ticketService.GetTicketById(request.Id);
            if (ticket == null)
            {
                return NotFound("Không tìm thấy vé.");
            }

            ticket.CheckIn = request.CheckIn;
            ticket.ReceptionistId = request.ReceptionistId;
            ticket.PaymentStatus = request.PaymentStatus;

            await _ticketService.UpdateTicket(ticket);

            return Ok("Cập nhật trạng thái Check-In thành công.");
        }
    }
}
