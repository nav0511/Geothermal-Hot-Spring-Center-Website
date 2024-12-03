using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using GHCW_BE.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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
        private readonly CustomerService _cusService;
        private readonly AccountService _accountService;

        public AuthenticationController(AuthenticationService service, Helper helper, CustomerService cusService, AccountService accountService)
        {
            _service = service;
            _helper = helper;
            _cusService = cusService;
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                var checkAccExist = await _accountService.CheckAccountExsit(registerDTO.Email);
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
                    Role = 5,
                    Gender = registerDTO.Gender
                };
                var encodedEmail = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(a.Email));
                var encodedActivationCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(a.ActivationCode));
                var activationLink = _service.CreateValidationLink(encodedEmail, encodedActivationCode);

                var emailSent = await _service.SendActivationEmail(registerDTO, activationLink);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email kích hoạt.");
                }

                await _service.Register(a);

                var existCustomer = await _cusService.CheckCustomerExsit(a.Email);
                if (existCustomer != null)
                {
                    existCustomer.FullName = a.Name;
                    existCustomer.AccountId = a.Id;
                    existCustomer.IsEmailNotify = true;
                    existCustomer.Gender = a.Gender;
                    var (isSuccess, message) = await _cusService.EditCustomer(existCustomer);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    return Ok("Đăng ký thành công, vui lòng kiểm tra email để kích hoạt tài khoản!");
                }
                else
                {
                    var addCustomer = new AddCustomerRequest()
                    {
                        FullName = a.Name,
                        Email = a.Email,
                        PhoneNumber = a.PhoneNumber,
                        AccountId = a.Id,
                        IsEmailNotify = true,
                        Gender = a.Gender,
                    };
                    var (isSuccess, message) = await _cusService.AddNewCustomer(addCustomer);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    return Ok("Đăng ký thành công, vui lòng kiểm tra email để kích hoạt tài khoản!");
                }
            }
            return BadRequest("Thông tin đăng ký không hợp lệ.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Thông tin đăng nhập không hợp lệ.");
            }
            var a = await _accountService.CheckActiveStatus(loginDTO.Email);
            if (a == null)
            {
                return BadRequest("Email hoặc mật khẩu không đúng. Vui lòng kiểm tra lại");
            }

            bool isCorrectPass = _helper.VerifyPassword(loginDTO.Password, a.Password);
            if (!isCorrectPass)
            {
                return BadRequest("Email hoặc mật khẩu không đúng. Vui lòng kiểm tra lại");
            }

            string token = await _service.Login(a);
            string refToken = await _service.RefTokenGenerator();
            a.RefreshToken = refToken;
            var (isSuccess, message) = await _service.UpdateRefreshToken(a);
            if (!isSuccess)
            {
                return BadRequest(message);
            }
            return Ok(new LoginResponse()
            {
                AccessToken = token,
                RefreshToken = refToken,
                Message = "Đăng nhập thành công"
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Refresh token không hợp lệ.");
            }

            var a = await _service.GetAccountByRefreshToken(request.RefreshToken);
            if (a == null)
            {
                return NotFound("Refresh token không hợp lệ hoặc hết hạn.");
            }

            await _service.DeleteRefToken(a);
            bool isValidatedRefToken = _service.Validate(request.RefreshToken);
            if (!isValidatedRefToken)
            {
                return BadRequest("Refresh token không hợp lệ.");
            }

            string token = await _service.Login(a);
            string refToken = await _service.RefTokenGenerator();
            a.RefreshToken = refToken;
            var (isSuccess, message) = await _service.UpdateRefreshToken(a);
            if (!isSuccess)
            {
                return BadRequest(message);
            }
            return Ok(new LoginResponse()
            {
                AccessToken = token,
                RefreshToken = refToken,
                Message = "Tạo refresh token mới thành công."
            });
        }

        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim != null)
            {
                var a = await _accountService.GetUserProfileById(int.Parse(userIdClaim.Value));
                if (a != null)
                {
                    if (a.Id.ToString() == userIdClaim.Value)
                    {
                        await _service.DeleteRefToken(a);
                        return Ok("Đăng xuất thành công.");
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
                    }
                }
                return NotFound("Không tìm thấy tài khoản tương ứng.");
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn phải đăng nhập để thực hiện tính năng này.");
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivationCode ac)
        {
            var decodeEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ac.Email));
            var decodeActivationCode = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ac.Code));
            ac.Email = decodeEmail;
            ac.Code = decodeActivationCode;
            var check = await _service.RedemActivationCode(ac);
            if (!check)
            {
                return BadRequest("Tài khoản chưa được kích hoạt.");
            }
            return Ok("Tài khoản của bạn đã được kích hoạt. Bạn có thể đăng nhập.");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassRequest email)
        {
            var user = await _accountService.CheckAccountExsit(email.Email);

            if (user != null)
            {
                var newPass = await _helper.GeneratePassword(16);
                user.Password = _helper.HashPassword(newPass);
                var emailSent = await _service.SendNewPasswordEmail(email.Email, newPass);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email đặt lại mật khẩu.");
                }
                await _service.ChangePassword(user.Id, user.Password);
                return Ok("Gửi thành công, vui lòng kiểm tra email để lấy tài khoản mới của bạn!");
            }
            return BadRequest("Không có tài khoản nào khớp với email đã nhập");
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePassRequest cp)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim == null || userIdClaim.Value != cp.Id.ToString())
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền đổi mật khẩu của người dùng khác.");
            }
            var user = await _accountService.GetUserProfileById(cp.Id);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }
            if (!_helper.VerifyPassword(cp.OldPassword, user.Password))
            {
                return Conflict("Mật khẩu cũ không chính xác.");
            }

            var success = await _service.ChangePassword(cp.Id, _helper.HashPassword(cp.NewPassword));
            if (success)
            {
                return Ok("Đổi mật khẩu thành công");
            }
            return BadRequest("Đổi mật khẩu thất bại.");
        }
    }
}
