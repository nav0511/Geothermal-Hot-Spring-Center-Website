using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly CustomerService _customerService;
        private readonly DiscountService _discountService;
        public TicketController(IMapper mapper, TicketService ticketService, CloudinaryService cloudinary, CustomerService customerService, DiscountService discountService)
        {
            _mapper = mapper;
            _ticketService = ticketService;
            _cloudinary = cloudinary;
            _customerService = customerService;
            _discountService = discountService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetBookingList(int? role, int? uId)
        {
            var list = _ticketService.GetListBooking(role, uId);
            if (list == null)
            {
                return NotFound("Không có danh sách vé nào.");
            }

            var projectedQuery = _mapper.ProjectTo<TicketDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("Total")]
        public async Task<IActionResult> GetTotalBooking(int? role, int? uId)
        {
            var list = _ticketService.GetListBooking(role, uId);
            if (list == null)
            {
                return Ok(0); 
            }

            var count = await list.CountAsync();
            return Ok(count);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var ticket = await _ticketService.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound("Vé không tồn tại");
            }

            return Ok(ticket);
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

        [Authorize]
        [HttpPost("Save")]
        public async Task<IActionResult> SaveTicket([FromBody] TicketDTOForPayment ticketDto)
        {
            if (ticketDto == null || ticketDto.CustomerId == 0 || ticketDto.TicketDetails == null || !ticketDto.TicketDetails.Any())
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var customer = await _customerService.GetCustomerProfileByAccountId(ticketDto.CustomerId);
            if (customer == null) return StatusCode(500, "Có lỗi xảy ra khi lưu vé.");

            var newTicket = _mapper.Map<Ticket>(ticketDto);
            newTicket.CustomerId = customer.Id;
            foreach (var ticket in newTicket.TicketDetails)
            {
                ticket.Total = ticket.Price * ticket.Quantity;
            }
            var discount = _discountService.GetDiscount(ticketDto.DiscountCode);
            if (discount != null) newTicket.Total *= (1 - (discount.Value / 100.0m));

            var result = await _ticketService.SaveTicketAsync(newTicket);

            if (result == null)
            {
                return StatusCode(500, "Có lỗi xảy ra khi lưu vé.");
            }

            var addedTicket = await _ticketService.GetTicketByIdIncludeService(newTicket.Id);
            var emailSent = await _ticketService.SendTicketToEmail(addedTicket, customer);
            if (emailSent == false)
            {
                return StatusCode(500, "Có lỗi xảy ra khi gửi email.");
            }

            return Ok("Đã lưu vé thành công.");
        }


        [HttpGet("TicketDetail")]
        [EnableQuery]
        public async Task<IActionResult> GetBookingList()
        {
            var list = _ticketService.GetListBookingDetails();
            if (list == null)
            {
                return NotFound("Không có danh sách vé nào.");
            }

            var projectedQuery = _mapper.ProjectTo<TicketDetailDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("TicketDetail/{id}")]
        public async Task<IActionResult> GetTicketDetailById(int id)
        {
            var details = await _ticketService.GetBookingDetails(id);
            if (details == null)
            {
                return NotFound();
            }

            var ticketDetailDTOs = _mapper.Map<List<TicketDetailDTO>>(details);
            return Ok(ticketDetailDTOs);
        }

    }
}
