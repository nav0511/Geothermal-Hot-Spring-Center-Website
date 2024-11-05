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
        public List<int> AvailableYears { get; set; }
        public List<string> WeeksOfSelectedYear { get; set; }


        public async Task OnGetAsync(int? year = null)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            AvailableYears = Enumerable.Range(DateTime.Now.Year - 1, 3).ToList();

            WeeksOfSelectedYear = GetWeeksOfYear(selectedYear);

            DateTime referenceDate = DateTime.Today;
            StartDate = referenceDate.AddDays(-(int)referenceDate.DayOfWeek + (int)DayOfWeek.Monday);

            var (statusCode, Schedules) = await _scheduleService.GetWeeklySchedule(StartDate);
            var schedules = Schedules;

            foreach (var schedule in schedules)
            {
                int dayIndex = (schedule.Date.Date - StartDate.Date).Days;
                if (dayIndex >= 0 && dayIndex < 7 && schedule.Shift >= 1 && schedule.Shift <= 2)
                {
                    ScheduleArray[dayIndex, schedule.Shift - 1] = schedule.ReceptionistId.ToString();
                }
            }
        }

        public List<string> GetWeeksOfYear(int year)
        {
            var weeks = new List<string>();
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            DateTime startOfWeek = firstDayOfYear;

            while (startOfWeek.Year == year)
            {
                DateTime endOfWeek = startOfWeek.AddDays(6);
                weeks.Add($"{startOfWeek:dd/MM} - {endOfWeek:dd/MM}");
                startOfWeek = startOfWeek.AddDays(7);
            }

            return weeks;
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
