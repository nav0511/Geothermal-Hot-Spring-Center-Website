using GHCW_FE.DTOs;
using System.Net;

namespace GHCW_FE.Services
{
    public class NewsService : BaseService
    {
        public async Task<(HttpStatusCode StatusCode, List<NewsDTO>? News)> GetNews(string url)
        {
            return await GetData<List<NewsDTO>>(url);
        }

        public async Task<(HttpStatusCode StatusCode, int TotalNews)> GetTotalNews(bool hasDiscount)
        {
            string url = $"News/Total/{hasDiscount}";
            return await GetData<int>(url);
        }

        public async Task<(HttpStatusCode StatusCode, NewsDTO? News)> GetNewsById(int id)
        {
            string url = $"News/GetById/{id}";
            return await GetData<NewsDTO>(url);
        }

        public async Task<(HttpStatusCode StatusCode, NewsDTO? News)> GetNewsByDiscountCode(string code)
        {
            string url = $"News/GetByDiscountCode/{code}";
            return await GetData<NewsDTO>(url);
        }

        public async Task<(HttpStatusCode StatusCode, int TotalRegularNews)> GetTotalRegularNews()
        {
            string url = "News/Reguler";
            return await GetData<int>(url);
        }

        public async Task<(HttpStatusCode StatusCode, int TotalPromotionNews)> GetTotalPromotionNews()
        {
            string url = "News/Promotion";
            return await GetData<int>(url);
        }

        //public async Task<NewsDTO?> GetNewsById(int id)
        //{
        //    string url = $"News/{id}";
        //    return await GetData<NewsDTO>(url);
        //}

        public async Task<HttpStatusCode> UpdateNews(NewsDTOForUpdate news, string accessToken, string? accepttype = null)
        {
            string url = $"News/{news.Id}";
            return await PutData<NewsDTOForUpdate>(url, news, accepttype, accessToken);
        }

        public async Task<HttpStatusCode> DeleteNews(int id)
        {
            string url = $"News/{id}";
            return await DeleteData(url);
        }

        public async Task<HttpStatusCode> CreateNews(NewsDTOForAdd news, string accessToken, string? accepttype = null)
        {
            string url = "News";
            if (accepttype != null) return await PushData(url, news, accepttype, accessToken);
            else return await PushData(url, news,null, accessToken); 
        }

        public async Task<HttpStatusCode> NewsActivation(string accessToken, int id)
        {
            var statusCode = await DeleteData($"News/NewsActivation/{id}", accessToken);
            return statusCode;
        }
    }
}
