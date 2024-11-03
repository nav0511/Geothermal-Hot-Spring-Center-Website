using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class ScheduleService
    {
        private readonly GHCWContext _context;

        public ScheduleService(GHCWContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetWeeklySchedule(DateTime startDate)
        {
            var endDate = startDate.AddDays(7); 
            return await _context.Schedules
                                 .Where(s => s.Date >= startDate && s.Date < endDate)
                                 .Include(s => s.Receptionist) 
                                 .ToListAsync();
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
