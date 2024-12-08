using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ScheduleService _scheduleService;

        public ScheduleController(IMapper mapper, ScheduleService scheduleService)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
        }

        [Authorize]
        [HttpGet("Weekly")]
        public async Task<IActionResult> GetWeeklySchedule([FromQuery] ScheduleByWeek sw)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && (int.Parse(roleClaim.Value) <= 1 || int.Parse(roleClaim.Value) == 4))
            {
                var schedules = await _scheduleService.GetWeeklySchedule(sw.StartDate, sw.EndDate);
                if (schedules == null)
                {
                    return NotFound("Không tìm thấy lịch làm việc nào trong tuần này.");
                }
                return Ok(schedules);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var schedule = await _scheduleService.GetScheduleById(id);
                if (schedule == null)
                {
                    return NotFound("Không tìm thấy lịch tương ứng");
                }
                return Ok(schedule);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpPost("AddSchedule")]
        public async Task<IActionResult> CreateSchedule(AddScheduleRequest ar)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _scheduleService.AddSchedule(ar);
                if (!isSuccess)
                {
                    if (message.Contains("Đã có lịch làm việc"))
                    {
                        return Conflict(message);
                    }
                    return StatusCode(500, message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [Authorize]
        [HttpPut("UpdateSchedule")]
        public async Task<IActionResult> UpdateSchedule(EditScheduleRequest er)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");

            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _scheduleService.UpdateSchedule(er);
                if (!isSuccess)
                {
                    if (message.Contains("Ca này đã có lễ tân làm việc"))
                    {
                        return Conflict(message);
                    }
                    if (message.Contains("Không tồn tại lịch này"))
                    {
                        return NotFound(message);
                    }
                    return StatusCode(500, message);
                }
                return Ok(message);
            }
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền thực hiện hành động này.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var roleClaim = identity?.FindFirst("Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) <= 1)
            {
                var (isSuccess, message) = await _scheduleService.DeleteSchedule(id);
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
