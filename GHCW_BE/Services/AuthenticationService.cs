using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GHCW_BE.Services
{
    public class AuthenticationService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private readonly Helper _helper;
        private readonly IMapper _mapper;

        public AuthenticationService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task Register(Account user)
        {
            await _context.Accounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public string CreateValidationLink(string encodedEmail, string encodedActivationCode)
        {
            var emailSettings = _configuration.GetSection("JWT")["Audience"];
            var activationLink = $"{emailSettings}/Authentications/EmailActivation?email={encodedEmail}&code={encodedActivationCode}";
            return activationLink;
        }

        //Kich hoat tai khoan, sau khi kich hoat se xoa code
        public async Task<bool> RedemActivationCode(ActivationCode code)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == code.Email && u.ActivationCode == code.Code);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true; // Kích hoạt tài khoản
            user.ActivationCode = null; // Xóa mã kích hoạt sau khi đã sử dụng
            await _context.SaveChangesAsync();
            return true;
        }

        //gửi email gọi đến API kích hoạt email
        public async Task<bool> SendActivationEmail(RegisterDTO registerDTO, string activationLink)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();

            SendEmailDTO emailDTO = new SendEmailDTO
            {
                FromEmail = emailSettings.FromEmail,
                Password = emailSettings.Password,
                ToEmail = registerDTO.Email,
                Subject = "Kích hoạt tài khoản",
                Body = $"Nhấp vào liên kết <strong><a href='{activationLink}'>này</a></strong> để kích hoạt tài khoản của bạn."
            };

            return await _helper.SendEmail(emailDTO);
        }

        //gửi email chứa mật khẩu mới được tạo
        public async Task<bool> SendNewPasswordEmail(string email, string newPass)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();

            SendEmailDTO emailDTO = new SendEmailDTO
            {
                FromEmail = emailSettings.FromEmail,
                Password = emailSettings.Password,
                ToEmail = email,
                Subject = "Đặt lại mật khẩu",
                Body = $"Đây là mật khẩu mới của bạn: <strong>{newPass}</strong><br/>Hãy sử dụng mật khẩu này để đăng nhập vào hệ thống.",
            };

            return await _helper.SendEmail(emailDTO);
        }

        public async Task<string> Login(AccountDTO a)
        {
            var key = _configuration["JWT:SecretKey"];
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];
            var expirationMinutes = double.Parse(_configuration["JWT:Expiration"]);
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, a.Email.ToString()),
                new Claim("ID", a.Id.ToString()),
                new Claim("Role", a.Role.ToString())
            };

            return await _helper.GenerateToken(
                key,
                issuer,
                audience,
                expirationMinutes,
                claims);
        }

        public async Task<string> RefTokenGenerator()
        {
            var key = _configuration["JWT:RefSecretKey"];
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];
            var expirationMinutes = double.Parse(_configuration["JWT:RefExpiration"]);
            return await _helper.GenerateToken(
                key,
                issuer,
                audience,
                expirationMinutes
                );
        }

        public bool Validate(string refreshToken)
        {
            TokenValidationParameters tokenValidation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:RefSecretKey"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                RoleClaimType = "Type",
                ClockSkew = TimeSpan.Zero
            }; ;
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(refreshToken, tokenValidation, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangePassword(int uId, string newPassword)
        {
            var user = await _context.Accounts.FindAsync(uId);
            if (user != null)
            {
                user.Password = newPassword;
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<AccountDTO?> GetAccountByRefreshToken(string refreshToken)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
            if (acc != null)
            {
                var accDTO = _mapper.Map<Account, AccountDTO>(acc);
                return accDTO;
            }
            return null;
        }

        public async Task DeleteRefToken(AccountDTO a)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == a.Id);
            if (user != null)
            {
                user.RefreshToken = null;
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateRefreshToken(AccountDTO a)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == a.Id);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }
            user.RefreshToken = a.RefreshToken;
            try
            {
                _context.Accounts.Update(user);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
        }
    }
}
