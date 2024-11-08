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
                                 .Where(s => s.Date >= startDate && s.Date < endDate && s.IsActive == true)
                                 .Include(s => s.Receptionist)
                                 .ToListAsync();
            if (schedules != null)
            {
                var result = _mapper.Map<List<ScheduleDTO>>(schedules);
                return result;
            }
            return null;
        }

        public async Task<Schedule?> GetScheduleById(int id)
        {
            return await _context.Schedules
                                 .Include(s => s.Receptionist)
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSchedule(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSchedule(Schedule schedule)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task AddSchedule(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
