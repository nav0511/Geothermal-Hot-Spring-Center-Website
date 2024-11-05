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

        public IQueryable<Ticket> GetListBooking()
        {
            return _context.Tickets.AsQueryable();
        }
    }
}
