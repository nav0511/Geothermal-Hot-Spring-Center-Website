using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class EditServiceModel : PageModel
    {
        private ServicesService _servicesService = new ServicesService();

        public ServiceDTO Service { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Service = await _servicesService.GetServiceById(id);

            if (Service == null)
            {
                ModelState.AddModelError(string.Empty, "Dịch vụ không tồn tại.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID dịch vụ không hợp lệ.");
                return Page();
            }

            Service = await _servicesService.GetServiceById(id);
            if (Service == null)
            {
                ModelState.AddModelError(string.Empty, "Dịch vụ không tồn tại.");
                return NotFound();
            }

            Service.Name = Request.Form["name"];
            Service.Price = Convert.ToDouble(Request.Form["price"]);
            Service.Time = Request.Form["time"];
            Service.Description = Request.Form["description"];
            Service.Image = "/images/" + Request.Form["image"];

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var serviceDto = new ServiceDTO
            {
                Id = Service.Id,
                Name = Service.Name,
                Price = Service.Price,
                Time = Service.Time,
                Description = Service.Description,
                Image = Service.Image,
            };

            var statusCode = await _servicesService.UpdateService(serviceDto);

            if (statusCode == HttpStatusCode.NoContent)
            {
                return RedirectToPage("/Admin/ServiceManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật dịch vụ.");
                return Page();
            }
        }


    }
}
