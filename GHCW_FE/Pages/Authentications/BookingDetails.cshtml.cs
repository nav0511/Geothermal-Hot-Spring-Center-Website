using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class BookingDetailsModel : PageModel
    {
        private TicketDetailService _ticketDetailService = new TicketDetailService();
        private DiscountService _discountService = new DiscountService();
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public BookingDetailsModel(TokenService tokenService, AuthenticationService authService, TicketDetailService ticketDetailService, AccountService accService, DiscountService discountService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _accService = accService;
            _ticketDetailService = ticketDetailService;
            _discountService = discountService;
        }
        public int TicketId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalBeforeDiscount { get; set; }
        public decimal DiscountPrice { get; set; }


        public List<TicketDetailDTO> TicketDetails { get; set; } = new List<TicketDetailDTO>();
        public List<DiscountDTO> DiscountDTOs { get; set; } = new List<DiscountDTO>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem hồ sơ.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 5)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập hồ sơ này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            TicketId = id;

            var (statusCode0, ticketDetails) = await _ticketDetailService.GetBookingDetailsById(id);
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

            TotalBeforeDiscount = TicketDetails.Sum(td => td.Price * td.Quantity);

            if (discount != null)
            {
                if (discount.Value > 0)
                {
                    TotalBeforeDiscount = TotalBeforeDiscount;
                    DiscountPrice = TotalBeforeDiscount * discountValue / 100;
                    TotalAmount = TotalBeforeDiscount - (DiscountPrice);
                }
            }
            else
            {
                TotalBeforeDiscount = TotalBeforeDiscount;
                DiscountPrice = 0;
                TotalAmount = TotalBeforeDiscount;
            }
            return Page();
        }
    }
}
