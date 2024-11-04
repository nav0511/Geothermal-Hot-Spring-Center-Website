﻿using GHCW_FE.DTOs;
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
                    HttpContext.Session.SetString("accessToken", token.AccessToken.Replace("\n", ""));
                    HttpContext.Session.SetString("refreshToken", token.RefreshToken.Replace("\n",""));
                    return true;
                }
            }
            return false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ, vui lòng thử lại.";
                return Page();
            }

            if (await AuthenticateUserAsync(Account.Email, Account.Password))
            {
                var token = HttpContext.Session.GetString("accessToken");
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
                            1 => RedirectToPage("/Admin/Dashboard"),
                            2 => Redirect("../Index"),
                            3 => Redirect("../Index"),
                            4 => Redirect("../Index"),
                            5 => Redirect("../Index")
                        };
                    }
                }
            }

            TempData["ErrorMessage"] = "Đăng nhập thất bại, vui lòng kiểm tra lại email hoặc mật khẩu.";
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var refreshToken = HttpContext.Session.GetString("refreshToken");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);
                var response = await _client.PostAsync(_logoutApiUrl, null); // Assuming your logout API does not need a body
            }

            // Clear session data
            HttpContext.Session.Remove("accessToken");
            HttpContext.Session.Remove("refreshToken");
            HttpContext.Session.Remove("acc");

            // Redirect to the login page or home page
            return RedirectToPage("/Authentications/Login");
        }
    }
}
