using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("Weekly")]
        public async Task<IActionResult> GetWeeklySchedule(DateTime startDate)
        {
            var schedules = await _scheduleService.GetWeeklySchedule(startDate);
            var result = _mapper.Map<List<ScheduleDTO>>(schedules);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await _scheduleService.GetScheduleById(id);
            if (schedule == null) return NotFound();
            var result = _mapper.Map<ScheduleDTO>(schedule);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleDTO scheduleDto)
        {
            if (scheduleDto == null) return BadRequest("Dữ liệu lịch không hợp lệ.");

            var schedule = _mapper.Map<Schedule>(scheduleDto);
            await _scheduleService.AddSchedule(schedule);
            return Ok("Thêm lịch thành công");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleDTO scheduleDto)
        {
            if (id != scheduleDto.Id) return BadRequest("ID không khớp.");

            var existingSchedule = await _scheduleService.GetScheduleById(id);
            if (existingSchedule == null) return NotFound();

            _mapper.Map(scheduleDto, existingSchedule);

            try
            {
                await _scheduleService.UpdateSchedule(existingSchedule);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi cập nhật lịch.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _scheduleService.GetScheduleById(id);
            if (schedule == null) return NotFound("Lịch không tồn tại.");

            try
            {
                await _scheduleService.DeleteSchedule(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa lịch: {ex.Message}");
            }

            return NoContent();
        }
    }
}
