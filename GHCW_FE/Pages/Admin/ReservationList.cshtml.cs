using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ReservationListModel : PageModel
    {
        private readonly TicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public ReservationListModel(TokenService tokenService, AuthenticationService authService, TicketService ticketService, AccountService accService)
        {
            _authService = authService;
            _ticketService = ticketService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public int ReceptionistID { get; set; }
        public List<TicketDTO> TicketDTOs { get; set; } = new List<TicketDTO>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, string? searchTerm = null, int sortOption = 0)
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
            if (userProfile?.Role > 4 || userProfile?.Role == 2)
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
            ReceptionistID = userProfile.Id;

            SearchTerm = searchTerm;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, tickets) = await _ticketService.GetBookingList("Ticket", userProfile.Role, userProfile.Id);
            if (statusCode1 != HttpStatusCode.OK || tickets == null)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách vé.";
                tickets = new List<TicketDTO>();
                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                tickets = tickets?.Where(d => d.Customer.FullName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            tickets = SortOption switch
            {
                1 => tickets.OrderBy(d => d.OrderDate).ToList(),
                2 => tickets.OrderByDescending(d => d.OrderDate).ToList(),
                3 => tickets.OrderBy(d => d.BookDate).ToList(),
                4 => tickets.OrderByDescending(d => d.BookDate).ToList(),
                _ => tickets.ToList(),
            };

            var totalTickets = tickets?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalTickets / PageSize);
            TicketDTOs = tickets?.Skip(skip).Take(PageSize).ToList() ?? new List<TicketDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateCheckIn(int receptionistID, int ticketId, int checkInStatus, int paymentStatus)
        {
            if (receptionistID == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }

            if (paymentStatus == 0 && checkInStatus != 0)
            {
                paymentStatus = 1;
            }

            var ticketDto = new TicketDTO2
            {
                Id = ticketId,
                CheckIn = (byte)checkInStatus,
                ReceptionistId = receptionistID,
                PaymentStatus = (byte)paymentStatus
            };

            var statusCode = await _ticketService.UpdateCheckinStatus(ticketDto);
            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Cập nhật trạng thái check-in thất bại.";
            }
            else
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái check-in thành công.";
            }

            return RedirectToPage();
        }

    }
}
