using System.Net;
using System.Text;
using System.Text.Json;

namespace GHCW_FE.Services
{
    public class BaseService
    {
        private string? _rootUrl;
        public BaseService()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _rootUrl = config.GetSection("ApiUrls")["MyApi"];
        }
        public async Task<T?> GetData<T>(string url, string? accepttype = null)
        {
            T? result = default(T);
            HttpClient client = new HttpClient();
            url = _rootUrl + url;
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (responseMessage.Content is not null)
                    result = responseMessage.Content.ReadFromJsonAsync<T>().Result;
                return result;
            }
            else throw new Exception(responseMessage.StatusCode.ToString());
        }

        public async Task<HttpStatusCode> PushData<T>(string url, T value, string? accepttype = null)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();
            var jsonStr = JsonSerializer.Serialize(value);
            HttpContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.PostAsync(url, content);
            return responseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> PutData<T>(string url, T value, string? accepttype = null)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();
            var jsonStr = JsonSerializer.Serialize(value);
            HttpContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.PutAsync(url, content);
            return responseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> DeleteData(string url)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();
            HttpResponseMessage responseMessage = await client.DeleteAsync(url);
            return responseMessage.StatusCode;
        }
    }
}
