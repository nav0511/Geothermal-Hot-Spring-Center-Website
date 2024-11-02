using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class NewsService : BaseService
    {
        public async Task<List<NewsDTO>?> GetNews(string url)
        {
            return await GetData<List<NewsDTO>>(url);
        }

        public async Task<int> GetTotalNews(bool hasDiscount)
        {
            string url = $"News/Total/{hasDiscount}";
            return await GetData<int>(url);
        }

        public async Task<NewsDTO> GetNewsById(int id)
        {
            string url = $"News/GetById/{id}";
            return await GetData<NewsDTO>(url);
        }

        public async Task<NewsDTO> GetNewsByDiscountCode(string code)
        {
            string url = $"News/GetByDiscountCode/{code}";
            return await GetData<NewsDTO>(url);
        }

        public async Task<int> GetTotalRegularNews()
        {
            string url = "News/Reguler";
            return await GetData<int>(url);
        }

        public async Task<int> GetTotalPromotionNews()
        {
            string url = "News/Promotion";
            return await GetData<int>(url);
        }

        //public async Task<NewsDTO?> GetNewsById(int id)
        //{
        //    string url = $"News/{id}";
        //    return await GetData<NewsDTO>(url);
        //}

        public async Task<HttpStatusCode> UpdateNews(NewsDTO news)
        {
            string url = $"News/{news.Id}";
            return await PutData(url, news);
        }

        public async Task<HttpStatusCode> DeleteNews(int id)
        {
            string url = $"News/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateNews(NewsDTO news)
        {
            string url = "News";
            return await PushData(url, news);
        }
    }
}
