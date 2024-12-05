using System.IdentityModel.Tokens.Jwt;
using System.Net;
using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace GHCW_FE.Pages.StaffBooking
{
    public class BookTicketModel : PageModel
    {
        private readonly ServicesService _servicesService;
        private readonly TicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly DiscountService _discountService;
        private readonly AccountService _accService;
        private readonly CustomerService _customerService;
        public BookTicketModel(ServicesService servicesService, TicketService ticketService,
            TokenService tokenService, AuthenticationService authService, 
            DiscountService discountService, AccountService accountService,
            CustomerService customerService)
        {
            _servicesService = servicesService;
            _ticketService = ticketService;
            _tokenService = tokenService;
            _authService = authService;
            _discountService = discountService;
            _accService = accountService;
            _customerService = customerService;
        }
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool IsLoggedIn { get; set; }
        public string ErrorMessage { get; set; }
        [BindProperty]
        public string CartData { get; set; }
        [BindProperty]
        public string? SelectedDiscountCode { get; set; }
        [BindProperty]
        public string? SelectedCustomerEmail { get; set; }
        public bool HasTicketSaved { get; set; } = false;
        public string Token { get; set; }

        public List<ServiceDTO> AvailableServices { get; set; } = new List<ServiceDTO>();
        public List<DiscountDTO> AvailableDiscounts { get; set; } = new List<DiscountDTO>();
        public Dictionary<int, int> SelectedServices { get; set; } = new Dictionary<int, int>();
        [BindProperty]
        public AddCustomerRequest AddRequest { get; set; }

        public async Task<IActionResult> OnGetAsync()
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
            if (roleClaim != null && int.Parse(roleClaim.Value) !=4 && int.Parse(roleClaim.Value) != 3)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            (HttpStatusCode StatusCode, List<ServiceDTO>? ListServices) = await _servicesService.GetServices($"Service?$filter=IsActive eq true");
            AvailableServices = ListServices;
            (HttpStatusCode discountStatusCode, List<DiscountDTO>? Discounts) = await _discountService.GetDiscounts("Discount?$filter=IsAvailable eq true");
            if (discountStatusCode == HttpStatusCode.OK && Discounts != null)
            {
                AvailableDiscounts = Discounts;
            }
            Token = accessToken;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng thử lại.";
            //    await OnGetAsync();
            //    return Page();
            //}
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);
                _ticketService.SetAccessToken(accessToken);
                _customerService.SetAccessToken(accessToken);
                Token = accessToken;

                if (SelectedCustomerEmail == null)
                {
                    var (statusCode1, account) = await _accService.GetUserByEmail(accessToken, AddRequest.Email);

                    if (statusCode1 == HttpStatusCode.OK && account != null)
                    {
                        AddRequest.AccountId = account.Id;
                    }
                    else if (statusCode1 != HttpStatusCode.NotFound)
                    {
                        AddRequest.AccountId = null;
                    }

                    var statusCode = await _customerService.AddCustomer(accessToken, AddRequest);
                    if (statusCode == HttpStatusCode.OK) { }
                    else if (statusCode == HttpStatusCode.Forbidden)
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền thêm khách hàng mới.";
                        await OnGetAsync();
                        return Page();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thêm người dùng mới thất bại, vui lòng thử lại.";
                        await OnGetAsync();
                        return Page();
                    }
                }

                var cart = JsonConvert.DeserializeObject<List<CartItemDTO>>(CartData);
                var user = JsonConvert.DeserializeObject<AccountDTO>(HttpContext.Session.GetString("acc"));
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                    await OnGetAsync();
                    return Page();
                }

                if (cart == null || cart.Count == 0)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống hoặc không hợp lệ.";
                    await OnGetAsync();
                    return Page();
                }

                string email = SelectedCustomerEmail != null ? SelectedCustomerEmail : AddRequest.Email;
                var (statusCode2, acc) = await _customerService.GetCustomerByEmail(accessToken, email);
                if (acc == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    await OnGetAsync();
                    return Page();
                }

                if (user == null || cart == null || cart.Count == 0)
                {
                    Message = "Có lỗi xảy ra, xin quý khách vui lòng thử lại sau";
                }
                else
                {
                    var newTicket = new TicketDTOForStaff
                    {
                        CustomerId = acc.Id,
                        ReceptionistId = user.Id,
                        SaleId = user.Id,
                        DiscountCode = SelectedDiscountCode,
                        Total = cart.Sum(item => item.Quantity * item.Price),
                        OrderDate = DateTime.Now,
                        BookDate = DateTime.Now,
                        PaymentStatus = 1,
                        CheckIn = 1,
                        TicketDetails = cart.Select(item => new TicketDetailDTOForPayment
                        {
                            ServiceId = item.ServiceId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        }).ToList()
                    };

                    var saveTicketStatusCode = await _ticketService.SaveTicketForStaffAsync(newTicket, accessToken);
                    if (saveTicketStatusCode == HttpStatusCode.OK)
                    {
                        TempData["SuccessMessage"] = "Thanh toán thành công.";
                        Success = true;
                        HasTicketSaved = true;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán không thành công.";
                        Success = false;
                    }

                }
                await OnGetAsync();
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }

            
        }
    }
}
