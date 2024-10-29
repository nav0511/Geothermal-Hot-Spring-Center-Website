using GHCW_FE.DTOs;

namespace GHCW_FE.Services
{
    public class CategoryService : BaseService
    {
        public async Task<List<CategoryDTO>?> GetCategory(string url)
        {
            return await GetData<List<CategoryDTO>>(url);
        }
    }
}
