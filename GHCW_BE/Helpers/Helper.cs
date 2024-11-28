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
            const string specialChars = "@$#^!%*?&"; // Ký tự đặc biệt

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

        public string ReadSystemPrompt(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{fileName}' không tồn tại trong đường dẫn.");
            }

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Lỗi load file '{fileName}': {ex.Message}");
            }
        }
    }
}
