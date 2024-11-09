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

        public ScheduleManagementModel(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public ScheduleByWeek SW {  get; set; } = new ScheduleByWeek();
        public List<ScheduleDTO>? Schedules { get; set; }
        public async Task<IActionResult> OnGet(DateTime startDate, DateTime endDate)
        {
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
