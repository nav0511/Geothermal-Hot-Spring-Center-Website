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

        public async Task<HttpStatusCode> UpdateSchedule(EditScheduleRequest er, string accessToken)
        {
            var statusCode = await PutData<EditScheduleRequest>("Schedule/UpdateSchedule", er, null, accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, ScheduleDTO? Schedule)> GetScheduleByID(int id)
        {
            var (statusCode, schedule) = await GetData<ScheduleDTO>($"Schedule/{id}");
            return (statusCode, schedule);
        }

        public async Task<HttpStatusCode> DeleteSchedule(int id)
        {
            var statusCode = await DeleteData($"Schedule/{id}");
            return statusCode;
        }

        public async Task<HttpStatusCode> AddSchedule(AddScheduleRequest ar, string accessToken)
        {
            var statusCode = await PushData<AddScheduleRequest>("Schedule/AddSchedule", ar, null, accessToken);
            return statusCode;
        }
    }
}
