using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GHCW_BE.Services
{
    public class NewsService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private readonly Helper _helper;

        public NewsService(GHCWContext context,IConfiguration configuration, Helper helper)
        {
            _context = context;
            _configuration = configuration;
            _helper = helper;
        }

        public IQueryable<News> GetListNews()
        {
            return _context.News.Include(n => n.Discount).AsQueryable();
        }

        public async Task<News> GetNewsById(int id)
        {
            return await _context.News.Include(n => n.Discount).FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News> GetNewsByDiscountCode(string code)
        {
            return await _context.News.Include(n => n.Discount).FirstOrDefaultAsync(n => n.DiscountId == code);
        }

        public async Task UpdateNews(News news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNews(News news)
        {
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
        }

        public async Task AddNews(News news)
        {

            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SendNewsNotificationAsync(News news, List<CustomerDTO> users)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();
            var url = _configuration.GetSection("JWT")["Audience"];

            string message;
            string newsUrl;
            if (news.IsActive)
            {
                if (news.DiscountId == null)
                {
                    message = "Chúng tôi vừa cập nhật tin tức mới";
                    newsUrl = $"{url}/News/Detail?Id={Uri.EscapeDataString(news.Id.ToString())}";
                }
                else
                {
                    message = "Chúng tôi vừa cập nhật khuyến mãi mới";
                    newsUrl = $"{url}/Promotions/Detail?Id={Uri.EscapeDataString(news.Id.ToString())}";
                }
            }
            else
            {
                return false;
            }

            foreach (var email in users)
            {
                var encodedEmail = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email.Email));
                var emailDTO = new SendEmailDTO
                {
                    FromEmail = emailSettings.FromEmail,
                    Password = emailSettings.Password,
                    ToEmail = email.Email,
                    Subject = "Tin tức mới từ hệ thống",
                   
                    Body = $"{message}: <strong>{news.Title}</strong>. Nhấn vào đây để xem chi tiết: <a href='{newsUrl}'>Xem tin tức</a>" +
                    $"Để hủy nhận thông báo từ chúng tôi, vui lòng bấm vào <a href='{url}/Notification/Subscriber?Email={encodedEmail}'>đây.</a>"
                   
                };

                bool emailSent = await _helper.SendEmail(emailDTO);
                if (!emailSent)
                {
                    return false;
                }
            }

            return true;
        }


        public async Task<(bool isSuccess, string message)> NewsActivation(int nid)
        {
            var news = await _context.News.FindAsync(nid);
            if (news == null)
            {
                return (false, "Tin tức không tồn tại.");
            }
            try
            {
                news.IsActive = !news.IsActive;
                _context.News.Update(news);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái tin tức thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái tin tức thất bại, vui lòng thử lại.");
            }
        }

    }
}
