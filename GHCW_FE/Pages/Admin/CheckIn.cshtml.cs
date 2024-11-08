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
        private DiscountService _discountService = new DiscountService();

        public int TicketId { get; set; }
        public decimal TotalAmount { get; set; } 

        public List<TicketDetailDTO> TicketDetails { get; set; } = new List<TicketDetailDTO>();
        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

        public async Task OnGetAsync(int id)
        {
            TicketId = id;

            var (statusCode, ticketDetails) = await _ticketDetailService.GetBookingDetailsById(id);
            var (statusCode1, discountList) = await _discountService.GetDiscounts("Discount");

            TicketDetails = ticketDetails.ToList();
            DiscountDTOs = discountList.ToList();

            var discountCode = TicketDetails.FirstOrDefault()?.Ticket?.DiscountCode;

            var discount = discountList.FirstOrDefault(d => d.Code == discountCode);

            int discountValue = 0;
            if (discount != null)
            {
                discountValue = discount?.Value ?? 0;
            }

            TotalAmount = TicketDetails.Sum(td => td.Price * td.Quantity);

            if (discount != null)
            {
                if (discount.Value > 0)
                {
                    TotalAmount -= TotalAmount * discountValue / 100;
                }
            }
        }
    }
}
