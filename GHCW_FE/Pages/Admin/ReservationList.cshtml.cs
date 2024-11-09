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

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
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
            if (userProfile?.Role > 4 && userProfile?.Role == 2)
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
            ReceptionistID = userProfile.Id;

            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode2, totalNewsCount) = await _ticketService.GetTotalBooking(userProfile.Role, userProfile.Id);
            if (statusCode2 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy tổng số vé đặt chỗ.";
                return RedirectToPage("/Admin/ResercationList");
            }

            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode1, list) = await _ticketService.GetBookingList($"Ticket?$top={PageSize}&$skip={skip}", userProfile.Role, userProfile.Id);

            if (statusCode1 == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin này này.";
                return RedirectToPage("/Admin/ResercationList");
            }
            else if (statusCode1 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách đặt trước.";
                return RedirectToPage("/Admin/ResercationList");
            }



            TicketDTOs = list?.ToList() ?? new List<TicketDTO>();
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
