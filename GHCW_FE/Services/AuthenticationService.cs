using GHCW_FE.DTOs;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace GHCW_FE.Services
{
    public class AuthenticationService : BaseService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string AuthorApiUrl => $"{_configuration.GetValue<string>("ApiUrls:MyApi")}Authentication/login";
        private string GetUserInfoApiUrl => $"{_configuration.GetValue<string>("ApiUrls:MyApi")}Account/profile";

        public AuthenticationService(IConfiguration configuration, HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> AuthenticateUserAsync(LoginDTO userLogin)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            HttpResponseMessage response = await _client.PostAsJsonAsync(AuthorApiUrl, userLogin);
            var statusCode = await PushData("Authentication/login", userLogin);
            if (statusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<LoginResponse>(content);

                if (!string.IsNullOrEmpty(token?.AccessToken))
                {
                    session.SetString("accessToken", token.AccessToken.Replace("\n", ""));
                    session.SetString("refreshToken", token.RefreshToken.Replace("\n", ""));
                    return true;
                }
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var accessToken = session.GetString("accessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                var statusCode = await DeleteData("Authentication/logout", accessToken);
                if (statusCode == HttpStatusCode.OK)
                {
                    session.Remove("accessToken");
                    session.Remove("refreshToken");
                    session.Remove("acc");
                }
            }
        }

        public async Task<HttpStatusCode> Register(RegisterDTO registerDTO)
        {
            var statusCode = await PushData("Authentication/Register", registerDTO);
            return statusCode;
        }

        public async Task<HttpStatusCode> ForgotPassword(string email)
        {
            var statusCode = await PushData("Authentication/ForgotPassword", new { email });
            return statusCode;
        }

        public async Task<HttpStatusCode> ChangePassword(ChangePassRequest changePassRequest, string accessToken)
        {
            var statusCode = await PushData("Authentication/ChangePassword", changePassRequest, null, accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> AccountActivation(ActivationCode ac)
        {
            var statusCode = await PushData("Authentication/activate", ac);
            return statusCode;
        }
    }
}
