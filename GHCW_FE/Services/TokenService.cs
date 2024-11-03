using GHCW_FE.DTOs;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Services
{
    public class TokenService : BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;

        public TokenService(IHttpContextAccessor httpContextAccessor, HttpClient client)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = client;
        }

        private bool IsTokenExpired(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken;
            return jwtToken == null || jwtToken.ValidTo < DateTime.UtcNow;
        }

        public async Task<string> CheckAndRefreshTokenAsync()
        {
            var accessToken = _httpContextAccessor.HttpContext.Session.GetString("accessToken");

            if (string.IsNullOrEmpty(accessToken) || IsTokenExpired(accessToken))
            {
                var refreshToken = _httpContextAccessor.HttpContext.Session.GetString("refreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    throw new UnauthorizedAccessException("No valid refresh token found.");
                }

                // Call the API to refresh the token
                var refreshTokenUrl = $"{_rootUrl}Authentication/refresh";
                var response = await _client.PostAsJsonAsync(refreshTokenUrl, new { RefreshToken = refreshToken });

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var newTokens = JsonConvert.DeserializeObject<LoginResponse>(content);

                    // Store the new tokens in session
                    _httpContextAccessor.HttpContext.Session.SetString("accessToken", newTokens.AccessToken);
                    _httpContextAccessor.HttpContext.Session.SetString("refreshToken", newTokens.RefreshToken);

                    return newTokens.AccessToken;
                }
                else
                {
                    throw new UnauthorizedAccessException("Failed to refresh token. Please log in again.");
                }
            }

            return accessToken;
        }
    }
}
