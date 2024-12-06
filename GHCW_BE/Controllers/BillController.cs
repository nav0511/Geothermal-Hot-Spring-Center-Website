using AutoMapper;
using GHCW_BE.Services;
using GHCW_BE.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GHCW_BE.Models;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private IMapper _mapper;
        private BillService _billService;
        private CloudinaryService _cloudinary;
        private readonly CustomerService _customerService;
        private readonly DiscountService _discountService;
        public BillController(IMapper mapper, BillService billService, CloudinaryService cloudinary, CustomerService customerService, DiscountService discountService)
        {
            _mapper = mapper;
            _billService = billService;
            _cloudinary = cloudinary;
            _customerService = customerService;
            _discountService = discountService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetBillList(int? role, int? uId)
        {
            var list = _billService.GetListBill(role, uId);
            if (list == null)
            {
                return NotFound("Không có danh sách hóa đơn nào.");
            }

            var projectedQuery = _mapper.ProjectTo<BillDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBillById(int id)
        {
            var ticket = await _billService.GetBilltById(id);
            if (ticket == null)
            {
                return NotFound("Hóa đơn không tồn tại");
            }

            return Ok(ticket);
        }

        [Authorize]
        [HttpDelete("BillActivation/{id}")]
        public async Task<IActionResult> BillActivation(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _billService.BillActivation(id);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpGet("BillDetail/{id}")]
        public async Task<IActionResult> GetBillDetailById(int id)
        {
            var details = await _billService.GetBillDetails(id);
            if (details == null)
            {
                return NotFound();
            }

            var billDetailDTOs = _mapper.Map<List<BillDetailDTO>>(details);
            return Ok(billDetailDTOs);
        }

        [Authorize]
        [HttpPost("Save")]
        public async Task<IActionResult> SaveBillForStaff([FromBody] BillDTOForBuyProducts billDto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) == 4)
            {
                if (billDto == null || billDto.CustomerId == 0 || billDto.BillDetails == null
                    || !billDto.BillDetails.Any() || billDto.ReceptionistId == null)
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                var discount = _discountService.GetDiscount(billDto.DiscountCode);
                if (discount != null) billDto.Total *= (1 - (discount.Value / 100.0m));
                else billDto.DiscountCode = null;

                var newbill = _mapper.Map<Bill>(billDto);
                foreach (var bill in newbill.BillDetails)
                {
                    bill.Total = bill.Price * bill.Quantity;
                }

                var result = await _billService.SaveBillAsync(newbill);

                if (result == null)
                {
                    return StatusCode(500, "Có lỗi xảy ra khi lưu vé.");
                }

                return Ok("Đã lưu vé thành công.");
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }


    }
}
