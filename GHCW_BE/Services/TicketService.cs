using AutoMapper;
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

        public async Task<IEnumerable<Ticket>> GetUserBookingList(int uid)
        {
            var tickets = await _context.Tickets.Include(c => c.Customer).Include(td => td.TicketDetails).Where(t => t.CustomerId == uid).ToListAsync();
            return tickets;
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
    }
}
