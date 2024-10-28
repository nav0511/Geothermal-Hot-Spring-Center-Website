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
        private Helper _helper;

        public AuthenticationService(GHCWContext context, IConfiguration configuration, Helper helper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
        }

        public async Task Register(Account user)
        {
            await _context.Accounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        //kiem tra nguoi dung co ton tai hay khong
        //neu co tra ve account, neu khong tra ve null
        public async Task<Account?> CheckAccountExsit(string email)
        {
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(email));
            return checkUser;
        }

        //kiem tra nguoi dung co ton tai va dang hoat dong hay khong
        //neu co tra ve account, neu khong tra ve null
        public async Task<Account?> CheckActiveStatus(string email)
        {
            var existUser = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
            return existUser;
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
                Body = $"Nhấp vào liên kết để kích hoạt tài khoản của bạn: {activationLink}"
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

        public async Task<string> Login(Account a)
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

        //lấy thông tin user bằng ID
        public async Task<Account?> GetUserProfileById(int uID)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == uID);
            if (user != null)
            {
                return user;
            }
            return null;
        }

        //lấy thông tin nhân viên bằng ID
        public async Task<Account?> GetEmployeeProfileById(int eID)
        {
            var employee = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == eID && e.Role >= 1);
            if (employee != null)
            {
                return employee;
            }
            return null;
        }

        //lấy thông tin khách hàng bằng ID
        public async Task<Account?> GetCustomerProfileById(int cID)
        {
            var customer = await _context.Accounts.FirstOrDefaultAsync(c => c.Id == cID && c.Role == 5);
            if (customer != null)
            {
                return customer;
            }
            return null;
        }

        //lấy danh sách user
        public async Task<IEnumerable<Account>> GetUserList()
        {
            var users = await _context.Accounts.ToListAsync();
            return users;
        }

        //lấy danh sách nhân viên role từ 1 - 4
        public async Task<IEnumerable<Account>> GetEmployeeList()
        {
            return await _context.Accounts.Where(a => a.Role >= 1 && a.Role <= 4).ToListAsync();
        }

        //lấy danh sách khách hàng role = 5
        public async Task<IEnumerable<Account>> GetCustomerList()
        {
            return await _context.Accounts.Where(a => a.Role == 5).ToListAsync();
        }


        //cập nhật profile của user
        public async Task UpdateProfile(Account user)
        {
            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();
        }

        //Đổi password
        public async Task<bool> UpdatePassword(Account user)
        {
            if (user != null)
            {
                var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.Email.Equals(user.Email));
                if (acc != null)
                {
                    acc.Password = user.Password;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<Account> GetAccountByRefreshToken(string refreshToken)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
            return acc;
        }

        public async Task DeleteRefToken(Account a)
        {
            a.RefreshToken = null;
            _context.Accounts.Update(a);
            await _context.SaveChangesAsync();
        }
    }
}
