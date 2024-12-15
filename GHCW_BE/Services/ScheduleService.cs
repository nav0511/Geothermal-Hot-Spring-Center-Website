using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class ScheduleService
    {
        private readonly GHCWContext _context;
        private readonly IMapper _mapper;

        public ScheduleService(GHCWContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ScheduleDTO>?> GetWeeklySchedule(DateTime startDate, DateTime endDate)
        {
            var schedules = await _context.Schedules
                                 .Where(s => s.Date >= startDate && s.Date <= endDate && s.Receptionist.IsActive)
                                 .Include(s => s.Receptionist)
                                 .ToListAsync();
            if (schedules != null)
            {
                var result = _mapper.Map<List<ScheduleDTO>>(schedules);
                return result;
            }
            return null;
        }

        public async Task<ScheduleDTO?> GetScheduleById(int id)
        {
            var schedule = await _context.Schedules
                                 .Include(s => s.Receptionist)
                                 .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule != null)
            {
                var result = _mapper.Map<ScheduleDTO>(schedule);
                return result;
            }
            return null;
        }

        public async Task<(bool isSuccess, string message)> UpdateSchedule(EditScheduleRequest er)
        {
            var checkExistSchedule = await _context.Schedules.FindAsync(er.Id);
            if (checkExistSchedule != null)
            {
                var existingScheduleForDateShift = await _context.Schedules
                    .Where(s => s.Date == er.Date && s.Shift == er.Shift && s.Id != er.Id)
                    .FirstOrDefaultAsync();
                if (existingScheduleForDateShift != null)
                {
                    return (false, "Ca này đã có lễ tân làm việc rồi.");
                }
                try
                {
                    _mapper.Map(er, checkExistSchedule);
                    _context.Schedules.Update(checkExistSchedule);
                    await _context.SaveChangesAsync();
                    return (true, "Cập nhật thông tin thành công.");
                }
                catch (Exception)
                {
                    return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
                }
            }
            return (false, "Không tồn tại lịch này.");
        }

        public async Task<(bool isSuccess, string message)> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return (false, "Lịch làm việc không tồn tại.");
            }
            try
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                return (true, "Xóa lịch làm việc thành công.");
            }
            catch (Exception)
            {
                return (false, "Xóa lịch làm việc thất bại, vui lòng thử lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> AddSchedule(AddScheduleRequest ar)
        {
            var existSchedule = await _context.Schedules.AnyAsync(s => s.Date == ar.Date && s.Shift == ar.Shift);
            if (existSchedule)
            {
                return (false, "Đã có lịch làm việc vào thời gian này.");
            }
            try
            {
                var schedule = _mapper.Map<AddScheduleRequest, Schedule>(ar);
                await _context.Schedules.AddAsync(schedule);
                await _context.SaveChangesAsync();
                return (true, "Thêm lịch làm việc mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Đã xảy ra lỗi trong quá trình thêm lịch làm việc mới.");
            }
        }
    }
}
