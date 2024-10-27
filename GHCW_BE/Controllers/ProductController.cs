using GHCW_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        GHCWContext context = new GHCWContext();

        [HttpGet]
        public ActionResult GetAllProduct()
        {
            try
            {
                var products = context.Products.ToList();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching data.");
            }
        }
    }
}
