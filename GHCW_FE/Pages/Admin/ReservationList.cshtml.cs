using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ReservationListModel : PageModel
    {
        private TicketService _ticketService = new TicketService();

        public List<TicketDTO> TicketDTOs { get; set; } = new List<TicketDTO>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 6;

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode, totalNewsCount) = await _ticketService.GetTotalBooking();
            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy tổng số vé đặt chỗ.";
                return RedirectToPage("/Authentications/Login");
            }

            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            var (statusCode1, list) = await _ticketService.GetBookingList($"Ticket?$top={PageSize}&$skip={skip}");

            if (statusCode1 == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập hồ sơ này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode1 == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode1 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            TicketDTOs = list?.ToList() ?? new List<TicketDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateCheckIn(int ticketId, int checkInStatus, int paymentStatus)
        {
            int receptionistId;
            //var userAccountJson = HttpContext.Session.GetString("acc");
            //if (!string.IsNullOrEmpty(userAccountJson))
            //{
            //    var userAccount = System.Text.Json.JsonSerializer.Deserialize<AccountDTO>(userAccountJson);
            //    receptionistId = userAccount.Id;
            //}
            //else
            //{
            //    TempData["ErrorMessage"] = "Bạn cần đăng nhập.";
            //    return RedirectToPage("/Authentications/Login");
            //}
            //Fix cứng Id lễ tân bằng 1 để test vì chưa đăng nhập bằng tài khoản lễ tân.
            receptionistId = 1;
            if (receptionistId == null)
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
                ReceptionistId = receptionistId,
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
