﻿using GHCW_BE.DTOs;
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

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 4)
            {
                var acc = await _service.GetCustomerList();
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
                //if (a.AccountId  != null)
                //{
                //    var existAcc = await _service.GetUserProfileById(a.AccountId.Value);
                //}
                var checkAccExist = await _accountService.CheckAccountExsit(a.Email);
                if (checkAccExist != null)
                {
                    return Conflict("Email đã được sử dụng, vui lòng dùng email khác để đăng ký");
                }

                var (isSuccess, message) = await _service.AddNewCustomer(a);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
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
                        IsEmailNotify = c.IsEmailNotify,
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

    }
}