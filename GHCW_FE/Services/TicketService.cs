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

        public async Task<HttpStatusCode> UpdateCheckinStatus(TicketDTO2 ticket)
        {
            string url = "Ticket/Update-Checkin";
            return await PutData(url, ticket);
        }

        public async Task<HttpStatusCode> SaveTicketAsync(TicketDTOForPayment ticket)
        {
            string url = "Ticket/Save";

            return await PushData(url, ticket);
        }
    }
}
