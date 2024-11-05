using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace GHCW_FE.Services
{
    public class TicketService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<TicketDTO>?)> GetBookingList(string url)
        {
            return await GetData<List<TicketDTO>>(url);
        }
        public async Task<(HttpStatusCode StatusCode, int Total)> GetTotalBooking()
        {
            string url = "Ticket/Total";
            return await GetData<int>(url);
        }
    }
}
