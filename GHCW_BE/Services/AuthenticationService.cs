using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> CheckAccountExsit(RegisterDTO registerDTO)
        {
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(registerDTO.Email));
            return checkUser == null;
        }

        public async Task<string?> Login(LoginDTO loginDTO)
        {
            var userLogin = await _context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(loginDTO.Email) && x.Password.Equals(loginDTO.Password) && x.IsActive == true);
            if (userLogin.Role != null && userLogin != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                      new Claim(ClaimTypes.Email, userLogin.Email.ToString()),
                      new Claim("ID", userLogin.Id.ToString()),
                      new Claim("Role", userLogin?.Role.ToString())
                    }),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _configuration["JWT:Issuer"],
                    Audience = _configuration["JWT:Audience"],
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            return null;
        }
    }
}
