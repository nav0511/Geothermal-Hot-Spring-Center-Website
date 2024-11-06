using GHCW_FE.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace GHCW_FE.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string AuthorApiUrl => $"{_configuration.GetValue<string>("ApiUrls:MyApi")}Authentication/login";
        private string GetUserInfoApiUrl => $"{_configuration.GetValue<string>("ApiUrls:MyApi")}Authentication/profile";
        private string LogoutApiUrl => $"{_configuration.GetValue<string>("ApiUrls:MyApi")}Authentication/logout";

        public AuthenticationService(IConfiguration configuration, HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            var userLogin = new LoginDTO { Email = email, Password = password };
            HttpResponseMessage response = await _client.PostAsJsonAsync(AuthorApiUrl, userLogin);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<LoginResponse>(content);

                if (!string.IsNullOrEmpty(token?.AccessToken))
                {
                    var session = _httpContextAccessor.HttpContext.Session;
                    session.SetString("accessToken", token.AccessToken.Replace("\n", ""));
                    session.SetString("refreshToken", token.RefreshToken.Replace("\n", ""));
                    return true;
                }
            }
            return false;
        }

        public async Task<AccountDTO?> GetUserInfoAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var token = session.GetString("accessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.GetAsync(GetUserInfoApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AccountDTO>(content);
                }
            }
            return null;
        }

        public async Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var refreshToken = session.GetString("refreshToken");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);
                await _client.PostAsync(LogoutApiUrl, null);
            }

            session.Remove("accessToken");
            session.Remove("refreshToken");
            session.Remove("acc");
        }
    }
}
