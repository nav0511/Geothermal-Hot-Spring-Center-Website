using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class TicketService
    {
        private readonly GHCWContext _context;
        private readonly IConfiguration _configuration;
        private Helper _helper;
        private IMapper _mapper;

        public TicketService(GHCWContext context, IConfiguration configuration, Helper helper, IMapper mapper)
        {
            _context = context;
            _helper = helper;
            _configuration = configuration;
            _mapper = mapper;
        }

        public IQueryable<Ticket> GetListBooking(int? role, int? uId)
        {
            if(role == 3)
            {
                return _context.Tickets.Include(t => t.Customer).Include(t => t.Receptionist).Where(t => t.SaleId == uId);
            }
            else if (role == 5)
            {
                return _context.Tickets.Include(t => t.Customer).Include(t => t.Receptionist).Where(t => t.Customer.AccountId == uId);
            }
            else
            {
                return _context.Tickets.Include(t => t.Customer).Include(t => t.Receptionist);

            }
        }

        public async Task<Ticket> GetTicketById(int id)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTicket(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<Ticket?> SaveTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                return ticket;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Ticket> GetTicketByIdIncludeService(int id)
        {
            return await _context.Tickets
                .Include(t => t.TicketDetails)
                .ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<bool> SendTicketToEmail(Ticket ticket, CustomerDTO customer)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<SendEmailDTO>();
            ticket.Total *= 1000;

            // Create the email body
            string ticketDetails = "";
            foreach (var detail in ticket.TicketDetails)
            {
                detail.Price *= 1000;
                detail.Total *= 1000;
                ticketDetails += $"<tr>" +
                                 $"<td>{detail.Service.Name}</td>" +
                                 $"<td>{detail.Quantity}</td>" +
                                 $"<td>{detail.Price:n0} VND</td>" +
                                 $"<td>{detail.Total:n0} VND</td>" +
                                 $"</tr>";
            }

            string body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2>Thông tin vé dịch vụ Trung tâm Khoáng nóng Địa chất</h2>
            <p>Xin chào {customer.FullName},</p>
            <p>Cảm ơn quý khách đã đặt vé tại Trung tâm Khoáng nóng Địa chất. Dưới đây là thông tin chi tiết về đơn hàng của quý khách:</p>
            <h3>Thông tin vé</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                <thead>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Dịch vụ</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Số lượng</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Đơn giá</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {ticketDetails}
                </tbody>
                <tfoot>
                    <tr style='font-weight: bold;'>
                        <td colspan='3' style='padding: 8px; border: 1px solid #ddd;'>Tổng cộng</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{ticket.Total:n0} VND</td>
                    </tr>
                </tfoot>
            </table>
            <h3>Thông tin đặt vé</h3>
            <p><strong>Ngày đặt:</strong> {ticket.OrderDate:dd/MM/yyyy}</p>
            <p><strong>Ngày sử dụng:</strong> {ticket.BookDate:dd/MM/yyyy}</p>
            <p>Vui lòng xuất trình email này tại quầy lễ tân khi đến sử dụng dịch vụ. Nếu có bất kỳ câu hỏi nào, xin vui lòng liên hệ với chúng tôi qua số điện thoại hỗ trợ khách hàng.</p>
            <p>Trân trọng,<br>Đội ngũ Trung tâm Khoáng nóng Địa chất</p>
        </div>";

            SendEmailDTO emailDTO = new SendEmailDTO
            {
                FromEmail = emailSettings.FromEmail,
                Password = emailSettings.Password,
                ToEmail = customer.Email,
                Subject = "Thông tin vé dịch vụ Trung tâm Khoáng nóng Địa chất",
                Body = body
            };

            return await _helper.SendEmail(emailDTO);
        }

    }
}
