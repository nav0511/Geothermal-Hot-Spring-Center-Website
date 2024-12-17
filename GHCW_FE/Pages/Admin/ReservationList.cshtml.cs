using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
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
        public int OrderOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, string? searchTerm = null, int orderOption = 0, int sortOption = 0)
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
            var idClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "ID");
            if (roleClaim != null && int.Parse(roleClaim.Value) != 4)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            ReceptionistID = int.Parse(idClaim.Value);

            SearchTerm = searchTerm;
            OrderOption = orderOption;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, tickets) = await _ticketService.GetBookingList(accessToken);
            if (statusCode1 != HttpStatusCode.OK || tickets == null)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách vé.";
                tickets = new List<TicketDTO>();
                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                tickets = tickets?.Where(t =>
                   (t.Receptionist?.Name != null && t.Receptionist.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                   (t.Customer.Name != null && t.Customer.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
               ).ToList();
            }

            tickets = OrderOption switch
            {
                1 => tickets.OrderBy(d => d.Id).ToList(),
                2 => tickets.OrderByDescending(d => d.Id).ToList(),
                3 => tickets.OrderBy(d => d.OrderDate).ToList(),
                4 => tickets.OrderByDescending(d => d.OrderDate).ToList(),
                5 => tickets.OrderBy(d => d.BookDate).ToList(),
                6 => tickets.OrderByDescending(d => d.BookDate).ToList(),
                _ => tickets.ToList(),
            };

            tickets = SortOption switch
            {
                1 => tickets.Where(d => d.PaymentStatus == 1).ToList(),
                2 => tickets.Where(d => d.PaymentStatus == 0).ToList(),
                3 => tickets.Where(d => d.CheckIn == 0).ToList(),
                4 => tickets.Where(d => d.CheckIn == 1).ToList(),
                5 => tickets.Where(d => d.CheckIn == 2).ToList(),
                6 => tickets.Where(d => d.IsActive).ToList(),
                7 => tickets.Where(d => !d.IsActive).ToList(),
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

            var (statusCode0, ticket) = await _ticketService.GetTicketById(ticketId);
            if (statusCode0 != HttpStatusCode.OK || ticket == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vé này.";
                return RedirectToPage();
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var currentDate = DateOnly.FromDateTime(localDateTime);
            var bookedDate = DateOnly.FromDateTime(ticket.BookDate);
            if (bookedDate != currentDate)
            {
                TempData["ErrorMessage"] = "Chỉ được thay đổi trạng thái check-in vào ngày đặt vé.";
                return RedirectToPage();
            }

            var currentTime = DateTime.Now.TimeOfDay;
            var startTime = new TimeSpan(7, 15, 0); 
            var endTime = new TimeSpan(22, 0, 0);  
            if (currentTime < startTime || currentTime > endTime)
            {
                TempData["ErrorMessage"] = "Chỉ được thay đổi trạng thái check-in trong khoảng từ 7:15 sáng đến 10:00 tối.";
                return RedirectToPage();
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
