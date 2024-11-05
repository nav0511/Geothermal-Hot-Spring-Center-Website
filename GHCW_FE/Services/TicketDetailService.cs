using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class TicketDetailService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<TicketDetailDTO>?)> GetBookingDetailsById(int id)
        {
            string url = $"TicketDetail/{id}";
            return await GetData<List<TicketDetailDTO>>(url);
        }
    }
}
