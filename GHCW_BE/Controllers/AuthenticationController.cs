using AutoMapper;
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
                    Role = 5,
                    IsEmailNotify = true
                };
                var encodedEmail = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(a.Email));
                var encodedActivationCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(a.ActivationCode));
                var activationLink = $"https://localhost:7260/Authentications/EmailActivation?email={encodedEmail}&code={encodedActivationCode}";

                var emailSent = await _service.SendActivationEmail(registerDTO, activationLink);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email kích hoạt.");
                }

                await _service.Register(a);

                var existCustomer = await _service.CheckPhoneExsit(a.PhoneNumber);
                if (existCustomer != null)
                {
                    existCustomer.Email = a.Email;
                    existCustomer.FullName = a.Name;
                    existCustomer.AccountId = a.Id;
                    var (isSuccess,message) = await _service.EditCustomer(existCustomer);
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
                        AccountId = a.Id
                    };
                    var (isSuccess, message) = await _service.AddNewCustomer(addCustomer);
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
            var a = await _service.CheckActiveStatus(loginDTO.Email);
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
            string rawUserID = HttpContext.User.FindFirstValue("ID");

            if (!int.TryParse(rawUserID, out int id))
            {
                return Unauthorized("ID không hợp lệ.");
            }

            var a = await _service.GetUserProfileById(id);
            if (a == null)
            {
                return NotFound("không tìm thấy tài khoản tương ứng.");
            }
            await _service.DeleteRefToken(a);
            return Ok("Đăng xuất thành công.");
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
            var user = await _service.CheckAccountExsit(email.Email);

            if (user != null)
            {
                var newPass = await _helper.GeneratePassword(16);
                user.Password = _helper.HashPassword(newPass);
                var emailSent = await _service.SendNewPasswordEmail(email.Email, newPass);
                if (!emailSent)
                {
                    return StatusCode(500, "Không thể gửi email đặt lại mật khẩu.");
                }
                await _service.ChangePassword(user.Id,user.Password);
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
            var user = await _service.GetUserProfileById(cp.Id);
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

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem hồ sơ của người dùng khác.");
            }
            var user = await _service.GetUserProfileById(int.Parse(userIdClaim.Value));

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng này");
            }

            return Ok(user);
        }

        [Authorize]
        [HttpGet("userlist")]
        public async Task<IActionResult> GetUserList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                var acc = await _service.GetUserList();
                if (acc == null)
                {
                    return NotFound("Danh sách người dùng trống");
                }
                return Ok(acc);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin này.");
        }

        [Authorize]
        [HttpGet("employeelist")]
        public async Task<IActionResult> GetEmployeeList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var acc = await _service.GetEmployeeList();
                if (acc == null)
                {
                    return NotFound("Danh sách nhân viên trống");
                }

                return Ok(acc);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin này.");
        }

        [Authorize]
        [HttpGet("customerAcclist")]
        public async Task<IActionResult> GetCustomerAccList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 4)
            {
                var acc = await _service.GetCustomerAccountList();
                if (acc == null)
                {
                    return NotFound("Danh sách khách hàng trống");
                }
                return Ok(acc);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin này.");
        }

        [Authorize]
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetUserProfileById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.TryParse(roleClaim.Value, out int userRole))
            {
                var requestedAcc = await _service.GetUserProfileById(id);
                if (requestedAcc == null)
                {
                    return NotFound("Tài khoản không tồn tại.");
                }
                switch (userRole)
                {
                    case 0: // Vai trò 0 có quyền xem tất cả tài khoản
                        return Ok(requestedAcc);

                    case 1: // Vai trò 1 có quyền xem tất cả trừ các tài khoản có vai trò 0
                        if (requestedAcc.Role != 0)
                        {
                            return Ok(requestedAcc);
                        }
                        return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin tài khoản này.");

                    case 2:
                    case 3:
                    case 4: // Vai trò 2, 3 và 4 chỉ có quyền xem tài khoản có vai trò 5
                        if (requestedAcc.Role == 5)
                        {
                            return Ok(requestedAcc);
                        }
                        return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin tài khoản này.");

                    default:
                        return BadRequest("Vai trò của bạn không được hỗ trợ.");
                }
            }
            return BadRequest("Không thể lấy role của tài khoản đăng nhập, vui lòng kiểm tra lại.");
        }

        [Authorize]
        [HttpPost("adduser")]
        public async Task<IActionResult> AddNewUser(AddRequest a)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                var checkAccExist = await _service.CheckAccountExsit(a.Email);
                if (checkAccExist != null)
                {
                    return Conflict("Email đã được sử dụng, vui lòng dùng email khác để đăng ký");
                }
                a.Password = _helper.HashPassword(a.Password);
                var (isSuccess, message) = await _service.AddNewUser(a);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpPut("edituser")]
        public async Task<IActionResult> EditUser(EditRequest r)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                var checkExistCustomer = await _service.CheckCustomerExsit(r.Id);
                if (checkExistCustomer != null)
                {
                    checkExistCustomer.FullName = r.Name;
                    checkExistCustomer.PhoneNumber = r.PhoneNumber;

                    var (isSuccess, message) = await _service.EditCustomer(checkExistCustomer);
                    if (isSuccess)
                    {
                        (isSuccess, message) = await _service.EditProfile(r);
                        if (!isSuccess)
                        {
                            return BadRequest(message);
                        }
                        return Ok(message);
                    }
                    return BadRequest(message);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpPut("updateprofile")]
        public async Task<IActionResult> UpdateProfile(UpdateRequest r)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim != null || userIdClaim?.Value == r.Id.ToString())
            {
                var checkExistCustomer = await _service.CheckCustomerExsit(r.Id);
                if (checkExistCustomer != null)
                {
                    checkExistCustomer.FullName = r.Name;
                    checkExistCustomer.PhoneNumber = r.PhoneNumber;
                    var (isSuccess, message) = await _service.EditCustomer(checkExistCustomer);
                    if (isSuccess)
                    {
                        (isSuccess, message) = await _service.UpdateProfile(r);
                        if (!isSuccess)
                        {
                            return BadRequest(message);
                        }
                        return Ok(message);
                    }
                    return BadRequest(message);
                }
                else if (checkExistCustomer == null && roleClaim != null && int.Parse(roleClaim.Value) != 5)
                {
                    var (isSuccess, message) = await _service.UpdateProfile(r);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    return Ok(message);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền cập nhật hồ sơ của người dùng khác.");
        }

        [Authorize]
        [HttpDelete("useractivation")]
        public async Task<IActionResult> UserActivation(int uid)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                var (isSuccess, message) = await _service.UserActivation(uid);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }

                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpGet("bookinghistory")]
        public async Task<IActionResult> BookingList(int uid)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim != null || userIdClaim?.Value == uid.ToString())
            {
                var bookings = await _service.GetBookingListById(uid);
                if (bookings == null)
                {
                    return NotFound("Danh sách đặt vé trống.");
                }
                return Ok(bookings);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem lịch sử của người dùng khác.");
        }

        [Authorize]
        [HttpPost("CustomerList")]
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
                var checkAccExist = await _service.CheckAccountExsit(a.Email);
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

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                UpdateRequest ur = new UpdateRequest()
                {
                    Id = c.AccountId ?? 0,
                    Name = c.FullName,
                    PhoneNumber = c.PhoneNumber
                };
                var (isSuccess, message) = await _service.UpdateProfile(ur);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                (isSuccess, message) = await _service.EditCustomer(c);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }
    }
}
