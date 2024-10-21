using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using GHCW_BE.DTOs;

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

                // Lấy claim "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Type");
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

                // Lấy claim "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "ID");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return -1;
                }

                // Chuyển đổi id từ chuỗi sang int
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
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(passwordBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
        public async Task<bool> SendEmail(SendMailDTO sendMailDTO)
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(sendMailDTO.FromEmail);
            mail.To.Add(sendMailDTO.ToEmail);
            mail.Subject = sendMailDTO.Subject;
            mail.Body = sendMailDTO.Body;

            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential(sendMailDTO.FromEmail, sendMailDTO.Password);
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

        public string GenerateVerificationCode(int length)
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
    }
}
