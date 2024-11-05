using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Admin
{
    public class ReservationListModel : PageModel
    {
        private TicketService _ticketService = new TicketService();

        public List<TicketDTO> TicketDTOs { get; set; } = new List<TicketDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task OnGet(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            int totalNewsCount = _ticketService.GetTotalBooking().Result;
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            TicketDTOs = await _ticketService.GetBookingList($"Ticket?$top={PageSize}&$skip={skip}");
        }

    }
}
