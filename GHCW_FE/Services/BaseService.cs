
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GHCW_FE.Services
{
    public class BaseService
    {
        protected readonly string? _rootUrl;
        public BaseService()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _rootUrl = config.GetSection("ApiUrls")["MyApi"];
        }

        public void SetAccessToken(string? accessToken)
        {
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = null; // Xóa header nếu không có accessToken
            }
        }

        public async Task<(HttpStatusCode StatusCode, T? Data)> GetData<T>(string url, string? accepttype = null, string? accessToken = null)
        {
            T? result = default(T);
            HttpClient client = new HttpClient();
            url = _rootUrl + url;

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url);
            var statusCode = responseMessage.StatusCode;

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (responseMessage.Content is not null)
                    result = await responseMessage.Content.ReadFromJsonAsync<T>();
                return (statusCode, result);
            }
            return (statusCode, result);
        }

        public async Task<HttpStatusCode> PushData<T>(string url, T value, string? accepttype = null, string? accessToken = null)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (accepttype == "multipart/form-data")
            {
                // Create multipart content
                var multipartContent = new MultipartFormDataContent();

                // Use reflection to add properties automatically
                foreach (var property in typeof(T).GetProperties())
                {
                    var propertyValue = property.GetValue(value);

                    // If the property is null, skip it
                    if (propertyValue == null) continue;

                    if (property.PropertyType == typeof(IFormFile))
                    {
                        // If the property is a file (IFormFile), add as StreamContent
                        var file = (IFormFile)propertyValue;
                        var fileContent = new StreamContent(file.OpenReadStream())
                        {
                            Headers =
                            {
                                ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType)
                            }
                        };
                        multipartContent.Add(fileContent, property.Name, file.FileName);
                    }
                    else
                    {
                        // Otherwise, add it as a string
                        multipartContent.Add(new StringContent(propertyValue.ToString()), property.Name);
                    }
                }

                // Send the multipart request
                HttpResponseMessage responseMessage = await client.PostAsync(url, multipartContent);
                return responseMessage.StatusCode;
            }
            else
            {
                // For application/json or other types, use JSON serialization
                var jsonStr = JsonSerializer.Serialize(value);
                HttpContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                HttpResponseMessage responseMessage = await client.PostAsync(url, content);
                return responseMessage.StatusCode;
            }
        }

        public async Task<HttpStatusCode> PutData<T>(string url, T value, string? accepttype = null, string? accessToken = null)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            var jsonStr = JsonSerializer.Serialize(value);
            HttpContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.PutAsync(url, content);
            return responseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> DeleteData(string url, string? accessToken = null)
        {
            url = _rootUrl + url;
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            HttpResponseMessage responseMessage = await client.DeleteAsync(url);
            return responseMessage.StatusCode;
        }
    }
}
