using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class CategoryService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<CategoryDTO>? Categories)> GetCategory(string url)
        {
            return await GetData<List<CategoryDTO>>(url);
        }
    }
}
