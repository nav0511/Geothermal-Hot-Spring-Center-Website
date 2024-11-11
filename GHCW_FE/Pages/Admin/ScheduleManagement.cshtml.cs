using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ScheduleManagementModel : PageModel
    {
        private readonly ScheduleService _scheduleService;
        private readonly TokenService _tokenService;
        private readonly AccountService _accService;
        private readonly AuthenticationService _authService;

        public ScheduleManagementModel(ScheduleService scheduleService, TokenService tokenService, AccountService accService, AuthenticationService authService)
        {
            _scheduleService = scheduleService;
            _tokenService = tokenService;
            _accService = accService;
            _authService = authService;
        }

        public ScheduleByWeek SW { get; set; } = new ScheduleByWeek();
        public List<ScheduleDTO>? Schedules { get; set; }
        public bool Flag { get; set; }
        public async Task<IActionResult> OnGetAsync(DateTime startDate, DateTime endDate, bool flag = true)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            if (flag)
            {
                var currentDate = DateTime.Now;
                // Calculate startDate (Monday of the current week)
                int daysToSubtract = (int)currentDate.DayOfWeek - (int)DayOfWeek.Monday;
                if (daysToSubtract < 0)
                {
                    daysToSubtract += 7; // Adjust for Sunday as the start of the week
                }
                var startOfWeek = currentDate.AddDays(-daysToSubtract);
                // Calculate endDate (Sunday of the current week)
                var endOfWeek = startOfWeek.AddDays(6);

                SW.StartDate = startOfWeek;
                SW.EndDate = endOfWeek;
            }
            else
            {
                SW.StartDate = startDate;
                SW.EndDate = endDate;
            }
            var (statusCode, schedules) = await _scheduleService.GetWeeklySchedule(SW);
            if (statusCode == HttpStatusCode.NotFound)
            {
                TempData["SuccessMessage"] = "Không có lịch làm việc nào trong tuần này";
                return Page();
            }
            else if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.Unauthorized)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy lịch làm việc.";
                return Page();
            }
            Schedules = schedules;
            Flag = flag;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSchedule(int id, DateTime startDate2, DateTime endDate2)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _scheduleService.DeleteSchedule(id);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if(statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Ẩn lịch làm việc thất bại, vui lòng thử lại sau.";
                await OnGetAsync(startDate2, endDate2, false);
                return Page();
            }
            else
            {
                TempData["SuccessMessage"] = "Ẩn lịch làm việc thành công.";
                await OnGetAsync(startDate2, endDate2, false);
                return Page();
            }
        }
    }
}
