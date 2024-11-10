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
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;
        public ScheduleManagementModel(ScheduleService scheduleService, TokenService tokenService, AuthenticationService authService, AccountService accService)
        {
            _scheduleService = scheduleService;
            _authService = authService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public ScheduleByWeek SW {  get; set; } = new ScheduleByWeek();
        public List<ScheduleDTO>? Schedules { get; set; }
        public async Task<IActionResult> OnGet(DateTime startDate, DateTime endDate)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode0, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 4 || userProfile?.Role == 2 || userProfile?.Role == 3)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode0 == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode0 != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            if (startDate == DateTime.MinValue && endDate == DateTime.MinValue)
            {
                var currentDate = DateTime.Now;
                // Tính startDate (ngày thứ Hai của tuần hiện tại)
                var startOfWeek = currentDate.AddDays(-((int)currentDate.DayOfWeek - (int)DayOfWeek.Monday));
                // Tính endDate (ngày Chủ Nhật của tuần hiện tại)
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
            else if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy lịch làm việc.";
                return Page();
            }
            Schedules = schedules;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSchedule(int id)
        {
            var responseStatus = await _scheduleService.DeleteSchedule(id);
            if (responseStatus == HttpStatusCode.NoContent)
            {
                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa lịch trình.");
                return Page();
            }
        }
    }
}
