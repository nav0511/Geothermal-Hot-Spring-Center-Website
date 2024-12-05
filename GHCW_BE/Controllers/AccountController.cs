using GHCW_BE.DTOs;
using GHCW_BE.Services;
using GHCW_BE.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _service;
        private readonly CustomerService _cusService;
        private readonly Helper _helper;
        private readonly AuthenticationService _authService;

        public AccountController(AccountService service, Helper helper, CustomerService cusService, AuthenticationService authService)
        {
            _service = service;
            _helper = helper;
            _cusService = cusService;
            _authService = authService;
        }

        [Authorize]
        [EnableQuery]
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
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (userIdClaim == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Bạn phải đăng nhập để thực hiện tính năng này.");
            }
            var user = await _service.GetUserProfileById(int.Parse(userIdClaim.Value));
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng này");
            }
            if (user.Id.ToString() == userIdClaim.Value)
            {
                return Ok(user);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem hồ sơ của người dùng khác.");
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
                if (roleClaim != null && int.Parse(roleClaim.Value) == 5)
                {
                    var checkExistCustomer = await _cusService.GetCustomerProfileById(r.Id);
                    if (checkExistCustomer != null)
                    {
                        checkExistCustomer.Name = r.Name;
                        checkExistCustomer.PhoneNumber = r.PhoneNumber;
                        checkExistCustomer.Address = r.Address;
                        checkExistCustomer.DoB = r.DoB;
                        checkExistCustomer.Gender = r.Gender;
                        var (isSuccess, message) = await _cusService.EditCustomer(checkExistCustomer);
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
                }
                else
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
        [EnableQuery]
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
        [HttpGet("receptionlist")]
        public async Task<IActionResult> GetReceptionList()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var acc = await _service.GetReceptionList();
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
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserProfileByEmail(string email)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) != 5)
            {
                var requestedAcc = await _service.CheckAccountExsit(email);
                if (requestedAcc == null)
                {
                    return NotFound("Tài khoản không tồn tại.");
                }
                return Ok(requestedAcc);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin tài khoản này.");
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
            else if (roleClaim != null && int.Parse(roleClaim.Value) == 1)
            {
                if (a.Role >= 2 && a.Role <= 4)
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
                else
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
                }
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
                var (isSuccess, message) = await _service.EditProfile(r);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                var checkExistCustomer = await _cusService.GetCustomerProfileByAccountId(r.Id);
                if (checkExistCustomer != null)
                {
                    checkExistCustomer.Name = r.Name;
                    checkExistCustomer.PhoneNumber = r.PhoneNumber;
                    checkExistCustomer.Address = r.Address;
                    checkExistCustomer.DoB = r.DoB;
                    checkExistCustomer.Gender = r.Gender;
                    (isSuccess, message) = await _cusService.EditCustomer(checkExistCustomer);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    return Ok(message);
                }
                return Ok(message);
            }
            else if (roleClaim != null && int.Parse(roleClaim.Value) == 1)
            {
                if (r.Role >= 2 && r.Role <= 4)
                {
                    var (isSuccess, message) = await _service.EditProfile(r);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    var checkExistCustomer = await _cusService.GetCustomerProfileById(r.Id);
                    if (checkExistCustomer != null)
                    {
                        checkExistCustomer.Name = r.Name;
                        checkExistCustomer.PhoneNumber = r.PhoneNumber;
                        checkExistCustomer.Address = r.Address;
                        checkExistCustomer.DoB = r.DoB;
                        checkExistCustomer.Gender = r.Gender;
                        (isSuccess, message) = await _cusService.EditCustomer(checkExistCustomer);
                        if (!isSuccess)
                        {
                            return BadRequest(message);
                        }
                        return Ok(message);
                    }
                    return Ok(message);
                }
                else
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }


        [Authorize]
        [HttpDelete("useractivation/{uid}")]
        public async Task<IActionResult> UserActivation(int uid)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");
            var user = await _service.GetUserProfileById(uid);

            if (roleClaim != null && int.Parse(roleClaim.Value) == 0)
            {
                var (isSuccess, message) = await _service.UserActivation(uid);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            else if (roleClaim != null && int.Parse(roleClaim.Value) == 1)
            {
                if (user != null && (user.Role >= 2 && user.Role <= 4))
                {
                    var (isSuccess, message) = await _service.UserActivation(uid);
                    if (!isSuccess)
                    {
                        return BadRequest(message);
                    }
                    return Ok(message);
                }
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }
    }
}
