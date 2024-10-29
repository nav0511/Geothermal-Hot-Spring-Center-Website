using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;


namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private IMapper _mapper;
        private CategoryService _categoryService;

        public CategoryController(IMapper mapper, CategoryService categoryService)
        {
            _mapper = mapper;
            _categoryService = categoryService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetCategory()
        {
            var list = _categoryService.GetListCategory();
            var projectedQuery = _mapper.ProjectTo<CategoryDTO>(list);
            var result = await projectedQuery.ToListAsync();
            return Ok(result);
        }
    }
}
