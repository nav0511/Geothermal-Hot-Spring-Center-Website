using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using GHCW_BE.DTOs;
using BCrypt.Net;
using GHCW_BE.Models;
using Microsoft.IdentityModel.Tokens;

namespace GHCW_BE.Helpers
{
    public class Helper
    {
        public string GetTypeInHeader(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                // Lấy "Role" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Role");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return null;
                }
                if (userIdClaim.Value != null)
                {
                    return userIdClaim.Value.ToString();
                }
                else
                {
                    Console.WriteLine("Invalid token: Type claim is missing.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while reading token: {ex.Message}");
                return null;
            }
        }

        public int GetIdInHeader(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                // Lấy "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "ID");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return -1;
                }

                // Chuyển đổi id từ string sang int
                if (int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                else
                {
                    Console.WriteLine("Invalid token: ID claim is not a valid integer.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while reading token: {ex.Message}");
                return -1;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<bool> SendEmail(SendEmailDTO s)
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(s.FromEmail);
            mail.To.Add(s.ToEmail);
            mail.Subject = s.Subject;
            mail.Body = s.Body;
            mail.IsBodyHtml = true;

            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential(s.FromEmail, "txyh told rcvo olyb");
                smtpClient.EnableSsl = true;

                try
                {
                    smtpClient.Send(mail);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<string> GenerateVerificationCode(int length)
        {
            const string chars = "0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        public async Task<string> GeneratePassword(int length)
        {
            if (length < 8) throw new ArgumentException("Password length must be at least 8.");

            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Ký tự hoa
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz"; // Ký tự thường
            const string digits = "0123456789"; // Ký tự số
            const string specialChars = "!@#$%^&*()_-+=<>?"; // Ký tự đặc biệt

            // Đảm bảo ít nhất 1 ký tự từ mỗi loại
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            password.Append(upperChars[random.Next(upperChars.Length)]);
            password.Append(lowerChars[random.Next(lowerChars.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Gen các ký tự ngẫu nhiên cho phần còn lại
            const string allChars = upperChars + lowerChars + digits + specialChars;
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Chuyển đổi chuỗi sang mảng ký tự để xáo trộn
            char[] passwordArray = password.ToString().ToCharArray();

            Random randomize = new Random();
            int n = passwordArray.Length;
            while (n > 1)
            {
                int k = randomize.Next(n--);
                (passwordArray[n], passwordArray[k]) = (passwordArray[k], passwordArray[n]); // Hoán đổi
            }

            return new string(passwordArray);
        }

        public async Task<string> GenerateToken(string secretKey, string issuer, string audience, double expirationMinutes, IEnumerable<System.Security.Claims.Claim> claims = null)
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(expirationMinutes),
                credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
