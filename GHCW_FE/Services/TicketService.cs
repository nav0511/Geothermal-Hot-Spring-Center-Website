using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace GHCW_FE.Services
{
    public class TicketService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<TicketDTO>?)> GetBookingList(string url, int? role, int? uId)
        {
            if (role.HasValue)
            {
                url += $"?role={role.Value}&uId={uId.Value}";
            }
            
            return await GetData<List<TicketDTO>>(url);
        }
        public async Task<(HttpStatusCode StatusCode, int Total)> GetTotalBooking(int? role, int? uId)
        {
            string url = "Ticket/Total";
            if (role.HasValue)
            {
                url += $"?role={role.Value}&uId={uId.Value}";
            }
            return await GetData<int>(url);
        }

        public async Task<HttpStatusCode> UpdateCheckinStatus(TicketDTO2 ticket)
        {
            string url = "Ticket/Update-Checkin";
            return await PutData(url, ticket);
        }

        public async Task<HttpStatusCode> SaveTicketAsync(TicketDTOForPayment ticket, string accessToken)
        {
            string url = "Ticket/Save";

            return await PushData(url, ticket, null, accessToken);
        }

        //Ticket Detail
        public async Task<(HttpStatusCode StatusCode, List<TicketDetailDTO>?)> GetBookingDetailsById(int id)
        {
            string url = $"Ticket/TicketDetail/{id}";
            return await GetData<List<TicketDetailDTO>>(url);
        }
    }
}
