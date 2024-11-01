using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace GHCW_FE.Pages.Authentications
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _authorApiUrl;
        private readonly string _getUserInfoApiUrl;
        private readonly string _logoutApiUrl;
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var baseUrlAPI = _configuration.GetValue<string>("ApiUrls:MyApi");
            _authorApiUrl = $"{baseUrlAPI}Authentication/login";
            _getUserInfoApiUrl = $"{baseUrlAPI}Authentication/profile";
            _logoutApiUrl = $"{baseUrlAPI}Authentication/logout";
        }
        [BindProperty]
        public LoginDTO Account { get; set; }

        public void OnGet()
        {
        }

        // Xác thực tài khoản và lấy token
        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            var userLogin = new LoginDTO { Email = email, Password = password };
            HttpResponseMessage response = await _client.PostAsJsonAsync(_authorApiUrl, userLogin);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<LoginResponse>(content);

                // Lưu token vào session nếu có
                if (!string.IsNullOrEmpty(token?.AccessToken))
                {
                    HttpContext.Session.SetString("token", token.AccessToken.Replace("\n", ""));
                    return true;
                }
            }
            return false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await AuthenticateUserAsync(Account.Email, Account.Password))
            {
                var token = HttpContext.Session.GetString("token");
                if (token != null)
                {
                    // Thêm token vào Authorization header
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Gọi API lấy thông tin người dùng
                    HttpResponseMessage response = await _client.GetAsync(_getUserInfoApiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var userAccount = JsonConvert.DeserializeObject<AccountDTO>(content);

                        // Lưu thông tin tài khoản vào session
                        HttpContext.Session.SetString("acc", System.Text.Json.JsonSerializer.Serialize(userAccount));

                        // Điều hướng người dùng dựa trên loại tài khoản
                        return userAccount.Role switch
                        {
                            0 => RedirectToPage("/Admin/Dashboard"),
                            1 => RedirectToPage(""),
                            2 => Redirect("../Index"),
                            3 => Redirect("../Index"),
                            4 => Redirect("../Index"),
                            5 => Redirect("../Index")
                        };
                    }
                }
            }

            // Thông báo lỗi khi đăng nhập thất bại
            ViewData["error"] = "Email hoặc mật khẩu không chính xác! Vui lòng thử lại!";
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var token = HttpContext.Session.GetString("token");

            if (token != null)
            {
                // Gọi đến API logout
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.DeleteAsync(_logoutApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ViewData["error"] = "Có lỗi xảy ra khi đăng xuất. Vui lòng thử lại sau.";
                    return Page();
                }
            }
            HttpContext.Session.Clear();
            return RedirectToPage("../Index");
        }
    }
}
