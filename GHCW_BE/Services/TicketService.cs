using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using GHCW_BE.Utils.Helpers;
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



        public async Task<List<TicketDTO>> GetListBooking(int role, int? uId)
        {
            var tickets = await _context.Tickets.Include(c => c.Customer).Include(a => a.Receptionist).Include(d => d.DiscountCodeNavigation).Include(s => s.Sale).ToListAsync();
            if (role == 3)
            {
                tickets = tickets.Where(t => t.SaleId == uId).ToList();
            }
            else if (role == 5)
            {
                tickets = tickets.Where(t => t.Customer.AccountId == uId && t.IsActive).ToList();
            }
            else if (role == 4)
            {
                tickets = tickets.Where(t => t.IsActive).ToList();
            }
            var ticketsDTO = _mapper.Map<List<Ticket>, List<TicketDTO>>(tickets);
            return ticketsDTO;
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
                                 $"<td style='padding: 8px; border: 1px solid #ddd;'>{detail.Service.Name}</td>" +
                                 $"<td style='padding: 8px; border: 1px solid #ddd;'>{detail.Quantity}</td>" +
                                 $"<td style='padding: 8px; border: 1px solid #ddd;'>{detail.Price:n0} VND</td>" +
                                 $"<td style='padding: 8px; border: 1px solid #ddd;'>{detail.Total:n0} VND</td>" +
                                 $"</tr>";
            }

            string body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2>Thông tin vé dịch vụ Trung tâm Khoáng nóng Địa chất</h2>
            <p>Xin chào {customer.Name},</p>
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
                        <td style='padding: 8px; border: 1px solid #ddd;'>Mã giảm giá</td>
                        <td colspan='3' style='padding: 8px; border: 1px solid #ddd;'>{ticket.DiscountCode}</td>
                    </tr>
                    <tr style='font-weight: bold;'>
                        <td style='padding: 8px; border: 1px solid #ddd;'>Tổng cộng</td>
                        <td colspan='3' style='padding: 8px; border: 1px solid #ddd;'>{ticket.Total:n0} VND</td>
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


        public async Task<(bool isSuccess, string message)> TicketActivation(int tid)
        {
            var ticket = await _context.Tickets.FindAsync(tid);
            if (ticket == null)
            {
                return (false, "Vé không tồn tại.");
            }
            try
            {
                ticket.IsActive = !ticket.IsActive;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                return (true, "Thay đổi trạng thái vé thành công.");
            }
            catch (Exception)
            {
                return (false, "Thay đổi trạng thái vé thất bại, vui lòng thử lại.");
            }
        }

        public IQueryable<TicketDetail> GetListBookingDetails()
        {
            return _context.TicketDetails.AsQueryable();
        }

        public async Task<List<TicketDetail>> GetBookingDetails(int id)
        {
            return await _context.TicketDetails
                .Include(t => t.Ticket)
                .Include(s => s.Service)
                .Where(t => t.TicketId == id)
                .ToListAsync();
        }
    }
}
