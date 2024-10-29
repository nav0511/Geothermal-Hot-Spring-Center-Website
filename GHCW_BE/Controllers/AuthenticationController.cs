using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _service;
        private readonly Helper _helper;

        public AuthenticationController(AuthenticationService service, Helper helper)
        {
            _service = service;
            _helper = helper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                var checkAccExist = await _service.CheckAccountExsit(registerDTO.Email);
                if (checkAccExist != null)
                {
                    return Conflict("Email đã được sử dụng, vui lòng dùng email khác để đăng ký");
                }

                var activeCode = await _helper.GenerateVerificationCode(6);
                Account a = new()
                {
                    Email = registerDTO.Email,
                    Password = _helper.HashPassword(registerDTO.Password),
                    Name = registerDTO.FullName,
                    PhoneNumber = registerDTO.PhoneNumber,
                    IsActive = false,
                    ActivationCode = activeCode,
                    Role = 5
                };
                var activationLink = $"https://localhost:7226/api/Authentication/activate/{a.Email}/{a.ActivationCode}";

                var emailSent = await _service.SendActivationEmail(registerDTO, activationLink);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email kích hoạt.");
                }

                await _service.Register(a);
                return Ok("Đăng ký thành công, vui lòng kiểm tra email để kích hoạt tài khoản!");
            }
            return BadRequest("Thông tin đăng ký không hợp lệ.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Mời nhập tên đăng nhập và mật khẩu");
            }
            var a = await _service.CheckActiveStatus(loginDTO.Email);
            if (a == null)
            {
                return Unauthorized("Email hoặc mật khẩu không đúng. Vui lòng kiểm tra lại");
            }
            
            bool isCorrectPass = _helper.VerifyPassword(loginDTO.Password, a.Password);
            if (!isCorrectPass)
            {
                return Unauthorized("Email hoặc mật khẩu không đúng. Vui lòng kiểm tra lại");
            }

            string token = await _service.Login(a);
            string refToken = await _service.RefTokenGenerator();
            a.RefreshToken = refToken;
            await _service.UpdateProfile(a);
            return Ok(new LoginResponse()
            {
                AccessToken = token,
                RefreshToken = refToken
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("");
            }

            var a = await _service.GetAccountByRefreshToken(request.RefreshToken);
            if (a == null)
            {
                return NotFound("Invalid or expired refresh token.");
            }

            await _service.DeleteRefToken(a);
            bool isValidatedRefToken = _service.Validate(request.RefreshToken);
            if (!isValidatedRefToken)
            {
                return BadRequest("Invalid Refresh token");
            }

            string token = await _service.Login(a);
            string refToken = await _service.RefTokenGenerator();
            a.RefreshToken = refToken;
            await _service.UpdateProfile(a);
            return Ok(new LoginResponse()
            {
                AccessToken = token,
                RefreshToken = refToken
            });
        }

        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            string rawUserID = HttpContext.User.FindFirstValue("ID");

            if (!int.TryParse(rawUserID, out int id))
            {
                return Unauthorized();
            }

            var a = await _service.GetUserProfileById(id);
            await _service.DeleteRefToken(a);
            return Ok();
        }

        [HttpPost("activate/{email}/{code}")]
        public async Task<IActionResult> ActivateAccount(string email, string code)
        {
            var activeCode = new ActivationCode()
            {
                Email = email,
                Code = code
            };
            var check = await _service.RedemActivationCode(activeCode);
            if (!check)
            {
                return BadRequest("Không có gì xảy ra");
            }
            return Ok("Tài khoản của bạn đã được kích hoạt. Bạn có thể đăng nhập.");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _service.CheckAccountExsit(email);

            if (user != null)
            {
                var newPass = await _helper.GeneratePassword(16);
                user.Password = _helper.HashPassword(newPass);
                var emailSent = await _service.SendNewPasswordEmail(email, newPass);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email đặt lại mật khẩu.");
                }
                await _service.UpdateProfile(user);
                return Ok("Gửi thành công, vui lòng kiểm tra email để lấy tài khoản mới của bạn!");
            }
            return BadRequest("Không có tài khoản nào khớp với email đã nhập");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required.");

            int userIdHeader = _helper.GetIdInHeader(token);
            var user = await _service.GetUserProfileById(userIdHeader);

            if (user == null)
                return NotFound("Không tìm thấy người dùng này");

            return Ok(user);
        }

        [Authorize]
        [HttpGet("userlist")]
        public async Task<IActionResult> GetUserList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var roleClaim = identity.FindFirst("Role");

                if (roleClaim != null && roleClaim.Value == "0")
                {
                    var acc = await _service.GetUserList();
                    return Ok(acc);
                }
            }

            return BadRequest("Bạn không có quyền truy cập vào danh sách người dùng");
        }

        [Authorize]
        [HttpGet("employeelist")]
        public async Task<IActionResult> GetEmployeeList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var roleClaim = identity.FindFirst("Role");

                if (roleClaim != null && roleClaim.Value == "0" || roleClaim.Value == "1")
                {
                    var acc = await _service.GetEmployeeList();
                    return Ok(acc);
                }
            }

            return BadRequest("Bạn không có quyền truy cập vào danh sách nhân viên");
        }

        [Authorize]
        [HttpGet("customerlist")]
        public async Task<IActionResult> GetCustomerList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var roleClaim = identity.FindFirst("Role");

                if (roleClaim != null && roleClaim.Value == "0" || roleClaim.Value == "1" || roleClaim.Value == "2" || roleClaim.Value == "3")
                {
                    var acc = await _service.GetCustomerList();
                    return Ok(acc);
                }
            }
            return BadRequest("Bạn không có quyền truy cập vào danh sách khách hàng");
        }

        [Authorize]
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetUserProfileById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var roleClaim = identity.FindFirst("Role");

                if (roleClaim != null)
                {
                    switch (int.Parse(roleClaim.Value))
                    {
                        case 0:
                            var acc = await _service.GetUserProfileById(id);
                            return Ok(acc);
                        case 1:
                            var em = await _service.GetEmployeeProfileById(id);
                            return Ok(em);
                        case 2:
                            var cus = await _service.GetCustomerProfileById(id);
                            return Ok(cus);
                        case 3:
                            var cust = await _service.GetCustomerProfileById(id);
                            return Ok(cust);
                        default:
                            return BadRequest("Bạn không có quyền xem thông tin người dùng này");
                    }
                }
            }
            return BadRequest("Bạn phải đăng nhập để sử dụng tính năng này");
        }

        [Authorize]
        [HttpPost("adduser")]
        public async Task<IActionResult> AddNewUser(Account a)
        {

        }

        [Authorize]
        [HttpPut("edituser")]
        public async Task<IActionResult> EditUser(Account a)
        {

        }

        [Authorize]
        [HttpDelete("useractivation")]
        public async Task<IActionResult> UserActivation(int uid)
        {

        }
    }
}
