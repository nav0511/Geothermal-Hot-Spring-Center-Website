using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class CheckInModel : PageModel
    {
        private TicketDetailService _ticketDetailService = new TicketDetailService();

        public int TicketId { get; set; }
        public decimal TotalAmount { get; set; } 

        public List<TicketDetailDTO> TicketDetails { get; set; } = new List<TicketDetailDTO>();

        public async Task OnGetAsync(int id)
        {
            TicketId = id;

            var (statusCode, ticketDetails) = await _ticketDetailService.GetBookingDetailsById(id);


            TicketDetails = ticketDetails.ToList();
            TotalAmount = TicketDetails.Sum(td => td.Price * td.Quantity);
        }
    }
}
