using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GHCW_FE.Pages.Admin
{
    public class CustomerManagementModel : PageModel
    {

        [BindProperty]
        public List<CustomerDTO> Customers { get; set; }
        public void OnGet()
        {
        }
    }
}
