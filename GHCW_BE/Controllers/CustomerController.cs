using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _service;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accountService;

        public CustomerController(CustomerService service, AuthenticationService authService, AccountService accountService)
        {
            _service = service;
            _authService = authService;
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetCustomerProfileById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 4)
            {
                var customer = await _service.GetCustomerProfileById(id);
                if (customer == null)
                {
                    return NotFound("Khách hàng không tồn tại.");
                }
                return Ok(customer);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin này.");
        }

        [Authorize]
        [EnableQuery]
        [HttpGet("CustomerList")]
        public async Task<IActionResult> GetCustomerList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var acc = await _service.GetCustomerList();
                if (acc == null)
                {
                    return NotFound("Danh sách khách hàng trống");
                }
                return Ok(acc);
            }
            else if (roleClaim != null && int.Parse(roleClaim.Value) > 1 && int.Parse(roleClaim.Value) <= 3)
            {
                var acc = await _service.GetSubcribeCustomerList();
                if (acc == null)
                {
                    return NotFound("Danh sách khách hàng trống");
                }
                return Ok(acc);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin này.");
        }

        [Authorize]
        [HttpPost("addcustomer")]
        public async Task<IActionResult> AddNewCustomer(AddCustomerRequest a)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 4)
            {
                var checkAccExist = await _service.CheckCustomerExsit(a.Email);
                if (checkAccExist != null)
                {
                    return Conflict("Email đã được sử dụng, vui lòng chọn email khác!");
                }
                else
                {
                    var (isSuccess, message) = await _service.AddNewCustomer(a);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    if (a.AccountId != null)
                    {
                        UpdateRequest ur = new UpdateRequest()
                        {
                            Id = a.AccountId ?? 0,
                            Name = a.FullName,
                            PhoneNumber = a.PhoneNumber,
                            Gender = a.Gender
                        };
                        var (isSuccess2, message2) = await _accountService.UpdateProfile(ur);
                        if (!isSuccess2)
                        {
                            return BadRequest(message2);
                        }
                        return Ok(message2);
                    }
                    return Ok(message);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpPut("editcustomer")]
        public async Task<IActionResult> EditCustomer(CustomerDTO c)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 4)
            {
                var (isSuccess, message) = await _service.EditCustomer(c);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                if (c.AccountId != null)
                {
                    UpdateRequest ur = new UpdateRequest()
                    {
                        Id = c.AccountId ?? 0,
                        Name = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        DoB = c.DoB,
                        Gender = c.Gender,
                        Address = c.Address
                    };
                    (isSuccess, message) = await _accountService.UpdateProfile(ur);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpPut("editsubscribe")]
        public async Task<IActionResult> EditSubscribe([FromBody] Subscriber s)
        {
            var decodeEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s.Email));
            s.Email = decodeEmail;
            var check = await _service.EditSubscriber(s);
            if (!check)
            {
                return BadRequest("Không có khách hàng nào tương ứng.");
            }
            return Ok("Đã cập nhật trạng thái nhận thông báo qua email.");
        }

        [HttpGet("SubUser")]
        public async Task<IActionResult> GetAllSubUser()
        {
            var cus = await _service.GetSubcribeCustomerList();
            return Ok(cus);
        }
    }
}
