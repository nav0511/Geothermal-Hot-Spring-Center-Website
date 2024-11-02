using GHCW_FE.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        public async Task<HttpStatusCode> ChangePassword(ChangePassRequest changePassRequest)
        {
            var statusCode = await PushData("Authentication/ChangePassword", changePassRequest);
            return statusCode;
        }
    }
}
