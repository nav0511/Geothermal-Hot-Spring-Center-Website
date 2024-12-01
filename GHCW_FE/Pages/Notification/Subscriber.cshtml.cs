using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Notification
{
    public class SubscriberModel : PageModel
    {
        private readonly CustomerService _cusService;

        [BindProperty]
        public Subscriber subscriber { get; set; }

        public SubscriberModel(CustomerService cusService)
        {
            _cusService = cusService;
        }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            subscriber = new Subscriber()
            {
                Email = email
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
            {
                TempData["ErrorMessage"] = "Không thể hủy thông báo do thông tin không hợp lệ.";
                return Page();
            }

            subscriber.IsEmailNotify = false;

            try
            {
                var statusCode = await _cusService.Subscriber(subscriber);
                if (statusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Bạn đã hủy đăng ký thông báo thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi hủy đăng ký.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
