using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Authentications
{
    public class BookingListModel : PageModel
    {
        private readonly TicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;
        public BookingListModel(TokenService tokenService, AuthenticationService authService, TicketService ticketService, AccountService accService)
        {
            _authService = authService;
            _ticketService = ticketService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<TicketDTO> TicketDTOs { get; set; } = new List<TicketDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 5)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
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

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode2, totalNewsCount) = await _ticketService.GetTotalBooking(userProfile?.Role, userProfile?.Id);
            if (statusCode2 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy tổng số vé đặt chỗ.";
                return RedirectToPage("/Authentications/BookingList");
            }

            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode1, list) = await _ticketService.GetBookingList($"Ticket", userProfile?.Role, userProfile?.Id);

            if (statusCode1 == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin này này.";
                return RedirectToPage("/Authentications/BookingList");
            }
            else if (statusCode1 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách đặt trước.";
                return RedirectToPage("/Authentications/BookingList");
            }

            var totalTickets = list?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalTickets / PageSize);
            TicketDTOs = list?.Skip(skip).Take(PageSize).ToList() ?? new List<TicketDTO>();

            return Page();
        }
    }
}
