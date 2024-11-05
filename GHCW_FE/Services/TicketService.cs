using GHCW_FE.DTOs;

namespace GHCW_FE.Services
{
    public class TicketService : BaseService
    {
        public async Task<List<TicketDTO>?> GetBookingList(string url)
        {
            return await GetData<List<TicketDTO>>(url);
        }
        public async Task<int> GetTotalBooking()
        {
            string url = "Ticket/Total";
            return await GetData<int>(url);
        }
    }
}
