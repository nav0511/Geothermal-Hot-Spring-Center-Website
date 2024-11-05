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

        //public async Task OnGet(int pageNumber = 1)
        //{
        //    CurrentPage = pageNumber;
        //    int skip = (pageNumber - 1) * PageSize;

        //    int totalNewsCount = _ticketService.GetTotalBooking().Result;
        //    TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

        //    TicketDTOs = await _ticketService.GetBookingList($"Ticket?$top={PageSize}&$skip={skip}");
        //}

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            // Lấy tổng số vé đặt chỗ
            var (statusCode, totalNewsCount) = await _ticketService.GetTotalBooking();
            if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy tổng số vé đặt chỗ.";
                return RedirectToPage("/Authentications/Login");
            }

            // Tính toán số trang
            TotalPages = (int)Math.Ceiling((double)totalNewsCount / PageSize);

            // Lấy danh sách vé
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

            // Chuyển danh sách vé vào biến TicketDTOs nếu thành công
            TicketDTOs = list?.ToList() ?? new List<TicketDTO>();
            return Page();
        }


    }
}
