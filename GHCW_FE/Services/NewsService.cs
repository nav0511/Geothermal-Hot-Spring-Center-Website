using GHCW_FE.DTOs;

namespace GHCW_FE.Services
{
    public class NewsService : BaseService
    {
        public async Task<List<NewsDTO>?> GetNews(string url)
        {
            return await GetData<List<NewsDTO>>(url);
        }

        public async Task<int> GetTotalNews()
        {
            string url = "News/Total";
            return await GetData<int>(url);
        }
    }
}
