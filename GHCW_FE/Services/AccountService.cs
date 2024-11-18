using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace GHCW_FE.Services
{
    public class AccountService : BaseService
    {
        public async Task<HttpStatusCode> UpdateProfile(UpdateRequest ur, string accessToken)
        {
            var statusCode = await PutData("Account/updateprofile", ur, null, accessToken);
            return statusCode;
        }

        public async Task<(HttpStatusCode StatusCode, AccountDTO? UserProfile)> UserProfile(string accessToken)
        {
            var (statusCode,user) = await GetData<AccountDTO>("Account/profile", null, accessToken);
            return (statusCode, user);
        }

        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListAccount)> ListAccount(string url, string accessToken)
        {
            var user = await GetData<List<AccountDTO>>(url, null, accessToken);
            return user;
        }

        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListEmployee)> ListEmployee(string url,string accessToken)
        {
            var user = await GetData<List<AccountDTO>>(url, null, accessToken);
            return user;
        }
        
        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListEmployee)> ListReception(string accessToken)
        {
            var user = await GetData<List<AccountDTO>>("Account/receptionlist", null, accessToken);
            return user;
        }

        public async Task<(HttpStatusCode StatusCode, List<AccountDTO>? ListCustomer)> ListCustomerAccount(string accessToken)
        {
            var user = await GetData<List<AccountDTO>>("Account/customerAcclist", null, accessToken);
            return user;
        }

        public async Task<(HttpStatusCode StatusCode, AccountDTO? UserInfo)> GetUserById(string accessToken, int uid)
        {
            var user = await GetData<AccountDTO>($"Account/profile/{uid}", null, accessToken);
            return user;
        }

        public async Task<HttpStatusCode> AccountActivation(string accessToken, int uid)
        {
            var statusCode = await DeleteData($"Account/useractivation/{uid}", accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> EditUserInfo(string accessToken, EditRequest er)
        {
            var statusCode = await PutData<EditRequest>("Account/edituser", er, null, accessToken);
            return statusCode;
        }

        public async Task<HttpStatusCode> AddUser(string accessToken, AddRequest ar)
        {
            var statusCode = await PushData<AddRequest>("Account/adduser", ar, null, accessToken);
            return statusCode;
        }
    }
}
