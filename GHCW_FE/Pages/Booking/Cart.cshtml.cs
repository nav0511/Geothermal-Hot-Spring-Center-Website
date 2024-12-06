using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Booking
{
    [BindProperties]
    public class CartModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly VnPayService _vnPayService;
        private readonly TicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly DiscountService _discountService;

        public CartModel(ServicesService servicesService, VnPayService vnPayService, 
            TicketService ticketService, TokenService tokenService, 
            AuthenticationService authService, DiscountService discountService)
        {
            _servicesService = servicesService;
            _vnPayService = vnPayService;
            _ticketService = ticketService;
            _tokenService = tokenService;
            _authService = authService;
            _discountService = discountService;
        }
        public PaymentInformationDTO PaymentInfo { get; set; }
        public string BookingDate { get; set; }
        public string Message { get; set; }
        public bool Success {  get; set; }
        public bool IsLoggedIn { get; set; }
        public string ErrorMessage { get; set; }
        public string CartData { get; set; }
        [BindProperty]
        public string SelectedDiscountCode { get; set; }
        public bool HasTicketSaved { get; set; } = false;
        [BindProperty]
        public string PaymentMethod { get; set; }
        [BindProperty]
        public bool PayLater { get; set; } = false ;

        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();
        public List<DiscountDTO> AvailableDiscounts { get; set; } = new List<DiscountDTO>();

        public async Task<IActionResult> OnGet(string bookingDate)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && int.Parse(roleClaim.Value) != 5)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            (HttpStatusCode StatusCode, List<ServiceDTO>? ListServices) = await _servicesService.GetServices($"Service?$filter=IsActive eq true");
            AvailableServices = ListServices;
            BookingDate = bookingDate;

            (HttpStatusCode discountStatusCode, List<DiscountDTO>? Discounts) = await _discountService.GetDiscounts("Discount?$filter=IsAvailable eq true");
            if (discountStatusCode == HttpStatusCode.OK && Discounts != null)
            {
                AvailableDiscounts = Discounts;
            }

            var user = HttpContext.Session.GetString("acc");
            if (user == null)
            {
                IsLoggedIn = false;
                ErrorMessage = "Quý khách cần đăng nhập để thanh toán đơn hàng";
            }
            else
            {
                IsLoggedIn = true;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostInitiatePaymentAsync()
        {
            if (PaymentMethod != null && PaymentMethod == "PayLater")
            {
                Message = "Đang xử lí đơn hàng của quý khách, xin đợi trong giây lát";
                Success = true;
                PayLater = true;
                return Page();
            }
            else if (PaymentMethod != null && PaymentMethod == "VnPay")
            {
                Message = "Đang xử lí đơn hàng của quý khách, xin đợi trong giây lát";
                Success = true;
                var user = JsonConvert.DeserializeObject<AccountDTO>(HttpContext.Session.GetString("acc"));
                PaymentInfo.OrderType = "Vé dịch vụ";
                PaymentInfo.OrderDescription = "Thanh toán vé dịch vụ";
                PaymentInfo.Name = user.Name;
                var paymentUrl = _vnPayService.CreatePaymentUrl(PaymentInfo, HttpContext);
                return Redirect(paymentUrl);
            }
            else
            {
                Message = "Có lỗi xảy ra khi thực hiện giao dịch, xin quý khách vui lòng thử lại sau";
                Success = false;
                return Page();
            }
        }
        public async Task OnGetPaymentCallbackAsync()
        {
            var collections = HttpContext.Request.Query;  // Get the query from the request
            var paymentResponse = _vnPayService.PaymentExecute(collections);

            if (paymentResponse.VnPayResponseCode == "00")
            {
                Message = "Đang xử lí đơn hàng của quý khách, xin đợi trong giây lát";
                Success = true;
            }
            else
            {
                Message = "Thanh toán không thành công, xin quý khách vui lòng thử lại sau";
                Success= false;
            }

        }
        public async Task OnPostSaveTicketAsync()
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                ErrorMessage = "Quý khách cần đăng nhập để thanh toán đơn hàng";
            }
            _ticketService.SetAccessToken(accessToken);

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
                    DiscountCode = SelectedDiscountCode,
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

                if (PayLater == true) newTicket.PaymentStatus = 0;

                var statusCode = await _ticketService.SaveTicketAsync(newTicket, accessToken);
                if (statusCode == HttpStatusCode.OK)
                {

                    Message = "Đặt vé thành công";
                    Success = true;
                    HasTicketSaved = true;
                }
                else
                {
                    Message = "Đặt vé không thành công";
                    Success = false;
                }

            }
        }


    }
}
