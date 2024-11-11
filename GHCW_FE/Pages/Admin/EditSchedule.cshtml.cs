using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EditScheduleModel : PageModel
    {
        private readonly ScheduleService _scheduleService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public EditScheduleModel(ScheduleService scheduleService, TokenService tokenService, AuthenticationService authService, AccountService accService)
        {
            _scheduleService = scheduleService;
            _tokenService = tokenService;
            _authService = authService;
            _accService = accService;
        }

        public ScheduleDTO? Schedule { get; set; }
        public List<AccountDTO>? Receptionists { get; set; }
        [BindProperty]
        public EditScheduleRequest EditRequest { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, receptionists) = await _accService.ListReception(accessToken);
            if (statusCode == HttpStatusCode.Forbidden)
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
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy danh sách thông tin người dùng.";
                return Page();
            }
            Receptionists = receptionists;

            var (statusCode2, schedule) = await _scheduleService.GetScheduleByID(id);
            if (statusCode2 == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Lịch làm việc không tồn tại.";
                return Page();
            }
            else if (statusCode2 != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin lịch làm việc.";
                return Page();
            }
            Schedule = schedule;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại";
                return await OnGetAsync(EditRequest.Id);
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);

                var statusCode = await _scheduleService.UpdateSchedule(EditRequest, accessToken);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật thông tin.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else if (statusCode == HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "Ca này đã có lễ tân làm việc rồi.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else if (statusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch làm việc này.";
                    return await OnGetAsync(EditRequest.Id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Cập nhật thông tin thất bại, vui lòng thử lại.";
                    return await OnGetAsync(EditRequest.Id);
                }
            }
            catch (UnauthorizedAccessException)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }
        }
    }
}
