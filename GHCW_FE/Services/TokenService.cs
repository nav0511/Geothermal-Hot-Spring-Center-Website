namespace GHCW_FE.Services
{
    public class TokenService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public TokenService(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }
    }
}
