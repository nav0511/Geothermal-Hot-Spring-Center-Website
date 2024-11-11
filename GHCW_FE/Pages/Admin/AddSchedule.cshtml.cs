using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddScheduleModel : PageModel
    {
        private readonly ScheduleService _scheduleService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public AddScheduleModel(ScheduleService scheduleService, TokenService tokenService, AuthenticationService authService, AccountService accService)
        {
            _scheduleService = scheduleService;
            _tokenService = tokenService;
            _authService = authService;
            _accService = accService;
        }

        public List<AccountDTO>? Receptionists { get; set; }

        [BindProperty]
        public AddScheduleRequest AddRequest { get; set; }

        public async Task<IActionResult> OnGetAsync()
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đã nhập không hợp lệ, vui lòng thử lại.";
                await OnGetAsync();
                return Page();
            }
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToPage("/Authentications/Login");
                }
                _scheduleService.SetAccessToken(accessToken);

                var statusCode = await _scheduleService.AddSchedule(AddRequest, accessToken);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Thêm lịch làm việc thành công.";
                    await OnGetAsync();
                    return Page();
                }
                else if (statusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thêm lịch làm việc.";
                    return Page();
                }
                else if (statusCode == HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "Lịch làm việc đã tồn tại, vui lòng chọn ngày hoặc ca làm khác.";
                    await OnGetAsync();
                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = "Thêm lịch làm việc mới thất bại, vui lòng thử lại.";
                    await OnGetAsync();
                    return Page();
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
