using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace GHCW_FE.Services
{
    public class AccountService : BaseService
    {
        public async Task<HttpStatusCode> ForgotPassword(string email)
        {
            var statusCode = await PushData("Authentication/ForgotPassword", new {email});
            return statusCode;
        }

        public async Task<HttpStatusCode> Register(RegisterDTO registerDTO)
        {
            var statusCode = await PushData("Authentication/Register", registerDTO);
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

        public async Task<HttpStatusCode> UpdateProfile(UpdateRequest ur, string accessToken)
        {
            var statusCode = await PutData("Authentication/updateprofile", ur, null, accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, AccountDTO? UserProfile)> UserProfile(string accessToken)
        {
            var (statusCode,user) = await GetData<AccountDTO>("Authentication/profile", null, accessToken);
            return (statusCode, user);
        }

        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListAccount)> ListAccount(string accessToken)
        {
            var user = await GetData<List<AccountDTO>>("Authentication/userlist", null, accessToken);
            return user;
        }

        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListEmployee)> ListEmployee(string accessToken)
        {
            var user = await GetData<List<AccountDTO>>("Authentication/employeelist", null, accessToken);
            return user;
        }

        public async Task<(HttpStatusCode StatusCode, AccountDTO? UserInfo)> GetUserById(string accessToken, int uid)
        {
            var user = await GetData<AccountDTO>($"Authentication/profile/{uid}", null, accessToken);
            return user;
        }

        public async Task<HttpStatusCode> AccountActivation(string accessToken, int uid)
        {
            var statusCode = await DeleteData($"Authentication/useractivation/{uid}", accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> EditUserInfo(string accessToken, EditRequest er)
        {
            var statusCode = await PutData<EditRequest>("Authentication/edituser", er, null, accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> AddUser(string accessToken, AddRequest ar)
        {
            var statusCode = await PushData<AddRequest>("Authentication/adduser", ar, null, accessToken);
            return statusCode;
        }
    }
}
