using GHCW_FE.DTOs;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GHCW_FE.Services
{
    public class ScheduleService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<ScheduleDTO>? Schedule)> GetWeeklySchedule(ScheduleByWeek sw)
        {
            var queryString = $"Schedule/Weekly?startDate={sw.StartDate:yyyy-MM-dd}&endDate={sw.EndDate:yyyy-MM-dd}";
            var (statusCode, Schedules) = await GetData<List<ScheduleDTO>>(queryString);
            return (statusCode, Schedules);
        }

        public async Task<HttpStatusCode> UpdateSchedule(ScheduleDTO schedule)
        {
            string url = $"Schedule/{schedule.Id}";
            return await PutData(url, schedule);
        }

        public async Task<HttpStatusCode> DeleteSchedule(int id)
        {
            string url = $"Schedule/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateSchedule(ScheduleDTO schedule)
        {
            string url = "Schedule";
            return await PushData(url, schedule);
        }
    }
}
