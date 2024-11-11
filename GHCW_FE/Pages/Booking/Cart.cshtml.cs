using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Net;

namespace GHCW_FE.Pages.Booking
{
    [BindProperties]
    public class CartModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly VnPayService _vnPayService;
        private readonly TicketService _ticketService;

        public CartModel(ServicesService servicesService, VnPayService vnPayService, TicketService ticketService)
        {
            _servicesService = servicesService;
            _vnPayService = vnPayService;
            _ticketService = ticketService;
        }
        public PaymentInformationDTO PaymentInfo { get; set; }
        public string BookingDate { get; set; }
        public string Message { get; set; }
        public bool Success {  get; set; }
        public bool isLoggedIn { get; set; }
        public string ErrorMessage { get; set; }
        public string CartData { get; set; }

        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();

        public async Task OnGet(string bookingDate)
        {
            (HttpStatusCode StatusCode, List<ServiceDTO>? ListServices) = await _servicesService.GetServices($"Service");
            AvailableServices = ListServices;
            BookingDate = bookingDate;

            var user = HttpContext.Session.GetString("acc");
            if (user == null)
            {
                isLoggedIn = false;
                ErrorMessage = "Quý khách cần đăng nhập để thanh toán đơn hàng";
            }
            else
            {
                isLoggedIn = true;
            }
        }

        public async Task<IActionResult> OnPostInitiatePaymentAsync()
        {
            var user = JsonConvert.DeserializeObject<AccountDTO>(HttpContext.Session.GetString("acc"));
            PaymentInfo.OrderType = "Vé dịch vụ";
            PaymentInfo.OrderDescription = "Thanh toán vé dịch vụ";
            PaymentInfo.Name = user.Name;
            var paymentUrl = _vnPayService.CreatePaymentUrl(PaymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }
        public async Task OnGetPaymentCallbackAsync()
        {
            var collections = HttpContext.Request.Query;  // Get the query from the request
            var paymentResponse = _vnPayService.PaymentExecute(collections);

            if (paymentResponse.VnPayResponseCode == "00")
            {
                Message = "Thanh toán thành công";
                Success = true;
            }
            else
            {
                Message = "Thanh toán không thành công";
                Success= false;
            }

        }
        public async Task OnPostSaveTicketAsync()
        {
            var cart = JsonConvert.DeserializeObject<List<CartItemDTO>>(CartData);
            var user = JsonConvert.DeserializeObject<AccountDTO>(HttpContext.Session.GetString("acc"));

            if (user == null || cart == null || cart.Count == 0)
            {
                Message = "Có lỗi xảy ra, xin quý khách vui lòng thử lại sau";
            }
            else
            {
                var newTicket = new TicketDTOForPayment
                {
                    CustomerId = user.Id,
                    Total = cart.Sum(item => item.Quantity * item.Price),
                    OrderDate = DateTime.Now,
                    BookDate = DateTime.Parse(BookingDate),
                    PaymentStatus = 1,
                    CheckIn = 0,
                    TicketDetails = cart.Select(item => new TicketDetailDTOForPayment
                    {
                        ServiceId = item.ServiceId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                await _ticketService.SaveTicketAsync(newTicket);

            }
        }


    }
}
