using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class ScheduleManagementModel : PageModel
    {
        private readonly ScheduleService _scheduleService = new ScheduleService();

        public DateTime StartDate { get; set; }
        public string[,] ScheduleArray { get; set; } = new string[7, 2]; 

        public async Task OnGetAsync(DateTime? selectedDate)
        {
            DateTime referenceDate = selectedDate ?? DateTime.Today;
            StartDate = referenceDate.AddDays(-(int)referenceDate.DayOfWeek + (int)DayOfWeek.Monday);

            var schedules = await _scheduleService.GetWeeklySchedule(StartDate);

            foreach (var schedule in schedules)
            {
                int dayIndex = (schedule.Date.Date - StartDate.Date).Days;
                if (dayIndex >= 0 && dayIndex < 7 && schedule.Shift >= 1 && schedule.Shift <= 2)
                {
                    ScheduleArray[dayIndex, schedule.Shift - 1] = schedule.ReceptionistId.ToString();
                }
            }
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
