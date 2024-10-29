using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class AddServiceModel : PageModel
    {
        private ServicesService _servicesService = new ServicesService();

        public ServiceDTO Service { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var service = new ServiceDTO
            {
                Name = Request.Form["name"],
                Price = Convert.ToDouble(Request.Form["price"]),
                Time = Request.Form["time"],
                Description = Request.Form["description"],
                Image = "/images/" + Request.Form["image"].ToString(),
            };

            Console.WriteLine($"Name: {Request.Form["name"]}");
            Console.WriteLine($"Price: {Request.Form["price"]}");
            Console.WriteLine($"Time: {Request.Form["time"]}");
            Console.WriteLine($"Description: {Request.Form["description"]}");
            Console.WriteLine($"Image: {Request.Form["image"]}");

            var response = await _servicesService.CreateService(service);

            if (response == HttpStatusCode.OK)
            {
                return RedirectToPage("/Admin/ServiceManagement"); 
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm dịch vụ."); 
                return Page();
            }
        }
    }
}
